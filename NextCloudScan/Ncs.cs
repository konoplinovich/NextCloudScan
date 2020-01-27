using Extensions;
using FileScanLib;
using System;
using System.Collections.Generic;
using NextCloudScan.Interfaces;

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

        private static IHumanInterface _interface;

        static void Main(string[] args)
        {
            if (args.Length == 0) return;
            _configFile = args[0];

            try
            {
                _config = new ConfigExtension<NcsConfig>(_configFile);
                _interface = InterfaceFabrique.GetInterface(_config.Conf.Interface);

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
                    ShowFileDetails();
                }

                ShowFolderDetails();
                ShowErrors();

                if (!string.IsNullOrEmpty(_config.Conf.FileActionApp))
                {
                    _interface.Show(MessageType.Start, "Launch action for each new file");
                    _fileActionsResult = Actions(_config.Conf.FileActionApp, _config.Conf.FileActionAppOptions, _fdb.AddedPath);

                    foreach (string logLine in _fileActionsResult.Log)
                    {
                        _interface.Show(MessageType.External, $"{logLine.Replace(Environment.NewLine,"")}");
                    }

                    ShowActionsErrors(_fileActionsResult);
                }

                if (!string.IsNullOrEmpty(_config.Conf.FolderActionApp))
                {
                    if (_config.Conf.IsNextCloud)
                    {
                        _interface.Show(MessageType.Start, "Launch action for each affected NextCloud folder");
                        _folderActionsResult = Actions(_config.Conf.FolderActionApp, _config.Conf.FolderActionAppOptions, _fdb.AffectedFolders, isNextCloud: true);
                    }
                    else
                    {
                        _interface.Show(MessageType.Start, "Launch action for each affected folder");
                        _folderActionsResult = Actions(_config.Conf.FolderActionApp, _config.Conf.FolderActionAppOptions, _fdb.AffectedFolders);
                    }
                    ShowActionsErrors(_folderActionsResult);
                }
            }

            ShowSummary();
        }

        private static void Scan()
        {
            _interface.Show(MessageType.Start, "Start scan");

            DateTime start = DateTime.Now;

            _fdb = new FileDataBase(_config.Conf.Path, _config.Conf.BaseFile, _config.Conf.DiffFile, _config.Conf.AffectedFoldersFile);

            DateTime stop = DateTime.Now;
            _scanTime = stop - start;

            _interface.Show(MessageType.Start, "Scan complete");
        }

        private static ActionsResult Actions(string action, string actionOptions, List<string> paths, bool isNextCloud = false)
        {
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
                _interface.Show(MessageType.RemovedFile, item.ToString());
            }

            foreach (FileItem item in _fdb.Added)
            {
                _interface.Show(MessageType.NewFile, item.ToString());
            }
        }

        private static void ShowFolderDetails()
        {
            foreach (string path in _fdb.AffectedFolders)
            {
                _interface.Show(MessageType.AffectedFolder, path);
            }
        }

        private static void ShowSummary()
        {
            _interface.Show(MessageType.Info, "---");

            if (_fdb.Removed.Count != 0) _interface.Show(MessageType.Info, $"Removed: {_fdb.Removed.Count}");
            if (_fdb.Added.Count != 0) _interface.Show(MessageType.Info, $"Added: {_fdb.Added.Count}");
            if (_fdb.AffectedFoldersCount != 0) _interface.Show(MessageType.Info, $"Affected folders: {_fdb.AffectedFoldersCount}");
            _interface.Show(MessageType.Info, $"Total in the database {_fdb.Count} files, scan elapsed time: {_scanTime.TotalSeconds}");
            if (_fdb.Errors.Count != 0)
            {
                _interface.Show(MessageType.Warning, $"({_fdb.Errors.Count} folders unavailable during the last scan)");
            }
            if (_fileActionsResult != null)
                _interface.Show(MessageType.Info, $"File actions result: {_fileActionsResult.Completed} ok, {_fileActionsResult.Errors.Count} error, elapsed time: {_fileActionsResult.ElapsedTime}");
            if (_folderActionsResult != null)
                _interface.Show(MessageType.Info, $"Folder action result: {_folderActionsResult.Completed} ok, {_folderActionsResult.Errors.Count} error, elapsed time: {_folderActionsResult.ElapsedTime}");
        }

        private static void ShowErrors()
        {
            if (_fdb.Errors.Count == 0) return;

            foreach (string error in _fdb.Errors)
            {
                _interface.Show(MessageType.Error, error);
            }
        }

        private static void ShowActionsErrors(ActionsResult result)
        {
            if (result == null) return;
            if (result.Errors.Count == 0) return;

            foreach (string error in result.Errors)
            {
                _interface.Show(MessageType.Error, error);
            }
        }

        private static void ShowFatalException(Exception e)
        {
            _interface.Show(MessageType.Info, $"Config file: {_configFile}");
            _interface.Show(MessageType.Error, e.Message);
            _interface.Show(MessageType.Error, "Exited");
        }

        private static void ShowDefaultConfigBanner()
        {
            _interface.Show(MessageType.Warning, $"The specified configuration file \"{_configFile}\" is missing.");
            _interface.Show(MessageType.Warning, $"A new file was created with this name and the following default settings:");

            ShowConfigParameters();
            _interface.Show(MessageType.Warning, "[!] Check the configuration file before next run!");
        }

        private static void ShowConfigParameters()
        {
            if (_config.Conf.ShowConfigParametersOnStart)
            {
                _interface.Show(MessageType.Config, $"Config file: {_configFile}");
                _interface.Show(MessageType.Config, $"Path: {_config.Conf.Path}");
                _interface.Show(MessageType.Config, $"Base file: {_config.Conf.BaseFile}");
                _interface.Show(MessageType.Config, $"Diff file: {_config.Conf.DiffFile}");
                _interface.Show(MessageType.Config, $"Affected folders file: {_config.Conf.AffectedFoldersFile}");
                _interface.Show(MessageType.Config, $"LogFile: {_config.Conf.LogFile}");
                _interface.Show(MessageType.Config, $"Interface: {_config.Conf.Interface}");
                _interface.Show(MessageType.Config, $"Interface module: {_interface}");
                _interface.Show(MessageType.Config, $"File App: {_config.Conf.FileActionApp}");
                _interface.Show(MessageType.Config, $"File App options: {_config.Conf.FileActionAppOptions}");
                _interface.Show(MessageType.Config, $"Folder App: {_config.Conf.FolderActionApp}");
                _interface.Show(MessageType.Config, $"Folder App options: {_config.Conf.FolderActionAppOptions}");
                _interface.Show(MessageType.Config, $"Is NextCloud: {_config.Conf.IsNextCloud}");
                _interface.Show(MessageType.Config, $"Show config on start: {_config.Conf.ShowConfigParametersOnStart}");
                _interface.Show(MessageType.Config, $"Wait on exit: {_config.Conf.WaitOnExit}");
                _interface.Show(MessageType.Config, $"Show file details: {_config.Conf.ShowFileDetails}");
            }
        }
    }
}