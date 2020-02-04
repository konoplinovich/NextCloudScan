namespace NextCloudScan.UI
{
    internal static class UIFactory
    {
        public static IHumanUI CreateUI(SupportedUI type, string logFilePath = null, bool singleLogFile = true, int ageLimit = 1)
        {
            switch (type)
            {
                case SupportedUI.Screen:
                    return new ConsoleUI();
                case SupportedUI.Logfile:
                    return new LogfileUI(logFilePath, singleLogFile, ageLimit);
                case SupportedUI.Combined:
                    return new CombinedUI(logFilePath, singleLogFile, ageLimit);
                default:
                    break;
            }

            return new ConsoleUI();
        }
    }
}