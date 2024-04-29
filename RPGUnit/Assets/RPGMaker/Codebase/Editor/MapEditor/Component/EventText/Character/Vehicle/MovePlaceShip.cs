using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Character.Vehicle
{
    public class MovePlaceShip : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            var variables = _GetVariablesList();
            var mapManagementService = Editor.Hierarchy.Hierarchy.mapManagementService;
            var mapEntities = mapManagementService.LoadMaps();
            var vehiclesDataModels = DatabaseManagementService.LoadCharacterVehicles();
            var vehicleDropdownChoices = new List<string>();
            var vehicleNameDropdownChoices = new List<string>();
            for (var j = 0; j < vehiclesDataModels.Count; j++)
            {
                vehicleDropdownChoices.Add(vehiclesDataModels[j].id);
                vehicleNameDropdownChoices.Add(vehiclesDataModels[j].name);
            }
            
            var index = vehicleDropdownChoices.IndexOf(eventCommand.parameters[0]);
            if (index < 0) index = 0;
            string vehicleName = vehicleNameDropdownChoices.Count > 0 ? EditorLocalize.LocalizeText("WORD_1009") + " " + vehicleNameDropdownChoices[index] : "";
            ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_1008") + " : " + vehicleName + "," + EditorLocalize.LocalizeText("WORD_1176") + " : ";

            var map = mapEntities.FirstOrDefault(c => c.id == eventCommand.parameters[2]);
            var mapName = "";
            if (map != null && map.name != null) mapName = map.name;
            else if (mapEntities.Count > 0) mapName = mapEntities[0].name;
            else mapName = "";


            if (eventCommand.parameters[1] == "0")
            {
                var y = int.Parse(eventCommand.parameters[4]) < 0
                    ? int.Parse(eventCommand.parameters[4]) * -1
                    : int.Parse(eventCommand.parameters[4]);
                ret += mapName + "(x : " + eventCommand.parameters[3] + ",y : " +
                       y + ")";
            }
            else
            {
                var xData = variables.FirstOrDefault(c => c.id == eventCommand.parameters[3]);
                var yData = variables.FirstOrDefault(c => c.id == eventCommand.parameters[4]);
                var xName = xData?.name;
                var yName = yData?.name;
                if (xName == "") xName = EditorLocalize.LocalizeText("WORD_1518");
                if (yName == "") yName = EditorLocalize.LocalizeText("WORD_1518");
                ret += mapName +
                       "(x : " + "#" + (variables.IndexOf(xData) + 1).ToString("0000") + " " + xName + "," +
                       "y : " + "#" + (variables.IndexOf(yData) + 1).ToString("0000") + " " + yName + ")";
            }

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }

        private List<FlagDataModel.Variable> _GetVariablesList() {
            var flagDataModel = DatabaseManagementService.LoadFlags();
            var fileNames = new List<FlagDataModel.Variable>();
            for (var i = 0; i < flagDataModel.variables.Count; i++) fileNames.Add(flagDataModel.variables[i]);

            return fileNames;
        }
    }
}