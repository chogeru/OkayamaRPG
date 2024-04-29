using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map;
using RPGMaker.Codebase.Runtime.Map.Component.Character;

namespace RPGMaker.Codebase.Runtime.Event.Party
{
    /// <summary>
    /// [パーティ]-[透明状態の変更]
    /// </summary>
    public class PartyAlphaProcess : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            var isAlpha = command.parameters[0] == "0";

            var targetEvent = (this.EventCommandChainLauncher as RPGMaker.Codebase.Runtime.Scene.Map.EventCommandChainLauncher)?.GetCustomMoveTargetEvent();
            if (targetEvent == null)
            {
                targetEvent = "-2";
            } else if (targetEvent == "-1")
            {
                targetEvent = eventID;
            }
            if (targetEvent == "-2")
            {
                foreach (var actor in MapManager.GetAllActorOnMap())
                {
                    if (actor != null && actor.Graphic != null)
                        if (!string.IsNullOrEmpty(actor.CharacterId))
                        {
                            actor.SetTransparent(isAlpha);
                        }
                }
            } else
            {
                var targetCharacer = new Commons.TargetCharacter(targetEvent);
                var targetObj = targetCharacer.GetGameObject();
                if (targetObj == null)
                {
                    ProcessEndAction();
                    return;
                }
                targetObj.GetComponent<CharacterOnMap>().SetTransparent(isAlpha);
            }

            //セーブデータにも保存
            DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.transparent = isAlpha ? 1 : 0;
            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}