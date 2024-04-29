// Foldout用のラベルを追加し、Foldoutの開閉とFoldout名クリックの操作を分離。

#define USE_FOLDOUT_LABEL

using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Outline.View;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Outline.View.Component
{
    public class ChapterFoldout : AbstractHierarchyView
    {
        protected override string MainUxml { get { return ""; } }

        // const
        //--------------------------------------------------------------------------------------------------------------

        // 利用するデータ
        //--------------------------------------------------------------------------------------------------------------
        private ChapterDataModel _chapterDataModel;
        public ChapterDataModel GetChapterDataModel() { return _chapterDataModel; }
        private VisualElement    _fieldMapRootVe;

        // 状態
        //--------------------------------------------------------------------------------------------------------------

        // UI要素
        //--------------------------------------------------------------------------------------------------------------
#if USE_FOLDOUT_LABEL
        private Label   _foldoutLabel;
        private Foldout _foldout;
#endif
        private List<MapDataModel>      _mapDataModels;
        private OutlineMapHierarchyInfo _mapHierarchyInfo;
        private List<SectionDataModel>  _sectionDataModels;
        private List<EventMapDataModel> _eventMapDataModels;

        //--------------------------------------------------------------------------------------------------------------
        //
        // methods
        //
        //--------------------------------------------------------------------------------------------------------------

        // 初期化・更新系
        //--------------------------------------------------------------------------------------------------------------
        public ChapterFoldout(
            ChapterDataModel chapterDataModel,
            List<SectionDataModel> sectionDataModels,
            List<MapDataModel> mapDataModels,
            List<EventMapDataModel> eventMapDataModels
        ) {
            _chapterDataModel = chapterDataModel;
            _sectionDataModels = sectionDataModels;
            _mapDataModels = mapDataModels;
            _eventMapDataModels = eventMapDataModels;

            InitUI();
        }

        public Dictionary<string, SectionFoldout> SectionFoldoutsByDataModelId { get; private set; }

        public List<string> SectionFoldoutKeys { get; private set; }


        /// <summary>
        /// 各コンテンツデータの初期化
        /// </summary>
        override protected void InitContentsData() {
#if USE_FOLDOUT_LABEL
            //Foldoutは開閉だけするように対処
            var foldout = new VisualElement();
#endif
            _foldout = new Foldout {name = "chapter-foldout" + _chapterDataModel.ID, value = false};
#if USE_FOLDOUT_LABEL
            _foldoutLabel = new Label();
            _foldoutLabel.name = _foldout.name + "_label";
            _foldoutLabel.style.position = Position.Absolute;
            _foldoutLabel.style.left = 20f;
            _foldoutLabel.style.right = 0;
            _foldoutLabel.style.paddingTop = 2f;
            foldout.Add(_foldout);
            foldout.Add(_foldoutLabel);
            Add(foldout);
#else
            Add(_foldout);
#endif

            _fieldMapRootVe = new VisualElement();
            _foldout.Add(_fieldMapRootVe);
            SetFoldout("chapter-foldout" + _chapterDataModel.ID, _foldout);

            // チャプターのFoldoutのコンテキストメニュー。
#if USE_FOLDOUT_LABEL
            BaseClickHandler.ClickEvent(_foldoutLabel, evt =>
#else
            BaseClickHandler.ClickEvent(_foldout, (evt) =>
#endif
            {
                if (evt != (int) MouseButton.RightMouse) return;

                var oeGraphView = OutlineEditor.OutlineEditor.GetOeGraphView();
                var currentChapterId = Inspector.Inspector.GetCurrentChapterId(oeGraphView);
                var copiedSectionNodeModel = CopyPasteDataUtil.GetLastCopiedNode<SectionNodeModel>();

                var menu = new GenericMenu();

                // チャプターのコピー。
                menu.AddItem(
                    new GUIContent(EditorLocalize.LocalizeText("WORD_0005")),
                    false,
                    () =>
                    {
                        // コピーする前にFoldoutを選択する。
                        Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(_foldout);

                        // GraphViewのCopyPasteDataへコピー。
                        oeGraphView.CallCopySelectionCallback();
                    });

                // チャプターの削除。
                menu.AddItem(
                    new GUIContent(EditorLocalize.LocalizeText("WORD_0006")),
                    false,
                    () => { 
                        OutlineEditor.OutlineEditor.RemoveGraphElementModel(_chapterDataModel.ID);
                        OutlineEditor.OutlineEditor.SaveOutline();
                    });

                // セクションの新規作成。
                menu.AddItem(
                    new GUIContent(EditorLocalize.LocalizeText("WORD_0007")),
                    false,
                    () => { OutlineEditor.OutlineEditor.AddNewSectionNode(_chapterDataModel.ID); });

                // セクションの貼り付け。
                menu.AddItem(
                    copiedSectionNodeModel == null,
                    new GUIContent(EditorLocalize.LocalizeText("WORD_0008")),
                    false,
                    () =>
                    {
                        // コピーしたSectionが属するChapterを一時的にFoldoutのChapterに変更。
                        var originalChapterId = copiedSectionNodeModel.ReplaceChapterId(_chapterDataModel.ID);

                        // GraphViewのCopyPasteDataから貼り付け。
                        oeGraphView.CallPasteCallback();

                        // コピーしたSectionが属するChapterを元に戻す。
                        copiedSectionNodeModel.ReplaceChapterId(originalChapterId);
                    });

                menu.ShowAsContext();
            });

            SectionFoldoutsByDataModelId = new Dictionary<string, SectionFoldout>();
            SectionFoldoutKeys = new List<string>();

            // "フィールドマップ"FoldoutのHierarchyの情報。
            _mapHierarchyInfo = new OutlineMapHierarchyInfo(
                _fieldMapRootVe,
                EditorLocalize.LocalizeText("WORD_0009"), 
                this);

            InitEventHandlers();
            Refresh(_chapterDataModel, _sectionDataModels, _mapDataModels, _eventMapDataModels);
        }

        private void InitEventHandlers() {
            Editor.Hierarchy.Hierarchy.AddSelectableElementAndAction(_foldout, () =>
            {
                var node = OutlineEditor.OutlineEditor.NodeModelsByUuid[_chapterDataModel.ID];
                if (node == null) return;
                OutlineEditor.OutlineEditor.SelectNode(node);
            });

#if USE_FOLDOUT_LABEL
            _foldoutLabel.RegisterCallback<ClickEvent>(evt =>
#else
            _foldout.Q<Toggle>().Q<VisualElement>().RegisterCallback<ClickEvent>((evt) =>
#endif
            {
                Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(_foldout);
            });
        }

        public void Refresh(
            [CanBeNull] ChapterDataModel chapterDataModel = null,
            [CanBeNull] List<SectionDataModel> sectionDataModels = null,
            [CanBeNull] List<MapDataModel> allMapDataModels = null,
            [CanBeNull] List<EventMapDataModel> allEventMapDataModels = null,
            string type = "",
            string id = null,
            string id2 = null
        ) {
            _chapterDataModel = chapterDataModel ?? _chapterDataModel;
            _sectionDataModels = sectionDataModels ?? _sectionDataModels;
            _mapDataModels = allMapDataModels ?? _mapDataModels;

            // foldout名
#if USE_FOLDOUT_LABEL
            _foldoutLabel.text = chapterDataModel?.Name;
#else
            _foldout.text = chapterDataModel?.Name;
#endif
            switch (type)
            {
                case RefreshTypeMapDelete:
                    _mapHierarchyInfo.RemoveMapHierarchy(id);
                    foreach (var sectionDataModel in _sectionDataModels)
                        SectionFoldoutsByDataModelId[sectionDataModel.ID].RemoveMap(id, sectionDataModel.ID);
                    break;
                case RefreshTypeMapName:
                    foreach (var sectionDataModel in _sectionDataModels)
                    {
                        SectionFoldoutsByDataModelId[sectionDataModel.ID].UpdateMapName(id);
                        for (int j = 0; j < sectionDataModel.Maps.Count; j++)
                        {
                            if (sectionDataModel.Maps[j].ID == id)
                            {
                                string mapName = _mapDataModels.FirstOrDefault(map => map.id == id).name;
                                OutlineEditor.OutlineEditor.UpdateSectionMapName(sectionDataModel.ID, id, mapName);
                            }
                        }
                    }
                    break;
                case RefreshTypeSectionName:
                    SectionFoldoutsByDataModelId[id].UpdateName(_sectionDataModels.Find(i => i.ID == id));
                    break;
                case RefreshTypeSectionMapAdd:
                    SectionFoldoutsByDataModelId[id].AddMap(id2);
                    break;
                case RefreshTypeSectionMapRemove:
                    SectionFoldoutsByDataModelId[id].RemoveMap(id2);
                    break;
                case RefreshTypeEventCreate:
                case RefreshTypeEvenDuplicate:
                case RefreshTypeEventDelete:
                case RefreshTypeEventEvCreate:
                case RefreshTypeEvenEvDuplicate:
                case RefreshTypeEventEvDelete:
                case RefreshTypeEventName:
                    _mapHierarchyInfo.UpdateEventHierarchy(id);
                    foreach (var sectionDataModel in _sectionDataModels)
                        SectionFoldoutsByDataModelId[sectionDataModel.ID].UpdateEvent(id);
                    break;
                case RefreshTypeChapterCreate:
                    break;
                case RefreshTypeChapterDelete:
                    Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(_foldout);
                    break;
                case RefreshTypeChapterEdit:
                    break;
                case RefreshTypeChapterName:
                    _foldoutLabel.name = chapterDataModel.Name;
                    break;
                case RefreshTypeSectionCreate:
                case RefreshTypeSectionDuplicate:
                    if (!SectionFoldoutsByDataModelId.ContainsKey(id))
                    {
                        for (int i = 0; i < sectionDataModels.Count; i++)
                        {
                            if (sectionDataModels[i].ID == id)
                            {
                                var sectionFoldout = new SectionFoldout(sectionDataModels[i], _mapDataModels, allEventMapDataModels);
                                SectionFoldoutsByDataModelId.Add(sectionDataModels[i].ID, sectionFoldout);
                                SectionFoldoutKeys.Add(sectionDataModels[i].ID);
                                _foldout.Add(sectionFoldout);

                                SectionFoldoutsByDataModelId[sectionDataModels[i].ID]
                                    .Refresh(sectionDataModels[i], _mapDataModels, allEventMapDataModels);
                                break;
                            }
                        }
                    }
                    break;
                case RefreshTypeSectionDelete:

                    int index = 0;
                    for (int i = 0; i < SectionFoldoutKeys.Count; i++)
                    {
                        if (SectionFoldoutKeys[i] == id)
                        {
                            index = i;
                            break;
                        }
                    }


                    _foldout.Remove(SectionFoldoutsByDataModelId[id]);
                    SectionFoldoutsByDataModelId.Remove(id);
                    SectionFoldoutKeys.Remove(id);

                    if(SectionFoldoutKeys.Count == 0)
                    {
                        Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(_foldout);
                        break;
                    }

                    if (SectionFoldoutKeys.Count - 1 < index)
                    {
                        index = SectionFoldoutKeys.Count - 1;
                    }



                    SectionFoldoutsByDataModelId[SectionFoldoutKeys[index]].UpdateFoldout();

                    break;
                case RefreshTypeSectionUpdate:
                default:
                    _mapHierarchyInfo.RefreshMapHierarchy(
                        _chapterDataModel.FieldMapSubDataModel != null
                            ? new[] { _chapterDataModel.FieldMapSubDataModel.ID }
                            : null);

                    // セクション一覧を更新・生成
                    foreach (var sectionDataModel in _sectionDataModels)
                    {
                        if (!SectionFoldoutsByDataModelId.ContainsKey(sectionDataModel.ID))
                        {
                            // Foldoutがまだ存在しない場合
                            var sectionFoldout = new SectionFoldout(sectionDataModel, _mapDataModels, allEventMapDataModels);
                            SectionFoldoutsByDataModelId.Add(sectionDataModel.ID, sectionFoldout);
                            SectionFoldoutKeys.Add(sectionDataModel.ID);
                            _foldout.Add(sectionFoldout);
                        }

                        SectionFoldoutsByDataModelId[sectionDataModel.ID]
                            .Refresh(sectionDataModel, _mapDataModels, allEventMapDataModels);
                    }

                    // 削除されたセクションがあればそのFoldoutを削除
                    var deleteSectionIds = new HashSet<string>();
                    foreach (var sectionId in SectionFoldoutsByDataModelId.Keys)
                    {
                        if (_sectionDataModels.Select(item => item.ID).Contains(sectionId)) continue;

                        _foldout.Remove(SectionFoldoutsByDataModelId[sectionId]);
                        deleteSectionIds.Add(sectionId);
                    }

                    foreach (var sectionId in deleteSectionIds) SectionFoldoutsByDataModelId.Remove(sectionId);
                    break;
            }

        }
    }
}