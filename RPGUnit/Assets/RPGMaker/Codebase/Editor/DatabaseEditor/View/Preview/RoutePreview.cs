using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Character;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.View.Preview
{
    public class RoutePreview : AbstractPreview
    {
        EditorCoroutine _animCoroutine = null;
        bool _stepAnim = false;
        bool _moveAnim = false;
        bool _step = false;
        bool _move = false;

        public void CreateMap(MapDataModel mapDataModel, Vector2 pos, List<EventMoveEnum> codeList, string eventId) {
            _isAnimation = false;
            MapDataModel = mapDataModel;
            _eventId = eventId;
            _nowPos = pos;
            //_nextPos = nextPos;
            _codeList = codeList;
            _mapPrefab = MapDataModel.EditorDirectLoadMapPrefab();
        }

        public void SetTargetId(string targetId) {
            _targetId = targetId;
            _characterGraphic.ChangeAsset(GetAssetId());
        }

        // 対象ID設定
        protected override string GetAssetId() {
            var database = Editor.Hierarchy.Hierarchy.databaseManagementService;
            var eventManage = new EventManagementService();

            var assetId = "";
            // プレイヤー
            if (_targetId == "-2" &&
                database.LoadSystem().initialParty.party.Count > 0)
            {
                var characters = database.LoadCharacterActor();
                for (int i = 0; i < characters.Count; i++)
                    if (characters[i].uuId == database.LoadSystem().initialParty.party[0])
                    {
                        assetId = characters[i].image.character;
                        break;
                    }
            }
            // このイベント
            else if (_targetId == "-1")
            {
                var eventMaps = eventManage.LoadEventMap();
                EventMapDataModel data = null;
                for (int i = 0; i < eventMaps.Count; i++)
                    if (eventMaps[i].eventId == _eventId)
                    {
                        data = eventMaps[i];
                        break;
                    }

                if (data != null)
                {
                    if (data.pages[0].condition.image.enabled == 1)
                        assetId = data.pages[0].image.sdName;
                    else
                    {
                        var characters = database.LoadCharacterActor();
                        for (int i = 0; i < characters.Count; i++)
                            if (characters[i].uuId == data.pages[0].condition.image.imageName)
                            {
                                assetId = characters[i].image.character;
                                break;
                            }
                    }
                }
            }
            // 指定イベント
            else
            {
                var eventMaps = eventManage.LoadEventMap();
                EventMapDataModel data = null;
                for (int i = 0; i < eventMaps.Count; i++)
                    if (eventMaps[i].eventId == _targetId)
                    {
                        data = eventMaps[i];
                        break;
                    }

                if (data != null)
                {
                    if (data.pages[0].condition.image.enabled == 1)
                        assetId = data.pages[0].image.sdName;
                    else
                    {
                        var characters = database.LoadCharacterActor();
                        for (int i = 0; i < characters.Count; i++)
                            if (characters[i].uuId == data.pages[0].condition.image.imageName)
                            {
                                assetId = characters[i].image.character;
                                break;
                            }
                    }
                }
            }
            return assetId;
        }

        public override void SetMoveType(int moveType) {
            _moveType = moveType;
        }

        public override void SetSpeed(Commons.SpeedMultiple.Id speedMultipleId) {
            _speed = MoveSetMovePoint.GetMoveSpeed(speedMultipleId) / Commons.Fps;
        }

        public override void SetMoveFrequencyWaitTime(int moveFrequencyIndex) {
            _moveFrequencyWaitTime = MoveSetMovePoint.GetMoveFrequencyWaitTime(moveFrequencyIndex);
        }

        public override void SetDirectionType(Commons.Direction.Id directionType) {
            _directionId = directionType;
        }

        public override void SetAnimation(int animation) {
            if (_animCoroutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(_animCoroutine);
                _animCoroutine = null;
            }

            switch (animation)
            {
                // 歩行、足踏み
                case 0:
                    StartAnimation();
                    _stepAnim = false;
                    _moveAnim = true;
                    break;
                case 1:
                    StartAnimation();
                    _stepAnim = true;
                    _moveAnim = false;
                    break;
                // 無し
                case 2:
                    _characterGraphic.StopTexture();
                    _isAnimation = false;
                    break;
                case 3:
                    StartAnimation();
                    _stepAnim = true;
                    _moveAnim = true;
                    break;
            }
        }

        private void StartAnimation() {
            _isAnimation = true;
            _animCoroutine = EditorCoroutineUtility.StartCoroutine(SteppingAnimation(), this);
        }

        protected override IEnumerator SteppingAnimation() {
            while (_isAnimation)
            {
                if ((_step && _stepAnim) || (_move && _moveAnim))
                {
                    _characterGraphic?.StepAnimation();
                    _sceneWindow.Render();
                }
                else
                    _characterGraphic?.StopTexture();

                yield return new EditorWaitForSeconds(1f / Commons.Fps);
            }
        }

        /// <summary>
        ///     初期状態のUI設定
        /// </summary>
        public override void InitUi(SceneWindow scene, bool isChange = false) {
            _map = MapDataModel.InstantiateMapPrefab(MapDataModel, _mapPrefab);
            _map.transform.localPosition = new Vector3(0f, 0f, 0f);
            _map.transform.localScale = Vector3.one * 1f;
            _sceneWindow = scene;
            SetUp();
            // プレビューシーンに移動
            scene.MoveGameObjectToPreviewScene(_map);
            SetCamera();
        }
        
        protected override void SetUp() {
            _actorOnMap = new GameObject();
            _actorOnMap.gameObject.transform.SetParent(_map.transform);
            _map.transform.position = new Vector3(-0.25f, 0f, 10f);

            var charaObj = new GameObject();
            var canvas = charaObj.AddComponent<Canvas>();
            canvas.sortingLayerName = "Editor_Event";
            _characterGraphic = charaObj.AddComponent<CharacterGraphic>();
            _characterGraphic.gameObject.transform.SetParent(_actorOnMap.transform);
            _characterGraphic.Init(GetAssetId());
            _characterGraphic.gameObject.transform.position = new Vector3(0f, 0f, 0f);
            //マップの裏に表示されるため、-3にしておく
            _actorOnMap.transform.position = new Vector3(_nowPos.x, _nowPos.y, -3f);
            _sceneWindow.Render();

            index = 0;
            _nextPos = currentPos;
            _moveFrequencyWaitStartTime = System.DateTime.Now;
        }
        
        public override void Update() {
            if (currentPos != _nextPos)
            {
                // 次のタイルへ移動中。
                var newPos = Vector2.MoveTowards(currentPos, _nextPos, _speed);
                _actorOnMap.transform.position = new Vector3(newPos.x, newPos.y, _actorOnMap.transform.position.z);
                SetDirection();
                _moveFrequencyWaitStartTime = System.DateTime.Now;
            }
            else if (index > 0 &&
                     (System.DateTime.Now - _moveFrequencyWaitStartTime).TotalSeconds < _moveFrequencyWaitTime)
            {
                // 『移動頻度』待ち時間。
                _move = false;
                _step = true;
            }
            else if (index < _codeList.Count)
            {
                // 次の移動先を設定。
                SetMoveToNextTile(_codeList[index]);
                index++;

                SetDirection();
                _move = true;
                _step = false;
            }

            base.Update();
        }
        

        // 隣のタイルへの移動を設定。
        private void SetMoveToNextTile(EventMoveEnum eventMoveEnum)
        {
            switch (eventMoveEnum)
            {
                case EventMoveEnum.MOVEMENT_MOVE_UP:
                    _characterGraphic.ChangeDirection(CharacterMoveDirectionEnum.Up);
                    _nextPos = _actorOnMap.transform.position + Vector3.up;
                    break;
                case EventMoveEnum.MOVEMENT_MOVE_LEFT:
                    _characterGraphic.ChangeDirection(CharacterMoveDirectionEnum.Left);
                    _nextPos = _actorOnMap.transform.position + Vector3.left;
                    break;
                case EventMoveEnum.MOVEMENT_MOVE_RIGHT:
                    _characterGraphic.ChangeDirection(CharacterMoveDirectionEnum.Right);
                    _nextPos = _actorOnMap.transform.position + Vector3.right;
                    break;
                case EventMoveEnum.MOVEMENT_MOVE_DOWN:
                    _characterGraphic.ChangeDirection(CharacterMoveDirectionEnum.Down);
                    _nextPos = _actorOnMap.transform.position + Vector3.down;
                    break;
            }
        }

        public override void DestroyLocalData() {
            //以下の破棄処理は、Unity側で既に破棄済みである可能性があるため try catch で括る
            if (_scrollChanvas != null) try { Object.DestroyImmediate(_scrollChanvas); } catch (System.Exception) { }
            if (_scrollText != null) try { Object.DestroyImmediate(_scrollText); } catch (System.Exception) { }
            if (_map != null) try { Object.DestroyImmediate(_map); } catch (System.Exception) { }
            if (_actorOnMap != null) try { Object.DestroyImmediate(_actorOnMap); } catch (System.Exception) { }
            _characterGraphic = null;
            _scrollChanvas = null;
            _scrollText = null;
            _mapPrefab = null;
            _map = null;
            _actorOnMap = null;
        }
    }
}