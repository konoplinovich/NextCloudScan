using FileScanLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NextCloudScan
{
    class Actions
    {
        private string _action;
        private string _actionOptions;
        private IPathParser _parser;
        private List<string> _rules;
        private List<string> _paths;

        public Actions(List<string> paths, string action, string actionOptions, IPathParser parser = null, List<string> rules = null)
        {
            _paths = paths;
            _action = action;
            _actionOptions = actionOptions;
            _parser = parser;
            _rules = rules;
        }

        public ActionsResult Run()
        {
            DateTime start = DateTime.Now;

            List<string> errors = new List<string>();
            int completeCount = 0;

            foreach (string path in _paths)
            {
                string currentPath = path;
                if (_parser != null) currentPath = _parser.Parse(path, _rules);
                string arguments = _actionOptions.Replace("$f", currentPath);

                try
                {
                    using (Process process = new Process())
                    {
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.FileName = _action;
                        process.StartInfo.Arguments = arguments;
                        process.StartInfo.CreateNoWindow = false;
                        process.Start();
                        process.WaitForExit();

                        if (process.ExitCode != 0)
                        {
                            errors.Add($"Internal process error, exit code: {process.ExitCode}");
                        }
                        else
                        {
                            completeCount++;
                        }
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