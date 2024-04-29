using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.OutlineEditor.GTF
{
    /// <summary>
    ///     Outline Editor Graph View
    /// </summary>
    public class OeGraphView : GraphView
    {
        public OeGraphView(GraphViewEditorWindow window, CommandDispatcher commandDispatcher, string graphViewName)
            :
            base(window, commandDispatcher, graphViewName) {
        }

        // コンテキストメニューを構築。
        protected override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            if (CommandDispatcher == null) return;

            if (evt.menu.MenuItems().Count > 0)
                // Connectionの場合設定されているのでクリアする。
                evt.menu.MenuItems().Clear();

            // マウスカーソル位置にある ～DataModel を取得。
            var targetModel = NodeOrEdgeToOutlineModel(evt.target);

            if (targetModel != null && !(targetModel is StartNodeModel))
            {
                // コピー。
                if (targetModel is OutlineNodeModel)
                    evt.menu.AppendAction(
                        EditorLocalize.LocalizeText("WORD_1462"),
                        delegate {
                            if (targetModel is SectionNodeModel copySectionNodeModel)
                                OutlineEditor.CopySectionNode(copySectionNodeModel.SectionDataModel);
                            else
                            {
                                OutlineEditor.RefreshCopySectionNode();
                                CopySelectionCallback();
                            }
                        },
                        CanCopySelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

                // 削除。
                if (!(targetModel is OutlineEdgeModel && OutlineEditor.Rendered == Rendered.SwitchLine))
                    evt.menu.AppendAction(
                        EditorLocalize.LocalizeText("WORD_0383"),
                        delegate 
                        { 
                            OutlineEditor.RemoveGraphElementModel(targetModel);
                            OutlineEditor.SaveOutline();
                        },
                        CanDeleteSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

                // マップの追加。
                if (targetModel is SectionNodeModel sectionNodeModel)
                    evt.menu.AppendAction(
                        EditorLocalize.LocalizeText("WORD_0049"),
                        delegate { OutlineEditor.AddNewMapToSectionNode(sectionNodeModel); });
            }
            else if (targetModel == null)
            {
                var mousePosition = evt.mousePosition;
                var graphPosition = ContentViewContainer.WorldToLocal(mousePosition);

                // チャプターの作成。
                evt.menu.AppendAction(
                    EditorLocalize.LocalizeText("WORD_1609"),
                    delegate { OutlineEditor.AddNewChapterNode((int)graphPosition.x, (int)graphPosition.y); });

                // セクションの作成。
                var currentChapterId = Inspector.Inspector.GetCurrentChapterId(this);
                evt.menu.AppendAction(
                    EditorLocalize.LocalizeText("WORD_1610"),
                    delegate { OutlineEditor.AddNewSectionNode(currentChapterId, (int) graphPosition.x, (int) graphPosition.y); },
                    currentChapterId != null ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

                // 貼り付け。
                evt.menu.AppendAction(
                    EditorLocalize.LocalizeText("WORD_1463"),
                    delegate
                    {
                        if (OutlineEditor.CanPasteSectionNode() == true)
                            OutlineEditor.PasteSectionNode(graphPosition);
                        else
                            PasteCallback();
                        OutlineEditor.DebugLog();
                    },
                    (CanPaste && CopyPasteDataUtil.IsEmpty() == false) || 
                    OutlineEditor.CanPasteSectionNode() == true
                        ? DropdownMenuAction.Status.Normal
                        : DropdownMenuAction.Status.Disabled);
            }

            IGraphElementModel NodeOrEdgeToOutlineModel(IEventHandler target) {
                switch (target)
                {
                    case Node node:
                        return node.Model;
                    case Edge edge:
                        return edge.Model;
                    case OeGraphView oeGraphView:
                        break;
                    default:
                        DebugUtil.Assert(false);
                        break;
                }

                return null;
            }
        }

        public void CallCopySelectionCallback() {
            CopySelectionCallback();
        }

        public void CallPasteCallback() {
            PasteCallback();
            OutlineEditor.DebugLog();
        }
    }
}