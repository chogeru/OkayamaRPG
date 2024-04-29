using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Common.View
{
    public class PopupFieldScrollView : PopupWindowContent
    {
        private const string UssDarkPath = "Assets/RPGMaker/Codebase/Editor/Hierarchy/Region/Base/Asset/hierarchyDark.uss";
        private const string UssLightPath = "Assets/RPGMaker/Codebase/Editor/Hierarchy/Region/Base/Asset/hierarchyLight.uss";
        private readonly List<string> _itemList = new List<string>();

        private readonly string[] _items;
        private float _width;
        private float _height;
        private List<string> _noItemList;
        private ImTextField _textField;
        private ScrollView _scrollView;
        private int _defaultSelect;
        private bool _isInitialize;

        private const float SEARCH_HEIGHT = 20f;
        private const float BUTTON_HEIGHT = 22f;
        private const float WINDOW_WIDTH = 200f;
        private const float WINDOW_MAX_HEIGHT = 500f;
        private const float SCROLLBAR_WIDTH = 18f;

        public Action<string> OnSelectChanged;

        public PopupFieldScrollView(List<string> lists, int defaultSelect, List<string> noItemList = null) {
            _isInitialize = false;

            _items = new string[lists.Count];
            _noItemList = noItemList;
            _defaultSelect = defaultSelect;

            Load(lists);
        }

        public override void OnGUI(Rect rect) {
            if (_isInitialize) return;
            _isInitialize = true;

            //検索用の窓と、ListViewはFixedで座標を決める
            _textField.style.position = Position.Absolute;
            _textField.style.left = 0;
            _textField.style.top = 0;

            _scrollView.style.position = Position.Absolute;
            _scrollView.style.left = 0;
            _scrollView.style.top = SEARCH_HEIGHT;
            //_scrollView.style.marginTop = SEARCH_HEIGHT;
            _scrollView.style.height = GetWindowSize().y - SEARCH_HEIGHT;
            _scrollView.style.overflow = Overflow.Hidden;

            //初期位置までスクロール
            Vector2 scroll = _scrollView.scrollOffset;
            scroll.y = _defaultSelect * BUTTON_HEIGHT;
            _scrollView.scrollOffset = scroll;
        }

        public override Vector2 GetWindowSize() {
            _width = WINDOW_WIDTH;
            _height = _items.Length * BUTTON_HEIGHT + SEARCH_HEIGHT * 2;
            if (_height > WINDOW_MAX_HEIGHT) _height = WINDOW_MAX_HEIGHT;
            return new Vector2(_width, _height);
        }

        public override void OnOpen() {
            var root = editorWindow.rootVisualElement;
            var listview = CreateListView();
            _textField = CreateTextField(listview);
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssDarkPath);
            if (!EditorGUIUtility.isProSkin)
            {
                styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssLightPath);
            }
            root.styleSheets.Add(styleSheet);
            root.Add(_textField);
            root.Add(listview);

            _textField.schedule.Execute(() => _textField.Focus()).StartingIn(15);
        }

        public void NoSelectNameList(List<string> lists) {
            _noItemList = lists;
            OnOpen();
        }

        private void Load(List<string> lists) {
            for (var i = 0; i < lists.Count; i++)
                _items[i] = lists[i];
        }

        private ImTextField CreateTextField(VisualElement content) {
            var textField = new ImTextField();

            textField.RegisterValueChangedCallback(e =>
            {
                content.schedule.Execute(() =>
                {
                    var newItems = _items.Where(item => item.ToLower().Contains(textField.value.ToLower()));
                    _itemList.Clear();
                    _scrollView.Clear();
                    _itemList.AddRange(newItems);
                    CreateList(_scrollView);
                }).StartingIn(30);
            });
            return textField;
        }

        private VisualElement CreateListView() {
            var ve = new VisualElement();

            _scrollView = new ScrollView();
            ve.Add(_scrollView);
            _itemList.AddRange(_items);
            CreateList(_scrollView);
            return ve;
        }

        private void CreateList(ScrollView scrollView) {
            foreach (var item in _itemList.Select((value, index) => new {value, index}))
            {
                var btn = new Button {text = item.value};
                var selectedName = item.value;
                btn.style.height = BUTTON_HEIGHT;
                btn.style.width = WINDOW_WIDTH - SCROLLBAR_WIDTH;
                btn.style.textOverflow = TextOverflow.Ellipsis;
                btn.clickable.clicked += () =>
                {
                    if (_noItemList != null && !_noItemList.Contains(selectedName))
                    {
                        OnSelectChanged?.Invoke(selectedName);
                    }

                    editorWindow.Close();
                };
                if (_noItemList != null && _noItemList.Contains(item.value))
                {
                    btn.SetEnabled(false);
                }

                scrollView.Add(btn);

                if (item.index == _defaultSelect)
                {
                    btn.AddToClassList("active");
                }
            }

            //スクロールビューの一番下に、margin 用の空のVisualElement配置
            //初期スクロール位置が一番下のアイテムだった場合に、そこまでスクロールできないため、それの解消用
            VisualElement visualElement = new VisualElement();
            visualElement.style.height = SEARCH_HEIGHT;
            scrollView.Add(visualElement);
        }
    }
}