using Extensions;
using System.Collections.Generic;
using System.IO;
using System;

namespace NextCloudScan.Statistics.Lib
{
    public class StatisticsAggregator
    {
        string _statsFile;

        public List<SessionStatistics> Statistics { get; private set; } = new List<SessionStatistics>();
        public bool Successfully { get; private set; } = false;
        public double Size { get; }
        public string ErrorMessage { get; private set; }

        public TimeSpan LoadTime { get; private set; }

        public StatisticsAggregator(string statsFile)
        {
            DateTime start = DateTime.Now;
            _statsFile = statsFile;

            if (File.Exists(_statsFile))
            {
                Size = (double)new FileInfo(_statsFile).Length / 1024;
                Load();
                Successfully = true;
            }
            else
            {
                ErrorMessage = $"Statistics file not found: { statsFile}";
            }

            LoadTime = DateTime.Now - start;
        }

        public void Append(SessionStatistics session)
        {
            Statistics.Add(session);
            Save();
        }

        private void Save()
        {
            XmlExtension.WriteToXmlFile<List<SessionStatistics>>(_statsFile, Statistics);
        }

        private void Load()
        {
            Statistics = XmlExtension.ReadFromXmlFile<List<SessionStatistics>>(_statsFile);
        }
    }
}