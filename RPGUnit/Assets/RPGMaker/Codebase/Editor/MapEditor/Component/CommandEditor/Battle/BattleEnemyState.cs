using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.State;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Battle
{
    public class BattleEnemyState : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_battle_enemy_state.uxml";

        public BattleEnemyState(
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

            var EnemyDropdownChoices = GetEnemyList();
            var StateDropdownChoices = _GetStateList();

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count != 3)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                if (StateDropdownChoices.Count > 0)
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters
                        .Add(StateDropdownChoices[0].id);
                else
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("");
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }
            
            if (EnemyDropdownChoices.Count == 0)
            {
                VisualElement enemyArea = RootElement.Q<VisualElement>("battle_enemy_state")
                    .Query<VisualElement>("enemy_area");
                enemyArea.style.display = DisplayStyle.None;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "0";
                Save(EventDataModels[EventIndex]);
                return;
            }


            //エネミードロップダウン
            VisualElement EnemySelect =
                RootElement.Q<VisualElement>("battle_enemy_state").Query<VisualElement>("enemy_select");
            int.TryParse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0], out var EnemyListId);
            
            //選択肢に名前を表示売る際に一時的に使用するList
            var CharacterName = new List<string>();
            var CharacterID = new List<string>();
            for (var i = 0; i < EnemyDropdownChoices.Count; i++)
            {
                CharacterName.Add(EnemyDropdownChoices[i].name);
                CharacterID.Add(EnemyDropdownChoices[i].id);
            }

            var EnemyDropdownPopupField =
                new PopupFieldBase<string>(GetEnemyNameList(true), EnemyListId);
            EnemySelect.Clear();
            EnemySelect.Add(EnemyDropdownPopupField);
            EnemyDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = EnemyDropdownPopupField.index.ToString();
                Save(EventDataModels[EventIndex]);
            });

            //操作
            RadioButton operation_on = RootElement.Q<VisualElement>("battle_enemy_state").Query<RadioButton>("radioButton-eventCommand-display157");
            RadioButton operation_off = RootElement.Q<VisualElement>("battle_enemy_state").Query<RadioButton>("radioButton-eventCommand-display158");
            //初期値
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] == "0")
            {
                operation_on.value = true;
                operation_off.value = false;
            }

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] == "1")
            {
                operation_off.value = true;
                operation_on.value = false;
            }
            
            var defaultSelect = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {operation_on, operation_off},
                defaultSelect, new List<System.Action>
                {
                    //付加
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "0";
                        Save(EventDataModels[EventIndex]);
                    },
                    //解除
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "1";
                        Save(EventDataModels[EventIndex]);
                    }
                });
            
            //ステートドロップダウン
            VisualElement StateSelect =
                RootElement.Q<VisualElement>("battle_enemy_state").Query<VisualElement>("state_select");
            var StateListId = -1;
            for (var i = 0; i < StateDropdownChoices.Count; i++)
                if (StateDropdownChoices[i].id == EventDataModels[EventIndex]
                    .eventCommands[EventCommandIndex].parameters[2])
                {
                    StateListId = i;
                    break;
                }

            //選択肢に名前を表示売る際に一時的に使用するList
            var StateName = new List<string>();
            var StateID = new List<string>();
            for (var i = 0; i < StateDropdownChoices.Count; i++)
            {
                StateName.Add(StateDropdownChoices[i].name);
                StateID.Add(StateDropdownChoices[i].id);
            }

            bool isNone = false;

            if (StateListId == -1)
            {
                StateListId = 0;
                isNone = true;
            }
            var StateDropdownPopupField =
                new PopupFieldBase<string>(StateName, StateListId);

            //設定しているステートがデータ上になかった場合、「なし」を表示させる
            if (isNone)
            {
                StateDropdownPopupField.ChangeButtonText(EditorLocalize.LocalizeText("WORD_0113"));
            }

            StateSelect.Clear();
            StateSelect.Add(StateDropdownPopupField);
            StateDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] =
                    StateID[StateDropdownPopupField.index];
                Save(EventDataModels[EventIndex]);
            });
        }

        //ステートListの取得
        private List<StateDataModel> _GetStateList() {
            var stateDataModels =
                DatabaseManagementService.LoadStateEdit();
            var fileNames = new List<StateDataModel>();
            for (var i = 0; i < stateDataModels.Count; i++) fileNames.Add(stateDataModels[i]);

            return fileNames;
        }
    }
}