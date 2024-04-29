using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.Editor.Common.View;
using System;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Model
{
    [Serializable]
    public class StartNodeModel : OutlineNodeModel
    {
        // application entity

        // object properties
        [SerializeField] public string id;
        [SerializeField] public string memo;
        [SerializeField] public string name;
        [SerializeField] public float  posX;
        [SerializeField] public float  posY;

        public StartDataModel StartDataModel { get; private set; }

        public void Init(StartDataModel startDataModel) {
            StartDataModel = startDataModel;
            SetPropertiesFromEntity();
        }

        private void SetPropertiesFromEntity() {
            id = StartDataModel.ID;
            name = StartDataModel.Name;
            posX = StartDataModel.PosX;
            posY = StartDataModel.PosY;
            memo = StartDataModel.Memo;
        }

        public override string GetEntityID() {
            return StartDataModel.ID;
        }

        public override void UpdateEntity() {
            base.UpdateEntity();
            bool refresh = StartDataModel.Name != name ? true : false;
            StartDataModel.ID = id;
            StartDataModel.Name = name;
            StartDataModel.PosX = posX;
            StartDataModel.PosY = posY;
            StartDataModel.Memo = memo;

            OutlineEditor.SaveOutline(AbstractHierarchyView.RefreshTypeTitle);
        }

        public override void UpdatePosition() {
            base.UpdatePosition();
            StartDataModel.PosX = posX = Position.x;
            StartDataModel.PosY = posY = Position.y;

            OutlineEditor.SaveOutline();
        }

        public override void SetUpToInspector() {
            base.SetUpToInspector();
            OutlineEditor.SetDataModelToInspector(StartDataModel);
        }

        public override void RenewEntity() {
            base.RenewEntity();
            StartDataModel = OutlineEditor.AddNewStartDataModel();
            id = StartDataModel.ID;
            UpdateEntity();
        }
    }
}