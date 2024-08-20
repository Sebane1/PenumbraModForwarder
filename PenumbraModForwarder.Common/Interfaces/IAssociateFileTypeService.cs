namespace PenumbraModForwarder.Common.Interfaces;

public interface IAssociateFileTypeService
{
    void AssociateFileTypes(string extension, string applicationPath);
}