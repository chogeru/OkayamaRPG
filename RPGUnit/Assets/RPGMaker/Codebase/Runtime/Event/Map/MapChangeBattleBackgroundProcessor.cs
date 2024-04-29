using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Misc;
using RPGMaker.Codebase.Runtime.Common;

namespace RPGMaker.Codebase.Runtime.Event.Map
{
    /// <summary>
    /// [マップ]-[戦闘背景の変更]
    /// </summary>
    public class MapChangeBattleBackground : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {

            BattleSceneTransition.Instance.EventMapBackgroundImage1 = command.parameters[0];
            BattleSceneTransition.Instance.EventMapBackgroundImage2 = command.parameters[1];

            DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.eventBattleBack1 =
                command.parameters[0];
            DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.eventBattleBack2 =
                command.parameters[1];
            
            ProcessEndAction();
        }

        private void ProcessEndAction() {
            // // メッセージウィンドウを閉じて、ランチャーに戻す
            SendBackToLauncher.Invoke();
        }
    }
}