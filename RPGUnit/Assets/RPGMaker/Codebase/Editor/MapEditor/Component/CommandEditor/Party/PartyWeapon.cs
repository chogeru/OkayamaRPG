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
    public class PartyWeapon : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_party_weapon.uxml";

        public PartyWeapon(
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

            var weaponDataModels =
                DatabaseManagementService.LoadWeapon();
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count == 0)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters
                    .Add(weaponDataModels.Count > 0 ? weaponDataModels[0].basic.id : "");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("1");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");

                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            VisualElement weapon = RootElement
                .Query<VisualElement>("weapon");

            var weaponNameList = new List<string>();
            var weaponIDList = new List<string>();
            var selectID = 0;
            for (var i = 0; i < weaponDataModels.Count; i++)
            {
                weaponNameList.Add(weaponDataModels[i].basic.name);
                weaponIDList.Add(weaponDataModels[i].basic.id);
            }
            
            if (weaponDataModels.Count == 0)
            {
                VisualElement partyArea = RootElement.Query<VisualElement>("party_area");
                partyArea.style.display = DisplayStyle.None;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "";
                Save(EventDataModels[EventIndex]);
                return;
            }

            selectID = weaponIDList.IndexOf(EventDataModels[EventIndex].eventCommands[EventCommandIndex]
                .parameters[0]);
            if (selectID == -1)
            {
                selectID = 0;
                if (weaponDataModels.Count > 0)
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = weaponDataModels[0].basic.id;
                    Save(EventDataModels[EventIndex]);
                }
            }

            var weaponPopupField = new PopupFieldBase<string>(weaponNameList, selectID);
            weapon.Clear();
            weapon.Add(weaponPopupField);
            weaponPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                    weaponIDList[weaponPopupField.index];
                Save(EventDataModels[EventIndex]);
            });

            RadioButton on = RootElement.Query<RadioButton>("radioButton-eventCommand-display33");
            RadioButton off = RootElement.Query<RadioButton>("radioButton-eventCommand-display34");
            Toggle equipToggle = RootElement.Query<Toggle>("equip");

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] == "0")
            {
                equipToggle.SetEnabled(false);
                on.value = true;
            }
            else if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] == "1")
            {
                if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] == "1")
                    equipToggle.value = true;
                off.value = true;
            }
            
            var defaultSelect = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {on, off},
                defaultSelect, new List<System.Action>
                {
                    //増やす
                    () =>
                    {
                        equipToggle.SetEnabled(false);
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "0";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] = "0";
                        equipToggle.value = false;
                        Save(EventDataModels[EventIndex]);
                    },
                    //減らす
                    () =>
                    {
                        equipToggle.SetEnabled(true);
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "1";
                        Save(EventDataModels[EventIndex]);
                    }
                });
            
            equipToggle.RegisterValueChangedCallback(o =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] =
                    equipToggle.value ? "1" : "0";
                Save(EventDataModels[EventIndex]);
            });

            RadioButton constant_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display35");
            RadioButton variable_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display36");
            IntegerField constant = RootElement
                .Query<IntegerField>("constantNum");
            VisualElement variable = RootElement
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
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] = constant.value.ToString();
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