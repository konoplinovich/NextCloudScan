using System;

namespace NextCloudScan.Activities
{
    internal class ActionsResult
    {
        public int Completed { get; set; }
        public int Failed { get; set; }
        public TimeSpan ElapsedTime { get; set; }
    }
}