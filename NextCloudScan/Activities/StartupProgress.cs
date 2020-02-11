using NextCloudScan.UI;
using System;

namespace NextCloudScan.Activities
{
    internal class StartupProgress : IProgress<StartupProgressResult>
    {
        IHumanUI _ui;
        public StartupProgress(IHumanUI ui)
        {
            _ui = ui;
        }

        public void Report(StartupProgressResult value)
        {
            _ui.Show(Message.Info, $"Item: {value.Path}");
            _ui.Show(Message.Start, $"Running: {value.Running}");
        }
    }
}