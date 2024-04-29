using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map;

namespace RPGMaker.Codebase.Runtime.Event.Systems
{
    /// <summary>
    /// [システム]-[セーブ禁止の変更]
    /// </summary>
    public class SystemIsSaveProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            //データの保存
            DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.saveEnabled = int.Parse(command.parameters[0]);

            //メニュー画面への反映
            MapManager.menu.CanSave();

            //次へ
            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}