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
            foreach (T newItem in newList)
            {
                if (!oldList.Contains(newItem))
                {
                    added.Add(newItem);
                }
            }

            foreach (T oldItem in oldList)
            {
                if (!newList.Contains(oldItem))
                {
                    removed.Add(oldItem);
                }
            }
        }
    }
}