using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository
{
    public class AutoGuideRepository : AbstractDatabaseRepository<AutoGuideDataModel>
    {
        private static readonly float MIN_STATUS_DIVISION = 10f;
        protected override string JsonPath => "Assets/RPGMaker/Storage/Initializations/JSON/autoGuide.json";

        #region serialization

        public static ClassDataModel.AutoGuide GetBaseParameter(int maxHp) {
            var baseParameter = ClassDataModel.AutoGuide.CreateDefault();
            baseParameter.maxHp = maxHp;
            baseParameter.maxMp = (int) Math.Round(maxHp * DataModels[0].baseParameterRatio[0].maxMp);
            baseParameter.attack = (int) Math.Round(maxHp * DataModels[0].baseParameterRatio[0].attack);
            baseParameter.defense = (int) Math.Round(maxHp * DataModels[0].baseParameterRatio[0].defense);
            baseParameter.magicAttack = (int) Math.Round(maxHp * DataModels[0].baseParameterRatio[0].magicAttack);
            baseParameter.magicDefense = (int) Math.Round(maxHp * DataModels[0].baseParameterRatio[0].magicDefense);
            baseParameter.speed = (int) Math.Round(maxHp * DataModels[0].baseParameterRatio[0].speed);
            baseParameter.luck = (int) Math.Round(maxHp * DataModels[0].baseParameterRatio[0].luck);

            return baseParameter;
        }

        public static ClassDataModel.AutoGuide GetMinParameter(ClassDataModel.AutoGuide baseParameter) {
            //ステータスの最小値(1Lvl)
            var minBaseParameter = ClassDataModel.AutoGuide.CreateDefault();
            minBaseParameter.maxHp = (int) Math.Round(baseParameter.maxHp / MIN_STATUS_DIVISION);
            minBaseParameter.maxMp = (int) Math.Round(baseParameter.maxMp / MIN_STATUS_DIVISION);
            minBaseParameter.attack = (int) Math.Round(baseParameter.attack / MIN_STATUS_DIVISION);
            minBaseParameter.defense = (int) Math.Round(baseParameter.defense / MIN_STATUS_DIVISION);
            minBaseParameter.magicAttack = (int) Math.Round(baseParameter.magicAttack / MIN_STATUS_DIVISION);
            minBaseParameter.magicDefense = (int) Math.Round(baseParameter.magicDefense / MIN_STATUS_DIVISION);
            minBaseParameter.speed = (int) Math.Round(baseParameter.speed / MIN_STATUS_DIVISION);
            minBaseParameter.luck = (int) Math.Round(baseParameter.luck / MIN_STATUS_DIVISION);

            return minBaseParameter;
        }

        public static ClassDataModel.AutoGuide GetClearParameter(
            ClassDataModel.AutoGuide baseParameter,
            int clearLevel
        ) {
            //クリア想定レベルのステータス
            //調整値
            var adjustmentValue = 1f - (99f - clearLevel) / 350f;
            var clearBaseParameter = ClassDataModel.AutoGuide.CreateDefault();

            clearBaseParameter.maxHp = (int) Math.Round(baseParameter.maxHp * adjustmentValue);
            clearBaseParameter.maxMp = (int) Math.Round(baseParameter.maxMp * adjustmentValue);
            clearBaseParameter.attack = (int) Math.Round(baseParameter.attack * adjustmentValue);
            clearBaseParameter.defense = (int) Math.Round(baseParameter.defense * adjustmentValue);
            clearBaseParameter.magicAttack = (int) Math.Round(baseParameter.magicAttack * adjustmentValue);
            clearBaseParameter.magicDefense = (int) Math.Round(baseParameter.magicDefense * adjustmentValue);
            clearBaseParameter.speed = (int) Math.Round(baseParameter.speed * adjustmentValue);
            clearBaseParameter.luck = (int) Math.Round(baseParameter.luck * adjustmentValue);

            return clearBaseParameter;
        }

        public static ClassDataModel.AutoGuide GetLevelParameter(int maxHp, int clearLevel, int nowLevel) {
            var baseParameter = GetBaseParameter(maxHp);
            var minBaseParameter = GetMinParameter(baseParameter);
            var clearBaseParameter = GetClearParameter(baseParameter, clearLevel);

            var nowParameter = ClassDataModel.AutoGuide.CreateDefault();

            if (nowLevel < clearLevel)
            {
                nowParameter.maxHp = (int) Math.Round(minBaseParameter.maxHp +
                                                      (clearBaseParameter.maxHp -
                                                       (float) minBaseParameter.maxHp) / (clearLevel - 1) *
                                                      (nowLevel - 1));
                nowParameter.maxMp = (int) Math.Round(minBaseParameter.maxMp +
                                                      (clearBaseParameter.maxMp -
                                                       (float) minBaseParameter.maxMp) / (clearLevel - 1) *
                                                      (nowLevel - 1));
                nowParameter.attack = (int) Math.Round(minBaseParameter.attack +
                                                       (clearBaseParameter.attack -
                                                        (float) minBaseParameter.attack) / (clearLevel - 1) *
                                                       (nowLevel - 1));
                nowParameter.defense = (int) Math.Round(minBaseParameter.defense +
                                                        (clearBaseParameter.defense -
                                                         (float) minBaseParameter.defense) / (clearLevel - 1) *
                                                        (nowLevel - 1));
                nowParameter.magicAttack = (int) Math.Round(minBaseParameter.magicAttack +
                                                            (clearBaseParameter.magicAttack -
                                                             (float) minBaseParameter.magicAttack) /
                                                            (clearLevel - 1) * (nowLevel - 1));
                nowParameter.magicDefense = (int) Math.Round(minBaseParameter.magicDefense +
                                                             (clearBaseParameter.magicDefense -
                                                              (float) minBaseParameter.magicDefense) /
                                                             (clearLevel - 1) * (nowLevel - 1));
                nowParameter.speed = (int) Math.Round(minBaseParameter.speed +
                                                      (clearBaseParameter.speed -
                                                       (float) minBaseParameter.speed) / (clearLevel - 1) *
                                                      (nowLevel - 1));
                nowParameter.luck = (int) Math.Round(minBaseParameter.luck +
                                                     (clearBaseParameter.luck - (float) minBaseParameter.luck) /
                                                     (clearLevel - 1) * (nowLevel - 1));
            }
            else
            {
                nowParameter.maxHp = (int) Math.Round(clearBaseParameter.maxHp +
                                                      (baseParameter.maxHp - (float) clearBaseParameter.maxHp) /
                                                      (99 - clearLevel) * (nowLevel - clearLevel));
                nowParameter.maxMp = (int) Math.Round(clearBaseParameter.maxMp +
                                                      (baseParameter.maxMp - (float) clearBaseParameter.maxMp) /
                                                      (99 - clearLevel) * (nowLevel - clearLevel));
                nowParameter.attack = (int) Math.Round(clearBaseParameter.attack +
                                                       (baseParameter.attack -
                                                        (float) clearBaseParameter.attack) / (99 - clearLevel) *
                                                       (nowLevel - clearLevel));
                nowParameter.defense = (int) Math.Round(clearBaseParameter.defense +
                                                        (baseParameter.defense -
                                                         (float) clearBaseParameter.defense) /
                                                        (99 - clearLevel) * (nowLevel - clearLevel));
                nowParameter.magicAttack = (int) Math.Round(clearBaseParameter.magicAttack +
                                                            (baseParameter.magicAttack -
                                                             (float) clearBaseParameter.magicAttack) /
                                                            (99 - clearLevel) *
                                                            (nowLevel - clearLevel));
                nowParameter.magicDefense = (int) Math.Round(clearBaseParameter.magicDefense +
                                                             (baseParameter.magicDefense -
                                                              (float) clearBaseParameter.magicDefense) /
                                                             (99 - clearLevel) *
                                                             (nowLevel - clearLevel));
                nowParameter.speed = (int) Math.Round(clearBaseParameter.speed +
                                                      (baseParameter.speed - (float) clearBaseParameter.speed) /
                                                      (99 - clearLevel) * (nowLevel - clearLevel));
                nowParameter.luck = (int) Math.Round(clearBaseParameter.luck +
                                                     (baseParameter.luck - (float) clearBaseParameter.luck) /
                                                     (99 - clearLevel) * (nowLevel - clearLevel));
            }

            return nowParameter;
        }

        public ClassDataModel.AutoGuide GetAutoGuideArmorParameter(int maxHp, int clearLevel, int nowLevel) {
            var armorParameter = GetLevelParameter(maxHp, clearLevel, nowLevel);
            armorParameter.maxHp = (int) Math.Round(armorParameter.maxHp * DataModels[0].armorRatio[0].maxHp);
            armorParameter.maxMp = (int) Math.Round(armorParameter.maxMp * DataModels[0].armorRatio[0].maxMp);
            armorParameter.attack =
                (int) Math.Round(armorParameter.attack * DataModels[0].armorRatio[0].attack);
            armorParameter.defense =
                (int) Math.Round(armorParameter.defense * DataModels[0].armorRatio[0].defense);
            armorParameter.magicAttack =
                (int) Math.Round(armorParameter.magicAttack * DataModels[0].armorRatio[0].magicAttack);
            armorParameter.magicDefense =
                (int) Math.Round(armorParameter.magicDefense * DataModels[0].armorRatio[0].magicDefense);
            armorParameter.speed = (int) Math.Round(armorParameter.speed * DataModels[0].armorRatio[0].speed);
            armorParameter.luck = (int) Math.Round(armorParameter.luck * DataModels[0].armorRatio[0].luck);

            return armorParameter;
        }

        public ClassDataModel.AutoGuide GetAutoGuideWeaponParameter(int maxHp, int clearLevel, int nowLevel) {
            var weaponParameter = GetLevelParameter(maxHp, clearLevel, nowLevel);
            weaponParameter.maxHp =
                (int) Math.Round(weaponParameter.maxHp * DataModels[0].weaponRatio[0].maxHp);
            weaponParameter.maxMp =
                (int) Math.Round(weaponParameter.maxMp * DataModels[0].weaponRatio[0].maxMp);
            weaponParameter.attack =
                (int) Math.Round(weaponParameter.attack * DataModels[0].weaponRatio[0].attack);
            weaponParameter.defense =
                (int) Math.Round(weaponParameter.defense * DataModels[0].weaponRatio[0].defense);
            weaponParameter.magicAttack =
                (int) Math.Round(weaponParameter.magicAttack * DataModels[0].weaponRatio[0].magicAttack);
            weaponParameter.magicDefense =
                (int) Math.Round(weaponParameter.magicDefense * DataModels[0].weaponRatio[0].magicDefense);
            weaponParameter.speed =
                (int) Math.Round(weaponParameter.speed * DataModels[0].weaponRatio[0].speed);
            weaponParameter.luck = (int) Math.Round(weaponParameter.luck * DataModels[0].weaponRatio[0].luck);
            return weaponParameter;
        }

        public ClassDataModel.AutoGuide GetAutoGuideEnemyParameter(
            ClassDataModel.AutoGuide levelParameter,
            int turnOfAttacksEndured,
            int turnOfAttacksToKill
        ) {
            var enemyParameter = ClassDataModel.AutoGuide.CreateDefault();
            enemyParameter.maxHp = (int) Math.Round(levelParameter.attack / 2 - levelParameter.defense * 0.8 / 4) *
                                   turnOfAttacksToKill;
            enemyParameter.maxMp = 0;
            enemyParameter.attack = (int) Math.Round((float) (levelParameter.maxHp / turnOfAttacksEndured) +
                                                     levelParameter.defense / 4 * 2);
            enemyParameter.defense = (int) Math.Round(levelParameter.defense * 0.8);
            enemyParameter.magicAttack = 0;
            enemyParameter.magicDefense = 0;
            enemyParameter.speed = levelParameter.speed;
            enemyParameter.luck = levelParameter.luck;

            return enemyParameter;
        }

        public float GetAutoGuideSkillUseMp(ClassDataModel.AutoGuide levelParameter, bool attacAll) {
            if (attacAll)
                return levelParameter.maxMp / 10f;
            return levelParameter.maxMp / 12f;
        }

        #endregion
        
#if !UNITY_EDITOR
        public new List<AutoGuideDataModel> Load() {
            if (DataModels != null)
            {
                // キャッシュがあればそれを返す
                return DataModels;
            }
            DataModels = ScriptableObjectOperator.GetClass<AutoGuideDataModel>(JsonPath) as List<AutoGuideDataModel>;
            SetSerialNumbers();
            return DataModels;
        }
#endif
    }
}