using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.Editor.Hierarchy.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Outline.View;
using System.Collections.Generic;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Outline
{
    /// <summary>
    /// アウトラインのHierarchy
    /// </summary>
    public class OutlineHierarchy : AbstractHierarchy
    {
        private List<EventMapDataModel> _eventMapDataModels;
        private List<MapDataModel> _mapDataModels;
        private OutlineDataModel _outlineDataModel;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public OutlineHierarchy() {
            View = new OutlineHierarchyView(this);
        }

        /// <summary>
        /// View
        /// </summary>
        public OutlineHierarchyView View { get; }

        /// <summary>
        /// データの読
        /// </summary>
        override protected void LoadData() {
            base.LoadData();
            _outlineDataModel = outlineManagementService.LoadOutline();
            _mapDataModels = mapManagementService.LoadMaps();
            _eventMapDataModels = eventManagementService.LoadEventMap();
        }

        /// <summary>
        /// Viewの更新
        /// </summary>
        protected override void UpdateView(string updateData = null) {
            base.UpdateView();
            View.Refresh(_outlineDataModel, _mapDataModels, _eventMapDataModels, updateData);
        }
    }
}