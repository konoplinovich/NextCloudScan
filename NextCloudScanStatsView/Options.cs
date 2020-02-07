using CommandLine;

namespace NextCloudScanStatsView
{
    partial class StatsViewer
    {
        internal class Options
        {
            [Option('f', "file", Required = true, HelpText = "Statistics XML files to be processed")]
            public string InputFile { get; set; }

            [Option('s', "sessions", Required = false, HelpText = "Show last <n> sessions")]
            public int Lines { get; set; }

            [Option('t', "total", Required = false, HelpText = "Summary only")]
            public bool SummaryOnly { get; set; }
        }
    }
}
