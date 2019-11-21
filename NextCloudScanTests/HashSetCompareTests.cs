using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextCloudScan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextCloudScan.Tests
{
    [TestClass()]
    public class HashSetCompareTests
    {
        HashSet<string> oldList = new HashSet<string>() { "1", "2", "3" };
        HashSet<string> newList = new HashSet<string>() { "1", "2", "3", "4" };
        HashSet<string> newList2 = new HashSet<string>() { "2", "3" };
        HashSet<string> newList3 = new HashSet<string>() { "2", "3", "4" };

        [TestMethod()]
        public void CompareTest()
        {
            HashSetCompare hsc = new HashSetCompare();

            hsc.Compare(oldList, newList);
            Assert.AreEqual(1, hsc.AddedCount);

            hsc.Compare(oldList, newList2);
            Assert.AreEqual(1, hsc.RemovedCount);
        }

        [TestMethod()]
        public void AddAndRemoveSameTimeTest()
        {
            HashSetCompare hsc = new HashSetCompare();

            hsc.Compare(oldList, newList3);
            Assert.AreEqual(1, hsc.AddedCount);
            Assert.AreEqual(1, hsc.RemovedCount);
        }

        [TestMethod()]
        public void AddedAndRemovedSameTimeTest()
        {
            HashSetCompare hsc = new HashSetCompare();

            hsc.Compare(oldList, newList3);
            Assert.AreEqual(1, hsc.AddedCount);
            Assert.AreEqual(true, hsc.Added.Contains("4"));
            Assert.AreEqual(1, hsc.RemovedCount);
            Assert.AreEqual(true, hsc.Removed.Contains("1"));
        }

        [TestMethod()]
        public void EmptyTest()
        {
            HashSetCompare hsc = new HashSetCompare();
            hsc.Compare(oldList, newList);
            Assert.AreEqual(true, hsc.RemovedIsEmpty);

            HashSetCompare hsc2 = new HashSetCompare();
            hsc.Compare(oldList, newList2);
            Assert.AreEqual(true, hsc2.AddedIsEmpty);
        }
    }
}
