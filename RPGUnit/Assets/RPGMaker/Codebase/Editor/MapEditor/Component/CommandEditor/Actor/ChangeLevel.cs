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
    public class ChangeLevel : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_change_level.uxml";

        public ChangeLevel(
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
            if (EventCommand.parameters.Count == 0)
            {
                EventCommand.parameters.Add("0");
                EventCommand.parameters.Add("-1");
                EventCommand.parameters.Add("0");
                EventCommand.parameters.Add("0");
                EventCommand.parameters.Add("1");
                EventCommand.parameters.Add("0");
                EventManagementService.SaveEvent(EventDataModel);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }


            RadioButton fix_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display71");
            RadioButton variable_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display72");
            VisualElement fix = RootElement.Query<VisualElement>("fix");
            VisualElement variable = RootElement.Query<VisualElement>("variable");
            
            var characterActorDataModels = DatabaseManagementService.LoadCharacterActor().FindAll(actor => actor.charaType == (int) ActorTypeEnum.ACTOR);
            var characterActorNameList = EditorLocalize.LocalizeTexts(new List<string> { "WORD_0892" });
            var characterActorIDList = EditorLocalize.LocalizeTexts(new List<string> { "-1" });

            var selectID = 0;
            for (var i = 0; i < characterActorDataModels.Count; i++)
            {
                characterActorNameList.Add(characterActorDataModels[i].basic.name);
                characterActorIDList.Add(characterActorDataModels[i].uuId);
            }

            if (EventCommand.parameters[0] == "0")
                selectID = characterActorIDList.IndexOf(EventCommand.parameters[1]);
            if (selectID == -1)
                selectID = 0;
            var fixPopupField = new PopupFieldBase<string>(characterActorNameList, selectID);

            var flagDataModel = DatabaseManagementService.LoadFlags();
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

            if (EventCommand.parameters[0] == "1")
                selectID = variableIDList.IndexOf(EventCommand.parameters[1]);
            if (selectID == -1)
            {
                selectID = 0;
                if (EventCommand.parameters[0] == "1" && variableIDList.Count > 0)
                {
                    EventCommand.parameters[1] = variableIDList[0];
                    Save(EventDataModels[EventIndex]);
                }
            }

            var variablePopupField = new PopupFieldBase<string>(variableNameList, selectID);

            if (EventCommand.parameters[0] == "0")
                fix_toggle.value = true;
            else
                variable_toggle.value = true;
            
            var defaultSelect = EventCommand.parameters[0] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {fix_toggle, variable_toggle},
                defaultSelect, new List<System.Action>
                {
                    //固定
                    () =>
                    {
                        EventCommand.parameters[0] = "0";
                        EventCommand.parameters[1] = characterActorIDList[fixPopupField.index];
                        fix.SetEnabled(true);
                        variable.SetEnabled(false);
                        Save(EventDataModel);
                    },
                    //変数
                    () =>
                    {
                        EventCommand.parameters[0] = "1";
                        EventCommand.parameters[1] = variableIDList[variablePopupField.index];
                        fix.SetEnabled(false);
                        variable.SetEnabled(true);
                        Save(EventDataModel);
                    }
                });
            
            
            fix.Clear();
            fix.Add(fixPopupField);
            fixPopupField.RegisterValueChangedCallback(evt =>
            {
                EventCommand.parameters[1] = characterActorIDList[fixPopupField.index];

                Save(EventDataModel);
            });
            
            variable.Clear();
            variable.Add(variablePopupField);
            variablePopupField.RegisterValueChangedCallback(evt =>
            {
                EventCommand.parameters[1] = variableIDList[variablePopupField.index];
                Save(EventDataModel);
            });

            RadioButton increase_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display73");
            RadioButton reduce_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display74");
            Toggle permit_toggle = RootElement.Query<Toggle>("permit_toggle");
            if (EventCommand.parameters[2] == "0")
                increase_toggle.value = true;
            else
                reduce_toggle.value = true;
            if (EventCommand.parameters[5] == "1")
                permit_toggle.value = true;
            
            var defaultIncreaseReduce = EventCommand.parameters[2] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {increase_toggle, reduce_toggle},
                defaultIncreaseReduce, new List<System.Action>
                {
                    //増やす
                    () =>
                    {
                        EventCommand.parameters[2] = "0";
                        permit_toggle.SetEnabled(true);
                        Save(EventDataModel);
                    },
                    //減らす
                    () =>
                    {
                        EventCommand.parameters[2] = "1";
                        EventCommand.parameters[5] = "0";
                        permit_toggle.value = false;
                        permit_toggle.SetEnabled(false);
                        Save(EventDataModel);
                    }
                });

            
            permit_toggle.RegisterValueChangedCallback(o =>
            {
                EventCommand.parameters[5] = permit_toggle.value ? "1" : "0";
                Save(EventDataModel);
            });


            RadioButton constant_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display75");
            RadioButton variableOperand_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display76");
            IntegerField constant = RootElement.Query<IntegerField>("constantNum");
            VisualElement variableOperand = RootElement.Query<VisualElement>("variableOperand");

            if (EventCommand.parameters[3] == "1")
            {
                selectID = variableIDList.IndexOf(EventCommand.parameters[4]);
            }
            else
            {
                selectID = 0;
                constant.value = int.Parse(EventCommand.parameters[4]);
            }

            if (selectID == -1)
            {
                selectID = 0;
                if (EventCommand.parameters[3] == "1" && variableIDList.Count > 0)
                {
                    EventCommand.parameters[4] = variableIDList[0];
                    Save(EventDataModels[EventIndex]);
                }
            }

            var variableOperandPopupField = new PopupFieldBase<string>(variableNameList, selectID);

            if (EventCommand.parameters[3] == "0")
            {
                constant_toggle.value = true;
                constant.SetEnabled(true);
            }
            
            var defaultVariableOperand = EventCommand.parameters[3] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {constant_toggle, variableOperand_toggle},
                defaultVariableOperand, new List<System.Action>
                {
                    //定数
                    () =>
                    {
                        EventCommand.parameters[3] = "0";
                        EventCommand.parameters[4] = constant.value.ToString();
                        Save(EventDataModel);
                        constant.SetEnabled(true);
                        variableOperand_toggle.value = false;
                        variableOperand.SetEnabled(false);
                    },
                    //変数
                    () =>
                    {
                        EventCommand.parameters[3] = "1";
                        EventCommand.parameters[4] = variableIDList[variableOperandPopupField.index];
                        Save(EventDataModel);
                        variableOperand.SetEnabled(true);
                        constant_toggle.value = false;
                        constant.SetEnabled(false);
                    }
                });

            var classDataModel = DatabaseManagementService.LoadClassCommon()[0];
            int maxLevel = classDataModel.maxLevel; //最大レベル
            BaseInputFieldHandler.IntegerFieldCallback(constant, evt =>
            {
                EventCommand.parameters[4] = constant.value.ToString();
                Save(EventDataModel);
            }, 1, maxLevel-1);

            if (EventCommand.parameters[3] == "1")
            {
                variableOperand_toggle.value = true;
                variableOperand.SetEnabled(true);
            }
            
            variableOperand.Clear();
            variableOperand.Add(variableOperandPopupField);
            variableOperandPopupField.RegisterValueChangedCallback(evt =>
            {
                EventCommand.parameters[4] = variableIDList[variableOperandPopupField.index];
                Save(EventDataModel);
            });
        }
    }
}