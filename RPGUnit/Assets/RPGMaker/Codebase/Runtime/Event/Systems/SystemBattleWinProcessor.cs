using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime.RuntimeSystemConfigDataModel;

namespace RPGMaker.Codebase.Runtime.Event.Systems
{
    /// <summary>
    /// [システム]-[勝利ME変更]
    /// </summary>
    public class SystemBattleWinProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            var sound = new Sound(
                command.parameters[0],
                int.Parse(command.parameters[3]),
                int.Parse(command.parameters[2]),
                int.Parse(command.parameters[1])
            );

            DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.victoryMe = sound;

            //次へ
            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}