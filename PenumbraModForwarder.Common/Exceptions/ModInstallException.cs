namespace PenumbraModForwarder.Common.Exceptions;

/// <summary>
/// Represents errors that occur during mod installation.
/// </summary>
public class ModInstallException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModInstallException"/> class.
    /// </summary>
    public ModInstallException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModInstallException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ModInstallException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModInstallException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ModInstallException(string message, Exception innerException) : base(message, innerException) { }
}