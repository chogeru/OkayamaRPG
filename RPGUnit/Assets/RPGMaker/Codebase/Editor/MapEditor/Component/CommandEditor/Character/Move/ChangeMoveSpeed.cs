using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Character.Move
{
    public class ChangeMoveSpeed : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_change_move_speed.uxml";

        public static List<string> _speedNameList = new List<string>
                    {"WORD_0847", "WORD_0848", "WORD_0849", "WORD_0985", "WORD_0851", "WORD_0852"};

        public ChangeMoveSpeed(
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
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("3");
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            VisualElement moveSpeed = RootElement.Query<VisualElement>("moveSpeed");
            moveSpeed.Clear();
            var moveSpeedTextDropdownChoices =
                EditorLocalize.LocalizeTexts(_speedNameList);
            var num = int.Parse(
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0]);

            var moveSpeedPopupField = new PopupFieldBase<string>(moveSpeedTextDropdownChoices, num);
            moveSpeed.Add(moveSpeedPopupField);
            moveSpeedPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                    moveSpeedTextDropdownChoices.IndexOf(moveSpeedPopupField.value).ToString();
                Save(EventDataModels[EventIndex]);
            });


            EventManagementService.SaveEvent(EventDataModels[EventIndex]);
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

        }
    }
}
