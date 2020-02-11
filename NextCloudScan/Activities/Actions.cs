using NextCloudScan.Parsers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace NextCloudScan.Activities
{
    internal class Actions
    {
        private string _action;
        private string _actionOptions;
        private IPathParser _parser;
        private List<string> _rules;
        private List<string> _paths;
        private Progress _progress;

        public Actions(List<string> paths, string action, string actionOptions, IPathParser parser = null, List<string> rules = null, Progress progress = null)
        {
            _paths = paths;
            _action = action;
            _actionOptions = actionOptions;
            _parser = parser;
            _rules = rules;
            _progress = progress;
        }

        public ActionsResult Run()
        {
            DateTime start = DateTime.Now;

            int completeCount = 0;
            int failedCount = 0;

            foreach (string path in _paths)
            {
                string currentPath = path;
                if (_parser != null) currentPath = _parser.Parse(path, _rules);
                string arguments = _actionOptions.Replace("$f", currentPath);
                string executed = $"{_action} {arguments}";

                _progress?.StartupProgress.Report(new StartupProgressResult() { Path = currentPath, Running = executed });

                try
                {
                    ExecuteExternalResult result = ExecuteExternal(_action, arguments, int.MaxValue);

                    if (result.ExitCode == 0)
                    {
                        completeCount++;
                        _progress.LogProgress.Report(new LogProgressResult() { Log = result.Log });
                        _progress.СompletingProgress.Report(new СompletingProgressResult()
                        {
                            HasError = false,
                            Message = $"Action complete, exit code: {result.ExitCode}"
                        });
                    }
                    else
                    {
                        failedCount++;
                        _progress.LogProgress.Report(new LogProgressResult() { Log = result.Log });
                        _progress.СompletingProgress.Report(new СompletingProgressResult()
                        {
                            HasError = true,
                            Message = $"External process error, running: \"{executed}\", exit code: {result.ExitCode}"
                        });
                    }
                }
                catch (Exception e)
                {
                    failedCount++;
                    _progress.СompletingProgress.Report(new СompletingProgressResult()
                    {
                        HasError = true,
                        Message = $"Error starting the process \"{executed}\", message: {e.Message}"
                    });
                }
            }

            DateTime stop = DateTime.Now;
            TimeSpan actionsTime = stop - start;

            return new ActionsResult() { Completed = completeCount, Failed = failedCount, ElapsedTime = actionsTime };
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
}