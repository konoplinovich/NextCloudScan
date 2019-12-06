using System;
using System.Collections.Generic;

namespace FileScanLib
{
    public class ListComparator<T> where T : IEquatable<T>
    {
        List<T> added = new List<T>();
        List<T> removed = new List<T>();

        public int AddedCount { get { return added.Count; } }
        public int RemovedCount { get { return removed.Count; } }
        public List<T> Added { get { return added; } }
        public List<T> Removed { get { return removed; } }
        public bool AddedIsEmpty { get { return added.Count == 0; } }
        public bool RemovedIsEmpty { get { return removed.Count == 0; } }

        public void Compare(List<T> oldList, List<T> newList)
        {
            foreach (T newPath in newList)
            {
                if (!oldList.Contains(newPath))
                {
                    added.Add(newPath);
                }
            }

            foreach (T oldPath in oldList)
            {
                if (!newList.Contains(oldPath))
                {
                    removed.Add(oldPath);
                }
            }
        }
    }
}