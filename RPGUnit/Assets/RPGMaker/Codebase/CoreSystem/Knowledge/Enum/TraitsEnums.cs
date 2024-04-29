using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.Enum
{
    public class TraitsEnums
    {
        public enum TraitsCategory
        {
            RESISTANCE = 1,
            ABILITY_SCORE,
            ATTACK,
            SKILL,
            EQUIPMENT,
            OTHER
        }

        public enum TraitsResistance
        {
            ATTRIBUTE_REINFORCEMENT = 1,
            ATTRIBUTE_WEAKEND,
            STATE_REINFORCEMENT,
            STATE_WEAKEND,
            NORMAL_ABILITY_SCORE,
            ADD_ABILITY_SCORE,
            SPECIAL_ABILITY_SCORE,
            ATTACK_ATTRIBUTE,
            ATTACK_STATE,
            ATTACK_SPEED_CORRECTION,
            ATTACK_ADD_COUNT,
            ADD_TYPE,
            SEALED_TYPE,
            ADD_SKILL,
            SEALED_SKILL,
            ADD_WEAPON_TYPE,
            ADD_ARMOR_TYPE,
            FIXED_EQUIPMENT,
            SEALED_EQUIPMENT,
            SLOT_TYPE,
            ADD_ACTION_COUNT,
            SPECIAL_FLAG,
            DISAPPEARANCE_EFFECT,
            PARTY_ABILITY
        }

        public List<string> traitsCategoryLabel = new List<string>
        {
            "耐性",
            "能力値",
            "攻撃",
            "スキル",
            "装備",
            "その他"
        };

        public List<string> traitsResistanceLabel = new List<string>
        {
            "属性有効度",
            "弱体有効度",
            "ステート有効度",
            "ステート無効化",
            "通常能力値",
            "追加能力値",
            "特殊能力値",
            "攻撃時属性",
            "攻撃時ステート",
            "攻撃速度補正",
            "攻撃追加回数",
            "スキルタイプ追加",
            "スキルタイプ封印",
            "スキル追加",
            "スキル封印",
            "武器タイプ装備",
            "防具タイプ装備",
            "装備固定",
            "装備封印",
            "スロットタイプ",
            "行動回数追加",
            "特殊フラグ",
            "パーティ能力"
        };
    }
}