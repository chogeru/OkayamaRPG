using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.AddonUIUtil;
using RPGMaker.Codebase.Runtime.Addon;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow
{
    public class AddonListModalWindow : AddonBaseModalWindow
    {
        private readonly Vector2Int WINDOW_SIZE = new Vector2Int(640, 480);

        private VisualElement listWindow;
        private VisualElement commentWindow;
        private VisualElement bottomWindow;
        private ListView      _listView;

        private AddonManagementService _addonManagementService;
        private List<AddonDataModel>   _addons;
        private AddonInfoContainer     _addonInfos;
        private bool                   _modified;
        private Label                  _commentLabel;
        private bool                   _firstFocus = true;

        protected override string ModalUxml =>
            "Assets/RPGMaker/Codebase/Editor/DatabaseEditor/ModalWindow/Uxml/addon_list_modalwindow.uxml";

        protected override string ModalUss => "";

        private void OnDestroy() {
            AddonEditorWindowManager.instance.CloseDescendantWindows(this);
            AddonEditorWindowManager.instance.UnregisterWindow(this);
        }

        public override void ShowWindow(string modalTitle, CallBackWidow callBack) {
            var wnd = this; //GetWindow<AddonListModalWindow>();
            AddonEditorWindowManager.instance.RegisterParameterEditWindow(wnd);

            if (callBack != null) _callBackWindow = callBack;

            wnd.titleContent = new GUIContent(EditorLocalize.LocalizeText("WORD_2500"));
            wnd.Init();
            Vector2 size = WINDOW_SIZE;
            wnd.minSize = size;
            wnd.maxSize = size;
            wnd.maximized = false;
            wnd.Show();
        }

        public override void Init() {
            var root = rootVisualElement;

            // Add-on設定を取得
            _addonManagementService = new AddonManagementService();
            _addons = _addonManagementService.LoadAddons();
            _addonInfos = AddonManager.Instance.GetAddonInfos();

            // 要素作成
            //----------------------------------------------------------------------
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModalUxml);
            VisualElement labelFromUxml = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUxml);
            root.Add(labelFromUxml);
            listWindow = labelFromUxml.Query<VisualElement>("system_window_listwindow").AtIndex(0);
            commentWindow = labelFromUxml.Query<VisualElement>("system_window_commentwindow").AtIndex(0);
            bottomWindow = labelFromUxml.Query<VisualElement>("system_window_bottomwindow").AtIndex(0);
            //descriptionWindow.style.width = 1000;
            //descriptionWindow.style.height = 400;

            // Add-onのリスト
            _listView = CreateListView();
            listWindow.Add(_listView);

            // コメント
            _commentLabel = new Label();
            _commentLabel.style.flexGrow = 1;
            _commentLabel.AddToClassList("text_ellipsis");
            commentWindow.Add(_commentLabel);
            if (_addons.Count > 0) UpdateAddonComment(0);
            if (_listView.itemsSource.Count > 0)
            {
                _listView.ClearSelection();
                _listView.AddToSelection(0);
            }

            // 確定、キャンセルボタン
            //----------------------------------------------------------------------
            var buttonOk = labelFromUxml.Query<Button>("Common_Button_Ok").AtIndex(0);
            var buttonCancel = labelFromUxml.Query<Button>("Common_Button_Cancel").AtIndex(0);
            buttonOk.style.alignContent = Align.FlexEnd;
            buttonOk.clicked += RegisterOkAction(() =>
            {
                var newList = new List<AddonDataModel>();
                var itemsSource = _listView.itemsSource as List<int>;
                for (var i = 0; i < _addons.Count; i++)
                {
                    if (itemsSource[i] != i) _modified = true;
                    newList.Add(_addons[itemsSource[i]]);
                }

                _addonManagementService.SaveAddons(newList);

                _callBackWindow(null);

                if (_modified) EditorUtility.RequestScriptReload();
                Close();
            });

            buttonCancel.clicked += () => { Close(); };
        }

        private void UpdateLabel(Toggle toggle) {
            toggle.label = toggle.value ? "ON" : "OFF";
        }

        // リストの要素作成
        private ListView CreateListView() {
            StripedListView<int> listView = null;
            Action<VisualElement, int> bindItem = (e, i) =>
            {
                e.Clear();
                {
                    var index = (listView.itemsSource as List<int>)[i];
                    VisualElement visualElement = new IndexVisualElement(index);
                    visualElement.style.flexDirection = FlexDirection.Row;

                    listView.SetVisualElementStriped(visualElement, index);
                    if (index == listView.itemsSource.Count - 1) listView.AddVisualElementStriped(e);

                    if (index >= 0)
                    {
                        var addon = _addons[index];
                        // ON/OFF
                        var toggle = new Toggle();
                        toggle.AddToClassList("list_view_item_toggle");
                        toggle.value = addon.Status;
                        UpdateLabel(toggle);
                        toggle.RegisterValueChangedCallback(e =>
                        {
                            var toggle = e.currentTarget as Toggle;
                            UpdateLabel(toggle);
                            var index = (toggle.parent as IndexVisualElement).Index;
                            //Debug.Log(index);
                            _addons[index].Status = toggle.value;
                            _modified = true;
                            AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                        });
                        listView.RegisterToggle(index, toggle);

                        // Name
                        var nameLabel = new Label(addon.Name);
                        nameLabel.AddToClassList("text_ellipsis");
                        nameLabel.AddToClassList("list_view_item_name_label");

                        // Description
                        var descriptionLabel = new Label(addon.Description);
                        descriptionLabel.AddToClassList("text_ellipsis");
                        descriptionLabel.AddToClassList("list_view_item_description_label");

                        visualElement.Add(toggle);
                        visualElement.Add(nameLabel);
                        visualElement.Add(descriptionLabel);
                    }

                    e.Add(visualElement);
                }
            };

            Func<VisualElement> makeItem = () => new Label();
            //var listView = new ListView(new String[_addons.Count], 16, makeItem, bindItem);
            listView = new StripedListView<int>(new string[_addons.Count], 16, makeItem, bindItem);
            listView.AddToClassList("list_view");
            listView.SolidQuad();
            listView.name = "list";
            listView.selectionType = SelectionType.Multiple;
            listView.reorderable = true;
            var list = new List<int>();
            for (var i = 0; i < _addons.Count; i++) list.Add(i);
            //list.Add(-1);   //空行。
            listView.itemsSource = list;

            //listView.selectedIndex = 0;

            listView.RegisterCallback<KeyDownEvent>(e =>
            {
                //Debug.Log($"pressed: '{e.character}'");
                var addonIndex = -1;
                switch (e.keyCode)
                {
                    case KeyCode.Space:
                        var first = true;
                        var newState = false;
                        foreach (var index in listView.selectedIndices)
                            //foreach (var o in listView.selectedItems)
                        {
                            //var lvItem = (listView as VisualElement)[index];
                            //var lvItemFirst = lvItem[0];
                            //var toggle = lvItemFirst as Toggle;
                            addonIndex = (listView.itemsSource as List<int>)[index];
                            if (addonIndex >= 0)
                            {
                                var toggle = listView.getToggle(addonIndex);
                                //var toggle = (o as IndexVisualElement)[0] as Toggle;
                                if (first)
                                {
                                    first = false;
                                    newState = !toggle.value;
                                }

                                toggle.value = newState;
                                UpdateLabel(toggle);
                            }
                        }

                        _modified = true;
                        AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                        break;

                    case KeyCode.KeypadEnter:
                    case KeyCode.Return:
                        addonIndex = (listView.itemsSource as List<int>)[listView.selectedIndex];
                        ShowAddonParameterWindow(addonIndex >= 0 ? _addons[addonIndex] : null);
                        break;
                }
            });
            listView.onItemsChosen += objects =>
            {
                //Debug.Log($"objects: {objects.ToString()}");
                var list = objects.ToList(); // as List<int>;
                if (list.Count == 0) return;
                //Debug.Log($"list: {list[0]}");
                var addonIndex = int.Parse(list[0].ToString());
                ShowAddonParameterWindow(addonIndex >= 0 ? _addons[addonIndex] : null);
            };
            listView.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (evt.button != (int) MouseButton.RightMouse)
                {
                    evt.StopPropagation();
                    return;
                }

                //存在する行ならフォーカスする。
                var index = (int) (evt.localMousePosition.y / listView.fixedItemHeight);
                /*if (index >= listView.itemsSource.Count)
                {
                    index = listView.itemsSource.Count - 1;
                }
                if (index >= 0 && index >= listView.itemsSource.Count)
                {
                    
                }*/
                var addonIndex = index < 0 || index >= listView.itemsSource.Count
                    ? -1
                    : (listView.itemsSource as List<int>)[listView.selectedIndex];
                var menu = new GenericMenu();
                if (addonIndex >= 0)
                    menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1459")), false, () =>
                    {
                        //Edit
                        ShowAddonParameterWindow(addonIndex >= 0 ? _addons[addonIndex] : null);
                    });
                else
                    menu.AddDisabledItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1459")));
                /*if (listView.selectedIndex != listView.itemsSource.Count - 1)
                {
                    menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1521")), false, () =>
                    {
                        //Delete
                        _addons.RemoveAt(addonIndex);
                        var list = listView.itemsSource as List<int>;
                        list.RemoveAt(listView.selectedIndex);
                        listView.itemsSource = list;
                        listView.Refresh();
                    });
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1521")));
                }*/
                menu.ShowAsContext();
            });
            listView.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button != (int) MouseButton.RightMouse)
                {
                    evt.StopPropagation();
                    return;
                }

                //存在する行かどうかに合わせて表示を変える。
                var index = (int) (evt.localMousePosition.y / listView.fixedItemHeight);
                if (index >= listView.itemsSource.Count) index = listView.itemsSource.Count - 1;
                if (index >= 0) listView.selectedIndex = index;
            });

            listView.onSelectionChange += objects =>
            {
                var list = objects.ToList(); // as List<int>;
                var addonIndex = list.Count == 0 ? -1 : int.Parse(list[0].ToString());
                UpdateAddonComment(addonIndex);
            };

            BaseClickHandler.ClickEvent(listView, evt =>
            {
                if (evt == (int) MouseButton.RightMouse)
                {
                    // 右クリック
                }
            });

            if (_firstFocus)
            {
                _firstFocus = false;
                SetDelayedAction(() => { listView.Focus(); });
            }

            return listView;
        }

        private int GetIndexOnListView(int addonIndex) {
            var list = _listView.itemsSource as List<int>;
            return list.FindIndex(x => x == addonIndex);
        }

        private void UpdateAddonComment(int addonIndex) {
            if (addonIndex < 0)
            {
                _commentLabel.text = "";
                return;
            }

            var addon = _addons[addonIndex];
            var addonInfo = _addonInfos.GetAddonInfo(addon.Name);
            foreach (var name in addonInfo.base_)
                if (_addonInfos.GetAddonInfo(name) == null)
                {
                    _commentLabel.text = string.Format(EditorLocalize.LocalizeText("WORD_2518"), addon.Name, name);
                    return;
                }

            var listIndex = GetIndexOnListView(addonIndex);
            foreach (var name in addonInfo.orderAfter)
            {
                if (_addonInfos.GetAddonInfo(name) == null) continue;
                var index = GetIndexOnListView(_addons.FindIndex(x => x.Name == name));
                if (index >= listIndex)
                {
                    _commentLabel.text = string.Format(EditorLocalize.LocalizeText("WORD_2519"), addon.Name, name);
                    return;
                }
            }

            foreach (var name in addonInfo.orderBefore)
            {
                if (_addonInfos.GetAddonInfo(name) == null) continue;
                var index = GetIndexOnListView(_addons.FindIndex(x => x.Name == name));
                if (index <= listIndex)
                {
                    _commentLabel.text = string.Format(EditorLocalize.LocalizeText("WORD_2520"), addon.Name, name);
                    return;
                }
            }

            _commentLabel.text = "";
        }


        private void ShowAddonParameterWindow(AddonDataModel addonDataModel) {
            var addonParameterModalWindow = new AddonParameterModalWindow();
            addonParameterModalWindow.SetAddon(addonDataModel);
            addonParameterModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Add-on Parameter"), obj =>
            {
                //Debug.Log($"AddonParameterModalWindow return: {obj}");
                var refresh = false;
                var addon = obj as AddonDataModel;
                var listView = listWindow[0] as ListView;
                if (addon != null)
                {
                    _addons.Add(addon);
                    var list = listView.itemsSource as List<int>;
                    list.Insert(list.Count - 1, list.Count - 1);
                    listView.itemsSource = list;
                    //(addon as AddonDataModel).CopyTo(addonDataModel);
                    refresh = true;
                    _modified = true;
                    AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                }
                else if (obj is bool && (bool) obj)
                {
                    refresh = true;
                    _modified = true;
                    AddonEditorWindowManager.instance.CloseDescendantWindows(this);
                }

                if (refresh) listView.Rebuild();
            });
        }

#if false
        [MenuItem("Tools/AddonList")]
        private static void AddonList() {
            var addonListModalWindow = new AddonListModalWindow();
            addonListModalWindow.ShowWindow("", data =>
            {
            });
        }

        [MenuItem("Tools/AddonManager.Refresh")]
        private static void Refresh() {
            AddonManager.Instance.Refresh();
        }
#endif
    }
}