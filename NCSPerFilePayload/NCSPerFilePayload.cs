using System;
using System.IO;

namespace NCSPerFilePayload
{
    class NCSPerFilePayload
    {
        private static string _fileName;
        
        static void Main(string[] args)
        {
            if (args.Length == 0) return;
            _fileName = args[0];

            if (!File.Exists(_fileName))
            {
                Console.WriteLine("[E]File not found");
            }
            else
            {
                try
                {
                    FileInfo fi = new FileInfo(_fileName);
                    if (fi != null)
                    {
                        Console.WriteLine($"[D]{fi.Directory} [F]{fi.Name} [Size]{fi.Length} [LWT]{fi.LastWriteTime} [LAT]{fi.LastAccessTime}");
                    }
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine($"[E]Access to {_fileName} is denied. {e.Message}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[E]Error. {e.Message}");
                }
            }
        }
    }
}