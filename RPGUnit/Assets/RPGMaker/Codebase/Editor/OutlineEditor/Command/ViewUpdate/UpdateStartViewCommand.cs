using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using System.Collections.Generic;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Command.ViewUpdate
{
    public class UpdateStartViewCommand : ModelCommand<StartNodeModel>
    {
        private const string TraitUndoStringSingular = "Update Start View";
        private const string TraitUndoStringPlural   = "Update Start Views";

        public UpdateStartViewCommand(IReadOnlyList<StartNodeModel> models)
            : base(TraitUndoStringSingular, TraitUndoStringPlural, models) {
        }

        public static void DefaultHandler(GraphToolState state, UpdateStartViewCommand command) {
            state.PushUndo(command);

            using (var graphUpdater = state.GraphViewState.UpdateScope)
            {
                foreach (var nodeModel in command.Models)
                {
                    graphUpdater.MarkChanged(nodeModel);
                    nodeModel.UpdateEntity();
                }
            }
        }
    }
}