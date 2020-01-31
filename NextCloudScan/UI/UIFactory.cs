namespace NextCloudScan.UI
{
    internal static class UIFactory
    {
        public static IHumanUI CreateUI(SupportedUI type, string logFilePath = null)
        {
            switch (type)
            {
                case SupportedUI.Screen:
                    return new ConsoleUI();
                case SupportedUI.Logfile:
                    return new LogfileUI(logFilePath); ;
                case SupportedUI.Combined:
                    return new CombinedUI(logFilePath);
                default:
                    break;
            }

            return new ConsoleUI();
        }
    }
}