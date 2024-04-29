using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Runtime.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Event
{
    public class MoveSetEventPoint : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_move_set_event_point.uxml";

        private RadioButton                 _direct_toggle;
        private Button                 _directButton;
        private VisualElement          _direction;
        private List<string>           _directionList;
        private PopupFieldBase<string> _directionPopupField;

        GenericPopupFieldBase<MapDataChoice> _provisionalMapPopupField;
        GenericPopupFieldBase<TargetCharacterChoice> _eventPopupField;
        GenericPopupFieldBase<TargetCharacterChoice> _eventPopupField2;

        private          VisualElement           _eventList;
        private          VisualElement           _eventList2;
        private          List<EventMapDataModel> _eventMapDataModels = new List<EventMapDataModel>();

        private RadioButton                 _exchange_toggle;

        private Vector2Int             _pos;
        private string                 _thisEventID;
        private RadioButton                 _variable_toggle;
        private List<string>           _variableID;
        private List<string>           _variableName;
        private PopupFieldBase<string> _variablePopupField;
        private PopupFieldBase<string> _variablePopupField2;
        private Label                  _xPos;
        private VisualElement          _xPos2;
        private Label                  _xPos3;
        private Label                  _yPos;
        private VisualElement          _yPos2;
        private Label                  _yPos3;

        private int posX;
        private int posY;

        public MoveSetEventPoint(
            VisualElement rootElement,
            List<EventDataModel> eventDataModels,
            int eventIndex,
            int eventCommandIndex
        )
            : base(rootElement, eventDataModels, eventIndex, eventCommandIndex) {
        }

        // 紐付けられているマップのid。
        private string CurrentMapId =>
            ThisEventMapId ??
            _provisionalMapPopupField?.value.MapDataModel?.id;

        // 紐付けられているマップのMapDataModel。
        private MapDataModel CurrentMapDataModel =>
            ThisEventMapId != null ?
                MapManagementService.LoadMapById(ThisEventMapId) :
                _provisionalMapPopupField?.value.MapDataModel;

        public override void Invoke() {
            var commandTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SettingUxml);
            VisualElement commandFromUxml = commandTree.CloneTree();
            EditorLocalize.LocalizeElements(commandFromUxml);
            commandFromUxml.style.flexGrow = 1;
            RootElement.Add(commandFromUxml);

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            if (EventCommand.parameters.Count == 0)
            {
                EventCommand.parameters.Add("-1"); //イベントリスト
                EventCommand.parameters.Add("0"); //指定方法選択
                EventCommand.parameters.Add("0"); //x 
                EventCommand.parameters.Add("0"); //y 
                EventCommand.parameters.Add("0"); //向き
                EventCommand.parameters.Add("-1"); //イベントリスト
                EventCommand.parameters.Add(EventDataModel.id); //自分のID
                EventCommand.parameters.Add("0"); //mapid

                EventManagementService.SaveEvent(EventDataModel);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            InitUI();
            InitDictionaries();
            SetPopupFieldToUI();
            SetEntityToUI();
            SetCallbackToUI();

            UpdateMapAndEnabledButtons();
        }

        private void UpdateMapAndEnabledButtons()
        {
            SetEnabledToButtons();
            if (IsCommonEvent())
            {
                if (CurrentMapDataModel != null)
                {
                    // 対象マップを表示する。
                    MapEditor.LaunchCommonEventEditMode(
                        CurrentMapDataModel, EventDataModel.page, notCoordinateMode: true);
                }

                Save(EventDataModel);
            }
        }

        /// <summary>
        /// 『設定開始』『プレビュー』ボタンの有効/無効を設定。
        /// </summary>
        private void SetEnabledToButtons()
        {
            // 『直接指定で』でマップが紐付けられていれば有効。
            bool isEnabled = EventCommand.parameters[1] == "0" && CurrentMapId != null;
            _directButton?.SetEnabled(isEnabled);
        }

        private void InitUI() {
            _direct_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display129");
            _variable_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display130");
            _exchange_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display131");
            _xPos = RootElement.Query<Label>("xPos");
            _yPos = RootElement.Query<Label>("yPos");
            _directButton = RootElement.Query<Button>("directButton");
            _xPos2 = RootElement.Query<VisualElement>("xPos2");
            _yPos2 = RootElement.Query<VisualElement>("yPos2");
            _xPos3 = RootElement.Query<Label>("xPos3");
            _yPos3 = RootElement.Query<Label>("yPos3");
            _eventList = RootElement.Query<VisualElement>("eventList");
            _eventList2 = RootElement.Query<VisualElement>("eventList2");
            _direction = RootElement.Query<VisualElement>("direction");
        }

        private void InitDictionaries() {
            _directionList = EditorLocalize.LocalizeTexts(new List<string>
                {"WORD_0926", "WORD_0299", "WORD_0813", "WORD_0814", "WORD_0297"});
            _variableName = new List<string>();
            _variableID = new List<string>();

            _eventMapDataModels = EventManagementService.LoadEventMap();
            EventMapDataModel eventMapDataModel = null;
            for (int i = 0; i < _eventMapDataModels.Count; i++)
                if (_eventMapDataModels[i].eventId == EventDataModel.id)
                {
                    eventMapDataModel = _eventMapDataModels[i];
                    break;
                }
            if ( eventMapDataModel != null)
            {
                EventCommand.parameters[7] = eventMapDataModel.mapId;
                EventManagementService.SaveEvent(EventDataModel);

                _pos = new Vector2Int(eventMapDataModel.x, eventMapDataModel.y);
                _thisEventID = eventMapDataModel.eventId;
            }

            var flagDataModel = DatabaseManagementService.LoadFlags();
            for (var i = 0; i < flagDataModel.variables.Count; i++)
            {
                if (flagDataModel.variables[i].name == "")
                    _variableName.Add("#" + string.Format("{0:D4}", i + 1) + " " + 
                                      EditorLocalize.LocalizeText("WORD_1518"));
                else
                    _variableName.Add("#" + string.Format("{0:D4}", i + 1) + " " + flagDataModel.variables[i].name);
                _variableID.Add(flagDataModel.variables[i].id);
            }
        }

        private void SetPopupFieldToUI() {
            AddEventPopupField();
            AddEventPopupField2();

            if (EventCommand.parameters[1] == "1")
            {
                var selectID = _variableID.IndexOf(EventCommand.parameters[2]);
                if (selectID == -1)
                {
                    selectID = 0;
                    if (_variableID.Count > 0)
                    {
                        EventCommand.parameters[2] = _variableID[0];
                    }
                }

                _variablePopupField = new PopupFieldBase<string>(_variableName, selectID);
                selectID = _variableID.IndexOf(EventCommand.parameters[3]);
                if (selectID == -1)
                {
                    selectID = 0;
                    if (_variableID.Count > 0)
                    {
                        EventCommand.parameters[3] = _variableID[0];
                    }

                }

                _variablePopupField2 = new PopupFieldBase<string>(_variableName, selectID);
            }
            else
            {
                _variablePopupField = new PopupFieldBase<string>(_variableName, 0);
                _variablePopupField2 = new PopupFieldBase<string>(_variableName, 0);
            }

            _directionPopupField = new PopupFieldBase<string>(_directionList, int.Parse(EventCommand.parameters[4]));
        }

        private void AddEventPopupField()
        {
            // 『マップ選択』(コモンイベントの場合)と『イベントリスト』のPopupField。
            int targetCharacterParameterIndex = 0;
            _provisionalMapPopupField = AddOrHideProvisionalMapAndAddTargetCharacterPopupField(
                targetCharacterParameterIndex,
                provisionalMapPopupField =>
                {
                    _eventPopupField = AddTargetCharacterPopupField(
                        _eventList,
                        targetCharacterParameterIndex,
                        forceMapId: provisionalMapPopupField?.value.MapDataModel?.id,
                        excludePlayer: true);

                    // 初期化時以外に呼ぶ。
                    if (_eventPopupField2 != null)
                    {
                        AddEventPopupField2();
                    }

                    UpdateMapAndEnabledButtons();
                });
        }

        private void AddEventPopupField2(int defaultIdParamaterIndex = 5)
        {
            _eventPopupField2 = AddTargetCharacterPopupField(
                _eventList2,
                defaultIdParamaterIndex,
                changeEvent =>
                {
                    string eventId = null;
                    Vector2Int pos = Vector2Int.zero;
                    if (changeEvent.newValue.Id == Commons.TargetType.ThisEvent.GetTargetCharacterId())
                    {
                        // 『このイベント』。
                        eventId = _thisEventID;
                        pos = _pos;
                    }
                    else
                    {
                        EventMapDataModel eventMapDataModel = null;
                        for (int i = 0; i < _eventMapDataModels.Count; i++)
                            if (_eventMapDataModels[i].eventId == changeEvent.newValue.Id)
                            {
                                eventMapDataModel = _eventMapDataModels[i];
                                break;
                            }
                        if (eventMapDataModel != null)
                        {
                            eventId = changeEvent.newValue.Id;
                            pos = new Vector2Int(eventMapDataModel.x, eventMapDataModel.y);
                        }
                    }

                    if (eventId != null)
                    {
                        EventCommand.parameters[5] = eventId;
                        EventCommand.parameters[2] = pos.x.ToString();
                        EventCommand.parameters[3] = pos.y.ToString();
                        _xPos3.text = pos.x.ToString();
                        _yPos3.text = System.Math.Abs(pos.y).ToString();
                    }

                    Save(EventDataModel);
                },
                forceMapId: _provisionalMapPopupField?.value.MapDataModel?.id,
                excludePlayer: true);
        }

        private void SetEntityToUI() {
            _xPos2.Clear();
            _xPos2.Add(_variablePopupField);
            _yPos2.Clear();
            _yPos2.Add(_variablePopupField2);

            //トグルの値によって一部初期化を変更
            if (EventCommand.parameters[1] == "0")
            {
                SetToggle(0);
                _xPos.text = EventCommand.parameters[2];
                var y = int.Parse(EventCommand.parameters[3]);
                y = y < 0 ? y * -1 : y;
                _yPos.text = y.ToString();
            }
            else if (EventCommand.parameters[1] == "1")
            {
                SetToggle(1);
            }
            else
            {
                SetToggle(2);
            }

            _direction.Clear();
            _direction.Add(_directionPopupField);
        }

        private void SetCallbackToUI() {
            PopUpCallback(2);
            PopUpCallback(3);
            
            var defaultSelect = int.Parse(EventCommand.parameters[1]);
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {_direct_toggle, _variable_toggle, _exchange_toggle},
                defaultSelect, new List<System.Action>
                {
                    //直接指定
                    () =>
                    {
                        EventCommand.parameters[1] = "0";
                        if (_xPos.text == "")
                            _xPos.text = "0";
                        if (_yPos.text == "")
                            _yPos.text = "0";
                        EventCommand.parameters[2] = _xPos.text;
                        var y = int.Parse(_yPos.text);
                        y = y > 0 ? y * -1 : y;

                        EventCommand.parameters[3] = y.ToString();
                        SetToggle(0);
                        Save(EventDataModel);
                    },
                    //変数で指定
                    () =>
                    {
                        EventCommand.parameters[1] = "1";
                        EventCommand.parameters[2] = _variableID[_variablePopupField.index];
                        EventCommand.parameters[3] = _variableID[_variableName.IndexOf(_variablePopupField2.value)];
                        _variablePopupField = new PopupFieldBase<string>(_variableName, _variablePopupField.index);
                        _variablePopupField2 = new PopupFieldBase<string>(_variableName, _variablePopupField2.index);
                        _xPos2.Clear();
                        _xPos2.Add(_variablePopupField);
                        _yPos2.Clear();
                        _yPos2.Add(_variablePopupField2);
                        PopUpCallback(2);
                        SetToggle(1);
                        Save(EventDataModel);
                    },
                    //キャラクターで指定
                    () =>
                    {
                        EventCommand.parameters[1] = "2";
                        EventCommand.parameters[2] = _pos.x.ToString();
                        EventCommand.parameters[3] = _pos.y.ToString();

                        AddEventPopupField2();

                        _xPos3.text = _pos.x.ToString();
                        var y = _pos.y;
                        y = y < 0 ? y * -1 : y;
                        _yPos3.text = y.ToString();
                        SetToggle(2);
                        Save(EventDataModel);
                    }
                });
            

            var isEdit = false;
            _directButton.text = EditorLocalize.LocalizeText("WORD_1583");
            _directButton.clicked += () =>
            {
                if (isEdit)
                {
                    _directButton.text = EditorLocalize.LocalizeText("WORD_1583");
                    EndMapPosition(CurrentMapId);
                }
                else
                {
                    _directButton.text = EditorLocalize.LocalizeText("WORD_1584");
                    SetMapPosition(CurrentMapId);
                }

                isEdit = !isEdit;
            };
        }

        private void PopUpCallback(int kind) {
            switch (kind)
            {
                case 2:
                    _variablePopupField.RegisterValueChangedCallback(evt =>
                    {
                        EventCommand.parameters[2] = _variableID[_variableName.IndexOf(_variablePopupField.value)];
                        Save(EventDataModel);
                    });
                    _variablePopupField2.RegisterValueChangedCallback(evt =>
                    {
                        EventCommand.parameters[3] = _variableID[_variableName.IndexOf(_variablePopupField2.value)];
                        Save(EventDataModel);
                    });
                    break;
                case 3:
                    _directionPopupField.RegisterValueChangedCallback(evt =>
                    {
                        EventCommand.parameters[4] = _directionPopupField.index.ToString();
                        Save(EventDataModel);
                    });
                    break;
            }
        }

        public void SetMapPosition(string mapId) {
            var mapDataModel = MapManagementService.LoadMapById(mapId) ?? MapManagementService.LoadMaps().First();

            MapEditor.LaunchCommonEventEditMode(mapDataModel, 0,
                v =>
                {
                    posX = v.x;
                    posY = v.y;
                    _xPos.text = v.x.ToString();
                    var y = v.y;
                    y = y < 0 ? y * -1 : y;
                    _yPos.text = y.ToString();
                });
        }

        public void EndMapPosition(string mapId) {
            var mapDataModel = MapManagementService.LoadMapById(mapId) ?? MapManagementService.LoadMaps().First();
            MapEditor.LaunchCommonEventEditModeEnd(mapDataModel);
            EventCommand.parameters[2] = posX.ToString();
            EventCommand.parameters[3] = posY.ToString();
            Save(EventDataModel);
        }


        private void SetToggle(int kind) {
            switch (kind)
            {
                case 0:
                    _eventList2.SetEnabled(false);
                    _xPos.SetEnabled(true);
                    _yPos.SetEnabled(true);
                    _xPos2.SetEnabled(false);
                    _yPos2.SetEnabled(false);
                    _xPos3.SetEnabled(false);
                    _yPos3.SetEnabled(false);
                    break;
                case 1:
                    _eventList2.SetEnabled(false);
                    _xPos.SetEnabled(false);
                    _yPos.SetEnabled(false);
                    _xPos2.SetEnabled(true);
                    _yPos2.SetEnabled(true);
                    _xPos3.SetEnabled(false);
                    _yPos3.SetEnabled(false);
                    break;
                case 2:
                    _eventList2.SetEnabled(true);
                    _xPos.SetEnabled(false);
                    _yPos.SetEnabled(false);
                    _xPos2.SetEnabled(false);
                    _yPos2.SetEnabled(false);
                    _xPos3.SetEnabled(true);
                    _yPos3.SetEnabled(true);
                    break;
            }

            SetEnabledToButtons();
        }
    }
}