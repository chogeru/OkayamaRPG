using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.State;
using RPGMaker.Codebase.Runtime.Battle.Wrapper;
using RPGMaker.Codebase.Runtime.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

namespace RPGMaker.Codebase.Runtime.Battle.Objects
{
    /// <summary>
    /// BattleManager に保持され、戦闘シーンでのパラメータの計算に利用される
    /// </summary>
    public class GameBattlerBase
    {
        /// <summary>
        /// 死亡ステートのID
        /// </summary>
        public const string DeathStateId  = "8fd93d41-fb58-401b-8d6b-f7d5396d3fec";
        public const string AttackSkillId = "1";
        public const string GuardSkillId = "2";

        /// <summary>
        /// [耐性 - 属性有効度]
        /// </summary>
        public const int TraitElementRate = 11;
        /// <summary>
        /// [耐性 - 弱体有効度]
        /// </summary>
        public const int TraitDebuffRate = 12;
        /// <summary>
        /// [耐性 - ステート有効度]
        /// </summary>
        public const int TraitStateRate = 13;
        /// <summary>
        /// [耐性 - ステート無効化]
        /// </summary>
        public const int TraitStateResist = 14;
        /// <summary>
        /// 属性優勢（Uniteで追加）
        /// </summary>
        public const int TraitSuperiority = 15;
        /// <summary>
        /// 属性劣勢（Uniteで追加）
        /// </summary>
        public const int TraitInferiority = 16;
        /// <summary>
        /// [能力値 - 通常能力値]
        /// </summary>
        public const int TraitParam = 21;
        /// <summary>
        /// [能力値 - 追加能力値]
        /// </summary>
        public const int TraitXparam = 22;
        /// <summary>
        /// [能力値 - 特殊能力値]
        /// </summary>
        public const int TraitSparam = 23;
        /// <summary>
        /// [攻撃 - 攻撃時属性]
        /// </summary>
        public const int TraitAttackElement = 31;
        /// <summary>
        /// [攻撃 - 攻撃時ステート]
        /// </summary>
        public const int TraitAttackState = 32;
        /// <summary>
        /// [攻撃 - 攻撃速度補正]
        /// </summary>
        public const int TraitAttackSpeed = 33;
        /// <summary>
        /// [攻撃 - 攻撃追加回数]
        /// </summary>
        public const int TraitAttackTimes = 34;
        /// <summary>
        /// 攻撃スキル（Uniteで追加）
        /// </summary>
        public const int TraitAttackSkill = 35;
        /// <summary>
        /// [スキル - スキルタイプ追加]
        /// </summary>
        public const int TraitStypeAdd = 41;
        /// <summary>
        /// [スキル - スキルタイプ封印]
        /// </summary>
        public const int TraitStypeSeal = 42;
        /// <summary>
        /// [スキル - スキル追加]
        /// </summary>
        public const int TraitSkillAdd = 43;
        /// <summary>
        /// [スキル - スキル封印]
        /// </summary>
        public const int TraitSkillSeal = 44;
        /// <summary>
        /// [装備 - 武器タイプ装備]
        /// </summary>
        public const int TraitEquipWeaponType = 51;
        /// <summary>
        /// [装備 - 防具タイプ装備]
        /// </summary>
        public const int TraitEquipArmorType = 52;
        /// <summary>
        /// [装備 - 装備固定]
        /// </summary>
        public const int TraitEquipLock = 53;
        /// <summary>
        /// [装備 - 装備封印]
        /// </summary>
        public const int TraitEquipSeal = 54;
        /// <summary>
        /// [装備 - スロットタイプ]
        /// </summary>
        public const int TraitSlotType = 55;
        /// <summary>
        /// [その他 - 行動回数追加]
        /// </summary>
        public const int TraitActionPlus = 61;
        /// <summary>
        /// [その他 - 特殊フラグ]
        /// </summary>
        public const int TraitSpecialFlag = 62;
        /// <summary>
        /// [その他 - 消滅エフェクト]
        /// </summary>
        public const int TraitCollapseType = 63;
        /// <summary>
        /// [その他 - パーティ能力]
        /// </summary>
        public const int TraitPartyAbility = 64;
        /// <summary>
        /// 特殊フラグIDの[自動戦闘]
        /// </summary>
        public const int FlagIDAutoBattle = 0;
        /// <summary>
        /// 特殊フラグIDの[防御]
        /// </summary>
        public const int FlagIDGuard      = 1;
        /// <summary>
        /// 特殊フラグIDの[身代わり]
        /// </summary>
        public const int FlagIDSubstitute = 2;
        /// <summary>
        /// 特殊フラグIDの[TP持ち越し]
        /// </summary>
        public const int FlagIDPreserveTp = 3;
        /// <summary>
        /// 能力強化アイコンの開始位置
        /// </summary>
        public const int IconBuffStart    = 32;
        /// <summary>
        /// 能力弱体アイコンの開始位置
        /// </summary>
        public const int IconDebuffStart  = 48;

