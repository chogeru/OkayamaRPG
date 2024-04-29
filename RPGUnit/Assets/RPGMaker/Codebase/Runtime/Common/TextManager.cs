using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.WordDefinition;

namespace RPGMaker.Codebase.Runtime.Common
{
    public class TextManager
    {
        public static string Format(string baseString, params string[] args) {
            var ret = baseString;
            var number = 1;
            foreach (var arg in args)
            {
                ret = ret.Replace("%" + number, arg);
                number++;
            }

            return ret;
        }

        public static WordDefinitionDataModel WordDefinitionDataModel = DataManager.Self().GetWordDefinitionDataModel();

        public static string level =>
            (WordDefinitionDataModel.basicStatus.level.enabled == 1)
                ? WordDefinitionDataModel.basicStatus.level.value
                : WordDefinitionDataModel.basicStatus.level.initialValue;

        public static string levelA =>
            (WordDefinitionDataModel.basicStatus.levelShort.enabled == 1)
                ? WordDefinitionDataModel.basicStatus.levelShort.value
                : WordDefinitionDataModel.basicStatus.levelShort.initialValue;

        public static string hp =>
            (WordDefinitionDataModel.basicStatus.hp.enabled == 1)
                ? WordDefinitionDataModel.basicStatus.hp.value
                : WordDefinitionDataModel.basicStatus.hp.initialValue;

        public static string hpA =>
            (WordDefinitionDataModel.basicStatus.hpShort.enabled == 1)
                ? WordDefinitionDataModel.basicStatus.hpShort.value
                : WordDefinitionDataModel.basicStatus.hpShort.initialValue;

        public static string mp =>
            (WordDefinitionDataModel.basicStatus.mp.enabled == 1)
                ? WordDefinitionDataModel.basicStatus.mp.value
                : WordDefinitionDataModel.basicStatus.mp.initialValue;

        public static string mpA =>
            (WordDefinitionDataModel.basicStatus.mpShort.enabled == 1)
                ? WordDefinitionDataModel.basicStatus.mpShort.value
                : WordDefinitionDataModel.basicStatus.mpShort.initialValue;

        public static string tp =>
            (WordDefinitionDataModel.basicStatus.tp.enabled == 1)
                ? WordDefinitionDataModel.basicStatus.tp.value
                : WordDefinitionDataModel.basicStatus.tp.initialValue;

        public static string tpA =>
            (WordDefinitionDataModel.basicStatus.tpShort.enabled == 1)
                ? WordDefinitionDataModel.basicStatus.tpShort.value
                : WordDefinitionDataModel.basicStatus.tpShort.initialValue;

        public static string exp =>
            (WordDefinitionDataModel.basicStatus.exp.enabled == 1)
                ? WordDefinitionDataModel.basicStatus.exp.value
                : WordDefinitionDataModel.basicStatus.exp.initialValue;

        public static string expA =>
            (WordDefinitionDataModel.basicStatus.expShort.enabled == 1)
                ? WordDefinitionDataModel.basicStatus.expShort.value
                : WordDefinitionDataModel.basicStatus.expShort.initialValue;

        public static string money =>
            (WordDefinitionDataModel.basicStatus.money.enabled == 1)
                ? WordDefinitionDataModel.basicStatus.money.value
                : WordDefinitionDataModel.basicStatus.money.initialValue;

        public static string fight =>
            (WordDefinitionDataModel.commands.battle.enabled == 1)
                ? WordDefinitionDataModel.commands.battle.value
                : WordDefinitionDataModel.commands.battle.initialValue;

        public static string escape =>
            (WordDefinitionDataModel.commands.escape.enabled == 1)
                ? WordDefinitionDataModel.commands.escape.value
                : WordDefinitionDataModel.commands.escape.initialValue;

        public static string attack =>
            (WordDefinitionDataModel.commands.attack.enabled == 1)
                ? WordDefinitionDataModel.commands.attack.value
                : WordDefinitionDataModel.commands.attack.initialValue;

