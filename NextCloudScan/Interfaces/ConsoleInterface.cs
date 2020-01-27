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
                case MessageType.Add:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("[+]");
                    break;
                case MessageType.Remove:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("[-]");
                    break;
                case MessageType.Affected:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("[A]");
                    break;
                case MessageType.Scan:
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
                case MessageType.Options:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("[#]");
                    break;
                default:
                    break;
            }
            Console.Write(" ");
            Console.ResetColor();
        }
    }
}