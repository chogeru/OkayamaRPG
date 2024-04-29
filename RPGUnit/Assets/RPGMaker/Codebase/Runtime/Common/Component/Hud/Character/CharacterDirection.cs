using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Runtime.Map;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using System.Collections.Generic;
using UnityEngine; //バトルでは本コマンドは利用しない

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud.Character
{
    /// <summary>
    ///     キャラクターの座標が必要
    ///     キャラクターの画像を変える必要がある
    /// </summary>
    public class CharacterDirection : MonoBehaviour
    {
        // const
        //--------------------------------------------------------------------------------------------------------------
        private const string PrefabPath =
            "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/ShowIcon.prefab";

        // コアシステムサービス
        //--------------------------------------------------------------------------------------------------------------
        private EventManagementService _eventManagementService;
        
        // initialize methods
        //--------------------------------------------------------------------------------------------------------------
        /**
         * 初期化
         */
        public void Init() {
            _eventManagementService = new EventManagementService();
        }

        public void SetDirection(string eventId, int direction, bool lockDirection, string currentEventID) {
            Init();
            GameObject obj = null;
            GameObject actor = null;
            var rand = 0;
            CharacterMoveDirectionEnum currentDirectionEnum;
            var directions = new List<CharacterMoveDirectionEnum>();
            var directionIndex = 0;
            //上から右回りに値を入れる
            directions.Add(CharacterMoveDirectionEnum.Up);
            directions.Add(CharacterMoveDirectionEnum.Right);
            directions.Add(CharacterMoveDirectionEnum.Down);
            directions.Add(CharacterMoveDirectionEnum.Left);

            eventId = (eventId == "-1") ? currentEventID : eventId;
            if (eventId == "-2") //プレイヤーの座標に出す
                obj = MapManager.GetOperatingCharacterGameObject();
            else
                obj = MapEventExecutionController.Instance.GetEventMapGameObject(eventId);

            currentDirectionEnum = obj.GetComponent<CharacterOnMap>().GetCurrentDirection();

            //今の向き
            if (currentDirectionEnum == CharacterMoveDirectionEnum.Up) directionIndex = 0;
            else if (currentDirectionEnum == CharacterMoveDirectionEnum.Right) directionIndex = 1;
            else if (currentDirectionEnum == CharacterMoveDirectionEnum.Down) directionIndex = 2;
            else if (currentDirectionEnum == CharacterMoveDirectionEnum.Left) directionIndex = 3;


            switch (direction)
            {
                case 0:
                    obj.GetComponent<CharacterOnMap>().ChangeCharacterDirection(CharacterMoveDirectionEnum.Down, true);
                    break;
                case 1:
                    obj.GetComponent<CharacterOnMap>().ChangeCharacterDirection(CharacterMoveDirectionEnum.Left, true);
                    break;
                case 2:
                    obj.GetComponent<CharacterOnMap>().ChangeCharacterDirection(CharacterMoveDirectionEnum.Right, true);
                    break;
                case 3:
                    obj.GetComponent<CharacterOnMap>().ChangeCharacterDirection(CharacterMoveDirectionEnum.Up, true);
                    break;
                // 右を向く
                case 4:
                    obj.GetComponent<CharacterOnMap>()
                        .ChangeCharacterDirection(directions[(directionIndex + 1) % 4], true);
                    break;
                // 左を向く
                case 5:
                    directionIndex = directionIndex - 1 < 0 ? 3 : directionIndex - 1;
                    obj.GetComponent<CharacterOnMap>().ChangeCharacterDirection(directions[directionIndex], true);
                    break;
                case 6:
                    obj.GetComponent<CharacterOnMap>()
                        .ChangeCharacterDirection(directions[(directionIndex + 2) % 4], true);
                    break;
                // 右か左
                case 7:
                    rand = Random.Range(0, 2);
                    if (rand % 2 == 1)
                    {
                        obj.GetComponent<CharacterOnMap>()
                            .ChangeCharacterDirection(directions[(directionIndex + 1) % 4], true);
                    }
                    else
                    {
                        directionIndex = directionIndex - 1 < 0 ? 3 : directionIndex - 1;
                        obj.GetComponent<CharacterOnMap>().ChangeCharacterDirection(directions[directionIndex], true);
                    }

                    break;
                case 8:
                    rand = Random.Range(0, 4);
                    obj.GetComponent<CharacterOnMap>().ChangeCharacterDirection(directions[rand], true);
                    break;
                case 9:
                    actor = MapManager.GetOperatingCharacterGameObject();
                    if (obj.transform.localPosition != actor.transform.localPosition)
                    {
                        if (obj.transform.localPosition.x < actor.transform.localPosition.x)
                            obj.GetComponent<CharacterOnMap>()
                                .ChangeCharacterDirection(CharacterMoveDirectionEnum.Right, true);
                        else
                            obj.GetComponent<CharacterOnMap>()
                                .ChangeCharacterDirection(CharacterMoveDirectionEnum.Left, true);
                        if (obj.transform.localPosition.x == actor.transform.localPosition.x)
                        {
                            if (obj.transform.localPosition.y < actor.transform.localPosition.y)
                                obj.GetComponent<CharacterOnMap>()
                                    .ChangeCharacterDirection(CharacterMoveDirectionEnum.Up, true);
                            else
                                obj.GetComponent<CharacterOnMap>()
                                    .ChangeCharacterDirection(CharacterMoveDirectionEnum.Down, true);
                        }
                    }

                    break;
                case 10:
                    actor = MapManager.GetOperatingCharacterGameObject();
                    if (obj.transform.localPosition != actor.transform.localPosition)
                    {
                        if (obj.transform.localPosition.x < actor.transform.localPosition.x)
                            obj.GetComponent<CharacterOnMap>()
                                .ChangeCharacterDirection(CharacterMoveDirectionEnum.Left, true);
                        else
                            obj.GetComponent<CharacterOnMap>()
                                .ChangeCharacterDirection(CharacterMoveDirectionEnum.Right, true);
                        if (obj.transform.localPosition.x == actor.transform.localPosition.x)
                        {
                            if (obj.transform.localPosition.y < actor.transform.localPosition.y)
                                obj.GetComponent<CharacterOnMap>()
                                    .ChangeCharacterDirection(CharacterMoveDirectionEnum.Down, true);
                            else
                                obj.GetComponent<CharacterOnMap>()
                                    .ChangeCharacterDirection(CharacterMoveDirectionEnum.Up, true);
                        }
                    }

                    break;
            }

            obj.GetComponent<CharacterOnMap>().SetIsLockDirection(lockDirection);

            if (eventId == "-2")
            {
                //プレイヤーの場合、パーティメンバーにも lockDirection の設定を適用する
                MapManager.OperatingActor.GetComponent<CharacterOnMap>().SetIsLockDirection(lockDirection);
                for (int i = 0; i <  MapManager.PartyOnMap?.Count; i++)
                {
                    MapManager.PartyOnMap[i].GetComponent<CharacterOnMap>().SetIsLockDirection(lockDirection);
                }
            }
        }
    }
}