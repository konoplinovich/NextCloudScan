using System.Collections.Generic;
using System.IO;

namespace NextCloudScan
{
    public class DataBase
    {
        string _baseFile = "base.dat";
        string _diffFile = "diff.dat";
        string _path;
        HashSet<string> _base;
        HashSet<string> _newFiles;

        public bool IsNewBase { get; private set; } = false;
        public long Count { get { return _base.Count; } }
        public HashSet<string> Added { get; private set; } = new HashSet<string>();
        public HashSet<string> Removed { get; private set; } = new HashSet<string>();

        public DataBase(string path, bool resetBase = false)
        {
            _path = path;
            _base = new HashSet<string>();

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

                HashSetCompare hsc = new HashSetCompare();
                hsc.Compare(_base, _newFiles);

                if (!hsc.AddedIsEmpty) Added = hsc.Added;
                if (!hsc.RemovedIsEmpty) Removed = hsc.Removed;

                HashSet<string> total = new HashSet<string>(Added);
                total.UnionWith(Removed);
                if (total.Count != 0) SaveDiff(total);

                _base = _newFiles;
                Save();
            }
        }

        private HashSet<string> Scan()
        {
            HashSet<string> result = new HashSet<string>();

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
                    if (!result.Contains(path)) result.Add(path);
                }
            }

            return result;
        }

        private void Save()
        {
            File.WriteAllLines(_baseFile, _base);
        }
        private void SaveDiff(HashSet<string> total)
        {
            File.WriteAllLines(_diffFile, total);
        }

        private void Load()
        {
            string[] files = File.ReadAllLines(_baseFile);
            _base = new HashSet<string>(files);
        }
    }
}