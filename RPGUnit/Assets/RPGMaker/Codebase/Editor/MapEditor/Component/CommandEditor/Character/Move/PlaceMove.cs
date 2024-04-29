#define IN_PROCESS_BEING_MODIFIED

using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Character.Move
{
    /// <summary>
    ///     場所移動
    /// </summary>
    public class PlaceMove : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_place_move.uxml";

        private EventMapDataModel _eventMapDataModel;

        // 移動先。
        private MapDataModel _mapDataModel;
        private Label        _xPos;
        private Label        _yPos;

        private int posX;
        private int posY;

        public PlaceMove(
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

            var MapEntities = MapManagementService.LoadMaps();

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            var num = 0;

            if (EventCommand.parameters.Count == 0)
            {
                EventCommand.parameters.Add("0");
                EventCommand.parameters.Add(MapEntities.Count > 0 ? MapEntities[0].name : "");
                EventCommand.parameters.Add(MapEntities.Count > 0 ? MapEntities[0].id : "-1");
                EventCommand.parameters.Add("0");
                EventCommand.parameters.Add("0");
                EventCommand.parameters.Add("0");
                EventCommand.parameters.Add("0");
                EventCommand.parameters.Add(MapEntities.Count > 0 ? MapEntities[0].id : "-1");
                Save(EventDataModel);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }
            

            var flagDataModel = DatabaseManagementService.LoadFlags();
            var variableNameList = new List<string>();
            var variableIDList = new List<string>();
            for (var i = 0; i < flagDataModel.variables.Count; i++)
            {
                if (flagDataModel.variables[i].name == "")
                    variableNameList.Add(EditorLocalize.LocalizeText("WORD_1518"));
                else
                    variableNameList.Add(flagDataModel.variables[i].name);
                variableIDList.Add(flagDataModel.variables[i].id);
            }

            // 通常イベント。
            var currentEventMapIndex = -1;
            var eventMapDataModels = EventManagementService.LoadEventMap();
            for (var i = 0; i < eventMapDataModels.Count; i++)
                if (eventMapDataModels[i].eventId == EventDataModel.id)
                {
                    _eventMapDataModel = eventMapDataModels[i];
                    _mapDataModel = MapEntities.FirstOrDefault(c => c.id == EventCommand.parameters[7]);
                    currentEventMapIndex = i;
                    break;
                }

            // コモンイベント。
            if (currentEventMapIndex == -1)
            {
                Type = EventType.Common;
                var eventCommonDataModels = EventManagementService.LoadEventCommon();
                for (var i = 0; i < eventCommonDataModels.Count; i++)
                    if (eventCommonDataModels[i].eventId == EventDataModel.id)
                    {
                        _mapDataModel = MapEntities.FirstOrDefault(c => c.id == EventCommand.parameters[7]);
                        currentEventMapIndex = i;
                        break;
                    }
            }


            VisualElement map_select = RootElement.Query<VisualElement>("mapIDSelect");
            VisualElement x_select = RootElement.Query<VisualElement>("x_select");
            VisualElement y_select = RootElement.Query<VisualElement>("y_select");
            RadioButton direct_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display9");
            RadioButton variable_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display10");
            ImTextField mapName = RootElement.Query<ImTextField>("mapName");

            _xPos = RootElement.Query<Label>("xPos");
            _yPos = RootElement.Query<Label>("yPos");

            // 『設定開始』ボタン。
            Button directButton = RootElement.Query<Button>("direct_button");
            directButton.SetEnabled(false);
            var isEdit = false;
            directButton.text = EditorLocalize.LocalizeText("WORD_1583");
            directButton.clickable.clicked += () =>
            {
                if (isEdit)
                {
                    // 設定終了へ。
                    directButton.text = EditorLocalize.LocalizeText("WORD_1583");
                    EndMapPosition();
                }
                else
                {
                    // 設定開始へ。
                    directButton.text = EditorLocalize.LocalizeText("WORD_1584");
                    SetMapPosition();
                }

                isEdit = !isEdit;
            };

            // マップ指定。
            var mapSelectPopupField = GenericPopupFieldBase<MapDataChoice>.Add(
                RootElement,
                "mapSelect",
                MapDataChoice.GenerateChoices(),
                EventCommand.parameters[7]);
                
            // mapSelectPopupFieldの選択項目変更時の処理。
            mapSelectPopupField.RegisterValueChangedCallback(
                (changeEvent) =>
                {
                    if (EventCommand.parameters[0] == "0")
                    {
                        var choice = changeEvent.newValue;

                        EventCommand.parameters[2] = choice.Id;
                        EventCommand.parameters[1] = choice.Name;

                        EventCommand.parameters[7] = choice.Id;

                        EventCommand.parameters[0] = "0";

                        int.TryParse(EventCommand.parameters[3], out int parameter3IntValue);
                        EventCommand.parameters[3] = parameter3IntValue.ToString();

                        int.TryParse(EventCommand.parameters[4], out int parameter4IntValue);
                        EventCommand.parameters[4] = parameter4IntValue.ToString();

                        _xPos.text = $"[{parameter3IntValue}]";
                        _yPos.text = $"[{System.Math.Abs(parameter4IntValue)}]";

                        mapName.value = choice.Name;
                        Save(EventDataModel);
                        _mapDataModel = choice.MapDataModel;

                        directButton.SetEnabled(choice.MapDataModel != null);
                    }
                });

            directButton.SetEnabled(mapSelectPopupField.value.MapDataModel != null);

            

            // マップID。
            if (EventCommand.parameters[2] != null)
                num = variableIDList.IndexOf(EventCommand.parameters[2]);
            if (num == -1)
                num = 0;
            var mapIDSelectPopupField = new PopupFieldBase<string>(variableNameList, num);
            map_select.Clear();
            map_select.Add(mapIDSelectPopupField);
            mapIDSelectPopupField.RegisterValueChangedCallback(evt =>
            {
                if (EventCommand.parameters[0] == "1")
                {
                    EventCommand.parameters[2] = variableIDList[mapIDSelectPopupField.index];
                    Save(EventDataModel);
                }
            });

            if (EventCommand.parameters[3] != null)
                num = variableIDList.IndexOf(EventCommand.parameters[3]);
            if (num == -1)
                num = 0;
            var x_selectPopupField = new PopupFieldBase<string>(variableNameList, num);
            x_select.Clear();
            x_select.Add(x_selectPopupField);
            x_selectPopupField.RegisterValueChangedCallback(evt =>
            {
                if (EventCommand.parameters[0] == "1")
                {
                    EventCommand.parameters[3] = variableIDList[x_selectPopupField.index];
                    Save(EventDataModel);
                }
            });


            if (EventCommand.parameters[4] != null)
                num = variableIDList.IndexOf(EventDataModel.eventCommands[EventCommandIndex].parameters[4]);
            if (num == -1)
                num = 0;
            var y_selectPopupField = new PopupFieldBase<string>(variableNameList, num);
            y_select.Clear();
            y_select.Add(y_selectPopupField);
            y_selectPopupField.RegisterValueChangedCallback(evt =>
            {
                if (EventCommand.parameters[0] == "1")
                {
                    EventCommand.parameters[4] = variableIDList[y_selectPopupField.index];
                    Save(EventDataModel);
                }
            });


            VisualElement mapSelectDirection = RootElement.Query<VisualElement>("mapSelect");
            VisualElement mapSelectVariables = RootElement.Query<VisualElement>("mapIDSelect");

            if (EventCommand.parameters[0] == "0")
            {
                direct_toggle.value = true;
                directButton.SetEnabled(true);
                mapSelectPopupField.SetEnabled(true);
                mapIDSelectPopupField.SetEnabled(false);
                x_select.SetEnabled(false);
                y_select.SetEnabled(false);

                mapSelectDirection.SetEnabled(true);
                mapSelectVariables.SetEnabled(false);
            }
            else
            {
                variable_toggle.value = true;
                directButton.SetEnabled(false);
                mapSelectPopupField.SetEnabled(false);
                mapIDSelectPopupField.SetEnabled(true);
                x_select.SetEnabled(true);
                y_select.SetEnabled(true);

                mapSelectDirection.SetEnabled(false);
                mapSelectVariables.SetEnabled(true);
            }
            
            var defaultSelect = EventCommand.parameters[0] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {direct_toggle, variable_toggle},
                defaultSelect, new List<System.Action>
                {
                    //直接指定トグル。
                    () =>
                    {
                        directButton.SetEnabled(true);
                        var num = 0;
                        variable_toggle.value = false;
                        mapSelectPopupField.SetEnabled(true);
                        mapIDSelectPopupField.SetEnabled(false);
                        x_select.SetEnabled(false);
                        y_select.SetEnabled(false);

                        mapSelectDirection.SetEnabled(true);
                        mapSelectVariables.SetEnabled(false);

                        EventCommand.parameters[0] = "0";
                        EventCommand.parameters[2] = EventCommand.parameters[7];

                        try
                        {
                            num = int.Parse(EventCommand.parameters[3]);
                        }
                        catch (Exception)
                        {
                            num = 0;
                        }

                        EventCommand.parameters[3] = num.ToString();

                        try
                        {
                            num = int.Parse(EventCommand.parameters[4]);
                        }
                        catch (Exception)
                        {
                            num = 0;
                        }

                        EventCommand.parameters[4] = num.ToString();

                        _xPos.text = "[" + EventCommand.parameters[3] + "]";
                        var y = int.Parse(EventCommand.parameters[4]) < 0
                            ? int.Parse(EventCommand.parameters[4]) * -1
                            : int.Parse(EventCommand.parameters[4]);
                        _yPos.text = "[" + y + "]";
                        for (int i = 0; i < MapEntities.Count; i++)
                            if (MapEntities[i].id == EventCommand.parameters[7])
                            {
                                _mapDataModel = MapEntities[i];
                                break;
                            }

                        mapName.value = mapSelectPopupField.value.Name;

                        Save(EventDataModel);
                    },
                    //変数指定トグル。
                    () =>
                    {
                        directButton.SetEnabled(false);
                        direct_toggle.value = false;
                        mapSelectPopupField.SetEnabled(false);
                        mapIDSelectPopupField.SetEnabled(true);
                        x_select.SetEnabled(true);
                        y_select.SetEnabled(true);

                        mapSelectDirection.SetEnabled(false);
                        mapSelectVariables.SetEnabled(true);

                        EventCommand.parameters[0] = "1";
                        EventCommand.parameters[2] = variableIDList[mapIDSelectPopupField.index];
                        EventCommand.parameters[3] = variableIDList[x_selectPopupField.index];
                        EventCommand.parameters[4] = variableIDList[y_selectPopupField.index];
                        MapEditor.WhenEventClosed();

                        mapName.value = "";

                        Save(EventDataModel);
                    }
                });

            mapName.SetEnabled(false);
            mapName.value = mapSelectPopupField.value.Name;
            
            VisualElement direction = RootElement.Query<VisualElement>("direction");
            var directionNameList =
                EditorLocalize.LocalizeTexts(new List<string>
                    {"WORD_0926", "WORD_0815", "WORD_0813", "WORD_0814", "WORD_0812"});
            var selectID = int.Parse(EventCommand.parameters[5]);
            if (selectID == -1)
                selectID = 0;
            var directionPopupField = new PopupFieldBase<string>(directionNameList, selectID);
            direction.Clear();
            direction.Add(directionPopupField);
            directionPopupField.RegisterValueChangedCallback(evt =>
            {
                EventCommand.parameters[5] = directionNameList.IndexOf(directionPopupField.value).ToString();
                Save(EventDataModel);
            });

            VisualElement feed = RootElement.Query<VisualElement>("feed");
            var feedNameList = EditorLocalize.LocalizeTexts(new List<string> {"WORD_0998", "WORD_0999", "WORD_0113"});
            selectID = int.Parse(EventCommand.parameters[6]);
            if (selectID == -1)
                selectID = 0;
            var feedPopupField = new PopupFieldBase<string>(feedNameList, selectID);
            feed.Clear();
            feed.Add(feedPopupField);
            feedPopupField.RegisterValueChangedCallback(evt =>
            {
                EventCommand.parameters[6] = feedNameList.IndexOf(feedPopupField.value).ToString();
                Save(EventDataModel);
            });
            
            //マップのデータがなければ設定開始は押せないようにする
            directButton.SetEnabled(_mapDataModel != null);

        }

        public void SetMapPosition() {
            _mapDataModel ??= MapManagementService.LoadMaps().First();

            MapEditor.LaunchCommonEventEditMode(_mapDataModel, 0,
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
            EventCommand.parameters[3] = posX.ToString();
            EventCommand.parameters[4] = posY.ToString();
            Save(EventDataModel);
        }
    }
}