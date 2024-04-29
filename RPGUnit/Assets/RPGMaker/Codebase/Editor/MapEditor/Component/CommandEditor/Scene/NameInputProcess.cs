using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Scene
{
    public class NameInputProcess : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_name_input_process.uxml";


        public NameInputProcess(
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
                DatabaseManagementService.LoadCharacterActor();
            VisualElement actor = RootElement.Query<VisualElement>("actor");
            var characterActorNameList = new List<string>();
            var characterActorID = new List<string>();
            var selectID = 0;
            for (var i = 0; i < characterActorDataModels.Count; i++)
                //アクターのみリストアップ
                if (characterActorDataModels[i].charaType == (int) ActorTypeEnum.ACTOR)
                {
                    characterActorNameList.Add(characterActorDataModels[i].basic.name);
                    characterActorID.Add(characterActorDataModels[i].uuId);
                }

            if (characterActorNameList.Count == 0)
                characterActorNameList.Add("アクターがいません");


            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count == 0)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add(characterActorID[0]);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("8");
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }


            selectID = characterActorID.IndexOf(EventDataModels[EventIndex]
                .eventCommands[EventCommandIndex].parameters[0]);
            if (selectID == -1)
                selectID = 0;
            var actorPopupField = new PopupFieldBase<string>(characterActorNameList, selectID);
            actor.Clear();
            actor.Add(actorPopupField);
            actorPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                    characterActorID[actorPopupField.index];
                Save(EventDataModels[EventIndex]);
            });

            IntegerField num = RootElement.Query<IntegerField>("num");
            num.value = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex]
                .parameters[1]);
            if (num.value > 16)
                num.value = 16;
            else if (num.value < 1)
                num.value = 1;
            num.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (num.value > 16)
                    num.value = 16;
                else if (num.value < 1)
                    num.value = 1;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    num.value.ToString();
                Save(EventDataModels[EventIndex]);
            });
        }
    }
}