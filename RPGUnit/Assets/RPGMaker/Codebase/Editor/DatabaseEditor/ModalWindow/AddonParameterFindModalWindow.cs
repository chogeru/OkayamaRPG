using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.AddonUIUtil;
using RPGMaker.Codebase.Editor.Common.View;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow
{
    public class AddonParameterFindInfo
    {
        public enum Match
        {
            Partial,
            Full,
            RegularExpression
        }

        public bool   ignoreCase;
        public Match  match = Match.Partial;
        public string pattern;
        public Regex  regex;
        public bool   valid;

        public void FindNext(EditorWindow editorWindow, IAddonParameterFinder parameterFinder) {
            editorWindow.RemoveNotification();
            if (!parameterFinder.FindNext())
                editorWindow.ShowNotification(new GUIContent(EditorLocalize.LocalizeText("WORD_2580")));
        }

        public void FindPrev(EditorWindow editorWindow, IAddonParameterFinder parameterFinder) {
            editorWindow.RemoveNotification();
            if (!parameterFinder.FindPrev())
                editorWindow.ShowNotification(new GUIContent(EditorLocalize.LocalizeText("WORD_2581")));
        }

        public bool IsMatch(string[] texts) {
            switch (match)
            {
                case Match.Partial:
                    foreach (var text in texts)
                        if (text.Contains(pattern,
                            ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                            return true;
                    return false;
                case Match.Full:
                    foreach (var text in texts)
                        if (text.Length == pattern.Length && text.Contains(pattern,
                            ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                            return true;
                    return false;
                case Match.RegularExpression:
                    foreach (var text in texts)
                        if (regex.Match(text).Success)
                            return true;
                    return false;
            }

            return false;
        }
    }

    public interface IAddonParameterFinder
    {
        public bool FindNext();
        public bool FindPrev();
    }

    public class AddonParameterFindModalWindow : AddonBaseModalWindow
    {
        private readonly Vector2Int WINDOW_SIZE = new Vector2Int(346, 200);

        private AddonParameterFindInfo _findInfo;
        private IAddonParameterFinder  _parameterFinder;

        private VisualElement listWindow;

        protected override string ModalUxml =>
            "Assets/RPGMaker/Codebase/Editor/DatabaseEditor/ModalWindow/Uxml/addon_parameter_find_modalwindow.uxml";

        protected override string ModalUss => "";

        public void SetInfo(AddonParameterFindInfo findInfo, IAddonParameterFinder parameterFinder) {
            _findInfo = findInfo;
            _parameterFinder = parameterFinder;
        }

        private void OnDestroy() {
            AddonEditorWindowManager.instance.CloseDescendantWindows(this);
            AddonEditorWindowManager.instance.UnregisterWindow(this);
        }

        public override void ShowWindow(string modalTitle, CallBackWidow callBack) {
            var wnd = this; // GetWindow<AddonParameterFindModalWindow>();
            AddonEditorWindowManager.instance.RegisterParameterEditWindow(wnd);

            if (callBack != null) _callBackWindow = callBack;

            wnd.titleContent = new GUIContent(EditorLocalize.LocalizeText("WORD_2574"));
            wnd.Init();
            Vector2 size = WINDOW_SIZE;
            wnd.minSize = size;
            wnd.maxSize = size;
            wnd.maximized = false;
            wnd.Show();
        }

        public override void Init() {
            var root = rootVisualElement;

            // 要素作成
            //----------------------------------------------------------------------
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModalUxml);
            VisualElement labelFromUxml = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUxml);
            root.Add(labelFromUxml);
            listWindow = labelFromUxml.Query<VisualElement>("system_window_rightwindow").AtIndex(0);
            var patternWindow = labelFromUxml.Query<VisualElement>("system_window_patternwindow").AtIndex(0);
            var matchWindow = labelFromUxml.Query<VisualElement>("system_window_matchwindow").AtIndex(0);

            // Pattern
            var label = new Label(EditorLocalize.LocalizeText("WORD_2575"));
            label.style.height = 16;
            patternWindow.Add(label);
            var textField = new ImTextFieldEnterFocusOut();
            textField.value = _findInfo.pattern;
            patternWindow.Add(textField);

            patternWindow.AddToClassList("four_borders");
            patternWindow.AddToClassList("margin4px");
            //label.AddToClassList("text_ellipsis");

            // Match
            label = new Label(EditorLocalize.LocalizeText("WORD_2576"));
            label.style.height = 16;
            matchWindow.Add(label);
            var nameList = new List<string>
            {
                EditorLocalize.LocalizeText("WORD_2577"), EditorLocalize.LocalizeText("WORD_2578"),
                EditorLocalize.LocalizeText("WORD_2579")
            };
            var popupField = new PopupFieldBase<string>(nameList, 0);
            //popupField.RegisterValueChangedCallback((evt =>
            //{
            //var popupField = evt.currentTarget as PopupFieldBase<string>;
            //Debug.Log($"popupField: {popupField.index}, {popupField.value}");
            //}));
            popupField.style.flexGrow = 1;
            matchWindow.Add(popupField);
            var toggle = new Toggle(EditorLocalize.LocalizeText("WORD_2583"));
            matchWindow.Add(toggle);

            // Next, Previous, Close
            //----------------------------------------------------------------------
            var buttonNext = labelFromUxml.Query<Button>("Common_Button_Next").AtIndex(0);
            var buttonPrev = labelFromUxml.Query<Button>("Common_Button_Previous").AtIndex(0);
            var buttonClose = labelFromUxml.Query<Button>("Common_Button_Close").AtIndex(0);
            buttonNext.style.alignContent = Align.FlexEnd;
            buttonNext.clicked += RegisterOkAction(() =>
            {
                var match = (AddonParameterFindInfo.Match) popupField.index;
                Regex regex = null;
                if (match == AddonParameterFindInfo.Match.RegularExpression)
                    if (!CheckRegularExpression(textField.value, !toggle.value, out regex))
                        return;
                _findInfo.pattern = textField.value;
                if (match == AddonParameterFindInfo.Match.RegularExpression) _findInfo.regex = regex;
                _findInfo.match = match;
                _findInfo.ignoreCase = !toggle.value;
                _findInfo.valid = true;
                RemoveNotification();
                if (!_parameterFinder.FindNext())
                    ShowNotification(new GUIContent(EditorLocalize.LocalizeText("WORD_2580")));
            });

            buttonPrev.clicked += () =>
            {
                var match = (AddonParameterFindInfo.Match) popupField.index;
                Regex regex = null;
                if (match == AddonParameterFindInfo.Match.RegularExpression)
                    if (!CheckRegularExpression(textField.value, !toggle.value, out regex))
                        return;
                _findInfo.pattern = textField.value;
                if (match == AddonParameterFindInfo.Match.RegularExpression) _findInfo.regex = regex;
                _findInfo.match = match;
                _findInfo.valid = true;
                _findInfo.ignoreCase = !toggle.value;
                RemoveNotification();
                if (!_parameterFinder.FindPrev())
                    ShowNotification(new GUIContent(EditorLocalize.LocalizeText("WORD_2581")));
            };

            buttonClose.clicked += () => { Close(); };
            SetDelayedAction(() => { textField.Focus(); });
        }

        private bool CheckRegularExpression(string str, bool ignoreCase, out Regex regex) {
            regex = null;
            try
            {
                regex = new Regex(str, RegexOptions.Compiled | (ignoreCase ? RegexOptions.IgnoreCase : 0));
            }
            catch (Exception)
            {
                RemoveNotification();
                ShowNotification(new GUIContent(EditorLocalize.LocalizeText("WORD_2582")));
                return false;
            }

            return true;
        }
    }
}