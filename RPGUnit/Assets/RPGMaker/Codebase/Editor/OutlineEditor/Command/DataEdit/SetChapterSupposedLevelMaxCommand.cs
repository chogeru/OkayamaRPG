using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using System.Collections.Generic;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Command.DataEdit
{
    public class SetChapterSupposedLevelMaxCommand : ModelCommand<ChapterNodeModel, int>
    {
        private const string undoStringSingular = "Set Chapter Supporsed Level Max";
        private const string undoStringPlural   = "Set Chapter Supporsed Level Max";

        public SetChapterSupposedLevelMaxCommand(IReadOnlyList<ChapterNodeModel> models, int value)
            : base(
                undoStringSingular,
                undoStringPlural, value, models) {
        }

        public static void DefaultHandler(GraphToolState state, SetChapterSupposedLevelMaxCommand command) {
            state.PushUndo(command);

            using (var graphUpdater = state.GraphViewState.UpdateScope)
            {
                foreach (var nodeModel in command.Models)
                {
                    nodeModel.supposedLevelMax = command.Value;
                    graphUpdater.MarkChanged(nodeModel);
                    nodeModel.UpdateEntity();
                }
            }
        }
    }
}