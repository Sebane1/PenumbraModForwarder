﻿using System.IO.Compression;
using FFXIVModExractor.Services;
using SevenZip;

namespace PenumbraModForwarder.Services
{
    internal static class FileHandler
    {
        public static void DeleteFile(string path)
        {
            if (Options.GetConfigValue<bool>("AutoDelete"))
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error deleting file: {e.Message}");
                }
            }
        }

        public static void DeleteDirectory(string path)
        {
            if (Options.GetConfigValue<bool>("AutoDelete"))
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error deleting directory: {e.Message}");
                }
            }
        }

        public static bool IsRoleplayingVoiceFile(string path)
        {
            return path.EndsWith(".rpvsp");
        }

        public static bool IsModFile(string path)
        {
            return path.EndsWith(".ttmp") || path.EndsWith(".ttmp2") || path.EndsWith(".pmp");
        }

        public static bool IsArchiveFile(string path)
        {
            return path.EndsWith(".7z") || path.EndsWith(".rar") || path.EndsWith(".zip");
        }

        // TODO: Everything below this line needs to be converted from the respective functions inside MainWindow.cs
        public static async Task ProcessZipFile(string path)
        {
            await WaitForFileRelease(path);
            List<string> extractedMods = ExtractModsFromArchive(path);

            foreach (var item in extractedMods)
            {
                await WaitForFileRelease(item);
                bool success = false;
                //SendModToPenumbra(item, ref success);
            }

            DeleteFile(path);
        }

        private static List<string> ExtractModsFromZip(string path)
        {
            var extractedMods = new List<string>();

            using (var zip = ZipFile.OpenRead(path))
            {
                foreach (var item in zip.Entries)
                {
                    if (IsModFile(item.FullName) || IsRoleplayingVoiceFile(item.FullName))
                    {
                        string outputFile = Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(item.FullName));
                        item.ExtractToFile(outputFile);
                        extractedMods.Add(outputFile);
                    }
                }
            }

            return extractedMods;
        }

        private static List<string> ExtractModsFromArchive(string path)
        {
            var extractedMods = new List<string>();

            try
            {
                SevenZipExtractor.SetLibraryPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "7z.dll"));

                using (var archive = new SevenZipExtractor(path))
                {
                    int index = 0;
                    foreach (var item in archive.ArchiveFileNames)
                    {
                        if (IsModFile(item) || IsRoleplayingVoiceFile(item))
                        {
                            string outputFile = Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(item));
                            using (var outputFileStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                            {
                                archive.ExtractFile(index, outputFileStream);
                                extractedMods.Add(outputFile);
                            }
                        }

                        if (item.EndsWith("/"))
                        {
                            string outputDirectory = Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(item));
                            Directory.CreateDirectory(outputDirectory);
                            extractedMods.Add(outputDirectory);
                        }
                        index++;
                    }
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }

            return extractedMods;
        }

        public static async Task WaitForFileRelease(string filePath) 
        {
            const int maxRetries = 10;
            const int delayBetweenRetriesMs = 500;

            for (var i = 0; i < maxRetries; i++) {
                try {
                    await using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None)) {
                        return;
                    }
                } catch (IOException) {
                    await Task.Delay(delayBetweenRetriesMs);
                }
            }
            throw new IOException($"File {filePath} is locked and could not be accessed after multiple attempts.");
        }

        private static bool IsFileLocked(string file)
        {
            try
            {
                using (FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }
    }
}