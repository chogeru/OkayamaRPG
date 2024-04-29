using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting
{
    [Serializable]
    public class SystemSettingDataModel
    {
        public List<ArmorType> armorTypes;
        public BattleScene     battleScene;
        public BGM             bgm;
        public int             displaySize;

        public List<Vector2Int> DisplaySize = new List<Vector2Int>
        {
            new Vector2Int(1920, 1080),
            new Vector2Int(1280, 720)
        };

        public List<Element>   elements;
        public List<EquipType> equipTypes;

        public InitialParty     initialParty;
        public int              isController;
        public OptionSetting    optionSetting;
        public List<SkillType>  skillTypes;
        public SoundSetting     soundSetting;
        public string           uiPatternId;
        public List<WeaponType> weaponTypes;

        public Vector2Int GetDisplaySize() {
            return DisplaySize[displaySize];
        }

        public bool isEqual(SystemSettingDataModel data) {
            if (armorTypes.Count != data.armorTypes.Count ||
                skillTypes.Count != data.skillTypes.Count ||
                elements.Count != data.elements.Count ||
                equipTypes.Count != data.equipTypes.Count ||
                weaponTypes.Count != data.weaponTypes.Count)
                return false;

            for (int i = 0; i < armorTypes.Count; i++)
                if (!armorTypes[i].isEqual(data.armorTypes[i]))
                    return false;

            if (!battleScene.isEqual(data.battleScene) ||
                !bgm.isEqual(data.bgm) ||
                displaySize != data.displaySize ||
                !initialParty.isEqual(data.initialParty) ||
                isController != data.isController ||
                !optionSetting.isEqual(data.optionSetting) ||
                !soundSetting.isEqual(data.soundSetting) ||
                uiPatternId != data.uiPatternId)
                return false;

            for (int i = 0; i < elements.Count; i++)
                if (!elements[i].isEqual(data.elements[i]))
                    return false;

            for (int i = 0; i < equipTypes.Count; i++)
                if (!equipTypes[i].isEqual(data.equipTypes[i]))
                    return false;

            for (int i = 0; i < skillTypes.Count; i++)
                if (!skillTypes[i].isEqual(data.skillTypes[i]))
                    return false;

            for (int i = 0; i < weaponTypes.Count; i++)
                if (!weaponTypes[i].isEqual(data.weaponTypes[i]))
                    return false;

            return true;
        }

        [Serializable]
        public class InitialParty
        {
            public List<string> party;
            public int          partyMax;
            public StartMap     startMap;

            public InitialParty(int partyMax, List<string> party, StartMap startMap) {
                this.partyMax = partyMax;
                this.party = party;
                this.startMap = startMap;
            }

            public bool isEqual(InitialParty data) {
                if (partyMax != data.partyMax ||
                    party.Count != data.party.Count ||
                    !startMap.isEqual(data.startMap))
                    return false;

                for (int i = 0; i < party.Count; i++)
                    if (party[i] != data.party[i])
                        return false;

                return true;
            }
        }

        [Serializable]
        public class WeaponType : WithSerialNumberDataModel
        {
            public int    delete;
            public string id;
            public string image;
            public int    motionId;
            public string value;

            public WeaponType(string id, string value, int motionId, string image, int delete) {
                this.id = id;
                this.value = value;
                this.motionId = motionId;
                this.image = image;
                this.delete = delete;
            }

            public static WeaponType CreateDefault() {
                return new WeaponType(Guid.NewGuid().ToString(), "", 0, "", 0);
            }

            public bool isEqual(WeaponType data) {
                return delete == data.delete &&
                       id == data.id &&
                       image == data.image &&
                       motionId == data.motionId &&
                       value == data.value;
            }
        }

        [Serializable]
        public class ArmorType : WithSerialNumberDataModel
        {
            public int    delete;
            public string id;
            public string name;

            public ArmorType(string id, string name, int delete) {
                this.id = id;
                this.name = name;
                this.delete = delete;
            }

            public static ArmorType CreateDefault() {
                return new ArmorType(Guid.NewGuid().ToString(), "", 0);
            }

            public bool isEqual(ArmorType data) {
                return delete == data.delete &&
                       id == data.id &&
                       name == data.name;
            }
        }

        [Serializable]
        public class EquipType : WithSerialNumberDataModel
        {
            public int    delete;
            public string id;
            public string name;

            public EquipType(string id, string name, int delete) {
                this.id = id;
                this.name = name;
                this.delete = delete;
            }

            public static EquipType CreateDefault() {
                return new EquipType(Guid.NewGuid().ToString(), "", 0);
            }

            public bool isEqual(EquipType data) {
                return delete == data.delete &&
                       id == data.id &&
                       name == data.name;
            }
        }

        [Serializable]
        public class Element : WithSerialNumberDataModel
        {
            public List<Advantage>    advantageous;
            public int                delete;
            public List<Disadvantage> disadvantage;
            public string             icon;
            public string             id;
            public int                sameElement;
            public string             value;

            public Element(
                string id,
                string value,
                string icon,
                int delete,
                int sameElement,
                List<Advantage> advantageous,
                List<Disadvantage> disadvantage
            ) {
                this.id = id;
                this.value = value;
                this.icon = icon;
                this.delete = delete;
                this.sameElement = sameElement;
                this.advantageous = advantageous;
                this.disadvantage = disadvantage;
            }

            public static Element CreateDefault() {
                return new Element(Guid.NewGuid().ToString(), "", "", 0, 0, new List<Advantage>(),
                    new List<Disadvantage>());
            }

            public bool isEqual(Element data) {
                if (delete != data.delete ||
                    icon != data.icon ||
                    id != data.id ||
                    sameElement != data.sameElement ||
                    value != data.value ||
                    advantageous.Count != data.advantageous.Count ||
                    disadvantage.Count != data.disadvantage.Count)
                    return false;

                for (int i = 0; i < advantageous.Count; i++)
                    if (!advantageous[i].isEqual(data.advantageous[i]))
                        return false;

                for (int i = 0; i < disadvantage.Count; i++)
                    if (!disadvantage[i].isEqual(data.disadvantage[i]))
                        return false;

                return true;
            }
        }

        [Serializable]
        public class SkillType : WithSerialNumberDataModel
        {
            public int    delete;
            public string id;
            public int    motion;
            public string value;

            public SkillType(string id, string value, int motion, int delete) {
                this.id = id;
                this.value = value;
                this.motion = motion;
                this.delete = delete;
            }

            public static SkillType CreateDefault() {
                return new SkillType(Guid.NewGuid().ToString(), "", 0, 0);
            }

            public bool isEqual(SkillType data) {
                return delete == data.delete &&
                       id == data.id &&
                       motion == data.motion &&
                       value == data.value;
            }
        }

        [Serializable]
        public class BGM
        {
            public SoundCommonDataModel battleBgm;
            public SoundCommonDataModel defeatMe;
            public SoundCommonDataModel gameOverMe;
            public SoundCommonDataModel title;
            public SoundCommonDataModel victoryMe;

            public BGM(
                SoundCommonDataModel title,
                SoundCommonDataModel defeatMe,
                SoundCommonDataModel gameOverMe,
                SoundCommonDataModel battleBgm,
                SoundCommonDataModel victoryMe
            ) {
                this.title = title;
                this.defeatMe = defeatMe;
                this.gameOverMe = gameOverMe;
                this.battleBgm = battleBgm;
                this.victoryMe = victoryMe;
            }

            public bool isEqual(BGM data) {
                return battleBgm.isEqual(data.battleBgm) &&
                       defeatMe.isEqual(data.defeatMe) &&
                       gameOverMe.isEqual(data.gameOverMe) &&
                       title.isEqual(data.title) &&
                       victoryMe.isEqual(data.victoryMe);
            }
        }

        [Serializable]
        public class SoundSetting
        {
            public SoundCommonDataModel actorDamage;
            public SoundCommonDataModel actorDied;
            public SoundCommonDataModel battleStart;
            public SoundCommonDataModel bossCollapse1;
            public SoundCommonDataModel bossCollapse2;
            public SoundCommonDataModel buzzer;
            public SoundCommonDataModel cancel;
            public SoundCommonDataModel cursor;
            public SoundCommonDataModel enemyAttack;
            public SoundCommonDataModel enemyCollapse;
            public SoundCommonDataModel enemyDamage;
            public SoundCommonDataModel equip;
            public SoundCommonDataModel escape;
            public SoundCommonDataModel evasion;
            public SoundCommonDataModel load;
            public SoundCommonDataModel magicEvasion;
            public SoundCommonDataModel magicReflection;
            public SoundCommonDataModel miss;
            public SoundCommonDataModel ok;
            public SoundCommonDataModel recovery;
            public SoundCommonDataModel save;
            public SoundCommonDataModel shop;
            public SoundCommonDataModel useItem;
            public SoundCommonDataModel useSkill;

            public SoundSetting(
                SoundCommonDataModel cursor,
                SoundCommonDataModel ok,
                SoundCommonDataModel cancel,
                SoundCommonDataModel buzzer,
                SoundCommonDataModel equip,
                SoundCommonDataModel save,
                SoundCommonDataModel load,
                SoundCommonDataModel battleStart,
                SoundCommonDataModel escape,
                SoundCommonDataModel enemyAttack,
                SoundCommonDataModel enemyDamage,
                SoundCommonDataModel enemyCollapse,
                SoundCommonDataModel bossCollapse1,
                SoundCommonDataModel bossCollapse2,
                SoundCommonDataModel actorDamage,
                SoundCommonDataModel actorDied,
                SoundCommonDataModel recovery,
                SoundCommonDataModel miss,
                SoundCommonDataModel evasion,
                SoundCommonDataModel magicEvasion,
                SoundCommonDataModel magicReflection,
                SoundCommonDataModel shop,
                SoundCommonDataModel useItem,
                SoundCommonDataModel useSkill
            ) {
                this.cursor = cursor;
                this.ok = ok;
                this.cancel = cancel;
                this.buzzer = buzzer;
                this.equip = equip;
                this.save = save;
                this.load = load;
                this.battleStart = battleStart;
                this.escape = escape;
                this.enemyAttack = enemyAttack;
                this.enemyDamage = enemyDamage;
                this.enemyCollapse = enemyCollapse;
                this.bossCollapse1 = bossCollapse1;
                this.bossCollapse2 = bossCollapse2;
                this.actorDamage = actorDamage;
                this.actorDied = actorDied;
                this.recovery = recovery;
                this.miss = miss;
                this.evasion = evasion;
                this.magicEvasion = magicEvasion;
                this.magicReflection = magicReflection;
                this.shop = shop;
                this.useItem = useItem;
                this.useSkill = useSkill;
            }

            public static SoundSetting CreateDefault() {
                return new SoundSetting(
                    SoundCommonDataModel.CreateDefault(),
                    SoundCommonDataModel.CreateDefault(),
                    SoundCommonDataModel.CreateDefault(),
                    SoundCommonDataModel.CreateDefault(),
                    SoundCommonDataModel.CreateDefault(),
                    SoundCommonDataModel.CreateDefault(),
                    SoundCommonDataModel.CreateDefault(),
                    SoundCommonDataModel.CreateDefault(),
                    SoundCommonDataModel.CreateDefault(),
                    SoundCommonDataModel.CreateDefault(),
                    SoundCommonDataModel.CreateDefault(),
                    SoundCommonDataModel.CreateDefault(),
                    SoundCommonDataModel.CreateDefault(),
                    SoundCommonDataModel.CreateDefault(),
                    SoundCommonDataModel.CreateDefault(),
                    SoundCommonDataModel.CreateDefault(),
                    SoundCommonDataModel.CreateDefault(),
                    SoundCommonDataModel.CreateDefault(),
                    SoundCommonDataModel.CreateDefault(),
                    SoundCommonDataModel.CreateDefault(),
                    SoundCommonDataModel.CreateDefault(),
                    SoundCommonDataModel.CreateDefault(),
                    SoundCommonDataModel.CreateDefault(),
                    SoundCommonDataModel.CreateDefault()
                );
            }

            public SoundCommonDataModel GetData(string name) {
                return typeof(SoundSetting).GetProperty(name)?.GetValue(this) as SoundCommonDataModel;
            }

            public bool isEqual(SoundSetting data) {
                return actorDamage.isEqual(data.actorDamage) &&
                       actorDied.isEqual(data.actorDied) &&
                       battleStart.isEqual(data.battleStart) &&
                       bossCollapse1.isEqual(data.bossCollapse1) &&
                       bossCollapse2.isEqual(data.bossCollapse2) &&
                       buzzer.isEqual(data.buzzer) &&
                       cancel.isEqual(data.cancel) &&
                       cursor.isEqual(data.cursor) &&
                       enemyAttack.isEqual(data.enemyAttack) &&
                       enemyCollapse.isEqual(data.enemyCollapse) &&
                       enemyDamage.isEqual(data.enemyDamage) &&
                       equip.isEqual(data.equip) &&
                       escape.isEqual(data.escape) &&
                       evasion.isEqual(data.evasion) &&
                       load.isEqual(data.load) &&
                       magicEvasion.isEqual(data.magicEvasion) &&
                       magicReflection.isEqual(data.magicReflection) &&
                       miss.isEqual(data.miss) &&
                       ok.isEqual(data.ok) &&
                       recovery.isEqual(data.recovery) &&
                       save.isEqual(data.save) &&
                       shop.isEqual(data.shop) &&
                       useItem.isEqual(data.useItem) &&
                       useSkill.isEqual(data.useSkill);
            }
        }

        [Serializable]
        public class OptionSetting
        {
            public int       enabledAutoSave;
            public string    locale;
            public int       optDisplayTp;
            public int       optExtraExp;
            public int       optFloorDeath;
            public int       optFollowers;
            public int       optSlipDeath;
            public int       optTransparent;
            public int       showKeyItemNum;
            public List<int> windowTone;

            public OptionSetting(
                int optDisplayTp,
                int optExtraExp,
                int optFloorDeath,
                int optFollowers,
                int optSlipDeath,
                int optTransparent,
                int showKeyItemNum,
                int enabledAutoSave,
                string locale,
                List<int> windowTone
            ) {
                this.optDisplayTp = optDisplayTp;
                this.optExtraExp = optExtraExp;
                this.optFloorDeath = optFloorDeath;
                this.optFollowers = optFollowers;
                this.optSlipDeath = optSlipDeath;
                this.optTransparent = optTransparent;
                this.showKeyItemNum = showKeyItemNum;
                this.enabledAutoSave = enabledAutoSave;
                this.locale = locale;
                this.windowTone = windowTone;
            }

            public bool isEqual(OptionSetting data) {
                if (enabledAutoSave != data.enabledAutoSave ||
                    locale != data.locale ||
                    optDisplayTp != data.optDisplayTp ||
                    optExtraExp != data.optExtraExp ||
                    optFloorDeath != data.optFloorDeath ||
                    optFollowers != data.optFollowers ||
                    optSlipDeath != data.optSlipDeath ||
                    optTransparent != data.optTransparent ||
                    showKeyItemNum != data.showKeyItemNum ||
                    windowTone.Count != data.windowTone.Count)
                    return false;

                for (int i = 0; i < windowTone.Count; i++)
                    if (windowTone[i] != data.windowTone[i])
                        return false;

                return true;
            }
        }

        [Serializable]
        public class BattleScene
        {
            public int       frontEnemyPositionY;
            public int       frontMiddleStartFlag;
            public int       sideActorSpace;
            public int       sideEnemyInclined;
            public int       sidePartyInclined;
            public List<int> sidePartyPosition;
            public int       viewType;

            public BattleScene(
                int viewType,
                int frontMiddleStartFlag,
                int frontEnemyPositionY,
                List<int> sidePartyPosition,
                int sidePartyInclined,
                int sideActorSpace,
                int sideEnemyInclined
            ) {
                this.viewType = viewType;
                this.frontMiddleStartFlag = frontMiddleStartFlag;
                this.frontEnemyPositionY = frontEnemyPositionY;
                this.sidePartyPosition = sidePartyPosition;
                this.sidePartyInclined = sidePartyInclined;
                this.sideActorSpace = sideActorSpace;
                this.sideEnemyInclined = sideEnemyInclined;
            }

            public bool isEqual(BattleScene data) {
                if (frontEnemyPositionY != data.frontEnemyPositionY ||
                    frontMiddleStartFlag != data.frontMiddleStartFlag ||
                    sideActorSpace != data.sideActorSpace ||
                    sideEnemyInclined != data.sideEnemyInclined ||
                    sidePartyInclined != data.sidePartyInclined ||
                    viewType != data.viewType ||
                    sidePartyPosition.Count != data.sidePartyPosition.Count)
                    return false;
                    
                for (int i = 0; i < sidePartyPosition.Count; i++)
                    if (sidePartyPosition[i] != data.sidePartyPosition[i])
                        return false;

                return true;
            }
        }

        [Serializable]
        public class Advantage
        {
            public int element;
            public int magnification;

            public Advantage(int element, int magnification) {
                this.element = element;
                this.magnification = magnification;
            }

            public bool isEqual(Advantage data) {
                return element == data.element && 
                       magnification == data.magnification;
            }
        }

        [Serializable]
        public class Disadvantage
        {
            public int element;
            public int magnification;

            public Disadvantage(int element, int magnification) {
                this.element = element;
                this.magnification = magnification;
            }

            public bool isEqual(Disadvantage data) {
                return element == data.element &&
                       magnification == data.magnification;
            }
        }

        [Serializable]
        public class StartMap
        {
            public string    mapId;
            public List<int> position;

            public StartMap(string mapId, List<int> position) {
                this.mapId = mapId;
                this.position = position;
            }

            public bool isEqual(StartMap data) {
                if (mapId != data.mapId ||
                    position.Count != data.position.Count)
                    return false;

                for (int i = 0; i < position.Count; i++)
                    if (position[i] != data.position[i])
                        return false;

                return true;
            }
        }
    }
}