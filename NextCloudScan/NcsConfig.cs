namespace NextCloudScan
{
    public sealed class NcsConfig
    {
        public string Path { get; set; } = @"w:\dev\";
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
        public NextCloudScan.Interfaces.InterfaceType Interface { get; set; } = NextCloudScan.Interfaces.InterfaceType.Screen;

        public NcsConfig() { }
    }
}