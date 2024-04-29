using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.FlowControl
{
    public class Loop : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_loop.uxml";

        public Loop(
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
                var data = EventDataModels[EventIndex].eventCommands.ToList();
                var eventDatas = new List<EventDataModel.EventCommand>();
                eventDatas.Add(new EventDataModel.EventCommand(0, new List<string>(),
                    new List<EventDataModel.EventCommandMoveRoute>()));
                eventDatas.Add(new EventDataModel.EventCommand(0, new List<string>(),
                    new List<EventDataModel.EventCommandMoveRoute>()));
                SetEventData(ref eventDatas, 0, 0);
                SetEventData(ref eventDatas, 1, (int) EventEnum.EVENT_CODE_FLOW_LOOP_END);
                for (var i = 0; i < eventDatas.Count; i++)
                    data.Insert(EventCommandIndex + i + 1, eventDatas[i]);
                EventDataModels[EventIndex].eventCommands = data;

                Save(EventDataModels[EventIndex]);
            }
        }

        private void SetEventData(ref List<EventDataModel.EventCommand> dataList, int index, int code) {
            dataList[index].code = code;
            dataList[index].parameters = new List<string>();
        }
    }
}