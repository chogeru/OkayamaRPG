using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCustom;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Skill.View;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Skill
{
    /// <summary>
    /// スキルのHierarchy
    /// </summary>
    public class SkillHierarchy : AbstractHierarchy
    {
        private List<SkillCustomDataModel> _skillCustomDataModels;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SkillHierarchy() {
            LoadData();
            View = new SkillHierarchyView(this, _skillCustomDataModels);
        }

        /// <summary>
        /// View
        /// </summary>
        public SkillHierarchyView View { get; }

        /// <summary>
        /// データの読込
        /// </summary>
        override protected void LoadData() {
            base.LoadData();
            _skillCustomDataModels = databaseManagementService.LoadSkillCustom();
        }

        /// <summary>
        /// Viewの更新
        /// </summary>
        protected override void UpdateView(string updateData = null) {
            base.UpdateView();
            View.Refresh();
        }

        /// <summary>
        /// 共通スキルのInspector表示
        /// </summary>
        public void OpenSkillCommonInspector() {
            Inspector.Inspector.SkillCommonView();
        }

        /// <summary>
        /// カスタムスキルのInspector表示
        /// </summary>
        /// <param name="skillCustomDataModel"></param>
        public void OpenSkillCustomInspector(SkillCustomDataModel skillCustomDataModel) {
            Inspector.Inspector.SkillCustomView(skillCustomDataModel);
        }

        /// <summary>
        /// スキルの新規作成
        /// </summary>
        public void CreateSkillCustomDataModel() {
            var system = databaseManagementService.LoadSystem();
            var newModel = SkillCustomDataModel.CreateDefault(Guid.NewGuid().ToString());
            newModel.basic.name = "#" + string.Format("{0:D4}", _skillCustomDataModels.Count + 1) + "　" + 
                                  EditorLocalize.LocalizeText("WORD_1518");
            
            newModel.basic.message = EditorLocalize.LocalizeText("WORD_0437");
            //スキルタイプが一つ以上あれば「なし」ではなく、一つ目を設定する
            if (system.skillTypes.Count > 0)
            {
                newModel.basic.skillType = 1;
            }
            //敵の単体
            newModel.targetEffect.targetTeam = 1;
            newModel.targetEffect.targetRange = 0;

            //スキル使用時のメッセージ
            newModel.basic.message.Trim();

            // 計算式の初期値を入れる
            var skillCommon = databaseManagementService.LoadSkillCommon();
            foreach (var data in system.skillTypes)
            {
                if (data.id == "7955b088-df05-461c-b2e3-7d9dfc3941f6")
                {
                    newModel.targetEffect.damage.value =
                        "a.atk * " + skillCommon[0].damage.normalAttack.aMag + " - b.def * " +
                        skillCommon[0].damage.normalAttack.bMag;
                    newModel.userEffect.damage.value =
                        "a.atk * " + skillCommon[0].damage.normalAttack.aMag + " - b.def * " +
                        skillCommon[0].damage.normalAttack.bMag;
                    break;
                }

                if (data.id == "64183358-c72f-4190-b42c-4ba9b373282c")
                {
                    newModel.targetEffect.damage.value =
                        skillCommon[0].damage.magicAttack.cDmg + " + a.mat * " +
                        skillCommon[0].damage.magicAttack.aMag + " - b.mdf * " + skillCommon[0].damage.magicAttack.bMag;
                    newModel.userEffect.damage.value =
                        skillCommon[0].damage.magicAttack.cDmg + " + a.mat * " +
                        skillCommon[0].damage.magicAttack.aMag + " - b.mdf * " + skillCommon[0].damage.magicAttack.bMag;
                    break;
                }

                if (data.id == "16b91790-cb8f-4a38-a663-d3a4fe2a4614")
                {
                    newModel.targetEffect.damage.value =
                        skillCommon[0].damage.specialAttack.cDmg + " + a.atk * " +
                        skillCommon[0].damage.specialAttack.aMag + " - b.def * " +
                        skillCommon[0].damage.specialAttack.bMag;
                    newModel.userEffect.damage.value =
                        skillCommon[0].damage.specialAttack.cDmg + " + a.atk * " +
                        skillCommon[0].damage.specialAttack.aMag + " - b.def * " +
                        skillCommon[0].damage.specialAttack.bMag;
                    break;
                }
            }

            _skillCustomDataModels.Add(newModel);
            databaseManagementService.SaveSkillCustom(_skillCustomDataModels);

            Refresh();
        }

        /// <summary>
        /// スキルのコピー＆貼り付け処理
        /// </summary>
        /// <param name="skillCustomDataModel"></param>
        public void DuplicateSkillCustomDataModel(SkillCustomDataModel skillCustomDataModel) {
            var duplicated = skillCustomDataModel.DataClone();
            duplicated.basic.id = Guid.NewGuid().ToString();
            duplicated.basic.name = CreateDuplicateName(_skillCustomDataModels.Select(s => s.basic.name).ToList(),
                duplicated.basic.name);
            _skillCustomDataModels.Add(duplicated);
            databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
            Refresh();
        }

        /// <summary>
        /// スキル削除
        /// </summary>
        /// <param name="skillCustomDataModel"></param>
        public void DeleteSkillCustomDataModel(SkillCustomDataModel skillCustomDataModel) {
            _skillCustomDataModels.Remove(skillCustomDataModel);
            databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
            Refresh();
        }
    }
}