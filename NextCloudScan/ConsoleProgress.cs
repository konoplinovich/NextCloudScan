using NextCloudScan.UI;
using System;

namespace NextCloudScan
{
    public class ConsoleProgress : IProgress<ProgressResult>
    {
        IHumanUI _ui;

        public ConsoleProgress(IHumanUI ui)
        {
            _ui = ui;
        }

        public void Report(ProgressResult value)
        {
            _ui.Show(Message.Info, value.Path);

            string[] separatelines = value.Log.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in separatelines) { _ui.Show(Message.External, $"{line}"); }
            if (value.HasError) { _ui.Show(Message.Error, value.ErrorMessage); }
        }
    }
}