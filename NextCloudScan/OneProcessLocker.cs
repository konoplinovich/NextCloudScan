using System;
using System.IO;

namespace NextCloudScan
{
    class OneProcessLocker
    {
        public bool IsLocked { get { return File.Exists(Lockfile); } }
        public string Lockfile { get; }

        public OneProcessLocker(string lockFile = "~NextCloudScan.exe.lock")
        {
            Lockfile = lockFile;
        }

        public Tuple<bool, string> Lock()
        {
            try
            {
                File.Create(Lockfile);
                return new Tuple<bool, string>(true, null);
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, e.Message);
            }
        }

        public Tuple<bool, string> Unlock()
        {
            try
            {
                File.Delete(Lockfile);
                return new Tuple<bool, string>(true, null);
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, e.Message);
            }
        }
    }
}