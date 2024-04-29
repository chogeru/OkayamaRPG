using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using System.Collections.Generic;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Command.DataEdit
{
    public class RemoveSectionMapCommand : ModelCommand<SectionNodeModel, string>
    {
        private const string undoStringSingular = "Remove Section Map";
        private const string undoStringPlural   = "Remove Section Maps";

        public RemoveSectionMapCommand(IReadOnlyList<SectionNodeModel> models, string value)
            : base(undoStringSingular,
                undoStringPlural, value, models) {
        }

        public static void DefaultHandler(GraphToolState state, RemoveSectionMapCommand command) {
            state.PushUndo(command);

            using (var graphUpdater = state.GraphViewState.UpdateScope)
            {
                foreach (var nodeModel in command.Models)
                {
                    nodeModel.maps.RemoveAll(mapEntity => mapEntity.ID == command.Value);
                    graphUpdater.MarkChanged(nodeModel);
                    nodeModel.UpdateEntity();
                }
            }
        }
    }
}