using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using System.Linq;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Command.CustomOnGTFCommand
{
    public class MoveElementsCommandCustom : MoveElementsCommand
    {
        public static void CustomCommandHandler(GraphToolState graphToolState, MoveElementsCommand command) {
            DefaultCommandHandler(graphToolState, command);

            using (var graphUpdater = graphToolState.GraphViewState.UpdateScope)
            {
                var movingNodes = command.Models.OfType<OutlineNodeModel>().ToList();

                foreach (var node in movingNodes) node.UpdatePosition();
            }
        }
    }
}