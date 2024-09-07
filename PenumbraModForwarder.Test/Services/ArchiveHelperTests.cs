using Microsoft.Extensions.Logging;
using Moq;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.Common.Services;
using Xunit;
using System;
using System.IO;
using System.Threading.Tasks;

public class ArchiveHelperTests
{
    private readonly Mock<ILogger<ArchiveHelperService>> _loggerMock;
    private readonly Mock<IFileSelector> _fileSelectorMock;
    private readonly Mock<IPenumbraInstallerService> _penumbraInstallerServiceMock;
    private readonly Mock<IConfigurationService> _configurationServiceMock;
    private readonly Mock<IErrorWindowService> _errorWindowServiceMock;
    private readonly Mock<IArkService> _arkServiceMock;
    private readonly Mock<IProgressWindowService> _progressWindowServiceMock;
    private readonly ArchiveHelperService _service;

    public ArchiveHelperTests()
    {
        _loggerMock = new Mock<ILogger<ArchiveHelperService>>();
        _fileSelectorMock = new Mock<IFileSelector>();
        _penumbraInstallerServiceMock = new Mock<IPenumbraInstallerService>();
        _configurationServiceMock = new Mock<IConfigurationService>();
        _errorWindowServiceMock = new Mock<IErrorWindowService>();
        _arkServiceMock = new Mock<IArkService>();
        _progressWindowServiceMock = new Mock<IProgressWindowService>();

        // Create the actual service instance with mocked dependencies
        _service = new ArchiveHelperService(
            _loggerMock.Object,
            _fileSelectorMock.Object,
            _penumbraInstallerServiceMock.Object,
            _configurationServiceMock.Object,
            _errorWindowServiceMock.Object,
            _arkServiceMock.Object,
            _progressWindowServiceMock.Object
        );
    }

    [Fact]
    public async Task ExtractArchive_ShouldThrowArgumentNullException_WhenFilePathIsNullOrEmpty()
    {
        string filePath = null;
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.QueueExtractionAsync(filePath));
        VerifyLoggerMessageAtLeastOnce(_loggerMock, LogLevel.Error, "File path is null or empty.");
    }

    [Fact]
    public async Task ExtractArchive_ShouldCallInstallArkFile_WhenArchiveContainsRolePlayVoiceFile()
    {
        var filePath = "test.rpvsp";
        var files = new[] { "test.rpvsp" };
        // Directly mock GetFilesInArchive method using Moq for dependency control
        var serviceWithMockedFiles = CreateArchiveHelperServiceWithMockedFiles(files);

        await serviceWithMockedFiles.QueueExtractionAsync(filePath);

        _arkServiceMock.Verify(ark => ark.InstallArkFile(filePath), Times.Once);
    }

    [Fact]
    public async Task ExtractArchive_ShouldExtractAndInstallFile_WhenSingleFileInArchive()
    {
        var files = new[] { "mod.ttmp2" };
        var tempZipPath = CreateDummyZipFile("mod.ttmp2");
        var serviceWithMockedFiles = CreateArchiveHelperServiceWithMockedFiles(files);

        await serviceWithMockedFiles.QueueExtractionAsync(tempZipPath);

        _penumbraInstallerServiceMock.Verify(install => install.InstallMod(It.IsAny<string>()), Times.Once);
        CleanUpFile(tempZipPath);
    }

    [Fact]
    public async Task ExtractArchive_ShouldShowFileSelector_WhenMultipleFilesInArchiveAndExtractAllIsFalse()
    {
        var files = new[] { "mod1.ttmp2", "mod2.ttmp2" };
        var tempZipPath = CreateDummyZipFile("mod1.ttmp2", "mod2.ttmp2");
        var serviceWithMockedFiles = CreateArchiveHelperServiceWithMockedFiles(files);

        _configurationServiceMock.Setup(config => config.GetConfigValue(It.IsAny<Func<ConfigurationModel, bool>>())).Returns(false);
        _fileSelectorMock.Setup(selector => selector.SelectFiles(It.IsAny<string[]>(), It.IsAny<string>())).Returns(new[] { "mod1.ttmp2" });

        await serviceWithMockedFiles.QueueExtractionAsync(tempZipPath);

        _fileSelectorMock.Verify(selector => selector.SelectFiles(It.IsAny<string[]>(), It.IsAny<string>()), Times.Once);
        _penumbraInstallerServiceMock.Verify(install => install.InstallMod(It.IsAny<string>()), Times.Once);
        CleanUpFile(tempZipPath);
    }

    [Fact]
    public async Task ExtractArchive_ShouldDeleteArchive_WhenAutoDeleteIsTrue()
    {
        var files = new[] { "mod.ttmp2" };
        var tempZipPath = CreateDummyZipFile("mod.ttmp2");
        var serviceWithMockedFiles = CreateArchiveHelperServiceWithMockedFiles(files);

        _configurationServiceMock.Setup(config => config.GetConfigValue(It.IsAny<Func<ConfigurationModel, bool>>())).Returns(true);

        await serviceWithMockedFiles.QueueExtractionAsync(tempZipPath);

        Assert.False(File.Exists(tempZipPath), "The archive file should be deleted after extraction.");
    }

    // Helper method to create a service with mocked GetFilesInArchive method
    private ArchiveHelperService CreateArchiveHelperServiceWithMockedFiles(string[] files)
    {
        var serviceMock = new Mock<ArchiveHelperService>(
            _loggerMock.Object,
            _fileSelectorMock.Object,
            _penumbraInstallerServiceMock.Object,
            _configurationServiceMock.Object,
            _errorWindowServiceMock.Object,
            _arkServiceMock.Object,
            _progressWindowServiceMock.Object
        ) { CallBase = true };

        serviceMock.Setup(service => service.GetFilesInArchive(It.IsAny<string>())).Returns(files);
        return serviceMock.Object;
    }

    private string CreateDummyZipFile(params string[] entryNames)
    {
        var tempFile = Path.GetTempFileName();
        var tempZipPath = Path.ChangeExtension(tempFile, ".zip");
        using (var archive = System.IO.Compression.ZipFile.Open(tempZipPath, System.IO.Compression.ZipArchiveMode.Create))
        {
            foreach (var entryName in entryNames)
            {
                var entry = archive.CreateEntry(entryName);
                using (var entryStream = entry.Open())
                using (var writer = new StreamWriter(entryStream))
                {
                    writer.Write("Dummy content");
                }
            }
        }
        return tempZipPath;
    }

    private void CleanUpFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private void VerifyLoggerMessageAtLeastOnce(Mock<ILogger<ArchiveHelperService>> loggerMock, LogLevel level, string expectedMessage)
    {
        loggerMock.Verify(
            logger => logger.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedMessage)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.AtLeastOnce, $"Expected the log message '{expectedMessage}' to be logged at least once, but it was not logged.");
    }
}
