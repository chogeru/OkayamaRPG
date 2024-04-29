using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.MapEditor.Component.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Window.EventEdit
{
    /// <summary>
    ///     イベント編集ウィンドウ.
    /// </summary>
    public class EventEditWindow : BaseWindow
    {
        // データプロパティ
        private MapDataModel            _mapDataModel;
        private List<EventMapDataModel> _eventMapEntities;
        private int                     _eventPageNum;

        // 状態プロパティ
        private EventMapDataModel _currentEditingEventEntity;

        // UI要素プロパティ
        private EventEditCanvas _eventEditCanvas;
        private VisualElement   _statusBarPanel;
        private VisualElement   _controlPanel;
        private VisualElement   _previewPanel;
        private Label           _pointLabel;
        private Button          _btnRouteSetting;
        private Button          _btnSettingEnd;
        private Button          _btnPreview;
        private Label           _settingLabel;

        private void OnEnable() {
        }

        /**
         * 初期化
         */
        public void Init(
            MapDataModel mapDataModel,
            List<EventMapDataModel> eventMapEntities,
            int pageNum,
            [CanBeNull] EventMapDataModel eventMapDataModelEntity = null
        ) {
            _mapDataModel = mapDataModel;
            _eventMapEntities = eventMapEntities;
            _currentEditingEventEntity = eventMapDataModelEntity;
            _eventPageNum = pageNum;
            InitUI();
            Refresh();
        }

        /**
         * データおよび表示を更新
         */
        public void Refresh(
            [CanBeNull] MapDataModel mapDataModel = null,
            [CanBeNull] List<EventMapDataModel> eventMapEntities = null,
            [CanBeNull] EventMapDataModel eventMapDataModelEntity = null
        ) {
            if (mapDataModel != null) _mapDataModel = mapDataModel;
            if (eventMapEntities != null) _eventMapEntities = eventMapEntities;
            if (eventMapDataModelEntity != null) _currentEditingEventEntity = eventMapDataModelEntity;

            if (_currentEditingEventEntity != null)
                MapEditor.SetEventEntityToInspector(_currentEditingEventEntity, _eventPageNum, this, _mapDataModel);

            _eventEditCanvas.Refresh(_mapDataModel);
        }

        /**
         * UIを初期化
         */
        private void InitUI() {
            rootVisualElement.Clear();

            // マップ
            //----------------------------------------------------------------------
            _eventEditCanvas?.Dispose();
            _eventEditCanvas = new EventEditCanvas(
                _mapDataModel,
                _currentEditingEventEntity,
                _currentEditingEventEntity?.pages.Single(eventMapPage => eventMapPage.page == _eventPageNum),
                Repaint);

            // ステータスバー。
            //----------------------------------------------------------------------
            _statusBarPanel = new VisualElement();
            _statusBarPanel.style.color = new StyleColor(new Color(0, 0, 0, 0));
            _statusBarPanel.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0));
            // コントロールパネル
            //----------------------------------------------------------------------
            _controlPanel = new VisualElement();
            _controlPanel.style.color = new StyleColor(new Color(0, 0, 0, 0));
            _controlPanel.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0));
            // プレビュー用ボタンパネル
            //----------------------------------------------------------------------
            _previewPanel = new VisualElement();
            _previewPanel.style.color = new StyleColor(new Color(0, 0, 0, 0));
            _previewPanel.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0));
            // 要素配置
            //----------------------------------------------------------------------
            _eventEditCanvas.style.width = Length.Percent(100);
            _eventEditCanvas.style.height = Length.Percent(80);
            _controlPanel.style.width = Length.Percent(100);
            _controlPanel.style.height = Length.Percent(10);
            _previewPanel.style.width = Length.Percent(100);
            _previewPanel.style.height = Length.Percent(5);
            rootVisualElement.Add(_eventEditCanvas);
            rootVisualElement.Add(_statusBarPanel);
            rootVisualElement.Add(_controlPanel);
            rootVisualElement.Add(_previewPanel);

            // ステータスバーに座標表示。
            _pointLabel = new Label();
            _pointLabel.style.color = new StyleColor(new Color(1, 1, 1, 1));
            _statusBarPanel.Add(_pointLabel);
            _pointLabel.AddToClassList("point_label_container");
            _pointLabel.style.alignSelf = Align.FlexEnd;

            // レイアウトが完了したタイミングでサイズが取れるようになるので、そのタイミングで初回Renderするように
            _eventEditCanvas.RegisterCallback<GeometryChangedEvent>(v => { 
                _eventEditCanvas.Refresh();
                _eventEditCanvas.SetEventEditWindow(this);
            });
        }

        /// <summary>
        ///     イベント位置にカーソルを設定する。
        /// </summary>
        public void SetCursorPosOnEventPositon() {
            if (_eventEditCanvas == null || _currentEditingEventEntity == null)
                return;

            _eventEditCanvas.SetCursorPos(
                new Vector3Int(_currentEditingEventEntity.x, _currentEditingEventEntity.y, 0),
                SetPointLabel);
        }

        public void SetPointLabel(int x, int y) {
            _pointLabel.text = $"{EditorLocalize.LocalizeText("WORD_0983")}:　　　　{x},{Math.Abs(y)}";
            _pointLabel.style.color = Color.white;
            if (!EditorGUIUtility.isProSkin)
            {
                _pointLabel.style.color = Color.black;
            }

        }

        /**
         * イベント配置モード起動
         */
        public void LaunchEventMode(string mapID) {
            _eventEditCanvas.LaunchEventDrawingMode(mapID);

            _controlPanel.Clear();
            _previewPanel.Clear();
        }

        /**
         * ルート描画モード起動
         */
        public void LaunchRouteMode(
            EventMapDataModel eventMapDataModelEntity,
            int eventIndex,
            int eventCommandIndex,
            List<EventDataModel.EventCommandMoveRoute> codeList,
            Action<List<EventDataModel.EventCommandMoveRoute>> setAndSaveMoveRouteAction,
            List<Vector3Int> pos = null
        ) {
            _currentEditingEventEntity = eventMapDataModelEntity;
            _eventEditCanvas.InitRouteTileRender(
                eventMapDataModelEntity, pos, codeList, setAndSaveMoveRouteAction, eventIndex, eventCommandIndex);
        }

        public void LaunchRouteDrawingMode(Vector3Int initializePos) {
            var box = new VisualElement();
            // 設定終了ボタン
            _settingLabel = new Label(EditorLocalize.LocalizeText("WORD_1605") + "...");
            _settingLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _settingLabel.style.fontSize = 32f;
            _settingLabel.style.color = Color.white;
            _settingLabel.style.backgroundColor = Color.red;

            box.Add(_settingLabel);
            _controlPanel.Add(box);

            _eventEditCanvas.LaunchRouteDrawingMode(initializePos);
        }

        public void LaunchRouteDrawingModeEnd() {
            _eventEditCanvas?.LaunchNoneDrawingModeByRoute();
            _controlPanel.Clear();
        }

        /**
         * 目的地描画モード起動
         */
        public void LaunchDestinationMode(
            EventMapDataModel eventMapDataModelEntity,
            int eventIndex,
            int eventCommandIndex,
            Vector2Int pos,
            Action callBack
        ) {
            // UI初期化
            //----------------------------------------------------------------------
            _eventEditCanvas.SetMoveEventViewTile(pos);

            // コンテナ
            var box = new VisualElement();
            box.style.flexDirection = FlexDirection.Column;

            // 移動先の設定ボタン
            _btnRouteSetting = new Button {text = EditorLocalize.LocalizeText("WORD_1583")};
            _btnRouteSetting.clicked += () =>
            {
                _eventEditCanvas.LaunchDestinationDrawingMode(eventMapDataModelEntity, eventIndex, eventCommandIndex);
            };

            // 設定終了ボタン
            _btnSettingEnd = new Button {text = EditorLocalize.LocalizeText("WORD_1584")};
            _btnSettingEnd.clicked += () =>
            {
                callBack?.Invoke();
                _eventEditCanvas.LaunchNoneDrawingMode();
            };

            // 要素配置
            _controlPanel.style.flexDirection = FlexDirection.RowReverse;
            _controlPanel.Clear();
            box.Add(_btnRouteSetting);
            box.Add(_btnSettingEnd);
            _controlPanel.Add(box);
        }

        /// <summary>
        ///     目的地描画モードを開始
        /// </summary>
        public void BeginDestinationMode(
            EventMapDataModel eventMapDataModelEntity,
            int eventIndex,
            int eventCommandIndex,
            Vector2Int pos
        ) {
            _eventEditCanvas?.SetMoveEventViewTile(pos);
            _eventEditCanvas?.LaunchDestinationDrawingMode(eventMapDataModelEntity, eventIndex, eventCommandIndex);
        }

        /// <summary>
        ///     目的地描画モードを終了
        /// </summary>
        public void EndDestinationMode() {
            _eventEditCanvas?.LaunchNoneDrawingMode();
            _controlPanel?.Clear();
        }


        public void PreviewButtonCreate(Action callBack) {
            // コンテナ
            var box = new VisualElement();
            box.style.flexDirection = FlexDirection.Column;
            _btnPreview = new Button {text = EditorLocalize.LocalizeText("WORD_0991")};
            _btnPreview.clicked += () => { callBack.Invoke(); };
            box.Add(_btnPreview);

            _previewPanel.style.flexDirection = FlexDirection.RowReverse;
            _previewPanel.Clear();
            _previewPanel.Add(box);
        }

        /*
         * 各モードの終了時にボタンを消す
         */
        public void SettingButtonDestroy() {
            _controlPanel?.Clear();
            _eventEditCanvas?.DeleteMoveEventViewTile();
        }

        /*
         * 各モードの終了時にボタンを消す
         */
        public void PreviewButtonDestroy() {
            _previewPanel?.Clear();
        }

        //イベント変更時にボタンを削除する
        public void WhenEventClosed() {
            SettingButtonDestroy();
            PreviewButtonDestroy();
            _eventEditCanvas?.ClearRouteTileDisplay();
        }

        /// <summary>
        ///     座標指定
        /// </summary>
        /// <param name="callBack"></param>
        public void LaunchCoordinateMode(Action<Vector3Int> callBack = null, string id = "", bool eventMove = false) {
            var box = new VisualElement();
            // 設定終了ボタン
            _settingLabel = new Label(EditorLocalize.LocalizeText("WORD_1605") + "...");
            _settingLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _settingLabel.style.fontSize = 32f;
            _settingLabel.style.color = Color.white;
            _settingLabel.style.backgroundColor = Color.red;

            box.Add(_settingLabel);
            _controlPanel.Add(box);
            _eventEditCanvas?.SetCoordinate(i => { callBack?.Invoke(i); }, id, eventMove);
        }

        public void LaunchCoordinateModeEnd() {
            _eventEditCanvas?.LaunchNoneDrawingMode();
            _controlPanel.Clear();
        }

        public EventEditCanvas ReloadMap(
            MapDataModel mapDataModel,
            List<EventMapDataModel> eventMapEntities,
            int pageNum,
            [CanBeNull] EventMapDataModel eventMapDataModelEntity = null
        ) {
            Init(mapDataModel, eventMapEntities, pageNum, eventMapDataModelEntity);
            return _eventEditCanvas;
        }

        private void OnDestroy() {
            _eventEditCanvas?.Dispose();
        }
    }
}