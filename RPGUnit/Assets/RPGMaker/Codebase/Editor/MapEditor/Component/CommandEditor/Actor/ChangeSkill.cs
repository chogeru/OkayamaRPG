using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCustom;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Actor
{
    /// <summary>
    ///     [スキルの増減]のコマンド設定枠の表示物
    /// </summary>
    public class ChangeSkill : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_change_skill.uxml";

        private EventCommand _targetCommand;

        public ChangeSkill(
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

            var skillCustom = _GetSkillList();
            _targetCommand = EventDataModels[EventIndex].eventCommands[EventCommandIndex];
            if (_targetCommand.parameters.Count == 0)
            {
                _targetCommand.parameters.Add("0");
                _targetCommand.parameters.Add("-1");
                _targetCommand.parameters.Add("0");
                _targetCommand.parameters.Add(skillCustom[0].basic.id);
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            RadioButton fix_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display83");
            RadioButton variable_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display84");
            VisualElement fix = RootElement.Query<VisualElement>("fix");
            VisualElement variable = RootElement.Query<VisualElement>("variable");

            var characterActorDataModels = DatabaseManagementService.LoadCharacterActor().FindAll(actor => actor.charaType == (int) ActorTypeEnum.ACTOR);
            var characterActorNameList = EditorLocalize.LocalizeTexts(new List<string> {"WORD_0892"});
            var characterActorIDList = EditorLocalize.LocalizeTexts(new List<string> {"-1"});

            var selectID = 0;
            for (var i = 0; i < characterActorDataModels.Count; i++)
            {
                characterActorNameList.Add(characterActorDataModels[i].basic.name);
                characterActorIDList.Add(characterActorDataModels[i].uuId);
            }

            if (_targetCommand.parameters[0] == "0")
                selectID = characterActorIDList.IndexOf(_targetCommand.parameters[1]);
            if (selectID == -1)
                selectID = 0;
            var fixPopupField = new PopupFieldBase<string>(characterActorNameList, selectID);


            var flagDataModel = DatabaseManagementService.LoadFlags();
            var variableNameList = new List<string>();
            var variableIDList = new List<string>();
            selectID = 0;
            for (var i = 0; i < flagDataModel.variables.Count; i++)
            {
                if (flagDataModel.variables[i].name == "")
                    variableNameList.Add($"#{i + 1:0000} {EditorLocalize.LocalizeText("WORD_1518")}");
                else
                    variableNameList.Add($"#{i + 1:0000} {flagDataModel.variables[i].name}");

                variableIDList.Add(flagDataModel.variables[i].id);
            }

            if (_targetCommand.parameters[0] == "1")
                selectID = variableIDList.IndexOf(_targetCommand.parameters[1]);
            if (selectID == -1)
                selectID = 0;
            var variablePopupField = new PopupFieldBase<string>(variableNameList, selectID);

            if (_targetCommand.parameters[0] == "0")
                fix_toggle.value = true;
            else
                variable_toggle.value = true;
            
            var defaultSelect = _targetCommand.parameters[0] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {fix_toggle, variable_toggle},
                defaultSelect, new List<System.Action>
                {
                    //固定
                    () =>
                    {
                        _targetCommand.parameters[0] = "0";
                        _targetCommand.parameters[1] = characterActorIDList[fixPopupField.index];
                        fix.SetEnabled(true);
                        variable.SetEnabled(false);
                        Save(EventDataModels[EventIndex]);
                    },
                    //変数
                    () =>
                    {
                        _targetCommand.parameters[0] = "1";
                        _targetCommand.parameters[1] =
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
                _targetCommand.parameters[1] = characterActorIDList[fixPopupField.index];

                Save(EventDataModels[EventIndex]);
            });
            
            variable.Clear();
            variable.Add(variablePopupField);
            variablePopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[1] =
                    variableIDList[variablePopupField.index];
                Save(EventDataModels[EventIndex]);
            });


            RadioButton remember_toggle =
                RootElement.Q<VisualElement>("command_actorSkill").Query<RadioButton>("radioButton-eventCommand-display85");
            RadioButton forget_toggle = RootElement.Q<VisualElement>("command_actorSkill").Query<RadioButton>("radioButton-eventCommand-display86");
            if (_targetCommand.parameters[2] == "0")
                remember_toggle.value = true;
            else
                forget_toggle.value = true;
            
            var defaultRememberForget = _targetCommand.parameters[2] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {remember_toggle, forget_toggle},
                defaultRememberForget, new List<System.Action>
                {
                    //覚える
                    () =>
                    {
                        _targetCommand.parameters[2] = "0";
                        Save(EventDataModels[EventIndex]);
                    },
                    //忘れる
                    () =>
                    {
                        _targetCommand.parameters[2] = "1";
                        Save(EventDataModels[EventIndex]);
                    }
                });
            
            VisualElement skill = RootElement.Q<VisualElement>("command_actorSkill").Query<VisualElement>("skill");
            var skillNameArray = new List<string>();
            var skillIDArray = new List<string>();
            for (var i = 0; i < skillCustom.Count; i++)
            {
                skillNameArray.Add(skillCustom[i].basic.name);
                skillIDArray.Add(skillCustom[i].basic.id);
            }

            selectID = skillIDArray.IndexOf(_targetCommand.parameters[3]);
            if (selectID == -1)
                selectID = 0;
            var skillPopupField = new PopupFieldBase<string>(skillNameArray, selectID);
            skill.Clear();
            skill.Add(skillPopupField);
            skillPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[3] = skillIDArray[skillPopupField.index];
                Save(EventDataModels[EventIndex]);
            });
        }

        private List<SkillCustomDataModel> _GetSkillList() {
            var skillCustomDataModels = DatabaseManagementService.LoadSkillCustom();
            var fileNames = new List<SkillCustomDataModel>();
            var skillCustomDataModelsWork = new List<SkillCustomDataModel>();
            // 基本スキルの「攻撃」と「防御」は選択できない仕様
            for (int i = 0; i < skillCustomDataModels.Count; i++)
            {
                if (skillCustomDataModels[i].basic.id == "1")
                {
                    continue;
                }
                if (skillCustomDataModels[i].basic.id == "2")
                {
                    continue;
                }
                skillCustomDataModelsWork.Add(skillCustomDataModels[i]);
            }

            for (var i = 0; i < skillCustomDataModelsWork.Count; i++) fileNames.Add(skillCustomDataModelsWork[i]);
            return fileNames;
        }
    }
}