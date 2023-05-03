namespace PenumbraModForwarder {
    partial class AppSelectionsWindow {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppSelectionsWindow));
            this.label1 = new System.Windows.Forms.Label();
            this.penumbraButton = new System.Windows.Forms.Button();
            this.textoolsButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(171, 32);
            this.label1.TabIndex = 0;
            this.label1.Text = "Open With.....";
            // 
            // penumbraButton
            // 
            this.penumbraButton.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.penumbraButton.BackgroundImage = global::PenumbraModForwarder.Properties.Resources.icon;
            this.penumbraButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.penumbraButton.Location = new System.Drawing.Point(16, 36);
            this.penumbraButton.Name = "penumbraButton";
            this.penumbraButton.Size = new System.Drawing.Size(70, 70);
            this.penumbraButton.TabIndex = 1;
            this.penumbraButton.UseVisualStyleBackColor = false;
            this.penumbraButton.Click += new System.EventHandler(this.penumbraButton_Click);
            // 
            // textoolsButton
            // 
            this.textoolsButton.BackgroundImage = global::PenumbraModForwarder.Properties.Resources.ffxiv2_1;
            this.textoolsButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.textoolsButton.Location = new System.Drawing.Point(96, 36);
            this.textoolsButton.Name = "textoolsButton";
            this.textoolsButton.Size = new System.Drawing.Size(70, 70);
            this.textoolsButton.TabIndex = 2;
            this.textoolsButton.UseVisualStyleBackColor = true;
            this.textoolsButton.Click += new System.EventHandler(this.textoolsButton_Click);
            // 
            // AppSelectionsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(183, 108);
            this.Controls.Add(this.textoolsButton);
            this.Controls.Add(this.penumbraButton);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AppSelectionsWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "What Should Open This?";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label label1;
        private Button penumbraButton;
        private Button textoolsButton;
    }
}