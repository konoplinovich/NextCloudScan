using NextCloudScan.UI;
using System;

namespace NextCloudScan.Activities
{
    public class Progress
    {
        private IHumanUI _ui;

        public IProgress<StartupProgressResult> StartupProgress { get; private set; }
        public IProgress<LogProgressResult> LogProgress { get; private set; }
        public IProgress<СompletingProgressResult> СompletingProgress { get; private set; }

        public Progress(IHumanUI ui)
        {
            _ui = ui;

            StartupProgress = new StartupProgress(_ui);
            LogProgress = new LogProgress(_ui);
            СompletingProgress = new СompletingProgress(_ui);
        }
    }
}