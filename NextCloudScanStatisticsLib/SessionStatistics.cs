using System;
using System.Collections.Generic;

namespace NextCloudScan.Statistics.Lib
{
    public class SessionStatistics
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public long TotalItems { get; set; }
        public long TotalFiles { get; set; }
        public long TotalFolders { get; set; }
        public long AddedFiles { get; set; }
        public long RemovedFiles { get; set; }
        public int AffectedFolders { get; set; }
        public long ScanElapsedTime { get; set; }
        public long FileProcessingElapsedTime { get; set; }
        public long FolderProcessingElapsedTime { get; set; }
        public List<string> ProcessedFolders { get; set; }
        public int Errors { get; set; }
        public int ReplacedWithParents { get; set; }
        public int RemovedAsSubfolders { get; set; }

        public SessionStatistics() { }
    }
}
