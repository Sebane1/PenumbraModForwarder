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
            this.aetherlinkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.heliosphereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nexusModsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.theGlamourDresserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.thePrettyKittyEmporiumToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.xIVModArchiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moddingResourcesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.texturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.looseTextureCompilerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toBeExpandedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.soundsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.voicePackCreatorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toBeExpandedToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.modelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.crossGenPortingToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toBeExpandedToolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.discordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.penumbraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pixellatedsAssistancePlaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.soundAndTextureResourceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.texToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.xIVModsResourcesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toBeExpandedToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.openConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.heliopshereButton = new System.Windows.Forms.Button();
            this.checkForUpdateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.aetherlink.Size = new System.Drawing.Size(72, 23);
            this.aetherlink.TabIndex = 5;
            this.aetherlink.Text = "Aetherlink";
            this.aetherlink.UseVisualStyleBackColor = true;
            this.aetherlink.Click += new System.EventHandler(this.aetherlink_Click);
            // 
            // kittyEmporium
            // 
            this.kittyEmporium.Location = new System.Drawing.Point(156, 116);
            this.kittyEmporium.Name = "kittyEmporium";
            this.kittyEmporium.Size = new System.Drawing.Size(156, 23);
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
            this.cooldownTimer.Interval = 10;
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
            this.trayIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.trayIcon_MouseDoubleClick);
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.quickLinksToolStripMenuItem,
            this.moddingResourcesToolStripMenuItem,
            this.openConfigurationToolStripMenuItem,
            this.checkForUpdateToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.contextMenu.Name = "contextMenuStrip1";
            this.contextMenu.Size = new System.Drawing.Size(181, 136);
            // 
            // quickLinksToolStripMenuItem
            // 
            this.quickLinksToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aetherlinkToolStripMenuItem,
            this.heliosphereToolStripMenuItem,
            this.nexusModsToolStripMenuItem,
            this.theGlamourDresserToolStripMenuItem,
            this.thePrettyKittyEmporiumToolStripMenuItem,
            this.xIVModArchiveToolStripMenuItem});
            this.quickLinksToolStripMenuItem.Name = "quickLinksToolStripMenuItem";
            this.quickLinksToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.quickLinksToolStripMenuItem.Text = "Quick Links";
            // 
            // aetherlinkToolStripMenuItem
            // 
            this.aetherlinkToolStripMenuItem.Name = "aetherlinkToolStripMenuItem";
            this.aetherlinkToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.aetherlinkToolStripMenuItem.Text = "Aetherlink";
            this.aetherlinkToolStripMenuItem.Click += new System.EventHandler(this.aetherlink_Click);
            // 
            // heliosphereToolStripMenuItem
            // 
            this.heliosphereToolStripMenuItem.Name = "heliosphereToolStripMenuItem";
            this.heliosphereToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.heliosphereToolStripMenuItem.Text = "Heliosphere";
            this.heliosphereToolStripMenuItem.Click += new System.EventHandler(this.heliosphereToolStripMenuItem_Click);
            // 
            // nexusModsToolStripMenuItem
            // 
            this.nexusModsToolStripMenuItem.Name = "nexusModsToolStripMenuItem";
            this.nexusModsToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.nexusModsToolStripMenuItem.Text = "Nexus Mods";
            this.nexusModsToolStripMenuItem.Click += new System.EventHandler(this.nexusMods_Click);
            // 
            // theGlamourDresserToolStripMenuItem
            // 
            this.theGlamourDresserToolStripMenuItem.Name = "theGlamourDresserToolStripMenuItem";
            this.theGlamourDresserToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.theGlamourDresserToolStripMenuItem.Text = "The Glamour Dresser";
            this.theGlamourDresserToolStripMenuItem.Click += new System.EventHandler(this.glamourDresser_Click);
            // 
            // thePrettyKittyEmporiumToolStripMenuItem
            // 
            this.thePrettyKittyEmporiumToolStripMenuItem.Name = "thePrettyKittyEmporiumToolStripMenuItem";
            this.thePrettyKittyEmporiumToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.thePrettyKittyEmporiumToolStripMenuItem.Text = "The Pretty Kitty Emporium";
            this.thePrettyKittyEmporiumToolStripMenuItem.Click += new System.EventHandler(this.kittyEmporium_Click);
            // 
            // xIVModArchiveToolStripMenuItem
            // 
            this.xIVModArchiveToolStripMenuItem.Name = "xIVModArchiveToolStripMenuItem";
            this.xIVModArchiveToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.xIVModArchiveToolStripMenuItem.Text = "XIV Mod Archive";
            this.xIVModArchiveToolStripMenuItem.Click += new System.EventHandler(this.xma_Click);
            // 
            // moddingResourcesToolStripMenuItem
            // 
            this.moddingResourcesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.texturesToolStripMenuItem,
            this.soundsToolStripMenuItem,
            this.modelsToolStripMenuItem,
            this.discordToolStripMenuItem});
            this.moddingResourcesToolStripMenuItem.Name = "moddingResourcesToolStripMenuItem";
            this.moddingResourcesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.moddingResourcesToolStripMenuItem.Text = "Modding Resources";
            // 
            // texturesToolStripMenuItem
            // 
            this.texturesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.looseTextureCompilerToolStripMenuItem,
            this.toBeExpandedToolStripMenuItem});
            this.texturesToolStripMenuItem.Name = "texturesToolStripMenuItem";
            this.texturesToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.texturesToolStripMenuItem.Text = "Textures";
            // 
            // looseTextureCompilerToolStripMenuItem
            // 
            this.looseTextureCompilerToolStripMenuItem.Name = "looseTextureCompilerToolStripMenuItem";
            this.looseTextureCompilerToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.looseTextureCompilerToolStripMenuItem.Text = "Loose Texture Compiler";
            this.looseTextureCompilerToolStripMenuItem.Click += new System.EventHandler(this.looseTextureCompilerToolStripMenuItem_Click);
            // 
            // toBeExpandedToolStripMenuItem
            // 
            this.toBeExpandedToolStripMenuItem.Enabled = false;
            this.toBeExpandedToolStripMenuItem.Name = "toBeExpandedToolStripMenuItem";
            this.toBeExpandedToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.toBeExpandedToolStripMenuItem.Text = "--To Be Expanded--";
            // 
            // soundsToolStripMenuItem
            // 
            this.soundsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.voicePackCreatorToolStripMenuItem,
            this.toBeExpandedToolStripMenuItem1});
            this.soundsToolStripMenuItem.Name = "soundsToolStripMenuItem";
            this.soundsToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.soundsToolStripMenuItem.Text = "Sounds";
            this.soundsToolStripMenuItem.Click += new System.EventHandler(this.soundsToolStripMenuItem_Click);
            // 
            // voicePackCreatorToolStripMenuItem
            // 
            this.voicePackCreatorToolStripMenuItem.Name = "voicePackCreatorToolStripMenuItem";
            this.voicePackCreatorToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.voicePackCreatorToolStripMenuItem.Text = "Voice Pack Creator";
            this.voicePackCreatorToolStripMenuItem.Click += new System.EventHandler(this.voicePackCreatorToolStripMenuItem_Click);
            // 
            // toBeExpandedToolStripMenuItem1
            // 
            this.toBeExpandedToolStripMenuItem1.Enabled = false;
            this.toBeExpandedToolStripMenuItem1.Name = "toBeExpandedToolStripMenuItem1";
            this.toBeExpandedToolStripMenuItem1.Size = new System.Drawing.Size(177, 22);
            this.toBeExpandedToolStripMenuItem1.Text = "--To Be Expanded--";
            // 
            // modelsToolStripMenuItem
            // 
            this.modelsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.crossGenPortingToolToolStripMenuItem,
            this.toBeExpandedToolStripMenuItem3});
            this.modelsToolStripMenuItem.Name = "modelsToolStripMenuItem";
            this.modelsToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.modelsToolStripMenuItem.Text = "Models";
            // 
            // crossGenPortingToolToolStripMenuItem
            // 
            this.crossGenPortingToolToolStripMenuItem.Name = "crossGenPortingToolToolStripMenuItem";
            this.crossGenPortingToolToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.crossGenPortingToolToolStripMenuItem.Text = "Cross Gen Porting Tool";
            this.crossGenPortingToolToolStripMenuItem.Click += new System.EventHandler(this.crossGenPortingToolToolStripMenuItem_Click);
            // 
            // toBeExpandedToolStripMenuItem3
            // 
            this.toBeExpandedToolStripMenuItem3.Enabled = false;
            this.toBeExpandedToolStripMenuItem3.Name = "toBeExpandedToolStripMenuItem3";
            this.toBeExpandedToolStripMenuItem3.Size = new System.Drawing.Size(194, 22);
            this.toBeExpandedToolStripMenuItem3.Text = "--To Be Expanded--";
            // 
            // discordToolStripMenuItem
            // 
            this.discordToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.penumbraToolStripMenuItem,
            this.pixellatedsAssistancePlaceToolStripMenuItem,
            this.soundAndTextureResourceToolStripMenuItem,
            this.texToolsToolStripMenuItem,
            this.xIVModsResourcesToolStripMenuItem,
            this.toBeExpandedToolStripMenuItem2});
            this.discordToolStripMenuItem.Name = "discordToolStripMenuItem";
            this.discordToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.discordToolStripMenuItem.Text = "Discords";
            // 
            // penumbraToolStripMenuItem
            // 
            this.penumbraToolStripMenuItem.Name = "penumbraToolStripMenuItem";
            this.penumbraToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.penumbraToolStripMenuItem.Text = "Penumbra";
            this.penumbraToolStripMenuItem.Click += new System.EventHandler(this.penumbraToolStripMenuItem_Click);
            // 
            // pixellatedsAssistancePlaceToolStripMenuItem
            // 
            this.pixellatedsAssistancePlaceToolStripMenuItem.Name = "pixellatedsAssistancePlaceToolStripMenuItem";
            this.pixellatedsAssistancePlaceToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.pixellatedsAssistancePlaceToolStripMenuItem.Text = "Pixellated\'s Assistance Place";
            this.pixellatedsAssistancePlaceToolStripMenuItem.Click += new System.EventHandler(this.pixellatedsAssistancePlaceToolStripMenuItem_Click);
            // 
            // soundAndTextureResourceToolStripMenuItem
            // 
            this.soundAndTextureResourceToolStripMenuItem.Name = "soundAndTextureResourceToolStripMenuItem";
            this.soundAndTextureResourceToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.soundAndTextureResourceToolStripMenuItem.Text = "Sound && Texture Resource";
            this.soundAndTextureResourceToolStripMenuItem.Click += new System.EventHandler(this.soundAndTextureResourceToolStripMenuItem_Click);
            // 
            // texToolsToolStripMenuItem
            // 
            this.texToolsToolStripMenuItem.Name = "texToolsToolStripMenuItem";
            this.texToolsToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.texToolsToolStripMenuItem.Text = "TexTools";
            this.texToolsToolStripMenuItem.Click += new System.EventHandler(this.texToolsToolStripMenuItem_Click);
            // 
            // xIVModsResourcesToolStripMenuItem
            // 
            this.xIVModsResourcesToolStripMenuItem.Name = "xIVModsResourcesToolStripMenuItem";
            this.xIVModsResourcesToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.xIVModsResourcesToolStripMenuItem.Text = "XIV Mods && Resources";
            this.xIVModsResourcesToolStripMenuItem.Click += new System.EventHandler(this.xIVModsResourcesToolStripMenuItem_Click);
            // 
            // toBeExpandedToolStripMenuItem2
            // 
            this.toBeExpandedToolStripMenuItem2.Enabled = false;
            this.toBeExpandedToolStripMenuItem2.Name = "toBeExpandedToolStripMenuItem2";
            this.toBeExpandedToolStripMenuItem2.Size = new System.Drawing.Size(222, 22);
            this.toBeExpandedToolStripMenuItem2.Text = "--To Be Expanded--";
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
            // heliopshereButton
            // 
            this.heliopshereButton.Location = new System.Drawing.Point(72, 116);
            this.heliopshereButton.Name = "heliopshereButton";
            this.heliopshereButton.Size = new System.Drawing.Size(84, 23);
            this.heliopshereButton.TabIndex = 13;
            this.heliopshereButton.Text = "Heliosphere";
            this.heliopshereButton.UseVisualStyleBackColor = true;
            this.heliopshereButton.Click += new System.EventHandler(this.heliosphereToolStripMenuItem_Click);
            // 
            // checkForUpdateToolStripMenuItem
            // 
            this.checkForUpdateToolStripMenuItem.Name = "checkForUpdateToolStripMenuItem";
            this.checkForUpdateToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.checkForUpdateToolStripMenuItem.Text = "Check For Update";
            this.checkForUpdateToolStripMenuItem.Click += new System.EventHandler(this.checkForUpdateToolStripMenuItem_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(316, 140);
            this.Controls.Add(this.heliopshereButton);
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
        private ToolStripMenuItem moddingResourcesToolStripMenuItem;
        private ToolStripMenuItem texturesToolStripMenuItem;
        private ToolStripMenuItem looseTextureCompilerToolStripMenuItem;
        private ToolStripMenuItem soundsToolStripMenuItem;
        private ToolStripMenuItem voicePackCreatorToolStripMenuItem;
        private ToolStripMenuItem heliosphereToolStripMenuItem;
        private Button heliopshereButton;
        private ToolStripMenuItem toBeExpandedToolStripMenuItem;
        private ToolStripMenuItem toBeExpandedToolStripMenuItem1;
        private ToolStripMenuItem discordToolStripMenuItem;
        private ToolStripMenuItem penumbraToolStripMenuItem;
        private ToolStripMenuItem soundAndTextureResourceToolStripMenuItem;
        private ToolStripMenuItem pixellatedsAssistancePlaceToolStripMenuItem;
        private ToolStripMenuItem texToolsToolStripMenuItem;
        private ToolStripMenuItem xIVModsResourcesToolStripMenuItem;
        private ToolStripMenuItem toBeExpandedToolStripMenuItem2;
        private ToolStripMenuItem modelsToolStripMenuItem;
        private ToolStripMenuItem crossGenPortingToolToolStripMenuItem;
        private ToolStripMenuItem toBeExpandedToolStripMenuItem3;
        private ToolStripMenuItem checkForUpdateToolStripMenuItem;
    }
}