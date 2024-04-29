using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map;

namespace RPGMaker.Codebase.Runtime.Event.Systems
{
    /// <summary>
    /// [システム]-[並べ替え禁止の変更]
    /// </summary>
    public class SystemIsSortProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            //データの保存
            DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.formationEnabled = int.Parse(command.parameters[0]);

            //メニュー画面への反映
            MapManager.menu.CanSort();

            //次へ
            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}