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
    public class AddonParameterEditArrayModalWindow : AddonBaseModalWindow, IAddonParameterFinder
    {
        readonly Vector2Int WINDOW_SIZE = new Vector2Int(740, 480);

        private VisualElement bottomWindow;
        private VisualElement _type_value_window;
        private ListView _listView;
        private ImTextField _textField;
        private TabbedMenuController _tabController;
        private ParamType _paramType;
        private int _arrayDimension;
        private string _structName;


        private AddonParameterContainer _parameters;
        private AddonParameterContainer _parametersOrg;
        private AddonInfo _addonInfo;
        private AddonParamInfo _paramInfo;
        private string _lastValue;
        private List<string> _lastValues;
        private bool _firstFocus = true;
        private string _localClipBoardRow = null;
        private AddonParameterFindInfo _finderInfo = new AddonParameterFindInfo();

        public class FoldableExpandedParent
        {
            public FoldableExpandedParent(bool foldable, bool expanded, int parent) {
                this.foldable = foldable;
                this.expanded = expanded;
                this.parent = parent;
            }
            public bool foldable;
            public bool expanded;
            public int parent;

        }
        private bool _modified = false;

        protected override string ModalUxml
        {
            get
            {
                return "Assets/RPGMaker/Codebase/Editor/DatabaseEditor/ModalWindow/Uxml/addon_parameter_edit_array_modalwindow.uxml";
            }
        }

        protected override string ModalUss
        {
            get { return ""; }
        }

        public void SetInfo(AddonInfo addonInfo, AddonParamInfo paramInfo, AddonParameterContainer parameters) {
            _parametersOrg = parameters;
            _parameters = new AddonParameterContainer(JsonHelper.FromJsonArray<AddonParameter>(JsonHelper.ToJsonArray<AddonParameter>(parameters)));

            _addonInfo = addonInfo;
            _paramInfo = paramInfo;
            var info = _paramInfo.GetInfo("type");
            var typeName = (info != null) ? info.value : "string";
            _paramType = AddonManager.GetParamType(typeName, out _arrayDimension, out _structName);
            //Debug.Log($"_paramType: {_paramType.ToString()}");
        }

        private void OnDestroy() {
            AddonEditorWindowManager.instance.CloseDescendantWindows(this);
            AddonEditorWindowManager.instance.UnregisterWindow(this);
        }

        public override void ShowWindow(string modalTitle, CallBackWidow callBack) {
            var wnd = this;// GetWindow<AddonParameterEditArrayModalWindow>();
            AddonEditorWindowManager.instance.RegisterParameterEditWindow(wnd);

            if (callBack != null)
            {
                _callBackWindow = callBack;
            }

            wnd.titleContent = new GUIContent(EditorLocalize.LocalizeText("WORD_2506"));
            wnd.Init();
            Vector2 size = WINDOW_SIZE;
            wnd.minSize = size;
            wnd.maxSize = size;
            wnd.maximized = false;
            wnd.Show();
        }

        public override void Init() {
            VisualElement root = rootVisualElement;

            // 要素作成
            //----------------------------------------------------------------------
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModalUxml);
            VisualElement labelFromUxml = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUxml);
            root.Add(labelFromUxml);

            var topWindow = labelFromUxml.Query<VisualElement>("system_window_topwindow").AtIndex(0);
            _tabController = new TabbedMenuController(topWindow);
            //Debug.Log($"_paramType: {_paramType.ToString()}");
            _tabController.RegisterTabCallbacks();

            var typeLabel = labelFromUxml.Query<VisualElement>("TypeTab").AtIndex(0) as Label;
            var typeWordId = AddonParameterEditModalWindow.GetTypeWord(_paramType);
            typeLabel.text = string.Format(EditorLocalize.LocalizeText("WORD_2521"), EditorLocalize.LocalizeText(typeWordId));
            var textLabel = labelFromUxml.Query<VisualElement>("TextTab").AtIndex(0) as Label;
            textLabel.text = EditorLocalize.LocalizeText("WORD_2508");

            var value = AddonManager.GetInitialValue(_parameters, _paramInfo);
            _lastValue = value;
            _lastValues = AddonManager.GetStringListFromJson(value);// (DataConverter.GetStringArrayFromJson(value) ?? new string[0]).ToList();
            _type_value_window = labelFromUxml.Query<VisualElement>($"system_window_type_value_window").AtIndex(0);
            for (int i = 0; i < 2; i++)
            {
                var typeText = (i == 0) ? "type" : "text";
                var label_window = labelFromUxml.Query<VisualElement>($"system_window_{typeText}_label_window").AtIndex(0);
                var textParam = _paramInfo.GetInfo("text");
                var text = (textParam != null) ? $"{textParam.value}({_paramInfo.name}):" : _paramInfo.name;
                var label = new Label(text);
                label.style.flexGrow = 1;
                label_window.Add(label);

                var value_window = labelFromUxml.Query<VisualElement>($"system_window_{typeText}_value_window").AtIndex(0);
                //Debug.Log($"typeText: {typeText}");
                if (i == 0)
                {
                    _type_value_window.Clear();
                    _type_value_window.Add(_listView = CreateListView());
                    _listView.Focus();
                }
                else
                {
                    _textField = new ImTextFieldEnterFocusOut();
                    _textField.value = value;
                    //_textField.style.flexGrow = 1;
                    _textField.RegisterCallback<FocusOutEvent>((e) =>
                    {
                        //Debug.Log($"_textField.FocusOutEvent");
                        var newValue = AddonManager.ValidateValue(_addonInfo, _paramInfo, (_paramInfo.GetInfo("type")?.value ?? "string[]"), _textField.value);
                        if (newValue != _textField.value)
                        {
                            _textField.value = newValue;
                        }
                        if (newValue != _lastValue)
                        {
                            _modified = true;
                            _lastValue = newValue;
                            _lastValues = AddonManager.GetStringListFromJson(_lastValue);//(DataConverter.GetStringArrayFromJson(_lastValue) ?? new string[0]).ToList();

                            _textField.value = _lastValue;
                            _type_value_window.Clear();
                            _type_value_window.Add(_listView = CreateListView());
                            _parameters.SetParameterValue(_paramInfo.name, _lastValue);
                            AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                        }
                    });
                    value_window.Add(_textField);
                }

                var description_window = labelFromUxml.Query<VisualElement>($"system_window_{typeText}_description_window").AtIndex(0);
                var descriptionParam = _paramInfo.GetInfo("desc");
                text = (descriptionParam != null) ? descriptionParam.value : "";
                label = new Label(text);
                label.style.flexGrow = 1;
                description_window.Add(label);
            }
            _listView.ClearSelection();
            _listView.AddToSelection(0);

            // 確定、キャンセルボタン
            //----------------------------------------------------------------------
            Button buttonOk = labelFromUxml.Query<Button>("Common_Button_Ok").AtIndex(0);
            Button buttonCancel = labelFromUxml.Query<Button>("Common_Button_Cancel").AtIndex(0);
            buttonOk.style.alignContent = Align.FlexEnd;
            buttonOk.clicked += RegisterOkAction(() =>
            {
                if (_modified)
                {
                    _parametersOrg.SetParameterValue(_paramInfo.name, _parameters.GetParameterValue(_paramInfo.name));
                    _callBackWindow(true);

                }
                this.Close();
            });

            buttonCancel.clicked += () =>
            {
                this.Close();
            };
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
                    if (index == listView.itemsSource.Count - 1)
                    {
                        listView.AddVisualElementStriped(e);
                    }

                    // Index
                    VisualElement vi1 = null;
                    vi1 = new VisualElement();
                    vi1.AddToClassList("list_view_item_header");
                    var label = new Label((i + 1).ToString());
                    label.style.flexGrow = 1;
                    vi1.Add(label);


                    // Value
                    var value = (index < _lastValues.Count) ? _lastValues[index] : "";
                    var labelText = (value.Length == 0 && _paramType == ParamType.Struct) ? "" : AddonUIUtil.GetEasyReadableText(_addonInfo, _paramInfo, (_structName != null) ? $"struct<{_structName}>" : AddonManager.GetTypeName(_paramType), value);
                    var valueLabel = new Label(labelText);
                    valueLabel.AddToClassList("list_view_item_value_label");
                    valueLabel.AddToClassList("text_ellipsis");

                    visualElement.Add(vi1);
                    visualElement.Add(valueLabel);
                    e.Add(visualElement);
                }
            };

            Func<VisualElement> makeItem = () => new Label();
            listView = new StripedListView<string>(new String[(_addonInfo != null && _addonInfo.paramInfos.Count > 0) ? _addonInfo.paramInfos.Count : 0], 16, makeItem, bindItem);
            listView.AddToClassList("list_view");
            listView.SolidQuad();
            listView.name = "list";
            listView.selectionType = SelectionType.Multiple;
            listView.reorderable = true;
            var list = new List<int>();
            for (int i = 0; i < _lastValues.Count + 1; i++) {
                list.Add(i);
            }
            listView.itemsSource = list;

            listView.selectedIndex = 0;

            listView.RegisterCallback<KeyDownEvent>((e) =>
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
                        if ((e.modifiers & EventModifiers.Control) == EventModifiers.Control)
                        {
                            ShowFindDialog();
                        }
                        break;

                    case KeyCode.F3:
                        if (_finderInfo.valid)
                        {
                            if ((e.modifiers & EventModifiers.Shift) == EventModifiers.Shift)
                            {
                                _finderInfo.FindPrev(this, this);
                            }
                            else
                            {
                                _finderInfo.FindNext(this, this);
                            }
                        }
                        break;
                }
            });
            listView.onItemsChosen += (objects) =>
            {
                //Debug.Log($"objects: {objects.ToString()}");
                var list = objects.ToList();// as List<int>;
                if (list.Count == 0) return;
                //Debug.Log($"list: {list[0]}");
                var index = int.Parse(list[0].ToString());
                ShowAddonParameterEditWindow(index);
            };
            listView.RegisterCallback<FocusOutEvent>((e) =>
            {
                //Debug.Log($"listView.FocusOutEvent");
                _lastValue = AddonManager.GetJsonFromStringList(_lastValues);// DataConverter.GetJsonStringArray(_lastValues.ToArray());
                _textField.value = _lastValue;
                _parameters.SetParameterValue(_paramInfo.name, _lastValue);
            });
            listView.RegisterCallback<DragExitedEvent>((e) =>
            {
                // The blank line should not be placed except at the end. 
                // Reorder _lastValues, refresh itemsSource, and set _lastValue.
                //Debug.Log($"listView.DragExitedEvent");
                _modified = true;
                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                var newValues = new List<string>();
                var itemsSource = (_listView.itemsSource as List<int>);
                for (int i = 0; i < itemsSource.Count; i++)
                {
                    if (itemsSource[i] == itemsSource.Count - 1) {
                        continue;
                    }
                    newValues.Add(_lastValues[itemsSource[i]]);
                }
                _lastValues = newValues;
                _lastValue = DataConverter.GetJsonStringArray(_lastValues.ToArray());
                var newItemsSource = new List<int>();
                for (int i = 0; i < itemsSource.Count; i++)
                {
                    newItemsSource.Add(i);
                }
                _listView.itemsSource = newItemsSource;
                _textField.value = _lastValue;
                _parameters.SetParameterValue(_paramInfo.name, _lastValue);
                _listView.Rebuild();
            });
            listView.RegisterCallback<MouseDownEvent>((evt) =>
            {
                if (evt.button != (int) MouseButton.RightMouse)
                {
                    return;
                }
                
                var index = (int) Math.Floor(evt.localMousePosition.y / listView.fixedItemHeight);
                index = Math.Max(0, Math.Min(_lastValues.Count, index));
                if (!listView.selectedIndices.Contains(index))
                {
                    listView.selectedIndex = index;

                }
            });
            listView.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (evt.button != (int) MouseButton.RightMouse)
                {
                    return;
                }
                var menu = new GenericMenu();
                //Edit
                menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1459")), false, () =>
                {
                    ShowAddonParameterEditWindow(_listView.selectedIndex);
                });
                menu.AddSeparator("");

                var validSelectedIndexCount = _listView.selectedIndices.Count();
                if (_listView.selectedIndices.ToList().IndexOf(_lastValues.Count) >= 0)
                {
                    validSelectedIndexCount--;
                }
                var allRowsSelected = (_lastValues.Count > 0 && validSelectedIndexCount >= _lastValues.Count);
                //Cut
                if (allRowsSelected || _listView.selectedIndex < _lastValues.Count)
                {
                    menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1461")), false, () =>
                    {
                        _localClipBoardRow = CutCopyRow(true, _listView.selectedIndices.ToArray());
                    });
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1461")));
                }
                //Copy
                if (allRowsSelected || _listView.selectedIndex < _lastValues.Count)
                {
                    menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1462")), false, () =>
                    {
                        _localClipBoardRow = CutCopyRow(false, _listView.selectedIndices.ToArray());
                    });
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1462")));
                }
                //Paste
                if (_localClipBoardRow != null)
                {
                    menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1463")), false, () =>
                    {
                        PasteRow(_listView.selectedIndex, _localClipBoardRow);
                    });
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1463")));
                }
                //Delete
                if (allRowsSelected || _listView.selectedIndex < _lastValues.Count)
                {
                    menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0383")), false, () =>
                    {
                        CutCopyRow(true, _listView.selectedIndices.ToArray());
                    });
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0383")));
                }
                //Select All
                menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_2584")), false, () =>
                {
                    _listView.ClearSelection();
                    for (int i = 0; i < _lastValues.Count; i++)
                    {
                        _listView.AddToSelection(i);
                    }
                });
                menu.AddSeparator("");
                //Find...
                menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1464")), false, () =>
                {
                    ShowFindDialog();
                });
                //Find Next
                if (_finderInfo.valid)
                {
                    //Paste
                    menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1465")), false, () =>
                    {
                        _finderInfo.FindNext(this, this);
                    });
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1465")));
                }
                //Find Previous
                if (_finderInfo.valid)
                {
                    //Paste
                    menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1466")), false, () =>
                    {
                        _finderInfo.FindPrev(this, this);
                    });
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1466")));
                }

                menu.ShowAsContext();
            });

            if (_firstFocus)
            {
                _firstFocus = false;
                SetDelayedAction(() => {
                    listView.Focus();
                });

            }
            return listView;
        }

        private string[] GetCellTexts(int rowIndex) {
            var texts = new string[] {
                    _lastValues[rowIndex],
                    AddonUIUtil.GetEasyReadableText(_addonInfo, _paramInfo, (_structName != null) ? $"struct<{_structName}>" : AddonManager.GetTypeName(_paramType), _lastValues[rowIndex]),
                };
            return texts;
        }

        public bool FindNext() {
            var index = _listView.selectedIndex;
            while (true)
            {
                index++;
                if (index >= _lastValues.Count)
                {
                    break;
                }
                if (_finderInfo.IsMatch(GetCellTexts(index))){
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
                if (index < 0)
                {
                    break;
                }
                if (_finderInfo.IsMatch(GetCellTexts(index)))
                {
                    _listView.selectedIndex = index;
                    return true;
                }
            }
            return false;
        }

        private void ShowFindDialog() {
            var addonParameterFindModalWindow = new AddonParameterFindModalWindow();
            addonParameterFindModalWindow.SetInfo(_finderInfo, this);
            addonParameterFindModalWindow.ShowWindow("", (o) => { });
        }

        private string CutCopyRow(bool doCut, int[] rowIndices) {
            var index = Array.IndexOf(rowIndices, _lastValues.Count);
            if (index >= 0)
            {
                var list = rowIndices.ToList();
                list.RemoveAt(index);
                rowIndices = list.ToArray();
            }
            Array.Sort(rowIndices);
            var copyArr = new string[rowIndices.Length];
            for (int i = 0; i < rowIndices.Length; i++)
            {
                copyArr[i] = _lastValues[rowIndices[i]];
            }
            //_lastValues.CopyTo(rowIndex, copyArr, 0, rowCount);
            var archive = DataConverter.GetJsonStringArray(copyArr);
            if (doCut)
            {
                for (int i = rowIndices.Length - 1; i >= 0; i--)
                {
                    _lastValues.RemoveAt(rowIndices[i]);
                }
                //_lastValues.RemoveRange(rowIndex, rowCount);
                _lastValue = AddonManager.GetJsonFromStringList(_lastValues);// DataConverter.GetJsonStringArray(_lastValues.ToArray());
                _textField.value = _lastValue;
                _parameters.SetParameterValue(_paramInfo.name, _lastValue);
                _modified = true;
                //_listView.Rebuild();
                _type_value_window.Clear();
                _type_value_window.Add(_listView = CreateListView());
                _listView.Focus();
                _listView.selectedIndex = rowIndices[0];
                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
            }
            return archive;
        }

        private void PasteRow(int rowIndex, string archive) {
            var rows = DataConverter.GetStringArrayFromJson(archive);
            _lastValues.InsertRange(rowIndex, rows);
            _lastValue = AddonManager.GetJsonFromStringList(_lastValues);// DataConverter.GetJsonStringArray(_lastValues.ToArray());
            _textField.value = _lastValue;
            _parameters.SetParameterValue(_paramInfo.name, _lastValue);
            _modified = true;
            //_listView.Rebuild();
            _type_value_window.Clear();
            _type_value_window.Add(_listView = CreateListView());
            _listView.Focus();
            _listView.selectedIndex = rowIndex;
            AddonEditorWindowManager.instance.CloseDescendantWindows(this);
        }


        private void ShowAddonParameterEditWindow(int index) {
            var parameters = new AddonParameterContainer();
            var modified = (index >= _lastValues.Count);
            var typeName = (_paramType == ParamType.Struct) ? $"struct<{_structName}>" : AddonManager.GetTypeName(_paramType);
            string lastValue = (index < _lastValues.Count) ? _lastValues[index] :
                AddonManager.ValidateValue(_addonInfo, _paramInfo, typeName, "");
            parameters.SetParameterValue(_paramInfo.name, lastValue);
            var mapIdParam = _parameters.FirstOrDefault(x => x.key == AddonCommand.mapIdKey);
            if (mapIdParam != null)
            {
                parameters.Add(mapIdParam);
            }
            AddonParameterModalWindow.ShowAddonParameterEditWindow(_addonInfo, _paramInfo, true, parameters, obj =>
            {
                //Debug.Log($"ShowAddonParameterEditWindow return: {obj}");
                if ((obj is bool) && (bool) obj == true)
                {
                    var newValue = parameters.GetParameterValue(_paramInfo.name);
                    if (index < _lastValues.Count)
                    {
                        _lastValues[index] = newValue;
                    }
                    else
                    {
                        _lastValues.Add(newValue);
                        var list = (_listView.itemsSource as List<int>);
                        list.Add(list.Count);
                        _listView.itemsSource = list;
                    }
                    _lastValue = AddonManager.GetJsonFromStringList(_lastValues);// DataConverter.GetJsonStringArray(_lastValues.ToArray());
                    _textField.value = _lastValue;
                    _parameters.SetParameterValue(_paramInfo.name, _lastValue);
                    _modified = true;
                    _listView.Rebuild();
                    AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                }
            }, modified);
        }

    }

}
