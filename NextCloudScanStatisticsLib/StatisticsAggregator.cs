using Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NextCloudScan.Statistics.Lib
{
    public class StatisticsAggregator
    {
        string _statsFile;
        string _statsDirectory;
        string _statsFileName;
        string _statsExt;

        public List<SessionStatistics> Statistics { get; private set; } = new List<SessionStatistics>();
        public bool Successfully { get; private set; } = false;
        public double Size { get; private set; }
        public string ErrorMessage { get; private set; }

        public StatisticsAggregator(string statsFile)
        {
            _statsFile = statsFile;

            _statsDirectory = (new FileInfo(_statsFile)).DirectoryName;
            _statsFileName = Path.GetFileNameWithoutExtension(_statsFile);
            _statsExt = Path.GetExtension(_statsFile);
        }

        public void Append(SessionStatistics session)
        {
            List<SessionStatistics> part = new List<SessionStatistics>();
            string partFile = AppendIdToFilename(session.Id.ToString());

            part.Add(session);
            Save(part, partFile);
        }

        public void Load()
        {
            CombineStatisticsParts();

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

        private void CombineStatisticsParts()
        {
            Console.WriteLine();
            Console.WriteLine(_statsDirectory);
            Console.WriteLine(_statsFileName);

            string[] files = Directory.GetFiles(_statsDirectory, $"{_statsFileName}*.*");

            HashSet<SessionStatistics> combined = new HashSet<SessionStatistics>();

            foreach (string file in files)
            {
                List<SessionStatistics> parts = new List<SessionStatistics>();
                parts = XmlExtension.ReadFromXmlFile<List<SessionStatistics>>(file);

                foreach(SessionStatistics part in parts)
                {
                    combined.Add(part);
                }

                Console.WriteLine($"{parts.Count}:{combined.Count}");
            }

            Save(new List<SessionStatistics>(combined.OrderBy(stat => { return stat.StartTime; })), _statsFile);
        }

        private void Save(List<SessionStatistics> statistics, string statsFile)
        {
            XmlExtension.WriteToXmlFile<List<SessionStatistics>>(statsFile, statistics);
        }

        private string AppendIdToFilename(string id)
        {
            string filename = $"{_statsFileName}_{id}{_statsExt}";
            string partFile = Path.Combine(_statsDirectory, filename);
            return partFile;
        }
    }
}