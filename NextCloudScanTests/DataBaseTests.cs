using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace NextCloudScan.Tests
{
    [TestClass()]
    public class DataBaseTests
    {
        string _tempFolder;
        Dictionary<string, string[]> _map = new Dictionary<string, string[]>()
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

            if (Directory.Exists(_tempFolder))
            {
                Directory.Delete(_tempFolder, true);
            }

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
                Path.Combine(_tempFolder, "four", "2", "file_105.tmp")
            };
        }

        [TestMethod()]
        public void IsNewBaseTest()
        {
            DataBase dataBase = new DataBase(_tempFolder, true);
            Assert.AreEqual(true, dataBase.IsNewBase);

            dataBase = new DataBase(_tempFolder);
            Assert.AreEqual(false, dataBase.IsNewBase);
        }

        [TestMethod()]
        public void RemoveOneFileTest()
        {
            DataBase dataBase = new DataBase(_tempFolder, true);

            File.Delete(Files[0]);
            dataBase = new DataBase(_tempFolder);
            Assert.AreEqual(1, dataBase.Removed.Count);
        }

        [TestMethod()]
        public void AddOneFileTest()
        {
            DataBase dataBase = new DataBase(_tempFolder, true);

            FileStream f = File.Open(Files[3], FileMode.Create);
            f.Flush();
            f.Close();
           
            dataBase = new DataBase(_tempFolder);
            Assert.AreEqual(1, dataBase.Added.Count);
        }

        [TestMethod()]
        public void AddAndRemoveTest()
        {
            DataBase dataBase = new DataBase(_tempFolder, true);

            File.Delete(Files[0]);
            File.Delete(Files[1]);
            File.Delete(Files[2]);
            FileStream f = File.Open(Files[3], FileMode.Create);
            f.Flush();
            f.Close();

            dataBase = new DataBase(_tempFolder);
            Assert.AreEqual(3, dataBase.Removed.Count);
            Assert.AreEqual(1, dataBase.Added.Count);
        }

        [TestMethod()]
        public void AddedFileNameTest()
        {
            DataBase dataBase = new DataBase(_tempFolder, true);

            FileStream f = File.Open(Files[3], FileMode.Create);
            f.Flush();
            f.Close();

            dataBase = new DataBase(_tempFolder);

            Assert.AreEqual(true, dataBase.Added.Contains(Files[3]));
        }

        [TestMethod()]
        public void RemovedFileNameTest()
        {
            DataBase dataBase = new DataBase(_tempFolder, true);

            File.Delete(Files[0]);
            File.Delete(Files[1]);
            File.Delete(Files[2]);

            dataBase = new DataBase(_tempFolder);

            Assert.AreEqual(true, dataBase.Removed.Contains(Files[0]));
            Assert.AreEqual(true, dataBase.Removed.Contains(Files[1]));
            Assert.AreEqual(true, dataBase.Removed.Contains(Files[2]));
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