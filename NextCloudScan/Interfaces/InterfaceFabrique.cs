namespace NextCloudScan.Interfaces
{
    public static class InterfaceFabrique
    {
        public static IHumanInterface GetInterface(InterfaceType type)
        {
            switch (type)
            {
                case InterfaceType.Screen:
                    return new ConsoleInterface();
                case InterfaceType.Logfile:
                    return new LogfileInterface();
                case InterfaceType.Combined:
                    return new CombinedInterface();
                default:
                    break;
            }

            return new ConsoleInterface();
        }
    }
}