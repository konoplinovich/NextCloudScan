namespace NextCloudScan
{
    public sealed class NcsConfig
    {
        public string Path { get; set; } = "[folder]";
        public bool IsNextCloud { get; set; } = false;
        public string BaseFile { get; set; } = "base.xml";
        public string DiffFile { get; set; } = "diff.xml";
        public string AffectedFoldersFile { get; set; } = "affected_folders";
        public string FileActionApp { get; set; } = "[file.payload.app]";
        public string FolderActionApp { get; set; } = "[folder.payload.app]";
        public string FileActionAppOptions { get; set; } = "--file=$f";
        public string FolderActionAppOptions { get; set; } = "--path=$f";
        public bool WaitOnExit { get; set; } = false;
        public bool ShowFileDetails { get; set; } = false;
        public bool ShowConfigParametersOnStart { get; set; } = true;
        public NextCloudScan.UI.SupportedUI Interface { get; set; } = NextCloudScan.UI.SupportedUI.Screen;
        public string LogFile { get; set; } = "[logFile]";

        public NcsConfig() { }
    }
}