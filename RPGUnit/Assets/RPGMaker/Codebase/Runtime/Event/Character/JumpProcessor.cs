using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Character;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Event.Character
{
    /// <summary>
    /// [キャラクター]-[ジャンプ(相対座標)]
    /// </summary>
    public class JumpProcessor : AbstractEventCommandProcessor
    {
        private class JumpProcessorData
        {
            public Jump jump;
            public string eventId;
        }

        private List<JumpProcessorData> _jumpData;

        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            MoveJump(eventID, command);
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }

        private void MoveJump(string currentEventID, EventDataModel.EventCommand command) {
            JumpProcessorData jumpData = null;
            if (_jumpData == null)
            {
                _jumpData = new List<JumpProcessorData>();
            }

            //既にジャンプ中のGameObjectが存在した場合、それを破棄して新規にジャンプする
            for (int i = 0; i < _jumpData.Count; i++)
            {
                if (_jumpData[i].eventId == command.parameters[0])
                {
                    Object.DestroyImmediate(_jumpData[i].jump.gameObject);
                    jumpData = new JumpProcessorData();
                    jumpData.eventId = command.parameters[0];
                    jumpData.jump = new GameObject().AddComponent<Jump>();
                    _jumpData[i] = jumpData;
                    break;
                }
            }

            //まだジャンプしていないイベントの場合は新規作成する
            if (jumpData == null)
            {
                jumpData = new JumpProcessorData();
                jumpData.eventId = command.parameters[0];
                jumpData.jump = new GameObject().AddComponent<Jump>();
                _jumpData.Add(jumpData);
            }

            //ジャンプ実行
            jumpData.jump.Init();
            jumpData.jump.MoveJumpProcess(currentEventID, command, CloseMovePlace, ProcessEndAction, command.parameters[0]);
        }

        private void CloseMovePlace(string eventId) {
            for (int i = 0; i < _jumpData.Count; i++)
            {
                if (_jumpData[i].eventId == eventId)
                {
                    Object.Destroy(_jumpData[i].jump.gameObject);
                    _jumpData.RemoveAt(i);
                    break;
                }
            }
        }
    }
}