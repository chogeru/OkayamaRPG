using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.Enum
{
    public class ItemEnums
    {
        public enum ItemCanUseTiming
        {
            ALL,
            BATTLE,
            MENU,
            NONE
        }

        public enum ItemType
        {
            NONE = 0,
            NORMAL,
            IMPORTANT,
            HIDDEN_ITEM_A,
            HIDDEN_ITEM_B
        }

        public List<string> damageTypeLabel = new List<string>
        {
            "WORD_0113",
            "WORD_0475",
            "WORD_0476",
            "WORD_0477",
            "WORD_0478"
        };

        public List<string> hitTypeLabel = new List<string>
        {
            "WORD_0113",
            "WORD_0460",
            "WORD_0481",
            "WORD_0420"
        };

        public List<string> itemCanUseTimingLabel = new List<string>
        {
            "WORD_0427",
            "WORD_0428",
            "WORD_0429",
            "WORD_0430"
        };

        public List<string> itemTypeLabel = new List<string>
        {
            "WORD_0113",
            "WORD_1212",
            "WORD_0130",
            "WORD_0549",
            "WORD_0550"
        };

        public List<string> skillTypeLabel = new List<string>
        {
            "WORD_0158",
            "WORD_0420",
            "WORD_0422"
        };

        public List<string> targetStatusLabel = new List<string>
        {
            "WORD_0450",
            "WORD_0451",
            "WORD_0452"
        };
    }
}