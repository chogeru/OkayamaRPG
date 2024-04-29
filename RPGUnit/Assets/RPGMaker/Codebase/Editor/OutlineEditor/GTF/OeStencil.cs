using RPGMaker.Codebase.Editor.Common;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace RPGMaker.Codebase.Editor.OutlineEditor.GTF
{
    public class OeStencil : Stencil
    {
        public static readonly string toolName = EditorLocalize.LocalizeText("WORD_1563") + " " +
                                                 EditorLocalize.LocalizeText("WORD_1571"); //"RPGMaker Outline Editor";

        public override string ToolName => toolName;

        public static TypeHandle Chapter { get; } = TypeHandleHelpers.GenerateCustomTypeHandle("Chapter");
        public static TypeHandle Section { get; } = TypeHandleHelpers.GenerateCustomTypeHandle("Section");

        public override IBlackboardGraphModel CreateBlackboardGraphModel(IGraphAssetModel graphAssetModel) {
            return new OeBlackboardGraphModel(graphAssetModel);
        }

        // ノード間の繋ぎ線を、エッジからドラッグして伸ばして、他のエッジに繋がずにドラッグをやめると、
        // "Choose an action for ExecutionFlow"というノードが表示されるが、
        // このノードの表示を抑止するためにこのオーバーライドメソッドを追加。
        public override ISearcherDatabaseProvider GetSearcherDatabaseProvider() {
            return null;
        }
    }
}