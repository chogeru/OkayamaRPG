using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Battle;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map;

namespace RPGMaker.Codebase.Runtime.Event.Screen
{
    /// <summary>
    /// [シーン制御]-[ゲームオーバー]
    /// </summary>
    public class GameOverProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            ProcessWait();
        }

        private void ProcessWait() {
            //ゲームオーバー表示
            DataManager.Self().IsGameOverCheck = true;
            if (GameStateHandler.IsMap())
                MapManager.ShowGameOver();
            else
                SceneBattle.GameOver();
        }
    }
}