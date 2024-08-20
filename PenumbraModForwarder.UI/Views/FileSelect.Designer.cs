using System.ComponentModel;

namespace PenumbraModForwarder.UI.Views;

partial class FileSelect
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private IContainer components = null;

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
    private System.Windows.Forms.CheckedListBox fileCheckedListBox;

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        fileCheckedListBox = new CheckedListBox();
        confirmButton = new Button();
        archivefile_Label = new Label();
        cancel_Button = new Button();
        SuspendLayout();
        // 
        // fileCheckedListBox
        // 
        fileCheckedListBox.FormattingEnabled = true;
        fileCheckedListBox.Location = new Point(12, 28);
        fileCheckedListBox.Name = "fileCheckedListBox";
        fileCheckedListBox.Size = new Size(270, 76);
        fileCheckedListBox.TabIndex = 0;
        // 
        // confirmButton
        // 
        confirmButton.Location = new Point(163, 110);
        confirmButton.Name = "confirmButton";
        confirmButton.Size = new Size(119, 37);
        confirmButton.TabIndex = 1;
        confirmButton.Text = "Confirm Selection";
        confirmButton.UseVisualStyleBackColor = true;
        // 
        // archivefile_Label
        // 
        archivefile_Label.AutoSize = true;
        archivefile_Label.Dock = DockStyle.Left;
        archivefile_Label.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold);
        archivefile_Label.Location = new Point(0, 0);
        archivefile_Label.Name = "archivefile_Label";
        archivefile_Label.Size = new Size(47, 25);
        archivefile_Label.TabIndex = 2;
        archivefile_Label.Text = "text";
        archivefile_Label.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // cancel_Button
        // 
        cancel_Button.Location = new Point(12, 110);
        cancel_Button.Name = "cancel_Button";
        cancel_Button.Size = new Size(119, 37);
        cancel_Button.TabIndex = 3;
        cancel_Button.Text = "Cancel";
        cancel_Button.UseVisualStyleBackColor = true;
        // 
        // FileSelect
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(300, 163);
        Controls.Add(cancel_Button);
        Controls.Add(archivefile_Label);
        Controls.Add(confirmButton);
        Controls.Add(fileCheckedListBox);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "FileSelect";
        ShowIcon = false;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Select Files";
        TopMost = true;
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Button confirmButton;
    private Label archivefile_Label;
    private Button cancel_Button;
}