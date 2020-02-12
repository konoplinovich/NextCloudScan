using CommandLine;
using CommandLine.Text;
using NextCloudScan.Statistics.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NextCloudScanStatsView.Interface;

namespace NextCloudScanStatsView
{
    partial class StatsViewer
    {
        private static int LAST_COLUMN_SPAN = 11;

        private static int _lines;
        private static string _statsFile;
        private static string _csvFile;
        private static bool _onlyWorking;
        private static bool _showFolders;
        private static string _logsPath;
        private static bool _summaryOnly;
        private static bool _showAll = false;

        private static long _added = 0;
        private static long _removed = 0;
        private static long _affected = 0;
        private static long _scanTime = 0;
        private static long _fileScanTime = 0;
        private static long _folderScanTime = 0;
        private static int _notZeroSessions = 0;

        private static List<Session> _sessions = new List<Session>();
        private static Table _sessionTable;

        static void Main(string[] args)
        {
            if (args.Length == 0) return;

            var parser = new Parser(with => with.HelpWriter = null);
            var parserResult = parser.ParseArguments<Options>(args);
            parserResult.WithParsed<Options>(options => RunOptions(options)).WithNotParsed(errs => DisplayHelp(parserResult, errs));

            StatisticsAgregator agregator = new StatisticsAgregator(_statsFile);
            if (!agregator.Successfully)
            {
                Console.WriteLine(agregator.ErrorMessage);
                return;
            }

            ParseStatistics(agregator.Statistics);
            SessionFilters filter = SelectFIlter(agregator);
            CreateSessionTable();

            switch (filter)
            {
                case SessionFilters.SummaryOnly:
                    ShowSummary(agregator);
                    break;
                case SessionFilters.AllSessions:
                    Console.WriteLine();
                    ShowSessions(_sessions);
                    Console.WriteLine($"all {_sessions.Count} sessions are shown");
                    ShowSummary(agregator);
                    break;
                case SessionFilters.WorkingOnly:
                    var working = _sessions
                        .Where(s => s.IsWorking)
                        .Select(s => s)
                        .ToList<Session>();
                    Console.WriteLine();
                    ShowSessions(working);
                    Console.WriteLine($"all {working.Count} working sessions out of {_sessions.Count} are shown");
                    ShowSummary(agregator);
                    break;
                case SessionFilters.LastNSessions:
                    var lastN = _sessions
                        .Skip(Math.Max(0, agregator.Statistics.Count() - _lines))
                        .ToList<Session>();
                    Console.WriteLine();
                    ShowSessions(lastN);
                    Console.WriteLine($"{lastN.Count} last sessions are shown");
                    ShowSummary(agregator);
                    break;
                case SessionFilters.LastNWorkingSessions:
                    var lastNWorking = _sessions
                        .Skip(Math.Max(0, agregator.Statistics.Count() - _lines))
                        .Where(s => s.IsWorking)
                        .Select(s => s)
                        .ToList<Session>();
                    Console.WriteLine();
                    ShowSessions(lastNWorking);
                    Console.WriteLine($"{lastNWorking.Count} working sessions from the last {_lines} are shown");
                    ShowSummary(agregator);
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(_csvFile))
            {
                ExportCsv(agregator.Statistics, _csvFile);
            }
        }

        private static void CreateSessionTable()
        {
            _sessionTable = new Table(new List<Column>
            {
                new Column(5,"#", Alignment.Left),
                new Column(38,"Id", Alignment.Left),
                new Column(21,"Start Time", Alignment.Left),
                new Column(8,"Total", Alignment.Left),
                new Column(10,"Scan", Alignment.Left),
                new Column(8,"[+]", Alignment.Left),
                new Column(8,"[-]", Alignment.Left),
                new Column(8,"[A]", Alignment.Left),
                new Column(10,"Files", Alignment.Left),
                new Column(10,"Folders", Alignment.Left),
                new Column(10,"Work time", Alignment.Left),
                new Column(25,"Log", Alignment.Left),
            });
        }
        
