using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Enemy;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Battle
{
    /// <summary>
    ///     [敵キャラの変身]のコマンド設定枠の表示物（バトルのみ）
    /// </summary>
    public class BattleEnemyTransform : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_battle_enemy_transform.uxml";

        private EventCommand           _targetCommand;

        public BattleEnemyTransform(
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
            var enemyTransformChoices = GetTransformEnemyList();

            _targetCommand = EventDataModels[EventIndex].eventCommands[EventCommandIndex];
            if (_targetCommand.parameters.Count == 0)
            {
                // 敵グループでの登録番号（フロントビューの場合は1~8、サイドビューの場合は1~6）
                _targetCommand.parameters.Add("1");
                // 変身後の敵キャラのID
                if (enemyTransformChoices.Count > 0)
                {
                    _targetCommand.parameters.Add(enemyTransformChoices[0].id);
                }
                else
                {
                    _targetCommand.parameters.Add("");
                }

                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            if (enemyTransformChoices.Count == 0)
            {
                VisualElement enemyArea = RootElement.Q<VisualElement>("battle_enemy_transform")
                    .Query<VisualElement>("enemy_area");
                enemyArea.style.display = DisplayStyle.None;
                _targetCommand.parameters[0] = "1";
                Save(EventDataModels[EventIndex]);
                return;
            }

            //エネミードロップダウン
            VisualElement enemySelect = RootElement.Q<VisualElement>("battle_enemy_transform")
                .Query<VisualElement>("enemy_select");
            var memberNo = 0;
            if (int.TryParse(_targetCommand.parameters[0], out memberNo)) memberNo -= 1; // 1から始まる番号で格納されているのでインデックス用に調整

            // 敵グループ内の敵キャラを選択するドロップメニュー
            var enemyPopupField = new PopupFieldBase<string>(enemyDropdownChoices, memberNo);
            enemySelect.Clear();
            enemySelect.Add(enemyPopupField);
            enemyPopupField.RegisterValueChangedCallback(_ =>
            {
                _targetCommand.parameters[0] = (enemyPopupField.index + 1).ToString();
                Save(EventDataModels[EventIndex]);
            });

            //エネミーグループ内のドロップダウン
            VisualElement enemyTransform = RootElement.Q<VisualElement>("battle_enemy_transform")
                .Query<VisualElement>("enemy_transform");
            var transformIndex = enemyTransformChoices.FindIndex(v => v.id == _targetCommand.parameters[1]);
            if (transformIndex == -1) transformIndex = 0;

            //選択肢に名前を表示売る際に一時的に使用するList
            var enemyNameList = new List<string>();
            var enemyIdList = new List<string>();
            for (var i = 0; i < enemyTransformChoices.Count; i++)
            {
                enemyNameList.Add(enemyTransformChoices[i].name);
                enemyIdList.Add(enemyTransformChoices[i].id);
            }

            // 変身後の敵キャラを選択するドロップメニュー
            var transformPopupField = new PopupFieldBase<string>(enemyNameList, transformIndex);
            enemyTransform.Clear();
            enemyTransform.Add(transformPopupField);
            transformPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[1] = enemyIdList[transformPopupField.index];
                Save(EventDataModels[EventIndex]);
            });
        }

        //敵Listの取得
        private List<EnemyDataModel> GetTransformEnemyList() {
            var enemyDataModels = DatabaseManagementService.LoadEnemy();
            return enemyDataModels.Where(v => v.deleted == 0).ToList();
        }
    }
}