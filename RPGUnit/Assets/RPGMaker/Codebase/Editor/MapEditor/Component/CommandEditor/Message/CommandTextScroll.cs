using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Message
{
    public class CommandTextScroll : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_text_scroll.uxml";


        public CommandTextScroll(
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
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("2");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");

                var data = EventDataModels[EventIndex].eventCommands.ToList();
                var eventDatas = new EventDataModel.EventCommand(0, new List<string>(),
                    new List<EventDataModel.EventCommandMoveRoute>());
                eventDatas.code = (int) EventEnum.EVENT_CODE_MESSAGE_TEXT_SCROLL_ONE_LINE;
                eventDatas.parameters = new List<string> {"", EventIndex.ToString()};
                data.Insert(EventCommandIndex + 1, eventDatas);
                EventDataModels[EventIndex].eventCommands = data;
                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            Label title = RootElement.Q<VisualElement>("command_textScroll").Query<Label>("title");
            title.text = EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_1213");

            Button preview = RootElement.Q<VisualElement>("command_textScroll").Query<Button>("preview");
            preview.clickable.clicked += () => { };

            IntegerField speed = RootElement.Query<IntegerField>("speed");
            speed.value = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0]);
            speed.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (speed.value > 8)
                    speed.value = 8;
                else if (speed.value < 2)
                    speed.value = 2;


                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                    speed.value.ToString();
                Save(EventDataModels[EventIndex]);
            });
            Toggle toggle = RootElement.Q<VisualElement>("command_textScroll").Query<Toggle>("toggle");
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] == "1")
                toggle.value = true;
            toggle.RegisterValueChangedCallback(evt =>
            {
                if (toggle.value)
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "1";
                else
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "0";

                Save(EventDataModels[EventIndex]);
            });

            Button buttonPreview = RootElement.Query<Button>("preview");

            buttonPreview.clickable.clicked += () =>
            {
                var _databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;

                var uiSettingDataModel = _databaseManagementService.LoadUiSettingDataModel();
                var _sceneWindow =
                    WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.DatabaseSceneWindow) as
                        SceneWindow;
                var toggles = new List<int> {0, 0, 0};
                _sceneWindow.Create(SceneWindow.PreviewId.Scroll);
                var text = EventDataModels[EventIndex].eventCommands[EventCommandIndex + 1].parameters[0];

                _sceneWindow.GetScrollPreview().SetEventData(text,
                    int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0]));
                _sceneWindow.GetScrollPreview().SetUiData(uiSettingDataModel);
                _sceneWindow.Init();
                _sceneWindow.SetRenderingSize(1920, 1080);
                _sceneWindow.Render();
            };
        }
    }
}