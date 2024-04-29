using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Message
{
    public class InputNumber : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_input_number.uxml";


        public InputNumber(
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
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("1");
                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }


            VisualElement variable =
                RootElement.Q<VisualElement>("command_inputNumber").Query<VisualElement>("Variable");
            var flagDataModel =
                DatabaseManagementService.LoadFlags();
            var variableDropdownChoices = new List<string>();
            var variableNameDropdownChoices = new List<string>();
            var variablesID = 0;
            for (var i = 0; i < flagDataModel.variables.Count; i++)
            {
                variableDropdownChoices.Add(flagDataModel.variables[i].id);
                if (flagDataModel.variables[i].name != "")
                    variableNameDropdownChoices.Add(flagDataModel.variables[i].name);
                else
                    variableNameDropdownChoices.Add(EditorLocalize.LocalizeText("WORD_1518"));
                //初期値探索
                if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] == flagDataModel.variables[i].id)
                    variablesID = i;
            }

            var variablePopupField = new PopupFieldBase<string>(variableNameDropdownChoices, variablesID);
            variable.Clear();
            variable.Add(variablePopupField);
            variablePopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                    variableDropdownChoices[variablePopupField.index];
                Save(EventDataModels[EventIndex]);
            });

            IntegerField digits = RootElement.Query<IntegerField>("Digits");
            digits.value = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]);
            digits.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (digits.value > 8)
                    digits.value = 8;
                else if (digits.value < 1)
                    digits.value = 1;

                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    digits.value.ToString();
                Save(EventDataModels[EventIndex]);
            });
            EventManagementService.SaveEvent(EventDataModels[EventIndex]);
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
        }
    }
}