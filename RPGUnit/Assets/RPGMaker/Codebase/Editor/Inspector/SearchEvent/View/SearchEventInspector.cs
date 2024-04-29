#define USE_HIERARCHY_SELECTION_IN_MOVE_BUTTON

using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.SearchEvent.View
{
    /// <summary>
    /// [アウトライン]-[イベント検索] Inspector
    /// </summary>
    public class SearchEventInspector : AbstractInspectorElement
    {
        private          List<ChapterDataModel>    _chapterDataModels;
        private          List<EventDataModel>      _eventDataModels;

        // 利用するデータ
        //--------------------------------------------------------------------------------------------------------------
        private          List<EventMapDataModel> _eventMapDataModels;
        private          string                  _eventNameWord;
        private          ImTextField               _eventText;
        private          FlagDataModel           _flagDataModel;

        // 項目名
        //--------------------------------------------------------------------------------------------------------------
        private readonly List<string> _header =
            EditorLocalize.LocalizeTexts(new List<string>
                {"WORD_0004", "WORD_0022", "WORD_0027", "WORD_0014", "WORD_0983"});

        private          Button                   _search;
        private          VisualElement            _searchResultAria;
        private          List<SectionDataModel>   _sectionDataModels;
        private          VisualElement            _switchFoldDown;
        private          string                   _switchId;

        // UI要素
        //--------------------------------------------------------------------------------------------------------------
        private Toggle _switchToggle;
        private Toggle _textToggle;

        // 検索に利用するデータ
        //--------------------------------------------------------------------------------------------------------------
        private int           _toggleNumber;
        private VisualElement _variableFoldDown;
        private string        _variableId;

        private Toggle _variableToggle;

        // const
        //--------------------------------------------------------------------------------------------------------------
        //インスペクター側
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/SearchEvent/Asset/inspector_searchEvent.uxml"; } }

        //検索結果表示用
        private readonly string resultUxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/AssetManage/Asset/inspector_search_result.uxml";

        public SearchEventInspector() {
            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            _flagDataModel = databaseManagementService.LoadFlags();

            //各イベントのload
            _eventMapDataModels = eventManagementService.LoadEventMap();
            _eventDataModels = eventManagementService.LoadEvent();

            //チャプター、セクションのload
            var outlineDataModel = outlineManagementService.LoadOutline();
            _chapterDataModels = outlineDataModel.Chapters;
            _sectionDataModels = outlineDataModel.Sections;

            Initialize();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();
            AssetDatabase.Refresh();

            //各UI要素
            _switchToggle = RootContainer.Query<Toggle>("switch_toggle");
            _switchFoldDown = RootContainer.Query<VisualElement>("switch_fold_down");
            _variableToggle = RootContainer.Query<Toggle>("variable_toggle");
            _variableFoldDown = RootContainer.Query<VisualElement>("variable_fold_down");
            _textToggle = RootContainer.Query<Toggle>("text_toggle");
            _eventText = RootContainer.Query<ImTextField>("event_text");
            _search = RootContainer.Query<Button>("search");
            _searchResultAria = RootContainer.Query<VisualElement>("search_result_area");

            //トグル制御
            _switchToggle.value = true;
            ToggleControl();

            //スイッチ検索
            SwitchFoldDown();

            //変数検索
            VariableFoldDown();

            //イベント名検索
            _eventText.RegisterCallback<FocusOutEvent>(o => { _eventNameWord = _eventText.value; });

            //検索ボタン
            _search.clicked += OnClickSearch;
        }

        /// <summary>
        /// スイッチのプルダウン
        /// </summary>
        private void SwitchFoldDown() {
            var switchDropdownChoices = SwitchList();
            var commonEventSwitchPopupField =
                new PopupFieldBase<string>(switchDropdownChoices, 0);
            _switchFoldDown.Add(commonEventSwitchPopupField);
            commonEventSwitchPopupField.RegisterValueChangedCallback(evt =>
            {
                if (commonEventSwitchPopupField.index - 1 >= 0)
                    _switchId = _flagDataModel.switches[commonEventSwitchPopupField.index - 1].id;
                else
                    _switchId = null;
            });

            //スイッチのリスト取得
            List<string> SwitchList() {
                List<string> returnList = new List<string>();
                returnList.Add(EditorLocalize.LocalizeText("WORD_0113"));
                foreach (var flagsValue in _flagDataModel.switches)
                {
                    //名前が空白だった場合は「名称未設定」
                    if (flagsValue.name == "")
                        returnList.Add(EditorLocalize.LocalizeText("WORD_1518"));
                    else
                        returnList.Add(flagsValue.name);
                }
                return returnList;
            }
        }

        /// <summary>
        /// 変数のプルダウン
        /// </summary>
        private void VariableFoldDown() {
            var variableDropdownChoices = VariableList();
            var commonEventVariablePopupField =
                new PopupFieldBase<string>(variableDropdownChoices, 0);
            _variableFoldDown.Add(commonEventVariablePopupField);
            commonEventVariablePopupField.RegisterValueChangedCallback(evt =>
            {
                if (commonEventVariablePopupField.index - 1 >= 0)
                    _variableId = _flagDataModel
                        .variables[commonEventVariablePopupField.index - 1].id;
                else
                    _variableId = null;
            });

            //変数のリスト取得
            List<string> VariableList() {
                List<string> returnList = new List<string>();
                returnList.Add(EditorLocalize.LocalizeText("WORD_0113"));
                foreach (var flagsValue in _flagDataModel.variables)
                {
                    //名前が空白だった場合は「名称未設定」
                    if (flagsValue.name == "")
                        returnList.Add(EditorLocalize.LocalizeText("WORD_1518"));
                    else
                        returnList.Add(flagsValue.name);
                }
                return returnList;
            }
        }

        /// <summary>
        /// トグルのコントロール
        /// </summary>
        private void ToggleControl() {
            _switchToggle.RegisterValueChangedCallback(evt =>
            {
                if (!_variableToggle.value && !_textToggle.value)
                {
                    _switchToggle.value = true;
                }
                else if (_switchToggle.value)
                {
                    _toggleNumber = 0;
                    _variableToggle.value = false;
                    _textToggle.value = false;
                }
            });
            _variableToggle.RegisterValueChangedCallback(evt =>
            {
                if (!_switchToggle.value && !_textToggle.value)
                {
                    _variableToggle.value = true;
                }
                else if (_variableToggle.value)
                {
                    _toggleNumber = 1;
                    _switchToggle.value = false;
                    _textToggle.value = false;
                }
            });
            _textToggle.RegisterValueChangedCallback(evt =>
            {
                if (!_switchToggle.value && !_variableToggle.value)
                {
                    _textToggle.value = true;
                }
                else if (_textToggle.value)
                {
                    _toggleNumber = 2;
                    _switchToggle.value = false;
                    _variableToggle.value = false;
                }
            });
        }

        /// <summary>
        /// 検索結果表示
        /// </summary>
        private void OnClickSearch() {
            switch (_toggleNumber)
            {
                //スイッチ参照
                case 0:
                    ShowResult(SearchSwitchOrVariable(_switchId));
                    break;
                //変数参照
                case 1:
                    ShowResult(SearchSwitchOrVariable(_variableId));
                    break;
                //イベント名参照
                case 2:
                    ShowResult(SearchText(_eventNameWord));
                    break;
            }
        }

        private void ShowResult(List<Result> results) {
            //セル幅
            var cellWidth = 70;

            _searchResultAria.Clear();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(resultUxml);
            VisualElement searchResultVe;
            VisualElement resultVe;

            //見出しの作成
            searchResultVe = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(searchResultVe);
            resultVe = searchResultVe.Query<VisualElement>("result");
            foreach (var headerValue in _header)
            {
                var label = new Label(headerValue);
                label.AddToClassList("border");
                label.style.width = cellWidth;
                resultVe.Add(label);
            }

            _searchResultAria.Add(searchResultVe);

            foreach (var result in results)
            {
                //検査結果表示要素の取得
                searchResultVe = visualTree.CloneTree();
                EditorLocalize.LocalizeElements(searchResultVe);
                resultVe = searchResultVe.Query<VisualElement>("result");

                //テキストの追加
                foreach (var text in result.ResultTexts)
                {
                    var label = new Label(text);
                    label.AddToClassList("border");
                    label.style.width = cellWidth;
                    label.style.whiteSpace = WhiteSpace.Normal;
                    resultVe.Add(label);
                }

                // "移動"ボタンの追加
                var moveButton = new Button {text = EditorLocalize.LocalizeText("WORD_1574")};
                moveButton.clicked += () =>
                {
#if USE_HIERARCHY_SELECTION_IN_MOVE_BUTTON
                    // 該当のヒエラルキーの要素を選択。
                    Editor.Hierarchy.Hierarchy.SelectButton(
                        CommonMapHierarchyView.GetEventPageButtonName(
                            result.EventMapDataModel.eventId, result.EventMapPageNumber));
#else
                    // 該当のイベントの編集モードに。
                    MapEditor.MapEditor.LaunchEventEditMode(
                        result.MapDataModel, result.EventMapDataModel, result.EventMapPageNumber);
#endif
                };
                resultVe.Add(moveButton);

                //検索結果を検索結果表示位置へAdd
                _searchResultAria.Add(searchResultVe);
            }
        }

        private List<Result> SearchSwitchOrVariable(string id) {
            var results = new List<Result>();

            //検索開始
            foreach (var eventMapDataModel in _eventMapDataModels)
            {
                foreach (var eventMapPage in eventMapDataModel.pages)
                {
                    if (string.IsNullOrEmpty(eventMapPage.chapterId) ||
                        string.IsNullOrEmpty(eventMapPage.sectionId))
                        continue;

                    //IDとの一致の確認
                    if (eventMapPage.condition.switchOne.enabled != 0 && eventMapPage.condition.switchOne.switchId == id ||
                        eventMapPage.condition.switchTwo.enabled != 0 && eventMapPage.condition.switchTwo.switchId == id ||
                        eventMapPage.condition.variables.enabled != 0 &&
                        eventMapPage.condition.variables.variableId == id ||
                        IsMatchParameter(_eventDataModels, eventMapDataModel.eventId, id))
                    {
                        DebugUtil.Log(
                            $"ヒット event name={eventMapDataModel.name}, event id={eventMapDataModel.eventId}, id={id}");
                        results.Add(new Result(this, eventMapDataModel, eventMapPage));
                    }

                    static bool IsMatchParameter(List<EventDataModel> eventDataModels, string eventId, string id) {
                        return eventDataModels.Where(eventDataModel => eventDataModel.id == eventId).Any(eventDataModel =>
                            eventDataModel.eventCommands.Any(eventCommand =>
                                eventCommand.parameters.Any(parameter => parameter == id)));
                    }
                }
            }

            return results;
        }

        private List<Result> SearchText(string text) {
            var results = new List<Result>();

            if (text == null || text == "")
                return results;

            //検索開始
            foreach (var eventMapDataModel in _eventMapDataModels)
            {
                if (eventMapDataModel.name == null)
                    continue;
                if (eventMapDataModel.name.IndexOf(text) >= 0)
                    foreach (var eventMapPage in eventMapDataModel.pages)
                    {
                        if (string.IsNullOrEmpty(eventMapPage.chapterId) ||
                            string.IsNullOrEmpty(eventMapPage.sectionId))
                            continue;

                        DebugUtil.Log(
                            $"ヒット event name={eventMapDataModel.name}, event id={eventMapDataModel.eventId}, text={text}");
                        results.Add(new Result(this, eventMapDataModel, eventMapPage));
                    }
            }

            return results;
        }

        private class Result
        {
            private readonly ChapterDataModel               _chapterDataModel;
            private readonly EventMapDataModel.EventMapPage _eventMapPage;
            private readonly SectionDataModel               _sectionDataModel;

            public Result(
                SearchEventInspector searchEventInspector,
                EventMapDataModel eventMapDataModel,
                EventMapDataModel.EventMapPage eventMapPage
            ) {
                EventMapDataModel = eventMapDataModel;
                _eventMapPage = eventMapPage;
                _chapterDataModel = searchEventInspector._chapterDataModels.Single(chapterDataModel =>
                    chapterDataModel.ID == eventMapPage.chapterId);
                _sectionDataModel = searchEventInspector._sectionDataModels.Single(sectionDataModel =>
                    sectionDataModel.ID == eventMapPage.sectionId);
                MapDataModel = searchEventInspector.mapManagementService.LoadMapById(eventMapDataModel.mapId);
            }

            public string[] ResultTexts
            {
                get
                {
                    return
                        new[]
                        {
                            _chapterDataModel?.Name,
                            _sectionDataModel?.Name,
                            MapDataModel.name,
                            EventMapDataModel.name,
                            $"({EventMapDataModel.x},{EventMapDataModel.y})"
                        };
                }
            }

            public MapDataModel MapDataModel { get; }

            public EventMapDataModel EventMapDataModel { get; }

            public int EventMapPageNumber => _eventMapPage != null ? _eventMapPage.page : 0;
        }
    }
}