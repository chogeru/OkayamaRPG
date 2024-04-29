using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement.Repository;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Common.Window.ModalWindow;
using RPGMaker.Codebase.Editor.Hierarchy.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Base.View.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Map.View
{
    /// <summary>
    ///     データベースヒエラルキーのマップ部分
    /// </summary>
    public class MapHierarchyView : AbstractHierarchyView
    {
        protected override string MainUxml { get { return ""; } }

        private Button _addTileButton;
        private readonly Dictionary<string, Foldout> _eventFoldouts = new Dictionary<string, Foldout>();
        private List<EventMapDataModel> _eventMapDataModels;
        private string _updateData;

        // const
        //--------------------------------------------------------------------------------------------------------------
        public ExecEventType _execEventType = ExecEventType.None;

        // 利用するデータ
        //--------------------------------------------------------------------------------------------------------------
        private List<MapDataModel> _mapEntities;
        private readonly Dictionary<string, Foldout> _mapFoldouts = new Dictionary<string, Foldout>();

        // ヒエラルキー本体
        //--------------------------------------------------------------------------------------------------------------
        private readonly MapHierarchy _mapHierarchy;

        // 状態
        //--------------------------------------------------------------------------------------------------------------
        private List<TileGroupDataModel> _tileGroupEntities;
        private HierarchyItemListView _tileGroupListView;

        private bool isInit;

        private List<CommonMapHierarchyView> _commonMapHierarchyViews;


        // UI要素
        //--------------------------------------------------------------------------------------------------------------


        //--------------------------------------------------------------------------------------------------------------
        //
        // methods
        //
        //--------------------------------------------------------------------------------------------------------------

        // 初期化・更新系
        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mapHierarchy"></param>
        public MapHierarchyView(MapHierarchy mapHierarchy) {
            _mapHierarchy = mapHierarchy;
            InitUI();
        }

        /// <summary>
        /// 各コンテンツデータの初期化
        /// </summary>
        override protected void InitContentsData() {
            // マップ設定Foldout
            Foldout mapSettingFoldout = new Foldout {text = EditorLocalize.LocalizeText("WORD_0732")};
            mapSettingFoldout.name = "mapSettingFoldout";
            Add(mapSettingFoldout);
            SetFoldout("mapSettingFoldout", mapSettingFoldout);

            // タイルデータの登録ボタン
            _addTileButton = new Button {text = EditorLocalize.LocalizeText("WORD_0733")};
            _addTileButton.AddToClassList("button-transparent");
            _addTileButton.AddToClassList("AnalyticsTag__page_view__map_tile");
            GetFoldout("mapSettingFoldout").Add(_addTileButton);

            // タイルグループFoldout
            Foldout tileGroupListFoldout = new Foldout {text = EditorLocalize.LocalizeText("WORD_0734")};
            tileGroupListFoldout.name = "tileGroupListFoldout";
            tileGroupListFoldout.AddToClassList("AnalyticsTag__page_view__map_tilegroup");
            GetFoldout("mapSettingFoldout").Add(tileGroupListFoldout);
            SetFoldout("tileGroupListFoldout", tileGroupListFoldout);

            // タイルグループリスト
            _tileGroupListView = new HierarchyItemListView(ViewName);
            GetFoldout("tileGroupListFoldout").Add(_tileGroupListView);

            // "マップリスト"Foldout
            Foldout mapListFoldout = new Foldout {text = EditorLocalize.LocalizeText("WORD_0739")};
            mapListFoldout.name = "mapListFoldout";
            GetFoldout("mapSettingFoldout").Add(mapListFoldout);
            SetFoldout("mapListFoldout", mapListFoldout);
            
            _commonMapHierarchyViews = new List<CommonMapHierarchyView>();

            InitEventHandlers();
        }

        /// <summary>
        /// イベントの初期設定
        /// </summary>
        private void InitEventHandlers() {
            // タイルデータの登録ボタン
            Editor.Hierarchy.Hierarchy.AddSelectableElementAndAction(_addTileButton,
                MapEditor.MapEditor.LaunchTileEditMode);
            _addTileButton.clicked += () =>
            {
                Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(_addTileButton);
            };
            
            InitContextMenu(RegistrationLimit.None);
            var dic = new Dictionary<string, List<string>>
            {
                {
                    KeyNameTileGroupList,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_0735"), EditorLocalize.LocalizeText("WORD_0736")
                    }
                },
                {
                    KeyNameMapList,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_3009"), EditorLocalize.LocalizeText("WORD_0025"), EditorLocalize.LocalizeText("WORD_0026")
                    }
                }
            };
            SetParentContextMenu(dic);

            // タイルグループリスト
            _tileGroupListView.SetEventHandler(
                (i, value) => { MapEditor.MapEditor.LaunchTileGroupEditMode(_tileGroupEntities[i]); },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameTileGroupList, new ContextMenuData()
                            {
                                UuId = _tileGroupEntities[i].id,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_0737"),
                                        EditorLocalize.LocalizeText("WORD_0738")
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.None
                            }
                        }
                    };
                    SetChildContextMenu(childDic);
                });
        }

        protected override void SetParentContextMenu(Dictionary<string, List<string>> contextMenuDic) {
            foreach (var dic in contextMenuDic)
            {
                var keyName = dic.Key;
                var names = dic.Value;
                //マップリストの場合
                if (keyName == KeyNameMapList)
                {
                    BaseClickHandler.ClickEvent(GetFoldout(keyName), evt =>
                    {
                        var menu = new GenericMenu();
                        var nowDataCount = NowDataCounts.ContainsKey(keyName) ? NowDataCounts[keyName] : 0;

                        // サンプルマップから作成
                        if (Editor.Hierarchy.Hierarchy.mapManagementService.LoadMapSamples().Count > 0)
                        {
                            menu.AddItem(new GUIContent(names[0]), false,
                                () =>
                                {
                                    var modal = new MapCreateForSampleMapModalWindow();
                                    modal.ShowWindow();
                                });
                        }
                        else
                        {
                            menu.AddDisabledItem(new GUIContent(names[0]));
                        }

                        //マップの新規作成
                        if ((int) MaxType > nowDataCount || MaxType == RegistrationLimit.None)
                        {
                            menu.AddItem(new GUIContent(names[1]), false,
                                () => CreateDataModel(keyName));
                        }

                        //マップの貼り付け
                        if (CommonMapHierarchyView.MapDataModel != null)
                        {
                            menu.AddItem(new GUIContent(names[2]), false,
                                () => Duplicate(keyName, CommonMapHierarchyView.MapDataModel.id));
                        }
                        else
                        {
                            menu.AddDisabledItem(new GUIContent(names[2]));
                        }

                        menu.ShowAsContext();
                    });
                }
                else
                {
                    base.SetParentContextMenu(contextMenuDic);
                }
            }
        }

        protected override VisualElement CreateDataModel(string keyName) {
            switch (keyName)
            {
                case KeyNameTileGroupList:
                    var newTileGroupDataModel = MapEditor.MapEditor.LaunchTileGroupEditMode(null);
                    MapEditor.MapEditor.SaveTileGroup(newTileGroupDataModel);

                    // データ更新
                    _tileGroupEntities = Editor.Hierarchy.Hierarchy.mapManagementService.LoadTileGroups();
                    _tileGroupListView.Refresh(_tileGroupEntities.Select(item => item.name).ToList());

                    var elements = new List<VisualElement>();
                    _tileGroupListView.Query<Button>().ForEach(button => { elements.Add(button); });
                    return elements[elements.Count - 1];

                case KeyNameMapList:
                    var visualElement = base.CreateDataModel(keyName);
                    isInit = true;
                    var map = MapEditor.MapEditor.LaunchMapEditMode(null);
                    //マップ新規作成時は、Prefabを強制的に保存する
                    MapEditor.MapEditor.SaveMap(map, MapRepository.SaveType.SAVE_PREFAB_FORCE);
                    MapEditor.MapEditor.ReloadMap(map, RefreshTypeMapCreate + "," + map.id);
                    return visualElement;
            }
            return null;
        }

        protected override VisualElement DuplicateDataModel(string keyName, string uuId) {
            switch (keyName)
            {
                case KeyNameTileGroupList:
                    var target = _tileGroupEntities.FirstOrDefault(t => t.id == uuId);
                    if (target != null)
                    {
                        // データコピー
                        var tiles = new List<TileDataModelInfo>();
                        for (int i = 0; i < target.tileDataModels?.Count; i++)
                            tiles.Add(target.tileDataModels[i].TileDataModel.tileDataModelInfo);
                        var groupNames = _tileGroupEntities.Select(t => t.name).ToList();
                        var tileGroupDataModel = new TileGroupDataModel(Guid.NewGuid().ToString(),
                            _mapHierarchy.CreateDuplicateName(groupNames, _tileGroupEntities.FirstOrDefault(t => t.id == uuId).name),
                            tiles);

                        MapEditor.MapEditor.SaveTileGroup(tileGroupDataModel);
                        MapEditor.MapEditor.LaunchTileGroupEditMode(tileGroupDataModel);

                        // データ更新
                        _tileGroupEntities = Editor.Hierarchy.Hierarchy.mapManagementService.LoadTileGroups();
                        _tileGroupListView.Refresh(_tileGroupEntities.Select(item => item.name).ToList());

                        var elements = new List<VisualElement>();
                        _tileGroupListView.Query<Button>().ForEach(button => { elements.Add(button); });
                        return elements[elements.Count - 1];
                    }
                    break;

                case KeyNameMapList:
                    var visualElement = base.DuplicateDataModel(keyName, uuId);

                    isInit = true;
                    // 指定のマップデータのIDのみ変更&Prefab複製
                    var mapData = MapDataModel.CopyData(CommonMapHierarchyView.MapDataModel);
                    mapData.id = Guid.NewGuid().ToString();
                    mapData.index = Editor.Hierarchy.Hierarchy.mapManagementService.LoadMaps()[Editor.Hierarchy.Hierarchy.mapManagementService.LoadMaps().Count - 1].index++;
                    MapDataModel.CopyMapPrefabForEditor(CommonMapHierarchyView.MapDataModel, mapData.id);
                    
                    // 名前設定
                    mapData.name =
                        _mapHierarchy.CreateDuplicateName(_mapEntities.Select(m => m.name).ToList(), mapData.name);
                    
                    // データ更新
                    // 新規作成時は、Prefabを強制的に保存する
                    MapEditor.MapEditor.SaveMap(mapData, MapRepository.SaveType.SAVE_PREFAB_FORCE);
                    MapEditor.MapEditor.ReloadMap(mapData, RefreshTypeMapDuplicate + "," + mapData.id);
                    return visualElement;
            }
            return null;
        }

        protected override VisualElement DeleteDataModel(string keyName, string uuId) {            
            switch (keyName)
            {
                case KeyNameTileGroupList:
                    var tileGroupDataModel = _tileGroupEntities.FirstOrDefault(t => t.id == uuId);
                    int index = 0;
                    for (int i = 0; i < _tileGroupEntities.Count; i++)
                    {
                        if (_tileGroupEntities[i].id == uuId)
                        {
                            tileGroupDataModel = _tileGroupEntities[i];
                            index = i;
                            break;
                        }
                    }

                    MapEditor.MapEditor.RemoveTileGroup(tileGroupDataModel);

                    // データ更新
                    _tileGroupEntities = Editor.Hierarchy.Hierarchy.mapManagementService.LoadTileGroups();
                    _tileGroupListView.Refresh(_tileGroupEntities.Select(item => item.name).ToList());

                    var elements = new List<VisualElement>();
                    _tileGroupListView.Query<Button>().ForEach(button => { elements.Add(button); });
                    var visualElement = elements.Count == 0 ? null : elements.Count - 1 < index ?  elements[^1] : elements.FirstOrDefault(e => e.name == "MapHierarchyView" + index);

                    //選択可能なVisualElementが存在しない場合には、Inspectorを初期化
                    if (elements.Count == 0)
                    {
                        Inspector.Inspector.Clear();
                        WindowLayoutManager.GetActiveWindow(
                            WindowLayoutManager.WindowLayoutId.MapTileGroupEditWindow)
                            .rootVisualElement.Clear();
                    }
                    return visualElement;
                case KeyNameMapList:
                    //CommonMapHierarchyViewで行うため、なし
                    return base.DeleteDataModel(keyName, uuId);
            }
            return null;
        }

        /// <summary>
        /// 初期化済みかどうかの設定
        /// </summary>
        public void SetInit() {
            isInit = true;
        }

        /// <summary>
        /// データ更新
        /// </summary>
        /// <param name="mapEntities"></param>
        /// <param name="tileGroupEntities"></param>
        /// <param name="eventMapDataModels"></param>
        public void Refresh(
            string updateData = null,
            List<MapDataModel> mapEntities = null,
            List<TileGroupDataModel> tileGroupEntities = null,
            List<EventMapDataModel> eventMapDataModels = null
        ) {
            _updateData = updateData;
            _mapEntities = mapEntities ?? _mapEntities;
            _tileGroupEntities = tileGroupEntities ?? _tileGroupEntities;
            _eventMapDataModels = eventMapDataModels ?? _eventMapDataModels;
            base.Refresh();
        }

        /// <summary>
        /// データ更新
        /// </summary>
        protected override void RefreshContents() {
            base.RefreshContents();
            
            if (_updateData == null)
            {
                SetMap();
                _tileGroupListView.Refresh(_tileGroupEntities.Select(item => item.name).ToList());
            }
            else
            {
                UpdateMap();
            }
        }

        /// <summary>
        /// タイルグループ更新
        /// </summary>
        public void RefreshTileGroupContents() {
            _tileGroupListView.Refresh(_tileGroupEntities.Select(item => item.name).ToList());
        }

        /// <summary>
        ///     全マップのヒエラルキーを設定。
        /// </summary>
        private void SetMap() {
            var mapHierarchyInfo =
                new MapHierarchyInfo(
                    GetFoldout("mapListFoldout"),
                    _mapFoldouts,
                    _eventFoldouts,
                    _eventMapDataModels,
                    _mapHierarchy,
                    this);

            GetFoldout("mapListFoldout").Clear();
            _commonMapHierarchyViews = new List<CommonMapHierarchyView>();
            _mapEntities?.ForEach(mapEntity =>
            {
                var commonMap = new CommonMapHierarchyView();
                _commonMapHierarchyViews.Add(commonMap.AddMapFoldout(mapEntity, mapHierarchyInfo, this));
                //CommonMapHierarchyView.AddMapFoldout(mapEntity, mapHierarchyInfo, this);
            });
            InvokeSelectableElementAction();
        }

        /// <summary>
        /// 特定のマップのヒエラルキーを更新
        /// </summary>
        private void UpdateMap() {
            var mapHierarchyInfo =
                new MapHierarchyInfo(
                    GetFoldout("mapListFoldout"),
                    _mapFoldouts,
                    _eventFoldouts,
                    _eventMapDataModels,
                    _mapHierarchy,
                    this);
            CommonMapHierarchyView commonMap = null;
            //どのマップでどの種別か配列で保持
            var data = GetRefreshType(_updateData);
            
            MapDataModel mapEntity = null;

            if (data.Count > 1)
                for (int i = 0; i < _mapEntities.Count; i++)
                {
                    if (_mapEntities[i].id == data[(int) RefreshType.MapId])
                    {
                        mapEntity = _mapEntities[i];
                        break;
                    }
                }

            switch (data[(int)RefreshType.Type])
            {
                //マップの新規作成、貼り付け
                case RefreshTypeMapCreate:
                case RefreshTypeMapDuplicate:
                    for (int i = 0; i < _commonMapHierarchyViews.Count; i++)
                    {
                        if (_commonMapHierarchyViews[i].MapId == data[(int)RefreshType.MapId])
                        {
                            return;
                        }
                    }

                    commonMap = new CommonMapHierarchyView();
                    _commonMapHierarchyViews.Add(commonMap.AddMapFoldout(mapEntity, mapHierarchyInfo, this));
                    //フォーカス位置更新
                    Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(commonMap.MapEdit);
                    break;
                //マップの削除
                case RefreshTypeMapDelete:
                    for (int i = 0; i < _commonMapHierarchyViews.Count; i++)
                    {
                        if (_commonMapHierarchyViews[i].MapId == data[(int)RefreshType.MapId])
                        {
                            GetFoldout("mapListFoldout").Remove(_commonMapHierarchyViews[i].MapFoldout);
                            _commonMapHierarchyViews.RemoveAt(i);
                            if (_commonMapHierarchyViews.Count > 0)
                            {
                                commonMap = _commonMapHierarchyViews.Count - 1 < i
                                    ? _commonMapHierarchyViews[^1]
                                    : _commonMapHierarchyViews[i];
                            }

                            break;
                        }
                    }

                    //フォーカス位置更新
                    if (commonMap != null)
                        Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(commonMap.MapEdit);

                    //マップが一つもなくなったらフォーカスはタイトルへ
                    if (commonMap == null && _commonMapHierarchyViews.Count == 0)
                    {
                        Inspector.Inspector.ClearCached();
                        WindowLayoutManager.GetActiveWindow(
                            WindowLayoutManager.WindowLayoutId.MapEditWindow)
                            .rootVisualElement.Clear();
                        Editor.Hierarchy.Hierarchy.SelectButton("title_button");
                    }

                    break;
                case RefreshTypeMapName:
                case RefreshTypeMapSize:
                    for (int i = 0; i < _commonMapHierarchyViews.Count; i++)
                    {
                        if (_commonMapHierarchyViews[i].MapId == data[(int) RefreshType.MapId])
                        {
                            _commonMapHierarchyViews[i].MapFoldout.text = mapEntity.name;
                            break;
                        }
                    }
                    break;

                //イベントの新規作成
                case RefreshTypeEventCreate:
                //イベントの貼り付け
                case RefreshTypeEvenDuplicate:
                //イベントの削除
                case RefreshTypeEventDelete:
                //イベントのEVページ新規作成
                case RefreshTypeEventEvCreate:
                //イベントのEVページ貼り付け
                case RefreshTypeEvenEvDuplicate:
                //イベントのEVページ削除
                case RefreshTypeEventEvDelete:
                // イベント名変更
                case RefreshTypeEventName:
                    for (int i = 0; i < _commonMapHierarchyViews.Count; i++)
                    {
                        if (_commonMapHierarchyViews[i].MapId == data[(int) RefreshType.MapId])
                        {
                            _commonMapHierarchyViews[i].CreateEventContent(mapHierarchyInfo);
                            break;
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 最終選択していたマップを返却（待ち時間あり）
        /// </summary>
        private async void InvokeSelectableElementAction() {
            await Task.Delay(200);
            if (isInit)
            {
                if (LastMapIndex() != null) Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(LastMapIndex());
                isInit = false;
            }
        }

        /// <summary>
        /// 最終選択していたマップを返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastMapIndex() {
            var elements = new List<VisualElement>();
            //マップ編集ボタンの名前に設定あるIDで詰めていく
            foreach (var mapEntity in _mapEntities)
                GetFoldout("mapListFoldout").Query<Button>().ForEach(button =>
                {
                    if (button.name == CommonMapHierarchyView.GetMapEditButtonName(mapEntity.id)) elements.Add(button);
                });

            return elements[elements.Count - 1];
        }

        /// <summary>
        ///     データベースのマップリスト用のマップヒエラルキー情報クラス。
        /// </summary>
        public class MapHierarchyInfo : IMapHierarchyInfo {
            private readonly MapHierarchy _mapHierarchy;
            private readonly MapHierarchyView _mapHierarchyView;

            public MapHierarchyInfo(
                VisualElement parentVe,
                Dictionary<string, Foldout> mapFoldouts,
                Dictionary<string, Foldout> eventFoldouts,
                List<EventMapDataModel> eventMapDataModels,
                MapHierarchy mapHierarchy,
                MapHierarchyView mapHierarchyView
            ) {
                ParentVe = parentVe;

                MapFoldouts = mapFoldouts;
                EventFoldouts = eventFoldouts;

                _mapHierarchy = mapHierarchy;
                _mapHierarchyView = mapHierarchyView;

                EventMapDataModels = eventMapDataModels;
            }

            public int ReplaceIndex { get; } = -1;

            public VisualElement ParentVe { get; }
            public string Name { get; } = null;

            public Dictionary<string, Foldout> MapFoldouts { get; }
            public Dictionary<string, Foldout> EventFoldouts { get; }

            public ExecEventType ExecEventType
            {
                get => _mapHierarchyView._execEventType;
                set => _mapHierarchyView._execEventType = value;
            }

            public List<EventMapDataModel> EventMapDataModels { get; }

            public AbstractHierarchyView ParentClass { get { return _mapHierarchyView; } }

            public void RefreshMapHierarchy(string[] mapIds = null) {
                // データベースヒエラルキーのマップリストは、マップidでの絞り込みはしないはず。
                DebugUtil.Assert(mapIds == null);

                _mapHierarchy.Refresh();
            }

            public void RefreshEventHierarchy(string updateData) {
                _mapHierarchyView.Refresh(updateData);
            }
        }
    }
}