using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Animation;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement.Repository;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.AddonUIUtil;
using RPGMaker.Codebase.Editor.Hierarchy.Region.AssetManage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow
{
#if UNITY_EDITOR
    public class DlcImporterWindow : AssetPostprocessor
    {
#region 定数
        
        static string _importedFlagFilename = "imported.txt";
        static string _importedInfoFilename = "import-info.txt";
        static string _importConfirmTitle = "WORD_2902";
        static string _importConfirmMessage = "WORD_2903";
        static string _importFailedMessage = "WORD_2904";
        static string _dlcOverwriteOrSkipTitle = "WORD_2905";
        static string _thereAreSameNameAssetsText = "WORD_2906";
        static string _overwriteAssetText = "WORD_2907";
        static string _skipOverwritingAssetText = "WORD_2908";
        static string _selectOnEachAssetText = "WORD_2909";
        static string _assetFileCollisionTitle = "WORD_2910";
        static string _assetFileCollisionMessage = "WORD_2911";
        static string _dlcAssetFileText = "WORD_2912";
        static string _currentAssetFileText = "WORD_2913";
        static string _skipFilesWithSameTimestampSizeText = "WORD_2914";
        static string _dlcImportingText = "WORD_2915";
        static string _finishedImportingText = "WORD_2916";
        static string _tileGroupSameNameAssetText = "WORD_2917";
        static string _yesText = "WORD_3058";
        static string _noText = "WORD_3059";
        static string _cancelText = "WORD_1530";
        static string _okText = "WORD_2900";
        static string _executeText = "WORD_2901";
#endregion

#region パス
        static string _dlcBasePath = "Assets/RPGMaker/DLC/";
        static string _storagePath = "Assets/RPGMaker/Storage/";
        const string kCategoryWalkChara = "Images/Characters";
        const string kCategoryCharaObject = "Images/Objects";
        const string kCategorySvActor = "Images/SV_Actors";
        const string kCategorySvWeapon = "Images/System/Weapon";
        const string kCategoryMapTileImages = "Map/TileImages";
        const string kCategoryMapBackground = "Map/BackgroundImages";
        const string kCategoryParallax = "Images/Parallaxes";
        const string kCategoryEffekseer = "Animation/Effekseer";
        const string kCategoryPrefab = "Animation/Prefab";
#endregion

#region 内部クラス
        class FirstExecution : ScriptableSingleton<FirstExecution> {
            bool _first = true;
            public bool IsFirst() {
                if (!_first) return false;
                _first = false;
                return true;
            }
        }
        class FilenameChoicePair {
            public string filename;
            public EnumChoice choice;
            public FilenameChoicePair(string filename, EnumChoice choice) {
                this.filename = filename;
                this.choice = choice;
            }
        }
        enum EnumChoice {
            Cancel = -1,
            Overwrite,
            Skip,
            Import,
            Select,
            NotChosenYet,
            Rename,
        }
        enum ImportStatus {
            NotFinished,
            Completed,
            Cancelled,
            CallbackFailed,
            Started,
        }
        public class EditorReadyWindow : EditorWindow
        {
            static Action _callback;

            public static void SetCallback(Action callback) {
                _callback = callback;
            }
            private void OnEnable() {
                var last = _callback;
                _callback = null;
                last?.Invoke();
            }
        }

        public class MakeChoiceWindow : AddonBaseModalWindow
        {
            readonly Vector2Int WINDOW_SIZE = new Vector2Int(447, 276);
            bool _responded = false;

            protected override string ModalUxml
            {
                get
                {
                    return "Assets/RPGMaker/Codebase/Editor/DatabaseEditor/ModalWindow/Uxml/dlcimport_make_choice_window.uxml";
                }
            }

            protected override string ModalUss
            {
                get { return ""; }
            }


            public override void ShowWindow(string modalTitle, CallBackWidow callBack) {
                var wnd = GetWindow<MakeChoiceWindow>();

                if (callBack != null)
                {
                    _callBackWindow = callBack;
                }

                wnd.titleContent = new GUIContent(EditorLocalize.LocalizeText(_dlcOverwriteOrSkipTitle));
                Vector2 size = WINDOW_SIZE;
                wnd.minSize = size;
                //wnd.maxSize = size;
                wnd.maximized = false;
                wnd.Show();

            }

            private void CreateGUI() {
                this.Init();
            }

            private void OnDestroy() {
                if (!_responded)
                {
                    _callBackWindow(-1);
                }
            }

            KeyValuePair<string, string> GetPathAndFilename(string path) {
                var index = path.LastIndexOf('/');
                if (index < 0)
                {
                    return new KeyValuePair<string, string>(string.Empty, path);
                }
                return new KeyValuePair<string, string>(path.Substring(0, index + 1), path.Substring(index + 1));
            }

            public override void Init() {
                VisualElement root = rootVisualElement;

                // 要素作成
                //----------------------------------------------------------------------
                var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModalUxml);
                VisualElement labelFromUxml = visualTree.CloneTree();
                root.Add(labelFromUxml);

                labelFromUxml.style.flexGrow = 1;

                var list_window = labelFromUxml.Query<VisualElement>($"system_window_list_window").AtIndex(0);
                var label = new Label($"{_dlcName}\n{string.Format(EditorLocalize.LocalizeText(_thereAreSameNameAssetsText), _collisionFileCount)}");
                list_window.Add(label);

                var choices = new List<string>();
                choices.Add(EditorLocalize.LocalizeText(_overwriteAssetText));
                choices.Add(EditorLocalize.LocalizeText(_skipOverwritingAssetText));
                choices.Add(EditorLocalize.LocalizeText(_selectOnEachAssetText));

                var radioButtonGroup = new RadioButtonGroup("", choices);
                radioButtonGroup.value = 0;
                list_window.Add(radioButtonGroup);

                // 削除、キャンセルボタン
                //----------------------------------------------------------------------
                var buttonExecute = labelFromUxml.Query<Button>("Execute").AtIndex(0);
                var buttonCancel = labelFromUxml.Query<Button>("Common_Button_Cancel").AtIndex(0);
                buttonExecute.text = EditorLocalize.LocalizeText(_executeText);
                buttonExecute.style.alignContent = Align.FlexEnd;
                buttonExecute.clicked += RegisterOkAction(() =>
                {
                    _responded = true;
                    _callBackWindow(radioButtonGroup.value == 0 ? EnumChoice.Overwrite : radioButtonGroup.value == 1 ? EnumChoice.Skip : EnumChoice.Select);
                    Close();
                });

                buttonCancel.clicked += () =>
                {
                    _responded = true;
                    _callBackWindow(-1);
                    Close();
                
                };
            }

        }

        public class SelectOnEachWindow : AddonBaseModalWindow
        {
            readonly Vector2Int WINDOW_SIZE = new Vector2Int(800, 270);
            List<UnityEngine.Object> _usedObjectList = new List<UnityEngine.Object>();
            Texture2D _iconSetTex2D;
            class AssetFileSelection {
                public string category;
                public string assetName;
                public string filename;
                public bool[] checked_ = new bool[]{false, false};
                public FilenameChoicePair filenameChoicePair;
                public AssetFileSelection(string category, string assetName, string filename, FilenameChoicePair filenameChoicePair) {
                    this.category = category;
                    this.assetName = assetName;
                    this.filename = filename;
                    this.filenameChoicePair = filenameChoicePair;
                }
            }
            List<AssetFileSelection> _itemList;
            ListView _listView;
            bool _responded = false;

            protected override string ModalUxml
            {
                get
                {
                    return "Assets/RPGMaker/Codebase/Editor/DatabaseEditor/ModalWindow/Uxml/dlcimport_select_on_each_window.uxml";
                }
            }

            protected override string ModalUss
            {
                get { return ""; }
            }


            public override void ShowWindow(string modalTitle, CallBackWidow callBack) {
                var wnd = GetWindow<SelectOnEachWindow>();

                if (callBack != null)
                {
                    _callBackWindow = callBack;
                }

                wnd.titleContent = new GUIContent(string.Format(EditorLocalize.LocalizeText(_assetFileCollisionTitle), _collisionFileCount));
                Vector2 size = WINDOW_SIZE;
                wnd.minSize = size;
                //wnd.maxSize = size;
                wnd.maximized = false;
                wnd.Show();

            }

            private void CreateGUI() {
                this.Init();
            }

            private void OnDestroy() {
                //foreach (var obj in _usedObjectList)
                //{
                    //DestroyImmediate(obj);
                //}
                _usedObjectList.Clear();
                //DestroyImmediate(_iconSetTex2D);
                _iconSetTex2D = null;
                if (!_responded)
                {
                    _callBackWindow(-1);
                }
            }

            KeyValuePair<string, string> GetPathAndFilename(string path) {
                var index = path.LastIndexOf('/');
                if (index < 0)
                {
                    return new KeyValuePair<string, string>(string.Empty, path);
                }
                return new KeyValuePair<string, string>(path.Substring(0, index + 1), path.Substring(index + 1));
            }


            public override void Init() {
                _skipSameAssets = false;

                VisualElement root = rootVisualElement;

                // 要素作成
                //----------------------------------------------------------------------
                var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModalUxml);
                VisualElement labelFromUxml = visualTree.CloneTree();
                root.Add(labelFromUxml);

                labelFromUxml.style.flexGrow = 1;

                _iconSetTex2D = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/RPGMaker/Storage/Images/System/IconSet.png");
                _itemList = new List<AssetFileSelection>();
                foreach (var item in _dlcInfoDic)
                {
                    foreach (var item2 in item.Value)
                    {
                        foreach (var item3 in item2.Value)
                        {
                            if (item3.choice == EnumChoice.Select)
                            {
                                _itemList.Add(new AssetFileSelection(item.Key.Split(',')[0], item2.Key, GetSubInfoCutString(item3.filename), item3));
                            }
                        }
                    }
                }

                var list_window = labelFromUxml.Query<VisualElement>($"system_window_list_window").AtIndex(0);
                var label = new Label($"{_dlcName}\n{EditorLocalize.LocalizeText(_assetFileCollisionMessage)}");
                list_window.Add(label);

                var buttonExecute = labelFromUxml.Query<Button>("Execute").AtIndex(0);

                var ve = new VisualElement();
                ve.style.flexDirection = FlexDirection.Row;
                ve.style.flexGrow = 1;
                var dlcAssetFileToggle = new Toggle(EditorLocalize.LocalizeText(_dlcAssetFileText));
                dlcAssetFileToggle.AddToClassList("list_view_item_toggle");
                dlcAssetFileToggle.RegisterValueChangedCallback(e =>
                {
                    var toggle = e.currentTarget as Toggle;
                    int index = 0;
                    foreach (var item in _itemList)
                    {
                        SetItemSelect(index, 0, toggle.value);
                        index++;
                    }
                    buttonExecute.SetEnabled(IsAllItemsSelected());
                    _listView.Rebuild();
                });
                dlcAssetFileToggle.AddToClassList("height_toggle");
                dlcAssetFileToggle.style.width = 400;
                ve.Add(dlcAssetFileToggle);
                var currentAssetFileToggle = new Toggle(EditorLocalize.LocalizeText(_currentAssetFileText));
                currentAssetFileToggle.AddToClassList("list_view_item_toggle");
                currentAssetFileToggle.RegisterValueChangedCallback(e =>
                {
                    var toggle = e.currentTarget as Toggle;
                    int index = 0;
                    foreach (var item in _itemList)
                    {
                        SetItemSelect(index, 1, toggle.value);
                        index++;
                    }
                    buttonExecute.SetEnabled(IsAllItemsSelected());
                    _listView.Rebuild();
                });
                currentAssetFileToggle.AddToClassList("height_toggle");
                currentAssetFileToggle.style.width = 400;
                ve.Add(currentAssetFileToggle);
                ve.style.minHeight = 24;
                list_window.Add(ve);

                _listView = CreateListView(_itemList, buttonExecute);
                list_window.Add(_listView);

                int sameCount = 0;
                foreach (var itemInfo in _itemList)
                {
                    if (IsSameTimestampSize(itemInfo))
                    {
                        sameCount++;
                    }
                }
                var skipFilesToggle = new Toggle(string.Format(EditorLocalize.LocalizeText(_skipFilesWithSameTimestampSizeText), sameCount));
                skipFilesToggle.AddToClassList("list_view_item_toggle");
                skipFilesToggle.RegisterValueChangedCallback(e =>
                {
                    var toggle = e.currentTarget as Toggle;
                    _skipSameAssets = toggle.value;
                    buttonExecute.SetEnabled(IsAllItemsSelected());
                    UpdateListViewItemsSource(_listView);
                    _listView.Rebuild();
                });
                skipFilesToggle.style.minHeight = 24;
                list_window.Add(skipFilesToggle);

                // 削除、キャンセルボタン
                var buttonCancel = labelFromUxml.Query<Button>("Common_Button_Cancel").AtIndex(0);
                //----------------------------------------------------------------------
                buttonExecute.text = EditorLocalize.LocalizeText(_executeText);
                buttonExecute.style.alignContent = Align.FlexEnd;
                buttonExecute.clicked += RegisterOkAction(() =>
                {
                    foreach (var item in _dlcInfoDic)
                    {
                        foreach (var item2 in item.Value)
                        {
                            foreach (var item3 in item2.Value)
                            {
                                if (item3.choice == EnumChoice.Select)
                                {
                                    var itemInfo = _itemList.FirstOrDefault(x => x.filenameChoicePair == item3);
                                    var newChoice = EnumChoice.NotChosenYet;
                                    if (_skipSameAssets && IsSameTimestampSize(itemInfo))
                                    {
                                        newChoice = EnumChoice.Skip;
                                    }
                                    else if (itemInfo.checked_[0] && itemInfo.checked_[1])
                                    {
                                        newChoice = EnumChoice.Rename;
                                    }
                                    else if (itemInfo.checked_[0])
                                    {
                                        newChoice = EnumChoice.Overwrite;
                                    }
                                    else if (itemInfo.checked_[1])
                                    {
                                        newChoice = EnumChoice.Skip;
                                    }
                                    if (newChoice != EnumChoice.NotChosenYet)
                                    {
                                        item3.choice = newChoice;
                                    }
                                }
                            }
                        }
                    }
                    _responded = true;
                    _callBackWindow(0);
                    Close();
                });
                buttonExecute.SetEnabled(IsAllItemsSelected());

                buttonCancel.clicked += () =>
                {
                    _responded = true;
                    _callBackWindow(-1);
                    Close();

                };
            }

            void SetItemSelect(int index, int subIndex, bool value) {
                _itemList[index].checked_[subIndex] = value;
            }

            bool IsSameTimestampSize(AssetFileSelection itemInfo) {
                var fileInfo1 = new FileInfo(GetAssetFilename(itemInfo, 0));
                var fileInfo2 = new FileInfo(GetAssetFilename(itemInfo, 1));
                if (fileInfo1.LastWriteTime == fileInfo2.LastWriteTime && fileInfo1.Length == fileInfo2.Length)
                {
                    return true;
                }
                return false;
            }

            void UpdateListViewItemsSource(ListView listView) {
                var list = new List<int>();
                for (int i = 0; i < _itemList.Count; i++)
                {
                    var itemInfo = _itemList[i];
                    if (_skipSameAssets && IsSameTimestampSize(itemInfo))
                    {
                        continue;
                    }
                    list.Add(i);
                }
                listView.itemsSource = list;
            }

            bool IsAllItemsSelected() {
                foreach (var itemInfo in _itemList)
                {
                    if (_skipSameAssets && IsSameTimestampSize(itemInfo)) continue;
                    if (!itemInfo.checked_[0] && !itemInfo.checked_[1]) return false;
                }
                return true;
            }

            VisualElement GetAssetInfoVe(int index, int subIndex, VisualElement buttonExecute) {
                var itemInfo = _itemList[index];
                var filename = GetAssetFilename(itemInfo, subIndex);
                var fileInfo = new FileInfo(filename);
                var ve = new VisualElement();
                ve.style.width = 400;
                ve.style.flexDirection = FlexDirection.Row;
                var toggle = new Toggle();
                toggle.AddToClassList("list_view_item_toggle");
                toggle.userData = new KeyValuePair<int, int>(index, subIndex);
                toggle.value = _itemList[index].checked_[subIndex];
                toggle.RegisterValueChangedCallback(e =>
                {
                    var toggle = e.currentTarget as Toggle;
                    var userData = (KeyValuePair<int, int>) toggle.userData;
                    var index = userData.Key;
                    var subIndex = userData.Value;
                    SetItemSelect(index, subIndex, toggle.value);
                    buttonExecute.SetEnabled(IsAllItemsSelected());
                    //_listView.Rebuild();
                });
                if (filename.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                {
                    var image = new Image();
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(filename);
                    _usedObjectList.Add(tex);
                    //image.style.backgroundImage = tex;
                    image.image = tex;
                    image.style.width = 180;
                    toggle.Add(image);
                }
                else if (filename.EndsWith("wav", StringComparison.OrdinalIgnoreCase) || itemInfo.filename.EndsWith("ogg", StringComparison.OrdinalIgnoreCase))
                {
                    var image = new Image();
                    //image.style.backgroundImage = tex;
                    image.image = _iconSetTex2D;
                    image.sourceRect = new Rect(0, 66 * 5, 66, 66);
                    toggle.Add(image);
                }
                else
                {
                    var image = new Image();
                    //image.style.backgroundImage = tex;
                    image.image = _iconSetTex2D;
                    image.sourceRect = new Rect(66 * 5, 66 * 10, 66, 66);
                    toggle.Add(image);
                }
                ve.Add(toggle);

                var vertVe = new VisualElement();
                vertVe.style.width = 180;
                var timestampLabel = new Label(fileInfo.LastWriteTime.ToString("yyyy'/'MM'/'dd' 'HH':'mm"));
                vertVe.Add(timestampLabel);
                var sizeLabel = new Label(string.Format("{0:#,0}", fileInfo.Length));
                vertVe.Add(sizeLabel);
                ve.Add(vertVe);
                return ve;
            }

            string GetAssetFilename(AssetFileSelection itemInfo, int subIndex) {
                string filename;
                if (subIndex == 0)
                {
                    filename = $"{_currentDlcPath}{itemInfo.category}/{itemInfo.filename}";
                }
                else
                {
                    filename = $"{_storagePath}{_categoryStoragePathDic[itemInfo.category]}/{GetRemovedPathFilename(itemInfo.filename)}";
                }
                return filename;
            }

            private ListView CreateListView(List<AssetFileSelection> itemList, VisualElement buttonExecute) {
                StripedListView<int> listView = null;
                Action<VisualElement, int> bindItem = (e, i) =>
                {
                    e.Clear();
                    {
                        var index = (listView.itemsSource as List<int>)[i];
                        VisualElement visualElement = new IndexVisualElement(index);
                        //visualElement.style.flexDirection = FlexDirection.Row;

                        listView.SetVisualElementStriped(visualElement, index);
                        if (index == listView.itemsSource.Count - 1) listView.AddVisualElementStriped(e);

                        if (index >= 0)
                        {
                            var itemInfo = itemList[index];

                            // Name
                            var nameLabel = new Label(GetSubInfoCutString(itemInfo.filename));
                            nameLabel.AddToClassList("text_ellipsis");
                            nameLabel.AddToClassList("list_view_item_name_label");
                            visualElement.Add(nameLabel);

                            var ve = new VisualElement();
                            ve.style.flexDirection = FlexDirection.Row;
                            for (int j = 0; j < 2; j++)
                            {
                                var subVe = GetAssetInfoVe(index, j, buttonExecute);
                                ve.Add(subVe);
                            }
                            visualElement.Add(ve);
                        }

                        e.Add(visualElement);
                    }
                };

                Func<VisualElement> makeItem = () => new Label();
                listView = new StripedListView<int>(new string[itemList.Count], 16 * 3, makeItem, bindItem);
                listView.AddToClassList("list_view");
                listView.SolidQuad();
                listView.name = "list";
                listView.selectionType = SelectionType.None;
                listView.reorderable = false;
                listView.style.width = 800;
                UpdateListViewItemsSource(listView);

                return listView;
            }

        }

        public class ResultWindow : AddonBaseModalWindow
        {
            readonly Vector2Int WINDOW_SIZE = new Vector2Int(447, 276);
            bool _responded = false;

            protected override string ModalUxml
            {
                get
                {
                    return "Assets/RPGMaker/Codebase/Editor/DatabaseEditor/ModalWindow/Uxml/dlcimport_result_window.uxml";
                }
            }

            protected override string ModalUss
            {
                get { return ""; }
            }


            public override void ShowWindow(string modalTitle, CallBackWidow callBack) {
                var wnd = GetWindow<ResultWindow>();

                if (callBack != null)
                {
                    _callBackWindow = callBack;
                }

                wnd.titleContent = new GUIContent(EditorLocalize.LocalizeText(_importConfirmTitle));
                Vector2 size = WINDOW_SIZE;
                wnd.minSize = size;
                //wnd.maxSize = size;
                wnd.maximized = false;
                wnd.Show();

            }

            private void CreateGUI() {
                if (_resultList == null)
                {
                    Close();
                    return;
                }
                this.Init();
            }

            private void OnDestroy() {
                if (!_responded)
                {
                    _callBackWindow(-1);
                }
            }

            public override void Init() {
                VisualElement root = rootVisualElement;

                // 要素作成
                //----------------------------------------------------------------------
                var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModalUxml);
                VisualElement labelFromUxml = visualTree.CloneTree();
                root.Add(labelFromUxml);

                labelFromUxml.style.flexGrow = 1;

                var list_window = labelFromUxml.Query<VisualElement>($"system_window_list_window").AtIndex(0);
                var label = new Label(EditorLocalize.LocalizeText(_finishedImportingText));
                list_window.Add(label);
                var listView = CreateListView();
                listView.RegisterCallback<KeyDownEvent>((e) =>
                {
                    //Debug.Log($"pressed: '{e.character}'");
                    switch (e.keyCode)
                    {
                        case KeyCode.C:
                            if ((e.modifiers & EventModifiers.Control) == EventModifiers.Control)
                            {
                                var list = new List<string>();
                                foreach (var i in listView.selectedIndices)
                                {
                                    list.Add(_resultList[i]);
                                }
                                GUIUtility.systemCopyBuffer = String.Join('\n', list);
                            }
                            break;
                    }
                });
                list_window.Add(listView);

                // 削除、キャンセルボタン
                //----------------------------------------------------------------------
                var buttonExecute = labelFromUxml.Query<Button>("Execute").AtIndex(0);
                buttonExecute.text = EditorLocalize.LocalizeText(_okText);
                buttonExecute.style.alignContent = Align.FlexEnd;
                buttonExecute.clicked += RegisterOkAction(() =>
                {
                    _responded = true;
                    _callBackWindow(0);
                    Close();
                });

            }

            private ListView CreateListView() {
                StripedListView<int> listView = null;
                Action<VisualElement, int> bindItem = (e, i) =>
                {
                    e.Clear();
                    {
                        var index = (listView.itemsSource as List<int>)[i];
                        VisualElement visualElement = new IndexVisualElement(index);
                        //visualElement.style.flexDirection = FlexDirection.Row;

                        listView.SetVisualElementStriped(visualElement, index);
                        if (index == listView.itemsSource.Count - 1) listView.AddVisualElementStriped(e);

                        if (index >= 0)
                        {
                            var text = _resultList[index];

                            // Name
                            var label = new Label(text);
                            label.AddToClassList("text_ellipsis");
                            label.AddToClassList("list_view_item_name_label");
                            visualElement.Add(label);
                        }

                        e.Add(visualElement);
                    }
                };

                Func<VisualElement> makeItem = () => new Label();
                listView = new StripedListView<int>(new string[_resultList.Count], 16, makeItem, bindItem);
                listView.AddToClassList("list_view");
                listView.SolidQuad();
                listView.name = "list";
                listView.selectionType = SelectionType.Multiple;
                listView.reorderable = false;
                var list = new List<int>();
                for (int i = 0; i < _resultList.Count; i++)
                {
                    list.Add(i);
                }
                listView.itemsSource = list;

                return listView;
            }
        }

        public class TileGroupSameNameWindow : AddonBaseModalWindow
        {
            readonly Vector2Int WINDOW_SIZE = new Vector2Int(447, 276);
            bool _responded = false;

            protected override string ModalUxml
            {
                get
                {
                    return "Assets/RPGMaker/Codebase/Editor/DatabaseEditor/ModalWindow/Uxml/dlcimport_tilegroup_samename_window.uxml";
                }
            }

            protected override string ModalUss
            {
                get { return ""; }
            }


            public override void ShowWindow(string modalTitle, CallBackWidow callBack) {
                var wnd = GetWindow<TileGroupSameNameWindow>();

                if (callBack != null)
                {
                    _callBackWindow = callBack;
                }

                wnd.titleContent = new GUIContent(EditorLocalize.LocalizeText(_importConfirmTitle));
                Vector2 size = WINDOW_SIZE;
                wnd.minSize = size;
                //wnd.maxSize = size;
                wnd.maximized = false;
                wnd.Show();

            }

            private void CreateGUI() {
                if (_resultList == null)
                {
                    Close();
                    return;
                }
                this.Init();
            }

            private void OnDestroy() {
                if (!_responded)
                {
                    _callBackWindow(-2);
                }
            }

            public override void Init() {
                VisualElement root = rootVisualElement;

                // 要素作成
                //----------------------------------------------------------------------
                var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModalUxml);
                VisualElement labelFromUxml = visualTree.CloneTree();
                root.Add(labelFromUxml);

                labelFromUxml.style.flexGrow = 1;

                var list_window = labelFromUxml.Query<VisualElement>($"system_window_list_window").AtIndex(0);
                var textField = new TextField();
                textField.value = EditorLocalize.LocalizeText(_tileGroupSameNameAssetText);
                textField.multiline = true;
                textField.isReadOnly = true;
                textField.style.whiteSpace = WhiteSpace.Normal;
                list_window.Add(textField);

                var spacingVe = new VisualElement();
                spacingVe.style.width = 16;
                spacingVe.style.height = 8;
                list_window.Add(spacingVe);
                var listView = CreateListView();
                listView.RegisterCallback<KeyDownEvent>((e) =>
                {
                    //Debug.Log($"pressed: '{e.character}'");
                    switch (e.keyCode)
                    {
                        case KeyCode.C:
                            if ((e.modifiers & EventModifiers.Control) == EventModifiers.Control)
                            {
                                var list = new List<string>();
                                foreach (var i in listView.selectedIndices)
                                {
                                    list.Add(_resultList[i]);
                                }
                                GUIUtility.systemCopyBuffer = String.Join('\n', list);
                            }
                            break;
                    }
                });
                list_window.Add(listView);

                // 削除、キャンセルボタン
                //----------------------------------------------------------------------
                var buttonExecute = labelFromUxml.Query<Button>("Execute").AtIndex(0);
                buttonExecute.text = EditorLocalize.LocalizeText(_okText);
                buttonExecute.style.alignContent = Align.FlexEnd;
                buttonExecute.clicked += RegisterOkAction(() =>
                {
                    _responded = true;
                    _callBackWindow(0);
                    Close();
                });
                //----------------------------------------------------------------------
                var buttonCancel = labelFromUxml.Query<Button>("Common_Button_Cancel").AtIndex(0);
                buttonCancel.text = EditorLocalize.LocalizeText(_noText);
                buttonCancel.clicked += () =>
                {
                    _responded = true;
                    _callBackWindow(-1);
                    Close();
                };

            }

            private ListView CreateListView() {
                StripedListView<int> listView = null;
                Action<VisualElement, int> bindItem = (e, i) =>
                {
                    e.Clear();
                    {
                        var index = (listView.itemsSource as List<int>)[i];
                        VisualElement visualElement = new IndexVisualElement(index);
                        //visualElement.style.flexDirection = FlexDirection.Row;

                        listView.SetVisualElementStriped(visualElement, index);
                        if (index == listView.itemsSource.Count - 1) listView.AddVisualElementStriped(e);

                        if (index >= 0)
                        {
                            var text = _resultList[index];

                            // Name
                            var label = new Label(text);
                            label.AddToClassList("text_ellipsis");
                            label.AddToClassList("list_view_item_name_label");
                            visualElement.Add(label);
                        }

                        e.Add(visualElement);
                    }
                };

                Func<VisualElement> makeItem = () => new Label();
                listView = new StripedListView<int>(new string[_resultList.Count], 16, makeItem, bindItem);
                listView.AddToClassList("list_view");
                listView.SolidQuad();
                listView.name = "list";
                listView.selectionType = SelectionType.Multiple;
                listView.reorderable = false;
                var list = new List<int>();
                for (int i = 0; i < _resultList.Count; i++)
                {
                    list.Add(i);
                }
                listView.itemsSource = list;

                return listView;
            }
        }

