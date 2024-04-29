using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.Editor.Hierarchy.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Flags.View;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Flags
{
    /// <summary>
    /// スイッチのHierarchy
    /// </summary>
    public class FlagsHierarchy : AbstractHierarchy
    {
        private FlagDataModel _flagDataModel;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FlagsHierarchy() {
            View = new FlagsHierarchyView(this);
        }

        /// <summary>
        /// View
        /// </summary>
        public FlagsHierarchyView View { get; }

        /// <summary>
        /// データの読込
        /// </summary>
        override protected void LoadData() {
            base.LoadData();
            _flagDataModel = databaseManagementService.LoadFlags();
        }

        /// <summary>
        /// Viewの更新
        /// </summary>
        protected override void UpdateView(string updateData = null) {
            base.UpdateView();
            View.Refresh(_flagDataModel);
        }

        /// <summary>
        /// スイッチのInspector表示
        /// </summary>
        /// <param name="sw"></param>
        public void OpenSwitchInspector(FlagDataModel.Switch sw) {
            Inspector.Inspector.SwitchEditView(sw);
        }

        /// <summary>
        /// スイッチの新規作成
        /// </summary>
        public void CreateSwitch() {
            var newModel = FlagDataModel.Switch.CreateDefault();
            _flagDataModel.switches.Add(newModel);
            databaseManagementService.SaveFlags(_flagDataModel);

            Refresh();
        }

        /// <summary>
        /// スイッチの削除
        /// </summary>
        public void DeleteSwitchAtTail() {
            _flagDataModel.switches.RemoveAt(_flagDataModel.switches.Count - 1);
            databaseManagementService.SaveFlags(_flagDataModel);
            Refresh();
        }

        /// <summary>
        /// 変数のInspector表示
        /// </summary>
        /// <param name="variable"></param>
        public void OpenVariableInspector(FlagDataModel.Variable variable) {
            Inspector.Inspector.VariableEditView(variable);
        }

        /// <summary>
        /// 変数の新規作成
        /// </summary>
        public void CreateVariable() {
            var newModel = FlagDataModel.Variable.CreateDefault();
            _flagDataModel.variables.Add(newModel);
            databaseManagementService.SaveFlags(_flagDataModel);

            Refresh();
        }

        /// <summary>
        /// 変数の削除
        /// </summary>
        public void DeleteVariableAtTail() {
            _flagDataModel.variables.RemoveAt(_flagDataModel.variables.Count - 1);
            databaseManagementService.SaveFlags(_flagDataModel);
            Refresh();
        }
    }
}