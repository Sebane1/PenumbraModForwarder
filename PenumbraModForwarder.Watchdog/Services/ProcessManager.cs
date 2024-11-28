using System.Diagnostics;
using System.Runtime.InteropServices;
using Serilog;

namespace PenumbraModForwarder.Watchdog.Services;

public class ProcessManager : IDisposable
{
    private readonly bool _isDevMode;
    private readonly string _solutionDirectory;
    private Process _uiProcess;
    private Process _backgroundServiceProcess;
    private bool _isShuttingDown = false;

    public ProcessManager()
    {
        _isDevMode = Environment.GetEnvironmentVariable("DEV_MODE") == "true";
        if (_isDevMode)
        {
            _solutionDirectory = GetSolutionDirectory();
            Log.Information($"Solution Directory: {_solutionDirectory}");
        }
        Log.Information($"Running in {(_isDevMode ? "DEV" : "PROD")} mode.");
        SetupShutdownHandlers();
    }

    public void Run()
    {
        try
        {
            Log.Information("Starting Penumbra Mod Forwarder");
            _uiProcess = StartProcess("PenumbraModForwarder.UI");
            _backgroundServiceProcess = StartProcess("PenumbraModForwarder.BackgroundWorker");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Failed to start Penumbra Mod Forwarder");
        }
        
        MonitorProcesses(_uiProcess, _backgroundServiceProcess);
    }


    private void SetupShutdownHandlers()
    {
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true; // Prevent immediate termination
            ShutdownChildProcesses();
        };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Imports.DllImports.SetConsoleCtrlHandler(ConsoleCtrlCheck, true);
        }

        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            ShutdownChildProcesses();
        };
    }

    private bool ConsoleCtrlCheck(int sig)
    {
        ShutdownChildProcesses();
        return true;
    }

    private void ShutdownChildProcesses()
    {
        if (_isShuttingDown) return;
        _isShuttingDown = true;

        Log.Information("Initiating graceful shutdown of child processes...");

        try
        {
            // Try graceful shutdown first
            if (_uiProcess != null && !_uiProcess.HasExited)
            {
                Log.Information($"Closing UI Process (PID: {_uiProcess.Id})");
                _uiProcess.Kill();  // Forcibly kill if it doesn't exit on its own
            }

            if (_backgroundServiceProcess != null && !_backgroundServiceProcess.HasExited)
            {
                Log.Information($"Closing Background Worker Process (PID: {_backgroundServiceProcess.Id})");
                _backgroundServiceProcess.Kill();  // Same for the background service
            }
        }
        catch (Exception ex)
        {
            Log.Information($"Error during shutdown: {ex.Message}");
        }
        finally
        {
            Environment.Exit(0);
        }
    }

    public Process StartProcess(string projectName)
    {
        return _isDevMode
            ? StartDevProcess(projectName)
            : StartProdProcess($"{projectName}.exe");
    }

    private Process StartDevProcess(string projectName)
    {
        string projectDirectory = Path.Combine(_solutionDirectory, projectName);
        string projectFilePath = Path.Combine(projectDirectory, $"{projectName}.csproj");

        if (!File.Exists(projectFilePath))
        {
            Log.Information($"Error: Project file not found at {projectFilePath}");
            throw new FileNotFoundException($"Project file not found: {projectFilePath}");
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{projectFilePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = projectDirectory
            }
        };

        process.OutputDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                Log.Information($"[{projectName} OUTPUT] {args.Data}");
            }
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                Log.Information($"[{projectName} ERROR] {args.Data}");
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        Log.Information($"Started {projectName} (PID: {process.Id})");
        return process;
    }

    private Process StartProdProcess(string executableName)
    {
        string executablePath = Path.Combine(AppContext.BaseDirectory, executableName);
        string executableDir = Path.GetDirectoryName(executablePath);

        Log.Information($"Executing executable in PROD Mode: {executablePath}");

        if (!File.Exists(executablePath))
        {
            Log.Information($"Error: {executablePath} not found.");
            throw new FileNotFoundException($"Executable not found: {executablePath}");
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = executableDir
            }
        };

        process.OutputDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                Log.Information($"[{executableName} OUTPUT] {args.Data}");
            }
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                Log.Information($"[{executableName} ERROR] {args.Data}");
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        Log.Information($"Started {executableName} (PID: {process.Id})");

        return process;
    }

    
    private string GetSolutionDirectory()
    {
        string currentDir = AppContext.BaseDirectory;
        while (currentDir != null)
        {
            string solutionFile = Directory.GetFiles(currentDir, "*.sln").FirstOrDefault();
            if (solutionFile != null)
            {
                return currentDir;
            }
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }

        throw new Exception("Could not find solution directory");
    }

    public void MonitorProcesses(Process uiProcess, Process backgroundServiceProcess)
    {
        // Monitor the processes indefinitely until the UI process exits
        while (!_isShuttingDown)
        {
            if (uiProcess.HasExited)
            {
                Log.Information($"UI Process {uiProcess.Id} exited with code {uiProcess.ExitCode}.");
                if (backgroundServiceProcess != null && !backgroundServiceProcess.HasExited)
                {
                    Log.Information($"Terminating Background Service (PID: {backgroundServiceProcess.Id}) due to UI exit.");
                    backgroundServiceProcess.Kill();
                }

                ShutdownChildProcesses(); // Ensure both processes are shut down cleanly
                break;
            }

            // If the background service has exited unexpectedly, restart it
            if (backgroundServiceProcess.HasExited)
            {
                Log.Information("Background Service exited unexpectedly!");
                backgroundServiceProcess = StartProcess("PenumbraModForwarder.BackgroundWorker");
            }

            // Sleep for 1 second to prevent excessive CPU usage
            Thread.Sleep(1000);
        }
    }

    public void Dispose()
    {
        ShutdownChildProcesses();
    }
}
