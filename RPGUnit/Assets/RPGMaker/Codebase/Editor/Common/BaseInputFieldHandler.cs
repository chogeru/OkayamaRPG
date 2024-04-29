using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Common
{
    public static class BaseInputFieldHandler
    {
        public static void FloatFieldCallback(
            FloatField element,
            Action<FocusOutEvent> callBack,
            float min,
            float max,
            int roundNum
        ) {
            element.RegisterCallback<FocusOutEvent>(o =>
            {
                if (element.value < min) element.value = min;
                if (element.value > max) element.value = max;
                element.value = (float) Math.Round(element.value, roundNum);
                callBack(o);
            });
        }

        public static void IntegerFieldCallback(
            IntegerField element,
            Action<FocusOutEvent> callBack,
            int min,
            int max
        ) {
            element.RegisterCallback<FocusOutEvent>(o =>
            {
                if (element.value < min) element.value = min;
                //最低値と最高値が同じだったら最高値は無し
                if (min != max)
                    if (element.value > max)
                        element.value = max;
                callBack(o);
            });
        }
        
        public static void FloatFieldCallback(
            FloatField element,
            Action<FocusOutEvent> callBack,
            float min,
            float max
        ) {
            element.RegisterCallback<FocusOutEvent>(o =>
            {
                if (element.value < min) element.value = min;
                //最低値と最高値が同じだったら最高値は無し
                if (min != max)
                    if (element.value > max)
                        element.value = max;

                element.value = (float)Math.Round(element.value, 1, MidpointRounding.AwayFromZero);
                callBack(o);
            });
        }
    }
}