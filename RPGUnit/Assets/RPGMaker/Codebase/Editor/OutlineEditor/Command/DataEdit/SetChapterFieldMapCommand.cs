using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using System.Collections.Generic;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Command.DataEdit
{
    public class SetChapterFieldMapCommand : ModelCommand<ChapterNodeModel, MapSubDataModel>
    {
        private const string undoStringSingular = "Set Chapter Field Map";
        private const string undoStringPlural   = "Set Chapter Field Map";

        public SetChapterFieldMapCommand(IReadOnlyList<ChapterNodeModel> models, MapSubDataModel value)
            : base(
                undoStringSingular,
                undoStringPlural, value, models) {
        }

        public static void DefaultHandler(GraphToolState state, SetChapterFieldMapCommand command) {
            state.PushUndo(command);

            using (var graphUpdater = state.GraphViewState.UpdateScope)
            {
                foreach (var nodeModel in command.Models)
                {
                    nodeModel.FieldMapSubDataModel = command.Value;
                    graphUpdater.MarkChanged(nodeModel);
                    nodeModel.UpdateEntity();
                }
            }
        }
    }
}