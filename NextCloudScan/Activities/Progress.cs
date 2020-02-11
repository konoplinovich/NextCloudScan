using NextCloudScan.UI;
using System;

namespace NextCloudScan.Activities
{
    public class Progress
    {
        private IHumanUI _ui;

        public IProgress<ProgressStartResult> StartProgress { get; set; }
        public IProgress<ProgressLogResult> LogProgress { get; set; }
        public IProgress<ProgressErrorResult> Stopprogress { get; set; }

        public Progress(IHumanUI ui)
        {
            _ui = ui;

            StartProgress = new ProgressStart(_ui);
            LogProgress = new ProgressLog(_ui);
            Stopprogress = new ProgressError(_ui);
        }
    }
}