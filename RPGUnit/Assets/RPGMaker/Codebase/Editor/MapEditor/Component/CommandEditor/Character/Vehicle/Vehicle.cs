using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Character.Vehicle
{
    public class Vehicle : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_vehicle.uxml";

        private EventMapDataModel _eventMapDataModel;

        private MapDataModel _mapDataModel;
        private Label        _xPos;
        private Label        _yPos;

        private int posX;
        private int posY;

        public Vehicle(
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

            var MapEntities = new List<MapDataModel>();
            MapEntities = MapManagementService.LoadMaps();

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            var num = 0;

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count == 0)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("1");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters
                    .Add(MapEntities.Count > 0 ? MapEntities[0].id : "-1");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }
            


            var flagDataModel =
                DatabaseManagementService.LoadFlags();
            var variableID = new List<string>();
            var variableName = new List<string>();
            for (var i = 0; i < flagDataModel.variables.Count; i++)
            {
                variableID.Add(flagDataModel.variables[i].id);
                if (flagDataModel.variables[i].name == "")
                    variableName.Add("#" + string.Format("{0:D4}", i + 1) + " " +
                                     EditorLocalize.LocalizeText("WORD_1518"));
                else
                    variableName.Add("#" + string.Format("{0:D4}", i + 1) + " " + flagDataModel.variables[i].name);
            }


            var vehiclesDataModels =
                DatabaseManagementService.LoadCharacterVehicles();
            var vehicleDropdownChoices = new List<string>();
            var vehicleNameDropdownChoices = new List<string>();
            for (var i = 0; i < vehiclesDataModels.Count; i++)
            {
                vehicleDropdownChoices.Add(vehiclesDataModels[i].id);
                vehicleNameDropdownChoices.Add(vehiclesDataModels[i].name);
            }
            
            if (vehicleDropdownChoices != null && vehicleDropdownChoices.Count == 0)
            {
                VisualElement vehicleArea = RootElement.Query<VisualElement>("vehicle_area");
                vehicleArea.style.display = DisplayStyle.None;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "";
                return;
            }

            var eventMapDataModels =
                EventManagementService.LoadEventMap();
            var eventCommonDataModels =
                EventManagementService.LoadEventCommon();
            var currentEventMapIndex = -1;
            Type = EventType.Map;
            for (var i = 0; i < eventMapDataModels.Count; i++)
                if (eventMapDataModels[i].eventId == EventDataModels[EventIndex].id)
                {
                    _eventMapDataModel = eventMapDataModels[i];
                    currentEventMapIndex = i;
                    break;
                }

            if (currentEventMapIndex == -1)
            {
                Type = EventType.Common;
                for (var i = 0; i < eventCommonDataModels.Count; i++)
                    if (eventCommonDataModels[i].eventId == EventDataModels[EventIndex].id)
                    {
                        _mapDataModel = MapEntities.FirstOrDefault(c =>
                            c.id == EventDataModels[EventIndex].eventCommands[EventCommandIndex]
                                .parameters[2]);
                        currentEventMapIndex = i;
                        break;
                    }
            }

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] != null)
                num = vehicleDropdownChoices.IndexOf(EventDataModels[EventIndex]
                    .eventCommands[EventCommandIndex]
                    .parameters[0]);
            if (num == -1)
                num = 0;
            EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                vehicleDropdownChoices[num];
            VisualElement command_vehicle = RootElement.Query<VisualElement>("vehicle");
            var command_vehiclePopupField = new PopupFieldBase<string>(vehicleNameDropdownChoices, num);
            command_vehicle.Clear();
            command_vehicle.Add(command_vehiclePopupField);
            command_vehiclePopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                    vehicleDropdownChoices[command_vehiclePopupField.index];
                Save(EventDataModels[EventIndex]);
            });


            VisualElement mapSelect = RootElement.Query<VisualElement>("mapSelect");
            VisualElement mapIDSelect = RootElement.Query<VisualElement>("mapIDSelect");
            VisualElement x_select = RootElement.Query<VisualElement>("x_select");
            VisualElement y_select = RootElement.Query<VisualElement>("y_select");
            RadioButton direct_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display15");
            RadioButton variable_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display16");
            _xPos = RootElement.Query<Label>("xPos");
            _yPos = RootElement.Query<Label>("yPos");
            var mapNameList = new List<string>();
            var mapIDList = new List<string>();
            var selectID = 0;

            for (var j = 0; j < MapEntities.Count; j++)
            {
                mapNameList.Add(MapEntities[j].name);
                mapIDList.Add(MapEntities[j].id);
                if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] ==
                    MapEntities[j].id)
                {
                    _mapDataModel = MapEntities[j];
                    selectID = j;
                }
            }

            if (mapNameList.Count == 0)
            {
                mapNameList.Add(EditorLocalize.LocalizeText("WORD_0113"));
                mapIDList.Add("-1");
            }

            Button directButton = RootElement.Query<Button>("direct_button");
            var isEdit = false;
            directButton.style.display = DisplayStyle.None;
            directButton.text = EditorLocalize.LocalizeText("WORD_1583");

            directButton.clickable.clicked += () =>
            {
                if (isEdit)
                {
                    directButton.text = EditorLocalize.LocalizeText("WORD_1583");
                    EndMapPosition();
                }
                else
                {
                    directButton.text = EditorLocalize.LocalizeText("WORD_1584");
                    SetMapPosition();
                }

                isEdit = !isEdit;
            };
            
            directButton.SetEnabled(MapEntities.Count > 0);

            ImTextField mapName = RootElement.Query<ImTextField>("mapName");


            var mapSelectPopupField = new PopupFieldBase<string>(mapNameList, selectID);
            mapSelect.Clear();
            mapSelect.Add(mapSelectPopupField);
            mapSelectPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] =
                    mapIDList[mapSelectPopupField.index];

                mapName.SetEnabled(false);
                mapName.value = mapSelectPopupField.value;
                Save(EventDataModels[EventIndex]);

                _mapDataModel = MapEntities[mapSelectPopupField.index];
            });

            mapName.SetEnabled(false);
            mapName.value = mapSelectPopupField.value;

            var mapIDSelectPopupField = new PopupFieldBase<string>(mapNameList, selectID);
            mapIDSelect.Clear();
            mapIDSelect.Add(mapIDSelectPopupField);
            mapIDSelectPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] =
                    mapIDList[mapIDSelectPopupField.index];

                mapName.value = mapIDSelectPopupField.value;
                Save(EventDataModels[EventIndex]);
                for (int i = 0; i < MapEntities.Count; i++)
                    if (MapEntities[i].id == mapIDList[mapIDSelectPopupField.index])
                    {
                        _mapDataModel = MapEntities[i];
                        break;
                    }
            });


            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] == "1")
                num = variableID.IndexOf(EventDataModels[EventIndex]
                    .eventCommands[EventCommandIndex]
                    .parameters[3]);
            if (num == -1)
                num = 0;
            var x_selectPopupField = new PopupFieldBase<string>(variableName, num);
            x_select.Clear();
            x_select.Add(x_selectPopupField);
            x_selectPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] =
                    variableID[x_selectPopupField.index];
                Save(EventDataModels[EventIndex]);
            });


            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] == "1")
                num = variableID.IndexOf(EventDataModels[EventIndex]
                    .eventCommands[EventCommandIndex]
                    .parameters[4]);
            if (num == -1)
                num = 0;
            var y_selectPopupField = new PopupFieldBase<string>(variableName, num);
            y_select.Clear();
            y_select.Add(y_selectPopupField);
            y_selectPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] =
                    variableID[y_selectPopupField.index];
                Save(EventDataModels[EventIndex]);
            });


            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] == "0")
            {
                direct_toggle.value = true;
                mapSelect.SetEnabled(true);
                mapIDSelect.SetEnabled(false);
                x_select.SetEnabled(false);
                y_select.SetEnabled(false);
            }
            else
            {
                variable_toggle.value = true;
                mapSelect.SetEnabled(false);
                mapIDSelect.SetEnabled(true);
                x_select.SetEnabled(true);
                y_select.SetEnabled(true);
            }
            
            var defaultSelect = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {direct_toggle, variable_toggle},
                defaultSelect, new List<System.Action>
                {
                    //直接指定トグル。
                    () =>
                    {
                        directButton.style.display = DisplayStyle.Flex;
                        variable_toggle.value = false;
                        mapSelect.SetEnabled(true);
                        mapIDSelect.SetEnabled(false);
                        x_select.SetEnabled(false);
                        y_select.SetEnabled(false);
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "0";
                        if (!int.TryParse(
                            EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3],
                            out num))
                            num = 0;
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] =
                            num.ToString();
                        if (!int.TryParse(
                            EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4],
                            out num))
                            num = 0;
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] =
                            num.ToString();
                        _xPos.text = "[" + EventDataModels[EventIndex].eventCommands[EventCommandIndex]
                            .parameters[3] + "]";

                        var y = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4]);
                        y = y < 0 ? y * -1 : y;

                        _yPos.text = "[" + y + "]";

                        _mapDataModel = MapEntities.Count > mapSelectPopupField.index ? MapEntities[mapSelectPopupField.index] : null;

                        Save(EventDataModels[EventIndex]);
                    },
                    //変数指定トグル。
                    () =>
                    {
                        directButton.style.display = DisplayStyle.None;
                        direct_toggle.value = false;
                        mapSelect.SetEnabled(false);
                        mapIDSelect.SetEnabled(true);
                        x_select.SetEnabled(true);
                        y_select.SetEnabled(true);
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "1";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] =
                            variableID[x_selectPopupField.index];
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] =
                            variableID[y_selectPopupField.index];
                        Save(EventDataModels[EventIndex]);

                    }
                });
            Save(EventDataModels[EventIndex]);
        }


        public void SetMapPosition() {
            _mapDataModel ??= MapManagementService.LoadMaps().First();
            MapEditor.LaunchCommonEventEditMode(_mapDataModel, EventDataModels[EventIndex].page,
                v =>
                {
                    posX = v.x;
                    posY = v.y;

                    _xPos.text = "[" + v.x + "]";
                    var y = v.y < 0 ? v.y * -1 : v.y;
                    _yPos.text = "[" + y + "]";
                });
        }

        public void EndMapPosition() {
            _mapDataModel ??= MapManagementService.LoadMaps().First();
            MapEditor.LaunchCommonEventEditModeEnd(_mapDataModel);
            EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] =
                posX.ToString();
            EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] =
                posY.ToString();
            Save(EventDataModels[EventIndex]);
        }
    }
}