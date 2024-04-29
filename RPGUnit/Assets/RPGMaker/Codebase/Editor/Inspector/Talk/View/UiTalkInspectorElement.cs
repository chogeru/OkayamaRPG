using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.UiSetting;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.View.Preview;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.Talk.View
{
    /// <summary>
    /// [初期設定]-[UI設定]-[会話ウィンドウ] Inspector
    /// </summary>
    public class UiTalkInspectorElement : AbstractInspectorElement
    {
        private const int MIN_POTISION = 0;
        private const int MAX_POTISION_X = 1920;
        private const int MAX_POTISION_Y = 1080;

        //フォントリスト
        private readonly List<string> _fontSelectList = FontManager.GetFontList();

        private readonly int _num;

        // シーンウィンドウ
        private readonly SceneWindow _sceneWindow;

        //保存する部分
        private UiSettingDataModel _uiSettingDataModel;

        //会話ウィンドウの子項目の各種インスペクター
        private readonly string characlrUxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/Talk/Asset/inspector_UI_talk_character.uxml";

        private readonly string itemSelectUxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/Talk/Asset/inspector_UI_talk_item_select.uxml";

        private readonly string numberUxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/Talk/Asset/inspector_UI_talk_number.uxml";

        private readonly string selectUxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/Talk/Asset/inspector_UI_talk_select.uxml";

        public UiTalkInspectorElement(int num) {
            _num = num;
            _sceneWindow =
                WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.DatabaseSceneWindow) as
                    SceneWindow;
            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            _uiSettingDataModel = databaseManagementService.LoadUiSettingDataModel();

            // シーン設定
            // ウィンドウ毎のサイズ
            var sceneSize = new List<Vector2>
            {
                new Vector2(2400, 1080),
                new Vector2(860, 1080),
                new Vector2(2400, 1080),
                new Vector2(960, 960)
            };
            _sceneWindow.Create(SceneWindow.PreviewId.TalkWindow);
            _sceneWindow.GetTalkWindowPreview()
                .SetWindowType(
                    (TalkWindowPreview.TalkWindowType) Enum.ToObject(
                        typeof(TalkWindowPreview.TalkWindowType), _num));
            _sceneWindow.GetTalkWindowPreview().SetUiData(_uiSettingDataModel);
            _sceneWindow.Init();
            _sceneWindow.SetRenderingSize((int) sceneSize[_num].x, (int) sceneSize[_num].y);
            _sceneWindow.Render();

            switch (_num)
            {
                case 0:
                    CreateCharacter();
                    break;
                case 1:
                    CreateSelect();
                    break;
                case 2:
                    CreateNumber();
                    break;
                case 3:
                    CreateItemSelect();
                    break;
            }
        }

        private void CreateCharacter() {
            MainUxml = characlrUxml;
            base.InitializeContents();

            var characterMenu = _uiSettingDataModel.talkMenu.characterMenu;

            Toggle nameDisplay = RootContainer.Query<Toggle>("name_display");
            nameDisplay.value = characterMenu.nameEnabled == 1;
            nameDisplay.RegisterValueChangedCallback(evt =>
            {
                characterMenu.nameEnabled = nameDisplay.value ? 1 : 0;
                Save();
            });


            RadioButton characterDisplay0 = RootContainer.Query<RadioButton>("radioButton-initialization-display15");
            RadioButton characterDisplay1 = RootContainer.Query<RadioButton>("radioButton-initialization-display16");
            RadioButton characterDisplay2 = RootContainer.Query<RadioButton>("radioButton-initialization-display17");
            var displayActions = new List<Action>
            {
                //
                () =>
                {
                    characterMenu.characterEnabled = 0;
                    _uiSettingDataModel.talkMenu.characterMenu = characterMenu;
                    Save();
                },
                //
                () =>
                {
                    characterMenu.characterEnabled = 1;
                    _uiSettingDataModel.talkMenu.characterMenu = characterMenu;
                    Save();
                },
                //
                () =>
                {
                    characterMenu.characterEnabled = 2;
                    _uiSettingDataModel.talkMenu.characterMenu = characterMenu;
                    Save();
                }
            };

            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton>() {characterDisplay0, characterDisplay1, characterDisplay2},
                characterMenu.characterEnabled, displayActions);

            VisualElement fontSelectDropdown = RootContainer.Query<VisualElement>("font_select_dropDown");
            var commonDropdownPopupField =
                new PopupFieldBase<string>(_fontSelectList, FontNameToFontNumber(characterMenu.talkFontSetting.font));
            fontSelectDropdown.Add(commonDropdownPopupField);
            commonDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                FontManager.CreateFont(commonDropdownPopupField.index, FontManager.FONT_TYPE.MESSAGE);
                characterMenu.talkFontSetting.font = commonDropdownPopupField.value;
                _uiSettingDataModel.talkMenu.characterMenu = characterMenu;
                Save();
            });

            var font_size_sliderArea = RootContainer.Query<VisualElement>("font_size_sliderArea");
            SliderAndFiledBase.IntegerSliderCallBack(font_size_sliderArea, 10, 100, "",
                characterMenu.talkFontSetting.size, evt =>
                {
                    characterMenu.talkFontSetting.size = evt;
                    _uiSettingDataModel.talkMenu.characterMenu = characterMenu;
                    Save();
                });

            //font_rgb_text
            Label fontRgbText = RootContainer.Query<Label>("font_rgb_text");
            fontRgbText.text = string.Format("R : {0}, G : {1}, B: {2}", characterMenu.talkFontSetting.color[0],
                characterMenu.talkFontSetting.color[1], characterMenu.talkFontSetting.color[2]);
            //font_color
            ColorFieldBase fontColor = RootContainer.Query<ColorFieldBase>("font_color");
            fontColor.value = new Color(characterMenu.talkFontSetting.color[0] / 255f,
                characterMenu.talkFontSetting.color[1] / 255f, characterMenu.talkFontSetting.color[2] / 255f);
            fontColor.RegisterCallback<FocusOutEvent>(evt =>
            {
                var co = fontColor.value * 255;
                var r = co.r;
                var g = co.g;
                var b = co.b;
                var colors = new List<int>
                {
                    (int) Math.Ceiling(r),
                    (int) Math.Ceiling(g),
                    (int) Math.Ceiling(b)
                };
                characterMenu.talkFontSetting.color = colors.ToList();
                fontRgbText.text = string.Format("R : {0}, G : {1}, B: {2}", characterMenu.talkFontSetting.color[0],
                    characterMenu.talkFontSetting.color[1], characterMenu.talkFontSetting.color[2]);
                _uiSettingDataModel.talkMenu.characterMenu = characterMenu;
                Save();
            });

            VisualElement fontSelectDropdown2 = RootContainer.Query<VisualElement>("font_select_dropDown2");
            var commonDropdownPopupField2 =
                new PopupFieldBase<string>(_fontSelectList, FontNameToFontNumber(characterMenu.nameFontSetting.font));
            fontSelectDropdown2.Add(commonDropdownPopupField2);
            commonDropdownPopupField2.RegisterValueChangedCallback(evt =>
            {
                FontManager.CreateFont(commonDropdownPopupField2.index, FontManager.FONT_TYPE.MESSAGE_NAME);
                characterMenu.nameFontSetting.font = commonDropdownPopupField2.value;
                _uiSettingDataModel.talkMenu.characterMenu = characterMenu;
                Save();
            });

            var font_size_slider2Area = RootContainer.Query<VisualElement>("font_size_slider2Area");
            SliderAndFiledBase.IntegerSliderCallBack(font_size_slider2Area, 10, 100, "",
                characterMenu.nameFontSetting.size, evt =>
                {
                    characterMenu.nameFontSetting.size = (int) evt;
                    _uiSettingDataModel.talkMenu.characterMenu = characterMenu;
                    Save();
                });

            //font_rgb_text
            Label fontRgbText2 = RootContainer.Query<Label>("font_rgb_text2");
            fontRgbText2.text = string.Format("R : {0}, G : {1}, B: {2}",
                characterMenu.nameFontSetting.color[0],
                characterMenu.nameFontSetting.color[1], characterMenu.nameFontSetting.color[2]);
            //font_color
            ColorFieldBase fontColor2 = RootContainer.Query<ColorFieldBase>("font_color2");
            fontColor2.value = new Color(characterMenu.nameFontSetting.color[0] / 255f,
                characterMenu.nameFontSetting.color[1] / 255f, characterMenu.nameFontSetting.color[2] / 255f);
            fontColor2.RegisterCallback<FocusOutEvent>(evt =>
            {
                var co = fontColor2.value * 255;
                var r = co.r;
                var g = co.g;
                var b = co.b;
                var colors = new List<int>
                {
                    (int) Math.Ceiling(r),
                    (int) Math.Ceiling(g),
                    (int) Math.Ceiling(b)
                };
                characterMenu.nameFontSetting.color = colors.ToList();
                fontRgbText2.text = string.Format("R : {0}, G : {1}, B: {2}",
                    characterMenu.nameFontSetting.color[0],
                    characterMenu.nameFontSetting.color[1], characterMenu.nameFontSetting.color[2]);
                _uiSettingDataModel.talkMenu.characterMenu = characterMenu;
                Save();
            });
        }

        private void CreateSelect() {
            MainUxml = selectUxml;
            base.InitializeContents();

            var selectMenu = _uiSettingDataModel.talkMenu.selectMenu;

            VisualElement fontSelectDropdown = RootContainer.Query<VisualElement>("font_select_dropDown");
            var commonDropdownPopupField =
                new PopupFieldBase<string>(_fontSelectList, FontNameToFontNumber(selectMenu.menuFontSetting.font));
            fontSelectDropdown.Add(commonDropdownPopupField);
            commonDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                FontManager.CreateFont(commonDropdownPopupField.index, FontManager.FONT_TYPE.MESSAGE_SELECT);
                selectMenu.menuFontSetting.font = commonDropdownPopupField.value;
                _uiSettingDataModel.talkMenu.selectMenu = selectMenu;
                Save();
            });

            var fontSizeSliderArea = RootContainer.Query<VisualElement>("font_size_sliderArea");
            SliderAndFiledBase.IntegerSliderCallBack(fontSizeSliderArea, 10, 100, "",
                selectMenu.menuFontSetting.size, evt =>
                {
                    selectMenu.menuFontSetting.size = (int) evt;
                    _uiSettingDataModel.talkMenu.selectMenu = selectMenu;
                    Save();
                });

            //font_rgb_text
            Label fontRgbText = RootContainer.Query<Label>("font_rgb_text");
            fontRgbText.text = string.Format("R : {0}, G : {1}, B: {2}",
                selectMenu.menuFontSetting.color[0],
                selectMenu.menuFontSetting.color[1], selectMenu.menuFontSetting.color[2]);
            //font_color
            ColorFieldBase fontColor = RootContainer.Query<ColorFieldBase>("font_color");
            fontColor.value = new Color(selectMenu.menuFontSetting.color[0] / 255f,
                selectMenu.menuFontSetting.color[1] / 255f, selectMenu.menuFontSetting.color[2] / 255f);
            fontColor.RegisterCallback<FocusOutEvent>(evt =>
            {
                var co = fontColor.value * 255;
                var r = co.r;
                var g = co.g;
                var b = co.b;
                var colors = new List<int>
                {
                    (int) Math.Ceiling(r),
                    (int) Math.Ceiling(g),
                    (int) Math.Ceiling(b)
                };
                selectMenu.menuFontSetting.color = colors.ToList();
                fontRgbText.text = string.Format("R : {0}, G : {1}, B: {2}",
                    selectMenu.menuFontSetting.color[0],
                    selectMenu.menuFontSetting.color[1], selectMenu.menuFontSetting.color[2]);
                _uiSettingDataModel.talkMenu.selectMenu = selectMenu;
                Save();
            });
        }

        private void CreateNumber() {
            MainUxml = numberUxml;
            base.InitializeContents();

            var numberMenu = _uiSettingDataModel.talkMenu.numberMenu;

            var numberToggle = new List<RadioButton>();
            for (var i = 18; i <= 22; i++)
            {
                RadioButton toggle = RootContainer.Query<RadioButton>("radioButton-initialization-display" + i);
                toggle.value = numberMenu.numberEnabled == i;
                numberToggle.Add(toggle);
            }

            var numberActions = new List<Action>
            {
                //中央
                () =>
                {
                    numberMenu.numberEnabled = 0;
                    _uiSettingDataModel.talkMenu.numberMenu = numberMenu;
                    Save();
                },
                //右下
                () =>
                {
                    numberMenu.numberEnabled = 1;
                    _uiSettingDataModel.talkMenu.numberMenu = numberMenu;
                    Save();
                },
                //右上
                () =>
                {
                    numberMenu.numberEnabled = 2;
                    _uiSettingDataModel.talkMenu.numberMenu = numberMenu;
                    Save();
                },
                //左下
                () =>
                {
                    numberMenu.numberEnabled = 3;
                    _uiSettingDataModel.talkMenu.numberMenu = numberMenu;
                    Save();
                },
                //左上
                () =>
                {
                    numberMenu.numberEnabled = 4;
                    _uiSettingDataModel.talkMenu.numberMenu = numberMenu;
                    Save();
                }
            };

            new CommonToggleSelector().SetRadioSelector(numberToggle, numberMenu.numberEnabled, numberActions);
            
            //number_integer_x
            //number_integer_y
            IntegerField numberIntegerX = RootContainer.Query<IntegerField>("number_integer_x");
            numberIntegerX.value = numberMenu.positionNumberWindow[0];
            numberIntegerX.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (numberIntegerX.value < MIN_POTISION)
                    numberIntegerX.value = MIN_POTISION;
                else if (numberIntegerX.value > MAX_POTISION_X)
                    numberIntegerX.value = MAX_POTISION_X;

                numberMenu.positionNumberWindow[0] = numberIntegerX.value;
                _uiSettingDataModel.talkMenu.numberMenu = numberMenu;
                Save();
            });

            IntegerField numberIntegerY = RootContainer.Query<IntegerField>("number_integer_y");
            numberIntegerY.value = numberMenu.positionNumberWindow[1];
            numberIntegerY.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (numberIntegerY.value < MIN_POTISION)
                    numberIntegerY.value = MIN_POTISION;
                else if (numberIntegerY.value > MAX_POTISION_Y)
                    numberIntegerY.value = MAX_POTISION_Y;

                numberMenu.positionNumberWindow[1] = numberIntegerY.value;
                _uiSettingDataModel.talkMenu.numberMenu = numberMenu;
                Save();
            });

            VisualElement fontSelectDropdown = RootContainer.Query<VisualElement>("font_select_dropDown");
            var commonDropdownPopupField =
                new PopupFieldBase<string>(_fontSelectList, FontNameToFontNumber(numberMenu.menuFontSetting.font));
            fontSelectDropdown.Add(commonDropdownPopupField);
            commonDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                FontManager.CreateFont(commonDropdownPopupField.index, FontManager.FONT_TYPE.MESSAGE_NUM);
                numberMenu.menuFontSetting.font = commonDropdownPopupField.value;
                _uiSettingDataModel.talkMenu.numberMenu = numberMenu;
                Save();
            });

            var font_size_sliderArea = RootContainer.Query<VisualElement>("font_size_sliderArea");
            SliderAndFiledBase.IntegerSliderCallBack(font_size_sliderArea, 10, 100, "",
                (int)numberMenu.menuFontSetting.size, evt =>
                {
                    numberMenu.menuFontSetting.size = (int) evt;
                    _uiSettingDataModel.talkMenu.numberMenu = numberMenu;
                    Save();
                });

            //font_rgb_text
            Label fontRgbText = RootContainer.Query<Label>("font_rgb_text");
            fontRgbText.text = string.Format("R : {0}, G : {1}, B: {2}",
                numberMenu.menuFontSetting.color[0],
                numberMenu.menuFontSetting.color[1], numberMenu.menuFontSetting.color[2]);
            //font_color
            ColorFieldBase fontColor = RootContainer.Query<ColorFieldBase>("font_color");
            fontColor.value = new Color(numberMenu.menuFontSetting.color[0] / 255f,
                numberMenu.menuFontSetting.color[1] / 255f, numberMenu.menuFontSetting.color[2] / 255f);
            fontColor.RegisterCallback<FocusOutEvent>(evt =>
            {
                var co = fontColor.value * 255;
                var r = co.r;
                var g = co.g;
                var b = co.b;
                var colors = new List<int>
                {
                    (int) Math.Ceiling(r),
                    (int) Math.Ceiling(g),
                    (int) Math.Ceiling(b)
                };
                numberMenu.menuFontSetting.color = colors.ToList();
                fontRgbText.text = string.Format("R : {0}, G : {1}, B: {2}",
                    numberMenu.menuFontSetting.color[0],
                    numberMenu.menuFontSetting.color[1], numberMenu.menuFontSetting.color[2]);
                _uiSettingDataModel.talkMenu.numberMenu = numberMenu;
                Save();
            });
        }


        private void CreateItemSelect() {
            MainUxml = itemSelectUxml;
            base.InitializeContents();

            var itemSelectMenu = _uiSettingDataModel.talkMenu.itemSelectMenu;

            var itemSelectToggle = new List<RadioButton>();
            for (var i = 23; i <= 25; i++)
            {
                RadioButton toggle = RootContainer.Query<RadioButton>("radioButton-initialization-display" + i);
                toggle.value = itemSelectMenu.positionItemWindow == i;
                itemSelectToggle.Add(toggle);
            }
            
            var itemSelectActions = new List<Action>
            {
                //上
                () =>
                {
                    itemSelectMenu.positionItemWindow = 0;
                    _uiSettingDataModel.talkMenu.itemSelectMenu = itemSelectMenu;
                    Save();
                },
                //中
                () =>
                {
                    itemSelectMenu.positionItemWindow = 1;
                    _uiSettingDataModel.talkMenu.itemSelectMenu = itemSelectMenu;
                    Save();
                },
                //下
                () =>
                {
                    itemSelectMenu.positionItemWindow = 2;
                    _uiSettingDataModel.talkMenu.itemSelectMenu = itemSelectMenu;
                    Save();
                }
            };

            new CommonToggleSelector().SetRadioSelector(itemSelectToggle, itemSelectMenu.positionItemWindow, itemSelectActions);
            
            VisualElement fontSelectDropdown = RootContainer.Query<VisualElement>("font_select_dropDown");
            var commonDropdownPopupField =
                new PopupFieldBase<string>(_fontSelectList, FontNameToFontNumber(itemSelectMenu.menuFontSetting.font));
            fontSelectDropdown.Add(commonDropdownPopupField);
            commonDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                FontManager.CreateFont(commonDropdownPopupField.index, FontManager.FONT_TYPE.MESSAGE_ITEM);
                itemSelectMenu.menuFontSetting.font = commonDropdownPopupField.value;
                _uiSettingDataModel.talkMenu.itemSelectMenu = itemSelectMenu;
                Save();
            });

            var fontSizeSliderArea = RootContainer.Query<VisualElement>("font_size_sliderArea");
            SliderAndFiledBase.IntegerSliderCallBack(fontSizeSliderArea, 10, 100, "",
                itemSelectMenu.menuFontSetting.size, evt =>
                {
                    itemSelectMenu.menuFontSetting.size = (int) evt;
                    _uiSettingDataModel.talkMenu.itemSelectMenu = itemSelectMenu;
                    Save();
                });

            //font_rgb_text
            Label fontRgbText = RootContainer.Query<Label>("font_rgb_text");
            fontRgbText.text = string.Format("R : {0}, G : {1}, B: {2}",
                itemSelectMenu.menuFontSetting.color[0],
                itemSelectMenu.menuFontSetting.color[1], itemSelectMenu.menuFontSetting.color[2]);
            //font_color
            ColorFieldBase fontColor = RootContainer.Query<ColorFieldBase>("font_color");
            fontColor.value = new Color(itemSelectMenu.menuFontSetting.color[0] / 255f,
                itemSelectMenu.menuFontSetting.color[1] / 255f, itemSelectMenu.menuFontSetting.color[2] / 255f);
            fontColor.RegisterCallback<FocusOutEvent>(evt =>
            {
                var co = fontColor.value * 255;
                var r = co.r;
                var g = co.g;
                var b = co.b;
                var colors = new List<int>
                {
                    (int) Math.Ceiling(r),
                    (int) Math.Ceiling(g),
                    (int) Math.Ceiling(b)
                };
                itemSelectMenu.menuFontSetting.color = colors.ToList();
                fontRgbText.text = string.Format("R : {0}, G : {1}, B: {2}",
                    itemSelectMenu.menuFontSetting.color[0],
                    itemSelectMenu.menuFontSetting.color[1], itemSelectMenu.menuFontSetting.color[2]);
                _uiSettingDataModel.talkMenu.itemSelectMenu = itemSelectMenu;
                Save();
            });
        }

        //セーブ
        override protected void SaveContents() {
            databaseManagementService.SaveUiSettingDataModel(_uiSettingDataModel);
            _sceneWindow.Render();
        }

        //フォント名を入れるとフォントリストの中でそれが何番目かを返すメソッド
        //該当フォントが無かった場合は0を返す
        private int FontNameToFontNumber(string fontName) {
            var fontNumber = _fontSelectList.IndexOf(fontName);

            return fontNumber > 0 ? fontNumber : 0;
        }
    }
}