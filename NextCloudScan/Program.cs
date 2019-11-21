using System;

namespace NextCloudScan
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) return;

            DateTime start = DateTime.Now;
            
            DataBase sf = new DataBase(args[0]);
            
            if (sf.IsNewBase)
            {
                DateTime stop = DateTime.Now;
                TimeSpan interval = stop - start;

                Console.WriteLine($"{sf.Count} files, time: {interval.TotalSeconds}");
                return;
            }
            else
            {
                DateTime stop = DateTime.Now;
                TimeSpan interval = stop - start;

                Console.WriteLine($"Removed: {sf.Removed.Count}, added: {sf.Added.Count}");

                foreach (string path in sf.Added)
                {
                    Console.WriteLine($"A: {path}");
                }

                foreach (string path in sf.Removed)
                {
                    Console.WriteLine($"R: {path}");
                }

                Console.WriteLine($"{sf.Count} files, time: {interval.TotalSeconds}");
                return;
            }
        }
    }
}