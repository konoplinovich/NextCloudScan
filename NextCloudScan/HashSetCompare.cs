using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextCloudScan
{
    public class HashSetCompare
    {
        HashSet<string> added = new HashSet<string>();
        HashSet<string> removed = new HashSet<string>();

        public int AddedCount { get { return added.Count; } }
        public int RemovedCount { get { return removed.Count; } }
        public HashSet<string> Added { get { return added; } }
        public HashSet<string> Removed { get { return removed; } }
        public bool AddedIsEmpty { get { return added.Count == 0; } }
        public bool RemovedIsEmpty { get { return removed.Count == 0; } }

        public void Compare(HashSet<string> oldList, HashSet<string> newList)
        {
            foreach(string newPath in newList)
            {
                if (!oldList.Contains(newPath))
                {
                    added.Add(newPath);
                }
            }

            foreach (string oldPath in oldList)
            {
                if (!newList.Contains(oldPath))
                {
                    removed.Add(oldPath);
                }
            }
        }
    }
}
