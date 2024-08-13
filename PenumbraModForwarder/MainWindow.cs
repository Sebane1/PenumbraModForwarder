using Anamnesis.Penumbra;
using System.Diagnostics;
using Microsoft.Win32;
using System.Reflection;
using AutoUpdaterDotNET;
using PenumbraModForwarder;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Configuration;
using System.IO.Compression;
using FFXIVVoicePackCreator.Json;
using Newtonsoft.Json;

// TODO: Rename to FFXIVModExtractor
namespace FFXIVModExractor {
    public partial class MainWindow : Form {
        bool exitInitiated = false;
        bool hideAfterLoad = false;
        private string roleplayingVoiceCache;
        private string _textoolsPath;

        public MainWindow() {
            InitializeComponent();
            GetDownloadPath();
            AutoScaleDimensions = new SizeF(96, 96);
        }

        private void filePicker1_Load(object sender, EventArgs e) {

        }

        private void xma_Click(object sender, EventArgs e) {
            try {
                Process.Start(new System.Diagnostics.ProcessStartInfo() {
                    FileName = "https://www.xivmodarchive.com/",
                    UseShellExecute = true,
                    Verb = "OPEN"
                });
            } catch {

            }
        }

        private void glamourDresser_Click(object sender, EventArgs e) {
            try {
                Process.Start(new System.Diagnostics.ProcessStartInfo() {
                    FileName = "https://www.glamourdresser.com/",
                    UseShellExecute = true,
                    Verb = "OPEN"
                });
            } catch {

            }
        }

        private void nexusMods_Click(object sender, EventArgs e) {
            try {
                Process.Start(new System.Diagnostics.ProcessStartInfo() {
                    FileName = "https://www.nexusmods.com/finalfantasy14",
                    UseShellExecute = true,
                    Verb = "OPEN"
                });
            } catch {

            }
        }

        private void aetherlink_Click(object sender, EventArgs e) {
            try {
                Process.Start(new System.Diagnostics.ProcessStartInfo() {
                    FileName = "https://beta.aetherlink.app/",
                    UseShellExecute = true,
                    Verb = "OPEN"
                });
            } catch {

            }
        }

        private void kittyEmporium_Click(object sender, EventArgs e) {
            try {
                Process.Start(new System.Diagnostics.ProcessStartInfo() {
                    FileName = "https://prettykittyemporium.blogspot.com/?zx=67bbd385fd16c2ff",
                    UseShellExecute = true,
                    Verb = "OPEN"
                });
            } catch {

            }
        }

        private void downloads_OnFileSelected(object sender, EventArgs e) {
            fileSystemWatcher.Path = downloads.FilePath.Text;
            WriteDownloadPath(downloads.FilePath.Text);
        }

        private void MainWindow_Load(object sender, EventArgs e) {
            // If this path is not found textools reliant functions will be disabled until textools is installed.
            var textoolsInk = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"Microsoft\Windows\Start Menu\Programs\FFXIV TexTools\FFXIV TexTools.lnk");
            if (File.Exists(textoolsInk)) {
                IWshRuntimeLibrary.IWshShell wsh = new IWshRuntimeLibrary.WshShellClass();
                IWshRuntimeLibrary.IWshShortcut sc = (IWshRuntimeLibrary.IWshShortcut)wsh.CreateShortcut(textoolsInk);
                var texToolsDirectory = Path.GetDirectoryName(sc.TargetPath);
                _textoolsPath = Path.Combine(texToolsDirectory, "ConsoleTools.exe");
            }
            string[] arguments = Environment.GetCommandLineArgs();
            bool foundValidFile = false;
            if (arguments.Length > 0) {
                for (int i = 1; i < arguments.Length; i++) {
                    if (arguments[i].EndsWith(".pmp") || arguments[i].EndsWith(".ttmp") || arguments[i].EndsWith(".ttmp2")) {
                        SendModToPenumbra(arguments[i], ref foundValidFile);
                    }
                    //if (arguments[i].EndsWith(".ttmp") || arguments[i].EndsWith(".ttmp2")) {
                    //    AppSelectionsWindow appSelectionsWindow = new AppSelectionsWindow();
                    //    if (File.Exists(_textoolsPath) || appSelectionsWindow.ShowDialog() == DialogResult.OK) {
                    //        switch (appSelectionsWindow.AppSelection) {
                    //            case AppSelectionsWindow.AppSelectionType.penumbra:
                    //                SendModToPenumbra(arguments[i], ref foundValidFile);
                    //                break;
                    //            case AppSelectionsWindow.AppSelectionType.textools:

                    //                foundValidFile = true;
                    //                break;
                    //        }
                    //    }
                    //}
                }
            }
            if (foundValidFile) {
                exitInitiated = true;
                this.Close();
                Application.Exit();
            } else {
                Process[] processes = Process.GetProcessesByName(Application.ProductName);
                if (processes.Length == 1) {
                    CheckForUpdate();
                    GetAutoLoadOption();
                    if (autoLoadModCheckbox.Checked) {
                        hideAfterLoad = true;
                    }
                } else {
                    MessageBox.Show("Penumbra Mod Forward is already running.", Text);
                    exitInitiated = true;
                    this.Close();
                    Application.Exit();
                }
            }
            ContextMenuStrip = contextMenu;
        }

