using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Runtime.Addon;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Common.AddonUIUtil
{
    public class StripedListView<T> : ListView
    {
        private static Vertex[] _vertices;
        private static ushort[] _indices;

        private static readonly Color[] _stripedColors =
            {new Color(0.22f, 0.22f, 0.22f, 0.5f), new Color(0.19f, 0.19f, 0.19f, 0.5f)};

        private static StyleColor[] _stripedStyleColors =
        {
            new StyleColor(new Color(0.22f, 0.22f, 0.22f, 0.5f)), new StyleColor(new Color(0.19f, 0.19f, 0.19f, 0.5f))
        };

        //int itemHeight;
        private float _lastHeight = -1;
        private int   itemCount;

        private readonly Dictionary<T, Toggle> keyToggleDic;

        public StripedListView(
            IList itemSource,
            int itemHeight,
            Func<VisualElement> makeItem,
            Action<VisualElement, int> bindItem
        )
            :
            base(itemSource, itemHeight, makeItem, bindItem) {
            itemCount = itemSource.Count;
            this.fixedItemHeight = itemHeight;
            keyToggleDic = new Dictionary<T, Toggle>();

            RegisterCallback<MouseMoveEvent>(evt =>
            {
                var scrollView = hierarchy[0] as ScrollView;
                if ((evt.pressedButtons & 3) != 0)
                {
                    var mouseRect = new Rect(evt.mousePosition, Vector2.one);
                    var index = (int) ((evt.mousePosition.y + scrollView.scrollOffset.y) / itemHeight) - 1;
                    if (index >= 0 && index < itemSource.Count)
                        if (onDragIndexChanged != null)
                            onDragIndexChanged(index);
                }
            });
        }

        public event Action<int> onDragIndexChanged;

        public void RegisterToggle(T key, Toggle toggle) {
            if (keyToggleDic.ContainsKey(key))
            {
                keyToggleDic[key] = toggle;
                return;
            }

            keyToggleDic.Add(key, toggle);
        }

        public Toggle getToggle(T key) {
            if (!keyToggleDic.ContainsKey(key)) return null;
            //(this.itemsSource as List<T>).IndexOf(key);

            return keyToggleDic[key];
        }

        public void SetVisualElementStriped(VisualElement ve, int index) {
            /*Rect r = contentRect;
            ve.style.top = itemHeight * index;
            ve.style.left = 0;
            ve.style.width = r.width;
            ve.style.height = itemHeight;
            ve.style.backgroundColor = _stripedStyleColors[index & 1];*/
        }

        public void AddVisualElementStriped(VisualElement parent) {
            /*int viewLast = (int) ((contentRect.height + itemHeight - 1) / itemHeight);
            for (int i = itemsSource.Count; i <= viewLast; i++)
            {
                var ve = new VisualElement();
                SetVisualElementStriped(ve, i);
                parent.Add(ve);
            }*/
        }

        public void SolidQuad() {
            generateVisualContent += OnGenerateVisualContent;
        }


        private void OnGenerateVisualContent(MeshGenerationContext mgc) {
            var r = contentRect;
            if (r.width < 0.01f || r.height < 0.01f)
                return; // Skip rendering when too small.

            var count = (int) Math.Ceiling(r.height / fixedItemHeight);
            if (r.height > _lastHeight)
            {
                _vertices = null;
                _indices = null;
                _lastHeight = r.height;
            }

            if (_vertices == null || _vertices.Length < count * 4)
            {
                _vertices = new Vertex[count * 4];

                for (var i = 0; i < count; i++)
                {
                    var color = _stripedColors[i & 1];
                    for (var j = 0; j < 4; j++) _vertices[i * 4 + j].tint = color;
                }
            }

            float left = 0;
            var right = r.width;
            float top = 0;
            var bottom = r.height;
            for (var i = 0; i < count; i++)
            {
                var below = i == count - 1 ? bottom : top + fixedItemHeight * (i + 1);
                _vertices[i * 4 + 0].position = new Vector3(left, below, Vertex.nearZ);
                _vertices[i * 4 + 1].position = new Vector3(left, top + fixedItemHeight * i, Vertex.nearZ);
                _vertices[i * 4 + 2].position = new Vector3(right, top + fixedItemHeight * i, Vertex.nearZ);
                _vertices[i * 4 + 3].position = new Vector3(right, below, Vertex.nearZ);
            }

            if (_indices == null)
            {
                _indices = new ushort[count * 6];
                for (var i = 0; i < count; i++)
                {
                    _indices[i * 6 + 0] = (ushort) (i * 4 + 0);
                    _indices[i * 6 + 1] = (ushort) (i * 4 + 1);
                    _indices[i * 6 + 2] = (ushort) (i * 4 + 2);
                    _indices[i * 6 + 3] = (ushort) (i * 4 + 2);
                    _indices[i * 6 + 4] = (ushort) (i * 4 + 3);
                    _indices[i * 6 + 5] = (ushort) (i * 4 + 0);
                }
            }

            var mwd = mgc.Allocate(_vertices.Length, _indices.Length);
            mwd.SetAllVertices(_vertices);
            mwd.SetAllIndices(_indices);
        }
    }

    public class IndexVisualElement : VisualElement
    {
        public IndexVisualElement(int index) {
            Index = index;
        }

        public int Index { get; }
    }

    public class KeyVisualElement<T> : VisualElement
    {
        public KeyVisualElement(T key) {
            Key = key;
        }

        public T Key { get; }
    }

    public class TabbedMenuController
    {
        /* Define member variables*/
        private const string tabClassName                  = "tab";
        private const string currentlySelectedTabClassName = "currentlySelectedTab";

        private const string unselectedContentClassName = "unselectedContent";

        // Tab and tab content have the same prefix but different suffix
        // Define the suffix of the tab name
        private const string tabNameSuffix = "Tab";

        // Define the suffix of the tab content name
        private const string contentNameSuffix = "Content";

        private readonly VisualElement root;

        public TabbedMenuController(VisualElement root) {
            this.root = root;
        }

        public void RegisterTabCallbacks() {
            var tabs = GetAllTabs();
            tabs.ForEach(tab => { tab.RegisterCallback<ClickEvent>(TabOnClick); });
        }

        /* Method for the tab on-click event: 

           - If it is not selected, find other tabs that are selected, unselect them 
           - Then select the tab that was clicked on
        */
        private void TabOnClick(ClickEvent evt) {
            var clickedTab = evt.currentTarget as Label;
            if (!TabIsCurrentlySelected(clickedTab))
            {
                GetAllTabs().Where(
                    tab => tab != clickedTab && TabIsCurrentlySelected(tab)
                ).ForEach(UnselectTab);
                SelectTab(clickedTab);
            }
        }

        //Method that returns a Boolean indicating whether a tab is currently selected
        private static bool TabIsCurrentlySelected(Label tab) {
            return tab.ClassListContains(currentlySelectedTabClassName);
        }

        private UQueryBuilder<Label> GetAllTabs() {
            return root.Query<Label>(className: tabClassName);
        }

        /* Method for the selected tab: 
           -  Takes a tab as a parameter and adds the currentlySelectedTab class
           -  Then finds the tab content and removes the unselectedContent class */
        private void SelectTab(Label tab) {
            tab.AddToClassList(currentlySelectedTabClassName);
            var content = FindContent(tab);
            content.RemoveFromClassList(unselectedContentClassName);
        }

        /* Method for the unselected tab: 
           -  Takes a tab as a parameter and removes the currentlySelectedTab class
           -  Then finds the tab content and adds the unselectedContent class */
        private void UnselectTab(Label tab) {
            tab.RemoveFromClassList(currentlySelectedTabClassName);
            var content = FindContent(tab);
            content.AddToClassList(unselectedContentClassName);
        }

        // Method to generate the associated tab content name by for the given tab name
        private static string GenerateContentName(Label tab) {
            return tab.name.Replace(tabNameSuffix, contentNameSuffix);
        }

        // Method that takes a tab as a parameter and returns the associated content element
        private VisualElement FindContent(Label tab) {
            return root.Q(GenerateContentName(tab));
        }
    }

    public class AddonBaseModalWindow : EditorWindow
    {
        public delegate void CallBackWidow(object data);

        protected static        Action                  _okAction;
        private static readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private static          Action                  _delayedAction;
        private static          double                  _delayedTime = 1;

        protected CallBackWidow _callBackWindow;
        protected virtual string ModalUxml { get; }
        protected virtual string ModalUss { get; }

        public virtual void ShowWindow(string modalTitle, CallBackWidow callBack) {
            var wnd = GetWindow<AddonBaseModalWindow>();

            if (callBack != null) _callBackWindow = callBack;

            wnd.titleContent = new GUIContent(modalTitle);
            wnd.ShowModal();
            wnd.Init();
        }

        public virtual void Init() {
        }

        protected virtual void CloseWindow() {
            Close();
        }

        public virtual void CallbackSelectedMapData(string scenename, int x, int y) {
        }

        public Action RegisterOkAction(Action action) {
            _okAction = action;
            rootVisualElement.focusable = true;
            rootVisualElement.RegisterCallback<KeyDownEvent>(e =>
            {
                switch (e.keyCode)
                {
                    case KeyCode.KeypadEnter:
                    case KeyCode.Return:
                        if ((e.modifiers & EventModifiers.Control) == EventModifiers.Control)
                        {
                            var buttonCancel = rootVisualElement.Query<Button>("Common_Button_Cancel").AtIndex(0);
                            buttonCancel.Focus();
                            SetDelayedAction(() =>
                            {
                                if (_okAction != null) _okAction();
                            });
                        }

                        break;

                    case KeyCode.Escape:
                        Close();
                        break;
                }
            });
            return _okAction;
        }

        public static void SetDelayedAction(Action action, double milliseconds = 1) {
            _delayedAction = action;
            _delayedTime = milliseconds;
            _ = DelayedAsync(_cancellationTokenSource.Token);
        }

        private static async Task DelayedAsync(CancellationToken token) {
            await Task.Delay(TimeSpan.FromMilliseconds(_delayedTime), token);
            var action = _delayedAction;
            _delayedAction = null;
            if (action != null) action.Invoke();
        }
    }

    public class AddonUIUtil
    {
        private static readonly bool            _checkCircularReference     = false;
        private static readonly HashSet<string> _validatingStructNameSet    = new HashSet<string>();
        private static readonly HashSet<string> _errorReportedStructNameSet = new HashSet<string>();

        public static void AddStyleEllipsisToPopupField(VisualElement popupField) {
            popupField[0].AddToClassList("text_ellipsis");
        }

        public static string GetEasyReadableText(
            AddonInfo addonInfo,
            AddonParamInfo paramInfo,
            string typeName,
            string value,
            object selectOption = null
        ) {
            var arrayDimension = 0;
            string structName = null;
            if (typeName == null) typeName = paramInfo.GetInfo("type")?.value ?? "string";
            var paramType = AddonManager.GetParamType(typeName, out arrayDimension, out structName);
            if (arrayDimension == 2)
            {
                var jsonNode = JSON.Parse(value);
                if (!jsonNode.IsArray) return value;
                var jsonArray = jsonNode.AsArray;
                var baseTypeName = typeName.Substring(0, typeName.Length - 4);
                var list1 = new List<string>();
                for (var i = 0; i < jsonArray.Count; i++)
                {
                    var list2 = new List<string>();
                    if (!jsonArray[i].IsArray)
                    {
                        list2.Add("[]");
                        continue;
                    }

                    var jsonArray2 = jsonArray[i].AsArray;
                    for (var j = 0; j < jsonArray2.Count; j++)
                        list2.Add(GetEasyReadableText(addonInfo, paramInfo, baseTypeName, jsonArray2[j].ToString()));
                    list1.Add($"[{string.Join(',', list2)}]");
                }

                return $"[{string.Join(',', list1)}]";
            }

            if (arrayDimension == 1)
            {
                var jsonNode = JSON.Parse(value);
                if (!jsonNode.IsArray) return "[]";
                var jsonArray = jsonNode.AsArray;
                var baseTypeName = typeName.Substring(0, typeName.Length - 2);
                var list1 = new List<string>();
                for (var i = 0; i < jsonArray.Count; i++)
                    list1.Add(GetEasyReadableText(addonInfo, paramInfo, baseTypeName, jsonArray[i].ToString()));
                return $"[{string.Join(',', list1)}]";
            }

            if (structName != null)
            {
                var structInfo = addonInfo.structInfos.GetStructInfo(structName);
                if (structInfo == null)
                {
                    Debug.LogWarning($"struct not declared: {structName}");
                    return "{}";
                }

                if (_checkCircularReference && _validatingStructNameSet.Contains(structName))
                {
                    if (!_errorReportedStructNameSet.Contains(structName))
                    {
                        _errorReportedStructNameSet.Add(structName);
                        Debug.LogError($"There is a circular reference to an element of ~struct~{structName}>");
                    }

                    return "{}";
                }

                _validatingStructNameSet.Add(structName);
                var jsonNode = JSON.Parse(value);
                if (!jsonNode.IsObject) jsonNode = new JSONObject();
                var jsonObject = jsonNode.AsObject;
                var list1 = new List<string>();
                foreach (var paramParamInfo in structInfo.params_)
                {
                    var key = paramParamInfo.name;
                    list1.Add(
                        $"\"{key}\":{GetEasyReadableText(addonInfo, paramParamInfo, paramParamInfo.GetInfo("type")?.value, jsonObject.HasKey(key) ? jsonObject[key].ToString() : "")}");
                }

                _validatingStructNameSet.Remove(structName);
                return "{" + string.Join(',', list1) + "}";
            }

            switch (paramType)
            {
                case ParamType.Select:
                {
                    foreach (var option in paramInfo.options) { 
                        if (option.value == value)
                        {
                            if ((selectOption is string) && (string) selectOption == "@value")
                            {
                                var info = paramInfo.GetInfo("valueSpecifiedKeys");
                                if (info != null && info.value.Split(',').Contains(option.key))
                                {
                                    break;
                                }
                            }
                            value = option.key;
                            break;
                        }
                    }
                    break;
                }

                case ParamType.CommonEvent:
                {
                    var eventManagementService = new EventManagementService();
                    var eventCommonDataModel =
                        eventManagementService.LoadEventCommon().FirstOrDefault(x => x.eventId == value);
                    if (value == null || value.Length == 0)
                    {
                        value = EditorLocalize.LocalizeText("WORD_0113");
                    }
                    else if (eventCommonDataModel != null)
                        value = $"{eventCommonDataModel.SerialNumberString} {eventCommonDataModel.name}";
                    else
                        value = "?";
                    break;
                }

                case ParamType.MapEvent:
                {
                    var noneStr = EditorLocalize.LocalizeText("WORD_0113");
                    var valueArray = DataConverter.GetStringArrayFromJson(value);
                    if (valueArray == null || valueArray.Length < 2)
                    {
                        value = $"{noneStr} {noneStr}";
                        break;
                    }

                    if (valueArray[0].Length == 0)
                    {
                        value = $"{noneStr} ";
                    }
                    else
                    {
                        var mapManagementService = new MapManagementService();
                        var mapEntity = mapManagementService.LoadMapById(valueArray[0]);
                        if (mapEntity == null)
                        {
                            value = "? ";
                        }
                        else
                        {
                            value = $"{mapEntity.SerialNumberString} {mapEntity.name} ";
                        }
                    }

                    if (valueArray[1].Length == 0)
                    {
                        value += $"{noneStr}";
                    }
                    else if (valueArray[1] == AddonManager.PlayerEventId)
                    {
                        value += EditorLocalize.LocalizeText("WORD_0860");
                    }
                    else if (valueArray[1] == AddonManager.ThisEventId)
                    {
                        value += EditorLocalize.LocalizeText("WORD_0920");
                    } else
                    {
                        //var nameList = new List<string>();
                        //var defaultIndex = -1;
                        //var index = 0;
                        var eventMapEntity = new EventManagementService().LoadEventMap()
                            .FirstOrDefault(x => x.mapId == valueArray[0] && x.eventId == valueArray[1]);
                        if (eventMapEntity != null)
                            value += $"{eventMapEntity.SerialNumberString} {eventMapEntity.name}";
                        else
                            value += "?";
                    }

                    break;
                }

                case ParamType.Switch:
                {
                    var databaseManagementService = new DatabaseManagementService();
                    var sw = databaseManagementService.LoadFlags().switches.FirstOrDefault(x => x.id == value);
                    if (sw == null) return EditorLocalize.LocalizeText("WORD_0113");
                    return $"{sw.SerialNumberString} {sw.name}";
                }

                case ParamType.Variable:
                {
                    var databaseManagementService = new DatabaseManagementService();
                    var v = databaseManagementService.LoadFlags().variables.FirstOrDefault(x => x.id == value);
                    if (v == null) return EditorLocalize.LocalizeText("WORD_0113");
                    return $"{v.SerialNumberString} {v.name}";
                }

                case ParamType.Animation:
                {
                    var animation = new DatabaseManagementService().LoadAnimation().FirstOrDefault(x => x.id == value);
                    if (animation == null) return EditorLocalize.LocalizeText("WORD_0113");
                    return $"{animation.SerialNumberString} {animation.particleName}";
                }

                case ParamType.Actor:
                {
                    var actor = new DatabaseManagementService().LoadCharacterActor()
                        .FirstOrDefault(x => x.uuId == value);
                    if (actor == null) return EditorLocalize.LocalizeText("WORD_0113");
                    return $"{actor.SerialNumberString} {actor.basic.name}";
                }

                case ParamType.Class:
                {
                    var class_ = new DatabaseManagementService().LoadCharacterActorClass()
                        .FirstOrDefault(x => x.id == value);
                    if (class_ == null) return EditorLocalize.LocalizeText("WORD_0113");
                    return $"{class_.SerialNumberString} {class_.basic.name}";
                }

                case ParamType.Skill:
                {
                    var skill = new DatabaseManagementService().LoadSkillCustom()
                        .FirstOrDefault(x => x.basic.id == value);
                    if (skill == null) return EditorLocalize.LocalizeText("WORD_0113");
                    return $"{skill.SerialNumberString} {skill.basic.name}";
                }

                case ParamType.Item:
                {
                    var item = new DatabaseManagementService().LoadItem().FirstOrDefault(x => x.basic.id == value);
                    if (item == null) return EditorLocalize.LocalizeText("WORD_0113");
                    return $"{item.SerialNumberString} {item.basic.name}";
                }

                case ParamType.Weapon:
                {
                    var weapon = new DatabaseManagementService().LoadWeapon().FirstOrDefault(x => x.basic.id == value);
                    if (weapon == null) return EditorLocalize.LocalizeText("WORD_0113");
                    return $"{weapon.SerialNumberString} {weapon.basic.name}";
                }

                case ParamType.Armor:
                {
                    var armor = new DatabaseManagementService().LoadArmor().FirstOrDefault(x => x.basic.id == value);
                    if (armor == null) return EditorLocalize.LocalizeText("WORD_0113");
                    return $"{armor.SerialNumberString} {armor.basic.name}";
                }

                case ParamType.Enemy:
                {
                    var enmey = new DatabaseManagementService().LoadEnemy().FirstOrDefault(x => x.id == value);
                    if (enmey == null) return EditorLocalize.LocalizeText("WORD_0113");
                    return $"{enmey.SerialNumberString} {enmey.name}";
                }

                case ParamType.Troop:
                {
                    var troop = new DatabaseManagementService().LoadTroop().FirstOrDefault(x => x.id == value);
                    if (troop == null) return EditorLocalize.LocalizeText("WORD_0113");
                    return $"{troop.SerialNumberString} {troop.name}";
                }

                case ParamType.State:
                {
                    var state = new DatabaseManagementService().LoadStateEdit().FirstOrDefault(x => x.id == value);
                    if (state == null) return EditorLocalize.LocalizeText("WORD_0113");
                    return $"{state.SerialNumberString} {state.name}";
                }

                case ParamType.Tileset:
                {
                    var tileset = new MapManagementService().LoadTileGroups().FirstOrDefault(x => x.id == value);
                    if (tileset == null) return EditorLocalize.LocalizeText("WORD_0113");
                    return $"{tileset.SerialNumberString} {tileset.name}";
                }
            }

            return value;
        }
    }

    public class ScrollTextField : ScrollView
    {
        private          int           _lastCursorIndex;
        private readonly VisualElement _rectVe;
        private readonly TextEditor    _textEditor;
        public           TextField     textField;

        public ScrollTextField(ScrollViewMode scrollViewMode)
            : base(scrollViewMode) {
            textField = new TextField();
            textField.multiline = true;
            textField.AddToClassList("textfield_align");
            _rectVe = new VisualElement();
            _rectVe.AddToClassList("rectangle_marker");
            textField.Add(_rectVe);

            textField.RegisterCallback<KeyUpEvent>(e => { cursorMovedCallback(); });
            textField[0].RegisterCallback<MouseUpEvent>(e => { cursorMovedCallback(); });
            textField[0].RegisterCallback<MouseMoveEvent>(e => { cursorMovedCallback(); });
            Add(textField);

            _textEditor = GetTextEditor(textField);
        }

        private static T GetPrivatePropertyValue<T>(object obj, string fieldName) {
            var eMapClassType = obj.GetType();
            var _privateProperty = eMapClassType.GetProperty(fieldName,
                BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance);
            return (T) _privateProperty.GetValue(obj);
        }

        // maybe depends on Unity Editor 2021LTS.
        private TextEditor GetTextEditor(TextField textField) {
            var textEditor = GetPrivatePropertyValue<TextEditor>(textField, "editorEngine");
            return textEditor;
        }

        private void cursorMovedCallback() {
            if (textField.cursorIndex != _lastCursorIndex)
            {
                _lastCursorIndex = textField.cursorIndex;
                if (_textEditor != null)
                {
                    _rectVe.style.left = _textEditor.graphicalCursorPos.x;
                    _rectVe.style.top = _textEditor.graphicalCursorPos.y;
                }

                EditorCoroutineUtility.StartCoroutine(ScrollToRectVe(), this);
            }
        }

        private IEnumerator ScrollToRectVe() {
            yield return new WaitForSeconds(1.0f);
            ScrollTo(_rectVe);
        }
    }

    public class ImScrollTextField : ImTextField {
        private Vector2 _scroll;
        private TextEditor _textEditor;
        private int _lastCursorIndex;
        private Vector2 _textAreaSize = new Vector2(100, 100);

        public ImScrollTextField(ScrollViewMode scrollViewMode)
            : base() {
            RemoveAt(0);
            multiline = true;
            maxLength = 0x7fffffff;
            IMGUIContainer imguiContainer = null;
            imguiContainer = new IMGUIContainer(() =>
            {
                var imGuiStyle =
                    new GUIStyle(!multiline ? EditorStyles.textField : EditorStyles.textArea) { wordWrap = false };

                var newValue = value;

                // 優先順位の順に実行。combinableFunctionNamesListに優先順位順に機能の可能な組み合わせを記述している。
                if (multiline)
                {
                    _scroll = EditorGUILayout.BeginScrollView(_scroll);
                    // maxLengthには対応。
                    newValue = maxLength >= 0 ?
                        GUILayout.TextArea(value, maxLength, imGuiStyle) :
                        EditorGUILayout.TextArea(value, imGuiStyle);
                    _textEditor = (TextEditor) GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                    if (_textEditor != null)
                    {
                        if (_textEditor.cursorIndex != _lastCursorIndex)
                        {
                            var newSize = imguiContainer.localBound.size;
                            if (newSize != _textAreaSize)
                            {
                                _textAreaSize = newSize;
                            }
                            _lastCursorIndex = _textEditor.cursorIndex;
                            var newPos = new Vector2(Math.Max(0, _textEditor.graphicalCursorPos.x - (_textAreaSize.x - 32)), Math.Max(0, _textEditor.graphicalCursorPos.y - (_textAreaSize.y - 32)));
                            if (newPos != _scroll)
                            {
                                _scroll = newPos;
                            }
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }

                value = newValue;
            });
            Add(imguiContainer);
        }
        public ImTextField textField
        {
            get
            {
                return this;
            }
        }

    }

    public class ImTextFieldEnterFocusOut : ImTextField
    {

        public ImTextFieldEnterFocusOut()
            : base() {
            RemoveAt(0);
            IMGUIContainer imguiContainer = null;
            imguiContainer = new IMGUIContainer(() =>
            {
                var imGuiStyle =
                    new GUIStyle(!multiline ? EditorStyles.textField : EditorStyles.textArea) { wordWrap = true };

                var newValue = value;

                // 優先順位の順に実行。combinableFunctionNamesListに優先順位順に機能の可能な組み合わせを記述している。
                if (multiline)
                {
                    // maxLengthには対応。
                    newValue = maxLength >= 0 ?
                        GUILayout.TextArea(value, maxLength, imGuiStyle) :
                        EditorGUILayout.TextArea(value, imGuiStyle);
                }
                else if (maxLength >= 0)
                {
                    newValue = GUILayout.TextField(value, maxLength, imGuiStyle);
                }
                else if (isReadOnly)
                {
                    EditorGUILayout.SelectableLabel(value, imGuiStyle, GUILayout.Height(18));
                }
                else if (isDelayed)
                {
                    // labelには対応。
                    newValue = !string.IsNullOrEmpty(label) ?
                        EditorGUILayout.DelayedTextField(label, value, imGuiStyle) :
                        EditorGUILayout.DelayedTextField(value, imGuiStyle);
                }
                else
                {
                    // labelには対応。
                    newValue = !string.IsNullOrEmpty(label) ?
                        EditorGUILayout.TextField(label, value, imGuiStyle) :
                        EditorGUILayout.TextField(value, imGuiStyle);
                }

                value = newValue;

                if (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return)
                {
                    // FocusOutEventを発生させる。
                    Focusable focusable = this;
                    Focusable willGiveFocusTo = this;
                    FocusChangeDirection direction = FocusChangeDirection.unspecified;
                    FocusController focusController = focusable.focusController;

                    using FocusOutEvent e = FocusOutEvent.GetPooled(focusable, willGiveFocusTo, direction, focusController);
                    focusable.SendEvent(e);
                }
            });
            Add(imguiContainer);
        }

    }
}