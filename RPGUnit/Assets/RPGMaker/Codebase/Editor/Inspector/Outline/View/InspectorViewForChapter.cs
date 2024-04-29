using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.Outline.View
{
    /// <summary>
    /// [アウトライン]-[チャプター] Inspector
    /// </summary>
    public class InspectorViewForChapter : AbstractOutlineInspectorView
    {
        private const string Uxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/Outline/Asset/InspectorViewForChapter.uxml";

        public InspectorViewForChapter(ChapterDataModel chapterDataModel) {
            ChapterDataModel = chapterDataModel;

            var container = new VisualElement();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Uxml);
            var labelFromUxml = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUxml);
            labelFromUxml.style.flexGrow = 1;
            container.Add(labelFromUxml);
            Add(container);

            // ID
            var lName = container.Q<Label>("WORD_0038");
            lName.text = ChapterDataModel.SerialNumberString;

            // 名前
            var tfName = container.Q<ImTextField>("name");
            tfName.value = chapterDataModel.Name;
            tfName.RegisterCallback<FocusOutEvent>(evt =>
            {
                OutlineEditor.OutlineEditor.ChangeChapterName(ChapterDataModel.ID, tfName.value);
            });

            // 想定レベル下限
            var slSupposedLevelMin = container.Q<SliderInt>("supposedLevelMin");
            slSupposedLevelMin.value = chapterDataModel.SupposedLevelMin;
            slSupposedLevelMin.RegisterValueChangedCallback(evt =>
            {
                OutlineEditor.OutlineEditor.ChangeChapterSupposedLevelMin(ChapterDataModel.ID, evt.newValue);
            });

            // 想定レベル上限
            var slSupposedLevelMax = container.Q<SliderInt>("supposedLevelMax");
            slSupposedLevelMax.value = chapterDataModel.SupposedLevelMax;
            slSupposedLevelMax.RegisterValueChangedCallback(evt =>
            {
                OutlineEditor.OutlineEditor.ChangeChapterSupposedLevelMax(ChapterDataModel.ID, evt.newValue);
            });

            // マップ指定
            var selectMapContainer = container.Q<VisualElement>("selectMap");
            var candidateMapNames = OutlineEditor.OutlineEditor.MapDataModels
                .Select(mapEntity => mapEntity.Name).ToList();
            candidateMapNames.Insert(0, EditorLocalize.LocalizeText("WORD_0113"));
            var selectingMapIndex = ChapterDataModel.FieldMapSubDataModel == null
                ? 0
                : OutlineEditor.OutlineEditor.MapDataModels.FindIndex(mapEntity =>
                      mapEntity.ID == ChapterDataModel.FieldMapSubDataModel.ID) +
                  1;
            var selectMap = new PopupFieldBase<string>(candidateMapNames, selectingMapIndex);
            selectMap.RegisterValueChangedCallback(evt =>
            {
                var targetIndex = selectMap.index - 1;
                var selectedMap = targetIndex > -1 ? OutlineEditor.OutlineEditor.MapDataModels[targetIndex] : null;
                OutlineEditor.OutlineEditor.ChangeChapterFieldMap(ChapterDataModel.ID, selectedMap);
            });
            selectMapContainer.Add(selectMap);

            // マップ編集ボタン
            var btnEditMap = container.Q<Button>("btnEditMap");
            btnEditMap.RegisterCallback<ClickEvent>(evt =>
            {
                var targetIndex = selectMap.index - 1;
                var selectedMap = targetIndex > -1 ? OutlineEditor.OutlineEditor.MapDataModels[targetIndex] : null;
                OutlineEditor.OutlineEditor.OpenMapToEdit(selectedMap);
            });

            // メモ
            var tfMemo = container.Q<ImTextField>("memo");
            tfMemo.value = chapterDataModel.Memo;
            tfMemo.RegisterCallback<FocusOutEvent>(evt =>
            {
                OutlineEditor.OutlineEditor.ChangeChapterMemo(ChapterDataModel.ID, tfMemo.value);
            });
        }

        public ChapterDataModel ChapterDataModel { get; }
    }
}