using System;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.Common
{
    /// <summary>
    ///     オートガイド用補助クラス
    /// </summary>
    public static class AutoguideHelper
    {
        public static AutoGuideModel CalcStandardModel(
            int maxLevel,
            int clearLevel,
            int expGainIncreaseValue,
            int maxHp,
            int baseHpmpBalance
        ) {
            //標準モデル用の箱を作成
            var standardModel = AutoGuideModel.CreateDefault(baseHpmpBalance);
            //標準モデルの値計算
            standardModel.CalcStandardModel(
                maxLevel,
                clearLevel,
                expGainIncreaseValue,
                maxHp
            );
            //返却
            return standardModel;
        }

        public static AutoGuideModel CalcClassModel(
            int maxLevel,
            int clearLevel,
            int expGainIncreaseValue,
            int maxHp,
            int hpmpBalance,
            int attackAdd,
            int defenseAdd,
            int magicAdd,
            int magicDefenseAdd,
            int speedAdd,
            int luckAdd
        ) {
            //標準モデル用の箱を作成
            var standardModel = AutoGuideModel.CreateDefault(hpmpBalance);
            //標準モデルの値計算
            standardModel.CalcClassModel(
                maxLevel,
                clearLevel,
                expGainIncreaseValue,
                maxHp,
                hpmpBalance,
                attackAdd,
                defenseAdd,
                magicAdd,
                magicDefenseAdd,
                speedAdd,
                luckAdd
            );
            //返却
            return standardModel;
        }

        [Serializable]
        public class AutoGuideModel
        {
            public int attack; //想定レベル攻撃力

            private float baseHpmpBalance; //HPMPバランス 入力値
            private float baseMaxAttack; //最大攻撃力 入力値
            private float baseMaxDefense; //最大防御力 入力値
            private float baseMaxHp; //最大HP 入力値
            private float baseMaxMagic; //最大魔法力 入力値
            private float baseMaxMagicDefense; //最大魔法防御 入力値
            private float baseMaxMp; //最大MP 入力値
            private float baseMaxSpeed; //最大俊敏性 入力値
            public  int   defense; //想定レベル防御力
            public  int   hp; //想定レベルHP
            public  float hpmpBalance; //HPMPバランス

            public int level; //想定レベル
            public int luck; //想定レベル運
            public int magic; //想定レベル魔法力
            public int magicDefense; //想定レベル魔法防御
            public int maxAttack; //最大攻撃力
            public int maxDefense; //最大防御力

            public int maxHp; //最大HP
            public int maxLuck; //最大運
            public int maxMagic; //最大魔法力
            public int maxMagicDefense; //最大魔法防御
            public int maxMp; //最大MP
            public int maxSpeed; //最大俊敏性
            public int minAttack; //最小攻撃力
            public int minDefense; //最小防御力

            public  int minHp; //最小HP
            public  int minLuck; //最小運
            public  int minMagic; //最小魔法力
            public  int minMagicDefense; //最小魔法防御
            public  int minMp; //最小MP
            public  int minSpeed; //最小俊敏性
            public  int mp; //想定レベルMP
            public  int speed; //想定レベル俊敏性
            private int systemClearLevel; //クリアレベル
            private int systemExpGainIncreaseValue; //経験値の上限
            private int systemMaxHp; //HPの上限

            private int systemMaxLevel; //最大レベル

            public AutoGuideModel(
                float baseHpmpBalance,
                float baseMaxHp,
                float baseMaxMp,
                float baseMaxAttack,
                float baseMaxDefense,
                float baseMaxMagic,
                float baseMaxMagicDefense,
                float baseMaxSpeed
            ) {
                this.baseHpmpBalance = baseHpmpBalance;
                this.baseMaxHp = baseMaxHp;
                this.baseMaxMp = baseMaxMp;
                this.baseMaxAttack = baseMaxAttack;
                this.baseMaxDefense = baseMaxDefense;
                this.baseMaxMagic = baseMaxMagic;
                this.baseMaxMagicDefense = baseMaxMagicDefense;
                this.baseMaxSpeed = baseMaxSpeed;
            }

            public static AutoGuideModel CreateDefault(
                float baseHpmpBalance,
                float baseMaxHp = 7.5f,
                float baseMaxMp = 7.5f,
                float baseMaxAttack = 7.5f,
                float baseMaxDefense = 7.5f,
                float baseMaxMagic = 7.5f,
                float baseMaxMagicDefense = 7.5f,
                float baseMaxSpeed = 7.5f
            ) {
                return new AutoGuideModel(
                    baseHpmpBalance,
                    baseMaxHp,
                    baseMaxMp,
                    baseMaxAttack,
                    baseMaxDefense,
                    baseMaxMagic,
                    baseMaxMagicDefense,
                    baseMaxSpeed
                );
            }

            public void CalcStandardModel(
                int maxLevel,
                int clearLevel,
                int expGainIncreaseValue,
                int maxHp
            ) {
                //標準モデルの場合、簡易的な計算を行う
                //HPMPバランス
                hpmpBalance = 0.75f;

                //各パラメーターの最大値
                this.maxHp = Mathf.CeilToInt((float)(maxHp * 0.75));
                maxMp = Mathf.CeilToInt((float)(maxHp * 0.3));
                maxAttack = Mathf.CeilToInt((float)(maxHp * 0.41));
                maxDefense = Mathf.CeilToInt((float)(maxHp * 0.252));
                maxMagic = Mathf.CeilToInt((float)(maxHp * 0.315));
                maxMagicDefense = Mathf.CeilToInt((float)(maxHp * 0.2));
                maxSpeed = Mathf.CeilToInt((float)(maxHp * 0.25));
                maxLuck = Mathf.CeilToInt((float)(maxHp * 0.16));

                //各パラメーターの最小値
                minHp = Mathf.CeilToInt((float)(maxHp * 0.045));
                minMp = Mathf.CeilToInt((float)(maxHp * 0.018));
                minAttack = Mathf.CeilToInt((float)(maxAttack * 0.045));
                minDefense = Mathf.CeilToInt((float)(maxDefense * 0.045));
                minMagic = Mathf.CeilToInt((float)(maxMagic * 0.045));
                minMagicDefense = Mathf.CeilToInt((float)(maxMagicDefense * 0.045));
                minSpeed = Mathf.CeilToInt((float)(maxSpeed * 0.045));
                minLuck = Mathf.CeilToInt((float)(maxLuck * 0.045));

                //標準モデル作成時の各初期値を保持
                systemMaxLevel = maxLevel;
                systemClearLevel = clearLevel;
                systemExpGainIncreaseValue = expGainIncreaseValue;
                systemMaxHp = maxHp;
            }

            public void CalcClassModel(
                int maxLevel,
                int clearLevel,
                int expGainIncreaseValue,
                int maxHp,
                int hpmpBalance,
                int attackAdd,
                int defenseAdd,
                int magicAdd,
                int magicDefenseAdd,
                int speedAdd,
                int luckAdd
            ) {
                //HPMPバランス
                this.hpmpBalance = hpmpBalance;

                //各パラメーターの最大値
                this.maxHp = Mathf.CeilToInt((float)(maxHp * (0.75 + 0.05 * hpmpBalance)));

                if (this.hpmpBalance < 0)
                    maxMp = Mathf.CeilToInt((float)(maxHp * (0.3 - 1.0 * hpmpBalance * 0.04)));
                else
                    maxMp = Mathf.CeilToInt((float)(maxHp * (0.3 - 1.0 * hpmpBalance * 0.06)));

                maxAttack = Mathf.CeilToInt((float)(maxHp * (0.41 + 0.012 * hpmpBalance + attackAdd * 0.002)));
                maxDefense = Mathf.CeilToInt((float)(maxHp * (0.252 + 0.006 * hpmpBalance + defenseAdd * 0.002)));
                ;
                maxSpeed = Mathf.CeilToInt((float)(maxHp * (0.25 + 0.014 * hpmpBalance + speedAdd * 0.002)));
                maxLuck = Mathf.CeilToInt((float)(maxHp * (0.16 - 0.008 * hpmpBalance + luckAdd * 0.002)));

                if (this.hpmpBalance < 0)
                {
                    maxMagic = Mathf.CeilToInt((float)(maxHp * (0.315 - 0.021 * hpmpBalance + magicAdd * 0.002)));
                    maxMagicDefense = Mathf.CeilToInt((float)(maxHp * (0.2 - 0.01 * hpmpBalance + magicDefenseAdd * 0.002)));
                }
                else
                {
                    maxMagic = Mathf.CeilToInt((float)(maxHp * (0.315 - 0.063 * hpmpBalance + magicAdd * 0.002)));
                    maxMagicDefense = Mathf.CeilToInt((float)(maxHp * (0.2 - 0.04 * hpmpBalance + magicDefenseAdd * 0.002)));
                }

                //各パラメーターの最小値
                minHp = Mathf.CeilToInt((float)(maxHp * (0.045 + 0.001 * hpmpBalance)));
                minMp = Mathf.CeilToInt((float)(maxHp * (0.018 - 0.0036 * hpmpBalance)));
                minAttack = Mathf.CeilToInt((float)(maxAttack * 0.045));
                minDefense = Mathf.CeilToInt((float)(maxDefense * 0.045));
                minMagic = Mathf.CeilToInt((float)(maxMagic * 0.045));
                minMagicDefense = Mathf.CeilToInt((float)(maxMagicDefense * 0.045));
                minSpeed = Mathf.CeilToInt((float)(maxSpeed * 0.045));
                minLuck = Mathf.CeilToInt((float)(maxLuck * 0.045));

                //標準モデル作成時の各初期値を保持
                systemMaxLevel = maxLevel;
                systemClearLevel = clearLevel;
                systemExpGainIncreaseValue = expGainIncreaseValue;
                systemMaxHp = maxHp;
            }

            public void CalcAssumedLevel(int level) {
                //引数で渡された想定レベルでのパラメータを計算する
                if (level < systemMaxLevel + 1)
                {
                    hp = CalcAssumedLevelBeforeClear(minHp, maxHp, level);
                    mp = CalcAssumedLevelBeforeClear(minMp, maxMp, level);
                    attack = CalcAssumedLevelBeforeClear(minAttack, maxAttack, level);
                    defense = CalcAssumedLevelBeforeClear(minDefense, maxDefense, level);
                    magic = CalcAssumedLevelBeforeClear(minMagic, maxMagic, level);
                    magicDefense = CalcAssumedLevelBeforeClear(minMagicDefense, maxMagicDefense, level);
                    speed = CalcAssumedLevelBeforeClear(minSpeed, maxSpeed, level);
                    luck = CalcAssumedLevelBeforeClear(minLuck, maxLuck, level);
                }
                else
                {
                    hp = CalcAssumedLevelAfterClear(minHp, maxHp, level);
                    mp = CalcAssumedLevelAfterClear(minMp, maxMp, level);
                    attack = CalcAssumedLevelAfterClear(minAttack, maxAttack, level);
                    defense = CalcAssumedLevelAfterClear(minDefense, maxDefense, level);
                    magic = CalcAssumedLevelAfterClear(minMagic, maxMagic, level);
                    magicDefense = CalcAssumedLevelAfterClear(minMagicDefense, maxMagicDefense, level);
                    speed = CalcAssumedLevelAfterClear(minSpeed, maxSpeed, level);
                    luck = CalcAssumedLevelAfterClear(minLuck, maxLuck, level);
                }

                this.level = level;
            }

            private int CalcAssumedLevelBeforeClear(int minParam, int maxParam, int calcLevel) {
                var b = maxParam * (1.0f * systemClearLevel / systemMaxLevel * 0.15f + 0.85f);
                return Mathf.CeilToInt(minParam + (b - minParam) / (systemClearLevel - 1.0f) * (calcLevel - 1.0f));
            }

            private int CalcAssumedLevelAfterClear(int minParam, int maxParam, int calcLevel) {
                var b = maxParam * (1.0f * systemClearLevel / systemMaxLevel * 0.15f + 0.85f);
                return Mathf.CeilToInt(b + (maxParam - b) / (systemMaxLevel - systemClearLevel) *
                    (calcLevel - systemClearLevel));
            }

            public void DebugLog() {
            }
        }
    }
}