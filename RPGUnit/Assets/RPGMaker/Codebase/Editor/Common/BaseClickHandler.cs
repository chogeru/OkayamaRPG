using System;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Common
{
    public static class BaseClickHandler
    {
        public static void ClickEvent(VisualElement element, Action<int> callBack) {
            element.RegisterCallback<MouseUpEvent>(evt =>
            {
                evt.StopPropagation();
                if (evt.button != (int) MouseButton.RightMouse)
                {
#if !UNITY_EDITOR_WIN
                    if (!evt.ctrlKey)
#endif
                    {
                        callBack((int) MouseButton.LeftMouse);
                        return;
                    }
                }

                callBack((int) MouseButton.RightMouse);
            });

            element.RegisterCallback<MouseDownEvent>(evt =>
            {
                evt.StopPropagation();
                if (evt.button != (int) MouseButton.RightMouse)
                {
#if !UNITY_EDITOR_WIN
                    if (!evt.ctrlKey)
#endif
                    {
                        callBack((int) MouseButton.LeftMouse);
                        return;
                    }
                }

                callBack((int) MouseButton.RightMouse + 1);
            });
        }
    }
}