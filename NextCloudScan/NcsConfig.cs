namespace NextCloudScan
{
    public sealed class NcsConfig
    {
        public string Path { get; set; } = @"w:\dev\";
        public string BaseFile { get; set; } = "base.xml";
        public string DiffFile { get; set; } = "diff.xml";
        public string AffectedFoldersFile { get; set; } = "affected_folders";
        public string FileActionApp { get; set; } = "[file.payload.app]";
        public string FolderActionApp { get; set; } = "[folder.payload.app]";
        public string FileActionAppOptions { get; set; } = "$f";
        public string FolderActionAppOptions { get; set; } = "$f";
        public bool WaitOnExit { get; set; } = false;
        public bool ShowFileDetails { get; set; } = false;
        public bool ShowConfigParametersOnStart { get; set; } = false;

        public NcsConfig() { }
    }
}