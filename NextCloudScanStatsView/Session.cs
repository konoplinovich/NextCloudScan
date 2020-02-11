using NextCloudScan.Statistics.Lib;
using System;

namespace NextCloudScanStatsView
{
    partial class StatsViewer
    {
        private class Session
        {
            public long Number { get; set; }
            public bool IsWorking { get; private set; }
            public SessionStatistics Statistics { get; set; }
            public TimeSpan ScanElapsedTime { get; set; }
            public TimeSpan FileProcessingElapsedTime { get; set; }
            public TimeSpan FolderProcessingElapsedTime { get; set; }
            public TimeSpan WorkTime { get; set; }

            public Session(long number, SessionStatistics statistics)
            {
                Number = number;
                Statistics = statistics;
                IsWorking = statistics.FileProcessingElapsedTime != 0 || statistics.FolderProcessingElapsedTime != 0;

                WorkTime = TimeSpan.FromTicks(statistics.ScanElapsedTime + statistics.FileProcessingElapsedTime + statistics.FolderProcessingElapsedTime);
                ScanElapsedTime = TimeSpan.FromTicks(statistics.ScanElapsedTime);
                FileProcessingElapsedTime = TimeSpan.FromTicks(statistics.FileProcessingElapsedTime);
                FolderProcessingElapsedTime = TimeSpan.FromTicks(statistics.FolderProcessingElapsedTime);
            }
        }
    }
}