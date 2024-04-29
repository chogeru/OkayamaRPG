using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Actor
{
    public class ChangeParameter : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_change_parameter.uxml";

        public ChangeParameter(
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
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("-1");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("1");
                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }


            RadioButton fix_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display77");
            RadioButton variable_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display78");
            VisualElement fix = RootElement.Query<VisualElement>("fix");
            VisualElement variable = RootElement.Query<VisualElement>("variable");

            var characterActorDataModels =
                DatabaseManagementService.LoadCharacterActor().FindAll(actor => actor.charaType == (int) ActorTypeEnum.ACTOR);
            var characterActorNameList = EditorLocalize.LocalizeTexts(new List<string> {"WORD_0892"});
            var characterActorIDList = EditorLocalize.LocalizeTexts(new List<string> {"-1"});

            var selectID = 0;
            for (var i = 0; i < characterActorDataModels.Count; i++)
            {
                characterActorNameList.Add(characterActorDataModels[i].basic.name);
                characterActorIDList.Add(characterActorDataModels[i].uuId);
            }

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] == "0")
                selectID = characterActorIDList.IndexOf(EventDataModels[EventIndex]
                    .eventCommands[EventCommandIndex].parameters[1]);
            if (selectID == -1)
                selectID = 0;
            var fixPopupField = new PopupFieldBase<string>(characterActorNameList, selectID);


            var flagDataModel =
                DatabaseManagementService.LoadFlags();
            var variableNameList = new List<string>();
            var variableIDList = new List<string>();
            selectID = 0;
            for (var i = 0; i < flagDataModel.variables.Count; i++)
            {
                if (flagDataModel.variables[i].name == "")
                    variableNameList.Add(EditorLocalize.LocalizeText("WORD_1518"));
                else
                    variableNameList.Add(flagDataModel.variables[i].name);

                variableIDList.Add(flagDataModel.variables[i].id);
            }

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] == "1")
                selectID = variableIDList.IndexOf(EventDataModels[EventIndex]
                    .eventCommands[EventCommandIndex]
                    .parameters[1]);
            if (selectID == -1)
            {
                selectID = 0;
                if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] == "1" && variableIDList.Count > 0)
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex]
                        .parameters[1] = variableIDList[0];
                    Save(EventDataModels[EventIndex]);
                }
            }

            var variablePopupField = new PopupFieldBase<string>(variableNameList, selectID);

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] == "0")
                fix_toggle.value = true;
            else
                variable_toggle.value = true;
            
            var defaultSelect = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {fix_toggle, variable_toggle},
                defaultSelect, new List<System.Action>
                {
                    //固定
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "0";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                            characterActorIDList[fixPopupField.index];
                        fix.SetEnabled(true);
                        variable.SetEnabled(false);
                        Save(EventDataModels[EventIndex]);
                    },
                    //変数
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "1";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                            variableIDList[variablePopupField.index];
                        fix.SetEnabled(false);
                        variable.SetEnabled(true);
                        Save(EventDataModels[EventIndex]);
                    }
                });
            
            
            fix.Clear();
            fix.Add(fixPopupField);
            fixPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    characterActorIDList[fixPopupField.index];

                Save(EventDataModels[EventIndex]);
            });
            
            variable.Clear();
            variable.Add(variablePopupField);
            variablePopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    variableIDList[variablePopupField.index];
                Save(EventDataModels[EventIndex]);
            });


            VisualElement status = RootElement.Query<VisualElement>("status");
            var parameter =
                EditorLocalize.LocalizeTexts(new List<string>
                {
                    "WORD_0395", "WORD_0539", "WORD_0177", "WORD_0178", "WORD_0179", "WORD_0180", "WORD_0181",
                    "WORD_0182"
                });
            selectID = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex]
                .parameters[2]);
            if (selectID == -1)
                selectID = 0;
            var statusPopupField = new PopupFieldBase<string>(parameter, selectID);
            status.Clear();
            status.Add(statusPopupField);
            statusPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] =
                    parameter.IndexOf(statusPopupField.value).ToString();
                Save(EventDataModels[EventIndex]);
            });

            RadioButton increase_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display79");
            RadioButton reduce_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display80");
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] == "0")
                increase_toggle.value = true;
            else
                reduce_toggle.value = true;
            
            var defaultIncreaseReduce = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {increase_toggle, reduce_toggle},
                defaultIncreaseReduce, new List<System.Action>
                {
                    //増やす
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] = "0";
                        Save(EventDataModels[EventIndex]);
                    },
                    //減らす
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] = "1";
                        Save(EventDataModels[EventIndex]);
                    }
                });
            
            

            RadioButton constant_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display81");
            RadioButton variableOperand_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display82");
            IntegerField constant = RootElement.Query<IntegerField>("constantNum");
            VisualElement variableOperand = RootElement.Query<VisualElement>("variableOperand");


            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] == "1")
            {
                selectID = variableIDList.IndexOf(EventDataModels[EventIndex]
                    .eventCommands[EventCommandIndex]
                    .parameters[5]);
            }
            else
            {
                selectID = 0;
                constant.value =
                    int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[5]);
            }

            if (selectID == -1)
            {
                selectID = 0;
                if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] == "1" && variableIDList.Count > 0)
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex]
                        .parameters[5] = variableIDList[0];
                    Save(EventDataModels[EventIndex]);
                }
            }

            var variableOperandPopupField = new PopupFieldBase<string>(variableNameList, selectID);
            variableOperand.Clear();
            variableOperand.Add(variableOperandPopupField);
            variableOperandPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[5] =
                    variableIDList[variableOperandPopupField.index];
                Save(EventDataModels[EventIndex]);
            });

            
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] == "0")
            {
                constant_toggle.value = true;
                constant.SetEnabled(true);
            }
            
            var defaultVariableOperand = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {constant_toggle, variableOperand_toggle},
                defaultVariableOperand, new List<System.Action>
                {
                    //定数
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] = "0";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[5] =
                            constant.value.ToString();
                        Save(EventDataModels[EventIndex]);

                        constant.SetEnabled(true);
                        variableOperand.SetEnabled(false);
                    },
                    //変数
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] = "1";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[5] =
                            variableIDList[variableOperandPopupField.index];

                        Save(EventDataModels[EventIndex]);

                        variableOperand.SetEnabled(true);
                        constant.SetEnabled(false);
                    }
                });
            
            BaseInputFieldHandler.IntegerFieldCallback(constant, evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[5] =
                    constant.value.ToString();
                Save(EventDataModels[EventIndex]);
            }, 1, 9999);

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] == "1")
            {
                variableOperand_toggle.value = true;
                variableOperand.SetEnabled(true);
            }
        }
    }
}