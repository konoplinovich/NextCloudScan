using System;
using System.Collections.Generic;
using System.IO;

namespace FileScanLib
{
    public class FileDataBase
    {
        string _path;
        string _baseFile;
        string _diffFile;
        string _affectedFoldersFile;

        List<FileItem> _base;
        List<FileItem> _newFiles;
        List<string> _affectedFolders;

        public bool IsNewBase { get; private set; } = false;
        public long Count { get { return _base.Count; } }
        public List<FileItem> Added { get; private set; } = new List<FileItem>();
        public List<FileItem> Removed { get; private set; } = new List<FileItem>();
        public List<string> AffectedFolders { get; private set; } = new List<string>();
        public int AffectedFoldersCount { get { return _affectedFolders.Count; } }

        public FileDataBase(string path, string baseFile = "base.xml", string diffFile = "diff.xml", string affectedFoldersFile = "affected_folders.log", bool resetBase = false)
        {
            _path = path;
            _baseFile = baseFile;
            _diffFile = diffFile;
            _affectedFoldersFile = affectedFoldersFile;

            _base = new List<FileItem>();
            _affectedFolders = new List<string>();

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
                _newFiles = Scan();

                ListComparator<FileItem> lc = new ListComparator<FileItem>();
                lc.Compare(_base, _newFiles);

                if (!lc.AddedIsEmpty) Added = lc.Added;
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

                    AffectedFolders = _affectedFolders;

                    SaveAffectedFoldersAsPlainText();
                    SaveDiff(diff);
                }

                _base = _newFiles;
                Save();
            }
        }

        private List<FileItem> Scan()
        {
            List<FileItem> result = new List<FileItem>();

            string[] list = Directory.GetFiles(_path, "*.*", SearchOption.AllDirectories);

            foreach (string path in list)
            {
                bool goodfile =
                    path.IndexOf("files_external") == -1
                    && path.IndexOf("appdata") == -1
                    && path.IndexOf(".htaccess") == -1
                    && path.IndexOf(".ocdata") == -1
                    && path.IndexOf("index.html") == -1
                    && path.IndexOf("nextcloud.log") == -1;

                if (goodfile)
                {
                    DateTime lwt = File.GetLastWriteTime(path);
                    FileItem fi = new FileItem() { Path = path, LastWriteTime = lwt };
                    if (!result.Contains(fi))
                    {
                        result.Add(fi);
                    }
                }
            }

            return result;
        }

        private void Save()
        {
            XmlExtensions.WriteToXmlFile<List<FileItem>>(_baseFile, _base);
        }
        
        private void SaveAffectedFoldersAsPlainText()
        {
            string[] folders = _affectedFolders.ToArray();
            File.WriteAllLines(_affectedFoldersFile, folders);
        }
        
        private void SaveDiff(List<FileItem> diff)
        {
            XmlExtensions.WriteToXmlFile<List<FileItem>>(_diffFile, diff);
        }

        private void Load()
        {
            _base = XmlExtensions.ReadFromXmlFile<List<FileItem>>(_baseFile);
        }
    }
}