        public static string guard =>
            (WordDefinitionDataModel.commands.guard.enabled == 1)
                ? WordDefinitionDataModel.commands.guard.value
                : WordDefinitionDataModel.commands.guard.initialValue;

        public static string item =>
            (WordDefinitionDataModel.commands.item.enabled == 1)
                ? WordDefinitionDataModel.commands.item.value
                : WordDefinitionDataModel.commands.item.initialValue;

        public static string skill =>
            (WordDefinitionDataModel.commands.skill.enabled == 1)
                ? WordDefinitionDataModel.commands.skill.value
                : WordDefinitionDataModel.commands.skill.initialValue;

        public static string equip =>
            (WordDefinitionDataModel.commands.equipment.enabled == 1)
                ? WordDefinitionDataModel.commands.equipment.value
                : WordDefinitionDataModel.commands.equipment.initialValue;

        public static string status =>
            (WordDefinitionDataModel.commands.status.enabled == 1)
                ? WordDefinitionDataModel.commands.status.value
                : WordDefinitionDataModel.commands.status.initialValue;

        public static string formation =>
            (WordDefinitionDataModel.commands.sort.enabled == 1)
                ? WordDefinitionDataModel.commands.sort.value
                : WordDefinitionDataModel.commands.sort.initialValue; 

        public static string save =>
            (WordDefinitionDataModel.commands.save.enabled == 1)
                ? WordDefinitionDataModel.commands.save.value
                : WordDefinitionDataModel.commands.save.initialValue;

        public static string gameEnd =>
            (WordDefinitionDataModel.commands.gameEnd.enabled == 1)
                ? WordDefinitionDataModel.commands.gameEnd.value
                : WordDefinitionDataModel.commands.gameEnd.initialValue;

        public static string options =>
            (WordDefinitionDataModel.commands.option.enabled == 1)
                ? WordDefinitionDataModel.commands.option.value
                : WordDefinitionDataModel.commands.option.initialValue;

        public static string weapon =>
            (WordDefinitionDataModel.commands.weapon.enabled == 1)
                ? WordDefinitionDataModel.commands.weapon.value
                : WordDefinitionDataModel.commands.weapon.initialValue;

        public static string armor =>
            (WordDefinitionDataModel.commands.armor.enabled == 1)
                ? WordDefinitionDataModel.commands.armor.value
                : WordDefinitionDataModel.commands.armor.initialValue;

        public static string keyItem =>
            (WordDefinitionDataModel.commands.keyItem.enabled == 1)
                ? WordDefinitionDataModel.commands.keyItem.value
                : WordDefinitionDataModel.commands.keyItem.initialValue;

        public static string equipment2 =>
            (WordDefinitionDataModel.commands.equipment2.enabled == 1)
                ? WordDefinitionDataModel.commands.equipment2.value
                : WordDefinitionDataModel.commands.equipment2.initialValue;

        public static string optimize =>
            (WordDefinitionDataModel.commands.strongestEquipment.enabled == 1)
                ? WordDefinitionDataModel.commands.strongestEquipment.value
                : WordDefinitionDataModel.commands.strongestEquipment.initialValue;

        public static string clear =>
            (WordDefinitionDataModel.commands.removeAll.enabled == 1)
                ? WordDefinitionDataModel.commands.removeAll.value
                : WordDefinitionDataModel.commands.removeAll.initialValue;

        public static string newGame =>
            (WordDefinitionDataModel.commands.newGame.enabled == 1)
                ? WordDefinitionDataModel.commands.newGame.value
                : WordDefinitionDataModel.commands.newGame.initialValue;

        public static string continue_ =>
            (WordDefinitionDataModel.commands.menuContinue.enabled == 1)
                ? WordDefinitionDataModel.commands.menuContinue.value
                : WordDefinitionDataModel.commands.menuContinue.initialValue;

