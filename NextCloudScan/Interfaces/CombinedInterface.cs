namespace NextCloudScan.Interfaces
{
    public class CombinedInterface : IHumanInterface
    {
        ConsoleInterface _console;
        LogfileInterface _logfile;

        public CombinedInterface(string logFilePath)
        {
            _logfile = new LogfileInterface(logFilePath);
            _console = new ConsoleInterface();
        }

        public void Show(MessageType type, string message)
        {
            _console.Show(type, message);
            _logfile.Show(type, message);
        }
    }
}