#endregion

#region 変数
        static bool _editorReady = false;
        static string _dlcName;    
        static Dictionary<string, Dictionary<string, List<FilenameChoicePair>>> _dlcInfoDic = new Dictionary<string, Dictionary<string, List<FilenameChoicePair>>>();
        static Dictionary<string, string> _tileGroupDic = new Dictionary<string, string>();
        static Dictionary<string, string> _categoryStoragePathDic = new Dictionary<string, string>()
        {
            { "Animation/Effekseer", "Animation/Effekseer" },
            { "Animation/Prefab", "Animation/Prefab" },
            { "Images/Background/Battle/01", "Images/Background/Battle/01" },
            { "Images/Background/Battle/02", "Images/Background/Battle/02" },
            { "Images/Characters", "Images/Characters" },
            { "Images/Enemy", "Images/Enemy" },
            { "Images/Faces", "Images/Faces" },
            { "Images/Objects", "Images/Objects" },
            { "Images/Parallaxes", "Images/Parallaxes" },
            { "Images/Pictures", "Images/Pictures" },
            { "Images/SV_Actors", "Images/SV_Actors" },
            { "Images/System/Balloon", "Images/System/Balloon" },
            { "Images/System/IconSet", "Images/System/IconSet" },
            { "Images/System/Status", "Images/System/Status" },
            { "Images/System/Weapon", "Images/System/Weapon" },
            { "Images/Titles1", "Images/Titles1" },
            { "Images/Titles2", "Images/Titles2" },
            { "Images/Ui/Bg", "Images/Ui/Bg" },
            { "Map/BackgroundImages", "Map/BackgroundImages" },
            { "Map/TileImages", "Map/TileImages" },
            { "Movies", "Movies" },
            { "Sounds/BGM", "Sounds/BGM" },
            { "Sounds/BGS", "Sounds/BGS" },
            { "Sounds/ME", "Sounds/ME" },
            { "Sounds/SE", "Sounds/SE" },
        };
        static int _collisionFileCount;
        static EnumChoice _choice;
        static bool _skipSameAssets = false;
        static string _currentDlcPath;
        static List<string> _resultList;
        static Dictionary<string, TileDataModel.Type> _tileTypeNameTypeDic = new Dictionary<string, TileDataModel.Type> {
            { "AutoTileA", TileDataModel.Type.AutoTileA },
            { "AutoTileB", TileDataModel.Type.AutoTileB },
            { "AutoTileC", TileDataModel.Type.AutoTileC },
            { "NormalTile", TileDataModel.Type.NormalTile },
            { "LargeParts", TileDataModel.Type.LargeParts },
            { "Effect", TileDataModel.Type.Effect },
        };
        static AssetManageHierarchy _assetManageHierarchy;
        static List<AssetManageDataModel> _walkingCharacterAssets;
        static List<AssetManageDataModel> _objectAssets;
        static List<AssetManageDataModel> _actorAssets;
        static List<AssetManageDataModel> _weaponAssets;
        static FieldInfo _cacheUsableFi;
        static MapManagementService _mapManagementService;
        static FieldInfo _tileRepositoryFi;
        static TileGroupRepository _tileGroupRepository;
        static DatabaseManagementService _databaseManagementService;
        static bool _initializing = false;        
        static ImportStatus _importStatus = ImportStatus.NotFinished;
