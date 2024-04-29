using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Command.CustomOnGTFCommand
{
    public class DeleteEdgeCommandCustom : DeleteEdgeCommand
    {
        public static void CustomCommandHandler(GraphToolState graphToolState, DeleteEdgeCommand command) {
            DefaultCommandHandler(graphToolState, command);

            foreach (var edge in command.Models)
                if (edge is OutlineEdgeModel outlineEdgeModel)
                    OutlineEditor.RemoveConnectionDataModel(outlineEdgeModel.ConnectionDataModel.ID);
        }
    }
}