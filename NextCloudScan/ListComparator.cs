using System.Collections.Generic;

namespace NextCloudScan
{
    public class ListComparator
    {
        List<string> added = new List<string>();
        List<string> removed = new List<string>();

        public int AddedCount { get { return added.Count; } }
        public int RemovedCount { get { return removed.Count; } }
        public List<string> Added { get { return added; } }
        public List<string> Removed { get { return removed; } }
        public bool AddedIsEmpty { get { return added.Count == 0; } }
        public bool RemovedIsEmpty { get { return removed.Count == 0; } }

        public void Compare(List<string> oldList, List<string> newList)
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
