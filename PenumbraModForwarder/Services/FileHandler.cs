using FFXIVModExractor.Services;
using SevenZip;
using System.IO.Compression;

namespace PenumbraModForwarder.Services
{
    internal static class FileHandler
    {
        public static void DeleteFile(string path)
        {
            if (Convert.ToBoolean(Options.ReadFromConfig("AutoDelete").Value))
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
            if (Convert.ToBoolean(Options.ReadFromConfig("AutoDelete").Value))
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
                // Handle exceptions as needed
                string error = ex.Message;
            }

            return extractedMods;
        }

        public static async Task WaitForFileRelease(string path)
        {
            Thread.Sleep(50);
            while (IsFileLocked(path))
            {
                Thread.Sleep(100);
            }
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
