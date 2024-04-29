using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.Editor.Hierarchy.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Map.View;
using System.Collections.Generic;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Map
{
    /// <summary>
    /// サンプルマップのHierarchy
    /// </summary>
    public class MapSampleHierarchy : AbstractHierarchy
    {
        private List<EventMapDataModel> _eventMapDataModels;
        private List<MapDataModel> _mapEntities;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MapSampleHierarchy() {
            View = new MapSampleHierarchyView(this);
        }

        /// <summary>
        /// View
        /// </summary>
        public MapSampleHierarchyView View { get; }

        /// <summary>
        /// データの読込
        /// </summary>
        override protected void LoadData() {
            base.LoadData();
            _mapEntities = mapManagementService.LoadMapSamples();
            _eventMapDataModels = eventManagementService.LoadEventMap();
        }

        /// <summary>
        /// Viewの更新
        /// </summary>
        protected override void UpdateView(string updateData = null) {
            base.UpdateView();
            View.Refresh(_mapEntities, _eventMapDataModels);
        }
    }
}