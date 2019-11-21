using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace NextCloudScan.Tests
{
    [TestClass()]
    public class ListComparatorTests
    {
        readonly List<string> _start = new List<string>() { "1", "2", "3" };
        readonly List<string> _addOne = new List<string>() { "1", "2", "3", "4" };
        readonly List<string> _removeOne = new List<string>() { "2", "3" };
        readonly List<string> _removeOneAndAddOne = new List<string>() { "2", "3", "4" };

        [TestMethod()]
        public void CompareTest()
        {
            ListComparator lc = new ListComparator();

            lc.Compare(_start, _addOne);
            Assert.AreEqual(1, lc.AddedCount);

            lc.Compare(_start, _removeOne);
            Assert.AreEqual(1, lc.RemovedCount);
        }

        [TestMethod()]
        public void AddAndRemoveSameTimeTest()
        {
            ListComparator lc = new ListComparator();

            lc.Compare(_start, _removeOneAndAddOne);
            Assert.AreEqual(1, lc.AddedCount);
            Assert.AreEqual(1, lc.RemovedCount);
        }

        [TestMethod()]
        public void AddedAndRemovedSameTimeTest()
        {
            ListComparator lc = new ListComparator();

            lc.Compare(_start, _removeOneAndAddOne);
            Assert.AreEqual(true, lc.Added.Contains("4"));
            Assert.AreEqual(true, lc.Removed.Contains("1"));
        }

        [TestMethod()]
        public void EmptyTest()
        {
            ListComparator lc = new ListComparator();
            lc.Compare(_start, _addOne);
            Assert.AreEqual(true, lc.RemovedIsEmpty);

            ListComparator lc2 = new ListComparator();
            lc.Compare(_start, _removeOne);
            Assert.AreEqual(true, lc2.AddedIsEmpty);
        }
    }
}
