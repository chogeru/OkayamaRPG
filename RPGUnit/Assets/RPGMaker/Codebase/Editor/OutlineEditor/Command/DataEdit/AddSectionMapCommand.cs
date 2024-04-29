using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Command.DataEdit
{
    public class AddSectionMapCommand : ModelCommand<SectionNodeModel, MapSubDataModel>
    {
        private const string TraitUndoStringSingular = "Add Section Map";
        private const string TraitUndoStringPlural   = "Add Section Maps";

        public AddSectionMapCommand(IReadOnlyList<SectionNodeModel> models, MapSubDataModel value)
            : base(TraitUndoStringSingular,
                TraitUndoStringPlural, value, models) {
        }

        public static void DefaultHandler(GraphToolState state, AddSectionMapCommand command) {
            state.PushUndo(command);

            using (var graphUpdater = state.GraphViewState.UpdateScope)
            {
                foreach (var nodeModel in command.Models)
                {
                    nodeModel.maps.Add(command.Value);
                    graphUpdater.MarkChanged(nodeModel);
                    nodeModel.UpdateEntity(AbstractHierarchyView.RefreshTypeSectionMapAdd + "," + nodeModel.chapterID + "," + nodeModel.SectionDataModel.ID + "," + command.Value.ID);
                }
            }
        }
    }
}