using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCommon;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.SkillCommon.View
{
    public class SkillCommonInspectorElement : AbstractInspectorElement
    {
        /// <summary>
        /// [スキルの編集]-[共通設定] Inspector
        /// </summary>
        private List<SkillCommonDataModel> _skillCommonDataModels;

        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/SkillCommon/Asset/inspector_skillCommon.uxml"; } }

        public SkillCommonInspectorElement() {
            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            _skillCommonDataModels = databaseManagementService.LoadSkillCommon();
            Initialize();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            //SkillCommonDataModelには、データが1つしか登録されない
            //編集データ設定
            SkillCommonDataModel skillCommonDataModel = _skillCommonDataModels[0];

            //通常攻撃のUXML部品
            FloatField attackAMagField = RootContainer.Query<FloatField>("attack_a_mag");
            FloatField attackBMagField = RootContainer.Query<FloatField>("attack_b_mag");
            Label attackTemplateFormula = RootContainer.Query<Label>("attack_template_formula");
            Label attackFormula = RootContainer.Query<Label>("attack_formula");

            //魔法のUXML部品
            FloatField magicBaseParam = RootContainer.Query<FloatField>("magic_base_param");
            FloatField magicAMagField = RootContainer.Query<FloatField>("magic_a_mag");
            FloatField magicBMagField = RootContainer.Query<FloatField>("magic_b_mag");
            Label magicTemplateFormula = RootContainer.Query<Label>("magic_template_formula");
            Label magicFormula = RootContainer.Query<Label>("magic_formula");

            //必殺技のUXML部品
            FloatField specialBaseParam = RootContainer.Query<FloatField>("special_base_param");
            FloatField specialAMagField = RootContainer.Query<FloatField>("special_a_mag");
            FloatField specialBMagField = RootContainer.Query<FloatField>("special_b_mag");
            Label specialTemplateFormula = RootContainer.Query<Label>("special_template_formula");
            Label specialFormula = RootContainer.Query<Label>("special_formula");

            //通常攻撃
            //計算式の雛形
            attackTemplateFormula.text = "(a.atk * a.mag - b.def * b.mag)";

            //攻撃側倍率(a.mag)
            attackAMagField.value = skillCommonDataModel.damage.normalAttack.aMag;
            BaseInputFieldHandler.FloatFieldCallback(attackAMagField, o =>
            {
                attackAMagField.value = float.Parse($"{attackAMagField.value:F3}");
                skillCommonDataModel.damage.normalAttack.aMag = attackAMagField.value;
                databaseManagementService.SaveSkillCommon(_skillCommonDataModels);
                attackFormula.text = "a.atk * " + attackAMagField.value + " - b.def * " + attackBMagField.value;
            }, 0.0f, 10.0f, 3);

            //対象側倍率(b.def)
            attackBMagField.value = skillCommonDataModel.damage.normalAttack.bMag;
            BaseInputFieldHandler.FloatFieldCallback(attackBMagField, o =>
            {
                attackBMagField.value = float.Parse($"{attackBMagField.value:F3}");
                skillCommonDataModel.damage.normalAttack.bMag = attackBMagField.value;
                databaseManagementService.SaveSkillCommon(_skillCommonDataModels);
                attackFormula.text = "a.atk * " + attackAMagField.value + " - b.def * " + attackBMagField.value;
            }, 0.0f, 10.0f, 3);

            //初期値設定
            attackFormula.text = "a.atk * " + attackAMagField.value + " - b.def * " + attackBMagField.value;

            //魔法
            //計算式の雛形
            magicTemplateFormula.text = "(cdmg + a.mat * a.mag - b.mdf * b.mag)";

            //固定値(cdmg)
            magicBaseParam.value = skillCommonDataModel.damage.magicAttack.cDmg;
            BaseInputFieldHandler.FloatFieldCallback(magicBaseParam, o =>
            {
                magicBaseParam.value = float.Parse($"{magicBaseParam.value:F3}");
                skillCommonDataModel.damage.magicAttack.cDmg = magicBaseParam.value;
                databaseManagementService.SaveSkillCommon(_skillCommonDataModels);
                magicFormula.text = magicBaseParam.value + " + a.mat * " + magicAMagField.value + " - b.mdf * " +
                                    magicBMagField.value;
            }, 0.0f, 100.0f, 0);

            //攻撃側倍率(a.mag)
            magicAMagField.value = skillCommonDataModel.damage.magicAttack.aMag;
            BaseInputFieldHandler.FloatFieldCallback(magicAMagField, o =>
            {
                magicAMagField.value = float.Parse($"{magicAMagField.value:F3}");
                skillCommonDataModel.damage.magicAttack.aMag = magicAMagField.value;
                databaseManagementService.SaveSkillCommon(_skillCommonDataModels);
                magicFormula.text = magicBaseParam.value + " + a.mat * " + magicAMagField.value + " - b.mdf * " +
                                    magicBMagField.value;
            }, 0.0f, 10.0f, 3);

            //対象側倍率(b.mag)
            magicBMagField.value = skillCommonDataModel.damage.magicAttack.bMag;
            BaseInputFieldHandler.FloatFieldCallback(magicBMagField, o =>
            {
                magicAMagField.value = float.Parse($"{magicAMagField.value:F3}");
                skillCommonDataModel.damage.magicAttack.bMag = magicBMagField.value;
                databaseManagementService.SaveSkillCommon(_skillCommonDataModels);
                magicFormula.text = magicBaseParam.value + " + a.mat * " + magicAMagField.value + " - b.mdf * " +
                                    magicBMagField.value;
            }, 0.0f, 10.0f, 3);

            //初期化
            magicFormula.text = magicBaseParam.value + " + a.mat * " + magicAMagField.value + " - b.mdf * " +
                                magicBMagField.value;

            //必殺技
            //計算式の雛形
            specialTemplateFormula.text = "(cdmg + a.atk * a.mag - b.def * b.mag)";

            //固定値(cdmg)
            specialBaseParam.value = skillCommonDataModel.damage.specialAttack.cDmg;
            BaseInputFieldHandler.FloatFieldCallback(specialBaseParam, o =>
            {
                specialBaseParam.value = float.Parse($"{specialBaseParam.value:F3}");
                skillCommonDataModel.damage.specialAttack.cDmg = specialBaseParam.value;
                databaseManagementService.SaveSkillCommon(_skillCommonDataModels);
                specialFormula.text = specialBaseParam.value + " + a.atk * " + specialAMagField.value +
                                      " - b.def * " + specialBMagField.value;
            }, 0.0f, 100.0f, 0);

            //攻撃側倍率(a.mag)
            specialAMagField.value = skillCommonDataModel.damage.specialAttack.aMag;
            BaseInputFieldHandler.FloatFieldCallback(specialAMagField, o =>
            {
                specialAMagField.value = float.Parse($"{specialAMagField.value:F3}");
                skillCommonDataModel.damage.specialAttack.aMag = specialAMagField.value;
                databaseManagementService.SaveSkillCommon(_skillCommonDataModels);
                specialFormula.text = specialBaseParam.value + " + a.atk * " + specialAMagField.value +
                                      " - b.def * " + specialBMagField.value;
            }, 0.0f, 10.0f, 3);

            //対象側倍率(b.mag)
            specialBMagField.value = skillCommonDataModel.damage.specialAttack.bMag;
            BaseInputFieldHandler.FloatFieldCallback(specialBMagField, o =>
            {
                specialBMagField.value = float.Parse($"{specialBMagField.value:F3}");
                skillCommonDataModel.damage.specialAttack.bMag = specialBMagField.value;
                databaseManagementService.SaveSkillCommon(_skillCommonDataModels);
                specialFormula.text = specialBaseParam.value + " + a.atk * " + specialAMagField.value +
                                      " - b.def * " + specialBMagField.value;
            }, 0.0f, 10.0f, 3);
            specialFormula.text = specialBaseParam.value + " + a.atk * " + specialAMagField.value + " - b.def * " +
                                  specialBMagField.value;
        }
    }
}