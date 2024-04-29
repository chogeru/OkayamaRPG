using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Actor
{
    public class ActorChangeClass : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            var characterActorDataModels =
                DatabaseManagementService.LoadCharacterActor();
            var actorId = eventCommand.parameters[0];
            var index = characterActorDataModels.IndexOf(
                characterActorDataModels.FirstOrDefault(c => c.uuId == actorId));
            if (index < 0) index = 0;
            ret += "◆" + EditorLocalize.LocalizeText("WORD_0913") + " : " + characterActorDataModels[index].basic.name;

            var classDataModels =
                DatabaseManagementService.LoadCharacterActorClass();

            // 職業名の取得
            var name = "";
            for (var i = 0; i < classDataModels.Count; i++)
                if (classDataModels[i].id == eventCommand.parameters[1])
                    name = classDataModels[i].basic.name;

            ret += " " + EditorLocalize.LocalizeText("WORD_0336") + " : " + name;
            if (eventCommand.parameters[2] == "1")
                ret += "(" + EditorLocalize.LocalizeText("WORD_1599") + ")";

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}