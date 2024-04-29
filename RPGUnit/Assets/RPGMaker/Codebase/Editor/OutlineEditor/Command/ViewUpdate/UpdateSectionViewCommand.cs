using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Command.ViewUpdate
{
    public class UpdateSectionViewCommand : ModelCommand<SectionNodeModel>
    {
        private const string undoStringSingular = "Update Section View";
        private const string undoStringPlural   = "Update Section Views";

        public UpdateSectionViewCommand(IReadOnlyList<SectionNodeModel> models)
            : base(undoStringSingular, undoStringPlural, models) {
        }

        public static void DefaultHandler(GraphToolState state, UpdateSectionViewCommand command) {
            state.PushUndo(command);

            using (var graphUpdater = state.GraphViewState.UpdateScope)
            {
                foreach (var nodeModel in command.Models)
                {
                    graphUpdater.MarkChanged(nodeModel);
                    nodeModel.UpdateEntity(AbstractHierarchyView.RefreshTypeSectionEdit);
                }
            }
        }
    }
}