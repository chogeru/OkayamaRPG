using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.AddonUIUtil;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Addons;
using RPGMaker.Codebase.Runtime.Addon;
using SimpleJSON;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow
{
    public class AddonParameterEditModalWindow : AddonBaseModalWindow
    {
        private readonly Vector2Int                WINDOW_SIZE = new Vector2Int(740, 480);
        private          AddonInfo                 _addonInfo;
        private          int                       _arrayDimension;
        private readonly DatabaseManagementService _databaseManagementService = new DatabaseManagementService();

        private readonly EventManagementService _eventManagementService = new EventManagementService();

        private readonly Dictionary<string, string[]> _fileFolderDic = new Dictionary<string, string[]>
        {
            {
                "Images",
                new[]
                {
                    "Background/Battle/01", "Background/Battle/02", "Characters", "Enemy", "Faces",
                    "Objects", "Parallaxes", "Pictures", "SV_Actors", "System", "System/Balloon",
                    "System/Damage", "System/IconSet", "System/Status", "System/Weapon",
                    "Titles1", "Titles2"
                }
            },
            {"Sounds", new[] {"BGM", "BGS", "ME", "SE"}},
            {"Map", new[] { "BackgroundImages"} }
        };

        private          string               _lastValue;
        private readonly MapManagementService _mapManagementService = new MapManagementService();
        private          bool                 _modified;


        private AddonParameterContainer _parameters;
        private AddonParameterContainer _parametersOrg;
        private AddonParamInfo          _paramInfo;
        private ParamType               _paramType;
        private string                  _structName;
        private TabbedMenuController    _tabController;

        private VisualElement bottomWindow;

        protected override string ModalUxml =>
            "Assets/RPGMaker/Codebase/Editor/DatabaseEditor/ModalWindow/Uxml/addon_parameter_edit_modalwindow.uxml";

        protected override string ModalUss => "";

        public void SetInfo(
            AddonInfo addonInfo,
            AddonParamInfo paramInfo,
            AddonParameterContainer parameters,
            bool modified
        ) {
            _parametersOrg = parameters;
            _parameters =
                new AddonParameterContainer(
                    JsonHelper.FromJsonArray<AddonParameter>(JsonHelper.ToJsonArray(parameters)));

            _addonInfo = addonInfo;
            _paramInfo = paramInfo;
            _modified = modified;
            var info = _paramInfo.GetInfo("type");
            var typeName = info != null ? info.value : "string";
            _paramType = AddonManager.GetParamType(typeName, out _arrayDimension, out _structName);
            //Debug.Log($"_paramType: {_paramType.ToString()}");
        }

        private void OnDestroy() {
            AddonEditorWindowManager.instance.CloseDescendantWindows(this);
            AddonEditorWindowManager.instance.UnregisterWindow(this);
        }

        public override void ShowWindow(string modalTitle, CallBackWidow callBack) {
            var wnd = this; //GetWindow<AddonParameterEditModalWindow>();
            AddonEditorWindowManager.instance.RegisterParameterEditWindow(wnd);

            if (callBack != null) _callBackWindow = callBack;

            wnd.titleContent = new GUIContent(EditorLocalize.LocalizeText("WORD_2506"));
            wnd.Init();
            Vector2 size = WINDOW_SIZE;
            wnd.minSize = size;
            wnd.maxSize = size;
            wnd.maximized = false;
            wnd.Show();
        }

        public static string GetTypeWord(ParamType paramType) {
            var typeWordId = "WORD_2507";
            switch (paramType)
            {
                case ParamType.Integer:
                    typeWordId = "WORD_2515";
                    break;
                case ParamType.Number:
                    typeWordId = "WORD_2507";
                    break;
                case ParamType.Boolean:
                    typeWordId = "WORD_2516";
                    break;
                case ParamType.String:
                case ParamType.MultilineString:
                    typeWordId = "WORD_2508";
                    break;
                case ParamType.Select:
                    typeWordId = "WORD_2514";
                    break;
                case ParamType.Combo:
                    typeWordId = "WORD_2571";
                    break;
                case ParamType.Note:
                    typeWordId = "WORD_2517";
                    break;
                case ParamType.Struct:
                    typeWordId = "WORD_2522";
                    break;
                case ParamType.CommonEvent:
                    typeWordId = "WORD_2523";
                    break;
                case ParamType.MapEvent:
                    typeWordId = "WORD_2524";
                    break;
                case ParamType.Switch:
                    typeWordId = "WORD_0605";
                    break;
                case ParamType.Variable:
                    typeWordId = "WORD_0839";
                    break;
                case ParamType.Animation:
                    typeWordId = "WORD_2525";
                    break;
                case ParamType.Actor:
                    typeWordId = "WORD_2526";
                    break;
                case ParamType.Class:
                    typeWordId = "WORD_2567";
                    break;
                case ParamType.Skill:
                    typeWordId = "WORD_0069";
                    break;
                case ParamType.Item:
                    typeWordId = "WORD_0068";
                    break;
                case ParamType.Weapon:
                    typeWordId = "WORD_2568";
                    break;
                case ParamType.Armor:
                    typeWordId = "WORD_2569";
                    break;
                case ParamType.Enemy:
                    typeWordId = "WORD_0559";
                    break;
                case ParamType.Troop:
                    typeWordId = "WORD_0564";
                    break;
                case ParamType.State:
                    typeWordId = "WORD_0602";
                    break;
                case ParamType.Tileset:
                    typeWordId = "WORD_0734";
                    break;
                case ParamType.File:
                    typeWordId = "WORD_2570";
                    break;
            }

            return typeWordId;
        }

        void ForceSetIndex(PopupFieldBase<string> popupField, int index) {
            popupField.ForceSetIndex(index);
            var type = popupField.GetType();
            var strWorkFi = type.GetField("strWork", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var buttonFi = type.GetField("button", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (strWorkFi != null && buttonFi != null)
            {
                strWorkFi.SetValue(popupField, ((Button) buttonFi.GetValue(popupField)).text);
            }
        }

        public override void Init() {
            var root = rootVisualElement;

            // 要素作成
            //----------------------------------------------------------------------
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModalUxml);
            VisualElement labelFromUxml = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUxml);
            root.Add(labelFromUxml);

            var topWindow = labelFromUxml.Query<VisualElement>("system_window_topwindow").AtIndex(0);
            _tabController = new TabbedMenuController(topWindow);
            //Debug.Log($"_paramType: {_paramType.ToString()}");
            var singleTab = _paramType != ParamType.Integer && _paramType != ParamType.Number &&
                            _paramType != ParamType.Boolean && _paramType != ParamType.Select &&
                            _paramType != ParamType.Combo && _paramType != ParamType.Note
                            && _paramType != ParamType.CommonEvent && _paramType != ParamType.MapEvent &&
                            _paramType != ParamType.Switch && _paramType != ParamType.Variable
                            && _paramType != ParamType.Animation && _paramType != ParamType.Actor
                            && _paramType != ParamType.Class && _paramType != ParamType.Skill &&
                            _paramType != ParamType.Item && _paramType != ParamType.Weapon &&
                            _paramType != ParamType.Armor
                            && _paramType != ParamType.Enemy && _paramType != ParamType.Troop &&
                            _paramType != ParamType.State && _paramType != ParamType.Tileset &&
                            _paramType != ParamType.File;
            if (!singleTab) _tabController.RegisterTabCallbacks();

            var typeLabel = labelFromUxml.Query<VisualElement>("TypeTab").AtIndex(0) as Label;
            var typeWordId = GetTypeWord(_paramType);
            typeLabel.text = EditorLocalize.LocalizeText(typeWordId);
            var textLabel = labelFromUxml.Query<VisualElement>("TextTab").AtIndex(0) as Label;
            textLabel.text = EditorLocalize.LocalizeText("WORD_2508");

            if (singleTab)
            {
                typeLabel.style.display = DisplayStyle.None;
                var typeContent = labelFromUxml.Query<VisualElement>("TypeContent").AtIndex(0);
                typeContent.style.display = DisplayStyle.None;
                var textContent = labelFromUxml.Query<VisualElement>("TextContent").AtIndex(0);
                textContent.style.display = DisplayStyle.Flex;
            }

            IntegerField integerField = null;
            DoubleField doubleField = null;
            PopupFieldBase<string> popupField = null;
            ImTextField textField = null;
            ImScrollTextField scrollTextField = null;
            Toggle toggle1 = null;
            Toggle toggle2 = null;
            var value = AddonManager.GetInitialValue(_parameters, _paramInfo);
            value = AddonManager.ValidateValue(_paramInfo, _paramType, value);
            if (_paramType == ParamType.MapEvent)
            {
                var arr = DataConverter.GetStringArrayFromJson(value);
                if (arr[0] == null || arr[0].Length == 0) arr[0] = _parameters.GetParameterValue(AddonCommand.mapIdKey);
                value = DataConverter.GetJsonStringArray(arr);
            }

            _lastValue = value;
            var toggleChanging = false;
            var multiline = false;
            var mapContainer = new VisualElement();
            var eventContainer = new VisualElement();
            var comboContainer = new VisualElement();
            var fileContainer = new VisualElement();
            var switchNameList = new List<string>();
            var variableNameList = new List<string>();
            var animationNameList = new List<string>();
            var actorNameList = new List<string>();
            var classNameList = new List<string>();
            var skillNameList = new List<string>();
            var itemNameList = new List<string>();
            var weaponNameList = new List<string>();
            var armorNameList = new List<string>();
            var enemyNameList = new List<string>();
            var troopNameList = new List<string>();
            var stateNameList = new List<string>();
            var tilesetNameList = new List<string>();
            for (var i = 0; i < 2; i++)
            {
                var typeText = i == 0 ? "type" : "text";
                var label_window = labelFromUxml.Query<VisualElement>($"system_window_{typeText}_label_window")
                    .AtIndex(0);
                var textParam = _paramInfo.GetInfo("text");
                var text = textParam != null ? $"{textParam.value}({_paramInfo.name}):" : _paramInfo.name;
                var label = new Label(text);
                label.style.flexGrow = 1;
                label_window.Add(label);

                var value_window = labelFromUxml.Query<VisualElement>($"system_window_{typeText}_value_window")
                    .AtIndex(0);
                //Debug.Log($"typeText: {typeText}");
                if (i == 0)
                {
                    if (_paramType == ParamType.Integer)
                    {
                        integerField = new IntegerField();
                        integerField.value = int.Parse(value);
                        integerField.style.flexGrow = 1;
                        /*integerField.RegisterValueChangedCallback((e) =>
                        {
                            Debug.Log($"integerField.valueChanged: {integerField.value}");
                        });*/
                        integerField.RegisterCallback<FocusOutEvent>(e =>
                        {
                            //Debug.Log($"integerField.FocusOutEvent");
                            var newValue = int.Parse(AddonManager.ValidateValue(_paramInfo, _paramType,
                                integerField.value.ToString()));
                            var newValueStr = newValue.ToString();
                            if (newValue != integerField.value) integerField.value = newValue;
                            if (newValueStr != _lastValue)
                            {
                                _modified = true;
                                _lastValue = newValueStr;
                                textField.value = newValueStr;
                                _parameters.SetParameterValue(_paramInfo.name, newValueStr);
                                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                            }
                        });
                        value_window.Add(integerField);
                        SetDelayedAction(() => { integerField.Focus(); });
                    }
                    else if (_paramType == ParamType.Number)
                    {
                        doubleField = new DoubleField();
                        //Debug.Log($"value: {value}");
                        doubleField.value = double.Parse(value);
                        doubleField.style.flexGrow = 1;
                        /*doubleField.RegisterValueChangedCallback((e) =>
                        {
                            Debug.Log($"doubleField.valueChanged: {doubleField.value}");
                        });*/
                        doubleField.RegisterCallback<FocusOutEvent>(e =>
                        {
                            //Debug.Log($"doubleField.FocusOutEvent");
                            var newValue = double.Parse(AddonManager.ValidateValue(_paramInfo, _paramType,
                                doubleField.value.ToString()));
                            var newValueStr = newValue.ToString();
                            if (newValue != doubleField.value) doubleField.value = newValue;
                            if (newValueStr != _lastValue)
                            {
                                _modified = true;
                                _lastValue = newValueStr;
                                textField.value = newValueStr;
                                _parameters.SetParameterValue(_paramInfo.name, newValueStr);
                                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                            }
                        });
                        value_window.Add(doubleField);
                        SetDelayedAction(() => { doubleField.Focus(); });
                    }
                    else if (_paramType == ParamType.Select)
                    {
                        var nameList = new List<string>();
                        var defaultIndex = 0;
                        var index = 0;
                        foreach (var option in _paramInfo.options)
                        {
                            nameList.Add(option.key);
                            if (option.value == value) defaultIndex = index;
                            index++;
                        }

                        if (nameList.Count == 0) nameList.Add("");
                        popupField = new PopupFieldBase<string>(nameList, defaultIndex);
                        popupField.RegisterValueChangedCallback(evt =>
                        {
                            var popupField = evt.currentTarget as PopupFieldBase<string>;
                            //Debug.Log($"popupField: {popupField.index}, {popupField.value}");
                            if (_paramInfo.options.Count == 0) return;
                            var newValue = _paramInfo.options[popupField.index].value;
                            if (newValue != _lastValue)
                            {
                                _modified = true;
                                _lastValue = newValue;
                                textField.value = AddonUIUtil.GetEasyReadableText(_addonInfo, _paramInfo,
                                    AddonManager.GetTypeName(_paramType), newValue, "@value");
                                _parameters.SetParameterValue(_paramInfo.name, newValue);
                                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                            }
                        });
                        popupField.style.flexGrow = 1;
                        AddonUIUtil.AddStyleEllipsisToPopupField(popupField);
                        value_window.Add(popupField);
                        SetDelayedAction(() => { popupField.Focus(); });
                    }
                    else if (_paramType == ParamType.Combo)
                    {
                        comboContainer.AddToClassList("combo_container");
                        value_window.Add(comboContainer);
                        UpdateCombo(comboContainer, () => { return textField; }, value);
                    }
                    else if (_paramType == ParamType.Boolean)
                    {
                        toggle1 = new Toggle();
                        toggle1.value = bool.Parse(value);
                        var onInfo = _paramInfo.GetInfo("on");
                        var onLabel = onInfo != null ? onInfo.value : "ON";
                        toggle1.label = $"{onLabel}(true)";
                        toggle1.AddToClassList("list_view_item_toggle");
                        toggle1.RegisterValueChangedCallback(e =>
                        {
                            if (toggleChanging) return;
                            var toggle = e.currentTarget as Toggle;
                            //Debug.Log($"toggle1.value: {toggle1.value}, {toggle.value}, {toggle2.value}");
                            if (!toggle!.value && !toggle2.value)
                            {
                                toggle1.value = true;
                                return;
                            }

                            if (toggle1.value && !toggle2.value) return;
                            SetDelayedAction(() =>
                            {
                                toggleChanging = true;
                                toggle2.value = !toggle1.value;
                                _modified = true;
                                textField.value = _lastValue = toggle1.value ? "true" : "false";
                                _parameters.SetParameterValue(_paramInfo.name, _lastValue);
                                toggleChanging = false;
                                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                            });
                        });
                        var spaceVe = new VisualElement();
                        spaceVe.style.width = 64;
                        toggle2 = new Toggle();
                        toggle2.value = !bool.Parse(value);
                        var offInfo = _paramInfo.GetInfo("off");
                        var offLabel = offInfo != null ? offInfo.value : "OFF";
                        toggle2.label = $"{offLabel}(false)";
                        toggle2.AddToClassList("list_view_item_toggle");
                        toggle2.RegisterValueChangedCallback(e =>
                        {
                            if (toggleChanging) return;
                            var toggle = e.currentTarget as Toggle;
                            //Debug.Log($"toggle2.value: {toggle2.value}, {toggle.value}, {toggle1.value}");
                            if (!toggle!.value && !toggle2.value)
                            {
                                toggle2.value = true;
                                return;
                            }

                            if (!toggle1.value && toggle2.value) return;
                            SetDelayedAction(() =>
                            {
                                toggleChanging = true;
                                toggle1.value = !toggle2.value;
                                _modified = true;
                                textField.value = _lastValue = toggle1.value ? "true" : "false";
                                _parameters.SetParameterValue(_paramInfo.name, _lastValue);
                                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                                toggleChanging = false;
                            });
                        });
                        value_window.style.alignItems = Align.Stretch;
                        value_window.style.flexDirection = FlexDirection.Row;
                        //value_window.style.justifyContent = Justify.FlexEnd;
                        value_window.Add(toggle1);
                        value_window.Add(spaceVe);
                        value_window.Add(toggle2);
                        if (toggle1.value)
                            SetDelayedAction(() => { toggle1.Focus(); });
                        else if (toggle2.value)
                            SetDelayedAction(() => { toggle2.Focus(); });
                    }
                    else if (_paramType == ParamType.Note)
                    {
                        multiline = true;
                        scrollTextField = new ImScrollTextField(ScrollViewMode.VerticalAndHorizontal);
                        scrollTextField.textField.value = DataConverter.GetStringFromJson(value);
                        scrollTextField.textField.RegisterCallback<FocusOutEvent>(e =>
                        {
                            //Debug.Log($"scrollTextField.textField.FocusOutEvent");
                            var newValue = DataConverter.GetJsonString(scrollTextField.textField.value);
                            if (newValue != _lastValue)
                            {
                                _modified = true;
                                _lastValue = newValue;
                                textField.value = newValue; // DataConverter.GetJsonString(newValue);
                                _parameters.SetParameterValue(_paramInfo.name, newValue);
                                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                            }
                        });
                        value_window.Add(scrollTextField);
                        SetDelayedAction(() => { scrollTextField.Focus(); });
                    }
                    else if (_paramType == ParamType.CommonEvent)
                    {
                        eventContainer = new VisualElement();
                        eventContainer.style.flexGrow = 1;
                        value_window.Add(eventContainer);
                        UpdateEvent(eventContainer, () => { return textField; }, value);
                    }
                    else if (_paramType == ParamType.MapEvent)
                    {
                        value_window.AddToClassList("align_row");
                        mapContainer = new VisualElement();
                        mapContainer.style.flexGrow = 1;
                        value_window.Add(mapContainer);
                        eventContainer = new VisualElement();
                        eventContainer.style.flexGrow = 1;
                        value_window.Add(eventContainer);

                        var valueArray = DataConverter.GetStringArrayFromJson(_lastValue);
                        var jsonArray = JSON.Parse(_lastValue).AsArray;
                        // Map
                        UpdateMap(mapContainer, eventContainer, () => { return textField; }, jsonArray[0].Value);

                        // Event
                        UpdateMapEvent(eventContainer, () => { return textField; }, valueArray[0], jsonArray[1].Value);
                    }
                    else if (_paramType == ParamType.Switch)
                    {
                        var flags = _databaseManagementService.LoadFlags();
                        var defaultIndex = 0;
                        var index = 0;
                        switchNameList.Add(EditorLocalize.LocalizeText("WORD_0113"));
                        foreach (var sw in flags.switches)
                        {
                            switchNameList.Add($"{sw.SerialNumberString} {sw.name}");
                            if (sw.id == value) defaultIndex = index + 1;
                            index++;
                        }

                        popupField = new PopupFieldBase<string>(switchNameList, defaultIndex);
                        popupField.RegisterValueChangedCallback(evt =>
                        {
                            var popupField = evt.currentTarget as PopupFieldBase<string>;
                            //Debug.Log($"popupField: {popupField.index}, {popupField.value}");
                            var newValue = popupField.index > 0 ? flags.switches[popupField.index - 1].id : "";
                            if (newValue != _lastValue)
                            {
                                _modified = true;
                                _lastValue = newValue;
                                textField.value = newValue;
                                _parameters.SetParameterValue(_paramInfo.name, newValue);
                                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                            }
                        });
                        popupField.style.flexGrow = 1;
                        AddonUIUtil.AddStyleEllipsisToPopupField(popupField);
                        value_window.Add(popupField);
                        SetDelayedAction(() => { popupField.Focus(); });
                    }
                    else if (_paramType == ParamType.Variable)
                    {
                        var flags = _databaseManagementService.LoadFlags();
                        var defaultIndex = 0;
                        var index = 0;
                        variableNameList.Add(EditorLocalize.LocalizeText("WORD_0113"));
                        foreach (var v in flags.variables)
                        {
                            variableNameList.Add($"{v.SerialNumberString} {v.name}");
                            if (v.id == value) defaultIndex = index + 1;
                            index++;
                        }

                        popupField = new PopupFieldBase<string>(variableNameList, defaultIndex);
                        popupField.RegisterValueChangedCallback(evt =>
                        {
                            var popupField = evt.currentTarget as PopupFieldBase<string>;
                            //Debug.Log($"popupField: {popupField.index}, {popupField.value}");
                            var newValue = popupField.index > 0 ? flags.variables[popupField.index - 1].id : "";
                            if (newValue != _lastValue)
                            {
                                _modified = true;
                                _lastValue = newValue;
                                textField.value = newValue;
                                _parameters.SetParameterValue(_paramInfo.name, newValue);
                                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                            }
                        });
                        popupField.style.flexGrow = 1;
                        AddonUIUtil.AddStyleEllipsisToPopupField(popupField);
                        value_window.Add(popupField);
                        SetDelayedAction(() => { popupField.Focus(); });
                    }
                    else if (_paramType == ParamType.Animation)
                    {
                        var animations = _databaseManagementService.LoadAnimation();
                        var defaultIndex = 0;
                        var index = 0;
                        foreach (var animation in animations)
                        {
                            animationNameList.Add($"{animation.SerialNumberString} {animation.particleName}");
                            if (animation.id == value) defaultIndex = index;
                            index++;
                        }

                        popupField = new PopupFieldBase<string>(animationNameList, defaultIndex);
                        popupField.RegisterValueChangedCallback(evt =>
                        {
                            var popupField = evt.currentTarget as PopupFieldBase<string>;
                            var newValue = animations[popupField.index].id;
                            if (newValue != _lastValue)
                            {
                                _modified = true;
                                _lastValue = newValue;
                                textField.value = newValue;
                                _parameters.SetParameterValue(_paramInfo.name, newValue);
                                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                            }
                        });
                        popupField.style.flexGrow = 1;
                        AddonUIUtil.AddStyleEllipsisToPopupField(popupField);
                        value_window.Add(popupField);
                        SetDelayedAction(() => { popupField.Focus(); });
                    }
                    else if (_paramType == ParamType.Actor)
                    {
                        var actors = _databaseManagementService.LoadCharacterActor();
                        var defaultIndex = 0;
                        var index = 0;
                        actorNameList.Add(EditorLocalize.LocalizeText("WORD_0113"));
                        foreach (var actor in actors)
                        {
                            actorNameList.Add($"{actor.SerialNumberString} {actor.basic.name}");
                            if (actor.uuId == value) defaultIndex = index + 1;
                            index++;
                        }

                        popupField = new PopupFieldBase<string>(actorNameList, defaultIndex);
                        popupField.RegisterValueChangedCallback(evt =>
                        {
                            var popupField = evt.currentTarget as PopupFieldBase<string>;
                            var newValue = popupField.index > 0 ? actors[popupField.index - 1].uuId : "";
                            if (newValue != _lastValue)
                            {
                                _modified = true;
                                _lastValue = newValue;
                                textField.value = newValue;
                                _parameters.SetParameterValue(_paramInfo.name, newValue);
                                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                            }
                        });
                        popupField.style.flexGrow = 1;
                        AddonUIUtil.AddStyleEllipsisToPopupField(popupField);
                        value_window.Add(popupField);
                        SetDelayedAction(() => { popupField.Focus(); });
                    }
                    else if (_paramType == ParamType.Class)
                    {
                        var classes = _databaseManagementService.LoadCharacterActorClass();
                        var defaultIndex = 0;
                        var index = 0;
                        classNameList.Add(EditorLocalize.LocalizeText("WORD_0113"));
                        foreach (var class_ in classes)
                        {
                            classNameList.Add($"{class_.SerialNumberString} {class_.basic.name}");
                            if (class_.id == value) defaultIndex = index + 1;
                            index++;
                        }

                        popupField = new PopupFieldBase<string>(classNameList, defaultIndex);
                        popupField.RegisterValueChangedCallback(evt =>
                        {
                            var popupField = evt.currentTarget as PopupFieldBase<string>;
                            var newValue = popupField.index > 0 ? classes[popupField.index - 1].id : "";
                            if (newValue != _lastValue)
                            {
                                _modified = true;
                                _lastValue = newValue;
                                textField.value = newValue;
                                _parameters.SetParameterValue(_paramInfo.name, newValue);
                                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                            }
                        });
                        popupField.style.flexGrow = 1;
                        AddonUIUtil.AddStyleEllipsisToPopupField(popupField);
                        value_window.Add(popupField);
                        SetDelayedAction(() => { popupField.Focus(); });
                    }
                    else if (_paramType == ParamType.Skill)
                    {
                        var skills = _databaseManagementService.LoadSkillCustom();
                        var defaultIndex = 0;
                        var index = 0;
                        skillNameList.Add(EditorLocalize.LocalizeText("WORD_0113"));
                        foreach (var skill in skills)
                        {
                            skillNameList.Add($"{skill.SerialNumberString} {skill.basic.name}");
                            if (skill.basic.id == value) defaultIndex = index + 1;
                            index++;
                        }

                        popupField = new PopupFieldBase<string>(skillNameList, defaultIndex);
                        popupField.RegisterValueChangedCallback(evt =>
                        {
                            var popupField = evt.currentTarget as PopupFieldBase<string>;
                            var newValue = popupField.index > 0 ? skills[popupField.index - 1].basic.id : "";
                            if (newValue != _lastValue)
                            {
                                _modified = true;
                                _lastValue = newValue;
                                textField.value = newValue;
                                _parameters.SetParameterValue(_paramInfo.name, newValue);
                                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                            }
                        });
                        popupField.style.flexGrow = 1;
                        AddonUIUtil.AddStyleEllipsisToPopupField(popupField);
                        value_window.Add(popupField);
                        SetDelayedAction(() => { popupField.Focus(); });
                    }
                    else if (_paramType == ParamType.Item)
                    {
                        var items = _databaseManagementService.LoadItem();
                        var defaultIndex = 0;
                        var index = 0;
                        itemNameList.Add(EditorLocalize.LocalizeText("WORD_0113"));
                        foreach (var item in items)
                        {
                            itemNameList.Add($"{item.SerialNumberString} {item.basic.name}");
                            if (item.basic.id == value) defaultIndex = index + 1;
                            index++;
                        }

                        popupField = new PopupFieldBase<string>(itemNameList, defaultIndex);
                        popupField.RegisterValueChangedCallback(evt =>
                        {
                            var popupField = evt.currentTarget as PopupFieldBase<string>;
                            var newValue = popupField.index > 0 ? items[popupField.index - 1].basic.id : "";
                            if (newValue != _lastValue)
                            {
                                _modified = true;
                                _lastValue = newValue;
                                textField.value = newValue;
                                _parameters.SetParameterValue(_paramInfo.name, newValue);
                                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                            }
                        });
                        popupField.style.flexGrow = 1;
                        AddonUIUtil.AddStyleEllipsisToPopupField(popupField);
                        value_window.Add(popupField);
                        SetDelayedAction(() => { popupField.Focus(); });
                    }
                    else if (_paramType == ParamType.Weapon)
                    {
                        var weapons = _databaseManagementService.LoadWeapon();
                        var defaultIndex = 0;
                        var index = 0;
                        weaponNameList.Add(EditorLocalize.LocalizeText("WORD_0113"));
                        foreach (var weapon in weapons)
                        {
                            weaponNameList.Add($"{weapon.SerialNumberString} {weapon.basic.name}");
                            if (weapon.basic.id == value) defaultIndex = index + 1;
                            index++;
                        }

                        popupField = new PopupFieldBase<string>(weaponNameList, defaultIndex);
                        popupField.RegisterValueChangedCallback(evt =>
                        {
                            var popupField = evt.currentTarget as PopupFieldBase<string>;
                            var newValue = popupField.index > 0 ? weapons[popupField.index - 1].basic.id : "";
                            if (newValue != _lastValue)
                            {
                                _modified = true;
                                _lastValue = newValue;
                                textField.value = newValue;
                                _parameters.SetParameterValue(_paramInfo.name, newValue);
                                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                            }
                        });
                        popupField.style.flexGrow = 1;
                        AddonUIUtil.AddStyleEllipsisToPopupField(popupField);
                        value_window.Add(popupField);
                        SetDelayedAction(() => { popupField.Focus(); });
                    }
                    else if (_paramType == ParamType.Armor)
                    {
                        var armors = _databaseManagementService.LoadArmor();
                        var defaultIndex = 0;
                        var index = 0;
                        armorNameList.Add(EditorLocalize.LocalizeText("WORD_0113"));
                        foreach (var armor in armors)
                        {
                            armorNameList.Add($"{armor.SerialNumberString} {armor.basic.name}");
                            if (armor.basic.id == value) defaultIndex = index + 1;
                            index++;
                        }

                        popupField = new PopupFieldBase<string>(armorNameList, defaultIndex);
                        popupField.RegisterValueChangedCallback(evt =>
                        {
                            var popupField = evt.currentTarget as PopupFieldBase<string>;
                            var newValue = popupField.index > 0 ? armors[popupField.index - 1].basic.id : "";
                            if (newValue != _lastValue)
                            {
                                _modified = true;
                                _lastValue = newValue;
                                textField.value = newValue;
                                _parameters.SetParameterValue(_paramInfo.name, newValue);
                                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                            }
                        });
                        popupField.style.flexGrow = 1;
                        AddonUIUtil.AddStyleEllipsisToPopupField(popupField);
                        value_window.Add(popupField);
                        SetDelayedAction(() => { popupField.Focus(); });
                    }
                    else if (_paramType == ParamType.Enemy)
                    {
                        var enemies = _databaseManagementService.LoadEnemy();
                        var defaultIndex = 0;
                        var index = 0;
                        enemyNameList.Add(EditorLocalize.LocalizeText("WORD_0113"));
                        foreach (var enemy in enemies)
                        {
                            enemyNameList.Add($"{enemy.SerialNumberString} {enemy.name}");
                            if (enemy.id == value) defaultIndex = index + 1;
                            index++;
                        }

                        popupField = new PopupFieldBase<string>(enemyNameList, defaultIndex);
                        popupField.RegisterValueChangedCallback(evt =>
                        {
                            var popupField = evt.currentTarget as PopupFieldBase<string>;
                            var newValue = popupField.index > 0 ? enemies[popupField.index - 1].id : "";
                            if (newValue != _lastValue)
                            {
                                _modified = true;
                                _lastValue = newValue;
                                textField.value = newValue;
                                _parameters.SetParameterValue(_paramInfo.name, newValue);
                                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                            }
                        });
                        popupField.style.flexGrow = 1;
                        AddonUIUtil.AddStyleEllipsisToPopupField(popupField);
                        value_window.Add(popupField);
                        SetDelayedAction(() => { popupField.Focus(); });
                    }
                    else if (_paramType == ParamType.Troop)
                    {
                        var troops = _databaseManagementService.LoadTroop();
                        var defaultIndex = 0;
                        var index = 0;
                        troopNameList.Add(EditorLocalize.LocalizeText("WORD_0113"));
                        foreach (var troop in troops)
                        {
                            troopNameList.Add($"{troop.SerialNumberString} {troop.name}");
                            if (troop.id == value) defaultIndex = index + 1;
                            index++;
                        }

                        popupField = new PopupFieldBase<string>(troopNameList, defaultIndex);
                        popupField.RegisterValueChangedCallback(evt =>
                        {
                            var popupField = evt.currentTarget as PopupFieldBase<string>;
                            var newValue = popupField.index > 0 ? troops[popupField.index - 1].id : "";
                            if (newValue != _lastValue)
                            {
                                _modified = true;
                                _lastValue = newValue;
                                textField.value = newValue;
                                _parameters.SetParameterValue(_paramInfo.name, newValue);
                                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                            }
                        });
                        popupField.style.flexGrow = 1;
                        AddonUIUtil.AddStyleEllipsisToPopupField(popupField);
                        value_window.Add(popupField);
                        SetDelayedAction(() => { popupField.Focus(); });
                    }
                    else if (_paramType == ParamType.State)
                    {
                        var states = _databaseManagementService.LoadStateEdit();
                        var defaultIndex = 0;
                        var index = 0;
                        stateNameList.Add(EditorLocalize.LocalizeText("WORD_0113"));
                        foreach (var state in states)
                        {
                            stateNameList.Add($"{state.SerialNumberString} {state.name}");
                            if (state.id == value) defaultIndex = index + 1;
                            index++;
                        }

                        popupField = new PopupFieldBase<string>(stateNameList, defaultIndex);
                        popupField.RegisterValueChangedCallback(evt =>
                        {
                            var popupField = evt.currentTarget as PopupFieldBase<string>;
                            var newValue = popupField.index > 0 ? states[popupField.index - 1].id : "";
                            if (newValue != _lastValue)
                            {
                                _modified = true;
                                _lastValue = newValue;
                                textField.value = newValue;
                                _parameters.SetParameterValue(_paramInfo.name, newValue);
                                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                            }
                        });
                        popupField.style.flexGrow = 1;
                        AddonUIUtil.AddStyleEllipsisToPopupField(popupField);
                        value_window.Add(popupField);
                        SetDelayedAction(() => { popupField.Focus(); });
                    }
                    else if (_paramType == ParamType.Tileset)
                    {
                        var tilesets = _mapManagementService.LoadTileGroups();
                        var defaultIndex = 0;
                        var index = 0;
                        tilesetNameList.Add(EditorLocalize.LocalizeText("WORD_0113"));
                        foreach (var tileset in tilesets)
                        {
                            tilesetNameList.Add($"{tileset.SerialNumberString} {tileset.name}");
                            if (tileset.id == value) defaultIndex = index + 1;
                            index++;
                        }

                        popupField = new PopupFieldBase<string>(tilesetNameList, defaultIndex);
                        popupField.RegisterValueChangedCallback(evt =>
                        {
                            var popupField = evt.currentTarget as PopupFieldBase<string>;
                            var newValue = popupField.index > 0 ? tilesets[popupField.index - 1].id : "";
                            if (newValue != _lastValue)
                            {
                                _modified = true;
                                _lastValue = newValue;
                                textField.value = newValue;
                                _parameters.SetParameterValue(_paramInfo.name, newValue);
                                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                            }
                        });
                        popupField.style.flexGrow = 1;
                        AddonUIUtil.AddStyleEllipsisToPopupField(popupField);
                        value_window.Add(popupField);
                        SetDelayedAction(() => { popupField.Focus(); });
                    }
                    else if (_paramType == ParamType.File)
                    {
                        fileContainer = new VisualElement();
                        fileContainer.AddToClassList("align_row");
                        fileContainer.style.flexGrow = 1;
                        value_window.Add(fileContainer);

                        var valueArray = DataConverter.GetStringArrayFromJson(_lastValue);
                        UpdateFile(fileContainer, () => { return textField; }, _lastValue);
                    }
                }
                else
                {
                    if (_paramType == ParamType.Select)
                    {
                        textField = new ImTextFieldEnterFocusOut();
                        value = AddonUIUtil.GetEasyReadableText(_addonInfo, _paramInfo,
                            AddonManager.GetTypeName(_paramType), value, "@value");
                        textField.value = value;
                        textField.style.flexGrow = 1;
                    }
                    else if (_paramType == ParamType.MultilineString)
                    {
                        multiline = true;
                        scrollTextField = new ImScrollTextField(ScrollViewMode.VerticalAndHorizontal);
                        //scrollTextField.RemoveFromClassList("unity-scroll-view");
                        textField = scrollTextField.textField;
                        textField.value = DataConverter.GetStringFromJson(value);
                    }
                    /*else if (_paramType == ParamType.Note)
                    {
                        textField = new ImTextField();
                        textField.value = DataConverter.GetJsonString(value);
                    }*/
                    else if (_paramType == ParamType.String || _paramType == ParamType.MultilineString)
                    {
                        textField = new ImTextField();
                        textField.value = DataConverter.GetStringFromJson(value);
                    }
                    else
                    {
                        textField = new ImTextFieldEnterFocusOut();
                        textField.value = value;
                    }

                    textField.RegisterCallback<FocusOutEvent>(e =>
                    {
                        //Debug.Log($"textField.FocusOutEvent");
                        var newValue = AddonManager.ValidateValue(_paramInfo, _paramType, textField.value);
                        /*var newLabel = AddonUIUtil.GetEasyReadableText(_addonInfo, _paramInfo, AddonManager.GetTypeName(_paramType), newValue);
                        if (newLabel != textField.value)
                        {
                            textField.value = newLabel;
                        }*/
                        /*if (_paramType == ParamType.Note)
                        {
                            newValue = DataConverter.GetStringFromJson(newValue);
                        }
                        else */
                        if (_paramType == ParamType.String || _paramType == ParamType.MultilineString)
                            newValue = DataConverter.GetJsonString(newValue);
                        if (newValue != _lastValue)
                        {
                            _modified = true;
                            _lastValue = newValue;
                            switch (_paramType)
                            {
                                case ParamType.Integer:
                                {
                                    integerField.value = int.Parse(newValue);
                                    break;
                                }
                                case ParamType.Number:
                                {
                                    doubleField.value = double.Parse(newValue);
                                    break;
                                }
                                case ParamType.Boolean:
                                {
                                    var boolValue = bool.Parse(newValue);
                                    //Debug.Log($"boolValue: {boolValue}");
                                    if (boolValue)
                                        toggle1.value = true;
                                    else
                                        toggle2.value = true;
                                    break;
                                }
                                case ParamType.Select:
                                {
                                    string popupValue = null;
                                    var popupIndex = _paramInfo.options.FindIndex(x => x.value == newValue);
                                    if (popupIndex >= 0) popupValue = _paramInfo.options[popupIndex].key;
                                    ForceSetIndex(popupField, popupIndex);
                                    break;
                                }
                                case ParamType.Combo:
                                {
                                    UpdateCombo(comboContainer, () => { return textField; }, newValue);
                                    break;
                                }
                                case ParamType.Note:
                                {
                                    scrollTextField.textField.value =
                                        DataConverter.GetStringFromJson(newValue); //newValue;
                                    break;
                                }
                                case ParamType.CommonEvent:
                                {
                                    UpdateEvent(eventContainer, () => { return textField; }, _lastValue);
                                    break;
                                }
                                case ParamType.MapEvent:
                                {
                                    //var valueArray = DataConverter.GetStringArrayFromJson(_lastValue);
                                    var jsonArray = JSON.Parse(_lastValue).AsArray;
                                    // Map
                                    UpdateMap(mapContainer, eventContainer, () => { return textField; },
                                        jsonArray[0].Value);

                                    // Event
                                    UpdateMapEvent(eventContainer, () => { return textField; }, jsonArray[0].ToString(),
                                        jsonArray[1].Value);
                                    break;
                                }
                                case ParamType.Switch:
                                {
                                    var flags = _databaseManagementService.LoadFlags();
                                    var popupIndex = flags.switches.FindIndex(x => x.id == newValue) + 1;
                                    //var popupValue = switchNameList[popupIndex];
                                    ForceSetIndex(popupField, popupIndex);
                                    break;
                                }
                                case ParamType.Variable:
                                {
                                    var flags = _databaseManagementService.LoadFlags();
                                    var popupIndex = flags.variables.FindIndex(x => x.id == newValue) + 1;
                                    //var popupValue = variableNameList[popupIndex];
                                    ForceSetIndex(popupField, popupIndex);
                                    break;
                                }
                                case ParamType.Animation:
                                {
                                    var animations = _databaseManagementService.LoadAnimation();
                                    var popupIndex = animations.FindIndex(x => x.id == newValue);
                                    if (popupIndex < 0) popupIndex = 0;
                                    //var popupValue = animationNameList[popupIndex];
                                    ForceSetIndex(popupField, popupIndex);
                                    break;
                                }
                                case ParamType.Actor:
                                {
                                    var actors = _databaseManagementService.LoadCharacterActor();
                                    var popupIndex = actors.FindIndex(x => x.uuId == newValue) + 1;
                                    //var popupValue = actorNameList[popupIndex];
                                    ForceSetIndex(popupField, popupIndex);
                                    break;
                                }
                                case ParamType.Class:
                                {
                                    var classes = _databaseManagementService.LoadCharacterActorClass();
                                    var popupIndex = classes.FindIndex(x => x.id == newValue) + 1;
                                    //var popupValue = classNameList[popupIndex];
                                    ForceSetIndex(popupField, popupIndex);
                                    break;
                                }
                                case ParamType.Skill:
                                {
                                    var skills = _databaseManagementService.LoadSkillCustom();
                                    var popupIndex = skills.FindIndex(x => x.basic.id == newValue) + 1;
                                    //var popupValue = skillNameList[popupIndex];
                                    ForceSetIndex(popupField, popupIndex);
                                    break;
                                }
                                case ParamType.Item:
                                {
                                    var items = _databaseManagementService.LoadItem();
                                    var popupIndex = items.FindIndex(x => x.basic.id == newValue) + 1;
                                    //var popupValue = itemNameList[popupIndex];
                                    ForceSetIndex(popupField, popupIndex);
                                    break;
                                }
                                case ParamType.Weapon:
                                {
                                    var weapons = _databaseManagementService.LoadWeapon();
                                    var popupIndex = weapons.FindIndex(x => x.basic.id == newValue) + 1;
                                    //var popupValue = weaponNameList[popupIndex];
                                    ForceSetIndex(popupField, popupIndex);
                                    break;
                                }
                                case ParamType.Armor:
                                {
                                    var armors = _databaseManagementService.LoadArmor();
                                    var popupIndex = armors.FindIndex(x => x.basic.id == newValue) + 1;
                                    //var popupValue = armorNameList[popupIndex];
                                    ForceSetIndex(popupField, popupIndex);
                                    break;
                                }
                                case ParamType.Enemy:
                                {
                                    var enemies = _databaseManagementService.LoadEnemy();
                                    var popupIndex = enemies.FindIndex(x => x.id == newValue) + 1;
                                    //var popupValue = enemyNameList[popupIndex];
                                    ForceSetIndex(popupField, popupIndex);
                                    break;
                                }
                                case ParamType.Troop:
                                {
                                    var troops = _databaseManagementService.LoadTroop();
                                    var popupIndex = troops.FindIndex(x => x.id == newValue) + 1;
                                    //var popupValue = troopNameList[popupIndex];
                                    ForceSetIndex(popupField, popupIndex);
                                    break;
                                }
                                case ParamType.State:
                                {
                                    var states = _databaseManagementService.LoadStateEdit();
                                    var popupIndex = states.FindIndex(x => x.id == newValue) + 1;
                                    //var popupValue = stateNameList[popupIndex];
                                    ForceSetIndex(popupField, popupIndex);
                                    break;
                                }
                                case ParamType.Tileset:
                                {
                                    var tilesets = _mapManagementService.LoadTileGroups();
                                    var popupIndex = tilesets.FindIndex(x => x.id == newValue) + 1;
                                    //var popupValue = tilesetNameList[popupIndex];
                                    ForceSetIndex(popupField, popupIndex);
                                    break;
                                }
                                case ParamType.File:
                                {
                                    UpdateFile(fileContainer, () => { return textField; }, _lastValue);
                                    break;
                                }
                            }

                            _parameters.SetParameterValue(_paramInfo.name, newValue);
                            AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                            /*if (_paramType == ParamType.Note)
                            {
                                newValue = DataConverter.GetJsonString(newValue);
                            }
                            else */
                            if (_paramType == ParamType.String || _paramType == ParamType.MultilineString)
                                newValue = DataConverter.GetStringFromJson(newValue);
                            textField.value = newValue;
                        }
                        else if (_paramType == ParamType.Select) 
                        {
                            var text = AddonUIUtil.GetEasyReadableText(_addonInfo, _paramInfo,
                                    AddonManager.GetTypeName(_paramType), newValue, "@value");
                            if (text != textField.value)
                            {
                                textField.value = text;
                            }
                        }
                    });
                    if (_paramType == ParamType.MultilineString)
                        value_window.Add(scrollTextField);
                    else
                        value_window.Add(textField);
                    if (_paramType == ParamType.MultilineString)
                        SetDelayedAction(() => { textField.Focus(); });
                    if (_paramType == ParamType.String)
                        SetDelayedAction(() => { textField.Focus(); });
                }

                var description_window = labelFromUxml
                    .Query<VisualElement>($"system_window_{typeText}_description_window").AtIndex(0);
                var descriptionParam = _paramInfo.GetInfo("desc");
                text = descriptionParam != null ? descriptionParam.value : "";
                label = new Label(text);
                label.style.flexGrow = 1;
                description_window.Add(label);
            }

            if (multiline)
                for (var i = 0; i < 2; i++)
                {
                    var typeText = i == 0 ? "type" : "text";
                    var value_window = labelFromUxml.Query<VisualElement>($"system_window_{typeText}_value_window")
                        .AtIndex(0);
                    value_window.AddToClassList("system_window_type_value_window_multiline_height");
                }

            // 確定、キャンセルボタン
            //----------------------------------------------------------------------
            var buttonOk = labelFromUxml.Query<Button>("Common_Button_Ok").AtIndex(0);
            var buttonCancel = labelFromUxml.Query<Button>("Common_Button_Cancel").AtIndex(0);
            buttonOk.style.alignContent = Align.FlexEnd;
            buttonOk.clicked += RegisterOkAction(() =>
            {
                if (_modified)
                {
                    _parametersOrg.SetParameterValue(_paramInfo.name, _parameters.GetParameterValue(_paramInfo.name));
                    _callBackWindow(true);
                }

                Close();
            });

            buttonCancel.clicked += () => { Close(); };
        }

        private void UpdateEvent(VisualElement container, GetTextField getTextField, string value) {
            container.Clear();

            var eventCommonDataModels = _eventManagementService.LoadEventCommon();
            var nameList = new List<string>();
            var defaultIndex = -1;
            var index = 0;
            nameList.Add(EditorLocalize.LocalizeText("WORD_0113"));
            if (value == null || value.Length == 0) defaultIndex = index;
            index++;
            foreach (var eventCommonDataModel in eventCommonDataModels)
            {
                nameList.Add($"{eventCommonDataModel.SerialNumberString} {eventCommonDataModel.name}");
                if (eventCommonDataModel.eventId == value) defaultIndex = index;
                index++;
            }

            if (defaultIndex < 0)
            {
                nameList.Add("?");
                defaultIndex = eventCommonDataModels.Count + 1;
            }

            var popupField = new PopupFieldBase<string>(nameList, defaultIndex);
            popupField.RegisterValueChangedCallback(evt =>
            {
                var popupField = evt.currentTarget as PopupFieldBase<string>;
                //Debug.Log($"popupField: {popupField.index}, {popupField.value}");
                if (popupField.index >= eventCommonDataModels.Count + 1) return;
                var newValue = (popupField.index == 0 ? "" : eventCommonDataModels[popupField.index - 1].eventId);
                if (nameList.Count > eventCommonDataModels.Count + 1)
                {
                    nameList.RemoveAt(eventCommonDataModels.Count + 1);
                    popupField.RefreshChoices(nameList);
                }

                if (newValue != _lastValue)
                {
                    _modified = true;
                    _lastValue = newValue;
                    getTextField().value = newValue;
                    _parameters.SetParameterValue(_paramInfo.name, newValue);
                    AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                }
            });
            popupField.style.flexGrow = 1;
            AddonUIUtil.AddStyleEllipsisToPopupField(popupField);
            container.Add(popupField);
        }

        private void UpdateMap(
            VisualElement mapContainer,
            VisualElement eventContainer,
            GetTextField getTextField,
            string mapId
        ) {
            mapContainer.Clear();
            var mapEntities = _mapManagementService.LoadMaps();
            var nameList = new List<string>();
            var defaultIndex = -1;
            var index = 0;
            nameList.Add(EditorLocalize.LocalizeText("WORD_0113"));
            if (mapId == null || mapId.Length == 0) defaultIndex = index;
            index++;
            foreach (var mapEntity in mapEntities)
            {
                nameList.Add($"{mapEntity.SerialNumberString} {mapEntity.name}");
                if (mapEntity.id == mapId) defaultIndex = index;
                index++;
            }

            if (defaultIndex < 0)
            {
                nameList.Add("?");
                defaultIndex = mapEntities.Count + 1;
            }

            var popupField = new PopupFieldBase<string>(nameList, defaultIndex);
            popupField.RegisterValueChangedCallback(evt =>
            {
                var popupField = evt.currentTarget as PopupFieldBase<string>;
                if (popupField.index >= mapEntities.Count + 1) return;
                var newValue = (popupField.index == 0 ? "" : mapEntities[popupField.index - 1].id);
                if (nameList.Count > mapEntities.Count + 1)
                {
                    nameList.RemoveAt(mapEntities.Count + 1);
                    popupField.RefreshChoices(nameList);
                }

                var valueArray = DataConverter.GetStringArrayFromJson(_lastValue);
                if (newValue != valueArray[0])
                {
                    _modified = true;
                    valueArray[0] = newValue;
                    _lastValue = DataConverter.GetJsonStringArray(valueArray);
                    getTextField().value = _lastValue;
                    _parameters.SetParameterValue(_paramInfo.name, _lastValue);
                    UpdateMapEvent(eventContainer, getTextField, valueArray[0], valueArray[1]);
                    AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                }
            });
            popupField.style.flexGrow = 1;
            AddonUIUtil.AddStyleEllipsisToPopupField(popupField);
            mapContainer.Add(popupField);
        }

        private void UpdateMapEvent(VisualElement container, GetTextField getTextField, string mapId, string eventId) {
            container.Clear();

            var eventMapDataModels = _eventManagementService.LoadEventMap();
            var nameList = new List<string>();
            var defaultIndex = -1;
            var index = 0;
            nameList.Add(EditorLocalize.LocalizeText("WORD_0113"));
            if (eventId == null || eventId.Length == 0) defaultIndex = index;
            index++;
            var playerText = EditorLocalize.LocalizeText("WORD_0860");
            nameList.Add(playerText);
            if (eventId == AddonManager.PlayerEventId) defaultIndex = index;
            index++;
            var thisEventText = EditorLocalize.LocalizeText("WORD_0920"); ;
            nameList.Add(thisEventText);
            if (eventId == AddonManager.ThisEventId) defaultIndex = index;
            index++;
            var eventMapEntities = eventMapDataModels.FindAll(x => x.mapId == mapId);
            foreach (var eventMapEntity in eventMapEntities)
            {
                //EditorLocalize.LocalizeText("WORD_1518");
                nameList.Add($"{eventMapEntity.SerialNumberString} {eventMapEntity.name}");
                if (eventMapEntity.eventId == eventId) defaultIndex = index;
                index++;
            }

            if (defaultIndex < 0)
            {
                nameList.Add("?");
                defaultIndex = eventMapEntities.Count + 3;
            }

            var popupField = new PopupFieldBase<string>(nameList, defaultIndex);
            popupField.RegisterValueChangedCallback(evt =>
            {
                var popupField = evt.currentTarget as PopupFieldBase<string>;
                if (popupField.index >= eventMapEntities.Count + 3) return;
                string newValue;
                if (popupField.index == 0)
                {
                    newValue = "";
                }
                else if (popupField.index == 1)
                {
                    newValue = AddonManager.PlayerEventId;
                }
                else if (popupField.index == 2)
                {
                    newValue = AddonManager.ThisEventId;
                }
                else
                {
                    newValue = eventMapEntities[popupField.index - 3].eventId;
                }
                if (nameList.Count > eventMapEntities.Count + 3)
                {
                    nameList.RemoveAt(eventMapEntities.Count + 3);
                    popupField.RefreshChoices(nameList);
                }

                var valueArray = DataConverter.GetStringArrayFromJson(_lastValue);
                if (newValue != valueArray[1])
                {
                    _modified = true;
                    valueArray[1] = newValue;
                    _lastValue = DataConverter.GetJsonStringArray(valueArray);
                    getTextField().value = _lastValue;
                    _parameters.SetParameterValue(_paramInfo.name, _lastValue);
                    AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                }
            });
            popupField.style.flexGrow = 1;
            AddonUIUtil.AddStyleEllipsisToPopupField(popupField);
            container.Add(popupField);
        }

        private void UpdateCombo(VisualElement container, GetTextField getTextField, string value) {
            container.Clear();

            var nameList = new List<string>();
            var defaultIndex = -1;
            var index = 0;
            foreach (var option in _paramInfo.options)
            {
                nameList.Add(option.key);
                if (option.key == value) defaultIndex = index;
                index++;
            }

            if (defaultIndex < 0)
            {
                if (value.Length > 0)
                {
                    defaultIndex = nameList.Count;
                    nameList.Add(value);
                }
                else if (nameList.Count > 0)
                {
                    defaultIndex = 0;
                }
            }

            var label = new Label("▼");
            label.AddToClassList("combo_label");
            label.RemoveFromClassList("unity-base-field");
            container.Add(label);
            var textField2 = new ImTextFieldEnterFocusOut();
            if (nameList.Count > 0)
            {
                var popupField = new PopupFieldBase<string>(nameList, defaultIndex);
                popupField.RegisterValueChangedCallback(evt =>
                {
                    var popupField = evt.currentTarget as PopupFieldBase<string>;
                    //Debug.Log($"popupField: {popupField.index}, {popupField.value}");
                    if (popupField.index >= _paramInfo.options.Count) return;
                    var newValue = nameList[popupField.index];
                    if (nameList.Count > _paramInfo.options.Count)
                    {
                        nameList.RemoveAt(nameList.Count - 1);
                        popupField.RefreshChoices(nameList);
                    }

                    if (newValue != _lastValue)
                    {
                        _modified = true;
                        _lastValue = newValue;
                        getTextField().value = newValue;
                        textField2.value = newValue;
                        _parameters.SetParameterValue(_paramInfo.name, newValue);
                        AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                    }
                });
                popupField.style.flexGrow = 1;
                AddonUIUtil.AddStyleEllipsisToPopupField(popupField);
                popupField.AddToClassList("combo_popup_field");
                popupField.RemoveFromClassList("unity-base-field");
                container.Add(popupField);
            }

            textField2.value = value;
            textField2.AddToClassList("combo_text_field");
            textField2.RemoveFromClassList("unity-base-field");
            textField2.RegisterCallback<FocusOutEvent>(e =>
            {
                //Debug.Log($"textField.FocusOutEvent");
                var newValue = textField2.value;
                if (newValue != _lastValue)
                {
                    _modified = true;
                    _lastValue = newValue;
                    getTextField().value = newValue;
                    _parameters.SetParameterValue(_paramInfo.name, newValue);
                    AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                    UpdateCombo(container, getTextField, newValue);
                }
            });
            container.Add(textField2);
        }

        private void UpdateFile(VisualElement fileContainer, GetTextField getTextField, string value) {
            fileContainer.Clear();

            value = value.Replace(@"\", "/");
            var nameList = new List<string>();
            var defaultIndex = -1;
            var index = 0;
            foreach (var str in _fileFolderDic.Keys)
            {
                nameList.Add($"{str}/");
                if (value == str || value.Length >= str.Length + 1 && value.Substring(0, str.Length + 1) == str + "/")
                    defaultIndex = index;
                index++;
            }

            if (defaultIndex >= 0)
            {
                if (value.Length >= nameList[defaultIndex].Length &&
                    value.Substring(0, nameList[defaultIndex].Length) == nameList[defaultIndex])
                    value = value.Substring(nameList[defaultIndex].Length);
                else
                    value = "";
            }
            else
            {
                value = "";
                defaultIndex = 0;
            }

            var popupField = new PopupFieldBase<string>(nameList, defaultIndex);
            popupField.RegisterValueChangedCallback(evt =>
            {
                var popupField = evt.currentTarget as PopupFieldBase<string>;
                if (popupField.index == defaultIndex) return;
                defaultIndex = popupField.index;
                _modified = true;
                _lastValue = nameList[defaultIndex].Substring(0, nameList[defaultIndex].Length - 1);
                getTextField().value = _lastValue;
                _parameters.SetParameterValue(_paramInfo.name, _lastValue);
                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                UpdateFile(fileContainer, getTextField, _lastValue);
            });
            popupField.style.flexGrow = 1;
            AddonUIUtil.AddStyleEllipsisToPopupField(popupField);
            fileContainer.Add(popupField);

            var nameList2 = new List<string>();
            var defaultIndex2 = -1;
            index = 0;
            foreach (var str in _fileFolderDic[nameList[defaultIndex].Substring(0, nameList[defaultIndex].Length - 1)])
            {
                nameList2.Add($"{str}/");
                if (value == str || value.Length >= str.Length + 1 && value.Substring(0, str.Length + 1) == str + "/")
                    defaultIndex2 = index;
                index++;
            }

            if (defaultIndex2 >= 0)
            {
                if (value.Length >= nameList2[defaultIndex2].Length &&
                    value.Substring(0, nameList2[defaultIndex2].Length) == nameList2[defaultIndex2])
                    value = value.Substring(nameList2[defaultIndex2].Length);
                else
                    value = "";
            }
            else
            {
                value = "";
                defaultIndex2 = 0;
            }

            var popupField2 = new PopupFieldBase<string>(nameList2, defaultIndex2);
            popupField2.RegisterValueChangedCallback(evt =>
            {
                var popupField2 = evt.currentTarget as PopupFieldBase<string>;
                if (popupField2.index == defaultIndex2) return;
                defaultIndex2 = popupField2.index;
                _modified = true;
                _lastValue =
                    $"{nameList[defaultIndex]}{nameList2[defaultIndex2].Substring(0, nameList2[defaultIndex2].Length - 1)}";
                getTextField().value = _lastValue;
                _parameters.SetParameterValue(_paramInfo.name, _lastValue);
                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                UpdateFile(fileContainer, getTextField, _lastValue);
            });
            popupField2.style.flexGrow = 1;
            AddonUIUtil.AddStyleEllipsisToPopupField(popupField2);
            fileContainer.Add(popupField2);

            var button = new Button();
            button.text = value;
            button.style.textOverflow = TextOverflow.Ellipsis;
            button.clickable.clicked += () =>
            {
                var selectImageModalWindow = new AddonSelectImageModalWindow();
                selectImageModalWindow.SetInfo($"{nameList[defaultIndex]}{nameList2[defaultIndex2]}", button.text);
                selectImageModalWindow.ShowWindow("", data =>
                {
                    var imageName = (string) data;
                    _modified = true;
                    _lastValue = $"{nameList[defaultIndex]}{nameList2[defaultIndex2]}{imageName}".Trim('/');
                    getTextField().value = _lastValue;
                    _parameters.SetParameterValue(_paramInfo.name, _lastValue);
                    AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                    UpdateFile(fileContainer, getTextField, _lastValue);
                });
            };
            button.style.flexGrow = 1;
            fileContainer.Add(button);
        }

        public class FoldableExpandedParent
        {
            public bool expanded;
            public bool foldable;
            public int  parent;

            public FoldableExpandedParent(bool foldable, bool expanded, int parent) {
                this.foldable = foldable;
                this.expanded = expanded;
                this.parent = parent;
            }
        }

        private delegate ImTextField GetTextField();
    }
}