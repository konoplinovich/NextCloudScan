namespace NextCloudScan.UI
{
    public class CombinedUI : IHumanUI
    {
        ConsoleUI _console;
        LogfileUI _logfile;

        public CombinedUI(string logFilePath)
        {
            _logfile = new LogfileUI(logFilePath);
            _console = new ConsoleUI();
        }

        public void Show(Message type, string message)
        {
            _console.Show(type, message);
            _logfile.Show(type, message);
        }
    }
}