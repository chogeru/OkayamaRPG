using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class
{
    [Serializable]
    public class ClassDataModel : WithSerialNumberDataModel
    {
        public AbilityAdd                 abilityAdd;
        public AbilityScore               abilityScore;
        public AbilitySp                  abilitySp;
        public List<string>               armorTypes;
        public AutoGuide                  autoGuide;
        public int                        baseHpMaxValue;
        public Basic                      basic;
        public int                        clearLevel;
        public string                     element;
        public int                        expGainIncreaseValue;
        public ExpScore                   expScore;
        public string                     id;
        public int                        maxLevel;
        public Parameter                  parameter;
        public List<SkillType>            skillTypes;
        public List<TraitCommonDataModel> traits;
        public List<string>               weaponTypes;

        public ClassDataModel(
            string id,
            Basic basic,
            string element,
            List<string> weaponTypes,
            List<string> armorTypes,
            List<SkillType> skillTypes,
            AutoGuide autoGuide,
            ExpScore expScore,
            AbilityScore abilityScore,
            Parameter parameter,
            AbilityAdd abilityAdd,
            AbilitySp abilitySp,
            List<TraitCommonDataModel> traits,
            int maxLevel,
            int clearLevel,
            int expGainIncreaseValue,
            int baseHpMaxValue
        ) {
            this.id = id;
            this.basic = basic;
            this.element = element;
            this.weaponTypes = weaponTypes;
            this.armorTypes = armorTypes;
            this.skillTypes = skillTypes;
            this.autoGuide = autoGuide;
            this.expScore = expScore;
            this.abilityScore = abilityScore;
            this.parameter = parameter;
            this.abilityAdd = abilityAdd;
            this.abilitySp = abilitySp;
            this.traits = traits;
            this.maxLevel = maxLevel;
            this.clearLevel = clearLevel;
            this.expGainIncreaseValue = expGainIncreaseValue;
            this.baseHpMaxValue = baseHpMaxValue;

            UpdateGraph();
        }

        public static ClassDataModel CreateDefault(string id, string name) {
            return new ClassDataModel(id, Basic.CreateDefault(id, name), "", new List<string>(), new List<string>(),
                new List<SkillType>(), AutoGuide.CreateDefault(), ExpScore.CreateDefault(),
                AbilityScore.CreateDefault(), Parameter.CreateDefault(), AbilityAdd.CreateDefault(),
                AbilitySp.CreateDefault(), new List<TraitCommonDataModel>(), 99, 60, 10, 10);
        }

        private void SetStatusValue(List<int> pram, Type type) {
            // グラフの計算
            var clearParam = 0;
            var _paramOne = 0;
            var _paramMax = 0;
            var _paramPeakLv = 0;
            var _paramGrow = 0;

            switch (type)
            {
                case Type.Hp:
                    _paramOne = abilityScore.maxHp.paramOne;
                    _paramMax = abilityScore.maxHp.paramMax;
                    _paramPeakLv = abilityScore.maxHp.paramPeakLv;
                    _paramGrow = abilityScore.maxHp.growType;
                    break;
                case Type.Mp:
                    _paramOne = abilityScore.maxMp.paramOne;
                    _paramMax = abilityScore.maxMp.paramMax;
                    _paramPeakLv = abilityScore.maxMp.paramPeakLv;
                    _paramGrow = abilityScore.maxMp.growType;
                    break;
                case Type.Attack:
                    _paramOne = abilityScore.attack.paramOne;
                    _paramMax = abilityScore.attack.paramMax;
                    _paramPeakLv = abilityScore.attack.paramPeakLv;
                    _paramGrow = abilityScore.attack.growType;
                    break;
                case Type.Defense:
                    _paramOne = abilityScore.defense.paramOne;
                    _paramMax = abilityScore.defense.paramMax;
                    _paramPeakLv = abilityScore.defense.paramPeakLv;
                    _paramGrow = abilityScore.defense.growType;
                    break;
                case Type.MagicAttack:
                    _paramOne = abilityScore.magicAttack.paramOne;
                    _paramMax = abilityScore.magicAttack.paramMax;
                    _paramPeakLv = abilityScore.magicAttack.paramPeakLv;
                    _paramGrow = abilityScore.magicAttack.growType;
                    break;
                case Type.MagicDefence:
                    _paramOne = abilityScore.magicDefense.paramOne;
                    _paramMax = abilityScore.magicDefense.paramMax;
                    _paramPeakLv = abilityScore.magicDefense.paramPeakLv;
                    _paramGrow = abilityScore.magicDefense.growType;
                    break;
                case Type.Speed:
                    _paramOne = abilityScore.speed.paramOne;
                    _paramMax = abilityScore.speed.paramMax;
                    _paramPeakLv = abilityScore.speed.paramPeakLv;
                    _paramGrow = abilityScore.speed.growType;
                    break;
                case Type.Luck:
                    _paramOne = abilityScore.luck.paramOne;
                    _paramMax = abilityScore.luck.paramMax;
                    _paramPeakLv = abilityScore.luck.paramPeakLv;
                    _paramGrow = abilityScore.luck.growType;
                    break;
            }

            //0はあり得ないので、この場合はMaxLVがピークとする
            if (_paramPeakLv <= 0) _paramPeakLv = maxLevel;

            for (var i = 1; i <= maxLevel; i++)
            {
                if (_paramOne == _paramMax)
                {
                    pram[i] = _paramOne;
                    continue;
                }

                double x;

                // ピークレベルで計算を分ける
                if (i > _paramPeakLv)
                {
                    // オートガイド資料に記載
                    var a = clearParam;
                    var b = _paramMax;

                    var n1 = a + (b - a) * (i - _paramPeakLv) / (maxLevel - _paramPeakLv);
                    var n2 = a + (b - a) * (i - _paramPeakLv) * (i - _paramPeakLv) / (maxLevel - _paramPeakLv) /
                        (maxLevel - _paramPeakLv);
                    x = Math.Ceiling((double) (n2 * _paramGrow + n1 * (10 - _paramGrow)) / 10);
                }
                else
                {
                    // オートガイド資料に記載
                    var a = _paramOne;
                    var b = _paramMax * (_paramPeakLv / maxLevel * 0.15 + 0.85);
                    var n1 = a + (b - a) * (i - 1) / (_paramPeakLv - 1);
                    var n2 = a + (b - a) * (i - 1) * (i - 1) / (_paramPeakLv - 1) / (_paramPeakLv - 1);
                    x = (n2 * _paramGrow + n1 * (10 - _paramGrow)) / 10;

                    if (i == _paramPeakLv) clearParam = (int) x;
                }

                pram[i] = (int) x;
            }
        }

        // グラフの更新
        public void UpdateGraph() {
            SetStatusValue(parameter.maxHp, Type.Hp);
            SetStatusValue(parameter.maxMp, Type.Mp);
            SetStatusValue(parameter.attack, Type.Attack);
            SetStatusValue(parameter.defense, Type.Defense);
            SetStatusValue(parameter.magicAttack, Type.MagicAttack);
            SetStatusValue(parameter.magicDefense, Type.MagicDefence);
            SetStatusValue(parameter.speed, Type.Speed);
            SetStatusValue(parameter.luck, Type.Luck);
        }

        public List<int> GetExpTable(int maxLevel, int increaseValueA, int increaseValueB, int growType, int clearLevel, int expGainIncreaseValue) {
            this.maxLevel = maxLevel;
            this.expScore.increaseValueA = increaseValueA;
            this.expScore.increaseValueB = increaseValueB;
            this.expScore.growType = growType;
            this.clearLevel = clearLevel;
            this.expGainIncreaseValue = expGainIncreaseValue;
            return GetExpTable();
        }

        public List<int> GetExpTable() {
            var maxLevel = this.maxLevel;
            var valueA = this.expScore.increaseValueA;
            var valueB = expScore.increaseValueB;
            var growthValue = expScore.growType;
            var clearLevel = this.clearLevel;
            var maxExp = this.expGainIncreaseValue;

            //旧マスタデータ用の最低値、最大値の丸め処理
            if (valueA < 10) valueA = 10;
            if (valueA > 50) valueA = 50;
            if (valueB < 10) valueB = 10;
            if (valueB > 50) valueB = 50;
            if (growthValue < 0) growthValue = 0;
            if (growthValue > 40) growthValue = 40;


            //経験値曲線の計算を行い、経験値テーブルを作成する
            var ret = new List<int>();

            //----------------------------------
            // 各計算用値
            //----------------------------------
            // 傾きA ＝ (入力値 - 10) * 0.025 + 3
            var increaseA = (valueA - 10) * 0.025 + 3;
            // 傾きB ＝ (50 - 入力値) * 22 + 100
            var increaseB = (50 - valueB) * 22 + 100;
            // 補正値
            var growth = growthValue + 10;
            // 係数1 ＝ (クリアレベル - 1) ^ 傾きA
            var coefficientA = Math.Pow(clearLevel - 1, (int) increaseA);
            // 係数2 ＝ クリアレベル ^ 傾きA - (クリアレベル - 1) ^ 傾きA
            var coefficientB = Math.Pow(clearLevel, (int) increaseA) - Math.Pow(clearLevel - 1, (int) increaseA);
            // 経験値上限 ＝ 経験値上限 - (補正値 + 10) * (最大レベル - 1)
            var maxExpValue = maxExp - growth * (maxLevel - 1);
            // 最大レベル後半の計算値
            // ((最大レベル - クリアレベル) / (1 + 1 / 2.95)  / 傾きB + 1) * 係数2 * (最大レベル - クリアレベル) + 係数1
            var latterCalculation =
                ((maxLevel - clearLevel) / (1 + 1 / 2.95) / increaseB + 1) * coefficientB * (maxLevel - clearLevel) +
                coefficientA;
            // 平均化係数
            var averageValue = maxExpValue / latterCalculation;

            for (var i = 1; i <= maxLevel; i++)
            {
                int value;

                // 経験値の計算
                // クリアレベル以前
                if (i <= clearLevel)
                {
                    // 経験値n = ((level - 1) ^ 傾きA) * 平均化係数 + (level - 1) * 補正値
                    value = (int) (Math.Pow(i - 1, (int) increaseA) * averageValue + (i - 1) * growth);
                }
                else
                {
                    // 係数3 = (level - クリアレベル)
                    var coefficientC = i - clearLevel;
                    // 経験値n = ((係数3 / (1 + 係数3 ^ 2 / ((最大レベル - クリアレベル) ^ 2 * 2.95)) / 傾きB + 1) * 係数2 * 係数3 + 係数1) * 平均化係数 + (level - 1) * 補正値
                    value = (int) (((coefficientC /
                                     (1 + Math.Pow(coefficientC, 2) / (Math.Pow(maxLevel - clearLevel, 2) * 2.95)) /
                                     increaseB +
                                     1) * coefficientB * coefficientC + coefficientA) * averageValue +
                                   (i - 1) * growth);
                }

                ret.Add(value);
            }

            return ret;
        }
        
        public int GetExpForLevel(int level) {
            var expTable = GetExpTable();

            //レベルに必要な経験値は、経験値曲線から配列を用意しているため、それを返却する
            if (expTable.Count < level)
            {
                return expTable[expTable.Count - 1];
            }
            
            //経験値テーブルは、配列要素0番目が Lv1 のものなので、-1したものを返却
            return expTable[level - 1];
        }

        public bool isEqual(ClassDataModel data) {
            if (!abilityAdd.isEqual(data.abilityAdd) ||
                !abilityScore.isEqual(data.abilityScore) ||
                abilitySp.targetedRate != data.abilitySp.targetedRate ||
                !autoGuide.isEqual(data.autoGuide) ||
                baseHpMaxValue != data.baseHpMaxValue ||
                !basic.isEqual(data.basic) ||
                clearLevel != data.clearLevel ||
                element != data.element ||
                expGainIncreaseValue != data.expGainIncreaseValue ||
                !expScore.isEqual(data.expScore) ||
                id != data.id ||
                maxLevel != data.maxLevel ||
                !parameter.isEqual(data.parameter))
                return false;

            if (armorTypes.Count != data.armorTypes.Count ||
                skillTypes.Count != data.skillTypes.Count ||
                traits.Count != data.traits.Count ||
                weaponTypes.Count != data.weaponTypes.Count)
                return false;

            for (int i = 0; i < armorTypes.Count; i++)
                if (armorTypes[i] != data.armorTypes[i])
                    return false;

            for (int i = 0; i < skillTypes.Count; i++)
                if (!skillTypes[i].isEqual(data.skillTypes[i]))
                    return false;

            for (int i = 0; i < traits.Count; i++)
                if (!traits[i].isEqual(data.traits[i]))
                    return false;

            for (int i = 0; i < weaponTypes.Count; i++)
                if (weaponTypes[i] != data.weaponTypes[i])
                    return false;

            return true;
        }

        [Serializable]
        public class Basic
        {
            public AbilityEnabled abilityEnabled;
            public int            expMax;
            public int            hpMax;
            public string         id;
            public Level          level;
            public int            maxLevel;
            public string         name;

            public Basic(
                string id,
                string name,
                Level level,
                int expMax,
                int hpMax,
                int maxLevel,
                AbilityEnabled abilityEnabled
            ) {
                this.id = id;
                this.name = name;
                this.level = level;
                this.expMax = expMax;
                this.hpMax = hpMax;
                this.maxLevel = maxLevel;
                this.abilityEnabled = abilityEnabled;
            }

            public static Basic CreateDefault(string id, string name) {
                return new Basic(id, name, new Level(), 1, 500, 99, AbilityEnabled.CreateDefault());
            }

            public bool isEqual(Basic data) {
                return abilityEnabled.isEqual(data.abilityEnabled) &&
                       expMax == data.expMax &&
                       hpMax == data.hpMax &&
                       id == data.id &&
                       level.isEqual(data.level) &&
                       maxLevel == data.maxLevel &&
                       name == data.name;
            }
        }

        [Serializable]
        public class Level
        {
            public int levelGameClear;
            public int levelMax;

            public bool isEqual(Level data) {
                return levelGameClear == data.levelGameClear &&
                       levelMax == data.levelMax;
            }
        }

        [Serializable]
        public class AutoGuide
        {
            public int attack;
            public int defense;
            public int luck;
            public int magicAttack;
            public int magicDefense;
            public int maxHp;
            public int maxMp;
            public int speed;

            public AutoGuide(
                int maxHp,
                int maxMp,
                int attack,
                int defense,
                int magicAttack,
                int magicDefense,
                int speed,
                int luck
            ) {
                this.maxHp = maxHp;
                this.maxMp = maxMp;
                this.attack = attack;
                this.defense = defense;
                this.magicAttack = magicAttack;
                this.magicDefense = magicDefense;
                this.speed = speed;
                this.luck = luck;
            }

            public static AutoGuide CreateDefault() {
                return new AutoGuide(-1, -1, -1, -1, -1, -1, -1, -1);
            }

            public bool isEqual(AutoGuide data) {
                return attack == data.attack &&
                       defense == data.defense &&
                       luck == data.luck &&
                       magicAttack == data.magicAttack &&
                       magicDefense == data.magicDefense &&
                       maxHp == data.maxHp &&
                       maxMp == data.maxMp &&
                       speed == data.speed;
            }
        }

        [Serializable]
        public class ExpScore
        {
            public int growType;
            public int increaseValueA;
            public int increaseValueB;

            public ExpScore(int increaseValueA, int increaseValueB, int growType) {
                this.increaseValueA = increaseValueA;
                this.increaseValueB = increaseValueB;
                this.growType = growType;
            }

            public static ExpScore CreateDefault() {
                return new ExpScore(10, 10, 0);
            }

            public bool isEqual(ExpScore data) {
                return growType == data.growType &&
                       increaseValueA == data.increaseValueA &&
                       increaseValueB == data.increaseValueB;
            }
        }

        [Serializable]
        public class SkillType
        {
            public int    level;
            public string skillId;

            public SkillType(string skillId, int level) {
                this.skillId = skillId;
                this.level = level;
            }

            public static SkillType CreateDefault() {
                return new SkillType("", 1);
            }

            public bool isEqual(SkillType data) {
                return level == data.level && 
                       skillId == data.skillId;
            }
        }

        [Serializable]
        public class AbilityEnabled
        {
            public int luck;
            public int magicAttack;
            public int magicDefense;
            public int mp;
            public int speed;
            public int tp;

            public AbilityEnabled(int mp, int tp, int magicAttack, int magicDefense, int speed, int luck) {
                this.mp = mp;
                this.tp = tp;
                this.magicAttack = magicAttack;
                this.magicDefense = magicDefense;
                this.speed = speed;
                this.luck = luck;
            }

            public static AbilityEnabled CreateDefault() {
                return new AbilityEnabled(1, 1, 1, 1, 1, 1);
            }

            public bool isEqual(AbilityEnabled data) {
                return luck == data.luck &&
                       magicAttack == data.magicAttack &&
                       magicDefense == data.magicDefense &&
                       mp == data.mp &&
                       speed == data.speed &&
                       tp == data.tp;
            }
        }

        [Serializable]
        public class AbilityScore
        {
            public Ability attack;
            public Ability defense;
            public Ability luck;
            public Ability magicAttack;
            public Ability magicDefense;
            public Ability maxHp;
            public Ability maxMp;
            public Ability speed;

            public AbilityScore(
                Ability maxHp,
                Ability maxMp,
                Ability attack,
                Ability defense,
                Ability magicAttack,
                Ability magicDefense,
                Ability speed,
                Ability luck
            ) {
                this.maxHp = maxHp;
                this.maxMp = maxMp;
                this.attack = attack;
                this.defense = defense;
                this.magicAttack = magicAttack;
                this.magicDefense = magicDefense;
                this.speed = speed;
                this.luck = luck;
            }

            public static AbilityScore CreateDefault() {
                return new AbilityScore(new Ability(1, 1, 0, 1, 1), Ability.CreateDefault(), Ability.CreateDefault(),
                    Ability.CreateDefault(), Ability.CreateDefault(), Ability.CreateDefault(), Ability.CreateDefault(),
                    Ability.CreateDefault());
            }

            public bool isEqual(AbilityScore data) {
                return attack.isEqual(data.attack) &&
                       defense.isEqual(data.defense) &&
                       luck.isEqual(data.luck) &&
                       magicAttack.isEqual(data.magicAttack) &&
                       magicDefense.isEqual(data.magicDefense) &&
                       maxHp.isEqual(data.maxHp) &&
                       maxMp.isEqual(data.maxMp) &&
                       speed.isEqual(data.speed);
            }
        }

        [Serializable]
        public class Ability
        {
            public int enabled;
            public int growType;
            public int paramMax;
            public int paramOne;
            public int paramPeakLv;

            public Ability(int paramOne, int paramMax, int paramPeakLv, int growType, int enabled) {
                this.paramOne = paramOne;
                this.paramMax = paramMax;
                this.paramPeakLv = paramPeakLv;
                this.growType = growType;
                this.enabled = enabled;
            }

            public static Ability CreateDefault() {
                return new Ability(0, 0, 0, 1, 1);
            }

            public bool isEqual(Ability data) {
                return enabled == data.enabled &&
                       growType == data.growType &&
                       paramMax == data.paramMax &&
                       paramOne == data.paramOne &&
                       paramPeakLv == data.paramPeakLv;
            }
        }

        [Serializable]
        public class Parameter
        {
            public List<int> attack;
            public List<int> defense;
            public List<int> luck;
            public List<int> magicAttack;
            public List<int> magicDefense;
            public List<int> maxHp;
            public List<int> maxMp;
            public List<int> speed;

            public Parameter(
                List<int> maxHp,
                List<int> maxMp,
                List<int> attack,
                List<int> defense,
                List<int> magicAttack,
                List<int> magicDefense,
                List<int> speed,
                List<int> luck
            ) {
                this.maxHp = maxHp;
                this.maxMp = maxMp;
                this.attack = attack;
                this.defense = defense;
                this.magicAttack = magicAttack;
                this.magicDefense = magicDefense;
                this.speed = speed;
                this.luck = luck;
            }

            public static Parameter CreateDefault() {
                var data = new List<int>();
                for (var i = 0; i < 100; i++) data.Add(0);
                return new Parameter(new List<int>(data), new List<int>(data), new List<int>(data), new List<int>(data),
                    new List<int>(data), new List<int>(data), new List<int>(data), new List<int>(data));
            }

            public bool isEqual(Parameter data) {
                if (attack.Count != data.attack.Count ||
                    defense.Count != data.defense.Count ||
                    luck.Count != data.luck.Count ||
                    magicAttack.Count != data.magicAttack.Count ||
                    magicDefense.Count != data.magicDefense.Count ||
                    maxHp.Count != data.maxHp.Count ||
                    maxMp.Count != data.maxMp.Count ||
                    speed.Count != data.speed.Count)
                    return false;


                for (int i = 0; i < attack.Count; i++)
                    if (attack[i] != data.attack[i])
                        return false;
                for (int i = 0; i < defense.Count; i++)
                    if (defense[i] != data.defense[i])
                        return false;
                for (int i = 0; i < luck.Count; i++)
                    if (luck[i] != data.luck[i])
                        return false;
                for (int i = 0; i < magicAttack.Count; i++)
                    if (magicAttack[i] != data.magicAttack[i])
                        return false;
                for (int i = 0; i < magicDefense.Count; i++)
                    if (magicDefense[i] != data.magicDefense[i])
                        return false;
                for (int i = 0; i < maxHp.Count; i++)
                    if (maxHp[i] != data.maxHp[i])
                        return false;
                for (int i = 0; i < maxMp.Count; i++)
                    if (maxMp[i] != data.maxMp[i])
                        return false;
                for (int i = 0; i < speed.Count; i++)
                    if (speed[i] != data.speed[i])
                        return false;

                return true;
            }
        }

        [Serializable]
        public class AbilityAdd
        {
            public int criticalRate;
            public int evasionRate;
            public int hitRate;

            public AbilityAdd(int hitRate, int evasionRate, int criticalRate) {
                this.hitRate = hitRate;
                this.evasionRate = evasionRate;
                this.criticalRate = criticalRate;
            }

            public static AbilityAdd CreateDefault() {
                return new AbilityAdd(1, 1, 1);
            }

            public bool isEqual(AbilityAdd data) {
                return criticalRate == data.criticalRate &&
                       evasionRate == data.evasionRate &&
                       hitRate == data.hitRate;
            }
        }

        [Serializable]
        public class AbilitySp
        {
            public int targetedRate;

            public AbilitySp(int targetedRate) {
                this.targetedRate = targetedRate;
            }

            public static AbilitySp CreateDefault() {
                return new AbilitySp(1);
            }

            public bool isEqual(AbilitySp data) {
                return targetedRate == data.targetedRate;
            }
        }

        //成長曲線
        private enum Type
        {
            Hp,
            Mp,
            Attack,
            Defense,
            MagicAttack,
            MagicDefence,
            Speed,
            Luck,
            Max
        }
    }
}