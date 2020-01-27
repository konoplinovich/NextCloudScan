using System;
using System.IO;
using System.Text;

namespace NextCloudScan.UI
{
    public class LogfileUI : IHumanUI
    {
        string _logFilePath;

        public LogfileUI(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        public void Show(Message type, string message)
        {
            if (string.IsNullOrEmpty(_logFilePath)) return;
            File.AppendAllText(_logFilePath, MakeLogString(type, message));
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