using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.UiSetting;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.BattleMenu.View
{
    /// <summary>
    /// [初期設定]-[UI設定]-[バトルメニュー] Inspector
    /// </summary>
    public class UIBattleMenuInspectorElement : AbstractInspectorElement
    {
        private readonly SceneWindow               _sceneView;
        private          UiSettingDataModel        _uiSettingDataModel;

        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/BattleMenu/Asset/inspector_UI_battleMenu.uxml"; } }

        public UIBattleMenuInspectorElement() {
            _sceneView =
                WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.DatabaseSceneWindow) as
                    SceneWindow;
            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            _uiSettingDataModel = databaseManagementService.LoadUiSettingDataModel();

            _sceneView.Create(SceneWindow.PreviewId.Battle);
            _sceneView.GetBattlePreview().SetUiData(_uiSettingDataModel);
            _sceneView.Init();
            // 初回の表示サイズが変わるので一旦固定値（アンカー設定周りが影響していそう）
            _sceneView.SetRenderingSize(1980, 1200);
            _sceneView.Render();
            Initialize();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            //HP
            Toggle battleMenu0 = RootContainer.Query<Toggle>("battleMenu_0");
            battleMenu0.value = _uiSettingDataModel.battleMenu.menuHp.enabled == 1 ? true : false;
            battleMenu0.RegisterValueChangedCallback(evt =>
            {
                _uiSettingDataModel.battleMenu.menuHp.enabled = battleMenu0.value ? 1 : 0;
                Save();
            });

            //MP
            Toggle battleMenu1 = RootContainer.Query<Toggle>("battleMenu_1");
            battleMenu1.value = _uiSettingDataModel.battleMenu.menuMp.enabled == 1 ? true : false;
            battleMenu1.RegisterValueChangedCallback(evt =>
            {
                _uiSettingDataModel.battleMenu.menuMp.enabled = battleMenu1.value ? 1 : 0;
                Save();
            });

            //TP
            Toggle battleMenu2 = RootContainer.Query<Toggle>("battleMenu_2");
            battleMenu2.value = _uiSettingDataModel.battleMenu.menuTp.enabled == 1 ? true : false;
            battleMenu2.RegisterValueChangedCallback(evt =>
            {
                _uiSettingDataModel.battleMenu.menuTp.enabled = battleMenu2.value ? 1 : 0;
                Save();
            });
        }

        override protected void SaveContents() {
            databaseManagementService.SaveUiSettingDataModel(_uiSettingDataModel);
            _sceneView.GetBattlePreview().SetUiData(_uiSettingDataModel);
            _sceneView.Render();
        }
    }
}