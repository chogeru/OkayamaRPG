using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.AddonUIUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow
{
    public class AddonSelectImageModalWindow : AddonBaseModalWindow
    {
        private AudioSource _audioSource;

        private string _currentBasename;

        private GameObject _gameObject;

        private bool          _imageSelect = true;
        private bool          _nowPlaying;
        private string        _path;
        private VisualElement _rightWindow;

        protected override string ModalUxml =>
            "Assets/RPGMaker/Codebase/Editor/DatabaseEditor/ModalWindow/Uxml/addon_select_file_modal_window.uxml";

        protected override string ModalUss => "";


        private void OnDestroy() {
            if (_gameObject != null)
            {
                Stop();
                DestroyImmediate(_gameObject);
            }

            AddonEditorWindowManager.instance.CloseDescendantWindows(this);
            AddonEditorWindowManager.instance.UnregisterWindow(this);
        }

        public void SetInfo(string path, string currentBasename) {
            _path = $"Assets/RPGMaker/Storage/{path}";
            _currentBasename = currentBasename;
            _imageSelect = !(path.Length >= 7 && path.Substring(0, 7) == "Sounds/");
        }

        public override void ShowWindow(string modalTitle, CallBackWidow callBack) {
            var wnd = this; // GetWindow<AddonSelectImageModalWindow>();
            AddonEditorWindowManager.instance.RegisterParameterEditWindow(wnd);

            if (callBack != null) _callBackWindow = callBack;

            wnd.titleContent = new GUIContent(EditorLocalize.LocalizeText(_imageSelect ? "WORD_1611" : "WORD_1635"));
            wnd.Init();
            wnd.Show();
        }

        public override void Init() {
            var root = rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModalUxml);
            VisualElement labelFromUxml = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUxml);
            root.Add(labelFromUxml);
            var leftWindow = labelFromUxml.Query<VisualElement>("system_window_leftwindow").AtIndex(0);
            _rightWindow = labelFromUxml.Query<VisualElement>("system_window_rightwindow").AtIndex(0);
            if (_imageSelect)
            {
                var dir = new DirectoryInfo(_path);
                var fileInfoList = dir.GetFiles("*.png");
                var fileNames = fileInfoList.Select(f => f.Name.Replace(".png", "")).ToList();

                Action<VisualElement, int> bindType = (e, i) =>
                {
                    e.Clear();
                    var l = new Label(fileNames[i]);
                    e.Add(l);
                };
                Func<VisualElement> makeType = () => new Label();
                var listView = new StripedListView<string>(new string[fileNames.Count], 16, makeType, bindType);
                listView.name = "list";
                //listView.selectionType = SelectionType.Multiple;
                listView.style.flexGrow = 1;
                listView.AddToClassList("list_view");
                listView.SolidQuad();
                leftWindow.Add(listView);

                listView.onSelectionChange += objects =>
                {
                    _currentBasename = fileNames[listView.selectedIndex];
                    SetImage(_path + _currentBasename + ".png");
                };
                listView.RegisterCallback<KeyDownEvent>(e =>
                {
                    //Debug.Log($"pressed: '{e.character}'");
                    switch (e.keyCode)
                    {
                        case KeyCode.KeypadEnter:
                        case KeyCode.Return:
                            _callBackWindow(_currentBasename);
                            break;
                    }
                });
                listView.onItemsChosen += objects =>
                {
                    //Debug.Log($"objects: {objects.ToString()}");
                    //var list = objects.ToList();// as List<int>;
                    //if (list.Count == 0) return;
                    //Debug.Log($"list: {list[0]}");
                    //var index = int.Parse(list[0].ToString());
                    _callBackWindow(_currentBasename);
                };
                listView.onDragIndexChanged += index =>
                {
                    if (listView.selectedIndex != index) listView.SetSelection(index);
                };
                var index = fileNames.IndexOf(_currentBasename);
                if (index >= 0)
                {
                    listView.SetSelection(index);
                    SetImage(_path + _currentBasename + ".png");
                }
            }
            else
            {
                _gameObject = new GameObject();
                _gameObject.name = "AddonManagerSound";
                _audioSource = _gameObject.AddComponent<AudioSource>();

                var dir = new DirectoryInfo(_path);
                var fileNames = new List<string>();
                foreach (var f in dir.GetFiles("*.ogg")) fileNames.Add(f.Name.Replace(".ogg", ""));
                foreach (var f in dir.GetFiles("*.wav")) fileNames.Add(f.Name.Replace(".wav", ""));
                Action<VisualElement, int> bindType = (e, i) =>
                {
                    e.Clear();
                    var l = new Label(fileNames[i]);
                    e.Add(l);
                };
                Func<VisualElement> makeType = () => new Label();
                var listView = new StripedListView<string>(new string[fileNames.Count], 16, makeType, bindType);
                listView.style.flexGrow = 1;
                listView.AddToClassList("list_view");
                listView.SolidQuad();
                leftWindow.Add(listView);

                listView.onSelectionChange += objects => { _currentBasename = fileNames[listView.selectedIndex]; };
                listView.RegisterCallback<KeyDownEvent>(e =>
                {
                    //Debug.Log($"pressed: '{e.character}'");
                    switch (e.keyCode)
                    {
                        case KeyCode.KeypadEnter:
                        case KeyCode.Return:
                            Play(AppendSuffix(_path + _currentBasename));
                            break;
                    }
                });
                listView.onItemsChosen += objects =>
                {
                    //Debug.Log($"objects: {objects.ToString()}");
                    var list = objects.ToList(); // as List<int>;
                    //if (list.Count == 0) return;
                    //Debug.Log($"list: {list[0]}");
                    //var index = int.Parse(list[0].ToString());
                    //_currentBasename = fileNames[index];
                    Play(AppendSuffix(_path + _currentBasename));
                };
                listView.onDragIndexChanged += index =>
                {
                    if (listView.selectedIndex != index) listView.SetSelection(index);
                };

                var container = new VisualElement();
                container.style.flexGrow = 1;

                var playButton = new Button();
                playButton.text = EditorLocalize.LocalizeText("WORD_1630");
                playButton.clicked += () => { Play(AppendSuffix(_path + _currentBasename)); };
                container.Add(playButton);

                var stopButton = new Button();
                stopButton.text = EditorLocalize.LocalizeText("WORD_1631");
                stopButton.clicked += () => { Stop(); };
                container.Add(stopButton);

                _rightWindow.Query<VisualElement>("titleback2").AtIndex(0).Add(container);

                var index = fileNames.IndexOf(_currentBasename);
                if (index >= 0)
                {
                    listView.SetSelection(index);
                    SetImage(_path + _currentBasename + ".png");
                }
            }

            var buttonOk = labelFromUxml.Query<Button>("Common_Button_Ok").AtIndex(0);
            var buttonCancel = labelFromUxml.Query<Button>("Common_Button_Cancel").AtIndex(0);

            buttonOk.clicked += RegisterOkAction(() =>
            {
                _callBackWindow(_currentBasename);
                Close();
            });

            buttonCancel.clicked += () => { Close(); };
        }

        private string AppendSuffix(string filename) {
            var f = filename + ".ogg";
            if (File.Exists(f)) return f;
            f = filename + ".wav";
            if (File.Exists(f)) return f;
            return filename;
        }

        private void SetImage(string filename) {
            if (!File.Exists(filename)) return;
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(filename);
            _rightWindow.Query<VisualElement>("titleback2").AtIndex(0).style.backgroundImage = tex;
            if (tex)
            {
                _rightWindow.Query<VisualElement>("titleback2").AtIndex(0).style.width = tex.width;
                _rightWindow.Query<VisualElement>("titleback2").AtIndex(0).style.height = tex.height;
            }
        }

        private void Play(string filename) {
            Stop();
            if (!File.Exists(filename)) return;
            var audioData = AssetDatabase.LoadAssetAtPath<AudioClip>(filename);
            _audioSource.clip = audioData;
            _audioSource.Play();
            _nowPlaying = true;
        }


        private void Stop() {
            if (_audioSource != null && _nowPlaying)
            {
                _nowPlaying = false;
                _audioSource.Stop();
            }
        }
    }
}