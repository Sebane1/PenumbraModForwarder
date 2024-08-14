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
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            Button associateFileTypes;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            fileSystemWatcher = new FileSystemWatcher();
            xma = new Button();
            glamourDresser = new Button();
            nexusMods = new Button();
            aetherlink = new Button();
            kittyEmporium = new Button();
            downloads = new FFXIVVoicePackCreator.FilePicker();
            cooldownTimer = new System.Windows.Forms.Timer(components);
            autoLoadModCheckbox = new CheckBox();
            label1 = new Label();
            trayIcon = new NotifyIcon(components);
            contextMenu = new ContextMenuStrip(components);
            quickLinksToolStripMenuItem = new ToolStripMenuItem();
            aetherlinkToolStripMenuItem = new ToolStripMenuItem();
            heliosphereToolStripMenuItem = new ToolStripMenuItem();
            nexusModsToolStripMenuItem = new ToolStripMenuItem();
            theGlamourDresserToolStripMenuItem = new ToolStripMenuItem();
            thePrettyKittyEmporiumToolStripMenuItem = new ToolStripMenuItem();
            xIVModArchiveToolStripMenuItem = new ToolStripMenuItem();
            moddingResourcesToolStripMenuItem = new ToolStripMenuItem();
            texturesToolStripMenuItem = new ToolStripMenuItem();
            looseTextureCompilerToolStripMenuItem = new ToolStripMenuItem();
            toBeExpandedToolStripMenuItem = new ToolStripMenuItem();
            soundsToolStripMenuItem = new ToolStripMenuItem();
            voicePackCreatorToolStripMenuItem = new ToolStripMenuItem();
            toBeExpandedToolStripMenuItem1 = new ToolStripMenuItem();
            modelsToolStripMenuItem = new ToolStripMenuItem();
            crossGenPortingToolToolStripMenuItem = new ToolStripMenuItem();
            toBeExpandedToolStripMenuItem3 = new ToolStripMenuItem();
            discordToolStripMenuItem = new ToolStripMenuItem();
            penumbraToolStripMenuItem = new ToolStripMenuItem();
            pixellatedsAssistancePlaceToolStripMenuItem = new ToolStripMenuItem();
            soundAndTextureResourceToolStripMenuItem = new ToolStripMenuItem();
            texToolsToolStripMenuItem = new ToolStripMenuItem();
            xIVModsResourcesToolStripMenuItem = new ToolStripMenuItem();
            toBeExpandedToolStripMenuItem2 = new ToolStripMenuItem();
            openConfigurationToolStripMenuItem = new ToolStripMenuItem();
            checkForUpdateToolStripMenuItem = new ToolStripMenuItem();
            donateToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            heliopshereButton = new Button();
            donateButton = new Button();
            discordButton = new Button();
            checkBox1 = new CheckBox();
            AutoDelete = new CheckBox();
            associateFileTypes = new Button();
            ((System.ComponentModel.ISupportInitialize)fileSystemWatcher).BeginInit();
            contextMenu.SuspendLayout();
            SuspendLayout();
            // 
            // associateFileTypes
            // 
            associateFileTypes.Location = new Point(0, 36);
            associateFileTypes.Name = "associateFileTypes";
            associateFileTypes.Size = new Size(120, 23);
            associateFileTypes.TabIndex = 9;
            associateFileTypes.Text = "Associate Mod Files";
            associateFileTypes.UseVisualStyleBackColor = true;
            associateFileTypes.Click += associateFileTypes_Click;
            // 
            // fileSystemWatcher
            // 
            fileSystemWatcher.EnableRaisingEvents = true;
            fileSystemWatcher.SynchronizingObject = this;
            fileSystemWatcher.Renamed += fileSystemWatcher_Renamed;
            // 
            // xma
            // 
            xma.Location = new Point(0, 90);
            xma.Name = "xma";
            xma.Size = new Size(104, 23);
            xma.TabIndex = 2;
            xma.Text = "XIV Mod Archive";
            xma.UseVisualStyleBackColor = true;
            xma.Click += xma_Click;
            // 
            // glamourDresser
            // 
            glamourDresser.Location = new Point(104, 90);
            glamourDresser.Name = "glamourDresser";
            glamourDresser.Size = new Size(124, 23);
            glamourDresser.TabIndex = 3;
            glamourDresser.Text = "The Glamour Dresser";
            glamourDresser.UseVisualStyleBackColor = true;
            glamourDresser.Click += glamourDresser_Click;
            // 
            // nexusMods
            // 
            nexusMods.Location = new Point(228, 90);
            nexusMods.Name = "nexusMods";
            nexusMods.Size = new Size(84, 23);
            nexusMods.TabIndex = 4;
            nexusMods.Text = "Nexus Mods";
            nexusMods.UseVisualStyleBackColor = true;
            nexusMods.Click += nexusMods_Click;
            // 
            // aetherlink
            // 
            aetherlink.Location = new Point(0, 114);
            aetherlink.Name = "aetherlink";
            aetherlink.Size = new Size(72, 23);
            aetherlink.TabIndex = 5;
            aetherlink.Text = "Aetherlink";
            aetherlink.UseVisualStyleBackColor = true;
            aetherlink.Click += aetherlink_Click;
            // 
            // kittyEmporium
            // 
            kittyEmporium.Location = new Point(156, 114);
            kittyEmporium.Name = "kittyEmporium";
            kittyEmporium.Size = new Size(156, 23);
            kittyEmporium.TabIndex = 6;
            kittyEmporium.Text = "The Pretty Kitty Emporium";
            kittyEmporium.UseVisualStyleBackColor = true;
            kittyEmporium.Click += kittyEmporium_Click;
            // 
            // downloads
            // 
            downloads.CurrentPath = null;
            downloads.Enabled = false;
            downloads.Filter = null;
            downloads.Index = -1;
            downloads.Location = new Point(4, 4);
            downloads.Margin = new Padding(4, 3, 4, 3);
            downloads.MinimumSize = new Size(300, 28);
            downloads.Name = "downloads";
            downloads.Size = new Size(312, 28);
            downloads.TabIndex = 8;
            downloads.OnFileSelected += downloads_OnFileSelected;
            // 
            // cooldownTimer
            // 
            cooldownTimer.Interval = 10;
            cooldownTimer.Tick += cooldownTimer_Tick;
            // 
            // autoLoadModCheckbox
            // 
            autoLoadModCheckbox.AutoSize = true;
            autoLoadModCheckbox.Location = new Point(180, 40);
            autoLoadModCheckbox.Name = "autoLoadModCheckbox";
            autoLoadModCheckbox.Size = new Size(131, 19);
            autoLoadModCheckbox.TabIndex = 10;
            autoLoadModCheckbox.Text = "Auto Forward Mods";
            autoLoadModCheckbox.UseVisualStyleBackColor = true;
            autoLoadModCheckbox.CheckedChanged += autoLoadModCheckbox_CheckedChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(0, 62);
            label1.Name = "label1";
            label1.Size = new Size(109, 25);
            label1.TabIndex = 12;
            label1.Text = "Quick links";
            // 
            // trayIcon
            // 
            trayIcon.BalloonTipIcon = ToolTipIcon.Info;
            trayIcon.ContextMenuStrip = contextMenu;
            trayIcon.Icon = (Icon)resources.GetObject("trayIcon.Icon");
            trayIcon.Text = "Penumbra Mod Forwarder";
            trayIcon.MouseDoubleClick += trayIcon_MouseDoubleClick;
            // 
            // contextMenu
            // 
            contextMenu.Items.AddRange(new ToolStripItem[] { quickLinksToolStripMenuItem, moddingResourcesToolStripMenuItem, openConfigurationToolStripMenuItem, checkForUpdateToolStripMenuItem, donateToolStripMenuItem, exitToolStripMenuItem });
            contextMenu.Name = "contextMenuStrip1";
            contextMenu.Size = new Size(181, 136);
            // 
            // quickLinksToolStripMenuItem
            // 
            quickLinksToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aetherlinkToolStripMenuItem, heliosphereToolStripMenuItem, nexusModsToolStripMenuItem, theGlamourDresserToolStripMenuItem, thePrettyKittyEmporiumToolStripMenuItem, xIVModArchiveToolStripMenuItem });
            quickLinksToolStripMenuItem.Name = "quickLinksToolStripMenuItem";
            quickLinksToolStripMenuItem.Size = new Size(180, 22);
            quickLinksToolStripMenuItem.Text = "Quick Links";
            // 
            // aetherlinkToolStripMenuItem
            // 
            aetherlinkToolStripMenuItem.Name = "aetherlinkToolStripMenuItem";
            aetherlinkToolStripMenuItem.Size = new Size(213, 22);
            aetherlinkToolStripMenuItem.Text = "Aetherlink";
            aetherlinkToolStripMenuItem.Click += aetherlink_Click;
            // 
            // heliosphereToolStripMenuItem
            // 
            heliosphereToolStripMenuItem.Name = "heliosphereToolStripMenuItem";
            heliosphereToolStripMenuItem.Size = new Size(213, 22);
            heliosphereToolStripMenuItem.Text = "Heliosphere";
            heliosphereToolStripMenuItem.Click += heliosphereToolStripMenuItem_Click;
            // 
            // nexusModsToolStripMenuItem
            // 
            nexusModsToolStripMenuItem.Name = "nexusModsToolStripMenuItem";
            nexusModsToolStripMenuItem.Size = new Size(213, 22);
            nexusModsToolStripMenuItem.Text = "Nexus Mods";
            nexusModsToolStripMenuItem.Click += nexusMods_Click;
            // 
            // theGlamourDresserToolStripMenuItem
            // 
            theGlamourDresserToolStripMenuItem.Name = "theGlamourDresserToolStripMenuItem";
            theGlamourDresserToolStripMenuItem.Size = new Size(213, 22);
            theGlamourDresserToolStripMenuItem.Text = "The Glamour Dresser";
            theGlamourDresserToolStripMenuItem.Click += glamourDresser_Click;
            // 
            // thePrettyKittyEmporiumToolStripMenuItem
            // 
            thePrettyKittyEmporiumToolStripMenuItem.Name = "thePrettyKittyEmporiumToolStripMenuItem";
            thePrettyKittyEmporiumToolStripMenuItem.Size = new Size(213, 22);
            thePrettyKittyEmporiumToolStripMenuItem.Text = "The Pretty Kitty Emporium";
            thePrettyKittyEmporiumToolStripMenuItem.Click += kittyEmporium_Click;
            // 
            // xIVModArchiveToolStripMenuItem
            // 
            xIVModArchiveToolStripMenuItem.Name = "xIVModArchiveToolStripMenuItem";
            xIVModArchiveToolStripMenuItem.Size = new Size(213, 22);
            xIVModArchiveToolStripMenuItem.Text = "XIV Mod Archive";
            xIVModArchiveToolStripMenuItem.Click += xma_Click;
            // 
            // moddingResourcesToolStripMenuItem
            // 
            moddingResourcesToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { texturesToolStripMenuItem, soundsToolStripMenuItem, modelsToolStripMenuItem, discordToolStripMenuItem });
            moddingResourcesToolStripMenuItem.Name = "moddingResourcesToolStripMenuItem";
            moddingResourcesToolStripMenuItem.Size = new Size(180, 22);
            moddingResourcesToolStripMenuItem.Text = "Modding Resources";
            // 
            // texturesToolStripMenuItem
            // 
            texturesToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { looseTextureCompilerToolStripMenuItem, toBeExpandedToolStripMenuItem });
            texturesToolStripMenuItem.Name = "texturesToolStripMenuItem";
            texturesToolStripMenuItem.Size = new Size(119, 22);
            texturesToolStripMenuItem.Text = "Textures";
            // 
            // looseTextureCompilerToolStripMenuItem
            // 
            looseTextureCompilerToolStripMenuItem.Name = "looseTextureCompilerToolStripMenuItem";
            looseTextureCompilerToolStripMenuItem.Size = new Size(198, 22);
            looseTextureCompilerToolStripMenuItem.Text = "Loose Texture Compiler";
            looseTextureCompilerToolStripMenuItem.Click += looseTextureCompilerToolStripMenuItem_Click;
            // 
            // toBeExpandedToolStripMenuItem
            // 
            toBeExpandedToolStripMenuItem.Enabled = false;
            toBeExpandedToolStripMenuItem.Name = "toBeExpandedToolStripMenuItem";
            toBeExpandedToolStripMenuItem.Size = new Size(198, 22);
            toBeExpandedToolStripMenuItem.Text = "--To Be Expanded--";
            // 
            // soundsToolStripMenuItem
            // 
            soundsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { voicePackCreatorToolStripMenuItem, toBeExpandedToolStripMenuItem1 });
            soundsToolStripMenuItem.Name = "soundsToolStripMenuItem";
            soundsToolStripMenuItem.Size = new Size(119, 22);
            soundsToolStripMenuItem.Text = "Sounds";
            soundsToolStripMenuItem.Click += soundsToolStripMenuItem_Click;
            // 
            // voicePackCreatorToolStripMenuItem
            // 
            voicePackCreatorToolStripMenuItem.Name = "voicePackCreatorToolStripMenuItem";
            voicePackCreatorToolStripMenuItem.Size = new Size(177, 22);
            voicePackCreatorToolStripMenuItem.Text = "Voice Pack Creator";
            voicePackCreatorToolStripMenuItem.Click += voicePackCreatorToolStripMenuItem_Click;
            // 
            // toBeExpandedToolStripMenuItem1
            // 
            toBeExpandedToolStripMenuItem1.Enabled = false;
            toBeExpandedToolStripMenuItem1.Name = "toBeExpandedToolStripMenuItem1";
            toBeExpandedToolStripMenuItem1.Size = new Size(177, 22);
            toBeExpandedToolStripMenuItem1.Text = "--To Be Expanded--";
            // 
            // modelsToolStripMenuItem
            // 
            modelsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { crossGenPortingToolToolStripMenuItem, toBeExpandedToolStripMenuItem3 });
            modelsToolStripMenuItem.Name = "modelsToolStripMenuItem";
            modelsToolStripMenuItem.Size = new Size(119, 22);
            modelsToolStripMenuItem.Text = "Models";
            // 
            // crossGenPortingToolToolStripMenuItem
            // 
            crossGenPortingToolToolStripMenuItem.Name = "crossGenPortingToolToolStripMenuItem";
            crossGenPortingToolToolStripMenuItem.Size = new Size(194, 22);
            crossGenPortingToolToolStripMenuItem.Text = "Cross Gen Porting Tool";
            crossGenPortingToolToolStripMenuItem.Click += crossGenPortingToolToolStripMenuItem_Click;
            // 
            // toBeExpandedToolStripMenuItem3
            // 
            toBeExpandedToolStripMenuItem3.Enabled = false;
            toBeExpandedToolStripMenuItem3.Name = "toBeExpandedToolStripMenuItem3";
            toBeExpandedToolStripMenuItem3.Size = new Size(194, 22);
            toBeExpandedToolStripMenuItem3.Text = "--To Be Expanded--";
            // 
            // discordToolStripMenuItem
            // 
            discordToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { penumbraToolStripMenuItem, pixellatedsAssistancePlaceToolStripMenuItem, soundAndTextureResourceToolStripMenuItem, texToolsToolStripMenuItem, xIVModsResourcesToolStripMenuItem, toBeExpandedToolStripMenuItem2 });
            discordToolStripMenuItem.Name = "discordToolStripMenuItem";
            discordToolStripMenuItem.Size = new Size(119, 22);
            discordToolStripMenuItem.Text = "Discords";
            // 
            // penumbraToolStripMenuItem
            // 
            penumbraToolStripMenuItem.Name = "penumbraToolStripMenuItem";
            penumbraToolStripMenuItem.Size = new Size(222, 22);
            penumbraToolStripMenuItem.Text = "Penumbra";
            penumbraToolStripMenuItem.Click += penumbraToolStripMenuItem_Click;
            // 
            // pixellatedsAssistancePlaceToolStripMenuItem
            // 
            pixellatedsAssistancePlaceToolStripMenuItem.Name = "pixellatedsAssistancePlaceToolStripMenuItem";
            pixellatedsAssistancePlaceToolStripMenuItem.Size = new Size(222, 22);
            pixellatedsAssistancePlaceToolStripMenuItem.Text = "Pixellated's Assistance Place";
            pixellatedsAssistancePlaceToolStripMenuItem.Click += pixellatedsAssistancePlaceToolStripMenuItem_Click;
            // 
            // soundAndTextureResourceToolStripMenuItem
            // 
            soundAndTextureResourceToolStripMenuItem.Name = "soundAndTextureResourceToolStripMenuItem";
            soundAndTextureResourceToolStripMenuItem.Size = new Size(222, 22);
            soundAndTextureResourceToolStripMenuItem.Text = "Sound && Texture Resource";
            soundAndTextureResourceToolStripMenuItem.Click += soundAndTextureResourceToolStripMenuItem_Click;
            // 
            // texToolsToolStripMenuItem
            // 
            texToolsToolStripMenuItem.Name = "texToolsToolStripMenuItem";
            texToolsToolStripMenuItem.Size = new Size(222, 22);
            texToolsToolStripMenuItem.Text = "TexTools";
            texToolsToolStripMenuItem.Click += texToolsToolStripMenuItem_Click;
            // 
            // xIVModsResourcesToolStripMenuItem
            // 
            xIVModsResourcesToolStripMenuItem.Name = "xIVModsResourcesToolStripMenuItem";
            xIVModsResourcesToolStripMenuItem.Size = new Size(222, 22);
            xIVModsResourcesToolStripMenuItem.Text = "XIV Mods && Resources";
            xIVModsResourcesToolStripMenuItem.Click += xIVModsResourcesToolStripMenuItem_Click;
            // 
            // toBeExpandedToolStripMenuItem2
            // 
            toBeExpandedToolStripMenuItem2.Enabled = false;
            toBeExpandedToolStripMenuItem2.Name = "toBeExpandedToolStripMenuItem2";
            toBeExpandedToolStripMenuItem2.Size = new Size(222, 22);
            toBeExpandedToolStripMenuItem2.Text = "--To Be Expanded--";
            // 
            // openConfigurationToolStripMenuItem
            // 
            openConfigurationToolStripMenuItem.Name = "openConfigurationToolStripMenuItem";
            openConfigurationToolStripMenuItem.Size = new Size(180, 22);
            openConfigurationToolStripMenuItem.Text = "Open Configuration";
            openConfigurationToolStripMenuItem.Click += openConfigurationToolStripMenuItem_Click;
            // 
            // checkForUpdateToolStripMenuItem
            // 
            checkForUpdateToolStripMenuItem.Name = "checkForUpdateToolStripMenuItem";
            checkForUpdateToolStripMenuItem.Size = new Size(180, 22);
            checkForUpdateToolStripMenuItem.Text = "Check For Update";
            checkForUpdateToolStripMenuItem.Click += checkForUpdateToolStripMenuItem_Click;
            // 
            // donateToolStripMenuItem
            // 
            donateToolStripMenuItem.BackColor = Color.MistyRose;
            donateToolStripMenuItem.Name = "donateToolStripMenuItem";
            donateToolStripMenuItem.Size = new Size(180, 22);
            donateToolStripMenuItem.Text = "Donate";
            donateToolStripMenuItem.Click += donateButton_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(180, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // heliopshereButton
            // 
            heliopshereButton.Location = new Point(72, 114);
            heliopshereButton.Name = "heliopshereButton";
            heliopshereButton.Size = new Size(84, 23);
            heliopshereButton.TabIndex = 13;
            heliopshereButton.Text = "Heliosphere";
            heliopshereButton.UseVisualStyleBackColor = true;
            heliopshereButton.Click += heliosphereToolStripMenuItem_Click;
            // 
            // donateButton
            // 
            donateButton.BackColor = Color.LightCoral;
            donateButton.ForeColor = Color.White;
            donateButton.Location = new Point(156, 138);
            donateButton.Name = "donateButton";
            donateButton.Size = new Size(156, 28);
            donateButton.TabIndex = 16;
            donateButton.Text = "Donate";
            donateButton.UseVisualStyleBackColor = false;
            donateButton.Click += donateButton_Click;
            // 
            // discordButton
            // 
            discordButton.BackColor = Color.SlateBlue;
            discordButton.ForeColor = Color.White;
            discordButton.Location = new Point(0, 138);
            discordButton.Name = "discordButton";
            discordButton.Size = new Size(156, 28);
            discordButton.TabIndex = 17;
            discordButton.Text = "Discord";
            discordButton.UseVisualStyleBackColor = false;
            discordButton.Click += discordButton_Click;
            // 
            // checkBox1
            // 
            checkBox1.Location = new Point(0, 0);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(104, 24);
            checkBox1.TabIndex = 0;
            // 
            // AutoDelete
            // 
            AutoDelete.AutoSize = true;
            AutoDelete.Location = new Point(180, 62);
            AutoDelete.Name = "AutoDelete";
            AutoDelete.Size = new Size(114, 19);
            AutoDelete.TabIndex = 18;
            AutoDelete.Text = "Auto Delete Files";
            AutoDelete.UseVisualStyleBackColor = true;
            AutoDelete.CheckedChanged += AutoDelete_CheckedChanged;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(313, 168);
            Controls.Add(AutoDelete);
            Controls.Add(discordButton);
            Controls.Add(donateButton);
            Controls.Add(heliopshereButton);
            Controls.Add(label1);
            Controls.Add(autoLoadModCheckbox);
            Controls.Add(associateFileTypes);
            Controls.Add(downloads);
            Controls.Add(kittyEmporium);
            Controls.Add(aetherlink);
            Controls.Add(nexusMods);
            Controls.Add(glamourDresser);
            Controls.Add(xma);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "MainWindow";
            Text = "Penumbra Mod Forwarder";
            Activated += MainWindow_Activated;
            FormClosing += MainWindow_FormClosing;
            Load += MainWindow_Load;
            ((System.ComponentModel.ISupportInitialize)fileSystemWatcher).EndInit();
            contextMenu.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
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
        private Button discordButton;
        private Button donateButton;
        private ToolStripMenuItem donateToolStripMenuItem;
        private CheckBox checkBox1;
        private CheckBox AutoDelete;
    }
}