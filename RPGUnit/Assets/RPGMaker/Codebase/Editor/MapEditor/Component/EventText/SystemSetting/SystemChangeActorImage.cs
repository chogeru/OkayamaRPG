using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.SystemSetting
{
    public class SystemChangeActorImage : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            var charaList = _GetCharacterList();
            var data = charaList.FirstOrDefault(c => c.uuId == eventCommand.parameters[0]);
            ret += "◆" + EditorLocalize.LocalizeText("WORD_1084") + " : " + data.basic.name;

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }

        private List<CharacterActorDataModel> _GetCharacterList() {
            var characterActorDataModels = DatabaseManagementService.LoadCharacterActor();
            var fileNames = new List<CharacterActorDataModel>();
            for (var i = 0; i < characterActorDataModels.Count; i++)
            {
                if (characterActorDataModels[i].charaType == (int) ActorTypeEnum.ACTOR)
                    fileNames.Add(characterActorDataModels[i]);
            }

            return fileNames;
        }
    }
}