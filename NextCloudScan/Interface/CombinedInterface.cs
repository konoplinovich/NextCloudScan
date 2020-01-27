namespace NextCloudScan.Interface
{
    public class CombinedInterface : IHumanInterface
    {
        ConsoleInterface _console = new ConsoleInterface();
        LogfileInterface _logfile = new LogfileInterface();

        public void Show(MessageType type, string message)
        {
            _console.Show(type, message);
            _logfile.Show(type, message);
        }
    }
}