using NextCloudScan.UI;
using System;

namespace NextCloudScan.Activities
{
    internal class LogProgress : IProgress<LogProgressResult>
    {
        private IHumanUI _ui;

        public LogProgress(IHumanUI ui)
        {
            _ui = ui;
        }

        public void Report(LogProgressResult value)
        {
            string[] separatelines = value.Log.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in separatelines) { _ui.Show(Message.External, line); }
        }
    }
}