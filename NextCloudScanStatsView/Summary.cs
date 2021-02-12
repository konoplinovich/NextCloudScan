using System;

namespace NextCloudScanStatsView
{
    internal class Summary
    {
        public long Added { get; set; }
        public long Removed { get; set; }
        public long Affected { get; set; }
        public long ScanTime { get; set; }
        public long FileScanTime { get; set; }
        public long FolderScanTime { get; set; }
        public double TotalFilesSize { get; set; }
        public TimeSpan TotalLoadTime { get; set; }
        public int TotalSessionsCount { get; set; }
        public int NotZeroSessions { get; set; }
    }
}