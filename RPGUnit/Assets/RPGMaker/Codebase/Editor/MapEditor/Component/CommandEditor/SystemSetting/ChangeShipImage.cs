using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.SystemSetting
{
    public class ChangeShipImage : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_change_ship_image.uxml";


        public ChangeShipImage(
            VisualElement rootElement,
            List<EventDataModel> eventDataModels,
            int eventIndex,
            int eventCommandIndex
        )
            : base(rootElement, eventDataModels, eventIndex, eventCommandIndex) {
        }

        public override void Invoke() {
            var commandTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SettingUxml);
            VisualElement commandFromUxml = commandTree.CloneTree();
            EditorLocalize.LocalizeElements(commandFromUxml);
            commandFromUxml.style.flexGrow = 1;
            RootElement.Add(commandFromUxml);

            var vehicleDropdownChoices = DatabaseManagementService.LoadCharacterVehicles();
            var charactorDropdownChoices = new List<AssetManageDataModel>();

            var assetDataModels = DatabaseManagementService.LoadAssetManage();
            var assetManageData = new List<AssetManageDataModel>();
            var orderData = AssetManageRepository.OrderManager.Load();
            
            for (var i = 0; i < orderData.orderDataList.Length; i++)
            {
                if (orderData.orderDataList[i].idList == null)
                    continue;
                if (orderData.orderDataList[i].assetTypeId == (int) AssetCategoryEnum.OBJECT)
                    for (var i2 = 0; i2 < orderData.orderDataList[i].idList.Count; i2++)
                    {
                        AssetManageDataModel data = null;
                        for (int j = 0; j < assetDataModels.Count; j++)
                        {
                            if (assetDataModels[j].id == orderData.orderDataList[i].idList[i2])
                            {
                                data = assetDataModels[j];
                                break;
                            }
                        }
                        assetManageData.Add(data);
                    }
            }
            
            

            foreach (var dataModel in assetManageData)
            {
                if (dataModel == null) continue;
                charactorDropdownChoices.Add(dataModel);
            }

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count <= 2)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters
                    .Add(vehicleDropdownChoices.Count > 0 ? vehicleDropdownChoices[0].id : "");

                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters
                    .Add(charactorDropdownChoices.Count > 0 ? charactorDropdownChoices[0].id : "");
                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
            }
            
            if (charactorDropdownChoices.Count == 0)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "";
            }
            if (vehicleDropdownChoices.Count == 0)
            {
                VisualElement vehicleArea = RootElement.Query<VisualElement>("vehicle_area");
                vehicleArea.style.display = DisplayStyle.None;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "";
                return;
            }

            VisualElement vehicleDropdown = RootElement.Q<VisualElement>("systemSetting_changeVehiclePicture")
                .Query<VisualElement>("vehicle_dropdown");
            var vheicleListId = 0;
            for (var i = 0; i < vehicleDropdownChoices.Count; i++)
                if (vehicleDropdownChoices[i].id ==
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0])
                {
                    vheicleListId = i;
                    break;
                }

            var vehicleName = new List<string>();
            for (var i = 0; i < vehicleDropdownChoices.Count; i++) vehicleName.Add(vehicleDropdownChoices[i].name);

            var vehicleDropdownPopupField = new PopupFieldBase<string>(vehicleName, vheicleListId);
            vehicleDropdown.Clear();
            vehicleDropdown.Add(vehicleDropdownPopupField);
            vehicleDropdownPopupField.RegisterValueChangedCallback(evt =>
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                        vehicleDropdownChoices[vehicleDropdownPopupField.index].id;
                    Save(EventDataModels[EventIndex]);
                })
                ;
            VisualElement charactorDropdown = RootElement.Q<VisualElement>("systemSetting_changeVehiclePicture")
                .Query<VisualElement>("character_dropdown");

            var characterListId = 0;
            for (var i = 0; i < charactorDropdownChoices.Count; i++)
                if (charactorDropdownChoices[i].id ==
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1])
                {
                    characterListId = i;
                    break;
                }

            var characterName = new List<string>();
            for (var i = 0; i < charactorDropdownChoices.Count; i++)
                characterName.Add(charactorDropdownChoices[i].name);

            var characterDropdownPopupField = new PopupFieldBase<string>(characterName, characterListId);
            if (charactorDropdownChoices.Count > 0)
            {
                ChangeVehiclePreview(charactorDropdownChoices[characterListId].id);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    charactorDropdownChoices[characterDropdownPopupField.index].id;
            }

            charactorDropdown.Clear();
            charactorDropdown.Add(characterDropdownPopupField);
            characterDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    charactorDropdownChoices[characterDropdownPopupField.index].id;
                //画像の更新
                ChangeVehiclePreview(charactorDropdownChoices[characterDropdownPopupField.index].id);
                Save(EventDataModels[EventIndex]);
            });
        }

        private void ChangeVehiclePreview(string path) {
            Image preview = RootElement.Q<VisualElement>("systemSetting_changeVehiclePicture").Query<Image>("preview");
            preview.scaleMode = ScaleMode.ScaleToFit;
            preview.image = ImageManager.LoadSvCharacter(path);
        }
    }
}