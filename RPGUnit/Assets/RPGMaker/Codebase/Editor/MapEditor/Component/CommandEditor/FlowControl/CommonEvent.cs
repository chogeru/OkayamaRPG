using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.FlowControl
{
    public class CommonEvent : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_common_event.uxml";


        public CommonEvent(
            VisualElement rootElement,
            List<EventDataModel> eventDataModels,
            int eventIndex,
            int eventCommandIndex
        )
            : base(rootElement, eventDataModels, eventIndex, eventCommandIndex) {
        }

        public override void Invoke() {
            EventDataModel eventDataModel = EventDataModels[EventIndex];
            EventDataModel.EventCommand eventCommand = eventDataModel.eventCommands[EventCommandIndex];

            var commandTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SettingUxml);
            VisualElement commandFromUxml = commandTree.CloneTree();
            EditorLocalize.LocalizeElements(commandFromUxml);
            commandFromUxml.style.flexGrow = 1;
            RootElement.Add(commandFromUxml);

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            var eventCommonDataModels = EventManagementService.LoadEventCommon();

            var defauleIndex = -1;
            if (eventCommand.parameters.Any())
            {
                // 設定済みのコモンイベントを探す。
                for (var i = 0; i < eventCommonDataModels.Count; i++)
                {
                    if (eventCommonDataModels[i].eventId == eventCommand.parameters[0])
                    {
                        defauleIndex = i;
                        break;
                    }
                }
            }
            else
            {
                eventCommand.parameters.Add("0");
            }

            // 設定済みのコモンイベントが存在せず、コモンイベントが1つでも存在すれば、
            // 先頭のコモンイベントを既定として設定し選択状態とする。
            if (defauleIndex < 0)
            {
                defauleIndex = 0;
                if (eventCommonDataModels.Any())
                {
                    eventCommand.parameters[0] = eventCommonDataModels[defauleIndex].eventId;
                }

                EventManagementService.SaveEvent(eventDataModel);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            var commonEventPopupField = new PopupFieldBase<string>(
                eventCommonDataModels.Select(eventCommonDataModel => eventCommonDataModel.name).ToList(),
                defauleIndex);
            VisualElement commonEventVe =
                RootElement.Q<VisualElement>("command_commonEvent").Query<VisualElement>("commonEvent");
            commonEventVe.Clear();
            commonEventVe.Add(commonEventPopupField);
            commonEventPopupField.RegisterValueChangedCallback(evt =>
            {
                eventCommand.parameters[0] = eventCommonDataModels[commonEventPopupField.index].eventId;
                Save(eventDataModel);
            });
        }
    }
}