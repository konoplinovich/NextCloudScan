using System;

namespace NextCloudScan.Statistics.Lib
{
    public class SessionStatistics
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public long TotalFiles { get; set; }
        public long AddedFiles { get; set; }
        public long RemovedFiles { get; set; }
        public int AffectedFolders { get; set; }
        public long ScanElapsedTime { get; set; }
        public long FileProcessingElapsedTime { get; set; }
        public long FolderProcessingElapsedTime { get; set; }

        public SessionStatistics() { }
    }
}
