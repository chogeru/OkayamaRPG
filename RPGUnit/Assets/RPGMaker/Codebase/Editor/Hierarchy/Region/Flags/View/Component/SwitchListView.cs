using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.Editor.Common;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Flags.View.Component
{
    public class SwitchListView : VisualElement
    {
        private readonly int                          _offsetInOtherList;
        private          Action<FlagDataModel.Switch> _onLeftClick;

        private Action<FlagDataModel.Switch> _onRightClick;

        // データプロパティ
        private List<FlagDataModel.Switch> _switches;

        private string _viewName;

        
        /**
         * コンストラクタ
         */
        public SwitchListView(
            List<FlagDataModel.Switch> switches,
            SelectionType targetSelectionType,
            int offsetInOtherList,
            string viewName
        ) {
            _switches = switches;
            style.flexDirection = FlexDirection.Column;
            _offsetInOtherList = offsetInOtherList;
            _viewName = viewName;

        }

        public void SetEventHandler(
            Action<FlagDataModel.Switch> onLeftClick,
            Action<FlagDataModel.Switch> onRightClick
        ) {
            _onLeftClick = onLeftClick;
            _onRightClick = onRightClick;
        }

        /**
         * データおよび表示を更新
         */
        public void Refresh([CanBeNull] List<FlagDataModel.Switch> items = null) {
            if (items != null) _switches = items;

            if (_switches != null)
            {
                Clear();
                var i = 0;
                foreach (var sw in _switches)
                {
                    var b = new Button
                    {
                        text = $"#{_offsetInOtherList + i + 1:0000}" + " " + _switches[i].name,
                        name = _viewName + $"{_offsetInOtherList + i + 1:0000}" + " " + _switches[i].id
                    };
                    var index = i;
                    Editor.Hierarchy.Hierarchy.AddSelectableElementAndAction(b, () =>
                    {
                        // 左クリック
                        _onLeftClick(_switches[index]);
                    });
                    BaseClickHandler.ClickEvent(b, evt =>
                    {
                        if (evt == (int) MouseButton.RightMouse + 1)
                            // 右クリック
                            _onRightClick(_switches[index]);
                        else
                            Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(b);
                    });
                    Add(b);
                    i++;
                }
            }
        }
    }
}