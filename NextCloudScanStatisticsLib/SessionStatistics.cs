using System;
using System.Collections.Generic;

namespace NextCloudScan.Statistics.Lib
{
    public class SessionStatistics : IEquatable<SessionStatistics>
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
        public List<string> ProcessedFolders { get; set; }
        public int Errors { get; set; }
        public int ReplacedWithParents { get; set; }
        public int RemovedAsSubfolders { get; set; }

        public SessionStatistics() { }

        public override bool Equals(object obj)
        {
            return Equals(obj as SessionStatistics);
        }

        public bool Equals(SessionStatistics other)
        {
            return other != null &&
                   Id.Equals(other.Id) &&
                   StartTime == other.StartTime;
        }

        public override int GetHashCode()
        {
            int hashCode = 1405877102;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + StartTime.GetHashCode();
            return hashCode;
        }
    }
}
