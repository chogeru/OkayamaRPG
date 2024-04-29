using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Enum;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.Type.View
{
    /// <summary>
    /// [タイプの編集]-[スキルタイプ] Inspector
    /// </summary>
    public class SkillTypeEditInspectorElement : AbstractInspectorElement
    {
        private SystemSettingDataModel.SkillType _skillType;
        private SystemSettingDataModel           _systemSettingDataModel;

        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Type/Asset/inspector_typeEdit.uxml"; } }

        public SkillTypeEditInspectorElement(SystemSettingDataModel.SkillType skillType) {
            _skillType = skillType;
            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            _systemSettingDataModel = databaseManagementService.LoadSystem();
            _skillType = databaseManagementService.LoadSystem().skillTypes
                .Find(item => item.id == _skillType.id);
            if (_skillType == null)
            {
                Clear();
                return;
            }
            Initialize();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            Foldout skill_type_list = RootContainer.Query<Foldout>("skill_type_list");
            skill_type_list.style.display = DisplayStyle.Flex;
            Skill();
        }

        private void Skill() {
            Label type_edit_skill_ID = RootContainer.Query<Label>("type_edit_skill_ID");
            type_edit_skill_ID.text = _skillType.SerialNumberString;

            ImTextField type_edit_skill_name = RootContainer.Query<ImTextField>("type_edit_skill_name");
            type_edit_skill_name.value = _skillType.value;
            type_edit_skill_name.RegisterCallback<FocusOutEvent>(o =>
            {
                _skillType.value = type_edit_skill_name.value;
                SaveData();
                _UpdateSceneView();
            });

            RadioButton type_edit_skill_chanting_motion1 = RootContainer.Query<RadioButton>("radioButton-typeEdit-display1");
            RadioButton type_edit_skill_chanting_motion2 = RootContainer.Query<RadioButton>("radioButton-typeEdit-display2");

            type_edit_skill_chanting_motion1.value =
                _skillType.motion == 1;
            type_edit_skill_chanting_motion2.value =
                _skillType.motion == 0;
            
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {type_edit_skill_chanting_motion2, type_edit_skill_chanting_motion1},
                _skillType.motion, new List<Action>
                {
                    //OFF
                    () =>
                    {
                        _skillType.motion = 0;
                        SaveData();
                    },
                    //ON
                    () =>
                    {
                        _skillType.motion = 1;
                        SaveData();
                    }

                });
        }

        private void SaveData() {
            databaseManagementService.SaveSystem(_systemSettingDataModel);
        }

        private void _UpdateSceneView() {
            _ = Editor.Hierarchy.Hierarchy.Refresh(Region.TypeEdit, _skillType.id);
        }
    }
}