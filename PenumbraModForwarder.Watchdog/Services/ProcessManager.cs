using System.Collections;
using System.Diagnostics;
using System.Net.Sockets;
using PenumbraModForwarder.Watchdog.Interfaces;
using Serilog;

namespace PenumbraModForwarder.Watchdog.Services
{
    class ProcessManager : IProcessManager
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

                int port = FindRandomAvailablePort();
                _backgroundServiceProcess = StartProcess("PenumbraModForwarder.BackgroundWorker", port.ToString());

                _uiProcess = StartProcess("PenumbraModForwarder.UI", port.ToString());
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to start Penumbra Mod Forwarder");
            }
            
            MonitorProcesses(_uiProcess, _backgroundServiceProcess);
        }

        private int FindRandomAvailablePort()
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 0));
                return ((System.Net.IPEndPoint)socket.LocalEndPoint).Port;
            }
        }

        private Process StartProcess(string projectName, string port)
        {
            Log.Information($"Starting: {projectName}");
            return _isDevMode
                ? StartDevProcess(projectName, port)
                : StartProdProcess($"{projectName}.exe", port);
        }

        private Process StartDevProcess(string projectName, string port)
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
                    Arguments = $"run --project \"{projectFilePath}\" -- {port}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = projectDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
            {
                process.StartInfo.EnvironmentVariables[entry.Key.ToString()] = entry.Value?.ToString();
            }

            process.StartInfo.EnvironmentVariables["WATCHDOG_INITIALIZED"] = "true";
            process.StartInfo.EnvironmentVariables["DEV_MODE"] = "true";

            process.Start();
            return process;
        }

        private Process StartProdProcess(string executableName, string port)
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
                    Arguments = port,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = executableDir
                }
            };

            process.Start();
            Log.Information($"Started {executableName} (PID: {process.Id})");

            return process;
        }

        private void SetupShutdownHandlers()
        {
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // Prevent immediate termination
                ShutdownChildProcesses();
            };
        }

        private void ShutdownChildProcesses()
        {
            if (_isShuttingDown) return;
            _isShuttingDown = true;

            Log.Information("Initiating graceful shutdown of child processes...");

            try
            {
                if (_uiProcess != null && !_uiProcess.HasExited)
                {
                    Log.Information($"Closing UI Process (PID: {_uiProcess.Id})");
                    _uiProcess.Kill();
                }

                if (_backgroundServiceProcess != null && !_backgroundServiceProcess.HasExited)
                {
                    Log.Information($"Closing Background Worker Process (PID: {_backgroundServiceProcess.Id})");
                    _backgroundServiceProcess.Kill();
                }
            }
            catch (Exception ex)
            {
                Log.Information($"Error during shutdown: {ex.Message}");
            }
            finally
            {
                Log.Information("Shutting down Penumbra Mod Forwarder");
                Environment.Exit(0);
            }
        }

        private void MonitorProcesses(Process uiProcess, Process backgroundServiceProcess)
        {
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

                    ShutdownChildProcesses();
                    break;
                }

                if (backgroundServiceProcess.HasExited)
                {
                    Log.Information("Background Service exited unexpectedly!");
                    backgroundServiceProcess = StartProcess("PenumbraModForwarder.BackgroundWorker", FindRandomAvailablePort().ToString());
                }

                Thread.Sleep(1000);
            }
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

        public void Dispose()
        {
            ShutdownChildProcesses();
        }
    }
}