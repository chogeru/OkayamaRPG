using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.SystemSetting
{
    public class ChangeWindowColor : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_change_window_color.uxml";

        public ChangeWindowColor(
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


            VisualElement colorWindow = RootElement.Q<VisualElement>("systemSetting_changeWindowColor")
                .Query<VisualElement>("color_window");

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count != 3)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            int r = 0, g = 0, b = 0;
            r = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0]);
            g = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]);
            b = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2]);
            var color = new Color32((byte) r, (byte) g, (byte) b, 255);
            colorWindow.style.backgroundColor = new StyleColor(color);


            Slider redSlider = RootElement.Q<VisualElement>("systemSetting_changeWindowColor")
                .Query<Slider>("red_slider");
            IntegerField redText = RootElement.Q<VisualElement>("systemSetting_changeWindowColor")
                .Query<IntegerField>("red_text");
            redText.maxLength = 3;
            redSlider.value =
                int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0]);
            redSlider.RegisterValueChangedCallback(evt =>
            {
                redText.value = (int) redSlider.value;
                color.r = (byte) redSlider.value;
                colorWindow.style.backgroundColor = new StyleColor(color);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                    ((int) redSlider.value).ToString();
                Save(EventDataModels[EventIndex]);
            });
            redText.value =
                int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0]);
            redText.RegisterCallback<FocusOutEvent>(evt =>
            {
                redSlider.value = redText.value;
                color.r = (byte) redText.value;
                colorWindow.style.backgroundColor = new StyleColor(color);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                    redText.value.ToString();
                Save(EventDataModels[EventIndex]);
            });
            Slider greenSlider = RootElement.Q<VisualElement>("systemSetting_changeWindowColor")
                .Query<Slider>("green_slider");
            IntegerField greenText = RootElement.Q<VisualElement>("systemSetting_changeWindowColor")
                .Query<IntegerField>("green_text");
            greenText.maxLength = 3;
            greenSlider.value =
                int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]);
            greenSlider.RegisterValueChangedCallback(evt =>
            {
                greenText.value = (int) greenSlider.value;
                color.g = (byte) greenSlider.value;
                colorWindow.style.backgroundColor = new StyleColor(color);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    ((int) greenSlider.value).ToString();
                Save(EventDataModels[EventIndex]);
            });
            greenText.value =
                int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]);
            greenText.RegisterCallback<FocusOutEvent>(evt =>
            {
                greenSlider.value = greenText.value;
                color.g = (byte) greenText.value;
                colorWindow.style.backgroundColor = new StyleColor(color);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    greenText.value.ToString();
                Save(EventDataModels[EventIndex]);
            });
            Slider blueSlider = RootElement.Q<VisualElement>("systemSetting_changeWindowColor")
                .Query<Slider>("blue_slider");
            IntegerField blueText = RootElement.Q<VisualElement>("systemSetting_changeWindowColor")
                .Query<IntegerField>("blue_text");
            blueText.maxLength = 3;
            blueSlider.value =
                int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2]);
            blueSlider.RegisterValueChangedCallback(evt =>
            {
                blueText.value = (int) blueSlider.value;
                color.b = (byte) blueSlider.value;
                colorWindow.style.backgroundColor = new StyleColor(color);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] =
                    ((int) blueSlider.value).ToString();
                Save(EventDataModels[EventIndex]);
            });
            blueText.value =
                int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2]);
            blueText.RegisterCallback<FocusOutEvent>(evt =>
            {
                blueSlider.value = blueText.value;
                color.b = (byte) blueText.value;
                colorWindow.style.backgroundColor = new StyleColor(color);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] =
                    blueText.value.ToString();
                Save(EventDataModels[EventIndex]);
            });
        }
    }
}