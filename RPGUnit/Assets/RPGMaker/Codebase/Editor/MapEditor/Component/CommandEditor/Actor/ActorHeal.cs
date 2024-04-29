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
    public class ActorHeal : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_actor_heal.uxml";

        public ActorHeal(
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
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("-1");
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            RadioButton fix_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display63");
            RadioButton variable_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display64");
            VisualElement fix = RootElement.Query<VisualElement>("fix");
            VisualElement variable = RootElement.Query<VisualElement>("variable");

            var flagDataModel =
                DatabaseManagementService.LoadFlags();
            var variableNameList = new List<string>();
            var variableIDList = new List<string>();
            var selectID = 0;
            for (var i = 0; i < flagDataModel.variables.Count; i++)
            {
                if (flagDataModel.variables[i].name == "")
                    variableNameList.Add(EditorLocalize.LocalizeText("WORD_1518"));
                else
                    variableNameList.Add(flagDataModel.variables[i].name);

                variableIDList.Add(flagDataModel.variables[i].id);
            }

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] == "1")
                selectID = variableIDList.IndexOf(EventDataModels[EventIndex]
                    .eventCommands[EventCommandIndex]
                    .parameters[1]);
            if (selectID == -1)
            {
                selectID = 0;
                if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] == "1" && variableIDList.Count > 0)
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex]
                        .parameters[1] = variableIDList[0];
                    Save(EventDataModels[EventIndex]);
                }
            }

            var variablePopupField = new PopupFieldBase<string>(variableNameList, selectID);

            var actorDataModels =
                DatabaseManagementService.LoadCharacterActor().FindAll(actor => actor.charaType == (int) ActorTypeEnum.ACTOR);
            var characterActorNameList = EditorLocalize.LocalizeTexts(new List<string> {"WORD_0892"});
            var characterActorIDList = EditorLocalize.LocalizeTexts(new List<string> {"-1"});

            selectID = 0;
            for (var i = 0; i < actorDataModels.Count; i++)
            {
                characterActorNameList.Add(actorDataModels[i].basic.name);
                characterActorIDList.Add(actorDataModels[i].uuId);
            }

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] == "0")
                selectID = characterActorIDList.IndexOf(EventDataModels[EventIndex]
                    .eventCommands[EventCommandIndex].parameters[1]);
            if (selectID == -1)
                selectID = 0;
            var fixPopupField = new PopupFieldBase<string>(characterActorNameList, selectID);


            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] == "0")
                fix_toggle.value = true;
            else
                variable_toggle.value = true;
            
            var defaultSelect = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {fix_toggle, variable_toggle},
                defaultSelect, new List<System.Action>
                {
                    //固定
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "0";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                            characterActorIDList[fixPopupField.index];

                        fix.SetEnabled(true);
                        variable.SetEnabled(false);
                        Save(EventDataModels[EventIndex]);
                    },
                    //変数
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "1";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                            variableIDList[variablePopupField.index];
                        fix.SetEnabled(false);
                        variable.SetEnabled(true);
                        Save(EventDataModels[EventIndex]);
                    }
                });
            
            fix.Clear();
            fix.Add(fixPopupField);
            fixPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    characterActorIDList[fixPopupField.index];

                Save(EventDataModels[EventIndex]);
            });
            
            variable.Clear();
            variable.Add(variablePopupField);
            variablePopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    variableIDList[variablePopupField.index];
                Save(EventDataModels[EventIndex]);
            });
        }
    }
}