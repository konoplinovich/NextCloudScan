using CommandLine;

namespace NextCloudScanStatsView
{
    internal class ParsedOptions
    {
        [Option("file", Required = true, HelpText = "Statistics XML files to be processed")]
        public string InputFile { get; set; }

        [Option("path", Required = true, HelpText = "Path to NextCloudScan log files")]
        public string LogsPath { get; set; }

        [Option("lastN", Required = false, HelpText = "Show last <n> sessions")]
        public int Lines { get; set; }

        [Option("working", Required = false, HelpText = "Show only working sessions from last <n> sessions")]
        public bool LinesOnlyWorking { get; set; }

        [Option("workingN", Required = false, HelpText = "Show last <n> working sessions")]
        public int Working { get; set; }

        [Option("summary", Required = false, HelpText = "Summary only")]
        public bool SummaryOnly { get; set; }

        [Option("export", Required = false, HelpText = "Export .CSV file")]
        public string Export { get; set; }
                
        [Option("affected", Required = false, HelpText = "Show affected folders (of avialable)")]
        public bool ShowFolders { get; set; }        

    }
}