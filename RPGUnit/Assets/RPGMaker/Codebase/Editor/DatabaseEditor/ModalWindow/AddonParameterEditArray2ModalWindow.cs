using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.AddonUIUtil;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Addons;
using RPGMaker.Codebase.Runtime.Addon;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow
{
    public class AddonParameterEditArray2ModalWindow : AddonBaseModalWindow, IAddonParameterFinder
    {
        readonly Vector2Int WINDOW_SIZE = new Vector2Int(740, 480);

        private VisualElement bottomWindow;
        private ListView _listView;
        private ImTextField _textField;
        private TabbedMenuController _tabController;
        private VisualElement _typeValueWindow;
        private VisualElement _columnFocusMarker;
        private VisualElement _columnSelectMarkerContainer;
        private List<VisualElement> _columnSelectMarkers = new List<VisualElement>();
        private VisualElement _columnVe;
        private VisualElement _rowVe;
        private ParamType _paramType;
        private int _arrayDimension;
        private string _structName;


        private AddonParameterContainer _parameters;
        private AddonParameterContainer _parametersOrg;
        private AddonInfo _addonInfo;
        private AddonParamInfo _paramInfo;
        private string _lastValue;
        private List<string> _lastValues;
        private int _columnCount;
        private int _rowCount;
        private List<int> _selectedColumnIndices = new List<int>();
        private int _focusColumnIndex;
        private CellPos _lastCellPos = new CellPos(-1, -1);
        private bool _firstFocus = true;
        private string _localClipBoardRow = null;
        private string _localClipBoardColumn = null;
        private AddonParameterFindInfo _finderInfo = new AddonParameterFindInfo();

        public class FoldableExpandedParent {
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
            var typename = (info != null) ? info.value : "string";
            _paramType = AddonManager.GetParamType(typename, out _arrayDimension, out _structName);
            //Debug.Log($"_paramType: {_paramType.ToString()}");
        }

        private void OnDestroy() {
            AddonEditorWindowManager.instance.CloseDescendantWindows(this);
            AddonEditorWindowManager.instance.UnregisterWindow(this);
        }

        public override void ShowWindow(string modalTitle, CallBackWidow callBack) {
            var wnd = this;// GetWindow<AddonParameterEditArray2ModalWindow>();
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

            _typeValueWindow = labelFromUxml.Query<VisualElement>($"system_window_type_value_window").AtIndex(0);
            var ve = new VisualElement();
            ve.AddToClassList("value_area");
            _typeValueWindow.Add(ve);
            _typeValueWindow = ve;
            var value = AddonManager.GetInitialValue(_parameters, _paramInfo);
            _lastValue = value;
            _lastValues = AddonManager.GetStringListFromJson(value);// (DataConverter.GetStringArrayFromJson(value) ?? new string[0]).ToList();
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
                    UpdateTypeValueWindow(_typeValueWindow);
                }
                else
                {
                    _textField = new ImTextFieldEnterFocusOut();
                    _textField.value = value;
                    //_textField.style.flexGrow = 1;
                    _textField.RegisterCallback<FocusOutEvent>((e) =>
                    {
                        //Debug.Log($"_textField.FocusOutEvent");
                        var newValue = AddonManager.ValidateValue(_addonInfo, _paramInfo, (_paramInfo.GetInfo("type")?.value ?? "string[][]"), _textField.value);
                        if (newValue != _textField.value)
                        {
                            _textField.value = newValue;
                        }
                        if (newValue != _lastValue)
                        {
                            _modified = true;
                            _lastValue = newValue;
                            _lastValues = AddonManager.GetStringListFromJson(_lastValue);// (DataConverter.GetStringArrayFromJson(_lastValue) ?? new string[0]).ToList();

                            _textField.value = _lastValue;
                            UpdateTypeValueWindow(_typeValueWindow);
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
            SelectRowsColumns(new List<int>() { 0 }, new List<int>() { 0 });

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

        private const int RowNumberWidth = 24;
        private const int CellWidth = 96;
        private const int CellHeight = 16;
        private void UpdateTypeValueWindow(VisualElement container) {
            container.Clear();
            _columnSelectMarkerContainer = new VisualElement();
            container.Add(_columnSelectMarkerContainer);
            _columnFocusMarker = new VisualElement();
            _columnFocusMarker.AddToClassList("column_focus_marker");
            _columnFocusMarker.AddToClassList("overflow_hidden");
            _columnFocusMarker.focusable = false;
            //_columnFocusMarker.SetEnabled(false);
#if false
            _columnFocusMarker.RegisterCallback<ClickEvent>((evt) =>
            {
                Debug.Log($"evt.clickCount: {evt.clickCount}");
                if (evt.clickCount >= 2)
                {
                    DoDoubleClick(evt.localPosition);
                }
                else
                {
                    RegisterSingleClick(evt.localPosition);
                }
            });
#endif
            container.Add(_columnFocusMarker);
            _rowCount = _lastValues.Count;
            _columnCount = 0;
            foreach (var row in _lastValues) {
                var columns = DataConverter.GetStringArrayFromJson(row) ?? new string[0];
                _columnCount = Math.Max(_columnCount, columns.Length);
            }

            container.Add(_listView = CreateListView());

#if true
            var columnMaskVe = new VisualElement();
            columnMaskVe.style.position = Position.Absolute;
            columnMaskVe.style.left = RowNumberWidth;
            columnMaskVe.style.top = 0;
            columnMaskVe.style.width = 640;
            columnMaskVe.style.height = CellHeight;
            columnMaskVe.AddToClassList("overflow_hidden");
            var rowMaskVe = new VisualElement();
            rowMaskVe.style.position = Position.Absolute;
            rowMaskVe.style.top = CellHeight;
            rowMaskVe.style.width = RowNumberWidth;
            rowMaskVe.style.height = 190;
            rowMaskVe.AddToClassList("overflow_hidden");
            var columnVe = new VisualElement();
            var rowVe = new VisualElement();
            _columnVe = columnVe;
            _rowVe = rowVe;

            columnMaskVe.RegisterCallback<MouseMoveEvent>((evt) =>
            {
                OnMouseMove(new Vector2(evt.localMousePosition.x + RowNumberWidth, evt.localMousePosition.y));
            });
            columnMaskVe.RegisterCallback<MouseDownEvent>((evt) =>
            {
                if (evt.button == (int) MouseButton.RightMouse)
                {
                    return;
                }
                OnMouseDown(new Vector2(evt.localMousePosition.x + RowNumberWidth, evt.localMousePosition.y), evt.modifiers);
            });
            columnMaskVe.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (evt.button != (int) MouseButton.RightMouse)
                {
                    return;
                }
                OnMouseUp(new Vector2(evt.localMousePosition.x + RowNumberWidth, evt.localMousePosition.y));
            });
            rowMaskVe.RegisterCallback<MouseMoveEvent>((evt) =>
            {
                OnMouseMove(new Vector2(evt.localMousePosition.x, evt.localMousePosition.y + CellHeight));
            });
            rowMaskVe.RegisterCallback<MouseDownEvent>((evt) =>
            {
                if (evt.button == (int) MouseButton.RightMouse)
                {
                    return;
                }
                OnMouseDown(new Vector2(evt.localMousePosition.x, evt.localMousePosition.y + CellHeight), evt.modifiers);
            });
            rowMaskVe.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (evt.button != (int) MouseButton.RightMouse)
                {
                    return;
                }
                OnMouseUp(new Vector2(evt.localMousePosition.x, evt.localMousePosition.y + CellHeight));
            });
            columnVe.style.position = Position.Absolute;
            for (int i = 0; i <= _columnCount; i++) {
                var l = new Label($"{i + 1}");
                l.style.position = Position.Absolute;
                l.style.left = CellWidth * i;
                l.style.top = 0;
                l.style.width = CellWidth;
                l.style.height = CellHeight;
                columnVe.Add(l);
            }
            columnMaskVe.Add(columnVe);
            container.Add(columnMaskVe);
            rowVe.style.position = Position.Absolute;
            for (int i = 0; i <= _lastValues.Count; i++)
            {
                var l = new Label($"{i + 1}");
                l.style.position = Position.Absolute;
                l.style.left = 0;
                l.style.top = CellHeight * i;
                l.style.width = RowNumberWidth;
                l.style.height = CellHeight;
                //l.AddToClassList("overflow_hidden");
                rowVe.Add(l);
            }
            rowMaskVe.Add(rowVe);
            container.Add(rowMaskVe);
#endif
        }

        private void OnMouseMove(Vector2 mousePosition) {
            var pos = GetCellPos(mousePosition);
            FocusColumn(pos.x);
        }

        private void OnMouseDown(Vector2 mousePosition, EventModifiers modifiers) {
            var pos = GetCellPos(mousePosition);
            var selectedIndices = _listView.selectedIndices.ToList();
            if (_lastCellPos.x < 0)
            {
                if ((modifiers & EventModifiers.Control) == EventModifiers.Control)
                {
                    if (!selectedIndices.Contains(pos.y))
                    {
                        selectedIndices.Add(pos.y);
                    }
                    else
                    {
                        selectedIndices.Remove(pos.y);
                    }
                    SelectRowsColumns(selectedIndices, _selectedColumnIndices);
                }
                else if ((modifiers & EventModifiers.Shift) == EventModifiers.Shift)
                {
                    if (selectedIndices.Count == 0 || pos.y == selectedIndices[0])
                    {
                        selectedIndices = new List<int>() { pos.y };
                    }
                    else if (pos.y < selectedIndices[0])
                    {
                        selectedIndices.RemoveRange(1, selectedIndices.Count - 1);
                        for (int i = pos.y; i <= selectedIndices[0]; i++)
                        {
                            selectedIndices.Add(i);
                        }
                    }
                    else
                    {
                        selectedIndices.RemoveRange(1, selectedIndices.Count - 1);
                        for (int i = selectedIndices[0]; i <= pos.y; i++)
                        {
                            selectedIndices.Add(i);
                        }
                    }
                    SelectRowsColumns(selectedIndices, _selectedColumnIndices);
                }
                else
                {
                    SelectRowsColumns(new List<int>() { pos.y }, _selectedColumnIndices);
                }
            }
            else if (_lastCellPos.y < 0)
            {
                if ((modifiers & EventModifiers.Control) == EventModifiers.Control)
                {
                    if (!_selectedColumnIndices.Contains(pos.x))
                    {
                        _selectedColumnIndices.Add(pos.x);
                    }
                    else
                    {
                        _selectedColumnIndices.Remove(pos.x);
                    }
                    SelectRowsColumns(selectedIndices, _selectedColumnIndices);
                }
                else if ((modifiers & EventModifiers.Shift) == EventModifiers.Shift)
                {
                    if (_selectedColumnIndices.Count == 0 || pos.x == _selectedColumnIndices[0])
                    {
                        _selectedColumnIndices.Add(pos.x);
                    }
                    else if (pos.x < _selectedColumnIndices[0])
                    {
                        _selectedColumnIndices.RemoveRange(1, _selectedColumnIndices.Count - 1);
                        for (int i = pos.x; i < _selectedColumnIndices[0]; i++)
                        {
                            _selectedColumnIndices.Add(i);
                        }
                    }
                    else
                    {
                        _selectedColumnIndices.RemoveRange(1, _selectedColumnIndices.Count - 1);
                        for (int i = _selectedColumnIndices[0] + 1; i <= pos.x; i++)
                        {
                            _selectedColumnIndices.Add(i);
                        }
                    }
                    SelectRowsColumns(selectedIndices, _selectedColumnIndices);
                }
                else
                {
                    SelectRowsColumns(selectedIndices, new List<int>() { pos.x });
                }
            }
            else
            {
#if false
                if ((modifiers & EventModifiers.Control) == EventModifiers.Control)
                {
                    /*if (!selectedIndices.Contains(pos.y))
                    {
                        _listView.AddToSelection(pos.y);
                    } else
                    {
                        selectedIndices.Remove(pos.y);
                        _listView.Clear();
                        foreach (var row in selectedIndices)
                        {
                            _listView.AddToSelection(row);
                        }
                    }*/
                    if (!_selectedColumnIndices.Contains(pos.x))
                    {
                        _selectedColumnIndices.Add(pos.x);
                    }
                    else
                    {
                        _selectedColumnIndices.Remove(pos.x);
                    }
                    SelectRowsColumns(selectedIndices, _selectedColumnIndices);
                }
                else if ((modifiers & EventModifiers.Shift) == EventModifiers.Shift)
                {
                    if (selectedIndices.Count == 0 || pos.y == selectedIndices[0])
                    {
                        selectedIndices = new List<int>() { pos.y };
                    }
                    else if (pos.y < selectedIndices[0])
                    {
                        selectedIndices.RemoveRange(1, selectedIndices.Count - 1);
                        for (int i = pos.y; i <= selectedIndices[0]; i++)
                        {
                            selectedIndices.Add(i);
                        }
                    }
                    else
                    {
                        selectedIndices.RemoveRange(1, selectedIndices.Count - 1);
                        for (int i = selectedIndices[0]; i <= pos.y; i++)
                        {
                            selectedIndices.Add(i);
                        }
                    }
                    if (_selectedColumnIndices.Count == 0 || pos.x == _selectedColumnIndices[0])
                    {
                        _selectedColumnIndices.Add(pos.x);
                    }
                    else if (pos.x < _selectedColumnIndices[0])
                    {
                        _selectedColumnIndices.RemoveRange(1, _selectedColumnIndices.Count - 1);
                        for (int i = pos.x; i < _selectedColumnIndices[0]; i++)
                        {
                            _selectedColumnIndices.Add(i);
                        }
                    }
                    else
                    {
                        _selectedColumnIndices.RemoveRange(1, _selectedColumnIndices.Count - 1);
                        for (int i = _selectedColumnIndices[0] + 1; i <= pos.x; i++)
                        {
                            _selectedColumnIndices.Add(i);
                        }
                    }
                    SelectRowsColumns(selectedIndices, _selectedColumnIndices);
                }
                else
#endif
                {
                    SelectRowsColumns(new List<int>() { pos.y }, new List<int>() { pos.x });
                }
            }
        }
        private void OnMouseUp(Vector2 mousePosition) {
            var pos = GetCellPos(mousePosition);
            if (_lastCellPos.x < 0)
            {
                if (!_listView.selectedIndices.ToArray().Contains(pos.y))
                {
                    SelectRowsColumns(new List<int>() { pos.y }, _selectedColumnIndices);
                }
            }
            else if (_lastCellPos.y < 0)
            {
                if (_listView.selectedIndices.Count() < _lastValues.Count)
                {
                    SelectRowsColumns(_listView.selectedIndices.ToList(), new List<int>() { pos.x });
                }
            }
            else
            {
                if (!_listView.selectedIndices.ToArray().Contains(pos.y))
                {
                    SelectRowsColumns(new List<int>() { pos.y }, new List<int>() { pos.x });
                }
            }
            var menu = new GenericMenu();
            //Edit
            menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1459")), false, () =>
            {
                ShowAddonParameterEditWindow(_listView.selectedIndex);
            });

            var validSelectedIndexCount = _listView.selectedIndices.Count();
            if (_listView.selectedIndices.ToList().IndexOf(_lastValues.Count) >= 0)
            {
                validSelectedIndexCount--;
            }
            var allRowsSelected = (_lastValues.Count > 0 && validSelectedIndexCount >= _lastValues.Count);
            //Cut
            if (allRowsSelected || (_lastCellPos.x < 0 && _listView.selectedIndex < _lastValues.Count) || (_lastCellPos.y < 0 && (_selectedColumnIndices.Count >= 2 || (_selectedColumnIndices.Count > 0 && _selectedColumnIndices[0] < _columnCount))))
            {
                menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1461")), false, () =>
                {
                    if (allRowsSelected || _lastCellPos.x < 0)
                    {
                        _localClipBoardRow = CutCopyRow(true, _listView.selectedIndices.ToArray());
                        _localClipBoardColumn = null;
                    }
                    else if (_lastCellPos.y < 0)
                    {
                        _localClipBoardRow = null;
                        _localClipBoardColumn = CutCopyColumn(true, _selectedColumnIndices);
                    }
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1461")));
            }
            //Copy
            if (allRowsSelected || (_lastCellPos.x < 0 && _listView.selectedIndex < _lastValues.Count) || (_lastCellPos.y < 0 && (_selectedColumnIndices.Count >= 2 || (_selectedColumnIndices.Count > 0 && _selectedColumnIndices[0] < _columnCount))))
            {
                menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1462")), false, () =>
                {
                    if (allRowsSelected || _lastCellPos.x < 0)
                    {
                        _localClipBoardRow = CutCopyRow(false, _listView.selectedIndices.ToArray());
                        _localClipBoardColumn = null;
                    }
                    else if (_lastCellPos.y < 0)
                    {
                        _localClipBoardRow = null;
                        _localClipBoardColumn = CutCopyColumn(false, _selectedColumnIndices);
                    }
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1462")));
            }
            //Paste
            if ((_lastCellPos.x < 0 && _localClipBoardRow != null) || (_lastCellPos.y < 0 && _localClipBoardColumn != null))
            {
                menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1463")), false, () =>
                {
                    if (_lastCellPos.x < 0)
                    {
                        PasteRow(_listView.selectedIndex, _localClipBoardRow);
                    }
                    else if (_lastCellPos.y < 0)
                    {
                        PasteColumn((_selectedColumnIndices.Count == 0 ? 0 : _selectedColumnIndices[0]), _localClipBoardColumn);
                    }
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1463")));
            }
            //Delete
            if (allRowsSelected || (_lastCellPos.x < 0 && _listView.selectedIndex < _lastValues.Count) || (_lastCellPos.y < 0 && (_selectedColumnIndices.Count >= 2 || (_selectedColumnIndices.Count > 0 && _selectedColumnIndices[0] < _columnCount))))
            {
                menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0383")), false, () =>
                {
                    if (allRowsSelected || _lastCellPos.x < 0)
                    {
                        CutCopyRow(true, _listView.selectedIndices.ToArray());
                    }
                    else if (_lastCellPos.y < 0)
                    {
                        CutCopyColumn(true, _selectedColumnIndices);
                    }
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
            //Save CSV
            menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_2572")), false, () =>
            {
                var filename = EditorUtility.SaveFilePanel(EditorLocalize.LocalizeText("WORD_2572"), "", "table.csv", "csv");
                if (filename.Length == 0)
                {
                    return;
                }
                var table = new string[_lastValues.Count, _columnCount];
                for (int i = 0; i < _lastValues.Count; i++)
                {
                    var jsonArr = JSON.Parse(_lastValues[i]).AsArray;
                    for (int j = 0; j < jsonArr.Count; j++)
                    {
                        if (_paramType == ParamType.String || _paramType == ParamType.MultilineString || _paramType == ParamType.Note)
                        {
                            table[i, j] = jsonArr[j].Value;
                        }
                        else
                        {
                            table[i, j] = jsonArr[j].ToString();
                        }
                    }
                }
                ////Copy as CSV
                //var csv = SimpleCsv.ConvertAsCSV(table);
                //if (csv != null)
                //{
                //GUIUtility.systemCopyBuffer = csv;
                //}
                SimpleCsv.WriteCsv(filename, table);
            });
            //Load CSV
            menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_2573")), false, () =>
            {
                var filename = EditorUtility.OpenFilePanel(EditorLocalize.LocalizeText("WORD_2573"), "", "csv");
                if (filename.Length == 0)
                {
                    return;
                }
                    ////Paste as CSV
                    //var table = SimpleCsv.ParseCSV(GUIUtility.systemCopyBuffer);
                var table = SimpleCsv.ReadCsv(filename);
                if (table != null)
                {
                    var rowCount = table.GetLength(0);
                    var columnCount = table.GetLength(1);
                    _lastValues = new string[rowCount].ToList();
                    var typeName = (_structName != null) ? $"struct<{_structName}>" : AddonManager.GetTypeName(_paramType);
                    for (int i = 0; i < rowCount; i++)
                    {
                        var columns = new List<string>();
                        for (int j = 0; j < columnCount; j++)
                        {
                            if (_paramType == ParamType.String || _paramType == ParamType.MultilineString || _paramType == ParamType.Note)
                            {
                                table[i, j] = DataConverter.GetJsonString(table[i, j] ?? "");
                            }
                            columns.Add(AddonManager.ValidateValue(_addonInfo, _paramInfo, typeName, table[i, j] ?? ""));
                        }
                        _lastValues[i] = AddonManager.GetJsonFromStringList(columns);// DataConverter.GetJsonStringArray(columns.ToArray());
                    }
                    _lastValue = AddonManager.GetJsonFromStringList(_lastValues);// DataConverter.GetJsonStringArray(_lastValues.ToArray());
                    _textField.value = _lastValue;
                    _parameters.SetParameterValue(_paramInfo.name, _lastValue);
                    _modified = true;
                        //_listView.Rebuild();
                    UpdateTypeValueWindow(_typeValueWindow);
                    SelectRowsColumns(new List<int>() { 0 }, new List<int>() { 0 });
                    AddonEditorWindowManager.instance.CloseDescendantWindows(this);
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
        }
        private ListView CreateListView() {
            StripedListView<string> listView = null;
            Action<VisualElement, int> bindItem = (e, i) =>
            {
                if (i == 0)
                {
                    var scrollView = listView.Query<ScrollView>().First();
                    scrollView.horizontalScroller.valueChanged += (value) =>
                    {
                        _columnVe.style.left = -value;
                    };
                    scrollView.verticalScroller.valueChanged += (value) =>
                    {
                        _rowVe.style.top = -value;
                    };
                    /*scrollView.contentContainer.RegisterCallback<GeometryChangedEvent>((evt) =>
                    {
                        _columnVe.style.left = -scrollView.scrollOffset.x;
                        _rowVe.style.top = -scrollView.scrollOffset.y;
                    });*/

                }
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

                    // Value
                    if (index < _lastValues.Count) {
                        var columns = AddonManager.GetStringListFromJson(_lastValues[index]);// DataConverter.GetStringArrayFromJson(_lastValues[index]);
                        for (int j = 0; j <= _columnCount; j++) {
                            var value = (j < columns.Count) ? columns[j] : "";
                            var valueLabel = new Label((j < columns.Count) ? AddonUIUtil.GetEasyReadableText(_addonInfo, _paramInfo, (_structName != null) ? $"struct<{_structName}>" : AddonManager.GetTypeName(_paramType), value) : "");
                            //valueLabel.AddToClassList("list_view_item_value_label");
                            valueLabel.style.width = CellWidth;
                            valueLabel.AddToClassList("text_ellipsis");
                            visualElement.Add(valueLabel);
                        }
                    } else {
                        var value = (index < _lastValues.Count) ? _lastValues[index] : "";
                        var valueLabel = new Label(value);
                        //valueLabel.AddToClassList("list_view_item_value_label");
                        valueLabel.style.width = CellWidth;
                        valueLabel.AddToClassList("text_ellipsis");
                        visualElement.Add(valueLabel);
                    }

                    e.Add(visualElement);
                }
            };

            var list = new List<int>();
            for (int i = 0; i < _lastValues.Count + 1; i++)
            {
                list.Add(i);
            }
            //listView.itemsSource = list;
            listView = new StripedListView<string>(list, 16, () => new Label(), bindItem);
            listView.AddToClassList("list_view_array2");
            listView.SolidQuad();
            listView.name = "list";
            listView.selectionType = SelectionType.Multiple;
            listView.reorderable = true;

            listView.selectedIndex = 0;

            listView.RegisterCallback<KeyDownEvent>((e) =>
            {
                //Debug.Log($"pressed: '{e.character}'");
                switch (e.keyCode)
                {
                    case KeyCode.KeypadEnter:
                    case KeyCode.Return:
                        var pos = listView.WorldToLocal(new Rect(e.originalMousePosition, Vector2.one));

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

                    case KeyCode.Delete:
                        if (listView.selectedIndices.Count() >= _lastValues.Count || _lastCellPos.x < 0)
                        {
                            CutCopyRow(true, _listView.selectedIndices.ToArray());
                        }
                        else if (_lastCellPos.y < 0)
                        {
                            CutCopyColumn(true, _selectedColumnIndices);
                        }
                        break;
                }
            });
            listView.onItemsChosen += (objects) =>
            {
                if (_lastCellPos.x < 0 || _lastCellPos.y < 0) return;
                var list = objects.ToList();
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
                _lastValue = AddonManager.GetJsonFromStringList(_lastValues);// DataConverter.GetJsonStringArray(_lastValues.ToArray());
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
            listView.RegisterCallback<MouseMoveEvent>((evt) =>
            {
                OnMouseMove(new Vector2(evt.localMousePosition.x + RowNumberWidth, evt.localMousePosition.y + CellHeight));
            });
            listView.RegisterCallback<MouseDownEvent>((evt) =>
            {
                if (evt.button == (int) MouseButton.RightMouse)
                {
                    return;
                }
                OnMouseDown(new Vector2(evt.localMousePosition.x + RowNumberWidth, evt.localMousePosition.y + CellHeight), evt.modifiers);
            });
            listView.RegisterCallback<FocusInEvent>((evt) =>
            {
                UpdateColumnSelectMarker(true);
            });
            listView.RegisterCallback<FocusOutEvent>((evt) =>
            {
                UpdateColumnSelectMarker(false);
            });
            listView.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (evt.button != (int) MouseButton.RightMouse)
                {
                    return;
                }
                OnMouseUp(new Vector2(evt.localMousePosition.x + RowNumberWidth, evt.localMousePosition.y + CellHeight));
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

        public class CellPos {
            public int x;
            public int y;
            public CellPos(int x, int y) {
                this.x = x;
                this.y = y;
            }
        }
        private CellPos GetCellPos(Vector2 localMousePosition) {
            var x = localMousePosition.x;
            var y = localMousePosition.y;
            var posX = (int)Math.Floor((x - RowNumberWidth) / CellWidth);
            var posY = (int)Math.Floor((y - CellHeight) / CellHeight);
            _lastCellPos = new CellPos(posX, posY);
            posX = Math.Max(0, Math.Min(_columnCount, posX));
            posY = Math.Max(0, Math.Min(_lastValues.Count, posY));
            return new CellPos(posX, posY);
        }


        private void FocusColumn(int columnIndex) {
            if (_focusColumnIndex == columnIndex)
            {
                return;
            }
            _focusColumnIndex = columnIndex;
            _columnFocusMarker.style.left = RowNumberWidth + columnIndex * CellWidth;
            _columnFocusMarker.style.width = CellWidth;
            UpdateFocusState();
        }

        private void SelectRowsColumns(List<int> rowIndices, List<int> columnIndices) {
            var differentRows = (rowIndices.Count != _listView.selectedIndices.Count());
            foreach (var index in _listView.selectedIndices)
            {
                if (!rowIndices.Contains(index))
                {
                    differentRows = true;
                    break;
                }
            }
            if (differentRows)
            {
                _listView.ClearSelection();
                foreach (var row in rowIndices)
                {
                    _listView.AddToSelection(row);
                }
            }
            _listView.Focus();
            _selectedColumnIndices = columnIndices;
            UpdateSelectedMarker();
            UpdateFocusState();
        }

        private void UpdateSelectedMarker() {
            while (_columnSelectMarkers.Count > _selectedColumnIndices.Count)
            {
                var marker = _columnSelectMarkers[_columnSelectMarkers.Count - 1];
                _columnSelectMarkers.Remove(marker);
                _columnSelectMarkerContainer.Remove(marker);
            }
            while (_columnSelectMarkers.Count < _selectedColumnIndices.Count)
            {
                var marker = new VisualElement();
                marker.AddToClassList("column_select_marker_unselected");
                marker.AddToClassList("overflow_hidden");
                marker.focusable = false;
                //marker.SetEnabled(false);
                _columnSelectMarkerContainer.Add(marker);
                _columnSelectMarkers.Add(marker);
            }
            for (int i = 0; i < _selectedColumnIndices.Count; i++)
            {
                var marker = _columnSelectMarkers[i];
                marker.style.left = RowNumberWidth + _selectedColumnIndices[i] * CellWidth;
                marker.style.width = CellWidth;
            }
        }

        private void UpdateFocusState() {
            if (_selectedColumnIndices.Contains(_focusColumnIndex))
            {
                _columnFocusMarker.style.display = DisplayStyle.None;
            }
            else
            {
                _columnFocusMarker.style.display = DisplayStyle.Flex;
            }
        }

        private void UpdateColumnSelectMarker(bool listViewFocused) {
            foreach (var marker in _columnSelectMarkers)
            {
                marker.AddToClassList(listViewFocused ? "column_select_marker_selected" : "column_select_marker_unselected");
            }
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
            var archive = DataConverter.GetJsonStringArray(copyArr);
            if (doCut)
            {
                for (int i = rowIndices.Length - 1; i >= 0; i--)
                {
                    _lastValues.RemoveAt(rowIndices[i]);
                }
                _lastValue = AddonManager.GetJsonFromStringList(_lastValues);// DataConverter.GetJsonStringArray(_lastValues.ToArray());
                _textField.value = _lastValue;
                _parameters.SetParameterValue(_paramInfo.name, _lastValue);
                _modified = true;
                //_listView.Rebuild();
                UpdateTypeValueWindow(_typeValueWindow);
                SelectRowsColumns(new List<int>() { rowIndices[0] }, new List<int>() { _selectedColumnIndices[0] });
                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
            }
            return archive;
        }

        private string CutCopyColumn(bool doCut, List<int> columnIndices) {
            var arr = columnIndices.ToArray();
            Array.Sort(arr);
            columnIndices = arr.ToList();
            var rows = new List<string>();
            var typeName = (_structName != null) ? $"struct<{_structName}>" : AddonManager.GetTypeName(_paramType);
            for (int i = 0; i < _lastValues.Count; i++)
            {
                var columns = AddonManager.GetStringListFromJson(_lastValues[i]);// DataConverter.GetStringArrayFromJson(_lastValues[i]).ToList();
                var newColumns = new List<string>();
                foreach (var columnIndex in columnIndices)
                {
                    if (columnIndex < columns.Count)
                    {
                        newColumns.Add(columns[columnIndex]);
                    } else
                    {
                        newColumns.Add(AddonManager.ValidateValue(_addonInfo, _paramInfo, typeName, ""));
                    }
                }
                if (doCut)
                {
                    for (int j = columnIndices.Count - 1; j >= 0; j--)
                    {
                        columns.RemoveAt(columnIndices[j]);
                    }
                    _lastValues[i] = AddonManager.GetJsonFromStringList(columns);// DataConverter.GetJsonStringArray(columns.ToArray());
                }
                rows.Add(DataConverter.GetJsonStringArray(newColumns.ToArray()));
            }
            var archive = DataConverter.GetJsonStringArray(rows.ToArray());
            if (doCut)
            {
                _lastValue = AddonManager.GetJsonFromStringList(_lastValues);// DataConverter.GetJsonStringArray(_lastValues.ToArray());
                _textField.value = _lastValue;
                _parameters.SetParameterValue(_paramInfo.name, _lastValue);
                _modified = true;
                //_listView.Rebuild();
                var selectedIndex = _listView.selectedIndex;
                UpdateTypeValueWindow(_typeValueWindow);
                SelectRowsColumns(new List<int>() { selectedIndex }, new List<int>() { columnIndices[0] });
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
            UpdateTypeValueWindow(_typeValueWindow);
            SelectRowsColumns(new List<int>() { rowIndex }, new List<int>() { _selectedColumnIndices[0] });
            AddonEditorWindowManager.instance.CloseDescendantWindows(this);
        }

        private void PasteColumn(int columnIndex, string archive) {
            var rows = DataConverter.GetStringArrayFromJson(archive);
            if (rows.Length != _lastValues.Count)
            {
                //Mismatch
                return;
            }
            var typeName = (_structName != null) ? $"struct<{_structName}>" : AddonManager.GetTypeName(_paramType);
            for (int i = 0; i < _lastValues.Count; i++)
            {
                var columns = AddonManager.GetStringListFromJson(_lastValues[i]);// DataConverter.GetStringArrayFromJson(_lastValues[i]).ToList();
                while (columns.Count < columnIndex)
                {
                    columns.Add(AddonManager.ValidateValue(_addonInfo, _paramInfo, typeName, ""));

                }
                var addColumns = DataConverter.GetStringArrayFromJson(rows[i]).ToList();
                columns.InsertRange(columnIndex, addColumns);
                _lastValues[i] = AddonManager.GetJsonFromStringList(columns);// DataConverter.GetJsonStringArray(columns.ToArray());
            }
            _lastValue = AddonManager.GetJsonFromStringList(_lastValues);// DataConverter.GetJsonStringArray(_lastValues.ToArray());
            _textField.value = _lastValue;
            _parameters.SetParameterValue(_paramInfo.name, _lastValue);
            _modified = true;
            //_listView.Rebuild();
            var selectedIndex = _listView.selectedIndex;
            UpdateTypeValueWindow(_typeValueWindow);
            SelectRowsColumns(new List<int>() { selectedIndex }, new List<int>() { columnIndex });
            AddonEditorWindowManager.instance.CloseDescendantWindows(this);
        }

        private string[] GetCellTexts(int rowIndex, int columnIndex) {
            var list = new List<string>();
            var columns = AddonManager.GetStringListFromJson(_lastValues[rowIndex]);
            if (columnIndex < columns.Count)
            {
                list.Add(columns[columnIndex]);
                list.Add(AddonUIUtil.GetEasyReadableText(_addonInfo, _paramInfo, (_structName != null) ? $"struct<{_structName}>" : AddonManager.GetTypeName(_paramType), columns[columnIndex]));
            }
            return list.ToArray();
        }
        public bool FindNext() {
            var rowIndex = _listView.selectedIndex;
            var columnIndex = (_selectedColumnIndices.Count == 0 ? 0 : _selectedColumnIndices[0]);
            while (true)
            {
                columnIndex++;
                if (columnIndex >= _columnCount)
                {
                    columnIndex = 0;
                    rowIndex++;
                }
                if (rowIndex >= _lastValues.Count)
                {
                    break;
                }
                if (_finderInfo.IsMatch(GetCellTexts(rowIndex, columnIndex)))
                {
                    SelectRowsColumns(new List<int>() { rowIndex }, new List<int>() { columnIndex });
                    return true;
                }
            }
            return false;
        }

        public bool FindPrev() {
            var rowIndex = _listView.selectedIndex;
            var columnIndex = (_selectedColumnIndices.Count == 0 ? 0 : _selectedColumnIndices[0]);
            while (true)
            {
                columnIndex--;
                if (columnIndex < 0)
                {
                    columnIndex = _columnCount - 1;
                    rowIndex--;
                }
                if (rowIndex < 0)
                {
                    break;
                }
                if (_finderInfo.IsMatch(GetCellTexts(rowIndex, columnIndex)))
                {
                    SelectRowsColumns(new List<int>() { rowIndex }, new List<int>() { columnIndex });
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

        private void ShowAddonParameterEditWindow(int index) {
            var parameters = new AddonParameterContainer();
            string lastValue = null;
            List<string> columns = new List<string>();
            var modified = (index >= _lastValues.Count);
            var columnIndex = (_selectedColumnIndices.Count == 0 ? 0 : _selectedColumnIndices[0]);
            if (index < _lastValues.Count) {
                columns = AddonManager.GetStringListFromJson(_lastValues[index]);// DataConverter.GetStringArrayFromJson(_lastValues[index]).ToList();
                if (columns.Count > columnIndex)
                {
                    lastValue = columns[columnIndex];
                } else
                {
                    modified = true;
                }
            }
            if (modified)
            {
                var typeName = (_paramType == ParamType.Struct) ? $"struct<{_structName}>" : AddonManager.GetTypeName(_paramType);
                lastValue = AddonManager.ValidateValue(_addonInfo, _paramInfo, typeName, "");
            }
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
                    var typeName = (_structName != null) ? $"struct<{_structName}>" : AddonManager.GetTypeName(_paramType);
                    for (int i = columns.Count; i < columnIndex; i++)
                    {
                        columns.Add(AddonManager.ValidateValue(_addonInfo, _paramInfo, typeName, ""));
                    }
                    if (columns.Count <= columnIndex)
                    {
                        columns.Add(null);
                    }
                    columns[columnIndex] = newValue;
                    if (_lastValues.Count <= index)
                    {
                        _lastValues.Add(null);
                        var list = (_listView.itemsSource as List<int>);
                        list.Add(list.Count);
                        _listView.itemsSource = list;
                    }
                    _lastValues[index] = AddonManager.GetJsonFromStringList(columns);// DataConverter.GetJsonStringArray(columns.ToArray());
                    _lastValue = AddonManager.GetJsonFromStringList(_lastValues);// DataConverter.GetJsonStringArray(_lastValues.ToArray());
                    _textField.value = _lastValue;
                    _parameters.SetParameterValue(_paramInfo.name, _lastValue);
                    _modified = true;
                    //_listView.Rebuild();
                    UpdateTypeValueWindow(_typeValueWindow);
                    SelectRowsColumns(new List<int>() { index }, new List<int>() { columnIndex });
                    AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                }
            }, modified);
        }

    }

    public class SimpleCsv {
        private static char Comma = ',';
        private static char DoubleQuote = '"';
        private static string DoubleQuote2 = "\"\"";
        private static char NL = '\n';
        private static char CR = '\r';
        public static string[,] ReadCsv(string filename) {
            string text = null;
            try
            {
                text = File.ReadAllText(filename);
            }
            catch (IOException)
            {
                Debug.LogError($"Failed to read {filename}");
                return null;
            }
            return ParseCSV(text);
        }

        public static string[,] ParseCSV(string text) {
            int head = 0;
            var rows = new List<string[]>();
            while (head < text.Length)
            {
                var cellHead = true;
                var columns = new List<string>();
                while (head < text.Length)
                {
                    if (text[head] == CR)
                    {
                        head++;
                        continue;
                    }
                    if (text[head] == NL)
                    {
                        if (cellHead)
                        {
                            columns.Add("");
                            head++;
                            break;
                        }
                        cellHead = true;
                        head++;
                        break;
                    }
                    if (text[head] == Comma)
                    {
                        if (cellHead)
                        {
                            columns.Add("");
                            head++;
                            continue;
                        }
                        cellHead = true;
                        head++;
                        continue;
                    }
                    if (text[head] == DoubleQuote)
                    {
                        head++;
                        if (!cellHead)
                        {
                            Debug.LogError($"Bad csv: need comma or new line: {rows.ToString()}, {columns.ToString()}");
                            return null;
                        }
                        string cell = "";
                        var closed = false;
                        while (head < text.Length)
                        {
                            if (text[head] == CR)
                            {
                                head++;
                                continue;
                            }
                            if (head + 1 < text.Length && text.Substring(head, 2) == DoubleQuote2)
                            {
                                head += 2;
                                cell += DoubleQuote;
                                continue;
                            }
                            if (text[head] == DoubleQuote)
                            {
                                head++;
                                closed = true;
                                break;
                            }
                            cell += text[head++];
                        }
                        if (!closed)
                        {
                            Debug.LogError($"Bad csv: double quote not closed: {rows.ToString()}, {columns.ToString()}, {cell}");
                            return null;
                        }
                        columns.Add(cell);
                        cellHead = false;
                        continue;
                    }
                    {
                        string cell = "";
                        while (head < text.Length)
                        {
                            if (text[head] == CR)
                            {
                                head++;
                                continue;
                            }
                            if (text[head] == Comma || text[head] == NL)
                            {
                                break;
                            }
                            cell += text[head++];
                        }
                        columns.Add(cell);
                        cellHead = false;
                    }
                }
                rows.Add(columns.ToArray());
            }
            int maxColumnCount = 1;
            foreach (var row in rows)
            {
                maxColumnCount = Math.Max(maxColumnCount, row.Length);
            }
            var table = new string[rows.Count, maxColumnCount];
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                for (int j = 0; j < maxColumnCount; j++)
                {
                    table[i, j] = (j < row.Length) ? row[j] : "";
                }
            }
            return table;
        }


        public static string ConvertAsCSV(string[,] table) {
            var rowCount = table.GetLength(0);
            var columnCount = table.GetLength(1);
            var rows = new List<string>();
            for (int i = 0; i < rowCount; i++)
            {
                int tail = 0;
                for (int j = columnCount - 1; j >= 0; j--)
                {
                    if (table[i, j] != null && table[i, j].Length > 0)
                    {
                        tail = j;
                        break;
                    }
                }
                var columns = new List<string>();
                for (int j = 0; j <= tail; j++)
                {
                    var cell = table[i, j];
                    if (cell == null || cell.Length == 0)
                    {
                        columns.Add("");
                    } else
                    {
                        if (cell.IndexOf(Comma) >= 0 || cell.IndexOf(DoubleQuote) >= 0 || cell.IndexOf(CR) >= 0 || cell.IndexOf(NL) >= 0)
                        {
                            var s = "";
                            for (int k = 0; k < cell.Length; k++)
                            {
                                if (cell[k] == DoubleQuote)
                                {
                                    s += DoubleQuote2;
                                } else
                                {
                                    s += cell[k];
                                }
                            }
                            columns.Add($"\"{s}\"");
                        } else
                        {
                            columns.Add(cell);
                        }
                    }
                }
                rows.Add(string.Join(Comma, columns));
            }
            return string.Join(NL, rows);
        }

        public static bool WriteCsv(string filename, string[,] table) {
            var text = ConvertAsCSV(table);
            try
            {
                File.WriteAllText(filename, text);
            }
            catch (IOException)
            {
                Debug.LogError($"Failed to write {filename}");
                return false;
            }
            return true;
        }
    }
}
