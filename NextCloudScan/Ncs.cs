using FileScanLib;
using System;
using Extensions;
using System.IO;

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
        static void Main(string[] args)
        {
            if (args.Length == 0) return;
            _configFile = args[0];

            _config = new ConfigExtension<NcsConfig>(_configFile);
            ConfigExtension<NcsConfig>.LoadStatus status = _config.LoadConfig();

            if (status == ConfigExtension<NcsConfig>.LoadStatus.LoadedDefault)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"The specified configuration file \"{_configFile}\" is missing.");
                Console.WriteLine($"A new file was created with this name and the following default settings:");
                Console.WriteLine($"Path: {_config.Conf.Path}");
                Console.WriteLine($"Base: {_config.Conf.BaseFile}");
                Console.WriteLine($"Diff: {_config.Conf.DiffFile}");
                Console.WriteLine($"Affected: {_config.Conf.AffectedFoldersFile}");
                Console.WriteLine($"WaitOnExit: {_config.Conf.WaitOnExit}");
                Console.ResetColor();

                return;
            }

            _path = _config.Conf.Path;
            _baseFile = _config.Conf.BaseFile;
            _diffFile = _config.Conf.DiffFile;
            _affectedFile = _config.Conf.AffectedFoldersFile;
            _waitOnExit = _config.Conf.WaitOnExit;

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