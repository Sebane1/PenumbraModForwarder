using Moq;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Statistics.Enums;
using PenumbraModForwarder.Statistics.Interfaces;
using PenumbraModForwarder.Statistics.Services;

namespace PenumbraModForwarder.Statistics.Tests.Services;

public class StatisticsServiceTests : IDisposable
{
    private readonly Mock<IFileStorage> _mockFileStorage;
    private readonly IStatisticService _statisticsService;
    private readonly string _tempDatabasePath;

    public StatisticsServiceTests()
    {
        _mockFileStorage = new Mock<IFileStorage>();

        // Mock the Exists method to return false to simulate a non-existing database file
        _mockFileStorage.Setup(fs => fs.Exists(It.IsAny<string>())).Returns(false);

        // Create a temporary file for the database
        _tempDatabasePath = Path.GetTempFileName();

        // Pass the temporary database path to the StatisticService
        _statisticsService = new StatisticService(_mockFileStorage.Object, _tempDatabasePath);
    }

    [Fact]
    public async Task IncrementStatAsync_IncrementsModsInstalledCount()
    {
        // Arrange
        var initialCount = await _statisticsService.GetStatCountAsync(Stat.ModsInstalled);

        // Act
        await _statisticsService.IncrementStatAsync(Stat.ModsInstalled);

        var updatedCount = await _statisticsService.GetStatCountAsync(Stat.ModsInstalled);

        // Assert
        Assert.Equal(initialCount + 1, updatedCount);
    }

    [Fact]
    public async Task GetStatCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        await _statisticsService.IncrementStatAsync(Stat.ModsInstalled);
        await _statisticsService.IncrementStatAsync(Stat.ModsInstalled);

        // Act
        var count = await _statisticsService.GetStatCountAsync(Stat.ModsInstalled);

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public void EnsureDatabaseExists_CreatesDatabaseFile()
    {
        // Act
        _statisticsService.IncrementStatAsync(Stat.ModsInstalled).Wait();

        // Assert
        _mockFileStorage.Verify(fs => fs.Exists(It.IsAny<string>()), Times.Once);
        _mockFileStorage.Verify(fs => fs.CreateDirectory(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task RecordModInstallationAsync_AddsNewModInstallationRecord()
    {
        // Arrange
        string modName = "Test Mod";

        // Act
        await _statisticsService.RecordModInstallationAsync(modName);

        // Assert
        var mostRecentMod = await _statisticsService.GetMostRecentModInstallationAsync();
        Assert.NotNull(mostRecentMod);
        Assert.Equal(modName, mostRecentMod.ModName);
    }

    [Fact]
    public async Task GetMostRecentModInstallationAsync_ReturnsMostRecentlyInstalledMod()
    {
        // Arrange
        string modName1 = "First Mod";
        string modName2 = "Second Mod";

        await _statisticsService.RecordModInstallationAsync(modName1);
        await Task.Delay(100); // Small delay to differentiate timestamps
        await _statisticsService.RecordModInstallationAsync(modName2);

        // Act
        var mostRecentMod = await _statisticsService.GetMostRecentModInstallationAsync();

        // Assert
        Assert.NotNull(mostRecentMod);
        Assert.Equal(modName2, mostRecentMod.ModName);
    }

    // Cleanup the temporary database file after tests
    public void Dispose()
    {
        if (File.Exists(_tempDatabasePath))
        {
            File.Delete(_tempDatabasePath);
        }
    }
}