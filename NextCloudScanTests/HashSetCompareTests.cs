using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace NextCloudScan.Tests
{
    [TestClass()]
    public class ListCompareTests
    {
        List<string> oldList = new List<string>() { "1", "2", "3" };
        List<string> newList = new List<string>() { "1", "2", "3", "4" };
        List<string> newList2 = new List<string>() { "2", "3" };
        List<string> newList3 = new List<string>() { "2", "3", "4" };

        [TestMethod()]
        public void CompareTest()
        {
            ListDiffFinder hsc = new ListDiffFinder();

            hsc.Compare(oldList, newList);
            Assert.AreEqual(1, hsc.AddedCount);

            hsc.Compare(oldList, newList2);
            Assert.AreEqual(1, hsc.RemovedCount);
        }

        [TestMethod()]
        public void AddAndRemoveSameTimeTest()
        {
            ListDiffFinder hsc = new ListDiffFinder();

            hsc.Compare(oldList, newList3);
            Assert.AreEqual(1, hsc.AddedCount);
            Assert.AreEqual(1, hsc.RemovedCount);
        }

        [TestMethod()]
        public void AddedAndRemovedSameTimeTest()
        {
            ListDiffFinder hsc = new ListDiffFinder();

            hsc.Compare(oldList, newList3);
            Assert.AreEqual(1, hsc.AddedCount);
            Assert.AreEqual(true, hsc.Added.Contains("4"));
            Assert.AreEqual(1, hsc.RemovedCount);
            Assert.AreEqual(true, hsc.Removed.Contains("1"));
        }

        [TestMethod()]
        public void EmptyTest()
        {
            ListDiffFinder hsc = new ListDiffFinder();
            hsc.Compare(oldList, newList);
            Assert.AreEqual(true, hsc.RemovedIsEmpty);

            ListDiffFinder hsc2 = new ListDiffFinder();
            hsc.Compare(oldList, newList2);
            Assert.AreEqual(true, hsc2.AddedIsEmpty);
        }
    }
}
