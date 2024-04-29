using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common;
using System;

namespace RPGMaker.Codebase.Runtime.Battle.Wrapper
{
    /// <summary>
    /// Unite用のデータと従来のデータを変換するためのWrapperクラス
    /// </summary>
    public class ConvertUniteData
    {
        /// <summary>
        /// 武器タイプをSerialNoからUUIDに変換する
        /// </summary>
        /// <param name="serialNo"></param>
        /// <returns></returns>
        public static string WeaponTypeSerialNoToUuid(int serialNo) {
            //無し=0
            if (serialNo == 0) return "";

            //その他のケースではSerialNoを検索する
            for (var i = 0; i < DataManager.Self().GetSystemDataModel().weaponTypes.Count; i++)
            {
                //引数で渡されてくるIDは0オリジンで、SerialNumberは1オリジンのため、1加算して比較する
                if (DataManager.Self().GetSystemDataModel().weaponTypes[i].SerialNumber == serialNo + 1)
                {
                    return DataManager.Self().GetSystemDataModel().weaponTypes[i].id;
                }
            }

            //該当しなければ、無しと同じ扱いとする
            return "";
        }

        /// <summary>
        /// ステートをSerialNoからUUIDに変換する
        /// </summary>
        /// <param name="serialNo"></param>
        /// <returns></returns>
        public static string StateSerialNoToUuid(int serialNo) {
            for (var i = 0; i < DataManager.Self().GetStateDataModels().Count; i++)
            {
                //引数で渡されてくるIDは0オリジンで、SerialNumberは1オリジンのため、1加算して比較する
                if (DataManager.Self().GetStateDataModels()[i].SerialNumber == serialNo + 1)
                {
                    return DataManager.Self().GetStateDataModels()[i].id;
                }
            }
            return "";
        }

        /// <summary>
        /// ステートをSerialNoからUUIDに変換する
        /// </summary>
        /// <param name="serialNo"></param>
        /// <returns></returns>
        public static string StateSerialNoToUuid(string serialNo) {
            try
            {
                return StateSerialNoToUuid(int.Parse(serialNo));
            }
            catch (Exception) { }
            return "";
        }


        /// <summary>
        /// ステートをUUIDからSerialNoに変換する
        /// </summary>
        /// <param name="serialNo"></param>
        /// <returns></returns>
        public static int StateUuidToSerialNo(string uuid) {
            for (var i = 0; i < DataManager.Self().GetStateDataModels().Count; i++)
            {
                if (DataManager.Self().GetStateDataModels()[i].id == uuid)
                {
                    //SerialNumberは1オリジンのため、1減算して返却する
                    return DataManager.Self().GetStateDataModels()[i].SerialNumber - 1;
                }
            }
            return -1;
        }

        /// <summary>
        /// スコープをMV相当のデータに変換する
        /// </summary>
        /// <param name="target"></param>
        /// <param name="targetRange"></param>
        /// <param name="random"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static int SetScope(int target, int targetRange, int random, int status) {
            //0：None
            //1：1 Enemy
            //2：All Enemies
            //3：1 Random Enemy
            //4：2 Random Enemy
            //5：3 Random Enemy
            //6：4 Random Enemy
            //7：1 Ally
            //8：All Allies
            //9：1 Ally(Dead)
            //10：All Allies(Dead)
            //11：The User
            var param = 0;

            switch (target)
            {
                //なし
                case 0:
                    param = 0;
                    break;
                //敵
                case 1:
                    //単体
                    if (targetRange == 0)
                    {
                        param = 1;
                    }
                    //全体
                    else if (targetRange == 1)
                    {
                        param = 2;
                    }
                    //ランダム
                    else if (targetRange == 2)
                    {
                        if (random == 1)
                            param = 3;
                        else if (random == 2)
                            param = 4;
                        else if (random == 3)
                            param = 5;
                        else
                            param = 6;
                    }

                    break;
                //味方
                case 2:
                    if (targetRange == 0)
                    {
                        //生存
                        if (status == 0)
                            param = 7;
                        //戦闘不能
                        else if (status == 1)
                            param = 9;
                        //無条件
                        else
                            param = 12;
                    }
                    //全体
                    else if (targetRange == 1)
                    {
                        //生存
                        if (status == 0)
                            param = 8;
                        //戦闘不能
                        else if (status == 1)
                            param = 10;
                        //無条件
                        else
                            param = 13;
                    }

                    break;
                //敵と味方
                case 3:
                    param = 50;
                    break;
            }

            return param;
        }

