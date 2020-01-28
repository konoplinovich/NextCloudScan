using System;
using System.IO;

namespace NCSPerFolderPayload
{
    class NCSPerFolderPayload
    {
        private static string _foldername;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("[E]No folder name specified");
                return;
            }
            _foldername = args[0];

            if (!Directory.Exists(_foldername))
            {
                Console.WriteLine("[E]Folder not found");
            }
            else
            {
                try
                {
                    DirectoryInfo di = new DirectoryInfo(_foldername);
                    if (di != null)
                    {
                        Console.WriteLine($"{di.Name} : [D]{di.Parent}");

                        string[] files = new string[] { };
                        if (Directory.Exists(_foldername))
                        {
                            files = Directory.GetFiles(_foldername);
                        }

                        foreach (var item in files)
                        {
                            Console.WriteLine($"     {item}");
                        }
                    }
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine($"[E]Access to {_foldername} is denied. {e.Message}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[E]Error. {e.Message}");
                }
            }

            Console.WriteLine();
        }
    }
}
