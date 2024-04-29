using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.UiSetting;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.GameMenu.View
{
    /// <summary>
    /// [初期設定]-[UI設定]-[ゲームメニュー] Inspector
    /// </summary>
    public class UIGameMenuInspectorElement : AbstractInspectorElement
    {
        private readonly int                       _displayType;
        private readonly SceneWindow               _sceneWindow;
        private          UiSettingDataModel        _uiSettingDataModel;

        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/GameMenu/Asset/inspector_UI_gameMenu.uxml"; } }

        public UIGameMenuInspectorElement(int num) {
            _sceneWindow =
                WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.DatabaseSceneWindow) as
                    SceneWindow;
            _displayType = num;
            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            _uiSettingDataModel = databaseManagementService.LoadUiSettingDataModel();
            _sceneWindow.Create(SceneWindow.PreviewId.Menu);
            _sceneWindow.GetMenuPreview().SetUiData(_uiSettingDataModel);
            _sceneWindow.GetMenuPreview().SetDisplayType(_displayType);
            _sceneWindow.Init();
            _sceneWindow.SetRenderingSize(_sceneWindow.GetRenderingSize().x,
                _sceneWindow.GetRenderingSize().x * 9 / 16);
            _sceneWindow.Render();

            // メインとアイテムウィンドウのみinspectorを作成
            if (_displayType == 0 || _displayType == 1)
                Initialize();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            Toggle gamemenuItemToggle = RootContainer.Query<Toggle>("gamemenu_item_toggle");
            gamemenuItemToggle.value = _uiSettingDataModel.gameMenu.menuItem.enabled == 0 ? true : false;
            gamemenuItemToggle.RegisterValueChangedCallback(evt =>
            {
                _uiSettingDataModel.gameMenu.menuItem.enabled = gamemenuItemToggle.value ? 0 : 1;
                Save();
            });

            Toggle gamemenuSkillToggle = RootContainer.Query<Toggle>("gamemenu_skill_toggle");
            gamemenuSkillToggle.value = _uiSettingDataModel.gameMenu.menuSkill.enabled == 0 ? true : false;
            gamemenuSkillToggle.RegisterValueChangedCallback(evt =>
            {
                _uiSettingDataModel.gameMenu.menuSkill.enabled = gamemenuSkillToggle.value ? 0 : 1;
                Save();
            });

            Toggle gamemenuEquipmentToggle = RootContainer.Query<Toggle>("gamemenu_equipment_toggle");
            gamemenuEquipmentToggle.value =
                _uiSettingDataModel.gameMenu.menuEquipment.enabled == 0 ? true : false;
            gamemenuEquipmentToggle.RegisterValueChangedCallback(evt =>
            {
                _uiSettingDataModel.gameMenu.menuEquipment.enabled =
                    gamemenuEquipmentToggle.value ? 0 : 1;
                Save();
            });

            Toggle gamemenuStatusToggle = RootContainer.Query<Toggle>("gamemenu_status_toggle");
            gamemenuStatusToggle.value = _uiSettingDataModel.gameMenu.menuStatus.enabled == 0 ? true : false;
            gamemenuStatusToggle.RegisterValueChangedCallback(evt =>
            {
                _uiSettingDataModel.gameMenu.menuStatus.enabled = gamemenuStatusToggle.value ? 0 : 1;
                Save();
            });

            Toggle gamemenuSortToggle = RootContainer.Query<Toggle>("gamemenu_sort_toggle");
            gamemenuSortToggle.value = _uiSettingDataModel.gameMenu.menuSort.enabled == 0 ? true : false;
            gamemenuSortToggle.RegisterValueChangedCallback(evt =>
            {
                _uiSettingDataModel.gameMenu.menuSort.enabled = gamemenuSortToggle.value ? 0 : 1;
                Save();
            });

            Toggle gamemenuSaveToggle = RootContainer.Query<Toggle>("gamemenu_save_toggle");
            gamemenuSaveToggle.value = _uiSettingDataModel.gameMenu.menuSave.enabled == 0 ? true : false;
            gamemenuSaveToggle.RegisterValueChangedCallback(evt =>
            {
                _uiSettingDataModel.gameMenu.menuSave.enabled = gamemenuSaveToggle.value ? 0 : 1;
                Save();
            });

            Toggle gamemenuOptionToggle = RootContainer.Query<Toggle>("gamemenu_option_toggle");
            gamemenuOptionToggle.value = _uiSettingDataModel.gameMenu.menuOption.enabled == 0 ? true : false;
            gamemenuOptionToggle.RegisterValueChangedCallback(evt =>
            {
                _uiSettingDataModel.gameMenu.menuOption.enabled = gamemenuOptionToggle.value ? 0 : 1;
                Save();
            });

            Toggle gamemenuEndToggle = RootContainer.Query<Toggle>("gamemenu_end_toggle");
            gamemenuEndToggle.value = _uiSettingDataModel.gameMenu.menuGameEnd.enabled == 0 ? true : false;
            gamemenuEndToggle.RegisterValueChangedCallback(evt =>
            {
                _uiSettingDataModel.gameMenu.menuGameEnd.enabled = gamemenuEndToggle.value ? 0 : 1;
                Save();
            });

            Toggle itemItemToggle = RootContainer.Query<Toggle>("item_item_toggle");
            itemItemToggle.value = _uiSettingDataModel.gameMenu.categoryItem.enabled == 0 ? true : false;
            itemItemToggle.RegisterValueChangedCallback(evt =>
            {
                _uiSettingDataModel.gameMenu.categoryItem.enabled = itemItemToggle.value ? 0 : 1;
                Save();
            });

            Toggle itemWeaponToggle = RootContainer.Query<Toggle>("item_weapon_toggle");
            itemWeaponToggle.value = _uiSettingDataModel.gameMenu.categoryWeapon.enabled == 0 ? true : false;
            itemWeaponToggle.RegisterValueChangedCallback(evt =>
            {
                _uiSettingDataModel.gameMenu.categoryWeapon.enabled = itemWeaponToggle.value ? 0 : 1;
                Save();
            });

            Toggle itemArmorToggle = RootContainer.Query<Toggle>("item_armor_toggle");
            itemArmorToggle.value = _uiSettingDataModel.gameMenu.categoryArmor.enabled == 0 ? true : false;
            itemArmorToggle.RegisterValueChangedCallback(evt =>
            {
                _uiSettingDataModel.gameMenu.categoryArmor.enabled = itemArmorToggle.value ? 0 : 1;
                Save();
            });

            Toggle itemImportantToggle = RootContainer.Query<Toggle>("item_important_toggle");
            itemImportantToggle.value = _uiSettingDataModel.gameMenu.categoryImportant.enabled == 0 ? true : false;
            itemImportantToggle.RegisterValueChangedCallback(evt =>
            {
                _uiSettingDataModel.gameMenu.categoryImportant.enabled = itemImportantToggle.value ? 0 : 1;
                Save();
            });

            //フォント設定
            VisualElement gamemenuTextDropdown = RootContainer.Query<VisualElement>("gamemenu_text_dropdown");
            var gamemenuTextDropdownChoices =
                EditorLocalize.LocalizeTexts(Font.GetOSInstalledFontNames().ToList());
            var gamemenuTextDropdownPopupField =
                new PopupFieldBase<string>(gamemenuTextDropdownChoices, 0);
            gamemenuTextDropdown.Add(gamemenuTextDropdownPopupField);
            gamemenuTextDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                _uiSettingDataModel.gameMenu.menuFontSetting.font = gamemenuTextDropdownPopupField.value;
                Save();
            });
            //サイズ設定
            ImTextField gamemenuFontSize = RootContainer.Query<ImTextField>("gamemenu_font_size");
            Slider gamemenuFontSizeSlider = RootContainer.Query<Slider>("gamemenu_fontSize_slider");
            gamemenuFontSizeSlider.value = _uiSettingDataModel.gameMenu.menuFontSetting.size;
            gamemenuFontSizeSlider.RegisterValueChangedCallback(evt =>
            {
                _uiSettingDataModel.gameMenu.menuFontSetting.size = (int) gamemenuFontSizeSlider.value;
                gamemenuFontSize.value = gamemenuFontSizeSlider.value.ToString();
                Save();
            });
            gamemenuFontSize.value = _uiSettingDataModel.gameMenu.menuFontSetting.size.ToString();
            gamemenuFontSize.RegisterCallback<FocusOutEvent>(evt =>
            {
                _uiSettingDataModel.gameMenu.menuFontSetting.size = int.Parse(gamemenuFontSize.value);
                gamemenuFontSizeSlider.value = int.Parse(gamemenuFontSize.value);
                Save();
            });
            //フォントカラー
            ImTextField fontColorR = RootContainer.Query<ImTextField>("font_color_R");
            fontColorR.value = _uiSettingDataModel.gameMenu.menuFontSetting.color[0].ToString();
            fontColorR.RegisterCallback<FocusOutEvent>(evt =>
            {
                _uiSettingDataModel.gameMenu.menuFontSetting.color[0] = int.Parse(fontColorR.value);
                Save();
            });
            ImTextField fontColorG = RootContainer.Query<ImTextField>("font_color_G");
            fontColorG.value = _uiSettingDataModel.gameMenu.menuFontSetting.color[1].ToString();
            fontColorG.RegisterCallback<FocusOutEvent>(evt =>
            {
                _uiSettingDataModel.gameMenu.menuFontSetting.color[1] = int.Parse(fontColorG.value);
                Save();
            });
            ImTextField fontColorB = RootContainer.Query<ImTextField>("font_color_B");
            fontColorB.value = _uiSettingDataModel.gameMenu.menuFontSetting.color[2].ToString();
            fontColorB.RegisterCallback<FocusOutEvent>(evt =>
            {
                _uiSettingDataModel.gameMenu.menuFontSetting.color[2] = int.Parse(fontColorB.value);
                Save();
            });

            // 表示制御（0はメニュー項目のみ表示、1はアイテム項目のみ表示）
            if (_displayType == 0)
                RootContainer.Q<Foldout>("item_foldout").style.display = DisplayStyle.None;
            else
                RootContainer.Q<Foldout>("menu_foldout").style.display = DisplayStyle.None;
        }

        override protected void SaveContents() {
            //セーブ部位の作成
            databaseManagementService.SaveUiSettingDataModel(_uiSettingDataModel);
            _sceneWindow.GetMenuPreview().SetUiData(_uiSettingDataModel);
            _sceneWindow.GetMenuPreview().MenuDisplayChangeSetting();
            _sceneWindow.Render();
        }
    }
}