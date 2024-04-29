using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.Editor.Hierarchy.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Map.View;
using System.Collections.Generic;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Map
{
    /// <summary>
    /// MapのHierarchy
    /// </summary>
    public class MapHierarchy : AbstractHierarchy
    {
        private List<EventMapDataModel> _eventMapDataModels;
        private List<MapDataModel> _mapEntities;
        private List<TileGroupDataModel> _tileGroupEntities;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MapHierarchy() {
            View = new MapHierarchyView(this);
        }

        /// <summary>
        /// View
        /// </summary>
        public MapHierarchyView View { get; }

        /// <summary>
        /// データの読込
        /// </summary>
        override protected void LoadData() {
            base.LoadData();
            _mapEntities = mapManagementService.LoadMaps();
            _tileGroupEntities = mapManagementService.LoadTileGroups();
            _eventMapDataModels = eventManagementService.LoadEventMap();
        }

        /// <summary>
        /// Viewの更新
        /// </summary>
        protected override void UpdateView(string updateData = null) {
            base.UpdateView();
            View.Refresh(updateData, _mapEntities, _tileGroupEntities, _eventMapDataModels);
        }
    }
}