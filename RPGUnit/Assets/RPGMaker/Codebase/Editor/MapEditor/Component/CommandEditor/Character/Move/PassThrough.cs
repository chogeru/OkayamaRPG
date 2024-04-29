using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Character.Move
{
    public class PassThrough : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_pass_through.uxml";

        public PassThrough(
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
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }


            Toggle on = RootElement.Q<VisualElement>("command_passThrough").Query<Toggle>("ON");
            Toggle off = RootElement.Q<VisualElement>("command_passThrough").Query<Toggle>("OFF");
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] == "0")
                on.value = true;
            else
                off.value = true;
            on.RegisterValueChangedCallback(o =>
            {
                if (!on.value)
                {
                    on.value = false;
                    off.value = true;
                }

                if (on.value)
                {
                    off.value = false;
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "0";
                    Save(EventDataModels[EventIndex]);
                }
            });
            off.RegisterValueChangedCallback(o =>
            {
                if (!off.value)
                {
                    off.value = false;
                    on.value = true;
                }

                if (off.value)
                {
                    on.value = false;
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "255";
                    Save(EventDataModels[EventIndex]);
                }
            });
        }
    }
}
