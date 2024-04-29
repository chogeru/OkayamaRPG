using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Common
{
    public static class UIElementsUtil
    {
        /// <summary>
        ///     横並びでチェックボックスがテキストの左に表示されるトグルのグループを追加する。
        /// </summary>
        /// <param name="ve">追加先</param>
        /// <param name="labelText">グループラベルテキスト</param>
        /// <param name="toggleTexts">各トグルのテキスト列</param>
        /// <param name="initialCheckedIndex">初期時にチェックされているトグルのインデックス</param>
        /// <param name="onClick">クリック時の動作</param>
        public static void AddToggleGroup(
            VisualElement ve,
            string labelText,
            List<string> toggleTexts,
            int initialCheckedToggleIndex,
            Action<int> onClick
        ) {
            ve.style.flexDirection = FlexDirection.Row;
            ve.style.height = 18;

            ve.Add(new Label(labelText));
            var toggles = new List<Toggle>();
            var togglesContainer = new VisualElement();
            togglesContainer.AddToClassList("multiple_item_in_row");
            foreach (var toggleText in toggleTexts)
            {
                var toggle = new Toggle {label = toggleText};
                togglesContainer.Add(toggle);

                var children = toggle.Children().ToArray();

                // ラベル。
                children[0].style.paddingLeft = 16;
                children[0].style.minWidth = new StyleLength(StyleKeyword.Auto);

                // チェックボックス。
                children[1].style.position = Position.Absolute;

                toggle.RegisterCallback<ClickEvent, int>(
                    (evt, clickToggleIndex) =>
                    {
                        foreach (var (toggle, index) in toggles.Select((item, index) => (item, index)))
                            toggle.value = index == clickToggleIndex;

                        onClick(clickToggleIndex);
                    },
                    toggles.Count);

                toggles.Add(toggle);
            }

            ve.Add(togglesContainer);

            toggles[initialCheckedToggleIndex].value = true;
        }

        /// <summary>
        ///     敵グループ専用
        ///     横並びでチェックボックスがテキストの左に表示されるトグルのグループを追加する。
        ///     押せないトグルを設定できる
        /// </summary>
        /// <param name="ve">追加先</param>
        /// <param name="labelText">グループラベルテキスト</param>
        /// <param name="toggleTexts">各トグルのテキスト列</param>
        /// <param name="initialCheckedIndex">初期時にチェックされているトグルのインデックス</param>
        /// <param name="EnableNum">押せないToggleのIndex</param>
        /// <param name="onClick">クリック時の動作</param>
        public static void AddToggleGroupEnable(
            VisualElement ve,
            string labelText,
            List<string> toggleTexts,
            int initialCheckedToggleIndex,
            List<int> EnableNum,
            Action<int> onClick
        ) {

            ve.Add(new Label(labelText));
            var toggles = new List<RadioButton>();
            var togglesContainer = new VisualElement();
            togglesContainer.AddToClassList("multiple_item_in_row");
            foreach (var toggleText in toggleTexts)
            {
                var toggle = new RadioButton {label = toggleText};
                toggle.AddToClassList("toggle_block");
                togglesContainer.Add(toggle);

                var children = toggle.Children().ToArray();

                // ラベル。
                children[0].style.minWidth = new StyleLength(StyleKeyword.Auto);
                children[0].AddToClassList("toggle_label");

                // チェックボックス。
                children[1].style.position = Position.Absolute;

                toggle.RegisterCallback<ClickEvent, int>(
                    (evt, clickToggleIndex) =>
                    {
                        foreach (var (toggle, index) in toggles.Select((item, index) => (item, index)))
                            toggle.value = index == clickToggleIndex;

                        onClick(clickToggleIndex);
                    },
                    toggles.Count);

                toggles.Add(toggle);
            }

            ve.Add(togglesContainer);

            toggles[initialCheckedToggleIndex].value = true;

            //押せないToggleを押せなくする
            foreach (var num in EnableNum)
                toggles[num].SetEnabled(false);
        }
    }
}