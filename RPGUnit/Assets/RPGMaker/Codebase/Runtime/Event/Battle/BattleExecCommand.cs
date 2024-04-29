using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Battle;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RPGMaker.Codebase.Runtime.Event.Battle
{
    /// <summary>
    /// [バトル]-[戦闘行動の強制]
    /// </summary>
    public class BattleExecCommand : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            int ActorOrEnemy;
            ActorOrEnemy = command.parameters[0] == "0" ? 0 : 1;

            bool isAction = false;
            IterateBattler(ActorOrEnemy, command.parameters[1], battler =>
            {
                if (!battler.IsDeathStateAffected())
                {
                    if (command.parameters.Count >= 6 && command.parameters[5] == "NEWDATA")
                        battler.ForceAction(command.parameters[2], int.Parse(command.parameters[4]));
                    else
                    {
                        if (command.parameters[3] == "0" || command.parameters[3] == "ラストターゲット" || command.parameters[3] == "Last Target" || command.parameters[3] == "最后一个目标")
                            battler.ForceAction(command.parameters[2], -2);
                        else if (command.parameters[3] == "ランダム" || command.parameters[3] == "Random" || command.parameters[3] == "随机")
                            battler.ForceAction(command.parameters[2], -1);
                        else
                            battler.ForceAction(command.parameters[2], int.Parse(command.parameters[3].Substring(command.parameters[3].Length - 1, 1)) - 1);
                    }
                    BattleEventCommandChainLauncher.PauseEvent(ProcessEndAction);
                    BattleManager.ForceAction(battler);
                    isAction = true;
                }
            });

            if (!isAction)
            {
                //戦闘行動の強制が行われなかった場合
                ProcessEndAction();
            }
        }

        private void IterateBattler(int param1, string param2, Action<GameBattler> callback) {
            if (DataManager.Self().GetGameParty().InBattle())
            {
                if (param1 == 0)
                {
                    var memberNo = 0;
                    if (int.TryParse(param2, out memberNo))
                    {
                        memberNo -= 1; // 1から始まる番号で格納されているのでインデックス用に調整
                        IterateEnemyIndex(memberNo, callback);
                    }
                }
                else
                {
                    IterateActorId(param2, callback);
                }
            }
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

        private void IterateActorId(string param, Action<GameBattler> callback) {
            if (param == "")
            {
                DataManager.Self().GetGameParty().Members().ForEach(callback);
            }
            else
            {
                var actors = DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels;
                for (var i = 0; i < actors.Count; i++)
                    if (param == actors[i].actorId)
                    {
                        var actor = DataManager.Self().GetGameActors().Actor(actors[i]);
                        if (actor != null) callback(actor);
                    }
            }
        }

        private async void ProcessEndAction() {
            await Task.Delay(1);
            SendBackToLauncher.Invoke();
        }
    }
}