namespace NextCloudScan
{
    public sealed class NcsConfig
    {
        public string Path { get; set; } = "[root folder for scanning]";
        public bool IsNextCloud { get; set; } = false;
        public bool ReduceToParents { get; set; } = false;
        public string BaseFile { get; set; } = "base.xml";
        public string DiffFile { get; set; } = "diff.xml";
        public string AffectedFoldersFile { get; set; } = "affected_folders";
        public string FileActionApp { get; set; } = "[application for scanning files]";
        public string FolderActionApp { get; set; } = "[application for scanning folders]";
        public string FileActionAppOptions { get; set; } = "[application arguments ($f will be replaced with the file name)]";
        public string FolderActionAppOptions { get; set; } = "[application arguments ($f will be replaced with the folder name)]";
        public bool WaitOnExit { get; set; } = false;
        public bool ShowFileDetails { get; set; } = false;
        public bool ShowConfigParametersOnStart { get; set; } = true;
        public bool OneProcessAtATime { get; set; } = true;
        public int LockLifeTime { get; set; } = 10;
        public NextCloudScan.UI.SupportedUI Interface { get; set; } = NextCloudScan.UI.SupportedUI.Screen;
        public string LogFile { get; set; } = "[logFile]";
        public bool SingleLogFile { get; set; } = true;
        public int LogFilesAgeLimit { get; set; } = 2;

        public NcsConfig() { }
    }
}