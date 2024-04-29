using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using System.Collections.Generic;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Command.DataEdit
{
    public class SetSectionNameCommand : ModelCommand<SectionNodeModel, string>
    {
        private const string TraitUndoStringSingular = "Set Section Name";
        private const string TraitUndoStringPlural   = "Set Section Names";

        public SetSectionNameCommand(IReadOnlyList<SectionNodeModel> models, string value)
            : base(TraitUndoStringSingular,
                TraitUndoStringPlural, value, models) {
        }

        public static void DefaultHandler(GraphToolState state, SetSectionNameCommand command) {
            state.PushUndo(command);

            using (var graphUpdater = state.GraphViewState.UpdateScope)
            {
                foreach (var nodeModel in command.Models)
                {
                    nodeModel.name = command.Value;
                    graphUpdater.MarkChanged(nodeModel);
                    nodeModel.UpdateEntity();
                }
            }
        }
    }
}