using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using NextCloudScan.Parsers;

namespace NextCloudScan
{
    internal class Actions
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

            Dictionary<string, List<string>> log = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();
            int completeCount = 0;

            foreach (string path in _paths)
            {
                string currentPath = path;
                if (_parser != null) currentPath = _parser.Parse(path, _rules);
                string arguments = _actionOptions.Replace("$f", currentPath);

                if (!log.ContainsKey(currentPath)) log.Add(currentPath, new List<string>());

                try
                {
                    ExecuteExternalResult result = ExecuteExternal(_action, arguments, int.MaxValue);

                    if (result.ExitCode == 0)
                    {
                        log[currentPath].Add(result.Log);
                        completeCount++;
                    }
                    else
                    {
                        if (!errors.ContainsKey(currentPath)) errors.Add(currentPath, new List<string>());
                        errors[currentPath].Add($"External process error, process: {_action}, exit code: {result.ExitCode}");
                    }
                }
                catch (Exception e)
                {
                    if (!errors.ContainsKey(currentPath)) errors.Add(currentPath, new List<string>());
                    errors[currentPath].Add($"External process error, process: {_action}, message: {e.Message}");
                }
            }

            DateTime stop = DateTime.Now;
            TimeSpan actionsTime = stop - start;

            return new ActionsResult() { Completed = completeCount, ElapsedTime = actionsTime, Errors = errors, Log = log };
        }

        private static ExecuteExternalResult ExecuteExternal(string fileName, string args, int timeout)
        {
            StringBuilder log = new StringBuilder();
            int exitCode;

            using (Process process = new Process())
            {
                process.StartInfo.FileName = fileName;
                process.StartInfo.Arguments = args;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            outputWaitHandle.Set();
                        }
                        else
                        {
                            log.AppendLine(e.Data);
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            errorWaitHandle.Set();
                        }
                        else
                        {
                            log.AppendLine(e.Data);
                        }
                    };

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (timeout > 0)
                    {
                        if (process.WaitForExit(timeout) &&
                            outputWaitHandle.WaitOne(timeout) &&
                            errorWaitHandle.WaitOne(timeout))
                        {

                        }
                    }
                    else
                    {
                        process.WaitForExit();
                        outputWaitHandle.WaitOne();
                        errorWaitHandle.WaitOne();

                    }
                }

                exitCode = process.ExitCode;
            }

            return new ExecuteExternalResult() { Log = log.ToString(), ExitCode = exitCode };
        }
    }

    internal class ExecuteExternalResult
    {
        public string Log { get; set; }
        public int ExitCode { get; set; }
    }
}