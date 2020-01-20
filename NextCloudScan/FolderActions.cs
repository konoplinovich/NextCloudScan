using FileScanLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NextCloudScan
{
    internal class FolderActions
    {
        private string _folderAction;
        private FileDataBase _fdb;

        public FolderActions(FileDataBase fdb, string folderAction)
        {
            _fdb = fdb;
            _folderAction = folderAction;
        }

        public ActionsResult Run()
        {
            DateTime start = DateTime.Now;

            List<string> errors = new List<string>();
            int folderActionCompleteCount = 0;

            foreach (string folder in _fdb.AffectedFolders)
            {
                try
                {
                    using (Process process = new Process())
                    {
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.FileName = _folderAction;
                        process.StartInfo.CreateNoWindow = false;
                        process.StartInfo.Arguments = folder;
                        process.Start();
                        process.WaitForExit();

                        folderActionCompleteCount++;
                    }
                }
                catch (Exception e)
                {
                    errors.Add(e.Message);
                }
            }

            DateTime stop = DateTime.Now;
            TimeSpan folderActionsTime = stop - start;

            return new ActionsResult() { Completed = folderActionCompleteCount, ElapsedTime = folderActionsTime, Errors = new List<string>(errors) };
        }
    }
}
