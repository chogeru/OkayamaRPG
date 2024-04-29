using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Party
{
    public class PartyItem : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_party_item.uxml";


        public PartyItem(
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

            var itemDataModels =
                DatabaseManagementService.LoadItem();


            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count == 0)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters
                    .Add(itemDataModels.Count > 0 ? itemDataModels[0].basic.id : "");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("1");
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }
            
            VisualElement item = RootElement.Q<VisualElement>("command_itemIncreaseDecrease")
                .Query<VisualElement>("item");
            var itemNameList = new List<string>();
            var itemIDList = new List<string>();
            var selectID = 0;
            for (var i = 0; i < itemDataModels.Count; i++)
            {
                itemNameList.Add(itemDataModels[i].basic.name);
                itemIDList.Add(itemDataModels[i].basic.id);
            }
            
            if (itemDataModels.Count == 0)
            {
                VisualElement partyArea = RootElement.Q<VisualElement>("command_itemIncreaseDecrease")
                    .Query<VisualElement>("party_area");
                partyArea.style.display = DisplayStyle.None;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "";
                Save(EventDataModels[EventIndex]);
                return;
            }
            

            selectID = itemIDList.IndexOf(EventDataModels[EventIndex].eventCommands[EventCommandIndex]
                .parameters[0]);
            if (selectID == -1)
            {
                selectID = 0;
                if (itemDataModels.Count > 0)
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = itemDataModels[0].basic.id;
                    Save(EventDataModels[EventIndex]);
                }
            }

            var itemPopupField = new PopupFieldBase<string>(itemNameList, selectID);
            item.Clear();
            item.Add(itemPopupField);
            itemPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                    itemIDList[itemPopupField.index];
                Save(EventDataModels[EventIndex]);
            });

            RadioButton on = RootElement.Q<VisualElement>("command_itemIncreaseDecrease").Query<RadioButton>("radioButton-eventCommand-display29");
            RadioButton off = RootElement.Q<VisualElement>("command_itemIncreaseDecrease").Query<RadioButton>("radioButton-eventCommand-display30");
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] == "0")
                on.value = true;
            else
                off.value = true;
            
            var defaultSelect = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {on, off},
                defaultSelect, new List<System.Action>
                {
                    //増やす
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "0";
                        Save(EventDataModels[EventIndex]);
                    },
                    //減らす
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "1";
                        Save(EventDataModels[EventIndex]);
                    }
                });

            RadioButton constant_toggle = RootElement.Q<VisualElement>("command_itemIncreaseDecrease")
                .Query<RadioButton>("radioButton-eventCommand-display31");
            RadioButton variable_toggle = RootElement.Q<VisualElement>("command_itemIncreaseDecrease")
                .Query<RadioButton>("radioButton-eventCommand-display32");
            IntegerField constant = RootElement.Q<VisualElement>("command_itemIncreaseDecrease")
                .Query<IntegerField>("constantNum");
            VisualElement variable = RootElement.Q<VisualElement>("command_itemIncreaseDecrease")
                .Query<VisualElement>("variable");

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

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] == "1")
                selectID = variableIDList.IndexOf(EventDataModels[EventIndex].eventCommands[EventCommandIndex]
                    .parameters[3]);
            else
                constant.value =
                    int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3]);
            if (selectID > variableNameList.Count)
                selectID = 0;
            if (selectID == -1)
            {
                selectID = 0;
                if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] == "1" && variableIDList.Count > 0)
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex]
                        .parameters[3] = variableIDList[0];
                }
            }

            var variablePopupField = new PopupFieldBase<string>(variableNameList, selectID);

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] == "0")
            {
                variable_toggle.value = false;
                variable.SetEnabled(false);
                constant_toggle.value = true;
                constant.SetEnabled(true);
            }
            else
            {
                variable_toggle.value = true;
                variable.SetEnabled(true);
                constant_toggle.value = false;
                constant.SetEnabled(false);
            }
            
            
            var defaultConstantSelect = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {constant_toggle, variable_toggle},
                defaultConstantSelect, new List<System.Action>
                {
                    //定数
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] = "0";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] =
                            constant.value.ToString();
                        Save(EventDataModels[EventIndex]);

                        variable_toggle.value = false;
                        variable.SetEnabled(false);
                        constant.SetEnabled(true);
                    },
                    //変数
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] = "1";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] =
                            variableIDList[variablePopupField.index];

                        Save(EventDataModels[EventIndex]);
                        variable.SetEnabled(true);
                        constant_toggle.value = false;
                        constant.SetEnabled(false);

                    }
                });

            if (constant.value < 1)
                constant.value = 1;
            constant.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (constant.value < 1)
                    constant.value = 1;

                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] =
                    constant.value.ToString();

                Save(EventDataModels[EventIndex]);
            });

            BaseInputFieldHandler.IntegerFieldCallback(constant, evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] =
                    constant.value.ToString();
                Save(EventDataModels[EventIndex]);
            }, 1, 9999);
            
            variable.Clear();
            variable.Add(variablePopupField);
            variablePopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] =
                    variableIDList[variablePopupField.index];
                Save(EventDataModels[EventIndex]);
            });
        }
    }
}