        private static SessionFilters SelectFIlter(StatisticsAgregator agregator)
        {
            SessionFilters filter;
            if (_summaryOnly) filter = SessionFilters.SummaryOnly;
            else if (_onlyWorking && _lines < agregator.Statistics.Count && _lines != 0) filter = SessionFilters.LastNWorkingSessions;
            else if (_onlyWorking) filter = SessionFilters.WorkingOnly;
            else if (_showAll || _lines >= agregator.Statistics.Count) filter = SessionFilters.AllSessions;
            else filter = SessionFilters.LastNSessions;
            return filter;
        }

        private static void ParseStatistics(List<SessionStatistics> statistics)
        {
            int number = 0;

            foreach (SessionStatistics stat in statistics)
            {
                AddValuesToSummary(stat);

                Session session = new Session(++number, stat);
                _sessions.Add(session);

                if (session.IsWorking) _notZeroSessions += 1;
            }
        }

        private static void AddValuesToSummary(SessionStatistics stat)
        {
            _added += stat.AddedFiles;
            _removed += stat.RemovedFiles;
            _affected += stat.AffectedFolders;
            _scanTime += stat.ScanElapsedTime;
            _fileScanTime += stat.FileProcessingElapsedTime;
            _folderScanTime += stat.FolderProcessingElapsedTime;
        }

        private static void ShowSessions(List<Session> sessions)
        {
            _sessionTable.DrawHeader();

            for (int s = 0; s < sessions.Count; s++)
            {
                Session session = sessions[s];
                SessionStatistics statistics = session.Statistics;

                Tuple<bool, string> result = SearchLog(statistics);
                string logFile = result.Item2;

                string hasLogMarker = result.Item1 ? logFile : string.Empty;

                _sessionTable.AddRow(new List<string>()
                {
                    $"{(session.Number)}",
                    $"{statistics.Id}",
                    $"{statistics.StartTime.ToString("dd-MM-yyyy HH:mm:ss")}",
                    $"{statistics.TotalFiles}",
                    $"{session.ScanElapsedTime.TotalSeconds:0.0000}",
                    $"{statistics.AddedFiles}",
                    $"{statistics.RemovedFiles}",
                    $"{statistics.AffectedFolders}",
                    $"{session.FileProcessingElapsedTime.TotalSeconds:0.0000}",
                    $"{session.FolderProcessingElapsedTime.TotalSeconds:0.0000}",
                    $"{session.WorkTime.TotalSeconds:0.0000}",
                    $"{hasLogMarker}"
                });

                if (_showFolders && statistics.ProcessedFolders.Count != 0)
                {
                    _sessionTable.StartRowsWithSpan(LAST_COLUMN_SPAN);

                    for (int f = 0; f < statistics.ProcessedFolders.Count; f++)
                    {
                        string item = statistics.ProcessedFolders[f];
                        _sessionTable.AddRow(new List<string>() { "", $"  [{(f + 1)}] {item}" }, LAST_COLUMN_SPAN);
                    }

                    if (s < sessions.Count - 1)
                    {
                        _sessionTable.EndRowsWithSpan(LAST_COLUMN_SPAN);
                    }
                }
            }

            if (_showFolders && sessions[sessions.Count - 1].IsWorking)
            {
                _sessionTable.LastRowWithSpan(LAST_COLUMN_SPAN);
            }
            else _sessionTable.Close();
        }

        private static Tuple<bool, string> SearchLog(SessionStatistics statistics)
        {
            if (string.IsNullOrEmpty(_logsPath)) return new Tuple<bool, string>(false, string.Empty);

            string pattern = statistics.StartTime.ToString("_ddMMyyyy_HHmmss");
            pattern = pattern.Remove(pattern.Length - 1);
            string[] files = Directory.GetFiles(_logsPath);

            foreach (string file in files)
            {
                if (file.IndexOf(pattern) != -1) return new Tuple<bool, string>(true, Path.GetFileName(file));
            }

            return new Tuple<bool, string>(false, string.Empty);
        }

