using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.Enum
{
    public class SkillEnums
    {
        public enum EffectTarget
        {
            NONE,
            ONE_ENEMY,
            ALL_ENEMIES,
            ONE_RANDOM_ENEMY,
            TWO_RANDOM_ENEMIES,
            THREE_RANDOM_ENEMIES,
            FOUR_RANDOM_ENEMIES,
            ONE_ALLY,
            ALL_ALLIES,
            ONE_ALLY_DEAD,
            ALL_ALLIES_DEAD,
            THE_USER
        }

        public enum ExecPlace
        {
            ALWAYS,
            BATTLE,
            MENU,
            NEVER
        }

        public List<string> EffectTargetListLabel = new List<string>
        {
            "None",
            "1 Enemy",
            "All Enemies",
            "1 Random Enemy",
            "2 Random Enemy",
            "3 Random Enemy",
            "4 Random Enemy",
            "1 Ally",
            "All Allies",
            "1 Ally(Dead)",
            "All Allies(Dead)",
            "The User"
        };

        public List<string> ExecPlaceListLabel = new List<string>
        {
            "Always",
            "Battle",
            "Menu",
            "Never"
        };
    }
}