using Moq;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Statistics.Enums;
using PenumbraModForwarder.Statistics.Interfaces;
using PenumbraModForwarder.Statistics.Services;

namespace PenumbraModForwarder.Statistics.Tests.Services;

public class StatisticsServiceTests
{
    private readonly Mock<IFileStorage> _mockFileStorage;
    private readonly IStatisticService _statisticsService;
    private readonly string _tempDatabasePath;

    public StatisticsServiceTests()
    {
        _mockFileStorage = new Mock<IFileStorage>();
        _mockFileStorage.Setup(fs => fs.Exists(It.IsAny<string>())).Returns(false);

        // Create a temporary file for the database
        _tempDatabasePath = Path.GetTempFileName();
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

    // Cleanup
    public void Dispose()
    {
        if (File.Exists(_tempDatabasePath))
        {
            File.Delete(_tempDatabasePath);
        }
    }
}