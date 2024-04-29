using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.Window;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow
{
    public class ImageSelectModalWindow : BaseModalWindow
    {
        private readonly string EXTENSION = "png";

        private string _currentSelectingImageFileName;

        private List<string> _fileNames;

        private ListView _imageListView;

        private string _path;
        private bool   _addNone;
        private string _noneText;

        private VisualElement _rightWindow;

        protected override string ModalUxml =>
            "Assets/RPGMaker/Codebase/Editor/DatabaseEditor/ModalWindow/Uxml/select_image_modal_window.uxml";

        protected override string ModalUss => "";

        public ImageSelectModalWindow(string path, bool addNone = false, string noneText = "WORD_0113") {
            _path = path;
            _addNone = addNone;
            _noneText = noneText;
        }

        public void ShowWindow(string modalTitle, CallBackWidow callBack, string currentSelectingImageFileName) {
            ShowWindow(modalTitle, callBack);
            _currentSelectingImageFileName = currentSelectingImageFileName;
            //描画を待ってから選択状態を更新する
            SelectImage();
        }

        private async void SelectImage() {
            await Task.Delay(1);
            bool flg = false;
            if (!string.IsNullOrEmpty(_currentSelectingImageFileName))
            {
                _SetImage(_path + _currentSelectingImageFileName + "." + EXTENSION);

                int index = _fileNames.FindIndex(item =>
                {
                    return item == _currentSelectingImageFileName;
                });

                if (index >= 0)
                {
                    _imageListView.SetSelection(index);
                    _imageListView.ScrollToId(index);
                    flg = true;
                }
            }
            if (!flg)
            {
                _imageListView.SetSelection(0);
                _imageListView.ScrollToId(0);
            }
        }

        public override void ShowWindow(string modalTitle, CallBackWidow callBack) {
            var wnd = GetWindow<ImageSelectModalWindow>();

            if (callBack != null) _callBackWindow = callBack;

            wnd.titleContent = new GUIContent(EditorLocalize.LocalizeText("WORD_1611"));
            wnd.Init();
        }

        public override void Init() {
            var root = rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModalUxml);
            VisualElement labelFromUxml = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUxml);
            root.Add(labelFromUxml);
            var leftWindow = labelFromUxml.Query<VisualElement>("system_window_leftwindow").AtIndex(0);
            _rightWindow = labelFromUxml.Query<VisualElement>("system_window_rightwindow").AtIndex(0);

            _currentSelectingImageFileName = "";

            var dir = new DirectoryInfo(_path);
            var fileInfoList = dir.GetFiles("*." + EXTENSION);
            _fileNames = fileInfoList.Select(f => f.Name.Replace("." + EXTENSION, "")).ToList();

            // 選択肢追加
            if (_addNone)
                _fileNames.Insert(0, EditorLocalize.LocalizeText(_noneText));

            Action<VisualElement, int> bindType = (e, i) =>
            {
                e.Clear();
                var l = new Label(_fileNames[i]);
                e.Add(l);
            };
            Func<VisualElement> makeType = () => new Label();
            _imageListView = new ListView(new string[_fileNames.Count], 16, makeType, bindType)
            {
                name = "list",
                selectionType = SelectionType.Multiple,
                style =
                {
                    flexGrow = 1.0f
                }
            };
            leftWindow.Add(_imageListView);

            _imageListView.onSelectionChange += objects =>
            {
                if (_addNone && _imageListView.selectedIndex == 0)
                    _currentSelectingImageFileName = "";
                else if(_fileNames.Count > 0)
                    _currentSelectingImageFileName = _fileNames[_imageListView.selectedIndex];
                else 
                    _currentSelectingImageFileName = "";
                _SetImage(_path + _currentSelectingImageFileName + "." + EXTENSION);
            };

            var buttonOk = labelFromUxml.Query<Button>("Common_Button_Ok").AtIndex(0);
            var buttonCancel = labelFromUxml.Query<Button>("Common_Button_Cancel").AtIndex(0);

            buttonOk.clicked += () =>
            {
                _callBackWindow(_currentSelectingImageFileName);
                Close();
            };

            buttonCancel.clicked += () => { Close(); };

            //ImageListViewにフォーカスをあてる
            _imageListView.Focus();
        }

        private void _SetImage(string path) {
            _UpdateImage(path);
        }

        private void _UpdateImage(string path) {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            _rightWindow.Query<VisualElement>("image").AtIndex(0).style.backgroundImage = tex;
            if (tex)
            {
                _rightWindow.Query<VisualElement>("image").AtIndex(0).style.width = tex.width;
                _rightWindow.Query<VisualElement>("image").AtIndex(0).style.height = tex.height;
            }
        }
    }
}