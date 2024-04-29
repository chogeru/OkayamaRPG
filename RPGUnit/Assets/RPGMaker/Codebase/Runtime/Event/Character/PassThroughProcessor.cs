using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Character;
using RPGMaker.Codebase.Runtime.Map.Component.Character;

namespace RPGMaker.Codebase.Runtime.Event.Character
{
    public class PassThroughProcessor : AbstractEventCommandProcessor
    {

        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            var isThrough = command.parameters[0] == "0";

            var targetCharacer = new Commons.TargetCharacter(eventID);
            var targetObj = targetCharacer.GetGameObject();
            if (targetObj == null)
            {
                ProcessEndAction();
                return;
            }

            targetObj.GetComponent<CharacterOnMap>().SetThrough(isThrough);
            var moveSetMovePoint = targetObj.GetComponent<MoveSetMovePoint>();
            moveSetMovePoint?.SetThrough(isThrough);

            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}
