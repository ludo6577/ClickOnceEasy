using System;
using System.IO;
using System.IO.Compression;
using System.Security.AccessControl;

namespace Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var zipName = "App.zip";

            if (File.Exists(zipName))
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

            string[] filesPaths = Directory.GetFiles(@".", "*.exe");
            foreach (var file in filesPaths)
            {
                if (file != ".\\" + AppDomain.CurrentDomain.FriendlyName)
                {
                    System.Diagnostics.Process.Start(file);
                    return;
                }
            }

            Console.WriteLine("No exe file founds");
            Console.WriteLine("Press a key to continue...");
            Console.ReadLine();
        }
    }
}
