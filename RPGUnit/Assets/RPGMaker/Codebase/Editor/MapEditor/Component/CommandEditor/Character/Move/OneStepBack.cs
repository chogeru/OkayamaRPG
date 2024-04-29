using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Character.Move
{
    public class OneStepBack : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_onestep_back.uxml";


        public OneStepBack(
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
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("1");
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }


            Toggle repeatOperation_toggle = RootElement.Q<VisualElement>("command_oneStepBack")
                .Query<Toggle>("repeatOperation_toggle");
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] == "1")
                repeatOperation_toggle.value = true;
            repeatOperation_toggle.RegisterValueChangedCallback(evt =>
            {
                var num = 0;
                if (repeatOperation_toggle.value)
                    num = 1;

                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                    num.ToString();
                Save(EventDataModels[EventIndex]);
            });
            Toggle moveSkip_toggle =
                RootElement.Q<VisualElement>("command_oneStepBack").Query<Toggle>("moveSkip_toggle");
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] == "1")
                moveSkip_toggle.value = true;
            moveSkip_toggle.RegisterValueChangedCallback(evt =>
            {
                var num = 0;
                if (moveSkip_toggle.value)
                    num = 1;

                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    num.ToString();
                Save(EventDataModels[EventIndex]);
            });
            Toggle waitComplete_toggle =
                RootElement.Q<VisualElement>("command_oneStepBack").Query<Toggle>("waitComplete_toggle");
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] == "1")
                waitComplete_toggle.value = true;
            waitComplete_toggle.RegisterValueChangedCallback(evt =>
            {
                var num = 0;
                if (waitComplete_toggle.value)
                    num = 1;

                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] =
                    num.ToString();
                Save(EventDataModels[EventIndex]);
            });
        }
    }
}