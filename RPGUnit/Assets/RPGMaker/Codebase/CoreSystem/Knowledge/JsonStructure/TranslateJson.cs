using System;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure
{
    [Serializable]
    public class CharacterJsonTranslation
    {
        public string uuId;
        public string name;
        public string secondName;
        public string profile;
        public string memo;
    }

    [Serializable]
    public class ClassJsonTranslation
    {
        public string id;
        public string name;
    }

    [Serializable]
    public class VehicleJsonTranslation
    {
        public string id;
        public string name;
    }

    [Serializable]
    public class SkillCustomJsonTranslation
    {
        public string id;
        public string name;
        public string description;
        public string message;
        public string memo;
    }

    [Serializable]
    public class EnemyJsonTranslation
    {
        public string id;
        public string name;
        public string memo;
    }

    [Serializable]
    public class TroopJsonTranslation
    {
        public string id;
        public string name;
    }

    [Serializable]
    public class WeaponJsonTranslation
    {
        public string id;
        public string name;
        public string description;
        public string memo;
    }

    [Serializable]
    public class ArmorJsonTranslation
    {
        public string id;
        public string name;
        public string description;
        public string memo;
    }

    [Serializable]
    public class ItemJsonTranslation
    {
        public string id;
        public string name;
        public string description;
        public string memo;
    }

    [Serializable]
    public class StateJsonTranslation
    {
        public string id;
        public string name;
        public string note;
        public string message1;
        public string message2;
        public string message3;
        public string message4;
    }

    [Serializable]
    public class AnimationJsonTranslation
    {
        public string id;
        public string particleName;
    }

    [Serializable]
    public class TileJsonTranslation
    {
        public string id;
        public string name;
    }
}