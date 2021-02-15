using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextCloudScanStatsView
{
    internal class Options
    {
        public string StatsFile { get; set; }
        public string LogsPath { get; set; }
        public string CsvFile { get; set; }
        public int Lines { get; set; }
        public bool LinesOnlyWorking { get; set; }
        public int Working { get; set; }
        public bool ShowFolders { get; set; }
        public bool SummaryOnly { get; set; }
        public bool ShowAll { get; set; } = false;
        public int LastColumnSpan { get; set; } = 15;
    }
}