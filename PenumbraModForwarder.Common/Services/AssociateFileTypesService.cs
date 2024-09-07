using System.Reflection;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Interop;

namespace PenumbraModForwarder.Common.Services;

public class AssociateFileTypesService : IAssociateFileTypeService
{
    private readonly ILogger<AssociateFileTypesService> _logger;
    private readonly IRegistryHelper _registryHelper;
    private readonly IConfigurationService _configurationService;
    private readonly IErrorWindowService _errorWindowService;
    
    public AssociateFileTypesService(ILogger<AssociateFileTypesService> logger, IRegistryHelper registryHelper, IConfigurationService configurationService, IErrorWindowService errorWindowService)
    {
        _logger = logger;
        _registryHelper = registryHelper;
        _configurationService = configurationService;
        _errorWindowService = errorWindowService;
    }

    public void AssociateFileTypes(string applicationPath)
    {
        var allowedExtensions = new[] {".pmp", ".ttmp2", ".ttmp", ".rpvsp"};
        try
        {
            if (_configurationService.GetConfigValue(c => c.FileLinkingEnabled))
            {
                _logger.LogInformation("Associating file types");
                _registryHelper.CreateFileAssociation(allowedExtensions, applicationPath);
            }
            else
            {
                _logger.LogInformation("Disassociating file types");
                _registryHelper.RemoveFileAssociation(allowedExtensions);
            }
        
            // Let the computer know a change as occurred.
            Imports.SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to associate file types");
            _errorWindowService.ShowError(e.ToString());
        }
    }
}