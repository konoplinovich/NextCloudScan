﻿namespace NextCloudScan
{
    public class ProgressResult
    {
        public string Path { get; set; }
        public string Log { get; set; }
        public string ErrorMessage { get; set; }
        public bool HasError { get; set; }
    }
}