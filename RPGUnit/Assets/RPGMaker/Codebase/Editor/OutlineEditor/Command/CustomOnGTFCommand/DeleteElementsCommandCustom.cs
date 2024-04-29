using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Command.CustomOnGTFCommand
{
    public class DeleteElementsCommandCustom : DeleteElementsCommand
    {
        public static void CustomCommandHandler(GraphToolState graphToolState, DeleteElementsCommand command) {
#if true
            // ここでは、command.Modelsにリストアップされた表示上削除されるもの (OutlineNodeModelまたはOutlineEdgeModel) に
            // 対応した～DataModelの削除のみ行い、ノードを削除して繋ぎ先がなくなった繋ぎ線は、
            // OutlineEditor.DispatchRemoveNotConnectedConnections() で削除することにする。
            foreach (var model in command.Models)
            {
                if (model is OutlineNodeModel outlineNodeModel)
                    OutlineEditor.RemoveNodeDataModel(outlineNodeModel.GetEntityID());

                if (model is OutlineEdgeModel outlineEdgeModel)
                    OutlineEditor.RemoveConnectionDataModel(outlineEdgeModel.ConnectionDataModel.ID);
            }
#else
            // このやり方でノードを削除した場合、繋ぎ線の表示が残ってしまう。
            // deletedModelsに削除するノードとそのノードに繋がっていた繋ぎ線がリストアップされるので、
            // それぞれ対応した～DataModelは削除されるが、
            // 表示が削除されるのはcommand.Modelsにリストアップされたノード(OutlineNodeModel)のみなので、
            // 繋がっていた繋ぎ線は表示されたままとなる。
            using (var graphUpdater = graphToolState.GraphViewState.UpdateScope)
            {
                var deletedModels = graphToolState.GraphViewState.GraphModel.DeleteElements(command.Models).ToList();
                foreach (var element in deletedModels)
                {
                    if (element is OutlineNodeModel outlineNodeModel)
                    {
                        OutlineEditor.RemoveNode(outlineNodeModel.GetEntityID());
                    }

                    if (element is OutlineEdgeModel outlineEdgeModel)
                    {
                        OutlineEditor.RemoveConnection(outlineEdgeModel.ConnectionDataModel.ID);
                    }
                }
            }
#endif

            DefaultCommandHandler(graphToolState, command);
        }
    }
}