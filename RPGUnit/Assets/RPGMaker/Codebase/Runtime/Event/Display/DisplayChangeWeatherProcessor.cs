using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;

namespace RPGMaker.Codebase.Runtime.Event.Display
{
    /// <summary>
    /// [画面]-[天候の設定]
    /// </summary>
    public class DisplayChangeWeatherProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            var type = int.Parse(command.parameters[0]);
            var value = int.Parse(command.parameters[1]);
            var flame = int.Parse(command.parameters[2]);
            var wait = command.parameters[3] == "0" ? false : true;

            HudDistributor.Instance.NowHudHandler().DisplayInit();
            HudDistributor.Instance.NowHudHandler().ChangeWeather(ProcessEndAction, type, value, flame, wait);
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}