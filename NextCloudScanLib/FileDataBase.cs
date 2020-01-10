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
        string _changedFile;

        List<FileItem> _base;
        List<FileItem> _newFiles;
        List<string> _changedFolders;

        public bool IsNewBase { get; private set; } = false;
        public long Count { get { return _base.Count; } }
        public List<FileItem> Added { get; private set; } = new List<FileItem>();
        public List<FileItem> Removed { get; private set; } = new List<FileItem>();
        public List<string> Changed { get; private set; } = new List<string>();
        public int ChangedCount { get { return _changedFolders.Count; } }

        public FileDataBase(string path, string baseFile = "base.xml", string diffFile = "diff.xml", string changedFile = "changed.log", bool resetBase = false)
        {
            _path = path;
            _baseFile = baseFile;
            _diffFile = diffFile;
            _changedFile = changedFile;

            _base = new List<FileItem>();
            _changedFolders = new List<string>();

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
                diff.AddRange(Removed);
                if (diff.Count != 0)
                {
                    foreach (FileItem item in diff)
                    {
                        string folder = Path.GetDirectoryName(item.Path);
                        if (!_changedFolders.Contains(folder)) _changedFolders.Add(folder);
                    }

                    Changed = _changedFolders;

                    SaveChangedPlainText();
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
        
        private void SaveChangedPlainText()
        {
            string[] folders = _changedFolders.ToArray();
            File.WriteAllLines(_changedFile, folders);
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