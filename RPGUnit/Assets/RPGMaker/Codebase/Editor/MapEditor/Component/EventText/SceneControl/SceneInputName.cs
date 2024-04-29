using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.SceneControl
{
    public class SceneInputName : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            var characterActorDataModels = DatabaseManagementService.LoadCharacterActor();
            var characterActorNameList = new List<string>();
            var characterActorID = new List<string>();
            for (var i = 0; i < characterActorDataModels.Count; i++)
            {
                characterActorNameList.Add(characterActorDataModels[i].basic.name);
                characterActorID.Add(characterActorDataModels[i].uuId);
            }

            if (characterActorNameList.Count == 0) characterActorNameList.Add(EditorLocalize.LocalizeText("WORD_0113"));
            ret += "◆" + EditorLocalize.LocalizeText("WORD_1064") + " : ";

            ret += characterActorNameList[characterActorID.IndexOf(eventCommand.parameters[0])] + "," +
                   eventCommand.parameters[1] + " " +
                   EditorLocalize.LocalizeText("WORD_1065");
            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}