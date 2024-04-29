using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCustom
{
    [Serializable]
    public class SkillCustomDataModel : WithSerialNumberDataModel
    {
        public Basic        basic;
        public string       memo;
        public TargetEffect targetEffect;
        public UserEffect   userEffect;

        public SkillCustomDataModel(Basic basic, TargetEffect targetEffect, UserEffect userEffect, string memo) {
            this.basic = basic;
            this.targetEffect = targetEffect;
            this.userEffect = userEffect;
            this.memo = memo;
        }

        public static SkillCustomDataModel CreateDefault(string id) {
            return new SkillCustomDataModel(
                Basic.CreateDefault(),
                TargetEffect.CreateDefault(),
                UserEffect.CreateDefault(),
                ""
            );
        }

        public bool isEqual(SkillCustomDataModel data) {
            return basic.isEqual(data.basic) &&
                   memo == data.memo &&
                   targetEffect.isEqual(data.targetEffect) &&
                   userEffect.isEqual(data.userEffect);
        }

        [Serializable]
        public class Basic
        {
            public int    assumptionLevel;
            public int    canUseTiming;
            public int    costMp;
            public int    costTp;
            public string description;
            public string id;
            public string message;
            public string name;
            public int    requiredWTypeId1;
            public int    requiredWTypeId2;
            public int    skillType;
            public string iconId;


            public Basic(
                string id,
                string name,
                string description,
                int skillType,
                int assumptionLevel,
                int costMp,
                int costTp,
                int canUseTiming,
                string message,
                int requiredWTypeId1,
                int requiredWTypeId2,
                string iconId
            ) {
                this.id = id;
                this.name = name;
                this.description = description;
                this.skillType = skillType;
                this.assumptionLevel = assumptionLevel;
                this.costMp = costMp;
                this.costTp = costTp;
                this.canUseTiming = canUseTiming;
                this.message = message;
                this.requiredWTypeId1 = requiredWTypeId1;
                this.requiredWTypeId2 = requiredWTypeId2;
                this.iconId = iconId;
            }

            public static Basic CreateDefault() {
                return new Basic(
                    Guid.NewGuid().ToString(),
                    "",
                    "",
                    0,
                    0,
                    0,
                    0,
                    0,
                    "",
                    0,
                    0,
                    "IconSet_000"
                );
            }

            public bool isEqual(Basic data) {
                return assumptionLevel == data.assumptionLevel &&
                       canUseTiming == data.canUseTiming &&
                       costMp == data.costMp &&
                       costTp == data.costTp &&
                       description == data.description &&
                       id == data.id &&
                       message == data.message &&
                       name == data.name &&
                       requiredWTypeId1 == data.requiredWTypeId1 &&
                       requiredWTypeId2 == data.requiredWTypeId2 &&
                       skillType == data.skillType &&
                       iconId == data.iconId;
            }
        }

        [Serializable]
        public class TargetEffect
        {
            public Activate                   activate;
            public Damage                     damage;
            public TargetHeal                 heal;
            public List<TraitCommonDataModel> otherEffects;
            public int                        randomNumber;
            public int                        targetRange;
            public int                        targetStatus;
            public int                        targetTeam;
            public int                        targetUser;

            public TargetEffect(
                int targetTeam,
                int targetUser,
                int targetRange,
                int randomNumber,
                int targetStatus,
                Activate activate,
                TargetHeal heal,
                Damage damage,
                List<TraitCommonDataModel> otherEffects
            ) {
                this.targetTeam = targetTeam;
                this.targetUser = targetUser;
                this.targetRange = targetRange;
                this.randomNumber = randomNumber;
                this.targetStatus = targetStatus;
                this.activate = activate;
                this.heal = heal;
                this.damage = damage;
                this.otherEffects = otherEffects;
            }

            public static TargetEffect CreateDefault() {
                var damame = Damage.CreateDefault();
                return new TargetEffect(
                    0,
                    0,
                    0,
                    0,
                    0,
                    Activate.CreateDefault(),
                    TargetHeal.CreateDefault(),
                    damame,
                    new List<TraitCommonDataModel>()
                );
            }

            public bool isEqual(TargetEffect data) {
                if (otherEffects.Count != data.otherEffects.Count)
                    return false;

                for (int i = 0; i < otherEffects.Count; i++)
                    if (!otherEffects[i].isEqual(data.otherEffects[i]))
                        return false;

                return activate.isEqual(data.activate) &&
                       damage.isEqual(data.damage) &&
                       heal.isEqual(data.heal) &&
                       randomNumber == data.randomNumber &&
                       targetRange == data.targetRange &&
                       targetStatus == data.targetStatus &&
                       targetTeam == data.targetTeam &&
                       targetUser == data.targetUser;
            }
        }

        [Serializable]
        public class UserEffect
        {
            public Activate                   activate;
            public Damage                     damage;
            public int                        getTp;
            public UserHeal                   heal;
            public List<TraitCommonDataModel> otherEffects;
            public int                        randomNumber;
            public int                        targetRange;
            public int                        targetStatus;
            public int                        targetTeam;

            public UserEffect(
                int targetTeam,
                int targetRange,
                int randomNumber,
                int targetStatus,
                int getTp,
                Activate activate,
                UserHeal heal,
                Damage damage,
                List<TraitCommonDataModel> otherEffects
            ) {
                this.targetTeam = targetTeam;
                this.targetRange = targetRange;
                this.randomNumber = randomNumber;
                this.targetStatus = targetStatus;
                this.getTp = getTp;
                this.activate = activate;
                this.heal = heal;
                this.damage = damage;
                this.otherEffects = otherEffects;
            }

            public static UserEffect CreateDefault() {
                var damage = Damage.CreateDefault();
                return new UserEffect(
                    0,
                    0,
                    0,
                    0,
                    0,
                    Activate.CreateDefault(),
                    UserHeal.CreateDefault(),
                    damage,
                    new List<TraitCommonDataModel>()
                );
            }

            public bool isEqual(UserEffect data) {
                if (otherEffects.Count != data.otherEffects.Count)
                    return false;

                for (int i = 0; i < otherEffects.Count; i++)
                    if (!otherEffects[i].isEqual(data.otherEffects[i]))
                        return false;

                return activate.isEqual(data.activate) &&
                       damage.isEqual(data.damage) &&
                       getTp == data.getTp &&
                       heal.isEqual(data.heal) &&
                       randomNumber == data.randomNumber &&
                       targetRange == data.targetRange &&
                       targetStatus == data.targetStatus &&
                       targetTeam == data.targetTeam;
            }
        }

        [Serializable]
        public class Activate
        {
            public string animationId;
            public int    continuousNumber;
            public int    correctionSpeed;
            public int    hitType;
            public int    successRate;

            public Activate(
                int correctionSpeed,
                int successRate,
                int continuousNumber,
                int hitType,
                string animationId
            ) {
                this.correctionSpeed = correctionSpeed;
                this.successRate = successRate;
                this.continuousNumber = continuousNumber;
                this.hitType = hitType;
                this.animationId = animationId;
            }

            public static Activate CreateDefault() {
                return new Activate(0, 100, 1, 0, "");
            }

            public bool isEqual(Activate data) {
                return animationId == data.animationId &&
                       continuousNumber == data.continuousNumber &&
                       correctionSpeed == data.correctionSpeed &&
                       hitType == data.hitType &&
                       successRate == data.successRate;
            }
        }

        [Serializable]
        public class TargetHeal
        {
            public ItemEffectHealParam hp;
            public ItemEffectHealParam mp;
            public ItemEffectHealParam tp;

            public TargetHeal(ItemEffectHealParam hp, ItemEffectHealParam mp, ItemEffectHealParam tp) {
                this.hp = hp;
                this.mp = mp;
                this.tp = tp;
            }

            public static TargetHeal CreateDefault() {
                return new TargetHeal(ItemEffectHealParam.CreateDefault(), ItemEffectHealParam.CreateDefault(),
                    ItemEffectHealParam.CreateDefault());
            }

            public bool isEqual(TargetHeal data) {
                return hp.isEqual(data.hp) &&
                       mp.isEqual(data.mp) &&
                       tp.isEqual(data.tp);
            }
        }

        [Serializable]
        public class UserHeal
        {
            public ItemEffectHealParam hp;
            public ItemEffectHealParam mp;
            public ItemEffectHealParam tp;

            public UserHeal(ItemEffectHealParam hp, ItemEffectHealParam mp, ItemEffectHealParam tp) {
                this.hp = hp;
                this.mp = mp;
                this.tp = tp;
            }

            public static UserHeal CreateDefault() {
                return new UserHeal(ItemEffectHealParam.CreateDefault(), ItemEffectHealParam.CreateDefault(),
                    ItemEffectHealParam.CreateDefault());
            }

            public bool isEqual(UserHeal data) {
                return hp.isEqual(data.hp) &&
                       mp.isEqual(data.mp) &&
                       tp.isEqual(data.tp);
            }
        }

        [Serializable]
        public class ItemEffectHealParam
        {
            public ItemEffectHealParamCalc   calc;
            public int                       enabled;
            public ItemEffectHealParamFixed  fix;
            public ItemEffectHealParamPerMax perMax;

            public ItemEffectHealParam(
                ItemEffectHealParamPerMax perMax,
                ItemEffectHealParamFixed fix,
                ItemEffectHealParamCalc calc,
                int enabled
            ) {
                this.perMax = perMax;
                this.fix = fix;
                this.calc = calc;
                this.enabled = enabled;
            }

            public static ItemEffectHealParam CreateDefault() {
                return new ItemEffectHealParam(ItemEffectHealParamPerMax.CreateDefault(),
                    ItemEffectHealParamFixed.CreateDefault(), ItemEffectHealParamCalc.CreateDefault(), 0);
            }

            public bool isEqual(ItemEffectHealParam data) {
                return calc.isEqual(data.calc) &&
                       enabled == data.enabled &&
                       fix.isEqual(data.fix) &&
                       perMax.isEqual(data.perMax);
            }
        }

        [Serializable]
        public class ItemEffectHealParamCalc
        {
            public int    distribute;
            public int    distributeEnabled;
            public int    enabled;
            public int    max;
            public int    maxEnabled;
            public string value;

            public ItemEffectHealParamCalc(
                int enabled,
                string value,
                int distributeEnabled,
                int distribute,
                int maxEnabled,
                int max
            ) {
                this.enabled = enabled;
                this.value = value;
                this.distributeEnabled = distributeEnabled;
                this.distribute = distribute;
                this.maxEnabled = maxEnabled;
                this.max = max;
            }

            public static ItemEffectHealParamCalc CreateDefault() {
                return new ItemEffectHealParamCalc(0, "", 0, 0, 0, 0);
            }

            public bool isEqual(ItemEffectHealParamCalc data) {
                return distribute == data.distribute &&
                       distributeEnabled == data.distributeEnabled &&
                       enabled == data.enabled &&
                       max == data.max &&
                       maxEnabled == data.maxEnabled &&
                       value == data.value;
            }
        }

        [Serializable]
        public class ItemEffectHealParamFixed
        {
            public int enabled;
            public int value;

            public ItemEffectHealParamFixed(int enabled, int value) {
                this.enabled = enabled;
                this.value = value;
            }

            public static ItemEffectHealParamFixed CreateDefault() {
                return new ItemEffectHealParamFixed(0, 0);
            }

            public bool isEqual(ItemEffectHealParamFixed data) {
                return enabled == data.enabled &&
                       value == data.value;
            }
        }

        [Serializable]
        public class ItemEffectHealParamPerMax
        {
            public int distribute;
            public int distributeEnabled;
            public int enabled;
            public int max;
            public int maxEnabled;
            public int value;

            public ItemEffectHealParamPerMax(
                int enabled,
                int value,
                int distributeEnabled,
                int distribute,
                int maxEnabled,
                int max
            ) {
                this.enabled = enabled;
                this.value = value;
                this.distributeEnabled = distributeEnabled;
                this.distribute = distribute;
                this.maxEnabled = maxEnabled;
                this.max = max;
            }

            public static ItemEffectHealParamPerMax CreateDefault() {
                return new ItemEffectHealParamPerMax(0, 0, 0, 0, 0, 0);
            }

            public bool isEqual(ItemEffectHealParamPerMax data) {
                return distribute == data.distribute &&
                       distributeEnabled == data.distributeEnabled &&
                       enabled == data.enabled &&
                       max == data.max &&
                       maxEnabled == data.maxEnabled &&
                       value == data.value;
            }
        }


        [Serializable]
        public class Damage
        {
            public int            attackType;
            public AutoGuideSkill autoguide;
            public int            autoguideEnabled;
            public int            critical;
            public int            damageType;
            public int            distribute;
            public int            distributeEnabled;
            public List<int>      elements;
            public int            max;
            public int            maxEnabled;
            public int            min;
            public int            minEnabled;
            public string         value;

            public Damage(
                int damageType,
                int attackType,
                List<int> elements,
                string value,
                int distributeEnabled,
                int distribute,
                int maxEnabled,
                int max,
                int minEnabled,
                int min,
                int critical,
                int autoGuideEnabled,
                AutoGuideSkill autoguide
            ) {
                this.damageType = damageType;
                this.attackType = attackType;
                this.elements = elements;
                this.value = value;
                this.distributeEnabled = distributeEnabled;
                this.distribute = distribute;
                this.maxEnabled = maxEnabled;
                this.max = max;
                this.minEnabled = minEnabled;
                this.min = min;
                this.critical = critical;
                autoguideEnabled = autoGuideEnabled;
                this.autoguide = autoguide;
            }

            public static Damage CreateDefault() {
                return new Damage(
                    0, 0,
                    new List<int> {0},
                    "",
                    0, 0, 0, 0, 0, 0, 0, 0, AutoGuideSkill.CreateDefault());
            }

            public bool isEqual(Damage data) {
                if (attackType != data.attackType ||
                    !autoguide.isEqual(data.autoguide) ||
                    autoguideEnabled != data.autoguideEnabled ||
                    critical != data.critical ||
                    damageType != data.damageType ||
                    distribute != data.distribute ||
                    distributeEnabled != data.distributeEnabled ||
                    max != data.max ||
                    maxEnabled != data.maxEnabled ||
                    min != data.min ||
                    minEnabled != data.minEnabled ||
                    value != data.value)
                    return false;

                if (elements.Count != data.elements.Count)
                    return false;

                for (int i = 0; i < elements.Count; i++)
                    if (elements[i] != data.elements[i])
                        return false;

                return true;
            }
        }

        [Serializable]
        public class Fixed
        {
            public int enabled;
            public int value;

            public Fixed(int enabled, int value) {
                this.enabled = enabled;
                this.value = value;
            }

            public static Fixed CreateDefault() {
                return new Fixed(1, 100);
            }
        }

        [Serializable]
        public class Hp
        {
            public int enabled;
            public int value;

            public Hp(int enabled, int value) {
                this.enabled = enabled;
                this.value = value;
            }

            public static Hp CreateDefault() {
                return new Hp(1, 100);
            }
        }

        [Serializable]
        public class Max
        {
            public int enabled;
            public int value;

            public Max(int enabled, int value) {
                this.enabled = enabled;
                this.value = value;
            }

            public static Max CreateDefault() {
                return new Max(1, 100);
            }
        }

        [Serializable]
        public class PerMax
        {
            public int enabled;
            public int value;

            public PerMax(int enabled, int value) {
                this.enabled = enabled;
                this.value = value;
            }

            public static PerMax CreateDefault() {
                return new PerMax(1, 100);
            }
        }

        [Serializable]
        public class Tp
        {
            public int enabled;
            public int value;

            public Tp(int enabled, int value) {
                this.enabled = enabled;
                this.value = value;
            }

            public static Tp CreateDefault() {
                return new Tp(1, 100);
            }
        }

        [Serializable]
        public class AutoGuideSkill
        {
            public float  aMag; //使用側倍率
            public float  bMag; //対象側倍率
            public string calc; //計算式
            public int    critical; //会心
            public int    distribute; //分散度
            public int    distributeEnabled; //分散度の有効状態
            public float  fix; //固定値
            public int    level; //習得想定レベル
            public float  max; //最大値
            public float  min; //最小値
            public int    mp; //消費MP
            public int    targetRange; //対象の対数

            public AutoGuideSkill() {
                level = 1;
                targetRange = 0;
                fix = 0;
                aMag = 0;
                bMag = 0;
                mp = 0;
                calc = "";
                min = 0;
                max = 0;
                distribute = 0;
                critical = 0;
            }

            public static AutoGuideSkill CreateDefault() {
                return new AutoGuideSkill();
            }

            public bool isEqual(AutoGuideSkill data) {
                return aMag == data.aMag &&
                       bMag == data.bMag &&
                       calc == data.calc &&
                       critical == data.critical &&
                       distribute == data.distribute &&
                       distributeEnabled == data.distributeEnabled &&
                       fix == data.fix &&
                       level == data.level &&
                       max == data.max &&
                       min == data.min &&
                       mp == data.mp &&
                       targetRange == data.targetRange;
            }
        }
    }
}