        public static string toTitle =>
            (WordDefinitionDataModel.commands.backTitle.enabled == 1)
                ? WordDefinitionDataModel.commands.backTitle.value
                : WordDefinitionDataModel.commands.backTitle.initialValue;

        public static string cancel =>
            (WordDefinitionDataModel.commands.pause.enabled == 1)
                ? WordDefinitionDataModel.commands.pause.value
                : WordDefinitionDataModel.commands.pause.initialValue;

        public static string buy =>
            (WordDefinitionDataModel.commands.buy.enabled == 1)
                ? WordDefinitionDataModel.commands.buy.value
                : WordDefinitionDataModel.commands.buy.initialValue;

        public static string sell =>
            (WordDefinitionDataModel.commands.sell.enabled == 1)
                ? WordDefinitionDataModel.commands.sell.value
                : WordDefinitionDataModel.commands.sell.initialValue;

        public static string alwaysDash =>
            (WordDefinitionDataModel.commands.alwaysDash.enabled == 1)
                ? WordDefinitionDataModel.commands.alwaysDash.value
                : WordDefinitionDataModel.commands.alwaysDash.initialValue;

        public static string commandRemember =>
            (WordDefinitionDataModel.commands.saveCommand.enabled == 1)
                ? WordDefinitionDataModel.commands.saveCommand.value
                : WordDefinitionDataModel.commands.saveCommand.initialValue;

        public static string bgmVolume =>
            (WordDefinitionDataModel.commands.volumeBgm.enabled == 1)
                ? WordDefinitionDataModel.commands.volumeBgm.value
                : WordDefinitionDataModel.commands.volumeBgm.initialValue;

        public static string bgsVolume =>
            (WordDefinitionDataModel.commands.volumeBgs.enabled == 1)
                ? WordDefinitionDataModel.commands.volumeBgs.value
                : WordDefinitionDataModel.commands.volumeBgs.initialValue;

        public static string meVolume =>
            (WordDefinitionDataModel.commands.volumeMe.enabled == 1)
                ? WordDefinitionDataModel.commands.volumeMe.value
                : WordDefinitionDataModel.commands.volumeMe.initialValue;

        public static string seVolume =>
            (WordDefinitionDataModel.commands.volumeSe.enabled == 1)
                ? WordDefinitionDataModel.commands.volumeSe.value
                : WordDefinitionDataModel.commands.volumeSe.initialValue;

        public static string possession =>
            (WordDefinitionDataModel.commands.possessionNum.enabled == 1)
                ? WordDefinitionDataModel.commands.possessionNum.value
                : WordDefinitionDataModel.commands.possessionNum
                    .initialValue;

        public static string stMaxHp => (WordDefinitionDataModel.status.maxHp.enabled == 1)
            ? WordDefinitionDataModel.status.maxHp.value
            : WordDefinitionDataModel.status.maxHp.initialValue;

        public static string stMaxMp => (WordDefinitionDataModel.status.maxMp.enabled == 1)
            ? WordDefinitionDataModel.status.maxMp.value
            : WordDefinitionDataModel.status.maxMp.initialValue;

        public static string stAttack => (WordDefinitionDataModel.status.attack.enabled == 1)
            ? WordDefinitionDataModel.status.attack.value
            : WordDefinitionDataModel.status.attack.initialValue;

        public static string stGuard => (WordDefinitionDataModel.status.guard.enabled == 1)
            ? WordDefinitionDataModel.status.guard.value
            : WordDefinitionDataModel.status.guard.initialValue;

        public static string stMagic => (WordDefinitionDataModel.status.magic.enabled == 1)
            ? WordDefinitionDataModel.status.magic.value
            : WordDefinitionDataModel.status.magic.initialValue;

