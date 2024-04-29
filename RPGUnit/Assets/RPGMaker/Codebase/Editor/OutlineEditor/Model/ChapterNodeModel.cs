using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.Editor.Common.View;
using System;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Model
{
    [Serializable]
    public class ChapterNodeModel : OutlineNodeModel
    {
        // application entity
        [SerializeField] public MapSubDataModel FieldMapSubDataModel;

        // object properties
        [SerializeField] public string id;
        [SerializeField] public string memo;
        [SerializeField] public string name;
        [SerializeField] public float  posX;
        [SerializeField] public float  posY;
        [SerializeField] public int    supposedLevelMax;
        [SerializeField] public int    supposedLevelMin;

        public ChapterDataModel ChapterDataModel { get; private set; }

        public void Init(ChapterDataModel chapterDataModel) {
            ChapterDataModel = chapterDataModel;
            SetPropertiesFromEntity();
        }

        private void SetPropertiesFromEntity() {
            id = ChapterDataModel.ID;
            name = ChapterDataModel.Name;
            supposedLevelMin = ChapterDataModel.SupposedLevelMin;
            supposedLevelMax = ChapterDataModel.SupposedLevelMax;
            FieldMapSubDataModel = ChapterDataModel.FieldMapSubDataModel;
            posX = ChapterDataModel.PosX;
            posY = ChapterDataModel.PosY;
            memo = ChapterDataModel.Memo;
        }

        public override string GetEntityID() {
            return ChapterDataModel.ID;
        }

        public override void UpdateEntity() {
            base.UpdateEntity();
            ChapterDataModel.ID = id;
            ChapterDataModel.Name = name;
            ChapterDataModel.SupposedLevelMin = supposedLevelMin;
            ChapterDataModel.SupposedLevelMax = supposedLevelMax;
            ChapterDataModel.FieldMapSubDataModel = FieldMapSubDataModel;
            ChapterDataModel.PosX = posX;
            ChapterDataModel.PosY = posY;
            ChapterDataModel.Memo = memo;

            OutlineEditor.SaveOutline(AbstractHierarchyView.RefreshTypeChapterEdit + "," + ChapterDataModel.ID);
        }

        public override void UpdatePosition() {
            base.UpdatePosition();
            ChapterDataModel.PosX = posX = Position.x;
            ChapterDataModel.PosY = posY = Position.y;

            OutlineEditor.SaveOutline(AbstractHierarchyView.RefreshTypeChapterEdit + "," + ChapterDataModel.ID);
        }

        public override void SetUpToInspector() {
            base.SetUpToInspector();
            OutlineEditor.SetDataModelToInspector(ChapterDataModel);
        }

        public override void RenewEntity() {
            base.RenewEntity();
            ChapterDataModel = OutlineEditor.PasteChapterDataModel();
            id = ChapterDataModel.ID;
            ChapterDataModel.ID = id;
            ChapterDataModel.Name = name;
            ChapterDataModel.SupposedLevelMin = supposedLevelMin;
            ChapterDataModel.SupposedLevelMax = supposedLevelMax;
            ChapterDataModel.FieldMapSubDataModel = FieldMapSubDataModel;
            ChapterDataModel.PosX = posX;
            ChapterDataModel.PosY = posY;
            ChapterDataModel.Memo = memo;

            OutlineEditor.SaveOutline(AbstractHierarchyView.RefreshTypeChapterCreate + "," + ChapterDataModel.ID);

        }
    }
}