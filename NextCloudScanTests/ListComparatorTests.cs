using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace NextCloudScan.Tests
{
    [TestClass()]
    public class ListComparatorTests
    {
        List<string> oldList = new List<string>() { "1", "2", "3" };
        List<string> newList = new List<string>() { "1", "2", "3", "4" };
        List<string> newList2 = new List<string>() { "2", "3" };
        List<string> newList3 = new List<string>() { "2", "3", "4" };

        [TestMethod()]
        public void CompareTest()
        {
            ListComparator lc = new ListComparator();

            lc.Compare(oldList, newList);
            Assert.AreEqual(1, lc.AddedCount);

            lc.Compare(oldList, newList2);
            Assert.AreEqual(1, lc.RemovedCount);
        }

        [TestMethod()]
        public void AddAndRemoveSameTimeTest()
        {
            ListComparator lc = new ListComparator();

            lc.Compare(oldList, newList3);
            Assert.AreEqual(1, lc.AddedCount);
            Assert.AreEqual(1, lc.RemovedCount);
        }

        [TestMethod()]
        public void AddedAndRemovedSameTimeTest()
        {
            ListComparator lc = new ListComparator();

            lc.Compare(oldList, newList3);
            Assert.AreEqual(1, lc.AddedCount);
            Assert.AreEqual(true, lc.Added.Contains("4"));
            Assert.AreEqual(1, lc.RemovedCount);
            Assert.AreEqual(true, lc.Removed.Contains("1"));
        }

        [TestMethod()]
        public void EmptyTest()
        {
            ListComparator lc = new ListComparator();
            lc.Compare(oldList, newList);
            Assert.AreEqual(true, lc.RemovedIsEmpty);

            ListComparator lc2 = new ListComparator();
            lc.Compare(oldList, newList2);
            Assert.AreEqual(true, lc2.AddedIsEmpty);
        }
    }
}
