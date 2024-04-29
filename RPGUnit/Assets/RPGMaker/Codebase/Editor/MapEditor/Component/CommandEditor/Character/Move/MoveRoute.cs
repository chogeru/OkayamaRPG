using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using RPGMaker.Codebase.Runtime.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Character.Move
{
    /// <summary>
    /// 『移動ルートの指定』イベントコマンドの編集UI。
    /// </summary>
    public class MoveRoute : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_move_route.uxml";

        public const int ProvisionalMapIdParameterIndex = 10;

        private GenericPopupFieldBase<MapDataChoice> _provisionalMapPopupField;
        private GenericPopupFieldBase<TargetCharacterChoice> _targetCharacterPopupField;

        private List<EventMoveEnum> _codeList = new List<EventMoveEnum>();

        private SceneWindow _sceneWindow;

        Button _routeSettingButton;
        Button _routeInitButton;
        Button _previewButton;

        public MoveRoute(
            VisualElement rootElement,
            List<EventDataModel> eventDataModels,
            int eventIndex,
            int eventCommandIndex
        )
            : base(rootElement, eventDataModels, eventIndex, eventCommandIndex)
        {
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
                EventCommand.parameters.AddRange(
                    new string[] { "-2", "0", "3", "2", "0", "0", "0", "0", "0", "1" });
                Save(EventDataModel);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            // 『マップ選択』(コモンイベントの場合)と『キャラクター』のPopupField。
            {
                int targetCharacterParameterIndex = 0;
                _provisionalMapPopupField = AddOrHideProvisionalMapAndAddTargetCharacterPopupField(
                    targetCharacterParameterIndex,
                    provisionalMapPopupField =>
                    {
                        _targetCharacterPopupField = AddTargetCharacterPopupField(
                            QRoot("eventSelect"),
                            targetCharacterParameterIndex,
                            changeEvent =>
                            {
                                EventCommand.parameters[targetCharacterParameterIndex] = changeEvent.newValue.Id;

                                _codeList =
                                    EventCommand.route.
                                        Select(eventCommandMoveRoute => (EventMoveEnum)eventCommandMoveRoute.code).
                                            ToList();

                                Save(EventDataModel);
                                SetEnabledToButtons();
                                SetMoveFromPosition();
                            },
                            forceMapId: provisionalMapPopupField?.value.MapDataModel?.id);

                        UpdateMapAndEnabledButtons();
                    });
            }


            VisualElement moveKindPopupFieldContainer = RootElement.Query<VisualElement>("moveKind");
            moveKindPopupFieldContainer.Clear();
            var moveKindTextDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string> {"WORD_1001", "WORD_0447", "WORD_1002", "WORD_1003"});

            var moveKindIndex = EventCommand.parameters[1] != null ?
                int.Parse(EventCommand.parameters[1]) :
                0;

            if (moveKindIndex == -1)
            {
                moveKindIndex = 0;
                EventCommand.parameters[1] = moveKindIndex.ToString();
            }

            _routeSettingButton = RootElement.Query<Button>("route_setting_button");
            _routeInitButton = RootElement.Query<Button>("route_init_button");
            _previewButton = RootElement.Query<Button>("preview_button");

            if (moveKindIndex == 0)
            {
                var pos = new List<Vector3Int>();
                var codeList = new List<EventMoveEnum>();
                var indexList = new List<int>();
                var textList = new List<string>();
                foreach (var data in EventCommand.route)
                    codeList.Add((EventMoveEnum) data.code);

                _codeList = codeList;
                pos.Add(new Vector3Int(0, 0, 0));

                var eventMaps = EventManagementService.LoadEventMap();

                //開始位置の設定
                var targetCharacterId = EventCommand.parameters[0];
                if (targetCharacterId == "-2")
                {
                    EventMapDataModel eventMap = null;
                    for (int i = 0; i < eventMaps.Count; i++)
                        if (eventMaps[i].eventId == EventDataModel.id)
                        {
                            eventMap = eventMaps[i];
                            break;
                        }
                    if (eventMap == null) eventMap = eventMaps.Count > 0 ? eventMaps[0] : null;

                    pos[0] = eventMap != null ? new Vector3Int(eventMap.x, eventMap.y, 0) : Vector3Int.zero;
                }
                else if (targetCharacterId == "-1")
                {
                    EventMapDataModel eventMap = null;
                    for (int i = 0; i < eventMaps.Count; i++)
                        if (eventMaps[i].eventId == EventDataModel.id)
                        {
                            eventMap = eventMaps[i];
                            break;
                        }
                    if (eventMap == null) eventMap = eventMaps.Count > 0 ? eventMaps[0] : null;
                    pos[0] = eventMap != null ? new Vector3Int(eventMap.x, eventMap.y, 0) : Vector3Int.zero;
                }
                else
                {
                    EventMapDataModel eventMap = null;
                    for (int i = 0; i < eventMaps.Count; i++)
                        if (eventMaps[i].eventId == targetCharacterId)
                        {
                            eventMap = eventMaps[i];
                            break;
                        }
                    if (eventMap == null)
                    {
                        for (int i = 0; i < eventMaps.Count; i++)
                            if (eventMaps[i].eventId == EventDataModel.id)
                            {
                                eventMap = eventMaps[i];
                                break;
                            }
                    }
                    if (eventMap == null) eventMap = eventMaps.Count > 0 ? eventMaps[0] : null;
                    pos[0] = eventMap != null ? new Vector3Int(eventMap.x, eventMap.y, 0) : Vector3Int.zero;
                }

                MapEditor.LaunchRouteEditMode(pos, indexList,
                    EventCommand.route,
                    textList,
                    SetAndSaveMoveRoute,
                    EventIndex,
                    EventCommandIndex);
            }

            var isEdit = false;
            _routeSettingButton.text = EditorLocalize.LocalizeText("WORD_1583");

            // 『設定開始』『設定終了』ボタンクリック。
            _routeSettingButton.clickable.clicked +=
                () =>
                {
                    if (isEdit)
                    {
                        // 『設定終了』ボタンクリック時の処理。
                        _routeSettingButton.text = EditorLocalize.LocalizeText("WORD_1583");
                        MapEditor.LaunchRouteDrawingModeEnd();

                        // プレビューのボタンを押下可能とする
                        _previewButton.SetEnabled(true);
                    }
                    else
                    {
                        // 『設定開始』ボタンクリック時の処理。
                        var fromPosition =
                            (Vector2Int) GetMoveRouteSettingsFromTilePositon(EventDataModel, EventCommand.parameters);

                        _routeSettingButton.text = EditorLocalize.LocalizeText("WORD_1584");
                        MapEditor.LaunchRouteDrawingMode(new (fromPosition.x, fromPosition.y, 0));

                        // プレビューのボタンを押下できないようにする
                        _previewButton.SetEnabled(false);
                    }

                    isEdit = !isEdit;
                };

            // 『ルートの初期化』ボタンクリック。
            _routeInitButton.clickable.clicked += () =>
            {
                var fromPosition =
                    (Vector2Int)GetMoveRouteSettingsFromTilePositon(EventDataModel, EventCommand.parameters);

                MapEditor.LaunchRouteEditMode(
                    new() { new(fromPosition.x, fromPosition.y, 0) },
                    new(),
                    new(),
                    new(),
                    SetAndSaveMoveRoute, 
                    EventIndex,
                    EventCommandIndex);
            };

            //プレビューボタン
            _previewButton.clickable.clicked += () => { Preview(); };

            var moveKindPopupField = new PopupFieldBase<string>(moveKindTextDropdownChoices, moveKindIndex);
            moveKindPopupFieldContainer.Add(moveKindPopupField);
            moveKindPopupField.RegisterValueChangedCallback(evt =>
            {
                switch (moveKindTextDropdownChoices.IndexOf(moveKindPopupField.value))
                {
                    case 0:
                        EventCommand.code = (int) EventEnum.EVENT_CODE_MOVE_SET_MOVE_POINT;
                        break;
                    case 1:
                        EventCommand.code = (int) EventEnum.MOVEMENT_MOVE_AT_RANDOM;
                        break;
                    case 2:
                        EventCommand.code = (int) EventEnum.MOVEMENT_MOVE_TOWARD_PLAYER;
                        break;
                    case 3:
                        EventCommand.code = (int) EventEnum.MOVEMENT_MOVE_AWAY_FROM_PLAYER;
                        break;
                }

                ExecutionContentsWindow.ProcessTextSetting();

                EventCommand.parameters[1] =
                    moveKindTextDropdownChoices.IndexOf(moveKindPopupField.value).ToString();
                Save(EventDataModel);
                SetEnabledToButtons();
            });

            // 移動速度。
            InitPopupField(
                "moveSpeed",
                new() { "WORD_0847", "WORD_0848", "WORD_0849", "WORD_0985", "WORD_0851", "WORD_0852" },
                2);

            // 移動頻度。
            InitPopupField(
                "moveFrequency",
                new() { "WORD_0854", "WORD_0855", "WORD_0850", "WORD_0856", "WORD_0857" },
                3);

            // 向き。
            InitPopupField(
                "direction",
                new() { "WORD_2595", "WORD_0860", "WORD_0815", "WORD_0813", "WORD_0814", "WORD_0812" },
                4);

            // 歩行アニメ、足踏みアニメ。
            {
                var moveAnimationOn_toggle = RootElement.Q<RadioButton>("radioButton-eventCommand-display11");
                var moveAnimationOff_toggle = RootElement.Q<RadioButton>("radioButton-eventCommand-display12");
                var stepAnimationOn_toggle = RootElement.Q<RadioButton>("radioButton-eventCommand-display13");
                var stepAnimationOff_toggle = RootElement.Q<RadioButton>("radioButton-eventCommand-display14");

                OnOffToggle(moveAnimationOn_toggle, moveAnimationOff_toggle, new string[] { "0", "3" });
                OnOffToggle(stepAnimationOn_toggle, stepAnimationOff_toggle, new string[] { "1", "3" });

                void OnOffToggle(RadioButton onToggle, RadioButton offToggle, string[] onParameterValues)
                {
                    onToggle.value = onParameterValues.Contains(EventCommand.parameters[5]);

                    offToggle.value = !onToggle.value;

                    onToggle.RegisterValueChangedCallback(evt =>
                    {
                        offToggle.value = !evt.newValue;
                        SetAndSave();
                    });

                    offToggle.RegisterValueChangedCallback(evt =>
                    {
                        onToggle.value = !evt.newValue;
                        SetAndSave();
                    });

                    void SetAndSave()
                    {
                        var value =
                            moveAnimationOff_toggle.value && stepAnimationOff_toggle.value ? 2 :
                            moveAnimationOn_toggle.value && stepAnimationOn_toggle.value ? 3 :
                            moveAnimationOff_toggle.value && stepAnimationOn_toggle.value ? 1 : 0;

                        EventCommand.parameters[5] = value.ToString();
                        Save(EventDataModel);
                    }
                }
            }

            // すり抜け。
            InitToggle("slidingThrough_toggle", 6);

            // 動作を繰り返す。
            InitToggle("repeatOperation_toggle", 7);

            // 移動できないときは飛ばす。
            InitToggle("moveSkip_toggle", 8);

            // 完了までウェイト。
            InitToggle("waitComplete_toggle", 9);

            EventManagementService.SaveEvent(EventDataModel);
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            UpdateMapAndEnabledButtons();
        }

        private void UpdateMapAndEnabledButtons()
        {
            SetEnabledToButtons();
            if (IsCommonEvent())
            {
                // 仮マップidを追加する。
                // イベントテキスト表示やプレビューで使用するGetJumpFromTilePositonメソッドで使用する。
                // セーブする情報に含まれてしまうが、セーブ情報としては使用しない。
                {
                    if (EventCommand.parameters.Count < ProvisionalMapIdParameterIndex + 1)
                    {
                        EventCommand.parameters.Add("");
                    }

                    EventCommand.parameters[ProvisionalMapIdParameterIndex] = CurrentMapId;
                }

                if (CurrentMapDataModel != null)
                {
                    // 対象マップを表示する。
                    MapEditor.LaunchCommonEventEditMode(
                        CurrentMapDataModel, EventDataModel.page, notCoordinateMode: true);
                }

                SetMoveFromPosition();

                Save(EventDataModel);
            }
        }

        /// <summary>
        /// 『設定開始』『プレビュー』ボタンの有効/無効を設定。
        /// </summary>
        private void SetEnabledToButtons()
        {
            // 『ルート指定』かつマップが紐付けられていれば有効。
            bool isEnabled = EventCommand.parameters[1] == "0" && CurrentMapId != null;
            _routeSettingButton?.SetEnabled(isEnabled);
            _routeInitButton?.SetEnabled(isEnabled);
            _previewButton?.SetEnabled(isEnabled);
        }

        // 開始位置の設定
        private void SetMoveFromPosition()
        {
            var pos = GetMoveRouteSettingsFromTilePositon(EventDataModel, EventCommand.parameters);
            if (pos == null)
            {
                return;
            }

            MapEditor.LaunchRouteEditMode(
                new() { new(((Vector2Int)pos).x, ((Vector2Int)pos).y, 0) },
                new(),
                EventCommand.route,
                new(),
                SetAndSaveMoveRoute,
                EventIndex,
                EventCommandIndex);
        }

        // 共通PopupFieldの初期化。
        private void InitPopupField(string containerName, List<string> choices, int parameterIndex)
        {
            var container = RootElement.Q<VisualElement>(containerName);
            container.Clear();

            choices = EditorLocalize.LocalizeTexts(choices);

            var choiceIndex = EventCommand.parameters[parameterIndex] != null ?
                int.Parse(EventCommand.parameters[parameterIndex]) :
                0;

            if (choiceIndex == -1)
            {
                choiceIndex = 0;
                EventCommand.parameters[parameterIndex] = choiceIndex.ToString();
            }

            var popupField = new PopupFieldBase<string>(choices, choiceIndex);
            container.Add(popupField);
            popupField.RegisterValueChangedCallback(evt =>
            {
                EventCommand.parameters[parameterIndex] = choices.IndexOf(popupField.value).ToString();
                Save(EventDataModel);
            });
        }

        // 共通Toggleの初期化。
        private void InitToggle(string toggleName, int parameterIndex)
        {
            var toggle = RootElement.Q<Toggle>(toggleName);
            toggle.value = EventCommand.parameters[parameterIndex] != "0";
            toggle.RegisterValueChangedCallback(evt =>
            {
                EventCommand.parameters[parameterIndex] = evt.newValue ? "1" : "0";
                Save(EventDataModel);
            });
        }

        /// <summary>
        ///     マップで入力したルート指定が入った配列を返却
        /// </summary>
        /// <param name="eventCommandMoveRoutes"></param>
        private void SetAndSaveMoveRoute(List<EventCommandMoveRoute> eventCommandMoveRoutes) {
            var moveRoutes = new List<EventCommandMoveRoute>();
            foreach (var moveRoute in eventCommandMoveRoutes)
            {
                var route = new EventCommandMoveRoute(moveRoute.code, new List<string>(), moveRoute.codeIndex);
                moveRoutes.Add(route);
            }

            EventCommand.route = moveRoutes;

            _codeList = new List<EventMoveEnum>();
            foreach (var data in moveRoutes) _codeList.Add((EventMoveEnum) data.code);

            EventManagementService.SaveEvent(EventDataModel);
        }

        private void Preview() {
            _sceneWindow =
                WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.DatabaseSceneWindow) as
                    SceneWindow;

            var fromPosition = GetMoveRouteSettingsFromTilePositon(EventDataModel, EventCommand.parameters);

            _sceneWindow.Clear();
            _sceneWindow.Create(SceneWindow.PreviewId.Route);
            _sceneWindow.GetRoutePreview().CreateMap(
                CurrentMapDataModel, (Vector2Int)fromPosition, _codeList, EventDataModel.id);
            _sceneWindow.Init();

            var routePreview = _sceneWindow.GetRoutePreview();
            routePreview.SetTargetId(EventCommand.parameters[0]);
            routePreview.SetSpeed((Commons.SpeedMultiple.Id)int.Parse(EventCommand.parameters[2]));
            routePreview.SetMoveFrequencyWaitTime(int.Parse(EventCommand.parameters[3]));
            routePreview.SetDirectionType((Commons.Direction.Id)int.Parse(EventCommand.parameters[4]));
            routePreview.SetAnimation(int.Parse(EventCommand.parameters[5]));

            _sceneWindow.SetRenderingSize(2400, 1080);
            _sceneWindow.Render();
        }

        /// <summary>
        /// 『移動ルート指定』の移動元のタイル座標を取得。
        /// </summary>
        /// <param name="eventDataModel">このイベントのデータ。</param>
        /// <param name="parameters">このイベントコマンドのパラメータ。</param>
        /// <returns>ジャンプ元タイル座標。紐づいたEventMapDataModelが存在しない場合はnull。</returns>
        private static Vector2Int? GetMoveRouteSettingsFromTilePositon(
            EventDataModel eventDataModel, List<string> parameters)
        {
            return GetMoveFromTilePositon(
                eventDataModel,
                parameters[0],
                parameters.Count > ProvisionalMapIdParameterIndex ?
                    parameters[ProvisionalMapIdParameterIndex] : null);
        }
    }
}