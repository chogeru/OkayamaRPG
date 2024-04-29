using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Common
{
    /// <summary>
    ///     トグルの選択処理クラス
    /// </summary>
    public class CommonToggleSelector
    {
        private int _activeNum;
        private List<Toggle> _toggles;
        private List<RadioButton> _radioButtons;
        private List<VisualElement> _elements;
        private List<Action> _actions;

        /// <summary>
        ///     対象の設定
        ///     引数1:設定対象(Toggle)
        ///     引数2:最初にチェックが入る配列番号
        ///     引数3:各Toggleに設定する処理（切り替え時に実行する処理）
        /// </summary>
        public void SetToggleSelector(List<Toggle> toggles, int defaultSelect = 0, List<Action> actions = null) {
            toggles[defaultSelect].value = true;
            _activeNum = defaultSelect;
            // nullでなければアクション実行
            if (actions != null && actions.Count > _activeNum && actions[_activeNum] != null)
                actions[_activeNum].Invoke();

            for (var i = 0; i < toggles.Count; i++)
            {
                var num = i;
                toggles[num].RegisterValueChangedCallback(evt =>
                {
                    // チェックが既に付いている場合は戻す
                    if (evt.previousValue && evt.newValue == false)
                    {
                        // 選択中の場合はそのまま
                        if (_activeNum == num)
                            toggles[num].value = true;
                    }
                    else if (evt.previousValue == false && evt.newValue)
                    {
                        // 選択を切り替える
                        if (_activeNum != num)
                        {
                            toggles[_activeNum].value = false;
                            _activeNum = num;

                            // nullでなければアクション実行
                            if (actions != null && actions.Count > num && actions[num] != null)
                                actions[num].Invoke();
                        }
                    }
                });
            }
        }
        
        public void SetRadioSelector(List<RadioButton> toggles, int defaultSelect = 0, List<Action> actions = null) {
            //選択可能な範囲で指定されていない場合には、初期値とする
            if (toggles.Count <= defaultSelect)
            {
                defaultSelect = 0;
            }
            toggles[defaultSelect].value = true;
            _activeNum = defaultSelect;
            // nullでなければアクション実行
            if (actions != null && actions.Count > _activeNum && actions[_activeNum] != null)
                actions[_activeNum].Invoke();

            for (var i = 0; i < toggles.Count; i++)
            {
                var num = i;
                toggles[num].RegisterValueChangedCallback(evt =>
                {
                    // チェックが既に付いている場合は戻す
                    if (evt.previousValue && evt.newValue == false)
                    {
                        // 選択中の場合はそのまま
                        if (_activeNum == num)
                            toggles[num].value = true;
                    }
                    else if (evt.previousValue == false && evt.newValue)
                    {
                        // 選択を切り替える
                        if (_activeNum != num)
                        {
                            toggles[_activeNum].value = false;
                            _activeNum = num;

                            // nullでなければアクション実行
                            if (actions != null && actions.Count > num && actions[num] != null)
                                actions[num].Invoke();
                        }
                    }
                });
            }
        }

        public void SetToggleMultipleSelectors(
            List<Toggle> toggles,
            List<int> defaultSelects,
            List<Action> actions = null
        ) {
            foreach (var defaultSelect in defaultSelects.Select((value, index) => new {value, index}))
                if (defaultSelect.value == 1)
                    toggles[defaultSelect.index].value = true;
            // nullでなければアクション実行
            if (actions != null && actions.Count > _activeNum && actions[_activeNum] != null)
                actions[_activeNum].Invoke();

            for (var i = 0; i < toggles.Count; i++)
            {
                var num = i;
                toggles[num].RegisterValueChangedCallback(evt =>
                {
                    // nullでなければアクション実行
                    if (actions != null && actions.Count > num && actions[num] != null)
                        actions[num].Invoke();
                });
            }
        }

        /// <summary>
        ///     対象の設定
        ///     Toggle切替時に、引数で渡されたVisualElementの子要素のEnabled状態も切り替える
        ///     引数のリストが1つでも動作する
        ///     
        ///     引数1:設定対象(Toggle)
        ///     引数2:Toggle切り替え時に同時に切り替えるVisualElement 引数で渡されたVisualElement内の class="toggle_contents" が対象（渡されたもの自体は非対象）
        ///     引数2:最初にチェックが入る配列番号
        ///     引数3:各Toggleに設定する処理（切り替え時に実行する処理）
        /// </summary>
        public void SetToggleInVisualElementSelector(List<Toggle> toggles, List<VisualElement> elements, int defaultSelect = 0, List<Action> actions = null) {
            toggles[defaultSelect].value = true;
            _activeNum = defaultSelect;
            _toggles = toggles;
            _elements = elements;
            _actions = actions;

            for (int i = 0; i < _elements.Count; i++)
            {
                if (i == defaultSelect)
                {
                    //子要素をActiveにする
                    foreach (var child in _elements[i].Children())
                        if (child.ClassListContains("toggle_contents"))
                        {
                            child.SetEnabled(true);
                        }
                }
                else
                {
                    //子要素を非Activeにする
                    foreach (var child in _elements[i].Children())
                        if (child.ClassListContains("toggle_contents"))
                        {
                            child.SetEnabled(false);
                        }
                }
            }

            // nullでなければアクション実行
            if (_actions != null && _actions.Count > _activeNum && _actions[_activeNum] != null)
                _actions[_activeNum].Invoke();

            for (var i = 0; i < _toggles.Count; i++)
            {
                var num = i;
                _toggles[num].RegisterValueChangedCallback(evt =>
                {
                    // チェックが既に付いている場合は戻す
                    if (evt.previousValue && evt.newValue == false)
                    {
                        // 選択中の場合はそのまま
                        if (_activeNum == num)
                            _toggles[num].value = true;
                    }
                    else if (evt.previousValue == false && evt.newValue)
                    {
                        // 選択を切り替える
                        if (_activeNum != num)
                        {
                            //元々選択状態だった子要素を非Activeにする
                            foreach (var child in _elements[_activeNum].Children())
                                if (child.ClassListContains("toggle_contents"))
                                {
                                    child.SetEnabled(false);
                                }

                            //新しく選択された子要素をActiveにする
                            foreach (var child in _elements[num].Children())
                                if (child.ClassListContains("toggle_contents"))
                                    if (child.ClassListContains("toggle_contents"))
                                    {
                                        child.SetEnabled(true);
                                    }

                            _toggles[_activeNum].value = false;
                            _activeNum = num;

                            // nullでなければアクション実行
                            if (_actions != null && _actions.Count > num && _actions[num] != null)
                                _actions[num].Invoke();
                        }
                    }
                });
            }
        }
        
        public void SetRadioInVisualElementSelector(List<RadioButton> toggles, List<VisualElement> elements, int defaultSelect = 0, List<Action> actions = null) {
            toggles[defaultSelect].value = true;
            _activeNum = defaultSelect;
            _radioButtons = toggles;
            _elements = elements;
            _actions = actions;

            for (int i = 0; i < _elements.Count; i++)
            {
                if (i == defaultSelect)
                {
                    //子要素をActiveにする
                    foreach (var child in _elements[i].Children())
                        if (child.ClassListContains("toggle_contents"))
                        {
                            child.SetEnabled(true);
                        }
                }
                else
                {
                    //子要素を非Activeにする
                    foreach (var child in _elements[i].Children())
                        if (child.ClassListContains("toggle_contents"))
                        {
                            child.SetEnabled(false);
                        }
                }
            }

            // nullでなければアクション実行
            if (_actions != null && _actions.Count > _activeNum && _actions[_activeNum] != null)
                _actions[_activeNum].Invoke();

            for (var i = 0; i < _radioButtons.Count; i++)
            {
                var num = i;
                _radioButtons[num].RegisterValueChangedCallback(evt =>
                {
                    // チェックが既に付いている場合は戻す
                    if (evt.previousValue && evt.newValue == false)
                    {
                        // 選択中の場合はそのまま
                        if (_activeNum == num)
                            _radioButtons[num].value = true;
                    }
                    else if (evt.previousValue == false && evt.newValue)
                    {
                        // 選択を切り替える
                        if (_activeNum != num)
                        {
                            //元々選択状態だった子要素を非Activeにする
                            foreach (var child in _elements[_activeNum].Children())
                                if (child.ClassListContains("toggle_contents"))
                                {
                                    child.SetEnabled(false);
                                }

                            //新しく選択された子要素をActiveにする
                            foreach (var child in _elements[num].Children())
                                if (child.ClassListContains("toggle_contents"))
                                    if (child.ClassListContains("toggle_contents"))
                                    {
                                        child.SetEnabled(true);
                                    }

                            _radioButtons[_activeNum].value = false;
                            _activeNum = num;

                            // nullでなければアクション実行
                            if (_actions != null && _actions.Count > num && _actions[num] != null)
                                _actions[num].Invoke();
                        }
                    }
                });
            }
        }

        /// <summary>
        ///     対象の設定
        ///     Toggle切替時に、引数で渡されたVisualElementの子要素のEnabled状態も切り替える
        ///     Toggle自体がラジオボタン形式ではなく、チェックボックス形式（1つのみの指定）の場合に利用する
        ///     
        ///     引数1:設定対象(Toggle)
        ///     引数2:Toggle切り替え時に同時に切り替えるVisualElement 引数で渡されたVisualElement内の class="toggle_contents" が対象（渡されたもの自体は非対象）
        ///     引数2:最初にチェックが入る配列番号
        ///     引数3:各Toggleに設定する処理（切り替え時に実行する処理）
        /// </summary>
        public void SetToggleInVisualElementSelectorSingle(Toggle toggle, VisualElement element, bool defaultSelect = false, Action actions = null) {
            toggle.value = defaultSelect;

            if (defaultSelect)
            {
                //子要素をActiveにする
                foreach (var child in element.Children())
                    if (child.ClassListContains("toggle_contents"))
                        child.SetEnabled(true);
            }
            else
            {
                //子要素を非Activeにする
                foreach (var child in element.Children())
                    if (child.ClassListContains("toggle_contents"))
                        child.SetEnabled(false);
            }

            // nullでなければアクション実行
            if (actions != null && defaultSelect)
                actions.Invoke();

            toggle.RegisterValueChangedCallback(evt =>
            {
                // チェックが既に付いている場合は戻す
                if (evt.previousValue && evt.newValue == false)
                {
                    //元々選択状態だった子要素を非Activeにする
                    foreach (var child in element.Children())
                        if (child.ClassListContains("toggle_contents"))
                            child.SetEnabled(false);

                    // nullでなければアクション実行
                    if (actions != null)
                        actions.Invoke();
                }
                else if (evt.previousValue == false && evt.newValue)
                {
                    //新しく選択された子要素をActiveにする
                    foreach (var child in element.Children())
                        if (child.ClassListContains("toggle_contents"))
                            child.SetEnabled(true);

                    // nullでなければアクション実行
                    if (actions != null)
                        actions.Invoke();
                }
            });
        }

        /// <summary>
        ///     対象の設定
        ///     Toggle切替時に、引数で渡されたVisualElementの子要素のEnabled状態も切り替える
        ///     Toggle自体がラジオボタン形式ではなく、チェックボックス形式（1つのみの指定）の場合に利用する
        ///     
        ///     引数1:設定対象(Toggle)
        ///     引数2:Toggle切り替え時に同時に切り替えるVisualElement 引数で渡されたVisualElement内の class="toggle_contents" が対象（渡されたもの自体は非対象）
        ///     引数2:最初にチェックが入る配列番号
        ///     引数3:各Toggleに設定する処理（切り替え時に実行する処理）
        /// </summary>
        public void SetToggleInVisualElementSelectorSingle(Toggle toggle, List<VisualElement> element, bool defaultSelect = false, Action actions = null) {
            toggle.value = defaultSelect;

            if (defaultSelect)
            {
                //子要素をActiveにする
                foreach (var e in element)
                    foreach (var child in e.Children())
                        if (child.ClassListContains("toggle_contents"))
                            child.SetEnabled(true);
            }
            else
            {
                //子要素を非Activeにする
                foreach (var e in element)
                    foreach (var child in e.Children())
                        if (child.ClassListContains("toggle_contents"))
                            child.SetEnabled(false);
            }

            // nullでなければアクション実行
            if (actions != null && defaultSelect)
                actions.Invoke();

            toggle.RegisterValueChangedCallback(evt =>
            {
                // チェックが既に付いている場合は戻す
                if (evt.previousValue && evt.newValue == false)
                {
                    //元々選択状態だった子要素を非Activeにする
                    foreach (var e in element)
                        foreach (var child in e.Children())
                            if (child.ClassListContains("toggle_contents"))
                                child.SetEnabled(false);

                    // nullでなければアクション実行
                    if (actions != null)
                        actions.Invoke();
                }
                else if (evt.previousValue == false && evt.newValue)
                {
                    //新しく選択された子要素をActiveにする
                    foreach (var e in element)
                        foreach (var child in e.Children())
                            if (child.ClassListContains("toggle_contents"))
                                child.SetEnabled(true);

                    // nullでなければアクション実行
                    if (actions != null)
                        actions.Invoke();
                }
            });
        }
    }
}