        private static void ShowSummary(StatisticsAgregator agregator)
        {
            List<SessionStatistics> statistics = agregator.Statistics;

            DateTime first = statistics[0].StartTime;
            DateTime last = statistics[statistics.Count - 1].StartTime;
            TimeSpan period = last - first;
            TimeSpan workTime = TimeSpan.FromTicks(_scanTime + _fileScanTime + _folderScanTime);

            double hoursAll = period.TotalHours;
            double hoursWork = workTime.TotalHours;
            double ratio = hoursWork / hoursAll;
            double averageInterval = period.TotalMinutes / statistics.Count;
            double averageRealInterval = period.TotalMinutes / _notZeroSessions;
            double workSessionPercent = (double)_notZeroSessions * 100 / statistics.Count;
            double averagefoldersPerSession = (double)_affected / statistics.Count;
            double scanPercentFromPeriod = TimeSpan.FromTicks(_scanTime).TotalSeconds * 100 / period.TotalSeconds;
            double filePercentFromPeriod = TimeSpan.FromTicks(_fileScanTime).TotalSeconds * 100 / period.TotalSeconds;
            double folderPercentFromPeriod = TimeSpan.FromTicks(_folderScanTime).TotalSeconds * 100 / period.TotalSeconds;
            double workPercentFromPeriod = workTime.TotalSeconds * 100 / period.TotalSeconds;

            Console.WriteLine();
            Console.WriteLine($"Statistics period:       {ToReadableString(period)}");
            Console.WriteLine($"First session:           {statistics[0].StartTime.ToString("dd-MM-yyyy HH:mm:ss")}");
            Console.WriteLine($"Last session start:      {statistics[statistics.Count - 1].StartTime.ToString("dd-MM-yyyy HH:mm:ss")}");
            Console.WriteLine($"Sessions count:          {statistics.Count} (~{averageInterval:0} min interval)");
            Console.WriteLine($"Sessions count (w/work): {_notZeroSessions} ({workSessionPercent:0}% of all sessions, ~{averageRealInterval:0} min interval)");
            Console.WriteLine();
            Console.WriteLine($"Files added/removed:     {_added}/{_removed}");
            Console.WriteLine($"Processed folders:       {_affected} (~{averagefoldersPerSession:0.00} folders/session)");
            Console.WriteLine();
            Console.WriteLine($"Scan time:               {ToReadableString(TimeSpan.FromTicks(_scanTime))} ({scanPercentFromPeriod:0.0}%)");
            Console.WriteLine($"File processing time:    {ToReadableString(TimeSpan.FromTicks(_fileScanTime))} ({filePercentFromPeriod:0.0}%)");
            Console.WriteLine($"Folder processing time:  {ToReadableString(TimeSpan.FromTicks(_folderScanTime))} ({folderPercentFromPeriod:0.0}%)");
            Console.WriteLine($"Total work time:         {ToReadableString(workTime)} ({workPercentFromPeriod:0.0}%)");
            Console.WriteLine($"Ratio (work/period):     {ratio:0.0000}");
            Console.WriteLine("---");
            Console.WriteLine($"Statistics file size:    {agregator.Size:0.00} Kb (~{(agregator.Size / period.TotalDays):0.00} Kb/day)");
            Console.WriteLine();
        }

        private static void ExportCsv(List<SessionStatistics> statistics, string csvFile)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"Number,Start Time,Total Files (pc.),Scan Elapsed Time (s),Added Files (pc.),Removed Files (pc.),Affected Folders (pc.),File Processing (s),Folder Processing (s)");
            sb.Append(Environment.NewLine);

            for (int i = 0; i < statistics.Count; i++)
            {
                SessionStatistics stat = statistics[i];
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

        private static void DisplayHelp(ParserResult<Options> parserResult, IEnumerable<Error> errs)
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

        private static void RunOptions(Options options)
        {
            _statsFile = options.InputFile;
            _lines = options.Lines;
            _summaryOnly = options.SummaryOnly;
            _csvFile = options.Export;
            _onlyWorking = options.OnlyWorking;
            _showFolders = options.ShowFolders;
            _logsPath = string.IsNullOrEmpty(options.LogsPath) ? string.Empty : options.LogsPath;

            if (_lines == 0) _showAll = true;
        }

        public static string ToReadableString(TimeSpan span)
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
    }
}