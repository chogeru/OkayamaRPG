using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Character.Move
{
    public class ChangeMoveFrequency : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_change_move_frequency.uxml";

        public static List<string> _frequencyNameList = new List<string>
                    {"WORD_0854", "WORD_0855", "WORD_0850", "WORD_0856", "WORD_0857"};

        public ChangeMoveFrequency(
            VisualElement rootElement,
            List<EventDataModel> eventDataModels,
            int eventIndex,
            int eventDataIndex
        )
            : base(rootElement, eventDataModels, eventIndex, eventDataIndex) {
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
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("2");
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            VisualElement moveFrequency = RootElement.Query<VisualElement>("moveFrequency");
            moveFrequency.Clear();
            var moveFrequencyTextDropdownChoices =
                EditorLocalize.LocalizeTexts(_frequencyNameList);
            var num = int.Parse(
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0]);

            var moveFrequencyPopupField = new PopupFieldBase<string>(moveFrequencyTextDropdownChoices, num);
            moveFrequency.Add(moveFrequencyPopupField);
            moveFrequencyPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                    moveFrequencyTextDropdownChoices.IndexOf(moveFrequencyPopupField.value).ToString();
                Save(EventDataModels[EventIndex]);
            });


            EventManagementService.SaveEvent(EventDataModels[EventIndex]);
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

        }
    }
}
