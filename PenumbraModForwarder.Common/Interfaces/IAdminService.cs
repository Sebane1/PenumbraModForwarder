namespace PenumbraModForwarder.Common.Interfaces;

public interface IAdminService
{
    public void PromptForAdminRestart();

    public void PromptForUserRestart();
    public bool IsAdmin();
}