using RPGMaker.Codebase.Editor.OutlineEditor.Component;
using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace RPGMaker.Codebase.Editor.OutlineEditor.GTF
{
    [GraphElementsExtensionMethodsCache(typeof(GraphView))]
    public static class OeUIFactoryExtensions
    {
        public static IModelUI CreateNode(
            this ElementBuilder elementBuilder,
            CommandDispatcher store,
            StartNodeModel model
        ) {
            IModelUI ui = new StartNode();
            ui.SetupBuildAndUpdate(model, store, elementBuilder.View, elementBuilder.Context);
            return ui;
        }

        public static IModelUI CreateNode(
            this ElementBuilder elementBuilder,
            CommandDispatcher store,
            ChapterNodeModel model
        ) {
            IModelUI ui = new ChapterNode();
            ui.SetupBuildAndUpdate(model, store, elementBuilder.View, elementBuilder.Context);
            return ui;
        }

        public static IModelUI CreateNode(
            this ElementBuilder elementBuilder,
            CommandDispatcher store,
            SectionNodeModel model
        ) {
            IModelUI ui = new SectionNode();
            ui.SetupBuildAndUpdate(model, store, elementBuilder.View, elementBuilder.Context);
            return ui;
        }
    }
}