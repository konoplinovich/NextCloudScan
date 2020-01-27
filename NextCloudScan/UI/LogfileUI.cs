using System;
using System.IO;

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
            File.AppendAllText(_logFilePath, $"[{DateTime.Now}] {message}{Environment.NewLine}");
        }
    }
}