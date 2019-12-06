using FileScanLib;
using System;

namespace NextCloudScan
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) return;

            DateTime start = DateTime.Now;

            FileDataBase fdb = new FileDataBase(args[0], "base.xml", "diff.xml");

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

                Console.WriteLine($"Removed: {fdb.Removed.Count}, added: {fdb.Added.Count}");

                foreach (FileItem path in fdb.Removed)
                {
                    Console.WriteLine($"R: {path}");
                }

                foreach (FileItem path in fdb.Added)
                {
                    Console.WriteLine($"A: {path}");
                }

                Console.WriteLine($"{fdb.Count} files, time: {interval.TotalSeconds}");
                Console.ReadLine();
                return;
            }
        }
    }
}