using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Enum;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using TextMP = TMPro.TextMeshProUGUI;

namespace RPGMaker.Codebase.Runtime.Map.Menu
{
    public class DebugToolValueItem : MonoBehaviour {
        //今何の項目を使っているかを保持しておく
        private int _index;
        private string _menus;
        private TextMP _name;
        private List<TextMP> _names = new List<TextMP>();
        private List<bool> _switchData;
        private List<string> _variableData;
        private List<RuntimeSaveDataModel.SaveDataSelfSwitchesData> _selfSwitchData;
        private string _eventId;
        private System.Action<DebugToolValueItem> _callbackAction;
        private bool _pressed = false;

        public class ButtonEventCaller
        {
            DebugToolValueItem _item;
            int _ssIndex;
            public ButtonEventCaller(DebugToolValueItem item, int ssIndex) {
                _item = item;
                _ssIndex = ssIndex;
            }

            public void Call() {
                _item.ButtonEvent(_ssIndex);
            }
        }
        public void Init(int index, string label, object data, string menus, ItemMenu itemMenu, System.Action<DebugToolValueItem> callbackAction, System.Action<DebugToolValueItem> selectedCallbackAction, string evId = null) {
            _index = index;

            _menus = menus;

            _name = transform.Find("Name").GetComponent<TextMP>();
            _name.text = label;
            if (menus == "switch" || menus == "variable")
            {
                _names.Add(transform.Find("Name2").GetComponent<TextMP>());
                var button = transform.GetComponent<DebugToolButton>();
                button.onClick.AddListener(() => ButtonEvent());
                button.OnSelected += (button) => selectedCallbackAction(this);
            }
            else if (menus == "selfSwitch")
            {
                for (int i = 0; i < 4; i++)
                {
                    var ssTrans = transform.Find($"SelfSwitchLayout/SelfSwitch{i + 1}");
                    _names.Add(ssTrans.Find("Name").GetComponent<TextMP>());
                    var button = ssTrans.GetComponent<DebugToolButton>();
                    button.onClick.AddListener(new ButtonEventCaller(this, i).Call);
                    button.OnSelected += (button) => selectedCallbackAction(this);
                }
            }


            switch (menus)
            {
                case "switch":
                    //_itemDataModel = DataManager.Self().GetItemDataModels()
                    //.FirstOrDefault(t => t.basic.id == itemId);
                    //_name.text = _itemDataModel.basic.name;
                    //if (_gameItem == null) _gameItem = new GameItem(itemId, GameItem.DataClassEnum.Item);
                    _switchData = (List<bool>) data;
                    break;

                case "variable":
                    _variableData = (List<string>) data;
                    break;

                case "selfSwitch":
                    _selfSwitchData = (List<RuntimeSaveDataModel.SaveDataSelfSwitchesData>) data;
                    _eventId = evId;
                    break;
            }
            UpdateNames();

            _callbackAction = callbackAction;

        }

        string[] _selfSwitchNames = new string[] { "A", "B", "C", "D" };
        void UpdateNames() {
            switch (_menus)
            {
                case "switch":
                    _names[0].text = $"[{(_switchData[_index] ? "ON" : "OFF")}]";
                    break;

                case "variable":
                    _names[0].text = $"{_variableData[_index]}";
                    break;

                case "selfSwitch":
                    var selfSwitches = _selfSwitchData.FirstOrDefault(x => x.id == _eventId)?.data;
                    for (int i = 0; i < 4; i++)
                    {
                        _names[i].text = $"{_selfSwitchNames[i]}: {(selfSwitches != null && selfSwitches[i] ? "ON" : "OFF")}";
                    }
                    break;
            }
        }

