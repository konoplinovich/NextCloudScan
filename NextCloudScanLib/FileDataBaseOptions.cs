using System.IO;

namespace NextCloudScan.Lib
{
    public class FileDataBaseOptions
    {
        public string Path { get; private set; }
        public string BasePath { get; private set; }
        public string BaseFile { get; private set; }
        public string DiffFile { get; private set; }
        public string AffectedFoldersFile { get; private set; }
        public bool ResetBase { get; private set; }
        public bool ReduceToParents { get; private set; }

        public FileDataBaseOptions(string path, string basePath = "base", bool resetBase = false, bool reduceToParents = false)
        {
            Path = path;
            BasePath = new DirectoryInfo(basePath).FullName;

            BaseFile = System.IO.Path.Combine(BasePath, "base.xml");
            DiffFile = System.IO.Path.Combine(BasePath, "diff.xml");
            AffectedFoldersFile = System.IO.Path.Combine(BasePath, "affected");

            ResetBase = resetBase;
            ReduceToParents = reduceToParents;
        }
    }
}
