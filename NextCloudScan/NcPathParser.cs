using System.Collections.Generic;

namespace NextCloudScan
{
    public class NcPathParser : IPathParser
    {
        public string Parse(string path, List<string> rules)
        {
            string ncDataRoot = rules[0];

            if (ncDataRoot.EndsWith(@"\") || ncDataRoot.EndsWith(@"/")) 
            { 
                ncDataRoot = ncDataRoot.TrimEnd(new char[] { '\\', '/' });
            };

            string userPath = path.Replace(ncDataRoot, "");
            return userPath;
        }
    }
}