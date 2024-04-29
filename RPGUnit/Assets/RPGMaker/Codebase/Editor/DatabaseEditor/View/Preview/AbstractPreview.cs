using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Character;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.View.Preview
{
    public class AbstractPreview
    {
        protected readonly Vector2              _offset = new Vector2(1, 1);
        protected          GameObject           _actorOnMap;
        protected          CharacterGraphic     _characterGraphic;
        protected          List<EventMoveEnum>  _codeList = new List<EventMoveEnum>();
        protected          string               _targetId;
        protected          bool                 _isAnimation;
        protected          bool                 _isAnimationCoroutine;
        protected          GameObject           _map;
        protected          GameObject           _mapPrefab;
        protected          int                  _moveType;
        protected          Vector2              _nextPos;
        protected          Vector2              _nowPos;
        protected          SceneWindow          _sceneWindow;
        protected          GameObject           _scrollChanvas;
        protected          Text                 _scrollText;
        protected          float                _speed;
        protected          float                _moveFrequencyWaitTime;
        protected          Commons.Direction.Id _directionId;
        protected          string               _eventId;
        protected string _assetId;

        protected System.DateTime _moveFrequencyWaitStartTime;

        protected          int          index;
        protected          MapDataModel MapDataModel;
        protected readonly float        screenAspect = 0;
        protected Vector2 currentPos => _actorOnMap.transform.position;


        
        // 対象ID設定
        protected virtual string GetAssetId() {
            return "";
        }

        public virtual void SetMoveType(int moveType) {
            _moveType = moveType;
        }

        public virtual void SetSpeed(Commons.SpeedMultiple.Id speedMultipleId) {
            _speed = MoveSetMovePoint.GetMoveSpeed(speedMultipleId) / Commons.Fps;
        }

        public virtual void SetMoveFrequencyWaitTime(int moveFrequencyIndex) {
            _moveFrequencyWaitTime = MoveSetMovePoint.GetMoveFrequencyWaitTime(moveFrequencyIndex);
        }

        public virtual void SetDirectionType(Commons.Direction.Id directionType) {
            _directionId = directionType;
        }

        public virtual void SetAnimation(int animation) {
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
                    _characterGraphic.StopTexture();
                    break;
            }
        }

        protected virtual IEnumerator SteppingAnimation() {
            if (_isAnimationCoroutine) yield break;
            _isAnimationCoroutine = true;
            while (_isAnimation)
            {
                if (_characterGraphic == null)
                {
                    _isAnimationCoroutine = false;
                    yield break;
                }
                _characterGraphic.StepAnimation();
                _sceneWindow.Render();
                yield return new EditorWaitForSeconds(1f / Commons.Fps);
            }
        }

        /// <summary>
        ///     初期状態のUI設定
        /// </summary>
        public virtual void InitUi(SceneWindow scene, bool isChange = false) {
        }

        public virtual VisualElement CreateUi() {
            var container = new VisualElement();
            container.style.display = DisplayStyle.Flex;
            container.style.flexDirection = FlexDirection.Row;
            container.SendToBack();

            // 再生ボタン
            var btnResetCam = new Button {text = EditorLocalize.LocalizeText("閉じる")};
            btnResetCam.clicked += () =>
            {
                WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.MapEventEditWindow);
            };
            container.Add(btnResetCam);
            return container;
        }

        protected virtual void SetUp() {
        }
        
        public virtual void Update() {
            _sceneWindow.Render();
        }

        protected virtual void SetDirection()
        {
            var direction = Commons.Direction.GetCharacterMoveDirection(_directionId, null, null);
            if (direction == CharacterMoveDirectionEnum.None)
            {
                return;
            }

            if (_characterGraphic != null) _characterGraphic.ChangeDirection(direction);
        }

        public virtual void DestroyLocalData() {
            //以下の破棄処理は、Unity側で既に破棄済みである可能性があるため try catch で括る
            if (_scrollChanvas != null) try { Object.DestroyImmediate(_scrollChanvas); } catch (System.Exception) { }
            if (_scrollText != null) try { Object.DestroyImmediate(_scrollText); } catch (System.Exception) { }
            if (_map != null) try { Object.DestroyImmediate(_map); } catch (System.Exception) { }
            if (_actorOnMap != null) try { Object.DestroyImmediate(_actorOnMap); } catch (System.Exception) { }
            _scrollChanvas = null;
            _scrollText = null;
            _mapPrefab = null;
            _characterGraphic = null;
            _map = null;
            _actorOnMap = null;
        }

        protected virtual void SetCamera() {
            if (MapDataModel.width <= 0) MapDataModel.width = 15;

            if (MapDataModel.height <= 0) MapDataModel.height = 15;
            // ２点間のベクトルを取得
            var targetsVector =
                AbsPositionDiff(new Vector3(0, 0), new Vector3(MapDataModel.width, MapDataModel.height * -1)) +
                (Vector3) _offset;

            // アスペクト比が縦長ならyの半分、横長ならxとアスペクト比でカメラのサイズを更新
            var targetsAspect = targetsVector.y / targetsVector.x;
            float targetOrthographicSize = 0;
            if (screenAspect < targetsAspect)
                targetOrthographicSize = targetsVector.y * 0.5f;
            else
                targetOrthographicSize = targetsVector.x * (1 / _sceneWindow.Camera.aspect) * 0.5f;

            _sceneWindow.Camera.orthographicSize = targetOrthographicSize;
            _sceneWindow.Camera.gameObject.transform.position =
                new Vector3(
                    MapDataModel.width / 2f,
                    -MapDataModel.height / 2f + MapManagementService.YPositionOffsetToMapTile,
                    -20f);
        }

        protected virtual Vector3 AbsPositionDiff(Vector3 target1, Vector3 target2) {
            var targetsDiff = target1 - target2;
            return new Vector3(Mathf.Abs(targetsDiff.x), Mathf.Abs(targetsDiff.y));
        }
    }
}