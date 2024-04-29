using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using System.Linq;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Command
{
    public class CreateConnectionCommand : UndoableCommand
    {
        private static ConnectionDataModel _connectionDataModel;

        public CreateConnectionCommand(ConnectionDataModel connectionDataModel, SerializableGUID guid = default) {
            _connectionDataModel = connectionDataModel;
        }

        public static void DefaultCommandHandler(GraphToolState graphToolState, CreateConnectionCommand command) {
            graphToolState.PushUndo(command);

            var lNode = OutlineEditor.NodeModelsByUuid[_connectionDataModel.LUuid];
            var rNode = OutlineEditor.NodeModelsByUuid[_connectionDataModel.RUuid];

            var fromPort = lNode.Ports.ToList().FirstOrDefault(port =>
                (int) port.Direction == _connectionDataModel.LPortDirection &&
                (int) port.Orientation == _connectionDataModel.LPortOrientation);

            var toPort = rNode.Ports.ToList().FirstOrDefault(port =>
                (int) port.Direction == _connectionDataModel.RPortDirection &&
                (int) port.Orientation == _connectionDataModel.RPortOrientation);

            var edge = (OutlineEdgeModel) graphToolState.GraphViewState.GraphModel.CreateEdge(toPort, fromPort);
            edge.Init(_connectionDataModel);

            if (!OutlineEditor.EdgeModelsByUuid.ContainsKey(_connectionDataModel.ID))
                OutlineEditor.EdgeModelsByUuid.Add(_connectionDataModel.ID, edge);
        }
    }
}