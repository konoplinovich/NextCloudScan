using System.Xml;
using System.Xml.Serialization;

namespace NextCloudScan
{
    public sealed class NcsConfig
    {
        [XmlAnyElement("PathComment")] 
        public XmlComment PathComment { get { return new XmlDocument().CreateComment("root folder for scanning"); } set { } }
        public string Path { get; set; } = "root";
        
        [XmlAnyElement("BasePathComment")] 
        public XmlComment BasePathComment { get { return new XmlDocument().CreateComment("folder for NCS service files"); } set { } }
        public string BasePath { get; set; } = "base";
        
        [XmlAnyElement("FileActionAppComment")] 
        public XmlComment FileActionAppComment { get { return new XmlDocument().CreateComment("file processing application"); } set { } }
        public string FileActionApp { get; set; } = "cmd.exe";
        
        [XmlAnyElement("FileActionAppOptionsComment")] 
        public XmlComment FileActionAppOptionsComment { get { return new XmlDocument().CreateComment("application arguments ($f will be replaced with the file name)"); } set { } }
        public string FileActionAppOptions { get; set; } = "$f";
        
        [XmlAnyElement("FolderActionAppComment")] 
        public XmlComment FolderActionAppComment { get { return new XmlDocument().CreateComment("file processing application"); } set { } }
        public string FolderActionApp { get; set; } = "cmd.exe";
        
        [XmlAnyElement("FolderActionAppOptionsComment")] 
        public XmlComment FolderActionAppOptionsComment { get { return new XmlDocument().CreateComment("application arguments ($f will be replaced with the folder name)"); } set { } }
        public string FolderActionAppOptions { get; set; } = "$f";

        [XmlAnyElement("InterfaceComment")]
        public XmlComment InterfaceComment { get { return new XmlDocument().CreateComment("application interface (screen, logfile or combined)"); } set { } }
        public NextCloudScan.UI.SupportedUI Interface { get; set; } = NextCloudScan.UI.SupportedUI.Screen;

        [XmlAnyElement("ShowConfigParametersOnStartComment")]
        public XmlComment ShowConfigParametersOnStartComment { get { return new XmlDocument().CreateComment("show config parameters on start"); } set { } }
        public bool ShowConfigParametersOnStart { get; set; } = true;

        [XmlAnyElement("ShowFileDetailsComment")]
        public XmlComment ShowFileDetailsComment { get { return new XmlDocument().CreateComment("show file details"); } set { } }
        public bool ShowFileDetails { get; set; } = false;

        [XmlAnyElement("IsNextCloudComment")]
        public XmlComment IsNextCloudComment { get { return new XmlDocument().CreateComment("if root folder is NextCloud data folder, must be true"); } set { } }
        public bool IsNextCloud { get; set; } = false;

        [XmlAnyElement("ReduceToParentsComment")]
        public XmlComment ReduceToParentsComment { get { return new XmlDocument().CreateComment("remove all subfolders, use parent folders only"); } set { } }
        public bool ReduceToParents { get; set; } = false;

        [XmlAnyElement("SingleLogFileComment")]
        public XmlComment SingleLogFileComment { get { return new XmlDocument().CreateComment("use one logfile for all sessions or separate logfile for each session"); } set { } }
        public bool SingleLogFile { get; set; } = true;

        [XmlAnyElement("LogFilesAgeLimitComment")]
        public XmlComment LogFilesAgeLimitComment { get { return new XmlDocument().CreateComment("log files age limit (in hours)"); } set { } }
        public int LogFilesAgeLimit { get; set; } = 2;

        [XmlAnyElement("OneProcessAtATimeComment")]
        public XmlComment OneProcessAtATimeComment { get { return new XmlDocument().CreateComment("one process at a time"); } set { } }
        public bool OneProcessAtATime { get; set; } = true;

        [XmlAnyElement("LockLifeTimeComment")]
        public XmlComment LockLifeTimeComment { get { return new XmlDocument().CreateComment("oldest and unused locks life time (in minutes)"); } set { } }
        public int LockLifeTime { get; set; } = 10;

        public NcsConfig() { }
    }
}