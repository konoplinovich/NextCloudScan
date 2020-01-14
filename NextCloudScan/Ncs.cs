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
        private static bool _showConfigParametersOnStart;
        private static FileDataBase _fdb;
        private static TimeSpan _interval;

        static void Main(string[] args)
        {
            if (args.Length == 0) return;
            _configFile = args[0];
            _config = new ConfigExtension<NcsConfig>(_configFile);
            ConfigExtension<NcsConfig>.LoadStatus status = _config.LoadConfig();

            if (status == ConfigExtension<NcsConfig>.LoadStatus.LoadedDefault)
            {
                ShowDefaultConfigBanner();
                return;
            }

            MapConfigParameters();
            Scan();

            if (_showFileDetails && !_fdb.IsNewBase)
            {
                Console.WriteLine();
                ShowDetails();
            }

            ShowSummary();
            ExitWaiter();
        }

        private static void Scan()
        {
            Console.WriteLine();
            Marker(Mark.Scan);
            Console.Write("Scan ...");

            DateTime start = DateTime.Now;

            _fdb = new FileDataBase(_path, _baseFile, _diffFile, _affectedFile);

            DateTime stop = DateTime.Now;
            _interval = stop - start;

            Console.WriteLine("\b\b\bcomplete.");
        }

        private static void ShowDetails()
        {
            foreach (FileItem item in _fdb.Removed)
            {
                Marker(Mark.Remove);
                Console.WriteLine(item);
            }

            foreach (FileItem item in _fdb.Added)
            {
                Marker(Mark.Add);
                Console.WriteLine(item);
            }

            Console.WriteLine();

            foreach (string path in _fdb.AffectedFolders)
            {
                Marker(Mark.Affected);
                Console.WriteLine(path);
            }
        }

        private static void ShowSummary()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("---");

            bool thereAreChanges = _fdb.Removed.Count != 0 || _fdb.Added.Count != 0 || _fdb.AffectedFoldersCount != 0;

            if (_fdb.Removed.Count != 0) Console.WriteLine($"Removed: {_fdb.Removed.Count}");
            if (_fdb.Added.Count != 0) Console.WriteLine($"Added: {_fdb.Added.Count}");
            if (_fdb.AffectedFoldersCount != 0) Console.WriteLine($"Affected folders: {_fdb.AffectedFoldersCount}");
            if (thereAreChanges) Console.WriteLine();

            Console.WriteLine($"Total in the database {_fdb.Count} files, scan time: {_interval.TotalSeconds}");
            Console.ResetColor();
        }

        private static void MapConfigParameters()
        {
            _path = _config.Parameters.Path;
            _baseFile = _config.Parameters.BaseFile;
            _diffFile = _config.Parameters.DiffFile;
            _affectedFile = _config.Parameters.AffectedFoldersFile;
            _waitOnExit = _config.Parameters.WaitOnExit;
            _showFileDetails = _config.Parameters.ShowFileDetails;
            _showConfigParametersOnStart = _config.Parameters.ShowConfigParametersOnStart;

            if (_showConfigParametersOnStart)
            {
                Console.WriteLine();
                ShowConfigParameters();
            }
        }

        private static void ShowDefaultConfigBanner()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"The specified configuration file \"{_configFile}\" is missing.");
            Console.WriteLine($"A new file was created with this name and the following default settings:");

            ShowConfigParameters();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[!] Check the configuration file before next run!");
            Console.ResetColor();
        }

        private static void ShowConfigParameters()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Path: {_config.Parameters.Path}");
            Console.WriteLine($"Base file: {_config.Parameters.BaseFile}");
            Console.WriteLine($"Diff file: {_config.Parameters.DiffFile}");
            Console.WriteLine($"Affected folders file: {_config.Parameters.AffectedFoldersFile}");
            Console.WriteLine($"Show config on start: {_config.Parameters.ShowConfigParametersOnStart}");
            Console.WriteLine($"Wait on exit: {_config.Parameters.WaitOnExit}");
            Console.WriteLine($"Show file details: {_config.Parameters.ShowFileDetails}");
            Console.ResetColor();
        }

        private static void ExitWaiter()
        {
            if (!_waitOnExit) return;

            Console.WriteLine();
            Console.Write("press <Enter> to exit... ");
            while (Console.ReadKey().Key != ConsoleKey.Enter) { }
            Console.WriteLine();
        }

        private static void Marker(Mark mark)
        {
            switch (mark)
            {
                case Mark.Add:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("[+]");
                    break;
                case Mark.Remove:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[-]");
                    break;
                case Mark.Affected:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("[#]");
                    break;
                case Mark.Scan:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("[>]");
                    break;
                default:
                    break;
            }
            Console.Write(" ");
            Console.ResetColor();
        }

        private enum Mark
        {
            Add,
            Remove,
            Affected,
            Scan
        }
    }
}