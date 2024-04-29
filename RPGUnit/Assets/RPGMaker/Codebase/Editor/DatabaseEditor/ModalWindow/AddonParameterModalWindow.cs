using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.AddonUIUtil;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Runtime.Addon;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow
{
    public class AddonParameterModalWindow : AddonBaseModalWindow
    {
        private readonly Vector2Int         WINDOW_SIZE = new Vector2Int(860, 480);
        private readonly AddonDataModel     _addon      = AddonDataModel.Create();
        private          AddonInfo          _addonInfo;
        private          AddonInfoContainer _addonInfos;
        private          AddonDataModel     _addonOrg;

        private          List<AddonDataModel>         _addons;
        private          bool                         _firstFocus     = true;
        private readonly List<FoldableExpandedParent> _itemStatusList = new List<FoldableExpandedParent>();
        private          bool                         _modified;
        private          bool                         _renew;
        private          VisualElement                bottomWindow;

        private VisualElement listWindow;

        protected override string ModalUxml =>
            "Assets/RPGMaker/Codebase/Editor/DatabaseEditor/ModalWindow/Uxml/addon_parameter_modalwindow.uxml";

        protected override string ModalUss => "";

        public void SetAddon(AddonDataModel addon) {
            if (addon == null)
            {
                _renew = true;
                _addonOrg = AddonDataModel.Create();
                _addonOrg.CopyTo(_addon);
                return;
            }

            _addonOrg = addon;
            addon.CopyTo(_addon);
        }

        private void OnDestroy() {
            AddonEditorWindowManager.instance.CloseDescendantWindows(this);
            AddonEditorWindowManager.instance.UnregisterWindow(this);
        }

        public override void ShowWindow(string modalTitle, CallBackWidow callBack) {
            var wnd = this; // GetWindow<AddonParameterModalWindow>();
            AddonEditorWindowManager.instance.RegisterParameterEditWindow(wnd);

            if (callBack != null) _callBackWindow = callBack;

            wnd.titleContent = new GUIContent(EditorLocalize.LocalizeText("WORD_2501"));
            wnd.Init();
            Vector2 size = WINDOW_SIZE;
            wnd.minSize = size;
            wnd.maxSize = size;
            wnd.maximized = false;
            wnd.Show();
        }

        public override void Init() {
            var addonManagementService = new AddonManagementService();
            _addons = addonManagementService.LoadAddons();
            _addonInfos = AddonManager.Instance.GetAddonInfos();
            _addonInfo = _addonInfos.GetAddonInfo(_addon.Name);

            var root = rootVisualElement;

            // 要素作成
            //----------------------------------------------------------------------
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModalUxml);
            VisualElement labelFromUxml = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUxml);
            root.Add(labelFromUxml);
            listWindow = labelFromUxml.Query<VisualElement>("system_window_rightwindow").AtIndex(0);
            bottomWindow = labelFromUxml.Query<VisualElement>("system_window_bottomwindow").AtIndex(0);
            var nameWindow = labelFromUxml.Query<VisualElement>("system_window_namewindow").AtIndex(0);
            var statusWindow = labelFromUxml.Query<VisualElement>("system_window_statuswindow").AtIndex(0);

            // Name
            var label = new Label(EditorLocalize.LocalizeText("WORD_2502"));
            label.style.height = 16;
            nameWindow.Add(label);

            label = new Label(_addon.Name);
            label.style.height = 16;
            label.style.flexGrow = 1;
            label.AddToClassList("four_borders");
            label.AddToClassList("margin4px");
            label.AddToClassList("text_ellipsis");
            nameWindow.Add(label);

            // Status
            label = new Label(EditorLocalize.LocalizeText("WORD_2503"));
            label.style.height = 16;
            statusWindow.Add(label);
            var nameList = new List<string>
                {EditorLocalize.LocalizeText("WORD_0052"), EditorLocalize.LocalizeText("WORD_0533")};
            var popupField = new PopupFieldBase<string>(nameList, _addon.Status ? 0 : 1);
            popupField.RegisterValueChangedCallback(evt =>
            {
                var popupField = evt.currentTarget as PopupFieldBase<string>;
                //Debug.Log($"popupField: {popupField.index}, {popupField.value}");
                _modified = true;
                _addon.Status = popupField.index == 0;
                AddonEditorWindowManager.instance.CloseDescendantWindows(this);
            });
            popupField.style.flexGrow = 1;
            statusWindow.Add(popupField);

            UpdateAuthorDescriptionUrlHelpParameters(labelFromUxml);

            // 確定、キャンセルボタン
            //----------------------------------------------------------------------
            var buttonOk = labelFromUxml.Query<Button>("Common_Button_Ok").AtIndex(0);
            var buttonCancel = labelFromUxml.Query<Button>("Common_Button_Cancel").AtIndex(0);
            buttonOk.style.alignContent = Align.FlexEnd;
            buttonOk.clicked += RegisterOkAction(() =>
            {
                if (_modified && _addon.Name.Length > 0)
                {
                    if (_renew)
                    {
                        _callBackWindow(_addon);
                    }
                    else
                    {
                        _addon.CopyTo(_addonOrg);
                        _callBackWindow(true);
                    }
                }

                Close();
            });

            buttonCancel.clicked += () => { Close(); };
        }

        private void UpdateAuthorDescriptionUrlHelpParameters(VisualElement labelFromUxml) {
            var authorWindow = labelFromUxml.Query<VisualElement>("system_window_authorwindow").AtIndex(0);
            var descriptionWindow = labelFromUxml.Query<VisualElement>("system_window_descriptionwindow").AtIndex(0);
            var urlWindow = labelFromUxml.Query<VisualElement>("system_window_urlwindow").AtIndex(0);
            var helpWindow = labelFromUxml.Query<VisualElement>("system_window_helpwindow").AtIndex(0);

            // Author
            authorWindow.Clear();
            var label = new Label(EditorLocalize.LocalizeText("WORD_2504"));
            label.style.height = 16;
            authorWindow.Add(label);
            var authorLabel = new Label(_addonInfo != null ? _addonInfo.author : "");
            authorLabel.style.flexGrow = 1;
            authorLabel.AddToClassList("four_borders");
            authorLabel.AddToClassList("margin4px");
            authorLabel.AddToClassList("text_ellipsis");
            authorWindow.Add(authorLabel);

            // Description
            descriptionWindow.Clear();
            var textElement = new TextElement();
            textElement.text = _addon.Description;
            textElement.style.flexGrow = 1;
            textElement.AddToClassList("text_ellipsis");
            descriptionWindow.Add(textElement);

            // URL
            urlWindow.Clear();
            var url = _addonInfo != null ? _addonInfo.url : "";
            if (url.Length > 0)
            {
                /*label = new Label(url);
                label.style.flexGrow = 1;
                label.AddToClassList("text_ellipsis");
                label.AddToClassList("text_underline");
                label.style.color = new StyleColor(new Color(0.25f, 0.25f, 1));
                label.RegisterCallback<MouseDownEvent>((e) =>
                {
                    Application.OpenURL(label.text);
                });
                urlWindow.Add(label);*/
                Button button = null;
                button = new Button(() => { Application.OpenURL(button.text); });
                button.text = url;
                button.style.flexGrow = 1;
                button.AddToClassList("text_underline");
                button.AddToClassList("text_ellipsis");
                button.style.color = new StyleColor(new Color(0, 0, 1));
                urlWindow.Add(button);
            }

            // Help
            helpWindow.Clear();
            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.unityTextAlign = TextAnchor.UpperLeft;
            var help = _addonInfo != null ? _addonInfo.help : "";
            var head = 0;
            var limit = 8000;
            while (head < help.Length)
            {
                var start = Math.Min(head + limit, help.Length);
                var index = help.IndexOf('\n', start);
                if (index >= 0)
                    index++;
                else
                    index = start;
                textElement = new TextElement();
                textElement.text = help.Substring(head, index - head);
                //textElement.style.position = 
                //textElement.style.flexGrow = 1;
                scrollView.Add(textElement);
                head = index;
            }

            helpWindow.Add(scrollView);

            // Parameters
            listWindow.Clear();
            var listView = CreateListView();
            listWindow.Add(listView);
            listView.ClearSelection();
            listView.AddToSelection(0);
        }


        private void UpdateLabel(Toggle toggle) {
            toggle.label = toggle.value ? "ON" : "OFF";
        }

        private int GetAncestorDepth(int index) {
            var depth = 0;
            var parent = _itemStatusList[index].parent;
            while (parent >= 0)
            {
                depth++;
                parent = _itemStatusList[parent].parent;
            }

            return depth;
        }

        private List<int> CreateItemSource() {
            var list = new List<int>();
            for (var i = 0; i < _itemStatusList.Count; i++)
            {
                var visible = true;
                var pair = _addonInfo.paramInfos[i];
                var parent = _itemStatusList[i].parent;
                while (parent >= 0)
                {
                    if (!_itemStatusList[parent].expanded)
                    {
                        visible = false;
                        break;
                    }

                    parent = _itemStatusList[parent].parent;
                }

                if (visible) list.Add(i);
            }

            return list;
        }

        // リストの要素作成
        private ListView CreateListView() {
            _itemStatusList.Clear();
            if (_addonInfo != null && _addonInfo.paramInfos.Count > 0)
            {
                var index = 0;
                foreach (var pi in _addonInfo.paramInfos)
                {
                    var foldable = false;
                    string parentName;
                    foreach (var pi2 in _addonInfo.paramInfos)
                    {
                        parentName = pi2.GetInfo("parent")?.value;
                        if (parentName == pi.name)
                        {
                            foldable = true;
                            break;
                        }
                    }

                    parentName = pi.GetInfo("parent")?.value;
                    var parent = -1;
                    if (parentName != null)
                    {
                        var i = 0;
                        foreach (var pi2 in _addonInfo.paramInfos)
                        {
                            if (pi2.name == parentName)
                            {
                                parent = i;
                                break;
                            }

                            i++;
                        }
                    }

                    var itemStatus = new FoldableExpandedParent(foldable, true, parent);
                    _itemStatusList.Add(itemStatus);
                    index++;
                }
            }

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

                    var paramKey = _addonInfo.paramInfos[index].name;
                    var paramInfo = _addonInfo.paramInfos[index];
                    var itemStatus = _itemStatusList[index];

                    var nameVe = new VisualElement();
                    nameVe.AddToClassList("list_view_item_name_area");
                    var nameWidth = 130; // nameVe.style.width.value.value;
                    var depth = GetAncestorDepth(index);
                    for (var j = 0; j < depth; j++)
                    {
                        var ve = new VisualElement();
                        ve.AddToClassList("list_view_item_space");
                        nameWidth -= 10; // ve.style.width.value.value;
                        nameVe.Add(ve);
                    }

                    // Foldout
                    VisualElement vi1 = null;
                    if (itemStatus.foldable)
                    {
                        var foldout = new Foldout();
                        foldout.value = itemStatus.expanded;
                        foldout.AddToClassList("list_view_item_foldout");
                        nameWidth -= 20; // foldout.style.width.value.value;
                        foldout.RegisterValueChangedCallback(e =>
                        {
                            var foldout = e.currentTarget as Foldout;
                            //Debug.Log($"foldout.value: {foldout.value}");

                            var index = (foldout.parent.parent as KeyVisualElement<int>).Key;
                            //Debug.Log(index);
                            var itemStatus = _itemStatusList[index];
                            itemStatus.expanded = foldout.value;
                            listView.itemsSource = CreateItemSource();
                            listView.Rebuild();
                        });
                        vi1 = foldout;
                    }
                    else
                    {
                        vi1 = new VisualElement();
                        vi1.AddToClassList("list_view_item_foldout");
                        nameWidth -= 20; // vi1.style.width.value.value;
                    }

                    nameVe.Add(vi1);

                    // Name
                    var nameInfo = paramInfo.GetInfo("text");
                    var nameLabel = new Label(nameInfo != null ? nameInfo.value : paramKey);
                    //nameLabel.AddToClassList("list_view_item_name_label");
                    nameLabel.style.width = nameWidth;
                    nameLabel.AddToClassList("text_ellipsis");
                    nameVe.Add(nameLabel);

                    // Value
                    var value = AddonManager.GetInitialValue(_addon.Parameters, paramInfo);
                    value = AddonUIUtil.GetEasyReadableText(_addonInfo, paramInfo, null, value);
                    var valueLabel = new Label(value);
                    valueLabel.AddToClassList("list_view_item_value_label");
                    valueLabel.AddToClassList("text_ellipsis");

                    visualElement.Add(nameVe);
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
            listView.reorderable = false;
            /*var list = new List<string>();
            foreach (var pair in _addonInfo.paramInfo)
            {
                var paramKey = pair.Key;
                list.Add(paramKey);
            }
            listView.itemsSource = list;*/
            listView.itemsSource = CreateItemSource();

            if (_addonInfo != null && _addonInfo.paramInfos.Count > 0) listView.selectedIndex = 0;

            listView.RegisterCallback<KeyDownEvent>(e =>
            {
                //Debug.Log($"pressed: '{e.character}'");
                switch (e.keyCode)
                {
                    case KeyCode.KeypadEnter:
                    case KeyCode.Return:
                        var index = (listView.itemsSource as List<int>)[listView.selectedIndex];
                        ShowAddonParameterEditWindow(_addonInfo, _addonInfo.paramInfos[index]);
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
                ShowAddonParameterEditWindow(_addonInfo, _addonInfo.paramInfos[index]);
            };
            if (_firstFocus)
            {
                _firstFocus = false;
                SetDelayedAction(() => { listView.Focus(); });
            }

            return listView;
        }

        private void ShowAddonParameterEditWindow(AddonInfo addonInfo, AddonParamInfo paramInfo) {
            ShowAddonParameterEditWindow(addonInfo, paramInfo, false, _addon.Parameters, obj =>
            {
                //Debug.Log($"AddonParameterEditModalWindow return: {obj}");
                var refresh = false;
                var value = obj as string;
                var listView = listWindow[0] as ListView;
                if (obj is bool && (bool) obj)
                {
                    _modified = true;
                    refresh = true;
                    AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                }

                if (refresh) listView.Rebuild();
            });
        }

        public static void ShowAddonParameterEditWindow(
            AddonInfo addonInfo,
            AddonParamInfo paramInfo,
            bool excludeArray,
            AddonParameterContainer parameters,
            CallBackWidow callback,
            bool modified = false
        ) {
            var info = paramInfo.GetInfo("type");
            var typeName = info != null ? info.value : "string";
            var arrayDimension = 0;
            string structName = null;
            var paramType = AddonManager.GetParamType(typeName, out arrayDimension, out structName);
            if (excludeArray) arrayDimension = 0;
            if (arrayDimension == 2)
            {
                var addonParameterEditArray2ModalWindow = new AddonParameterEditArray2ModalWindow();
                addonParameterEditArray2ModalWindow.SetInfo(addonInfo, paramInfo, parameters);
                addonParameterEditArray2ModalWindow.ShowWindow("", callback);
                return;
            }

            if (arrayDimension == 1)
            {
                var addonParameterEditArrayModalWindow = new AddonParameterEditArrayModalWindow();
                addonParameterEditArrayModalWindow.SetInfo(addonInfo, paramInfo, parameters);
                addonParameterEditArrayModalWindow.ShowWindow("", callback);
                return;
            }

            if (structName != null)
            {
                var addonParameterEditStructModalWindow = new AddonParameterEditStructModalWindow();
                addonParameterEditStructModalWindow.SetInfo(addonInfo, paramInfo, parameters);
                addonParameterEditStructModalWindow.ShowWindow("", callback);
                return;
            }

            var addonParameterEditModalWindow = new AddonParameterEditModalWindow();
            addonParameterEditModalWindow.SetInfo(addonInfo, paramInfo, parameters, modified);
            addonParameterEditModalWindow.ShowWindow("", callback);
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