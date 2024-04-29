using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.Option.View
{
    /// <summary>
    /// [初期設定]-[オプション] Inspector
    /// </summary>
    public class OptionInspectorElement : AbstractInspectorElement
    {
        private          SceneWindow               _sceneView;
        private          SystemSettingDataModel    _systemSettings;


        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Option/Asset/inspector_option.uxml"; } }

        public OptionInspectorElement() {
            _sceneView = DatabaseEditor.DatabaseEditor.GetDatabaseSceneWindow();

            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            _systemSettings = databaseManagementService.LoadSystem();
            Initialize();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            //透明状態で開始
            Toggle option0 = RootContainer.Query<Toggle>("option_0");
            option0.value = _systemSettings.optionSetting.optTransparent == 1;
            option0.RegisterValueChangedCallback(evt =>
            {
                _systemSettings.optionSetting.optTransparent = option0.value ? 1 : 0;
                Save();
            });

            //パーティの隊列歩行
            Toggle option1 = RootContainer.Query<Toggle>("option_1");
            option1.value = _systemSettings.optionSetting.optFollowers == 1;
            option1.RegisterValueChangedCallback(evt =>
            {
                _systemSettings.optionSetting.optFollowers = option1.value ? 1 : 0;
                Save();
            });

            //スリップダメージで戦闘不能
            Toggle option2 = RootContainer.Query<Toggle>("option_2");
            option2.value = _systemSettings.optionSetting.optSlipDeath == 1
                ? true
                : false;
            option2.RegisterValueChangedCallback(evt =>
            {
                _systemSettings.optionSetting.optSlipDeath =
                    option2.value ? 1 : 0;
                Save();
            });

            //床ダメージで戦闘不能
            Toggle option3 = RootContainer.Query<Toggle>("option_3");
            option3.value = _systemSettings.optionSetting.optFloorDeath == 1
                ? true
                : false;
            option3.RegisterValueChangedCallback(evt =>
            {
                _systemSettings.optionSetting.optFloorDeath =
                    option3.value ? 1 : 0;
                Save();
            });

            //控えメンバーも経験値を獲得
            Toggle option4 = RootContainer.Query<Toggle>("option_4");
            option4.value = _systemSettings.optionSetting.optExtraExp == 1
                ? true
                : false;
            option4.RegisterValueChangedCallback(evt =>
            {
                _systemSettings.optionSetting.optExtraExp =
                    option4.value ? 1 : 0;
                Save();
            });

            //大事なものの個数を表示
            Toggle option5 = RootContainer.Query<Toggle>("option_5");
            option5.value = _systemSettings.optionSetting.showKeyItemNum == 1
                ? true
                : false;
            option5.RegisterValueChangedCallback(evt =>
            {
                _systemSettings.optionSetting.showKeyItemNum =
                    option5.value ? 1 : 0;
                Save();
            });

            //オートセーブを有効化
            Toggle option6 = RootContainer.Query<Toggle>("option_6");
            option6.value = _systemSettings.optionSetting.enabledAutoSave == 1
                ? true
                : false;
            option6.RegisterValueChangedCallback(evt =>
            {
                _systemSettings.optionSetting.enabledAutoSave =
                    option6.value ? 1 : 0;
                Save();
            });
        }

        override protected void SaveContents() {
            //セーブ部位の作成
            databaseManagementService.SaveSystem(_systemSettings);
        }
    }
}