using UnityEditor;
using UnityEngine;
using static UnityEditor.GenericMenu;

namespace RPGMaker.Codebase.Editor.Common
{
    public static class EditorUtil
    {
        /// <summary>
        ///     第1引数isDisabledでAddItem(～)とAddDisabledItem(～)を呼び分ける、
        ///     GenericMenuクラスの拡張メソッド。
        /// </summary>
        public static void AddItem(
            this GenericMenu genericMenu,
            bool isDisabled,
            GUIContent content,
            bool on,
            MenuFunction func
        ) {
            if (isDisabled)
                // 選択不可アイテム。
                genericMenu.AddDisabledItem(content, @on);
            else
                // 選択可アイテム。
                genericMenu.AddItem(content, @on, func);
        }

        /// <summary>
        ///     第1引数isDisabledでAddItem(～)とAddDisabledItem(～)を呼び分ける、
        ///     GenericMenuクラスの拡張メソッドのuserData有り版。
        /// </summary>
        public static void AddItem(
            this GenericMenu genericMenu,
            bool isDisabled,
            GUIContent content,
            bool on,
            MenuFunction2 func,
            object userData
        ) {
            if (isDisabled)
                // 選択不可アイテム。
                genericMenu.AddDisabledItem(content, @on);
            else
                // 選択可アイテム。
                genericMenu.AddItem(content, @on, func, userData);
        }
    }
}