using Xunit;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Services;
using PenumbraModForwarder.Common.Interfaces;
using System.IO;

public class ConfigurationTest : IDisposable
{
    private readonly Mock<ILogger<ConfigurationService>> _loggerMock;
    private readonly Mock<IErrorWindowService> _errorWindowServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly string _tempDirectory;

    public ConfigurationTest()
    {
        _loggerMock = new Mock<ILogger<ConfigurationService>>();
        _errorWindowServiceMock = new Mock<IErrorWindowService>();
        _mapperMock = new Mock<IMapper>();
        _tempDirectory = Path.Combine(Path.GetTempPath(), "PenumbraModForwarderTest");

        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public void GetConfigValue_ShouldReturnCorrectValue()
    {
        // Arrange
        var service = new ConfigurationService(_loggerMock.Object, _mapperMock.Object, _errorWindowServiceMock.Object);
        service.SetConfigValue((config, value) => config.DownloadPath = value, "test/path");

        // Act
        var result = service.GetConfigValue(config => config.DownloadPath);

        // Assert
        Assert.Equal("test/path", result);
    }

    [Fact]
    public void SetConfigValue_ShouldSaveConfigAndTriggerConfigChangedEvent()
    {
        // Arrange
        var service = new ConfigurationService(_loggerMock.Object, _mapperMock.Object, _errorWindowServiceMock.Object);
        bool configChangedEventTriggered = false;
        service.ConfigChanged += (sender, args) => configChangedEventTriggered = true;

        // Act
        service.SetConfigValue((config, value) => config.AutoLoad = value, true);

        // Assert
        Assert.True(configChangedEventTriggered);
        Assert.True(service.GetConfigValue(config => config.AutoLoad));
    }

    [Fact]
    public void MigrateOldConfig_ShouldMigrateOldConfigFiles()
    {
        var oldConfigDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PenumbraModForwarder", "PenumbraModForwarder");
        var oldDownloadPathConfig = Path.Combine(oldConfigDirectory, "DownloadPath.config");

        if (!Directory.Exists(oldConfigDirectory))
        {
            Directory.CreateDirectory(oldConfigDirectory);
        }

        File.WriteAllText(oldDownloadPathConfig, "old/path");

        var service = new ConfigurationService(_loggerMock.Object, _mapperMock.Object, _errorWindowServiceMock.Object);

        service.MigrateOldConfig();

        var newConfigValue = service.GetConfigValue(config => config.DownloadPath);
        Assert.Equal("old/path", newConfigValue);

        // Clean up
        if (File.Exists(oldDownloadPathConfig))
        {
            File.Delete(oldDownloadPathConfig);
        }
        if (Directory.Exists(oldConfigDirectory))
        {
            Directory.Delete(oldConfigDirectory, true);
        }
    }

    [Fact]
    public void MigrateOldConfig_ShouldHandleInvalidJsonInOldConfigFile()
    {
        var oldConfigDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PenumbraModForwarder", "PenumbraModForwarder");
        var oldConfigPath = Path.Combine(oldConfigDirectory, "Config.json");

        if (!Directory.Exists(oldConfigDirectory))
        {
            Directory.CreateDirectory(oldConfigDirectory);
        }

        File.WriteAllText(oldConfigPath, "Invalid JSON");

        var service = new ConfigurationService(_loggerMock.Object, _mapperMock.Object, _errorWindowServiceMock.Object);

        service.MigrateOldConfig();

        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Old config file is empty or invalid JSON")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        // Clean up
        if (File.Exists(oldConfigPath))
        {
            File.Delete(oldConfigPath);
        }
        if (Directory.Exists(oldConfigDirectory))
        {
            Directory.Delete(oldConfigDirectory, true);
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }
}
