using System;
using System.IO;

namespace NextCloudScan
{
    internal class OneProcessLocker
    {
        public bool IsLocked { get { return File.Exists(Lockfile); } }
        public string Lockfile { get; }

        public OneProcessLocker(string lockFile = "~NextCloudScan.exe.lock")
        {
            Lockfile = lockFile;
        }

        public LockResult Lock()
        {
            try
            {
                File.Create(Lockfile);
                return new LockResult() { Result = LockResultType.Successfull, ErrorMessage = null };
            }
            catch (Exception e)
            {
                return new LockResult() { Result = LockResultType.Error, ErrorMessage = e.Message };
            }
        }

        public LockResult Unlock()
        {
            try
            {
                File.Delete(Lockfile);
                return new LockResult() { Result = LockResultType.Successfull, ErrorMessage = null };
            }
            catch (Exception e)
            {
                return new LockResult() { Result = LockResultType.Error, ErrorMessage = e.Message };
            }
        }
    }

    internal class LockResult
    {
        public LockResultType Result {get; set;}
        public string ErrorMessage { get; set; }
    }

    internal enum LockResultType
    {
        Successfull,
        Error
    }
}