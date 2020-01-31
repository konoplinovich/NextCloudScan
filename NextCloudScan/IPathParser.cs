using System.Collections.Generic;

namespace NextCloudScan
{
    internal interface IPathParser
    {
        string Parse(string path, List<string> rules);
    }
}