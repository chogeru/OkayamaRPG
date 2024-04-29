using Assets.RPGMaker.Codebase.Editor.Common.Asset;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;

namespace RPGMaker.Codebase.Editor.Common.View
{
    public sealed class PopupFieldBase<T> : VisualElement
    {
        private readonly List<string> _choices = new List<string>();

        //選択肢から選択できないリストを格納する
        private List<string> _noItemList = new List<string>();
        public  int          index;
        public  string       value;

        private Button       button;
        private bool         isSlected = false;
        
        private string strWork;
        private int strWidth;
        
        public PopupFieldBase(
            List<string> choices,
            int defaultIndex,
            Func<T, string> formatSelectedValueCallback = null,
            Func<T, string> formatListItemCallback = null,
            int isNumberPattern = 0,
            List<string> noItemList = null
        ) {
            button = new Button();
            if (choices.Count <= defaultIndex) defaultIndex = 0; //フェールセーフ
            if (choices.Count == 0) return; //データが無いケース
            index = defaultIndex;
            RefreshChoices(choices, noItemList);
            Vector2 maxSize = Vector2.zero;
            button.text = _choices[defaultIndex];
            value = choices[defaultIndex];
            strWidth = (int)button.contentRect.width;
            strWork = button.text;
            button.clickable.clicked += () =>
            {
                var mouseRect = new Rect(Event.current.mousePosition, Vector2.one);
                var popupwindow = new PopupFieldScrollView(_choices, index, _noItemList);
                popupwindow.OnSelectChanged += s =>
                {
                    //0000 : →7文字
                    var str = s.Substring(7);
                    strWork = s;
                    button.text = s;
                    index = _choices.IndexOf(s);
                    value = str;
                    isSlected = true;
                };
                PopupWindow.Show(mouseRect, popupwindow);
            };
            Add(button);
            SetRegisterCallback();
        }

        /// <summary>
        /// Window幅が変更されたときに呼ばれる
        /// </summary>
        private void SetRegisterCallback() {
            this.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                EditorUpdate();
            });
        }

        private void EditorUpdate() {
            if (strWidth == (int) button.contentRect.width) return;
            for (int i = strWork.Length; i > 0; i--)
            {
                var s = strWork.Substring(0, i);
                if (i != 1 && GetTextWidth(s) > button.contentRect.width)
                {
                    continue;
                }

                if (i == strWork.Length)
                {
                    button.text = s;
                    button.tooltip = "";
                    break;
                }

                if (i - 2 <= 1)
                {
                    i = 3;
                }

                button.text = s.Substring(0, i - 2) + "...";
                button.tooltip = strWork;
                break;
            }

            strWidth = (int) button.contentRect.width;
        }

        public void RegisterValueChangedCallback(EventCallback<ChangeEvent<T>> callback) {
            RegisterCallback<MouseCaptureEvent>(evt =>
            {
                RegisterCallback<ChangeEvent<T>>(evt2 => 
                { 
                    if (isSlected)
                    {
                        callback.Invoke(evt2);
                        isSlected = false;
                    }
                });
            });
        }

        /// <summary>
        ///     選択肢を更新する
        /// </summary>
        /// <param name="choices">選択肢一覧</param>
        /// <param name="noItemList"></param>
        public void RefreshChoices(List<string> choices, List<string> noItemList = null) {
            _choices.Clear();
            var noItemListWork = new List<string>();

            if (choices.Count == _choices.Count)
                for (var i = 0; i < choices.Count; i++)
                {
                    _choices[i] = (i + 1).ToString("0000") + " : " + choices[i];
                    if (noItemList != null && noItemList.Contains(choices[i]))
                        noItemListWork.Add((i + 1).ToString("0000") + " : " + choices[i]);
                }
            else
                for (var i = 0; i < choices.Count; i++)
                {
                    _choices.Add((i + 1).ToString("0000") + " : " + choices[i]);
                    if (noItemList != null && noItemList.Contains(choices[i]))
                        noItemListWork.Add((i + 1).ToString("0000") + " : " + choices[i]);
                }

            _noItemList = noItemListWork;
        }

        public void ForceSetIndex(int index)
        {
            this.index = index;
            this.value = _choices[index];
            ChangeButtonText(index);
        }

        public void ChangeButtonText(int index) {
            button.text = _choices[index];
        }
        public void ChangeButtonText(string text) {
            button.text = text;
        }

        private float GetTextWidth(string text) {
            try
            {
                var size = EditorStyles.label.CalcSize(text);
                return size.x;
            }
            catch (Exception)
            {
                //
            }
            return 0f;
        }
    }
}