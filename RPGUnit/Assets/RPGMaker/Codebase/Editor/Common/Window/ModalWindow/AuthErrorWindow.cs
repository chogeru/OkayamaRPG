using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Common.Window.ModalWindow
{
    public class AuthErrorWindow : BaseModalWindow
    {
        protected override string ModalUxml =>
            "Assets/RPGMaker/Codebase/Editor/Common/Window/ModalWindow/Uxml/auth_error_window.uxml";

        private static string _errorMessage;
        
        public static void ShowWindow(string errorMessage) {
            _errorMessage = errorMessage;
            
            var wnd = GetWindow<AuthErrorWindow>();

            // 処理タイトル名適用
            wnd.titleContent = new GUIContent(EditorLocalize.LocalizeText(_errorMessage));
            wnd.Init();

            var size = new Vector2(600, 400);
            wnd.minSize = size;
            wnd.maxSize = size;
            wnd.maximized = false;
            
            wnd.ShowModalUtility();
        }
        
        /// <summary>
        /// タブ名と、メッセージが異なるとき
        /// </summary>
        /// <param name="tabTitle"></param>
        /// <param name="errorMessage"></param>
        public static void ShowWindow(string tabTitle, string errorMessage) {
            _errorMessage = errorMessage;
            
            var wnd = GetWindow<AuthErrorWindow>();

            // 処理タイトル名適用
            wnd.titleContent = new GUIContent(EditorLocalize.LocalizeText(tabTitle));
            wnd.Init();

            var size = new Vector2(600, 400);
            wnd.minSize = size;
            wnd.maxSize = size;
            wnd.maximized = false;
            
            wnd.ShowModalUtility();
        }

        public override void Init() {
            var root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModalUxml);
            VisualElement labelFromUxml = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUxml);
            root.Add(labelFromUxml);
            
            Label errorMessageLabel = labelFromUxml.Query<Label>("errorMessageLabel");
            errorMessageLabel.text = EditorLocalize.LocalizeText(_errorMessage);
        }
    }
}