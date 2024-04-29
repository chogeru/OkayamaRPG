using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.GameProgress;

namespace RPGMaker.Codebase.Runtime.Event.GameProgress
{
    /// <summary>
    /// [ゲーム進行]-[セルフスイッチの操作]
    /// </summary>
    public class GameSelfSwitchProcessor : AbstractEventCommandProcessor
    {
        private GameSelfSwitch _gameSelfSwitch;

        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            if (_gameSelfSwitch == null)
            {
                _gameSelfSwitch = new GameSelfSwitch();
                _gameSelfSwitch.Init();
            }
            _gameSelfSwitch.SetGameSelfSwitch(eventID, int.Parse(command.parameters[2]), command.parameters[0],
                command.parameters[1] == "1" ? true : false);
            ProcessEndAction();
        }

        private void ProcessEndAction() {
            _gameSelfSwitch = null;
            SendBackToLauncher.Invoke();
        }
    }
}