using Microsoft.Extensions.Logging;
using Moq;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.Common.Services;
using Xunit;
using System;
using System.IO;

public class ArchiveHelperTests
{
    private readonly Mock<ILogger<ArchiveHelperService>> _loggerMock;
    private readonly Mock<IFileSelector> _fileSelectorMock;
    private readonly Mock<IPenumbraInstallerService> _penumbraInstallerServiceMock;
    private readonly Mock<IConfigurationService> _configurationServiceMock;
    private readonly Mock<IErrorWindowService> _errorWindowServiceMock;
    private readonly Mock<IArkService> _arkServiceMock;
    private readonly Mock<ArchiveHelperService> _serviceMock;

    public ArchiveHelperTests()
    {
        _loggerMock = new Mock<ILogger<ArchiveHelperService>>();
        _fileSelectorMock = new Mock<IFileSelector>();
        _penumbraInstallerServiceMock = new Mock<IPenumbraInstallerService>();
        _configurationServiceMock = new Mock<IConfigurationService>();
        _errorWindowServiceMock = new Mock<IErrorWindowService>();
        _arkServiceMock = new Mock<IArkService>();

        _serviceMock = new Mock<ArchiveHelperService>(
            _loggerMock.Object,
            _fileSelectorMock.Object,
            _penumbraInstallerServiceMock.Object,
            _configurationServiceMock.Object,
            _errorWindowServiceMock.Object,
            _arkServiceMock.Object
        ) { CallBase = true };
    }

    [Fact]
    public void ExtractArchive_ShouldThrowArgumentNullException_WhenFilePathIsNullOrEmpty()
    {
        string filePath = null;
        Assert.ThrowsAsync<ArgumentNullException>(() => _serviceMock.Object.QueueExtractionAsync(filePath));
        VerifyLoggerMessageAtLeastOnce(_loggerMock, LogLevel.Error, "File path is null or empty.");
    }

    [Fact]
    public void ExtractArchive_ShouldCallInstallArkFile_WhenArchiveContainsRolePlayVoiceFile()
    {
        var filePath = "test.rpvsp";
        var files = new[] { "test.rpvsp" };
        _serviceMock.Setup(service => service.GetFilesInArchive(It.IsAny<string>())).Returns(files);
        _serviceMock.Object.QueueExtractionAsync(filePath);
        _arkServiceMock.Verify(ark => ark.InstallArkFile(filePath), Times.Once);
    }

    [Fact]
    public async Task ExtractArchive_ShouldExtractAndInstallFile_WhenSingleFileInArchive()
    {
        var files = new[] { "mod.ttmp2" };
        var tempZipPath = CreateDummyZipFile("mod.ttmp2");
        _serviceMock.Setup(service => service.GetFilesInArchive(It.IsAny<string>())).Returns(files);

        // Await the queue processing
        await _serviceMock.Object.QueueExtractionAsync(tempZipPath);

        // Verify the InstallMod method was called
        _penumbraInstallerServiceMock.Verify(install => install.InstallMod(It.IsAny<string>()), Times.Once);
        CleanUpFile(tempZipPath);
    }

    [Fact]
    public async Task ExtractArchive_ShouldShowFileSelector_WhenMultipleFilesInArchiveAndExtractAllIsFalse()
    {
        var files = new[] { "mod1.ttmp2", "mod2.ttmp2" };
        var tempZipPath = CreateDummyZipFile("mod1.ttmp2", "mod2.ttmp2");
        _serviceMock.Setup(service => service.GetFilesInArchive(It.IsAny<string>())).Returns(files);
        _configurationServiceMock.Setup(config => config.GetConfigValue(It.IsAny<Func<ConfigurationModel, bool>>())).Returns(false);
        _fileSelectorMock.Setup(selector => selector.SelectFiles(It.IsAny<string[]>(), It.IsAny<string>())).Returns(new[] { "mod1.ttmp2" });

        // Await the queue processing
        await _serviceMock.Object.QueueExtractionAsync(tempZipPath);

        // Verify that the file selector was shown and the file was installed
        _fileSelectorMock.Verify(selector => selector.SelectFiles(It.IsAny<string[]>(), It.IsAny<string>()), Times.Once);
        _penumbraInstallerServiceMock.Verify(install => install.InstallMod(It.IsAny<string>()), Times.Once);
        CleanUpFile(tempZipPath);
    }

    [Fact]
    public async Task ExtractArchive_ShouldDeleteArchive_WhenAutoDeleteIsTrue()
    {
        var files = new[] { "mod.ttmp2" };
        var tempZipPath = CreateDummyZipFile("mod.ttmp2");
        _serviceMock.Setup(service => service.GetFilesInArchive(It.IsAny<string>())).Returns(files);
        _configurationServiceMock.Setup(config => config.GetConfigValue(It.IsAny<Func<ConfigurationModel, bool>>())).Returns(true);

        // Await the queue processing
        await _serviceMock.Object.QueueExtractionAsync(tempZipPath);

        // Ensure the file is deleted after extraction completes
        Assert.False(File.Exists(tempZipPath), "The archive file should be deleted after extraction.");
        VerifyLoggerMessageAtLeastOnce(_loggerMock, LogLevel.Information, $"Deleting archive: {tempZipPath}");
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