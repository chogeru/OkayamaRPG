using System;
using UnityEngine;
using TextMP = TMPro.TextMeshProUGUI;

namespace RPGMaker.Codebase.Runtime.Map.Menu
{
    public class DebugToolGroupItem : MonoBehaviour
    {
        //今何の項目を使っているかを保持しておく
        private int                   _startIndex;
        private string                _menus;
        private TextMP                _name;

        private Action<DebugToolGroupItem> _clickedCallbackAction;

        public void Init(int startIndex, string label, string menus, ItemMenu itemMenu, Action<DebugToolGroupItem> clickedCallbackAction, Action<DebugToolGroupItem> selectedCallbackAction) {
            _startIndex = startIndex;
            _menus = menus;

            _name = transform.Find("Name").GetComponent<TextMP>();
            _name.text = label;

            var button = transform.GetComponent<DebugToolButton>();
            button.onClick.AddListener(ButtonEvent);
            button.OnSelected += (button) => selectedCallbackAction(this);

            _clickedCallbackAction = clickedCallbackAction;

        }

        public void ButtonEvent() {
            if (_clickedCallbackAction != null) _clickedCallbackAction(this);
        }

        public string GetMenus() {
            return _menus;
        }

        public int GetStartIndex() {
            return _startIndex;
        }
    }
}
