using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;

namespace RPGMaker.Codebase.Runtime.Event.Systems
{
    /// <summary>
    /// [システム]-[エンカウント禁止の変更]
    /// </summary>
    public class SystemIsEncountProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            //データの保存
            DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.encounterEnabled = int.Parse(command.parameters[0]);

            //次へ
            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}