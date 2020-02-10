using CommandLine;
using CommandLine.Text;
using NextCloudScan.Statistics.Lib;
using System;
using System.Collections.Generic;

namespace NextCloudScanStatsView
{
    partial class StatsViewer
    {
        private static int _lines;
        private static string _statsFile;
        private static bool _summaryOnly;
        private static bool _showAll = false;

        private static long _added = 0;
        private static long _removed = 0;
        private static long _affected = 0;
        private static long _scanTime = 0;
        private static long _fileScanTime = 0;
        private static long _folderScanTime = 0;

        static void Main(string[] args)
        {
            if (args.Length == 0) return;

            var parser = new Parser(with => with.HelpWriter = null);
            var parserResult = parser.ParseArguments<Options>(args);
            parserResult.WithParsed<Options>(options => RunOptions(options)).WithNotParsed(errs => DisplayHelp(parserResult, errs));

            StatisticsAgregator agregator = new StatisticsAgregator(_statsFile);
            if (!agregator.Successfully) return;

            List<SessionStatistics> statistics = new List<SessionStatistics>();
            if (agregator.Successfully) statistics = agregator.Statistisc;

            CalculateSummary(statistics);
            if (!_summaryOnly) 
            {
                if (_showAll) ShowSessions(statistics, statistics.Count);
                else ShowSessions(statistics, _lines); 
            }
            
            ShowSummary(agregator, statistics);
        }

        private static void CalculateSummary(List<SessionStatistics> statistics)
        {
            foreach (SessionStatistics stat in statistics)
            {
                _added += stat.AddedFiles;
                _removed += stat.RemovedFiles;
                _affected += stat.AffectedFolders;

                _scanTime += stat.ScanElapsedTime;
                _fileScanTime += stat.FileProcessingElapsedTime;
                _folderScanTime += stat.FolderProcessingElapsedTime;
            }
        }

        private static void ShowSessions(List<SessionStatistics> statistics, int lines)
        {
            Console.WriteLine();
            Console.WriteLine($"Show last {lines} sessions:");

            Console.WriteLine("──────┬─────────────────────────────────────┬──────────────────────┬──────────┬───────────┬───────────┬───────┬───────┬───────────┬───────────────");
            Console.WriteLine("     #│                                   Id│            Start Time│     Total│       Scan│        [+]│    [-]│    [A]│      Files│        Folders");
            Console.WriteLine("──────┼─────────────────────────────────────┼──────────────────────┼──────────┼───────────┼───────────┼───────┼───────┼───────────┼───────────────");

            for (int index = (statistics.Count - lines); index < statistics.Count; index++)
            {
                SessionStatistics stat = statistics[index];
                TimeSpan scanElapsedTime = TimeSpan.FromTicks(stat.ScanElapsedTime);
                TimeSpan fileProcessingElapsedTime = TimeSpan.FromTicks(stat.FileProcessingElapsedTime);
                TimeSpan folderProcessingElapsedTime = TimeSpan.FromTicks(stat.FolderProcessingElapsedTime);

                Console.WriteLine($"{(index + 1),6}│\t{stat.Id}│\t{stat.StartTime.ToString("dd-MM-yyyy HH:mm:ss")}│\t{stat.TotalFiles,6}│\t{scanElapsedTime.TotalSeconds,10:0.0000}│\t{stat.AddedFiles,6}│\t{stat.RemovedFiles,6}│\t{stat.AffectedFolders,6}│\t{fileProcessingElapsedTime.TotalSeconds,10:0.0000}│\t{folderProcessingElapsedTime.TotalSeconds,10:0.0000}");
            }

            Console.WriteLine("──────┴─────────────────────────────────────┴──────────────────────┴──────────┴───────────┴───────────┴───────┴───────┴───────────┴───────────────");
        }

        private static void ShowSummary(StatisticsAgregator agregator, List<SessionStatistics> statistics)
        {
            DateTime first = statistics[0].StartTime;
            DateTime last = statistics[statistics.Count - 1].StartTime;
            TimeSpan period = last - first;

            double hoursAll = period.TotalHours;
            double hoursWork = (TimeSpan.FromTicks(_scanTime) + TimeSpan.FromTicks(_fileScanTime) + TimeSpan.FromTicks(_folderScanTime)).TotalHours;
            double ratio = hoursWork / hoursAll;

            Console.WriteLine("");
            Console.WriteLine($"Statistics period:      {ToReadableString(period)}");
            Console.WriteLine($"First session:          {statistics[0].StartTime}");
            Console.WriteLine($"Last session start:     {statistics[statistics.Count - 1].StartTime}");
            Console.WriteLine($"Sessions count:         {statistics.Count} (average interval {(period.TotalMinutes / statistics.Count):0.000} min)");
            Console.WriteLine("");
            Console.WriteLine($"Files added/removed:    {_added}/{_removed}");
            Console.WriteLine($"Processed folders:      {_affected} (average {((double)_affected/ statistics.Count):0.000} folders/session)");
            Console.WriteLine("");
            Console.WriteLine($"Scan time:              {ToReadableString(TimeSpan.FromTicks(_scanTime))}");
            Console.WriteLine($"File processing time:   {ToReadableString(TimeSpan.FromTicks(_fileScanTime))}");
            Console.WriteLine($"Folder processing time: {ToReadableString(TimeSpan.FromTicks(_folderScanTime))}");
            Console.WriteLine($"Total work time:        {ToReadableString(TimeSpan.FromTicks(_scanTime) + TimeSpan.FromTicks(_fileScanTime) + TimeSpan.FromTicks(_folderScanTime))}");
            Console.WriteLine($"Ratio (work/period):    {ratio:0.0000}");
            Console.WriteLine("---");
            Console.WriteLine($"Statistics file size:   {agregator.Size:0.00} Kb (average {(agregator.Size / period.TotalDays):0.00} Kb/day)");
            Console.WriteLine("");
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
                    h.Heading = "NextCloudScan Statistics Viewer 1.1.0.0";
                    h.Copyright = "";
                    return HelpText.DefaultParsingErrorsHandler(parserResult, h);
                }, e => e);
                Console.WriteLine($"{Environment.NewLine}{helpText}");
            }
        }

        private static void RunOptions(Options options)
        {
            _statsFile = options.InputFile;
            _lines = options.Lines;
            _summaryOnly = options.SummaryOnly;

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