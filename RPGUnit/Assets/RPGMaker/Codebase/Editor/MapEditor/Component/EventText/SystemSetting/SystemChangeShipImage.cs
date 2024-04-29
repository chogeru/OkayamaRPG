using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.SystemSetting
{
    public class SystemChangeShipImage : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            ret += "◆" + EditorLocalize.LocalizeText("WORD_1085")
                       + " : " +
                       VehicleIdToName(eventCommand.parameters[0])
                       + "/" +
                       CharacterAssetIdToName(eventCommand.parameters[1]);
            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;

            //乗り物ID→乗り物名
            string VehicleIdToName(string id) {
                var returnName = "";

                foreach (var vehiclesDataModel in DatabaseManagementService.LoadCharacterVehicles())
                    if (vehiclesDataModel.id == id)
                        returnName = vehiclesDataModel.name;

                return returnName;
            }

            //素材ID→素材名
            string CharacterAssetIdToName(string id) {
                var returnName = "";

                foreach (var assetManageDataModel in DatabaseManagementService.LoadWalkingCharacterAssets())
                    if (assetManageDataModel.id == id)
                        returnName = assetManageDataModel.name;

                if (returnName == "")
                {
                    foreach (var assetManageDataModel in DatabaseManagementService.LoadObjectAssets())
                        if (assetManageDataModel.id == id)
                            returnName = assetManageDataModel.name;
                }

                return returnName;
            }
        }
    }
}