        private void SendModToPenumbra(string modPackPath, ref bool foundValidFile) {
            string finalModPath = modPackPath;
            if (File.Exists(_textoolsPath)) {
                string originatingModDirectory = Path.GetDirectoryName(modPackPath);
                var outputModName = modPackPath
                .Replace(".pmp", "_dt.pmp").Replace(".ttmp", "_dt.ttmp");
                Directory.CreateDirectory(Path.Combine(originatingModDirectory, @"Dawntrail Converted\"));
                finalModPath = Path.Combine(originatingModDirectory, @"Dawntrail Converted\" + Path.GetFileName(outputModName));
                trayIcon.BalloonTipText = "Mod pack has been sent to textools for Dawntrail conversion.";
                trayIcon.ShowBalloonTip(5000);
                Process process = new Process();
                process.StartInfo.FileName = _textoolsPath;
                //process.StartInfo.Verb = "runas";
                process.StartInfo.WorkingDirectory = Path.GetDirectoryName(_textoolsPath);
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.Arguments = @"/upgrade """ + modPackPath + @""" " + @"""" + finalModPath + @"""";
                process.Start();
                process.WaitForExit();
                if (!File.Exists(finalModPath)) {
                    finalModPath = modPackPath;
                    trayIcon.BalloonTipText = "Mod pac was not converted to Dawntrail.";
                    trayIcon.ShowBalloonTip(5000);
                }
            }
            PenumbraHttpApi.OpenWindow();
            PenumbraHttpApi.Install(finalModPath);
            trayIcon.BalloonTipText = "Mod pack has been sent to penumbra.";
            trayIcon.ShowBalloonTip(5000);
            Thread.Sleep(10000);
            foundValidFile = true;
        }

        private void CheckForUpdate() {
            AutoUpdater.InstalledVersion = new Version(Application.ProductVersion.Split("+")[0]);
            AutoUpdater.DownloadPath = Application.StartupPath;
            AutoUpdater.Synchronous = true;
            AutoUpdater.Mandatory = true;
            AutoUpdater.UpdateMode = Mode.ForcedDownload;
            AutoUpdater.Start("https://raw.githubusercontent.com/Sebane1/PenumbraModForwarder/master/update.xml");
            AutoUpdater.ApplicationExitEvent += delegate () {
                hideAfterLoad = true;
                exitInitiated = true;
            };
        }

        private void fileSystemWatcher_Renamed(object sender, RenamedEventArgs e) {
            ProcessModPackRequest(e);
        }
        private void RoleplayingVoiceCheck() {
            string roleplayingVoiceConfig = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
         + @"\XIVLauncher\pluginConfigs\RoleplayingVoiceDalamud.json";
            if (File.Exists(roleplayingVoiceConfig)) {
                RoleplayingVoiceConfig file = JsonConvert.DeserializeObject<RoleplayingVoiceConfig>(
                    File.OpenText(roleplayingVoiceConfig).ReadToEnd());
                roleplayingVoiceCache = file.CacheFolder;
            }
        }
        async Task ProcessModPackRequest(RenamedEventArgs e) {
            if (e.FullPath.EndsWith(".pmp") || e.FullPath.EndsWith(".ttmp") || e.FullPath.EndsWith(".ttmp2")) {
                Thread.Sleep(50);
                while (IsFileLocked(e.FullPath)) {
                    Thread.Sleep(100);
                }
                bool value = false;
                SendModToPenumbra(e.FullPath, ref value);
            } else if (e.FullPath.EndsWith(".rpvsp")) {
                RoleplayingVoiceCheck();
                if (!string.IsNullOrEmpty(roleplayingVoiceCache)) {
                    Thread.Sleep(50);
                    while (IsFileLocked(e.FullPath)) {
                        Thread.Sleep(100);
                    }
                    string directory = roleplayingVoiceCache + @"\VoicePack\" + Path.GetFileNameWithoutExtension(e.FullPath);
                    ZipFile.ExtractToDirectory(e.FullPath, directory);
                    trayIcon.BalloonTipText = "Mod has been sent to Artemis Roleplaying Kit";
                    trayIcon.ShowBalloonTip(5000);
                } else {
                    if (MessageBox.Show("This mod requires the Artemis Roleplaying Kit dalamud plugin to be installed. Would you like to install it now?",
                        "Penumbra Mod Forwarder", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                        try {
                            Process.Start(new System.Diagnostics.ProcessStartInfo() {
                                FileName = "https://github.com/Sebane1/RoleplayingVoiceDalamud",
                                UseShellExecute = true,
                                Verb = "OPEN"
                            });
                        } catch {

                        }
                    }
                }
            }
        }

        public static bool IsFileLocked(string file) {
            try {
                using (FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None)) {
                    stream.Close();
                }
            } catch (IOException) {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }
        public void GetDownloadPath() {
            string dataPath = Application.UserAppDataPath.Replace(Application.ProductVersion, null);
            string path = Path.Combine(dataPath, @"DownloadPath.config");
            if (File.Exists(path)) {
                using (StreamReader reader = new StreamReader(path)) {
                    downloads.CurrentPath = reader.ReadLine();
                    fileSystemWatcher.Path = downloads.FilePath.Text;
                }
            }
        }

        public void WriteDownloadPath(string path) {
            string dataPath = Application.UserAppDataPath.Replace(Application.ProductVersion, null);
            using (StreamWriter writer = new StreamWriter(Path.Combine(dataPath, @"DownloadPath.config"))) {
                writer.WriteLine(path);
            }
        }

        public void WriteTexToolsPath(string path) {
            string dataPath = Application.UserAppDataPath.Replace(Application.ProductVersion, null);
            using (StreamWriter writer = new StreamWriter(Path.Combine(dataPath, @"TexTools.config"))) {
                writer.WriteLine(path);
            }
        }

        private static void RegisterForFileExtension(string extension, string applicationPath) {
            RegistryKey FileReg = Registry.CurrentUser.CreateSubKey("Software\\Classes\\" + extension);
            FileReg.CreateSubKey("shell\\open\\command").SetValue("", $"\"{applicationPath}\" \"%1\"");
            FileReg.Close();

            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        }
        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        public void GetAutoLoadOption() {
            string dataPath = Application.UserAppDataPath.Replace(Application.ProductVersion, null);
            string path = Path.Combine(dataPath, @"AutoLoad.config");
            if (File.Exists(path)) {
                using (StreamReader reader = new StreamReader(path)) {
                    autoLoadModCheckbox.Checked = bool.Parse(reader.ReadLine());
                }
            }
        }

        public void WriteAutoLoadOption(bool option) {
            try {
                string dataPath = Application.UserAppDataPath.Replace(Application.ProductVersion, null);
                using (StreamWriter writer = new StreamWriter(Path.Combine(dataPath, @"AutoLoad.config"))) {
                    writer.WriteLine(option);
                }
            } catch {

            }
        }

        private void cooldownTimer_Tick(object sender, EventArgs e) {
            cooldownTimer.Enabled = false;
        }

        private void autoLoadModCheckbox_CheckedChanged(object sender, EventArgs e) {
            downloads.Enabled = autoLoadModCheckbox.Checked;
            trayIcon.Visible = autoLoadModCheckbox.Checked;
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (autoLoadModCheckbox.Checked) {
                rk.SetValue(Text, Application.ExecutablePath);
                trayIcon.BalloonTipText = "Penumbra Mod Forwarder will now appear in the system tray!";
                trayIcon.ShowBalloonTip(5000);
            } else {
                rk.DeleteValue(Text, false);
            }
            WriteAutoLoadOption(autoLoadModCheckbox.Checked);
        }

        private void associateFileTypes_Click(object sender, EventArgs e) {
            if (MessageBox.Show("Associate all .pmp, .ttmp, .ttmp2, and .rpvsp files to be redirected via this program?",
                Text, MessageBoxButtons.YesNo) == DialogResult.Yes) {
                string myExecutable = Assembly.GetEntryAssembly().Location;
                string command = "\"" + myExecutable + "\"" + " \"%1\"";
                string keyName = "";
                try {
                    RegisterForFileExtension(".pmp", command);
                } catch {
                    MessageBox.Show("Failed to set .pmp association. Try again with admin privileges or set this manually.", Text);
                }

                try {
                    RegisterForFileExtension(".ttmp", command);
                } catch {
                    MessageBox.Show("Failed to set .ttmp association. Try again with admin privileges or set this manually.", Text);
                }

                try {
                    RegisterForFileExtension(".ttmp2", command);
                } catch {
                    MessageBox.Show("Failed to set .ttmp2 association. Try again with admin privileges or set this manually.", Text);
                }
                try {
                    RegisterForFileExtension(".rpvsp", command);
                } catch {
                    MessageBox.Show("Failed to set .rpvsp association. Try again with admin privileges or set this manually.", Text);
                }
                MessageBox.Show("Associations have been added!", Text);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            exitInitiated = true;
            Application.Exit();
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e) {
            if (autoLoadModCheckbox.Checked && !exitInitiated) {
                Hide();
                e.Cancel = true;
            }
        }

        private void openConfigurationToolStripMenuItem_Click(object sender, EventArgs e) {
            Show();
            TopMost = true;
            BringToFront();
        }

        private void MainWindow_Activated(object sender, EventArgs e) {
            if (hideAfterLoad) {
                SendToBack();
                Hide();
                hideAfterLoad = false;
            } else {
                TopMost = true;
                BringToFront();
                TopMost = false;
            }
        }

        private void looseTextureCompilerToolStripMenuItem_Click(object sender, EventArgs e) {
            try {
                Process.Start(new System.Diagnostics.ProcessStartInfo() {
                    FileName = "https://github.com/Sebane1/FFXIVLooseTextureCompiler/",
                    UseShellExecute = true,
                    Verb = "OPEN"
                });
            } catch {

            }
        }

        private void voicePackCreatorToolStripMenuItem_Click(object sender, EventArgs e) {
            try {
                Process.Start(new System.Diagnostics.ProcessStartInfo() {
                    FileName = "https://github.com/Sebane1/FFXIVVoicePackCreator/",
                    UseShellExecute = true,
                    Verb = "OPEN"
                });
            } catch {

            }
        }

        private void heliosphereToolStripMenuItem_Click(object sender, EventArgs e) {
            try {
                if (MessageBox.Show("Heliosphere requires a separate dalamud plugin to use.", Text) == DialogResult.OK) {
                    Process.Start(new System.Diagnostics.ProcessStartInfo() {
                        FileName = "https://heliosphere.app/",
                        UseShellExecute = true,
                        Verb = "OPEN"
                    });
                }
            } catch {

            }
        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e) {
            Show();
            TopMost = true;
            BringToFront();
        }

        private void soundsToolStripMenuItem_Click(object sender, EventArgs e) {

        }

        private void penumbraToolStripMenuItem_Click(object sender, EventArgs e) {
            try {
                Process.Start(new System.Diagnostics.ProcessStartInfo() {
                    FileName = "https://www.xivmodarchive.com/",
                    UseShellExecute = true,
                    Verb = "OPEN"
                });
            } catch {

            }
        }

        private void pixellatedsAssistancePlaceToolStripMenuItem_Click(object sender, EventArgs e) {
            try {
                Process.Start(new System.Diagnostics.ProcessStartInfo() {
                    FileName = "https://discord.gg/9XtTqws2cJ",
                    UseShellExecute = true,
                    Verb = "OPEN"
                });
            } catch {

            }
        }

        private void soundAndTextureResourceToolStripMenuItem_Click(object sender, EventArgs e) {
            try {
                Process.Start(new System.Diagnostics.ProcessStartInfo() {
                    FileName = "https://discord.gg/rtGXwMn7pX",
                    UseShellExecute = true,
                    Verb = "OPEN"
                });
            } catch {

            }
        }

        private void texToolsToolStripMenuItem_Click(object sender, EventArgs e) {
            try {
                Process.Start(new System.Diagnostics.ProcessStartInfo() {
                    FileName = "https://discord.gg/ffxivtextools",
                    UseShellExecute = true,
                    Verb = "OPEN"
                });
            } catch {

            }
        }

        private void xIVModsResourcesToolStripMenuItem_Click(object sender, EventArgs e) {
            try {
                Process.Start(new System.Diagnostics.ProcessStartInfo() {
                    FileName = "https://discord.gg/8x2G75D46w",
                    UseShellExecute = true,
                    Verb = "OPEN"
                });
            } catch {

            }
        }

        private void crossGenPortingToolToolStripMenuItem_Click(object sender, EventArgs e) {
            try {
                Process.Start(new System.Diagnostics.ProcessStartInfo() {
                    FileName = "https://www.xivmodarchive.com/modid/56505",
                    UseShellExecute = true,
                    Verb = "OPEN"
                });
            } catch {

            }
        }

        private void checkForUpdateToolStripMenuItem_Click(object sender, EventArgs e) {
            CheckForUpdate();
        }

        private void donateButton_Click(object sender, EventArgs e) {
            try {
                Process.Start(new System.Diagnostics.ProcessStartInfo() {
                    FileName = "https://ko-fi.com/sebastina",
                    UseShellExecute = true,
                    Verb = "OPEN"
                });
            } catch {

            }
        }

        private void discordButton_Click(object sender, EventArgs e) {
            try {
                Process.Start(new System.Diagnostics.ProcessStartInfo() {
                    FileName = "https://discord.gg/rtGXwMn7pX",
                    UseShellExecute = true,
                    Verb = "OPEN"
                });
            } catch {

            }
        }
    }
}