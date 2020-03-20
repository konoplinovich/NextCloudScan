using Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace NextCloudScan.Statistics.Lib
{
    public class StatisticsAggregator
    {
        string _statsFile;

        public List<SessionStatistics> Statistics { get; private set; } = new List<SessionStatistics>();
        public bool Successfully { get; private set; } = false;
        public double Size { get; private set; }
        public string ErrorMessage { get; private set; }

        public StatisticsAggregator(string statsFile)
        {
            _statsFile = statsFile;
        }

        public void Append(SessionStatistics session)
        {
            Statistics.Add(session);
            Save(Statistics, _statsFile);
        }

        private void Load()
        {
            if (File.Exists(_statsFile))
            {
                Size = (double)new FileInfo(_statsFile).Length / 1024;
                Statistics = XmlExtension.ReadFromXmlFile<List<SessionStatistics>>(_statsFile);
                Successfully = true;
            }
            else
            {
                ErrorMessage = $"Statistics file not found: { _statsFile}";
            }
        }

        public void AppendLazy(SessionStatistics session)
        {
            List<SessionStatistics> part = new List<SessionStatistics>();

            part.Add(session);

            string directory = (new FileInfo(_statsFile)).DirectoryName;
            string filename = Path.GetFileNameWithoutExtension(_statsFile);
            string ext = Path.GetExtension(_statsFile);
            string time = DateTime.Now.ToString("_ddMMyyyy_HHmmss");

            filename += time + ext;
            string partFile = Path.Combine(directory, filename);

            Save(part, partFile);
        }

        private void Save(List<SessionStatistics> statistics, string statsFile)
        {
            XmlExtension.WriteToXmlFile<List<SessionStatistics>>(statsFile, statistics);
        }
    }
}