namespace PenumbraModForwarder.Common.Interfaces;

public interface IVoidToolsEverything
{
    void SetSearch(string searchString);
    bool Query(bool wait);
    int GetNumResults();
    string GetResultFullPathName(int index);
}