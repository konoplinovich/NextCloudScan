using NextCloudScan.UI;
using System;

namespace NextCloudScan.Activities
{
    public class ProgressExternal : IProgress<ProgressExternalResult>
    {
        IHumanUI _ui;

        public ProgressExternal(IHumanUI ui)
        {
            _ui = ui;
        }

        public void Report(ProgressExternalResult value)
        {
            _ui.Show(Message.Info, $"Item: {value.Path}");
            _ui.Show(Message.Info, $"Running: {value.Running}");

            string[] separatelines = value.Log.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in separatelines) { _ui.Show(Message.External, line); }
            if (value.HasError) { _ui.Show(Message.Error, value.ErrorMessage); }
        }
    }
}