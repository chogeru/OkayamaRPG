using RPGMaker.Codebase.Editor.OutlineEditor.Command;
using RPGMaker.Codebase.Editor.OutlineEditor.Command.CustomOnGTFCommand;
using RPGMaker.Codebase.Editor.OutlineEditor.Command.DataEdit;
using RPGMaker.Codebase.Editor.OutlineEditor.Command.ViewUpdate;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace RPGMaker.Codebase.Editor.OutlineEditor.GTF
{
    public class OeGraphState : GraphToolState
    {
        public OeGraphState(Hash128 graphViewEditorWindowGUID, Preferences preferences)
            : base(
                graphViewEditorWindowGUID, preferences) {
        }

        public override void RegisterCommandHandlers(Dispatcher dispatcher) {
            base.RegisterCommandHandlers(dispatcher);

            if (!(dispatcher is CommandDispatcher commandDispatcher))
                return;

            // 要素生成
            commandDispatcher.RegisterCommandHandler<CreateStartNodeCommand>(CreateStartNodeCommand
                .DefaultCommandHandler);
            commandDispatcher.RegisterCommandHandler<CreateChapterNodeCommand>(CreateChapterNodeCommand
                .DefaultCommandHandler);
            commandDispatcher.RegisterCommandHandler<CreateSectionNodeCommand>(CreateSectionNodeCommand
                .DefaultCommandHandler);
            commandDispatcher.RegisterCommandHandler<CreateConnectionCommand>(CreateConnectionCommand
                .DefaultCommandHandler);

            // 表示更新
            commandDispatcher.RegisterCommandHandler<UpdateChapterViewCommand>(UpdateChapterViewCommand.DefaultHandler);
            commandDispatcher.RegisterCommandHandler<UpdateSectionViewCommand>(UpdateSectionViewCommand.DefaultHandler);
            commandDispatcher.RegisterCommandHandler<UpdateStartViewCommand>(UpdateStartViewCommand.DefaultHandler);

            // データ編集
            commandDispatcher.RegisterCommandHandler<SetChapterNameCommand>(SetChapterNameCommand.DefaultHandler);
            commandDispatcher.RegisterCommandHandler<SetChapterSupposedLevelMinCommand>(
                SetChapterSupposedLevelMinCommand.DefaultHandler);
            commandDispatcher.RegisterCommandHandler<SetChapterSupposedLevelMaxCommand>(
                SetChapterSupposedLevelMaxCommand.DefaultHandler);
            commandDispatcher.RegisterCommandHandler<SetChapterFieldMapCommand>(
                SetChapterFieldMapCommand.DefaultHandler);
            commandDispatcher.RegisterCommandHandler<SetChapterMemoCommand>(SetChapterMemoCommand.DefaultHandler);
            commandDispatcher.RegisterCommandHandler<SetSectionChapterIdCommand>(SetSectionChapterIdCommand
                .DefaultHandler);
            commandDispatcher.RegisterCommandHandler<SetSectionNameCommand>(SetSectionNameCommand.DefaultHandler);
            commandDispatcher.RegisterCommandHandler<RemoveSectionMapCommand>(RemoveSectionMapCommand.DefaultHandler);
            commandDispatcher.RegisterCommandHandler<SetSectionMemoCommand>(SetSectionMemoCommand.DefaultHandler);

            // 要素選択時（既存のイベントハンドラをoverride）
            commandDispatcher.RegisterCommandHandler<SelectElementsCommand>(SelectElementsCommandCustom
                .CustomCommandHandler);

            // 要素選択時（既存のイベントハンドラをoverride）
            commandDispatcher.RegisterCommandHandler<SelectElementsCommandCustom>(SelectElementsCommandCustom
                .CustomCommandCustomHandler);

            // 要素移動時（既存のイベントハンドラをoverride）
            commandDispatcher.RegisterCommandHandler<MoveElementsCommand>(
                MoveElementsCommandCustom.CustomCommandHandler);

            // 要素削除時（既存のイベントハンドラをoverride）
            commandDispatcher.RegisterCommandHandler<DeleteElementsCommand>(DeleteElementsCommandCustom
                .CustomCommandHandler);

            // 要素コピペ時（既存のイベントハンドラをoverride）
            commandDispatcher.RegisterCommandHandler<PasteSerializedDataCommand>(PasteSerializedDataCommandCustom
                .CustomCommandHandler);

            // エッジ作成時（既存のイベントハンドラをoverride）
            commandDispatcher.RegisterCommandHandler<CreateEdgeCommand>(CreateEdgeCommandCustom.CustomCommandHandler);

            // エッジ削除時（既存のイベントハンドラをoverride）
            commandDispatcher.RegisterCommandHandler<DeleteEdgeCommand>(DeleteEdgeCommandCustom.CustomCommandHandler);

            // セクションへマップ追加時
            commandDispatcher.RegisterCommandHandler<AddSectionMapCommand>(AddSectionMapCommand.DefaultHandler);
        }
    }
}