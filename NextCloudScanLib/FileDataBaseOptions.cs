using System.IO;

namespace NextCloudScan.Lib
{
    public class FileDataBaseOptions
    {
        private const string BASE_FILE = "base.xml";
        private const string DIFF_FILE = "diff.xml";
        private const string AFFECTED_FILE = "affected_folders";
        private const string BASE_DAFAULT = "base";
        public string Path { get; private set; }
        public string BasePath { get; private set; }
        public string BaseFile { get; private set; }
        public string DiffFile { get; private set; }
        public string AffectedFoldersFile { get; private set; }
        public bool ResetBase { get; private set; }
        public bool ReduceToParents { get; private set; }

        public FileDataBaseOptions(string path, string basePath = BASE_DAFAULT, bool resetBase = false, bool reduceToParents = false)
        {
            Path = path;
            BasePath = new DirectoryInfo(basePath).FullName;

            BaseFile = System.IO.Path.Combine(BasePath, BASE_FILE);
            DiffFile = System.IO.Path.Combine(BasePath, DIFF_FILE);
            AffectedFoldersFile = System.IO.Path.Combine(BasePath, AFFECTED_FILE);

            ResetBase = resetBase;
            ReduceToParents = reduceToParents;
        }
    }
}