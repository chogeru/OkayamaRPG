using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Actor
{
    public class ChangeClass : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_change_class.uxml";

        public ChangeClass(
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
            var characterActorDataModels =
                DatabaseManagementService.LoadCharacterActor().FindAll(actor => actor.charaType == (int) ActorTypeEnum.ACTOR);
            var classDataModels = DatabaseManagementService.LoadCharacterActorClass();


            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count == 0)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters
                    .Add(characterActorDataModels[0].uuId);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters
                    .Add(classDataModels[0].id);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }


            // アクター
            VisualElement actor = RootElement.Query<VisualElement>("actor");
            var characterActorNameList = new List<string>();
            var characterActorIDList = new List<string>();
            for (var i = 0; i < characterActorDataModels.Count; i++)
            {
                characterActorNameList.Add(characterActorDataModels[i].basic.name);
                characterActorIDList.Add(characterActorDataModels[i].uuId);
            }

            var selectID = 0;
            selectID = characterActorIDList.IndexOf(EventDataModels[EventIndex]
                .eventCommands[EventCommandIndex].parameters[0]);
            if (selectID == -1)
                selectID = 0;
            var actorPopupField = new PopupFieldBase<string>(characterActorNameList, selectID);
            actor.Clear();
            actor.Add(actorPopupField);
            actorPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                    characterActorIDList[actorPopupField.index];
                Save(EventDataModels[EventIndex]);
            });

            // 職業設定
            VisualElement classElement = RootElement.Query<VisualElement>("class");
            var actorClassName = new List<string>();
            selectID = 0;
            for (var i = 0; i < classDataModels.Count; i++)
            {
                actorClassName.Add(classDataModels[i].basic.name);
                if (classDataModels[i].id == EventDataModels[EventIndex]
                    .eventCommands[EventCommandIndex].parameters[1])
                    selectID = i;
            }

            var classPopupField = new PopupFieldBase<string>(actorClassName, selectID);
            classElement.Clear();
            classElement.Add(classPopupField);
            classPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    classDataModels[classPopupField.index].id;
                Save(EventDataModels[EventIndex]);
            });

            // レベル保存
            Toggle lvelSave_toggle = RootElement.Query<Toggle>("lvelSave_toggle");
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] == "1")
                lvelSave_toggle.value = true;
            lvelSave_toggle.RegisterValueChangedCallback(o =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] =
                    lvelSave_toggle.value ? "1" : "0";
                Save(EventDataModels[EventIndex]);
            });
        }
    }
}