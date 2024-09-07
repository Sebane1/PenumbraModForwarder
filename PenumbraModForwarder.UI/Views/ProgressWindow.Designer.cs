namespace PenumbraModForwarder.UI.Views
{
    partial class ProgressWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            progress_Bar = new ProgressBar();
            file_Label = new Label();
            operation_Label = new Label();
            SuspendLayout();
            // 
            // progress_Bar
            // 
            progress_Bar.Location = new Point(12, 107);
            progress_Bar.Name = "progress_Bar";
            progress_Bar.Size = new Size(345, 23);
            progress_Bar.TabIndex = 0;
            // 
            // file_Label
            // 
            file_Label.AutoSize = true;
            file_Label.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            file_Label.Location = new Point(12, 20);
            file_Label.Name = "file_Label";
            file_Label.Size = new Size(57, 21);
            file_Label.TabIndex = 1;
            file_Label.Text = "label1";
            // 
            // operation_Label
            // 
            operation_Label.AutoSize = true;
            operation_Label.Location = new Point(12, 64);
            operation_Label.Name = "operation_Label";
            operation_Label.Size = new Size(38, 15);
            operation_Label.TabIndex = 2;
            operation_Label.Text = "label1";
            // 
            // ProgressWindow
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(369, 174);
            ControlBox = false;
            Controls.Add(operation_Label);
            Controls.Add(file_Label);
            Controls.Add(progress_Bar);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ProgressWindow";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            TopMost = true;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ProgressBar progress_Bar;
        private Label file_Label;
        private Label operation_Label;
    }
}