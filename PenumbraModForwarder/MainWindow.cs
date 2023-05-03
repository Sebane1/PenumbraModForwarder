using Anamnesis.Penumbra;
using System.Diagnostics;
using Microsoft.Win32;
using System.Reflection;
using AutoUpdaterDotNET;

// TODO: Rename to FFXIVModExtractor
namespace FFXIVModExractor {
    public partial class MainWindow : Form {
        bool exitInitiated = false;
        bool hideAfterLoad = false;
        public MainWindow() {
            InitializeComponent();
            GetDownloadPath();
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
            string[] arguments = Environment.GetCommandLineArgs();
            bool foundValidFile = false;
            if (arguments.Length > 0) {
                for (int i = 1; i < arguments.Length; i++) {
                    if (arguments[i].EndsWith(".pmp") || arguments[i].EndsWith(".ttmp") || arguments[i].EndsWith(".ttmp2")) {
                        PenumbraHttpApi.Install(arguments[i]);
                        PenumbraHttpApi.OpenWindow();
                        Thread.Sleep(10000);
                        foundValidFile = true;
                    }
                }
            }
            if (foundValidFile) {
                exitInitiated = true;
                this.Close();
                Application.Exit();
            } else {
                CheckForUpdate();
                GetAutoLoadOption();
                if (autoLoadModCheckbox.Checked) {
                    hideAfterLoad = true;
                }
            }
            ContextMenuStrip = contextMenu;
        }

        private void CheckForUpdate() {
            AutoUpdater.InstalledVersion = new Version(Application.ProductVersion);
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
        async Task ProcessModPackRequest(RenamedEventArgs e) {
            if (e.FullPath.EndsWith(".pmp") || e.FullPath.EndsWith(".ttmp") || e.FullPath.EndsWith(".ttmp2")) {
                Thread.Sleep(50);
                while(IsFileLocked(e.FullPath)) {
                    Thread.Sleep(100);
                }
                PenumbraHttpApi.Install(e.FullPath);
                PenumbraHttpApi.OpenWindow();
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
            } else {
                rk.DeleteValue(Text, false);
            }
            WriteAutoLoadOption(autoLoadModCheckbox.Checked);
        }

        private void associateFileTypes_Click(object sender, EventArgs e) {
            if (MessageBox.Show("Associate all .pmp, .ttmp, and .ttmp2 files to be redirected to penumbra via this program?",
                Text, MessageBoxButtons.YesNo) == DialogResult.Yes) {
                string myExecutable = Assembly.GetEntryAssembly().Location;
                string command = "\"" + myExecutable + "\"" + " \"%1\"";
                string keyName = "";
                try {
                    var pmp = Registry.ClassesRoot.OpenSubKey(".pmp");
                    var pmpType = pmp.GetValue("");
                    keyName = pmpType + @"\shell\Open\command";
                    using (var key = Registry.ClassesRoot.CreateSubKey(keyName)) {
                        key.SetValue("", command);
                    }
                } catch {
                    MessageBox.Show("Failed to set .pmp association. Try again with admin privileges or set this manually.", Text);
                }

                try {
                    var ttmp = Registry.ClassesRoot.OpenSubKey(".ttmp");
                    var ttmpType = ttmp.GetValue("");
                    keyName = ttmpType + @"\shell\Open\command";
                    using (var key = Registry.ClassesRoot.CreateSubKey(keyName)) {
                        key.SetValue("", command);
                    }
                } catch {
                    MessageBox.Show("Failed to set .ttmp association. Try again with admin privileges or set this manually.", Text);
                }

                try {
                    var ttmp2 = Registry.ClassesRoot.OpenSubKey(".ttmp2");
                    var ttmp2Type = ttmp2.GetValue("");
                    keyName = ttmp2Type + @"\shell\Open\command";
                    using (var key = Registry.ClassesRoot.CreateSubKey(keyName)) {
                        key.SetValue("", command);
                    }
                } catch {
                    MessageBox.Show("Failed to set .ttmp2 association. Try again with admin privileges or set this manually.", Text);
                }
                MessageBox.Show("Associations have been set!", Text);
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
    }
}