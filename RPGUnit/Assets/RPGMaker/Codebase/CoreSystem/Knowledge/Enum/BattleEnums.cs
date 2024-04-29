using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.Enum
{
    public class BattleEnums
    {
        public enum AttackMotion
        {
            Poke,
            Swing,
            Projectile
        }

        public enum AttackMotionImage
        {
            NONE = 0,
            DAGGER,
            SWORD,
            FRAIL,
            AXE,
            WHIP,
            CANE,
            BOW,
            CROSSBOW,
            GUN,
            NAIL,
            GLOVES,
            SPEAR,
            MACE,
            ROD,
            CLUB,
            CHAIN,
            FUTURE_SWORD,
            PIPE,
            PACHINKO,
            SHOTGUN,
            RIFLE,
            CHAINSAW,
            RAILGUN,
            STANROD,
            USER1,
            USER2,
            USER3,
            USER4,
            USER5,
            USER6
        }

        public List<string> AttackMotionImageLabel = new List<string>
        {
            "無し",
            "ダガー",
            "剣",
            "フレイル",
            "斧",
            "ウィップ",
            "杖",
            "弓",
            "クロスボウ",
            "銃",
            "爪",
            "グローブ",
            "槍",
            "メイス",
            "ロッド",
            "こん棒",
            "チェーン",
            "未来の剣",
            "パイプ",
            "ショットガン",
            "ライフル",
            "チェーンソー",
            "レールガン",
            "スタンロッド",
            "ユーザ定義1",
            "ユーザ定義2",
            "ユーザ定義3",
            "ユーザ定義4",
            "ユーザ定義5",
            "ユーザ定義6"
        };

        public List<string> AttackMotionLabel = new List<string>
        {
            "WORD_1289",
            "WORD_1290",
            "WORD_1291"
        };
    }
}