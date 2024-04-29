using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Character;
using RPGMaker.Codebase.Runtime.Map;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Event.Character
{
    /// <summary>
    /// 移動に関連するイベント
    /// </summary>
    public class MoveSetMovePointProcessor : AbstractEventCommandProcessor
    {
        private DatabaseManagementService _databaseManagementService;
        private bool _endAction = false;

        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            _databaseManagementService = new DatabaseManagementService();
            _endAction = false;

            //イベントの位置指定
            if (command.code == (int) EventEnum.EVENT_CODE_MOVE_SET_EVENT_POINT)
            {
                // 対象Playerにも対応。
                eventID = (command.parameters[0] == "-1") ? eventID : command.parameters[0];
                var targetObj = MapEventExecutionController.Instance.GetEventMapGameObject(eventID);

                if (eventID == "-2")
                {
                    targetObj = MapManager.GetOperatingCharacterGameObject();
                }

                if (targetObj == null)
                {
                    ProcessEndAction();
                    _endAction = true;
                    return;
                }
                //実際に使用する座標
                var xValue = 0;
                var yValue = 0;
                switch (command.parameters[1])
                {
                    //直接指定
                    case "0":
                        ChangePoint(targetObj, int.Parse(command.parameters[2]), int.Parse(command.parameters[3]));
                        break;
                    //変数指定
                    case "1":
                        var runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();

                        var flagDataModel = _databaseManagementService.LoadFlags();
                        for (var i = 0; i < flagDataModel.variables.Count; i++)
                            if (flagDataModel.variables[i].id == command.parameters[2])
                            {
                                xValue = int.Parse(runtimeSaveDataModel.variables.data[i]);
                                break;
                            }

                        for (var i = 0; i < flagDataModel.variables.Count; i++)
                            if (flagDataModel.variables[i].id == command.parameters[3])
                            {
                                yValue = int.Parse(runtimeSaveDataModel.variables.data[i]);
                                break;
                            }

                        // 変数指定では、0以上の値をユーザーが設定するようなので符号を反転させる。
                        yValue = -yValue;

                        ChangePoint(targetObj, xValue, yValue);
                        break;
                    //他のイベントと交換
                    case "2":
                        //オブジェクトの取得
                        var exchangeObj = MapEventExecutionController.Instance.GetEventMapGameObject(command.parameters[5]);

                        //座標の取得
                        var exchangePosition = new Vector2(
                            exchangeObj.GetComponent<EventOnMap>().x_now, exchangeObj.GetComponent<EventOnMap>().y_now
                        );
                        var originPosition = new Vector2(
                            targetObj.GetComponent<EventOnMap>().x_now, targetObj.GetComponent<EventOnMap>().y_now
                        );

                        //交換
                        ChangePoint(exchangeObj, (int) originPosition.x, (int) originPosition.y);
                        ChangePoint(targetObj, (int) exchangePosition.x, (int) exchangePosition.y);
                        break;
                }

                //向きの変更
                if (command.parameters[4] != "0")
                {
                    var direction = CharacterMoveDirectionEnum.Max;
                    switch (command.parameters[4])
                    {
                        case "1":
                            direction = CharacterMoveDirectionEnum.Down;
                            break;
                        case "2":
                            direction = CharacterMoveDirectionEnum.Left;
                            break;
                        case "3":
                            direction = CharacterMoveDirectionEnum.Right;
                            break;
                        case "4":
                            direction = CharacterMoveDirectionEnum.Up;
                            break;
                    }

                    if (eventID == "-2")
                    {
                        var chara = targetObj.GetComponent<CharacterOnMap>();
                        chara.ChangeCharacterDirection(direction);
                    }
                    else
                        targetObj.GetComponent<EventOnMap>()
                            .ChangeCharacterDirection(direction);
                }

                //イベント終了
                ProcessEndAction();
                _endAction = true;
            }
            //移動ルートの指定
            else if (command.code == (int) EventEnum.EVENT_CODE_MOVE_SET_MOVE_POINT || 
                     command.code == (int) EventEnum.MOVEMENT_MOVE_AT_RANDOM ||
                     command.code == (int) EventEnum.MOVEMENT_MOVE_TOWARD_PLAYER ||
                     command.code == (int) EventEnum.MOVEMENT_MOVE_AWAY_FROM_PLAYER)
            {
                GameObject targetObj = null;

                //-2：プレイヤー
                //-1：このイベント
                //イベントid：指定のイベントID
                eventID = (command.parameters[0] == "-1") ? eventID : command.parameters[0];
                if (eventID == "-2")
                {
                    if (command.code == (int) EventEnum.MOVEMENT_MOVE_TOWARD_PLAYER ||
                        command.code == (int) EventEnum.MOVEMENT_MOVE_AWAY_FROM_PLAYER)
                    {
                        //対象がプレイヤーで、プレイヤーに近づく、遠ざかることは出来ないため、即終了する
                        ProcessEndAction();
                        _endAction = true;
                        return;
                    }

                    targetObj = MapManager.GetOperatingCharacterGameObject();

                    //現在移動途中であった場合には、移動後の座標に強制的に移動する
                    CharacterOnMap characterOnMap = MapManager.OperatingCharacter;
                    if (characterOnMap.IsMoving())
                    {
                        characterOnMap.MoveEnd();
                    }

                    List<ActorOnMap> operatingParty = MapManager.OperatingParty;
                    if (operatingParty != null)
                    {
                        for (int i = 0; i < operatingParty.Count; i++)
                        {
                            if (operatingParty[i].IsMoving())
                            {
                                operatingParty[i].MoveEnd();
                            }
                        }
                    }
                }
                else
                {
                    targetObj = MapEventExecutionController.Instance.GetEventMapGameObject(eventID);
                }

                //対象のオブジェクトが存在しない場合は処理終了
                if (targetObj == null)
                {
                    ProcessEndAction();
                    _endAction = true;
                    return;
                }

                //移動ルート指定であるにも関わらず、ルートが1つも登録されていない場合は処理終了
                if (command.code == (int) EventEnum.EVENT_CODE_MOVE_SET_MOVE_POINT && command.route.Count <= 0)
                {
                    ProcessEndAction();
                    _endAction = true;
                    return;
                }

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

                //移動処理
                moveSetMovePoint.SetMovePointProcess(
                    ProcessEndAction,
                    () => { moveSetMovePoint.UpdateMove(); },
                    command,
                    eventID,
                    eventID == "-2");

                //完了を待たない場合、EndActionを先に実行する
                if (command.parameters[9] == "0")
                {
                    ProcessEndAction();
                }
            }
        }

        //イベントの座標変更
        //座標を変更するイベントオブジェクト、X座標、Y座標が入ってくる
        private static void ChangePoint(GameObject obj, int x, int y) {
            //ループを考慮した座標に変換する
            Vector2 pos = MapManager.LoopInstance.MovePointLoopEvent(new Vector2(x, y));
            x = (int) pos.x;
            y = (int) pos.y;

            var actorOnMap = obj.GetComponent<ActorOnMap>();
            if (actorOnMap != null)
            {
                //Player
                actorOnMap.SetToPositionOnTile(new Vector2(x, y));
                //茂み情報の更新
                actorOnMap.GetComponent<CharacterOnMap>().ResetBush(false, false);
                //パーティメンバーも強制的に同じ座標に移動
                foreach (var partyIndex in Enumerable.Range(0, MapManager.GetPartyMemberNum()))
                {
                    MapManager.GetPartyGameObject(partyIndex).GetComponent<ActorOnMap>().SetToPositionOnTile(new Vector2(x, y));
                    //茂み情報の更新
                    MapManager.GetPartyGameObject(partyIndex).GetComponent<CharacterOnMap>().ResetBush(false, false);
                }
                return;
            }
            //通行判定の座標の更新
            obj.transform.position =
                MapManager.GetWorldPositionByTilePositionForRuntime(new Vector2(x, y)
                );
            //話しかけられる座標の更新
            obj.GetComponent<EventOnMap>().SetCurrentPositionOnTile(new Vector2(x, y));

            //表示座標の更新
            obj.GetComponent<EventOnMap>().x_now = x;
            obj.GetComponent<EventOnMap>().y_now = y;
            obj.GetComponent<EventOnMap>().x_next = x;
            obj.GetComponent<EventOnMap>().y_next = y;

            //茂み情報の更新
            obj.GetComponent<CharacterOnMap>().ResetBush(false, false);

            // Z座標更新
            obj.GetComponent<CharacterOnMap>().SetGameObjectPositionWithRenderingOrder(new Vector2(x, y));
        }

        private void ProcessEndAction() {
            // 「完了を待たない」設定の場合、移動開始時に呼ばているはずなのでそのチェック。
            if (_endAction == false)
            {
                // 次イベントコマンドへシーケンスを進める。
                SendBackToLauncher?.Invoke();
            }
        }
    }
}