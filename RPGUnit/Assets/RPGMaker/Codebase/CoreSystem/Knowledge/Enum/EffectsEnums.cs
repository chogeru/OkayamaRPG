using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.Enum
{
    public class EffectsEnums
    {
        public enum EffectCategory
        {
            HEAL_HP = 0,
            HEAL_MP,
            HEAL_TP,
            DELETE_STATE,
            ADD_BUFF,
            ADD_DEBUFF,
            DELETE_BUFF,
            DELETE_DEBUFF,
            SPECIAL_EFFECT,
            ADD_PARAMETER,
            MASTER_SKILL,
            COMMON_EVENT
        }

        public List<string> EffectCategoryLabel = new List<string>
        {
            "HP回復",
            "MP回復",
            "TP増加",
            "状態追加",
            "状態削除",
            "バフ追加",
            "デバフ追加",
            "バフ削除",
            "デバフ削除",
            "特殊効果",
            "パラメータ追加",
            "スキル習得",
            "CommonEvent"
        };
    }
}