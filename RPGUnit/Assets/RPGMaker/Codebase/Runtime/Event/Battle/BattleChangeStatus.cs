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
    public class BattleChangeStatus : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            if (command.parameters[1] == "True")
            {
                var value = OperateValue(
                    command.parameters[2] == "up" ? 0 : 1,
                    command.parameters[4] == "True" ? 0 : 1,
                    int.Parse(command.parameters[4] == "True"
                        ? command.parameters[5]
                        : command.parameters[9]));
                IterateEnemyIndex(int.Parse(command.parameters[0]),
                    enemy => { ChangeHp(enemy, value, command.parameters[3] == "True"); });
            }

            if (command.parameters[10] == "True")
            {
                var value = OperateValue(
                    command.parameters[11] == "up" ? 0 : 1,
                    command.parameters[13] == "True" ? 0 : 1,
                    int.Parse(command.parameters[13] == "True"
                        ? command.parameters[14]
                        : command.parameters[18]));
                IterateEnemyIndex(int.Parse(command.parameters[0]),
                    enemy => { enemy.GainMp(value); });
            }

            if (command.parameters[19] == "True")
            {
                var value = OperateValue(
                    command.parameters[20] == "up" ? 0 : 1,
                    command.parameters[22] == "True" ? 0 : 1,
                    int.Parse(command.parameters[22] == "True"
                        ? command.parameters[22]
                        : command.parameters[25]));
                IterateEnemyIndex(int.Parse(command.parameters[0]),
                    enemy => { enemy.GainTp(value); });
            }

            //次のイベントへ
            ProcessEndAction();
        }

        private int OperateValue(int operation, int operandType, int operand) {
            var value = operandType == 0
                ? operand
                : int.Parse(DataManager.Self().GetRuntimeSaveDataModel().variables.data[operand]);
            return operation == 0 ? value : -value;
        }

        private void IterateEnemyIndex(int param, Action<GameBattler> callback) {
            if (param < 0)
            {
                DataManager.Self().GetGameTroop().Members().ForEach(callback);
            }
            else
            {
                var enemy = DataManager.Self().GetGameTroop().Members().ElementAtOrDefault(param);
                if (enemy != null)
                {
                    callback(enemy);
                }
            }
        }

        public void ChangeHp(GameBattler target, int value, bool allowDeath) {
            if (target.IsAlive())
            {
                var maxHP = target.Mhp;
                var minHP = allowDeath ? 0 : 1;
                target.Hp = Math.Max(minHP, Math.Min(maxHP, target.Hp + value));

                if (allowDeath && target.Hp == 0)
                {
                    target.Refresh();
                    target.PerformCollapse();
                }
            }
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}