namespace PenumbraModForwarder {
    partial class ModPackSelectionWindow {
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
            modPackOptions = new CheckedListBox();
            label1 = new Label();
            confirmButtom = new Button();
            SuspendLayout();
            // 
            // modPackOptions
            // 
            modPackOptions.FormattingEnabled = true;
            modPackOptions.Location = new Point(12, 36);
            modPackOptions.Name = "modPackOptions";
            modPackOptions.Size = new Size(286, 112);
            modPackOptions.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            label1.ForeColor = Color.Black;
            label1.Location = new Point(0, 8);
            label1.Name = "label1";
            label1.Size = new Size(309, 25);
            label1.TabIndex = 1;
            label1.Text = "This Release Has Multiple Options";
            // 
            // confirmButtom
            // 
            confirmButtom.Location = new Point(12, 154);
            confirmButtom.Name = "confirmButtom";
            confirmButtom.Size = new Size(286, 23);
            confirmButtom.TabIndex = 2;
            confirmButtom.Text = "Confirm";
            confirmButtom.UseVisualStyleBackColor = true;
            confirmButtom.Click += confirmButtom_Click;
            // 
            // ModPackSelectionWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(310, 187);
            Controls.Add(confirmButtom);
            Controls.Add(label1);
            Controls.Add(modPackOptions);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ModPackSelectionWindow";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ModPackSelectionWindow";
            TopMost = true;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckedListBox modPackOptions;
        private Label label1;
        private Button confirmButtom;
    }
}