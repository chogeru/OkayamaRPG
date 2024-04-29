using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Message
{
    public class SelectItem : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_select_item.uxml";

        public SelectItem(
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
            var flagDataModel =
                DatabaseManagementService.LoadFlags();

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count == 0)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add(flagDataModel.variables[0].id);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("1");
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }


            VisualElement variable =
                RootElement.Q<VisualElement>("command_selectItem").Query<VisualElement>("Variable");

            var variableDropdownChoices = new List<string>();
            var variableNameDropdownChoices = new List<string>();
            var variablesID = 0;
            for (var i = 0; i < flagDataModel.variables.Count; i++)
            {
                variableDropdownChoices.Add(flagDataModel.variables[i].id);
                if (flagDataModel.variables[i].name == "")
                    variableNameDropdownChoices.Add(EditorLocalize.LocalizeText("WORD_1518"));
                else
                    variableNameDropdownChoices.Add(flagDataModel.variables[i].name);
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

            VisualElement itemType =
                RootElement.Q<VisualElement>("command_selectItem").Query<VisualElement>("ItemType");
            var itemTypeTextDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string> {"WORD_1212", "WORD_0130", "WORD_0549", "WORD_0550"});
            var itemTypePopupField = new PopupFieldBase<string>(itemTypeTextDropdownChoices,
                int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]));
            itemType.Clear();
            itemType.Add(itemTypePopupField);
            itemTypePopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    itemTypeTextDropdownChoices.IndexOf(itemTypePopupField.value).ToString();
                Save(EventDataModels[EventIndex]);
            });
        }
    }
}