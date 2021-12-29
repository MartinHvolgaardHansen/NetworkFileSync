using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace NetworkFileSync
{
    class Program
    {
	    public static IConfigurationRoot configuration;

        private static string localFolder = @"C:\FileSyncLocal\";
        private static string remoteFolder = @"C:\FileSyncRemote\";
        static void Main(string[] args)
        {
		    configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .Build();

            using var localWatcher = new System.IO.FileSystemWatcher();
            localWatcher.Path = localFolder;
            localWatcher.Filter = "*.txt";
            localWatcher.IncludeSubdirectories = true;
            localWatcher.EnableRaisingEvents = true;
            localWatcher.Created += (o, e) => OnLocalWrite(e);
            localWatcher.Changed += (o, e) => OnLocalWrite(e);
            localWatcher.Deleted += (o, e) => OnLocalWrite(e);
            localWatcher.Renamed += (o, e) => OnLocalWrite(e);

            Console.ReadLine();
        }

        private static void OnLocalWrite(FileSystemEventArgs e)
        {
            var fullPath = e.FullPath;
            var fileName = fullPath.Replace(localFolder, "");
            var changeType = e.ChangeType;
            var fileChange = new FileChange { Name = fileName, Type = changeType };
            switch (changeType)
            {
                case WatcherChangeTypes.Created:
                case WatcherChangeTypes.Changed:
                    string content = File.ReadAllText(fullPath);
                    fileChange.Content = content;
                    UpdateLocal(fileChange);
                    break;
                case WatcherChangeTypes.Deleted:
                    UpdateLocal(fileChange);
                    break;
                case WatcherChangeTypes.Renamed:
                    break;
                default:
                    break;
            }
        }

        private static void UpdateLocal(FileChange fileChange)
        {
            var filePath = remoteFolder + fileChange.Name;
            switch (fileChange.Type)
            {
                case WatcherChangeTypes.Created:
                case WatcherChangeTypes.Changed:
                    File.WriteAllText(filePath, fileChange.Content);
                    break;
                case WatcherChangeTypes.Deleted:
                    File.Delete(filePath);
                    break;
                case WatcherChangeTypes.Renamed:
                    // Rename file: Rename-Item -Path $filePath -NewName $Content
                    break;
            }
        }

        private static void UpdateRemote(FileChange fileChange)
        {

        }
    }
}
