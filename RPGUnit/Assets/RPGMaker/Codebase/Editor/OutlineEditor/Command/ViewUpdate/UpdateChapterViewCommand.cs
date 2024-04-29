using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using System.Collections.Generic;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Command.ViewUpdate
{
    public class UpdateChapterViewCommand : ModelCommand<ChapterNodeModel>
    {
        private const string undoStringSingular = "Update Chapter View";
        private const string undoStringPlural   = "Update Chapter Views";

        public UpdateChapterViewCommand(IReadOnlyList<ChapterNodeModel> models)
            : base(undoStringSingular, undoStringPlural, models) {
        }

        public static void DefaultHandler(GraphToolState state, UpdateChapterViewCommand command) {
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