        public static string stMagicGuard => (WordDefinitionDataModel.status.magicGuard.enabled == 1)
            ? WordDefinitionDataModel.status.magicGuard.value
            : WordDefinitionDataModel.status.magicGuard.initialValue;

        public static string stSpeed => (WordDefinitionDataModel.status.speed.enabled == 1)
            ? WordDefinitionDataModel.status.speed.value
            : WordDefinitionDataModel.status.speed.initialValue;

        public static string stLuck =>
            (WordDefinitionDataModel.status.luck.enabled == 1)
                ? WordDefinitionDataModel.status.luck.value
                : WordDefinitionDataModel.status.luck.initialValue;

        public static string stEvasion => (WordDefinitionDataModel.status.evasion.enabled == 1)
            ? WordDefinitionDataModel.status.evasion.value
            : WordDefinitionDataModel.status.evasion.initialValue;

        public static string expTotal =>
            (WordDefinitionDataModel.messages.expTotal.enabled == 1)
                ? WordDefinitionDataModel.messages.expTotal.value
                : WordDefinitionDataModel.messages.expTotal.initialValue;

        public static string expNext =>
            (WordDefinitionDataModel.messages.expNext.enabled == 1)
                ? WordDefinitionDataModel.messages.expNext.value
                : WordDefinitionDataModel.messages.expNext.initialValue;

        public static string saveMessage =>
            (WordDefinitionDataModel.messages.saveMessage.enabled == 1)
                ? WordDefinitionDataModel.messages.saveMessage.value
                : WordDefinitionDataModel.messages.saveMessage.initialValue;

        public static string loadMessage =>
            (WordDefinitionDataModel.messages.loadMessage.enabled == 1)
                ? WordDefinitionDataModel.messages.loadMessage.value
                : WordDefinitionDataModel.messages.loadMessage.initialValue;

        public static string file =>
            (WordDefinitionDataModel.messages.file.enabled == 1)
                ? WordDefinitionDataModel.messages.file.value
                : WordDefinitionDataModel.messages.file.initialValue;

        public static string partyName =>
            (WordDefinitionDataModel.messages.partyName.enabled == 1)
                ? WordDefinitionDataModel.messages.partyName.value
                : WordDefinitionDataModel.messages.partyName.initialValue;

        public static string emerge =>
            (WordDefinitionDataModel.messages.emerge.enabled == 1)
                ? WordDefinitionDataModel.messages.emerge.value
                : WordDefinitionDataModel.messages.emerge.initialValue;

        public static string preemptive =>
            (WordDefinitionDataModel.messages.preemptive.enabled == 1)
                ? WordDefinitionDataModel.messages.preemptive.value
                : WordDefinitionDataModel.messages.preemptive.initialValue;

        public static string surprise =>
            (WordDefinitionDataModel.messages.surprise.enabled == 1)
                ? WordDefinitionDataModel.messages.surprise.value
                : WordDefinitionDataModel.messages.surprise.initialValue;

        public static string escapeStart =>
            (WordDefinitionDataModel.messages.escapeStart.enabled == 1)
                ? WordDefinitionDataModel.messages.escapeStart.value
                : WordDefinitionDataModel.messages.escapeStart.initialValue;

        public static string escapeFailure =>
            (WordDefinitionDataModel.messages.escapeFailure.enabled == 1)
                ? WordDefinitionDataModel.messages.escapeFailure.value
                : WordDefinitionDataModel.messages.escapeFailure.initialValue;

        public static string victory =>
            (WordDefinitionDataModel.messages.victory.enabled == 1)
                ? WordDefinitionDataModel.messages.victory.value
                : WordDefinitionDataModel.messages.victory.initialValue;

        public static string defeat =>
            (WordDefinitionDataModel.messages.defeat.enabled == 1)
                ? WordDefinitionDataModel.messages.defeat.value
                : WordDefinitionDataModel.messages.defeat.initialValue;

