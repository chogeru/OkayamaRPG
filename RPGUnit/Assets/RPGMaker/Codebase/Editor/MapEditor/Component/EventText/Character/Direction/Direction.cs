using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Character.Direction
{
    public class Direction : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            var name = "";
            var direction = "";
            var directionList = new List<string>
            {
                "WORD_0955",
                "WORD_0956",
                "WORD_0957",
                "WORD_0958",
                "WORD_0959",
                "WORD_0960",
                "WORD_0961",
                "WORD_0962",
                "WORD_0963",
                "WORD_0964",
                "WORD_0965"
            };

            switch (eventCommand.code)
            {
                case (int) EventEnum.MOVEMENT_TURN_DOWN:
                    direction = directionList[0];
                    break;
                case (int) EventEnum.MOVEMENT_TURN_LEFT:
                    direction = directionList[1];
                    break;
                case (int) EventEnum.MOVEMENT_TURN_RIGHT:
                    direction = directionList[2];
                    break;
                case (int) EventEnum.MOVEMENT_TURN_UP:
                    direction = directionList[3];
                    break;
                case (int) EventEnum.MOVEMENT_TURN_90_RIGHT:
                    direction = directionList[4];
                    break;
                case (int) EventEnum.MOVEMENT_TURN_90_LEFT:
                    direction = directionList[5];
                    break;
                case (int) EventEnum.MOVEMENT_TURN_180:
                    direction = directionList[6];
                    break;
                case (int) EventEnum.MOVEMENT_TURN_90_RIGHT_OR_LEFT:
                    direction = directionList[7];
                    break;
                case (int) EventEnum.MOVEMENT_TURN_AT_RANDOM:
                    direction = directionList[8];
                    break;
                case (int) EventEnum.MOVEMENT_TURN_TOWARD_PLAYER:
                    direction = directionList[9];
                    break;
                case (int) EventEnum.MOVEMENT_TURN_AWAY_FROM_PLAYER:
                    direction = directionList[10];
                    break;
            }


            if (eventCommand.parameters[0] == "-1")
                name = EditorLocalize.LocalizeText("WORD_0920");
            else if (eventCommand.parameters[0] == "-2")
                name = EditorLocalize.LocalizeText("WORD_0860");
            else
                name = GetEventDisplayName(eventCommand.parameters[0]);

            name += ", ";

            var toggle = "OFF";
            if (eventCommand.parameters[2] == "1")
                toggle = "ON";

            ret += "◆" + EditorLocalize.LocalizeText("WORD_0858") + " : " +
                   EditorLocalize.LocalizeText("WORD_0014") + " : " + name + EditorLocalize.LocalizeText("WORD_0858") +
                   " : " + EditorLocalize.LocalizeText(direction) + " " + EditorLocalize.LocalizeText("WORD_0844") +
                   " : " + toggle;

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}