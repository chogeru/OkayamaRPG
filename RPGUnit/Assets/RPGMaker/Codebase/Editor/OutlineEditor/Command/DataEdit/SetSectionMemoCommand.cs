using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using System.Collections.Generic;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Command.DataEdit
{
    public class SetSectionMemoCommand : ModelCommand<SectionNodeModel, string>
    {
        private const string TraitUndoStringSingular = "Set Section Memo";
        private const string TraitUndoStringPlural   = "Set Section Memos";

        public SetSectionMemoCommand(IReadOnlyList<SectionNodeModel> models, string value)
            : base(TraitUndoStringSingular,
                TraitUndoStringPlural, value, models) {
        }

        public static void DefaultHandler(GraphToolState state, SetSectionMemoCommand command) {
            state.PushUndo(command);

            using (var graphUpdater = state.GraphViewState.UpdateScope)
            {
                foreach (var nodeModel in command.Models)
                {
                    nodeModel.memo = command.Value;
                    graphUpdater.MarkChanged(nodeModel);
                    nodeModel.UpdateEntity();
                }
            }
        }
    }
}