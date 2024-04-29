using JetBrains.Annotations;
using RPGMaker.Codebase.Editor.Common;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Base.View.Component
{
    /// <summary>
    /// ヒエラルキーアイテムリストコンポーネント
    /// </summary>
    public class HierarchyItemListView : VisualElement
    {
        // データプロパティ
        private List<string> _items;
        private List<Button> _buttons;
        private Action<int, string> _onLeftClick;
        private Action<int, string> _onRightClick;

        private string _viewName = "";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="viewName"></param>
        public HierarchyItemListView(string viewName) {
            style.flexDirection = FlexDirection.Column;
            _buttons = new List<Button>();
            _viewName = viewName;
        }

        public void SetEventHandler(Action<int, string> onLeftClick, Action<int, string> onRightClick) {
            _onLeftClick = onLeftClick;
            _onRightClick = onRightClick;
        }

        /// <summary>
        /// データおよび表示を更新
        /// </summary>
        /// <param name="items"></param>
        /// <param name="updateNumber"></param>
        public void Refresh([CanBeNull] List<string> items, int updateNumber = -1) {

            if (items != null) _items = items;
            if (_items != null)
            {
                //全更新
                if (updateNumber == -1)
                {
                    Clear();
                    _buttons.Clear();
                    
                    var i = 0;
                    foreach (var item in _items)
                    {
                        var b = new Button { text = item, name = _viewName + i };
                        var index = i;
                        Editor.Hierarchy.Hierarchy.AddSelectableElementAndAction(b, () =>
                        {
                            // 左クリック
                            _onLeftClick(index, _items[index]);
                        });
                        BaseClickHandler.ClickEvent(b, evt =>
                        {
                            if (evt == (int) MouseButton.RightMouse + 1)
                            {
                                // 左クリック
                                _onLeftClick(index, _items[index]);
                                Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(b);
                            }
                            else if (evt == (int) MouseButton.RightMouse)
                            {
                                // 右クリック
                                _onRightClick(index, _items[index]);
                            }
                            else
                            {
                                Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(b);
                            }
                        });
                        i++;
                        Add(b);
                        _buttons.Add(b);
                    }
                }
                //差分更新
                else
                {
                    _buttons[updateNumber].text = items[updateNumber];
                }
            }
        }
    }
}