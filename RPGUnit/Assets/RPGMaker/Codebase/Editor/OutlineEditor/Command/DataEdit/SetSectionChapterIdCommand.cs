using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using System.Collections.Generic;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Command.DataEdit
{
    public class SetSectionChapterIdCommand : ModelCommand<SectionNodeModel, string>
    {
        private const string TraitUndoStringSingular = "Set Section Chapter ID";
        private const string TraitUndoStringPlural   = "Set Section Chapter ID";

        public SetSectionChapterIdCommand(IReadOnlyList<SectionNodeModel> models, string value)
            : base(
                TraitUndoStringSingular,
                TraitUndoStringPlural, value, models) {
        }

        public static void DefaultHandler(GraphToolState state, SetSectionChapterIdCommand command) {
            state.PushUndo(command);

            using (var graphUpdater = state.GraphViewState.UpdateScope)
            {
                foreach (var nodeModel in command.Models)
                {
                    nodeModel.chapterID = command.Value;
                    graphUpdater.MarkChanged(nodeModel);
                    nodeModel.UpdateEntity();
                }
            }
        }
    }
}