﻿namespace NextCloudScan.UI
{
    internal class CombinedUI : IHumanUI
    {
        ConsoleUI _console;
        LogfileUI _logfile;

        public CombinedUI(string logFilePath, bool singleLogFile = true, int agelimit = 1)
        {
            _logfile = new LogfileUI(logFilePath, singleLogFile, agelimit);
            _console = new ConsoleUI();
        }

        public void Show(Message type, string message)
        {
            _console.Show(type, message);
            _logfile.Show(type, message);
        }
    }
}