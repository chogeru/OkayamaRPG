using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.OutlineEditor.View
{
    public class SceneViewNavi : VisualElement
    {
        public SceneViewNavi(VisualElement parent) {
            var container = new VisualElement();
            parent.Add(container);
            parent.style.flexDirection = FlexDirection.Column;
            container.style.display = DisplayStyle.Flex;
            container.style.flexDirection = FlexDirection.Row;
            container.SendToBack();

            var btnStoryLine = new Button {text = EditorLocalize.LocalizeText("WORD_0032")};
            btnStoryLine.clicked += OutlineEditor.RenderStoryLine;
            container.Add(btnStoryLine);

            var btnSwitchLine = new Button {text = EditorLocalize.LocalizeText("WORD_0033")};
            btnSwitchLine.clicked += OutlineEditor.RenderSwitchLine;
            container.Add(btnSwitchLine);

            // "原寸"ボタン。
            var btnZoomReset = new Button {text = EditorLocalize.LocalizeText("WORD_1481")};
            btnZoomReset.clicked += OutlineEditor.ResetViewScale;
            container.Add(btnZoomReset);

            // "拡大"ボタン。
            var btnZoomIn = new Button {text = EditorLocalize.LocalizeText("WORD_1479")};
            btnZoomIn.clicked += OutlineEditor.ZoomInViewScale;
            container.Add(btnZoomIn);

            // "縮小"ボタン。
            var btnZoomOut = new Button {text = EditorLocalize.LocalizeText("WORD_1480")};
            btnZoomOut.clicked += OutlineEditor.ZoomOutViewScale;
            container.Add(btnZoomOut);


            var btnAddChapter = new Button
                {text = EditorLocalize.LocalizeText("WORD_0035"), name = "add-chapter-button"};
            btnAddChapter.clicked += () =>
            {
                OutlineEditor.AddNewChapterNode();
            };
            container.Add(btnAddChapter);

            var btnAddSection = new Button
                {text = EditorLocalize.LocalizeText("WORD_0036"), name = "add-section-button"};
            btnAddSection.clicked += () =>
            {
                OutlineEditor.AddNewSectionNode(Inspector.Inspector.GetCurrentChapterId(btnAddSection));
            };
            container.Add(btnAddSection);
            btnAddSection.SetEnabled(false);
        }
    }
}