using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.Enum
{
    public class StateEnums
    {
        public enum AbnormalEffectLimit
        {
            NONE = 0,
            AUTO_ATTACK_ENEMY,
            AUTO_ATTACK_ALL,
            AUTO_ATTACK_ALLY,
            NOT_MOVE
        }

        public List<string> AbnormalEffectLimitLabel = new List<string>
        {
            "WORD_0113",
            "敵を自動攻撃",
            "敵味方を自動攻撃",
            "味方を自動攻撃",
            "動けない"
        };

        public List<string> StateActionConstraints = new List<string>
        {
            "WORD_0113",
            "WORD_0685",
            "WORD_0684",
            "WORD_0686",
            "WORD_0683",
            "WORD_1363"
        };

        public List<string> StateAnimation = new List<string>
        {
            "WORD_0614",
            "WORD_0666",
            "WORD_0667",
            "WORD_0451"
        };

        public List<string> StateApplication = new List<string>
        {
            "WORD_0680",
            "WORD_0681",
            "WORD_0427"
        };

        public List<string> StateBattling = new List<string>
        {
            "WORD_0113",
            "WORD_0690",
            "WORD_0691"
        };

        public List<string> StateSuperposition = new List<string>
        {
            "WORD_0113",
            "WORD_0669",
            "WORD_0670",
            "WORD_0671",
            "WORD_0672",
            "WORD_0673",
            "WORD_0674",
            "WORD_0675",
            "WORD_0676",
            "WORD_0677",
            "WORD_0678"
        };
    }
}