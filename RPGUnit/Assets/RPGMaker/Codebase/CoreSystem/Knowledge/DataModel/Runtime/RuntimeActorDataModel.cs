using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime
{
    [Serializable]
    public class RuntimeActorDataModel
    {
        public string       actorId;
        public string       advImage;
        public string       battlerImage;
        public string       characterImage;
        public string       classId;
        public List<Equip>  equips;
        public Exp          exp;
        public string       faceImage;
        public int          hidden;
        public int          hp;
        public string       lastCommandSymbol;
        public int          lastCommandSymbolStypeId;
        public int          lastTargetIndex;
        public Skill        lastBattleSkill;
        public Skill        lastMenuSkill;
        public int          level;
        public int          mp;
        public string       name;
        public string       nickname;
        public ParamPlus    paramPlus;
        public string       profile;
        public List<string> skills;
        public List<State>  states;
        public int          tp;
        public int          initialized;

        public RuntimeActorDataModel(
            string id,
            string name,
            string nickname,
            string profile,
            string classId,
            int level,
            string characterImage,
            string faceImage,
            string battlerImage,
            string advImage,
            Exp exp,
            int hp,
            int mp,
            int tp,
            ParamPlus paramPlus,
            int initialized
        ) {
            actorId = id;
            this.name = name;
            this.nickname = nickname;
            this.profile = profile;
            this.classId = classId;
            this.level = level;
            this.characterImage = characterImage;
            this.faceImage = faceImage;
            this.battlerImage = battlerImage;
            this.advImage = advImage;
            this.exp = exp;
            this.hp = hp;
            this.mp = mp;
            this.tp = tp;
            hidden = 0;
            this.paramPlus = paramPlus;
            this.initialized = initialized;

            states = new List<State>();
            skills = new List<string>();
            var equip = new Equip();
            equip.itemId = "1";
            equip.equipType = "";
            equips = new List<Equip> {equip};

            lastMenuSkill = new Skill();
            lastMenuSkill.itemId = "0";
            lastMenuSkill.dataClass = "";

            lastBattleSkill = new Skill();
            lastBattleSkill.itemId = "0";
            lastBattleSkill.dataClass = "";

            lastTargetIndex = 0;
            lastCommandSymbol = "";
            lastCommandSymbolStypeId = 0;
        }

        /// <summary>
        ///     追加能力値を含めた現在の最大HPを取得する
        /// </summary>
        /// <param name="currentClass">アクターの職業</param>
        /// <returns>追加能力値を含めた現在の最大HP</returns>
        public int GetCurrentMaxHp(ClassDataModel currentClass) {
            var ret = currentClass.parameter.maxHp[level] + paramPlus.maxHp;
            return Math.Max(ret, 1);
        }

        /// <summary>
        ///     追加能力値を含めた現在の最大MPを取得する
        /// </summary>
        /// <param name="currentClass">アクターの職業</param>
        /// <returns>追加能力値を含めた現在の最大MP</returns>
        public int GetCurrentMaxMp(ClassDataModel currentClass) {
            var ret = currentClass.parameter.maxMp[level] + paramPlus.maxMp;
            return Math.Max(ret, 0);
        }

        /// <summary>
        ///     追加能力値を含めた現在の攻撃力を取得する
        /// </summary>
        /// <param name="currentClass">アクターの職業</param>
        /// <returns>追加能力値を含めた現在の攻撃力</returns>
        public int GetCurrentAttack(ClassDataModel currentClass) {
            var ret = currentClass.parameter.attack[level] + paramPlus.attack;
            return Math.Max(ret, 1);
        }

        /// <summary>
        ///     追加能力値を含めた現在の防御力を取得する
        /// </summary>
        /// <param name="currentClass">アクターの職業</param>
        /// <returns>追加能力値を含めた現在の防御力</returns>
        public int GetCurrentDefense(ClassDataModel currentClass) {
            var ret = currentClass.parameter.defense[level] + paramPlus.defense;
            return Math.Max(ret, 1);
        }

        /// <summary>
        ///     追加能力値を含めた現在の魔法力を取得する
        /// </summary>
        /// <param name="currentClass">アクターの職業</param>
        /// <returns>追加能力値を含めた現在の魔法力</returns>
        public int GetCurrentMagicAttack(ClassDataModel currentClass) {
            var ret = currentClass.parameter.magicAttack[level] + paramPlus.magicAttack;
            return Math.Max(ret, 1);
        }

        /// <summary>
        ///     追加能力値を含めた現在の魔法防御を取得する
        /// </summary>
        /// <param name="currentClass">アクターの職業</param>
        /// <returns>追加能力値を含めた現在の魔法防御</returns>
        public int GetCurrentMagicDefense(ClassDataModel currentClass) {
            var ret = currentClass.parameter.magicDefense[level] + paramPlus.magicDefence;
            return Math.Max(ret, 1);
        }

        /// <summary>
        ///     追加能力値を含めた現在の敏捷性を取得する
        /// </summary>
        /// <param name="currentClass">アクターの職業</param>
        /// <returns>追加能力値を含めた現在の敏捷性</returns>
        public int GetCurrentAgility(ClassDataModel currentClass) {
            var ret = currentClass.parameter.speed[level] + paramPlus.speed;
            return Math.Max(ret, 1);
        }

        /// <summary>
        ///     追加能力値を含めた現在の運を取得する
        /// </summary>
        /// <param name="currentClass">アクターの職業</param>
        /// <returns>追加能力値を含めた現在の運</returns>
        public int GetCurrentLuck(ClassDataModel currentClass) {
            var ret = currentClass.parameter.luck[level] + paramPlus.luck;
            return Math.Max(ret, 1);
        }

        [Serializable]
        public class Exp
        {
            public string classId;
            public int    value;
        }

        [Serializable]
        public class ParamPlus
        {
            public int attack;
            public int defense;
            public int luck;
            public int magicAttack;
            public int magicDefence;
            public int maxHp;
            public int maxMp;
            public int maxTp;
            public int speed;
        }

        [Serializable]
        public class State
        {
            public string id;
            public int    walkingCount;
        }

        [Serializable]
        public class Equip
        {
            public string equipType;
            public string itemId;
        }

        [Serializable]
        public class Skill
        {
            public string dataClass;
            public string itemId;
        }
    }
}