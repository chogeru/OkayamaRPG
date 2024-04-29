using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Map
{
    /// <summary>
    ///     [指定位置の情報取得]のコマンド設定枠の表示物
    /// </summary>
    public class DesignatedLocationObtain : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_designated_location_obtain.uxml";

        private RadioButton        character_toggle;
        private VisualElement character;
        private VisualElement provisionalMapContainer;
        private bool          destinationDrawing;

        // UI要素プロパティ
        private RadioButton        direct_toggle;
        private Button        directButton;
        private RadioButton        variable_toggle;
        private VisualElement xPos2;
        private Label         xPosLabel;
        private VisualElement yPos2;
        private Label         yPosLabel;

        private GenericPopupFieldBase<MapDataChoice> _provisionalMapPopupField;
        private GenericPopupFieldBase<TargetCharacterChoice> _targetCharacterPopupField;

        // 整数の文字列を負号反転する。
        private static string InvertsNegativeSignOfStringOfInt(string value) {
            if (int.TryParse(value, out int intValue))
            {
                value = (-intValue).ToString();
            }

            return value;
        }

        public DesignatedLocationObtain(
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

            direct_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display111");
            variable_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display112");
            character_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display113");
            xPosLabel = RootElement.Query<Label>("xPos");
            yPosLabel = RootElement.Query<Label>("yPos");
            xPosLabel.text = "0";
            yPosLabel.text = "0";
            directButton = RootElement.Query<Button>("directButton");
            xPos2 = RootElement.Query<VisualElement>("xPos2");
            yPos2 = RootElement.Query<VisualElement>("yPos2");
            character = RootElement.Query<VisualElement>("character");
            provisionalMapContainer = RootElement.Query<VisualElement>("provisional_map_popupfield_container");

            var flagDataModel = DatabaseManagementService.LoadFlags();
            if (EventCommand.parameters.Count == 0 || EventCommand.parameters.Count == 6)
            {
                if (EventCommand.parameters.Count == 0)
                {
                    // スイッチのID
                    EventCommand.parameters.Add(flagDataModel.variables[0].id);
                    // 情報タイプ
                    EventCommand.parameters.Add("1");
                    // レイヤー指定
                    EventCommand.parameters.Add("0");
                    // 場所指定（0: 直接指定, 1: 変数で指定, 2: キャラクターで指定）
                    EventCommand.parameters.Add("0");
                    // X座標用の情報
                    EventCommand.parameters.Add("0");
                    // Y座標用の情報
                    EventCommand.parameters.Add("0");
                }

                // キャラクター(イベント)指定 (不具合で以前はparameters[4]に割り当てられていた)。
                EventCommand.parameters.Add(
                    EventCommand.parameters.Count == 6 && EventCommand.parameters[3] == "2" ?
                        EventCommand.parameters[4] :"-2");

                Save(EventDataModel);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            VisualElement variable = RootElement.Query<VisualElement>("variable");
            var variableName = new List<string>();
            var variableID = new List<string>();
            var selectID = 0;
            for (var i = 0; i < flagDataModel.variables.Count; i++)
            {
                if (flagDataModel.variables[i].name == "")
                    variableName.Add(EditorLocalize.LocalizeText("WORD_1518"));
                else
                    variableName.Add(flagDataModel.variables[i].name);
                variableID.Add(flagDataModel.variables[i].id);
            }

            selectID = variableID.IndexOf(EventCommand.parameters[0]);
            if (selectID == -1)
            {
                selectID = 0;
                if (variableID.Count > 0)
                {
                    EventCommand.parameters[0] = variableID[0];
                    Save(EventDataModel);
                }
            }

            var variablePopupField = new PopupFieldBase<string>(variableName, selectID);
            variable.Add(variablePopupField);
            variablePopupField.RegisterValueChangedCallback(evt =>
            {
                EventCommand.parameters[0] = variableID[variablePopupField.index];
                Save(EventDataModel);
            });


            VisualElement mapSelect = RootElement.Query<VisualElement>("mapSelect");
            VisualElement layer = RootElement.Query<VisualElement>("layer");
            var mapSelectName = EditorLocalize.LocalizeTexts(new List<string> { "WORD_0822", "WORD_1186", "WORD_1187", "WORD_1188" });
            selectID = int.Parse(EventCommand.parameters[1]);
            if (selectID == -1)
                selectID = 0;
            var mapSelectPopupField = new PopupFieldBase<string>(mapSelectName, selectID);
            if (mapSelectPopupField.value == EditorLocalize.LocalizeText("WORD_1187") || mapSelectPopupField.value == mapSelectName[0])
                layer.SetEnabled(true);
            else
                layer.SetEnabled(false);
            mapSelect.Add(mapSelectPopupField);
            mapSelectPopupField.RegisterValueChangedCallback(evt =>
            {
                EventCommand.parameters[1] = mapSelectName.IndexOf(mapSelectPopupField.value).ToString();
                if (mapSelectPopupField.value == EditorLocalize.LocalizeText("WORD_1187") || mapSelectPopupField.value == mapSelectName[0])
                    layer.SetEnabled(true);
                else
                    layer.SetEnabled(false);
                Save(EventDataModel);
            });

            var layerName =
                EditorLocalize.LocalizeTexts(new List<string> {"WORD_1475", "WORD_1476", "WORD_1477", "WORD_1478"});
            selectID = int.Parse(EventCommand.parameters[2]);
            if (selectID == -1)
                selectID = 0;
            var layerPopupField = new PopupFieldBase<string>(layerName, selectID);
            layer.Add(layerPopupField);
            layerPopupField.RegisterValueChangedCallback(evt =>
            {
                EventCommand.parameters[2] = layerName.IndexOf(layerPopupField.value).ToString();
                Save(EventDataModel);
            });


            if (EventCommand.parameters[3] == "1")
                selectID = variableID.IndexOf(EventCommand.parameters[4]);
            if (selectID == -1)
                selectID = 0;
            var xPos2PopupField = new PopupFieldBase<string>(variableName, selectID);
            xPos2.Add(xPos2PopupField);
            xPos2PopupField.RegisterValueChangedCallback(evt =>
            {
                EventCommand.parameters[4] = variableID[xPos2PopupField.index];
                Save(EventDataModel);
            });

            if (EventCommand.parameters[3] == "1")
                selectID = variableID.IndexOf(EventCommand.parameters[5]);
            if (selectID == -1)
                selectID = 0;
            var yPos2PopupField = new PopupFieldBase<string>(variableName, selectID);
            yPos2.Add(yPos2PopupField);
            yPos2PopupField.RegisterValueChangedCallback(evt =>
            {
                EventCommand.parameters[5] = variableID[yPos2PopupField.index];
                Save(EventDataModel);
            });

            // キャラクター指定
            {
                int targetCharacterParameterIndex = 6;
                _provisionalMapPopupField = AddOrHideProvisionalMapAndAddTargetCharacterPopupField(
                    targetCharacterParameterIndex,
                    provisionalMapPopupField =>
                    {
                        _targetCharacterPopupField = AddTargetCharacterPopupField(
                            character,
                            targetCharacterParameterIndex,
                            forceMapId: provisionalMapPopupField?.value.MapDataModel?.id,
                            // 『キャラクターで指定』がオフの場合、強制的に選択項目indexを0にする。
                            // (多分パラメータ配列要素を使い回ししているのが理由)。
                            forceDefaultIndexIsZero: EventCommand.parameters[3] != "2");

                        UpdateMapAndEnabledButtons();
                    });
            }

            // 各編集項目の有効無効を指定方法に合わせて切り替える
            if (EventCommand.parameters[3] == "0")
            {
                xPosLabel.text = EventCommand.parameters[4];
                yPosLabel.text = InvertsNegativeSignOfStringOfInt(EventCommand.parameters[5]);
            }

            SwitchEditItem();
            
            var defaultSelect = int.Parse(EventCommand.parameters[3]);
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {direct_toggle, variable_toggle, character_toggle},
                defaultSelect, new List<System.Action>
                {
                    //直接指定
                    () =>
                    {
                        EventCommand.parameters[3] = "0";
                        EventCommand.parameters[4] = xPosLabel.text;
                        EventCommand.parameters[5] = InvertsNegativeSignOfStringOfInt(yPosLabel.text);
                        Save(EventDataModel);
                        SwitchEditItem();
                    },
                    //変数で指定
                    () =>
                    {
                        EventCommand.parameters[3] = "1";
                        EventCommand.parameters[4] = variableID[xPos2PopupField.index];
                        EventCommand.parameters[5] = variableID[yPos2PopupField.index];
                        Save(EventDataModel);
                        SwitchEditItem();
                    },
                    //キャラクターで指定
                    () =>
                    {
                        EventCommand.parameters[3] = "2";
                        EventCommand.parameters[6] = _targetCharacterPopupField.value.Id;
                        Save(EventDataModel);
                        SwitchEditItem();
                    }
                });
            
            directButton.clicked += () =>
            {
                destinationDrawing = !destinationDrawing;
                if (destinationDrawing)
                {
                    int posX = 0, posY = 0;
                    int.TryParse(EventCommand.parameters[4], out posX);
                    int.TryParse(EventCommand.parameters[5], out posY);

                    directButton.text = EditorLocalize.LocalizeText("WORD_2604");
                    MapEditor.BeginDestinationMode(EventIndex, EventCommandIndex, new Vector2Int(posX, posY));
                }
                else
                {
                    directButton.text = EditorLocalize.LocalizeText("WORD_0356");
                    MapEditor.EndDestinationMode();
                    xPosLabel.text = EventCommand.parameters[4];
                    yPosLabel.text = InvertsNegativeSignOfStringOfInt(EventCommand.parameters[5]);
                }
            };

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
        /// 『設定開始』ボタンの有効/無効を設定。
        /// </summary>
        private void SetEnabledToButtons()
        {
            // 『直接指定』でマップが紐付けられていれば有効。
            bool isEnabled = EventCommand.parameters[3] == "0" && CurrentMapId != null;
            directButton?.SetEnabled(isEnabled);
        }

        /// <summary>
        ///     場所指定の方法に基づいて有効にする設定項目を切り替える
        /// </summary>
        private void SwitchEditItem() {
            var location = EventCommand.parameters[3];

            // 直接指定
            xPosLabel.SetEnabled(direct_toggle.value);
            yPosLabel.SetEnabled(direct_toggle.value);
            if (!direct_toggle.value && destinationDrawing)
            {
                MapEditor.EndDestinationMode();
            }

            // 変数で指定
            xPos2.SetEnabled(variable_toggle.value);
            yPos2.SetEnabled(variable_toggle.value);

            // キャラクター指定
            character.SetEnabled(character_toggle.value);

            // 『マップ』選択。
            provisionalMapContainer.SetEnabled(location == "0" || location == "2");

            SetEnabledToButtons();
        }
    }
}