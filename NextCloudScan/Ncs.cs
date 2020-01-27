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
                ConfigExtension<NcsConfig>.LoadStatus status = _config.LoadConfig();

                if (status == ConfigExtension<NcsConfig>.LoadStatus.LoadedDefault)
                {
                    ShowDefaultConfigBanner();
                    return;
                }

                _interface = InterfaceFabrique.GetInterface(_config.Conf.Interface);

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
                    _interface.Show(MessageType.Scan, "Launch action for each new file");
                    _fileActionsResult = Actions(_config.Conf.FileActionApp, _config.Conf.FileActionAppOptions, _fdb.AddedPath);
                    ShowActionsErrors(_fileActionsResult);
                }

                if (!string.IsNullOrEmpty(_config.Conf.FolderActionApp))
                {
                    if (_config.Conf.IsNextCloud)
                    {
                        _interface.Show(MessageType.Scan, "Launch action for each affected NextCloud folder");
                        _folderActionsResult = Actions(_config.Conf.FolderActionApp, _config.Conf.FolderActionAppOptions, _fdb.AffectedFolders, isNextCloud: true);
                    }
                    else
                    {
                        _interface.Show(MessageType.Scan, "Launch action for each affected folder");
                        _folderActionsResult = Actions(_config.Conf.FolderActionApp, _config.Conf.FolderActionAppOptions, _fdb.AffectedFolders);
                    }
                    ShowActionsErrors(_folderActionsResult);
                }
            }

            ShowSummary();
        }

        private static void Scan()
        {
            _interface.Show(MessageType.Scan, "Start scan");

            DateTime start = DateTime.Now;

            _fdb = new FileDataBase(_config.Conf.Path, _config.Conf.BaseFile, _config.Conf.DiffFile, _config.Conf.AffectedFoldersFile);

            DateTime stop = DateTime.Now;
            _scanTime = stop - start;

            _interface.Show(MessageType.Scan, "Scan complete");
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
                _interface.Show(MessageType.Remove, item.ToString());
            }

            foreach (FileItem item in _fdb.Added)
            {
                _interface.Show(MessageType.Add, item.ToString());
            }
        }

        private static void ShowFolderDetails()
        {
            foreach (string path in _fdb.AffectedFolders)
            {
                _interface.Show(MessageType.Affected, path);
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
                _interface.Show(MessageType.Info, $"({_fdb.Errors.Count} folders unavailable during the last scan)");
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
            _interface.Show(MessageType.Info, $"The specified configuration file \"{_configFile}\" is missing.");
            _interface.Show(MessageType.Info, $"A new file was created with this name and the following default settings:");

            ShowConfigParameters();
            _interface.Show(MessageType.Info, "[!] Check the configuration file before next run!");
        }

        private static void ShowConfigParameters()
        {
            if (_config.Conf.ShowConfigParametersOnStart)
            {
                _interface.Show(MessageType.Info, $"Config:");
                _interface.Show(MessageType.Options, $"    Config file: {_configFile}");
                _interface.Show(MessageType.Options, $"    Path: {_config.Conf.Path}");
                _interface.Show(MessageType.Options, $"    Base file: {_config.Conf.BaseFile}");
                _interface.Show(MessageType.Options, $"    Diff file: {_config.Conf.DiffFile}");
                _interface.Show(MessageType.Options, $"    Affected folders file: {_config.Conf.AffectedFoldersFile}");
                _interface.Show(MessageType.Options, $"    Is NextCloud: {_config.Conf.IsNextCloud}");
                _interface.Show(MessageType.Options, $"    Show config on start: {_config.Conf.ShowConfigParametersOnStart}");
                _interface.Show(MessageType.Options, $"    Wait on exit: {_config.Conf.WaitOnExit}");
                _interface.Show(MessageType.Options, $"    Show file details: {_config.Conf.ShowFileDetails}");
                _interface.Show(MessageType.Options, $"    Interface: {_config.Conf.Interface}");
                _interface.Show(MessageType.Options, $"    Interface module: {_interface}");
                _interface.Show(MessageType.Options, $"    App: {_config.Conf.FileActionApp}");
                _interface.Show(MessageType.Options, $"    Options: {_config.Conf.FileActionAppOptions}");
                _interface.Show(MessageType.Options, $"    App: {_config.Conf.FolderActionApp}");
                _interface.Show(MessageType.Options, $"    Options: {_config.Conf.FolderActionAppOptions}");
            }
        }
    }
}