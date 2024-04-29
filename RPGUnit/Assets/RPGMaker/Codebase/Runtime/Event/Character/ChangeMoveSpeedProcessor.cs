using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Character;
using RPGMaker.Codebase.Runtime.Map;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using System.Linq;

namespace RPGMaker.Codebase.Runtime.Event.Character
{
    /// <summary>
    /// [キャラクター]-[移動速度の変更]
    /// </summary>
    public class ChangeMoveSpeedProcessor : AbstractEventCommandProcessor
    {

        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            var moveSpeed = int.Parse(command.parameters[0]);

            var targetCharacer = new Commons.TargetCharacter(eventID);
            var targetObj = targetCharacer.GetGameObject();
            if (targetObj == null)
            {
                ProcessEndAction();
                return;
            }

            var speed = Commons.SpeedMultiple.GetValue((Commons.SpeedMultiple.Id)moveSpeed) * MoveSetMovePoint.BaseMoveSpeed;

            var characterOnMap = targetObj.GetComponent<CharacterOnMap>();
            characterOnMap.SetCharacterSpeed(speed);
            if (eventID == "-2")
            {
                foreach (var partyIndex in Enumerable.Range(0, MapManager.GetPartyMemberNum()))
                {
                    MapManager.GetPartyGameObject(partyIndex).GetComponent<CharacterOnMap>().SetCharacterSpeed(speed);
                }
            }

            var moveSetMovePoint = targetObj.GetComponent<MoveSetMovePoint>();
            moveSetMovePoint?.SetMoveSpeed(moveSpeed);

            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}
