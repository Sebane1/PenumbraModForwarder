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
	private void InitializeComponent() {
		ComponentResourceManager resources = new ComponentResourceManager(typeof(FileSelect));
		fileCheckedListBox = new CheckedListBox();
		confirmButton = new Button();
		archivefile_Label = new Label();
		cancel_Button = new Button();
		selectall_Button = new Button();
		predt_Label = new Label();
		SuspendLayout();
		// 
		// fileCheckedListBox
		// 
		fileCheckedListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		fileCheckedListBox.FormattingEnabled = true;
		fileCheckedListBox.Location = new Point(12, 28);
		fileCheckedListBox.MinimumSize = new Size(150, 50);
		fileCheckedListBox.Name = "fileCheckedListBox";
		fileCheckedListBox.Size = new Size(270, 76);
		fileCheckedListBox.TabIndex = 1;
		// 
		// confirmButton
		// 
		confirmButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		confirmButton.Location = new Point(163, 140);
		confirmButton.Name = "confirmButton";
		confirmButton.Size = new Size(119, 37);
		confirmButton.TabIndex = 4;
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
		archivefile_Label.TabIndex = 0;
		archivefile_Label.Text = "text";
		archivefile_Label.TextAlign = ContentAlignment.MiddleCenter;
		// 
		// cancel_Button
		// 
		cancel_Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
		cancel_Button.Location = new Point(12, 140);
		cancel_Button.Name = "cancel_Button";
		cancel_Button.Size = new Size(119, 37);
		cancel_Button.TabIndex = 5;
		cancel_Button.Text = "Cancel";
		cancel_Button.UseVisualStyleBackColor = true;
		// 
		// selectall_Button
		// 
		selectall_Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
		selectall_Button.Enabled = false;
		selectall_Button.Location = new Point(12, 111);
		selectall_Button.Name = "selectall_Button";
		selectall_Button.Size = new Size(75, 23);
		selectall_Button.TabIndex = 2;
		selectall_Button.Text = "Select All";
		selectall_Button.UseVisualStyleBackColor = true;
		selectall_Button.Visible = false;
		// 
		// predt_Label
		// 
		predt_Label.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		predt_Label.AutoSize = true;
		predt_Label.Location = new Point(128, 115);
		predt_Label.Name = "predt_Label";
		predt_Label.Size = new Size(154, 15);
		predt_Label.TabIndex = 3;
		predt_Label.Text = "*  Pre DT version of the mod";
		predt_Label.Visible = false;
		// 
		// FileSelect
		// 
		AutoScaleDimensions = new SizeF(96F, 96F);
		AutoScaleMode = AutoScaleMode.Dpi;
		ClientSize = new Size(300, 181);
		Controls.Add(selectall_Button);
		Controls.Add(predt_Label);
		Controls.Add(cancel_Button);
		Controls.Add(archivefile_Label);
		Controls.Add(confirmButton);
		Controls.Add(fileCheckedListBox);
		Icon = (Icon)resources.GetObject("$this.Icon");
		MaximizeBox = false;
		MinimizeBox = false;
		MinimumSize = new Size(300, 200);
		Name = "FileSelect";
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
    private Button selectall_Button;
    private Label predt_Label;
}