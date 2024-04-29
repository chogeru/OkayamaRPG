using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.Title.View
{
    /// <summary>
    /// [初期設定]-[タイトル] Inspector
    /// </summary>
    public class TitleInspectorElement : AbstractInspectorElement
    {
        private const int MIN_POTISION   = 0;
        private const int MAX_POTISION_X = 1920;
        private const int MAX_POTISION_Y = 1080;

        private          bool                  _isInit;
        private          RuntimeTitleDataModel _runtimeTitleDataModel;
        private readonly SceneWindow           _sceneView;

        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Title/Asset/inspector_title.uxml"; } }

        public TitleInspectorElement(bool notSwitchDisplaySceneWindow = false) {
            _isInit = true;
            _sceneView =
                (notSwitchDisplaySceneWindow
                    ? WindowLayoutManager.GetActiveWindow(WindowLayoutManager.WindowLayoutId.DatabaseSceneWindow)
                    : WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.DatabaseSceneWindow)) as
                SceneWindow;

            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            _runtimeTitleDataModel = databaseManagementService.LoadTitle();

            _sceneView.Create(SceneWindow.PreviewId.Title);
            _sceneView.GetTitlePreview().SetTitleData(_runtimeTitleDataModel);
            _sceneView.Init();

            Initialize();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            ImTextField titleTextField = RootContainer.Query<ImTextField>("title_name");
            titleTextField.value = _runtimeTitleDataModel.gameTitle;
            titleTextField.RegisterCallback<FocusOutEvent>(evt =>
            {
                _runtimeTitleDataModel.gameTitle =
                    titleTextField.value;
                _UpdateSceneView();
                OutlineEditor.OutlineEditor.UpdateStartView();
            });

            //▼タイトル名の設定 - タイトル名
            Label titleBgText = RootContainer.Query<Label>("title_bg_text");
            titleBgText.text = _runtimeTitleDataModel.titleBackgroundImage + ".png";

            IntegerField titleTitlenameX = RootContainer.Query<IntegerField>("title_titlename_x");
            titleTitlenameX.value = _runtimeTitleDataModel.gameTitleCommon.position[0];
            titleTitlenameX.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (titleTitlenameX.value < MIN_POTISION)
                    titleTitlenameX.value = MIN_POTISION;
                else if (titleTitlenameX.value > MAX_POTISION_X)
                    titleTitlenameX.value = MAX_POTISION_X;

                _runtimeTitleDataModel.gameTitleCommon.position[0] = titleTitlenameX.value;
                _UpdateSceneView();
            });
            IntegerField titleTitlenameY = RootContainer.Query<IntegerField>("title_titlename_y");
            titleTitlenameY.value = _runtimeTitleDataModel.gameTitleCommon.position[1];
            titleTitlenameY.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (titleTitlenameY.value < MIN_POTISION)
                    titleTitlenameY.value = MIN_POTISION;
                else if (titleTitlenameY.value > MAX_POTISION_Y)
                    titleTitlenameY.value = MAX_POTISION_Y;

                _runtimeTitleDataModel.gameTitleCommon.position[1] = titleTitlenameY.value;
                _UpdateSceneView();
            });

            //▼背景画像の選択 - 画像
            Button titleBgImage = RootContainer.Query<Button>("title_bg_image");
            titleBgImage.clicked += () =>
            {
                var backgroundSelectModalWindow = new ImageSelectModalWindow(PathManager.UI_TITLE_BG);
                backgroundSelectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("WORD_0101"), data =>
                {
                    var imageName = (string) data;
                    titleBgText.text = imageName + ".png";
                    _runtimeTitleDataModel.titleBackgroundImage = imageName;
                    _UpdateSceneView();
                    OutlineEditor.OutlineEditor.UpdateStartView();
                }, _runtimeTitleDataModel.titleBackgroundImage);
            };
            // 背景画像インポート
            Button titleBgImport = RootContainer.Query<Button>("title_bg_import");
            titleBgImport.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.UI_TITLE_BG);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    titleBgText.text = path;
                    _runtimeTitleDataModel.titleBackgroundImage = path;
                    _UpdateSceneView();
                    OutlineEditor.OutlineEditor.UpdateStartView();
                }
            };

            //▼装飾画像の選択
            Label titleFrText = RootContainer.Query<Label>("title_fr_text");
            titleFrText.text = _runtimeTitleDataModel.titleFront.image + ".png";
            Button titleFrImage = RootContainer.Query<Button>("title_fr_image");
            titleFrImage.clicked += () =>
            {
                var frontSelectModalWindow = new ImageSelectModalWindow(PathManager.UI_TITLE_FRAME);
                frontSelectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("WORD_0102"), data =>
                {
                    var imageName = (string) data;
                    titleFrText.text = imageName + ".png";
                    _runtimeTitleDataModel.titleFront.image = imageName;
                    _UpdateSceneView();
                    OutlineEditor.OutlineEditor.UpdateStartView();
                }, _runtimeTitleDataModel.titleFront.image);
            };
            // 装飾画像インポート
            Button titleFrImport = RootContainer.Query<Button>("title_fr_import");
            titleFrImport.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.UI_TITLE_FRAME);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    titleFrText.text = path;
                    _runtimeTitleDataModel.titleFront.image = path;
                    _UpdateSceneView();
                    OutlineEditor.OutlineEditor.UpdateStartView();
                    //インポート直後では100％にする
                    _runtimeTitleDataModel.titleFront.scale = 100;
                    RefreshScroll();
                }
            };
            //装飾画像の調整
            var titleDecorationSliderArea = RootContainer.Query<VisualElement>("title_decoration_sliderArea");
            SliderAndFiledBase.IntegerSliderCallBack(titleDecorationSliderArea, 10, 200, "%",
                (int)_runtimeTitleDataModel.titleFront.scale, evt =>
                {
                    _runtimeTitleDataModel.titleFront.scale = (int) evt;
                    _UpdateSceneView();
                    OutlineEditor.OutlineEditor.UpdateStartView();
                });

            //X座標
            IntegerField titleDecorationX = RootContainer.Query<IntegerField>("title_decoration_x");
            titleDecorationX.value = _runtimeTitleDataModel.titleFront.position[0];
            titleDecorationX.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (titleDecorationX.value < MIN_POTISION)
                    titleDecorationX.value = MIN_POTISION;
                else if (titleDecorationX.value > MAX_POTISION_X)
                    titleDecorationX.value = MAX_POTISION_X;

                _runtimeTitleDataModel.titleFront.position[0] = titleDecorationX.value;
                _UpdateSceneView();
                OutlineEditor.OutlineEditor.UpdateStartView();
            });
            //Y座標
            IntegerField titleDecorationY = RootContainer.Query<IntegerField>("title_decoration_y");
            titleDecorationY.value = _runtimeTitleDataModel.titleFront.position[1];
            titleDecorationY.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (titleDecorationY.value < MIN_POTISION)
                    titleDecorationY.value = MIN_POTISION;
                else if (titleDecorationY.value > MAX_POTISION_Y)
                    titleDecorationY.value = MAX_POTISION_Y;

                _runtimeTitleDataModel.titleFront.position[1] = titleDecorationY.value;
                _UpdateSceneView();
                OutlineEditor.OutlineEditor.UpdateStartView();
            });

            //▼タイトル名の描画
            RadioButton titleTitlenameToggle1 = RootContainer.Query<RadioButton>("radioButton-initialization-display1");
            RadioButton titleTitlenameToggle2 = RootContainer.Query<RadioButton>("radioButton-initialization-display2");
            Foldout titleTitlenameFoldout1 = RootContainer.Query<Foldout>("title_titlename_foldout1");
            Foldout titleTitlenameFoldout2 = RootContainer.Query<Foldout>("title_titlename_foldout2");

            int defaultSelect = _runtimeTitleDataModel.gameTitleCommon.gameTitleType - 1;
            new CommonToggleSelector().SetRadioInVisualElementSelector(
                new List<RadioButton> { titleTitlenameToggle1, titleTitlenameToggle2 },
                new List<VisualElement> { titleTitlenameFoldout1, titleTitlenameFoldout2 },
                defaultSelect, new List<Action>
                {
                    () =>
                    {
                        _runtimeTitleDataModel.gameTitleCommon.gameTitleType =
                            titleTitlenameToggle1.value ? 1 : 2;
                        _UpdateSceneView();
                    },
                    () =>
                    {
                        _runtimeTitleDataModel.gameTitleCommon.gameTitleType =
                            titleTitlenameToggle2.value ? 2 : 1;
                        _UpdateSceneView();
                    }
                });

            //▼タイトル名の描画 - ▼テキスト
            VisualElement titleTitlenameTextDropdown = RootContainer.Query<VisualElement>("title_titlename_text_dropdown");
            var titleTitlenameTextDropdownChoices = FontManager.GetFontList();
            var titleFontIndex =
                titleTitlenameTextDropdownChoices.IndexOf(_runtimeTitleDataModel.gameTitleText.font) >= 0
                    ? titleTitlenameTextDropdownChoices.IndexOf(_runtimeTitleDataModel.gameTitleText.font)
                    : 0;
            var titleTitlenameTextDropdownPopupField =
                new PopupFieldBase<string>(titleTitlenameTextDropdownChoices, titleFontIndex);
            titleTitlenameTextDropdown.Add(titleTitlenameTextDropdownPopupField);
            titleTitlenameTextDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                _runtimeTitleDataModel.gameTitleText.font =
                    titleTitlenameTextDropdownPopupField.value;
                FontManager.CreateFont(titleTitlenameTextDropdownPopupField.index, FontManager.FONT_TYPE.TITLE);
                _UpdateSceneView();
            });

            var titleTitleNameTextSliderArea = RootContainer.Query<VisualElement>("title_titleName_text_sliderArea");
            SliderAndFiledBase.IntegerSliderCallBack(titleTitleNameTextSliderArea, 10, 1000, "",
                _runtimeTitleDataModel.gameTitleText.size, evt =>
                {
                    _runtimeTitleDataModel.gameTitleText.size = (int) evt;
                    _UpdateSceneView();
                });
            
            ColorFieldBase titleTitlenameTextFont = RootContainer.Query<ColorFieldBase>("title_titlename_text_color");
            
            titleTitlenameTextFont.RegisterCallback<FocusOutEvent>(evt =>
            {
                var co = titleTitlenameTextFont.value * 255;
                var r = co.r;
                var g = co.g;
                var b = co.b;
                var colors = new List<int>
                {
                    (int) Math.Ceiling(r),
                    (int) Math.Ceiling(g),
                    (int) Math.Ceiling(b)
                };
                _runtimeTitleDataModel.gameTitleText.color = colors.ToArray();
                _UpdateSceneView();
            });
            titleTitlenameTextFont.value = new Color(
                _runtimeTitleDataModel.gameTitleText.color[0] / 255f,
                _runtimeTitleDataModel.gameTitleText.color[1] / 255f,
                _runtimeTitleDataModel.gameTitleText.color[2] / 255f,
                1.0f
            );

            //▼タイトル名の描画 - ▼画像
            var titleTitlenameImageSliderArea = RootContainer.Query<VisualElement>("title_titlename_image_sliderArea");

            //10以下だった場合最低を10にする
            if (_runtimeTitleDataModel.gameTitleImage.scale < 11)
            {
                _runtimeTitleDataModel.gameTitleImage.scale = 10;
            }
            
            SliderAndFiledBase.IntegerSliderCallBack(titleTitlenameImageSliderArea, 10, 200, "%",
                (int)_runtimeTitleDataModel.gameTitleImage.scale, evt =>
                {
                    _runtimeTitleDataModel.gameTitleImage.scale = (int) evt;
                    _UpdateSceneView();
                });

            //タイトルネームの画像設定PU部分
            Label titleTitlenameText = RootContainer.Query<Label>("title_titlename_text");
            titleTitlenameText.text = _runtimeTitleDataModel.gameTitleImage.image + ".png";
            Button titleTitlenameTextImage = RootContainer.Query<Button>("title_titlename_text_image");
            titleTitlenameTextImage.clicked += () =>
            {
                var backgroundSelectModalWindow = new ImageSelectModalWindow(PathManager.UI_TITLE_NAME);
                backgroundSelectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("WORD_0099"), data =>
                {
                    var imageName = (string) data;
                    _runtimeTitleDataModel.gameTitleImage.image = imageName;
                    titleTitlenameText.text = _runtimeTitleDataModel.gameTitleImage.image + ".png";
                    _UpdateSceneView();
                }, _runtimeTitleDataModel.gameTitleImage.image);
            };
            
            Button titleTitleNameImport = RootContainer.Query<Button>("title_titlename_import");
            titleTitleNameImport.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.UI_TITLE_NAME);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    _runtimeTitleDataModel.gameTitleImage.image = path;
                    titleTitlenameText.text = _runtimeTitleDataModel.gameTitleImage.image + ".png";
                    //インポート直後では100％にする
                    _runtimeTitleDataModel.gameTitleImage.scale = 100;
                    _UpdateSceneView();
                    RefreshScroll();
                }
            };

            //▼スタートメニューの設定
            Toggle titleStartmenuNewgameToggle = RootContainer.Query<Toggle>("title_startmenu_newgame_toggle");
            new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                titleStartmenuNewgameToggle,
                RootContainer.Query<VisualElement>("title_startmenu_newgame_toggle_contents"),
                _runtimeTitleDataModel.startMenu.menuNewGame.enabled,
                () =>
                {
                    _runtimeTitleDataModel.startMenu.menuNewGame.enabled =
                        titleStartmenuNewgameToggle.value;
                    _UpdateSceneView();
                }
            );

            ImTextField titleStartmenuNewgameText = RootContainer.Query<ImTextField>("title_startmenu_newgame_text");

            titleStartmenuNewgameText.value = _runtimeTitleDataModel.startMenu.menuNewGame.value;
            titleStartmenuNewgameText.RegisterCallback<FocusOutEvent>(evt =>
            {
                _runtimeTitleDataModel.startMenu.menuNewGame.value =
                    titleStartmenuNewgameText.value;
                _UpdateSceneView();
            });

            Toggle titleStartmenuContinueToggle = RootContainer.Query<Toggle>("title_startmenu_continue_toggle");
            new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                titleStartmenuContinueToggle,
                RootContainer.Query<VisualElement>("title_startmenu_continue_toggle_contents"),
                _runtimeTitleDataModel.startMenu.menuContinue.enabled,
                () =>
                {
                    _runtimeTitleDataModel.startMenu.menuContinue.enabled =
                        titleStartmenuContinueToggle.value;
                    _UpdateSceneView();
                }
            );

            ImTextField titleStartmenuContinueText = RootContainer.Query<ImTextField>("title_startmenu_continue_text");
            titleStartmenuContinueText.value =
                _runtimeTitleDataModel.startMenu.menuContinue.value;
            titleStartmenuContinueText.RegisterCallback<FocusOutEvent>(evt =>
            {
                _runtimeTitleDataModel.startMenu.menuContinue.value =
                    titleStartmenuContinueText.value;
                _UpdateSceneView();
            });

            Toggle titleStartmenuGamesettingToggle = RootContainer.Query<Toggle>("title_startmenu_gamesetting_toggle");
            new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                titleStartmenuGamesettingToggle,
                RootContainer.Query<VisualElement>("title_startmenu_gamesetting_toggle_contents"),
                _runtimeTitleDataModel.startMenu.menuOption.enabled,
                () =>
                {
                    _runtimeTitleDataModel.startMenu.menuOption.enabled =
                        titleStartmenuGamesettingToggle.value;
                    _UpdateSceneView();
                }
            );

            ImTextField titleStartmenuGamesettingText = RootContainer.Query<ImTextField>("title_startmenu_gamesetting_text");
            titleStartmenuGamesettingText.value =
                _runtimeTitleDataModel.startMenu.menuOption.value;
            titleStartmenuGamesettingText.RegisterCallback<FocusOutEvent>(evt =>
            {
                _runtimeTitleDataModel.startMenu.menuOption.value =
                    titleStartmenuGamesettingText.value;
                _UpdateSceneView();
            });

            //フォント設定
            VisualElement titleFontsettingDropdown = RootContainer.Query<VisualElement>("title_fontsetting_dropdown");
            var titleFontsettingDropdownChoices = FontManager.GetFontList();
            var menuFontIndex =
                titleFontsettingDropdownChoices.IndexOf(_runtimeTitleDataModel.startMenu.menuFontSetting.font) >= 0
                    ? titleFontsettingDropdownChoices.IndexOf(_runtimeTitleDataModel.startMenu.menuFontSetting.font)
                    : 0;
            var titleFontsettingDropdownPopupField =
                new PopupFieldBase<string>(titleFontsettingDropdownChoices, menuFontIndex);
            titleFontsettingDropdown.Add(titleFontsettingDropdownPopupField);
            titleFontsettingDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                _runtimeTitleDataModel.startMenu.menuFontSetting.font =
                    titleFontsettingDropdownPopupField.value;
                FontManager.CreateFont(titleFontsettingDropdownPopupField.index, FontManager.FONT_TYPE.MENU);
                _UpdateSceneView();
            });

            var title_fontsetting_sliderArea = RootContainer.Query<VisualElement>("title_fontsetting_sliderArea");
            SliderAndFiledBase.IntegerSliderCallBack(title_fontsetting_sliderArea, 10, 100, "",
                (int)_runtimeTitleDataModel.startMenu.menuFontSetting.size, evt =>
                {
                    _runtimeTitleDataModel.startMenu.menuFontSetting.size = (int) evt;
                    _UpdateSceneView();
                });

            //スタートメニューのフォントカラー
            ColorFieldBase titleFontsettingColor = RootContainer.Query<ColorFieldBase>("title_fontsetting_color");
            titleFontsettingColor.RegisterCallback<FocusOutEvent>(evt =>
            {
                var co = titleFontsettingColor.value * 255;
                var r = co.r;
                var g = co.g;
                var b = co.b;
                var colors = new List<int>
                {
                    (int) Math.Ceiling(r),
                    (int) Math.Ceiling(g),
                    (int) Math.Ceiling(b)
                };
                _runtimeTitleDataModel.startMenu.menuFontSetting.color = colors;
                _UpdateSceneView();
            });
            titleFontsettingColor.value = new Color(
                _runtimeTitleDataModel.startMenu.menuFontSetting.color[0] / 255f,
                _runtimeTitleDataModel.startMenu.menuFontSetting.color[1] / 255f,
                _runtimeTitleDataModel.startMenu.menuFontSetting.color[2] / 255f,
                1.0f
            );

            IntegerField titleUisettingTextX = RootContainer.Query<IntegerField>("title_uisetting_textx");
            titleUisettingTextX.value =
                _runtimeTitleDataModel.startMenu.menuUiSetting.position[0];
            titleUisettingTextX.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (titleUisettingTextX.value < MIN_POTISION)
                    titleUisettingTextX.value = MIN_POTISION;
                else if (titleUisettingTextX.value > MAX_POTISION_X)
                    titleUisettingTextX.value = MAX_POTISION_X;

                _runtimeTitleDataModel.startMenu.menuUiSetting.position[0] =
                    titleUisettingTextX.value;
                _UpdateSceneView();
            });
            IntegerField titleUisettingTextY = RootContainer.Query<IntegerField>("title_uisetting_texty");
            titleUisettingTextY.value =
                _runtimeTitleDataModel.startMenu.menuUiSetting.position[1];
            titleUisettingTextY.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (titleUisettingTextY.value < MIN_POTISION)
                    titleUisettingTextY.value = MIN_POTISION;
                else if (titleUisettingTextY.value > MAX_POTISION_Y)
                    titleUisettingTextY.value = MAX_POTISION_Y;

                _runtimeTitleDataModel.startMenu.menuUiSetting.position[1] =
                    titleUisettingTextY.value;
                _UpdateSceneView();
            });

            //▼メモ
            ImTextField note = RootContainer.Query<ImTextField>("note");
            note.value = _runtimeTitleDataModel.note;

            note.RegisterCallback<FocusOutEvent>(evt =>
            {
                _runtimeTitleDataModel.note = note.value;
                _UpdateSceneView();
            });

            _InitAsync();
        }

        private void _UpdateSceneView() {
            if (_isInit) return;

            databaseManagementService.SaveTitle(_runtimeTitleDataModel);
            _sceneView.Render();
        }

        private async void _InitAsync() {
            await Task.Delay(500);
            _isInit = false;
            _UpdateSceneView();
        }
    }
}