using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Command
{
    public class CreateChapterNodeCommand : UndoableCommand
    {
        private static ChapterDataModel _chapterDataModel;

        public CreateChapterNodeCommand(ChapterDataModel chapterDataModel, SerializableGUID guid = default) {
            _chapterDataModel = chapterDataModel;
        }

        public static void DefaultCommandHandler(GraphToolState graphToolState, CreateChapterNodeCommand command) {
            graphToolState.PushUndo(command);

            var node = graphToolState.GraphViewState.GraphModel.CreateNode<ChapterNodeModel>(_chapterDataModel.ID,
                new Vector2(_chapterDataModel.PosX, _chapterDataModel.PosY));
            node.Init(_chapterDataModel);

            if (!OutlineEditor.NodeModelsByUuid.ContainsKey(_chapterDataModel.ID))
                OutlineEditor.NodeModelsByUuid.Add(_chapterDataModel.ID, node);

            using var graphUpdater = graphToolState.GraphViewState.UpdateScope;
            graphUpdater.MarkNew(node);
        }
    }
}