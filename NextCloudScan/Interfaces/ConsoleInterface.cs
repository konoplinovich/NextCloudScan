using System;

namespace NextCloudScan.Interfaces
{
    public class ConsoleInterface : IHumanInterface
    {
        public void Show(MessageType type, string message)
        {
            Marker(type);
            Console.WriteLine(message);
        }

        private static void Marker(MessageType mark)
        {
            switch (mark)
            {
                case MessageType.NewFile:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("[+]");
                    break;
                case MessageType.RemovedFile:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("[-]");
                    break;
                case MessageType.AffectedFolder:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("[A]");
                    break;
                case MessageType.Start:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("[>]");
                    break;
                case MessageType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[E]");
                    break;
                case MessageType.Info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("[I]");
                    break;
                case MessageType.Config:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("[#]");
                    break;
                case MessageType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("[!]");
                    break;
                default:
                    break;
            }
            Console.Write(" ");
            Console.ResetColor();
        }
    }
}