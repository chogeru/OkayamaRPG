using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.SystemSetting
{
    public class TwoToggle : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_two_toggle.uxml";


        public TwoToggle(
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


            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count != 1)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }


            Label label = RootElement.Q<VisualElement>("systemSetting_two_toggle").Query<Label>("Label");
            label.text = "♦";
            switch ((EventEnum) EventDataModels[EventIndex].eventCommands[EventCommandIndex].code)
            {
                case EventEnum.EVENT_CODE_SYSTEM_IS_SAVE:
                    label.text += EditorLocalize.LocalizeText("WORD_1075");
                    break;
                case EventEnum.EVENT_CODE_SYSTEM_IS_MENU:
                    label.text += EditorLocalize.LocalizeText("WORD_1077");
                    break;
                case EventEnum.EVENT_CODE_SYSTEM_IS_ENCOUNT:
                    label.text += EditorLocalize.LocalizeText("WORD_1078");
                    break;
                case EventEnum.EVENT_CODE_SYSTEM_IS_SORT:
                    label.text += EditorLocalize.LocalizeText("WORD_1079");
                    break;
            }

            RadioButton no = RootElement.Q<VisualElement>("systemSetting_two_toggle").Query<RadioButton>("radioButton-eventCommand-display162");
            RadioButton yes = RootElement.Q<VisualElement>("systemSetting_two_toggle").Query<RadioButton>("radioButton-eventCommand-display161");

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] == "0")
                no.value = true;
            else if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] == "1")
                yes.value = true;
            else yes.value = true;
            
            var defaultSelect = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {no, yes},
                defaultSelect, new List<System.Action>
                {
                    //禁止
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "0";
                        Save(EventDataModels[EventIndex]);
                    },
                    //許可
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "1";
                        Save(EventDataModels[EventIndex]);
                    }
                });
        }
    }
}