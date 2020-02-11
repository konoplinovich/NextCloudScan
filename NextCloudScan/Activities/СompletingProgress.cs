using NextCloudScan.UI;
using System;

namespace NextCloudScan.Activities
{
    internal class СompletingProgress : IProgress<СompletingProgressResult>
    {
        private IHumanUI _ui;

        public СompletingProgress(IHumanUI ui)
        {
            _ui = ui;
        }

        public void Report(СompletingProgressResult value)
        {
            if (value.HasError) { _ui.Show(Message.Error, value.Message); }
            else { _ui.Show(Message.Stop, value.Message); }
        }
    }
}