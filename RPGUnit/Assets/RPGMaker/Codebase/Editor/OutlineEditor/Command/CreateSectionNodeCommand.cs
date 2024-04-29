using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Command
{
    public class CreateSectionNodeCommand : UndoableCommand
    {
        private static SectionDataModel _sectionDataModel;

        public CreateSectionNodeCommand(SectionDataModel sectionDataModel, SerializableGUID guid = default) {
            _sectionDataModel = sectionDataModel;
        }

        public static void DefaultCommandHandler(GraphToolState graphToolState, CreateSectionNodeCommand command) {
            graphToolState.PushUndo(command);

            var node = graphToolState.GraphViewState.GraphModel.CreateNode<SectionNodeModel>(_sectionDataModel.ID,
                new Vector2(_sectionDataModel.PosX, _sectionDataModel.PosY));
            node.Init(_sectionDataModel);

            if (!OutlineEditor.NodeModelsByUuid.ContainsKey(_sectionDataModel.ID))
                OutlineEditor.NodeModelsByUuid.Add(_sectionDataModel.ID, node);

            using var graphUpdater = graphToolState.GraphViewState.UpdateScope;
            graphUpdater.MarkNew(node);
        }
    }
}