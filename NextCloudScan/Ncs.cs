using Extensions;
using FileScanLib;
using System;
using System.Collections.Generic;

namespace NextCloudScan
{
    class Ncs
    {
        private static ConfigExtension<NcsConfig> _config;
        private static string _configFile;

        private static FileDataBase _fdb;
        private static TimeSpan _scanTime;
        private static ActionsResult _fileActionsResult;
        private static ActionsResult _folderActionsResult;

        static void Main(string[] args)
        {
            if (args.Length == 0) return;
            _configFile = args[0];

            try
            {
                _config = new ConfigExtension<NcsConfig>(_configFile);
                ConfigExtension<NcsConfig>.LoadStatus status = _config.LoadConfig();

                if (status == ConfigExtension<NcsConfig>.LoadStatus.LoadedDefault)
                {
                    ShowDefaultConfigBanner();
                    return;
                }

                ShowConfigParameters();
                Scan();
            }
            catch (Exception e)
            {
                ShowFatalException(e);
                return;
            }

            if (!_fdb.IsNewBase)
            {
                if (_config.Conf.ShowFileDetails)
                {
                    Console.WriteLine();
                    ShowFileDetails();
                }

                Console.WriteLine();
                ShowFolderDetails();
                ShowErrors();

                if (!string.IsNullOrEmpty(_config.Conf.FileActionApp))
                {
                    _fileActionsResult = Actions(_config.Conf.FileActionApp, _config.Conf.FileActionAppOptions, "Launch action for each new file", _fdb.AddedPath);
                    ShowActionsErrors(_fileActionsResult);
                }

                if (!string.IsNullOrEmpty(_config.Conf.FolderActionApp))
                {
                    if (_config.Conf.IsNextCloud)
                    {
                        _folderActionsResult = Actions(_config.Conf.FolderActionApp, _config.Conf.FolderActionAppOptions, "Launch action for each affected NextCloud folder", _fdb.AffectedFolders, isNextCloud: true);
                    }
                    else
                    {
                        _folderActionsResult = Actions(_config.Conf.FolderActionApp, _config.Conf.FolderActionAppOptions, "Launch action for each affected folder", _fdb.AffectedFolders);
                    }
                    ShowActionsErrors(_folderActionsResult);
                }
            }

            ShowSummary();
            GoAway();
        }

        private static void Scan()
        {
            Console.WriteLine();
            Marker(Mark.Scan);
            Console.Write("Scan ...");

            DateTime start = DateTime.Now;

            _fdb = new FileDataBase(_config.Conf.Path, _config.Conf.BaseFile, _config.Conf.DiffFile, _config.Conf.AffectedFoldersFile);

            DateTime stop = DateTime.Now;
            _scanTime = stop - start;

            Console.WriteLine("\b\b\bcomplete.");
        }

        private static ActionsResult Actions(string action, string actionOptions, string message, List<string> paths, bool isNextCloud = false)
        {
            //if (string.IsNullOrEmpty(_config.Conf.FileActionApp)) return null;

            Console.WriteLine();
            Marker(Mark.Scan);
            Console.WriteLine($"{message}: ");
            Console.WriteLine();

            ActionsResult result;

            if (isNextCloud)
            {
                Actions fa = new Actions(paths, action, actionOptions, new NcPathParser(), new List<string>() { _config.Conf.Path });
                result = fa.Run();
            }
            else
            {
                Actions fa = new Actions(paths, action, actionOptions);
                result = fa.Run();
            }

            return result;
        }

        private static void ShowFileDetails()
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
        }

        private static void ShowFolderDetails()
        {
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

            Console.WriteLine($"Total in the database {_fdb.Count} files, scan elapsed time: {_scanTime.TotalSeconds}");
            if (_fdb.Errors.Count != 0)
            {
                Console.WriteLine($"({_fdb.Errors.Count} folders unavailable during the last scan)");
            }

            Console.WriteLine();
            if (_fileActionsResult != null)
                Console.WriteLine($"File actions result: {_fileActionsResult.Completed} ok, {_fileActionsResult.Errors.Count} error, elapsed time: {_fileActionsResult.ElapsedTime}");
            if (_folderActionsResult != null)
                Console.WriteLine($"Folder action result: {_folderActionsResult.Completed} ok, {_folderActionsResult.Errors.Count} error, elapsed time: {_folderActionsResult.ElapsedTime}");
            Console.ResetColor();
        }

        private static void ShowErrors()
        {
            if (_fdb.Errors.Count == 0) return;

            Console.WriteLine();

            foreach (var item in _fdb.Errors)
            {
                Marker(Mark.Info);
                Console.WriteLine(item);
            }
        }

        private static void ShowActionsErrors(ActionsResult result)
        {
            if (result == null) return;
            if (result.Errors.Count == 0) return;

            foreach (string error in result.Errors)
            {
                Marker(Mark.Error);
                Console.WriteLine(error);
            }
        }

        private static void ShowFatalException(Exception e)
        {
            Console.WriteLine();
            Marker(Mark.Info);
            Console.WriteLine($"Config file: {_configFile}");
            Marker(Mark.Error);
            Console.WriteLine(e.Message);
            Marker(Mark.Error);
            Console.WriteLine("Exit");
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
            if (_config.Conf.ShowConfigParametersOnStart)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                Marker(Mark.Options); Console.WriteLine($"Files");
                Console.WriteLine($"    Path: {_config.Conf.Path}");
                Console.WriteLine($"    Base file: {_config.Conf.BaseFile}");
                Console.WriteLine($"    Diff file: {_config.Conf.DiffFile}");
                Console.WriteLine($"    Affected folders file: {_config.Conf.AffectedFoldersFile}");
                Console.WriteLine();
                Marker(Mark.Options); Console.WriteLine($"Options");
                Console.WriteLine($"    Is NextCloud: {_config.Conf.IsNextCloud}");
                Console.WriteLine($"    Show config on start: {_config.Conf.ShowConfigParametersOnStart}");
                Console.WriteLine($"    Wait on exit: {_config.Conf.WaitOnExit}");
                Console.WriteLine($"    Show file details: {_config.Conf.ShowFileDetails}");
                Console.WriteLine();
                Marker(Mark.Options); Console.WriteLine($"New files action");
                Console.WriteLine($"    App: {_config.Conf.FileActionApp}");
                Console.WriteLine($"    Options: {_config.Conf.FileActionAppOptions}");
                Console.WriteLine();
                Marker(Mark.Options); Console.WriteLine($"Affected folders action");
                Console.WriteLine($"    App: {_config.Conf.FolderActionApp}");
                Console.WriteLine($"    Options: {_config.Conf.FolderActionAppOptions}");
                Console.ResetColor();
            }
        }

        private static void GoAway()
        {
            if (!_config.Conf.WaitOnExit) return;

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
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("[-]");
                    break;
                case Mark.Affected:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("[A]");
                    break;
                case Mark.Scan:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("[>]");
                    break;
                case Mark.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[E]");
                    break;
                case Mark.Info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("[I]");
                    break;
                case Mark.Options:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("[#]");
                    break;
                default:
                    break;
            }
            Console.Write(" ");
            Console.ResetColor();
        }
    }
}