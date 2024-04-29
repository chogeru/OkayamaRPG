using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.OutlineManagement.Repository;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.OutlineEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.Outline.View
{
    /// <summary>
    /// [アウトライン]-[セクション] Inspector
    /// </summary>
    public class InspectorViewForSection : AbstractOutlineInspectorView
    {
        private const string Uxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/Outline/Asset/InspectorViewForSection.uxml";

        public InspectorViewForSection(SectionDataModel sectionDataModel) {
            SectionDataModel = sectionDataModel;
            Show();
        }

        public SectionDataModel SectionDataModel { get; }

        private void Show() {
            Clear();

            var container = new VisualElement();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Uxml);
            var labelFromUxml = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUxml);
            labelFromUxml.style.flexGrow = 1;
            container.Add(labelFromUxml);
            Add(container);

            // テンプレート設定
            var templateSlecter = container.Q<VisualElement>("template");
            var templateSelectItems = new List<string>
            {
                EditorLocalize.LocalizeText("WORD_0027"), EditorLocalize.LocalizeText("イベントセットの呼び出し")
            };
            var templateSlecterPopupField = new PopupField<string>(
                EditorLocalize.LocalizeText("WORD_0046"), templateSelectItems, 0);
            templateSlecter.Add(templateSlecterPopupField);

            // インポート
            var templateImportButton = container.Q<Button>("import_button");
            templateImportButton.clicked += () =>
            {
                var json = AssetManageImporter.StartToJson<SectionJson>();
                var outlineRepo = new OutlineRepository();
                outlineRepo.GetOutline();
                var sectionData = outlineRepo.JsonToSection(json);

                // Map
                if (templateSlecterPopupField.index == 0)
                {
                    for (var i = 0; i < SectionDataModel.Maps.Count; i++)
                        OutlineEditor.OutlineEditor.RemoveSectionMap(SectionDataModel.ID, SectionDataModel.Maps[i].ID);

                    SectionDataModel.Maps = sectionData.Maps;
                    OutlineEditor.OutlineEditor.AddSectionMaps(SectionDataModel.ID, SectionDataModel.Maps);
                }
                // イベント
                else
                {
                    SectionDataModel.ReferringSwitches = sectionData.ReferringSwitches;
                    SectionDataModel.RelatedBySwitchSectionIds = sectionData.RelatedBySwitchSectionIds;
                }

                Show();
            };

            // エクスポート
            var templateExportButton = container.Q<Button>("export_button");
            templateExportButton.clicked += () =>
            {
                SectionDataModel tes = null;
                for (int i = 0; i < OutlineEditor.OutlineEditor.OutlineDataModel.Sections.Count; i++)
                    if (OutlineEditor.OutlineEditor.OutlineDataModel.Sections[i].ID == SectionDataModel.ChapterID)
                    {
                        tes = OutlineEditor.OutlineEditor.OutlineDataModel.Sections[i];
                        break;
                    }

                var sectionsJson =
                    DataConverter
                        .ConvertSectionToJson(SectionDataModel);

                AssetManageExporter.StartToJson(sectionsJson, SectionDataModel.ID);
            };


            // ID
            var lName = container.Q<Label>("WORD_0038");
            lName.text = EditorLocalize.LocalizeText("WORD_0038") + ": " + SectionDataModel.SerialNumberString;

            // チャプターID
            var selectChapterIdContainer = container.Q<VisualElement>("selectChapterId");
            var candidateChapterIds =
                OutlineEditor.OutlineEditor.OutlineDataModel.Chapters.Select(chapter => chapter.Name).ToList();
            var selectingChapterIndex =
                OutlineEditor.OutlineEditor.OutlineDataModel.Chapters.FindIndex(
                    chapter => chapter.ID == SectionDataModel.ChapterID);
            var selectChapterPopupField = new PopupFieldBase<string>(candidateChapterIds, selectingChapterIndex);
            selectChapterPopupField.RegisterValueChangedCallback(evt =>
            {
                OutlineEditor.OutlineEditor.ChangeSectionChapterID(
                    SectionDataModel.ID,
                    OutlineEditor.OutlineEditor.OutlineDataModel.Chapters[selectChapterPopupField.index].ID);
            });
            selectChapterIdContainer.Add(selectChapterPopupField);

            // 名前
            var tfName = container.Q<ImTextField>("name");
            tfName.value = SectionDataModel.Name;
            tfName.RegisterCallback<FocusOutEvent>(evt =>
            {
                OutlineEditor.OutlineEditor.ChangeSectionName(SectionDataModel.ID, tfName.value);
            });

            // マップ
            {
                var mapsVe = container.Q<VisualElement>("maps");
                foreach (var mapEntity in SectionDataModel.Maps)
                {
                    var mapVe = new VisualElement();
                    mapVe.style.flexDirection = FlexDirection.Row;
                    mapVe.style.alignItems = Align.Center;
                    mapsVe.Add(mapVe);

                    var mapNameLabel = new Label(mapEntity.Name);
                    mapNameLabel.style.flexGrow = 1;
                    mapVe.Add(mapNameLabel);

                    var removeBtn = new Button {text = EditorLocalize.LocalizeText("WORD_0902")};
                    removeBtn.RegisterCallback<ClickEvent>(evt =>
                    {
                        OutlineEditor.OutlineEditor.RemoveSectionMap(SectionDataModel.ID, mapEntity.ID);
                        Show();
                    });
                    mapVe.Add(removeBtn);
                }
            }

            // マップ追加
            {
                var mapSelectVe = container.Q<VisualElement>("map-select");
                var selectedMapIds = SectionDataModel.Maps.Select(mapSubDataModel => mapSubDataModel.ID).ToArray();
                var candidateMapSubDataModels = OutlineEditor.OutlineEditor.MapDataModels
                    .Where(mapEntity => !selectedMapIds.Contains(mapEntity.ID)).ToArray();
                var candidateMapNames = candidateMapSubDataModels.Select(mapEntity => mapEntity.Name).ToList();
                candidateMapNames.Insert(0, EditorLocalize.LocalizeText("WORD_0043"));
                var selectMap = new PopupFieldBase<string>(candidateMapNames, 0);
                selectMap.style.flexGrow = 1;
                mapSelectVe.Add(selectMap);
                var addBtn = new Button {text = EditorLocalize.LocalizeText("WORD_0049")};
                addBtn.RegisterCallback<ClickEvent>(evt =>
                {
                    var targetIndex = selectMap.index - 1;
                    if (targetIndex < 0) return;
                    var targetMap = candidateMapSubDataModels[targetIndex];
                    OutlineEditor.OutlineEditor.AddSectionMap(SectionDataModel.ID, targetMap);
                    Show();
                });
                mapSelectVe.Add(addBtn);
            }

            // イベント
            try
            {
                SectionCorrelationEventInfo.Instance.ShowSectionInspectorUi(SectionDataModel, container);
            }
            catch (Exception)
            {
            }

            // メモ
            var tfMemo = container.Q<ImTextField>("memo");
            tfMemo.value = SectionDataModel.Memo;
            tfMemo.RegisterCallback<FocusOutEvent>(evt =>
            {
                OutlineEditor.OutlineEditor.ChangeSectionMemo(SectionDataModel.ID, tfMemo.value);
            });
        }
    }
}