        /// <summary>
        /// HP
        /// </summary>
        public virtual int Hp { get; set; }
        /// <summary>
        /// MP
        /// </summary>
        public virtual int Mp { get; set; }
        /// <summary>
        /// TP
        /// </summary>
        public virtual int Tp { get; set; }
        /// <summary>
        /// 最大HP
        /// </summary>
        public int Mhp => Param(0);
        /// <summary>
        /// 最大MP
        /// </summary>
        public int Mmp => Param(1);
        /// <summary>
        /// 最大TP
        /// </summary>
        public const int Mtp = 100;
        /// <summary>
        /// 攻撃力
        /// </summary>
        public int Atk => Param(2);
        /// <summary>
        /// 防御力
        /// </summary>
        public int Def => Param(3);
        /// <summary>
        /// 魔法力
        /// </summary>
        public int Mat => Param(4);
        /// <summary>
        /// 魔法防御力
        /// </summary>
        public int Mdf => Param(5);
        /// <summary>
        /// 俊敏性
        /// </summary>
        public int Agi => Param(6);
        /// <summary>
        /// 運
        /// </summary>
        public int Luk => Param(7);
        /// <summary>
        /// 命中率
        /// </summary>
        public double Hit => Xparam(0);
        /// <summary>
        /// 回避率
        /// </summary>
        public double Eva => Xparam(1);
        /// <summary>
        /// 会心率
        /// </summary>
        public double Cri => Xparam(2);
        /// <summary>
        /// 会心回避率
        /// </summary>
        public double Cev => Xparam(3);
        /// <summary>
        /// 魔法会心率
        /// </summary>
        public double Mev => Xparam(4);
        /// <summary>
        /// 魔法反射率
        /// </summary>
        public double Mrf => Xparam(5);
        /// <summary>
        /// 反撃率
        /// </summary>
        public double Cnt => Xparam(6);
        /// <summary>
        /// HP回復率
        /// </summary>
        public double Hrg => Xparam(7);
        /// <summary>
        /// MP回復率
        /// </summary>
        public double Mrg => Xparam(8);
        /// <summary>
        /// TP回復率
        /// </summary>
        public double Trg => Xparam(9);
        /// <summary>
        /// 狙われ率
        /// </summary>
        public double Tgr => Sparam(0);
        /// <summary>
        /// 防御率
        /// </summary>
        public double Grd => Sparam(1);
        /// <summary>
        /// 回復率
        /// </summary>
        public double Rec => Sparam(2);
        /// <summary>
        /// 薬効果率
        /// </summary>
        public double Pha => Sparam(3);
        /// <summary>
        /// MP消費率
        /// </summary>
        public double Mcr => Sparam(4);
        /// <summary>
        /// TPチャージ率
        /// </summary>
        public double Tcr => Sparam(5);
        /// <summary>
        /// 物理ダメージ率
        /// </summary>
        public double Pdr => Sparam(6);
        /// <summary>
        /// 魔法ダメージ率
        /// </summary>
        public double Mdr => Sparam(7);
        /// <summary>
        /// 床ダメージ率
        /// </summary>
        public double Fdr => Sparam(8);
        /// <summary>
        /// 経験値率
        /// </summary>
        public double Exr => Sparam(9);
        /// <summary>
        /// 隠れているか
        /// </summary>
        public bool Hidden { get; set; }
        /// <summary>
        /// 能力値強化量の配列
        /// </summary>
        public List<int> _paramPlus { get; private set; }
        /// <summary>
        /// ステートIDの配列
        /// </summary>
        public List<StateDataModel> States { get; private set; } = new List<StateDataModel>();
        /// <summary>
        /// ステートの残りターン
        /// </summary>
        public Dictionary<string, int> StateTurns { get; private set; }
        /// <summary>
        /// 能力の強化の配列
        /// </summary>
        public List<int> Buffs { get; private set; }
        /// <summary>
        /// 強化の残りターン
        /// </summary>
        public List<int> BuffTurns { get; private set; }
        /// <summary>
        /// 属性（Uniteで追加）
        /// </summary>
        public List<int> MyElement { get; set; }

