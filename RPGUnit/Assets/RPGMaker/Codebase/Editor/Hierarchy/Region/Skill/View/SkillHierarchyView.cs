using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCustom;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Base.View.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Skill.View
{
    /// <summary>
    /// スキルのHierarchyView
    /// </summary>
    public class SkillHierarchyView : AbstractHierarchyView
    {
        // const
        //--------------------------------------------------------------------------------------------------------------
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Hierarchy/Region/Skill/Asset/database_skill.uxml"; } }

        private Button _attackSkillButton;
        private VisualElement _customSkillListContainer;
        private HierarchyItemListView _customSkillListView;
        private Button _defenseSkillButton;

        private const int foldoutCount = 3;

        // 状態
        //--------------------------------------------------------------------------------------------------------------

        // UI要素
        //--------------------------------------------------------------------------------------------------------------
        private Button _skillCommonButton;

        // 利用するデータ
        //--------------------------------------------------------------------------------------------------------------
        private List<SkillCustomDataModel> _skillCustomDataModels;

        // ヒエラルキー本体クラス
        //--------------------------------------------------------------------------------------------------------------
        private readonly SkillHierarchy _skillHierarchy;

        //--------------------------------------------------------------------------------------------------------------
        //
        // methods
        //
        //--------------------------------------------------------------------------------------------------------------

        // 初期化・更新系
        //--------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="skillHierarchy"></param>
        /// <param name="skillCustomDataModels"></param>
        public SkillHierarchyView(SkillHierarchy skillHierarchy, List<SkillCustomDataModel> skillCustomDataModels) {
            _skillHierarchy = skillHierarchy;
            _skillCustomDataModels = skillCustomDataModels;
            InitUI();
        }

        /// <summary>
        /// 各コンテンツデータの初期化
        /// </summary>
        override protected void InitContentsData() {
            _skillCommonButton = UxmlElement.Query<Button>("skill_common_button");
            _attackSkillButton = UxmlElement.Query<Button>("attack_skill_button");
            _defenseSkillButton = UxmlElement.Query<Button>("defense_skill_button");
            SetFoldout("customSkillFoldout");
            _customSkillListContainer = UxmlElement.Query<VisualElement>("skill_custom_list");
            _customSkillListView = new HierarchyItemListView(ViewName);
            _customSkillListContainer.Add(_customSkillListView);

            //Foldoutの開閉状態保持用
            for (int i = 0; i < foldoutCount; i++)
                SetFoldout("foldout_" + (i + 1));

            InitEventHandlers();
        }

        /// <summary>
        /// イベントの初期設定
        /// </summary>
        private void InitEventHandlers() {
            Editor.Hierarchy.Hierarchy.AddSelectableElementAndAction(_skillCommonButton,
                _skillHierarchy.OpenSkillCommonInspector);
            _skillCommonButton.clickable.clicked += () =>
            {
                Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(_skillCommonButton);
            };

            Editor.Hierarchy.Hierarchy.AddSelectableElementAndAction(_attackSkillButton,
                () => { _skillHierarchy.OpenSkillCustomInspector(_skillCustomDataModels[0]); });
            _attackSkillButton.clickable.clicked += () =>
            {
                Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(_attackSkillButton);
            };

            Editor.Hierarchy.Hierarchy.AddSelectableElementAndAction(_defenseSkillButton,
                () => { _skillHierarchy.OpenSkillCustomInspector(_skillCustomDataModels[1]); });
            _defenseSkillButton.clickable.clicked += () =>
            {
                Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(_defenseSkillButton);
            };

            InitContextMenu(RegistrationLimit.None);
            var dic = new Dictionary<string, List<string>>
            {
                {
                    KeyNameCustomSkill,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_0410"), EditorLocalize.LocalizeText("WORD_0411")
                    }
                }
            };
            SetParentContextMenu(dic);

            _customSkillListView.SetEventHandler(
                (i, value) => { _skillHierarchy.OpenSkillCustomInspector(_skillCustomDataModels[i + 2]); },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameCustomSkill, new ContextMenuData()
                            {
                                UuId = _skillCustomDataModels[i + 2].basic.id,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_0412"),
                                        EditorLocalize.LocalizeText("WORD_0413")
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.None
                            }
                        }
                    };
                    SetChildContextMenu(childDic);
                });
        }

        protected override VisualElement CreateDataModel(string keyName) {
            var visualElement =  base.CreateDataModel(keyName);
            if (keyName == KeyNameCustomSkill)
            {
                _skillHierarchy.CreateSkillCustomDataModel();
                visualElement = LastSkillIndex();
            }

            return visualElement;
        }

        protected override VisualElement DuplicateDataModel(string keyName, string uuId) {
            var visualElement =  base.DuplicateDataModel(keyName, uuId);
            if (keyName == KeyNameCustomSkill)
            {
                var skillCustomDataModel = _skillCustomDataModels.FirstOrDefault(s => s.basic.id == uuId);
                _skillHierarchy.DuplicateSkillCustomDataModel(skillCustomDataModel);
                visualElement = LastSkillIndex();
            }
            return visualElement;
        }

        protected override VisualElement DeleteDataModel(string keyName, string uuId) {
            var visualElement =  base.DeleteDataModel(keyName, uuId);
            if (keyName == KeyNameCustomSkill)
            {
                SkillCustomDataModel skillCustomDataModel = null;
                int index = 0;
                for (int i = 0; i < _skillCustomDataModels.Count; i++)
                {
                    if (_skillCustomDataModels[i].basic.id == uuId)
                    {
                        skillCustomDataModel = _skillCustomDataModels[i];
                        index = i;
                        break;
                    }
                }
                _skillHierarchy.DeleteSkillCustomDataModel(skillCustomDataModel);
                var elements = new List<VisualElement>();
                _customSkillListView.Query<Button>().ForEach(button => { elements.Add(button); });
                visualElement = elements.Count < index ? LastSkillIndex() : elements.FirstOrDefault(e => e.name == "SkillHierarchyView" + (index - 2));
            }
            return visualElement;

        }

        /// <summary>
        /// データ更新
        /// </summary>
        /// <param name="skillCustomDataModels"></param>
        public void Refresh([CanBeNull] List<SkillCustomDataModel> skillCustomDataModels = null) {
            if (skillCustomDataModels != null) _skillCustomDataModels = skillCustomDataModels;
            base.Refresh();
        }

        /// <summary>
        /// データ更新
        /// </summary>
        protected override void RefreshContents() {
            base.RefreshContents();
            _customSkillListView.Refresh(GetOptionalCustomSkillList().Select(item => item.basic.name).ToList());
        }

        /// <summary>
        /// カスタムスキル取得
        /// </summary>
        /// <returns></returns>
        private IEnumerable<SkillCustomDataModel> GetOptionalCustomSkillList() {
            return new ArraySegment<SkillCustomDataModel>(_skillCustomDataModels.ToArray(), 2,
                _skillCustomDataModels.Count - 2);
        }

        /// <summary>
        /// 最終選択していたスキルを返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastSkillIndex() {
            var elements = new List<VisualElement>();
            _customSkillListView.Query<Button>().ForEach(button => { elements.Add(button); });

            //選択可能なVisualElementが存在しない場合には、Inspectorを初期化して終了する
            if (elements.Count == 0)
            {
                Inspector.Inspector.Clear();
                return null;
            }

            return elements[elements.Count - 1];
        }
    }
}