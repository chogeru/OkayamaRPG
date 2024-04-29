using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Battle
{
    public class BattleEnemyAppearance : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_battle_enemy_appearance.uxml";

        public BattleEnemyAppearance(
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

            var enemyDropdownChoices = GetEnemyNameList();

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count != 1)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }
            
            if (enemyDropdownChoices.Count == 0)
            {
                VisualElement enemyArea = RootElement.Q<VisualElement>("battle_enemy_appearance")
                    .Query<VisualElement>("enemy_area");
                enemyArea.style.display = DisplayStyle.None;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "0";
                Save(EventDataModels[EventIndex]);
                return;
            }


            //エネミードロップダウン
            VisualElement enemySelect = RootElement.Q<VisualElement>("battle_enemy_appearance")
                .Query<VisualElement>("enemy_select");
            int.TryParse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0], out var enemyListId);

            var enemyDropdownPopupField = new PopupFieldBase<string>(enemyDropdownChoices, enemyListId);
            enemySelect.Clear();
            enemySelect.Add(enemyDropdownPopupField);
            enemyDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = enemyDropdownPopupField.index.ToString();
                Save(EventDataModels[EventIndex]);
            });
        }
    }
}