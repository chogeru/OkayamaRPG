using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement.Repository;
using RPGMaker.Codebase.Editor.Inspector.Map.View;
using RPGMaker.Codebase.Editor.MapEditor.Window.MapEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.Canvas
{
    /// <summary>
    /// マップを表示する機能だけ持つキャンバスコンポーネント
    /// </summary>
    public class MapCanvas : VisualElement, IDisposable
    {
        // const

        // マップ内の1タイルの横および縦の表示サイズ。
        public const float EditorTileDisplaySizeInMap = 1f;

        protected const int EditorMapTileSize = 100;

        protected const int   DefaultOrthographicSize = 10;
        protected const float DragMoveMultiple = 2f;

        // データプロパティ
        protected MapDataModel MapDataModel;
        protected int          CurrentOrthographicSize;

        // 状態プロパティ
        protected bool IsShiftOn;
        protected bool IsAltOn;
        protected bool IsMouseDown;
        protected bool IsRightClick;

        // 関数プロパティ
        protected readonly Action RepaintMethod;

        // UI要素プロパティ
        protected Scene         PreviewScene;
        protected Camera        Camera;
        protected GameObject    CameraContainer;
        protected RenderTexture RenderTexture;
        protected Image         RenderTextureCanvasImage;

        private readonly Vector2 _offset      = new Vector2(1, 1);
        private readonly float   screenAspect = 0;

        // 直前に表示していたマップのカメラ情報
        private static MapDataModel _lastMapDataModel;
        private static int _lastCurrentOrthographicSize;
        private static Vector3 _lastCameraPosition;
        private static float _lastCameraOrthographicSize;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mapDataModel"></param>
        /// <param name="repaintMethod"></param>
        /// <param name="isSampleMap"></param>
        public MapCanvas(MapDataModel mapDataModel, Action repaintMethod, bool isSampleMap = false) {
            MapDataModel = mapDataModel;
            RepaintMethod = repaintMethod;
            InitUI(isSampleMap);
            Refresh();
        }

        public void Dispose() {
            if (MapDataModel != null)
            {
                _lastMapDataModel = MapDataModel;
                _lastCurrentOrthographicSize = CurrentOrthographicSize;
                _lastCameraPosition = Camera.gameObject.transform.position;
                _lastCameraOrthographicSize = Camera.orthographicSize;

                UnloadPrefabAndLayersForEditor();

                Object.DestroyImmediate(CameraContainer);
                Object.DestroyImmediate(Camera);

                EditorSceneManager.ClosePreviewScene(PreviewScene);

                MapDataModel = null;

                void UnloadPrefabAndLayersForEditor()
                {
                    MapDataModel.MapPrefabManagerForEditor.UnloadPrefabAndLayers();
                }
            }
        }

        /// <summary>
        /// UI初期化
        /// </summary>
        /// <param name="isSampleMap"></param>
        protected virtual void InitUI(bool isSampleMap = false) {
            DebugUtil.LogMethod("NewPreviewScene()");
            PreviewScene = EditorSceneManager.NewPreviewScene();

            CameraContainer = new GameObject("Map Edit Scene Camera", typeof(Camera));
            Camera = CameraContainer.GetComponent<Camera>();
            Camera.cameraType = CameraType.SceneView;
            Camera.aspect = 1;
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            Camera.orthographic = true;
            Camera.forceIntoRenderTexture = false;
            Camera.scene = PreviewScene;
            Camera.enabled = false; // Deactivate so as not to affect GameView
            ResetCameraPosition();

            MoveGameObjectToPreviewScene(CameraContainer);
            MoveAndCorrectionMapPrefabForEditor();

            SetMapLayerImageForEditor(
                MapDataModel.Layer.LayerType.Background,
                UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                    MapDataModel.background.ImageFilePath));

            SetMapLayerImageForEditor(
                MapDataModel.Layer.LayerType.DistantView,
                UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                    MapDataModel.Parallax.ImageFilePath));

            SetCamera();

            RenderTextureCanvasImage = new Image();
            Add(RenderTextureCanvasImage);

            // イベントハンドラ設定
            focusable = true; // キーイベントを受けるためにfocusableにする
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
            RegisterCallback<MouseMoveEvent>(OnMouseDrag);
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<KeyUpEvent>(OnKeyUp);
            RegisterCallback<WheelEvent>(OnScrollWheel);

            void MoveAndCorrectionMapPrefabForEditor()
            {
                MoveGameObjectToPreviewScene(MapDataModel.MapPrefabManagerForEditor.LoadPrefab(isSampleMap));
                EditorCorrectionMapPrefabForEditor(MapDataModel, MapDataModel.MapPrefabManagerForEditor.mapPrefab);
            }
        }

        /// <summary>
        /// エディター用の新規作成またはロードしたマッププレハブの内容を、使用できる状態に補正する。
        /// </summary>
        /// <param name="mapDataModel">マップデータモデル。</param>
        /// <param name="mapPrefab">マッププレハブ。</param>
        public static void EditorCorrectionMapPrefabForEditor(MapDataModel mapDataModel, GameObject mapPrefab)
        {
            MaskDistantViewToMapSize();

            // 遠景レイヤーをマップサイズにマスクする。
            void MaskDistantViewToMapSize()
            {
                const float MaskGoScaleCoefficient =  1000f / (97.66f / 100f);

                // 遠景レイヤーのスプライトマスクを内部マスクの可視化に設定。
                var spriteRenderer =
                    mapDataModel.GetLayerTransformForEditor(MapDataModel.Layer.LayerType.DistantView).
                        GetComponent<SpriteRenderer>();
                spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

                // マスク用のゲームオブジェクトが無ければ追加。
                var maskGameObjetName = $"Layer {nameof(MapDataModel.Layer.LayerType.DistantView)} Mask";
                var maskGameObject =
                    mapPrefab.transform.Find(maskGameObjetName)?.gameObject ??
                    new GameObject(maskGameObjetName);
                maskGameObject.transform.SetParent(mapPrefab.transform);

                // マスクに使用する単色正方形スプライトを生成。
                int textureSize = 32;
                var texture2d = ImageUtility.CreateFrameTexture(textureSize, textureSize, Color.white, Color.clear, 0);
                var sprite = Sprite.Create(
                    texture2d,
                    rect: new Rect(Vector2.zero, texture2d.texelSize),
                    pivot: new Vector2(0f, 1f));

                // 無ければスプライトマスクコンポーネントをゲームオブジェクトに追加し設定。
                var spriteMask = maskGameObject.GetOrAddComponent<SpriteMask>();
                spriteMask.sprite = sprite;
                spriteMask.alphaCutoff = 0f;

                // スプライトマスク用ゲームオブジェクトの位置＆スケール調整。
                maskGameObject.transform.localPosition =
                    new Vector3(0f, MapManagementService.YPositionOffsetToMapTile, 0f);
                maskGameObject.transform.localScale =
                    new Vector2(mapDataModel.width, mapDataModel.height) *
                    sprite.pixelsPerUnit / textureSize * MaskGoScaleCoefficient;
            }
        }

        public void SetLineGridActive(bool isActive) {
            if (_tilemapLayers == null) return;

            _tilemapLayers[(int) TilemapLayer.Type.LineGrid].tilemap.gameObject.SetActive(isActive);
        }

        /// <summary>
        /// ゲームオブジェクトを現在属しているシーンからプレビューシーンに移動させる。
        /// </summary>
        /// <param name="go">ゲームオブジェクト</param>
        public virtual void MoveGameObjectToPreviewScene(GameObject go) {
            try
            {
                SceneManager.MoveGameObjectToScene(go, PreviewScene);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// データおよび表示更新
        /// </summary>
        /// <param name="mapDataModel"></param>
        public virtual void Refresh([CanBeNull] MapDataModel mapDataModel = null) {
            if (mapDataModel != null) MapDataModel = mapDataModel;
            Render();
        }

        /// <summary>
        /// カメラ位置・倍率をリセット
        /// </summary>
        public void ResetCameraPosition() {
            CurrentOrthographicSize = DefaultOrthographicSize;
            Camera.gameObject.transform.position = new Vector3(0, 0, DefaultOrthographicSize * -1);
            Camera.orthographicSize = DefaultOrthographicSize;
        }

        public void ZoomInCameraPosition() {
            CurrentOrthographicSize += 2;
            Camera.gameObject.transform.position = new Vector3(0, 0, CurrentOrthographicSize * -1);
            Camera.orthographicSize = DefaultOrthographicSize;
        }

        public void ZoomOutCameraPosition() {
            CurrentOrthographicSize -= 2;
            Camera.gameObject.transform.position = new Vector3(0, 0, CurrentOrthographicSize * -1);
            Camera.orthographicSize = DefaultOrthographicSize;
        }


        // 描画処理
        //--------------------------------------------------------------------------------------

        /// <summary>
        /// マップ描画
        /// </summary>
        protected virtual void Render() {
            if (double.IsNaN(contentRect.width) ||
                double.IsNaN(contentRect.height) ||
                contentRect.width < 1 ||
                contentRect.height < 1)
                return;

            if (!RenderTexture || RenderTexture.width != (int) contentRect.width ||
                RenderTexture.height != (int) contentRect.height)
            {
                if (RenderTexture)
                {
                    Object.DestroyImmediate(RenderTexture);
                    RenderTexture = null;
                }

                var format = (Camera != null && Camera.allowHDR) ? GraphicsFormat.R16G16B16A16_SFloat : GraphicsFormat.R8G8B8A8_UNorm;
                RenderTexture = new RenderTexture((int) contentRect.width, (int) contentRect.height, 32, format);
            }

            try
            {
                Camera.aspect = RenderTexture.width / (float) RenderTexture.height;
            }
            catch (Exception)
            {
                if (CameraContainer == null || CameraContainer.GetComponent<Camera>() == null)
                    return;
                Camera = CameraContainer?.GetComponent<Camera>();
                Camera.cameraType = CameraType.SceneView;
                Camera.aspect = 1;
                Camera.clearFlags = CameraClearFlags.SolidColor;
                Camera.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
                Camera.orthographic = true;
                Camera.forceIntoRenderTexture = false;
                Camera.scene = PreviewScene;
                Camera.enabled = false; // Deactivate so as not to affect GameView
                Camera.aspect = RenderTexture.width / (float) RenderTexture.height;
            }

            Camera.targetTexture = RenderTexture;
            Camera.Render();
            Camera.targetTexture = null;

            RenderTextureCanvasImage.image = RenderTexture;

            RepaintMethod?.Invoke();
        }

        /// <summary>
        /// カメラの設定を一時的に変更して、表示中のマップの全体を原寸でキャプチャする。
        /// </summary>
        /// <remarks>
        /// 1タイルの横縦サイズを、TileDataModel.TileSize (96ピクセル) 換算でキャプチャします。
        /// グリッド線のみ非表示でキャプチャします。
        /// </remarks>
        /// <returns>キャプチャしたテクスチャ。</returns>
        public Texture2D CaptureFullSizeMapFromCamera()
        {
            // グリッド線を非表示に。
            SetLineGridActive(false);

            // 変更するカメラ設定値を一時退避。
            var originalCameraSetting = (Camera.transform.position, Camera.orthographicSize, Camera.aspect);

            // カメラをマップ中心位置へ。
            Camera.transform.position = new Vector3(
                MapDataModel.width / 2f,
                -MapDataModel.height / 2f + MapManagementService.YPositionOffsetToMapTile,
                Camera.transform.position.z);

            // 垂直ビューボリュームのサイズの半分を設定する (Unityの仕様)。
            Camera.orthographicSize = MapDataModel.height / 2f;

            Camera.aspect = (float)MapDataModel.width / MapDataModel.height;

            RenderTexture renderTexture = new(
                MapDataModel.width * TileDataModel.TileSize,
                MapDataModel.height * TileDataModel.TileSize,
                24,
                RenderTextureFormat.ARGB32);
            renderTexture.Create();

            // カメラの描画先を一時的に画面からRenderTexture変更しそれに対して描画。
            Camera.targetTexture = renderTexture;
            Camera.Render();
            Camera.targetTexture = null;

            // アクティブなRenderTextureを一時的に変更。
            var originalActiveRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;

            var captureTexture2d = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            captureTexture2d.ReadPixels(new Rect(0f, 0f, renderTexture.width, renderTexture.height), 0, 0);
            captureTexture2d.Apply();

            // アクティブなRenderTextureを元に戻す。
            RenderTexture.active = originalActiveRenderTexture;

            renderTexture.Release();
            UnityEngine.Object.DestroyImmediate(renderTexture);

            // カメラ設定値を元に戻す。
            (Camera.transform.position, Camera.orthographicSize, Camera.aspect) = originalCameraSetting;

            // グリッド線を表示状態に戻す。
            SetLineGridActive(true);
            Refresh();

            return captureTexture2d;
        }

        /// <summary>
        /// タイル座標を取得する
        /// </summary>
        /// <param name="mousePos"></param>
        /// <param name="shadow"></param>
        /// <param name="dontCompensateLeftAndUpValues"></param>
        /// <returns></returns>
        protected Vector3Int GetTilePosOnMousePosForEditor(
            Vector3 mousePos,
            bool shadow = false,
            bool dontCompensateLeftAndUpValues = false
        ) {
            var viewW = contentRect.width;
            var viewH = contentRect.height;

            // テクスチャサイズとウィンドウサイズの差分を補正する
            mousePos.x -= (viewW - RenderTexture.width) / 2;
            mousePos.y += (viewH - RenderTexture.height) / 2;

            // 左下が0なのでyを反転、+ 補正値（タイルサイズ/4）
            mousePos.y = viewH - mousePos.y + 96f / 4f;

            // カメラサイズとテクスチャサイズの差分を補正する
            var ratio = new Vector2(
                viewW / Camera.pixelWidth * (RenderTexture.width / viewW),
                viewH / Camera.pixelHeight * (RenderTexture.height / viewH)
            );
            mousePos.x = mousePos.x / ratio.x;
            mousePos.y = mousePos.y / ratio.y;

            // カメラから対象物の距離をいれる
            mousePos.z = Math.Abs(Camera.transform.position.z);
            var worldPoint = Camera.ScreenToWorldPoint(mousePos);
            var layerType = shadow
                ? MapDataModel.Layer.LayerType.Shadow
                : // ※ レイヤーShadowを基準とする
                MapDataModel.Layer.LayerType.A; // ※ レイヤーAを基準とする
            var vector = MapDataModel.MapPrefabManagerForEditor.GetLayerByType(layerType).tilemap.WorldToCell(worldPoint);


            if (!dontCompensateLeftAndUpValues)
            {
                if (vector.x < 0)
                {
                    vector.x = 0;
                    vector.y = 0;
                }

                if (vector.y > 0)
                {
                    vector.x = 0;
                    vector.y = 0;
                }
            }

            //指定座標の表示の更新
            if (shadow)
                MapEditWindow.PointDisplay(vector.x / 2, vector.y / 2);
            else
                MapEditWindow.PointDisplay(vector.x, vector.y);

            return new Vector3Int(vector.x, vector.y, 0);
        }

        /// <summary>
        ///     4分の1のタイルの場合の座標取得
        /// </summary>
        /// <param name="mousePos"></param>
        /// <returns></returns>
        protected Vector3Int GetTile4PosOnMousePosForEditor(Vector3 mousePos) {
            var viewW = contentRect.width;
            var viewH = contentRect.height;

            // テクスチャサイズとウィンドウサイズの差分を補正する
            mousePos.x -= (viewW - RenderTexture.width) / 2;
            mousePos.y += (viewH - RenderTexture.height) / 2;

            // 左下が0なのでyを反転
            mousePos.y = viewH - mousePos.y;

            // カメラサイズとテクスチャサイズの差分を補正する
            var ratio = new Vector2(
                viewW / Camera.pixelWidth * (RenderTexture.width / viewW),
                viewH / Camera.pixelHeight * (RenderTexture.height / viewH)
            );
            mousePos.x = mousePos.x / ratio.x;
            mousePos.y = mousePos.y / ratio.y;

            // カメラから対象物の距離をいれる
            mousePos.z = Math.Abs(Camera.transform.position.z);
            var result = Camera.ScreenToWorldPoint(mousePos);
            var vector = MapDataModel.MapPrefabManagerForEditor.GetLayerByType(MapDataModel.Layer.LayerType.Shadow).tilemap
                .WorldToCell(result); // ※ レイヤーAを基準とする
            //指定座標の表示の更新
            MapEditWindow.PointDisplay(vector.x, vector.y);
            return new Vector3Int(vector.x, vector.y, 0);
        }

        /// <summary>
        /// 背景レイヤーの更新
        /// </summary>
        /// <param name="mapDataModel"></param>
        /// <param name="spr"></param>
        public void UpdateBackgroundLayer(MapDataModel mapDataModel, Sprite spr) {
            MapImageChange(
                MapDataModel.Layer.LayerType.Background,
                spr,
                (mapDataModel) => UpdateBackgroundDisplayedAndScaleForEditor(mapDataModel));

            MapEditor.SaveMap(mapDataModel, MapRepository.SaveType.SAVE_PREFAB);
            UnityEditorWrapper.AssetDatabaseWrapper.SaveAssets();
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            MapEditor.SetBackgroundViewToInspector(mapDataModel);
        }

        /// <summary>
        /// 背景レイヤーの表示の有無とスケールを更新
        /// </summary>
        /// <param name="mapDataModel"></param>
        public static void UpdateBackgroundDisplayedAndScaleForEditor(MapDataModel mapDataModel) {
            var backgroundTransform =
                mapDataModel.MapPrefabManagerForEditor.GetLayerTransform(MapDataModel.Layer.LayerType.Background);
            var backgroundSpriteRenderer = backgroundTransform.GetComponent<SpriteRenderer>();

            // 表示の有無を設定。
            backgroundSpriteRenderer.enabled = mapDataModel.background.showInEditor;

            // スケール設定。
            int scale = mapDataModel.background.imageZoomIndex.GetZoomValue();
            backgroundTransform.localScale = new Vector3(scale, scale, 1);
        }

        /// <summary>
        /// 遠景レイヤーの更新
        /// </summary>
        /// <param name="mapDataModel"></param>
        /// <param name="spr"></param>
        public void UpdateMapDistantViewLayer(MapDataModel mapDataModel, Sprite spr) {
            MapEditor.SetDistantViewToInspector(mapDataModel);
            MapImageChange(
                MapDataModel.Layer.LayerType.DistantView,
                spr,
                (mapDataModel) => UpdateDistantViewDisplayedAndScaleForEditor(mapDataModel));

            MapEditor.SaveMap(mapDataModel, MapRepository.SaveType.SAVE_PREFAB);
            UnityEditorWrapper.AssetDatabaseWrapper.SaveAssets();
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
        }

        /// <summary>
        /// 遠景レイヤーの表示の有無とスケールを更新
        /// </summary>
        /// <param name="mapDataModel"></param>
        public static void UpdateDistantViewDisplayedAndScaleForEditor(MapDataModel mapDataModel) {
            var transform =
                mapDataModel.MapPrefabManagerForEditor.GetLayerTransform(MapDataModel.Layer.LayerType.DistantView);
            var spriteRenderer = transform.GetComponent<SpriteRenderer>();

            // 表示の有無を設定。
            spriteRenderer.enabled = mapDataModel.Parallax.show;

            // スケール設定。
            int scale = mapDataModel.Parallax.GetZoomScale();
            transform.localScale = new Vector3(scale, scale, 1);

            // スプライトレンダラーを設定。
            {
                // 描画モードを設定。
                spriteRenderer.drawMode = SpriteDrawMode.Tiled;

                // サイズを設定。
                float mapToSpriteRendererScale = EditorTileDisplaySizeInMap / scale;
                spriteRenderer.size = new Vector2(mapDataModel.width, mapDataModel.height) * mapToSpriteRendererScale;

                // 描画モードがタイル時の縦サイズ補正。
                if (spriteRenderer.drawMode == SpriteDrawMode.Tiled && spriteRenderer.sprite != null)
                {
                    // 描画モードがタイルの場合、テクスチャの原点が左下になるので、画像の左上がテクスチャの左上と
                    // なるよう、縦サイズを、縦画像サイズの倍数かつ表示に要な縦サイズ以上になるよう算出 (ユニットの
                    // ピクセル数込み)。
                    // 下部が余計に表示されるので、SpriteMaskコンポーネントなどでのマスク表示が必要となる。
                    float height = spriteRenderer.sprite.texture.height / spriteRenderer.sprite.pixelsPerUnit;   
                    int yCount = (int)Math.Ceiling(spriteRenderer.size.y / height);
                    spriteRenderer.size = new Vector2(spriteRenderer.size.x, height * yCount);
                }
            }
        }

        /// <summary>
        /// 背景または遠景の画像設定 (表示更新あり)。
        /// </summary>
        /// <param name="layerType">レイヤー。</param>
        /// <param name="spr">画像のスプライト。</param>
        /// <param name="beforeRenderAction">描画前に実行する処理。</param>
        public void MapImageChange(
            MapDataModel.Layer.LayerType layerType, Sprite spr, Action<MapDataModel> beforeRenderAction = null)
        {
            SetMapLayerImageForEditor(layerType, spr);
            beforeRenderAction?.Invoke(MapDataModel);
            Render();
        }

        /// <summary>
        /// 背景または遠景の画像設定。
        /// </summary>
        /// <param name="layerType">レイヤー。</param>
        /// <param name="spr">画像のスプライト。</param>
        private void SetMapLayerImageForEditor(
            MapDataModel.Layer.LayerType layerType, Sprite spr)
        {
            var mapPrefabManager = MapDataModel.MapPrefabManagerForEditor;
            mapPrefabManager.GetLayerTransform(layerType).GetComponent<SpriteRenderer>().sprite = spr;
            mapPrefabManager.layers[(int) layerType].spr = spr;
        }

        // イベントハンドラ
        //--------------------------------------------------------------------------------------
        protected virtual void OnMouseDown(IMouseEvent e) {
            IsMouseDown = true;
        }

        protected virtual void OnMouseUp(IMouseEvent e) {
            IsMouseDown = false;
        }

        protected virtual void OnMouseDrag(MouseMoveEvent e) {
            if (!IsMouseDown) return;

            if (e.altKey || e.pressedButtons == MousePressedButtons.Middle)
            {
                // Altキーか中央ボタン(ホイール)が押されている場合はマップドラッグ
                var coefficient = -Camera.orthographicSize / RenderTexture.height * DragMoveMultiple;
                Camera.gameObject.transform.position +=
                    new Vector3(e.mouseDelta.x * coefficient, -e.mouseDelta.y * coefficient, 0f);
                Render();
            }
        }

        protected virtual void OnScrollWheel(WheelEvent e) {
            Camera.orthographicSize += e.delta.y / 3;
            if (Camera.orthographicSize < 1) Camera.orthographicSize = 1;
            if (Camera.orthographicSize > 50) Camera.orthographicSize = 50;
            Render();
        }

        protected virtual void OnKeyDown(KeyDownEvent e) {
            if (e.altKey)
                IsAltOn = true;
            if (e.keyCode == KeyCode.RightArrow)
                Camera.gameObject.transform.position += new Vector3(1, 0, 0);
            Render();
            if (e.keyCode == KeyCode.LeftArrow)
                Camera.gameObject.transform.position += new Vector3(-1, 0, 0);
            Render();
            if (e.keyCode == KeyCode.UpArrow)
                Camera.gameObject.transform.position += new Vector3(0, 1, 0);
            Render();
            if (e.keyCode == KeyCode.DownArrow)
                Camera.gameObject.transform.position += new Vector3(0, -1, 0);
            Render();
        }

        protected void OnKeyUp(KeyUpEvent e) {
            if (e.altKey) IsAltOn = false;
        }

        private void SetCamera() {
            if (_lastMapDataModel != null && _lastMapDataModel.id == MapDataModel.id)
            {
                CurrentOrthographicSize = _lastCurrentOrthographicSize;
                Camera.gameObject.transform.position = _lastCameraPosition;
                Camera.orthographicSize = _lastCameraOrthographicSize;
                return;
            }

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
                targetOrthographicSize = targetsVector.x * (1 / Camera.aspect) * 0.5f;

            Camera.orthographicSize = targetOrthographicSize;
            Camera.gameObject.transform.position =
                new Vector3(
                    MapDataModel.width / 2f,
                    -MapDataModel.height / 2f + MapManagementService.YPositionOffsetToMapTile,
                    -20f);
        }

        private Vector3 AbsPositionDiff(Vector3 target1, Vector3 target2) {
            var targetsDiff = target1 - target2;
            return new Vector3(Mathf.Abs(targetsDiff.x), Mathf.Abs(targetsDiff.y));
        }

        /// <summary>
        ///     以下、MapEditCanvasクラスではイベント画像マップタイル表示専用。
        ///     EventEditCanvasクラスではイベント画像マップタイル表示以外でも使用。
        /// </summary>
        protected const string EventMassImg = "Assets/RPGMaker/Storage/System/Map/EventMass.asset";

        protected EventManagementService _eventManagementService;
        protected Tilemap                _routeTileMap;

        /// <summary>
        ///     初期パーティ、乗り物のタイルを設定用
        /// </summary>
        protected DatabaseManagementService _databaseManagementService;


        /// <summary>
        ///     以下、イベント画像マップタイル表示専用。
        /// </summary>
        protected List<TilemapLayer> _tilemapLayers;

        protected GameObject _gridGameObject;

        protected List<EventMapDataModel> _eventMapDataModels
        {
            get
            {
                return _eventManagementService.LoadEventMap()
                    .Where(eventMapDataModel => eventMapDataModel.mapId == MapDataModel.id).ToList();
            }
        }

        /// <summary>
        ///     マップの全てのイベントの位置にイベントを表わすタイルを設定する。
        /// </summary>
        protected void SetAllEventTiles(EventMapDataModel.EventMapPage targetEventMapPage) {
            InitTilemapLayers();

            var eventTile = AssetDatabase.LoadAssetAtPath<TileBase>(EventMassImg);

            _eventManagementService ??= new EventManagementService();

            var eventTilemap = _tilemapLayers[(int) TilemapLayer.Type.Event].tilemap;
            var imageTilemap = _tilemapLayers[(int) TilemapLayer.Type.Image].tilemap;

            //刷新用に前のデータを消す
            eventTilemap.ClearAllTiles();
            imageTilemap.ClearAllTiles();

            foreach (var eventMapDataModel in _eventMapDataModels)
            {
                var position = new Vector3Int(eventMapDataModel.x, eventMapDataModel.y, 0);

                if (eventTilemap.GetTile(position) != null)
                    // 仕様上ありえないはず。
                    continue;

                // イベント設定済を表わすタイルを設定。
                eventTilemap.SetTile(position, eventTile);

                // 対象イベントページがあればそれに設定された画像を優先して表示。
                if (targetEventMapPage != null &&
                    eventMapDataModel.pages.Any(eventMapPage => eventMapPage == targetEventMapPage))
                    PutImage(imageTilemap, position, targetEventMapPage);

                // イベントページに設定された画像があれば、それをタイルとして設定。
                foreach (var eventMapPage in eventMapDataModel.pages)
                {
                    if (imageTilemap.GetTile(position) != null)
                        // 最初のもののみ表示するのでループ終了。
                        break;

                    PutImage(imageTilemap, position, eventMapPage);
                }

                static void PutImage(
                    Tilemap imageTilemap,
                    Vector3Int position,
                    EventMapDataModel.EventMapPage eventMapPage
                ) {
                    var texture = EventInspector.LoadMapTileTexture(eventMapPage);
                    if (texture == null) return;

                    var imageTile = TextureToMapTile(texture);
                    imageTilemap.SetTile(position, imageTile);
                }
            }

            MoveGameObjectToPreviewScene(_gridGameObject);
        }

        /// <summary>
        ///     初期パーティ、乗り物のタイルを設定する
        /// </summary>
        protected void SetEarlyPosition() {
            var imageTilemap = _tilemapLayers[(int) TilemapLayer.Type.Image].tilemap;

            if (_databaseManagementService == null) _databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;

            var systemSettingDataModel = _databaseManagementService.LoadSystem();
            if (systemSettingDataModel.initialParty.startMap.mapId == MapDataModel.id)
            {
                var actors = _databaseManagementService.LoadCharacterActor();
                CharacterActorDataModel actor = null;
                for (int i = 0; i < actors.Count; i++)
                    if (actors[i].uuId == systemSettingDataModel.initialParty.party[0])
                    {
                        actor = actors[i];
                        break;
                    }
                
                var tex = ImageManager.LoadSvCharacter(actor.image.character);
                var imageTile = TextureToMapTile(tex);
                var position = new Vector3Int(systemSettingDataModel.initialParty.startMap.position[0],
                    systemSettingDataModel.initialParty.startMap.position[1], 0);
                imageTilemap.SetTile(position, imageTile);
            }

            var vehiclesDataModels = _databaseManagementService.LoadCharacterVehicles();

            foreach (var vehicle in vehiclesDataModels)
                if (vehicle.mapId == MapDataModel.id)
                {
                    var tex = ImageManager.LoadSvCharacter(vehicle.images);
                    var imageTile = TextureToMapTile(tex);
                    var position = new Vector3Int(vehicle.initialPos[0], vehicle.initialPos[1], 0);
                    imageTilemap.SetTile(position, imageTile);
                }

            MoveGameObjectToPreviewScene(_gridGameObject);
        }

        private static Tile TextureToMapTile(Texture2D texture) {
            if (texture == null) return null;
            var scale = (float) TileRepository.TileDefaultSize / Math.Max(texture.width, texture.height);
            var pixelsPerUnit = 100.0f / scale;
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                Vector2.one * 0.5f,
                pixelsPerUnit);
            return tile;
        }

        protected void InitTilemapLayers() {
            if (_tilemapLayers != null) return;

            _gridGameObject = new GameObject(nameof(TilemapLayer));
            _gridGameObject.AddComponent<Grid>();

            _tilemapLayers = new List<TilemapLayer>();

            foreach (TilemapLayer.Type tilemapLayerType in Enum.GetValues(typeof(TilemapLayer.Type)))
            {
                var tilemapLayer = new TilemapLayer();
                _tilemapLayers.Add(tilemapLayer);
                tilemapLayer.gameObject = new GameObject(tilemapLayerType.ToString());
                tilemapLayer.tilemap = tilemapLayer.gameObject.AddComponent<Tilemap>();
                tilemapLayer.gameObject.AddComponent<TilemapRenderer>();
                tilemapLayer.gameObject.transform.localPosition = Vector3.zero;
                MapRenderingOrderManager.SetLayerRendererSortingLayer(
                    tilemapLayer.gameObject, GetTilemapLayerrSortingLayerId(tilemapLayerType));
                tilemapLayer.gameObject.transform.SetParent(_gridGameObject.transform);
            }

            // グリッド線タイルのマップレイヤーを設定。
            {
                var gridLineTile = GridLineTileCreater.GetGridLineTile();
                var tilemap = _tilemapLayers[(int) TilemapLayer.Type.LineGrid].tilemap;
                tilemap.ClearAllTiles();
                foreach (var y in Enumerable.Range(0, MapDataModel.height))
                foreach (var x in Enumerable.Range(0, MapDataModel.width))
                    tilemap.SetTile(new Vector3Int(x, -y, 0), gridLineTile);
            }
        }
        private static int GetTilemapLayerrSortingLayerId(TilemapLayer.Type tilemapLayerType)
        {
            return UnityUtil.SortingLayerManager.GetId(
                tilemapLayerType switch
                {
                    TilemapLayer.Type.Event => UnityUtil.SortingLayerManager.SortingLayerIndex.Editor_Event,
                    TilemapLayer.Type.Image => UnityUtil.SortingLayerManager.SortingLayerIndex.Editor_Image,
                    TilemapLayer.Type.LineGrid => UnityUtil.SortingLayerManager.SortingLayerIndex.Editor_LineGrid,
                    TilemapLayer.Type.Cursor => UnityUtil.SortingLayerManager.SortingLayerIndex.Editor_Cursor,
                    _ => throw new NotImplementedException(),
                });
        }

        protected class TilemapLayer
        {
            public static int TypeCount = Enum.GetValues(typeof(Type)).Length;
            public enum Type
            {
                Event,              // イベント半透明矩形。
                Image,              // イベントに設定した画像。
                LineGrid,           // タイルグリッド線。
                Cursor,             // カーソル矩形。
            }

            public GameObject gameObject;
            public Tilemap    tilemap;
        }

        /// <summary>
        /// マップタイルグリッド線用のタイル生成クラス。
        /// </summary>
        private static class GridLineTileCreater
        {
            static Tile gridLineTile = null;

            public static Tile GetGridLineTile()
            {
                if (gridLineTile?.gameObject == null)
                {
                    gridLineTile = CreateGridLineTile();
                }

                return gridLineTile;
            }

            /// <summary>
            /// マップタイルグリッド線用のタイルを生成。
            /// </summary>
            /// <returns>タイル。</returns>
            public static Tile CreateGridLineTile()
            {
                var gridTexture = CreateGridLineTexture();

                var sprite = Sprite.Create(
                    gridTexture,
                    new Rect(0, 0, gridTexture.width, gridTexture.height),
                    Vector2.one * 0.5f,
                    pixelsPerUnit: 100f,
                    extrude: 0,
                    SpriteMeshType.FullRect);

                var gridLineTile = ScriptableObject.CreateInstance<Tile>();
                gridLineTile.sprite = sprite;

                return gridLineTile;

                /// <summary>
                /// マップタイルグリッド線用のテクスチャを生成。
                /// </summary>
                /// <returns>テクスチャ。</returns>
                Texture2D CreateGridLineTexture()
                {
                    // グリッド線の色。
                    const uint LineGridColor = 0x_80_80_80_80;

                    // グリッド線の太さ。
                    const int LineGridThickness = 2;

                    return ImageUtility.CreateFrameTexture(
                        EditorMapTileSize,
                        EditorMapTileSize,
                        Color.clear,
                        ImageUtility.ToColor32(LineGridColor),
                        LineGridThickness,
                        mipChain: true);
                }
            }
        }

        public static class MousePressedButtons
        {
            public const int Left = 1 << (int)MouseButton.LeftMouse;        // 左。
            public const int Right = 1 << (int)MouseButton.RightMouse;      // 右。
            public const int Middle = 1 << (int)MouseButton.MiddleMouse;    // 中央 (ホイール)。
        }
    }
}