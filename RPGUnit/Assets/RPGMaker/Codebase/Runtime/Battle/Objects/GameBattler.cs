using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.Runtime.Battle.Wrapper;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Character;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGMaker.Codebase.Runtime.Battle.Objects
{
    /// <summary>
    /// 戦闘シーンでのアイコンやアニメーションを含む、バトラーの動作を制御する
    /// </summary>
    public class GameBattler : GameBattlerBase
    {
        /// <summary>
        /// アクションの状態定義
        /// </summary>
        public enum ActionStateEnum
        {
            Undecided,      //行動未決定
            Inputting,      //入力中
            Waiting,        //待ち状態
            Acting,         //行動中
            Done,
            Null
        }
        public string Id { get; set; }
        /// <summary>
        /// 行動の配列
        /// </summary>
        public List<GameAction> Actions { get; set; }
        /// <summary>
        /// 速度(行動順を決定する)
        /// </summary>
        public float Speed { get; set; }
        GameActionResult _result;
        /// <summary>
        /// 行動結果
        /// </summary>
        public GameActionResult Result {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
            }
        }
        /// <summary>
        /// アクション状態
        /// </summary>
        private ActionStateEnum _actionState;
        /// <summary>
        /// 最後の対象番号（直接指定用）
        /// </summary>
        public virtual int LastTargetIndex { get; set; }
        /// <summary>
        /// アニメーションの配列
        /// MVと異なり、パーティクルを再生可能なアニメーションの型としている
        /// </summary>
        private List<CharacterAnimationActor> _animations = new List<CharacterAnimationActor>();
        /// <summary>
        /// ダメージポップアップするか
        /// </summary>
        private bool _damagePopup;
        /// <summary>
        /// エフェクトタイプ
        /// </summary>
        private string _effectType;
        /// <summary>
        /// モーションタイプ
        /// </summary>
        private string _motionType;
        /// <summary>
        /// 武器画像ID
        /// </summary>
        private string _weaponImageId;
        /// <summary>
        /// モーションを更新するか
        /// </summary>
        private bool _motionRefresh;
        /// <summary>
        /// 選択されているか
        /// </summary>
        private bool _selected;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GameBattler() {
            this.InitMembers();
        }

        /// <summary>
        /// メンバ変数を初期化
        /// </summary>
        override public void InitMembers() {
            base.InitMembers();
            Actions = new List<GameAction>();
            Speed = 0;
            Result = new GameActionResult();
            _actionState = ActionStateEnum.Null;
            LastTargetIndex = 0;
            _animations = new List<CharacterAnimationActor>();
            _damagePopup = false;
            _effectType = null;
            _motionType = null;
            _weaponImageId = null;
            _motionRefresh = false;
            _selected = false;
        }

        /// <summary>
        /// アニメーションを消去
        /// </summary>
        public void ClearAnimations() {
            _animations.Clear();
        }

        /// <summary>
        /// ダメージポップアップを消去
        /// </summary>
        public void ClearDamagePopup() {
            _damagePopup = false;
        }

        /// <summary>
        /// 武器アニメーションを消去
        /// </summary>
        public void ClearWeaponAnimation() {
            _weaponImageId = null;
        }

        /// <summary>
        /// エフェクトを消去
        /// </summary>
        public void ClearEffect() {
            _effectType = null;
        }

        /// <summary>
        /// モーションを消去
        /// </summary>
        public void ClearMotion() {
            _motionType = null;
            _motionRefresh = false;
        }

        /// <summary>
        /// 指定エフェクトを要求
        /// </summary>
        /// <param name="effectType"></param>
        public void RequestEffect(string effectType) {
            _effectType = effectType;
        }

        /// <summary>
        /// 指定モーションを要求
        /// </summary>
        /// <param name="motionType"></param>
        public void RequestMotion(string motionType) {
            _motionType = motionType;
        }

        /// <summary>
        /// モーションの初期化を要求
        /// </summary>
        public void RequestMotionRefresh() {
            _motionRefresh = true;
        }

        /// <summary>
        /// バトラーの選択
        /// </summary>
        public void Select() {
            _selected = true;
        }

        /// <summary>
        /// 選択を外す
        /// </summary>
        public void Deselect() {
            _selected = false;
        }

        /// <summary>
        /// アニメーションが要求されているか
        /// </summary>
        /// <returns></returns>
        public bool IsAnimationRequested() {
            return _animations.Count > 0;
        }

        /// <summary>
        /// ダメージポップアップが要求されているか
        /// </summary>
        /// <returns></returns>
        public bool IsDamagePopupRequested() {
            return _damagePopup;
        }

        /// <summary>
        /// エフェクトが要求されているか
        /// </summary>
        /// <returns></returns>
        public bool IsEffectRequested() {
            return !string.IsNullOrEmpty(_effectType);
        }

        /// <summary>
        /// モーションが要求されているか
        /// </summary>
        /// <returns></returns>
        public bool IsMotionRequested() {
            return !string.IsNullOrEmpty(_motionType);
        }

        /// <summary>
        /// 武器アニメーションが要求されているか
        /// </summary>
        /// <returns></returns>
        public bool IsWeaponAnimationRequested() {
            return !string.IsNullOrEmpty(_weaponImageId);
        }

        /// <summary>
        /// モーションの初期化が要求されているか
        /// </summary>
        /// <returns></returns>
        public bool IsMotionRefreshRequested() {
            return _motionRefresh;
        }

        /// <summary>
        /// 選択されているか
        /// </summary>
        /// <returns></returns>
        public bool IsSelected() {
            return _selected;
        }

        /// <summary>
        /// エフェクトタイプを返す
        /// </summary>
        /// <returns></returns>
        public string EffectType() {
            return _effectType;
        }

        /// <summary>
        /// 行動タイプを返す
        /// </summary>
        /// <returns></returns>
        public string MotionType() {
            return _motionType;
        }

        /// <summary>
        /// 武器画像IDを返す
        /// </summary>
        /// <returns></returns>
        public string WeaponImageId() {
            return _weaponImageId;
        }

        /// <summary>
        /// 次のアニメーションを返す
        /// </summary>
        /// <returns></returns>
        public CharacterAnimationActor ShiftAnimation() {
            var ret = _animations.First();
            _animations.Remove(ret);
            return ret;
        }

        /// <summary>
        /// 指定アニメーション開始(追加)
        /// </summary>
        /// <param name="animationId"></param>
        /// <param name="mirror"></param>
        /// <param name="delay"></param>
        public virtual void StartAnimation(string animationId, bool mirror, float delay) {
            var data = new CharacterAnimationActor
            {
                animationId = animationId,
                mirror = mirror,
                delay = delay
            };
            _animations.Add(data);
        }

        /// <summary>
        /// ダメージポップアップ開始
        /// </summary>
        public virtual void StartDamagePopup() {
            _damagePopup = true;
        }

        /// <summary>
        /// 指定武器のアニメーション開始
        /// </summary>
        /// <param name="weaponImageId"></param>
        public virtual void StartWeaponAnimation(string weaponImageId) {
            _weaponImageId = weaponImageId;
        }

        /// <summary>
        /// 指定番号のアクションを返す
        /// </summary>
        /// <param name="index">バトラー番号</param>
        /// <returns></returns>
        public GameAction Action(int index) {
            return Actions[index];
        }

        /// <summary>
        /// 指定番号のバトラーにアクションを設定
        /// </summary>
        /// <param name="index">バトラー番号</param>
        /// <param name="action">アクション</param>
        public void SetAction(int index, GameAction action) {
            Actions[index] = action;
        }

        /// <summary>
        /// 行動番号を返す
        /// </summary>
        /// <returns></returns>
        public int NumActions() {
            return Actions.Count;
        }

        /// <summary>
        /// アクションを消去
        /// </summary>
        public virtual void ClearActions() {
            Actions.Clear();
        }

        /// <summary>
        /// 結果を初期化する
        /// </summary>
        public void ClearResult() {
            Result = new GameActionResult();
        }

        /// <summary>
        /// 能力値やステートを規定値内に収める処理
        /// </summary>
        public override void Refresh() {
            base.Refresh();
            if (Hp <= 0)
                AddState(DeathStateId);
            else
                RemoveState(DeathStateId);
        }

        /// <summary>
        /// 指定ステートを追加
        /// </summary>
        /// <param name="stateId"></param>
        public virtual bool AddState(string stateId) {
            if (IsStateAddable(stateId))
            {
                if (!IsStateAffected(stateId))
                {
                    var state = DataManager.Self().GetStateDataModel(stateId);

                    if (state.stateOn == 1)
                        AddNewMapState(stateId);
                    else
                        AddNewState(stateId);

                    // バトル以外の特徴設定
                    var actorDataModels = DataManager.Self().GetRuntimeSaveDataModel()?.runtimeActorDataModels;
                    for (int i = 0; i < actorDataModels?.Count; i++)
                        if (Id == actorDataModels[i].actorId)
                            ItemManager.SetTraits(state.traits, actorDataModels[i]);

                    Refresh();
                }

                ResetStateCounts(stateId);
                Result.PushAddedState(stateId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 指定マップ用ステートを追加
        /// </summary>
        /// <param name="stateId"></param>
        private void AddNewMapState(string stateId) {
            // 付与ステートデータ作成
            RuntimeActorDataModel.State item = new RuntimeActorDataModel.State();
            item.id = DataManager.Self().GetStateDataModel(stateId).id;
            item.walkingCount = 0;

            var actorDataModels = DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels;
            for (int i = 0; i < actorDataModels.Count; i++)
            {
                bool isFind = false;
                if (Id == actorDataModels[i].actorId)
                {
                    isFind = true;

                    // 追加済みか
                    bool isAdded = false;
                    for (int i2 = 0; i2 < actorDataModels[i].states.Count; i2++)
                        if (actorDataModels[i].states[i2].id == item.id)
                        {
                            isAdded = true;
                            break;
                        }

                    if (!isAdded)
                    {
                        actorDataModels[i].states.Add(item);
                        States.Add(DataManager.Self().GetStateDataModel(stateId));
                    }
                }

                if (isFind) break;
            }
        }

        /// <summary>
        /// 指定ステートが付加可能か
        /// </summary>
        /// <param name="stateId"></param>
        /// <returns></returns>
        public bool IsStateAddable(string stateId) {
            return IsAlive() && DataManager.Self().GetStateDataModel(stateId) != null &&
                    !IsStateResist(ConvertUniteData.StateUuidToSerialNo(stateId)) &&
                    !Result.IsStateRemoved(stateId) &&
                    !IsStateRestrict(stateId) &&
                    IsStateTiming(stateId);
        }

        /// <summary>
        /// 指定ステートが[行動制約によって解除]かつ、現在行動制約中か
        /// </summary>
        /// <param name="stateId"></param>
        /// <returns></returns>
        public bool IsStateRestrict(string stateId) {
            return (DataManager.Self().GetStateDataModel(stateId).stateOn == 2 &&
                    DataManager.Self().GetStateDataModel(stateId).removeByRestriction == 1 ||
                    DataManager.Self().GetStateDataModel(stateId).stateOn == 0 &&
                    DataManager.Self().GetStateDataModel(stateId).inBattleRemoveRestriction == 1) &&
                   IsRestricted();
        }

        /// <summary>
        /// 指定ステートが付与可能なタイミングか（バトル、マップ、常時）
        /// マップステートはバトル中も付与可能とする
        /// </summary>
        /// <param name="stateId"></param>
        /// <returns></returns>
        public bool IsStateTiming(string stateId) {
            return DataManager.Self().GetStateDataModel(stateId).stateOn == 0 &&GameStateHandler.IsBattle()||
                   DataManager.Self().GetStateDataModel(stateId).stateOn == 1 ||
                   DataManager.Self().GetStateDataModel(stateId).stateOn == 2;
        }

        /// <summary>
        /// 行動制約された時に呼ばれるハンドラ
        /// </summary>
        public override void OnRestrict() {
            base.OnRestrict();
            ClearActions();
            for (int i = 0; i < States.Count; i++)
            {
                if (States[i].stateOn == 2 && States[i].removeByRestriction == 1 ||
                    States[i].stateOn == 0 && States[i].inBattleRemoveRestriction == 1)
                {
                    RemoveState(States[i].id);
                    i--;
                }
            }
        }

        /// <summary>
        /// 指定ステートを解除
        /// </summary>
        /// <param name="stateId"></param>
        public virtual bool RemoveState(string stateId) {
            if (IsStateAffected(stateId))
            {
                if (stateId == DeathStateId) Revive();

                EraseState(stateId);
                Refresh();
                Result.PushRemovedState(stateId);

                var actorDataModels = DataManager.Self().GetRuntimeSaveDataModel()?.runtimeActorDataModels;
                for (int i = 0; i < actorDataModels?.Count; i++)
                    if (Id == actorDataModels[i].actorId)
                    {
                        var state = DataManager.Self().GetStateDataModel(stateId);
                        ItemManager.SetTraits(state.traits, actorDataModels[i], remove : true);
                        actorDataModels[i].states.RemoveAll(s => s.id == stateId);
                    }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 戦闘から逃げる
        /// </summary>
        public void Escape() {
            if (DataManager.Self().GetGameParty().InBattle())
            {
                IsEscaped = true;
                Hide();
            }

            ClearActions();

            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE,
                DataManager.Self().GetSystemDataModel().soundSetting.escape);
            SoundManager.Self().PlaySe();
        }

        /// <summary>
        /// 指定通常能力に指定ターン数の[強化]を追加
        /// </summary>
        /// <param name="paramId"></param>
        /// <param name="turns"></param>
        public void AddBuff(int paramId, int turns) {
            if (IsAlive())
            {
                IncreaseBuff(paramId);
                if (IsBuffAffected(paramId)) OverwriteBuffTurns(paramId, turns);

                Result.PushAddedBuff(paramId);
                Refresh();
            }
        }

        /// <summary>
        /// 指定通常能力に指定ターン数の[弱体]を追加
        /// </summary>
        /// <param name="paramId"></param>
        /// <param name="turns"></param>
        public void AddDeBuff(int paramId, int turns) {
            if (IsAlive())
            {
                DecreaseBuff(paramId);
                if (IsDebuffAffected(paramId)) OverwriteBuffTurns(paramId, turns);

                Result.PushAddedDebuff(paramId);
                Refresh();
            }
        }

        /// <summary>
        /// 指定通常能力の[強化]を解除
        /// </summary>
        /// <param name="paramId"></param>
        public void RemoveBuff(int paramId) {
            if (IsAlive() && IsBuffOrDebuffAffected(paramId))
            {
                EraseBuff(paramId);
                Result.PushRemovedBuff(paramId);
                Refresh();
            }
        }

        /// <summary>
        /// ステートを解除
        /// </summary>
        public void RemoveBattleStates() {
            var toRemoveIds = new List<string>();
            States.ForEach(state =>
            {
                //ステートがバトル中のみ有効、またはバトル終了時に解除かどうか
                if (state.stateOn == 0 || state.removeAtBattleEnd == 1) toRemoveIds.Add(state.id);
            });

            for (int i = 0; i < toRemoveIds.Count; i++)
            {
                RemoveState(toRemoveIds[i]);
            }
        }

        /// <summary>
        /// 全能力の[強化]を解除
        /// </summary>
        public void RemoveAllBuffs() {
            for (var i = 0; i < BuffLength(); i++) RemoveBuff(i);
        }

        /// <summary>
        /// 状態異常を自動解除する
        /// </summary>
        /// <param name="timing"></param>
        public void RemoveStatesAuto(int timing) {
            var toRemoveIds = new List<string>();
            States.ForEach(state =>
            {
                if (IsStateExpired(state.id) && state.removeAtBattling == 1 && state.autoRemovalTiming == timing) toRemoveIds.Add(state.id);
            });

            for (int i = 0; i < toRemoveIds.Count; i++)
            {
                RemoveState(toRemoveIds[i]);
            }
        }

        /// <summary>
        /// ターン終了した能力[強化][弱体]を解除
        /// </summary>
        public void RemoveBuffsAuto() {
            for (var i = 0; i < BuffLength(); i++)
                if (IsBuffExpired(i))
                    RemoveBuff(i);
        }

        /// <summary>
        /// [ダメージで解除]のステートを解除
        /// </summary>
        public void RemoveStatesByDamage() {
            var toRemoveIds = new List<string>();
            States.ForEach(state =>
            {
                if (state.stateOn == 2 && state.removeByDamage == 1 && TforuUtility.MathRandom() * 100 < state.removeProbability)
                    toRemoveIds.Add(state.id);
                else if (state.stateOn == 0 && state.inBattleRemoveDamage == 1 && TforuUtility.MathRandom() * 100 < state.inBattleRemoveProbability)
                    toRemoveIds.Add(state.id);
            });

            for (int i = 0; i < toRemoveIds.Count; i++)
            {
                RemoveState(toRemoveIds[i]);
            }
        }

        /// <summary>
        /// 行動回数を設定して返す
        /// </summary>
        /// <returns></returns>
        public int MakeActionTimes() {
            return ActionPlusSet().Aggregate(1, (r, p) => { return TforuUtility.MathRandom() < p ? r + 1 : r; });
        }

        /// <summary>
        /// アニメーションを生成
        /// </summary>
        public virtual void MakeActions() {
            ClearActions();
            if (CanMove())
            {
                var actionTimes = MakeActionTimes();
                Actions = new List<GameAction>();
                for (var i = 0; i < actionTimes; i++) Actions.Add(new GameAction(this));
            }
        }

        /// <summary>
        /// 速度(行動順を決定する)を設定
        /// </summary>
        public void MakeSpeed() {
            Speed = Actions.ElementAtOrDefault(0)?.Speed() ?? 0;
        }

        /// <summary>
        /// 現在のアクションを返す
        /// </summary>
        /// <returns></returns>
        public GameAction CurrentAction() {
            return Actions.Any() ? Actions[0] : null;
        }

        /// <summary>
        /// 現在の行動を解除
        /// </summary>
        public void RemoveCurrentAction() {
            Actions.RemoveAt(0);
        }

        /// <summary>
        /// 目標バトラーを設定
        /// </summary>
        /// <param name="target"></param>
        public void SetLastTarget(GameBattler target) {
            if (target != null)
                LastTargetIndex = target.Index();
            else
                LastTargetIndex = 0;
        }

        /// <summary>
        /// 指定したスキルを強制する
        /// </summary>
        /// <param name="skillId"></param>
        /// <param name="targetIndex"></param>
        public void ForceAction(string skillId, int targetIndex) {
            ClearActions();
            var action = new GameAction(this, true);
            action.SetSkill(skillId);

            if (targetIndex == -2)
                action.SetTarget(LastTargetIndex);
            else if (targetIndex == -1)
                action.DecideRandomTarget();
            else
                action.SetTarget(targetIndex);

            Actions.Add(action);
        }

        /// <summary>
        /// 指定アイテムを使用
        /// </summary>
        /// <param name="item"></param>
        public void UseItem(GameItem item) {
            if (item.IsSkill())
                PaySkillCost(item);
            else if (item.IsItem()) ConsumeItem(item);
        }

        /// <summary>
        /// 指定アイテムを消費
        /// </summary>
        /// <param name="item"></param>
        public void ConsumeItem(GameItem item) {
            DataManager.Self().GetGameParty().ConsumeItem(item);
        }

        /// <summary>
        /// 指定量のHPを回復
        /// </summary>
        /// <param name="value"></param>
        public void GainHp(int value) {
            if (Result == null) ClearResult();
            Result.HpDamage = -value;
            Result.HpAffected = true;
            SetHp(Hp + value);
        }

        /// <summary>
        /// 指定量のMPを回復
        /// </summary>
        /// <param name="value"></param>
        public void GainMp(int value) {
            Result.MpDamage = -value;
            SetMp(Mp + value);
        }

        /// <summary>
        /// 指定量のTPを回復
        /// </summary>
        /// <param name="value"></param>
        public void GainTp(int value) {
            Result.TpDamage = -value;
            SetTp(Tp + value);
        }

        /// <summary>
        /// 指定量のTPを非表示で回復
        /// </summary>
        /// <param name="value"></param>
        public void GainSilentTp(int value) {
            SetTp(Tp + value);
        }

        /// <summary>
        /// TPの量を25までのランダムな値に初期化
        /// </summary>
        public void InitTp() {
            SetTp(new System.Random().Next(0, 25));
        }

        /// <summary>
        /// TPを0に
        /// </summary>
        public void ClearTp() {
            SetTp(0);
        }

        /// <summary>
        /// ダメージ率にしたがって、TPを増やす
        /// </summary>
        /// <param name="damageRate"></param>
        public void ChargeTpByDamage(double damageRate) {
            var value = (int) Math.Floor(50 * damageRate * Tcr);
            GainSilentTp(value);
        }

        /// <summary>
        /// 自動回復・ダメージを適用
        /// </summary>
        public void RegenerateHp() {
            var value = (int) Math.Floor(Mhp * Hrg);
            value = Math.Max(value, -MaxSlipDamage());
            if (value != 0) GainHp(value);
        }

        /// <summary>
        /// 速度(行動順を決定する)を設定
        /// </summary>
        /// <returns></returns>
        public int MaxSlipDamage() {
            return DataManager.Self().GetSystemDataModel().optionSetting.optSlipDeath == 1 ? Hp : Math.Max(Hp - 1, 0);
        }

        /// <summary>
        /// MP自動回復を適用
        /// </summary>
        public void RegenerateMp() {
            var value = (int) Math.Floor(Mmp * Mrg);
            if (value != 0) GainMp(value);
        }

        /// <summary>
        /// TP自動回復を適用
        /// </summary>
        public void RegenerateTp() {
            var value = (int) Math.Floor(100 * Trg);
            GainSilentTp(value);
        }

        /// <summary>
        /// 自動回復・ダメージを適用
        /// </summary>
        public void RegenerateAll() {
            if (IsAlive())
            {
                RegenerateHp();
                RegenerateMp();
                RegenerateTp();
            }
        }

        /// <summary>
        /// 戦闘開始ハンドラ
        /// </summary>
        public void OnBattleStart() {
            SetActionState(ActionStateEnum.Undecided);
            ClearMotion();
            if (!IsPreserveTp()) InitTp();
        }

        /// <summary>
        /// 全行動終了ハンドラ
        /// </summary>
        public void OnAllActionsEnd() {
            ClearResult();
            RemoveStatesAuto(1);
            RemoveBuffsAuto();
        }

        /// <summary>
        /// ターン終了ハンドラ
        /// </summary>
        /// <param name="isForcedTurn"></param>
        public void OnTurnEnd(bool isForcedTurn = false) {
            ClearResult();
            RegenerateAll();
            if (!isForcedTurn)
            {
                UpdateStateTurns();
                UpdateBuffTurns();
            }

            RemoveStatesAuto(2);
        }

        /// <summary>
        /// 戦闘終了ハンドラ
        /// </summary>
        public void OnBattleEnd() {
            ClearResult();
            RemoveBattleStates();
            RemoveAllBuffs();
            ClearActions();
            if (!IsPreserveTp()) ClearTp();

            Appear();
        }

        /// <summary>
        /// 被ダメージハンドラ
        /// </summary>
        /// <param name="value"></param>
        public void OnDamage(int value) {
            RemoveStatesByDamage();
            ChargeTpByDamage(1.0 * value / Mhp);

            Refresh();
        }

        /// <summary>
        /// 指定アクション状態を設定
        /// </summary>
        /// <param name="actionState"></param>
        public void SetActionState(ActionStateEnum actionState) {
            _actionState = actionState;
            RequestMotionRefresh();
        }

        /// <summary>
        /// 行動が未選択か
        /// </summary>
        /// <returns></returns>
        public bool IsUndecided() {
            return _actionState == ActionStateEnum.Undecided;
        }

        /// <summary>
        /// 戦闘コマンド入力中か
        /// </summary>
        /// <returns></returns>
        public bool IsInputting() {
            return _actionState == ActionStateEnum.Inputting;
        }

        /// <summary>
        /// 待機中か
        /// </summary>
        /// <returns></returns>
        public bool IsWaiting() {
            return _actionState == ActionStateEnum.Waiting;
        }

        /// <summary>
        /// アクション実行中か
        /// </summary>
        /// <returns></returns>
        public bool IsActing() {
            return _actionState == ActionStateEnum.Acting;
        }

        /// <summary>
        /// 魔法詠唱中か
        /// </summary>
        /// <returns></returns>
        public bool IsChanting() {
            if (IsWaiting()) return Actions.Any(action => action?.IsMagicSkill() ?? false);

            return false;
        }

        /// <summary>
        /// [防御]して待機中か
        /// </summary>
        /// <returns></returns>
        public bool IsGuardWaiting() {
            return Actions.Any(action => action?.IsGuard() ?? false);
        }

        /// <summary>
        /// 指定アクションの開始動作を実行
        /// </summary>
        /// <param name="action"></param>
        public virtual void PerformActionStart(GameAction action) {
            if (!action.IsGuard()) SetActionState(ActionStateEnum.Acting);
        }

        /// <summary>
        /// 指定アクションを実行
        /// </summary>
        /// <param name="action"></param>
        public virtual void PerformAction(GameAction action) {
        }

        /// <summary>
        /// 行動終了を実行
        /// </summary>
        public virtual void PerformActionEnd() {
            SetActionState(ActionStateEnum.Done);
        }

        /// <summary>
        /// 被ダメージ動作を実行
        /// </summary>
        public virtual void PerformDamage() {
        }

        /// <summary>
        /// 失敗動作を実行
        /// </summary>
        public virtual void PerformMiss() {
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE,
                DataManager.Self().GetSystemDataModel().soundSetting.miss);
            SoundManager.Self().PlaySe();
        }

        /// <summary>
        /// 回復動作を実行
        /// </summary>
        public virtual void PerformRecovery() {
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE,
                DataManager.Self().GetSystemDataModel().soundSetting.recovery);
            SoundManager.Self().PlaySe();
        }

        /// <summary>
        /// 回避動作を実行
        /// </summary>
        public virtual void PerformEvasion() {
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE,
                DataManager.Self().GetSystemDataModel().soundSetting.evasion);
            SoundManager.Self().PlaySe();
        }

        /// <summary>
        /// 魔法回避動作を実行
        /// </summary>
        public virtual void PerformMagicEvasion() {
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE,
                DataManager.Self().GetSystemDataModel().soundSetting.magicEvasion);
            SoundManager.Self().PlaySe();
        }

        /// <summary>
        /// カウンター動作を実行
        /// </summary>
        public virtual void PerformCounter() {
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE,
                DataManager.Self().GetSystemDataModel().soundSetting.evasion);
            SoundManager.Self().PlaySe();
        }

        /// <summary>
        /// 魔法反射動作を実行
        /// </summary>
        public virtual void PerformReflection() {
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE,
                DataManager.Self().GetSystemDataModel().soundSetting.magicReflection);
            SoundManager.Self().PlaySe();
        }

        /// <summary>
        /// 身代わり動作を実行
        /// </summary>
        /// <param name="target"></param>
        public virtual void PerformSubstitute(GameBattler target) {
        }

        /// <summary>
        /// 倒れる動作を実行
        /// </summary>
        public virtual void PerformCollapse() {
        }

        //================================================================================
        // 以下はUniteで追加されたメソッド
        //================================================================================

        /// <summary>
        /// アクションの実行者の名前を返却
        /// </summary>
        /// <returns></returns>
        public string GetName() {
            //以下については、本来は try catch の必要が無いが、
            //不具合調査時に埋め込むログに、名前を出力する際に、通常では呼ばれないタイミングで、本メソッドを
            //実行してしまうことがある
            //その場合でもエラーにならないように、未だ初期化が行われていない場合には、空文字列を返却するようにしている
            if (this is GameActor actor)
            {
                try
                {
                    return actor.Name;
                } catch (Exception) { }
            }

            if (this is GameEnemy enemy)
            {
                try
                {
                    return enemy.Name();
                }
                catch (Exception) { }
            }
            return "";
        }
        
        /// <summary>
        ///制御文字が含まれていても読み込まない用
        /// </summary>
        /// <returns></returns>
        public string GetNameNoColChar() {
            if (this is GameActor actor)
            {
                try
                {
                    var name = actor.Name;
                    name = name.Replace("\\", "\\\\");

                    return name;
                } catch (Exception) { }
            }

            if (this is GameEnemy enemy)
            {
                try
                {
                    return enemy.Name();
                }
                catch (Exception) { }
            }
            return "";
        }


        /// <summary>
        /// アクターかどうか
        /// </summary>
        /// <returns></returns>
        public override bool IsActor() {
            return this is GameActor;
        }

        /// <summary>
        /// 敵かどうか
        /// </summary>
        /// <returns></returns>
        public override bool IsEnemy() {
            return this is GameEnemy;
        }

        /// <summary>
        /// 自分から見て味方キャラクターを返却
        /// </summary>
        /// <returns></returns>
        public virtual GameUnit FriendsUnit() {
            switch (this)
            {
                case GameActor _:
                    return DataManager.Self().GetGameParty();
                case GameEnemy _:
                    return DataManager.Self().GetGameTroop();
                default:
                    throw new Exception();
            }
        }

        /// <summary>
        /// 自分から見て敵キャラクターを返却
        /// </summary>
        /// <returns></returns>
        public virtual GameUnit OpponentsUnit() {
            switch (this)
            {
                case GameActor _:
                    return DataManager.Self().GetGameTroop();
                case GameEnemy _:
                    return DataManager.Self().GetGameParty();
                default:
                    throw new Exception();
            }
        }

        /// <summary>
        /// バトルメンバーかどうか
        /// </summary>
        /// <returns></returns>
        public virtual bool IsBattleMember() {
            return true;
        }

        /// <summary>
        /// Index
        /// </summary>
        /// <returns></returns>
        public virtual int Index() {
            return -1;
        }

        /// <summary>
        /// Sprite表示中かどうか
        /// </summary>
        /// <returns></returns>
        public virtual bool IsSpriteVisible() {
            return true;
        }
    }
}