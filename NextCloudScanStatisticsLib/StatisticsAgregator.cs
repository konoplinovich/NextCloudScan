﻿using Extensions;
using System.Collections.Generic;
using System.IO;

namespace NextCloudScan.Statistics.Lib
{
    public class StatisticsAgregator
    {
        string _statsFile;

        public List<SessionStatistics> Statistisc { get; private set; } = new List<SessionStatistics>();
        public bool Successfully { get; private set; } = false;

        public StatisticsAgregator(string statsFile)
        {
            _statsFile = statsFile;

            if (File.Exists(_statsFile))
            {
                Load();
                Successfully = true;
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