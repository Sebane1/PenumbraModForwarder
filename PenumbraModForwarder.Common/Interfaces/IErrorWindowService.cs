namespace PenumbraModForwarder.Common.Interfaces;

public interface IErrorWindowService
{
    public void ShowError(string message);
    public string? TexToolPathError();
}