using System;
using System.IO;

namespace NextCloudScan.Interfaces
{
    public class LogfileInterface : IHumanInterface
    {
        string _logFilePath;

        public LogfileInterface(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        public void Show(MessageType type, string message)
        {
            if (string.IsNullOrEmpty(_logFilePath)) return;
            File.AppendAllText(_logFilePath, $"[{DateTime.Now}] {message}{Environment.NewLine}");
        }
    }
}