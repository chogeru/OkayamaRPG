using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Animation;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Character
{
    public class CharacterShowAnimation : AbstractEventText, IEventCommandView
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
                name = GetEventDisplayName(eventCommand.parameters[0]);
            }


            var wait = "";
            if (eventCommand.parameters[2] == "1") wait = " (" + EditorLocalize.LocalizeText("WORD_1087") + ")";

            var animation = DatabaseManagementService.LoadAnimation();
            AnimationDataModel animationData = null;
            for (int i = 0; i < animation.Count; i++)
                if (animation[i].id == eventCommand.parameters[1])
                {
                    animationData = animation[i];
                    break;
                }
            var particleName = eventCommand.parameters[1];

            if (animationData != null)
            {
                particleName = animationData.particleName;
            }
            else
            {
                particleName = EditorLocalize.LocalizeText("WORD_0113");
            }

            ret += "◆" + EditorLocalize.LocalizeText("WORD_0951") + " : " + EditorLocalize.LocalizeText("WORD_0287") +
                   " : " + name + "/ " + EditorLocalize.LocalizeText("WORD_0463") + " : " + particleName + wait;
            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}