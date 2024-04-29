using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Battle;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common;
using System;
using System.Linq;

namespace RPGMaker.Codebase.Runtime.Event.Battle
{
    /// <summary>
    /// [バトル]-[敵キャラの出現]
    /// </summary>
    public class BattleAppear : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            IterateEnemyIndex(int.Parse(command.parameters[0]), enemy =>
            {
                enemy.Appear();
                DataManager.Self().GetGameTroop().MakeUniqueNames();

                //敵選択Windowを再生成する
                BattleManager.SceneBattle.CreateEnemyWindow();
            });

            //次のイベントへ
            ProcessEndAction();
        }

        private void IterateEnemyIndex(int param, Action<GameBattler> callback) {
            if (param < 0)
            {
                DataManager.Self().GetGameTroop().Members().ForEach(callback);
            }
            else
            {
                var enemy = DataManager.Self().GetGameTroop().Members().ElementAtOrDefault(param);
                if (enemy != null) callback(enemy);
            }
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}