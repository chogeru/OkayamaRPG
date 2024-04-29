using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Party
{
    public class PartyChange : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            var characterActorDataModels = DatabaseManagementService.LoadCharacterActor();
            ret += "◆" + EditorLocalize.LocalizeText("WORD_1089") + " : ";

            //アクター名
            var data = characterActorDataModels.FirstOrDefault(c => c.uuId == eventCommand.parameters[0]);
            var name = "";
            if (data != null && data.basic.name != null) name = data.basic.name;
            else name = characterActorDataModels[0].basic.name;
            ret += name + " / ";

            //操作(増やす、減らす)
            if (int.Parse(eventCommand.parameters[1]) == 1)
                ret += EditorLocalize.LocalizeText("WORD_1090") + " ";
            else
                ret += EditorLocalize.LocalizeText("WORD_1091") + " ";

            if (eventCommand.parameters[2] == "1") ret += "/" + EditorLocalize.LocalizeText("WORD_1093");

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}