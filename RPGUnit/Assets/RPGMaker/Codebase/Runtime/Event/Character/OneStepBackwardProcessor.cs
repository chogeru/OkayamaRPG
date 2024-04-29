using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Character;
using RPGMaker.Codebase.Runtime.Map;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Event.Character
{
    /// <summary>
    /// [キャラクター]-[一歩後退]
    /// </summary>
    public class OneStepBackwardProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            // カスタム移動内にある場合はカスタム移動で指定したイベントIDを参照。
            var targetEvent = (this.EventCommandChainLauncher as RPGMaker.Codebase.Runtime.Scene.Map.EventCommandChainLauncher)?.GetCustomMoveTargetEvent();
            if (targetEvent == null)
            {
                targetEvent = "-2";
            }
            else if (targetEvent == "-1")
            {
                targetEvent = eventID;
            }

            GameObject targetObj = null;
            if (targetEvent == "-2")
            {
                //プレイヤーのGameObject
                targetObj = MapManager.GetOperatingCharacterGameObject();
            }
            else
            {
                var targetCharacer = new Commons.TargetCharacter(targetEvent);
                targetObj = targetCharacer.GetGameObject();
            }

            //対象のオブジェクトが存在しない場合は処理終了
            if (targetObj == null)
            {
                ProcessEndAction();
                return;
            }

            //移動ルート情報を取得
            MoveSetMovePoint moveSetMovePoint;

            //移動ルート指定
            if (targetObj.GetComponent<MoveSetMovePoint>() == null)
            {
                moveSetMovePoint = targetObj.AddComponent<MoveSetMovePoint>();
            }
            else
            {
                moveSetMovePoint = targetObj.GetComponent<MoveSetMovePoint>();
            }

            //移動ルートの指定、のイベントに処理を移譲するためのパラメータ
            List<string> paramsMovePoint = new List<string>();
            paramsMovePoint.Add(targetEvent);  //対象
            paramsMovePoint.Add("0");   //カスタム移動
            paramsMovePoint.Add("-1");  //移動速度は「設定変更しない」
            paramsMovePoint.Add("4");   //移動頻度は最高
            paramsMovePoint.Add("");    //向きはあとで設定
            paramsMovePoint.Add("");    //歩行アニメ、足踏みアニメ設定はあとで設定
            paramsMovePoint.Add("0");   //すり抜け設定
            paramsMovePoint.Add(command.parameters[0]);  //繰り返し
            paramsMovePoint.Add(command.parameters[1]);  //移動できなければ飛ばす
            paramsMovePoint.Add(command.parameters[2]);  //完了までウェイト

            //移動ルートは現在の向きから決定する
            List<EventDataModel.EventCommandMoveRoute> paramMoveRoute = new List<EventDataModel.EventCommandMoveRoute>();
            CharacterMoveDirectionEnum currentDirectionEnum = targetObj.GetComponent<CharacterOnMap>().GetCurrentDirection();
            if (currentDirectionEnum == CharacterMoveDirectionEnum.Up)
            {
                //今上を向いている場合は、下に移動
                paramMoveRoute.Add(new EventDataModel.EventCommandMoveRoute((int) EventMoveEnum.MOVEMENT_MOVE_DOWN, null, 0));
                //向きは今と同じ
                paramsMovePoint[4] = ((int)Commons.Direction.Id.Up).ToString();
            }
            else if (currentDirectionEnum == CharacterMoveDirectionEnum.Right)
            {
                //今右を向いている場合は、左に移動
                paramMoveRoute.Add(new EventDataModel.EventCommandMoveRoute((int) EventMoveEnum.MOVEMENT_MOVE_LEFT, null, 0));
                //向きは今と同じ
                paramsMovePoint[4] = ((int)Commons.Direction.Id.Right).ToString();
            }
            else if (currentDirectionEnum == CharacterMoveDirectionEnum.Down)
            {
                //今下を向いている場合は、上に移動
                paramMoveRoute.Add(new EventDataModel.EventCommandMoveRoute((int) EventMoveEnum.MOVEMENT_MOVE_UP, null, 0));
                //向きは今と同じ
                paramsMovePoint[4] = ((int)Commons.Direction.Id.Down).ToString();
            }
            else if (currentDirectionEnum == CharacterMoveDirectionEnum.Left)
            {
                //今左を向いている場合は、右に移動
                paramMoveRoute.Add(new EventDataModel.EventCommandMoveRoute((int) EventMoveEnum.MOVEMENT_MOVE_RIGHT, null, 0));
                //向きは今と同じ
                paramsMovePoint[4] = ((int)Commons.Direction.Id.Left).ToString();
            }

            //歩行アニメ、足踏みアニメ
            if (targetObj.GetComponent<CharacterOnMap>().GetAnimation() && !targetObj.GetComponent<CharacterOnMap>().GetStepAnimation())
            {
                paramsMovePoint[5] = "0";
            }
            else if (!targetObj.GetComponent<CharacterOnMap>().GetAnimation() && targetObj.GetComponent<CharacterOnMap>().GetStepAnimation())
            {
                paramsMovePoint[5] = "1";
            }
            else if (!targetObj.GetComponent<CharacterOnMap>().GetAnimation() && !targetObj.GetComponent<CharacterOnMap>().GetStepAnimation())
            {
                paramsMovePoint[5] = "2";
            }
            else
            {
                paramsMovePoint[5] = "3";
            }

            //すり抜け設定
            if (targetObj.GetComponent<CharacterOnMap>().GetCharacterThrough())
            {
                paramsMovePoint[6] = "1";
            }
            else
            {
                paramsMovePoint[6] = "0";
            }

            //イベント設定
            EventDataModel.EventCommand commandMovePoint = new EventDataModel.EventCommand(
                command.code,
                paramsMovePoint,
                paramMoveRoute);

            //移動処理
            moveSetMovePoint.SetMovePointProcess(
                ProcessEndAction,
                () => { moveSetMovePoint.UpdateMove(); },
                commandMovePoint,
                eventID,
                true);

            //完了を待たない場合、EndActionを先に実行する
            if (command.parameters[2] == "0")
            {
                ProcessEndAction();
            }
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}