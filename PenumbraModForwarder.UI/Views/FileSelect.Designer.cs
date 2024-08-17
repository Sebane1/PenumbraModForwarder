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
        SuspendLayout();
        // 
        // fileCheckedListBox
        // 
        fileCheckedListBox.FormattingEnabled = true;
        fileCheckedListBox.Location = new Point(12, 12);
        fileCheckedListBox.Name = "fileCheckedListBox";
        fileCheckedListBox.Size = new Size(776, 328);
        fileCheckedListBox.TabIndex = 0;
        // 
        // confirmButton
        // 
        confirmButton.Location = new Point(669, 377);
        confirmButton.Name = "confirmButton";
        confirmButton.Size = new Size(119, 37);
        confirmButton.TabIndex = 1;
        confirmButton.Text = "Confirm Selection";
        confirmButton.UseVisualStyleBackColor = true;
        // 
        // FileSelect
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(800, 450);
        Controls.Add(confirmButton);
        Controls.Add(fileCheckedListBox);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "FileSelect";
        Text = "Select Files";
        ResumeLayout(false);
    }

    #endregion

    private Button confirmButton;
}