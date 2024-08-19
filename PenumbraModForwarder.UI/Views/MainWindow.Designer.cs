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
        xivarchive_Button = new Button();
        glamourdressor_Button = new Button();
        nexusmods_Button = new Button();
        aetherlink_Button = new Button();
        heliosphere_Button = new Button();
        prettykitty_Button = new Button();
        discord_Button = new Button();
        donate_Button = new Button();
        label1 = new Label();
        SuspendLayout();
        // 
        // select_directory
        // 
        select_directory.Location = new Point(226, 7);
        select_directory.Name = "select_directory";
        select_directory.Size = new Size(75, 23);
        select_directory.TabIndex = 0;
        select_directory.Text = "Select";
        select_directory.UseVisualStyleBackColor = true;
        // 
        // directory_text
        // 
        directory_text.Enabled = false;
        directory_text.Location = new Point(12, 8);
        directory_text.Name = "directory_text";
        directory_text.ReadOnly = true;
        directory_text.Size = new Size(208, 23);
        directory_text.TabIndex = 1;
        // 
        // autoforward_checkbox
        // 
        autoforward_checkbox.AutoSize = true;
        autoforward_checkbox.Location = new Point(170, 37);
        autoforward_checkbox.Name = "autoforward_checkbox";
        autoforward_checkbox.Size = new Size(131, 19);
        autoforward_checkbox.TabIndex = 2;
        autoforward_checkbox.Text = "Auto Forward Mods";
        autoforward_checkbox.UseVisualStyleBackColor = true;
        // 
        // autodelete_checkbox
        // 
        autodelete_checkbox.AutoSize = true;
        autodelete_checkbox.Location = new Point(170, 87);
        autodelete_checkbox.Name = "autodelete_checkbox";
        autodelete_checkbox.Size = new Size(121, 19);
        autodelete_checkbox.TabIndex = 3;
        autodelete_checkbox.Text = "Auto Delete Mods";
        autodelete_checkbox.UseVisualStyleBackColor = true;
        // 
        // extractall_checkbox
        // 
        extractall_checkbox.AutoSize = true;
        extractall_checkbox.Location = new Point(170, 62);
        extractall_checkbox.Name = "extractall_checkbox";
        extractall_checkbox.Size = new Size(112, 19);
        extractall_checkbox.TabIndex = 4;
        extractall_checkbox.Text = "Extract All Mods";
        extractall_checkbox.UseVisualStyleBackColor = true;
        // 
        // xivarchive_Button
        // 
        xivarchive_Button.Location = new Point(0, 113);
        xivarchive_Button.Name = "xivarchive_Button";
        xivarchive_Button.Size = new Size(104, 23);
        xivarchive_Button.TabIndex = 5;
        xivarchive_Button.Text = "XIV Mod Archive";
        xivarchive_Button.UseVisualStyleBackColor = true;
        // 
        // glamourdressor_Button
        // 
        glamourdressor_Button.Location = new Point(104, 113);
        glamourdressor_Button.Name = "glamourdressor_Button";
        glamourdressor_Button.Size = new Size(124, 23);
        glamourdressor_Button.TabIndex = 6;
        glamourdressor_Button.Text = "The Glamour Dresser";
        glamourdressor_Button.UseVisualStyleBackColor = true;
        // 
        // nexusmods_Button
        // 
        nexusmods_Button.Location = new Point(228, 113);
        nexusmods_Button.Name = "nexusmods_Button";
        nexusmods_Button.Size = new Size(84, 23);
        nexusmods_Button.TabIndex = 7;
        nexusmods_Button.Text = "Nexus Mods";
        nexusmods_Button.UseVisualStyleBackColor = true;
        // 
        // aetherlink_Button
        // 
        aetherlink_Button.Location = new Point(0, 137);
        aetherlink_Button.Name = "aetherlink_Button";
        aetherlink_Button.Size = new Size(72, 23);
        aetherlink_Button.TabIndex = 8;
        aetherlink_Button.Text = "Aetherlink";
        aetherlink_Button.UseVisualStyleBackColor = true;
        // 
        // heliosphere_Button
        // 
        heliosphere_Button.Location = new Point(72, 137);
        heliosphere_Button.Name = "heliosphere_Button";
        heliosphere_Button.Size = new Size(84, 23);
        heliosphere_Button.TabIndex = 9;
        heliosphere_Button.Text = "Heliosphere";
        heliosphere_Button.UseVisualStyleBackColor = true;
        // 
        // prettykitty_Button
        // 
        prettykitty_Button.Location = new Point(156, 137);
        prettykitty_Button.Name = "prettykitty_Button";
        prettykitty_Button.Size = new Size(156, 23);
        prettykitty_Button.TabIndex = 10;
        prettykitty_Button.Text = "The Pretty Kitty Emporium";
        prettykitty_Button.UseVisualStyleBackColor = true;
        // 
        // discord_Button
        // 
        discord_Button.BackColor = Color.SlateBlue;
        discord_Button.ForeColor = Color.White;
        discord_Button.Location = new Point(0, 161);
        discord_Button.Name = "discord_Button";
        discord_Button.Size = new Size(156, 28);
        discord_Button.TabIndex = 11;
        discord_Button.Text = "Discord";
        discord_Button.UseVisualStyleBackColor = false;
        // 
        // donate_Button
        // 
        donate_Button.BackColor = Color.LightCoral;
        donate_Button.ForeColor = Color.White;
        donate_Button.Location = new Point(156, 161);
        donate_Button.Name = "donate_Button";
        donate_Button.Size = new Size(156, 28);
        donate_Button.TabIndex = 12;
        donate_Button.Text = "Donate";
        donate_Button.UseVisualStyleBackColor = false;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold);
        label1.Location = new Point(0, 85);
        label1.Name = "label1";
        label1.Size = new Size(109, 25);
        label1.TabIndex = 13;
        label1.Text = "Quick links";
        // 
        // MainWindow
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(313, 191);
        Controls.Add(label1);
        Controls.Add(donate_Button);
        Controls.Add(discord_Button);
        Controls.Add(prettykitty_Button);
        Controls.Add(heliosphere_Button);
        Controls.Add(aetherlink_Button);
        Controls.Add(nexusmods_Button);
        Controls.Add(glamourdressor_Button);
        Controls.Add(xivarchive_Button);
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
    private Button xivarchive_Button;
    private Button glamourdressor_Button;
    private Button nexusmods_Button;
    private Button aetherlink_Button;
    private Button heliosphere_Button;
    private Button prettykitty_Button;
    private Button discord_Button;
    private Button donate_Button;
    private Label label1;
}