﻿
namespace PenumbraModForwarder.Common.Interfaces;

public interface ISystemTrayManager : IDisposable
{
    void ShowNotification(string title, string message);
    event Action OnExitRequested;
    void TriggerExit();
}