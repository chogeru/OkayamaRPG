using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Outline.View.Component
{
    /// <summary>
    ///     アウトラインのチャプターとセクション用のマップヒエラルキー情報クラス。
    /// </summary>
    public class OutlineMapHierarchyInfo : IMapHierarchyInfo
    {
        private List<MapDataModel> _mapDataModels;

        public OutlineMapHierarchyInfo(VisualElement parentVe, string name, AbstractHierarchyView hierarchyView) {
            ParentVe = parentVe;
            Name = name;
            ParentClass = hierarchyView;
        }

        public VisualElement ParentVe { get; }
        public string Name { get; }

        public Dictionary<string, Foldout> MapFoldouts { get; } = new Dictionary<string, Foldout>();
        public Dictionary<string, Foldout> EventFoldouts { get; } = new Dictionary<string, Foldout>();

        public ExecEventType ExecEventType { get; set; } = ExecEventType.None;

        public List<EventMapDataModel> EventMapDataModels { get; private set; }
        
        public AbstractHierarchyView ParentClass { get; }
        
        private List<CommonMapHierarchyView> _commonMapHierarchyViews;


        public void RefreshMapHierarchy(string[] mapIds = null) {
            _mapDataModels =
                Editor.Hierarchy.Hierarchy.mapManagementService.LoadMaps()
                    .Where(mapDataModel => mapIds != null && mapIds.Contains(mapDataModel.id)).ToList();
            RefreshEventHierarchy();
        }

        public void RefreshEventHierarchy(string updateData = null) {
            //MapHierarchyViewと同じにする
            EventMapDataModels = new EventManagementService().LoadEventMap();
            ParentVe.Clear();
            _commonMapHierarchyViews = new List<CommonMapHierarchyView>();
            foreach (var mapDataModel in _mapDataModels)
            {
                var commonMap = new CommonMapHierarchyView();
                commonMap.AddMapFoldout(mapDataModel, this);
                _commonMapHierarchyViews.Add(commonMap);
            }
        }

        // 指定マップのHierarchyを追加
        public void AddMapHierarchy(string id = null) {
            if (id == null) return;

            var map = Hierarchy.mapManagementService.LoadMaps().Find(data => data.id == id);
            if (map != null)
            {
                _mapDataModels.Add(map);
                _mapDataModels.Sort((d1, d2) => d1.index.CompareTo(d2.index));
                RefreshEventHierarchy();
            }
        }

        // 指定マップのHierarchyを削除
        public void RemoveMapHierarchy(string id = null, string sectionId = null) {
            if (id == null) return;

            for (int i = 0; i < _commonMapHierarchyViews.Count; i++)
            {
                if (_commonMapHierarchyViews[i].MapId == id)
                {
                    if (sectionId != null)
                        OutlineEditor.OutlineEditor.RemoveSectionMap(sectionId, id, false);
                    ParentVe.Remove(_commonMapHierarchyViews[i].MapFoldout);
                    _commonMapHierarchyViews.RemoveAt(i);
                    _mapDataModels.RemoveAt(i);
                    i--;
                }
            }
        }

        // 指定マップの名称更新
        public void UpdateMapNameHierarchy(string id = null) {
            if (id == null) return;

            for (int i = 0; i < _mapDataModels.Count; i++)
            {
                if (_mapDataModels[i].id == id)
                    for (int i2 = 0; i2 < _commonMapHierarchyViews.Count; i2++)
                        if (_commonMapHierarchyViews[i2].MapId == id)
                            _commonMapHierarchyViews[i2].MapFoldout.text = _mapDataModels[i].name;
            }
        }

        // 指定マップのイベント更新
        public void UpdateEventHierarchy(string id = null) {
            if (id == null) return;

            for (int i = 0; i < _commonMapHierarchyViews.Count; i++)
            {
                if (_commonMapHierarchyViews[i].MapId == id)
                {
                    _commonMapHierarchyViews[i].CreateEventContent(this);
                    break;
                }
            }
        }
    }
}