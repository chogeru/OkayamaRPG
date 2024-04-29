using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.Window;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow
{
    public class PreviewWindow : BaseModalWindow
    {
        public string previewImage;

        private VisualElement rightWindow;

        public string CharacterPath { get; set; } = "";

        protected override string ModalUxml =>
            "Assets/RPGMaker/Codebase/Editor/DatabaseEditor/ModalWindow/Uxml/preview_window.uxml";

        protected override string ModalUss => "";

        public bool IsLeftWindow { get; set; } = false;

        public string ImagePath { get; set; } = "";


        public override void ShowWindow(string modalTitle, CallBackWidow callBack) {
            var wnd = GetWindow<PreviewWindow>();

            if (callBack != null) _callBackWindow = callBack;

            wnd.titleContent = new GUIContent(EditorLocalize.LocalizeText("WORD_1611"));
            wnd.Init();
            var size = new Vector2(400, 400);
            wnd.minSize = size;
            wnd.maxSize = size;
            wnd.maximized = false;
        }

        public override void Init() {
            var root = rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModalUxml);
            VisualElement labelFromUxml = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUxml);
            root.Add(labelFromUxml);
            var leftWindow = labelFromUxml.Query<VisualElement>("").AtIndex(0);
            rightWindow = labelFromUxml.Query<VisualElement>("system_window_rightwindow").AtIndex(0);

            var path = ImagePath;
            _SetImage(rightWindow, path + previewImage + ".png");
        }

        private void _SetImage(VisualElement element, string path) {
            CharacterPath = path;
            _UpdateImage();
        }

        private void _UpdateImage() {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(CharacterPath);

            rightWindow.Query<VisualElement>("titleback2").AtIndex(0).style.backgroundImage = tex;
            rightWindow.Query<VisualElement>("titleback2").AtIndex(0).style.width = tex.width;
            rightWindow.Query<VisualElement>("titleback2").AtIndex(0).style.height = tex.height;
        }
    }
}