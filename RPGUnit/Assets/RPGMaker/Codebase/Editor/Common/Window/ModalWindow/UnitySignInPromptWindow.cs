using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Common.Window.ModalWindow
{
    public class UnitySignInPromptWindow : BaseModalWindow
    {
        protected override string ModalUxml =>
            "Assets/RPGMaker/Codebase/Editor/Common/Window/ModalWindow/Uxml/unity_sign_in_prompt_window.uxml";

        public static void ShowWindow() {
            var wnd = GetWindow<UnitySignInPromptWindow>();

            // 処理タイトル名適用
            wnd.titleContent = new GUIContent(EditorLocalize.LocalizeText("WORD_5001"));
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
            
            var toUnityIdButton = labelFromUxml.Query<Button>("to_unity_id_button").AtIndex(0);
            labelFromUxml.Query<Label>("restart_unity_message").AtIndex(0).style.visibility = Visibility.Hidden;
            toUnityIdButton.clicked += () =>
            {
                labelFromUxml.Query<Label>("restart_unity_message").AtIndex(0).style.visibility = Visibility.Visible;
                EditorApplication.ExecuteMenuItem("Edit/Sign in...");
                //Application.OpenURL("https://id.unity.com//");
            };
        }
    }
}