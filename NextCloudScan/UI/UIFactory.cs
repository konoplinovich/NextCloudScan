namespace NextCloudScan.UI
{
    internal static class UIFactory
    {
        public static IHumanUI CreateUI(SupportedUI type, string logFilePath = null, bool singleLogFile = true)
        {
            switch (type)
            {
                case SupportedUI.Screen:
                    return new ConsoleUI();
                case SupportedUI.Logfile:
                    return new LogfileUI(logFilePath, singleLogFile);
                case SupportedUI.Combined:
                    return new CombinedUI(logFilePath, singleLogFile);
                default:
                    break;
            }

            return new ConsoleUI();
        }
    }
}