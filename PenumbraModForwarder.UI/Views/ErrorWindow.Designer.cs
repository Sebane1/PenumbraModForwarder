using System.ComponentModel;

namespace PenumbraModForwarder.UI.Views;

partial class ErrorWindow
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

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        error_TextBox = new RichTextBox();
        openlog_Button = new Button();
        openDiscord_Button = new Button();
        SuspendLayout();
        // 
        // error_TextBox
        // 
        error_TextBox.CausesValidation = false;
        error_TextBox.Location = new Point(12, 12);
        error_TextBox.Name = "error_TextBox";
        error_TextBox.ReadOnly = true;
        error_TextBox.Size = new Size(776, 365);
        error_TextBox.TabIndex = 0;
        error_TextBox.Text = "";
        // 
        // openlog_Button
        // 
        openlog_Button.Location = new Point(263, 394);
        openlog_Button.Name = "openlog_Button";
        openlog_Button.Size = new Size(112, 44);
        openlog_Button.TabIndex = 1;
        openlog_Button.Text = "Open Log";
        openlog_Button.UseVisualStyleBackColor = true;
        // 
        // openDiscord_Button
        // 
        openDiscord_Button.Location = new Point(435, 394);
        openDiscord_Button.Name = "openDiscord_Button";
        openDiscord_Button.Size = new Size(112, 44);
        openDiscord_Button.TabIndex = 2;
        openDiscord_Button.Text = "Open Discord";
        openDiscord_Button.UseVisualStyleBackColor = true;
        // 
        // ErrorWindow
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Controls.Add(openDiscord_Button);
        Controls.Add(openlog_Button);
        Controls.Add(error_TextBox);
        Name = "ErrorWindow";
        Text = "Error Window";
        ResumeLayout(false);
    }

    #endregion

    private RichTextBox error_TextBox;
    private Button openlog_Button;
    private Button openDiscord_Button;
}