using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Runtime.Event.Character;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Character.Move
{
    public class StepMove : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_step_move.uxml";

        public static List<string> _typeNameList = new List<string> {
                "WORD_2803", "WORD_2804", "WORD_2805", "WORD_2806",
                "WORD_2807", "WORD_2808", "WORD_2809", "WORD_2810",
                "WORD_2811", "WORD_1002", "WORD_1003", "WORD_0980" };
        public static int _jumpIndex = 11;

        public StepMove(
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
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            VisualElement positionContainer = RootElement.Q<VisualElement>("positionContainer");

            var jumpIndex = StepMoveProcessor._jumpIndex;
            var selectID = 0;
            VisualElement type = RootElement.Query<VisualElement>("type");
            var typeNameList = EditorLocalize.LocalizeTexts(_typeNameList);
            selectID = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0]);
            var typePopupField = new PopupFieldBase<string>(typeNameList, selectID);
            type.Clear();
            type.Add(typePopupField);
            typePopupField.RegisterValueChangedCallback(evt =>
            {
                var index = typeNameList.IndexOf(typePopupField.value);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = index.ToString();
                Save(EventDataModels[EventIndex]);
                positionContainer.SetEnabled(index == jumpIndex);
            });

            positionContainer.SetEnabled(selectID == jumpIndex);
            IntegerField positionX = RootElement.Q<IntegerField>("positionX");
            IntegerField positionY = RootElement.Q<IntegerField>("positionY");

            positionX.value = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]);
            positionX.RegisterCallback<FocusOutEvent>(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    positionX.value.ToString();
                Save(EventDataModels[EventIndex]);
            });

            positionY.value = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2]);
            positionY.RegisterCallback<FocusOutEvent>(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] =
                    positionY.value.ToString();
                Save(EventDataModels[EventIndex]);
            });

        }
    }
}
