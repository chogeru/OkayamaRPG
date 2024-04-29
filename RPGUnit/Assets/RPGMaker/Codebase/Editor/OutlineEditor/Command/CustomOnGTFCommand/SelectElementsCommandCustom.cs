using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Command.CustomOnGTFCommand
{
    public class SelectElementsCommandCustom : SelectElementsCommand
    {
        // HierarchyのFoldoutクリックで使用されるコンストラクタ。
        public SelectElementsCommandCustom(SelectionMode mode, params IGraphElementModel[] models)
            : base(mode, models) {
        }

        // ノードクリックで呼ばれる。
        public static void CustomCommandHandler(GraphToolState state, SelectElementsCommand command) {
            DefaultCommandHandler(state, command);

            foreach (var nodeModel in command.Models)
                if (nodeModel is OutlineNodeModel outlineNodeModel)
                    OutlineEditor.SelectElementsCommandProcess(outlineNodeModel);
        }

        // HierarchyのFoldoutクリックまたはCustomCommandHandler()下から呼ばれる。
        public static void CustomCommandCustomHandler(GraphToolState state, SelectElementsCommand command) {
            DefaultCommandHandler(state, command);

            foreach (var nodeModel in command.Models)
                if (nodeModel is OutlineNodeModel outlineNodeModel)
                    outlineNodeModel.SetUpToInspector();
        }
    }
}