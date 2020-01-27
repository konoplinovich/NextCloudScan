namespace NextCloudScan.Interfaces
{
    public enum MessageType
    {
        NewFile,
        RemovedFile,
        AffectedFolder,
        Start,
        Config,
        Info,
        Error,
        Warning
    }
}