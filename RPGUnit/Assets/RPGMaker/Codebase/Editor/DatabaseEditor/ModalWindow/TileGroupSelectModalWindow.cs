using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Common.Window;
using RPGMaker.Codebase.Editor.MapEditor.Component.ListView;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow
{
    public class TileGroupSelectModalWindow : BaseModalWindow
    {
        // 入力規制
        private const int    TEXT_FIELD_WIDTH = 140;
        private const int    TEXT_MAX_SIZE    = 14;

        private static MapManagementService     _mapManagementService;
        private static List<TileGroupDataModel> _tileGroupEntities;
        private static List<TileDataModelInfo>  _tileData;
        private static int                      _selectIndex;

        private readonly string ADD_TEXT = EditorLocalize.LocalizeText("WORD_1613");

        private readonly Vector2Int WINDOW_SIZE = new Vector2Int(1050, 400);


        private bool _isSave;

        //デフォルトでのPathはタイトル背景のものになっています
        private string _path = "Assets/RPGMaker/Storage/Images/Titles1/";

        private VisualElement leftWindow;
        private VisualElement rightWindow;
        private VisualElement tileWindow;

        private ListView _typeView;

        protected override string ModalUxml =>
            "Assets/RPGMaker/Codebase/Editor/DatabaseEditor/ModalWindow/Uxml/select_tilegroup_modalwindow.uxml";

        protected override string ModalUss => "";

        public bool IsLeftWindow { get; set; } = false;

        public void ChangePath(string path) {
            _path = path;
        }

        public override void ShowWindow(string modalTitle, CallBackWidow callBack) {
            var wnd = GetWindow<TileGroupSelectModalWindow>();

            if (callBack != null) _callBackWindow = callBack;

            wnd.titleContent = new GUIContent(EditorLocalize.LocalizeText("WORD_1611"));
            wnd.Init();
            Vector2 size = WINDOW_SIZE;
            wnd.minSize = size;
            wnd.maxSize = size;
            wnd.maximized = false;
        }

        public void ShowSaveWindow(string modalTitle, CallBackWidow callBack, List<TileDataModelInfo> tileData) {
            _isSave = true;
            _tileData = tileData;
            ShowWindow(modalTitle, callBack);
        }

        public void ShowLoadWindow(string modalTitle, CallBackWidow callBack) {
            _isSave = false;
            ShowWindow(modalTitle, callBack);
        }

        public override void Init() {
            var root = rootVisualElement;

            // タイルグループを取得
            _mapManagementService = Editor.Hierarchy.Hierarchy.mapManagementService;
            _tileGroupEntities = _mapManagementService.LoadTileGroups();

            // 要素作成
            //----------------------------------------------------------------------
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModalUxml);
            VisualElement labelFromUxml = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUxml);
            root.Add(labelFromUxml);
            leftWindow = labelFromUxml.Query<VisualElement>("system_window_leftwindow").AtIndex(0);
            rightWindow = labelFromUxml.Query<VisualElement>("system_window_rightwindow").AtIndex(0);
            rightWindow.style.width = 1000;
            rightWindow.style.height = 400;
            tileWindow = labelFromUxml.Query<VisualElement>("tile_list").AtIndex(0);
            tileWindow.style.flexGrow = 1;

            // タイルグループのリスト
            //----------------------------------------------------------------------
            if (_tileGroupEntities != null && _tileGroupEntities.Count != 0)
            {
                leftWindow.Add(CreateListView());
                //ListViewにフォーカスをあてる
                _typeView.Focus();
            }
            else
            {
                var visualElement = new VisualElement();
                visualElement.style.flexGrow = 1;
                leftWindow.Add(visualElement);
            }

            // セーブ時は新規追加ボタンを追加
            if (_isSave)
                leftWindow.Add(CreateAddButton(leftWindow));

            // 確定、キャンセルボタン
            //----------------------------------------------------------------------
            var buttonOk = labelFromUxml.Query<Button>("Common_Button_Ok").AtIndex(0);
            var buttonCancel = labelFromUxml.Query<Button>("Common_Button_Cancel").AtIndex(0);
            buttonOk.style.alignContent = Align.FlexEnd;
            buttonOk.clicked += () =>
            {
                if (_tileGroupEntities != null && _tileGroupEntities.Count != 0)
                {
                    if (_isSave)
                    {
                        // 確定時に引数のタイルグループを保存
                        _tileGroupEntities[_selectIndex].tileDataModels = _tileData;
                        _mapManagementService.SaveTileGroup(_tileGroupEntities[_selectIndex]);
                        _callBackWindow(null);
                    }
                    else
                    {
                        _callBackWindow(_tileGroupEntities[_selectIndex].tileDataModels);
                    }
                }

                Close();
            };

            buttonCancel.clicked += () =>
            {
                if (_isSave)
                    if (_tileGroupEntities != null && _tileGroupEntities.Count != 0)
                        _callBackWindow(null);
                Close();
            };
        }

        // リストの要素作成
        private ListView CreateListView() {
            Action<VisualElement, int> bindType = (e, i) =>
            {
                e.Clear();
                // セーブ用のUI作成
                if (_isSave)
                {
                    var visualElement = new VisualElement();
                    visualElement.style.flexDirection = FlexDirection.Row;

                    var l = new Label("#" + string.Format("{0:D4}", i + 1) + " ");
                    var text = new ImTextField();
                    text.value = _tileGroupEntities[i].name;
                    text.style.width = TEXT_FIELD_WIDTH;
                    text.RegisterCallback<FocusOutEvent>(o =>
                    {
                        if (text.value.Length > TEXT_MAX_SIZE)
                            text.value = text.value.Substring(0, TEXT_MAX_SIZE);
                        _tileGroupEntities[i].name = text.value;
                        _mapManagementService.SaveTileGroup(_tileGroupEntities[i]);
                        MapEditor.MapEditor.ReloadTileGroups();
                    });

                    visualElement.Add(l);
                    visualElement.Add(text);
                    e.Add(visualElement);
                }
                else
                {
                    var l = new Label("#" + string.Format("{0:D4}", i + 1) + " " + _tileGroupEntities[i].name);
                    e.Add(l);
                }
            };

            Func<VisualElement> makeType = () => new Label();
            _typeView = new ListView(new string[_tileGroupEntities.Count], 16, makeType, bindType);
            _typeView.name = "list";
            _typeView.selectionType = SelectionType.Multiple;
            _typeView.style.flexGrow = 1.0f;

            _typeView.onSelectionChange += objects =>
            {
                _SelectTileGroup(tileWindow, _typeView.selectedIndex);
                _selectIndex = _typeView.selectedIndex;
            };
            _typeView.selectedIndex = 0;
            _selectIndex = _typeView.selectedIndex;

            return _typeView;
        }

        // 新規追加ボタン
        private Button CreateAddButton(VisualElement visualElement) {
            var button = new Button();
            button.text = ADD_TEXT;
            button.clicked += () =>
            {
                // データを作成して追加、セーブする
                _mapManagementService.SaveTileGroup(new TileGroupDataModel(
                    Guid.NewGuid().ToString(), EditorLocalize.LocalizeText("WORD_1596"), new List<TileDataModelInfo>()));
                _tileGroupEntities = _mapManagementService.LoadTileGroups();
                visualElement.Clear();
                visualElement.Add(CreateListView());
                visualElement.Add(CreateAddButton(visualElement));
                MapEditor.MapEditor.ReloadTileGroups();
            };
            return button;
        }

        // タイルの表示更新
        private void _SelectTileGroup(VisualElement element, int index) {
            element.Clear();
            element.style.flexGrow = 1;
            element.Add(new TileListView(_tileGroupEntities[index].tileDataModels, null, null, false, true,
                WINDOW_SIZE.x - leftWindow.style.width.value.value));
        }
    }
}