using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Outline.View.Component;
using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Outline.View
{
    public class OutlineHierarchyView : AbstractHierarchyView
    {
        protected override string MainUxml { get { return ""; } }
        private List<EventMapDataModel> _eventMapDataModels;
        private List<MapDataModel>      _mapDataModels;

        // 利用するデータ
        //--------------------------------------------------------------------------------------------------------------
        private OutlineDataModel _outlineDataModel;
        private string _updateData;

        // const
        //--------------------------------------------------------------------------------------------------------------

        // ヒエラルキー本体クラス
        //--------------------------------------------------------------------------------------------------------------
        private OutlineHierarchy _outlineHierarchy;

        // 状態
        //--------------------------------------------------------------------------------------------------------------

        // UI要素
        //--------------------------------------------------------------------------------------------------------------
        private Button _searchEventButton;

        //--------------------------------------------------------------------------------------------------------------
        //
        // methods
        //
        //--------------------------------------------------------------------------------------------------------------

        // 初期化・更新系
        //--------------------------------------------------------------------------------------------------------------
        public OutlineHierarchyView(OutlineHierarchy outlineHierarchy) {
            _outlineHierarchy = outlineHierarchy;
            InitUI();
        }

        public Dictionary<string, ChapterFoldout> ChapterFoldoutsByDataModelId { get; private set; }
        public List<string> ChapterFoldoutKeys { get; private set; }


        /// <summary>
        /// 各コンテンツデータの初期化
        /// </summary>
        override protected void InitContentsData() {
            AddToClassList("AnalyticsTag__page_view__outline");

            //イベント検索のボタン
            _searchEventButton = new Button {text = EditorLocalize.LocalizeText("WORD_1484")};
            _searchEventButton.AddToClassList("button-transparent");
            _searchEventButton.AddToClassList("AnalyticsTag__page_view__event_search");
            Add(_searchEventButton);

            ChapterFoldoutsByDataModelId = new Dictionary<string, ChapterFoldout>();
            ChapterFoldoutKeys = new List<string>();
            InitEventHandlers();
        }

        /// <summary>
        /// 各コンテンツデータの初期化
        /// </summary>
        private void InitEventHandlers() {
            Editor.Hierarchy.Hierarchy.AddSelectableElementAndAction(_searchEventButton,
                Inspector.Inspector.SearchEventView);
            _searchEventButton.clicked += () =>
            {
                Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(_searchEventButton);
            };
        }

        /// <summary>
        /// データ更新
        /// </summary>
        /// <param name="outlineDataModel"></param>
        /// <param name="mapDataModels"></param>
        /// <param name="eventMapDataModels"></param>
        public void Refresh(
            [CanBeNull] OutlineDataModel outlineDataModel = null,
            [CanBeNull] List<MapDataModel> mapDataModels = null,
            [CanBeNull] List<EventMapDataModel> eventMapDataModels = null,
            string updateData = null
        ) {
            _outlineDataModel = outlineDataModel ?? _outlineDataModel;
            _mapDataModels = mapDataModels ?? _mapDataModels;
            _eventMapDataModels = eventMapDataModels ?? _eventMapDataModels;
            _updateData = updateData;
            base.Refresh();
        }

        /// <summary>
        /// データ更新
        /// </summary>
        protected override void RefreshContents() {
            base.RefreshContents();
            if (_updateData == null)
            {
                // チャプター一覧の更新・生成
                foreach (var chapterDataModel in _outlineDataModel.Chapters)
                {
                    var sectionDataModels =
                        _outlineDataModel.Sections.FindAll(item => item.ChapterID == chapterDataModel.ID);
                    if (!ChapterFoldoutsByDataModelId.ContainsKey(chapterDataModel.ID))
                    {
                        // Foldoutがまだ存在しない場合
                        var chapterFoldout = new ChapterFoldout(
                            chapterDataModel,
                            sectionDataModels,
                            _mapDataModels,
                            _eventMapDataModels
                        );
                        ChapterFoldoutsByDataModelId.Add(chapterDataModel.ID, chapterFoldout);
                        ChapterFoldoutKeys.Add(chapterDataModel.ID);

                        Add(chapterFoldout);
                    }

                    ChapterFoldoutsByDataModelId[chapterDataModel.ID]
                        .Refresh(chapterDataModel, sectionDataModels, _mapDataModels, _eventMapDataModels);
                }

                // 削除されたチャプターがあればそのFoldoutを削除
                var deleteChapterIds = new HashSet<string>();
                foreach (var chapterId in ChapterFoldoutsByDataModelId.Keys)
                {
                    if (_outlineDataModel.Chapters.Select(item => item.ID).Contains(chapterId)) continue;

                    Remove(ChapterFoldoutsByDataModelId[chapterId]);
                    deleteChapterIds.Add(chapterId);
                }

                foreach (var chapterId in deleteChapterIds) ChapterFoldoutsByDataModelId.Remove(chapterId);
            }
            else
            {
                switch (_updateData.Split(',')[0])
                {
                    case RefreshTypeTitle:
                    case RefreshTypeMapCreate:
                    case RefreshTypeMapDuplicate:
                        // Hierarchyの更新はないので処理不要
                        break;
                    case RefreshTypeMapDelete:
                    case RefreshTypeMapName:
                        foreach (var chapterDataModel in _outlineDataModel.Chapters)
                        {
                            var sectionDataModels =
                                _outlineDataModel.Sections.FindAll(item => item.ChapterID == chapterDataModel.ID);
                            ChapterFoldoutsByDataModelId[chapterDataModel.ID]
                                .Refresh(chapterDataModel, sectionDataModels, _mapDataModels, _eventMapDataModels, _updateData.Split(',')[0], _updateData.Split(',')[1]);

                            OutlineEditor.OutlineEditor.ChangeChapterFieldMap(chapterDataModel.ID, chapterDataModel.FieldMapSubDataModel);
                        }

                        break;

                    // イベント系
                    case RefreshTypeEventCreate:
                    case RefreshTypeEvenDuplicate:
                    case RefreshTypeEventDelete:
                    case RefreshTypeEventEvCreate:
                    case RefreshTypeEvenEvDuplicate:
                    case RefreshTypeEventEvDelete:
                    case RefreshTypeEventName:
                        foreach (var chapterDataModel in _outlineDataModel.Chapters)
                        {
                            var sectionDataModels =
                                _outlineDataModel.Sections.FindAll(item => item.ChapterID == chapterDataModel.ID);
                            ChapterFoldoutsByDataModelId[chapterDataModel.ID]
                                .Refresh(chapterDataModel, sectionDataModels, _mapDataModels, _eventMapDataModels, _updateData.Split(',')[0], _updateData.Split(',')[1]);
                        }
                        break;
                    case RefreshTypeChapterCreate:
                    case RefreshTypeChapterDuplicate:
                        // チャプター一覧の更新・生成
                        for (int i = 0; i < _outlineDataModel.Chapters.Count; i++)
                        {
                            if (_outlineDataModel.Chapters[i].ID == _updateData.Split(',')[1])
                            {
                                var sectionDataModels =
                                    _outlineDataModel.Sections.FindAll(item => item.ChapterID == _outlineDataModel.Chapters[i].ID);
                                
                                // Foldoutがまだ存在しない場合
                                var chapterFoldout = new ChapterFoldout(
                                    _outlineDataModel.Chapters[i],
                                    sectionDataModels,
                                    _mapDataModels,
                                    _eventMapDataModels
                                );
                                ChapterFoldoutsByDataModelId.Add(_outlineDataModel.Chapters[i].ID, chapterFoldout);
                                ChapterFoldoutKeys.Add(_outlineDataModel.Chapters[i].ID);

                                Add(chapterFoldout);

                                ChapterFoldoutsByDataModelId[_outlineDataModel.Chapters[i].ID]
                                    .Refresh(_outlineDataModel.Chapters[i], sectionDataModels, _mapDataModels, _eventMapDataModels);
                                break;
                            }
                        }
                        break;
                    case RefreshTypeChapterDelete:
                        int index = 0;
                        for (int i = 0; i < ChapterFoldoutKeys.Count; i++)
                        {
                            if (ChapterFoldoutKeys[i] == _updateData.Split(',')[1])
                            {
                                index = i;
                                break;
                            }
                        }

                        Remove(ChapterFoldoutsByDataModelId[_updateData.Split(',')[1]]);
                        ChapterFoldoutsByDataModelId.Remove(_updateData.Split(',')[1]);
                        ChapterFoldoutKeys.Remove(_updateData.Split(',')[1]);

                        if(ChapterFoldoutKeys.Count == 0)
                        {
                            Inspector.Inspector.Clear();
                            break;
                        }

                        if (ChapterFoldoutKeys.Count- 1 < index)
                        {
                            index = ChapterFoldoutKeys.Count - 1;
                        }

                        var sectionDataModelsWork =
                                    _outlineDataModel.Sections.FindAll(item => item.ChapterID == ChapterFoldoutsByDataModelId[ChapterFoldoutKeys[index]].GetChapterDataModel().ID);

                        ChapterFoldoutsByDataModelId[ChapterFoldoutKeys[index]]
                                .Refresh(ChapterFoldoutsByDataModelId[ChapterFoldoutKeys[index]].GetChapterDataModel(), sectionDataModelsWork, _mapDataModels, _eventMapDataModels, _updateData.Split(',')[0]);

                        break;
                    case RefreshTypeChapterName:
                        for (int i = 0; i < _outlineDataModel.Chapters.Count; i++)
                        {
                            if (_outlineDataModel.Chapters[i].ID == _updateData.Split(',')[1])
                            {
                                var sectionDataModels =
                                    _outlineDataModel.Sections.FindAll(item => item.ChapterID == _outlineDataModel.Chapters[i].ID);

                                ChapterFoldoutsByDataModelId[_outlineDataModel.Chapters[i].ID]
                                    .Refresh(_outlineDataModel.Chapters[i], sectionDataModels, _mapDataModels, _eventMapDataModels, _updateData.Split(',')[0]);
                                break;
                            }
                        }
                        break;
                    case RefreshTypeChapterEdit:
                    case RefreshTypeSectionEdit:
                        // Hierarchyの更新はないので処理不要
                        break;
                    case RefreshTypeSectionName:
                    case RefreshTypeSectionCreate:
                    case RefreshTypeSectionDuplicate:
                        for (int i = 0; i < _outlineDataModel.Chapters.Count; i++)
                        {
                            if (_outlineDataModel.Chapters[i].ID == _updateData.Split(',')[1])
                            {
                                var sectionDataModels =
                                    _outlineDataModel.Sections.FindAll(item => item.ChapterID == _outlineDataModel.Chapters[i].ID);

                                ChapterFoldoutsByDataModelId[_outlineDataModel.Chapters[i].ID]
                                    .Refresh(_outlineDataModel.Chapters[i], sectionDataModels, _mapDataModels, _eventMapDataModels, _updateData.Split(',')[0], _updateData.Split(',')[2]);
                                break;
                            }
                        }
                        break;
                    case RefreshTypeSectionDelete:
                        for (int i = 0; i < _outlineDataModel.Chapters.Count; i++)
                        {
                            if (_outlineDataModel.Chapters[i].ID == _updateData.Split(',')[1])
                            {
                                var sectionDataModels =
                                    _outlineDataModel.Sections.FindAll(item => item.ChapterID == _outlineDataModel.Chapters[i].ID);

                                ChapterFoldoutsByDataModelId[_outlineDataModel.Chapters[i].ID]
                                    .Refresh(_outlineDataModel.Chapters[i], sectionDataModels, _mapDataModels, _eventMapDataModels, _updateData.Split(',')[0], _updateData.Split(',')[2]);
                                break;
                            }
                        }
                        break;
                    case RefreshTypeSectionMapAdd:
                    case RefreshTypeSectionMapRemove:
                        for (int i = 0; i < _outlineDataModel.Chapters.Count; i++)
                        {
                            if (_outlineDataModel.Chapters[i].ID == _updateData.Split(',')[1])
                            {
                                var sectionDataModels =
                                    _outlineDataModel.Sections.FindAll(item => item.ChapterID == _outlineDataModel.Chapters[i].ID);

                                ChapterFoldoutsByDataModelId[_outlineDataModel.Chapters[i].ID
                                    ].Refresh(_outlineDataModel.Chapters[i], sectionDataModels, _mapDataModels, _eventMapDataModels, _updateData.Split(',')[0], _updateData.Split(',')[2], _updateData.Split(',')[3]);
                                break;
                            }
                        }
                        break;
                    case RefreshTypeSectionUpdate:
                        for (int i = 0; i < _outlineDataModel.Chapters.Count; i++)
                        {
                            if (_outlineDataModel.Chapters[i].ID == _updateData.Split(',')[1])
                            {
                                var sectionDataModels =
                                    _outlineDataModel.Sections.FindAll(item => item.ChapterID == _outlineDataModel.Chapters[i].ID);

                                ChapterFoldoutsByDataModelId[_outlineDataModel.Chapters[i].ID]
                                    .Refresh(_outlineDataModel.Chapters[i], sectionDataModels, _mapDataModels, _eventMapDataModels, _updateData.Split(',')[0]);
                            }

                            if (_outlineDataModel.Chapters[i].ID == _updateData.Split(',')[3])
                            {
                                var sectionDataModels =
                                    _outlineDataModel.Sections.FindAll(item => item.ChapterID == _outlineDataModel.Chapters[i].ID);

                                ChapterFoldoutsByDataModelId[_outlineDataModel.Chapters[i].ID]
                                    .Refresh(_outlineDataModel.Chapters[i], sectionDataModels, _mapDataModels, _eventMapDataModels, _updateData.Split(',')[0]);
                            }
                        }
                        break;

                    default:
                        foreach (var chapterDataModel in _outlineDataModel.Chapters)
                        {
                            var sectionDataModels =
                                _outlineDataModel.Sections.FindAll(item => item.ChapterID == chapterDataModel.ID);
                            ChapterFoldoutsByDataModelId[chapterDataModel.ID]
                                .Refresh(chapterDataModel, sectionDataModels, _mapDataModels, _eventMapDataModels, "", _updateData.Split(',')[0]);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// アウトラインFoldoutのコンテキストメニュー。
        /// </summary>
        /// <param name="parebtVe"></param>
        public static void AddContextMenu(VisualElement parebtVe) {
            BaseClickHandler.ClickEvent(parebtVe, evt =>
            {
                if (evt != (int) MouseButton.RightMouse) return;

                var menu = new GenericMenu();

                menu.AddItem(
                    new GUIContent(EditorLocalize.LocalizeText("WORD_0002")),
                    false,
                    () => { OutlineEditor.OutlineEditor.AddNewChapterNode(); });

                menu.AddItem(
                    CopyPasteDataUtil.GetLastCopiedNode<ChapterNodeModel>() == null,
                    new GUIContent(EditorLocalize.LocalizeText("WORD_0003")),
                    false,
                    () =>
                    {
                        // GraphViewのCopyPasteDataから貼り付け。
                        OutlineEditor.OutlineEditor.GetOeGraphView().CallPasteCallback();
                    });

                menu.ShowAsContext();
            });
        }
    }
}