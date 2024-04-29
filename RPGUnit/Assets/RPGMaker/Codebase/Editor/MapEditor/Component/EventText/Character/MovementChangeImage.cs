using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Character
{
    public class MovementChangeImage : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            var name = "";
            var imageName = "";
            var direction = "";
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
                var allEventMap = EventManagementService.LoadEventMap();
                foreach (var EventMap in allEventMap)
                    if (EventMap.eventId == eventCommand.parameters[0])
                        name = "EV " + string.Format("{0:D4}", EventMap.SerialNumber) + EventMap.name;
            }

            if (eventCommand.parameters[2] == "0")
            {
                imageName = "/ " + EditorLocalize.LocalizeText("WORD_0061");
            }
            else
            {
                var targetAssets = Editor.Hierarchy.Hierarchy.databaseManagementService.LoadAssetManage();
                var num = targetAssets.FindIndex(item => item.id == eventCommand.parameters[1]);

                //素材が見つからなかった時
                if (num < 0)
                    imageName = EditorLocalize.LocalizeText("WORD_0113");
                else
                    imageName = targetAssets[num].name;
            }

            // 向き
            switch (eventCommand.parameters[7])
            {
                case "0":
                    direction = "/ " + EditorLocalize.LocalizeText("WORD_0061");
                    break;
                case "1":
                    direction = "/ " + EditorLocalize.LocalizeText("WORD_0858") + " : " + EditorLocalize.LocalizeText("WORD_1532");
                    break;
                case "2":
                    direction = "/ " + EditorLocalize.LocalizeText("WORD_0858") + " : " + EditorLocalize.LocalizeText("WORD_1533");
                    break;
                case "3":
                    direction = "/ " + EditorLocalize.LocalizeText("WORD_0858") + " : " + EditorLocalize.LocalizeText("WORD_1534");
                    break;
                case "4":
                    direction = "/ " + EditorLocalize.LocalizeText("WORD_0858") + " : " + EditorLocalize.LocalizeText("WORD_1535");
                    break;
                case "5":
                    direction = "/ " + EditorLocalize.LocalizeText("WORD_0858") + " : " + EditorLocalize.LocalizeText("WORD_0509");
                    break; 
            }

            List<string> animation = EditorLocalize.LocalizeTexts(new List<string> {"WORD_2594", "WORD_0976", "WORD_0977", "WORD_0978"});

            var opacity = "";
            if (eventCommand.parameters[4] == "0")
                opacity = "/ " + EditorLocalize.LocalizeText("WORD_0061");
            else
                opacity = "/ " + EditorLocalize.LocalizeText("WORD_1120") + " : " + eventCommand.parameters[3];

            var synthesis = "";
            if (eventCommand.parameters[6] == "0")
                synthesis = "/ " + EditorLocalize.LocalizeText("WORD_0061");
            else
                synthesis = "/ " + EditorLocalize.LocalizeText("WORD_0975") + " : " +
                            animation[int.Parse(eventCommand.parameters[5])];
            ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_0972") + " : " + EditorLocalize.LocalizeText("WORD_0014") +
                                               " : " + name + "/ " + EditorLocalize.LocalizeText("WORD_0093") + " : " + imageName + direction + opacity +
                                               synthesis;


            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}