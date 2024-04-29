using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Command.CustomOnGTFCommand
{
    public class CreateEdgeCommandCustom : CreateEdgeCommand
    {
        public static void CustomCommandHandler(GraphToolState graphToolState, CreateEdgeCommand command) {
            // スイッチライン表示時は、ユーザーの操作で接続線は繋がない。
            if (OutlineEditor.Rendered == Rendered.SwitchLine) return;

            graphToolState.PushUndo(command);

            using (var graphUpdater = graphToolState.GraphViewState.UpdateScope)
            {
                var graphModel = graphToolState.GraphViewState.GraphModel;

                var fromPortModel = command.FromPortModel;
                var toPortModel = command.ToPortModel;

                var edgesToDelete = command.EdgeModelsToDelete ?? new List<IEdgeModel>();

                // Delete previous connections
                if (toPortModel != null && toPortModel.Capacity != PortCapacity.Multi)
                    edgesToDelete = edgesToDelete.Concat(toPortModel.GetConnectedEdges()).ToList();

                if (command.EdgeModelsToDelete != null)
                {
                    graphModel.DeleteEdges(edgesToDelete);
                    graphUpdater.MarkDeleted(edgesToDelete);
                }
                var edgeModel = graphModel.CreateEdge(toPortModel, fromPortModel) as OutlineEdgeModel;
                graphUpdater.MarkNew(edgeModel);

                if (command.PortAlignment.HasFlag(PortDirection.Input))
                    graphUpdater.MarkModelToAutoAlign(toPortModel.NodeModel);
                if (command.PortAlignment.HasFlag(PortDirection.Output))
                    graphUpdater.MarkModelToAutoAlign(fromPortModel.NodeModel);

                if (
                    command.FromPortModel.NodeModel is OutlineNodeModel lOutlineNodeModel &&
                    command.ToPortModel.NodeModel is OutlineNodeModel rOutlineNodeModel
                )
                {
                    var connectionDataModel = OutlineEditor.AddNewConnection(
                        lOutlineNodeModel.GetEntityID(),
                        (int) fromPortModel.Direction,
                        (int) fromPortModel.Orientation,
                        rOutlineNodeModel.GetEntityID(),
                        (int) toPortModel.Direction,
                        (int) toPortModel.Orientation
                    );

                    // 新規に作成したConneectionをRPGMakerを開き直さずに削除すると、
                    // ConnectionDataModelがnullで例外エラーになるので、ここで設定する。
                    edgeModel.Init(connectionDataModel);
                    OutlineEditor.EdgeModelsByUuid.Add(connectionDataModel.ID, edgeModel);

                    OutlineEditor.DebugLog();
                }
            }
        }
    }
}