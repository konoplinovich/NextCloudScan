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
            
            foreach (SessionStatistics stat in statistics)
            {
                TimeSpan scanElapsedTime = TimeSpan.FromTicks(stat.ScanElapsedTime);
                TimeSpan fileProcessingElapsedTime = TimeSpan.FromTicks(stat.FileProcessingElapsedTime);
                TimeSpan folderProcessingElapsedTime = TimeSpan.FromTicks(stat.FolderProcessingElapsedTime);

                //Console.WriteLine("{0}\t{5}\t{8}\t{1:0.000}\t{2}\t{3}\t{4}\t{6:0.000}\t{7:0.000}",
                //    stat.Id,
                //    scanElapsedTime.TotalSeconds,
                //    stat.AddedFiles,
                //    stat.RemovedFiles,
                //    stat.AffectedFolders,
                //    stat.StartTime,
                //    fileProcessingElapsedTime.TotalSeconds,
                //    folderProcessingElapsedTime.TotalSeconds,
                //    stat.TotalFiles);

                Console.WriteLine($"{stat.Id}│\t{stat.StartTime}│\t{stat.TotalFiles,6}│\t{scanElapsedTime.TotalSeconds,10:0.0000}│\t{stat.AddedFiles,6}│\t{stat.RemovedFiles,6}│\t{stat.AffectedFolders,6}│\t{fileProcessingElapsedTime.TotalSeconds,10:0.0000}│\t{folderProcessingElapsedTime.TotalSeconds,10:0.0000}");
            }

            Console.WriteLine("────────────────────────────────────┴──────────────────────┴──────────┴───────────┴───────────┴───────┴───────┴───────────┴───────────────");
        }
    }
}
