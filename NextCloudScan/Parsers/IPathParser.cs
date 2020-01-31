using System.Collections.Generic;

namespace NextCloudScan.Parsers
{
    internal interface IPathParser
    {
        string Parse(string path, List<string> rules);
    }
}