using System;

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
            throw new NotImplementedException();
        }
    }
}