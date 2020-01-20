using FileScanLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NextCloudScan
{
    class Actions
    {
        private string _action;
        private FileDataBase _fdb;

        public Actions(FileDataBase fileDataBase, string action)
        {
            _fdb = fileDataBase;
            _action = action;
        }

        public ActionsResult Run()
        {
            DateTime start = DateTime.Now;

            List<string> errors = new List<string>();
            int completeCount = 0;

            foreach (FileItem item in _fdb.Added)
            {
                try
                {
                    using (Process process = new Process())
                    {
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.FileName = _action;
                        process.StartInfo.Arguments = item.Path;
                        process.StartInfo.CreateNoWindow = false;
                        process.Start();
                        process.WaitForExit();

                        completeCount++;
                    }
                }
                catch (Exception e)
                {
                    errors.Add(e.Message);
                }
            }

            DateTime stop = DateTime.Now;
            TimeSpan actionsTime = stop - start;

            return new ActionsResult() { Completed = completeCount, ElapsedTime = actionsTime, Errors = new List<string>(errors) };
        }
    }
}