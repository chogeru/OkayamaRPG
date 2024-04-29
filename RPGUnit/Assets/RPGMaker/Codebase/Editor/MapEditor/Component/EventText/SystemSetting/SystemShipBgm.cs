using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.SystemSetting
{
    public class SystemShipBgm : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            ret += "◆" + EditorLocalize.LocalizeText("WORD_1074") + " : " +
                   VehicleIdToName(eventCommand.parameters[4]) + ", " +
                   SoundHelper.RemoveExtention(eventCommand.parameters[0]) + ", (" +
                   eventCommand.parameters[1] + ", " +
                   eventCommand.parameters[2] + ", " +
                   eventCommand.parameters[3] + ")";
            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }

        //表示用に乗り物IDを名前にして返す
        private string VehicleIdToName(string id) {
            var vehiclesDataModels = DatabaseManagementService.LoadCharacterVehicles();
            var returnName = "";
            foreach (var vehiclesDataModel in vehiclesDataModels)
            {
                if (vehiclesDataModel.id == id)
                {
                    returnName = vehiclesDataModel.name;
                    break;
                }
            }

            return returnName;
        }
    }
}