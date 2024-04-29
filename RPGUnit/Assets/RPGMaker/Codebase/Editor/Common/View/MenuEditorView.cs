using RPGMaker.Codebase.Editor.Common.Window;
using RPGMaker.Codebase.Editor.Common.Window.ModalWindow;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Common.View
{
    public class MenuEditorView : VisualElement
    {
        public const string ImagePath = "Assets/RPGMaker/Storage/Images/System/Menu/";
        public const string Dark = "Dark/";
        public const string Light = "Light/";
        public const string ImagePathActive = "Active/";
        public const string ImagePathDown = "Down/";
        public const string ImagePathDisable = "Disable/";
        public const string ImagePathLog = "unite_logo_s.png";

        public const string HelpURL_JP = "https://support.rpgmakerunite.com/hc/ja";
        public const string HelpURL_EN = "https://support.rpgmakerunite.com/hc/en-us";
        public const string HelpURL_CN = "https://support.rpgmakerunite.com/hc/zh-cn";
        public const string ImageIconMenu  = "uibl_icon_menu_";
        public const string ImageIconMenuP = "uibl_icon_menu_p_";
        public const string ImageIconMenuD = "uibl_icon_menu_d_";

        private readonly List<int> _btnTypes = new List<int>
        {
            (int) MenuWindow.BtnType.New,
            (int) MenuWindow.BtnType.Open,
            (int) MenuWindow.BtnType.Save,
            (int) MenuWindow.BtnType.Pen,
            (int) MenuWindow.BtnType.Close,
            (int) MenuWindow.BtnType.Play,
            (int) MenuWindow.BtnType.Addon
        };

        private readonly List<int> _btnTypesPlay = new List<int>
        {
            (int) MenuWindow.BtnType.New,
            (int) MenuWindow.BtnType.Open,
            (int) MenuWindow.BtnType.Save,
            (int) MenuWindow.BtnType.Pen,
            (int) MenuWindow.BtnType.Close,
            (int) MenuWindow.BtnType.Stop
        };


        private readonly string mainUxml = "Assets/RPGMaker/Codebase/Editor/Common/Asset/Uxml/menu.uxml";
        private readonly MenuWindow _menuWindow;

        private VisualElement _helpArea;
        private float _helpPos;
        private VisualElement _logoArea;
        private VisualElement _menuArea;

        private List<Image> _imagesIcon;

        public MenuEditorView(MenuWindow menuWindow) {
            _menuWindow = menuWindow;
            Init();
        }

        private void Init() {
            Clear();
            var items = new VisualElement();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(mainUxml);
            VisualElement labelFromUxml = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUxml);
            labelFromUxml.style.flexGrow = 1;
            items.Add(labelFromUxml);
            _menuArea = items.Query<VisualElement>("menu_area");
            _logoArea = items.Query<VisualElement>("logo_area");
            _helpArea = items.Query<Button>("help_area");
            InitUi();
            Add(items);
        }

        private void InitUi() {
            var btns = _btnTypes;

            //テストプレイ中はアイコンがStopになる
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                btns = _btnTypesPlay;

            _imagesIcon = new List<Image>();
            for (var i = 0; i < btns.Count; i++)
            {
                var image = new Image();
                image.name = btns[i].ToString("000");
                image.image =
                    AssetDatabase.LoadAssetAtPath<Texture>(ImagePath + EditerMode() + ImagePathActive + ImageIconMenu +
                                                           btns[i].ToString("000") + ".png");
                image.style.width = image.image.width;
                image.style.height = image.image.height;
                image.RegisterCallback<MouseOverEvent>(MouseOverEvent);
                image.RegisterCallback<MouseUpEvent>(OnMouseUpEvent);
                image.RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
                _imagesIcon.Add(image);
                _menuArea.Add(image);
                _helpPos += image.image.width;
            }

            //ヘルプボタン
            _helpArea.style.left = _helpPos;
            _helpArea.style.height = _menuArea.style.height;
            BaseClickHandler.ClickEvent(_helpArea, evt =>
            {
                if (evt != (int) MouseButton.LeftMouse) return;
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1503")), false, () => { Help(); });

                menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1507")), false, () => { About(); });
                menu.ShowAsContext();
            });


            var logo = new Image();
            logo.image = AssetDatabase.LoadAssetAtPath<Texture>(ImagePath + EditerMode() +ImagePathLog);
            logo.style.width = logo.image.width;
            logo.style.height = logo.image.height;
            _logoArea.Add(logo);
        }

        public static void Help() {
            //現在の言語設定
            SystemLanguage _lang;
            var assembly2 = typeof(EditorWindow).Assembly;
            var localizationDatabaseType2 = assembly2.GetType("UnityEditor.LocalizationDatabase");
            var currentEditorLanguageProperty2 = localizationDatabaseType2.GetProperty("currentEditorLanguage");
            _lang = (SystemLanguage) currentEditorLanguageProperty2.GetValue(null);

            switch (_lang) 
            {
                case SystemLanguage.Japanese:
                    Application.OpenURL(HelpURL_JP);
                    break;
                
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                    Application.OpenURL(HelpURL_CN);
                    break;

                case SystemLanguage.English:
                default:
                    Application.OpenURL(HelpURL_EN);
                    break;
            }

            AnalyticsManager.Instance.PostEvent(
                AnalyticsManager.EventName.action,
                AnalyticsManager.EventParameter.help);
        }

        public static void About() {
            var versionCheckWindow = ScriptableObject.CreateInstance<VersionCheckWindow>();
            versionCheckWindow.ShowWindow();
        }

        //ダークモード、ライトモードのPath切り替え用
        public static string EditerMode() {
            return EditorGUIUtility.isProSkin ? Dark : Light ;
        }

        private void MouseOverEvent(MouseEventBase<MouseOverEvent> evt) {
            var image = (Image) evt.target;
            var iconName = image.name;
            image.image =
                AssetDatabase.LoadAssetAtPath<Texture>(
                    ImagePath + EditerMode() + ImagePathActive + ImageIconMenu + iconName + ".png");
        }

        private void OnMouseUpEvent(MouseEventBase<MouseUpEvent> evt) {
            var image = (Image) evt.target;
            var iconName = image.name;
            image.image =
                AssetDatabase.LoadAssetAtPath<Texture>(
                    ImagePath + EditerMode() + ImagePathActive + ImageIconMenu + iconName + ".png");
            _menuWindow.Select(int.Parse(image.name));
        }

        private void OnMouseDownEvent(MouseEventBase<MouseDownEvent> evt) {
            var image = (Image) evt.target;
            var iconName = image.name;
            image.image =
                AssetDatabase.LoadAssetAtPath<Texture>(
                    ImagePath + EditerMode() + ImagePathDown + ImageIconMenuP + iconName + ".png");
        }

        public Image GetIconImage(int index) {
            Image image = null;
            for (int i = 0; i < _imagesIcon.Count; i++)
            {
                if (int.Parse(_imagesIcon[i].name) == index)
                {
                    image = _imagesIcon[i];
                    break;
                }
            }

            return image;
        }
    }
}