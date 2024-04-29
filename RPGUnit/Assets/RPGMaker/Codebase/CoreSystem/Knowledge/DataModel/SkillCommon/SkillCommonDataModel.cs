using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCommon
{
    [Serializable]
    public class SkillCommonDataModel : WithSerialNumberDataModel
    {
        public enum SkillType
        {
            None = -1,
            NormalAttack, //通常攻撃
            MagicAttack, //魔法
            SpecialAttack //必殺技
        }

        public Damage damage;

        public string id;

        public SkillCommonDataModel(string id, Damage damage) {
            this.id = id;
            this.damage = damage;
        }

        public bool isEqual(SkillCommonDataModel data) {
            return damage.isEqual(data.damage) &&
                   id == data.id;
        }

        [Serializable]
        public class Damage
        {
            public MagicAttack   magicAttack;
            public NormalAttack  normalAttack;
            public SpecialAttack specialAttack;

            public Damage(NormalAttack normalAttack, MagicAttack magicAttack, SpecialAttack specialAttack) {
                this.normalAttack = normalAttack;
                this.magicAttack = magicAttack;
                this.specialAttack = specialAttack;
            }

            public bool isEqual(Damage data) {
                return magicAttack.isEqual(data.magicAttack) &&
                       normalAttack.isEqual(data.normalAttack) &&
                       specialAttack.isEqual(data.specialAttack);
            }
        }

        [Serializable]
        public class NormalAttack
        {
            public float aMag;
            public float bMag;

            public NormalAttack(float aMag, float bMag) {
                this.aMag = aMag;
                this.bMag = bMag;
            }

            public bool isEqual(NormalAttack data) {
                return aMag == data.aMag &&
                       bMag == data.bMag;
            }
        }

        [Serializable]
        public class MagicAttack
        {
            public float aMag;
            public float bMag;
            public float cDmg;

            public MagicAttack(float cDmg, float aMag, float bMag) {
                this.cDmg = cDmg;
                this.aMag = aMag;
                this.bMag = bMag;
            }

            public bool isEqual(MagicAttack data) {
                return aMag == data.aMag &&
                       bMag == data.bMag &&
                       cDmg == data.cDmg;
            }
        }

        [Serializable]
        public class SpecialAttack
        {
            public float aMag;
            public float bMag;
            public float cDmg;

            public SpecialAttack(float cDmg, float aMag, float bMag) {
                this.cDmg = cDmg;
                this.aMag = aMag;
                this.bMag = bMag;
            }

            public bool isEqual(SpecialAttack data) {
                return aMag == data.aMag &&
                       bMag == data.bMag &&
                       cDmg == data.cDmg;
            }
        }
    }
}