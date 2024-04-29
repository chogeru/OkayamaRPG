using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Character;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Event.Character
{
    /// <summary>
    /// [キャラクター]-[向き]
    /// </summary>
    public class CharacterDirectionProcessor : AbstractEventCommandProcessor
    {
        private CharacterDirection _characterDirection;

        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            //イベントコードから、向きを設定する
            var direction = 0;
            if (command.code == (int) EventEnum.MOVEMENT_TURN_DOWN) direction = 0;
            else if (command.code == (int) EventEnum.MOVEMENT_TURN_LEFT) direction = 1;
            else if (command.code == (int) EventEnum.MOVEMENT_TURN_RIGHT) direction = 2;
            else if (command.code == (int) EventEnum.MOVEMENT_TURN_UP) direction = 3;
            else if (command.code == (int) EventEnum.MOVEMENT_TURN_90_RIGHT) direction = 4;
            else if (command.code == (int) EventEnum.MOVEMENT_TURN_90_LEFT) direction = 5;
            else if (command.code == (int) EventEnum.MOVEMENT_TURN_180) direction = 6;
            else if (command.code == (int) EventEnum.MOVEMENT_TURN_90_RIGHT_OR_LEFT) direction = 7;
            else if (command.code == (int) EventEnum.MOVEMENT_TURN_AT_RANDOM) direction = 8;
            else if (command.code == (int) EventEnum.MOVEMENT_TURN_TOWARD_PLAYER) direction = 9;
            else if (command.code == (int) EventEnum.MOVEMENT_TURN_AWAY_FROM_PLAYER) direction = 10;
            else direction = int.Parse(command.parameters[1]);

            //0 イベントの番号
            //1 向きの種類
            //2 ウェイト
            CharacterDirection(
                command.parameters[0],
                direction,
                command.parameters[2] == "1" ? true : false,
                eventID
            );
            CloseCharacterDirection();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }

        private void CharacterDirection(string eventId, int direction, bool lockDirection, string currentEventID) {
            if (_characterDirection == null)
            {
                _characterDirection = new GameObject().AddComponent<CharacterDirection>();
                _characterDirection.Init();
            }

            _characterDirection.SetDirection(eventId, direction, lockDirection, currentEventID);
        }

        private void CloseCharacterDirection() {
            if (_characterDirection == null)
            {
                ProcessEndAction();
                return;
            }

            Object.Destroy(_characterDirection.gameObject);
            _characterDirection = null;
            ProcessEndAction();
        }
    }
}