namespace PenumbraModForwarder {
    partial class WaitingMessageWindow {
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
            label1 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 26.25F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(26, 9);
            label1.Name = "label1";
            label1.Size = new Size(352, 47);
            label1.TabIndex = 0;
            label1.Text = "Preparing Mod Files";
            label1.TextAlign = ContentAlignment.BottomCenter;
            // 
            // WaitingMessageWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(399, 72);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "WaitingMessageWindow";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "WaitingMessageWindow";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
    }
}