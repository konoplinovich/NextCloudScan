namespace NextCloudScan.Interfaces
{
    public static class InterfaceFabrique
    {
        public static IHumanInterface GetInterface(InterfaceType type, string logFilePath = null)
        {
            switch (type)
            {
                case InterfaceType.Screen:
                    return new ConsoleInterface();
                case InterfaceType.Logfile:
                    return new LogfileInterface(logFilePath); ;
                case InterfaceType.Combined:
                    return new CombinedInterface(logFilePath);
                default:
                    break;
            }

            return new ConsoleInterface();
        }
    }
}