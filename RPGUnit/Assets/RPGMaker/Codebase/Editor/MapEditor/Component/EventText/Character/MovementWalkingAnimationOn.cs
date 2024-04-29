using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using System;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Character
{
    public class MovementWalkingAnimationOn : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            var name = "";
            if (eventCommand.parameters[0] == "-1")
            {
                name = EditorLocalize.LocalizeText("WORD_0920");
            }
            else if (eventCommand.parameters[0] == "-2")
            {
                name = EditorLocalize.LocalizeText("WORD_0860");
            }
            else
            {
                try
                {
                    var events =
                        EventManagementService.LoadEvent();
                    name +=
                        "EV" + (events.IndexOf(events.FirstOrDefault
                            (c => c.id == eventCommand.parameters[0])) + 1).ToString("0000");
                }
                catch (Exception)
                {
                }
            }

            switch (eventCommand.code)
            {
                case (int) EventEnum.MOVEMENT_WALKING_ANIMATION_ON:
                    ret += "◆" + EditorLocalize.LocalizeText("WORD_0967") + " : " + name + ", " +
                           EditorLocalize.LocalizeText("WORD_0968");
                    break;
                case (int) EventEnum.MOVEMENT_WALKING_ANIMATION_OFF:
                    ret += "◆" + EditorLocalize.LocalizeText("WORD_0967") + " : " + name + ", " +
                           EditorLocalize.LocalizeText("WORD_0969");
                    break;
                case (int) EventEnum.MOVEMENT_STEPPING_ANIMATION_ON:
                    ret += "◆" + EditorLocalize.LocalizeText("WORD_0967") + " : " + name + ", " +
                           EditorLocalize.LocalizeText("WORD_0970");
                    break;
                case (int) EventEnum.MOVEMENT_STEPPING_ANIMATION_OFF:
                    ret += "◆" + EditorLocalize.LocalizeText("WORD_0967") + " : " + name + ", " +
                           EditorLocalize.LocalizeText("WORD_0971");
                    break;
            }


            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}