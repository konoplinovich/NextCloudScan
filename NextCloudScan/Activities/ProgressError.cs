using NextCloudScan.UI;
using System;

namespace NextCloudScan.Activities
{
    internal class ProgressError : IProgress<ProgressErrorResult>
    {
        private IHumanUI _ui;

        public ProgressError(IHumanUI ui)
        {
            _ui = ui;
        }

        public void Report(ProgressErrorResult value)
        {
            if (value.HasError) { _ui.Show(Message.Error, value.ErrorMessage); }
        }
    }
}