        const float RepeatStartTime = 0.5f;
        const float RepeatInterval = 0.1f;
        const int MinusIndex = 0;
        const int PlusIndex = 1;
        const int BigMinusIndex = 2;
        const int BigPlusIndex = 3;
        const int MinusBit = (1 << 0);
        const int PlusBit = (1 << 1);
        const int BigMinusBit = (1 << 2);
        const int BigPlusBit = (1 << 3);
        int _pressedBits = 0;
        float[] _repeatCountdownArr = new float[] { 0, 0, 0, 0 };
        private void Update() {
            if (_menus == "switch" && EventSystem.current.currentSelectedGameObject == gameObject)
            {
                if (Input.GetKey(KeyCode.KeypadEnter))
                {
                    if (!_pressed)
                    {
                        _pressed = true;
                        ButtonEvent();
                    }
                } else
                {
                    _pressed = false;
                }
                return;
            }
            if (_menus == "selfSwitch")
            {
                if (Input.GetKey(KeyCode.KeypadEnter))
                {
                    if (!_pressed)
                    {
                        _pressed = true;
                        for (int i = 0; i < 4; i++)
                        {
                            var ssTrans = transform.Find($"SelfSwitchLayout/SelfSwitch{i + 1}");
                            if (EventSystem.current.currentSelectedGameObject == ssTrans.gameObject)
                            {
                                ButtonEvent(i);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    _pressed = false;
                }
                return;
            }

            if (_menus != "variable" || EventSystem.current.currentSelectedGameObject != gameObject) return;
            var value = int.Parse(_variableData[_index]);
            int inputBits = 0;
            if (InputHandler.OnDown(HandleType.Left) || Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus)) inputBits |= MinusBit;
            if (InputHandler.OnDown(HandleType.Right) || Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.KeypadPlus)) inputBits |= PlusBit;
            if (Input.GetKey(KeyCode.PageUp)) inputBits |= BigMinusBit;
            if (Input.GetKey(KeyCode.PageDown)) inputBits |= BigPlusBit;

            if (inputBits == 0 && _pressedBits == 0) return;

            for (int i = 0; i < 4; i++)
            {
                var bit = (1 << i);
                if ((inputBits & bit) != 0)
                {
                    if ((_pressedBits & bit) != 0)
                    {
                        var countdown = _repeatCountdownArr[i] - Time.deltaTime;
                        if (countdown <= 0)
                        {
                            _repeatCountdownArr[i] = RepeatInterval;
                        } else
                        {
                            _repeatCountdownArr[i] = countdown;
                            inputBits &= ~bit;
                        }
                    } else
                    {
                        _pressedBits |= bit;
                        _repeatCountdownArr[i] = RepeatStartTime;
                    }
                }
                else
                {
                    _pressedBits &= ~bit;
                }
            }
            if (inputBits == 0) return;

            if ((inputBits & MinusBit) != 0){
                if (value > -0x7fffffff)
                {
                    value--;
                }
            } else if ((inputBits & PlusBit) != 0)
            {
                if (value < 0x7fffffff)
                {
                    value++;
                }
            } else if ((inputBits & BigMinusBit) != 0)
            {
                if (value >= -0x7fffffff + 10)
                {
                    value -= 10;
                }
                else
                {
                    value = -0x7fffffff;
                }
            }
            else if ((inputBits & BigPlusBit) != 0)
            {
                if (value <= 0x7fffffff - 10)
                {
                    value += 10;
                }
                else
                {
                    value = 0x7fffffff;
                }
            }
            if (value.ToString() != _variableData[_index])
            {
                _variableData[_index] = value.ToString();
                UpdateNames();
            }
        }

        public void ButtonEvent(int ssIndex = 0) {
            switch (_menus)
            {
                case "switch":
                    _switchData[_index] = !_switchData[_index];
                    break;

                case "variable":
                    break;

                case "selfSwitch":
                    var swData = _selfSwitchData.Find(x => x.id == _eventId);
                    if (swData == null)
                    {
                        swData = new RuntimeSaveDataModel.SaveDataSelfSwitchesData();
                        swData.id = _eventId;
                        swData.data = new List<bool>() { false, false, false, false };
                        _selfSwitchData.Add(swData);
                    }
                    swData.data[ssIndex] = !swData.data[ssIndex];
                    break;
            }
            UpdateNames();

            if (_callbackAction != null) _callbackAction(this);
        }

    }
}
