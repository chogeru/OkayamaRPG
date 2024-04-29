using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Model
{
    public class OutlineNodeModel : NodeModel
    {
        protected override void OnDefineNode() {
            base.OnDefineNode();
            this.AddExecutionInputPort("", "VIn", PortOrientation.Vertical);
            this.AddExecutionInputPort("", "HIn", PortOrientation.Horizontal);
            this.AddExecutionOutputPort("", "VOut", PortOrientation.Vertical);
            this.AddExecutionOutputPort("", "HOut", PortOrientation.Horizontal);
        }

        public virtual string GetEntityID() {
            // to override
            return "";
        }

        public virtual void UpdateEntity() {
            // to override
        }

        public virtual void UpdatePosition() {
            // to override
        }

        public virtual void SetUpToInspector() {
            // to override
        }

        public virtual void RenewEntity() {
            // to override
        }
    }
}