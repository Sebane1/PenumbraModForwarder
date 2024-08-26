using System.Diagnostics;
using System.Security.Principal;
using PenumbraModForwarder.Common.Interfaces;

public class AdminService : IAdminService
{
    public void PromptForAdminRestart()
    {
        var result = MessageBox.Show(
            "The application needs to restart with administrator privileges to update the registry. Do you want to restart as administrator?",
            "Administrator Privileges Required",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            RestartAsAdmin();
        }
    }

    private void RestartAsAdmin(string argument = "--admin")
    {
        try
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    UseShellExecute = true,
                    Verb = "runas",
                    Arguments = argument
                }
            };
            proc.Start();
            Environment.Exit(0);
        }
        catch (Exception e)
        {
            MessageBox.Show("Failed to restart the application as administrator. Please manually restart it with elevated privileges.");
        }
    }


    public void PromptForUserRestart()
    {
        MessageBox.Show("Registry changes have been made. Please restart the application in normal user mode.", "Restart Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public bool IsAdmin()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
