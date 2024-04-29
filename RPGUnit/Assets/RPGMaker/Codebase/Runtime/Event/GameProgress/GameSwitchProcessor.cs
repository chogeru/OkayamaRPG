using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.GameProgress;

namespace RPGMaker.Codebase.Runtime.Event.GameProgress
{
    /// <summary>
    /// [ゲーム進行]-[スイッチの操作]
    /// </summary>
    public class GameSwitchProcessor : AbstractEventCommandProcessor
    {
        private GameSwitch _gameSwitch;

        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            if (_gameSwitch == null)
            {
                _gameSwitch = new GameSwitch();
            }

            var index = 0;
            var indexMax = 0;

            var flags = DataManager.Self().GetFlags();

            // 単体選択
            if (command.parameters[0] == "0")
            {
                for (var i = 0; i < flags.switches.Count; i++)
                    if (flags.switches[i].id == command.parameters[1])
                        index = i;
            }
            // 複数選択
            else
            {
                index = int.Parse(command.parameters[1]) - 1;
                indexMax = int.Parse(command.parameters[2]) - 1;
            }

            _gameSwitch.SetGameSwitch(
                command.parameters[0] == "1" ? true : false,
                index,
                indexMax,
                command.parameters[3] == "0" ? true : false);

            ProcessEndAction();
        }

        private void ProcessEndAction() {
            _gameSwitch = null;
            SendBackToLauncher.Invoke();
        }
    }
}