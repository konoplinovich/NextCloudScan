using NextCloudScan.UI;
using System;

namespace NextCloudScan.Activities
{
    internal class ProgressLog : IProgress<ProgressLogResult>
    {
        private IHumanUI _ui;

        public ProgressLog(IHumanUI ui)
        {
            _ui = ui;
        }

        public void Report(ProgressLogResult value)
        {
            string[] separatelines = value.Log.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in separatelines) { _ui.Show(Message.External, line); }
        }
    }
}