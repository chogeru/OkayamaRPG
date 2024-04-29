using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using System.Collections.Generic;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Command.DataEdit
{
    public class SetChapterMemoCommand : ModelCommand<ChapterNodeModel, string>
    {
        private const string undoStringSingular = "Set Chapter Memo";
        private const string undoStringPlural   = "Set Chapter Memos";

        public SetChapterMemoCommand(IReadOnlyList<ChapterNodeModel> models, string value)
            : base(undoStringSingular,
                undoStringPlural, value, models) {
        }

        public static void DefaultHandler(GraphToolState state, SetChapterMemoCommand command) {
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