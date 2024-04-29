using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime.RuntimeSystemConfigDataModel;

namespace RPGMaker.Codebase.Runtime.Event.Systems
{
    /// <summary>
    /// [システム]-[戦闘BGM変更]
    /// </summary>
    public class SystemBattleBgmProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            //音楽周りのデータをまとめる
            var sound = new Sound(
                command.parameters[0],
                int.Parse(command.parameters[3]),
                int.Parse(command.parameters[2]),
                int.Parse(command.parameters[1])
            );

            DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.battleBgm = sound;

            //次へ
            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}