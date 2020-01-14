using Extensions;
using FileScanLib;
using System;

namespace NextCloudScan
{
    class Ncs
    {
        private static ConfigExtension<NcsConfig> _config;
        private static string _configFile;
        private static string _path;
        private static string _baseFile;
        private static string _diffFile;
        private static string _affectedFile;
        private static bool _waitOnExit;
        private static bool _showFileDetails;
        static void Main(string[] args)
        {
            if (args.Length == 0) return;
            _configFile = args[0];

            _config = new ConfigExtension<NcsConfig>(_configFile);
            ConfigExtension<NcsConfig>.LoadStatus status = _config.LoadConfig();

            if (status == ConfigExtension<NcsConfig>.LoadStatus.LoadedDefault)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"The specified configuration file \"{_configFile}\" is missing.");
                Console.WriteLine($"A new file was created with this name and the following default settings:");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Path: {_config.Parameters.Path}");
                Console.WriteLine($"Base file: {_config.Parameters.BaseFile}");
                Console.WriteLine($"Diff file: {_config.Parameters.DiffFile}");
                Console.WriteLine($"Affected folders file: {_config.Parameters.AffectedFoldersFile}");
                Console.WriteLine($"Wait on exit: {_config.Parameters.WaitOnExit}");
                Console.WriteLine($"Show file details: {_config.Parameters.WaitOnExit}");
                Console.ResetColor();

                return;
            }

            _path = _config.Parameters.Path;
            _baseFile = _config.Parameters.BaseFile;
            _diffFile = _config.Parameters.DiffFile;
            _affectedFile = _config.Parameters.AffectedFoldersFile;
            _waitOnExit = _config.Parameters.WaitOnExit;
            _showFileDetails = _config.Parameters.ShowFileDetails;

            DateTime start = DateTime.Now;

            FileDataBase fdb = new FileDataBase(_path, _baseFile, _diffFile, _affectedFile);

            if (fdb.IsNewBase)
            {
                DateTime stop = DateTime.Now;
                TimeSpan interval = stop - start;

                Console.WriteLine($"{fdb.Count} files, time: {interval.TotalSeconds}");

                ExitWaiter(_waitOnExit);
            }
            else
            {
                DateTime stop = DateTime.Now;
                TimeSpan interval = stop - start;

                Console.WriteLine();

                if (_showFileDetails)
                {

                    foreach (FileItem item in fdb.Removed)
                    {
                        Marker(ConsoleColor.Red, "[-]");
                        Console.WriteLine(item);
                    }

                    foreach (FileItem item in fdb.Added)
                    {
                        Marker(ConsoleColor.Green, "[+]");
                        Console.WriteLine(item);
                    }

                    Console.WriteLine("---");

                    foreach (string path in fdb.AffectedFolders)
                    {
                        Console.WriteLine($"Affected: {path}");
                    }

                    Console.WriteLine("---");
                }

                Console.WriteLine($"Removed: {fdb.Removed.Count}");
                Console.WriteLine($"Added: {fdb.Added.Count}");
                Console.WriteLine($"Affected folders: {fdb.AffectedFoldersCount}");
                Console.WriteLine();
                Console.WriteLine($"{fdb.Count} files, time: {interval.TotalSeconds}");

                ExitWaiter(_waitOnExit);
            }
        }

        private static void ExitWaiter(bool waitOnExit)
        {
            if (!waitOnExit) return;

            Console.WriteLine();
            Console.Write("press <Enter> to exit... ");
            while (Console.ReadKey().Key != ConsoleKey.Enter) { }
            Console.WriteLine();
        }

        private static void Marker(ConsoleColor color, string mark)
        {
            Console.ForegroundColor = color;
            Console.Write($"{mark} ");
            Console.ResetColor();
        }
    }
}