        public static string obtainExp =>
            (WordDefinitionDataModel.messages.obtainExp.enabled == 1)
                ? WordDefinitionDataModel.messages.obtainExp.value
                : WordDefinitionDataModel.messages.obtainExp.initialValue;

        public static string obtainGold =>
            (WordDefinitionDataModel.messages.obtainGold.enabled == 1)
                ? WordDefinitionDataModel.messages.obtainGold.value
                : WordDefinitionDataModel.messages.obtainGold.initialValue;

        public static string obtainItem =>
            (WordDefinitionDataModel.messages.obtainItem.enabled == 1)
                ? WordDefinitionDataModel.messages.obtainItem.value
                : WordDefinitionDataModel.messages.obtainItem.initialValue;

        public static string levelUp =>
            (WordDefinitionDataModel.messages.levelUp.enabled == 1)
                ? WordDefinitionDataModel.messages.levelUp.value
                : WordDefinitionDataModel.messages.levelUp.initialValue;

        public static string obtainSkill =>
            (WordDefinitionDataModel.messages.obtainSkill.enabled == 1)
                ? WordDefinitionDataModel.messages.obtainSkill.value
                : WordDefinitionDataModel.messages.obtainSkill.initialValue;

        public static string useItem =>
            (WordDefinitionDataModel.messages.useItem.enabled == 1)
                ? WordDefinitionDataModel.messages.useItem.value
                : WordDefinitionDataModel.messages.useItem.initialValue;

        public static string criticalToEnemy =>
            (WordDefinitionDataModel.messages.criticalToEnemy.enabled == 1)
                ? WordDefinitionDataModel.messages.criticalToEnemy.value
                : WordDefinitionDataModel.messages.criticalToEnemy.initialValue;

        public static string criticalToActor =>
            (WordDefinitionDataModel.messages.criticalToActor.enabled == 1)
                ? WordDefinitionDataModel.messages.criticalToActor.value
                : WordDefinitionDataModel.messages.criticalToActor.initialValue;

        public static string actorDamage =>
            (WordDefinitionDataModel.messages.actorDamage.enabled == 1)
                ? WordDefinitionDataModel.messages.actorDamage.value
                : WordDefinitionDataModel.messages.actorDamage.initialValue;

        public static string actorRecovery =>
            (WordDefinitionDataModel.messages.actorRecovery.enabled == 1)
                ? WordDefinitionDataModel.messages.actorRecovery.value
                : WordDefinitionDataModel.messages.actorRecovery.initialValue;

        public static string actorGain =>
            (WordDefinitionDataModel.messages.actorGain.enabled == 1)
                ? WordDefinitionDataModel.messages.actorGain.value
                : WordDefinitionDataModel.messages.actorGain.initialValue;

        public static string actorLoss =>
            (WordDefinitionDataModel.messages.actorLoss.enabled == 1)
                ? WordDefinitionDataModel.messages.actorLoss.value
                : WordDefinitionDataModel.messages.actorLoss.initialValue;

        public static string actorDrain =>
            (WordDefinitionDataModel.messages.actorDrain.enabled == 1)
                ? WordDefinitionDataModel.messages.actorDrain.value
                : WordDefinitionDataModel.messages.actorDrain.initialValue;

        public static string actorNoDamage =>
            (WordDefinitionDataModel.messages.actorNoDamage.enabled == 1)
                ? WordDefinitionDataModel.messages.actorNoDamage.value
                : WordDefinitionDataModel.messages.actorNoDamage.initialValue;

        public static string actorNoHit =>
            (WordDefinitionDataModel.messages.actorNoHit.enabled == 1)
                ? WordDefinitionDataModel.messages.actorNoHit.value
                : WordDefinitionDataModel.messages.actorNoHit.initialValue;

        public static string enemyDamage =>
            (WordDefinitionDataModel.messages.enemyDamage.enabled == 1)
                ? WordDefinitionDataModel.messages.enemyDamage.value
                : WordDefinitionDataModel.messages.enemyDamage.initialValue;

