using FileScanLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NextCloudScan
{
    internal class FileActions
    {
        private string _fileAction;
        private FileDataBase _fdb;

        public FileActions(FileDataBase fdb, string fileAction)
        {
            _fdb = fdb;
            _fileAction = fileAction;
        }

        public ActionsResult Run()
        {
            DateTime start = DateTime.Now;

            List<string> errors = new List<string>();
            int fileActionCompleteCount = 0;

            foreach (FileItem item in _fdb.Added)
            {
                try
                {
                    using (Process process = new Process())
                    {
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.FileName = _fileAction;
                        process.StartInfo.Arguments = item.Path;
                        process.StartInfo.CreateNoWindow = false;
                        process.Start();
                        process.WaitForExit();

                        fileActionCompleteCount++;
                    }
                }
                catch (Exception e)
                {
                    errors.Add(e.Message);
                }
            }

            DateTime stop = DateTime.Now;
            TimeSpan fileActionsTime = stop - start;

            return new ActionsResult() { Completed = fileActionCompleteCount, ElapsedTime = fileActionsTime, Errors = new List<string>(errors) };
        }
    }
}
