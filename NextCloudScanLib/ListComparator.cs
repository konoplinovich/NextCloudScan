using System;
using System.Collections.Generic;

namespace NextCloudScanLib
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
            HashSet<T> oldH = new HashSet<T>(oldList);
            HashSet<T> newH = new HashSet<T>(newList);
            
            foreach (T newItem in newH)
            {
                if (!oldH.Contains(newItem))
                {
                    added.Add(newItem);
                }
            }

            foreach (T oldItem in oldH)
            {
                if (!newH.Contains(oldItem))
                {
                    removed.Add(oldItem);
                }
            }
        }
    }
}
