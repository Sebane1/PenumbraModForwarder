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
    private System.Windows.Forms.ListBox fileListBox;

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.fileListBox = new System.Windows.Forms.ListBox();
        this.SuspendLayout();
        // 
        // fileListBox
        // 
        this.fileListBox.FormattingEnabled = true;
        this.fileListBox.Location = new System.Drawing.Point(12, 12);
        this.fileListBox.Name = "fileListBox";
        this.fileListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
        this.fileListBox.Size = new System.Drawing.Size(760, 274);
        this.fileListBox.TabIndex = 0;
        // 
        // FileSelect
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.ClientSize = new System.Drawing.Size(796, 328);
        this.Controls.Add(this.fileListBox);
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "FileSelect";
        this.Text = "Select a File";
        this.ResumeLayout(false);
    }

    #endregion
}