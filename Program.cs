using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace NetworkFileSync
{
    class Program
    {
        private static IConfigurationRoot _configuration;
        private static TcpTransceiver<FileChange> _transceiver;
        private static TcpClient _connection;
        private static string _clientServer;
        private static string _blueprintsFolder;
        private static IPAddress _remoteAddress;
        private static int _remotePort;
        private static FileSystemWatcher _localWatcher;

        static void Main(string[] args)
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .Build();
            _clientServer = _configuration["Settings:clientServer"];
            _blueprintsFolder = _configuration["Settings:blueprintsFolder"];
            _remoteAddress = Dns.GetHostEntry(_configuration["Settings:remoteAddress"]).AddressList[0];
            _remotePort = int.Parse(_configuration["Settings:remotePort"]);
            var encoder = new FileChangeEncoder();
            _transceiver = new(encoder);

            switch (_clientServer.ToUpper())
            {
                case "SERVER":
                    var server = new ConnectionListener(_remoteAddress, _remotePort);
                    System.Console.WriteLine("Listening...");
                    _connection = server.Listen();
                    System.Console.WriteLine("Connected!");
                    _transceiver.BeginReceive(_connection, UpdateLocal);
                    break;
                case "CLIENT":
                    var client = new ClientConnector(_remoteAddress, _remotePort);
                    System.Console.WriteLine("Connecting...");
                    _connection = client.Connect();
                    System.Console.WriteLine("Connected!");
                    _transceiver.BeginReceive(_connection, UpdateLocal);
                    break;
            }

            CreateLocalWatcher();

            Console.ReadLine();
        }
        private static void OnLocalWrite(object sender, FileSystemEventArgs e)
        {
            var fullPath = e.FullPath;
            var fileName = fullPath.Replace(_blueprintsFolder, "");
            var changeType = e.ChangeType;
            var fileChange = new FileChange { Name = fileName, Type = changeType };
            Task.Run(() => UpdateRemote(fileChange));
        }

        private static void UpdateLocal(FileChange fileChange)
        {
            _localWatcher.Dispose();
            System.Console.WriteLine($"Update local: \t{fileChange.Type} \t{fileChange.Name}");
            var filePath = _blueprintsFolder + fileChange.Name;
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
            CreateLocalWatcher();
        }

        private static void CreateLocalWatcher() {
            _localWatcher = new FileSystemWatcher();
            _localWatcher.Path = _blueprintsFolder;
            _localWatcher.Filter = "*.txt";
            _localWatcher.IncludeSubdirectories = true;
            _localWatcher.EnableRaisingEvents = true;
            _localWatcher.Created += OnLocalWrite;
            _localWatcher.Changed += OnLocalWrite;
            _localWatcher.Deleted += OnLocalWrite;
            _localWatcher.Renamed += OnLocalWrite;
        }


        private static async Task UpdateRemote(FileChange fileChange)
        {
            System.Console.WriteLine($"Update remote: \t{fileChange.Type} \t{fileChange.Name}");
            await _transceiver.BeginSend(_connection, fileChange);
        }
    }
}
