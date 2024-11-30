using Moq;
using PenumbraModForwarder.Common.Consts;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.Common.Services;

namespace PenumbraModForwarder.Common.Tests.Services;

public class ConfigurationServiceTests
{
    private readonly Mock<IFileStorage> _mockFileStorage;
    private readonly ConfigurationService _configService;

    public ConfigurationServiceTests()
    {
        _mockFileStorage = new Mock<IFileStorage>();
        _configService = new ConfigurationService(_mockFileStorage.Object);
    }

    [Fact]
    public void ReturnConfigValue_ReturnsCorrectBooleanProperty()
    {
        // Act
        var autoLoadValue = (bool)_configService.ReturnConfigValue(config => config.AutoLoad);

        // Assert
        Assert.False(autoLoadValue); // Default value
    }

    [Fact]
    public void UpdateConfigValue_ModifiesAutoLoadProperty()
    {
        // Act
        _configService.UpdateConfigValue(config => config.AutoLoad = true);

        // Assert
        var updatedValue = (bool)_configService.ReturnConfigValue(config => config.AutoLoad);
        Assert.True(updatedValue);
    }

    [Fact]
    public void UpdateConfigValue_ModifiesDownloadPath()
    {
        // Act
        _configService.UpdateConfigValue(config => config.DownloadPath = @"C:\Test\Path");

        // Assert
        var updatedPath = (string)_configService.ReturnConfigValue(config => config.DownloadPath);
        Assert.Equal(@"C:\Test\Path", updatedPath);
    }

    [Fact]
    public void CreateConfiguration_CreatesRequiredDirectories()
    {
        // Act
        _configService.CreateConfiguration();

        // Assert
        _mockFileStorage.Verify(fs => fs.CreateDirectory(It.IsAny<string>()), Times.Exactly(4));
    }
    
    [Fact]
    public void MultiplePropertyUpdates_SavesCompleteConfiguration()
    {
        _configService.UpdateConfigValue(config => 
        {
            config.AutoLoad = true;
            config.DownloadPath = @"C:\TestDownload";
            config.NotificationEnabled = false;
        });

        // Verify each updated property
        Assert.True((bool)_configService.ReturnConfigValue(c => c.AutoLoad));
        Assert.Equal(@"C:\TestDownload", 
            (string)_configService.ReturnConfigValue(c => c.DownloadPath));
        Assert.False((bool)_configService.ReturnConfigValue(c => c.NotificationEnabled));
    }
    
    [Fact]
    public void CreateConfiguration_CreatesDirectoriesAndConfigFile()
    {
        _configService.CreateConfiguration();

        // Verify directories are created
        _mockFileStorage.Verify(fs => fs.CreateDirectory(It.IsAny<string>()), Times.Exactly(4));
    
        // Verify config file is written (if it doesn't exist)
        _mockFileStorage.Verify(fs => fs.Write(
                It.Is<string>(path => path == ConfigurationConsts.ConfigurationFilePath), 
                It.IsAny<string>()), 
            Times.Once);
    }
    
    [Fact]
    public void AdvancedOptions_DefaultInitialization()
    {
        // Verify Advanced Options are not null on default initialization
        var advancedOptions = (AdvancedConfigurationModel)_configService
            .ReturnConfigValue(config => config.AdvancedOptions);
        
        Assert.NotNull(advancedOptions);
    }
}