        public static string enemyRecovery =>
            (WordDefinitionDataModel.messages.enemyRecovery.enabled == 1)
                ? WordDefinitionDataModel.messages.enemyRecovery.value
                : WordDefinitionDataModel.messages.enemyRecovery.initialValue;

        public static string enemyGain =>
            (WordDefinitionDataModel.messages.enemyGain.enabled == 1)
                ? WordDefinitionDataModel.messages.enemyGain.value
                : WordDefinitionDataModel.messages.enemyGain.initialValue;

        public static string enemyLoss =>
            (WordDefinitionDataModel.messages.enemyLoss.enabled == 1)
                ? WordDefinitionDataModel.messages.enemyLoss.value
                : WordDefinitionDataModel.messages.enemyLoss.initialValue;

        public static string enemyDrain =>
            (WordDefinitionDataModel.messages.enemyDrain.enabled == 1)
                ? WordDefinitionDataModel.messages.enemyDrain.value
                : WordDefinitionDataModel.messages.enemyDrain.initialValue;

        public static string enemyNoDamage =>
            (WordDefinitionDataModel.messages.enemyNoDamage.enabled == 1)
                ? WordDefinitionDataModel.messages.enemyNoDamage.value
                : WordDefinitionDataModel.messages.enemyNoDamage.initialValue;

        public static string enemyNoHit =>
            (WordDefinitionDataModel.messages.enemyNoHit.enabled == 1)
                ? WordDefinitionDataModel.messages.enemyNoHit.value
                : WordDefinitionDataModel.messages.enemyNoHit.initialValue; 

        public static string evasion =>
            (WordDefinitionDataModel.messages.evasion.enabled == 1)
                ? WordDefinitionDataModel.messages.evasion.value
                : WordDefinitionDataModel.messages.evasion.initialValue; 

        public static string magicEvasion =>
            (WordDefinitionDataModel.messages.magicEvasion.enabled == 1)
                ? WordDefinitionDataModel.messages.magicEvasion.value
                : WordDefinitionDataModel.messages.magicEvasion.initialValue; 

        public static string magicReflection =>
            (WordDefinitionDataModel.messages.magicReflection.enabled == 1)
                ? WordDefinitionDataModel.messages.magicReflection.value
                : WordDefinitionDataModel.messages.magicReflection.initialValue; 

        public static string counterAttack =>
            (WordDefinitionDataModel.messages.counterAttack.enabled == 1)
                ? WordDefinitionDataModel.messages.counterAttack.value
                : WordDefinitionDataModel.messages.counterAttack.initialValue; 

        public static string substitute =>
            (WordDefinitionDataModel.messages.substitute.enabled == 1)
                ? WordDefinitionDataModel.messages.substitute.value
                : WordDefinitionDataModel.messages.substitute.initialValue; 

        public static string buffAdd =>
            (WordDefinitionDataModel.messages.buffAdd.enabled == 1)
                ? WordDefinitionDataModel.messages.buffAdd.value
                : WordDefinitionDataModel.messages.buffAdd.initialValue; 

        public static string debuffAdd =>
            (WordDefinitionDataModel.messages.debuffAdd.enabled == 1)
                ? WordDefinitionDataModel.messages.debuffAdd.value
                : WordDefinitionDataModel.messages.debuffAdd.initialValue; 

        public static string buffRemove =>
            (WordDefinitionDataModel.messages.buffRemove.enabled == 1)
                ? WordDefinitionDataModel.messages.buffRemove.value
                : WordDefinitionDataModel.messages.buffRemove.initialValue; 

        public static string actionFailure =>
            (WordDefinitionDataModel.messages.actionFailure.enabled == 1)
                ? WordDefinitionDataModel.messages.actionFailure.value
                : WordDefinitionDataModel.messages.actionFailure.initialValue;
    }
}