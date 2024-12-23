﻿using System;
using System.Collections;
using System.Diagnostics;
using System.Net.Sockets;
using PenumbraModForwarder.Watchdog.Interfaces;
using Serilog;
using ILogger = Serilog.ILogger;

namespace PenumbraModForwarder.Watchdog.Services
{
    public class ProcessManager : IProcessManager, IDisposable
    {
        private readonly bool _isDevMode;
        private readonly string _solutionDirectory;
        private readonly int _port;
        private Process _uiProcess;
        private Process _backgroundServiceProcess;
        private bool _isShuttingDown = false;
        private readonly ILogger _logger;

        public ProcessManager()
        {
            _logger = Log.ForContext<ProcessManager>();
            _isDevMode = Environment.GetEnvironmentVariable("DEV_MODE") == "true";
            if (_isDevMode)
            {
                _solutionDirectory = GetSolutionDirectory();
                _logger.Information("Solution Directory: {SolutionDirectory}", _solutionDirectory);
            }
            _logger.Information("Running in {Mode} mode.", _isDevMode ? "DEV" : "PROD");

            // Initialize the port once
            _port = FindRandomAvailablePort();
            SetupShutdownHandlers();
        }

        public void Run()
        {
            try
            {
                _logger.Information("Starting Penumbra Mod Forwarder");
                _backgroundServiceProcess = StartProcess("PenumbraModForwarder.BackgroundWorker", _port.ToString());
                _uiProcess = StartProcess("PenumbraModForwarder.UI", _port.ToString());
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Failed to start Penumbra Mod Forwarder");
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
            _logger.Information("Starting: {ProjectName}", projectName);
            return _isDevMode ? StartDevProcess(projectName, port) : StartProdProcess($"{projectName}.exe", port);
        }

        private Process StartDevProcess(string projectName, string port)
        {
            string projectDirectory = Path.Combine(_solutionDirectory, projectName);
            string projectFilePath = Path.Combine(projectDirectory, $"{projectName}.csproj");

            if (!File.Exists(projectFilePath))
            {
                _logger.Error("Project file not found at {ProjectFilePath}", projectFilePath);
                throw new FileNotFoundException($"Project file not found: {projectFilePath}");
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{projectFilePath}\" -- {port}",
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = projectDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
            {
                startInfo.EnvironmentVariables[entry.Key.ToString()] = entry.Value?.ToString();
            }
            startInfo.EnvironmentVariables["WATCHDOG_INITIALIZED"] = "true";
            startInfo.EnvironmentVariables["DEV_MODE"] = "true";

            var process = new Process { StartInfo = startInfo };
            process.EnableRaisingEvents = true;

            // Attach event handlers to capture output and error streams
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    // Log the output from the child process
                    _logger.Debug("[{ProjectName} STDOUT]: {Data}", projectName, e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    // Log the error output from the child process
                    _logger.Error("[{ProjectName} STDERR]: {Data}", projectName, e.Data);
                }
            };

            process.Exited += (sender, e) =>
            {
                _logger.Information("{ProjectName} exited with code {ExitCode}", projectName, process.ExitCode);
            };

            process.Start();

            // Begin reading the output streams
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return process;
        }

        private Process StartProdProcess(string executableName, string port)
        {
            string executablePath = Path.Combine(AppContext.BaseDirectory, executableName);
            string executableDir = Path.GetDirectoryName(executablePath);

            _logger.Information("Executing executable in PROD Mode: {ExecutablePath}", executablePath);

            if (!File.Exists(executablePath))
            {
                _logger.Error("Executable not found at {ExecutablePath}", executablePath);
                throw new FileNotFoundException($"Executable not found: {executablePath}");
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = port,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = executableDir,
                RedirectStandardOutput = false,
                RedirectStandardError = false
            };

            startInfo.EnvironmentVariables["WATCHDOG_INITIALIZED"] = "true";

            var process = new Process { StartInfo = startInfo };
            process.EnableRaisingEvents = true;

            process.Exited += (sender, e) =>
            {
                _logger.Information("{ExecutableName} exited with code {ExitCode}", executableName, process.ExitCode);
            };

            process.Start();

            _logger.Information("Started {ExecutableName} (PID: {ProcessId})", executableName, process.Id);

            return process;
        }

        private void SetupShutdownHandlers()
        {
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                ShutdownChildProcesses();
            };

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                ShutdownChildProcesses();
            };
        }

        private void ShutdownChildProcesses()
        {
            if (_isShuttingDown) return;
            _isShuttingDown = true;

            _logger.Information("Initiating graceful shutdown of child processes...");

            try
            {
                if (_uiProcess != null && !_uiProcess.HasExited)
                {
                    _logger.Information("Closing UI Process (PID: {ProcessId})", _uiProcess.Id);
                    _uiProcess.Kill();
                }

                if (_backgroundServiceProcess != null && !_backgroundServiceProcess.HasExited)
                {
                    _logger.Information("Closing Background Worker Process (PID: {ProcessId})", _backgroundServiceProcess.Id);
                    _backgroundServiceProcess.Kill();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during shutdown");
            }
            finally
            {
                _logger.Information("Shutting down Penumbra Mod Forwarder");
                Environment.Exit(0);
            }
        }

        private void MonitorProcesses(Process uiProcess, Process backgroundServiceProcess)
        {
            while (!_isShuttingDown)
            {
                if (uiProcess.HasExited)
                {
                    _logger.Information("UI Process {ProcessId} exited with code {ExitCode}", uiProcess.Id, uiProcess.ExitCode);
                    if (backgroundServiceProcess != null && !backgroundServiceProcess.HasExited)
                    {
                        _logger.Information("Terminating Background Service (PID: {ProcessId}) due to UI exit", backgroundServiceProcess.Id);
                        backgroundServiceProcess.Kill();
                    }
                    ShutdownChildProcesses();
                    break;
                }

                if (backgroundServiceProcess.HasExited)
                {
                    _logger.Information("Background Service exited unexpectedly!");
                    backgroundServiceProcess = StartProcess("PenumbraModForwarder.BackgroundWorker", _port.ToString());
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