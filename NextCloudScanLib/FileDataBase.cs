using Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace NextCloudScan.Lib
{
    public class FileDataBase
    {
        string _path;
        string _basePath;
        string _baseFile;
        string _diffFile;
        string _affectedFoldersFile;
        bool _reduceToParents;
        private bool _resetBase;
        List<FileItem> _base;
        List<FileItem> _newFiles;
        List<string> _affectedFolders;
        private HashSet<string> _files;

        public bool IsNewBase { get; private set; } = false;
        public long Count { get { return _base.Count; } }
        public List<FileItem> Added { get; private set; } = new List<FileItem>();
        public List<string> AddedPath { get; set; } = new List<string>();
        public List<FileItem> Removed { get; private set; } = new List<FileItem>();
        public List<string> AffectedFolders { get; private set; } = new List<string>();
        public int AffectedFoldersCount { get { return AffectedFolders.Count; } }
        public List<string> Errors { get; private set; } = new List<string>();
        public bool ChangeFoldersToParent { get; private set; } = false;
        public List<Tuple<string, string>> FoldersReplacedWithParents { get; private set; } = new List<Tuple<string, string>>();
        public bool RemoveSubfolders { get; private set; } = false;
        public List<Tuple<string, string>> FoldersRemovedAsSubfolders { get; private set; } = new List<Tuple<string, string>>();
        public string BasePath { get => _basePath; private set => _basePath = value; }

        public FileDataBase(FileDataBaseOptions options)
        {
            _path = options.Path;
            _basePath = options.BasePath;

            _baseFile = options.BaseFile;
            _diffFile = options.DiffFile;
            _affectedFoldersFile = options.AffectedFoldersFile;
            _reduceToParents = options.ReduceToParents;
            _resetBase = options.ResetBase;

            if (!Directory.Exists(_basePath)) Directory.CreateDirectory(_basePath);
        }

        public void Refresh()
        {
            _base = new List<FileItem>();
            _affectedFolders = new List<string>();
            _files = new HashSet<string>();

            if (!File.Exists(_baseFile) || _resetBase)
            {
                _base = Scan();
                Save();
                IsNewBase = true;
            }
            else
            {
                if (File.Exists(_diffFile)) File.Delete(_diffFile);
                if (File.Exists(_affectedFoldersFile)) File.Delete(_affectedFoldersFile);

                Load();
                _newFiles = Scan();

                ListComparator<FileItem> lc = new ListComparator<FileItem>();
                lc.Compare(_base, _newFiles);

                if (!lc.AddedIsEmpty)
                {
                    Added = lc.Added;
                    AddedPath = FileItemsToPaths(Added);
                }
                if (!lc.RemovedIsEmpty) Removed = lc.Removed;

                List<FileItem> diff = new List<FileItem>();
                diff.AddRange(Added);
                diff.AddRange(Removed);

                if (diff.Count != 0)
                {
                    foreach (FileItem item in diff)
                    {
                        string folder = Path.GetDirectoryName(item.Path);
                        if (!_affectedFolders.Contains(folder)) _affectedFolders.Add(folder);
                    }

                    AffectedFolders = CheckAffectedFolders(_affectedFolders);

                    SaveAffectedFoldersAsPlainText();
                    SaveDiff(diff);
                }

                _base = _newFiles;
                Save();
            }
        }

        private List<FileItem> Scan()
        {
            HashSet<FileItem> result = new HashSet<FileItem>();
            GetFiles(_path);

            foreach (string path in _files)
            {
                bool goodfile =
                    path.IndexOf("files_external") == -1
                    && path.IndexOf("files_trashbin") == -1
                    && path.IndexOf("files_versions") == -1
                    && path.IndexOf("uploads") == -1
                    && path.IndexOf("cache") == -1
                    && path.IndexOf("appdata") == -1
                    && path.IndexOf(".htaccess") == -1
                    && path.IndexOf(".ocdata") == -1
                    && path.IndexOf("index.html") == -1
                    && path.IndexOf("nextcloud.log") == -1;

                if (goodfile)
                {
                    DateTime lwt = File.GetLastWriteTime(path);
                    FileItem fi = new FileItem() { Path = path, LastWriteTime = lwt };
                    result.Add(fi);
                }
            }

            FileItem[] resultArray = new FileItem[result.Count];
            result.CopyTo(resultArray);
            return new List<FileItem>(resultArray);
        }

        private List<string> CheckAffectedFolders(List<string> unfiltered)
        {
            List<string> result = new List<string>();

            foreach (string folder in unfiltered)
            {
                string currentFolder = folder;

                if (Directory.Exists(currentFolder)) result.Add(currentFolder);
                else
                {
                    do
                    {
                        currentFolder = Path.GetDirectoryName(currentFolder);
                    }
                    while (!Directory.Exists(currentFolder));

                    if (!result.Contains(currentFolder)) result.Add(currentFolder);
                    ChangeFoldersToParent = true;
                    FoldersReplacedWithParents.Add(new Tuple<string, string>(folder, currentFolder));
                }
            }

            if (!_reduceToParents)
            {
                return result;
            }
            else
            {
                List<string> parentsOnly = new List<string>(result);

                foreach (string folder in result)
                {
                    string currentFolder = folder;
                    if (!currentFolder.EndsWith(Path.DirectorySeparatorChar.ToString())) currentFolder += Path.DirectorySeparatorChar;

                    foreach (string testFolder in result)
                    {
                        if (!parentsOnly.Contains(testFolder)) continue;

                        bool testFolderIsSubfolder = testFolder.IndexOf(currentFolder) != -1 && testFolder.Length > currentFolder.Length;

                        if (testFolderIsSubfolder)
                        {
                            parentsOnly.Remove(testFolder);
                            RemoveSubfolders = true;
                            FoldersRemovedAsSubfolders.Add(new Tuple<string, string>(testFolder, currentFolder));
                        }
                    }
                }

                return parentsOnly;
            }
        }

        void GetFiles(string path)
        {
            try
            {
                foreach (string directory in Directory.GetDirectories(path))
                {
                    foreach (string file in Directory.GetFiles(directory))
                    {
                        _files.Add(file);
                    }
                    GetFiles(directory);
                }
            }
            catch (Exception e)
            {
                Errors.Add(e.Message);
            }
        }

        private void Save()
        {
            XmlExtension.WriteToXmlFile<List<FileItem>>(_baseFile, _base);
        }

        private void SaveAffectedFoldersAsPlainText()
        {
            string[] folders = _affectedFolders.ToArray();
            File.WriteAllLines(_affectedFoldersFile, folders);
        }

        private void SaveDiff(List<FileItem> diff)
        {
            XmlExtension.WriteToXmlFile<List<FileItem>>(_diffFile, diff);
        }

        private void Load()
        {
            _base = XmlExtension.ReadFromXmlFile<List<FileItem>>(_baseFile);
        }

        private List<string> FileItemsToPaths(List<FileItem> items)
        {
            List<string> paths = new List<string>();

            foreach (FileItem item in items)
            {
                paths.Add(item.Path);
            }

            return paths;
        }
    }
}