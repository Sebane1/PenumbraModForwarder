using System.ComponentModel;

namespace PenumbraModForwarder.UI.Views;

partial class MainWindow
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
        select_directory = new Button();
        directory_text = new TextBox();
        autoforward_checkbox = new CheckBox();
        autodelete_checkbox = new CheckBox();
        extractall_checkbox = new CheckBox();
        SuspendLayout();
        // 
        // select_directory
        // 
        select_directory.Location = new Point(226, 12);
        select_directory.Name = "select_directory";
        select_directory.Size = new Size(75, 23);
        select_directory.TabIndex = 0;
        select_directory.Text = "Select";
        select_directory.UseVisualStyleBackColor = true;
        // 
        // directory_text
        // 
        directory_text.Enabled = false;
        directory_text.Location = new Point(12, 12);
        directory_text.Name = "directory_text";
        directory_text.ReadOnly = true;
        directory_text.Size = new Size(208, 23);
        directory_text.TabIndex = 1;
        // 
        // autoforward_checkbox
        // 
        autoforward_checkbox.AutoSize = true;
        autoforward_checkbox.Location = new Point(170, 52);
        autoforward_checkbox.Name = "autoforward_checkbox";
        autoforward_checkbox.Size = new Size(131, 19);
        autoforward_checkbox.TabIndex = 2;
        autoforward_checkbox.Text = "Auto Forward Mods";
        autoforward_checkbox.UseVisualStyleBackColor = true;
        // 
        // autodelete_checkbox
        // 
        autodelete_checkbox.AutoSize = true;
        autodelete_checkbox.Location = new Point(170, 102);
        autodelete_checkbox.Name = "autodelete_checkbox";
        autodelete_checkbox.Size = new Size(121, 19);
        autodelete_checkbox.TabIndex = 3;
        autodelete_checkbox.Text = "Auto Delete Mods";
        autodelete_checkbox.UseVisualStyleBackColor = true;
        // 
        // extractall_checkbox
        // 
        extractall_checkbox.AutoSize = true;
        extractall_checkbox.Location = new Point(170, 77);
        extractall_checkbox.Name = "extractall_checkbox";
        extractall_checkbox.Size = new Size(112, 19);
        extractall_checkbox.TabIndex = 4;
        extractall_checkbox.Text = "Extract All Mods";
        extractall_checkbox.UseVisualStyleBackColor = true;
        // 
        // MainWindow
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(313, 191);
        Controls.Add(extractall_checkbox);
        Controls.Add(autodelete_checkbox);
        Controls.Add(autoforward_checkbox);
        Controls.Add(directory_text);
        Controls.Add(select_directory);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Name = "MainWindow";
        Text = "Penumbra Mod Forwarder";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion
    private Button select_directory;
    private TextBox directory_text;
    private CheckBox autoforward_checkbox;
    private CheckBox autodelete_checkbox;
    private CheckBox extractall_checkbox;
}