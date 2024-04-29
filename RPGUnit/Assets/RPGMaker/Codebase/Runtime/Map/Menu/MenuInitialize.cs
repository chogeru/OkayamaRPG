using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.Runtime.Common;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Map.Menu
{
    public class MenuInitialize : MonoBehaviour
    {
        private const string UI_PATH = "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/";

        private SystemSettingDataModel _systemSettingDataModel;

        public static bool enableDebugTool =
#if UNITY_EDITOR
            true
#else
            false
#endif
            ;

        // 開始時にウィンドウを追加だけ行う
        private void Start() {
            _systemSettingDataModel = DataManager.Self().GetSystemDataModel();

            var pattern = int.Parse(_systemSettingDataModel.uiPatternId) + 1;
            if (pattern < 1 || pattern > 6)
                pattern = 1;

            // メニューPrefabロード、生成
            var path = UI_PATH + "MenuWindow0" + pattern + ".prefab";
            var menuPrefab = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<GameObject>(path);
            menuPrefab.SetActive(true);
            Instantiate(menuPrefab).transform.SetParent(transform);

            if (enableDebugTool)
            {
                // DebugTool Prefabロード、生成
                var debugToolPath = UI_PATH + "DebugToolWindow.prefab";
                var debugToolMenuPrefab = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<GameObject>(debugToolPath);
                debugToolMenuPrefab.SetActive(true);
                Instantiate(debugToolMenuPrefab).transform.SetParent(transform);
            }

            //Canvas大きさ設定
            var displaySize = DataManager.Self().GetSystemDataModel()
                .DisplaySize[DataManager.Self().GetSystemDataModel().displaySize];
            var scales = transform.GetComponentsInChildren<CanvasScaler>();
            foreach (var scale in scales)
            {
                scale.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                //scale.referenceResolution = displaySize;
            }
        }
    }
}