        /// <summary>
        /// 特徴の変換処理
        /// </summary>
        /// <param name="trait"></param>
        /// <returns></returns>
        public static TraitCommonDataModel SetTraitCode(TraitCommonDataModel trait) {
            var category = -1;
            switch (trait.categoryId)
            {
                //耐性
                case 1:
                    if (trait.traitsId == 1)
                        category = GameBattlerBase.TraitElementRate;
                    else if (trait.traitsId == 2)
                        category = GameBattlerBase.TraitDebuffRate;
                    else if (trait.traitsId == 3)
                        category = GameBattlerBase.TraitStateRate;
                    else if (trait.traitsId == 4) 
                        category = GameBattlerBase.TraitStateResist;

                    break;
                //能力値
                case 2:
                    if (trait.traitsId == 1)
                        category = GameBattlerBase.TraitParam;
                    else if (trait.traitsId == 2)
                        category = GameBattlerBase.TraitXparam;
                    else if (trait.traitsId == 3)
                        category = GameBattlerBase.TraitSparam;

                    break;
                //攻撃
                case 3:
                    if (trait.traitsId == 1)
                        category = GameBattlerBase.TraitAttackElement;
                    else if (trait.traitsId == 2)
                        category = GameBattlerBase.TraitAttackState;
                    else if (trait.traitsId == 3)
                        category = GameBattlerBase.TraitAttackSpeed;
                    else if (trait.traitsId == 4)
                        category = GameBattlerBase.TraitAttackTimes;
                    else if (trait.traitsId == 5) 
                        category = GameBattlerBase.TraitAttackSkill;

                    break;
                //スキル
                case 4:
                    if (trait.traitsId == 2)
                        category = GameBattlerBase.TraitStypeSeal;
                    else if (trait.traitsId == 3)
                        category = GameBattlerBase.TraitSkillAdd;
                    else if (trait.traitsId == 4) 
                        category = GameBattlerBase.TraitSkillSeal;

                    break;
                //装備
                case 5:
                    if (trait.traitsId == 1)
                        category = GameBattlerBase.TraitEquipWeaponType;
                    else if (trait.traitsId == 2)
                        category = GameBattlerBase.TraitEquipArmorType;
                    else if (trait.traitsId == 3)
                        category = GameBattlerBase.TraitEquipLock;
                    else if (trait.traitsId == 4)
                        category = GameBattlerBase.TraitEquipSeal;
                    else if (trait.traitsId == 5) 
                        category = GameBattlerBase.TraitSlotType;

                    break;
                //その他
                case 6:
                    if (trait.traitsId == 1)
                        category = GameBattlerBase.TraitActionPlus;
                    else if (trait.traitsId == 2)
                        category = GameBattlerBase.TraitSpecialFlag;
                    else if (trait.traitsId == 3)
                        category = GameBattlerBase.TraitCollapseType;
                    else if (trait.traitsId == 4) 
                        category = GameBattlerBase.TraitPartyAbility;

                    break;
            }

            var ret = new TraitCommonDataModel(category, trait.traitsId, trait.effectId, trait.value);
            return ret;
        }

        /// <summary>
        /// 使用効果のコードを、UniteからMVに変換する
        /// </summary>
        /// <param name="effect"></param>
        /// <returns></returns>
        public static int SetEffectCode(TraitCommonDataModel effect) {
            var code = -1;
            if (effect.categoryId == 1)
            {
                if (effect.traitsId == 1)
                    code = GameAction.EffectAddState;
                else
                    code = GameAction.EffectRemoveState;
            }
            else if (effect.categoryId == 2)
            {
                if (effect.traitsId == 1)
                    code = GameAction.EffectAddBuff;
                else if (effect.traitsId == 2)
                    code = GameAction.EffectAddDebuff;
                else if (effect.traitsId == 3)
                    code = GameAction.EffectRemoveBuff;
                else if (effect.traitsId == 4) 
                    code = GameAction.EffectRemoveDebuff;
            }
            else if (effect.categoryId == 3)
            {
                if (effect.traitsId == 1)
                    code = GameAction.EffectSpecial;
                else if (effect.traitsId == 2)
                    code = GameAction.EffectGrow;
                else if (effect.traitsId == 3) 
                    code = GameAction.EffectLearnSkill;
                else if (effect.traitsId == 4) 
                    code = GameAction.EffectCommonEvent;
            }

            return code;
        }

        /// <summary>
        /// ダメージタイプを、UniteからMVに変換する
        /// Unite:[なし],[HPダメージ],[HP吸収],[MPダメージ],[MP吸収]
        /// MV:[なし],[HPダメージ],[MPダメージ],[HP回復],[MP回復],[HP吸収],[MP吸収]
        /// </summary>
        /// <param name="damageType"></param>
        /// <returns></returns>
        public static int SetDamageType(int damageType) {
            switch (damageType)
            {
                case 1:
                    return 1;
                case 2:
                    return 5;
                case 3:
                    return 2;
                case 4:
                    return 6;
            }
            return 0;
        }

        /// <summary>
        /// SParamのデータをUniteからMVに変換する
        /// 
        /// 最終的に不要になるが、なにかの用途（ログ出力など）に利用する可能性があるため残しておく
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static int SetSparam(int code) {
            switch (code)
            {
                case 0: //狙われ率
                case 1: //防御率
                case 2: //回復率
                case 3: //薬効果率
                case 4: //MP消費率
                case 5: //TPチャージ率
                case 6: //物理ダメージ率
                case 7: //魔法ダメージ率
                case 8: //床ダメージ率
                case 9: //経験値率
                    return code;
                default:
                    //本来は存在しないが、万が一来てしまった場合は、全然関係のない数値を返却する
                    return 10;
            }
        }
    }
}
