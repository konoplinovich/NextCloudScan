using CommandLine;

namespace NextCloudScanStatsView
{
    partial class StatsViewer
    {
        internal class Options
        {
            [Option('f', "file", Required = true, HelpText = "Statistics XML files to be processed")]
            public string InputFile { get; set; }

            [Option('l', "last", Required = false, HelpText = "Show last <n> sessions")]
            public int Lines { get; set; }

            [Option('t', "total", Required = false, HelpText = "Summary only")]
            public bool SummaryOnly { get; set; }

            [Option('e', "export", Required = false, HelpText = "Export .CSV file")]
            public string Export { get; set; }

            [Option('w', "working", Required = false, HelpText = "Show only working sessions")]
            public bool OnlyWorking { get; set; }
            
            [Option('a', "affected", Required = false, HelpText = "Show affected folders (of avialable)")]
            public bool ShowFolders { get; set; }
        }
    }
}