#endregion
        
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload) {
            var doCheck = false;
            foreach (string str in importedAssets)
            {
                //Debug.Log("Reimported Asset: " + str);
                if (str.StartsWith(_dlcBasePath))
                {
                    doCheck = true;
                    break;
                }
            }
            foreach (string str in deletedAssets)
            {
                //Debug.Log("Deleted Asset: " + str);
                if (str.StartsWith(_dlcBasePath))
                {
                    doCheck = true;
                    break;
                }
            }

            foreach (string str in movedAssets)
            {
                //Debug.Log("Moved Asset: " + str);
                if (str.StartsWith(_dlcBasePath))
                {
                    doCheck = true;
                    break;
                }
            }
            foreach (string str in movedFromAssetPaths)
            {
                //Debug.Log("MovedFrom Asset: " + str);
                if (str.StartsWith(_dlcBasePath))
                {
                    doCheck = true;
                    break;
                }
            }

            if (didDomainReload)
            {
                var first = FirstExecution.instance.IsFirst();
                //Debug.Log($"Domain has been reloaded: {first}");
                doCheck = first;
            }
            if (doCheck)
            {
                InitializeAsync();
            }
        }

        static async Task InitializeAsync() {
            if (_initializing) return;
            _initializing = true;
            await Task.Delay(10);
            await Task.Delay(500);
            if (!EditorApplication.isPlaying)
            {
                await ImportProcessAsync();
            }
            _initializing = false;
        }

        static async Task ImportProcessAsync() {
            try
            {
                _currentDlcPath = string.Empty;
                _assetManageHierarchy = (AssetManageHierarchy) typeof(RPGMaker.Codebase.Editor.Hierarchy.Hierarchy).GetField("_assetManageHierarchy", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                if (_assetManageHierarchy == null) return;  // skip if failed to get a instance.
                RefreshWalkingCharacterAssets();
                RefreshObjectAssets();
                RefreshActorAssets();
                RefreshWeaponAssets();

                //_databaseManagementService = (DatabaseManagementService) typeof(AssetManageHierarchy).GetField("_databaseManagementService", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_assetManageHierarchy);
                //Debug.Log($"_databaseManagementService: {_databaseManagementService}");
                _databaseManagementService = RPGMaker.Codebase.Editor.Hierarchy.Hierarchy.databaseManagementService;

                //Debug.Log($"_tileImageEntitiesFi: {_tileImageEntitiesFi}");
                _cacheUsableFi = typeof(TileImageRepository).GetField("_cacheUsable", BindingFlags.NonPublic | BindingFlags.Static);
                //Debug.Log($"_cacheUsableFi: {_cacheUsableFi}");

                _mapManagementService = (MapManagementService) typeof(RPGMaker.Codebase.Editor.MapEditor.MapEditor).GetField("_mapManagementService", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                //Debug.Log($"_mapManagementService: {_mapManagementService}");

                _tileRepositoryFi = typeof(MapManagementService).GetField("_tileRepository", BindingFlags.NonPublic | BindingFlags.Instance);
                //Debug.Log($"_tileRepositoryFi: {_tileRepositoryFi}");

                _dlcInfoDic.Clear();
                _tileGroupDic.Clear();
                await WaitForEditorReady();

                // 未インポートのDLC一覧を取得
                IEnumerable<string> notYetImportedDlcs = GetNotYetImportedDlcDirectories();

                foreach (string notYetImportedDlc in notYetImportedDlcs) {
                    _currentDlcPath = notYetImportedDlc;
                    var filename = $"{_currentDlcPath}{_importedFlagFilename}";

                    _dlcName = notYetImportedDlc.Substring(_dlcBasePath.Length);

                    // DLCをインポートしますか？のダイアログ
                    var code = EditorUtility.DisplayDialogComplex(EditorLocalize.LocalizeText(_importConfirmTitle), $"{_dlcName}\n{EditorLocalize.LocalizeText(_importConfirmMessage)}", EditorLocalize.LocalizeText(_yesText), EditorLocalize.LocalizeText(_cancelText), EditorLocalize.LocalizeText(_noText));
                    if (code == 1)  //Cancel
                    {
                        continue;
                    }
                    if (code == 2)    //No
                    {
                        File.WriteAllText(filename, "skipped");
                        continue;
                    }

                    //Yes
                    if (!LoadDlcInfo())
                    {
                        // エラーが起きた
                        EditorUtility.DisplayDialog(EditorLocalize.LocalizeText(_importConfirmTitle), $"{_dlcName}\n{EditorLocalize.LocalizeText(_importFailedMessage)}", EditorLocalize.LocalizeText(_okText));
                        continue;
                    }

                    if ((await CheckSameNames()) == false) return;

                    // Do import!
                    int fileCount = 0;
                    foreach (var dlcInfoItem in _dlcInfoDic)
                    {
                        foreach (var assetInfoItem in dlcInfoItem.Value)
                        {
                            fileCount += assetInfoItem.Value.Count;
                        }
                    }

                    int count = 0;
                    _resultList = new List<string>();
                    try
                    {
                        //  タイルグループインポート
                        foreach (var tileGroupInfo in _tileGroupDic)
                        {
                            var unitypackageFilename = $"{_currentDlcPath}{tileGroupInfo.Value}";
                            Debug.Log(unitypackageFilename + "をインポートします");
                            AssetDatabase.importPackageCompleted += ImportCompleted;
                            AssetDatabase.importPackageCancelled += ImportCancelled;
                            AssetDatabase.importPackageFailed += ImportCallBackFailed;
                            _importStatus = ImportStatus.NotFinished;
                            AssetDatabase.ImportPackage(unitypackageFilename, true);
                            while (_importStatus == ImportStatus.NotFinished)
                            {
                                await Task.Delay(100);
                            }
                            AssetDatabase.importPackageCompleted -= ImportCompleted;
                            AssetDatabase.importPackageCancelled -= ImportCancelled;
                            AssetDatabase.importPackageFailed -= ImportCallBackFailed;
                            if (_importStatus != ImportStatus.Completed)
                            {
                                return;
                            }
                            var categoryInfoArr = tileGroupInfo.Key.Split(',');
                            var tgdFilename = $"{_dlcBasePath}${categoryInfoArr[1]}.tgd";
                            await Task.Delay(1);
                            if (!File.Exists(tgdFilename))
                            {
                                //インポートされなかった。
                                return;
                            }
                            var created = ImportTgd(tgdFilename);
                            AssetDatabase.DeleteAsset(tgdFilename);
                            _resultList.Add($"{(created ? "Created" : "Updated")} TileGroup {categoryInfoArr[1]}");
                        }
                        var itemAdded = false;
                        var reloadTileImageEntities = false;
                        var reloadTileGroups = false;
                        var mapTileRegDicDic = new Dictionary<string, Dictionary<string, List<string>>>();
                        foreach (var dlcInfoItem in _dlcInfoDic)
                        {
                            if (_tileGroupDic.ContainsKey(dlcInfoItem.Key))
                            {
                                continue;
                            }
                            var categoryInfoArr = dlcInfoItem.Key.Split(',');
                            var category = categoryInfoArr[0];
                            var categorySub = (categoryInfoArr.Length >= 2) ? categoryInfoArr[1] : string.Empty;
                            foreach (var assetInfoItem in dlcInfoItem.Value)
                            {
                                EditorUtility.DisplayProgressBar(EditorLocalize.LocalizeText(_importConfirmTitle), EditorLocalize.LocalizeText(_dlcImportingText), (float) count / fileCount);
                                var dstFilenames = new List<string>();
                                var importFileDataList = new List<ImportFileData>();
                                ImportFileData importFileData = null;
                                foreach (var assetFilenameChoice in assetInfoItem.Value)
                                {
                                    count++;
                                    var dstFilename = $"{_storagePath}{_categoryStoragePathDic[category]}/{GetRemovedPathFilename(assetFilenameChoice.filename)}";
                                    if (assetFilenameChoice.choice == EnumChoice.Skip)
                                    {
                                        _resultList.Add($"Unchanged {GetSubInfoCutString(assetFilenameChoice.filename)}");
                                        dstFilenames.Add(dstFilename);
                                        continue;
                                    }
                                    var srcFilename = $"{_currentDlcPath}{category}/{assetFilenameChoice.filename}";
                                    if (assetFilenameChoice.choice == EnumChoice.Overwrite)
                                    {
                                        File.Delete(GetSubInfoCutString(dstFilename));
                                        _resultList.Add($"Overwrite {GetSubInfoCutString(assetFilenameChoice.filename)}");
                                    }
                                    else
                                    if (assetFilenameChoice.choice == EnumChoice.Rename)
                                    {
                                        dstFilename = GetRenamedFilename($"{_storagePath}{_categoryStoragePathDic[category]}/", GetRemovedPathFilename(assetFilenameChoice.filename));
                                        //Debug.Log($"newDstFilename: {dstFilename}");
                                        _resultList.Add($"Renamed {GetSubInfoCutString(assetFilenameChoice.filename)} -> {GetSubInfoCutString(GetRemovedPathFilename(dstFilename))}");
                                    }
                                    else if (assetFilenameChoice.choice == EnumChoice.Import)
                                    {
                                        _resultList.Add($"Imported {GetSubInfoCutString(assetFilenameChoice.filename)}");
                                    }
                                    else if (assetFilenameChoice.choice == EnumChoice.Cancel)
                                    {
                                        EditorUtility.ClearProgressBar();
                                        return;
                                    }
                                    else
                                    {
                                        throw new Exception($"Must be import: {assetFilenameChoice.choice.ToString()}");
                                    }
                                    dstFilenames.Add(dstFilename);
                                    if (category == kCategoryMapBackground)
                                    {
                                        //var result = AssetManageImporter.ImportFile(GetSubInfoCutString(srcFilename), GetSubInfoCutString(dstFilename), new Vector2(0, 1), true, false, true, true, true);
                                        importFileData = new ImportFileData(GetSubInfoCutString(srcFilename), GetSubInfoCutString(dstFilename));
                                        importFileData.Pivot = new Vector2(0, 1);
                                        importFileData.TextureSprite = true;
                                        importFileData.TextureReadable = false;
                                        importFileData.TextureWrap = true;
                                        importFileData.SetPixelsPerUnit = true;
                                        importFileData.SetLargeSize = true;
                                        importFileDataList.Add(importFileData);
                                    }
                                    else if (category == kCategoryParallax)
                                    {
                                        //var result = AssetManageImporter.ImportFile(GetSubInfoCutString(srcFilename), GetSubInfoCutString(dstFilename), new Vector2(0, 1), true, false, true, true);
                                        importFileData = new ImportFileData(GetSubInfoCutString(srcFilename), GetSubInfoCutString(dstFilename));
                                        importFileData.Pivot = new Vector2(0, 1);
                                        importFileData.TextureSprite = true;
                                        importFileData.TextureReadable = false;
                                        importFileData.TextureWrap = true;
                                        importFileData.SetPixelsPerUnit = true;
                                        importFileDataList.Add(importFileData);
                                    }
                                    else if (category == kCategoryEffekseer || category == kCategoryPrefab)
                                    {
                                        var animationList =
                                            AssetManageImporter.ImportZip_Effect<AnimationDataModel>(
                                                srcFilename,
                                                new List<string> { "png", "ogg", "wav" },
                                                new List<string>
                                                {
                                                    PathManager.IMAGE_ANIMATION,
                                                    PathManager.SOUND_SE,
                                                    PathManager.SOUND_SE,
                                                },
                                                new List<string>
                                                {
                                                    PathManager.ANIMATION_PREFAB,
                                                    PathManager.ANIMATION_EFFEKSEER,
                                                });

                                        // データがある
                                        if (animationList != null && animationList.Count > 0)
                                        {
                                            // 読み込んだJSONデータを適用する
                                            var animationDataModels = _databaseManagementService.LoadAnimation();

                                            for (var i2 = 0; i2 < animationList.Count; i2++)
                                            {
                                                // IDのみ新規設定
                                                animationList[i2].id = Guid.NewGuid().ToString();
                                                animationDataModels.Add(animationList[i2]);
                                                _databaseManagementService.SaveAnimation(animationDataModels);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //var result = AssetManageImporter.ImportFile(GetSubInfoCutString(srcFilename), GetSubInfoCutString(dstFilename));
                                        importFileData = new ImportFileData(GetSubInfoCutString(srcFilename), GetSubInfoCutString(dstFilename));
                                        importFileDataList.Add(importFileData);
                                    }
                                    if (category == kCategoryMapTileImages)
                                    {
                                        reloadTileImageEntities = true;
                                    }
                                }
                                if (importFileDataList.Count > 0)
                                {
                                    var result = AssetManageImporter.ImportFile(importFileDataList);
                                    importFileDataList.Clear();
                                }
                                if (assetInfoItem.Key.Length > 0)
                                {

                                    if (category == kCategoryWalkChara)
                                    {
                                        //歩行キャラを新規作成する。
                                        var existingIdList = _walkingCharacterAssets.Select(x => x.id).ToList();
                                        _assetManageHierarchy.CreateAssetManageDataModel(AssetCategoryEnum.MOVE_CHARACTER);
                                        RefreshWalkingCharacterAssets();
                                        var walkingCharacterAsset = _walkingCharacterAssets.Where(x => !existingIdList.Contains(x.id)).FirstOrDefault();
                                        walkingCharacterAsset.name = assetInfoItem.Key;
                                        for (int i = 0; i < 5; i++)
                                        {
                                            var imageSetting = walkingCharacterAsset.imageSettings[i];
                                            if (i > dstFilenames.Count - 1 || dstFilenames[i].Length == 0) continue;
                                            var dstFilename = GetSubInfoCutString(dstFilenames[i]);
                                            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(dstFilename);
                                            var fn = GetRemovedPathFilename(dstFilename);
                                            var match = Regex.Match(fn, @"_(\d+)\.");
                                            if (match.Success)
                                            {
                                                int n;
                                                if (int.TryParse(match.Groups[1].Value, out n))
                                                {
                                                    imageSetting.animationFrame = n;
                                                }

                                            }
                                            imageSetting.path = fn;
                                            imageSetting.sizeX = tex.width;
                                            imageSetting.sizeY = tex.height;
                                            var animationSpeed = GetSubInfoValue(dstFilenames[i]);
                                            imageSetting.animationSpeed = (animationSpeed >= 0 ? animationSpeed : imageSetting.animationFrame == 1 ? 0 : 40);
                                        }
                                        _databaseManagementService.SaveAssetManage(walkingCharacterAsset);
                                        _resultList.Add($"Created MoveChara {assetInfoItem.Key}");
                                        itemAdded = true;
                                    }
                                    else if (category == kCategoryCharaObject)
                                    {
                                        //キャラオブジェクトを新規作成する。
                                        var existingIdList = _objectAssets.Select(x => x.id).ToList();
                                        _assetManageHierarchy.CreateAssetManageDataModel(AssetCategoryEnum.OBJECT);
                                        RefreshObjectAssets();
                                        var objectAsset = _objectAssets.Where(x => !existingIdList.Contains(x.id)).FirstOrDefault();
                                        objectAsset.name = assetInfoItem.Key;
                                        for (int i = 0; i < 4; i++)
                                        {
                                            var imageSetting = objectAsset.imageSettings[i];
                                            if (i > dstFilenames.Count - 1 || dstFilenames[i].Length == 0) continue;
                                            var dstFilename = GetSubInfoCutString(dstFilenames[i]);
                                            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(dstFilename);
                                            var fn = GetRemovedPathFilename(dstFilename);
                                            var match = Regex.Match(fn, @"_(\d+)\.");
                                            if (match.Success)
                                            {
                                                int n;
                                                if (int.TryParse(match.Groups[1].Value, out n))
                                                {
                                                    imageSetting.animationFrame = n;
                                                }

                                            }
                                            imageSetting.path = fn;
                                            imageSetting.sizeX = tex.width;
                                            imageSetting.sizeY = tex.height;
                                            var animationSpeed = GetSubInfoValue(dstFilenames[i]);
                                            imageSetting.animationSpeed = (animationSpeed >= 0 ? animationSpeed : imageSetting.animationFrame == 1 ? 0 : 30);
                                        }
                                        _databaseManagementService.SaveAssetManage(objectAsset);
                                        _resultList.Add($"Created Object {assetInfoItem.Key}");
                                        itemAdded = true;
                                    }
                                    else if (category == kCategorySvActor)
                                    {
                                        //SVアクターを新規作成する。
                                        var existingIdList = _actorAssets.Select(x => x.id).ToList();
                                        _assetManageHierarchy.CreateAssetManageDataModel(AssetCategoryEnum.SV_BATTLE_CHARACTER);
                                        RefreshActorAssets();
                                        var actorAsset = _actorAssets.Where(x => !existingIdList.Contains(x.id)).FirstOrDefault();
                                        actorAsset.name = assetInfoItem.Key;
                                        for (int i = 0; i < 18; i++)
                                        {
                                            var imageSetting = actorAsset.imageSettings[i];
                                            if (i > dstFilenames.Count - 1 || dstFilenames[i].Length == 0) continue;
                                            var dstFilename = dstFilenames[i];
                                            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(dstFilename);
                                            var fn = GetRemovedPathFilename(dstFilename);
                                            var match = Regex.Match(fn, @"_(\d+)\.");
                                            if (match.Success)
                                            {
                                                int n;
                                                if (int.TryParse(match.Groups[1].Value, out n))
                                                {
                                                    imageSetting.animationFrame = n;
                                                }

                                            }
                                            imageSetting.path = fn;
                                            imageSetting.sizeX = tex.width;
                                            imageSetting.sizeY = tex.height;
                                            imageSetting.animationSpeed = 20;
                                        }
                                        _databaseManagementService.SaveAssetManage(actorAsset);
                                        _resultList.Add($"Created SvActor {assetInfoItem.Key}");
                                        itemAdded = true;
                                    }
                                    else if (category == kCategorySvWeapon)
                                    {
                                        //SV戦闘用武器を新規作成する。
                                        var existingIdList = _weaponAssets.Select(x => x.id).ToList();
                                        _assetManageHierarchy.CreateAssetManageDataModel(AssetCategoryEnum.SV_WEAPON);
                                        RefreshWeaponAssets();
                                        var weaponAsset = _weaponAssets.Where(x => !existingIdList.Contains(x.id)).FirstOrDefault();
                                        var index = assetInfoItem.Key.IndexOf(',');
                                        var name = (index < 0) ? assetInfoItem.Key : assetInfoItem.Key.Substring(0, index);
                                        var weaponTypeId = 0;
                                        weaponAsset.name = name;
                                        if (index > 0 && int.TryParse(assetInfoItem.Key.Substring(index + 1), out weaponTypeId))
                                        {
                                            weaponAsset.weaponTypeId = weaponTypeId;
                                        }
                                        for (int i = 0; i < 1; i++)
                                        {
                                            var imageSetting = weaponAsset.imageSettings[i];
                                            if (i > dstFilenames.Count - 1 || dstFilenames[i].Length == 0) continue;
                                            var dstFilename = dstFilenames[i];
                                            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(dstFilename);
                                            var fn = GetRemovedPathFilename(dstFilename);
                                            var match = Regex.Match(fn, @"_(\d+)\.");
                                            if (match.Success)
                                            {
                                                int n;
                                                if (int.TryParse(match.Groups[1].Value, out n))
                                                {
                                                    imageSetting.animationFrame = n;
                                                }

                                            }
                                            imageSetting.path = fn;
                                            imageSetting.sizeX = tex.width;
                                            imageSetting.sizeY = tex.height;
                                            imageSetting.animationSpeed = 20;
                                        }
                                        _databaseManagementService.SaveAssetManage(weaponAsset);
                                        _resultList.Add($"Created SvWeapon {name}");
                                        itemAdded = true;
                                    }
                                    else if (category == kCategoryMapTileImages)
                                    {
                                        if (!mapTileRegDicDic.ContainsKey(categorySub))
                                        {
                                            mapTileRegDicDic.Add(categorySub, new Dictionary<string, List<string>>());
                                        }
                                        mapTileRegDicDic[categorySub].Add(assetInfoItem.Key, dstFilenames);
                                    }
                                }
                            }
                        }

                        if (mapTileRegDicDic.Count > 0)
                        {
                            if (ApiManager.IsAssetDatabaseStop)
                            {
                                throw new Exception("Fatal error: ApiManager.IsAssetDatabaseStop is true");
                            }
                            fileCount = 0;
                            foreach (var dicEntry in mapTileRegDicDic)
                            {
                                foreach (var entity in dicEntry.Value)
                                {
                                    foreach (var dstFilename in entity.Value)
                                    {
                                        fileCount++;
                                    }
                                }
                            }
                            count = 0;
                            if (reloadTileImageEntities)
                            {
                                _cacheUsableFi.SetValue(null, false);
                                RPGMaker.Codebase.Editor.MapEditor.MapEditor.ReloadTileImageEntities();
                                reloadTileImageEntities = false;
                            }
                            TileRepository tileRepository = null;
                            foreach (var dicEntry in mapTileRegDicDic)
                            {
                                var tileGroupName = dicEntry.Key;
                                TileGroupDataModel newTileGroupDataModel = null;
                                if (tileGroupName.Length > 0)
                                {
                                    newTileGroupDataModel = RPGMaker.Codebase.Editor.MapEditor.MapEditor.LaunchTileGroupEditMode(null);
                                    newTileGroupDataModel.name = tileGroupName;
                                    if (tileRepository == null)
                                    {
                                        tileRepository = (TileRepository) _tileRepositoryFi.GetValue(_mapManagementService);
                                    }
                                }
                                var tileDataModelList = new List<TileDataModel>();
                                bool tileAdded = false;
                                foreach (var entity in dicEntry.Value)
                                {
                                    //マップタイルを登録する。
                                    var tileImageDataModels = GetTileImageEntities();
                                    var list = entity.Key.Split(',');
                                    if (list.Length >= 2 && _tileTypeNameTypeDic.ContainsKey(list[1]))
                                    {
                                        var name = list[0];
                                        foreach (var dstFilename in entity.Value)
                                        {
                                            EditorUtility.DisplayProgressBar(EditorLocalize.LocalizeText(_importConfirmTitle), EditorLocalize.LocalizeText(_dlcImportingText), (float) count / fileCount);
                                            count++;
#if true    //for debug
                                            Debug.Log($"Processing ... {dstFilename}");
                                            await Task.Delay(1);
#endif
                                            var type = _tileTypeNameTypeDic[list[1]];
                                            List<TileDataModelInfo> lastTileEntities = null;
                                            if (type == TileDataModel.Type.NormalTile)
                                            {
                                                lastTileEntities = _mapManagementService.LoadTileTable();
                                            }
                                            var fn = GetRemovedPathFilename(dstFilename);
                                            var tileImageDataModel = tileImageDataModels.FirstOrDefault(x => x.filename == fn);
                                            var tileDataModel = RPGMaker.Codebase.Editor.MapEditor.MapEditor.CreateTile(tileImageDataModel, type);
                                            if (tileDataModel.type == TileDataModel.Type.NormalTile)
                                            {
                                                tileDataModel.imageAdjustType = TileDataModel.ImageAdjustType.Split;
                                            }
                                            if (tileDataModel.type == TileDataModel.Type.Effect)
                                            {
                                                tileDataModel.hasAnimation = true;
                                                if (list.Length < 4)
                                                {
                                                    Debug.LogError($"Tile Effect needs parameters of frame count and play speed: {dstFilename}");
                                                    continue;
                                                }
                                            }
                                            if (tileDataModel.type == TileDataModel.Type.AutoTileA
                                                || tileDataModel.type == TileDataModel.Type.AutoTileB
                                                || tileDataModel.type == TileDataModel.Type.AutoTileC
                                                || tileDataModel.type == TileDataModel.Type.NormalTile
                                                || tileDataModel.type == TileDataModel.Type.LargeParts)
                                            {
                                                if (list.Length >= 4)
                                                {
                                                    tileDataModel.hasAnimation = true;
                                                }
                                            }
                                            if (tileDataModel.hasAnimation)
                                            {
                                                tileDataModel.animationFrame = int.Parse(list[2]);
                                                tileDataModel.animationSpeed = int.Parse(list[3]);
                                            }
                                            tileDataModel.name = name;
                                            //await RPGMaker.Codebase.Editor.MapEditor.MapEditor.SaveTile(tileDataModel);
                                            if (tileDataModel.type != TileDataModel.Type.LargeParts && tileDataModel.type != TileDataModel.Type.NormalTile)
                                            {
                                                tileDataModelList.Add(tileDataModel);
                                            } else
                                            {
                                                await RPGMaker.Codebase.Editor.MapEditor.MapEditor.SaveTile(tileDataModelList);
                                                tileDataModelList.Clear();
                                                await RPGMaker.Codebase.Editor.MapEditor.MapEditor.SaveTile(tileDataModel);
                                            }
                                            _resultList.Add($"Created MapTile {list[1]} {name}");
                                            if (newTileGroupDataModel != null)
                                            {
                                                //大型パーツの場合は特殊処理を実施
                                                if (tileDataModel.type == TileDataModel.Type.LargeParts)
                                                {
                                                    var tileEntities = tileRepository.GetTileTable();
                                                    /*foreach (var tileEntity in tileEntities)
                                                    {
                                                        if (tileEntity.type == TileDataModel.Type.LargeParts)
                                                        {
                                                            Debug.Log($"tileEntity.largePartsDataModel.parentId: {tileEntity.largePartsDataModel.parentId}");
                                                        }
                                                    }*/
                                                    var tiles = tileEntities.Where(
                                                        t => t.type == TileDataModel.Type.LargeParts &&
                                                             t.largePartsDataModel.parentId == tileDataModel.id);

                                                    foreach (var tile in tiles)
                                                        if (newTileGroupDataModel.tileDataModels.Find(t => t.id == tile.id) == null)
                                                        {
                                                            newTileGroupDataModel.tileDataModels.Add(tile);
                                                            tileAdded = true;
                                                        }
                                                }
                                                else if (tileDataModel.type == TileDataModel.Type.NormalTile)
                                                {
                                                    foreach (var tdm in _mapManagementService.LoadTileTable())
                                                    {
                                                        if (lastTileEntities.Contains(tdm)) continue;
                                                        if (newTileGroupDataModel.tileDataModels.Find(t => t.id == tdm.id) == null)
                                                        {
                                                            newTileGroupDataModel.tileDataModels.Add(tdm);
                                                            tileAdded = true;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (newTileGroupDataModel.tileDataModels.Find(t => t.id == tileDataModel.id) == null)
                                                    {
                                                        newTileGroupDataModel.tileDataModels.Add(tileDataModel.tileDataModelInfo);
                                                        tileAdded = true;
                                                    }
                                                }

                                            }
                                        }
                                    }
                                    else
                                    {
                                        //不明
                                        Debug.LogError($"Unknown MapTile: {list[1]}");
                                    }
                                }
                                if (tileDataModelList.Count > 0)
                                {
                                    await RPGMaker.Codebase.Editor.MapEditor.MapEditor.SaveTile(tileDataModelList);
                                    tileDataModelList.Clear();
                                }
                                EditorUtility.ClearProgressBar();
                                if (tileAdded)
                                {
                                    //_tilesInGroup.Refresh(newTileGroupDataModel.tileDataModels);
                                    RPGMaker.Codebase.Editor.MapEditor.MapEditor.SaveTileGroup(newTileGroupDataModel);
                                    reloadTileGroups = true;
                                }
                                if (newTileGroupDataModel != null)
                                {
                                    _resultList.Add($"Created TileGroup {tileGroupName}");
                                }
                            }
                        }
                        if (itemAdded)
                        {
                            _assetManageHierarchy.Refresh();
                        }
                        if (reloadTileImageEntities)
                        {
                            _cacheUsableFi.SetValue(null, false);
                            RPGMaker.Codebase.Editor.MapEditor.MapEditor.ReloadTileImageEntities();
                        }
                        if (reloadTileGroups)
                        {
                            RPGMaker.Codebase.Editor.MapEditor.MapEditor.ReloadTileGroups();
                        }
                    } catch (Exception e)
                    {
                        EditorUtility.ClearProgressBar();
                        Debug.LogError(e);
                        EditorUtility.DisplayDialog(EditorLocalize.LocalizeText(_importConfirmTitle), "Import Error", EditorLocalize.LocalizeText(_okText));
                        return;
                    }
                    EditorUtility.ClearProgressBar();

                    File.WriteAllText(filename, "done");
                    var window = EditorWindow.GetWindow<ResultWindow>();
                    window.ShowWindow("", (o) =>
                    {
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        /**
        * 未インポートのDLCの一覧を取得（ディレクトリ）
        */
        static IEnumerable<string> GetNotYetImportedDlcDirectories() {
            return Directory.EnumerateFiles($"{_dlcBasePath}", _importedFlagFilename, SearchOption.AllDirectories)
                    // 未インポートのみ
                    .Where(filePath => {
                        string status = File.ReadAllText(filePath);
                        return !(status == "done" || status == "skipped");
                    })
                    .Select(filePath => Regex.Replace(Path.GetDirectoryName(filePath) + "/", @"\\", "/")) ;
        }

        static string GetRenamedFilename(string path, string filename) {
            var index = filename.IndexOf('.');
            var basename = filename.Substring(0, index);
            var extension = filename.Substring(index + 1);
            var files = Directory.GetFiles(path, $"{basename} (*).{extension}", SearchOption.TopDirectoryOnly);
            int number = 2;
            for (int i = 2; i < files.Length + 3; i++)
            {
                var renamedFilename = $"{path}{basename} ({i}).{extension}";
                if (!files.Contains(renamedFilename))
                {
                    number = i;
                    break;
                }
            }
            return $"{path}{basename} ({number}).{extension}";
        }

        static void RefreshWalkingCharacterAssets() {
            _walkingCharacterAssets = (List<AssetManageDataModel>) typeof(AssetManageHierarchy).GetField("_walkingCharacterAssets", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_assetManageHierarchy);
        }

        static void RefreshObjectAssets() {
            _objectAssets = (List<AssetManageDataModel>) typeof(AssetManageHierarchy).GetField("_objectAssets", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_assetManageHierarchy);
        }

        static void RefreshActorAssets() {
            _actorAssets = (List<AssetManageDataModel>) typeof(AssetManageHierarchy).GetField("_actorAssets", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_assetManageHierarchy);
        }

        static void RefreshWeaponAssets() {
            _weaponAssets = (List<AssetManageDataModel>) typeof(AssetManageHierarchy).GetField("_weaponAssets", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_assetManageHierarchy);
        }

        private static void RegistTileTable(TileGroupJson json) {
            List<TileDataModelInfo> tileTable = new TileRepository().GetTileTable();
            int listNumber = tileTable.Select(taile => taile.listNumber).Max();

            string TileAssetFolderPath = "Assets/RPGMaker/Storage/Map/TileAssets/";

            // タイルテーブルへ登録する要素を作成
            List<TileDataModel> tileDataModelList = Directory.GetFiles(TileAssetFolderPath, "*.asset", SearchOption.AllDirectories)
                        .Select(assetPath => UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<TileDataModel>(assetPath))
                        .Where(tileAsset => tileAsset != null)
                        .ToList();
            foreach (var tile in json.tileList) {
                TileDataModel tileDataModel = tileDataModelList.Find(tileDataModel => tileDataModel.id == tile.id);
                if (tileDataModel == null) continue;

                listNumber++;
                TileDataModelInfo tileDataModelInfo = new TileDataModelInfo();
                tileDataModelInfo.id = tileDataModel.id;
                tileDataModelInfo.listNumber = listNumber;
                tileDataModelInfo.serialNumber = tileDataModel.serialNumber;
                tileDataModelInfo.largePartsDataModel = tileDataModel.largePartsDataModel;
                tileDataModelInfo.type = tileDataModel.type;
                tileTable.Add(tileDataModelInfo);

                string destPath = TileRepository.GetAssetPath(tileDataModelInfo, true);
                if (destPath == "")
                    continue;

                if (File.Exists(TileAssetFolderPath + tile.id + ".asset"))
                    File.Move(TileAssetFolderPath + tile.id + ".asset", destPath + tile.id + ".asset");
                if (File.Exists(TileAssetFolderPath + tile.id + ".asset.meta"))
                    File.Move(TileAssetFolderPath + tile.id + ".asset.meta", destPath + tile.id + ".asset.meta");
                if (File.Exists(TileAssetFolderPath + tile.id + ".meta"))
                    File.Move(TileAssetFolderPath + tile.id + ".meta", destPath + tile.id + ".meta");
                if (Directory.Exists(TileAssetFolderPath + tile.id))
                    Directory.Move(TileAssetFolderPath + tile.id, destPath + tile.id);
            }

            // タイルテーブルへ登録
            File.WriteAllText("Assets/RPGMaker/Storage/Map/JSON/tileTable.json", JsonHelper.ToJsonArray(tileTable));
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh2();
        }

        static bool ImportTgd(string assetPath) {
            typeof(TileImageRepository).GetField("_cacheUsable", BindingFlags.NonPublic | BindingFlags.Static)?.SetValue(null, false);
            RPGMaker.Codebase.Editor.MapEditor.MapEditor.ReloadTileImageEntities();

            var created = true;
            _mapManagementService = (MapManagementService) typeof(RPGMaker.Codebase.Editor.MapEditor.MapEditor).GetField("_mapManagementService", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            _tileGroupRepository = (TileGroupRepository) typeof(MapManagementService).GetField("_tileGroupRepository", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_mapManagementService);
            var convertJsonToEntityMi = typeof(TileGroupRepository).GetMethod("ConvertJsonToEntity", BindingFlags.Instance | BindingFlags.NonPublic);
            var json = JsonUtility.FromJson<TileGroupJson>(File.ReadAllText(assetPath));
            TileGroupDataModel model = null;
            foreach (var entity in _tileGroupRepository.GetTileGroupEntities())
            {
                if (entity.id == json.id)
                {
                    model = entity;
                    break;
                }
            }
            if (model == null) {
                RegistTileTable(json);
                model = (TileGroupDataModel) convertJsonToEntityMi.Invoke(_tileGroupRepository, new object[] { json });
            }
            else
            {
                created = false;
                var tileEntities = new TileRepository().GetTileTable();
                Debug.Log($"tileEntities.Count: {tileEntities.Count}");
                Debug.Log($"json.tileList.Count: {json.tileList.Count}");
                Debug.Log($"model.tileDataModels.Count: {model.tileDataModels.Count}");

                foreach (var tile in json.tileList)
                {
                    if (model.tileDataModels.Exists(x => x.id == tile.id))
                    {
                        // already contains.
                        Debug.Log($"already contains: {tile.id}");
                        continue;
                    }
                    var tileDataModel = tileEntities.FirstOrDefault(x => x.id == tile.id);
                    if (tileDataModel == null)
                    {
                        // tile not exists.
                        Debug.Log($"tile not exists: {tile.id}");
                        continue;
                    }
                    Debug.Log($"Add: {tile.id}, {tileDataModel.TileDataModel.name}, {tileDataModel.id}");
                    model.tileDataModels.Add(tileDataModel);
                }
            }
            _tileGroupRepository.StoreTileGroupEntity(model);
            RPGMaker.Codebase.Editor.MapEditor.MapEditor.ReloadTileGroups();
            return created;
        }

        static void ImportCompleted(string packageName) {
            //Debug.Log("Completed " + packageName);
            _importStatus = ImportStatus.Completed;
        }

        static void ImportCancelled(string packageName) {
            //Debug.Log("Cancelled " + packageName);
            _importStatus = ImportStatus.Cancelled;
        }

        static void ImportCallBackFailed(string packageName, string _error) {
            //Debug.Log("Failed " + packageName);
            _importStatus = ImportStatus.CallbackFailed;
        }

        static async Task WaitForEditorReady() {
            await Task.Delay(1);
            await Task.Delay(1);
            await Task.Delay(1);
            EditorReadyWindow.SetCallback(() =>
            {
                _editorReady = true;
            });
            var window = (EditorReadyWindow) EditorWindow.GetWindow(typeof(EditorReadyWindow));
            window.Show();
            while (!_editorReady)
            {
                await Task.Delay(100);
            }
            window.Close();
        }

        static bool LoadDlcInfo() {
            var importedInfoFilename = $"{_currentDlcPath}{_importedInfoFilename}";
            var lines = File.ReadAllText(importedInfoFilename).Replace("\r", "").Split("\n").Select(line => line.Split('\t'));
            //Debug.Log($"LoadDlcInfo: lines.Count: {lines.Count()}");
            string category = null;
            var unknownCategoryNames = new List<string>();
            foreach (var columns in lines)
            {
                //Debug.Log($"LoadDlcInfo: columns: {string.Join("\n", columns)}");
                if (columns.Length == 0) continue;
                if (columns[0].Length != 0)
                {
                    if (columns[0].StartsWith("#")) continue;
                    var categoryInfoArr = columns[0].Split(',');
                    category = columns[0];
                    if (!_categoryStoragePathDic.ContainsKey(categoryInfoArr[0]))
                    {
                        unknownCategoryNames.Add(categoryInfoArr[0]);
                    }
                }
                if (columns.Length < 2) continue;
                //if (columns[1].Length == 0) continue;
                if (columns[1] == ",TileGroup")
                {
                    if (_tileGroupDic.ContainsKey(category))
                    {
                        Debug.LogError($"Tile Group multiple specified: {category}");
                        return false;
                    }
                    if (columns.Length < 3)
                    {
                        Debug.LogError($"Tile Group unitypackage not specified: {category}");
                        return false;
                    }
                    var unitypackageFilename = $"{_currentDlcPath}{columns[2]}";
                    if (!File.Exists(unitypackageFilename))
                    {
                        Debug.LogError($"Tile Group unitypackage not found: {unitypackageFilename}");
                        return false;
                    }
                    _tileGroupDic.Add(category, columns[2]);
                    continue;
                }
                if (!_dlcInfoDic.ContainsKey(category))
                {
                    _dlcInfoDic.Add(category, new Dictionary<string, List<FilenameChoicePair>>());
                }
                if (!_dlcInfoDic[category].ContainsKey(columns[1]))
                {
                    _dlcInfoDic[category].Add(columns[1], new List<FilenameChoicePair>());
                }
                for (int i = 2; i < columns.Length; i++)
                {
                    _dlcInfoDic[category][columns[1]].Add(new FilenameChoicePair(columns[i], EnumChoice.Import));
                }
            }
            if (unknownCategoryNames.Count > 0)
            {
                Debug.LogError($"Unknown categories: {string.Join(", ", unknownCategoryNames)}");
                return false;
            }
            return true;
        }

        static string GetRemovedPathFilename(string filename) {
            var index = filename.LastIndexOf('/');
            if (index < 0) return filename;
            return filename.Substring(index + 1);
        }

        static string GetSubInfoCutString(string str) {
            var index = str.LastIndexOf(',');
            if (index < 0) return str;
            return str.Substring(0, index);
        }

        static int GetSubInfoValue(string str) {
            var index = str.LastIndexOf(',');
            if (index < 0) return -1;
            int value = 0;
            if (!int.TryParse(str.Substring(index + 1), out value))
            {
                return -1;
            }
            return value;
        }

        static async Task<bool> CheckSameNames() {
            foreach (var tileGroupInfo in _tileGroupDic)
            {
                if (!_dlcInfoDic.ContainsKey(tileGroupInfo.Key))
                {
                    Debug.Log($"Tile group asset info not found in import-info.txt: {tileGroupInfo.Key}");
                    return false;
                }
            }
            var collisionFilenames = new List<string>();
            var tileGroupCollisionFilenameChoisePairs  = new List<FilenameChoicePair>();
            foreach (var dlcInfoItem in _dlcInfoDic)
            {
                var category = dlcInfoItem.Key.Split(',')[0];
                var storagePath = $"{_storagePath}{_categoryStoragePathDic[category]}/";
                //Debug.Log($"storagePath: {storagePath}");
                foreach (var assetInfoItem in dlcInfoItem.Value)
                {
                    var assetName = assetInfoItem.Key;
                    var assetFilenameChoiceList = assetInfoItem.Value;
                    foreach (var assetFilenameChoice in assetFilenameChoiceList)
                    {
                        //Debug.Log($"check: {storagePath}{GetRemovedPathFilename(assetFilenameChoice.filename)}");
                        if (File.Exists($"{storagePath}{GetSubInfoCutString(GetRemovedPathFilename(assetFilenameChoice.filename))}"))
                        {
                            if (_tileGroupDic.ContainsKey(dlcInfoItem.Key))
                            {
                                tileGroupCollisionFilenameChoisePairs.Add(assetFilenameChoice);
                            }
                            else {
                                collisionFilenames.Add(assetFilenameChoice.filename);
                                assetFilenameChoice.choice = EnumChoice.NotChosenYet;
                            }
                        }
                    }
                }
            }
            if (tileGroupCollisionFilenameChoisePairs.Count > 0)
            {
                _resultList = tileGroupCollisionFilenameChoisePairs.Select(x => x.filename).ToList();
                int code = -3;
                var w = EditorWindow.GetWindow<TileGroupSameNameWindow>();
                w.ShowWindow("", (o) =>
                {
                    code = (int) o;
                });
                while (code == -3)
                {
                    await Task.Delay(100);
                }
                _resultList.Clear();
                if (code == -2)
                {
                    return false;
                }
                if (code == -1)
                {
                    foreach (var assetFilenameChoice in tileGroupCollisionFilenameChoisePairs)
                    {
                        collisionFilenames.Add(assetFilenameChoice.filename);
                        assetFilenameChoice.choice = EnumChoice.NotChosenYet;
                    }
                    tileGroupCollisionFilenameChoisePairs.Clear();
                    _tileGroupDic.Clear();
                }
            }
            _collisionFileCount = collisionFilenames.Count;
            //Debug.Log($"_collisionFileCount: {_collisionFileCount}");
            if (_collisionFileCount == 0) return true;

            _choice = EnumChoice.NotChosenYet;
            var window = EditorWindow.GetWindow<MakeChoiceWindow>();
            window.ShowWindow("", (choice) =>
            {
                _choice = (EnumChoice) choice;
            });
            while (_choice == EnumChoice.NotChosenYet)
            {
                await Task.Delay(100);
            }
            if (_choice == EnumChoice.Cancel) return false;
            //Debug.Log($"_choice: {_choice.ToString()}");
            foreach (var dlcInfoItem in _dlcInfoDic)
            {
                foreach (var assetInfoItem in dlcInfoItem.Value)
                {
                    var assetFilenameChoiceList = assetInfoItem.Value;
                    
                    foreach (var assetFilenameChoice in assetFilenameChoiceList)
                    {
                        if (assetFilenameChoice.choice == EnumChoice.NotChosenYet)
                        {
                            assetFilenameChoice.choice = _choice;
                        }
                    }
                }
            }
            if (_choice != EnumChoice.Select)
            {
            } else
            {
                int result = -2;
                var win2 = EditorWindow.GetWindow<SelectOnEachWindow>();
                win2.ShowWindow("", (code) =>
                {
                    result = (int) code;
                });
                while (result == -2)
                {
                    await Task.Delay(10);
                }
                if (result < 0) return false;
            }
            return true;
        }

#if false
            EditorUtility.RequestScriptReload();    //タイルデータの登録が変化する可能性があるので、ウィンドウを初期化。
#endif

        /**
         * インポート済みのタイル用画像をエンティティとして一覧取得する
         */
        private static List<TileImageDataModel> GetTileImageEntities() {
            return Directory.GetFiles(PathManager.MAP_TILE_IMAGE)
                .Select(Path.GetFileName)
                .Where(filename =>
                    filename.EndsWith(".gif") || filename.EndsWith(".jpg") || filename.EndsWith(".png"))
                .Select(filename => new TileImageDataModel(_mapManagementService.ReadImage(filename), filename))
                .ToList();
        }

    }

#endif
}
