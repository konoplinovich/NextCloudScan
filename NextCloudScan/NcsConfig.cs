namespace NextCloudScan
{
    public sealed class NcsConfig
    {
        public string Path { get; set; } = @"w:\dev\";
        public string BaseFile { get; set; } = "base.xml";
        public string DiffFile { get; set; } = "diff.xml";
        public string AffectedFoldersFile { get; set; } = "affected_folders";
        public string FileAction { get; set; } = "[payload.app] $f";
        public bool WaitOnExit { get; set; } = false;
        public bool ShowFileDetails { get; set; } = false;
        public bool ShowConfigParametersOnStart { get; set; } = false;

        public NcsConfig() { }
    }
}