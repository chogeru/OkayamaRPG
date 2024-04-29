using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Command
{
    public class CreateStartNodeCommand : UndoableCommand
    {
        private static StartDataModel _startDataModel;

        public CreateStartNodeCommand(StartDataModel startDataModel, SerializableGUID guid = default) {
            _startDataModel = startDataModel;
        }

        public static void DefaultCommandHandler(GraphToolState graphToolState, CreateStartNodeCommand command) {
            graphToolState.PushUndo(command);

            var node = graphToolState.GraphViewState.GraphModel.CreateNode<StartNodeModel>(_startDataModel.ID,
                new Vector2(_startDataModel.PosX, _startDataModel.PosY));
            node.Init(_startDataModel);

            if (!OutlineEditor.NodeModelsByUuid.ContainsKey(_startDataModel.ID))
                OutlineEditor.NodeModelsByUuid.Add(_startDataModel.ID, node);

            using var graphUpdater = graphToolState.GraphViewState.UpdateScope;
            graphUpdater.MarkNew(node);
        }
    }
}