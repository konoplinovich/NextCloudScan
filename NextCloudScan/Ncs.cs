using FileScanLib;
using System;

namespace NextCloudScan
{
    class Ncs
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) return;

            DateTime start = DateTime.Now;

            FileDataBase fdb = new FileDataBase(args[0], "base.xml", "diff.xml", "affected_folders.log");

            if (fdb.IsNewBase)
            {
                DateTime stop = DateTime.Now;
                TimeSpan interval = stop - start;

                Console.WriteLine($"{fdb.Count} files, time: {interval.TotalSeconds}");
                Console.ReadLine();
                return;
            }
            else
            {
                DateTime stop = DateTime.Now;
                TimeSpan interval = stop - start;

                Console.WriteLine();

                foreach (FileItem path in fdb.Removed)
                {
                    Console.WriteLine($"R: {path}");
                }

                foreach (FileItem path in fdb.Added)
                {
                    Console.WriteLine($"A: {path}");
                }

                foreach (string path in fdb.AffectedFolders)
                {
                    Console.WriteLine($"Affected: {path}");
                }
                
                Console.WriteLine("---");
                Console.WriteLine($"Removed: {fdb.Removed.Count}");
                Console.WriteLine($"Added: {fdb.Added.Count}");
                Console.WriteLine($"Affected folders: {fdb.AffectedFoldersCount}");
                Console.WriteLine();
                Console.WriteLine($"{fdb.Count} files, time: {interval.TotalSeconds}");
                
                Console.ReadLine();
                return;
            }
        }
    }
}