// Foldout用のラベルを追加し、Foldoutの開閉とFoldout名クリックの操作を分離。

#define USE_FOLDOUT_LABEL

using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Outline.View.Component
{
    public class SectionFoldout : AbstractHierarchyView
    {
        protected override string MainUxml { get { return ""; } }

        // 状態
        //--------------------------------------------------------------------------------------------------------------

        // UI要素
        //--------------------------------------------------------------------------------------------------------------
#if USE_FOLDOUT_LABEL
        private Label _foldoutLabel;
#endif
        private OutlineMapHierarchyInfo _mapHierarchyInfo;
        // const
        //--------------------------------------------------------------------------------------------------------------

        // 利用するデータ
        //--------------------------------------------------------------------------------------------------------------
        private SectionDataModel _sectionDataModel;
        private List<MapDataModel> _allMapDataModels;
        private List<EventMapDataModel> _allEventMapDataModels;

        //--------------------------------------------------------------------------------------------------------------
        //
        // methods
        //
        //--------------------------------------------------------------------------------------------------------------

        // 初期化・更新系
        //--------------------------------------------------------------------------------------------------------------
        public SectionFoldout(
            SectionDataModel sectionDataModel,
            List<MapDataModel> allMapDataModels,
            List<EventMapDataModel> allEventMapDataModels
        ) {
            _sectionDataModel = sectionDataModel;
            _allMapDataModels = allMapDataModels;
            _allEventMapDataModels = allEventMapDataModels;

            InitUI();
        }

        /// <summary>
        /// 各コンテンツデータの初期化
        /// </summary>
        override protected void InitContentsData() {
#if USE_FOLDOUT_LABEL
            var foldout = new VisualElement();
#endif
            Foldout foldoutWork = new Foldout {name = "section-foldout" + _sectionDataModel.ID, value = false};
#if USE_FOLDOUT_LABEL
            _foldoutLabel = new Label();
            _foldoutLabel.name = foldoutWork.name + "_label";
            _foldoutLabel.style.position = Position.Absolute;
            _foldoutLabel.style.left = 50f;
            _foldoutLabel.style.right = 0;
            _foldoutLabel.style.paddingTop = 2f;
            foldout.Add(foldoutWork);
            foldout.Add(_foldoutLabel);
            Add(foldout);
#else
            Add(foldoutWork);
#endif
            SetFoldout("section-foldout" + _sectionDataModel.ID, foldoutWork);

            // セクションのFoldoutのコンテキストメニュー。
#if USE_FOLDOUT_LABEL
            BaseClickHandler.ClickEvent(_foldoutLabel, evt =>
#else
            BaseClickHandler.ClickEvent(_foldout, (evt) =>
#endif
            {
                if (evt != (int) MouseButton.RightMouse) return;

                var oeGraphView = OutlineEditor.OutlineEditor.GetOeGraphView();

                var menu = new GenericMenu();

                menu.AddItem(
                    new GUIContent(EditorLocalize.LocalizeText("WORD_0023")),
                    false,
                    () =>
                    {
                        // コピーする前にFoldoutを選択する。
                        Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(GetFoldout("section-foldout" + _sectionDataModel.ID));

                        // GraphViewのCopyPasteDataへコピー。
                        oeGraphView.CallCopySelectionCallback();
                    });

                menu.AddItem(
                    new GUIContent(EditorLocalize.LocalizeText("WORD_0024")),
                    false,
                    () => { 
                        OutlineEditor.OutlineEditor.RemoveGraphElementModel(_sectionDataModel.ID);
                        OutlineEditor.OutlineEditor.SaveOutline();
                    });

                menu.AddItem(
                    new GUIContent(EditorLocalize.LocalizeText("WORD_0025")),
                    false,
                    () =>
                    {
                        OutlineEditor.OutlineEditor.AddNewMapToSectionNode(
                            OutlineEditor.OutlineEditor.NodeModelsByUuid[_sectionDataModel.ID] as SectionNodeModel);
                    });

                menu.AddItem(
                    new GUIContent(EditorLocalize.LocalizeText("WORD_0026")),
                    false,
                    () =>
                    {
                        DebugUtil.LogWarning("マップの貼り付けは現在未実装です。");
                    });

                menu.ShowAsContext();
            });

            _mapHierarchyInfo = new OutlineMapHierarchyInfo(foldoutWork, null, this);

            InitEventHandlers();
            Refresh(_sectionDataModel, _allMapDataModels, _allEventMapDataModels);
        }

        private void InitEventHandlers() {
            Editor.Hierarchy.Hierarchy.AddSelectableElementAndAction(GetFoldout("section-foldout" + _sectionDataModel.ID), () =>
            {
                var node = OutlineEditor.OutlineEditor.NodeModelsByUuid[_sectionDataModel.ID];
                if (node == null) return;
                OutlineEditor.OutlineEditor.SelectNode(node);
            });

#if USE_FOLDOUT_LABEL
            _foldoutLabel.RegisterCallback<ClickEvent>(evt =>
#else
            _foldout.Q<Toggle>().Q<VisualElement>().RegisterCallback<ClickEvent>((evt) =>
#endif
            {
                Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(GetFoldout("section-foldout" + _sectionDataModel.ID));
            });
        }

        public void Refresh(
            [CanBeNull] SectionDataModel sectionDataModel = null,
            [CanBeNull] List<MapDataModel> allMapDataModels = null,
            [CanBeNull] List<EventMapDataModel> allEventMapDataModels = null
        ) {
            _sectionDataModel = sectionDataModel ?? _sectionDataModel;

            // foldout名
#if USE_FOLDOUT_LABEL
            _foldoutLabel.text = sectionDataModel?.Name;
#else
            _foldout.text = sectionDataModel?.Name;
#endif

            // マップ一覧
            _mapHierarchyInfo.RefreshMapHierarchy(_sectionDataModel.Maps.Select(map => map.ID).ToArray());
        }

        public void UpdateName([CanBeNull] SectionDataModel sectionDataModel = null) {
            _sectionDataModel = sectionDataModel ?? _sectionDataModel;
            _foldoutLabel.text = _sectionDataModel?.Name;
        }

        public void AddMap(string id) {
            _mapHierarchyInfo.AddMapHierarchy(id);
        }

        public void RemoveMap(string id = null, string sectionId = null) {
            _mapHierarchyInfo.RemoveMapHierarchy(id, sectionId);
        }

        public void UpdateMapName(string id) {
            _mapHierarchyInfo.UpdateMapNameHierarchy(id);
        }

        public void UpdateEvent(string id) {
            _mapHierarchyInfo.UpdateEventHierarchy(id);
        }

        public void UpdateFoldout() {
            Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(GetFoldout("section-foldout" + _sectionDataModel.ID));
        } 
    }
}