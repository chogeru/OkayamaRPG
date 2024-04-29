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
    public class ChangeExp : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_change_exp.uxml";

        public ChangeExp(
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
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("1");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }


            RadioButton fix_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display65");
            RadioButton variable_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display66");
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

            RadioButton increase_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display67");
            RadioButton reduce_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display68");
            Toggle permit_toggle = RootElement.Query<Toggle>("permit_toggle");
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] == "0")
                increase_toggle.value = true;
            else
                reduce_toggle.value = true;
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[5] == "1")
                permit_toggle.value = true;
            
            var defaultIncreaseReduce = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {increase_toggle, reduce_toggle},
                defaultIncreaseReduce, new List<System.Action>
                {
                    //増やす
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] = "0";
                        permit_toggle.SetEnabled(true);
                        Save(EventDataModels[EventIndex]);
                    },
                    //減らす
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] = "1";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[5] = "0";
                        permit_toggle.SetEnabled(false);
                        Save(EventDataModels[EventIndex]);
                    }
                });

            
            permit_toggle.RegisterValueChangedCallback(o =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[5] =
                    permit_toggle.value ? "1" : "0";
                Save(EventDataModels[EventIndex]);
            });


            RadioButton constant_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display69");
            RadioButton variableOperand_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display70");
            IntegerField constant = RootElement.Query<IntegerField>("constantNum");
            VisualElement variableOperand = RootElement.Query<VisualElement>("variableOperand");

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] == "1")
            {
                selectID = variableIDList.IndexOf(EventDataModels[EventIndex]
                    .eventCommands[EventCommandIndex]
                    .parameters[4]);
            }
            else
            {
                selectID = 0;
                constant.value =
                    int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4]);
            }

            if (selectID == -1)
            {
                selectID = 0;
                if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] == "1" && variableIDList.Count > 0)
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex]
                        .parameters[4] = variableIDList[0];
                    Save(EventDataModels[EventIndex]);
                }
            }

            var variableOperandPopupField = new PopupFieldBase<string>(variableNameList, selectID);

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] == "0")
            {
                constant_toggle.value = true;
                constant.SetEnabled(true);
            }
            
            var defaultVariableOperand = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {constant_toggle, variableOperand_toggle},
                defaultVariableOperand, new List<System.Action>
                {
                    //定数
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] = "0";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] =
                            constant.value.ToString();
                        Save(EventDataModels[EventIndex]);
                        constant.SetEnabled(true);
                        variableOperand_toggle.value = false;
                        variableOperand.SetEnabled(false);
                    },
                    //変数
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] = "1";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] =
                            variableIDList[variableOperandPopupField.index];
                        Save(EventDataModels[EventIndex]);
                        variableOperand.SetEnabled(true);
                        constant_toggle.value = false;
                        constant.SetEnabled(false);
                    }
                });

            
            
            var classDataModel = DatabaseManagementService.LoadClassCommon()[0];
            
            BaseInputFieldHandler.IntegerFieldCallback(constant, (evt) =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] =
                    constant.value.ToString();
                Save(EventDataModels[EventIndex]);
            },1,classDataModel.expGainIncreaseValue -1);


            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] == "1")
            {
                variableOperand_toggle.value = true;
                variableOperand.SetEnabled(true);
            }

            

            variableOperand.Clear();
            variableOperand.Add(variableOperandPopupField);
            variableOperandPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] =
                    variableIDList[variableOperandPopupField.index];
                Save(EventDataModels[EventIndex]);
            });
        }
    }
}