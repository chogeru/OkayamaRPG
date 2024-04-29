using System;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.WordDefinition
{
    [Serializable]
    public class WordDefinitionDataModel
    {
        public BasicStatus basicStatus;
        public Commands    commands;
        public Messages    messages;
        public Status      status;

        public bool isEqual(WordDefinitionDataModel data) {
            return basicStatus.isEqual(data.basicStatus) &&
                   commands.isEqual(data.commands) &&
                   messages.isEqual(data.messages) &&
                   status.isEqual(data.status);
        }

        [Serializable]
        public class WordItem
        {
            public int    enabled;
            public string initialValue;
            public string value;

            public WordItem(int enabled, string initialValue, string value) {
                this.enabled = enabled;
                this.initialValue = initialValue;
                this.value = value;
            }

		    public bool isEqual(WordItem data)
		    {
		        return enabled == data.enabled &&
		               initialValue == data.initialValue &&
		               value == data.value;
		    }
        }

        [Serializable]
        public class BasicStatus
        {
            public WordItem exp;
            public WordItem expShort;
            public WordItem hp;
            public WordItem hpShort;
            public WordItem level;
            public WordItem levelShort;
            public WordItem money;
            public WordItem mp;
            public WordItem mpShort;
            public WordItem tp;
            public WordItem tpShort;

            public BasicStatus(
                WordItem level,
                WordItem levelShort,
                WordItem hp,
                WordItem hpShort,
                WordItem mp,
                WordItem mpShort,
                WordItem tp,
                WordItem tpShort,
                WordItem exp,
                WordItem expShort,
                WordItem money
            ) {
                this.level = level;
                this.levelShort = levelShort;
                this.hp = hp;
                this.hpShort = hpShort;
                this.mp = mp;
                this.mpShort = mpShort;
                this.tp = tp;
                this.tpShort = tpShort;
                this.exp = exp;
                this.expShort = expShort;
                this.money = money;
            }

            public WordItem Level => level;
            public WordItem LevelShort => levelShort;
            public WordItem Hp => hp;
            public WordItem HpShort => hpShort;
            public WordItem Mp => mp;
            public WordItem MpShort => mpShort;
            public WordItem Tp => tp;
            public WordItem TpShort => tpShort;
            public WordItem Exp => exp;
            public WordItem ExpShort => expShort;
            public WordItem Money => money;

            public WordItem GetData(string name) {
                var property = typeof(BasicStatus).GetProperty(name);
                var value = property.GetValue(this) as WordItem;
                return value;
            }

		    public bool isEqual(BasicStatus data)
		    {
		        return exp.isEqual(data.exp) &&
		               expShort.isEqual(data.expShort) &&
		               hp.isEqual(data.hp) &&
		               hpShort.isEqual(data.hpShort) &&
		               level.isEqual(data.level) &&
		               levelShort.isEqual(data.levelShort) &&
		               money.isEqual(data.money) &&
		               mp.isEqual(data.mp) &&
		               mpShort.isEqual(data.mpShort) &&
		               tp.isEqual(data.tp) &&
		               tpShort.isEqual(data.tpShort);
		    }
        }

        [Serializable]
        public class Commands
        {
            public WordItem alwaysDash;
            public WordItem armor;
            public WordItem attack;
            public WordItem backTitle;
            public WordItem battle;
            public WordItem buy;
            public WordItem equipment;
            public WordItem equipment2;
            public WordItem escape;
            public WordItem gameEnd;
            public WordItem guard;
            public WordItem item;
            public WordItem keyItem;
            public WordItem menuContinue;
            public WordItem newGame;
            public WordItem option;
            public WordItem pause;
            public WordItem possessionNum;
            public WordItem removeAll;
            public WordItem save;
            public WordItem saveCommand;
            public WordItem sell;
            public WordItem skill;
            public WordItem sort;
            public WordItem status;
            public WordItem strongestEquipment;
            public WordItem volumeBgm;
            public WordItem volumeBgs;
            public WordItem volumeMe;
            public WordItem volumeSe;
            public WordItem weapon;
            

            public Commands(
                WordItem battle,
                WordItem escape,
                WordItem attack,
                WordItem guard,
                WordItem item,
                WordItem skill,
                WordItem equipment,
                WordItem status,
                WordItem sort,
                WordItem option,
                WordItem save,
                WordItem gameEnd,
                WordItem weapon,
                WordItem armor,
                WordItem keyItem,
                WordItem equipment2,
                WordItem strongestEquipment,
                WordItem removeAll,
                WordItem buy,
                WordItem sell,
                WordItem newGame,
                WordItem menuContinue,
                WordItem backTitle,
                WordItem pause,
                WordItem alwaysDash,
                WordItem saveCommand,
                WordItem volumeBgm,
                WordItem volumeBgs,
                WordItem volumeMe,
                WordItem volumeSe,
                WordItem possessionNum
            ) {
                this.battle = battle;
                this.escape = escape;
                this.attack = attack;
                this.guard = guard;
                this.item = item;
                this.skill = skill;
                this.equipment = equipment;
                this.status = status;
                this.sort = sort;
                this.option = option;
                this.save = save;
                this.gameEnd = gameEnd;
                this.weapon = weapon;
                this.armor = armor;
                this.keyItem = keyItem;
                this.equipment2 = equipment2;
                this.strongestEquipment = strongestEquipment;
                this.removeAll = removeAll;
                this.buy = buy;
                this.sell = sell;
                this.newGame = newGame;
                this.menuContinue = menuContinue;
                this.backTitle = backTitle;
                this.pause = pause;
                this.alwaysDash = alwaysDash;
                this.saveCommand = saveCommand;
                this.volumeBgm = volumeBgm;
                this.volumeBgs = volumeBgs;
                this.volumeMe = volumeMe;
                this.volumeSe = volumeSe;
                this.possessionNum = possessionNum;
            }

            public WordItem Battle => battle;
            public WordItem Escape => escape;
            public WordItem Attack => attack;
            public WordItem Guard => guard;
            public WordItem Item => item;
            public WordItem Skill => skill;
            public WordItem Equipment => equipment;
            public WordItem Status => status;
            public WordItem Sort => sort;
            public WordItem Option => option;
            public WordItem Save => save;
            public WordItem GameEnd => gameEnd;
            public WordItem Weapon => weapon;
            public WordItem Armor => armor;
            public WordItem KeyItem => keyItem;
            public WordItem Equipment2 => equipment2;
            public WordItem StrongestEquipment => strongestEquipment;
            public WordItem RemoveAll => removeAll;
            public WordItem Buy => buy;
            public WordItem Sell => sell;
            public WordItem NewGame => newGame;
            public WordItem MenuContinue => menuContinue;
            public WordItem BackTitle => backTitle;
            public WordItem Pause => pause;
            public WordItem AlwaysDash => alwaysDash;
            public WordItem SaveCommand => saveCommand;
            public WordItem VolumeBgm => volumeBgm;
            public WordItem VolumeBgs => volumeBgs;
            public WordItem VolumeMe => volumeMe;
            public WordItem VolumeSe => volumeSe;
            public WordItem PosessionNum => possessionNum;
            
            public WordItem GetData(string name) {
                var property = typeof(Commands).GetProperty(name);
                var value = property.GetValue(this) as WordItem;
                return value;
            }

		    public bool isEqual(Commands data)
		    {
		        return alwaysDash.isEqual(data.alwaysDash) &&
		               armor.isEqual(data.armor) &&
		               attack.isEqual(data.attack) &&
		               backTitle.isEqual(data.backTitle) &&
		               battle.isEqual(data.battle) &&
		               buy.isEqual(data.buy) &&
		               equipment.isEqual(data.equipment) &&
		               equipment2.isEqual(data.equipment2) &&
		               escape.isEqual(data.escape) &&
		               gameEnd.isEqual(data.gameEnd) &&
		               guard.isEqual(data.guard) &&
		               item.isEqual(data.item) &&
		               keyItem.isEqual(data.keyItem) &&
		               menuContinue.isEqual(data.menuContinue) &&
		               newGame.isEqual(data.newGame) &&
		               option.isEqual(data.option) &&
		               pause.isEqual(data.pause) &&
		               possessionNum.isEqual(data.possessionNum) &&
		               removeAll.isEqual(data.removeAll) &&
		               save.isEqual(data.save) &&
		               saveCommand.isEqual(data.saveCommand) &&
		               sell.isEqual(data.sell) &&
		               skill.isEqual(data.skill) &&
		               sort.isEqual(data.sort) &&
		               status.isEqual(data.status) &&
		               strongestEquipment.isEqual(data.strongestEquipment) &&
		               volumeBgm.isEqual(data.volumeBgm) &&
		               volumeBgs.isEqual(data.volumeBgs) &&
		               volumeMe.isEqual(data.volumeMe) &&
		               volumeSe.isEqual(data.volumeSe) &&
		               weapon.isEqual(data.weapon);
		    }
        }

        [Serializable]
        public class Status
        {
            public WordItem attack;
            public WordItem evasion;
            public WordItem guard;
            public WordItem hit;
            public WordItem luck;
            public WordItem magic;
            public WordItem magicGuard;
            public WordItem maxHp;
            public WordItem maxMp;
            public WordItem speed;

            public Status(
                WordItem maxHp,
                WordItem maxMp,
                WordItem attack,
                WordItem guard,
                WordItem magic,
                WordItem magicGuard,
                WordItem speed,
                WordItem luck,
                WordItem hit,
                WordItem evasion
            ) {
                this.maxHp = maxHp;
                this.maxMp = maxMp;
                this.attack = attack;
                this.guard = guard;
                this.magic = magic;
                this.magicGuard = magicGuard;
                this.speed = speed;
                this.luck = luck;
                this.hit = hit;
                this.evasion = evasion;
            }

            public WordItem MaxHp => maxHp;
            public WordItem MaxMp => maxMp;
            public WordItem Attack => attack;
            public WordItem Guard => guard;
            public WordItem Magic => magic;
            public WordItem MagicGuard => magicGuard;
            public WordItem Speed => speed;
            public WordItem Luck => luck;
            public WordItem Hit => hit;
            public WordItem Evasion => evasion;

            public WordItem GetData(string name) {
                var property = typeof(Status).GetProperty(name);
                var value = property.GetValue(this) as WordItem;
                return value;
            }

            public bool isEqual(Status data) {
                return attack.isEqual(data.attack) &&
                       evasion.isEqual(data.evasion) &&
                       guard.isEqual(data.guard) &&
                       hit.isEqual(data.hit) &&
                       luck.isEqual(data.luck) &&
                       magic.isEqual(data.magic) &&
                       magicGuard.isEqual(data.magicGuard) &&
                       maxHp.isEqual(data.maxHp) &&
                       maxMp.isEqual(data.maxMp) &&
                       speed.isEqual(data.speed);
            }
        }

        [Serializable]
        public class Messages
        {
            public WordItem actionFailure;
            public WordItem actorDamage;
            public WordItem actorDrain;
            public WordItem actorGain;
            public WordItem actorLoss;
            public WordItem actorNoDamage;
            public WordItem actorNoHit;
            public WordItem actorRecovery;
            public WordItem buffAdd;
            public WordItem buffRemove;
            public WordItem counterAttack;
            public WordItem criticalToActor;
            public WordItem criticalToEnemy;
            public WordItem debuffAdd;
            public WordItem defeat;
            public WordItem emerge;
            public WordItem enemyDamage;
            public WordItem enemyDrain;
            public WordItem enemyGain;
            public WordItem enemyLoss;
            public WordItem enemyNoDamage;
            public WordItem enemyNoHit;
            public WordItem enemyRecovery;
            public WordItem escapeFailure;
            public WordItem escapeStart;
            public WordItem evasion;
            public WordItem expNext;
            public WordItem expTotal;
            public WordItem file;
            public WordItem levelUp;
            public WordItem loadMessage;
            public WordItem magicEvasion;
            public WordItem magicReflection;
            public WordItem obtainExp;
            public WordItem obtainGold;
            public WordItem obtainItem;
            public WordItem obtainSkill;
            public WordItem partyName;
            public WordItem preemptive;
            public WordItem saveMessage;
            public WordItem substitute;
            public WordItem surprise;
            public WordItem useItem;
            public WordItem victory;

            public Messages(
                WordItem expTotal,
                WordItem expNext,
                WordItem saveMessage,
                WordItem loadMessage,
                WordItem file,
                WordItem partyName,
                WordItem emerge,
                WordItem preemptive,
                WordItem surprise,
                WordItem escapeStart,
                WordItem escapeFailure,
                WordItem victory,
                WordItem defeat,
                WordItem obtainExp,
                WordItem obtainGold,
                WordItem obtainItem,
                WordItem levelUp,
                WordItem obtainSkill,
                WordItem useItem,
                WordItem criticalToEnemy,
                WordItem criticalToActor,
                WordItem actorDamage,
                WordItem actorRecovery,
                WordItem actorGain,
                WordItem actorLoss,
                WordItem actorDrain,
                WordItem actorNoDamage,
                WordItem actorNoHit,
                WordItem enemyDamage,
                WordItem enemyRecovery,
                WordItem enemyGain,
                WordItem enemyLoss,
                WordItem enemyDrain,
                WordItem enemyNoDamage,
                WordItem enemyNoHit,
                WordItem evasion,
                WordItem magicEvasion,
                WordItem magicReflection,
                WordItem counterAttack,
                WordItem substitute,
                WordItem buffAdd,
                WordItem debuffAdd,
                WordItem buffRemove,
                WordItem actionFailure
            ) {
                this.expTotal = expTotal;
                this.expNext = expNext;
                this.saveMessage = saveMessage;
                this.loadMessage = loadMessage;
                this.file = file;
                this.partyName = partyName;
                this.emerge = emerge;
                this.preemptive = preemptive;
                this.surprise = surprise;
                this.escapeStart = escapeStart;
                this.escapeFailure = escapeFailure;
                this.victory = victory;
                this.defeat = defeat;
                this.obtainExp = obtainExp;
                this.obtainGold = obtainGold;
                this.obtainItem = obtainItem;
                this.levelUp = levelUp;
                this.obtainSkill = obtainSkill;
                this.useItem = useItem;
                this.criticalToEnemy = criticalToEnemy;
                this.criticalToActor = criticalToActor;
                this.actorDamage = actorDamage;
                this.actorRecovery = actorRecovery;
                this.actorGain = actorGain;
                this.actorLoss = actorLoss;
                this.actorDrain = actorDrain;
                this.actorNoDamage = actorNoDamage;
                this.actorNoHit = actorNoHit;
                this.enemyDamage = enemyDamage;
                this.enemyRecovery = enemyRecovery;
                this.enemyGain = enemyGain;
                this.enemyLoss = enemyLoss;
                this.enemyDrain = enemyDrain;
                this.enemyNoDamage = enemyNoDamage;
                this.enemyNoHit = enemyNoHit;
                this.evasion = evasion;
                this.magicEvasion = magicEvasion;
                this.magicReflection = magicReflection;
                this.counterAttack = counterAttack;
                this.substitute = substitute;
                this.buffAdd = buffAdd;
                this.debuffAdd = debuffAdd;
                this.buffRemove = buffRemove;
                this.actionFailure = actionFailure;
            }

            public WordItem ExpTotal => expTotal;
            public WordItem ExpNext => expNext;
            public WordItem SaveMessage => saveMessage;
            public WordItem LoadMessage => loadMessage;
            public WordItem File => file;
            public WordItem PartyName => partyName;
            public WordItem Emerge => emerge;
            public WordItem Preemptive => preemptive;
            public WordItem Surprise => surprise;
            public WordItem EscapeStart => escapeStart;
            public WordItem EscapeFailure => escapeFailure;
            public WordItem Victory => victory;
            public WordItem Defeat => defeat;
            public WordItem ObtainExp => obtainExp;
            public WordItem ObtainGold => obtainGold;
            public WordItem ObtainItem => obtainItem;
            public WordItem LevelUp => levelUp;
            public WordItem ObtainSkill => obtainSkill;
            public WordItem UseItem => useItem;
            public WordItem CriticalToEnemy => criticalToEnemy;
            public WordItem CriticalToActor => criticalToActor;
            public WordItem ActorDamage => actorDamage;
            public WordItem ActorRecovery => actorRecovery;
            public WordItem ActorGain => actorGain;
            public WordItem ActorLoss => actorLoss;
            public WordItem ActorDrain => actorDrain;
            public WordItem ActorNoDamage => actorNoDamage;
            public WordItem ActorNoHit => actorNoHit;
            public WordItem EnemyDamage => enemyDamage;
            public WordItem EnemyRecovery => enemyRecovery;
            public WordItem EnemyGain => enemyGain;
            public WordItem EnemyLoss => enemyLoss;
            public WordItem EnemyDrain => enemyDrain;
            public WordItem EnemyNoDamage => enemyNoDamage;
            public WordItem EnemyNoHit => enemyNoHit;
            public WordItem Evasion => evasion;
            public WordItem MagicEvasion => magicEvasion;
            public WordItem MagicReflection => magicReflection;
            public WordItem CounterAttack => counterAttack;
            public WordItem Substitute => substitute;
            public WordItem BuffAdd => buffAdd;
            public WordItem DebuffAdd => debuffAdd;
            public WordItem BuffRemove => buffRemove;
            public WordItem ActionFailure => actionFailure;

            public WordItem GetData(string name) {
                var property = typeof(Messages).GetProperty(name);
                var value = property.GetValue(this) as WordItem;
                return value;
            }

            public bool isEqual(Messages data) {
                return actionFailure.isEqual(data.actionFailure) &&
                       actorDamage.isEqual(data.actorDamage) &&
                       actorDrain.isEqual(data.actorDrain) &&
                       actorGain.isEqual(data.actorGain) &&
                       actorLoss.isEqual(data.actorLoss) &&
                       actorNoDamage.isEqual(data.actorNoDamage) &&
                       actorNoHit.isEqual(data.actorNoHit) &&
                       actorRecovery.isEqual(data.actorRecovery) &&
                       buffAdd.isEqual(data.buffAdd) &&
                       buffRemove.isEqual(data.buffRemove) &&
                       counterAttack.isEqual(data.counterAttack) &&
                       criticalToActor.isEqual(data.criticalToActor) &&
                       criticalToEnemy.isEqual(data.criticalToEnemy) &&
                       debuffAdd.isEqual(data.debuffAdd) &&
                       defeat.isEqual(data.defeat) &&
                       emerge.isEqual(data.emerge) &&
                       enemyDamage.isEqual(data.enemyDamage) &&
                       enemyDrain.isEqual(data.enemyDrain) &&
                       enemyGain.isEqual(data.enemyGain) &&
                       enemyLoss.isEqual(data.enemyLoss) &&
                       enemyNoDamage.isEqual(data.enemyNoDamage) &&
                       enemyNoHit.isEqual(data.enemyNoHit) &&
                       enemyRecovery.isEqual(data.enemyRecovery) &&
                       escapeFailure.isEqual(data.escapeFailure) &&
                       escapeStart.isEqual(data.escapeStart) &&
                       evasion.isEqual(data.evasion) &&
                       expNext.isEqual(data.expNext) &&
                       expTotal.isEqual(data.expTotal) &&
                       file.isEqual(data.file) &&
                       levelUp.isEqual(data.levelUp) &&
                       loadMessage.isEqual(data.loadMessage) &&
                       magicEvasion.isEqual(data.magicEvasion) &&
                       magicReflection.isEqual(data.magicReflection) &&
                       obtainExp.isEqual(data.obtainExp) &&
                       obtainGold.isEqual(data.obtainGold) &&
                       obtainItem.isEqual(data.obtainItem) &&
                       obtainSkill.isEqual(data.obtainSkill) &&
                       partyName.isEqual(data.partyName) &&
                       preemptive.isEqual(data.preemptive) &&
                       saveMessage.isEqual(data.saveMessage) &&
                       substitute.isEqual(data.substitute) &&
                       surprise.isEqual(data.surprise) &&
                       useItem.isEqual(data.useItem) &&
                       victory.isEqual(data.victory);
            }
        }
    }
}