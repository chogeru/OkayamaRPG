using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class
{
    [Serializable]
    public class AutoGuideDataModel
    {
        public List<AutoGuideRatio> armorRatio;
        public List<AutoGuideRatio> baseParameterRatio;
        public string               id;
        public List<AutoGuideRatio> weaponRatio;

        public AutoGuideDataModel(
            string id,
            List<AutoGuideRatio> baseParameterRatio,
            List<AutoGuideRatio> weaponRatio,
            List<AutoGuideRatio> armorRatio
        ) {
            this.id = id;
            this.baseParameterRatio = baseParameterRatio;
            this.weaponRatio = weaponRatio;
            this.armorRatio = armorRatio;
        }

        public bool isEqual(AutoGuideDataModel data) {
            if (armorRatio.Count != data.armorRatio.Count || 
                baseParameterRatio.Count != data.baseParameterRatio.Count ||
                weaponRatio.Count != data.weaponRatio.Count ||
                id != data.id)
                return false;

            for (int i = 0; i < armorRatio.Count; i++)
                if (!armorRatio[i].isEqual(data.armorRatio[i]))
                    return false;

            for (int i = 0; i < baseParameterRatio.Count; i++)
                if (!baseParameterRatio[i].isEqual(data.baseParameterRatio[i]))
                    return false;

            for (int i = 0; i < weaponRatio.Count; i++)
                if (!weaponRatio[i].isEqual(data.weaponRatio[i]))
                    return false;

            return true;
        }

        [Serializable]
        public class AutoGuideRatio
        {
            public float attack;
            public float defense;
            public float luck;
            public float magicAttack;
            public float magicDefense;
            public float maxHp;
            public float maxMp;
            public float speed;

            public AutoGuideRatio(
                float maxHp,
                float maxMp,
                float attack,
                float defense,
                float magicAttack,
                float magicDefense,
                float speed,
                float luck
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

            public bool isEqual(AutoGuideRatio data) {
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
    }
}