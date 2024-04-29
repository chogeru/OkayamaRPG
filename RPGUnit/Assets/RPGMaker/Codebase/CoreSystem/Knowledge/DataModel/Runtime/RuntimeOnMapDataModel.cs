using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Character;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime
{
    [Serializable]
    public class RuntimeOnMapDataModel {
        public List<OnMapData> onMapDatas;

        [Serializable]
        public class OnMapData {
            public bool isActor;

            public string id;
            public string AssetId;

            public int x_next;
            public int x_now;
            public int y_next;
            public int y_now;

            public CharacterMoveDirectionEnum direction;
            public CharacterMoveDirectionEnum lastMoveDirectionEnum;
            public bool isLockDirection;
            public bool isAnimation;
            public bool isSteppingAnimation;
            public bool isThrough;

            public bool isValid;
            public float opacity = 1f;

            //イベント用
            public RuntimeOnMapEventDataModel eventData;

            //移動関連
            public RuntimeOnMapMoveDataModel moveData;

            public OnMapData Create(EventOnMap data) {
                var onMapData = new OnMapData();

                onMapData.isActor = false;
                onMapData.id = data.MapDataModelEvent.eventId;
                SetData(data, onMapData);
                SetEventData(data, onMapData);
                SetMoveData(data, onMapData);

                return onMapData;
            }

            public OnMapData Create(ActorOnMap data, int num) {
                var onMapData = new OnMapData();

                onMapData.isActor = true;
                onMapData.id = num.ToString();
                SetData(data, onMapData);

                return onMapData;
            }

            private void SetData(CharacterOnMap character, OnMapData onMapData) {
                onMapData.AssetId = character.AssetId;
                onMapData.x_next = character.x_next;
                onMapData.x_now = character.x_now;
                onMapData.y_next = character.y_next;
                onMapData.y_now = character.y_now;
                onMapData.direction = character.GetCurrentDirection();
                onMapData.lastMoveDirectionEnum = character.GetLastMoveDirection();
                onMapData.isLockDirection = character.GetIsLockDirection();
                onMapData.isAnimation = character.GetAnimation();
                onMapData.isSteppingAnimation = character.GetStepAnimation();
                onMapData.isThrough = character.GetCharacterThrough();
                onMapData.opacity = character.GetCharacterOpacity();
            }

            private void SetEventData(EventOnMap character, OnMapData onMapData) {
                onMapData.eventData = character.GetMapEventData();
            }

            private void SetMoveData(EventOnMap character, OnMapData onMapData) {
                MoveSetMovePoint data = character.GetComponent<MoveSetMovePoint>();
                if (data != null)
                {
                    onMapData.moveData = data.GetMapMoveData();
                }
            }
        }

        [Serializable]
        public class RuntimeOnMapEventDataModel {
            //現在のイベントページ番号
            public int page;
            //イベント実行中かどうか
            public int isExecute;
            //イベント実行の種別（1:決定キーor接触、2:自動実行、3:並列処理）
            public int runningType;
            //イベントの実行index
            public int index;
        }

        [Serializable]
        public class RuntimeOnMapMoveDataModel
        {
            public EventDataModel.EventCommand command;
            public int moveKind;
            public int moveSpeed;
            public int index;
            public int indexMax;
            public bool repeatOperation;
            public bool moveSkip;
            public bool moveSkipNext;

            public EventDataModel.EventCommand defaultCommand;
            public int defaultMoveKind;
            public int defaultMoveSpeed;
            public int defaultIndex;
            public int defaultIndexMax;
            public bool defaultRepeatOperation;
            public bool defaultSave;
            public bool defaultMoveSkip;
        }

        public RuntimeOnMapDataModel() {
            onMapDatas = new List<OnMapData>();
        }
    }
}