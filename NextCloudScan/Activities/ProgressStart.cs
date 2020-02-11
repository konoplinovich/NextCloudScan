using NextCloudScan.UI;
using System;

namespace NextCloudScan.Activities
{
    internal class ProgressStart : IProgress<ProgressStartResult>
    {
        IHumanUI _ui;
        public ProgressStart(IHumanUI ui)
        {
            _ui = ui;
        }
        
        public void Report(ProgressStartResult value)
        {
            _ui.Show(Message.Info, $"Item: {value.Path}");
            _ui.Show(Message.Info, $"Running: {value.Running}");
        }
    }
}