using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace NextCloudScan.Lib.Tests
{
    [TestClass()]
    public class FileDataBaseTests
    {
        FileDataBaseOptions _optionsReset;
        FileDataBaseOptions _options;
        string _tempFolder;
        readonly Dictionary<string, string[]> _map = new Dictionary<string, string[]>()
        {
            { "one", new string[]{"1","2"} },
            { "two", new string[]{"1","2"} },
            { "three", new string[]{"1","2"} },
            { "four", new string[]{"1","2"} },
            { "five", new string[]{"1","2"} },
            { "six", new string[]{"1","2"} },
        };

        

        string[] Files;

        [TestInitialize()]
        public void Startup()
        {
            _tempFolder = Path.Combine(Path.GetTempPath(), "NCSTemp");
            _optionsReset = new FileDataBaseOptions(_tempFolder, resetBase: true);
            _options = new FileDataBaseOptions(_tempFolder);

            Directory.CreateDirectory(_tempFolder);

            foreach (string L1Folder in _map.Keys)
            {
                string[] value = _map[L1Folder];
                foreach (string L2Folder in value)
                {
                    string path = Path.Combine(_tempFolder, L1Folder, L2Folder);
                    Directory.CreateDirectory(path);
                    CreateTempFiles(path);
                }
            }

            Files = new string[]
            {
                Path.Combine(_tempFolder, "one", "1", "file_0.tmp"),
                Path.Combine(_tempFolder, "three", "2", "file_5.tmp"),
                Path.Combine(_tempFolder, "five", "1", "file_9.tmp"),
                Path.Combine(_tempFolder, "four", "2", "file_105.tmp"),
                Path.Combine(_tempFolder, "one", "1", "file_3.tmp"),
                Path.Combine(_tempFolder, "three", "2", "file_1.tmp")
            };
        }

        [TestCleanup()]
        public void CleanUp()
        {
            if (Directory.Exists(_tempFolder))
            {
                Directory.Delete(_tempFolder, true);
            }
        }

        [TestMethod()]
        public void IsNewBaseTest()
        {
            FileDataBase dataBase = new FileDataBase(_optionsReset);
            dataBase.Refresh();

            Assert.AreEqual(true, dataBase.IsNewBase);

            dataBase = new FileDataBase(_options);
            dataBase.Refresh();

            Assert.AreEqual(false, dataBase.IsNewBase);
        }

        [TestMethod()]
        public void RemoveOneFileTest()
        {
            FileDataBase dataBase = new FileDataBase(_optionsReset);
            dataBase.Refresh();

            File.Delete(Files[0]);
            dataBase = new FileDataBase(_options);
            dataBase.Refresh();

            Assert.AreEqual(1, dataBase.Removed.Count);
        }

        [TestMethod()]
        public void AddOneFileTest()
        {
            FileDataBase dataBase = new FileDataBase(_optionsReset);
            dataBase.Refresh();

            FileStream f = File.Open(Path.Combine(_tempFolder, "three", "2", "file_xxx.tmp"), FileMode.Create);
            f.Flush();
            f.Close();

            dataBase = new FileDataBase(_options);
            dataBase.Refresh();

            Assert.AreEqual(1, dataBase.Added.Count);
        }

        [TestMethod()]
        public void AddAndRemoveTest()
        {
            FileDataBase dataBase = new FileDataBase(_optionsReset);
            dataBase.Refresh();

            File.Delete(Files[0]);
            File.Delete(Files[1]);
            File.Delete(Files[2]);
            FileStream f = File.Open(Path.Combine(_tempFolder, "three", "2", "file_xxx.tmp"), FileMode.Create);
            f.Flush();
            f.Close();

            dataBase = new FileDataBase(_options);
            dataBase.Refresh();

            Assert.AreEqual(3, dataBase.Removed.Count);
            Assert.AreEqual(1, dataBase.Added.Count);
        }

        [TestMethod()]
        public void AddedFileNameTest()
        {
            FileDataBase dataBase = new FileDataBase(_optionsReset);
            dataBase.Refresh();

            FileStream f = File.Open(Files[3], FileMode.Create);
            f.Flush();
            f.Close();

            DateTime lwt = File.GetLastWriteTime(Files[3]);

            dataBase = new FileDataBase(_options);
            dataBase.Refresh();

            Assert.AreEqual(true, dataBase.Added.Contains(new FileItem() { Path = Files[3], LastWriteTime = lwt }));
        }

        [TestMethod()]
        public void RemovedFileNameTest()
        {
            FileDataBase dataBase = new FileDataBase(_optionsReset);
            dataBase.Refresh();

            DateTime lwt0 = File.GetLastWriteTime(Files[0]); File.Delete(Files[0]);
            DateTime lwt1 = File.GetLastWriteTime(Files[1]); File.Delete(Files[1]);
            DateTime lwt2 = File.GetLastWriteTime(Files[2]); File.Delete(Files[2]);

            dataBase = new FileDataBase(_options);
            dataBase.Refresh();

            Assert.AreEqual(true, dataBase.Removed.Contains(new FileItem() { Path = Files[0], LastWriteTime = lwt0 }));
            Assert.AreEqual(true, dataBase.Removed.Contains(new FileItem() { Path = Files[1], LastWriteTime = lwt1 }));
            Assert.AreEqual(true, dataBase.Removed.Contains(new FileItem() { Path = Files[2], LastWriteTime = lwt2 }));
        }

        [TestMethod()]
        public void ChangeLWTTest()
        {
            FileDataBase dataBase = new FileDataBase(_optionsReset);
            dataBase.Refresh();

            DateTime lwt_old = File.GetLastWriteTime(Files[0]);
            File.AppendAllText(Files[0], "TEST");
            DateTime lwt_new = File.GetLastWriteTime(Files[0]);

            dataBase = new FileDataBase(_options);
            dataBase.Refresh();

            Assert.AreEqual(true, dataBase.Added.Contains(new FileItem() { Path = Files[0], LastWriteTime = lwt_new }));
            Assert.AreEqual(true, dataBase.Removed.Contains(new FileItem() { Path = Files[0], LastWriteTime = lwt_old }));
        }

        [TestMethod()]
        public void AffectedFoldersCountTest()
        {
            FileDataBase dataBase = new FileDataBase(_optionsReset);
            dataBase.Refresh();

            File.AppendAllText(Files[0], "TEST");
            File.AppendAllText(Files[1], "TEST");
            File.AppendAllText(Files[4], "TEST");
            File.AppendAllText(Files[5], "TEST");

            dataBase = new FileDataBase(_options);
            dataBase.Refresh();

            Assert.AreEqual(2, dataBase.AffectedFoldersCount);
        }

        [TestMethod()]
        public void AffectedFoldersAddOnlyCountTest()
        {
            FileDataBase dataBase = new FileDataBase(_optionsReset);
            dataBase.Refresh();

            FileStream f1 = File.Open(Path.Combine(_tempFolder, "three", "2", "file_xxx1.tmp"), FileMode.Create);
            f1.Flush();
            f1.Close();

            FileStream f2 = File.Open(Path.Combine(_tempFolder, "three", "2", "file_xxx2.tmp"), FileMode.Create);
            f2.Flush();
            f2.Close();

            dataBase = new FileDataBase(_options);
            dataBase.Refresh();

            Assert.AreEqual(1, dataBase.AffectedFoldersCount);
        }

        [TestMethod()]
        public void AffectedFoldersRemoveOnlyCountTest()
        {
            FileDataBase dataBase = new FileDataBase(_optionsReset);
            dataBase.Refresh();

            File.Delete(Files[0]);
            File.Delete(Files[4]);

            dataBase = new FileDataBase(_options);
            dataBase.Refresh();

            Assert.AreEqual(1, dataBase.AffectedFoldersCount);
        }

        [TestMethod()]
        public void AffectedFoldersTest()
        {
            FileDataBase dataBase = new FileDataBase(_optionsReset);
            dataBase.Refresh();

            File.AppendAllText(Files[0], "TEST");
            File.AppendAllText(Files[1], "TEST");
            File.AppendAllText(Files[4], "TEST");
            File.AppendAllText(Files[5], "TEST");

            dataBase = new FileDataBase(_options);
            dataBase.Refresh();

            Assert.AreEqual(Path.GetDirectoryName(Files[0]), dataBase.AffectedFolders[0]);
            Assert.AreEqual(Path.GetDirectoryName(Files[1]), dataBase.AffectedFolders[1]);
            Assert.AreEqual(Path.GetDirectoryName(Files[4]), dataBase.AffectedFolders[0]);
            Assert.AreEqual(Path.GetDirectoryName(Files[5]), dataBase.AffectedFolders[1]);
        }

        private void CreateTempFiles(string path)
        {
            for (int i = 0; i < 11; i++)
            {
                string file = Path.Combine(path, $"file_{i}.tmp");
                FileStream f = File.Open(file, FileMode.Create);
                f.Flush();
                f.Close();
            }
        }
    }
}