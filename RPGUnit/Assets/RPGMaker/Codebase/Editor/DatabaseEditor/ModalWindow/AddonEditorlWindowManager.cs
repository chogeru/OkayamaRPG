using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow
{
    public class AddonEditorWindowManager : ScriptableSingleton<AddonEditorWindowManager>
    {
        private List<EditorWindow> _paramEditWindows = new List<EditorWindow>();
        private List<EditorWindow> _windows          = new List<EditorWindow>();

        public void RegisterWindow(EditorWindow w) {
            _windows.Add(w);
        }

        public void RegisterParameterEditWindow(EditorWindow w) {
            _windows.Add(w);
            _paramEditWindows.Add(w);
        }

        public void UnregisterWindow(EditorWindow w) {
            _windows.Remove(w);
            _paramEditWindows.Remove(w);
        }

        public void CloseAllAddonWindows() {
            var list = _windows.ToList();
            foreach (var w in list)
                try
                {
                    w.Close();
                }
                catch (Exception)
                {
                }

            _windows.Clear();
            _paramEditWindows.Clear();
        }

        public void CloseDescendantWindows(EditorWindow w) {
            var index = _paramEditWindows.FindIndex(x => x == w);
            if (index >= 0)
                for (var i = _paramEditWindows.Count - 1; i > index; i--)
                {
                    var w2 = _paramEditWindows[i];
                    _paramEditWindows.RemoveAt(i);
                    w2.Close();
                }
        }

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad() {
            EditorCoroutineUtility.StartCoroutine(Initialize(), instance);
        }

        private static IEnumerator Initialize() {
            while (EditorApplication.timeSinceStartup < 0.2f) yield return null;
            instance.CloseAllAddonWindows();
        }
    }
}