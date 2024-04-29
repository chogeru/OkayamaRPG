using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud.Character
{
    public class ShowIcon : MonoBehaviour
    {
        private const float AnimationSpeedScale = 1f / 60;

        private Action _endEventAction;
        private Action<ShowIcon> _closeShowIconAction;
        private AssetManageDataModel.ImageSetting _imageSetting;
        private List<Sprite> _animeSprites;
        private int _frameCount;

        /// <summary>
        ///     フキダシアイコンテクスチャを読み込んでアニメ用スプライト列を生成する。
        /// </summary>
        /// <param name="imageSetting">フキダシアイコン画像設定</param>
        /// <returns>スプライト列</returns>
        private static List<Sprite> CreateSpritesFromLoadBaloonIconTexture(
            AssetManageDataModel.ImageSetting imageSetting
        ) {
            var animeSprites = new List<Sprite>();

            var texture = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                "Assets/RPGMaker/Storage/Images/System/Balloon/" + imageSetting.path);

            var frameWidth = texture.width / imageSetting.animationFrame;
            var frameHeight = texture.height;
            foreach (var frameIndex in Enumerable.Range(0, imageSetting.animationFrame))
            {
                var sprite = Sprite.Create(
                    texture,
                    new Rect(frameWidth * frameIndex, 0, frameWidth, frameHeight),
                    new Vector2(0.5f, 0f));
                animeSprites.Add(sprite);
            }

            return animeSprites;
        }

        /// <summary>
        ///     フキダシアイコンを対象GameObjectの上に配置する。
        /// </summary>
        /// <param name="targetGo">プキダシアイコンを表示する対象のGameObject</param>
        /// <param name="balloonIconGo">プキダシアイコンGameObject</param>
        /// <param name="imageRect">プキダシアイコン画像の横縦サイズ</param>
        /// <returns>配置されたフキダシアイコンGameObject</returns>
        private static void PlaceBalloonIcon(GameObject targetGo, GameObject balloonIconGo, Rect imageRect) {
            var actorTransform = targetGo.transform.Find("actor");

            var balloonIconSpriteRenderer = balloonIconGo.AddComponent<SpriteRenderer>();
            balloonIconSpriteRenderer.sortingLayerID = 
                UnityUtil.SortingLayerManager.GetId(UnityUtil.SortingLayerManager.SortingLayerIndex.Runtime_MapBalloon);

            balloonIconGo.transform.SetParent(actorTransform);

            var balloonIconRectTransform = balloonIconGo.AddComponent<RectTransform>();

            // サイズ設定。
            balloonIconRectTransform.sizeDelta =
                new Vector2(imageRect.width, imageRect.height) /
                TileRepository.TileDefaultSize;
            var actorRectTransformLocalScale = actorTransform.GetComponent<RectTransform>().localScale;
            balloonIconRectTransform.localScale =
                new Vector2(1f / actorRectTransformLocalScale.x, 1f / actorRectTransformLocalScale.y);

            // 位置設定 (actorの上に表示)。
            balloonIconRectTransform.pivot = new Vector2(0.5f, 0f);
            balloonIconRectTransform.anchorMin = new Vector2(0.5f, 1f);
            balloonIconRectTransform.anchorMax = new Vector2(0.5f, 1f);
            balloonIconRectTransform.anchoredPosition3D = Vector3.zero;
        }

        public void Init() {
        }

        public void PlayAnimation(
            Action endEventAction,
            Action<ShowIcon> closeShowIconAction,
            string eventId,
            string popupIconAssetId,
            bool isEventProgressWait,
            string currentEventID
        ) {
            DebugUtil.Log($"eventId={eventId}");

            var targetGo = new Commons.TargetCharacter(eventId, currentEventID).GetGameObject();

            // 対象が存在しない？
            if (targetGo == null)
            {
                endEventAction();
                closeShowIconAction(this);
                return;
            }

            AssetManageDataModel popupIconAssetManageDataModel =
                new DatabaseManagementService().LoadPopupIconAssets()
                    .ForceSingle(asset => asset.id == popupIconAssetId);
            AssetManageDataModel.ImageSetting imageSetting = popupIconAssetManageDataModel.imageSettings[0];

            List<Sprite> animeSprites = CreateSpritesFromLoadBaloonIconTexture(imageSetting);
            PlaceBalloonIcon(targetGo, gameObject, animeSprites[0].rect);

            _endEventAction = isEventProgressWait ? endEventAction : null;
            _closeShowIconAction = closeShowIconAction;
            _imageSetting = imageSetting;
            _animeSprites = animeSprites;
            _frameCount = 0;

            gameObject.SetActive(false);
            gameObject.GetComponent<SpriteRenderer>().sprite = _animeSprites[_frameCount];
            TimeHandler.Instance.AddTimeAction(
                imageSetting.animationSpeed * AnimationSpeedScale, SurveillanceAnimationCoroutine, true);

            //最初のアニメーション表示
            SurveillanceAnimationCoroutine();

            // 完了までイベント進行を待たない？
            if (!isEventProgressWait) endEventAction();
        }

        private void SurveillanceAnimationCoroutine() {
            try
            {
                if (_frameCount < _animeSprites.Count)
                {
                    gameObject.SetActive(true);
                    gameObject.GetComponent<SpriteRenderer>().sprite = _animeSprites[_frameCount];
                    _frameCount++;
                }
                else
                {
                    TimeHandler.Instance.RemoveTimeAction(SurveillanceAnimationCoroutine);
                    _endEventAction?.Invoke();
                    _closeShowIconAction(this);
                }
            }
            catch (Exception) {
                //フキダシアイコン表示中にマップ移動時、このオブジェクト自体が破棄されている
                //nullチェック等での判定が出来ないため、ここで破棄済みの場合の処理を実施する
                TimeHandler.Instance.RemoveTimeAction(SurveillanceAnimationCoroutine);
            }
        }
    }
}