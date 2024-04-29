using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using Region = RPGMaker.Codebase.Editor.Hierarchy.Enum.Region;

namespace RPGMaker.Codebase.Editor.Inspector.Variable.View
{
    /// <summary>
    /// [変数] Inspector
    /// </summary>
    public class VariableEditInspectorElement : AbstractInspectorElement
    {
        //チャプター一覧の取得
        private readonly List<ChapterDataModel> _chapterDataModel;

        //イベント用
        private readonly List<EventMapDataModel> _eventMapDataModel;
        private List<EventDataModel> _eventDataModels;

        // 状態
        //--------------------------------------------------------------------------------------------------------------

        // 項目名
        //--------------------------------------------------------------------------------------------------------------
        private readonly List<string> _header =
            EditorLocalize.LocalizeTexts(new List<string>
                {"WORD_0004", "WORD_0022", "WORD_0027", "WORD_0014", "WORD_0983"});

        // UI要素
        //--------------------------------------------------------------------------------------------------------------
        private          Label                    _idLabel;
        private          ImTextField              _nameText;
        private          Button                   _searchButton;
        private          VisualElement            _searchResultAria;

        //セクション一覧の取得
        private readonly List<SectionDataModel> _sectionDataModel;

        // 利用するデータ
        //--------------------------------------------------------------------------------------------------------------
        //変数
        private FlagDataModel.Variable _variable;

        // const
        //--------------------------------------------------------------------------------------------------------------
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Variable/Asset/inspector_variableEdit.uxml"; } }

        //検索結果表示用
        private readonly string resultUxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/AssetManage/Asset/inspector_search_result.uxml";

        //--------------------------------------------------------------------------------------------------------------
        //
        // methods
        //
        //--------------------------------------------------------------------------------------------------------------

        // 初期化・更新系
        //--------------------------------------------------------------------------------------------------------------
        public VariableEditInspectorElement(FlagDataModel.Variable variable) {
            _variable = variable;

            //各イベントのload
            _eventMapDataModel = eventManagementService.LoadEventMap();
            _eventDataModels = eventManagementService.LoadEvent();

            //チャプター、セクションのload
            var outlineDataModel = outlineManagementService.LoadOutline();
            _chapterDataModel = outlineDataModel.Chapters;
            _sectionDataModel = outlineDataModel.Sections;

            Initialize();
            InitEventHandlers();
            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            _variable = databaseManagementService.LoadFlags().variables
                .Find(item => item.id == _variable.id);

            if (_variable == null)
            {
                Clear();
                return;
            }

            _idLabel.text = _variable.SerialNumberString;
            _nameText.value = _variable.name;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            _idLabel = RootContainer.Query<Label>("attribute_ID");
            _nameText = RootContainer.Query<ImTextField>("attribute_name");
            _searchButton = RootContainer.Query<Button>("search");
            _searchResultAria = RootContainer.Query<VisualElement>("search_result_area");
        }

        private void InitEventHandlers() {
            _nameText.RegisterCallback<FocusOutEvent>(o =>
            {
                _variable.name = _nameText.value;
                databaseManagementService.SaveVariable(_variable);
                _ = Editor.Hierarchy.Hierarchy.Refresh(Region.FlagsEdit);
                Refresh();
            });
            _searchButton.clicked += OnClickSearch;
        }

        //検索結果表示用
        private void OnClickSearch() {
            var resultList = SearchResult();
            //セルサイズ
            var cellSize = 70;

            _searchResultAria.Clear();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(resultUxml);
            VisualElement searchResult;
            VisualElement result;

            //見出しの作成
            searchResult = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(searchResult);
            result = searchResult.Query<VisualElement>("result");
            foreach (var headerValue in _header)
            {
                var value = new Label(headerValue);
                value.AddToClassList("border");
                value.style.width = cellSize;
                result.Add(value);
            }

            _searchResultAria.Add(searchResult);

            for (var i = 0; i < resultList.Count; i++)
            {
                int work = i;
                //検査結果表示要素の取得
                searchResult = visualTree.CloneTree();
                EditorLocalize.LocalizeElements(searchResult);
                result = searchResult.Query<VisualElement>("result");

                //テキストの追加
                for (var j = 0; j < resultList[i].Count - 1; j++)
                {
                    var value = new Label();
                    //座標の時は二つをひとまとめ
                    if (j == 4)
                        continue;
                    if (j == 5)
                        value = new Label("(" + resultList[i][j - 1] + "," + resultList[i][j] + ")");
                    else
                        value = new Label(resultList[i][j]);

                    value.AddToClassList("border");
                    value.style.width = cellSize;
                    value.style.whiteSpace = WhiteSpace.Normal;
                    result.Add(value);
                }

                //ボタンの追加
                var searchButton = new Button();
                searchButton.text = EditorLocalize.LocalizeText("WORD_1574");
                searchButton.clicked += () => { OnClickMoveEventPoint(int.Parse(resultList[work][resultList[work].Count - 1])); };
                result.Add(searchButton);
                //検索結果を検索結果表示位置へAdd
                _searchResultAria.Add(searchResult);
            }
        }

