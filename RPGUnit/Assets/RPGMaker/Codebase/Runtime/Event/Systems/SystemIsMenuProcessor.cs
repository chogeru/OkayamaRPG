using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map;

namespace RPGMaker.Codebase.Runtime.Event.Systems
{
    /// <summary>
    /// [システム]-[メニュー禁止の変更]
    /// </summary>
    public class SystemIsMenuProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            // 禁止
            if (command.parameters[0] == "0")
            {
                DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.menuEnabled = 0;
                MapManager.menu.CanMenuOpen(false);
            }
            // 通常・禁止解除
            else
            {
                DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.menuEnabled = 1;
                MapManager.menu.CanMenuOpen(true);
            }

            //次へ
            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}