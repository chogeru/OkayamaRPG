using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Map
{
    public class MapScroll : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_map_scroll.uxml";

        public MapScroll(
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
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("3");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }


            VisualElement direction = RootElement.Query<VisualElement>("direction");
            var directions = EditorLocalize.LocalizeTexts(new List<string>
                {"WORD_0815", "WORD_0813", "WORD_0814", "WORD_0812"});
            var selectID =
                int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex]
                    .parameters[0]);
            if (selectID == -1)
                selectID = 0;
            var directionPopupField = new PopupFieldBase<string>(directions, selectID);
            direction.Clear();
            direction.Add(directionPopupField);
            directionPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                    directions.IndexOf(directionPopupField.value).ToString();
                Save(EventDataModels[EventIndex]);
            });

            IntegerField distance = RootElement.Query<IntegerField>("distance");
            distance.value =
                int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]);
            if (distance.value > 100)
                distance.value = 100;
            else if (distance.value < 1)
                distance.value = 1;
            distance.RegisterCallback<FocusOutEvent>(e =>
            {
                if (distance.value > 100)
                    distance.value = 100;
                else if (distance.value < 1)
                    distance.value = 1;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    distance.value.ToString();
                Save(EventDataModels[EventIndex]);
            });

            VisualElement speed = RootElement.Query<VisualElement>("speed");
            var speeds =
                EditorLocalize.LocalizeTexts(new List<string>
                    {"WORD_0847", "WORD_0848", "WORD_0849", "WORD_0985", "WORD_0851", "WORD_0852"});
            selectID = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex]
                .parameters[2]);
            if (selectID == -1)
                selectID = 3;
            var speedPopupField = new PopupFieldBase<string>(speeds, selectID);
            speed.Clear();
            speed.Add(speedPopupField);
            speedPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] =
                    speeds.IndexOf(speedPopupField.value).ToString();
                Save(EventDataModels[EventIndex]);
            });

            Toggle wait = RootElement.Query<Toggle>("wait");
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] == "1")
                wait.value = true;
            wait.RegisterValueChangedCallback(o =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] =
                    wait.value ? "1" : "0";
                Save(EventDataModels[EventIndex]);
            });
        }
    }
}