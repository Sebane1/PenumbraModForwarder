namespace FFXIVModExractor {
    partial class MainWindow {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Button associateFileTypes;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.fileSystemWatcher = new System.IO.FileSystemWatcher();
            this.xma = new System.Windows.Forms.Button();
            this.glamourDresser = new System.Windows.Forms.Button();
            this.nexusMods = new System.Windows.Forms.Button();
            this.aetherlink = new System.Windows.Forms.Button();
            this.kittyEmporium = new System.Windows.Forms.Button();
            this.downloads = new FFXIVVoicePackCreator.FilePicker();
            this.cooldownTimer = new System.Windows.Forms.Timer(this.components);
            this.autoLoadModCheckbox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.quickLinksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.xIVModArchiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.theGlamourDresserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nexusModsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aetherlinkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.thePrettyKittyEmporiumToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            associateFileTypes = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher)).BeginInit();
            this.contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // associateFileTypes
            // 
            associateFileTypes.Location = new System.Drawing.Point(0, 32);
            associateFileTypes.Name = "associateFileTypes";
            associateFileTypes.Size = new System.Drawing.Size(120, 23);
            associateFileTypes.TabIndex = 9;
            associateFileTypes.Text = "Associate Mod Files";
            associateFileTypes.UseVisualStyleBackColor = true;
            associateFileTypes.Click += new System.EventHandler(this.associateFileTypes_Click);
            // 
            // fileSystemWatcher
            // 
            this.fileSystemWatcher.EnableRaisingEvents = true;
            this.fileSystemWatcher.SynchronizingObject = this;
            this.fileSystemWatcher.Changed += new System.IO.FileSystemEventHandler(this.fileSystemWatcher_Created);
            this.fileSystemWatcher.Created += new System.IO.FileSystemEventHandler(this.fileSystemWatcher_Created);
            this.fileSystemWatcher.Renamed += new System.IO.RenamedEventHandler(this.fileSystemWatcher_Renamed);
            // 
            // xma
            // 
            this.xma.Location = new System.Drawing.Point(0, 92);
            this.xma.Name = "xma";
            this.xma.Size = new System.Drawing.Size(104, 23);
            this.xma.TabIndex = 2;
            this.xma.Text = "XIV Mod Archive";
            this.xma.UseVisualStyleBackColor = true;
            this.xma.Click += new System.EventHandler(this.xma_Click);
            // 
            // glamourDresser
            // 
            this.glamourDresser.Location = new System.Drawing.Point(104, 92);
            this.glamourDresser.Name = "glamourDresser";
            this.glamourDresser.Size = new System.Drawing.Size(124, 23);
            this.glamourDresser.TabIndex = 3;
            this.glamourDresser.Text = "The Glamour Dresser";
            this.glamourDresser.UseVisualStyleBackColor = true;
            this.glamourDresser.Click += new System.EventHandler(this.glamourDresser_Click);
            // 
            // nexusMods
            // 
            this.nexusMods.Location = new System.Drawing.Point(228, 92);
            this.nexusMods.Name = "nexusMods";
            this.nexusMods.Size = new System.Drawing.Size(84, 23);
            this.nexusMods.TabIndex = 4;
            this.nexusMods.Text = "Nexus Mods";
            this.nexusMods.UseVisualStyleBackColor = true;
            this.nexusMods.Click += new System.EventHandler(this.nexusMods_Click);
            // 
            // aetherlink
            // 
            this.aetherlink.Location = new System.Drawing.Point(0, 116);
            this.aetherlink.Name = "aetherlink";
            this.aetherlink.Size = new System.Drawing.Size(104, 23);
            this.aetherlink.TabIndex = 5;
            this.aetherlink.Text = "Aetherlink";
            this.aetherlink.UseVisualStyleBackColor = true;
            this.aetherlink.Click += new System.EventHandler(this.aetherlink_Click);
            // 
            // kittyEmporium
            // 
            this.kittyEmporium.Location = new System.Drawing.Point(104, 116);
            this.kittyEmporium.Name = "kittyEmporium";
            this.kittyEmporium.Size = new System.Drawing.Size(208, 23);
            this.kittyEmporium.TabIndex = 6;
            this.kittyEmporium.Text = "The Pretty Kitty Emporium";
            this.kittyEmporium.UseVisualStyleBackColor = true;
            this.kittyEmporium.Click += new System.EventHandler(this.kittyEmporium_Click);
            // 
            // downloads
            // 
            this.downloads.CurrentPath = null;
            this.downloads.Enabled = false;
            this.downloads.Filter = null;
            this.downloads.Index = -1;
            this.downloads.Location = new System.Drawing.Point(4, 4);
            this.downloads.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.downloads.MinimumSize = new System.Drawing.Size(300, 28);
            this.downloads.Name = "downloads";
            this.downloads.Size = new System.Drawing.Size(312, 28);
            this.downloads.TabIndex = 8;
            this.downloads.OnFileSelected += new System.EventHandler(this.downloads_OnFileSelected);
            // 
            // cooldownTimer
            // 
            this.cooldownTimer.Interval = 2000;
            this.cooldownTimer.Tick += new System.EventHandler(this.cooldownTimer_Tick);
            // 
            // autoLoadModCheckbox
            // 
            this.autoLoadModCheckbox.AutoSize = true;
            this.autoLoadModCheckbox.Location = new System.Drawing.Point(180, 36);
            this.autoLoadModCheckbox.Name = "autoLoadModCheckbox";
            this.autoLoadModCheckbox.Size = new System.Drawing.Size(131, 19);
            this.autoLoadModCheckbox.TabIndex = 10;
            this.autoLoadModCheckbox.Text = "Auto Forward Mods";
            this.autoLoadModCheckbox.UseVisualStyleBackColor = true;
            this.autoLoadModCheckbox.CheckedChanged += new System.EventHandler(this.autoLoadModCheckbox_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(0, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 25);
            this.label1.TabIndex = 12;
            this.label1.Text = "Quick links";
            // 
            // trayIcon
            // 
            this.trayIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.trayIcon.ContextMenuStrip = this.contextMenu;
            this.trayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("trayIcon.Icon")));
            this.trayIcon.Text = "Penumbra Mod Forwarder";
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.quickLinksToolStripMenuItem,
            this.openConfigurationToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.contextMenu.Name = "contextMenuStrip1";
            this.contextMenu.Size = new System.Drawing.Size(181, 70);
            // 
            // quickLinksToolStripMenuItem
            // 
            this.quickLinksToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.xIVModArchiveToolStripMenuItem,
            this.theGlamourDresserToolStripMenuItem,
            this.nexusModsToolStripMenuItem,
            this.aetherlinkToolStripMenuItem,
            this.thePrettyKittyEmporiumToolStripMenuItem});
            this.quickLinksToolStripMenuItem.Name = "quickLinksToolStripMenuItem";
            this.quickLinksToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.quickLinksToolStripMenuItem.Text = "Quick Links";
            // 
            // xIVModArchiveToolStripMenuItem
            // 
            this.xIVModArchiveToolStripMenuItem.Name = "xIVModArchiveToolStripMenuItem";
            this.xIVModArchiveToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.xIVModArchiveToolStripMenuItem.Text = "XIV Mod Archive";
            this.xIVModArchiveToolStripMenuItem.Click += new System.EventHandler(this.xma_Click);
            // 
            // theGlamourDresserToolStripMenuItem
            // 
            this.theGlamourDresserToolStripMenuItem.Name = "theGlamourDresserToolStripMenuItem";
            this.theGlamourDresserToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.theGlamourDresserToolStripMenuItem.Text = "The Glamour Dresser";
            this.theGlamourDresserToolStripMenuItem.Click += new System.EventHandler(this.glamourDresser_Click);
            // 
            // nexusModsToolStripMenuItem
            // 
            this.nexusModsToolStripMenuItem.Name = "nexusModsToolStripMenuItem";
            this.nexusModsToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.nexusModsToolStripMenuItem.Text = "Nexus Mods";
            this.nexusModsToolStripMenuItem.Click += new System.EventHandler(this.nexusMods_Click);
            // 
            // aetherlinkToolStripMenuItem
            // 
            this.aetherlinkToolStripMenuItem.Name = "aetherlinkToolStripMenuItem";
            this.aetherlinkToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.aetherlinkToolStripMenuItem.Text = "Aetherlink";
            this.aetherlinkToolStripMenuItem.Click += new System.EventHandler(this.aetherlink_Click);
            // 
            // thePrettyKittyEmporiumToolStripMenuItem
            // 
            this.thePrettyKittyEmporiumToolStripMenuItem.Name = "thePrettyKittyEmporiumToolStripMenuItem";
            this.thePrettyKittyEmporiumToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.thePrettyKittyEmporiumToolStripMenuItem.Text = "The Pretty Kitty Emporium";
            this.thePrettyKittyEmporiumToolStripMenuItem.Click += new System.EventHandler(this.kittyEmporium_Click);
            // 
            // openConfigurationToolStripMenuItem
            // 
            this.openConfigurationToolStripMenuItem.Name = "openConfigurationToolStripMenuItem";
            this.openConfigurationToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openConfigurationToolStripMenuItem.Text = "Open Configuration";
            this.openConfigurationToolStripMenuItem.Click += new System.EventHandler(this.openConfigurationToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(316, 140);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.autoLoadModCheckbox);
            this.Controls.Add(associateFileTypes);
            this.Controls.Add(this.downloads);
            this.Controls.Add(this.kittyEmporium);
            this.Controls.Add(this.aetherlink);
            this.Controls.Add(this.nexusMods);
            this.Controls.Add(this.glamourDresser);
            this.Controls.Add(this.xma);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.Text = "Penumbra Mod Forwarder";
            this.Activated += new System.EventHandler(this.MainWindow_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.Load += new System.EventHandler(this.MainWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher)).EndInit();
            this.contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private FileSystemWatcher fileSystemWatcher;
        private Button kittyEmporium;
        private Button aetherlink;
        private Button nexusMods;
        private Button glamourDresser;
        private Button xma;
        private FFXIVVoicePackCreator.FilePicker downloads;
        private System.Windows.Forms.Timer cooldownTimer;
        private Label label1;
        private CheckBox autoLoadModCheckbox;
        private NotifyIcon trayIcon;
        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem quickLinksToolStripMenuItem;
        private ToolStripMenuItem xIVModArchiveToolStripMenuItem;
        private ToolStripMenuItem theGlamourDresserToolStripMenuItem;
        private ToolStripMenuItem nexusModsToolStripMenuItem;
        private ToolStripMenuItem aetherlinkToolStripMenuItem;
        private ToolStripMenuItem thePrettyKittyEmporiumToolStripMenuItem;
        private ToolStripMenuItem openConfigurationToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
    }
}