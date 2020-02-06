using Extensions;
using System.Collections.Generic;
using System.IO;

namespace NextCloudScan.Statistics.Lib
{
    public class StatisticsAgregator
    {
        List<SessionStatistics> _stats = new List<SessionStatistics>();
        string _statsFile;

        public StatisticsAgregator(string statsFile)
        {
            _statsFile = statsFile;

            if (File.Exists(_statsFile))
            {
                Load();
            }
        }

        public void Append(SessionStatistics session)
        {
            _stats.Add(session);
            Save();
        }

        private void Save()
        {
            XmlExtension.WriteToXmlFile<List<SessionStatistics>>(_statsFile, _stats);
        }

        private void Load()
        {
            _stats = XmlExtension.ReadFromXmlFile<List<SessionStatistics>>(_statsFile);
        }
    }
}