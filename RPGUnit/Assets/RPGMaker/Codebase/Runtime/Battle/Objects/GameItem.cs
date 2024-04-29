using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCustom;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.Runtime.Battle.Wrapper;
using RPGMaker.Codebase.Runtime.Common;
using System.Collections.Generic;

namespace RPGMaker.Codebase.Runtime.Battle.Objects
{
    /// <summary>
    /// アイテム全般とスキルをまとめて扱うクラス
    /// </summary>
    public class GameItem {
        /// <summary>
        /// アイテムの種類
        /// </summary>
        public enum DataClassEnum {
            Skill,      //スキル
            Item,       //アイテム
            Weapon,     //武器
            Armor,      //防具
            None
        }

        /// <summary>
        /// アイテムの種類( ‘item’, ‘skill’, ‘weapon’, ‘armor’, ‘’ )
        /// </summary>
        public DataClassEnum DataClass { get; private set; }
        /// <summary>
        /// アイテムID(種類毎に異なる)
        /// </summary>
        public string ItemId { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="dataClass"></param>
        public GameItem(string itemId, DataClassEnum dataClass) {
            DataClass = dataClass;
            ItemId = itemId;
            switch (DataClass)
            {
                case DataClassEnum.Skill:
                    SkillData = DataManager.Self().GetSkillCustomDataModel(ItemId);
                    if (SkillData == null) DataClass = DataClassEnum.None;
                    break;
                case DataClassEnum.Item:
                    ItemData = DataManager.Self().GetItemDataModel(ItemId);
                    if (ItemData == null) DataClass = DataClassEnum.None;
                    break;
                case DataClassEnum.Weapon:
                    WeaponData = DataManager.Self().GetWeaponDataModel(ItemId);
                    if (WeaponData == null) DataClass = DataClassEnum.None;
                    break;
                case DataClassEnum.Armor:
                    ArmorData = DataManager.Self().GetArmorDataModel(ItemId);
                    if (ArmorData == null) DataClass = DataClassEnum.None;
                    break;
                default:
                    //追加だけする
                    break;
            }
        }

        /// <summary>
        /// スキルか
        /// </summary>
        /// <returns></returns>
        public bool IsSkill() {
            return DataClass == DataClassEnum.Skill;
        }

        /// <summary>
        /// アイテムか
        /// </summary>
        /// <returns></returns>
        public bool IsItem() {
            return DataClass == DataClassEnum.Item;
        }

        /// <summary>
        /// 武器か
        /// </summary>
        /// <returns></returns>
        public bool IsWeapon() {
            return DataClass == DataClassEnum.Weapon;
        }

        /// <summary>
        /// 防具か
        /// </summary>
        /// <returns></returns>
        public bool IsArmor() {
            return DataClass == DataClassEnum.Armor;
        }

        //===============================================================
        // 各データモデル
        //===============================================================

        /// <summary>
        /// Skill用
        /// </summary>
        public SkillCustomDataModel SkillData { get; set; }
        /// <summary>
        /// Item用
        /// </summary>
        public ItemDataModel ItemData { get; set; }
        /// <summary>
        /// Weapon用
        /// </summary>
        public WeaponDataModel WeaponData { get; set; }
        /// <summary>
        /// Armor用
        /// </summary>
        public ArmorDataModel ArmorData { get; set; }

        /// <summary>
        /// ダメージや回復に関するデータ
        /// </summary>
        public class CalcData
        {
            /// <summary>
            /// 計算式
            /// </summary>
            public string formula;
            /// <summary>
            /// 最大値に対する割合
            /// </summary>
            public int valuePerMax;
            /// <summary>
            /// 最小値
            /// </summary>
            public float valueMin;
            /// <summary>
            /// 最大値
            /// </summary>
            public float valueMax;
            /// <summary>
            /// 固定値
            /// </summary>
            public int valueFix;
            /// <summary>
            /// 分散度
            /// </summary>
            public int variance;
            /// <summary>
            /// 会心有無
            /// </summary>
            public int critical;
        }

        //===============================================================
        // 全ての種別で利用する
        //===============================================================

        /// <summary>
        /// 名前
        /// </summary>
        public string Name {
            get {
                if (IsSkill()) return SkillData.basic.name;
                else if (IsItem()) return ItemData.basic.name;
                else if (IsWeapon()) return WeaponData.basic.name;
                else if (IsArmor()) return ArmorData.basic.name;
                return "";
            }
        }
        /// <summary>
        /// 説明文
        /// </summary>
        public string Description
        {
            get
            {
                if (IsSkill()) return SkillData.basic.description;
                else if (IsItem()) return ItemData.basic.description;
                else if (IsWeapon()) return WeaponData.basic.description;
                else if (IsArmor()) return ArmorData.basic.description;
                return "";
            }
        }
        /// <summary>
        /// アイコン
        /// </summary>
        public string Icon {
            get
            {
                if (IsSkill()) return SkillData.basic.iconId;
                else if (IsItem()) return ItemData.basic.iconId;
                else if (IsWeapon()) return WeaponData.basic.iconId;
                else if (IsArmor()) return ArmorData.basic.iconId;
                return "";
            }
        }

        /// <summary>
        /// 使用者への影響
        /// </summary>
        public int UserScope { 
            get {
                if (IsSkill()) return SkillData.targetEffect.targetUser;
                else if (IsItem()) return ItemData.targetEffect.targetUser;
                return 0;
            }
        }


        //===============================================================
        // DataClassEnum.Skill又は、Itemの場合に利用する
        //===============================================================

        /// <summary>
        /// 使用可能時（常時、メニュー、バトル、使用不可）
        /// </summary>
        public int Occasion
        {
            get
            {
                if (IsSkill()) return SkillData.basic.canUseTiming;
                else if (IsItem()) return ItemData.basic.canUseTiming;
                return 3;
            }
        }
        /// <summary>
        /// 対象者への影響 範囲
        /// </summary>
        public int Scope
        {
            get
            {
                if (IsSkill())
                {
                    return ConvertUniteData.SetScope(SkillData.targetEffect.targetTeam, SkillData.targetEffect.targetRange,
                        SkillData.targetEffect.randomNumber, SkillData.targetEffect.targetStatus);
                }
                else if (IsItem())
                {
                    return ConvertUniteData.SetScope(ItemData.targetEffect.targetTeam, ItemData.targetEffect.targetRange,
                        ItemData.targetEffect.randomNumber, ItemData.targetEffect.targetStatus);
                }
                return 0;
            }
        }
        /// <summary>
        /// 対象者への影響 速度補正
        /// </summary>
        public int Speed
        {
            get
            {
                if (IsSkill()) return SkillData.targetEffect.activate.correctionSpeed;
                else if (IsItem()) return ItemData.targetEffect.activate.correctionSpeed;
                return 0;
            }
        }
        /// <summary>
        /// 成功率
        /// </summary>
        public int SuccessRate
        {
            get
            {
                if (IsSkill()) return SkillData.targetEffect.activate.successRate;
                else if (IsItem()) return ItemData.targetEffect.activate.successRate;
                return 0;
            }
        }
        /// <summary>
        /// 対象者への影響 連続回数
        /// </summary>
        public double Repeats
        {
            get
            {
                if (IsSkill()) return SkillData.targetEffect.activate.continuousNumber;
                else if (IsItem()) return ItemData.targetEffect.activate.continuousNumber;
                return 0;
            }
        }
        /// <summary>
        /// 対象者への影響 命中タイプ
        /// </summary>
        public int HitType
        {
            get
            {
                if (IsSkill()) return SkillData.targetEffect.activate.hitType;
                else if (IsItem()) return ItemData.targetEffect.activate.hitType;
                return 0;
            }
        }
        /// <summary>
        /// アニメーションID
        /// </summary>
        public string AnimationId
        {
            get
            {
                if (IsSkill()) return SkillData.targetEffect.activate.animationId;
                else if (IsItem()) return ItemData.targetEffect.activate.animationId;
                else if (IsWeapon()) return WeaponData.basic.animationId;
                return "";
            }
        }

        /// <summary>
        /// 対象者への影響 HP回復するかどうか
        /// </summary>
        public int RecoverHp
        {
            get
            {
                if (IsSkill()) return SkillData.targetEffect.heal.hp.enabled;
                else if (IsItem()) return ItemData.targetEffect.heal.hp.enabled;
                return 0;
            }
        }

        /// <summary>
        /// 使用者への影響 HP回復するかどうか
        /// </summary>
        public int RecoverHpMyself
        {
            get
            {
                if (IsSkill()) return SkillData.userEffect.heal.hp.enabled;
                else if (IsItem()) return ItemData.userEffect.heal.hp.enabled;
                return 0;
            }
        }

        /// <summary>
        /// 対象者への影響 MP回復するかどうか
        /// </summary>
        public int RecoverMp
        {
            get
            {
                if (IsSkill()) return SkillData.targetEffect.heal.mp.enabled;
                else if (IsItem()) return ItemData.targetEffect.heal.mp.enabled;
                return 0;
            }
        }

        /// <summary>
        /// 使用者への影響 MP回復するかどうか
        /// </summary>
        public int RecoverMpMyself
        {
            get
            {
                if (IsSkill()) return SkillData.userEffect.heal.mp.enabled;
                else if (IsItem()) return ItemData.userEffect.heal.mp.enabled;
                return 0;
            }
        }

        /// <summary>
        /// 対象者への影響 TP回復するかどうか
        /// </summary>
        public int RecoverTp
        {
            get
            {
                if (IsSkill()) return SkillData.targetEffect.heal.tp.enabled;
                else if (IsItem()) return ItemData.targetEffect.heal.tp.enabled;
                return 0;
            }
        }

        /// <summary>
        /// 対象者への影響 タイプ
        /// Uniteで設定したダメージタイプを、MVの設定値に変換の上で返却する
        /// </summary>
        public int DamageType
        {
            get
            {
                if (IsSkill()) return ConvertUniteData.SetDamageType(SkillData.targetEffect.damage.damageType);
                else if (IsItem()) return ConvertUniteData.SetDamageType(ItemData.targetEffect.damage.damageType);
                return 0;
            }
        }

        /// <summary>
        /// 対象者への影響 タイプ
        /// Uniteで設定したダメージタイプを、MVの設定値に変換の上で返却する
        /// </summary>
        public int DamageTypeMyself
        {
            get
            {
                if (IsSkill()) return ConvertUniteData.SetDamageType(SkillData.userEffect.damage.damageType);
                else if (IsItem()) return ConvertUniteData.SetDamageType(ItemData.userEffect.damage.damageType);
                return 0;
            }
        }

        /// <summary>
        /// 対象者への影響 攻撃タイプ
        /// </summary>
        public int AttackType
        {
            get
            {
                if (IsSkill()) return SkillData.targetEffect.damage.attackType;
                return -1;
            }
        }

        /// <summary>
        /// 対象者への影響 属性
        /// </summary>
        public int DamageElementId
        {
            get
            {
                if (IsSkill())
                {
                    return SkillData.targetEffect.damage.elements.Count > 0
                            ? SkillData.targetEffect.damage.elements[0]
                            : 0;
                }
                else if (IsItem())
                {
                    return ItemData.targetEffect.damage.elements.Count > 0
                            ? ItemData.targetEffect.damage.elements[0]
                            : 0;
                }
                return 0;
            }
        }

        /// <summary>
        /// 対象者への影響 ダメージに関するデータ
        /// </summary>
        public CalcData DamageData
        {
            get
            {
                CalcData data = new CalcData();
                data.formula = "";
                data.valuePerMax = -1;
                data.valueMax = -1;
                data.valueMin = -1;
                data.valueFix = -1;
                data.variance = 0;
                data.critical = -1;

                if (IsSkill())
                {
                    if (SkillData.targetEffect.damage.autoguideEnabled == 0)
                    {
                        //オートガイド未使用
                        //計算式
                        data.formula = SkillData.targetEffect.damage.value;
                        //ダメージの最大値
                        if (SkillData.targetEffect.damage.maxEnabled == 1)
                            data.valueMax = SkillData.targetEffect.damage.max;
                        //ダメージの最小値
                        if (SkillData.targetEffect.damage.minEnabled == 1)
                            data.valueMin = SkillData.targetEffect.damage.min;
                        //分散度
                        if (SkillData.targetEffect.damage.distributeEnabled == 1)
                            data.variance = SkillData.targetEffect.damage.distribute;
                        //会心有無
                        data.critical = SkillData.targetEffect.damage.critical;
                    }
                    else
                    {
                        //オートガイド利用
                        //計算式
                        data.formula = SkillData.targetEffect.damage.autoguide.calc;
                        //ダメージの最大値
                        if (SkillData.targetEffect.damage.maxEnabled == 1)
                            data.valueMax = SkillData.targetEffect.damage.autoguide.max;
                        //ダメージの最小値
                        if (SkillData.targetEffect.damage.minEnabled == 1)
                            data.valueMin = SkillData.targetEffect.damage.autoguide.min;
                        //分散度
                        if (SkillData.targetEffect.damage.distributeEnabled == 1)
                            data.variance = SkillData.targetEffect.damage.autoguide.distribute;
                        //会心有無
                        data.critical = SkillData.targetEffect.damage.autoguide.critical;
                    }
                }
                else if (IsItem())
                {
                    //計算式
                    data.formula = ItemData.targetEffect.damage.value;
                    //ダメージの最大値
                    if (ItemData.targetEffect.damage.maxEnabled == 1)
                        data.valueMax = ItemData.targetEffect.damage.max;
                    //ダメージの最小値
                    if (ItemData.targetEffect.damage.minEnabled == 1)
                        data.valueMin = ItemData.targetEffect.damage.min;
                    //分散度
                    if (ItemData.targetEffect.damage.distributeEnabled == 1)
                        data.variance = ItemData.targetEffect.damage.distribute;
                    //会心有無
                    data.critical = ItemData.targetEffect.damage.critical;
                }

                return data;
            }
        }

        /// <summary>
        /// 対象者への影響 ダメージに関するデータ（使用者への影響）
        /// </summary>
        public CalcData DamageDataMyself
        {
            get
            {
                CalcData data = new CalcData();
                data.formula = "";
                data.valuePerMax = -1;
                data.valueMax = -1;
                data.valueMin = -1;
                data.valueFix = -1;
                data.variance = 0;
                data.critical = -1;

                if (IsSkill())
                {
                    //計算式
                    data.formula = SkillData.userEffect.damage.value;
                    //ダメージの最大値
                    if (SkillData.userEffect.damage.maxEnabled == 1)
                        data.valueMax = SkillData.userEffect.damage.max;
                    //ダメージの最小値
                    if (SkillData.userEffect.damage.minEnabled == 1)
                        data.valueMin = SkillData.userEffect.damage.min;
                    //分散度
                    if (SkillData.userEffect.damage.distributeEnabled == 1)
                        data.variance = SkillData.userEffect.damage.distribute;
                    //会心有無
                    data.critical = SkillData.userEffect.damage.critical;
                }
                else if (IsItem())
                {
                    //計算式
                    data.formula = ItemData.userEffect.damage.value;
                    //ダメージの最大値
                    if (ItemData.userEffect.damage.maxEnabled == 1)
                        data.valueMax = ItemData.userEffect.damage.max;
                    //ダメージの最小値
                    if (ItemData.userEffect.damage.minEnabled == 1)
                        data.valueMin = ItemData.userEffect.damage.min;
                    //分散度
                    if (ItemData.userEffect.damage.distributeEnabled == 1)
                        data.variance = ItemData.userEffect.damage.distribute;
                    //会心有無
                    data.critical = ItemData.userEffect.damage.critical;
                }

                return data;
            }
        }

        /// <summary>
        /// 対象者への影響 使用効果
        /// 型は特徴と一緒
        /// </summary>
        public List<TraitCommonDataModel> Effects {
            get
            {
                if (IsSkill()) return SkillData.targetEffect.otherEffects;
                else if (IsItem()) return ItemData.targetEffect.otherEffects;
                return new List<TraitCommonDataModel>();
            }
        }

        /// <summary>
        /// 使用者への影響 使用効果
        /// 型は特徴と一緒
        /// </summary>
        public List<TraitCommonDataModel> EffectsMyself
        {
            get
            {
                if (IsSkill()) return SkillData.userEffect.otherEffects;
                else if (IsItem()) return ItemData.userEffect.otherEffects;
                return new List<TraitCommonDataModel>();
            }
        }

        /// <summary>
        /// 使用者への影響 TP増加
        /// </summary>
        public int TpGain {
            get
            {
                if (IsSkill()) return SkillData.userEffect.getTp;
                if (IsItem()) return ItemData.userEffect.activate.getTp;
                return 0;
            }
        }

        //===============================================================
        // DataClassEnum.Skillの場合に利用する
        //===============================================================
        /// <summary>
        /// スキルタイプ
        /// </summary>
        public int STypeId
        {
            get
            {
                if (IsSkill()) return SkillData.basic.skillType;
                return 0;
            }
        }
        /// <summary>
        /// 消費MP
        /// </summary>
        public int MpCost {
            get
            {
                if (IsSkill()) return SkillData.basic.costMp;
                return 0;
            }
        }
        /// <summary>
        /// 消費TP
        /// </summary>
        public int TpCost {
            get
            {
                if (IsSkill()) return SkillData.basic.costTp;
                return 0;
            }
        }
        /// <summary>
        /// メッセージ
        /// </summary>
        public string Message1
        {
            get
            {
                if (IsSkill()) return SkillData.basic.message;
                return "";
            }
        }
        /// <summary>
        /// 必要武器タイプ1
        /// </summary>
        public int RequiredWTypeId1
        {
            get
            {
                if (IsSkill())
                {
                    return SkillData.basic.requiredWTypeId1;
                }
                return 0;
            }
        }
        /// <summary>
        /// 必要武器タイプ2
        /// </summary>
        public int RequiredWTypeId2
        {
            get
            {
                if (IsSkill()) return SkillData.basic.requiredWTypeId2;
                return 0;
            }
        }

        //===============================================================
        // DataClassEnum.Itemの場合に利用する
        //===============================================================

        /// <summary>
        /// アイテムタイプ
        /// </summary>
        public int ITypeId {
            get
            {
                if (IsItem()) return ItemData.basic.itemType;
                return 0;
            }
        }
        /// <summary>
        /// 消耗品
        /// </summary>
        public bool Consumable { 
            get
            {
                if (IsItem()) return ItemData.basic.consumable == 0 ? true : false;
                return false;
            }
        }

        //===============================================================
        // DataClassEnum.Weapon又は、Armorの場合に利用する
        //===============================================================

        /// <summary>
        /// 装備タイプ
        /// </summary>
        public string ETypeId {
            get
            {
                if (IsWeapon()) return WeaponData.basic.equipmentTypeId;
                else if (IsArmor()) return ArmorData.basic.equipmentTypeId;
                return "";
            }
        }
        /// <summary>
        /// 追加パラメータ
        /// </summary>
        public List<int> Parameters {
            get
            {
                if (IsWeapon()) return WeaponData.parameters;
                else if (IsArmor()) return ArmorData.parameters;
                return new List<int>();
            }
        }
        /// <summary>
        /// 特徴
        /// </summary>
        public List<TraitCommonDataModel> Traits {
            get
            {
                if (IsWeapon()) return WeaponData.traits;
                else if (IsArmor()) return ArmorData.traits;
                return new List<TraitCommonDataModel>();
            }
        }

        //===============================================================
        // DataClassEnum.Weaponの場合に利用する
        //===============================================================

        /// <summary>
        /// 武器タイプ
        /// </summary>
        public string WTypeId {
            get
            {
                if (IsWeapon()) return WeaponData.basic.weaponTypeId;
                return "";
            }
        }

        //===============================================================
        // DataClassEnum.Armorの場合に利用する
        //===============================================================

        /// <summary>
        /// 防具タイプ
        /// </summary>
        public string ATypeId {
            get
            {
                if (IsArmor()) return ArmorData.basic.armorTypeId;
                return "";
            }
        }
    }
}