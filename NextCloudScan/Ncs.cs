using Extensions;
using NextCloudScan.Lib;
using NextCloudScan.Parsers;
using NextCloudScan.UI;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NextCloudScan
{
    internal class Ncs
    {
        private static ConfigExtension<NcsConfig> _config;
        private static string _configFile;

        private static FileDataBase _fdb;
        private static TimeSpan _scanTime;
        private static ActionsResult _fileActionsResult;
        private static ActionsResult _folderActionsResult;
        private static Version _version;
        private static Dictionary<string, Version> _componentsVersions;

        private static IHumanUI _interface;

        static void Main(string[] args)
        {
            if (args.Length == 0) return;
            _configFile = args[0];

            GetVersions();

            try
            {
                _config = new ConfigExtension<NcsConfig>(_configFile);
                ConfigExtension<NcsConfig>.LoadStatus status = _config.LoadConfig();

                _interface = UIFactory.CreateUI(_config.Conf.Interface, _config.Conf.LogFile);
                ShowStartUpBanner();

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
                ShowFatalException(e.Message);
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
                    _interface.Show(Message.Start, "Launch action for each new file");
                    _fileActionsResult = Actions(_config.Conf.FileActionApp, _config.Conf.FileActionAppOptions, _fdb.AddedPath);

                    ShowExternaMessages(_fileActionsResult.Log);
                    ShowActionsErrors(_fileActionsResult.Errors);
                }

                if (!string.IsNullOrEmpty(_config.Conf.FolderActionApp))
                {
                    if (_config.Conf.IsNextCloud)
                    {
                        _interface.Show(Message.Start, "Launch action for each affected NextCloud folder");
                        _folderActionsResult = Actions(_config.Conf.FolderActionApp, _config.Conf.FolderActionAppOptions, _fdb.AffectedFolders, isNextCloud: true);
                    }
                    else
                    {
                        _interface.Show(Message.Start, "Launch action for each affected folder");
                        _folderActionsResult = Actions(_config.Conf.FolderActionApp, _config.Conf.FolderActionAppOptions, _fdb.AffectedFolders);
                    }

                    ShowExternaMessages(_folderActionsResult.Log);
                    ShowActionsErrors(_folderActionsResult.Errors);
                }
            }

            ShowSummary();
        }

        private static void ShowExternaMessages(Dictionary<string, List<string>> messages)
        {
            if (messages == null) return;
            if (messages.Values.Count == 0) return;

            foreach (var item in messages.Keys)
            {
                List<string> lines = messages[item];
                _interface.Show(Message.Info, $"Item: {item}");

                foreach (string logLine in lines)
                {
                    string[] separatelines = logLine.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string line in separatelines)
                    {
                        _interface.Show(Message.External, $"{line}");
                    }
                }
            }
        }

        private static void Scan()
        {
            _interface.Show(Message.Start, "Start scan");

            DateTime start = DateTime.Now;

            _fdb = new FileDataBase(_config.Conf.Path, _config.Conf.BaseFile, _config.Conf.DiffFile, _config.Conf.AffectedFoldersFile);

            DateTime stop = DateTime.Now;
            _scanTime = stop - start;

            _interface.Show(Message.Start, "Scan complete");
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
                _interface.Show(Message.RemovedFile, item.ToString());
            }

            foreach (FileItem item in _fdb.Added)
            {
                _interface.Show(Message.NewFile, item.ToString());
            }
        }

        private static void ShowFolderDetails()
        {
            foreach (string path in _fdb.AffectedFolders)
            {
                _interface.Show(Message.AffectedFolder, path);
            }
        }

        private static void ShowSummary()
        {
            _interface.Show(Message.None, "");

            if (_fdb.Removed.Count != 0) _interface.Show(Message.Info, $"Removed: {_fdb.Removed.Count}");
            if (_fdb.Added.Count != 0) _interface.Show(Message.Info, $"Added: {_fdb.Added.Count}");
            if (_fdb.AffectedFoldersCount != 0) _interface.Show(Message.Info, $"Affected folders: {_fdb.AffectedFoldersCount}");
            _interface.Show(Message.Info, $"Total in the database {_fdb.Count} files, scan elapsed time: {_scanTime.TotalSeconds}");
            if (_fdb.Errors.Count != 0)
            {
                _interface.Show(Message.Warning, $"({_fdb.Errors.Count} folders unavailable during the last scan)");
            }
            if (_fileActionsResult != null)
                _interface.Show(Message.Info, $"File actions result: {_fileActionsResult.Completed} ok, {_fileActionsResult.Errors.Count} error, elapsed time: {_fileActionsResult.ElapsedTime}");
            if (_folderActionsResult != null)
                _interface.Show(Message.Info, $"Folder action result: {_folderActionsResult.Completed} ok, {_folderActionsResult.Errors.Count} error, elapsed time: {_folderActionsResult.ElapsedTime}");
        }

        private static void ShowErrors()
        {
            if (_fdb.Errors.Count == 0) return;

            foreach (string error in _fdb.Errors)
            {
                _interface.Show(Message.Error, error);
            }
        }

        private static void ShowActionsErrors(Dictionary<string, List<string>> errors)
        {
            if (errors == null) return;

            foreach (var item in errors.Keys)
            {
                List<string> currentErrors = errors[item];
                _interface.Show(Message.Error, $"Item processed with error: {item}");

                foreach (string error in currentErrors)
                {
                    _interface.Show(Message.Error, $"{error}");
                }
            }
        }

        private static void ShowFatalException(string message)
        {
            IHumanUI defaultInterface = UIFactory.CreateUI(UI.SupportedUI.Screen);

            defaultInterface.Show(Message.Info, $"Config file: {_configFile}");
            defaultInterface.Show(Message.Error, message);
            defaultInterface.Show(Message.Error, "Exited");
        }

        private static void ShowDefaultConfigBanner()
        {
            _interface.Show(Message.Warning, $"The specified configuration file \"{_configFile}\" is missing.");
            _interface.Show(Message.Warning, $"A new file was created with this name and the following default settings:");

            ShowConfigParameters();
            _interface.Show(Message.Warning, "[!] Check the configuration file before next run!");
        }

        private static void ShowConfigParameters()
        {
            if (_config.Conf.ShowConfigParametersOnStart)
            {
                _interface.Show(Message.Config, $"Config file: {_configFile}");
                _interface.Show(Message.Config, $"Path: {_config.Conf.Path}");
                _interface.Show(Message.Config, $"Base file: {_config.Conf.BaseFile}");
                _interface.Show(Message.Config, $"Diff file: {_config.Conf.DiffFile}");
                _interface.Show(Message.Config, $"Affected folders file: {_config.Conf.AffectedFoldersFile}");
                _interface.Show(Message.Config, $"LogFile: {_config.Conf.LogFile}");
                _interface.Show(Message.Config, $"Interface: {_config.Conf.Interface}");
                _interface.Show(Message.Config, $"Interface module: {_interface}");
                _interface.Show(Message.Config, $"File App: {_config.Conf.FileActionApp}");
                _interface.Show(Message.Config, $"File App options: {_config.Conf.FileActionAppOptions}");
                _interface.Show(Message.Config, $"Folder App: {_config.Conf.FolderActionApp}");
                _interface.Show(Message.Config, $"Folder App options: {_config.Conf.FolderActionAppOptions}");
                _interface.Show(Message.Config, $"Is NextCloud: {_config.Conf.IsNextCloud}");
                _interface.Show(Message.Config, $"Show config on start: {_config.Conf.ShowConfigParametersOnStart}");
                _interface.Show(Message.Config, $"Wait on exit: {_config.Conf.WaitOnExit}");
                _interface.Show(Message.Config, $"Show file details: {_config.Conf.ShowFileDetails}");
            }
        }

        private static void ShowStartUpBanner()
        {
            _interface.Show(Message.None, $"");
            _interface.Show(Message.None, $"NextClouScan started. Version {_version}");

            foreach (var componentVersion in _componentsVersions)
            {
                _interface.Show(Message.None, $"{componentVersion.Key}, version={componentVersion.Value}");
            }
            
            _interface.Show(Message.None, $"");
        }
        
        private static void GetVersions()
        {
            AssemblyName anBase = Assembly.GetExecutingAssembly().GetName();

            _version = anBase.Version;
            _componentsVersions = new Dictionary<string, Version>();

            foreach (AssemblyName an in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
            {
                _componentsVersions[an.Name] = an.Version;
            }
        }
    }
}