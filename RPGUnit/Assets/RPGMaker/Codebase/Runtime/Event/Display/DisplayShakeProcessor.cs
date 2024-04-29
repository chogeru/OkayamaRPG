using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;

namespace RPGMaker.Codebase.Runtime.Event.Display
{
    /// <summary>
    /// [画面]-[画面のシェイク]
    /// </summary>
    public class DisplayShakeProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            var type = int.Parse(command.parameters[0]);
            var value = int.Parse(command.parameters[1]);
            var flame = int.Parse(command.parameters[2]);
            var wait = command.parameters[3] == "0" ? false : true;
            HudDistributor.Instance.NowHudHandler().DisplayInit();
            HudDistributor.Instance.NowHudHandler().Shake(ProcessEndAction, type, value, flame, wait);
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}