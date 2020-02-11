using Extensions;
using System.Collections.Generic;
using System.IO;

namespace NextCloudScan.Statistics.Lib
{
    public class StatisticsAgregator
    {
        string _statsFile;

        public List<SessionStatistics> Statistisc { get; private set; } = new List<SessionStatistics>();
        public bool Successfully { get; private set; } = false;
        public double Size { get; }
        public string ErrorMessage { get; private set; }

        public StatisticsAgregator(string statsFile)
        {
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
        }

        public void Append(SessionStatistics session)
        {
            Statistisc.Add(session);
            Save();
        }

        private void Save()
        {
            XmlExtension.WriteToXmlFile<List<SessionStatistics>>(_statsFile, Statistisc);
        }

        private void Load()
        {
            Statistisc = XmlExtension.ReadFromXmlFile<List<SessionStatistics>>(_statsFile);
        }
    }
}