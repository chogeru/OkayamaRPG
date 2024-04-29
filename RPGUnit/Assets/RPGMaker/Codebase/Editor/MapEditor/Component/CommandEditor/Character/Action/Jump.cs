using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using RPGMaker.Codebase.Runtime.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Character.Action
{
    /// <summary>
    /// イベントコマンド『ジャンプ』編集UI。
    /// </summary>
    public class Jump : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_jump.uxml";

        public const int ProvisionalMapIdParameterIndex = 12;

        private SceneWindow _sceneWindow;
        private Vector2Int _mapPositionSetResultPos;

        private GenericPopupFieldBase<MapDataChoice> _provisionalMapPopupField;
        private GenericPopupFieldBase<TargetCharacterChoice> _targetCharacterPopupField;

        private Button _coordinateButton;
        private Button _previewButton;

        public Jump(
            VisualElement rootElement,
            List<EventDataModel> eventDataModels,
            int eventIndex,
            int eventCommandIndex
        )
            : base(rootElement, eventDataModels, eventIndex, eventCommandIndex) {
        }

        // 仮の (コモンイベント用の) マップのid。『なし』選択中ならnull。
        private string ProvisionalMapId => _provisionalMapPopupField?.value.MapDataModel?.id;

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

            var num = 0;

            if (EventCommand.parameters.Count == 0)
            {
                EventCommand.parameters.Add("-2");
                EventCommand.parameters.Add("0");
                EventCommand.parameters.Add("3");
                EventCommand.parameters.Add("0");
                EventCommand.parameters.Add("0");
                EventCommand.parameters.Add("0");
                EventCommand.parameters.Add("0");
                EventCommand.parameters.Add("0");
                EventCommand.parameters.Add("1");
                EventCommand.parameters.Add(EditorLocalize.LocalizeText("WORD_1176"));
                EventCommand.parameters.Add("0");
                EventCommand.parameters.Add("0");
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
                                SetAndSaveOffsetPos(GetJumpToTilePositon(EventDataModel, EventCommand.parameters));
                                SetEnabledToButtons();
                            },
                            forceMapId: provisionalMapPopupField?.value.MapDataModel?.id);

                        UpdateMapAndEnabledButtons();
                    });
            }

            // 『座標指定定』or『その場で』
            VisualElement moveKind = RootElement.Q<VisualElement>("command_jump").Query<VisualElement>("moveKind");
            var moveKindTextDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string> { "WORD_0983", "WORD_0984" });
            num = 0;
            if (EventCommand.parameters[1] != null)
                num = int.Parse(EventCommand.parameters[1]);
            if (num == -1)
                num = 0;

            var moveKindPopupField = new PopupFieldBase<string>(moveKindTextDropdownChoices, num);
            moveKind.Add(moveKindPopupField);

            // 『設定開始』or『設定終了』ボタン、『プレビュー』ボタン。
            _coordinateButton = new Button();
            _previewButton = new Button { text = EditorLocalize.LocalizeText("WORD_0991") };
            moveKind.Add(_coordinateButton);
            moveKind.Add(_previewButton);

            // 『座標指定定』or『その場で』
            moveKindPopupField.RegisterValueChangedCallback(evt =>
            {
                EventCommand.parameters[1] = moveKindPopupField.index.ToString();
                Save(EventDataModel);
                SetEnabledToButtons();
            });

            //プレビューボタン
            _previewButton.clickable.clicked += () => { Preview(); };

            var isEdit = false;
            _coordinateButton.text = EditorLocalize.LocalizeText("WORD_1583");

            // 『設定開始』『設定終了』ボタンクリック。
            _coordinateButton.clickable.clicked += () =>
            {
                if (isEdit)
                {
                    // 『設定終了』ボタンクリック時の処理。
                    _coordinateButton.text = EditorLocalize.LocalizeText("WORD_1583");
                    EndMapPositionSet(CurrentMapId);

                    // プレビューのボタンを押下可能とする
                    _previewButton.SetEnabled(true);
                }
                else
                {
                    // 『設定開始』ボタンクリック時の処理。
                    _coordinateButton.text = EditorLocalize.LocalizeText("WORD_1584");
                    BeginMapPositionSet(CurrentMapId);

                    // プレビューのボタンを押下できないようにする
                    _previewButton.SetEnabled(false);
                }

                isEdit = !isEdit;
            };

            // 『移動速度』
            VisualElement moveSpeed = RootElement.Q<VisualElement>("command_jump").Query<VisualElement>("moveSpeed");
            var moveSpeedTextDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string>
                    {"WORD_0847", "WORD_0848", "WORD_0849", "WORD_0985", "WORD_0851", "WORD_0852"});
            num = 0;
            if (EventCommand.parameters[2] != null)
                num = int.Parse(EventCommand.parameters[2]);
            if (num == -1)
                num = 3;

            var moveSpeedPopupField = new PopupFieldBase<string>(moveSpeedTextDropdownChoices, num);
            moveSpeed.Add(moveSpeedPopupField);
            moveSpeedPopupField.RegisterValueChangedCallback(evt =>
            {
                _sceneWindow?.GetJumpPreview().SetSpeed((Commons.SpeedMultiple.Id)moveSpeedPopupField.index);
                EventCommand.parameters[2] = moveSpeedPopupField.index.ToString();
                Save(EventDataModel);
            });

            // 『向き』
            VisualElement direction = RootElement.Q<VisualElement>("command_jump").Query<VisualElement>("direction");
            var directionTextDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string>
                    {"WORD_2595", "WORD_0860", "WORD_0815", "WORD_0813", "WORD_0814", "WORD_0812"});
            num = 0;
            if (EventCommand.parameters[3] != null)
                num = int.Parse(EventCommand.parameters[3]);
            if (num == -1)
                num = 0;

            var directionPopupField = new PopupFieldBase<string>(directionTextDropdownChoices, num);
            direction.Add(directionPopupField);
            directionPopupField.RegisterValueChangedCallback(evt =>
            {
                EventCommand.parameters[3] = directionPopupField.index.ToString();
                Save(EventDataModel);
            });

            // 『アニメーション』
            VisualElement animation = RootElement.Q<VisualElement>("command_jump").Query<VisualElement>("animation");
            var animationTextDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string> { "WORD_0987", "WORD_0988", "WORD_0113" });
            num = 0;
            if (EventCommand.parameters[4] != null)
                num = int.Parse(EventCommand.parameters[4]);
            if (num == -1)
                num = 0;

            var animationPopupField = new PopupFieldBase<string>(animationTextDropdownChoices, num);
            animation.Add(animationPopupField);
            animationPopupField.RegisterValueChangedCallback(evt =>
            {
                EventCommand.parameters[4] = animationPopupField.index.ToString();
                Save(EventDataModel);
            });

            // 『動作を繰り返す』
            Toggle repeatOperation_toggle =
                RootElement.Q<VisualElement>("command_jump").Query<Toggle>("repeatOperation_toggle");
            if (EventCommand.parameters[6] == "1")
                repeatOperation_toggle.value = true;
            repeatOperation_toggle.RegisterValueChangedCallback(evt =>
            {
                var num = repeatOperation_toggle.value ? 1 : 0;
                EventCommand.parameters[6] = num.ToString();
                Save(EventDataModel);
            });

            // 『完了までウェイト』
            Toggle waitComplete_toggle =
                RootElement.Q<VisualElement>("command_jump").Query<Toggle>("waitComplete_toggle");
            if (EventCommand.parameters[8] == "1")
                waitComplete_toggle.value = true;
            waitComplete_toggle.RegisterValueChangedCallback(evt =>
            {
                var num = waitComplete_toggle.value ? 1 : 0;
                EventCommand.parameters[8] = num.ToString();
                Save(EventDataModel);
            });

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

                Save(EventDataModel);
            }
        }

        /// <summary>
        /// 『設定開始』『プレビュー』ボタンの有効/無効を設定。
        /// </summary>
        private void SetEnabledToButtons()
        {
            // 『その場で』ではなくてマップが紐付けられていれば有効。
            bool isEnabled = EventCommand.parameters[1] != "1" && CurrentMapId != null;
            _coordinateButton?.SetEnabled(isEnabled);
            _previewButton?.SetEnabled(isEnabled);
        }

        /// <summary>
        /// 座標の設定開始。
        /// </summary>
        /// <param name="mapId">マップid。</param>
        public void BeginMapPositionSet(string mapId) {
            var mapDataModel = MapManagementService.LoadMapById(mapId) ?? MapManagementService.LoadMaps().First();
            MapEditor.LaunchCommonEventEditMode(mapDataModel, EventDataModel.page, pos =>
            {
                _mapPositionSetResultPos = new Vector2Int(pos.x, pos.y);
            });
        }

        /// <summary>
        /// 座標の設定終了。
        /// </summary>
        /// <param name="mapId">マップid。</param>
        public void EndMapPositionSet(string mapId)
        {
            var mapDataModel = MapManagementService.LoadMapById(mapId) ?? MapManagementService.LoadMaps().First();
            MapEditor.LaunchCommonEventEditModeEnd(mapDataModel);
            SetAndSaveOffsetPos(_mapPositionSetResultPos);
        }

        private void SetAndSaveOffsetPos(Vector2Int? toPos)
        {
            if (toPos == null)
            {
                return;
            }

            var fromPos = GetJumpFromTilePositon(EventDataModel, EventCommand.parameters);
            if (fromPos == null)
            {
                return;
            }

            var offsetPos = toPos - fromPos;
            if (offsetPos != null)
            {
                var notNullOffsetPos = (Vector2Int)offsetPos;
                EventCommand.parameters[10] = notNullOffsetPos.x.ToString();
                EventCommand.parameters[11] = notNullOffsetPos.y.ToString();
            }

            Save(EventDataModel);
        }

        private void Preview() {
            _sceneWindow =
                WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.DatabaseSceneWindow) as
                    SceneWindow;

            _sceneWindow.Create(SceneWindow.PreviewId.Jump);
            _sceneWindow.GetJumpPreview().CreateMap(
                CurrentMapDataModel,
                (Vector2Int)GetJumpFromTilePositon(EventDataModel, EventCommand.parameters),
                (Vector2Int)GetJumpToTilePositon(EventDataModel, EventCommand.parameters),
                EventDataModel);

            _sceneWindow.Init();

            var jumpPreview = _sceneWindow.GetJumpPreview();
            jumpPreview.SetTargetId(EventCommand.parameters[0]);
            jumpPreview.SetSpeed((Commons.SpeedMultiple.Id)int.Parse(EventCommand.parameters[2]));
            jumpPreview.SetDirection((Commons.Direction.Id)int.Parse(EventCommand.parameters[3]));
            jumpPreview.SetMoveType(int.Parse(EventCommand.parameters[1]));
            jumpPreview.SetAnimation(int.Parse(EventCommand.parameters[4]));
            _sceneWindow.SetRenderingSize(2400, 1080);
            _sceneWindow.Render();
        }

        /// <summary>
        /// 『ジャンプ』の移動元のタイル座標を取得。
        /// </summary>
        /// <param name="eventDataModel">このイベントのデータ。</param>
        /// <param name="parameters">このイベントコマンドのパラメータ。</param>
        /// <returns>ジャンプ元タイル座標。紐づいたEventMapDataModelが存在しない場合はnull。</returns>
        private static Vector2Int? GetJumpFromTilePositon(EventDataModel eventDataModel, List<string> parameters)
        {
            return GetMoveFromTilePositon(
                eventDataModel,
                parameters[0],
                parameters.Count > ProvisionalMapIdParameterIndex ?
                    parameters[ProvisionalMapIdParameterIndex] : null);
        }

        /// <summary>
        /// ジャンプ先のタイル座標を取得。
        /// </summary>
        /// <returns>ジャンプ先タイル座標。</returns>
        public static Vector2Int? GetJumpToTilePositon(EventDataModel eventDataModel, List<string> parameters)
        {
            var offsetPosition = parameters[1] == "1" ? 
                Vector2Int.zero :
                new Vector2Int(int.Parse(parameters[10]), int.Parse(parameters[11]));
            var fromtPosition = GetJumpFromTilePositon(eventDataModel, parameters);
            return fromtPosition != null ? fromtPosition + offsetPosition : null;
        }
    }
}