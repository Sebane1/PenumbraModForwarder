namespace PenumbraModForwarder.UI.Views
{
    partial class ArchivePasswordWindow
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
            file_Label = new Label();
            protected_Label = new Label();
            password_TextBox = new TextBox();
            confim_Button = new Button();
            cancel_Button = new Button();
            SuspendLayout();
            // 
            // file_Label
            // 
            file_Label.AutoSize = true;
            file_Label.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            file_Label.Location = new Point(28, 29);
            file_Label.Name = "file_Label";
            file_Label.Size = new Size(0, 21);
            file_Label.TabIndex = 2;
            // 
            // protected_Label
            // 
            protected_Label.AutoSize = true;
            protected_Label.Location = new Point(33, 65);
            protected_Label.Name = "protected_Label";
            protected_Label.Size = new Size(264, 15);
            protected_Label.TabIndex = 3;
            protected_Label.Text = "Is Password Protected, please enter the password";
            // 
            // password_TextBox
            // 
            password_TextBox.Location = new Point(33, 142);
            password_TextBox.Name = "password_TextBox";
            password_TextBox.Size = new Size(264, 23);
            password_TextBox.TabIndex = 4;
            // 
            // confim_Button
            // 
            confim_Button.Location = new Point(184, 194);
            confim_Button.Name = "confim_Button";
            confim_Button.Size = new Size(75, 23);
            confim_Button.TabIndex = 5;
            confim_Button.Text = "Confirm";
            confim_Button.UseVisualStyleBackColor = true;
            // 
            // cancel_Button
            // 
            cancel_Button.Location = new Point(80, 194);
            cancel_Button.Name = "cancel_Button";
            cancel_Button.Size = new Size(75, 23);
            cancel_Button.TabIndex = 6;
            cancel_Button.Text = "Cancel";
            cancel_Button.UseVisualStyleBackColor = true;
            // 
            // ArchivePasswordWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(357, 241);
            Controls.Add(cancel_Button);
            Controls.Add(confim_Button);
            Controls.Add(password_TextBox);
            Controls.Add(protected_Label);
            Controls.Add(file_Label);
            Name = "ArchivePasswordWindow";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Input Archive Password";
            TopMost = true;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label file_Label;
        private Label protected_Label;
        private TextBox password_TextBox;
        private Button confim_Button;
        private Button cancel_Button;
    }
}