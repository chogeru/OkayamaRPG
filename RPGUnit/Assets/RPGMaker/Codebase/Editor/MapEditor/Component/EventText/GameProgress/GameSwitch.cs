using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.Editor.Common;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.GameProgress
{
    public class GameSwitch : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            ret += "◆" + EditorLocalize.LocalizeText("WORD_1012") + " : ";
            var switchList = _GetSwitchList();
            if (eventCommand.parameters[0] == "0")
            {
                var index = -1;
                try
                {
                    index = int.Parse(eventCommand.parameters[1]);
                }
                catch (Exception)
                {
                }

                var data = switchList[0];
                if (index < 0)
                {
                    for (var i = 0; i < switchList.Count; i++)
                        if (switchList[i].id == eventCommand.parameters[1])
                        {
                            data = switchList[i];
                            index = i;
                            break;
                        }
                }
                else
                {
                    data = switchList[index];
                }

                var name = data.name;
                if (name == "") name = EditorLocalize.LocalizeText("WORD_1518");
                if (index < 0) index = 0;
                ret += "#" + (index + 1).ToString("0000") + name + " = ";
            }
            else
            {
                ret += "#" +
                       int.Parse(eventCommand.parameters[1]).ToString("0000") + ".." +
                       int.Parse(eventCommand.parameters[2]).ToString("0000") + " = ";
            }

            try
            {
                if (int.Parse(eventCommand.parameters[3]) == 0)
                    ret += "ON";
                else
                    ret += "OFF";
            }
            catch (Exception)
            {
                ret += "OFF";
            }

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }

        private List<FlagDataModel.Switch> _GetSwitchList() {
            var flagDataModel = DatabaseManagementService.LoadFlags();
            var fileNames = new List<FlagDataModel.Switch>();
            for (var i = 0; i < flagDataModel.switches.Count; i++) fileNames.Add(flagDataModel.switches[i]);
            return fileNames;
        }
    }
}