using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PenumbraModForwarder.Watchdog.Services;

public class ProcessManager : IDisposable
{
    private readonly bool _isDevMode;
    private readonly string _solutionDirectory;
    private Process _uiProcess;
    private Process _backgroundServiceProcess;
    private bool _isShuttingDown = false;

    public ProcessManager(bool isDevMode)
    {
        _isDevMode = isDevMode;
        _solutionDirectory = GetSolutionDirectory();
        Console.WriteLine($"Solution Directory: {_solutionDirectory}");
        Console.WriteLine($"Running in {(_isDevMode ? "DEV" : "PROD")} mode.");
        
        SetupShutdownHandlers();
    }
    
    private void SetupShutdownHandlers()
    {
        // Handle console cancel events (Ctrl+C, Ctrl+Break)
        Console.CancelKeyPress += (sender, e) => 
        {
            e.Cancel = true; // Prevent immediate termination
            ShutdownChildProcesses();
        };

        // For Windows-specific signal handling
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Imports.DllImports.SetConsoleCtrlHandler(ConsoleCtrlCheck, true);
        }

        // Handle normal process exit
        AppDomain.CurrentDomain.ProcessExit += (sender, e) => 
        {
            ShutdownChildProcesses();
        };
    }
    
    // Console control handler implementation
    private bool ConsoleCtrlCheck(int sig)
    {
        ShutdownChildProcesses();
        return true;
    }
    
    public void ShutdownChildProcesses()
    {
        // Prevent multiple simultaneous shutdown attempts
        if (_isShuttingDown) return;
        _isShuttingDown = true;

        Console.WriteLine("Initiating graceful shutdown of child processes...");

        try 
        {
            // Attempt to close UI process
            if (_uiProcess != null && !_uiProcess.HasExited)
            {
                Console.WriteLine($"Closing UI Process (PID: {_uiProcess.Id})");
                _uiProcess.Kill(true);
            }

            // Attempt to close Background Worker process
            if (_backgroundServiceProcess != null && !_backgroundServiceProcess.HasExited)
            {
                Console.WriteLine($"Closing Background Worker Process (PID: {_backgroundServiceProcess.Id})");
                _backgroundServiceProcess.Kill(true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during shutdown: {ex.Message}");
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
        // Construct the full path to the project directory
        string projectDirectory = Path.Combine(_solutionDirectory, projectName);
        string projectFilePath = Path.Combine(projectDirectory, $"{projectName}.csproj");

        Console.WriteLine($"Project Directory: {projectDirectory}");
        Console.WriteLine($"Project File Path: {projectFilePath}");

        // Verify project file exists
        if (!File.Exists(projectFilePath))
        {
            Console.WriteLine($"Error: Project file not found at {projectFilePath}");
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
                Console.WriteLine($"[{projectName} OUTPUT] {args.Data}");
            }
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                Console.WriteLine($"[{projectName} ERROR] {args.Data}");
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        
        Console.WriteLine($"Started {projectName} (PID: {process.Id})");
        return process;
    }

    private Process StartProdProcess(string executableName)
    {
        string executablePath = Path.Combine(AppContext.BaseDirectory, "apps", executableName);
        if (!File.Exists(executablePath))
        {
            Console.WriteLine($"Error: {executablePath} not found.");
            throw new FileNotFoundException($"Executable not found: {executablePath}");
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        Console.WriteLine($"Started {executableName} (PID: {process.Id})");
        return process;
    }

    private string GetSolutionDirectory()
    {
        // Start from the current directory and walk up until we find the solution file
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
        while (true)
        {
            if (uiProcess.HasExited)
            {
                Console.WriteLine($"Monitoring for {uiProcess.ProcessName} exited.");
                backgroundServiceProcess.Kill();
                Environment.Exit(0);
            }

            if (backgroundServiceProcess.HasExited)
            {
                Console.WriteLine("Background Service exited unexpectedly!");
                backgroundServiceProcess = StartProcess("PenumbraModForwarder.BackgroundWorker");
            }

            Thread.Sleep(1000);
        }
    }
    
    public void Dispose()
    {
        ShutdownChildProcesses();
    }
}