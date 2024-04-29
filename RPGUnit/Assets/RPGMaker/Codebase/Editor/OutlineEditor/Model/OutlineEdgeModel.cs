using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Model
{
    public class OutlineEdgeModel : EdgeModel
    {
        public ConnectionDataModel ConnectionDataModel { get; private set; }

        public void Init(ConnectionDataModel connectionDataModel) {
            ConnectionDataModel = connectionDataModel;
        }
    }
}