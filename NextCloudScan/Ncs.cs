﻿using Extensions;
using NextCloudScan.Activities;
using NextCloudScan.Lib;
using NextCloudScan.Lock;
using NextCloudScan.Parsers;
using NextCloudScan.Statistics.Lib;
using NextCloudScan.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NextCloudScan
{
    internal class Ncs
    {
        private const int IS_LOCKED = 403;
        private const int IS_FATAL_EXCEPTION = 404;
        private const int IS_LOCK_ERROR = 405;
        private const int IS_CONFIG_PATHS_IS_WRONG = 406;
        private const string LOG_FILE = "ncs.log";
        private const string LOG_DIR = "logs";
        private const string STATISTICS_FILE = "statistics.xml";
        private const string STATISTICS_DIR = "statistics";

        private static ConfigExtension<NcsConfig> _config;
        private static string _configFile;
        private static string _logfile;
        private static string _statisticsFile;
        private static FileDataBase _fdb;
        private static FileDataBaseOptions _fdbOptions;
        private static TimeSpan _scanTime;
        private static DateTime _startTime;
        private static ActionsResult _fileActionsResult;
        private static ActionsResult _folderActionsResult;
        private static Version _version;
        private static Dictionary<string, Version> _componentsVersions;
        private static int _notFatalErrors;
        private static int _replacedWithParents;
        private static int _removedAsSubfolders;

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

                if (status == ConfigExtension<NcsConfig>.LoadStatus.LoadedDefault)
                {
                    _interface = UIFactory.CreateUI(SupportedUI.Screen);
                    ShowComponentsVersions();
                    ShowDefaultConfigBanner();
                    return;
                }

                CheckRootFolder();

                _fdbOptions = new FileDataBaseOptions(_config.Conf.Path, _config.Conf.BasePath, reduceToParents: _config.Conf.ReduceToParents);
                _fdb = new FileDataBase(_fdbOptions);

                CreateServiceFolders();

                _startTime = DateTime.Now;
                _interface = UIFactory.CreateUI(_config.Conf.Interface, _logfile, _config.Conf.SingleLogFile, _config.Conf.LogFilesAgeLimit);

                ShowComponentsVersions();
                ShowConfigParameters();

                if (_config.Conf.OneProcessAtATime)
                {
                    SetLock();
                }

                Scan();
            }
            catch (Exception e)
            {
                ShowErrorAndExit(Message.Error, $"{e.Message} [in {e.TargetSite}]", IS_FATAL_EXCEPTION);
            }

            if (!_fdb.IsNewBase)
            {
                if (_config.Conf.ShowFileDetails)
                {
                    ShowFileDetails();
                }

                ShowFolderDetails();
                ShowErrors();

                LaunchFileActions();
                LaunchFolderAction();
            }

            ShowSummary();
            AggregateStatistics();

            if (_config.Conf.OneProcessAtATime)
            {
                RemoveLock();
            }
        }

        private static void Scan()
        {
            _interface.Show(Message.Start, "Start scan");
            DateTime start = DateTime.Now;

            _fdb.Refresh();

            if (_fdb.Errors.Count != 0) _notFatalErrors += _fdb.Errors.Count;

            DateTime stop = DateTime.Now;
            _scanTime = stop - start;

            _interface.Show(Message.Stop, _fdb.IsNewBase ? "Scan is complete. A new file database has been created" : "Scan is complete");
        }

        private static ActionsResult Actions(string action, string actionOptions, List<string> paths, bool isNextCloud = false)
        {
            ActionsResult result;

            if (isNextCloud)
            {
                Actions fa = new Actions(paths, action, actionOptions, new NcPathParser(), new List<string>() { _config.Conf.Path }, new Progress(_interface));
                result = fa.Run();
            }
            else
            {
                Actions fa = new Actions(paths, action, actionOptions, progress: new Progress(_interface));
                result = fa.Run();
            }

            return result;
        }

        private static void LaunchFolderAction()
        {
            if (string.IsNullOrEmpty(_config.Conf.FolderActionApp) || _fdb.AffectedFoldersCount == 0) return;

            if (_config.Conf.IsNextCloud)
            {
                _interface.Show(Message.Start, "Launch actions for each affected NextCloud folder");
                _folderActionsResult = Actions(_config.Conf.FolderActionApp, _config.Conf.FolderActionAppOptions, _fdb.AffectedFolders, isNextCloud: true);
                _notFatalErrors += _folderActionsResult.Failed;
            }
            else
            {
                _interface.Show(Message.Start, "Launch actions for each affected folder");
                _folderActionsResult = Actions(_config.Conf.FolderActionApp, _config.Conf.FolderActionAppOptions, _fdb.AffectedFolders);
                _notFatalErrors += _folderActionsResult.Failed;
            }
        }

        private static void LaunchFileActions()
        {
            if (string.IsNullOrEmpty(_config.Conf.FileActionApp) || _fdb.AddedPath.Count == 0) return;

            _interface.Show(Message.Start, "Launch actions for each new file");
            _fileActionsResult = Actions(_config.Conf.FileActionApp, _config.Conf.FileActionAppOptions, _fdb.AddedPath);
            _notFatalErrors += _fileActionsResult.Failed;
        }

        private static void CheckRootFolder()
        {
            if (!Directory.Exists(_config.Conf.Path))
            {
                ShowErrorAndExit(Message.Error, $"The root folder does not exist: {_config.Conf.Path}", IS_CONFIG_PATHS_IS_WRONG);
            }
        }

        private static void CreateServiceFolders()
        {
            string logs = Path.Combine(_fdbOptions.BasePath, LOG_DIR);
            if (!Directory.Exists(logs)) Directory.CreateDirectory(logs);
            _logfile = Path.Combine(logs, LOG_FILE);

            string statistics = Path.Combine(_fdbOptions.BasePath, STATISTICS_DIR);
            if (!Directory.Exists(statistics)) Directory.CreateDirectory(statistics);
            _statisticsFile = Path.Combine(statistics, STATISTICS_FILE);
        }

        private static void SetLock()
        {
            OneProcessLocker locker = new OneProcessLocker();
            LockResult result = locker.Lock(_config.Conf.LockLifeTime);

            switch (result.Result)
            {
                case LockResultType.AlreadyLocked:
                    ShowErrorAndExit(Message.Warning, $"There is already a running process, lock file is \"{locker.Lockfile}\"", IS_LOCKED);
                    break;
                case LockResultType.Successfull:
                    _interface.Show(Message.Info, "Work in single instance mode, the lock is set");
                    break;
                case LockResultType.DeleteOldLock:
                    _interface.Show(Message.Info, "Work in single instance mode, the lock is set (the outdated lock has been removed)");
                    break;
                case LockResultType.Error:
                    ShowErrorAndExit(Message.Error, $"Unable to create a lock file \"{locker.Lockfile}\", error: {result.ErrorMessage}", IS_LOCK_ERROR);
                    break;
                default:
                    break;
            }
        }

        private static void RemoveLock()
        {
            OneProcessLocker locker = new OneProcessLocker();
            LockResult result = locker.Unlock();

            switch (result.Result)
            {
                case LockResultType.AlreadyLocked:
                    break;
                case LockResultType.Successfull:
                    _interface.Show(Message.Info, "The lock was successfully removed");
                    break;
                case LockResultType.DeleteOldLock:
                    break;
                case LockResultType.Error:
                    ShowErrorAndExit(Message.Error, $"Сannot delete lock \"{locker.Lockfile}\", you must delete the file manually, error: {result.ErrorMessage}", IS_LOCK_ERROR);
                    break;
                default:
                    break;
            }
        }

        private static void AggregateStatistics()
        {
            SessionStatistics ss = new SessionStatistics()
            {
                Id = Guid.NewGuid(),
                StartTime = _startTime,
                AddedFiles = _fdb.Added.Count,
                RemovedFiles = _fdb.Removed.Count,
                AffectedFolders = _fdb.AffectedFolders.Count,
                TotalFiles = _fdb.Count,
                ScanElapsedTime = _scanTime.Ticks,
                FileProcessingElapsedTime = TimeSpan.FromSeconds(0).Ticks,
                FolderProcessingElapsedTime = TimeSpan.FromSeconds(0).Ticks,
                ProcessedFolders = _fdb.AffectedFolders,
                Errors = _notFatalErrors,
                ReplacedWithParents = _replacedWithParents,
                RemovedAsSubfolders = _removedAsSubfolders
            };

            if (_fileActionsResult != null) ss.FileProcessingElapsedTime = _fileActionsResult.ElapsedTime.Ticks;
            if (_folderActionsResult != null) ss.FolderProcessingElapsedTime = _folderActionsResult.ElapsedTime.Ticks;

            StatisticsAggregator aggregator = new StatisticsAggregator(_statisticsFile);

            bool successfullAppend = false;
            if (aggregator.CreateSuccessfully)
            {
                successfullAppend = aggregator.Append(ss);
            }

            if (successfullAppend) _interface.Show(Message.Info, "Statistics aggregated and saved");
            else _interface.Show(Message.Error, aggregator.ErrorMessage);
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
            _interface.Show(Message.None, string.Empty);

            if (_fdb.ChangeFoldersToParent)
            {
                _interface.Show(Message.Warning, $"{_fdb.FoldersReplacedWithParents.Count} folder(s) were replaced with parent folder(s) due to unavailability");
                _replacedWithParents = _fdb.FoldersReplacedWithParents.Count;

                foreach (var item in _fdb.FoldersReplacedWithParents)
                {
                    _interface.Show(Message.Warning, $"\"{item.Item1}\" change to \"{item.Item2}\"");
                }
            }

            if (_fdb.RemoveSubfolders)
            {
                _interface.Show(Message.Warning, $"{_fdb.FoldersRemovedAsSubfolders.Count} folder(s) have been removed because they are in subfolders");
                _removedAsSubfolders = _fdb.FoldersRemovedAsSubfolders.Count;

                foreach (var item in _fdb.FoldersRemovedAsSubfolders)
                {
                    _interface.Show(Message.Warning, $"\"{item.Item2}\" used instead \"{item.Item1}\"");
                }
            }

            if (_fdb.Removed.Count != 0)
                _interface.Show(Message.Info, $"Removed: {_fdb.Removed.Count}");
            if (_fdb.Added.Count != 0)
                _interface.Show(Message.Info, $"Added: {_fdb.Added.Count}");
            if (_fdb.AffectedFoldersCount != 0)
                _interface.Show(Message.Info, $"Affected folders: {_fdb.AffectedFoldersCount}");

            _interface.Show(Message.Info, $"Total in the database {_fdb.Count} files, scan elapsed time: {_scanTime.TotalSeconds:0.000} seconds");

            if (_fdb.Errors.Count != 0)
                _interface.Show(Message.Warning, $"({_fdb.Errors.Count} folder(s) unavailable during the last scan)");
            if (_fileActionsResult != null)
                _interface.Show(Message.Info, $"File actions result: {_fileActionsResult.Completed} ok, {_fileActionsResult.Failed} error, elapsed time: {_fileActionsResult.ElapsedTime.TotalSeconds:0.000} seconds");
            if (_folderActionsResult != null)
                _interface.Show(Message.Info, $"Folder action result: {_folderActionsResult.Completed} ok, {_folderActionsResult.Failed} error, elapsed time: {_folderActionsResult.ElapsedTime.TotalSeconds:0.000} seconds");
        }

        private static void ShowErrors()
        {
            if (_fdb.Errors.Count == 0) return;

            foreach (string error in _fdb.Errors)
            {
                _interface.Show(Message.Error, error);
            }
        }

        private static void ShowErrorAndExit(Message type, string message, int errorCode)
        {
            IHumanUI defaultInterface;

            if (_interface == null) defaultInterface = UIFactory.CreateUI(SupportedUI.Screen);
            else defaultInterface = _interface;

            defaultInterface.Show(Message.Info, $"Config file: {_configFile}");
            defaultInterface.Show(type, message);
            defaultInterface.Show(type, "Exited");
            Environment.Exit(errorCode);
        }

        private static void ShowDefaultConfigBanner()
        {
            _interface.Show(Message.Warning, $"The specified configuration file \"{_configFile}\" is missing");
            _interface.Show(Message.Warning, $"A new file was created with this name and the following default settings:");

            ShowConfigParameters();

            _interface.Show(Message.Warning, "Check the configuration file before next run!");
        }

        private static void ShowConfigParameters()
        {
            if (_config.Conf.ShowConfigParametersOnStart)
            {
                _interface.Show(Message.Config, $"Config file: {_configFile}");
                _interface.Show(Message.Config, $"Scaning path: {_config.Conf.Path}");

                if (_fdbOptions != null) _interface.Show(Message.Config, $"NCS data files path: {_fdbOptions.BasePath}");
                else _interface.Show(Message.Config, $"NCS data files path: {_config.Conf.BasePath}");
                _interface.Show(Message.Config, $"Statistics file: {_statisticsFile}");
                _interface.Show(Message.Config, $"Log file: {_logfile}");
                _interface.Show(Message.Config, $"Age limit for log files: {_config.Conf.LogFilesAgeLimit} hour(s)");
                _interface.Show(Message.Config, $"Interface: {_config.Conf.Interface}");
                _interface.Show(Message.Config, $"Interface module: {_interface}");
                _interface.Show(Message.Config, $"File App: {_config.Conf.FileActionApp}");
                _interface.Show(Message.Config, $"File App options: {_config.Conf.FileActionAppOptions}");
                _interface.Show(Message.Config, $"Folder App: {_config.Conf.FolderActionApp}");
                _interface.Show(Message.Config, $"Folder App options: {_config.Conf.FolderActionAppOptions}");
                _interface.Show(Message.Config, $"Is NextCloud: {_config.Conf.IsNextCloud}");
                _interface.Show(Message.Config, $"Reduce to parents: {_config.Conf.ReduceToParents}");
                _interface.Show(Message.Config, $"Single log file: {_config.Conf.SingleLogFile}");
                _interface.Show(Message.Config, $"One process at a time: {_config.Conf.OneProcessAtATime}");
                _interface.Show(Message.Config, $"One process at a time (lock lifetime): {_config.Conf.LockLifeTime} minute(s)");
                _interface.Show(Message.Config, $"Show config on start: {_config.Conf.ShowConfigParametersOnStart}");
                _interface.Show(Message.Config, $"Show file details: {_config.Conf.ShowFileDetails}");
            }
        }

        private static void ShowComponentsVersions()
        {
            _interface.Show(Message.None, string.Empty);
            _interface.Show(Message.None, $"NextClouScan started. Version {_version}");

            foreach (var componentVersion in _componentsVersions)
            {
                _interface.Show(Message.None, $"{componentVersion.Key}, version={componentVersion.Value}");
            }

            _interface.Show(Message.None, string.Empty);
        }
    }
}
