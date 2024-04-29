using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.State;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Region.State.View;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.State
{
    /// <summary>
    /// ステートのHierarchy
    /// </summary>
    public class StateHierarchy : AbstractHierarchy
    {
        private List<StateDataModel> _stateDataModels;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public StateHierarchy() {
            View = new StateHierarchyView(this);
        }

        /// <summary>
        /// View
        /// </summary>
        public StateHierarchyView View { get; }

        /// <summary>
        /// データの読込
        /// </summary>
        override protected void LoadData() {
            base.LoadData();
            _stateDataModels = databaseManagementService.LoadStateEdit();
        }

        /// <summary>
        /// Viewの更新
        /// </summary>
        protected override void UpdateView(string updateData = null) {
            base.UpdateView();
            View.Refresh(_stateDataModels);
        }

        /// <summary>
        /// ステートのInspector表示
        /// </summary>
        /// <param name="stateDataModel"></param>
        public void OpenStateInspector(StateDataModel stateDataModel) {
            Inspector.Inspector.StateEditView(stateDataModel);
        }

        /// <summary>
        /// ステート作成
        /// </summary>
        public void CreateStateDataModel() {
            var newModel = StateDataModel.CreateDefault(Guid.NewGuid().ToString());
            newModel.name = "#" + string.Format("{0:D4}", _stateDataModels.Count + 1) + "　" + 
                            EditorLocalize.LocalizeText("WORD_1518");
            _stateDataModels.Add(newModel);
            databaseManagementService.SaveStateEdit(_stateDataModels);


            Refresh();

        }

        /// <summary>
        /// ステートのコピー＆貼り付け処理
        /// </summary>
        /// <param name="stateDataModel"></param>
        public void DuplicateStateDataModel(StateDataModel stateDataModel) {
            var duplicated = stateDataModel.DataClone();
            duplicated.id = Guid.NewGuid().ToString();
            duplicated.name = CreateDuplicateName(_stateDataModels.Select(s => s.name).ToList(), duplicated.name);
            _stateDataModels.Add(duplicated);
            databaseManagementService.SaveStateEdit(_stateDataModels);
            Refresh();
        }

        /// <summary>
        /// ステートの削除
        /// </summary>
        /// <param name="stateDataModel"></param>
        public void DeleteStateDataModel(StateDataModel stateDataModel) {
            _stateDataModels.Remove(stateDataModel);
            databaseManagementService.SaveStateEdit(_stateDataModels);
            Refresh();
        }
    }
}