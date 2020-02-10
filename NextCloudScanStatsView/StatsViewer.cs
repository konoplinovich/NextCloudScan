using CommandLine;
using CommandLine.Text;
using NextCloudScan.Statistics.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NextCloudScanStatsView
{
    partial class StatsViewer
    {
        private static int _lines;
        private static string _statsFile;
        private static string _csvFile;
        private static bool _summaryOnly;
        private static bool _showAll = false;

        private static long _added = 0;
        private static long _removed = 0;
        private static long _affected = 0;
        private static long _scanTime = 0;
        private static long _fileScanTime = 0;
        private static long _folderScanTime = 0;
        private static int _notZeroSessions = 0;

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

            CalculateSummary(agregator.Statistisc);

            if (!_summaryOnly)
            {
                if (_showAll) ShowSessions(agregator.Statistisc, agregator.Statistisc.Count);
                else ShowSessions(agregator.Statistisc, _lines);
            }

            ShowSummary(agregator);

            if (!string.IsNullOrEmpty(_csvFile))
            {
                ExportCsv(agregator.Statistisc, _csvFile);
            }
        }

        private static void CalculateSummary(List<SessionStatistics> statistics)
        {
            foreach (SessionStatistics stat in statistics)
            {
                _added += stat.AddedFiles;
                _removed += stat.RemovedFiles;
                _affected += stat.AffectedFolders;

                if (stat.AffectedFolders != 0) _notZeroSessions += 1;

                _scanTime += stat.ScanElapsedTime;
                _fileScanTime += stat.FileProcessingElapsedTime;
                _folderScanTime += stat.FolderProcessingElapsedTime;
            }
        }

        private static void ShowSessions(List<SessionStatistics> statistics, int lines)
        {
            Console.WriteLine();

            int displayedlines = 0;
            if (lines >= statistics.Count)
            {
                displayedlines = statistics.Count;
                Console.WriteLine($"Show all {displayedlines} sessions:");
            }
            else
            {
                displayedlines = lines;
                Console.WriteLine($"Show last {displayedlines} sessions:");
            }

            Console.WriteLine("───────┬──────────────────────────────────────┬─────────────────────┬────────┬──────────┬────────┬────────┬────────┬──────────┬──────────");
            Console.WriteLine("      #│                                    Id│           Start Time│   Total│      Scan│     [+]│     [-]│     [A]│     Files│   Folders");
            Console.WriteLine("───────┼──────────────────────────────────────┼─────────────────────┼────────┼──────────┼────────┼────────┼────────┼──────────┼──────────");


            for (int index = (statistics.Count - displayedlines); index < statistics.Count; index++)
            {
                SessionStatistics stat = statistics[index];
                TimeSpan scanElapsedTime = TimeSpan.FromTicks(stat.ScanElapsedTime);
                TimeSpan fileProcessingElapsedTime = TimeSpan.FromTicks(stat.FileProcessingElapsedTime);
                TimeSpan folderProcessingElapsedTime = TimeSpan.FromTicks(stat.FolderProcessingElapsedTime);

                Console.WriteLine($"{(index + 1),7}│{stat.Id,38}│{stat.StartTime.ToString("dd-MM-yyyy HH:mm:ss"),21}│{stat.TotalFiles,8}│{scanElapsedTime.TotalSeconds,10:0.0000}│{stat.AddedFiles,8}│{stat.RemovedFiles,8}│{stat.AffectedFolders,8}│{fileProcessingElapsedTime.TotalSeconds,10:0.0000}│{folderProcessingElapsedTime.TotalSeconds,10:0.0000}");
            }

            Console.WriteLine("───────┴──────────────────────────────────────┴─────────────────────┴────────┴──────────┴────────┴────────┴────────┴──────────┴──────────");
        }

        private static void ShowSummary(StatisticsAgregator agregator)
        {
            List<SessionStatistics> statistics = agregator.Statistisc;

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

            Console.WriteLine("");
            Console.WriteLine($"Statistics period:       {ToReadableString(period)}");
            Console.WriteLine($"First session:           {statistics[0].StartTime}");
            Console.WriteLine($"Last session start:      {statistics[statistics.Count - 1].StartTime}");
            Console.WriteLine($"Sessions count:          {statistics.Count} (~{averageInterval:0.000} min interval)");
            Console.WriteLine($"Sessions count (w/work): {_notZeroSessions} ({workSessionPercent:0}% of all sessions, ~{averageRealInterval:0.000} min interval)");
            Console.WriteLine("");
            Console.WriteLine($"Files added/removed:     {_added}/{_removed}");
            Console.WriteLine($"Processed folders:       {_affected} (~{averagefoldersPerSession:0.000} folders/session)");
            Console.WriteLine("");
            Console.WriteLine($"Scan time:               {ToReadableString(TimeSpan.FromTicks(_scanTime))} ({scanPercentFromPeriod:0.000}%)");
            Console.WriteLine($"File processing time:    {ToReadableString(TimeSpan.FromTicks(_fileScanTime))} ({filePercentFromPeriod:0.000}%)");
            Console.WriteLine($"Folder processing time:  {ToReadableString(TimeSpan.FromTicks(_folderScanTime))} ({folderPercentFromPeriod:0.000}%)");
            Console.WriteLine($"Total work time:         {ToReadableString(workTime)} ({workPercentFromPeriod:0.000}%)");
            Console.WriteLine($"Ratio (work/period):     {ratio:0.0000}");
            Console.WriteLine("---");
            Console.WriteLine($"Statistics file size:    {agregator.Size:0.00} Kb (~{(agregator.Size / period.TotalDays):0.00} Kb/day)");
            Console.WriteLine("");
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
                    h.Heading = "NextCloudScan Statistics Viewer 1.2.0.0";
                    h.Copyright = "";
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