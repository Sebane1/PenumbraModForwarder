namespace PenumbraModForwarder.Common.Interfaces;

public interface IProgressWindowService
{
    public void ShowProgressWindow();
    public void UpdateProgress(string fileName, string operation, int progress);
    public void CloseProgressWindow();
}