using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Map.Menu
{
    public class DebugToolButton : Button, ISelectHandler
    {
        public static bool blockEvent = false;

        public event Action<DebugToolButton> OnSelected;
        bool _selected = false;
        public bool IsSelected { get { return _selected; } }

        public override void OnSelect(BaseEventData eventData) {
            _selected = true;
            if (blockEvent) return;
            OnSelected?.Invoke(this);
        }

        public override void OnDeselect(BaseEventData eventData) {
            _selected = false;
        }

    }
}
