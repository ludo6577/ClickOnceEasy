using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, string> infoDic;
            try
            {
                infoDic = ReadInfoFile("infos.txt");
            }
            catch (Exception e)
            {
                Console.WriteLine("No infos.txt found");
                Console.WriteLine("Press a key to continue...");
                Console.ReadLine();
                return;
            }

            try
            {
                // Check for an update on the server
                var updateAvailable = CheckUpdateAvailable(Path.Combine(infoDic["ServerUrl"], "infos.txt"), infoDic["Version"]);
                if (updateAvailable)
                {
                    Process.Start(Path.Combine(infoDic["ServerUrl"], infoDic["AppName"]) + ".application");
                    return;
                }
                
                // Unzip file and create desktop icon
                var zipName = "App.zip";
                if (File.Exists(zipName))
                {
                   UnzipApp(zipName);
                   CreateDesktopIcon(infoDic["Publisher"], infoDic["AppName"]);
                }

                // Get this program path
                var fullPath = Process.GetCurrentProcess().MainModule.FileName;
                
                // Used only while executing in visual studio
                var fileNameVsHost = "";
                if(fullPath.Contains("vshost"))
                    fileNameVsHost = fullPath.Substring(0, fullPath.LastIndexOf("vshost")) + "exe";   //Remove the .vshost
                else
                    fileNameVsHost = fullPath.Substring(0, fullPath.LastIndexOf('.')) + ".vshost.exe";  //Add the .vshost

                // Find all exe files in the current folder and start the first one
                string[] filesPaths = Directory.GetFiles(fullPath.Substring(0, fullPath.LastIndexOf("\\")), "*.exe");
                foreach (var file in filesPaths)
                {
                    if (file != fullPath && file != fileNameVsHost)
                    {
                        Process.Start(file);
                        return;
                    }
                }

                Console.WriteLine("No exe file founds");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("Press a key to continue...");
            Console.ReadLine();
        }

        private static Dictionary<string, string> ReadInfoFile(string path)
        {
            var dic = new Dictionary<string, string>();
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                var values = line.Split(':');
                dic.Add(values[0], values[1]);
            }
            return dic;
        }

        private static bool CheckUpdateAvailable(string url, string clientVersion)
        {
            var serverInfos = ReadInfoFile(url);
            if (Version.Parse(serverInfos["Version"]) > Version.Parse(clientVersion))
            {
                return true;
            }
            return false;
        }

        private static void UnzipApp(string zipName)
        {
            Console.WriteLine("First execution: unziping project");
            using (ZipArchive archive = ZipFile.OpenRead(zipName))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (File.Exists(entry.FullName))
                        File.Delete(entry.FullName);

                    var paths = entry.FullName.Split('\\');
                    if (paths.Length > 1)
                    {
                        string path = "";
                        for (var i = 0; i < paths.Length - 1; i++)
                        {
                            path = Path.Combine(path, paths[i]);
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);
                        }
                    }

                    entry.ExtractToFile(entry.FullName);
                }
            }
            Console.WriteLine("Project unziped with success");
            File.Delete(zipName);
        }

        private static void CreateDesktopIcon(string Publisher, string AppName)
        {
            string desktopPath = string.Empty;
            desktopPath = string.Concat(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "\\",
                AppName,
                ".appref-ms");

            string shortcutName = string.Empty;
            shortcutName = string.Concat(
                Environment.GetFolderPath(Environment.SpecialFolder.Programs),
                "\\",
                Publisher,
                "\\",
                AppName,
                ".appref-ms");

            File.Copy(shortcutName, desktopPath, true);
        }
    }
}
