using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.AddonUIUtil;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.View.Preview;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using RPGMaker.Codebase.Runtime.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.FlowControl
{
    public class CustomMove : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_custom_move.uxml";

        private List<Vector2Int> _posList = new List<Vector2Int>();

        private Vector2 _nowPos = Vector2.one;


        private SceneWindow _sceneWindow;

        public CustomMove(
            VisualElement rootElement,
            List<EventDataModel> eventDataModels,
            int eventIndex,
            int eventDataIndex
        )
            : base(rootElement, eventDataModels, eventIndex, eventDataIndex) {
        }

        public override void Invoke() {
            var commandTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SettingUxml);
            VisualElement commandFromUxml = commandTree.CloneTree();
            EditorLocalize.LocalizeElements(commandFromUxml);
            commandFromUxml.style.flexGrow = 1;
            RootElement.Add(commandFromUxml);

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count == 0)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("1");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("-2");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("1");
                var data = EventDataModels[EventIndex].eventCommands.ToList();
                var eventDatas = new List<EventDataModel.EventCommand>();
                eventDatas.Add(new EventDataModel.EventCommand(0, new List<string>(),
                    new List<EventDataModel.EventCommandMoveRoute>()));
                eventDatas.Add(new EventDataModel.EventCommand(0, new List<string>(),
                    new List<EventDataModel.EventCommandMoveRoute>()));
                SetEventData(ref eventDatas, 0, 0);
                SetEventData(ref eventDatas, 1, (int) EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE_END);
                for (var i = 0; i < eventDatas.Count; i++)
                    data.Insert(EventCommandIndex + i + 1, eventDatas[i]);
                EventDataModels[EventIndex].eventCommands = data;

                Save(EventDataModels[EventIndex]);
            }

            VisualElement mapSelect = RootElement.Query<VisualElement>("mapSelect");
            string mapId = null;
            if (!IsCommonEvent())
            {
                mapSelect.parent.style.display = DisplayStyle.None;
                mapId = GetEventMapDataModelByEventId(EventDataModels[EventIndex].id)?.mapId;
            } else
            {
                mapId = GetEventMapDataModelByEventId(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1])?.mapId;
                var mapDataChoices = MapDataChoice.GenerateChoices();
                var nameList = new List<string>();
                var defaultIndex = 0;
                var index = 0;
                foreach (var mapDataChoice in mapDataChoices)
                {
                    nameList.Add(mapDataChoice.Name);
                    if (mapDataChoice.Id == mapId) defaultIndex = index;
                    index++;
                }

                var popupField = new PopupFieldBase<string>(nameList, defaultIndex);
                popupField.RegisterValueChangedCallback(evt =>
                {
                    var popupField = evt.currentTarget as PopupFieldBase<string>;
                    mapId = mapDataChoices[popupField.index].Id;

                    UpdateEventList(mapId);
                });
                popupField.style.flexGrow = 1;
                AddonUIUtil.AddStyleEllipsisToPopupField(popupField);
                mapSelect.Add(popupField);
            }

            UpdateEventList(mapId);

            Button previewButton = RootElement.Query<Button>("preview_button");

            //プレビューボタン
            previewButton.clickable.clicked += () => { Preview(); };
            var isCommonEvent = false;
            foreach (var eventCommonDataModel in EventManagementService.LoadEventCommon())
            {
                if (eventCommonDataModel.eventId == EventDataModels[EventIndex].id)
                {
                    isCommonEvent = true;
                    break;
                }
            }
            previewButton.SetEnabled(!isCommonEvent);    // コモンイベントでなければプレビューボタンを有効化。

            //動作を繰り返す
            Toggle repeatOperation_toggle = RootElement.Query<Toggle>("repeatOperation_toggle");
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] == "1")
                repeatOperation_toggle.value = true;
            repeatOperation_toggle.RegisterValueChangedCallback(evt =>
            {
                var num = 0;
                if (repeatOperation_toggle.value)
                    num = 1;

                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] =
                    num.ToString();
                Save(EventDataModels[EventIndex]);
            });
            //移動できないときは飛ばす
            Toggle moveSkip_toggle = RootElement.Query<Toggle>("moveSkip_toggle");
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] == "1")
                moveSkip_toggle.value = true;
            moveSkip_toggle.RegisterValueChangedCallback(evt =>
            {
                var num = 0;
                if (moveSkip_toggle.value)
                    num = 1;

                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] =
                    num.ToString();
                Save(EventDataModels[EventIndex]);
            });
            //ウエイト
            Toggle waitComplete_toggle = RootElement.Query<Toggle>("waitComplete_toggle");
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] == "1")
                waitComplete_toggle.value = true;
            waitComplete_toggle.RegisterValueChangedCallback(evt =>
            {
                var num = 0;
                if (waitComplete_toggle.value)
                    num = 1;

                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] =
                    num.ToString();
                Save(EventDataModels[EventIndex]);
            });

            EventManagementService.SaveEvent(EventDataModels[EventIndex]);
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
        }

        private void UpdateEventList(string mapId) {
            var eventId = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1];
            if (eventId != "-2" && eventId != "-1")
            {
                var eventMapId = GetEventMapDataModelByEventId(eventId)?.mapId;
                if (eventMapId != mapId)
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "-2";
                    Save(EventDataModels[EventIndex]);
                }
            }
            VisualElement eventSelect = RootElement.Query<VisualElement>("eventSelect");
            eventSelect.Clear();
            var eventIDList = new List<string> { "-2", "-1" };
            var eventNameList = EditorLocalize.LocalizeTexts(new List<string> { "WORD_0860", "WORD_0920" });
            int num = 0;

            // マップ内のイベントリスト
            var eventMapEntities = GetEventMapDataModelsInMap(mapId);
            for (var i = 0; i < eventMapEntities.Count; i++)
            {
                eventNameList.Add(GetEventDisplayName(eventMapEntities[i]));
                eventIDList.Add(eventMapEntities[i].eventId);
            }

            num = eventIDList.IndexOf(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]);
            if (num == -1)
                num = 0;
            var eventSelectPopupField = new PopupFieldBase<string>(eventNameList, num);
            eventSelect.Add(eventSelectPopupField);
            eventSelectPopupField.RegisterValueChangedCallback(evt =>
            {
                if (eventSelectPopupField.index == 0)
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "-2";
                else if (eventSelectPopupField.index == 1)
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "-1";
                else
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                        eventIDList[eventSelectPopupField.index];

                Save(EventDataModels[EventIndex]);
            });
        }

        private void SetEventData(ref List<EventDataModel.EventCommand> dataList, int index, int code) {
            dataList[index].code = code;
            dataList[index].parameters = new List<string>();
        }

        private bool IsSameEvent(string eventID, string customMoveEventId) {
            if (eventID == "-1" || eventID == customMoveEventId) return true;
            return false;
        }

        private Commons.Direction.Id GetDirectionId(Vector2Int direction) {
            if (direction.y < 0) return Commons.Direction.Id.Down;
            if (direction.x < 0) return Commons.Direction.Id.Left;
            if (direction.x > 0) return Commons.Direction.Id.Right;
            return Commons.Direction.Id.Up;
        }

        int CountLimit = 600;
        private void MakePosList() {
            _posList.Clear();
            var eventCommands = EventDataModels[EventIndex].eventCommands;
            var indent = eventCommands[EventCommandIndex].indent;
            var customMoveEventId = (eventCommands[EventCommandIndex].parameters[1] == "-1") ? EventDataModels[EventIndex].id : eventCommands[EventCommandIndex].parameters[1];
            var direction = new Vector2Int(0, -1);  //下向き
            var pos = new Vector2Int((int)_nowPos.x, (int) _nowPos.y);
            var waitMarker = new Vector2Int((int) CustomMovePreview.PosId.Wait, 1);
            Vector2Int v;
            _posList.Add(new Vector2Int((int)CustomMovePreview.PosId.TurnAndNext, (int) Commons.Direction.Id.Down));
            _posList.Add(pos);
            for (int i = EventCommandIndex + 1; i < eventCommands.Count; i++)
            {
                var eventCommand = eventCommands[i];
                if (eventCommand.indent == indent && eventCommand.code == (int) EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE_END)
                {
                    break;
                }
                if (eventCommand.indent != indent + 1) {
                    continue;
                }
                switch (eventCommand.code)
                {
                    case (int) EventEnum.EVENT_CODE_STEP_MOVE:
                        switch (int.Parse(eventCommand.parameters[0]))
                        {
                            case 0://下に移動
                                direction = new Vector2Int(0, -1);
                                pos = pos + direction;
                                _posList.Add(new Vector2Int((int) CustomMovePreview.PosId.TurnAndNext, (int) GetDirectionId(direction)));
                                _posList.Add(pos);
                                break;
                            case 1://左に移動
                                direction = new Vector2Int(-1, 0);
                                pos = pos + direction;
                                _posList.Add(new Vector2Int((int) CustomMovePreview.PosId.TurnAndNext, (int) GetDirectionId(direction)));
                                _posList.Add(pos);
                                break;
                            case 2://右に移動
                                direction = new Vector2Int(1, 0);
                                pos = pos + direction;
                                _posList.Add(new Vector2Int((int) CustomMovePreview.PosId.TurnAndNext, (int) GetDirectionId(direction)));
                                _posList.Add(pos);
                                break;
                            case 3://上に移動
                                direction = new Vector2Int(0, 1);
                                pos = pos + direction;
                                _posList.Add(new Vector2Int((int) CustomMovePreview.PosId.TurnAndNext, (int) GetDirectionId(direction)));
                                _posList.Add(pos);
                                break;
                            case 4://左下に移動
                                v = new Vector2Int(-1, -1);
                                pos = pos + v;
                                if (direction.x * v.x + direction.y * v.y <= 0)
                                {
                                    direction = -direction;
                                    _posList.Add(new Vector2Int((int) CustomMovePreview.PosId.TurnAndNext, (int) GetDirectionId(direction)));
                                }
                                _posList.Add(pos);
                                break;
                            case 5://右下に移動
                                v = new Vector2Int(1, -1);
                                pos = pos + v;
                                if (direction.x * v.x + direction.y * v.y <= 0)
                                {
                                    direction = -direction;
                                    _posList.Add(new Vector2Int((int) CustomMovePreview.PosId.TurnAndNext, (int) GetDirectionId(direction)));
                                }
                                _posList.Add(pos);
                                break;
                            case 6://左上に移動
                                v = new Vector2Int(-1, 1);
                                pos = pos + v;
                                if (direction.x * v.x + direction.y * v.y <= 0)
                                {
                                    direction = -direction;
                                    _posList.Add(new Vector2Int((int) CustomMovePreview.PosId.TurnAndNext, (int) GetDirectionId(direction)));
                                }
                                _posList.Add(pos);
                                break;
                            case 7://右上に移動
                                v = new Vector2Int(1, 1);
                                pos = pos + v;
                                if (direction.x * v.x + direction.y * v.y <= 0)
                                {
                                    direction = -direction;
                                    _posList.Add(new Vector2Int((int) CustomMovePreview.PosId.TurnAndNext, (int) GetDirectionId(direction)));
                                }
                                _posList.Add(pos);
                                break;
                            case 8://ランダムに移動
                            case 9://プレイヤーに近づく
                            case 10://プレイヤーから遠ざかる
                                _posList.Add(waitMarker);
                                break;
                            case 11://ジャンプ
                                pos = new Vector2Int(int.Parse(eventCommand.parameters[1]), -int.Parse(eventCommand.parameters[2]));
                                _posList.Add(pos);
                                break;
                        }
                        break;
                    case (int) EventEnum.MOVEMENT_ONE_STEP_FORWARD:
                        do
                        {
                            pos = pos + direction;
                            _posList.Add(pos);
                        } while (eventCommand.parameters[0] == "1" && _posList.Count < CountLimit);
                        break;
                    case (int) EventEnum.MOVEMENT_ONE_STEP_BACKWARD:
                        do
                        {
                            pos = pos - direction;
                            _posList.Add(pos);
                        } while (eventCommand.parameters[0] == "1" && _posList.Count < CountLimit);
                        break;
                    case (int) EventEnum.MOVEMENT_JUMP:
                        if (IsSameEvent(eventCommand.parameters[0], customMoveEventId))
                        {
                            do
                            {
                                if (eventCommand.parameters[1] == "1")
                                {
                                    //その場
                                } else
                                {
                                    pos = pos + new Vector2Int(int.Parse(eventCommand.parameters[10]), int.Parse(eventCommand.parameters[11]));
                                }
                                _posList.Add(pos);
                            } while (eventCommand.parameters[6] == "1" && _posList.Count < CountLimit);
                        }
                        else
                        {
                            _posList.Add(waitMarker);
                        }
                        break;
                    case (int) EventEnum.MOVEMENT_TURN_DOWN:
                        if (IsSameEvent(eventCommand.parameters[0], customMoveEventId))
                        {
                            direction = new Vector2Int(0, -1);
                            _posList.Add(new Vector2Int((int) CustomMovePreview.PosId.Turn, (int) GetDirectionId(direction)));
                        }
                        else
                        {
                            _posList.Add(waitMarker);
                        }
                        break;
                    case (int) EventEnum.MOVEMENT_TURN_LEFT:
                        if (IsSameEvent(eventCommand.parameters[0], customMoveEventId))
                        {
                            direction = new Vector2Int(-1, 0);
                            _posList.Add(new Vector2Int((int) CustomMovePreview.PosId.Turn, (int) GetDirectionId(direction)));
                        }
                        else
                        {
                            _posList.Add(waitMarker);
                        }
                        break;
                    case (int) EventEnum.MOVEMENT_TURN_RIGHT:
                        if (IsSameEvent(eventCommand.parameters[0], customMoveEventId))
                        {
                            direction = new Vector2Int(1, 0);
                            _posList.Add(new Vector2Int((int) CustomMovePreview.PosId.Turn, (int) GetDirectionId(direction)));
                        }
                        else
                        {
                            _posList.Add(waitMarker);
                        }
                        break;
                    case (int) EventEnum.MOVEMENT_TURN_UP:
                        if (IsSameEvent(eventCommand.parameters[0], customMoveEventId))
                        {
                            direction = new Vector2Int(0, 1);
                            _posList.Add(new Vector2Int((int) CustomMovePreview.PosId.Turn, (int) GetDirectionId(direction)));
                        }
                        else
                        {
                            _posList.Add(waitMarker);
                        }
                        break;
                    case (int) EventEnum.MOVEMENT_TURN_90_RIGHT:
                    case (int) EventEnum.MOVEMENT_TURN_90_LEFT:
                    case (int) EventEnum.MOVEMENT_TURN_180:
                    case (int) EventEnum.MOVEMENT_TURN_90_RIGHT_OR_LEFT:
                    case (int) EventEnum.MOVEMENT_TURN_AT_RANDOM:
                    case (int) EventEnum.MOVEMENT_TURN_TOWARD_PLAYER:
                    case (int) EventEnum.MOVEMENT_TURN_AWAY_FROM_PLAYER:
                        _posList.Add(waitMarker);
                        break;
                    case (int) EventEnum.EVENT_CODE_TIMING_WAIT:
                    case (int) EventEnum.EVENT_CODE_MOVE_PLACE:
                        _posList.Add(waitMarker);
                        break;
                    case (int) EventEnum.EVENT_CODE_MOVE_SET_EVENT_POINT:
                        if (IsSameEvent(eventCommand.parameters[0], customMoveEventId) && eventCommand.parameters[1] == "0")
                        {
                            // 同じイベントで、座標の直接指定。
                            pos = new Vector2Int(int.Parse(eventCommand.parameters[2]), int.Parse(eventCommand.parameters[3]));
                            switch (int.Parse(eventCommand.parameters[4]))
                            {
                                case 1:
                                    direction = new Vector2Int(0, -1);
                                    _posList.Add(new Vector2Int((int) CustomMovePreview.PosId.TurnAndNext, (int) GetDirectionId(direction)));
                                    break;
                                case 2:
                                    direction = new Vector2Int(-1, 0);
                                    _posList.Add(new Vector2Int((int) CustomMovePreview.PosId.TurnAndNext, (int) GetDirectionId(direction)));
                                    break;
                                case 3:
                                    direction = new Vector2Int(1, 0);
                                    _posList.Add(new Vector2Int((int) CustomMovePreview.PosId.TurnAndNext, (int) GetDirectionId(direction)));
                                    break;
                                case 4:
                                    direction = new Vector2Int(0, 1);
                                    _posList.Add(new Vector2Int((int) CustomMovePreview.PosId.TurnAndNext, (int) GetDirectionId(direction)));
                                    break;
                                default:
                                    break;
                            }
                            _posList.Add(pos);
                        }
                        else
                        {
                            _posList.Add(waitMarker);
                        }
                        break;
                }
                if (_posList.Count >= CountLimit) break;
            }
        }

        private void Preview() {
            //プレビュー用のポジション保持
            var nowEventMap = EventManagementService.LoadEventMap()
                .Find(e => e.eventId == EventDataModels[EventIndex].id);
            if (nowEventMap != null)
            {
                _nowPos = new Vector2(nowEventMap.x, nowEventMap.y);
            }
            MakePosList();

            var _databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
            var uiSettingDataModel = _databaseManagementService.LoadUiSettingDataModel();

            _sceneWindow =
                WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.DatabaseSceneWindow) as
                    SceneWindow;

            var eventMapDataModels = EventManagementService.LoadEventMap();
            var _mapId = "";
            for (var i = 0; i < eventMapDataModels.Count; i++)
                if (eventMapDataModels[i].eventId == EventDataModels[EventIndex].id)
                {
                    _mapId = eventMapDataModels[i].mapId;
                    break;
                }

            var mapDataModel = MapManagementService.LoadMapById(_mapId);
            _sceneWindow.Clear();
            _sceneWindow.Create(SceneWindow.PreviewId.CustomMove);
            _sceneWindow.GetCustomMovePreview().CreateMap(mapDataModel, _nowPos, _posList, EventDataModels[EventIndex].id);
            _sceneWindow.Init();

            var customMovePreview = _sceneWindow.GetCustomMovePreview();
            var parameters = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters;
            customMovePreview.SetTargetId(parameters[1]);

            _sceneWindow.SetRenderingSize(2400, 1080);
            _sceneWindow.Render();
        }
    }
}
