using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;


namespace NextCloudScanLib.Tests
{
    [TestClass()]
    public class ListComparatorTests
    {
        readonly List<FileItem> _start = new List<FileItem>()
        {
            new FileItem(){Path = "1", LastWriteTime = new DateTime(2019,5,2,12,15,5)},
            new FileItem(){Path = "2", LastWriteTime = new DateTime(2019,5,2,12,15,5)},
            new FileItem(){Path = "3", LastWriteTime = new DateTime(2019,5,2,12,15,5)}
        };
        readonly List<FileItem> _addOne = new List<FileItem>()
        {
            new FileItem(){Path = "1", LastWriteTime = new DateTime(2019,5,2,12,15,5)},
            new FileItem(){Path = "2", LastWriteTime = new DateTime(2019,5,2,12,15,5)},
            new FileItem(){Path = "3", LastWriteTime = new DateTime(2019,5,2,12,15,5)},
            new FileItem(){Path = "4", LastWriteTime = new DateTime(2019,5,2,12,15,5)},
        };
        readonly List<FileItem> _removeOne = new List<FileItem>()
        {
            new FileItem(){Path = "2", LastWriteTime = new DateTime(2019,5,2,12,15,5)},
            new FileItem(){Path = "3", LastWriteTime = new DateTime(2019,5,2,12,15,5)}
        };
        readonly List<FileItem> _removeOneAndAddOne = new List<FileItem>()
        {
            new FileItem(){Path = "2", LastWriteTime = new DateTime(2019,5,2,12,15,5)},
            new FileItem(){Path = "3", LastWriteTime = new DateTime(2019,5,2,12,15,5)},
            new FileItem(){Path = "4", LastWriteTime = new DateTime(2019,5,2,12,15,5)}
        };

        [TestMethod()]
        public void CompareTest()
        {
            ListComparator<FileItem> lc = new ListComparator<FileItem>();

            lc.Compare(_start, _addOne);
            Assert.AreEqual(1, lc.AddedCount);

            lc.Compare(_start, _removeOne);
            Assert.AreEqual(1, lc.RemovedCount);
        }

        [TestMethod()]
        public void AddAndRemoveSameTimeTest()
        {
            ListComparator<FileItem> lc = new ListComparator<FileItem>();

            lc.Compare(_start, _removeOneAndAddOne);
            Assert.AreEqual(1, lc.AddedCount);
            Assert.AreEqual(1, lc.RemovedCount);
        }

        [TestMethod()]
        public void AddedAndRemovedSameTimeTest()
        {
            ListComparator<FileItem> lc = new ListComparator<FileItem>();

            lc.Compare(_start, _removeOneAndAddOne);
            Assert.AreEqual(true, lc.Added.Contains(new FileItem() { Path = "4", LastWriteTime = new DateTime(2019, 5, 2, 12, 15, 5) }));
            Assert.AreEqual(true, lc.Removed.Contains(new FileItem() { Path = "1", LastWriteTime = new DateTime(2019, 5, 2, 12, 15, 5) }));
        }

        [TestMethod()]
        public void EmptyTest()
        {
            ListComparator<FileItem> lc = new ListComparator<FileItem>();
            lc.Compare(_start, _addOne);
            Assert.AreEqual(true, lc.RemovedIsEmpty);

            ListComparator<FileItem> lc2 = new ListComparator<FileItem>();
            lc.Compare(_start, _removeOne);
            Assert.AreEqual(true, lc2.AddedIsEmpty);
        }
    }
}