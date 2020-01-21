using System.Collections.Generic;

namespace NextCloudScan
{
    public interface IPathParser
    {
        string Parse(string path, List<string> rules);
    }
}