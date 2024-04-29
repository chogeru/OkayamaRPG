using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Enum;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.Map.View
{
    /// <summary>
    /// [マップ設定]-[タイルグループ] Inspector
    /// </summary>
    public class TileGroupInspector : AbstractInspectorElement
    {
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Map/Asset/TileGroupInspector.uxml"; } }

        private readonly TileGroupDataModel _tileGroupDataModel;

        private Label     _tileIdLabel;
        private ImTextField _tileNameText;

        public TileGroupInspector(TileGroupDataModel tileGroupDataModel) {
            _tileGroupDataModel = tileGroupDataModel;

            Initialize();
            SetEntityToUI();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            _tileIdLabel = RootContainer.Query<Label>("tile_id");
            _tileNameText = RootContainer.Query<ImTextField>("tile_name");
        }

        private void SetEntityToUI() {
            _tileIdLabel.text = _tileGroupDataModel.SerialNumberString;
            _tileNameText.value = _tileGroupDataModel.name;
            _tileNameText.RegisterCallback<FocusOutEvent>(evt =>
            {
                _tileGroupDataModel.name = _tileNameText.value;
                mapManagementService.SaveTileGroup(_tileGroupDataModel);
                _ = Editor.Hierarchy.Hierarchy.Refresh(Region.TileGroup);
                Refresh();
            });
        }
    }
}