using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Character;
using System.Collections.Generic;

namespace RPGMaker.Codebase.Runtime.Event.Character
{
    public class StepMoveProcessor : AbstractEventCommandProcessor
    {
        public static int _jumpIndex = 11;
        static List<EventMoveEnum> _eventMoveEnumList = new List<EventMoveEnum>() {
            EventMoveEnum.MOVEMENT_MOVE_DOWN,
            EventMoveEnum.MOVEMENT_MOVE_LEFT,
            EventMoveEnum.MOVEMENT_MOVE_RIGHT,
            EventMoveEnum.MOVEMENT_MOVE_UP,
            EventMoveEnum.MOVEMENT_MOVE_LOWER_LEFT,
            EventMoveEnum.MOVEMENT_MOVE_LOWER_RIGHT,
            EventMoveEnum.MOVEMENT_MOVE_UPPER_LEFT,
            EventMoveEnum.MOVEMENT_MOVE_UPPER_RIGHT,
            EventMoveEnum.MOVEMENT_MOVE_AT_RANDOM,
            EventMoveEnum.MOVEMENT_MOVE_TOWARD_PLAYER,
            EventMoveEnum.MOVEMENT_MOVE_AWAY_FROM_PLAYER,
            EventMoveEnum.MOVEMENT_JUMP,
        };

        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            //下に移動、左に移動、右に移動、上に移動、左下に移動、右下に移動、左上に移動、右上に移動、ランダムに移動、プレイヤーに近づく、プレイヤーから遠ざかる、ジャンプ
            var type = int.Parse(command.parameters[0]);
            var positionX = int.Parse(command.parameters[1]);
            var positionY = int.Parse(command.parameters[2]);

            var targetCharacer = new Commons.TargetCharacter(eventID);
            var targetObj = targetCharacer.GetGameObject();
            if (targetObj == null)
            {
                ProcessEndAction();
                return;
            }

            var skipIfUnableToMove = true;
            var eventCommandChainLauncher = this.EventCommandChainLauncher as RPGMaker.Codebase.Runtime.Scene.Map.EventCommandChainLauncher;
            if (eventCommandChainLauncher != null)
            {
                skipIfUnableToMove = eventCommandChainLauncher.IsCustomMoveSkipIfUnabletoMove();
            }

            //ステップ移動コントローラー
            var stepMoveController = targetObj.GetComponent<StepMoveController>();
            if (stepMoveController == null)
            {
                stepMoveController = targetObj.AddComponent<StepMoveController>();
            }

            //移動処理
            stepMoveController.StartStepMove(
                    ProcessEndAction,
                    () => { stepMoveController.UpdateMove(); },
                    eventID,
                    _eventMoveEnumList[type], positionX, positionY, skipIfUnableToMove);
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}
