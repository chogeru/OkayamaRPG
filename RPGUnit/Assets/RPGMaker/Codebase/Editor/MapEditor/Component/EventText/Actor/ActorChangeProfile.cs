using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Actor;
using System.Linq;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.Editor.MapEditor.Window.EventEdit.ExecutionContentsWindow;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Actor
{
    /// <summary>
    ///     [アクター設定の変更]のうち、プロフィールについての実行内容枠での表示物
    /// </summary>
    public class ActorChangeProfile : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            //イベント内のコマンドを検索
            var instance = ExecutionContentsWindowParam.instance;
            var eventDataModels = EventManagementService.LoadEvent();
            var eventIndex = eventDataModels.FindIndex(v => v.id == instance.eventId);
            var commands = eventDataModels[eventIndex].eventCommands;
            var currentIndex = commands.IndexOf(eventCommand);

            // [アクター設定の変更]の先頭にあるかどうかをチェック
            var baseIndex = ChangeName.GetBaseCommandIndex(commands, currentIndex);
            var isHeadCommnad = baseIndex == currentIndex;

            // 先頭の行じゃければインデントをずらしてパラメータだけ表示する
            var title = isHeadCommnad ? $"◆{EditorLocalize.LocalizeText("WORD_0916")} : " : "\t\t\t";
            var characterActorDataModels = DatabaseManagementService.LoadCharacterActor();
            var actorId = eventCommand.parameters[0];
            var actor = characterActorDataModels.FirstOrDefault(c => c.uuId == actorId);
            var actorName = actor?.basic.name;

            // テキストを代入
            ret =
                $"{indent}{title}{EditorLocalize.LocalizeText("WORD_0339")} : {actorName}, {eventCommand.parameters[6]}";
            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}