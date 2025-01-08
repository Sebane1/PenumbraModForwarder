using PenumbraModForwarder.Updater.Interfaces;

namespace PenumbraModForwarder.Updater.Services;

public class AppArguments : IAppArguments
{
    public AppArguments(string[] args)
    {
        Args = args;
    }

    public string[] Args { get; }
}