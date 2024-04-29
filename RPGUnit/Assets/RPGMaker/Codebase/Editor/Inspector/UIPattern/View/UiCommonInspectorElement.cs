using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.UiSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using RPGMaker.Codebase.Editor.DatabaseEditor.View.Preview;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.UIPattern.View
{
    /// <summary>
    /// [初期設定]-[UI設定]-[共通設定] Inspector
    /// </summary>
    public class UiCommonInspectorElement : AbstractInspectorElement
    {
        private readonly SceneWindow               _sceneWindow;
        private          SystemSettingDataModel    _systemSettingDataModel;

        private VisualElement      _uiMainView;
        private UiSettingDataModel _uiSettingDataModel;

        private MenuPreview.UI_CHANGE_TYPE _uiSetting;

        private readonly string headUxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/UIPattern/Asset/inspector_UI_common.uxml";

        private readonly string mainUxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/UIPattern/Asset/inspector_UI_common_main.uxml";

        public UiCommonInspectorElement() {
            _sceneWindow =
                WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.DatabaseSceneWindow) as
                    SceneWindow;

            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            _uiSettingDataModel = databaseManagementService.LoadUiSettingDataModel();
            _systemSettingDataModel = databaseManagementService.LoadSystem();
            CreateScene();
            Initialize();
        }

        private void CreateScene(MenuPreview.UI_CHANGE_TYPE uiSetting = MenuPreview.UI_CHANGE_TYPE.NONE) {
            _sceneWindow?.Clear();
            _sceneWindow.Create(SceneWindow.PreviewId.Menu);
            _sceneWindow.GetMenuPreview().SetUiData(_uiSettingDataModel);
            _sceneWindow.GetMenuPreview().SetDisplayType(0);
            _sceneWindow.Init(uiSetting);
            _sceneWindow.SetRenderingSize(_sceneWindow.GetRenderingSize().x,
                _sceneWindow.GetRenderingSize().x * 9 / 16);
            _sceneWindow.Render();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            Clear();
            var items = new VisualElement();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(headUxml);
            VisualElement labelFromUxml = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUxml);
            labelFromUxml.style.flexGrow = 1;
            items.Add(labelFromUxml);
            Add(items);

            var commonData = _uiSettingDataModel.commonMenus.ToList();
            VisualElement commonDropdown = items.Query<VisualElement>("UI_pattern_dropdown");
            var commonList = new List<string> {EditorLocalize.LocalizeText("WORD_4014"), EditorLocalize.LocalizeText("WORD_4015"), EditorLocalize.LocalizeText("WORD_4016") };

            var selectNum = int.Parse(_systemSettingDataModel.uiPatternId) / 2;
            var commonDropdownPopupField = new PopupFieldBase<string>(commonList, selectNum);

            Toggle commonToggle = items.Query<Toggle>("reverse_toggle");
            commonToggle.value = int.Parse(_systemSettingDataModel.uiPatternId) % 2 == 0 ? false : true;
            commonToggle.RegisterValueChangedCallback(evt =>
            {
                if (commonToggle.value)
                    _systemSettingDataModel.uiPatternId = (commonDropdownPopupField.index * 2 + 1).ToString();
                else
                    _systemSettingDataModel.uiPatternId = (commonDropdownPopupField.index * 2).ToString();
                _uiSetting = MenuPreview.UI_CHANGE_TYPE.PATTERN;
                Save();
            });

            commonDropdown.Add(commonDropdownPopupField);
            commonDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                _systemSettingDataModel.uiPatternId = (
                    commonDropdownPopupField.index * 2 +
                    (commonToggle.value ? 1 : 0)
                ).ToString();
                _uiSetting = MenuPreview.UI_CHANGE_TYPE.PATTERN;
                Save();
            });


            _uiMainView = items.Query<VisualElement>("ui_main_view");
            SetMain(0);
        }

        private void SetMain(int index) {
            var commonData = _uiSettingDataModel.commonMenus[index];

            _uiMainView.Clear();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(mainUxml);
            VisualElement labelFromUXML = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUXML);
            _uiMainView.Add(labelFromUXML);
            RadioButton characterDisplay0 = _uiMainView.Query<RadioButton>("radioButton-initialization-display3");
            RadioButton characterDisplay1 = _uiMainView.Query<RadioButton>("radioButton-initialization-display4");
            RadioButton characterDisplay2 = _uiMainView.Query<RadioButton>("radioButton-initialization-display5");
            RadioButton characterDisplay3 = _uiMainView.Query<RadioButton>("radioButton-initialization-display6");

            var characterDisPlayActions =new List<Action>
            {
                //顔アイコン
                () =>
                {
                    commonData.characterType = (int) MenuIconTypeEnum.FACE;
                    _uiSetting = MenuPreview.UI_CHANGE_TYPE.CHARACTER;
                    Save();
                },
                //歩行キャラ
                () =>
                {
                    commonData.characterType = (int) MenuIconTypeEnum.SD;
                    _uiSetting = MenuPreview.UI_CHANGE_TYPE.CHARACTER;
                    Save();
                },
                //立ち絵
                () =>
                {
                    commonData.characterType = (int) MenuIconTypeEnum.PICTURE;
                    _uiSetting = MenuPreview.UI_CHANGE_TYPE.CHARACTER;
                    Save();
                },
                //なし
                () =>
                {
                    commonData.characterType = (int) MenuIconTypeEnum.NONE;
                    _uiSetting = MenuPreview.UI_CHANGE_TYPE.CHARACTER;
                    Save();
                }
            };
            
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton>() {characterDisplay0, characterDisplay1, characterDisplay2, characterDisplay3},
                commonData.characterType, characterDisPlayActions);

            //font_select_dropDown
            VisualElement fontSelectDropdown = _uiMainView.Query<VisualElement>("font_select_dropDown");
            var fontSelectList = FontManager.GetFontList();
            var titleFontIndex = fontSelectList.IndexOf(commonData.menuFontSetting.font) >= 0
                ? fontSelectList.IndexOf(commonData.menuFontSetting.font)
                : 0;
            var commonDropdownPopupField =
                new PopupFieldBase<string>(fontSelectList, titleFontIndex);
            fontSelectDropdown.Add(commonDropdownPopupField);
            commonDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                commonData.menuFontSetting.font = commonDropdownPopupField.value;
                FontManager.CreateFont(commonDropdownPopupField.index);
                _uiSetting = MenuPreview.UI_CHANGE_TYPE.FONT;
                Save();
            });


            var fontSizeFieldArea = _uiMainView.Query<VisualElement>("font_size_sliderArea");
            SliderAndFiledBase.IntegerSliderCallBack(fontSizeFieldArea, 10, 100, "",
                commonData.menuFontSetting.size, evt =>
                {
                    commonData.menuFontSetting.size = evt;
                    _uiSetting = MenuPreview.UI_CHANGE_TYPE.FONT_SIZE;
                    Save();
                },true);
            //font_rgb_text
            Label fontRgbText = _uiMainView.Query<Label>("font_rgb_text");
            fontRgbText.text = string.Format("R : {0}, G : {1}, B: {2}, ",
                                   commonData.menuFontSetting.color[0],
                                   commonData.menuFontSetting.color[1],
                                   commonData.menuFontSetting.color[2])
                               //透明度
                               + EditorLocalize.LocalizeText("WORD_0120") + " : " + commonData.menuFontSetting.color[3]
                ;
            //font_color
            ColorFieldBase fontColor = _uiMainView.Query<ColorFieldBase>("font_color");
            fontColor.value = new Color(
                commonData.menuFontSetting.color[0] / 255f,
                commonData.menuFontSetting.color[1] / 255f,
                commonData.menuFontSetting.color[2] / 255f,
                commonData.menuFontSetting.color[3] / 255f
            );
            fontColor.RegisterCallback<FocusOutEvent>(evt =>
            {
                var co = fontColor.value * 255;
                var r = (int) Math.Ceiling(co.r);
                var g = (int) Math.Ceiling(co.g);
                var b = (int) Math.Ceiling(co.b);
                var a = (int) Math.Ceiling(co.a);
                var colors = new List<int> {r, g, b, a};
                commonData.menuFontSetting.color = colors.ToList();
                fontRgbText.text = string.Format("R : {0}, G : {1}, B: {2}, ", r, g, b)
                                   //透明度
                                   + EditorLocalize.LocalizeText("WORD_0120") + " : " + a;
                FontManager.ChangeFontColor(FontManager.FONT_TYPE.NORMAL, fontColor.value);
                _uiSetting = MenuPreview.UI_CHANGE_TYPE.FONT_COLOR;
                Save();
            });

            //background_color_text
            Label backgroundColorText = _uiMainView.Query<Label>("background_color_text");
            backgroundColorText.text = string.Format("R : {0}, G : {1}, B: {2}, ",
                                           commonData.backgroundImage.color[0],
                                           commonData.backgroundImage.color[1],
                                           commonData.backgroundImage.color[2])
                                       //透明度
                                       + EditorLocalize.LocalizeText("WORD_0120") + " : " +
                                       commonData.backgroundImage.color[3]
                ;
            //background_color
            ColorFieldBase backgroundColor = _uiMainView.Query<ColorFieldBase>("background_color");
            backgroundColor.value = new Color(
                commonData.backgroundImage.color[0] / 255f,
                commonData.backgroundImage.color[1] / 255f,
                commonData.backgroundImage.color[2] / 255f,
                commonData.backgroundImage.color[3] / 255f
            );
            backgroundColor.RegisterCallback<FocusOutEvent>(evt =>
            {
                var co = backgroundColor.value * 255;
                var r = (int) Math.Ceiling(co.r);
                var g = (int) Math.Ceiling(co.g);
                var b = (int) Math.Ceiling(co.b);
                var a = (int) Math.Ceiling(co.a);
                var colors = new List<int> {r, g, b, a};
                commonData.backgroundImage.color = colors.ToList();
                backgroundColorText.text = string.Format("R : {0}, G : {1}, B: {2}, ", r, g, b)
                                           //透明度
                                           + EditorLocalize.LocalizeText("WORD_0120") + " : " + a
                    ;
                _uiSetting = MenuPreview.UI_CHANGE_TYPE.BG_COLOR;
                Save();
            });

            Toggle backgroundPictureDisplay = _uiMainView.Query<Toggle>("background_picture_display");
            backgroundPictureDisplay.value = commonData.backgroundImage.type == 1;
            BaseClickHandler.ClickEvent(backgroundPictureDisplay, evt =>
            {
                commonData.backgroundImage.type = backgroundPictureDisplay.value ? 1 : 0;
                _uiSetting = MenuPreview.UI_CHANGE_TYPE.BG_IMAGE;
                Save();
            });
            Button backgroundPicture = _uiMainView.Query<Button>("background_picture");
            backgroundPicture.text = commonData.backgroundImage.image;
            backgroundPicture.clickable.clicked += () =>
            {
                var uiSelectModalWindow = new ImageSelectModalWindow(PathManager.UI_BG);
                uiSelectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Background"), data =>
                {
                    var img = (string) data;
                    backgroundPicture.text = img;
                    commonData.backgroundImage.image = img;
                    _uiSetting = MenuPreview.UI_CHANGE_TYPE.BG_IMAGE;
                    Save();
                }, commonData.backgroundImage.image);
            };
            // インポートボタン
            Button backgroundImport = _uiMainView.Query<Button>("background_import");
            backgroundImport.clickable.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.UI_BG);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    backgroundPicture.text = path;
                    commonData.backgroundImage.image = path;
                    _uiSetting = MenuPreview.UI_CHANGE_TYPE.BG_IMAGE;
                    Save();
                }
            };

            //ウィンドウ背景の設定
            RadioButton windowBackgroundColorToggle = _uiMainView.Query<RadioButton>("radioButton-initialization-display7");
            RadioButton windowBackgroundPictureToggle = _uiMainView.Query<RadioButton>("radioButton-initialization-display8");
            Foldout windowBackgroundColorFoldout = _uiMainView.Query<Foldout>("window_background_color_foldout");
            Foldout windowBackgroundPictureFoldout = _uiMainView.Query<Foldout>("window_background_picture_foldout");

            int defaultSelect = commonData.windowBackgroundImage.type;
            new CommonToggleSelector().SetRadioInVisualElementSelector(
                new List<RadioButton> { windowBackgroundColorToggle, windowBackgroundPictureToggle },
                new List<VisualElement> { windowBackgroundColorFoldout, windowBackgroundPictureFoldout },
                defaultSelect, new List<Action>
                {
                    () =>
                    {
                        commonData.windowBackgroundImage.type = 0;
                        _uiSetting = MenuPreview.UI_CHANGE_TYPE.WINDOW_COLOR;
                        Save();
                    },
                    () =>
                    {
                        commonData.windowBackgroundImage.type = 1;
                        _uiSetting = MenuPreview.UI_CHANGE_TYPE.WINDOW_IMAGE;
                        Save();
                    }
                });

            //background_color_text
            Label windowBackgroundColorText = _uiMainView.Query<Label>("window_background_color_text");
            windowBackgroundColorText.text = string.Format(
                                                 "R : {0}, G : {1}, B : {2}, ",
                                                 commonData.windowBackgroundImage.color[0],
                                                 commonData.windowBackgroundImage.color[1],
                                                 commonData.windowBackgroundImage.color[2])
                                             //透明度
                                             + EditorLocalize.LocalizeText("WORD_0120") + " : " +
                                             commonData.windowBackgroundImage.color[3]
                ;
            //background_color
            ColorFieldBase windowBackgroundColor = _uiMainView.Query<ColorFieldBase>("window_background_color");
            windowBackgroundColor.value = new Color(
                commonData.windowBackgroundImage.color[0] / 255f,
                commonData.windowBackgroundImage.color[1] / 255f,
                commonData.windowBackgroundImage.color[2] / 255f,
                commonData.windowBackgroundImage.color[3] / 255f
            );
            windowBackgroundColor.RegisterCallback<FocusOutEvent>(evt =>
            {
                var co = windowBackgroundColor.value * 255;
                var r = (int) Math.Ceiling(co.r);
                var g = (int) Math.Ceiling(co.g);
                var b = (int) Math.Ceiling(co.b);
                var a = (int) Math.Ceiling(co.a);
                var colors = new List<int> {r, g, b, a};
                commonData.windowBackgroundImage.color = colors.ToList();
                windowBackgroundColorText.text = string.Format("R : {0}, G : {1}, B: {2}, ",
                                                     r,
                                                     g,
                                                     b)
                                                 //透明度
                                                 + EditorLocalize.LocalizeText("WORD_0120") + " : " + a
                    ;
                _uiSetting = MenuPreview.UI_CHANGE_TYPE.WINDOW_COLOR;
                Save();
            });

            Button windowPicture = _uiMainView.Query<Button>("window_picture");
            windowPicture.text = commonData.windowBackgroundImage.image;
            windowPicture.clickable.clicked += () =>
            {
                var uiSelectModalWindow = new ImageSelectModalWindow(PathManager.UI_WINDOW);
                uiSelectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Ui"), data =>
                {
                    var img = (string) data;
                    windowPicture.text = img;
                    commonData.windowBackgroundImage.image = img;
                    _uiSetting = MenuPreview.UI_CHANGE_TYPE.WINDOW_IMAGE;
                    Save();
                }, commonData.windowBackgroundImage.image);
            };

            // インポートボタン
            Button windowImport = _uiMainView.Query<Button>("window_import");
            windowImport.clickable.clicked += () =>
            {
                var path = AssetManageImporter.StartToFileBySprite("png", PathManager.UI_WINDOW);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    windowPicture.text = path;
                    commonData.windowBackgroundImage.image = path;
                    _uiSetting = MenuPreview.UI_CHANGE_TYPE.WINDOW_IMAGE;
                    Save();
                }
            };

            //ウィンドウフレームの設定
            RadioButton windowFramePictureColorToggle = _uiMainView.Query<RadioButton>("radioButton-initialization-display9");
            RadioButton windowFramePictureToggle = _uiMainView.Query<RadioButton>("radioButton-initialization-display10");
            VisualElement windowFramePictureArea = _uiMainView.Query<VisualElement>("window_frame_picture_area");
            VisualElement windowFrameColorPictureArea = _uiMainView.Query<VisualElement>("window_frame_color_picture_area");

            defaultSelect = commonData.windowFrameImage.type;
            new CommonToggleSelector().SetRadioInVisualElementSelector(
                new List<RadioButton> { windowFramePictureColorToggle, windowFramePictureToggle },
                new List<VisualElement> { windowFrameColorPictureArea, windowFramePictureArea },
                defaultSelect, new List<Action>
                {
                    () =>
                    {
                        commonData.windowFrameImage.type = 0;
                        _uiSetting = MenuPreview.UI_CHANGE_TYPE.FRAME_COLOR;
                        Save();
                    },
                    () =>
                    {
                        commonData.windowFrameImage.type = 1;
                        _uiSetting = MenuPreview.UI_CHANGE_TYPE.FRAME_IMAGE;
                        Save();
                    }
                });

            Button windowFramePicture = _uiMainView.Query<Button>("window_frame_picture");
            windowFramePicture.text = commonData.windowFrameImage.image;
            windowFramePicture.clickable.clicked += () =>
            {
                var uiSelectModalWindow = new ImageSelectModalWindow(PathManager.UI_WINDOW);
                uiSelectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Ui"), data =>
                {
                    var img = (string) data;
                    windowFramePicture.text = img;
                    commonData.windowFrameImage.image = img;
                    _uiSetting = MenuPreview.UI_CHANGE_TYPE.FRAME_IMAGE;
                    Save();
                }, commonData.windowFrameImage.image);
            };
            Button windowImportPicture = _uiMainView.Query<Button>("window_import_picture");
            windowImportPicture.clickable.clicked += () =>
            {
                var path = AssetManageImporter.StartToFileBySprite("png", PathManager.UI_WINDOW);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    windowFramePicture.text = path;
                    commonData.windowFrameImage.image = path;
                    _uiSetting = MenuPreview.UI_CHANGE_TYPE.FRAME_IMAGE;
                    Save();
                }
            };
            //background_color_text
            Label windowFrameColorText = _uiMainView.Query<Label>("window_frame_color_text");
            windowFrameColorText.text = string.Format("R : {0}, G : {1}, B: {2}, ",
                                            commonData.windowFrameImage.color[0],
                                            commonData.windowFrameImage.color[1],
                                            commonData.windowFrameImage.color[2])
                                        //透明度
                                        + EditorLocalize.LocalizeText("WORD_0120") + " : " +
                                        commonData.windowFrameImage.color[3]
                ;
            //background_color
            ColorFieldBase windowFrameColor = _uiMainView.Query<ColorFieldBase>("window_frame_color");
            windowFrameColor.value = new Color(
                commonData.windowFrameImage.color[0] / 255f,
                commonData.windowFrameImage.color[1] / 255f,
                commonData.windowFrameImage.color[2] / 255f,
                commonData.windowFrameImage.color[3] / 255f
            );
            windowFrameColor.RegisterCallback<FocusOutEvent>(evt =>
            {
                var co = windowFrameColor.value * 255;
                var r = (int) Math.Ceiling(co.r);
                var g = (int) Math.Ceiling(co.g);
                var b = (int) Math.Ceiling(co.b);
                var a = (int) Math.Ceiling(co.a);
                var colors = new List<int> {r, g, b, a};
                commonData.windowFrameImage.color = colors.ToList();
                windowFrameColorText.text = string.Format("R : {0}, G : {1}, B: {2}, ", r, g, b)
                                            //透明度
                                            + EditorLocalize.LocalizeText("WORD_0120") + " : " + a
                    ;
                _uiSetting = MenuPreview.UI_CHANGE_TYPE.FRAME_COLOR;
                Save();
            });

            //ウィンドウフレームのハイライト設定
            Label windowFrameHighlightText = _uiMainView.Query<Label>("window_frame_highlight_text");
            windowFrameHighlightText.text = string.Format("R : {0}, G : {1}, B: {2}, ",
                                                commonData.windowFrameImageHighlight[0],
                                                commonData.windowFrameImageHighlight[1],
                                                commonData.windowFrameImageHighlight[2])
                                            //透明度
                                            + EditorLocalize.LocalizeText("WORD_0120") + " : " +
                                            commonData.windowFrameImageHighlight[3]
                ;
            ColorFieldBase windowFrameHighlight = _uiMainView.Query<ColorFieldBase>("window_frame_highlight");
            windowFrameHighlight.value = new Color(
                commonData.windowFrameImageHighlight[0] / 255f,
                commonData.windowFrameImageHighlight[1] / 255f,
                commonData.windowFrameImageHighlight[2] / 255f,
                commonData.windowFrameImageHighlight[3] / 255f
            );
            windowFrameHighlight.RegisterCallback<FocusOutEvent>(evt =>
            {
                var co = windowFrameHighlight.value * 255;
                var r = (int) Math.Ceiling(co.r);
                var g = (int) Math.Ceiling(co.g);
                var b = (int) Math.Ceiling(co.b);
                var a = (int) Math.Ceiling(co.a);
                var colors = new List<int> {r, g, b, a};
                commonData.windowFrameImageHighlight = colors.ToList();
                windowFrameHighlightText.text = string.Format("R : {0}, G : {1}, B: {2}, ", r, g, b)
                                                //透明度
                                                + EditorLocalize.LocalizeText("WORD_0120") + " : " + a
                    ;
                windowFrameHighlight.style.backgroundColor = new StyleColor(
                    new Color(
                        commonData.windowFrameImageHighlight[0] / 255f,
                        commonData.windowFrameImageHighlight[1] / 255f,
                        commonData.windowFrameImageHighlight[2] / 255f,
                        commonData.windowFrameImageHighlight[3] / 255f
                    )
                );
                _uiSetting = MenuPreview.UI_CHANGE_TYPE.FRAME_HIGHLIGHT;
                Save();
            });

            //ボタン背景の設定
            RadioButton buttonBackgroundColorToggle = _uiMainView.Query<RadioButton>("radioButton-initialization-display11");
            RadioButton buttonBackgroundPictureToggle = _uiMainView.Query<RadioButton>("radioButton-initialization-display12");
            Foldout buttonBackgroundColorFoldout = _uiMainView.Query<Foldout>("button_background_color_foldout");
            Foldout buttonBackgroundPictureFoldout = _uiMainView.Query<Foldout>("button_background_picture_foldout");

            defaultSelect = commonData.buttonImage.type;
            new CommonToggleSelector().SetRadioInVisualElementSelector(
                new List<RadioButton> { buttonBackgroundColorToggle, buttonBackgroundPictureToggle },
                new List<VisualElement> { buttonBackgroundColorFoldout, buttonBackgroundPictureFoldout },
                defaultSelect, new List<Action>
                {
                    () =>
                    {
                        commonData.buttonImage.type = 0;
                        _uiSetting = MenuPreview.UI_CHANGE_TYPE.BUTTON_COLOR;
                        Save();
                    },
                    () =>
                    {
                        commonData.buttonImage.type = 1;
                        _uiSetting = MenuPreview.UI_CHANGE_TYPE.BUTTON_IMAGE;
                        Save();
                    }
                });

            //background_color_text
            Label buttonBackgroundColorText = _uiMainView.Query<Label>("button_background_color_text");
            buttonBackgroundColorText.text = string.Format("R : {0}, G : {1}, B: {2}, ",
                                                 commonData.buttonImage.color[0],
                                                 commonData.buttonImage.color[1],
                                                 commonData.buttonImage.color[2])
                                             //透明度
                                             + EditorLocalize.LocalizeText("WORD_0120") + " : " +
                                             commonData.buttonImage.color[3]
                ;
            //background_color
            ColorFieldBase buttonBackgroundColor = _uiMainView.Query<ColorFieldBase>("button_background_color");
            buttonBackgroundColor.value = new Color(
                commonData.buttonImage.color[0] / 255f,
                commonData.buttonImage.color[1] / 255f,
                commonData.buttonImage.color[2] / 255f,
                commonData.buttonImage.color[3] / 255f
            );
            buttonBackgroundColor.RegisterCallback<FocusOutEvent>(evt =>
            {
                var co = buttonBackgroundColor.value * 255;
                var r = (int) Math.Ceiling(co.r);
                var g = (int) Math.Ceiling(co.g);
                var b = (int) Math.Ceiling(co.b);
                var a = (int) Math.Ceiling(co.a);
                var colors = new List<int> {r, g, b, a};
                commonData.buttonImage.color = colors.ToList();
                buttonBackgroundColorText.text = string.Format("R : {0}, G : {1}, B: {2}, ", r, g, b)
                                                 //透明度
                                                 + EditorLocalize.LocalizeText("WORD_0120") + " : " + a
                    ;
                buttonBackgroundColor.style.backgroundColor = new StyleColor(
                    new Color(
                        commonData.buttonImage.color[0] / 255f,
                        commonData.buttonImage.color[1] / 255f,
                        commonData.buttonImage.color[2] / 255f,
                        commonData.buttonImage.color[3] / 255f
                    )
                );
                _uiSetting = MenuPreview.UI_CHANGE_TYPE.BUTTON_COLOR;
                Save();
            });

            //button_picture
            Button buttonPicture = _uiMainView.Query<Button>("button_picture");
            buttonPicture.text = commonData.buttonImage.image;
            buttonPicture.clickable.clicked += () =>
            {
                var uiSelectModalWindow = new ImageSelectModalWindow(PathManager.UI_BUTTON);
                uiSelectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Ui"), data =>
                {
                    var img = (string) data;
                    buttonPicture.text = img;
                    commonData.buttonImage.image = img;
                    _uiSetting = MenuPreview.UI_CHANGE_TYPE.BUTTON_IMAGE;
                    Save();
                }, commonData.buttonImage.image);
            };

            // インポートボタン
            Button buttonImport = _uiMainView.Query<Button>("button_import");
            buttonImport.clickable.clicked += () =>
            {
                var path = AssetManageImporter.StartToFileBySprite("png", PathManager.UI_BUTTON);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    buttonPicture.text = path;
                    commonData.buttonImage.image = path;
                    _uiSetting = MenuPreview.UI_CHANGE_TYPE.BUTTON_IMAGE;
                    Save();
                }
            };

            //ボタンハイライトの設定

            Label buttonHighlightColorText = _uiMainView.Query<Label>("button_highlight_color_text");
            buttonHighlightColorText.text = string.Format("R : {0}, G : {1}, B: {2}, ",
                                                commonData.buttonImageHighlight[0],
                                                commonData.buttonImageHighlight[1],
                                                commonData.buttonImageHighlight[2])
                                            //透明度
                                            + EditorLocalize.LocalizeText("WORD_0120") + " : " +
                                            commonData.buttonImageHighlight[3]
                ;
            ColorFieldBase buttonhighlightColor = _uiMainView.Query<ColorFieldBase>("button_highlight_color");
            buttonhighlightColor.value = new Color(
                commonData.buttonImageHighlight[0] / 255f,
                commonData.buttonImageHighlight[1] / 255f,
                commonData.buttonImageHighlight[2] / 255f,
                commonData.buttonImageHighlight[3] / 255f
            );
            buttonhighlightColor.RegisterCallback<FocusOutEvent>(evt =>
            {
                var co = buttonhighlightColor.value * 255;
                var r = (int) Math.Ceiling(co.r);
                var g = (int) Math.Ceiling(co.g);
                var b = (int) Math.Ceiling(co.b);
                var a = (int) Math.Ceiling(co.a);
                var colors = new List<int> {r, g, b, a};
                commonData.buttonImageHighlight = colors.ToList();
                buttonHighlightColorText.text = string.Format("R : {0}, G : {1}, B: {2}, ", r, g, b)
                                                //透明度
                                                + EditorLocalize.LocalizeText("WORD_0120") + " : " + a
                    ;
                _uiSetting = MenuPreview.UI_CHANGE_TYPE.BUTTON_HIGHLIGHT;
                Save();
            });

            ///ボタンフレームの設定
            
            RadioButton buttonFrameColorToggle = _uiMainView.Query<RadioButton>("radioButton-initialization-display13");
            RadioButton buttonFramePictureToggle = _uiMainView.Query<RadioButton>("radioButton-initialization-display14");
            VisualElement buttonFrameColorFoldout = _uiMainView.Query<VisualElement>("button_frame_color_foldout");
            VisualElement buttonFramePictureFoldout = _uiMainView.Query<VisualElement>("button_frame_picture_foldout");

            defaultSelect = commonData.buttonFrameImage.type;
            new CommonToggleSelector().SetRadioInVisualElementSelector(
                new List<RadioButton> { buttonFrameColorToggle, buttonFramePictureToggle },
                new List<VisualElement> { buttonFrameColorFoldout, buttonFramePictureFoldout },
                defaultSelect, new List<Action>
                {
                    () =>
                    {
                        commonData.buttonFrameImage.type = 0;
                        _uiSetting = MenuPreview.UI_CHANGE_TYPE.BUTTON_FRAME_COLOR;
                        Save();
                    },
                    () =>
                    {
                        commonData.buttonFrameImage.type = 1;
                        _uiSetting = MenuPreview.UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE;
                        Save();
                    }
                });

            Button buttonFramePicture = _uiMainView.Query<Button>("button_frame_picture");
            buttonFramePicture.text = commonData.buttonFrameImage.image;
            buttonFramePicture.clickable.clicked += () =>
            {
                var uiSelectModalWindow = new ImageSelectModalWindow(PathManager.UI_BUTTON);
                uiSelectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Ui"), data =>
                {
                    var img = (string) data;
                    buttonFramePicture.text = img;
                    commonData.buttonFrameImage.image = img;
                    _uiSetting = MenuPreview.UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE;
                    Save();
                }, commonData.buttonFrameImage.image);
            };
            
            //button_frame_import
            Button buttonFrameImport = _uiMainView.Query<Button>("button_frame_import");
            buttonFrameImport.clickable.clicked += () =>
            {
                var path = AssetManageImporter.StartToFileBySprite("png", PathManager.UI_BUTTON);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    buttonFramePicture.text = path;
                    commonData.buttonFrameImage.image = path;
                    _uiSetting = MenuPreview.UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE;
                    Save();
                }
            };

            Label buttonFrameColorText = _uiMainView.Query<Label>("button_frame_color_text");
            buttonFrameColorText.text = string.Format("R : {0}, G : {1}, B: {2}, ",
                                            commonData.buttonFrameImage.color[0],
                                            commonData.buttonFrameImage.color[1],
                                            commonData.buttonFrameImage.color[2])
                                        //透明度
                                        + EditorLocalize.LocalizeText("WORD_0120") + " : " +
                                        commonData.buttonFrameImage.color[3];
            ColorFieldBase buttonFrameColor = _uiMainView.Query<ColorFieldBase>("button_frame_color");
            buttonFrameColor.value = new Color(
                commonData.buttonFrameImage.color[0] / 255f,
                commonData.buttonFrameImage.color[1] / 255f,
                commonData.buttonFrameImage.color[2] / 255f,
                commonData.buttonFrameImage.color[3] / 255f
            );
            buttonFrameColor.RegisterCallback<FocusOutEvent>(evt =>
            {
                var co = buttonFrameColor.value * 255;
                var r = (int) Math.Ceiling(co.r);
                var g = (int) Math.Ceiling(co.g);
                var b = (int) Math.Ceiling(co.b);
                var a = (int) Math.Ceiling(co.a);
                var colors = new List<int> {r, g, b, a};
                commonData.buttonFrameImage.color = colors.ToList();
                buttonFrameColorText.text = string.Format("R : {0}, G : {1}, B: {2}", r, g, b)
                                            //透明度
                                            + EditorLocalize.LocalizeText("WORD_0120") + " : " + a
                    ;
                buttonFrameColor.style.backgroundColor = new StyleColor(new Color(
                    commonData.buttonFrameImage.color[0] / 255f,
                    commonData.buttonFrameImage.color[1] / 255f,
                    commonData.buttonFrameImage.color[2] / 255f,
                    commonData.buttonFrameImage.color[3] / 255f
                ));
                _uiSetting = MenuPreview.UI_CHANGE_TYPE.BUTTON_FRAME_COLOR;
                Save();
            });
        }

        override protected void SaveContents() {
            databaseManagementService.SaveUiSettingDataModel(_uiSettingDataModel);
            databaseManagementService.SaveSystem(_systemSettingDataModel);
            CreateScene(_uiSetting);
            _uiSetting = MenuPreview.UI_CHANGE_TYPE.NONE;
        }
    }
}