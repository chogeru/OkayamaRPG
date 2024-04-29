using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Screen
{
    public class DisplayWeather : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_display_weather.uxml";

        public DisplayWeather(
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
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("1");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("255");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            var strengthSliderArea = RootElement.Query<VisualElement>("strength_sliderArea");
            SliderAndFiledBase.IntegerSliderCallBack(strengthSliderArea, 1, 9, "",
                int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]), evt =>
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                        ((int) evt).ToString();
                    Save(EventDataModels[EventIndex]);
               });
            
            VisualElement type = RootElement.Q<VisualElement>("command_changeWeatherSettings")
                .Query<VisualElement>("type");

            var syntheticList = EditorLocalize.LocalizeTexts(new List<string>
                {"WORD_0113", "WORD_1227", "WORD_1228", "WORD_1229"});
            var selectID = 0;
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] != null)
                selectID = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0]);
            if (selectID == -1)
                selectID = 0;
            var syntheticPopupField = new PopupFieldBase<string>(syntheticList, selectID);
            type.Clear();
            type.Add(syntheticPopupField);
            syntheticPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                    syntheticList.IndexOf(syntheticPopupField.value).ToString();
                Save(EventDataModels[EventIndex]);
            });
            

            IntegerField flame = RootElement.Q<VisualElement>("command_changeWeatherSettings")
                .Query<IntegerField>("flame");
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] != null)
                flame.value =
                    int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2]);
            flame.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (flame.value < 0)
                    flame.value = 0;
                else if (flame.value > 999)
                    flame.value = 999;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] =
                    flame.value.ToString();
                Save(EventDataModels[EventIndex]);
            });

            Toggle wait_toggle = RootElement.Q<VisualElement>("command_changeWeatherSettings")
                .Query<Toggle>("wait_toggle");
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] == "1")
                wait_toggle.value = true;

            wait_toggle.RegisterValueChangedCallback(o =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] =
                    wait_toggle.value ? "1" : "0";
                Save(EventDataModels[EventIndex]);
            });
        }
    }
}