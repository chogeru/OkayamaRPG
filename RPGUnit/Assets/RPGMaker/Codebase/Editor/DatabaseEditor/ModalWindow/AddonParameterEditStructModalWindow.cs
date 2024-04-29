using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.AddonUIUtil;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Addons;
using RPGMaker.Codebase.Runtime.Addon;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow
{
    public class AddonParameterEditStructModalWindow : AddonBaseModalWindow, IAddonParameterFinder
    {
        private readonly Vector2Int              WINDOW_SIZE = new Vector2Int(740, 480);
        private          AddonInfo               _addonInfo;
        private          int                     _arrayDimension;
        private readonly AddonParameterFindInfo  _finderInfo = new AddonParameterFindInfo();
        private          bool                    _firstFocus = true;
        private          string                  _lastValue;
        private          AddonParameterContainer _lastValues;
        private          ListView                _listView;
        private          string                  _localClipBoardRow;
        private          bool                    _modified;


        private AddonParameterContainer _parameters;
        private AddonParameterContainer _parametersOrg;
        private AddonParamInfo          _paramInfo;
        private ParamType               _paramType = ParamType.String; //default
        private AddonStructInfo         _structInfo;
        private string                  _structName;
        private TabbedMenuController    _tabController;
        private ImTextField             _textField;

        private VisualElement bottomWindow;

        protected override string ModalUxml =>
            "Assets/RPGMaker/Codebase/Editor/DatabaseEditor/ModalWindow/Uxml/addon_parameter_edit_struct_modalwindow.uxml";

        protected override string ModalUss => "";

        public bool FindNext() {
            var index = _listView.selectedIndex;
            while (true)
            {
                index++;
                if (index >= _lastValues.Count) break;
                if (_finderInfo.IsMatch(GetCellTexts(index)))
                {
                    _listView.selectedIndex = index;
                    return true;
                }
            }

            return false;
        }

        public bool FindPrev() {
            var index = _listView.selectedIndex;
            while (true)
            {
                index--;
                if (index < 0) break;
                if (_finderInfo.IsMatch(GetCellTexts(index)))
                {
                    _listView.selectedIndex = index;
                    return true;
                }
            }

            return false;
        }

        public void SetInfo(AddonInfo addonInfo, AddonParamInfo paramInfo, AddonParameterContainer parameters) {
            _parametersOrg = parameters;
            _parameters =
                new AddonParameterContainer(
                    JsonHelper.FromJsonArray<AddonParameter>(JsonHelper.ToJsonArray(parameters)));

            _addonInfo = addonInfo;
            _paramInfo = paramInfo;
            var info = _paramInfo.GetInfo("type");
            var typeName = info != null ? info.value : "string";
            _paramType = AddonManager.GetParamType(typeName, out _arrayDimension, out _structName);
            _structInfo = addonInfo.structInfos.FirstOrDefault(x => x.name == _structName);
            //Debug.Log($"_paramType: {_paramType.ToString()}");
        }

        private void OnDestroy() {
            AddonEditorWindowManager.instance.CloseDescendantWindows(this);
            AddonEditorWindowManager.instance.UnregisterWindow(this);
        }

        public override void ShowWindow(string modalTitle, CallBackWidow callBack) {
            var wnd = this; //GetWindow<AddonParameterEditStructModalWindow>();
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
            _tabController.RegisterTabCallbacks();

            var typeLabel = labelFromUxml.Query<VisualElement>("TypeTab").AtIndex(0) as Label;
            typeLabel.text = EditorLocalize.LocalizeText("WORD_2522");
            var textLabel = labelFromUxml.Query<VisualElement>("TextTab").AtIndex(0) as Label;
            textLabel.text = EditorLocalize.LocalizeText("WORD_2508");

            var value = AddonManager.ValidateValue(_addonInfo, _paramInfo, $"struct<{_structInfo.name}>",
                AddonManager.GetInitialValue(_parameters, _paramInfo));
            _lastValue = value;
            _lastValues = AddonManager.GetStructParameters(_structInfo, _lastValue);
            var paramValue = _parameters.GetParameterValue(_paramInfo.name);
            if (paramValue == null || paramValue.Length == 0)
            {
                _modified = true;
                _parameters.SetParameterValue(_paramInfo.name, _lastValue);
                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
            }

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
                    value_window.Clear();
                    value_window.Add(_listView = CreateListView());
                }
                else
                {
                    _textField = new ImTextFieldEnterFocusOut();
                    _textField.value = value;
                    //_textField.style.flexGrow = 1;
                    _textField.RegisterCallback<FocusOutEvent>(e =>
                    {
                        //Debug.Log($"_textField.FocusOutEvent");
                        var newValue = AddonManager.ValidateValue(_addonInfo, _paramInfo, $"struct<{_structInfo.name}>",
                            _textField.value);
                        if (newValue != _textField.value) _textField.value = newValue;
                        if (newValue != _lastValue)
                        {
                            _modified = true;
                            _lastValue = newValue;
                            _lastValues = AddonManager.GetStructParameters(_structInfo, _lastValue);

                            _textField.value = _lastValue;
                            var type_value_window = labelFromUxml
                                .Query<VisualElement>("system_window_type_value_window").AtIndex(0);
                            type_value_window.Clear();
                            type_value_window.Add(_listView = CreateListView());
                            _parameters.SetParameterValue(_paramInfo.name, _lastValue);
                            AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                        }
                    });
                    value_window.Add(_textField);
                }

                var description_window = labelFromUxml
                    .Query<VisualElement>($"system_window_{typeText}_description_window").AtIndex(0);
                var descriptionParam = _paramInfo.GetInfo("desc");
                text = descriptionParam != null ? descriptionParam.value : "";
                label = new Label(text);
                label.style.flexGrow = 1;
                description_window.Add(label);
            }

            _listView.ClearSelection();
            _listView.AddToSelection(0);

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

        private ListView CreateListView() {
            StripedListView<string> listView = null;
            Action<VisualElement, int> bindItem = (e, i) =>
            {
                e.Clear();
                {
                    var index = (listView.itemsSource as List<int>)[i];
                    VisualElement visualElement = new KeyVisualElement<int>(index);
                    visualElement.style.flexDirection = FlexDirection.Row;

                    listView.SetVisualElementStriped(visualElement, index);
                    if (index == listView.itemsSource.Count - 1) listView.AddVisualElementStriped(e);


                    var parameter = _lastValues[index];
                    var paramInfo = _structInfo.params_.GetParamInfo(parameter.key);

                    // Name
                    var nameLabel = new Label(paramInfo.GetInfo("text")?.value ?? parameter.key);
                    nameLabel.AddToClassList("text_ellipsis");
                    nameLabel.AddToClassList("list_view_item_name_label");

                    // Value
                    var value = parameter != null ? parameter.value : "";
                    var valueLabel = new Label(AddonUIUtil.GetEasyReadableText(_addonInfo, paramInfo,
                        paramInfo.GetInfo("type")?.value ?? "string", value));
                    valueLabel.AddToClassList("list_view_item_value_label");
                    valueLabel.AddToClassList("text_ellipsis");

                    visualElement.Add(nameLabel);
                    visualElement.Add(valueLabel);
                    e.Add(visualElement);
                }
            };

            Func<VisualElement> makeItem = () => new Label();
            listView = new StripedListView<string>(
                new string[_addonInfo != null && _addonInfo.paramInfos.Count > 0 ? _addonInfo.paramInfos.Count : 0], 16,
                makeItem, bindItem);
            listView.AddToClassList("list_view");
            listView.SolidQuad();
            listView.name = "list";
            //listView.selectionType = SelectionType.Multiple;
            //listView.reorderable = true;
            var list = new List<int>();
            for (var i = 0; i < _lastValues.Count; i++) list.Add(i);
            listView.itemsSource = list;

            listView.selectedIndex = 0;

            listView.RegisterCallback<KeyDownEvent>(e =>
            {
                //Debug.Log($"pressed: '{e.character}'");
                switch (e.keyCode)
                {
                    case KeyCode.KeypadEnter:
                    case KeyCode.Return:
                        var index = listView.selectedIndex;
                        ShowAddonParameterEditWindow(index);
                        break;

                    case KeyCode.F:
                        if ((e.modifiers & EventModifiers.Control) == EventModifiers.Control) ShowFindDialog();
                        break;

                    case KeyCode.F3:
                        if (_finderInfo.valid)
                        {
                            if ((e.modifiers & EventModifiers.Shift) == EventModifiers.Shift)
                                _finderInfo.FindPrev(this, this);
                            else
                                _finderInfo.FindNext(this, this);
                        }

                        break;
                }
            });
            listView.onItemsChosen += objects =>
            {
                //Debug.Log($"objects: {objects.ToString()}");
                var list = objects.ToList(); // as List<int>;
                if (list.Count == 0) return;
                //Debug.Log($"list: {list[0]}");
                var index = int.Parse(list[0].ToString());
                ShowAddonParameterEditWindow(index);
            };
            listView.RegisterCallback<FocusOutEvent>(e =>
            {
                //Debug.Log($"listView.FocusOutEvent");
                _lastValue = AddonManager.GetStructValue(_structInfo, _lastValues);
                _textField.value = _lastValue;
                _parameters.SetParameterValue(_paramInfo.name, _lastValue);
            });
            listView.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button != (int) MouseButton.RightMouse) return;
                var posY = (int) Math.Floor(evt.localMousePosition.y / listView.fixedItemHeight);
                posY = Math.Max(0, Math.Min(_lastValues.Count, posY));
                listView.selectedIndex = posY;
            });
            listView.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (evt.button != (int) MouseButton.RightMouse) return;
                var menu = new GenericMenu();
                //Edit
                menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1459")), false,
                    () => { ShowAddonParameterEditWindow(_listView.selectedIndex); });
                menu.AddSeparator("");
                //Copy
                if (_listView.selectedIndex < _lastValues.Count)
                    menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1462")), false,
                        () => { _localClipBoardRow = CopyRow(false, _listView.selectedIndex); });
                else
                    menu.AddDisabledItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1462")));
                //Paste
                if (_localClipBoardRow != null)
                    menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1463")), false,
                        () => { PasteRow(_listView.selectedIndex, _localClipBoardRow); });
                else
                    menu.AddDisabledItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1463")));
                //Clear
                if (_listView.selectedIndex < _lastValues.Count)
                    menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0610")), false, () =>
                    {
                        var paramInfo = _structInfo.params_.GetParamInfo(_lastValues[_listView.selectedIndex].key);
                        _lastValues[_listView.selectedIndex].value = AddonManager.ValidateValue(_addonInfo, paramInfo,
                            paramInfo.GetInfo("type")?.value ?? "string", "");
                        _lastValue = AddonManager.GetStructValue(_structInfo, _lastValues);
                        _textField.value = _lastValue;
                        _parameters.SetParameterValue(_paramInfo.name, _lastValue);
                        _modified = true;
                        _listView.Rebuild();
                        AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                    });
                else
                    menu.AddDisabledItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0610")));
                menu.AddSeparator("");
                //Find...
                menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1464")), false,
                    () => { ShowFindDialog(); });
                //Find Next
                if (_finderInfo.valid)
                    //Paste
                    menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1465")), false,
                        () => { _finderInfo.FindNext(this, this); });
                else
                    menu.AddDisabledItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1465")));
                //Find Previous
                if (_finderInfo.valid)
                    //Paste
                    menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1466")), false,
                        () => { _finderInfo.FindPrev(this, this); });
                else
                    menu.AddDisabledItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1466")));
                menu.ShowAsContext();
            });

            if (_firstFocus)
            {
                _firstFocus = false;
                SetDelayedAction(() => { listView.Focus(); });
            }

            return listView;
        }


        private string[] GetCellTexts(int rowIndex) {
            var paramInfo = _structInfo.params_.GetParamInfo(_lastValues[rowIndex].key);
            var texts = new List<string>
            {
                _lastValues[rowIndex].value,
                _lastValues[rowIndex].key,
                AddonUIUtil.GetEasyReadableText(_addonInfo, paramInfo, paramInfo.GetInfo("type")?.value ?? "string",
                    _lastValues[rowIndex].value)
            };
            var text = paramInfo.GetInfo("text");
            if (text != null) texts.Add(text.value);
            return texts.ToArray();
        }

        private void ShowFindDialog() {
            var addonParameterFindModalWindow = new AddonParameterFindModalWindow();
            addonParameterFindModalWindow.SetInfo(_finderInfo, this);
            addonParameterFindModalWindow.ShowWindow("", o => { });
        }

        private string CopyRow(bool doCut, int rowIndex) {
            var archive = _lastValues[rowIndex].value;
            return archive;
        }

        private void PasteRow(int rowIndex, string archive) {
            var paramInfo = _structInfo.params_.GetParamInfo(_lastValues[rowIndex].key);
            _lastValues[rowIndex].value = AddonManager.ValidateValue(_addonInfo, paramInfo,
                paramInfo.GetInfo("type")?.value ?? "string", archive);
            _lastValue = AddonManager.GetStructValue(_structInfo, _lastValues);
            _textField.value = _lastValue;
            _parameters.SetParameterValue(_paramInfo.name, _lastValue);
            _modified = true;
            _listView.Rebuild();
            _listView.Focus();
            _listView.selectedIndex = rowIndex;
            AddonEditorWindowManager.instance.CloseDescendantWindows(this);
        }

        private void ShowAddonParameterEditWindow(int index) {
            var paramInfo = _structInfo.params_[index];
            var parameters = new AddonParameterContainer();
            parameters.SetParameterValue(paramInfo.name, _lastValues[index].value);
            var mapIdParam = _parameters.FirstOrDefault(x => x.key == AddonCommand.mapIdKey);
            if (mapIdParam != null) parameters.Add(mapIdParam);
            AddonParameterModalWindow.ShowAddonParameterEditWindow(_addonInfo, paramInfo, false, parameters, obj =>
            {
                //Debug.Log($"ShowAddonParameterEditWindow return: {obj}");
                if (obj is bool && (bool) obj)
                {
                    var newValue = parameters.GetParameterValue(paramInfo.name);
                    _lastValues[index].value = newValue;
                    _lastValue = AddonManager.GetStructValue(_structInfo, _lastValues);
                    _textField.value = _lastValue;
                    _parameters.SetParameterValue(_paramInfo.name, _lastValue);
                    _modified = true;
                    _listView.Rebuild();
                    AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                }
            });
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
    }
}