        //検索結果が[チャプター,セクション,マップ,イベント,座標X,座標Y]で返ってきます
        private List<List<string>> SearchResult() {
            //返す用List
            var returnList = new List<List<string>>();

            //検索開始
            for (int i = 0; i < _eventMapDataModel.Count; i++)
            {
                foreach (var eventMapPage in _eventMapDataModel[i].pages)
                {
                    //イベント自体にチャプター、セクションが未設定の場合は処理しない
                    if (string.IsNullOrEmpty(eventMapPage.chapterId) ||
                        string.IsNullOrEmpty(eventMapPage.sectionId))
                        continue;

                    //IDとの一致の確認
                    if (eventMapPage.condition.variables.enabled != 0 &&
                        eventMapPage.condition.variables.variableId == _variable.id ||
                        IsMatchParameter(_eventDataModels, _eventMapDataModel[i].eventId, _variable.id))
                    {
                        bool flg = false;
                        for (var k = 0; k < _chapterDataModel.Count; k++)
                        {
                            for (var l = 0; l < _sectionDataModel.Count; l++)
                            {
                                if (_chapterDataModel[k].ID != _sectionDataModel[l].ChapterID) continue;

                                if (_sectionDataModel[l].ChapterID == eventMapPage.chapterId &&
                                    _sectionDataModel[l].ID == eventMapPage.sectionId)
                                {
                                    //検索結果が入ってくる
                                    var dataList = new List<string>();
                                    //検索結果のList化
                                    foreach (var statu in CommonStatus(k, l, i)) dataList.Add(statu);
                                    //検索結果の確定
                                    returnList.Add(dataList);
                                    flg = true;
                                    break;
                                }
                            }
                            if (flg) break;
                        }
                    }

                    static bool IsMatchParameter(List<EventDataModel> eventDataModels, string eventId, string id) {
                        return eventDataModels.Where(eventDataModel => eventDataModel.id == eventId).Any(eventDataModel =>
                            eventDataModel.eventCommands.Any(eventCommand =>
                                eventCommand.parameters.Any(parameter => parameter == id)));
                    }
                }
            }

            return returnList;
        }

        //共通で返される検索結果
        private List<string> CommonStatus(int num1, int num2, int num3) {
            var returnStatus = new List<string>();
            //チャプター名
            returnStatus.Add(_chapterDataModel[num1].Name);

            //セクション名(チャプター参照の場合空白が入る)
            if (num2 < 0)
                returnStatus.Add("");
            else
                returnStatus.Add(_sectionDataModel[num2].Name);

            //マップ名
            returnStatus.Add(mapManagementService.LoadMapById(_eventMapDataModel[num3].mapId).name);

            //イベント名
            returnStatus.Add(_eventMapDataModel[num3].name);

            //座標
            returnStatus.Add(_eventMapDataModel[num3].x.ToString());
            returnStatus.Add(_eventMapDataModel[num3].y.ToString());

            //EventMapDataModelの添え字
            returnStatus.Add(num3.ToString());

            return returnStatus;
        }

        //イベント使用箇所への移動
        private void OnClickMoveEventPoint(int num) {
            var mapDatamodel = mapManagementService.LoadMapById(_eventMapDataModel[num].mapId);
            MapEditor.MapEditor.LaunchEventEditMode(mapDatamodel, _eventMapDataModel[num]);
        }
    }
}