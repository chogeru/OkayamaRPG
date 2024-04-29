using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.GameProgress
{
    public class GameTimer : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_self_timer_control.uxml";

        public GameTimer(
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
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("1");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }


            IntegerField minute = RootElement.Q<VisualElement>("command_timerControl").Query<IntegerField>("minute");
            IntegerField second = RootElement.Q<VisualElement>("command_timerControl").Query<IntegerField>("second");
            RadioButton activation = RootElement.Q<VisualElement>("command_timerControl").Query<RadioButton>("radioButton-eventCommand-display107");
            RadioButton stop = RootElement.Q<VisualElement>("command_timerControl").Query<RadioButton>("radioButton-eventCommand-display108");

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] == "0")
                stop.value = true;
            else
                activation.value = true;
            minute.SetEnabled(activation.value);
            second.SetEnabled(activation.value);
            
            var defaultSelect = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {stop, activation},
                defaultSelect, new List<System.Action>
                {
                    //停止
                    () =>
                    {
                        minute.SetEnabled(activation.value);
                        second.SetEnabled(activation.value);

                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "0";
                        Save(EventDataModels[EventIndex]);
                    },
                    //始動
                    () =>
                    {
                        minute.SetEnabled(activation.value);
                        second.SetEnabled(activation.value);

                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "1";
                        Save(EventDataModels[EventIndex]);
                    }
                });

            

            minute.value = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]) / 60;
            BaseInputFieldHandler.IntegerFieldCallback(minute, evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    (minute.value * 60 + second.value).ToString();
                Save(EventDataModels[EventIndex]);
            }, 0, 99);
            second.value = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]) % 60;
            BaseInputFieldHandler.IntegerFieldCallback(second, evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    (minute.value * 60 + second.value).ToString();
                Save(EventDataModels[EventIndex]);
            }, 0, 59);
        }
    }
}