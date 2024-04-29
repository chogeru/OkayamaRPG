using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.AudioVideo
{
    public class AudioBgsFadeOut : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_audio_bgs_fade_out.uxml";

        public AudioBgsFadeOut(
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
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("10");
                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }


            IntegerField second = RootElement.Query<IntegerField>("second");
            if (int.TryParse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0],
                out var i))
            {
                if (i < 1)
                    second.value = 1;
                else
                    second.value = i;
            }
            else
            {
                second.value = 1;
            }

            if (second.value < 1)
            {
                second.value = 1;
            }
            else if (second.value > 999)
            {
                second.value = 999;
            }

            BaseInputFieldHandler.IntegerFieldCallback(second, evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                    second.value.ToString();
                Save(EventDataModels[EventIndex]);
            }, 1, 999);
        }
    }
}