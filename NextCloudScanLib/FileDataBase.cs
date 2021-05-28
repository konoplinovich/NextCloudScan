using Extensions;
using FsTree;
using System;
using System.Collections.Generic;
using System.IO;

namespace NextCloudScan.Lib
{
    public class FileDataBase
    {
        string _path;
        string _baseFile;
        string _diffFile;
        string _affectedFoldersFile;
        bool _reduceToParents;
        List<string> _filters = new List<string>()
        {
            "files_external",
            "files_trashbin",
            "files_versions",
            "uploads",
            "cache",
            "appdata",
            ".htaccess",
            ".ocdata",
            "index.html",
            "nextcloud.log"
        };

        Tree _base;
        Tree _newBase;
        List<string> _affectedFolders;
        private HashSet<string> _files;

        public bool IsNewBase { get; private set; } = false;
        public long ItemsCount { get { return _base.ItemsCount; } }
        public long FilesCount { get { return _base.FilesCount; } }
        public long FoldersCount { get { return _base.FoldersCount; } }
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

        public FileDataBase(string path, string baseFile = "base.xml", string diffFile = "diff.xml", string affectedFoldersFile = "affected_folders.log", bool resetBase = false, bool reduceToParents = false)
        {
            _path = path;
            _baseFile = baseFile;
            _diffFile = diffFile;
            _affectedFoldersFile = affectedFoldersFile;
            _reduceToParents = reduceToParents;

            _base = new Tree();
            _affectedFolders = new List<string>();
            _files = new HashSet<string>();

            if (!File.Exists(_baseFile) || resetBase)
            {
                _base = Scan();
                Save();
                IsNewBase = true;
            }
            else
            {
                if (File.Exists(_diffFile)) File.Delete(_diffFile);

                Load();
                _newBase = Scan();

                DiffResult result = _base.DiffWith(_newBase);

                foreach (var item in result.AddedFiles)
                {
                    FileItem file = new FileItem() { Path = item.FullName, LastWriteTime = item.LWT };

                    Added.Add(file);
                    AddedPath.Add(file.Path);
                }

                foreach (var item in result.RemovedFiles)
                {
                    FileItem file = new FileItem() { Path = item.FullName, LastWriteTime = item.LWT };

                    Removed.Add(file);

                }

                List<FileItem> diff = new List<FileItem>();
                diff.AddRange(Added);
                diff.AddRange(Removed);

                AffectedFolders = CheckAffectedFolders(new List<string>(result.AffectedFolders));
                SaveAffectedFoldersAsPlainText();
                SaveDiff(diff);

                _base = _newBase;
                Save();
            }
        }

        private Tree Scan()
        {
            Tree tree = new Tree(_path, _filters);
            tree.GetTree();

            return tree;
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
            _base.Save(_baseFile);
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
            _base = new Tree();
            _base.Load(_baseFile);
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