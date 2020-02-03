using System;
using System.IO;

namespace NextCloudScan.Lock
{
    internal class OneProcessLocker
    {
        public bool IsLocked { get { return File.Exists(Lockfile); } }
        public string Lockfile { get; }

        public OneProcessLocker(string lockFile = "~NextCloudScan.exe.lock")
        {
            Lockfile = lockFile;
        }

        public LockResult Lock(int lockLifeTime = 10)
        {
            try
            {
                if (File.Exists(Lockfile))
                {
                    FileInfo fi = new FileInfo(Lockfile);
                    DateTime lwt = fi.LastWriteTime;

                    TimeSpan age = DateTime.Now - lwt;

                    if (age <= TimeSpan.FromMinutes(lockLifeTime))
                    {
                        return new LockResult() { Result = LockResultType.AlreadyLocked, ErrorMessage = null };
                    }
                    else
                    {
                        File.Delete(Lockfile);
                        File.Create(Lockfile);
                        return new LockResult() { Result = LockResultType.DeleteOldLock, ErrorMessage = null };
                    }
                }
                
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
}