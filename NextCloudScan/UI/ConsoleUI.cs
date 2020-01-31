using System;

namespace NextCloudScan.UI
{
    internal class ConsoleUI : IHumanUI
    {
        public void Show(Message type, string message)
        {
            Marker(type);
            Console.WriteLine(message);
        }

        private static void Marker(Message mark)
        {
            switch (mark)
            {
                case Message.None:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("");
                    break;
                case Message.NewFile:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("[+] ");
                    break;
                case Message.RemovedFile:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("[-] ");
                    break;
                case Message.AffectedFolder:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("[A] ");
                    break;
                case Message.Start:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("[>] ");
                    break;
                case Message.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[E] ");
                    break;
                case Message.Info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("[I] ");
                    break;
                case Message.Config:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("[#] ");
                    break;
                case Message.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("[!] ");
                    break;
                case Message.External:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("[=] ");
                    break;
                default:
                    break;
            }
            Console.ResetColor();
        }
    }
}