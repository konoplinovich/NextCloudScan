using System;
using System.IO;
using System.Text;

namespace NextCloudScan.UI
{
    internal class LogfileUI : IHumanUI
    {
        string _logFilePath;
        int _ageLimit;

        public LogfileUI(string logFilePath, bool singleLogFile = true, int ageLimit = 1)
        {
            _ageLimit = ageLimit;

            if (singleLogFile) { _logFilePath = logFilePath; }
            else
            {
                string directory = (new FileInfo(logFilePath)).DirectoryName;
                string filename = Path.GetFileNameWithoutExtension(logFilePath);
                string ext = Path.GetExtension(logFilePath);
                string time = DateTime.Now.ToString("_ddMMyyyy_HHmmss");

                filename += time + ext;
                _logFilePath = Path.Combine(directory, filename);

                ClearOldLogs(directory, ext);
            }
        }

        public void Show(Message type, string message)
        {
            if (string.IsNullOrEmpty(_logFilePath)) return;
            File.AppendAllText(_logFilePath, MakeLogString(type, message));
        }

        private void ClearOldLogs(string directory, string ext)
        {
            string[] files = Directory.GetFiles(directory, $"*{ext}");
            DateTime now = DateTime.Now;

            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                DateTime lwt = fi.LastWriteTime;

                TimeSpan age = now - lwt;
                if (age > TimeSpan.FromHours(_ageLimit))
                {
                    File.Delete(file);
                }
            }
        }

        private string MakeLogString(Message type, string message)
        {
            StringBuilder result = new StringBuilder();
            result.Append($"[{DateTime.Now}] ");

            switch (type)
            {
                case Message.None:
                    break;
                case Message.NewFile:
                    result.Append("[+] ");
                    break;
                case Message.RemovedFile:
                    result.Append("[-] ");
                    break;
                case Message.AffectedFolder:
                    result.Append("[Affected] ");
                    break;
                case Message.Start:
                    result.Append("[=>] ");
                    break;
                case Message.Stop:
                    result.Append("[x] ");
                    break;
                case Message.Config:
                    result.Append("[Config] ");
                    break;
                case Message.Info:
                    result.Append("[Info] ");
                    break;
                case Message.Error:
                    result.Append("[Error] ");
                    break;
                case Message.Warning:
                    result.Append("[Warning] ");
                    break;
                case Message.External:
                    result.Append("   | ");
                    break;
                default:
                    break;
            }

            result.Append(message);
            result.Append(Environment.NewLine);

            return result.ToString();
        }
    }
}