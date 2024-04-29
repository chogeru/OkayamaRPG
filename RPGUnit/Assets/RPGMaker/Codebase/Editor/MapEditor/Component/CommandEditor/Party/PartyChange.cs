using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Party
{
    public class PartyChange : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_party_change.uxml";


        public PartyChange(
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

            //各データ読み込み
            var characterActorDataModels = DatabaseManagementService.LoadCharacterActor();


            //初期データ生成
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count == 0)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add(characterActorDataModels[0].uuId);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("1");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            //アクターのリスト
            VisualElement actor = RootElement.Q<VisualElement>("command_replaceMember").Query<VisualElement>("actor");
            var characterActorNameList = new List<string>();
            var characterActorIDList = new List<string>();
            var selectID = 0;
            for (var i = 0; i < characterActorDataModels.Count; i++)
                //アクターのみリストアップ
                if (characterActorDataModels[i].charaType == (int) ActorTypeEnum.ACTOR)
                {
                    characterActorNameList.Add(characterActorDataModels[i].basic.name);
                    characterActorIDList.Add(characterActorDataModels[i].uuId);
                }

            //初期選択データ
            selectID = characterActorIDList.IndexOf(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0]);
            if (selectID == -1)
                selectID = 0;

            //プルダウン作成
            var actorPopupField = new PopupFieldBase<string>(characterActorNameList, selectID);
            actor.Clear();
            actor.Add(actorPopupField);
            actorPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = characterActorIDList[actorPopupField.index];
                Save(EventDataModels[EventIndex]);
            });

            //パーティ加入する、離脱するのトグル設定
            var toggleOnOffList = new List<RadioButton>();
            toggleOnOffList.Add(RootElement.Q<VisualElement>("command_replaceMember").Query<RadioButton>("radioButton-eventCommand-display17"));
            toggleOnOffList.Add(RootElement.Q<VisualElement>("command_replaceMember").Query<RadioButton>("radioButton-eventCommand-display18"));

            //トグル切り替え時の非表示設定領域
            var toggleElementList = new List<VisualElement>();
            toggleElementList.Add(RootElement.Query<VisualElement>("toggle_contents"));
            toggleElementList.Add(new VisualElement());

            //トグル切替時のAction
            var toggleActions = new List<Action>
            {
                //加入する
                () =>
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "1";
                    Save(EventDataModels[EventIndex]);
                },
                //離脱する
                () =>
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "0";
                    Save(EventDataModels[EventIndex]);
                }
            };

            int index = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]);
            if (index == 1) index = 0;
            else if (index == 0) index = 1;

            //トグルの登録
            new CommonToggleSelector().SetRadioInVisualElementSelector(toggleOnOffList, toggleElementList,
                index, toggleActions);

            //初期化する、しないのトグル設定
            var toggleInitializeList = new List<RadioButton>();
            toggleInitializeList.Add(RootElement.Q<VisualElement>("command_replaceMember").Query<RadioButton>("radioButton-eventCommand-display19"));
            toggleInitializeList.Add(RootElement.Q<VisualElement>("command_replaceMember").Query<RadioButton>("radioButton-eventCommand-display20"));

            //トグル切替時のAction
            var toggleInitializeActions = new List<Action>
            {
                //初期化する
                () =>
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] = "1";
                    Save(EventDataModels[EventIndex]);
                },
                //しない
                () =>
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] = "0";
                    Save(EventDataModels[EventIndex]);
                }
            };

            index = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2]);
            if (index == 1) index = 0;
            else if (index == 0) index = 1;

            //トグルの登録
            new CommonToggleSelector().SetRadioSelector(toggleInitializeList, index, toggleInitializeActions);
        }
    }
}