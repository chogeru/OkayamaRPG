using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Character.Direction
{
    public class Direction : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_direction.uxml";

        private GenericPopupFieldBase<TargetCharacterChoice> _targetCharacterPopupField;

        public Direction(
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

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count == 0)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("-2");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            // キャラクター。
            {
                int targetCharacterParameterIndex = 0;
                AddOrHideProvisionalMapAndAddTargetCharacterPopupField(
                    targetCharacterParameterIndex,
                    provisionalMapPopupField =>
                    {
                        _targetCharacterPopupField = AddTargetCharacterPopupField(
                            QRoot("character"),
                            targetCharacterParameterIndex,
                            forceMapId: provisionalMapPopupField?.value.MapDataModel?.id);
                    });
            }

            VisualElement popIcon = RootElement.Q<VisualElement>("command_direction").Query<VisualElement>("direction");
            var popIconTextDropdownChoices = EditorLocalize.LocalizeTexts(new List<string>
            {
                "WORD_0955", "WORD_0956", "WORD_0957", "WORD_0958", "WORD_0959", "WORD_0960", "WORD_0961", "WORD_0962",
                "WORD_0963", "WORD_0964",
                "WORD_0965"
            });
            int num = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]);
            if (num == -1)
                num = 0;

            if (num == 0 && EventDataModels[EventIndex].eventCommands[EventCommandIndex].code != (int) EventMoveEnum.MOVEMENT_TURN_DOWN)
            {
                //各コードに応じてnumの値を変換
                switch (EventDataModels[EventIndex].eventCommands[EventCommandIndex].code)
                {
                    case (int) EventMoveEnum.MOVEMENT_TURN_LEFT:
                        num = 1;
                        break;
                    case (int) EventMoveEnum.MOVEMENT_TURN_RIGHT:
                        num = 2;
                        break;
                    case (int) EventMoveEnum.MOVEMENT_TURN_UP:
                        num = 3;
                        break;
                    case (int) EventMoveEnum.MOVEMENT_TURN_90_RIGHT:
                        num = 4;
                        break;
                    case (int) EventMoveEnum.MOVEMENT_TURN_90_LEFT:
                        num = 5;
                        break;
                    case (int) EventMoveEnum.MOVEMENT_TURN_180:
                        num = 6;
                        break;
                    case (int) EventMoveEnum.MOVEMENT_TURN_90_RIGHT_OR_LEFT:
                        num = 7;
                        break;
                    case (int) EventMoveEnum.MOVEMENT_TURN_AT_RANDOM:
                        num = 8;
                        break;
                    case (int) EventMoveEnum.MOVEMENT_TURN_TOWARD_PLAYER:
                        num = 9;
                        break;
                    case (int) EventMoveEnum.MOVEMENT_TURN_AWAY_FROM_PLAYER:
                        num = 10;
                        break;
                    default:
                        break;
                }
            }
            var popIconPopupField = new PopupFieldBase<string>(popIconTextDropdownChoices, num);
            popIcon.Clear();
            popIcon.Add(popIconPopupField);
            popIconPopupField.RegisterValueChangedCallback(evt =>
            {
                switch (popIconTextDropdownChoices.IndexOf(popIconPopupField.value))
                {
                    case 0:
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].code =
                            (int) EventMoveEnum.MOVEMENT_TURN_DOWN;
                        break;
                    case 1:
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].code =
                            (int) EventMoveEnum.MOVEMENT_TURN_LEFT;
                        break;
                    case 2:
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].code =
                            (int) EventMoveEnum.MOVEMENT_TURN_RIGHT;
                        break;
                    case 3:
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].code =
                            (int) EventMoveEnum.MOVEMENT_TURN_UP;
                        break;
                    case 4:
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].code =
                            (int) EventMoveEnum.MOVEMENT_TURN_90_RIGHT;
                        break;
                    case 5:
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].code =
                            (int) EventMoveEnum.MOVEMENT_TURN_90_LEFT;
                        break;
                    case 6:
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].code =
                            (int) EventMoveEnum.MOVEMENT_TURN_180;
                        break;
                    case 7:
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].code =
                            (int) EventMoveEnum.MOVEMENT_TURN_90_RIGHT_OR_LEFT;
                        break;
                    case 8:
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].code =
                            (int) EventMoveEnum.MOVEMENT_TURN_AT_RANDOM;
                        break;
                    case 9:
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].code =
                            (int) EventMoveEnum.MOVEMENT_TURN_TOWARD_PLAYER;
                        break;
                    case 10:
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].code =
                            (int) EventMoveEnum.MOVEMENT_TURN_AWAY_FROM_PLAYER;
                        break;
                }

                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    popIconTextDropdownChoices.IndexOf(popIconPopupField.value).ToString();
                Save(EventDataModels[EventIndex]);
            });


            RadioButton command_direction_on =
                RootElement.Q<VisualElement>("command_direction").Query<RadioButton>("radioButton-eventCommand-display7");
            RadioButton command_direction_off =
                RootElement.Q<VisualElement>("command_direction").Query<RadioButton>("radioButton-eventCommand-display8");
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] == "1")
                command_direction_on.value = true;
            else
                command_direction_off.value = true;
            var defaultSelect = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] == "1" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {command_direction_on, command_direction_off},
                defaultSelect, new List<System.Action>
                {
                    //ON
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] = "1";
                        Save(EventDataModels[EventIndex]);
                    },
                    //OFF
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] = "0";
                        Save(EventDataModels[EventIndex]);
                    }
                    
                });
        }
    }
}