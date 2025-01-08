using Moq;
using PenumbraModForwarder.Common.Events;
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
        
        _mockFileStorage.Setup(fs => fs.Exists(It.IsAny<string>())).Returns(false);
        
        _mockFileStorage.Setup(fs => fs.OpenWrite(It.IsAny<string>()))
            .Returns(() => new MemoryStream());
        
        _mockFileStorage.Setup(fs => fs.OpenRead(It.IsAny<string>()))
            .Returns(() => new MemoryStream());

        _configService = new ConfigurationService(_mockFileStorage.Object);
    }

    [Fact]
    public void UpdateConfigValue_ModifiesEnableSentryProperty()
    {
        _configService.UpdateConfigValue(
            config => config.Common.EnableSentry = true,
            "Common.EnableSentry", 
            true
        );

        var updatedValue = (bool)_configService.ReturnConfigValue(
            config => config.Common.EnableSentry
        );
        Assert.True(updatedValue);
    }

    [Fact]
    public void UpdateConfigValue_ModifiesDownloadPath()
    {
        // Act
        var testPath = new List<string> { @"/test/path" };
        _configService.UpdateConfigValue(config => config.BackgroundWorker.DownloadPath = testPath, "BackgroundWorker.DownloadPath", testPath);

        // Assert
        var updatedPath = (List<string>)_configService.ReturnConfigValue(config => config.BackgroundWorker.DownloadPath);
        Assert.Equal(testPath, updatedPath);
    }


    [Fact]
    public void CreateConfiguration_CreatesRequiredDirectories()
    {
        // Act
        _configService.CreateConfiguration();

        // Assert
        _mockFileStorage.Verify(fs => fs.CreateDirectory(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void AdvancedOptions_DefaultInitialization()
    {
        // Verify Advanced Options are not null on default initialization
        var advancedOptions = (AdvancedConfigurationModel)_configService.ReturnConfigValue(config => config.AdvancedOptions);
        Assert.NotNull(advancedOptions);
    }
        
    [Fact]
    public void MultiplePropertyUpdates_RaisesSingleConfigurationChangedEvent()
    {
        // Arrange
        var changes = new List<ConfigurationChangedEventArgs>();
        _configService.ConfigurationChanged += (sender, args) => changes.Add(args);

        // Act
        _configService.UpdateConfigValue(config =>
            {
                config.Common.EnableSentry = true;
                config.BackgroundWorker.DownloadPath = new List<string> { @"/test/path" };
                config.UI.NotificationEnabled = false;
            },
            "MultipleProperties",
            null);

        // Assert
        Assert.Single(changes);
        var change = changes.First();
        Assert.Equal("MultipleProperties", change.PropertyName);
        Assert.Null(change.NewValue);
    }

    [Fact]
    public void UpdateConfigValue_RaisesConfigurationChangedEventWithDetails()
    {
        // Arrange
        var eventRaised = false;
        _configService.ConfigurationChanged += (sender, args) =>
        {
            eventRaised = true;
            Assert.Equal("Common.EnableSentry", args.PropertyName);
            Assert.True((bool)args.NewValue);
        };

        // Act
        _configService.UpdateConfigValue(config => config.Common.EnableSentry = true, "Common.EnableSentry", true);

        // Assert
        Assert.True(eventRaised, "ConfigurationChanged event was not raised.");
    }

    [Fact]
    public void MultiplePropertyUpdates_RaisesConfigurationChangedEventForEachChange()
    {
        // Arrange
        var changes = new List<ConfigurationChangedEventArgs>();
        _configService.ConfigurationChanged += (sender, args) => changes.Add(args);

        // Act
        _configService.UpdateConfigValue(config => config.Common.EnableSentry = true, "Common.EnableSentry", true);
        _configService.UpdateConfigValue(config => config.BackgroundWorker.DownloadPath = new List<string> { @"/test/path" }, "BackgroundWorker.DownloadPath", new List<string> { @"/test/path" });
        _configService.UpdateConfigValue(config => config.UI.NotificationEnabled = false, "UI.NotificationEnabled", false);

        // Assert
        Assert.Contains(changes, change => change.PropertyName == "Common.EnableSentry" && (bool)change.NewValue);
        Assert.Contains(changes, change => change.PropertyName == "BackgroundWorker.DownloadPath" && ((List<string>)change.NewValue).SequenceEqual(new List<string> { @"/test/path" }));
        Assert.Contains(changes, change => change.PropertyName == "UI.NotificationEnabled" && (bool)change.NewValue == false);
    }
}