using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.Editor.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Flags.View.Component
{
    public class VariableListView : VisualElement
    {
        private readonly int _offsetInOtherList;

        private Action<FlagDataModel.Variable> _onLeftClick;

        private Action<FlagDataModel.Variable> _onRightClick;

        // データプロパティ
        private List<FlagDataModel.Variable> _variables;

        private string _viewName;


        /**
         * コンストラクタ
         */
        public VariableListView(
            List<FlagDataModel.Variable> variables,
            SelectionType targetSelectionType,
            int offsetInOtherList,
            string viewName
        ) {
            _variables = variables;
            style.flexDirection = FlexDirection.Column;
            _offsetInOtherList = offsetInOtherList;
            _viewName = viewName;
        }

        public void SetEventHandler(
            Action<FlagDataModel.Variable> onLeftClick,
            Action<FlagDataModel.Variable> onRightClick
        ) {
            _onLeftClick = onLeftClick;
            _onRightClick = onRightClick;
        }

        /**
         * データおよび表示を更新
         */
        public void Refresh([CanBeNull] List<FlagDataModel.Variable> items = null) {
            if (items != null) _variables = items;

            if (_variables != null)
            {
                Clear();
                var i = 0;
                foreach (var variable in _variables)
                {
                    var b = new Button
                    {
                        text = $"#{_offsetInOtherList + i + 1:0000}" + " " + _variables[i].name,
                        name = _viewName + $"{_offsetInOtherList + i + 1:0000}" + " " + _variables[i].id
                    };
                    var index = i;
                    Editor.Hierarchy.Hierarchy.AddSelectableElementAndAction(b, () =>
                    {
                        // 左クリック
                        _onLeftClick(_variables[index]);
                    });
                    BaseClickHandler.ClickEvent(b, evt =>
                    {
                        if (evt == (int) MouseButton.RightMouse + 1)
                            // 右クリック
                            _onRightClick(_variables[index]);
                        else
                            Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(b);
                    });
                    Add(b);
                    i++;
                }
            }
        }

        private class ClickEvent : Manipulator
        {
            private readonly Action<FlagDataModel.Variable> _onLeftClick;
            private readonly Action<FlagDataModel.Variable> _onRightClick;
            private readonly FlagDataModel.Variable         _variable;

            public ClickEvent(
                FlagDataModel.Variable sw,
                Action<FlagDataModel.Variable> onLeftClick,
                Action<FlagDataModel.Variable> onRightClick
            ) {
                _variable = sw;
                _onLeftClick = onLeftClick;
                _onRightClick = onRightClick;
            }

            protected override void RegisterCallbacksOnTarget() {
                target.RegisterCallback<MouseDownEvent>(MouseDownEvent);
            }

            protected override void UnregisterCallbacksFromTarget() {
                target.UnregisterCallback<MouseDownEvent>(MouseDownEvent);
            }

            private void MouseDownEvent(MouseEventBase<MouseDownEvent> evt) {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
                    _onRightClick(_variable);
                else
                    _onLeftClick(_variable);
            }
        }
    }
}