        /// <summary>
        /// 逃走済みかどうか
        /// </summary>
        public bool IsEscaped { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GameBattlerBase() {
            this.InitMembers();
        }

        /// <summary>
        /// メンバ変数を初期化
        /// </summary>
        public virtual void InitMembers() {
            Hp = 1;
            Mp = 0;
            Tp = 0;
            Hidden = false;
            IsEscaped = false;
            ClearParamPlus();
            ClearStates();
            ClearBuffs();
        }

        /// <summary>
        /// 能力強化量を戻す
        /// </summary>
        protected void ClearParamPlus() {
            _paramPlus = new List<int>();
            for (var i = 0; i < 9; i++) _paramPlus.Add(0);
        }

        /// <summary>
        /// ステート変化を戻す
        /// </summary>
        public virtual void ClearStates() {
            States = new List<StateDataModel>();
            StateTurns = new Dictionary<string, int>();
        }

        /// <summary>
        /// ステート変化を戻す（バトル終了時）
        /// </summary>
        protected virtual void ClearStatesEndBattle() {
            //バトル終了時は、ステートが「バトル」のものを初期化する
            for (int i = 0; i < States.Count; i++)
            {
                if (States[i].stateOn == 0)
                {
                    StateTurns.Remove(States[i].id);
                    States.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// 指定ステートを消す
        /// </summary>
        /// <param name="stateId"></param>
        public virtual void EraseState(string stateId) {
            States.RemoveAll(state => state.id == stateId);
            StateTurns.Remove(stateId);
        }

        /// <summary>
        /// 指定したステートか
        /// </summary>
        /// <param name="stateId"></param>
        /// <returns></returns>
        public virtual bool IsStateAffected(string stateId) {
            for (int i = 0; i < States.Count; i++)
                if (States[i].id == stateId)
                    return true;
            return false;
        }

        /// <summary>
        /// 指定したステートか
        /// </summary>
        /// <param name="stateId"></param>
        /// <returns></returns>
        public virtual bool IsStateAffected(int stateId) {
            for (int i = 0; i < States.Count; i++)
                if (States[i].SerialNumber == stateId + 1)
                    return true;
            return false;
        }

        /// <summary>
        /// 死亡ステートか
        /// </summary>
        /// <returns></returns>
        public bool IsDeathStateAffected() {
            return IsStateAffected(DeathStateId);
        }

        /// <summary>
        /// 指定ステートの有効ターン数を初期化
        /// </summary>
        /// <param name="stateId"></param>
        public virtual void ResetStateCounts(string stateId) {
            var state = DataManager.Self().GetStateDataModel(stateId);
            var variance = 1 + Math.Max(state.maxTurns - state.minTurns, 0);
            StateTurns[stateId] = state.minTurns + new Random().Next(0, variance);
        }

        /// <summary>
        /// 指定ステートが切れているか
        /// </summary>
        /// <param name="stateId"></param>
        /// <returns></returns>
        public virtual bool IsStateExpired(string stateId) {
            return StateTurns[stateId] == 0;
        }

        /// <summary>
        /// ステート変化のアップデート
        /// </summary>
        public virtual void UpdateStateTurns() {
            States.ForEach(state =>
            {
                if (StateTurns[state.id] > 0) StateTurns[state.id]--;
            });
        }

        /// <summary>
        /// 能力[強化]を戻す
        /// </summary>
        protected virtual void ClearBuffs() {
            Buffs = new List<int>();
            BuffTurns = new List<int>();
            for (var i = 0; i < 8; i++)
            {
                Buffs.Add(0);
                BuffTurns.Add(0);
            }
        }

        /// <summary>
        /// 指定通常能力の[強化]を消す
        /// </summary>
        /// <param name="paramId"></param>
        public virtual void EraseBuff(int paramId) {
            Buffs[paramId] = 0;
            BuffTurns[paramId] = 0;
        }

        /// <summary>
        /// 設定されているバフ数返却
        /// </summary>
        /// <returns></returns>
        public virtual int BuffLength() {
            return Buffs.Count;
        }

        /// <summary>
        /// 指定通常能力値に[強化]がかかっているか
        /// </summary>
        /// <param name="paramId"></param>
        /// <returns></returns>
        public virtual bool IsBuffAffected(int paramId) {
            return Buffs[paramId] > 0;
        }

        /// <summary>
        /// 指定通常能力値が[弱体]されているか
        /// </summary>
        /// <param name="paramId"></param>
        /// <returns></returns>
        public virtual bool IsDebuffAffected(int paramId) {
            return Buffs[paramId] < 0;
        }

        /// <summary>
        /// 指定通常能力値が[強化][弱体]されているか
        /// </summary>
        /// <param name="paramId"></param>
        /// <returns></returns>
        public virtual bool IsBuffOrDebuffAffected(int paramId) {
            return Buffs[paramId] != 0;
        }

        /// <summary>
        /// 指定通常能力値が最[強化]されているか
        /// </summary>
        /// <param name="paramId"></param>
        /// <returns></returns>
        public virtual bool IsMaxBuffAffected(int paramId) {
            return Buffs[paramId] == 2;
        }

        /// <summary>
        /// 指定通常能力値が最[弱体]されているか
        /// </summary>
        /// <param name="paramId"></param>
        /// <returns></returns>
        public virtual bool IsMaxDebuffAffected(int paramId) {
            return Buffs[paramId] == -2;
        }

        /// <summary>
        /// 指定通常能力を[強化]
        /// </summary>
        /// <param name="paramId"></param>
        public virtual void IncreaseBuff(int paramId) {
            if (!IsMaxBuffAffected(paramId)) Buffs[paramId]++;
        }

        /// <summary>
        /// 指定通常能力の[強化]を減少させる
        /// </summary>
        /// <param name="paramId"></param>
        public virtual void DecreaseBuff(int paramId) {
            if (!IsMaxDebuffAffected(paramId)) Buffs[paramId]--;
        }

        /// <summary>
        /// 通常能力[強化]の有効ターンを追加
        /// </summary>
        /// <param name="paramId"></param>
        /// <param name="turns"></param>
        public virtual void OverwriteBuffTurns(int paramId, int turns) {
            if (BuffTurns[paramId] < turns) BuffTurns[paramId] = turns;
        }

        /// <summary>
        /// 指定通常能力値の[強化]が切れているか
        /// </summary>
        /// <param name="paramId"></param>
        /// <returns></returns>
        public virtual bool IsBuffExpired(int paramId) {
            return BuffTurns[paramId] == 0;
        }

        /// <summary>
        /// 通常能力値[強化]のアップデート
        /// </summary>
        public virtual void UpdateBuffTurns() {
            for (var i = 0; i < BuffTurns.Count; i++)
                if (BuffTurns[i] > 0)
                    BuffTurns[i]--;
        }

        /// <summary>
        /// バトラーを死亡ステートにする
        /// </summary>
        public void Die() {
            Hp = 0;
            ClearStates();
            ClearBuffs();
        }

        /// <summary>
        /// 復活
        /// </summary>
        public virtual void Revive() {
            if (Hp == 0) Hp = 1;
        }

        /// <summary>
        /// ステートのアイコン番号を配列で返す
        /// </summary>
        /// <returns></returns>
        public List<string> StateIcons() {
            var icons = new List<string>();
            for (int i = 0; i < States.Count; i++)
            {
                icons.Add(States[i].iconId + ".png");
            }
            return icons;
        }

        /// <summary>
        /// 付加中の[強化]アイコン番号を配列で返す
        /// </summary>
        /// <returns></returns>
        public List<string> BuffIcons() {
            var icons = new List<string>();
            for (var i = 0; i < Buffs.Count; i++)
                if (Buffs[i] != 0)
                    icons.Add(BuffIconIndex(Buffs[i], i));

            return icons;
        }

        /// <summary>
        /// [強化]アイコンの番号を返す
        /// </summary>
        /// <param name="buffLevel"></param>
        /// <param name="paramId"></param>
        /// <returns></returns>
        public string BuffIconIndex(int buffLevel, int paramId) {
            if (buffLevel > 0)
                return "IconSet_" + (IconBuffStart + (buffLevel - 1) * 8 + paramId).ToString("000") + ".png";
            if (buffLevel < 0)
                return "IconSet_" + (IconDebuffStart + (-buffLevel - 1) * 8 + paramId).ToString("000") + ".png";
            return "";
        }

        /// <summary>
        /// 全アイコン画像を配列で返す
        /// MVではアイコン番号だったが、Uniteではステート画像を任意に設定可能なため、画像ファイル名を返却する
        /// </summary>
        /// <returns></returns>
        public List<string> AllIcons() {
            var icons = new List<string>();
            var stateIcons = StateIcons();
            var buffIcons = BuffIcons();
            for (int i = 0; i < stateIcons.Count; i++)
            {
                icons.Add(stateIcons[i]);
            }
            for (int i = 0; i < buffIcons.Count; i++)
            {
                icons.Add(buffIcons[i]);
            }
            return icons;
        }

        /// <summary>
        /// 特徴オブジェクトを配列で返す
        /// </summary>
        /// <returns></returns>
        public virtual List<TraitCommonDataModel> TraitObjects() {
            //現在のステートに付加されている特徴を返却
            List<TraitCommonDataModel> traitList = new List<TraitCommonDataModel>();
            foreach (var state in States)
                foreach (var trait in state.traits)
                    if (!traitList.Contains(trait))
                        traitList.Add(trait);

            return traitList;
        }

        /// <summary>
        /// 全特徴を配列で返す
        /// </summary>
        /// <returns></returns>
        public virtual List<TraitCommonDataModel> AllTraits() {
            var ret = new List<TraitCommonDataModel>();
            foreach (var traitObject in TraitObjects())
            {
                var newTraits = ConvertUniteData.SetTraitCode(traitObject);
                ret.Add(newTraits);
            }

            return ret;
        }

        /// <summary>
        /// 指定特徴コード(TRAIT_定数)の特徴を返す
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public virtual IEnumerable<TraitCommonDataModel> Traits(int code) {
            return AllTraits().Where(trait => trait.categoryId == code);
        }

        /// <summary>
        /// 指定特徴コード(TRAIT_定数)・IDの特徴を配列で返す
        /// </summary>
        /// <param name="code"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual IEnumerable<TraitCommonDataModel> TraitsWithId(int code, int id) {
            return AllTraits().Where(trait => trait.categoryId == code && trait.effectId == id);
        }

        /// <summary>
        /// 指定特徴コード(TRAIT_定数)・IDの値を返す
        /// </summary>
        /// <param name="code"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual float TraitsPi(int code, int id) {
            if (TraitsWithId(code, id).Count() == 0)
            {
                return 1;
            }
            return TraitsWithId(code, id).Aggregate((float) 1, (current, trait) => (trait.value / 1000.0f));
        }

        /// <summary>
        /// 指定特徴コード(TRAIT_定数)・IDの特徴を足し合わせて返す
        /// </summary>
        /// <param name="code"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual float TraitsSumPi(int code, int id) {
            return TraitsWithId(code, id).Aggregate((float) 0, (current, trait) => current + trait.value) / 1000.0f;
        }

        /// <summary>
        /// 指定特徴コード(TRAIT_定数)の特徴を積算して返す
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public virtual float TraitsSumAll(int code) {
            if (code == TraitAttackTimes)
            {
                return Traits(code).Aggregate((float) 0, (current, trait) => current + trait.value);
            }
            else
            {
                return Traits(code).Aggregate((float) 0, (current, trait) => current + (trait.value / 10f));
            }
        }

        /// <summary>
        /// 指定特徴コード(TRAIT_定数)の、特徴IDの配列を返す
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public virtual List<int> TraitsSet(int code) {
            var ret = new List<int>();
            foreach (var trait in Traits(code)) ret.Add(trait.effectId);
            return ret;
        }

        /// <summary>
        /// 指定通常能力値の基本値を返す
        /// </summary>
        /// <param name="paramId"></param>
        /// <returns></returns>
        public virtual int ParamBase(int paramId) {
            return 0;
        }

        /// <summary>
        /// 指定通常能力値に加算される値を返す
        /// </summary>
        /// <param name="paramId"></param>
        /// <returns></returns>
        public virtual int ParamPlus(int paramId) {
            return _paramPlus[paramId];
        }

        /// <summary>
        /// 指定通常能力値の最小値を返す
        /// </summary>
        /// <param name="paramId"></param>
        /// <returns></returns>
        public virtual int ParamMin(int paramId) {
            //各パラメーターの最低値は0
            if (paramId == 1)
            {
                //MMP
                return 0;
            }
            else
            {
                //Other
                return 1;
            }
        }

        /// <summary>
        /// 指定通常能力値の最大値を返す
        /// </summary>
        /// <param name="paramId"></param>
        /// <returns></returns>
        public virtual int ParamMax(int paramId) {
            if (paramId == 0)
            {
                //MHP
                return 999999;
            }
            if (paramId == 1)
            {
                //MMP
                return 9999;
            }

            //MVでは999だったが、Uniteでは9999
            //return 999;
            return 9999;
        }

        /// <summary>
        /// 指定した[能力値 - 通常能力値]の値を返す
        /// </summary>
        /// <param name="paramId"></param>
        /// <returns></returns>
        public virtual float ParamRate(int paramId) {
            return TraitsPi(TraitParam, paramId);
        }

        /// <summary>
        /// 指定通常能力値の[強化]率を返す
        /// </summary>
        /// <param name="paramId"></param>
        /// <returns></returns>
        public virtual double ParamBuffRate(int paramId) {
            return Buffs[paramId] * 0.25 + 1.0;
        }

        /// <summary>
        /// パラメータを返却
        /// </summary>
        /// <param name="paramId"></param>
        /// <returns></returns>
        public virtual int Param(int paramId) {
            double value = ParamBase(paramId) + ParamPlus(paramId);
            value *= ParamRate(paramId) * ParamBuffRate(paramId);
            var maxValue = ParamMax(paramId);
            var minValue = ParamMin(paramId);

            return (int) Math.Round(Math.Min(Math.Max(value, minValue), maxValue));
        }

        /// <summary>
        /// 指定した[能力値 - 追加能力値] の値を返す
        /// </summary>
        /// <param name="xparamId"></param>
        /// <returns></returns>
        public virtual double Xparam(int xparamId) {
            return TraitsSumPi(TraitXparam, xparamId);
        }

        /// <summary>
        /// 指定した[能力値 - 特殊能力値]の値を返す
        /// </summary>
        /// <param name="sparamId"></param>
        /// <returns></returns>
        public virtual double Sparam(int sparamId) {
            return TraitsPi(TraitSparam, ConvertUniteData.SetSparam(sparamId));
        }

        /// <summary>
        /// 指定の属性に対する[耐性 - 属性有効度]を返す
        /// </summary>
        /// <param name="elementId"></param>
        /// <returns></returns>
        public virtual float ElementRate(int elementId) {
            return TraitsPi(TraitElementRate, elementId);
        }

        /// <summary>
        /// 指定した能力値に対する[耐性 - 弱体有効度] を返す
        /// </summary>
        /// <param name="paramId"></param>
        /// <returns></returns>
        public virtual float DebuffRate(int paramId) {
            return TraitsPi(TraitDebuffRate, paramId);
        }

        /// <summary>
        /// 指定したステートに対する[耐性 - ステート有効度]を返す
        /// </summary>
        /// <param name="stateId"></param>
        /// <returns></returns>
        public virtual float StateRate(int stateId) {
            return TraitsPi(TraitStateRate, stateId);
        }

        /// <summary>
        /// [耐性 - ステート無効化]に対応する、ステートIDの配列を返す
        /// </summary>
        /// <returns></returns>
        public virtual List<int> StateResistSet() {
            return TraitsSet(TraitStateResist);
        }

        /// <summary>
        /// 指定ステートが無効化されているか
        /// </summary>
        /// <param name="stateId"></param>
        /// <returns></returns>
        public virtual bool IsStateResist(int stateId) {
            return StateResistSet().Contains(stateId);
        }

        /// <summary>
        /// [攻撃時属性]の配列を返す
        /// </summary>
        /// <returns></returns>
        public virtual List<int> AttackElements() {
            return TraitsSet(TraitAttackElement);
        }

        /// <summary>
        /// 攻撃ステートIDを配列で返す
        /// </summary>
        /// <returns></returns>
        public virtual List<int> AttackStates() {
            return TraitsSet(TraitAttackState);
        }

        /// <summary>
        /// 指定攻撃ステートの付加率を返す
        /// </summary>
        /// <param name="stateId"></param>
        /// <returns></returns>
        public virtual float AttackStatesRate(int stateId) {
            return TraitsSumPi(TraitAttackState, stateId);
        }

        /// <summary>
        /// [攻撃 - 攻撃速度補正] を返す
        /// </summary>
        /// <returns></returns>
        public virtual float AttackSpeed() {
            return TraitsSumAll(TraitAttackSpeed);
        }

        /// <summary>
        /// [攻撃 - 攻撃追加回数]を返す
        /// </summary>
        /// <returns></returns>
        public virtual double AttackTimesAdd() {
            return Math.Max(TraitsSumAll(TraitAttackTimes), 0);
        }
        
        /// <summary>
        /// [攻撃 - 攻撃スキル]を返す
        /// </summary>
        /// <returns></returns>
        public virtual int AttackSkill() {
            if (TraitsSet(TraitAttackSkill).Count > 0)
                return TraitsSet(TraitAttackSkill).Last();
            return 0;
        }
        
        /// <summary>
        /// 指定スキルタイプが [スキル - スキルタイプ封印]されているか
        /// </summary>
        /// <param name="stypeId"></param>
        /// <returns></returns>
        public virtual bool IsSkillTypeSealed(int stypeId) {
            return TraitsSet(TraitStypeSeal).Contains(stypeId);
        }

        /// <summary>
        /// 指定スキルタイプが[スキル - スキル封印]か
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        public virtual bool IsSkillSealed(int skillId) {
            return TraitsSet(TraitSkillSeal).Contains(skillId);
        }

        /// <summary>
        /// 指定装備タイプが [装備 - 装備固定]か
        /// </summary>
        /// <param name="etypeId"></param>
        /// <returns></returns>
        public virtual bool IsEquipTypeLocked(int etypeId) {
            return TraitsSet(TraitEquipLock).Contains(etypeId);
        }

        /// <summary>
        /// 指定装備タイプが [装備 - 装備封印]か
        /// </summary>
        /// <param name="etypeId"></param>
        /// <returns></returns>
        public virtual bool IsEquipTypeSealed(int etypeId) {
            return TraitsSet(TraitEquipSeal).Contains(etypeId);
        }

        /// <summary>
        /// [装備 - スロットタイプ]を返す
        /// </summary>
        /// <returns></returns>
        public virtual int SlotType() {
            var set = TraitsSet(TraitSlotType);
            return set.Count > 0 ? set.Max() : 0;
        }

        /// <summary>
        /// 二刀流か
        /// </summary>
        /// <returns></returns>
        public virtual bool IsDualWield() {
            return SlotType() == 1;
        }

        /// <summary>
        /// 行動回数追加
        /// </summary>
        /// <returns></returns>
        public virtual List<float> ActionPlusSet() {
            return Traits(TraitActionPlus).Aggregate(new List<float>(), (ret, trait) =>
            {
                ret.Add(trait.value);
                return ret;
            });
        }

        /// <summary>
        /// 特徴が付与されているかどうかを返却する
        /// </summary>
        /// <param name="flagId"></param>
        /// <returns></returns>
        public virtual bool SpecialFlag(int flagId) {
            return Traits(TraitSpecialFlag).Any(trait => trait.effectId == flagId);
        }

        /// <summary>
        /// [その他 - 消滅エフェクト]を返す
        /// </summary>
        /// <returns></returns>
        public virtual int CollapseType() {
            var set = TraitsSet(TraitCollapseType);
            return set.Count > 0 ? set.Max() : 0;
        }

        /// <summary>
        /// 指定パーティ能力が[その他 - パーティ能力]か
        /// </summary>
        /// <param name="abilityId"></param>
        /// <returns></returns>
        public virtual bool PartyAbility(int abilityId) {
            return Traits(TraitPartyAbility).Any(trait => trait.effectId == abilityId);
        }

        /// <summary>
        /// [自動戦闘]か
        /// </summary>
        /// <returns></returns>
        public virtual bool IsAutoBattle() {
            return SpecialFlag(FlagIDAutoBattle);
        }

        /// <summary>
        /// 防御中か
        /// </summary>
        /// <returns></returns>
        public virtual bool IsGuard() {
            return SpecialFlag(FlagIDGuard) && CanMove();
        }

        /// <summary>
        /// [身代わり]ステートか
        /// </summary>
        /// <returns></returns>
        public virtual bool IsSubstitute() {
            return SpecialFlag(FlagIDSubstitute) && CanMove();
        }

        /// <summary>
        /// [TP持ち越し]か
        /// </summary>
        /// <returns></returns>
        public virtual bool IsPreserveTp() {
            return SpecialFlag(FlagIDPreserveTp);
        }

        /// <summary>
        /// 指定能力に指定した値を追加
        /// </summary>
        /// <param name="paramId"></param>
        /// <param name="value"></param>
        public virtual void AddParam(int paramId, int value) {
            _paramPlus[paramId] += value;
            Refresh();
        }

        /// <summary>
        /// HPを設定
        /// </summary>
        /// <param name="data"></param>
        public virtual void SetHp(int data) {
            Hp = data;
            if (Mhp <= Hp) Hp = Mhp;
            else if (Hp < 0) Hp = 0;
        }

        /// <summary>
        /// MPを設定
        /// </summary>
        /// <param name="data"></param>
        public virtual void SetMp(int data) {
            Mp = data;
            if (Mmp <= Mp) Mp = Mmp;
            else if (Mp < 0) Mp = 0;
        }

        /// <summary>
        /// TPを設定
        /// </summary>
        /// <param name="data"></param>
        public virtual void SetTp(int data) {
            Tp = data;
            if (Mtp <= Tp) Tp = Mtp;
            else if (Tp < 0) Tp = 0;
        }

        /// <summary>
        /// 能力値やステートを規定値内に収める処理
        /// </summary>
        public virtual void Refresh() {
            List<int> stateResistSetWork = new List<int>();
            for (int i = 0; i < StateResistSet().Count; i++)
            {
                stateResistSetWork.Add(StateResistSet()[i]);
            }

            List<StateDataModel> statesWork = new List<StateDataModel>();
            for (int i = 0; i < States.Count; i++)
            {
                statesWork.Add(States[i]);
            }

            for (int i = 0; i < stateResistSetWork.Count; i++)
            {
                for (int j = 0; j < statesWork.Count; j++)
                {
                    if (statesWork[j].SerialNumber == stateResistSetWork[i] + 1)
                    {
                        EraseState(statesWork[j].id);
                    }
                }
            }

            Hp = Math.Min(Math.Max(Hp, 0), Mhp);
            Mp = Math.Min(Math.Max(Mp, 0), Mmp);
            Tp = Math.Min(Math.Max(Tp, 0), Mtp);
        }

        /// <summary>
        /// HP・MP全回復しステートを解除
        /// </summary>
        public virtual void RecoverAll() {
            ClearStates();
            Hp = Mhp;
            Mp = Mmp;
        }

        /// <summary>
        /// HPのパーセント量を返す
        /// </summary>
        /// <returns></returns>
        public virtual double HpRate() {
            return 1f * Hp / Mhp;
        }

        /// <summary>
        /// MPのパーセント量を返す
        /// </summary>
        /// <returns></returns>
        public virtual double MpRate() {
            return Mmp > 0 ? 1f * Mp / Mmp : 0;
        }

        /// <summary>
        /// バトラーを隠す
        /// </summary>
        public virtual void Hide() {
            Hidden = true;
        }

        /// <summary>
        /// バトラーを出現させる
        /// </summary>
        public virtual void Appear() {
            //既に逃走済みの場合には表示しない
            Hidden = false;
        }

        /// <summary>
        /// 隠れているか
        /// </summary>
        /// <returns></returns>
        public virtual bool IsHidden() {
            return Hidden;
        }

        /// <summary>
        /// 現れているか
        /// </summary>
        /// <returns></returns>
        public virtual bool IsAppeared() {
            return !IsHidden();
        }

        /// <summary>
        /// 表示されて死亡ステートか
        /// </summary>
        /// <returns></returns>
        public virtual bool IsDead() {
            return IsAppeared() && IsDeathStateAffected();
        }

        /// <summary>
        /// 
        /// 生きているか
        /// </summary>
        /// <returns></returns>
        public virtual bool IsAlive() {
            return IsAppeared() && !IsDeathStateAffected();
        }

        /// <summary>
        /// 瀕死(規定値:最大HPの1/4以下)か
        /// </summary>
        /// <returns></returns>
        public virtual bool IsDying() {
            return IsAlive() && Hp < Mhp / 4;
        }

        /// <summary>
        /// 行動制約があるかどうか
        /// </summary>
        /// <returns></returns>
        public virtual bool IsRestricted() {
            return IsAppeared() && Restriction() > 0;
        }

        /// <summary>
        /// 行動の入力可能か
        /// </summary>
        /// <returns></returns>
        public virtual bool CanInput() {
            return IsAppeared() && !IsRestricted() && !IsAutoBattle();
        }

        /// <summary>
        /// 動作可能か
        /// </summary>
        /// <returns></returns>
        public virtual bool CanMove() {
            return IsAppeared() && Restriction() < 4;
        }

        /// <summary>
        /// 混乱しているか
        /// </summary>
        /// <returns></returns>
        public virtual bool IsConfused() {
            return IsAppeared() && Restriction() >= 1 && Restriction() <= 3;
        }

        /// <summary>
        /// 混乱レベルを返す
        /// </summary>
        /// <returns></returns>
        public virtual int ConfusionLevel() {
            return IsConfused() ? Restriction() : 0;
        }

        /// <summary>
        /// アクターか
        /// </summary>
        /// <returns></returns>
        public virtual bool IsActor() {
            return false;
        }

        /// <summary>
        /// 敵か
        /// </summary>
        /// <returns></returns>
        public virtual bool IsEnemy() {
            return false;
        }

        /// <summary>
        /// 優先度でステートの並び替え
        /// </summary>
        public virtual void SortStates() {
            States.Sort((a, b) => (int) b.priority - (int) a.priority);
        }

        /// <summary>
        /// 行動制約の状態を示す値を返す
        /// </summary>
        /// <returns></returns>
        public virtual int Restriction() {
            return States.Select(state => state?.restriction)?.ToList()?.Max() ?? 0;
        }

        /// <summary>
        /// 新たなステートを追加
        /// </summary>
        /// <param name="stateId"></param>
        public virtual void AddNewState(string stateId) {
            if (stateId == DeathStateId) Die();

            var restricted = IsRestricted();
            States.Add(DataManager.Self().GetStateDataModel(stateId));
            SortStates();
            if (!restricted && IsRestricted())
            {
                OnRestrict();
            }

            //新たなステートを追加したとき、ステート無効の特徴がついている可能性があるため、Refreshを実施
            Refresh();
        }

        /// <summary>
        /// 行動制約された時に呼ばれるハンドラ
        /// overrideして利用する
        /// </summary>
        public virtual void OnRestrict() {
        }

        /// <summary>
        /// 現在のステートを表すメッセージ文字列を返す
        /// </summary>
        /// <returns></returns>
        public virtual string MostImportantStateText() {
            var states = States;
            for (var i = 0; i < states.Count; i++)
                if (states[i].message3 != "")
                    return states[i].message3;

            return "";
        }

        /// <summary>
        /// SVでのステートの動作番号を返す
        /// </summary>
        /// <returns></returns>
        public virtual int StateMotionIndex() {
            var states = States;
            if (states.Count > 0)
            {
                int motion = 0;
                for (int i = 0; i < states.Count; i++) 
                {
                    if (motion < states[i].motion)
                    {
                        motion = states[i].motion;
                    }
                }

                return motion;
            }
            return 0;
        }

        /// <summary>
        /// 指定スキルの発動条件に合う装備をしているか
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual bool IsSkillWtypeOk(GameItem item) {
            return true;
        }

        /// <summary>
        /// 指定スキルに必要なMPを返す
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual int SkillMpCost(GameItem item) {
            return (int) Math.Floor(item.MpCost * Mcr);
        }

        /// <summary>
        /// 指定スキルに必要なTPを返す
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual int SkillTpCost(GameItem item) {
            return item.TpCost;
        }

        /// <summary>
        /// 指定スキルの動作可能か
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual bool CanPaySkillCost(GameItem item) {
            return Tp >= SkillTpCost(item) && Mp >= SkillMpCost(item);
        }

        /// <summary>
        /// スキルに必要なコスト(MP・TP)を消費
        /// </summary>
        /// <param name="item"></param>
        public virtual void PaySkillCost(GameItem item) {
            Mp -= SkillMpCost(item);
            Tp -= SkillTpCost(item);
        }

        /// <summary>
        /// 指定アイテムが使用可能か
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual bool IsOccasionOk(GameItem item) {
            if (DataManager.Self().GetGameParty().InBattle())
                return item.Occasion == 0 || item.Occasion == 1;
            return item.Occasion == 0 || item.Occasion == 2;
        }

        /// <summary>
        /// 指定アイテムが使用可能か
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual bool MeetsUsableItemConditions(GameItem item) {
            return CanMove() && IsOccasionOk(item);
        }

        /// <summary>
        /// 指定スキルを使用可能か
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual bool MeetsSkillConditions(GameItem item) {
            var skill = DataManager.Self().GetSkillCustomDataModel(item.ItemId);
            return MeetsUsableItemConditions(item) &&
                   IsSkillWtypeOk(item) && CanPaySkillCost(item) &&
                   !IsSkillSealed(skill.SerialNumber - 1) && !IsSkillTypeSealed(item.STypeId);
        }

        /// <summary>
        /// 指定アイテムが使用可能か
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual bool MeetsItemConditions(GameItem item) {
            return MeetsUsableItemConditions(item) && DataManager.Self().GetGameParty().HasItem(item);
        }

        /// <summary>
        /// 指定アイテムを使用可能か
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual bool CanUse(GameItem item) {
            if (item == null)
            {
                return false;
            }
            if (item.IsSkill())
            {
                return MeetsSkillConditions(item);
            }
            if (item.IsItem())
            {
                return MeetsItemConditions(item);
            }
            return false;
        }

        /// <summary>
        /// 指定アイテムを装備可能か
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual bool CanEquip(GameItem item) {
            if (item == null)
                return false;
            if (item.IsWeapon())
                return CanEquipWeapon(item);
            if (item.IsArmor())
                return CanEquipArmor(item);
            return false;
        }

        /// <summary>
        /// 指定武器を装備可能か
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual bool CanEquipWeapon(GameItem item) {
            return true;
        }

        /// <summary>
        /// 指定防具を装備可能か
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual bool CanEquipArmor(GameItem item) {
            return true;
        }

        /// <summary>
        /// 攻撃可能か
        /// </summary>
        /// <returns></returns>
        public virtual bool CanAttack(string skillId = AttackSkillId) {
            return CanUse(new GameItem(skillId, GameItem.DataClassEnum.Skill));
        }

        /// <summary>
        /// 防御可能か
        /// </summary>
        /// <returns></returns>
        public virtual bool CanGuard() {
            return CanUse(new GameItem(GuardSkillId, GameItem.DataClassEnum.Skill));
        }

        //================================================================================
        // 以下はUniteで追加されたメソッド
        //================================================================================

        /// <summary>
        /// 追加パラメータ設定
        /// </summary>
        /// <param name="paramPlus"></param>
        protected void SetParamPlus(RuntimeActorDataModel.ParamPlus paramPlus) {
            //パラメータの内容をMVに合わせる
            //0:最大HP
            //1:最大MP
            //2:攻撃力
            //3:防御力
            //4:魔法力
            //5:魔法防御
            //6:俊敏性
            //7:運
            _paramPlus[0] = paramPlus.maxHp;
            _paramPlus[1] = paramPlus.maxMp;
            _paramPlus[2] = paramPlus.attack;
            _paramPlus[3] = paramPlus.defense;
            _paramPlus[4] = paramPlus.magicAttack;
            _paramPlus[5] = paramPlus.magicDefence;
            _paramPlus[6] = paramPlus.speed;
            _paramPlus[7] = paramPlus.luck;
        }


        /// <summary>
        /// 状態異常の番号から状態異常IDを返却
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public virtual string GetStateIdByNumber(int number) {
            var state = DataManager.Self().GetStateDataModels();
            string stateId = null;
            for (int i = 0; i < state.Count; i++)
                if (state[i].SerialNumber == number + 1)
                {
                    stateId = state[i].id;
                    break;
                }
            return stateId;
        }
    }
}