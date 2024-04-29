using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.Runtime.Common;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Battle.Window
{
    /// <summary>
    /// Unite用のバトルで利用するPrefab生成クラス
    /// UIパターンに応じてPrefabをロードする
    /// </summary>
    public class WindowInitialize
    {
        private SystemSettingDataModel _systemSettingDataModel;

        /// <summary>
        /// バトル用のPrefabをロードする
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public GameObject Create(GameObject obj) {
            // ウィンドウの作成のみ行う
            _systemSettingDataModel = DataManager.Self().GetSystemDataModel();

            var pattern = int.Parse(_systemSettingDataModel.uiPatternId) + 1;
            if (pattern < 1 || pattern > 6)
                pattern = 1;

            var window =
                Object.Instantiate(
                    UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<GameObject>(
                        "Assets/RPGMaker/Codebase/Runtime/Battle/Windows0" + pattern + ".prefab"));
            window.transform.SetParent(obj.transform, false);
            return window;
        }
    }
}