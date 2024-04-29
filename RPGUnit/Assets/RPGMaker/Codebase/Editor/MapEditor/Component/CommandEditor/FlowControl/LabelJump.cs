using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.FlowControl
{
    public class LabelJump : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_label_jump.uxml";


        public LabelJump(
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
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters
                    .Add(EditorLocalize.LocalizeText("WORD_0113"));
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            VisualElement labelJump =
                RootElement.Q<VisualElement>("command_labelJump").Query<VisualElement>("labelJump");
            var eventLabelNameList = new List<string> {EditorLocalize.LocalizeText("WORD_0113")};
            var selectId = 0;
            for (var i = 0; i < EventDataModels[EventIndex].eventCommands.Count; i++)
            {
                if (EventDataModels[EventIndex].eventCommands[i].code == (int) EventEnum.EVENT_CODE_FLOW_LABEL)
                {
                    eventLabelNameList.Add(EventDataModels[EventIndex].eventCommands[i].parameters[0]);
                }
            }

            for (var i = 0; i < eventLabelNameList.Count; i++)
            {
                if (eventLabelNameList[i] == EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0])
                {
                    selectId = i;
                    break;
                }
            }

            if (selectId == 0)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                    EditorLocalize.LocalizeText("WORD_0113");
                Save(EventDataModels[EventIndex]);
            }

            var commonEventPopupField = new PopupFieldBase<string>(eventLabelNameList, selectId);
            labelJump.Clear();
            labelJump.Add(commonEventPopupField);
            commonEventPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                    eventLabelNameList[commonEventPopupField.index];
                Save(EventDataModels[EventIndex]);
            });
        }
    }
}