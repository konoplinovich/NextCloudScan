using System.IO;
using System.Collections.Generic;

namespace NextCloudScan
{
    public class DataBase
    {
        string _baseFile = "base.dat";
        string _diffFile = "diff.dat";
        string _path;
        List<string> _base;
        List<string> _newFiles;

        public bool IsNewBase { get; private set; } = false;
        public long Count { get { return _base.Count; } }
        public List<string> Added { get; private set; } = new List<string>();
        public List<string> Removed { get; private set; } = new List<string>();

        public DataBase(string path, bool resetBase = false)
        {
            _path = path;
            _base = new List<string>();

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

                ListComparator lc = new ListComparator();
                lc.Compare(_base, _newFiles);

                if (!lc.AddedIsEmpty) Added = lc.Added;
                if (!lc.RemovedIsEmpty) Removed = lc.Removed;

                List<string> total = new List<string>(Added);
                total.AddRange(Removed);
                if (total.Count != 0) SaveDiff(total);

                _base = _newFiles;
                Save();
            }
        }

        private List<string> Scan()
        {
            List<string> result = new List<string>();

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
        
        private void SaveDiff(List<string> total)
        {
            File.WriteAllLines(_diffFile, total);
        }

        private void Load()
        {
            string[] files = File.ReadAllLines(_baseFile);
            _base = new List<string>(files);
        }
    }
}