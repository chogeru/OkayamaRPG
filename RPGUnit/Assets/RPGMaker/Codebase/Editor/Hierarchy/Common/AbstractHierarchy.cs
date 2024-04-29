using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement;
using RPGMaker.Codebase.CoreSystem.Service.OutlineManagement;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;

namespace RPGMaker.Codebase.Editor.Hierarchy.Common
{
    /// <summary>
    /// 各Hierarchy表示の基底クラス
    /// </summary>
    public class AbstractHierarchy
    {
        /// <summary>
        /// データの更新処理中かどうか
        /// </summary>
        private bool _isRefresh = false;

        /// <summary>
        /// MapManagementService取得
        /// </summary>
        /// <returns></returns>
        protected MapManagementService mapManagementService
        {
            get
            {
                return Hierarchy.mapManagementService;
            }
        }
        /// <summary>
        /// EventManagementService取得
        /// </summary>
        /// <returns></returns>
        protected EventManagementService eventManagementService{
            get
            {
                return Hierarchy.eventManagementService;
            }
        }
        /// <summary>
        /// DatabaseManagementService取得
        /// </summary>
        /// <returns></returns>
        protected DatabaseManagementService databaseManagementService
        {
            get
            {
                return Hierarchy.databaseManagementService;
            }
        }
        /// <summary>
        /// OutlineManagementService取得
        /// </summary>
        /// <returns></returns>
        protected OutlineManagementService outlineManagementService
        {
            get
            {
                return Hierarchy.outlineManagementService;
            }
        }
        /// <summary>
        /// 更新処理
        /// </summary>
        public virtual void Refresh(string updateData = null) {
            if (_isRefresh) return;
            _isRefresh = true;
            LoadData();
            UpdateView(updateData);
            _isRefresh = false;
        }
        /// <summary>
        /// データの読込
        /// </summary>
        protected virtual void LoadData() { }
        /// <summary>
        /// Viewの更新
        /// </summary>
        protected virtual void UpdateView(string updateData = null) { }
        /// <summary>
        /// 現在メインになっているWindowの取得
        /// </summary>
        /// <returns></returns>
        protected static WindowLayoutManager.WindowLayoutId GetCurrentMainWindowId() {
            return new List<WindowLayoutManager.WindowLayoutId>
            {
                WindowLayoutManager.WindowLayoutId.MapEditWindow,
                WindowLayoutManager.WindowLayoutId.MapEventExecutionContentsWindow,
                WindowLayoutManager.WindowLayoutId.MapEventCommandSettingWindow,
                WindowLayoutManager.WindowLayoutId.MapEventEditWindow
            }.Find(id => WindowLayoutManager.IsActiveWindow(id));
        }

        /// <summary>
        /// 貼り付け時の名前生成
        /// </summary>
        /// <param name="names"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public string CreateDuplicateName(List<string> names, string output) {
            string name = output;
            while (names.Contains(name))
            {
                name += " " + EditorLocalize.LocalizeText("WORD_1462");
            }
            return name;
        }

    }
}