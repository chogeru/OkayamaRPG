using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Character;

namespace RPGMaker.Codebase.Runtime.Event.Character
{
    /// <summary>
    /// [キャラクター]-[移動頻度の変更]
    /// </summary>
    public class ChangeMoveFrequencyProcessor : AbstractEventCommandProcessor
    {

        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            var moveFrequency = int.Parse(command.parameters[0]);

            var targetCharacer = new Commons.TargetCharacter(eventID);
            var targetObj = targetCharacer.GetGameObject();
            if (targetObj == null)
            {
                ProcessEndAction();
                return;
            }

            var moveSetMovePoint = targetObj.GetComponent<MoveSetMovePoint>();
            moveSetMovePoint?.SetMoveFrequency(moveFrequency);

            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}
