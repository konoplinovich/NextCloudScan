using NextCloudScan.Statistics.Lib;
using System;
using System.Collections.Generic;

namespace NextCloudScanStatsView
{
    class StatsViewer
    {
        private static string _statsFile;
        
        static void Main(string[] args)
        {
            if (args.Length == 0) return;
            _statsFile = args[0];

            StatisticsAgregator agregator = new StatisticsAgregator(_statsFile);

            List<SessionStatistics> statistics = new List<SessionStatistics>();
            if (agregator.Successfully) statistics = agregator.Statistisc;

            Console.WriteLine("────────────────────────────────────┬──────────────────────┬──────────┬───────────┬───────────┬───────┬───────┬───────────┬───────────────");
            Console.WriteLine("Id                                  │            Start Time│     Total│       Scan│        [+]│    [-]│    [A]│      Files│        Folders");
            Console.WriteLine("────────────────────────────────────┼──────────────────────┼──────────┼───────────┼───────────┼───────┼───────┼───────────┼───────────────");

            long added = 0;
            long removed = 0;
            long affected = 0;
            long scanTime = 0;
            long fileScanTime = 0;
            long folderScanTime = 0;

            foreach (SessionStatistics stat in statistics)
            {
                TimeSpan scanElapsedTime = TimeSpan.FromTicks(stat.ScanElapsedTime);
                TimeSpan fileProcessingElapsedTime = TimeSpan.FromTicks(stat.FileProcessingElapsedTime);
                TimeSpan folderProcessingElapsedTime = TimeSpan.FromTicks(stat.FolderProcessingElapsedTime);

                added += stat.AddedFiles;
                removed += stat.RemovedFiles;
                affected += stat.AffectedFolders;

                scanTime += stat.ScanElapsedTime;
                fileScanTime += stat.FileProcessingElapsedTime;
                folderScanTime += stat.FolderProcessingElapsedTime;

                Console.WriteLine($"{stat.Id}│\t{stat.StartTime}│\t{stat.TotalFiles,6}│\t{scanElapsedTime.TotalSeconds,10:0.0000}│\t{stat.AddedFiles,6}│\t{stat.RemovedFiles,6}│\t{stat.AffectedFolders,6}│\t{fileProcessingElapsedTime.TotalSeconds,10:0.0000}│\t{folderProcessingElapsedTime.TotalSeconds,10:0.0000}");
            }

            Console.WriteLine("────────────────────────────────────┴──────────────────────┴──────────┴───────────┴───────────┴───────┴───────┴───────────┴───────────────");

            DateTime first = statistics[0].StartTime;
            DateTime last = statistics[statistics.Count-1].StartTime;
            TimeSpan period = last - first;

            double hoursAll = period.TotalHours;
            double hoursWork = (TimeSpan.FromTicks(scanTime) + TimeSpan.FromTicks(fileScanTime) + TimeSpan.FromTicks(folderScanTime)).TotalHours;
            double ratio = hoursWork / hoursAll;

            Console.WriteLine("");
            Console.WriteLine($"Statistics period:      {ToReadableString(period)}");
            Console.WriteLine($"First session:          {statistics[0].StartTime}");
            Console.WriteLine($"Last session start:     {statistics[statistics.Count - 1].StartTime}");
            Console.WriteLine($"Sessions count:         {statistics.Count} (average interval {(period.TotalMinutes/statistics.Count):0.000} min)");
            Console.WriteLine("");
            Console.WriteLine($"Files added/removed:    {added}/{removed}");
            Console.WriteLine($"Processed folders:      {affected}");
            Console.WriteLine("");
            Console.WriteLine($"Scan time:              {ToReadableString(TimeSpan.FromTicks(scanTime))}");
            Console.WriteLine($"File processing time:   {ToReadableString(TimeSpan.FromTicks(fileScanTime))}");
            Console.WriteLine($"Folder processing time: {ToReadableString(TimeSpan.FromTicks(folderScanTime))}");
            Console.WriteLine($"Total work time:        {ToReadableString(TimeSpan.FromTicks(scanTime) + TimeSpan.FromTicks(fileScanTime) + TimeSpan.FromTicks(folderScanTime))}");
            Console.WriteLine($"Ratio (work/period):    {ratio:0.0000}");
            Console.WriteLine($"Statistics file size:   {agregator.Size} bytes");

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
