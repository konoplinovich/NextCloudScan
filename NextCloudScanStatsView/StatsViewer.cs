using CommandLine;
using CommandLine.Text;
using NextCloudScan.Statistics.Lib;
using NextCloudScanStatsView.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NextCloudScanStatsView
{
    internal static class StatsViewer
    {
        private static Summary _summary = new Summary();
        private static Options _options = new Options();
        private static List<Session> _sessions = new List<Session>();
        private static List<SessionStatistics> _sessionsStatistics = new List<SessionStatistics>();

        private static Version _version;
        private static Dictionary<string, Version> _componentsVersions;

        static void Main(string[] args)
        {
            if (args.Length == 0) return;

            ShowStartUpBanner();
            ParseCommandLine(args);

            string ext = Path.GetExtension(_options.StatsFile);
            string name = Path.GetFileNameWithoutExtension(_options.StatsFile);
            string path = Path.GetDirectoryName(_options.StatsFile);

            string[] files = Directory.GetFiles(path, $"{name}*{ext}");

            foreach (string file in files)
            {
                Console.Write($"Load file: {file} ");
                StatisticsAggregator aggregator = new StatisticsAggregator(file);
                if (!aggregator.Successfully)
                {
                    Console.WriteLine(aggregator.ErrorMessage);
                    return;
                }

                Console.WriteLine($"--> {aggregator.Statistics.Count} sessions / {aggregator.Size:0.0} Kb in {aggregator.LoadTime.TotalSeconds:0.000} seconds");

                _sessionsStatistics.AddRange(aggregator.Statistics);
                _summary.TotalFilesSize += aggregator.Size;
                _summary.TotalLoadTime += aggregator.LoadTime;
            }

            _sessionsStatistics = new List<SessionStatistics>(_sessionsStatistics.OrderBy(x => x.StartTime));

            ParseStatistics();
            SessionFilters filter = SelectFilter();

            Console.WriteLine($"Loaded {_sessions.Count} sessions from {files.Length} files ({_summary.TotalFilesSize:0.00} Kb in {_summary.TotalLoadTime.TotalSeconds:0.000} seconds)");

            switch (filter)
            {
                case SessionFilters.SummaryOnly:
                    ShowSummary();
                    break;
                case SessionFilters.AllSessions:
                    AllSessionsFilter();
                    ShowSummary();
                    break;
                case SessionFilters.WorkingOnly:
                    WorkingOnlyFilter();
                    ShowSummary();
                    break;
                case SessionFilters.LastNSessions:
                    LastNSessionsFilter();
                    ShowSummary();
                    break;
                case SessionFilters.LastNWorkingSessions:
                    LastNWorkingSessionsFilter();
                    ShowSummary();
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(_options.CsvFile))
            {
                ExportCsv(_options.CsvFile);
            }
        }

        private static void ParseCommandLine(string[] args)
        {
            var parser = new Parser(with => with.HelpWriter = null);
            var parserResult = parser.ParseArguments<ParsedOptions>(args);
            parserResult.WithParsed<ParsedOptions>(options => RunOptions(options)).WithNotParsed(errs => DisplayHelp(parserResult, errs));
        }

        private static void AllSessionsFilter()
        {
            Console.WriteLine();
            ShowSessions(_sessions);
            Console.WriteLine($"all {_sessions.Count} sessions are shown (for the last {ToReadableString(_sessions.Period())})");
        }

        private static void WorkingOnlyFilter()
        {
            var working = _sessions
                .Where(s => s.IsWorking)
                .Select(s => s)
                .ToList<Session>();
            Console.WriteLine();
            ShowSessions(working);
            Console.WriteLine($"all {working.Count} working sessions out of {_sessions.Count} are shown (for the last {ToReadableString(_sessions.Period())})");
        }

        private static void LastNSessionsFilter()
        {
            var lastN = _sessions
                .Skip(Math.Max(0, _summary.TotalSessionsCount - _options.Lines))
                .ToList<Session>();
            Console.WriteLine();
            ShowSessions(lastN);
            Console.WriteLine($"{lastN.Count} last sessions are shown (for the last {ToReadableString(lastN.Period())})");
        }

        private static void LastNWorkingSessionsFilter()
        {
            var lastNWorking = _sessions
                .Skip(Math.Max(0, _summary.TotalSessionsCount - _options.Lines))
                .Where(s => s.IsWorking)
                .Select(s => s)
                .ToList<Session>();
            var LastNAll = _sessions
                .Skip(Math.Max(0, _summary.TotalSessionsCount - _options.Lines))
                .ToList<Session>();
            Console.WriteLine();
            ShowSessions(lastNWorking);
            Console.WriteLine($"{lastNWorking.Count} working sessions from the last {_options.Lines} are shown (for the last {ToReadableString(LastNAll.Period())})");
        }

        private static SessionFilters SelectFilter()
        {
            SessionFilters filter;
            if (_options.SummaryOnly) filter = SessionFilters.SummaryOnly;
            else if (_options.OnlyWorking && _options.Lines < _sessions.Count && _options.Lines != 0) filter = SessionFilters.LastNWorkingSessions;
            else if (_options.OnlyWorking) filter = SessionFilters.WorkingOnly;
            else if (_options.ShowAll || _options.Lines >= _sessions.Count) filter = SessionFilters.AllSessions;
            else filter = SessionFilters.LastNSessions;
            return filter;
        }

        private static void ParseStatistics()
        {
            foreach (SessionStatistics stat in _sessionsStatistics)
            {
                AddValuesToSummary(stat);

                Session session = new Session(++_summary.TotalSessionsCount, stat);
                _sessions.Add(session);

                if (session.IsWorking) _summary.NotZeroSessions += 1;
            }
        }

        private static void AddValuesToSummary(SessionStatistics stat)
        {
            _summary.Added += stat.AddedFiles;
            _summary.Removed += stat.RemovedFiles;
            _summary.Affected += stat.AffectedFolders;
            _summary.ScanTime += stat.ScanElapsedTime;
            _summary.FileScanTime += stat.FileProcessingElapsedTime;
            _summary.FolderScanTime += stat.FolderProcessingElapsedTime;
        }

        private static void ShowSessions(List<Session> sessions)
        {
            Table table = CreateTable();

            table.Header();

            for (int currentSession = 0; currentSession < sessions.Count; currentSession++)
            {
                Session session = sessions[currentSession];
                SessionStatistics statistics = session.Statistics;
                string IsWorkingMarker = session.IsWorking ? "*" : "";

                Tuple<bool, string> result = SearchLog(statistics);
                string logFileName = result.Item1 ? result.Item2 : string.Empty;

                List<string> values = new List<string>()
                {
                    $"{session.Number}",
                    $"{statistics.Id}",
                    $"{IsWorkingMarker}",
                    $"{statistics.StartTime.ToString("dd-MM-yyyy HH:mm:ss")}",
                    $"{statistics.TotalFiles}",
                    $"{session.ScanElapsedTime.TotalSeconds:0.0000}",
                    $"{statistics.AddedFiles}",
                    $"{statistics.RemovedFiles}",
                    $"{statistics.AffectedFolders}",
                    $"{session.FileProcessingElapsedTime.TotalSeconds:0.0000}",
                    $"{session.FolderProcessingElapsedTime.TotalSeconds:0.0000}",
                    $"{session.WorkTime.TotalSeconds:0.0000}",
                    $"{statistics.Errors}",
                    $"{statistics.ReplacedWithParents}",
                    $"{statistics.RemovedAsSubfolders}",
                    $"{logFileName}"
                };

                table.AddRow(values);

                if (_options.ShowFolders && statistics.ProcessedFolders.Count != 0)
                {
                    for (int currentFolder = 0; currentFolder < statistics.ProcessedFolders.Count; currentFolder++)
                    {
                        string folder = statistics.ProcessedFolders[currentFolder];

                        table.AddRow(new List<string>() { "", $"  [{(currentFolder + 1)}] {folder}" },
                            _options.LastColumnSpan,
                            currentFolder >= statistics.ProcessedFolders.Count - 1,
                            currentSession >= sessions.Count - 1);
                    }
                }
            }

            table.CloseTable();
        }

        private static Table CreateTable()
        {
            return new Table(new List<Column>
            {
                new Column(7,"#", Alignment.Left),
                new Column(38,"Id", Alignment.Left),
                new Column(1,"W", Alignment.Left),
                new Column(21,"Start Time", Alignment.Left),
                new Column(8,"Total", Alignment.Left),
                new Column(10,"Scan", Alignment.Left),
                new Column(8,"[+]", Alignment.Left),
                new Column(8,"[-]", Alignment.Left),
                new Column(8,"[A]", Alignment.Left),
                new Column(10,"Files", Alignment.Left),
                new Column(10,"Folders", Alignment.Left),
                new Column(10,"Work time", Alignment.Left),
                new Column(3,"[E]", Alignment.Left),
                new Column(4,"[P]", Alignment.Left),
                new Column(4,"[S]", Alignment.Left),
                new Column(25,"Log", Alignment.Left)
            });
        }

        private static Tuple<bool, string> SearchLog(SessionStatistics statistics)
        {
            if (string.IsNullOrEmpty(_options.LogsPath)) return new Tuple<bool, string>(false, string.Empty);

            string pattern = statistics.StartTime.ToString("_ddMMyyyy_HHmmss");
            pattern = pattern.Remove(pattern.Length - 1);
            string[] files = Directory.GetFiles(_options.LogsPath);

            foreach (string file in files)
            {
                if (file.IndexOf(pattern) != -1) return new Tuple<bool, string>(true, Path.GetFileName(file));
            }

            return new Tuple<bool, string>(false, string.Empty);
        }

        private static void ShowSummary()
        {
            DateTime first = _sessionsStatistics[0].StartTime;
            DateTime last = _sessionsStatistics[_summary.TotalSessionsCount - 1].StartTime;
            TimeSpan period = last - first;
            TimeSpan workTime = TimeSpan.FromTicks(_summary.ScanTime + _summary.FileScanTime + _summary.FolderScanTime);

            double hoursAll = period.TotalHours;
            double hoursWork = workTime.TotalHours;
            double ratio = hoursWork / hoursAll;
            double averageInterval = period.TotalMinutes / _summary.TotalSessionsCount;
            double averageRealInterval = period.TotalMinutes / _summary.NotZeroSessions;
            double workSessionPercent = (double)_summary.NotZeroSessions * 100 / _summary.TotalSessionsCount;
            double averagefoldersPerDay = (double)_summary.Affected / period.TotalDays;
            double scanPercentFromPeriod = TimeSpan.FromTicks(_summary.ScanTime).TotalSeconds * 100 / period.TotalSeconds;
            double filePercentFromPeriod = TimeSpan.FromTicks(_summary.FileScanTime).TotalSeconds * 100 / period.TotalSeconds;
            double folderPercentFromPeriod = TimeSpan.FromTicks(_summary.FolderScanTime).TotalSeconds * 100 / period.TotalSeconds;
            double workPercentFromPeriod = workTime.TotalSeconds * 100 / period.TotalSeconds;
            double sessionPerDay = _summary.TotalSessionsCount / period.TotalDays;
            double workingSessionPerDay = _summary.NotZeroSessions / period.TotalDays;

            Console.WriteLine();
            Console.WriteLine($"Statistics period:        {ToReadableString(period)}");
            Console.WriteLine($"First session start:      {_sessionsStatistics[0].StartTime.ToString("dd-MM-yyyy HH:mm:ss")}");
            Console.WriteLine($"Last session start:       {_sessionsStatistics[_summary.TotalSessionsCount - 1].StartTime.ToString("dd-MM-yyyy HH:mm:ss")}");
            Console.WriteLine($"Sessions count:           {_summary.TotalSessionsCount} (~{averageInterval:0.0} min interval), ~{sessionPerDay:0} session/day");
            Console.WriteLine($"Sessions count (w/work):  {_summary.NotZeroSessions} (~{averageRealInterval:0.0} min interval), ~{workingSessionPerDay:0} session/day, {workSessionPercent:0}% of all sessions");
            Console.WriteLine();
            Console.WriteLine($"Files added/removed:      {_summary.Added}/{_summary.Removed}");
            Console.WriteLine($"Processed folders:        {_summary.Affected} (~{averagefoldersPerDay:0} folders/day)");
            Console.WriteLine();
            Console.WriteLine($"Scan time:                {ToReadableString(TimeSpan.FromTicks(_summary.ScanTime))} ({scanPercentFromPeriod:0.0}%)");
            Console.WriteLine($"File processing time:     {ToReadableString(TimeSpan.FromTicks(_summary.FileScanTime))} ({filePercentFromPeriod:0.0}%)");
            Console.WriteLine($"Folder processing time:   {ToReadableString(TimeSpan.FromTicks(_summary.FolderScanTime))} ({folderPercentFromPeriod:0.0}%)");
            Console.WriteLine($"Total work time:          {ToReadableString(workTime)} ({workPercentFromPeriod:0.0}%)");
            Console.WriteLine($"Ratio (work/period):      {ratio:0.0000}");
            Console.WriteLine("---");
            Console.WriteLine($"Statistics file(s) size:  {_summary.TotalFilesSize:0.00} Kb (~{(_summary.TotalFilesSize / period.TotalDays):0.00} Kb/day)");
            Console.WriteLine();
        }

        private static void ExportCsv(string csvFile)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"Number,Start Time,Total Files (pc.),Scan Elapsed Time (s),Added Files (pc.),Removed Files (pc.),Affected Folders (pc.),File Processing (s),Folder Processing (s)");
            sb.Append(Environment.NewLine);

            for (int i = 0; i < _sessionsStatistics.Count; i++)
            {
                SessionStatistics stat = _sessionsStatistics[i];
                sb.Append($"{i + 1}," +
                    $"{stat.StartTime.ToString("dd-MM-yyyy HH:mm:ss")}," +
                    $"{stat.TotalFiles}," +
                    $"{TimeSpan.FromTicks(stat.ScanElapsedTime).TotalSeconds}," +
                    $"{stat.AddedFiles}," +
                    $"{stat.RemovedFiles}," +
                    $"{stat.AffectedFolders}," +
                    $"{TimeSpan.FromTicks(stat.FileProcessingElapsedTime).TotalSeconds}," +
                    $"{TimeSpan.FromTicks(stat.FolderProcessingElapsedTime).TotalSeconds}");
                sb.Append(Environment.NewLine);
            }

            File.WriteAllText(csvFile, sb.ToString());
            Console.WriteLine($"CSV File exported: {csvFile}");
        }

        private static void DisplayHelp(ParserResult<ParsedOptions> parserResult, IEnumerable<Error> errs)
        {
            AssemblyName assembly = Assembly.GetExecutingAssembly().GetName();

            HelpText helpText = null;
            if (errs.IsVersion())
            {
                helpText = HelpText.AutoBuild(parserResult);
                Console.WriteLine();
                Console.WriteLine(helpText);
            }
            else
            {
                helpText = HelpText.AutoBuild(parserResult, h =>
                {
                    h.AdditionalNewLineAfterOption = false;
                    h.Heading = $"NextCloudScan Statistics Viewer {assembly.Version.ToString()}";
                    h.Copyright = string.Empty;
                    return HelpText.DefaultParsingErrorsHandler(parserResult, h);
                }, e => e);
                Console.WriteLine($"{Environment.NewLine}{helpText}");
            }

            Environment.Exit(102);
        }

        private static void RunOptions(ParsedOptions options)
        {
            StatsViewer._options.StatsFile = options.InputFile;
            StatsViewer._options.Lines = options.Lines;
            StatsViewer._options.SummaryOnly = options.SummaryOnly;
            StatsViewer._options.CsvFile = options.Export;
            StatsViewer._options.OnlyWorking = options.OnlyWorking;
            StatsViewer._options.ShowFolders = options.ShowFolders;
            StatsViewer._options.LogsPath = options.LogsPath;

            if (StatsViewer._options.Lines == 0) StatsViewer._options.ShowAll = true;
        }

        private static void ShowStartUpBanner()
        {
            GetVersions();

            Console.WriteLine();
            Console.WriteLine($"NextCloudScan statistics viewer started. Version {_version}");

            foreach (var componentVersion in _componentsVersions)
            {
                Console.WriteLine($"{componentVersion.Key}, version={componentVersion.Value}");
            }

            Console.WriteLine();
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

        private static string ToReadableString(TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? string.Empty : "s") : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

            return formatted;
        }

        private static TimeSpan Period(this List<Session> sessions)
        {
            if (sessions == null || sessions.Count == 0) return TimeSpan.Zero;

            DateTime start = sessions[0].StartTime;
            DateTime end = sessions[sessions.Count - 1].StartTime;

            return end - start;
        }
    }
}