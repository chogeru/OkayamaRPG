using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Screen
{
    public class DisplayChangeColor : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_display_change_color.uxml";

        public DisplayChangeColor(
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
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("255");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");

                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }


            ColorFieldBase colorPicker = RootElement.Q<VisualElement>("command_changeColorScreen")
                .Query<ColorFieldBase>("colorPicker");

            VisualElement template = RootElement.Q<VisualElement>("command_changeColorScreen")
                .Query<VisualElement>("template");

            var syntheticList =
                EditorLocalize.LocalizeTexts(new List<string>
                    {"WORD_3031", "WORD_1132", "WORD_1133", "WORD_1134", "WORD_1135"});
            var num = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[6]);
            if (num == -1)
                num = 0;
            var syntheticPopupField = new PopupFieldBase<string>(syntheticList, num);
            template.Clear();
            template.Add(syntheticPopupField);
            syntheticPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[6] =
                    syntheticPopupField.index.ToString();
                switch (syntheticList.IndexOf(syntheticPopupField.value))
                {
                    case 0:
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "0";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "0";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] = "0";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] = "0";
                        break;
                    case 1:
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "0";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "0";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] = "0";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] = "114";
                        break;
                    case 2:
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "48";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "32";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] = "32";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] = "140";
                        break;
                    case 3:
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "255";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "116";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] = "76";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] = "76";
                        break;
                    case 4:
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "25";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "25";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] = "102";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] = "134";
                        break;
                }

                var r = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0]);
                var g = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]);
                var b = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2]);
                var a = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3]);
                colorPicker.value = new Color32((byte) r, (byte) g, (byte) b, (byte) a);

                Save(EventDataModels[EventIndex]);
            });

            var r = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0]);
            var g = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]);
            var b = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2]);
            var a = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3]);
            colorPicker.value = new Color32((byte) r, (byte) g, (byte) b, (byte) a);
            colorPicker.RegisterValueChangedCallback(evt =>
            {
                Color32 co = colorPicker.value;
                var coR = co.r;
                var coG = co.g;
                var coB = co.b;
                var alpha = co.a;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                    coR.ToString(CultureInfo.InvariantCulture);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    coG.ToString(CultureInfo.InvariantCulture);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] =
                    coB.ToString(CultureInfo.InvariantCulture);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] =
                    alpha.ToString(CultureInfo.InvariantCulture);

                Save(EventDataModels[EventIndex]);
            });


            IntegerField flame = RootElement.Query<IntegerField>("flame");
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] != null)
                flame.value =
                    int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4]);
            flame.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (flame.value < 0)
                    flame.value = 0;
                else if (flame.value > 999)
                    flame.value = 999;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] =
                    flame.value.ToString();
                Save(EventDataModels[EventIndex]);
            });

            Toggle wait_toggle = RootElement.Q<VisualElement>("command_changeColorScreen").Query<Toggle>("wait_toggle");
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[5] == "1")
                wait_toggle.value = true;

            wait_toggle.RegisterValueChangedCallback(o =>
            {
                var num = 0;
                if (wait_toggle.value)
                    num = 1;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[5] =
                    num.ToString();
                Save(EventDataModels[EventIndex]);
            });
        }
    }
}