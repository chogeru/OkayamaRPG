using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common;
using System;
using System.Linq;

namespace RPGMaker.Codebase.Runtime.Event.Battle
{
    /// <summary>
    /// [バトル]-[敵キャラのステート変更]
    /// </summary>
    public class BattleChangeState : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            IterateEnemyIndex(int.Parse(command.parameters[0]), enemy =>
            {
                var alreadyDead = enemy.IsDead();
                if (command.parameters[1] == "0")
                    enemy.AddState(command.parameters[2]);
                else
                    enemy.RemoveState(command.parameters[2]);

                if (enemy.IsDead() && !alreadyDead) enemy.PerformCollapse();

                enemy.ClearResult();
            });

            //次のイベントへ
            ProcessEndAction();
        }

        public void IterateEnemyIndex(int param, Action<GameBattler> callback) {
            if (param == 0)
            {
                DataManager.Self().GetGameTroop().Members().ForEach(callback);
            }
            else
            {
                var enemy = DataManager.Self().GetGameTroop().Members().ElementAtOrDefault(param - 1);
                if (enemy != null) callback(enemy);
            }
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}