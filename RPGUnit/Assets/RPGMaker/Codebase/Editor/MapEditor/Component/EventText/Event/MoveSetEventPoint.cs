using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Event
{
    public class MoveSetEventPoint : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            var variables = _GetVariablesList();
            var eventNameList = new List<string>();
            var eventIDList = new List<string>();

            var eventMapEntities = new List<EventMapDataModel>();
            var allEventMap = EventManagementService.LoadEventMap();
            EventMapDataModel eventDataModel = null;
            foreach (var EventMap in allEventMap)
                if (EventMap.eventId == eventCommand.parameters[0])
                    eventDataModel = EventMap;

            if (eventDataModel != null)
            {
                if (eventDataModel.name == "")
                    eventNameList.Add("EV " + string.Format("{0:D4}", eventDataModel.SerialNumber) +
                                      EditorLocalize.LocalizeText("WORD_1518"));
                else
                    eventNameList.Add("EV " + string.Format("{0:D4}", eventDataModel.SerialNumber) +
                                      eventDataModel.name);

                eventIDList.Add(eventDataModel.eventId);
            }

            ret = indent;

            //向きのList
            var direct = new List<string> {"WORD_0926", "WORD_0299", "WORD_0813", "WORD_0814", "WORD_0297"};

            ret += "◆" + EditorLocalize.LocalizeText("WORD_0919") + " : ";

            //パラメーター[0]に"-1"かイベントIdに合致する物が無かったら「このイベント」
            if (eventCommand.parameters[0] == "-1" || eventIDList.IndexOf(eventCommand.parameters[0]) == -1)
                ret += EditorLocalize.LocalizeText("WORD_0920");
            else
                ret += eventNameList[eventIDList.IndexOf(eventCommand.parameters[0])] + ",";

            //各指定方式でswitch
            switch (eventCommand.parameters[1])
            {
                //直接指定
                case "0":
                    var posY = int.Parse(eventCommand.parameters[3]) * -1;
                    ret += " (" + eventCommand.parameters[2] + "," + posY + ")";
                    break;

                //変数指定
                case "1":
                    var xData = variables.FirstOrDefault(c => c.id == eventCommand.parameters[2]);
                    var yData = variables.FirstOrDefault(c => c.id == eventCommand.parameters[3]);
                    var xName = xData?.name;
                    var yName = yData?.name;
                    if (xName == "")
                        xName = EditorLocalize.LocalizeText("WORD_1518");
                    if (yName == "")
                        yName = EditorLocalize.LocalizeText("WORD_1518");

                    ret += "(" + "#" + (variables.IndexOf(xData) + 1).ToString("0000") + " " + xName + "," +
                           "#" + (variables.IndexOf(yData) + 1).ToString("0000") + " " + yName + ")";
                    break;

                case "2":
                    //パラメーター[5]に"-1"かイベントIdに合致する物が無かったら「このイベント」
                    if (eventCommand.parameters[5] == "-1" || eventIDList.IndexOf(eventCommand.parameters[5]) == -1)
                        ret += " " + EditorLocalize.LocalizeText("WORD_0920");
                    else
                        ret += " " + eventNameList[eventIDList.IndexOf(eventCommand.parameters[5])] + ",";
                    ret += EditorLocalize.LocalizeText("WORD_3069");
                    break;
            }

            //向き            
            ret += "(" + EditorLocalize.LocalizeText("WORD_0858") +
                   EditorLocalize.LocalizeText(direct[int.Parse(eventCommand.parameters[4])]) + ")";

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }

        private List<FlagDataModel.Variable> _GetVariablesList() {
            var flagDataModel = DatabaseManagementService.LoadFlags();
            var fileNames = new List<FlagDataModel.Variable>();
            for (var i = 0; i < flagDataModel.variables.Count; i++)
                fileNames.Add(flagDataModel.variables[i]);
            return fileNames;
        }
    }
}