using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Character;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.View.Preview
{
    /// <summary>
    /// イベントコマンド『ジャンプ』のプレビュー。
    /// </summary>
    public class JumpPreview : AbstractPreview
    {
        private bool    _isPlay;
        private float   _maxJumpHeight;
        private float   _moveRate;
        private Commons.SpeedMultiple.Id
            _speedMultipleId;
        private          EventDataModel _eventDataModel;

        public void CreateMap(MapDataModel mapDataModel, Vector2 pos, Vector2 nextPos, EventDataModel eventDataModel) {
            _isAnimation = false;
            _isPlay = false;
            MapDataModel = mapDataModel;
            _eventDataModel = eventDataModel;
            _nowPos = pos;
            _nextPos = nextPos;
            _mapPrefab = MapDataModel.EditorDirectLoadMapPrefab();
        }

        public void SetTargetId(string targetId) {
            _targetId = targetId;
            _characterGraphic?.ChangeAsset(GetAssetId());
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
                    if (eventMaps[i].eventId == _eventDataModel.id)
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
            _speedMultipleId = speedMultipleId;
            _speed = Jump.GetMoveSpeed(speedMultipleId);
        }

        public void SetDirection(Commons.Direction.Id directionId) {
            var characterMoveDirection =
                directionId switch
                {
                    Commons.Direction.Id.NowDirection => Commons.GetDirection(_nowPos, _nextPos), 
                    Commons.Direction.Id.Player => CharacterMoveDirectionEnum.None,
                    _ => Commons.Direction.GetCharacterMoveDirection(directionId, null, null)
                };
            if (characterMoveDirection != CharacterMoveDirectionEnum.None)
            {
                _characterGraphic?.ChangeDirection(characterMoveDirection);
            }
        }

        public override void SetAnimation(int animation) {
            switch (animation)
            {
                // 歩行、足踏み
                case 0:
                case 1:
                    _isAnimation = true;
                    EditorCoroutineUtility.StartCoroutine(SteppingAnimation(), this);
                    break;
                // 無し
                case 2:
                    _characterGraphic?.StopTexture();
                    break;
            }
        }

        protected override IEnumerator SteppingAnimation() {
            if (_isAnimationCoroutine) yield break;
            _isAnimationCoroutine = true;
            while (_isAnimation)
            {
                _characterGraphic?.StepAnimation();
                _sceneWindow.Render();
                yield return new EditorWaitForSeconds(0.1f);
            }
        }

        /// <summary>
        ///     初期状態のUI設定
        /// </summary>
        public override void InitUi(SceneWindow scene, bool isChange = false) {
            _map = MapDataModel.InstantiateMapPrefab(MapDataModel, _mapPrefab);
            SetUp();
            // プレビューシーンに移動
            scene.MoveGameObjectToPreviewScene(_map);
            _sceneWindow = scene;
            SetCamera();
        }
        
        protected override void SetUp() {
            _actorOnMap = new GameObject();
            _actorOnMap.gameObject.transform.SetParent(_map.transform);
            _actorOnMap.transform.position = _nowPos;

            var charaObj = new GameObject();
            var canvas = charaObj.AddComponent<Canvas>();
            canvas.sortingLayerName = "Editor_Event";
            _characterGraphic = charaObj.AddComponent<CharacterGraphic>();
            _characterGraphic.gameObject.transform.SetParent(_actorOnMap.transform);
            _characterGraphic.Init(GetAssetId());

            AnimationPlayForEditor();
        }

        private void AnimationPlayForEditor() {
            // 座標をMAPに合わせる
            var layers = MapDataModel.MapPrefabManagerForEditor.layers;
            _nowPos = layers[(int) MapDataModel.Layer.LayerType.A]
                .tilemap.CellToWorld(new Vector3Int((int) _nowPos.x, (int) _nowPos.y, -3));
            _nextPos = layers[(int) MapDataModel.Layer.LayerType.A]
                .tilemap.CellToWorld(new Vector3Int((int) _nextPos.x, (int) _nextPos.y, -3));

            //マップの裏に表示されるため、-3にしておく
            _actorOnMap.transform.position = new Vector3(_nowPos.x, _nowPos.y, -3f);

            // 座標:その場
            if (_moveType == 1)
            {
                _nextPos = _nowPos;
            }

            _speed = Jump.GetMoveSpeed(_speedMultipleId);

            _maxJumpHeight = Jump.GetMaxJumpHeight(Vector2.Distance(_nowPos, _nextPos));

            _moveRate = 0;
            _isPlay = true;
        }

        public override void Update() {
            if (!_isPlay)
            {
                return;
            }

            if (_moveRate < 1f)
            {
                _moveRate = System.Math.Min(_moveRate + _speed, 1f);
                var newPos = Vector3.Lerp(_nowPos, _nextPos, _moveRate);
                var jumpHeight = Mathf.Sin(Mathf.PI * _moveRate) * _maxJumpHeight;
                _actorOnMap.transform.position =
                    new Vector3(newPos.x, newPos.y + jumpHeight, _actorOnMap.transform.position.z);
                _sceneWindow.Render();
            }
        }

        public override void DestroyLocalData() {
            if (_scrollChanvas != null) Object.DestroyImmediate(_scrollChanvas);
            if (_scrollText != null) Object.DestroyImmediate(_scrollText);
            if (_map != null) Object.DestroyImmediate(_map);
            if (_actorOnMap != null) Object.DestroyImmediate(_actorOnMap);
            _characterGraphic = null;
            _scrollChanvas = null;
            _scrollText = null;
            _mapPrefab = null;
            _map = null;
            _actorOnMap = null;
            _isAnimation = false;
        }
    }
}