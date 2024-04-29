using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using System.Collections.Generic;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Command.DataEdit
{
    public class SetChapterSupposedLevelMinCommand : ModelCommand<ChapterNodeModel, int>
    {
        private const string TraitUndoStringSingular = "Set Chapter Supporsed Level Min";
        private const string TraitUndoStringPlural   = "Set Chapter Supporsed Level Min";

        public SetChapterSupposedLevelMinCommand(IReadOnlyList<ChapterNodeModel> models, int value)
            : base(
                TraitUndoStringSingular,
                TraitUndoStringPlural, value, models) {
        }

        public static void DefaultHandler(GraphToolState state, SetChapterSupposedLevelMinCommand command) {
            state.PushUndo(command);

            using (var graphUpdater = state.GraphViewState.UpdateScope)
            {
                foreach (var nodeModel in command.Models)
                {
                    nodeModel.supposedLevelMin = command.Value;
                    graphUpdater.MarkChanged(nodeModel);
                    nodeModel.UpdateEntity();
                }
            }
        }
    }
}