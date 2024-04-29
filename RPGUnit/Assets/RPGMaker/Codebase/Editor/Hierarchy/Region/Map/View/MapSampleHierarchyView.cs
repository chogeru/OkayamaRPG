using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Map.View
{
    /// <summary>
    ///     データベースヒエラルキーのマップ部分
    /// </summary>
    public class MapSampleHierarchyView : AbstractHierarchyView
    {
        protected override string MainUxml { get { return ""; } }
        private readonly Dictionary<string, Foldout> _eventFoldouts = new Dictionary<string, Foldout>();
        private List<EventMapDataModel> _eventMapDataModels;

        // const
        //--------------------------------------------------------------------------------------------------------------
        public ExecEventType _execEventType = ExecEventType.None;

        // 利用するデータ
        //--------------------------------------------------------------------------------------------------------------
        private List<MapDataModel> _mapEntities;
        private readonly Dictionary<string, Foldout> _mapFoldouts = new Dictionary<string, Foldout>();

        // ヒエラルキー本体
        //--------------------------------------------------------------------------------------------------------------
        private readonly MapSampleHierarchy _mapSampleHierarchy;

        // 状態
        //--------------------------------------------------------------------------------------------------------------
        private bool isInit;


        // UI要素
        //--------------------------------------------------------------------------------------------------------------


        //--------------------------------------------------------------------------------------------------------------
        //
        // methods
        //
        //--------------------------------------------------------------------------------------------------------------

        // 初期化・更新系
        //--------------------------------------------------------------------------------------------------------------
        public MapSampleHierarchyView(MapSampleHierarchy mapHierarchy) {
            _mapSampleHierarchy = mapHierarchy;
            InitUI();
        }

        /// <summary>
        /// 各コンテンツデータの初期化
        /// </summary>
        override protected void InitContentsData() {
            // マップ設定Foldout
            Foldout mapSettingFoldout = new Foldout {text = EditorLocalize.LocalizeText("WORD_3008")};
            mapSettingFoldout.name = "mapSettingFoldout";
            Add(mapSettingFoldout);
            SetFoldout("mapSettingFoldout", mapSettingFoldout);
        }

        /// <summary>
        /// データ更新
        /// </summary>
        /// <param name="mapEntities"></param>
        /// <param name="eventMapDataModels"></param>
        public void Refresh(
            List<MapDataModel> mapEntities = null,
            List<EventMapDataModel> eventMapDataModels = null
        ) {
            _mapEntities = mapEntities ?? _mapEntities;
            _eventMapDataModels = eventMapDataModels ?? _eventMapDataModels;
            base.Refresh();
        }

        /// <summary>
        /// データ更新
        /// </summary>
        protected override void RefreshContents() {
            base.RefreshContents();
            SetMap();
            Editor.Hierarchy.Hierarchy.SetMapFoldout();
        }

        /// <summary>
        ///     全マップのヒエラルキーを設定。
        /// </summary>
        private void SetMap() {
            var mapHierarchyInfo =
                new MapSampleHierarchyInfo(
                    GetFoldout("mapSettingFoldout"),
                    _mapFoldouts,
                    _eventFoldouts,
                    _eventMapDataModels,
                    _mapSampleHierarchy,
                    this);

            GetFoldout("mapSettingFoldout").Clear();
            _mapEntities?.ForEach(mapEntity =>
            {
                CommonMapHierarchyView.AddMapSampleFoldout(mapEntity, mapHierarchyInfo);
            });

            InvokeSelectableElementAction();
        }

        /// <summary>
        /// 最終選択していたマップを返却（待ち時間あり）
        /// </summary>
        private async void InvokeSelectableElementAction() {
            await Task.Delay(200);
            if (isInit)
            {
                Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(LastMapIndex());
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
                GetFoldout("mapSettingFoldout").Query<Button>().ForEach(button =>
                {
                    if (button.name == CommonMapHierarchyView.GetMapEditButtonName(mapEntity.id)) elements.Add(button);
                });

            return elements[elements.Count - 1];
        }

        /// <summary>
        ///     データベースのマップリスト用のマップヒエラルキー情報クラス。
        /// </summary>
        private class MapSampleHierarchyInfo : IMapHierarchyInfo
        {
            private readonly MapSampleHierarchy     _mapSampleHierarchy;
            private readonly MapSampleHierarchyView _MapSampleHierarchyView;

            public MapSampleHierarchyInfo(
                VisualElement parentVe,
                Dictionary<string, Foldout> mapFoldouts,
                Dictionary<string, Foldout> eventFoldouts,
                List<EventMapDataModel> eventMapDataModels,
                MapSampleHierarchy mapSampleHierarchy,
                MapSampleHierarchyView MapSampleHierarchyView
            ) {
                ParentVe = parentVe;

                MapFoldouts = mapFoldouts;
                EventFoldouts = eventFoldouts;

                _mapSampleHierarchy = mapSampleHierarchy;
                _MapSampleHierarchyView = MapSampleHierarchyView;

                EventMapDataModels = eventMapDataModels;
            }

            public int ReplaceIndex { get; } = -1;

            public VisualElement ParentVe { get; }
            public string Name { get; } = null;

            public Dictionary<string, Foldout> MapFoldouts { get; }
            public Dictionary<string, Foldout> EventFoldouts { get; }

            public ExecEventType ExecEventType
            {
                get => _MapSampleHierarchyView._execEventType;
                set => _MapSampleHierarchyView._execEventType = value;
            }

            public List<EventMapDataModel> EventMapDataModels { get; }

            public AbstractHierarchyView ParentClass { get { return _MapSampleHierarchyView; } }

            public void RefreshMapHierarchy(string[] mapIds = null) {
                // データベースヒエラルキーのマップリストは、マップidでの絞り込みはしないはず。
                DebugUtil.Assert(mapIds == null);

                _mapSampleHierarchy.Refresh();
            }

            public void RefreshEventHierarchy(string updateData = null) {
                _MapSampleHierarchyView.Refresh();
            }
        }
    }
}