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
    ///     [戦闘行動の強制]のコマンド設定枠の表示物
    /// </summary>
    public class BattleEnemyForced : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_battle_enemy_forced.uxml";

        private EventCommand  _targetCommand;
        private VisualElement actorSelect;
        private RadioButton        actorSelectToggle;
        private VisualElement enemySelect;

        private RadioButton enemySelectToggle;

        public BattleEnemyForced(
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
            var actorList = DatabaseManagementService.LoadCharacterActor();
            var skillList = DatabaseManagementService.LoadSkillCustom();

            _targetCommand = EventDataModels[EventIndex].eventCommands[EventCommandIndex];
            if (_targetCommand.parameters.Count == 0)
            {
                // 行動主体（0: 敵キャラ、1: アクター）
                _targetCommand.parameters.Add("0");
                // 敵グループ内の敵キャラの登録番号またはアクターのID
                _targetCommand.parameters.Add("1");
                //何のスキルか(skillId)
                _targetCommand.parameters.Add(skillList[0].basic.id);
                //対象の選択（名称）
                _targetCommand.parameters.Add("0");
                //対象の番号
                _targetCommand.parameters.Add("-2");
                //旧データを救うための文字
                _targetCommand.parameters.Add("NEWDATA");
                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            //敵か味方か
            enemySelectToggle = RootElement.Q<VisualElement>("battle_forced").Query<RadioButton>("radioButton-eventCommand-display159");
            actorSelectToggle = RootElement.Q<VisualElement>("battle_forced").Query<RadioButton>("radioButton-eventCommand-display160");
            //トグルで変更された時に対象も変更するために保持
            enemySelect = RootElement.Q<VisualElement>("battle_forced").Query<VisualElement>("enemy_select_dropdown");
            actorSelect = RootElement.Q<VisualElement>("battle_forced").Query<VisualElement>("actor_dropdown");

            //　[行動主体]>[敵キャラ]のドロップダウン
            var memberNo = 0;
            if (_targetCommand.parameters[0] == "0" && int.TryParse(_targetCommand.parameters[1], out memberNo))
                memberNo -= 1; // 1から始まる番号で格納されているのでインデックス用に調整

            var enemyPopupField = new PopupFieldBase<string>(enemyList, memberNo);
            enemySelect.Clear();
            enemySelect.Add(enemyPopupField);
            enemyPopupField.RegisterValueChangedCallback(_ =>
            {
                _targetCommand.parameters[1] = (enemyPopupField.index + 1).ToString();
                Save(EventDataModels[EventIndex]);
            });

            // [行動主体]>[アクター]のドロップダウン
            var actorNameList = new List<string>();
            var actorIdList = new List<string>();
            var actorIndex = 0;
            if (_targetCommand.parameters[0] == "1")
            {
                actorIndex = actorList.FindIndex(v => v.uuId == _targetCommand.parameters[1]);
                if (actorIndex == -1) actorIndex = 0;
            }

            for (var i = 0; i < actorList.Count; i++)
            {
                actorNameList.Add(actorList[i].basic.name);
                actorIdList.Add(actorList[i].uuId);
            }

            var actorPopupField = new PopupFieldBase<string>(actorNameList, actorIndex);
            actorSelect.Clear();
            actorSelect.Add(actorPopupField);
            actorPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[1] = actorIdList[actorPopupField.index];
                Save(EventDataModels[EventIndex]);
            });
            
            // [行動主体]の各トグルの設定
            SwitchSubjectEditItem(_targetCommand.parameters[0]);
            var defaultSelect = _targetCommand.parameters[0] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {enemySelectToggle, actorSelectToggle},
                defaultSelect, new List<System.Action>
                {
                    //敵
                    () =>
                    {
                        _targetCommand.parameters[0] = "0";
                        _targetCommand.parameters[1] = (enemyPopupField.index + 1).ToString();
                        SwitchSubjectEditItem(_targetCommand.parameters[0]);
                        Save(EventDataModels[EventIndex]);
                    },
                    //アクター
                    () =>
                    {
                        _targetCommand.parameters[0] = "1";
                        _targetCommand.parameters[1] = actorIdList[actorPopupField.index];
                        SwitchSubjectEditItem(_targetCommand.parameters[0]);
                        Save(EventDataModels[EventIndex]);
                    }
                });
            
            //スキルドロップダウン
            VisualElement skillSelect =
                RootElement.Q<VisualElement>("battle_forced").Query<VisualElement>("skill_dropdown");
            var skillListIndex = skillList.FindIndex(v => v.basic.id == _targetCommand.parameters[2]);
            if (actorIndex == -1) actorIndex = 0;

            //選択肢に名前を表示売る際に一時的に使用するList
            var skillNameList = new List<string>();
            var skillIdList = new List<string>();
            for (var i = 0; i < skillList.Count; i++)
            {
                skillNameList.Add(skillList[i].basic.name);
                skillIdList.Add(skillList[i].basic.id);
            }

            var skillPopupField = new PopupFieldBase<string>(skillNameList, skillListIndex);
            skillSelect.Clear();
            skillSelect.Add(skillPopupField);
            skillPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[2] = skillIdList[skillPopupField.index];
                Save(EventDataModels[EventIndex]);
            });

            //対象ドロップダウン
            VisualElement targetSelect =
                RootElement.Q<VisualElement>("battle_forced").Query<VisualElement>("subject_dropdown");
            var targetListId = 0;
            //選択肢に名前を表示売る際に一時的に使用するList
            var targetName = new List<string> {"0", "1"};
            var targetNameChoices = EditorLocalize.LocalizeTexts(new List<string> {"WORD_1113", "WORD_0447"});
            for (var i = 1; i <= 8; i++)
            {
                var name = EditorLocalize.LocalizeText("WORD_1114");
                targetName.Add(name + i);
                targetNameChoices.Add(EditorLocalize.LocalizeText(name) + i);
            }

            targetListId = targetName.IndexOf(_targetCommand.parameters[3]);
            if (targetListId == -1)
                targetListId = 0;
            var targetPopupField = new PopupFieldBase<string>(targetNameChoices, targetListId);
            targetSelect.Clear();
            targetSelect.Add(targetPopupField);
            targetPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[3] = targetName[targetPopupField.index];
                _targetCommand.parameters[4] = (targetPopupField.index - 2).ToString();
                _targetCommand.parameters[5] = "NEWDATA";
                Save(EventDataModels[EventIndex]);
            });
        }

        /// <summary>
        ///     行動主体に基づいて有効にする設定項目を切り替える
        /// </summary>
        /// <param name="subject"></param>
        private void SwitchSubjectEditItem(string subject) {
            // 敵キャラ関係
            enemySelect.SetEnabled(enemySelectToggle.value);

            // アクター関係
            actorSelect.SetEnabled(actorSelectToggle.value);
        }
    }
}