namespace NextCloudScan
{
    public sealed class NcsConfig
    {
        public string Path { get; set; } = "[root folder for scanning]";
        public string BasePath { get; set; } = "base";
        public string FileActionApp { get; set; } = "[application for scanning files]";
        public string FileActionAppOptions { get; set; } = "[application arguments ($f will be replaced with the file name)]";
        public string FolderActionApp { get; set; } = "[application for scanning folders]";
        public string FolderActionAppOptions { get; set; } = "[application arguments ($f will be replaced with the folder name)]";
        public NextCloudScan.UI.SupportedUI Interface { get; set; } = NextCloudScan.UI.SupportedUI.Screen;
        public bool ShowConfigParametersOnStart { get; set; } = true;
        public bool ShowFileDetails { get; set; } = false;
        public bool IsNextCloud { get; set; } = false;
        public bool ReduceToParents { get; set; } = false;
        public bool SingleLogFile { get; set; } = true;
        public int LogFilesAgeLimit { get; set; } = 2;
        public bool OneProcessAtATime { get; set; } = true;
        public int LockLifeTime { get; set; } = 10;

        public NcsConfig() { }
    }
}