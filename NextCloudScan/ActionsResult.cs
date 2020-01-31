using System;
using System.Collections.Generic;

namespace NextCloudScan
{
    internal class ActionsResult
    {
        public int Completed { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public Dictionary<string, List<string>> Log { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; }
    }
}
