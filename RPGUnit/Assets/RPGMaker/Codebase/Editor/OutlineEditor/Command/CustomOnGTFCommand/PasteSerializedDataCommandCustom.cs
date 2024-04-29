using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using UnityEditor.GraphToolsFoundation.Overdrive;
using Region = RPGMaker.Codebase.Editor.Hierarchy.Enum.Region;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Command.CustomOnGTFCommand
{
    public class PasteSerializedDataCommandCustom : PasteSerializedDataCommand
    {
        public static async void CustomCommandHandler(GraphToolState graphToolState, PasteSerializedDataCommand command) {
            DefaultCommandHandler(graphToolState, command);

            // ペーストされたノードモデルは直後セレクトされている（という前提）
            foreach (var node in
                graphToolState.SelectionState.GetSelection(graphToolState.GraphViewState.GraphModel))
            {
                if (node is OutlineNodeModel outlineNodeModel)
                {
                    outlineNodeModel.RenewEntity();
                    OutlineEditor.NodeModelsByUuid.Add(outlineNodeModel.GetEntityID(), outlineNodeModel);

                    await Hierarchy.Hierarchy.Refresh(Region.Outline, null, true, true);

                    // ペースト(貼り付け)されたノードが選択状態になっているので、
                    // ヒエラルキーの選択項目とインスペクターの内容をペースト(貼り付け)されたノードのものに変更する。
                    OutlineEditor.SelectElementsCommandProcess(outlineNodeModel);
                }
            }
        }
    }
}