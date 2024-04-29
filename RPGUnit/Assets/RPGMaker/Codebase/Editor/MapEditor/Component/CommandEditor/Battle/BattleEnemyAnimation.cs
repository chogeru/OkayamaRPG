using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Battle
{
    /// <summary>
    ///     [戦闘アニメーションの表示]のコマンド設定枠の表示物
    /// </summary>
    public class BattleEnemyAnimation : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_battle_enemy_animation.uxml";

        private EventCommand _targetCommand;

        public BattleEnemyAnimation(
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

            var enemyList = GetEnemyNameList();
            var animationDataList = DatabaseManagementService.LoadAnimation();
            _targetCommand = EventDataModels[EventIndex].eventCommands[EventCommandIndex];
            if (_targetCommand.parameters.Count == 0)
            {
                // 敵グループでの登録番号（フロントビューの場合は1~8、サイドビューの場合は1~6）
                _targetCommand.parameters.Add("1");
                // アニメーションのID
                _targetCommand.parameters.Add(animationDataList[0].id);
                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }
            
            if (enemyList.Count == 0)
            {
                VisualElement enemyArea = RootElement.Q<VisualElement>("battle_enemy_animation")
                    .Query<VisualElement>("enemy_area");
                enemyArea.style.display = DisplayStyle.None;
                _targetCommand.parameters[0] = "1";
                Save(EventDataModels[EventIndex]);
                return;
            }
            //エネミードロップダウン
            VisualElement enemySelect = RootElement.Q<VisualElement>("battle_enemy_animation")
                .Query<VisualElement>("enemy_select");
            var memberNo = 0;
            if (int.TryParse(_targetCommand.parameters[0], out memberNo)) memberNo -= 1; // 1から始まる番号で格納されているのでインデックス用に調整
            
            // 敵グループ内の敵キャラドロップダウン
            var enemyPopupField = new PopupFieldBase<string>(enemyList, memberNo);
            enemySelect.Clear();
            enemySelect.Add(enemyPopupField);
            enemyPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[0] = (enemyPopupField.index + 1).ToString();
                Save(EventDataModels[EventIndex]);
            });

            //アニメーションのドロップダウン
            VisualElement animationSelect = RootElement.Q<VisualElement>("battle_enemy_animation")
                .Query<VisualElement>("enemy_animation");
            var animationIndex = -1;

            //選択肢に名前を表示売る際に一時的に使用するList
            var animationNameList = new List<string>();
            var animationIdList = new List<string>();
            for (var i = 0; i < animationDataList.Count; i++)
            {
                animationNameList.Add(animationDataList[i].particleName);
                animationIdList.Add(animationDataList[i].id);
            }

            animationIndex = animationIdList.IndexOf(_targetCommand.parameters[1]);
            bool isNone = false;
            if (animationIndex == -1)
            {
                animationIndex = 0;
                isNone = true;
            }

            var animationPopupField = new PopupFieldBase<string>(animationNameList, animationIndex);
            animationSelect.Clear();
            animationSelect.Add(animationPopupField);
            animationPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[1] = animationIdList[animationPopupField.index];
                Save(EventDataModels[EventIndex]);
            });

            //設定しているデータがなかった場合、「なし」を表示させる
            if (isNone)
            {
                animationPopupField.ChangeButtonText(EditorLocalize.LocalizeText("WORD_0113"));
            }

        }
    }
}