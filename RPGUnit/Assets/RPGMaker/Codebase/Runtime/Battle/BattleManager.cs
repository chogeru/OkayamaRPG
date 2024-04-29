using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Battle.Sprites;
using RPGMaker.Codebase.Runtime.Battle.Window;
using RPGMaker.Codebase.Runtime.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Battle
{
    /// <summary>
    /// 戦闘の進行を制御する静的クラス
    /// </summary>
    public static class BattleManager
    {
        /// <summary>
        /// [static] 行動状態
        /// </summary>
        private static string _phase;
        /// <summary>
        /// [static] [逃走可]
        /// </summary>
        private static bool _canEscape;
        /// <summary>
        /// [static] [敗北可]
        /// </summary>
        private static bool _canLose;
        /// <summary>
        /// [static] [戦闘テスト]か
        /// </summary>
        private static bool _battleTest;
        /// <summary>
        /// [static] [先制攻撃]か
        /// </summary>
        private static bool _preemptive;
        /// <summary>
        /// [static] [不意打ち]か
        /// </summary>
        private static bool _surprise;
        /// <summary>
        /// [static] アクター番号
        /// </summary>
        private static int _actorIndex;
        /// <summary>
        /// [static] 強制行動のアクター
        /// </summary>
        private static GameBattler _actionForcedBattler;
        /// <summary>
        /// [static] 強制行動中
        /// </summary>
        private static bool _actionForced;
        /// <summary>
        /// [static] 戦闘BGM
        /// </summary>
        private static SoundCommonDataModel _mapBgm;
        /// <summary>
        /// [static] 戦闘BGS
        /// </summary>
        private static SoundCommonDataModel _mapBgs;
        /// <summary>
        /// [static] アクションを行うバトラーの配列(行動順)
        /// </summary>
        private static List<GameBattler> _actionBattlers;
        /// <summary>
        /// [static] 対象バトラー
        /// </summary>
        private static GameBattler _subject;
        /// <summary>
        /// [static] アクション
        /// </summary>
        private static GameAction _action;
        /// <summary>
        /// [static] 目標バトラーの配列
        /// </summary>
        private static List<GameBattler> _targets;
        /// <summary>
        /// [static] 使用者への影響バトラー
        /// </summary>
        private static GameBattler _targetMyself;
        /// <summary>
        /// [static] 使用者への影響バトラー（フラグ）
        /// </summary>
        private static bool _isTargetMyself;
        /// <summary>
        /// [static] ログウィンドウ
        /// </summary>
        private static WindowBattleLog _logWindow;
        /// <summary>
        /// [static] ステータスウィンドウ
        /// </summary>
        private static WindowBattleStatus _statusWindow;
        /// <summary>
        /// [static] スプライトセット
        /// </summary>
        private static GameObject _commandWindowParent;
        /// <summary>
        /// [static] スプライトセット
        /// </summary>
        private static SpritesetBattle _spriteset;
        /// <summary>
        /// [static] 逃走確率
        /// </summary>
        private static double _escapeRatio;
        /// <summary>
        /// [static] 逃走成功か
        /// </summary>
        private static bool _escaped;
        /// <summary>
        /// [static] 報酬
        /// </summary>
        private static BattleRewards _rewards;
        /// <summary>
        /// 強制行動中かどうか
        /// </summary>
        private static bool _turnForced;
        /// <summary>
        /// SceneBattleのインスタンス
        /// </summary>
        public static SceneBattle SceneBattle { get; set; }

        /// <summary>
        /// 現在バトル中かどうか
        /// </summary>
        public static bool IsBattle { get; set; }

        public static Canvas GetCanvas() {
            return _spriteset?.transform.parent.GetComponent<Canvas>();
        }
        public static Canvas GetCanvasUI() {
            return _commandWindowParent?.transform.parent.GetComponent<Canvas>();
        }

        /// <summary>
        /// 戦闘の設定
        /// </summary>
        /// <param name="troopId"></param>
        /// <param name="canEscape"></param>
        /// <param name="canLose"></param>
        /// <param name="sceneBattle"></param>
        public static void Setup(string troopId, bool canEscape, bool canLose, SceneBattle sceneBattle, bool battleTest) {
            //SceneBattle保持
            SceneBattle = sceneBattle;
            //バトル用の変数の初期化処理
            InitMembers();
            //逃走可能、敗北可能フラグを保持
            _canEscape = canEscape;
            _canLose = canLose;

            //パーティの刷新
            if (battleTest)
            {
                var party = new GameParty();
                DataManager.Self().SetGamePartyBattleTest(party);
            }
            else
            {
                //GameActorを念のため、最新の状態にする
                var actors = DataManager.Self().GetGameParty().Actors;
                for (int i = 0; i < actors.Count; i++)
                    actors[i].ResetActorData(true);
            }

            //敵グループ設定
            DataManager.Self().SetTroopForBattle(new GameTroop(troopId));

            //逃走確率作成
            MakeEscapeRatio();
        }

        /// <summary>
        /// メンバ変数の初期化
        /// </summary>
        public static void InitMembers() {
            _phase = "init";
            _canEscape = false;
            _canLose = false;
            _battleTest = false;
            _preemptive = false;
            _surprise = false;
            _actorIndex = -1;
            _actionForced = false;
            _actionForcedBattler = null;
            _mapBgm = null;
            _mapBgs = null;
            _actionBattlers = new List<GameBattler>();
            _subject = null;
            _action = null;
            _targets = new List<GameBattler>();
            _targetMyself = null;
            _logWindow = null;
            _statusWindow = null;
            _spriteset = null;
            _escapeRatio = 0;
            _escaped = false;
            _rewards = new BattleRewards();
            _turnForced = false;
        }

        /// <summary>
        /// [戦闘テスト]での実行か
        /// </summary>
        /// <returns></returns>
        public static bool IsBattleTest() {
            return _battleTest;
        }

        /// <summary>
        /// [テスト戦闘]状態か設定
        /// </summary>
        /// <param name="battleTest"></param>
        public static void SetBattleTest(bool battleTest) {
            _battleTest = battleTest;
        }

        /// <summary>
        /// ログウィンドウを取得
        /// </summary>
        /// <param name="logWindow"></param>
        public static WindowBattleLog GetLogWindow() {
            return _logWindow;
        }

        /// <summary>
        /// ログウィンドウを設定
        /// </summary>
        /// <param name="logWindow"></param>
        public static void SetLogWindow(WindowBattleLog logWindow) {
            _logWindow = logWindow;
        }

        /// <summary>
        /// ステータスウィンドウを設定
        /// </summary>
        /// <param name="statusWindow"></param>
        public static void SetStatusWindow(WindowBattleStatus statusWindow) {
            _statusWindow = statusWindow;
        }

        public static void SetCommandWindowParent(GameObject commandWindowParent) {
            _commandWindowParent = commandWindowParent;
        }
        public static GameObject GetCommandWindowParent() {
            return _commandWindowParent;
        }

        /// <summary>
        /// スプライトセットを設定
        /// </summary>
        /// <param name="spriteset"></param>
        public static void SetSpriteset(SpritesetBattle spriteset) {
            _spriteset = spriteset;
        }

        /// <summary>
        /// スプライトセットを取得
        /// </summary>
        /// <returns></returns>
        public static SpritesetBattle GetSpriteSet() {
            return _spriteset;
        }

        /// <summary>
        /// エンカウント時に呼ばれるハンドラ。 [先制攻撃][不意打ち]の判定
        /// </summary>
        public static void OnEncounter() {
            _preemptive = TforuUtility.MathRandom() < RatePreemptive();
            _surprise = TforuUtility.MathRandom() < RateSurprise() && !_preemptive;
        }

        /// <summary>
        /// 先制攻撃の確率
        /// </summary>
        /// <returns></returns>
        public static double RatePreemptive() {
            return DataManager.Self().GetGameParty().RatePreemptive(DataManager.Self().GetGameTroop().Agility());
        }

        /// <summary>
        /// 不意打ちの確率
        /// </summary>
        /// <returns></returns>
        public static double RateSurprise() {
            return DataManager.Self().GetGameParty().RateSurprise(DataManager.Self().GetGameTroop().Agility());
        }

        /// <summary>
        /// 戦闘BGMを再生
        /// </summary>
        public static async void PlayBattleBgm(SoundCommonDataModel bgm) {
            _mapBgm = SoundManager.Self().GetBgmSound();
            _mapBgs = SoundManager.Self().GetBgsSound();
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_BGM,
                bgm);
            await SoundManager.Self().PlayBgm();
            SoundManager.Self().StopBgs();
        }

        /// <summary>
        /// 勝利MEを再生
        /// </summary>
        public static async void PlayVictoryMe() {
            SoundManager.Self().StopBgm();
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_ME,
                new SoundCommonDataModel(
                    DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.victoryMe.name,
                    DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.victoryMe.pan,
                    DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.victoryMe.pitch,
                    DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.victoryMe.volume
                ));
            await SoundManager.Self().PlayMe();
        }

        /// <summary>
        /// 敗北MEを再生
        /// </summary>
        public static async void PlayDefeatMe() {
            SoundManager.Self().StopBgm();
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_ME,
                new SoundCommonDataModel(
                    DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.defeatMe.name,
                    DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.defeatMe.pan,
                    DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.defeatMe.pitch,
                    DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.defeatMe.volume
                ));
            await SoundManager.Self().PlayMe();
        }

        /// <summary>
        /// BGMとBGSの続きを再生
        /// </summary>
        public static async void ReplayBgmAndBgs() {
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_BGM, _mapBgm);
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_BGS, _mapBgs);

            await SoundManager.Self().PlayBgm();
            await SoundManager.Self().PlayBgs();
        }

        /// <summary>
        /// 逃走確率を設定
        /// </summary>
        public static void MakeEscapeRatio() {
            _escapeRatio = 0.5 * DataManager.Self().GetGameParty().Agility() /
                           DataManager.Self().GetGameTroop().Agility();
        }

        /// <summary>
        /// Update処理
        /// </summary>
        public static void UpdateBattleProcess() {
            if (!IsBusy()) {
                if (!UpdateEvent())
                {
                    switch (_phase)
                    {
                        case "start":
                            StartInput();
                            break;
                        case "turn":
                            UpdateTurn();
                            break;
                        case "action":
                            UpdateAction();
                            break;
                        case "turnEnd":
                            UpdateTurnEnd();
                            break;
                        case "battleEnd":
                            UpdateBattleEnd();
                            break;
                    }
                }
                else
                {
                    if (!_actionForced)
                    {
                        _logWindow.Hide();
                        _statusWindow.Hide();
                    }
                    else
                    {
                        _statusWindow.Hide();
                    }
                }
            }
        }

        /// <summary>
        /// イベントのアップデートを行い、何か実行されたか返す
        /// </summary>
        /// <returns></returns>
        public static bool UpdateEvent() {
            //イベントを実行中であればtrue
            if (GameStateHandler.CurrentGameState() == GameStateHandler.GameState.BATTLE_EVENT)
                return true;

            switch (_phase)
            {
                case "start":
                case "turn":
                case "turnEnd":
                    if (IsActionForced())
                    {
                        return ProcessForcedAction();
                    }
                    else
                    {
                        return UpdateEventMain();
                    }
            }

            return CheckAbort();
        }

        /// <summary>
        /// イベント主要部分のアップデートを行い、何か実行されたか返す
        /// </summary>
        /// <returns></returns>
        public static bool UpdateEventMain() {
            DataManager.Self().GetGameParty().RequestMotionRefresh();

            if (DataManager.Self().GetGameTroop().IsEventRunning() || CheckBattleEnd())
            {
                return true;
            }

            DataManager.Self().GetGameTroop().SetupBattleEvent();
            if (DataManager.Self().GetGameTroop().IsEventRunning())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// メッセージ表示などの処理中か
        /// </summary>
        /// <returns></returns>
        public static bool IsBusy() {
            var ret = DataManager.Self().GetGameMessage().IsBusy() || (_logWindow?.IsBusy() ?? false) || (_spriteset?.IsBusy() ?? false);
            return ret;
        }

        /// <summary>
        /// 入力中か
        /// </summary>
        /// <returns></returns>
        public static bool IsInputting() {
            return _phase == "input";
        }

        /// <summary>
        /// ターンの最中か
        /// </summary>
        /// <returns></returns>
        public static bool IsInTurn() {
            return _phase == "turn";
        }

        /// <summary>
        /// ターンの終了状態か
        /// </summary>
        /// <returns></returns>
        public static bool IsTurnEnd() {
            return _phase == "turnEnd";
        }

        /// <summary>
        /// 中断処理中か
        /// </summary>
        /// <returns></returns>
        public static bool IsAborting() {
            return _phase == "aborting";
        }

        /// <summary>
        /// 戦闘終了状態(敵か味方が全滅)か
        /// </summary>
        /// <returns></returns>
        public static bool IsBattleEnd() {
            return _phase == "battleEnd";
        }

        /// <summary>
        /// [逃走可]か
        /// </summary>
        /// <returns></returns>
        public static bool CanEscape() {
            return _canEscape;
        }

        /// <summary>
        /// [敗北可]か
        /// </summary>
        /// <returns></returns>
        public static bool CanLose() {
            return _canLose;
        }

        /// <summary>
        /// 逃走完了したか
        /// </summary>
        /// <returns></returns>
        public static bool IsEscaped() {
            return _escaped;
        }

        /// <summary>
        /// アクターを返す
        /// </summary>
        /// <returns></returns>
        public static GameActor Actor() {
            return _actorIndex >= 0 && _actorIndex < DataManager.Self().GetGameParty().Members().Count
                ? (GameActor) DataManager.Self().GetGameParty().Members()[_actorIndex]
                : null;
        }

        /// <summary>
        /// アクターの順番を初期位置に戻す
        /// </summary>
        public static void ClearActor() {
            ChangeActor(-1, GameBattler.ActionStateEnum.Null);
        }

        /// <summary>
        /// アクターの変更
        /// </summary>
        /// <param name="newActorIndex"></param>
        /// <param name="lastActorActionState"></param>
        public static void ChangeActor(int newActorIndex, GameBattler.ActionStateEnum lastActorActionState) {
            var lastActor = Actor();
            lastActor?.SetActionState(lastActorActionState);

            _actorIndex = newActorIndex;
            var newActor = Actor();
            newActor?.SetActionState(GameBattler.ActionStateEnum.Inputting);
        }

        /// <summary>
        /// 戦闘開始
        /// </summary>
        public static void StartBattle() {
            _phase = "start";
            //戦闘回数をインクリメントする
            DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.battleCount++;
            DataManager.Self().GetGameParty().OnBattleStart();
            DataManager.Self().GetGameTroop().OnBattleStart();
            DisplayStartMessages();

            //開始時点では、ログWindowは非表示
            _logWindow.gameObject.SetActive(false);
        }

        /// <summary>
        /// [出現]メッセージを表示
        /// </summary>
        public static void DisplayStartMessages() {
            DataManager.Self().GetGameTroop().EnemyNames().ForEach(name =>
            {
                DataManager.Self().GetGameMessage().Add(TextManager.Format(TextManager.emerge, name));
            });
            if (_preemptive)
                DataManager.Self().GetGameMessage()
                    .Add(TextManager.Format(TextManager.preemptive, DataManager.Self().GetGameParty().Name()));
            else if (_surprise)
                DataManager.Self().GetGameMessage()
                    .Add(TextManager.Format(TextManager.surprise, DataManager.Self().GetGameParty().Name()));
        }

        /// <summary>
        /// 入力開始
        /// </summary>
        public static void StartInput() {
            _phase = "input";
            DataManager.Self().GetGameTroop().AppearEnemy();
            DataManager.Self().GetGameParty().MakeActions();
            DataManager.Self().GetGameTroop().MakeActions();
            ClearActor();
            if (_surprise || !DataManager.Self().GetGameParty().CanInput()) StartTurn();
        }

        /// <summary>
        /// 入力中のアクターのアクションを返す
        /// </summary>
        /// <returns></returns>
        public static GameAction InputtingAction() {
            if (_phase != "input") return null;
            try
            {
                return Actor()?.InputtingAction();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// ひとつ先のコマンドを選択
        /// </summary>
        public static void SelectNextCommand() {
            if (_phase != "input") return;
            do
            {
                if (Actor() == null || !Actor().SelectNextCommand())
                {
                    ChangeActor(_actorIndex + 1, GameBattler.ActionStateEnum.Waiting);
                    if (_actorIndex >= DataManager.Self().GetGameParty().Size())
                    {
                        StartTurn();
                        break;
                    }
                }
            } while (Actor().CanInput() == false);
        }

        /// <summary>
        /// ひとつ前のコマンドを選択
        /// </summary>
        public static void SelectPreviousCommand() {
            do
            {
                if (Actor() == null || !Actor().SelectPreviousCommand())
                {
                    ChangeActor(_actorIndex - 1, GameBattler.ActionStateEnum.Undecided);
                    if (_actorIndex < 0) return;
                }
            } while (!Actor().CanInput());
        }

        /// <summary>
        /// [ステータス]表示を再描画
        /// </summary>
        public static void RefreshStatus() {
            _statusWindow.Refresh();
        }

        /// <summary>
        /// ターン開始
        /// </summary>
        public static void StartTurn() {
            _phase = "turn";
            ClearActor();
            DataManager.Self().GetGameTroop().IncreaseTurn();
            MakeActionOrders();
            DataManager.Self().GetGameParty().RequestMotionRefresh();
            _logWindow.Open();
            _logWindow.StartTurn();

            //ターン継続中は、ログWindowを表示し、ステータスWindowを非表示にする
            _statusWindow.gameObject.SetActive(false);
            _logWindow.gameObject.SetActive(true);
        }

        /// <summary>
        /// ターンのアップデート
        /// </summary>
        public static void UpdateTurn() {
            //ターン継続中は、ログWindowを表示
            _logWindow.gameObject.SetActive(true);

            DataManager.Self().GetGameParty().RequestMotionRefresh();
            if (_subject == null)
                _subject = GetNextSubject();

            if (_subject != null)
                ProcessTurn();
            else
                EndTurn();
        }

        /// <summary>
        /// ターン継続処理
        /// </summary>
        public static void ProcessTurn() {
            var subject = _subject;
            var action = subject.CurrentAction();
            if (action != null)
            {
                action.Prepare();
                if (action.IsValid()) StartAction();
                subject.RemoveCurrentAction();
            }
            else
            {
                subject.OnAllActionsEnd();
                RefreshStatus();
                _logWindow.Show();
                _logWindow.DisplayAutoAffectedStatus(subject);
                _logWindow.DisplayCurrentState(subject);
                _logWindow.DisplayRegeneration(subject);
                _subject = GetNextSubject();
            }
        }

        /// <summary>
        /// ターン終了処理
        /// </summary>
        public static void EndTurn() {
            _phase = "turnEnd";
            _preemptive = false;
            _surprise = false;
            bool turnForced = _turnForced;
            AllBattleMembers().ForEach(battler =>
            {
                if (!turnForced)
                {
                    battler.OnTurnEnd();
                    RefreshStatus();
                    _logWindow.DisplayAutoAffectedStatus(battler);
                    _logWindow.DisplayRegeneration(battler);
                }
                else
                {
                    RefreshStatus();
                }
            });
            if (IsForcedTurn())
            {
                _actionForced = false;
                _turnForced = false;
                BattleEventCommandChainLauncher.ResumeEvent();
            }

            _logWindow.Close();

            //ターン終了後は、ログWindowを非表示にし、ステータスWindowを表示する
            //バトルイベントが継続していた場合には、ステータスWindowは表示せず、イベント終了後に表示とする
            _logWindow.gameObject.SetActive(false);
            if (GameStateHandler.CurrentGameState() != GameStateHandler.GameState.BATTLE_EVENT)
                _statusWindow.gameObject.SetActive(true);
        }

        /// <summary>
        /// 強制されたターンか
        /// </summary>
        /// <returns></returns>
        public static bool IsForcedTurn() {
            return _turnForced;
        }

        /// <summary>
        /// ターン終了のアップテート
        /// </summary>
        public static void UpdateTurnEnd() {
            StartInput();
        }

        /// <summary>
        /// 次の対象バトラーを返す
        /// </summary>
        /// <returns></returns>
        public static GameBattler GetNextSubject() {
            for (;;)
            {
                if (_actionBattlers.Count <= 0) return null;

                var battler = _actionBattlers[0];
                _actionBattlers.RemoveAt(0);
                if (battler.IsBattleMember() && battler.IsAlive()) return battler;
            }
        }

        /// <summary>
        /// 戦闘に参加している全バトラーを返す
        /// </summary>
        /// <returns></returns>
        public static List<GameBattler> AllBattleMembers() {
            var members = new List<GameBattler>();
            DataManager.Self().GetGameParty().Members().Aggregate(members, (m, a) =>
            {
                m.Add(a);
                return m;
            });
            DataManager.Self().GetGameTroop().Members().Aggregate(members, (m, e) =>
            {
                m.Add(e);
                return m;
            });
            return members;
        }

        /// <summary>
        /// アクションの順番を設定
        /// </summary>
        public static void MakeActionOrders() {
            var battlers = new List<GameBattler>();

            if (!_surprise) DataManager.Self().GetGameParty().Members().ForEach(actor => battlers.Add(actor));

            if (!_preemptive) DataManager.Self().GetGameTroop().Members().ForEach(enemy => battlers.Add(enemy));

            battlers.ForEach(battler => { battler.MakeSpeed(); });
            battlers.Sort((a, b) => (int) b.Speed - (int) a.Speed);
            _actionBattlers = battlers;
        }

        /// <summary>
        /// アクション開始
        /// </summary>
        public static void StartAction() {
            var subject = _subject;
            var action = subject.CurrentAction();
            var targets = action.MakeTargets();
            _phase = "action";
            _action = action;
            _targets = targets;
            _targetMyself = action.MakeTargetMyself();
            _isTargetMyself = _targetMyself != null ? true : false;
            subject.UseItem(action.Item);
            _action.ApplyGlobal();
            RefreshStatus();
            _logWindow.StartAction(subject, action, targets);
        }

        /// <summary>
        /// アクションのアップデート
        /// </summary>
        public static void UpdateAction() {
            if (_targets.Count > 0)
            {
                var target = _targets[0];
                _targets.RemoveAt(0);
                InvokeAction(_subject, target);
            }
            else if (_targetMyself != null)
            {
                var targetMyself = _targetMyself;
                InvokeActionMyself(_subject, _subject);
                _targetMyself = null;
            }
            else
            {
                //コモンイベントが存在する場合はキューに貯める
                bool isCommonEvent = false;
                bool isCommonEventForUser = false;

                //コモンイベントが設定されているか
                foreach (var effect in _action.Item.Effects)
                {
                    if (isCommonEvent) break;
                    isCommonEvent = _action.IsEffectCommonEvent(effect);
                }

                //使用者への影響にチェックが入っている場合、使用者への影響側のコモンイベントも確認
                if (_action.IsForUser())
                    foreach (var effect in _action.Item.EffectsMyself)
                    {
                        if (isCommonEventForUser) break;
                        isCommonEventForUser = _action.IsEffectCommonEvent(effect);
                    }

                //いずれかのコモンイベントが存在した場合は、Inspector上最後に設定されているコモンイベントを実行
                if (isCommonEvent || isCommonEventForUser)
                {
                    _action.SetCommonEvent(isCommonEventForUser);
                }

                _isTargetMyself = false;

                //Actionは終了
                EndAction();
            }
        }

        /// <summary>
        /// 行動終了処理
        /// </summary>
        public static void EndAction() {
            _logWindow.EndAction(_subject);
            _phase = "turn";
        }

        /// <summary>
        /// 指定対象が指定目標に対してのアクションを起動する
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="target"></param>
        public static void InvokeAction(GameBattler subject, GameBattler target) {
            _logWindow.Push(_logWindow.PushBaseLine);
            if (TforuUtility.MathRandom() < _action.ItemCnt(target))
                InvokeCounterAttack(subject, target);
            else if (TforuUtility.MathRandom() < _action.ItemMrf(target))
                InvokeMagicReflection(subject, target);
            else
                InvokeNormalAction(subject, target);

            subject.SetLastTarget(target);
            _logWindow.Push(_logWindow.PopBaseLine);
            RefreshStatus();
        }

        /// <summary>
        /// 指定対象が指定目標に対しての通常アクションを起動する
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="target"></param>
        public static void InvokeNormalAction(GameBattler subject, GameBattler target) {
            var realTarget = ApplySubstitute(target);
            _action.Apply(realTarget);
            _logWindow.DisplayActionResults(subject, realTarget);
        }

        /// <summary>
        /// 指定対象が指定目標に対してのアクションを起動する（使用者への影響）
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="target"></param>
        public static void InvokeActionMyself(GameBattler subject, GameBattler target) {
            _logWindow.Push(_logWindow.PushBaseLine);

            var realTarget = ApplySubstitute(target);
            _action.ApplyMyself(realTarget);
            _logWindow.DisplayActionResults(subject, realTarget);

            subject.SetLastTarget(target);
            _logWindow.Push(_logWindow.PopBaseLine);
            RefreshStatus();
        }

        /// <summary>
        /// 指定対象が指定目標に対しての反撃アクションを起動する
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="target"></param>
        public static void InvokeCounterAttack(GameBattler subject, GameBattler target) {
            var action = new GameAction(target);
            action.SetAttack();
            action.Apply(subject);
            _logWindow.DisplayCounter(target);
            _logWindow.DisplayActionResults(target, subject);
        }

        /// <summary>
        /// 指定対象が指定目標に対しての魔法反射アクションを起動する
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="target"></param>
        public static void InvokeMagicReflection(GameBattler subject, GameBattler target) {
            _action._reflectionTarget = target;
            _logWindow.DisplayReflection(target);
            _action.Apply(subject);
            _logWindow.DisplayActionResults(target, subject);
        }

        /// <summary>
        /// 対象が死んでいるなどしたら、代わりを選んで返す。 問題なければ、対象をそのまま返す
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static GameBattler ApplySubstitute(GameBattler target) {
            if (CheckSubstitute(target))
            {
                var substitute = target.FriendsUnit().SubstituteBattler();
                if (substitute != null && target != substitute)
                {
                    _logWindow.DisplaySubstitute(substitute, target);
                    return substitute;
                }
            }

            return target;
        }

        /// <summary>
        /// 対象が死んでいるなどして代わりが必要か返す
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool CheckSubstitute(GameBattler target) {
            return target.IsDying() && !_action.IsCertainHit();
        }

        /// <summary>
        /// 強制行動中か
        /// </summary>
        /// <returns></returns>
        public static bool IsActionForced() {
            return _actionForced;
        }

        /// <summary>
        /// 強制行動
        /// </summary>
        /// <param name="battler"></param>
        public static void ForceAction(GameBattler battler) {
            _actionForced = true;
            _actionForcedBattler = battler;
            var index = _actionBattlers.IndexOf(battler);
            if (index >= 0) _actionBattlers.RemoveAt(index);
        }

        /// <summary>
        /// 強制アクションの処理
        /// </summary>
        public static bool ProcessForcedAction() {
            if (_actionForcedBattler != null)
            {
                //強制行動中は、ログWindowを表示し、ステータスWindowを非表示にする
                _logWindow.Open();
                _logWindow.StartTurn();
                _statusWindow.gameObject.SetActive(false);
                _logWindow.gameObject.SetActive(true);

                _turnForced = true;
                _subject = _actionForcedBattler;
                _actionForcedBattler = null;
                StartAction();
                _subject.RemoveCurrentAction();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 中止
        /// </summary>
        public static void Abort() {
            _phase = "aborting";
        }

        /// <summary>
        /// 味方か敵が全滅しているなど戦闘終了状態なら終了し、終了を実行したか返す
        /// </summary>
        /// <returns></returns>
        public static bool CheckBattleEnd() {
            if (_phase != "")
            {
                if (CheckAbort())
                {
                    //バトルが終了している場合、ログWindowは非表示
                    _logWindow.gameObject.SetActive(false);
                    return true;
                }

                if (DataManager.Self().GetGameParty().IsAllDead())
                {
                    //バトルが終了している場合、ログWindowは非表示
                    _logWindow.gameObject.SetActive(false);
                    ProcessDefeat();
                    return true;
                }

                if (DataManager.Self().GetGameTroop().IsAllDead())
                {
                    //バトルが終了している場合、ログWindowは非表示
                    _logWindow.gameObject.SetActive(false);
                    ProcessVictory();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// パーティがいないなど中止する状態なら中止し、中止を実行したか返す
        /// </summary>
        /// <returns></returns>
        public static bool CheckAbort() {
            if (DataManager.Self().GetGameParty().IsEmpty() || IsAborting())
            {
                SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.escape);
                SoundManager.Self().PlaySe();
                _escaped = true;
                ProcessAbort();
            }

            return false;
        }

        /// <summary>
        /// 勝利処理
        /// </summary>
        public static void ProcessVictory() {
            DataManager.Self().GetGameParty().RemoveBattleStates();
            //戦闘中に変更となったステート情報をマップに引き継ぐ
            DataManager.Self().GetGameParty().AllMembers().ForEach(actor => { actor.SetBattleEndStates(); });
            DataManager.Self().GetGameParty().PerformVictory();
            PlayVictoryMe();
            MakeRewards();
            DisplayVictoryMessage();
            DisplayRewards();
            GainRewards();
            EndBattle(0);
        }

        /// <summary>
        /// 逃走処理を行い、逃走が成功したか返す
        /// </summary>
        /// <returns></returns>
        public static bool ProcessEscape() {
            DataManager.Self().GetGameParty().PerformEscape();
            var success = _preemptive ? true : TforuUtility.MathRandom() < _escapeRatio;
            if (success)
            {
                //逃走成功
                SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE,
                    DataManager.Self().GetSystemDataModel().soundSetting.escape);
                SoundManager.Self().PlaySe();

                DataManager.Self().GetGameParty().RemoveBattleStates();
                //戦闘中に変更となったステート情報をマップに引き継ぐ
                DataManager.Self().GetGameParty().AllMembers().ForEach(actor => { actor.SetBattleEndStates(); });

                //逃走メッセージの表示を実施
                DisplayEscapeSuccessMessage();
                _escaped = true;
                ProcessAbort();
            }
            else
            {
                DisplayEscapeFailureMessage();
                _escapeRatio += 0.1;
                DataManager.Self().GetGameParty().ClearActions();
                StartTurn();
            }

            //逃走メッセージを表示する際には、一時的にログWindowを消去
            _logWindow.gameObject.SetActive(false);
            return success;
        }

        /// <summary>
        /// 中止処理
        /// </summary>
        public static void ProcessAbort() {
            DataManager.Self().GetGameParty().RemoveBattleStates();
            //戦闘中に変更となったステート情報をマップに引き継ぐ
            DataManager.Self().GetGameParty().AllMembers().ForEach(actor => { actor.SetBattleEndStates(); });
            EndBattle(1);
        }

        /// <summary>
        /// 敗北処理
        /// </summary>
        public static void ProcessDefeat() {
            DisplayDefeatMessage();
            PlayDefeatMe();

            //敗北不可の場合はGAMEOVER時の音声を、別のところで鳴動する
            //敗北可の場合はマップに戻る際に、マップのBGMを再生しなおすため、ここでは処理しない
            if (!_canLose)
                SoundManager.Self().StopBgm();

            EndBattle(2);
        }

        /// <summary>
        /// 戦闘終了処理
        /// </summary>
        /// <param name="result"></param>
        public static void EndBattle(int result) {
            DataManager.Self().BattleResult = result;
            _phase = "battleEnd";

            //勝利回数と逃走回数をインクリメントする
            if (result == 0)
            {
                DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.winCount++;
            }
            else if (_escaped)
            {
                DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.escapeCount++;
            }
        }

        /// <summary>
        /// 戦闘終了のアップデート
        /// </summary>
        public static void UpdateBattleEnd() {
            //遷移先をMAPで初期化
            SceneBattle.NextScene = GameStateHandler.GameState.MAP;

            if (IsBattleTest())
            {
                SoundManager.Self().StopBgm();
            }
            //逃げていなくて全員死んだときの処理
            else if (!_escaped && DataManager.Self().GetGameParty().IsAllDead())
            {
                //負けられるかの判定
                if (_canLose)
                {
                    //負けられる時の処理
                    DataManager.Self().GetGameParty().ReviveBattleMembers();
                }
                else
                {
                    //負けられない時の処理
                    //SceneBattle.GameOver();
                    //遷移先をGAMEOVERとする
                    SceneBattle.NextScene = GameStateHandler.GameState.GAME_OVER;
                }
            }

            //バトル終了処理
            SceneBattle.Stop();
            _phase = null;
        }

        /// <summary>
        /// 報酬を設定
        /// </summary>
        public static void MakeRewards() {
            _rewards = new BattleRewards
            {
                gold = DataManager.Self().GetGameTroop().GoldTotal(),
                exp = DataManager.Self().GetGameTroop().ExpTotal(),
                items = DataManager.Self().GetGameTroop().MakeDropItems()
            };
        }

        /// <summary>
        /// [勝利]メッセージを表示
        /// </summary>
        public static void DisplayVictoryMessage() {
            DataManager.Self().GetGameMessage().Add((TextManager.Format(TextManager.victory, DataManager.Self().GetGameParty().Name())));
        }

        /// <summary>
        /// [敗北]メッセージを表示
        /// </summary>
        public static void DisplayDefeatMessage() {
            DataManager.Self().GetGameMessage().Add((TextManager.Format(TextManager.defeat, DataManager.Self().GetGameParty().Name())));
        }

        /// <summary>
        /// [逃走成功]メッセージを表示
        /// </summary>
        public static void DisplayEscapeSuccessMessage() {
            DataManager.Self().GetGameMessage().Add((TextManager.Format(TextManager.escapeStart, DataManager.Self().GetGameParty().Name())));
        }

        /// <summary>
        /// [逃走失敗]メッセージを表示
        /// </summary>
        public static void DisplayEscapeFailureMessage() {
            DataManager.Self().GetGameMessage().Add(TextManager.Format(TextManager.escapeStart, DataManager.Self().GetGameParty().Name()));
            DataManager.Self().GetGameMessage().Add(TextManager.Format("\\." + TextManager.escapeFailure));
        }

        /// <summary>
        /// 報酬(経験値・お金・アイテム)メッセージを表示
        /// </summary>
        public static void DisplayRewards() {
            DisplayExp();
            DisplayGold();
            DisplayDropItems();
        }

        /// <summary>
        /// [経験値獲得]メッセージを表示
        /// </summary>
        public static void DisplayExp() {
            var exp = _rewards.exp;
            if (exp > 0)
            {
                //Uniteでは経験値を人数で割る
                //逃げたメンバーはカウントしない
                int count = DataManager.Self().GetGameParty().AllMembers().Count;
                var members = DataManager.Self().GetGameParty().AllMembers();
                for (int i = 0; i < members.Count; i++)
                {
                    if (members[i].IsEscaped) count--;
                }
                exp = Mathf.FloorToInt(exp / count);

                var text = TextManager.Format(TextManager.obtainExp, exp.ToString(), TextManager.exp);
                DataManager.Self().GetGameMessage().Add("\\." + text);
            }
        }

        /// <summary>
        /// [お金獲得]メッセージを表示
        /// </summary>
        public static void DisplayGold() {
            var gold = _rewards.gold;
            if (gold > 0)
            {
                var text = TextManager.Format(TextManager.obtainGold, gold.ToString());
                DataManager.Self().GetGameMessage().Add("\\." + text);
            }
        }

        /// <summary>
        /// [アイテム獲得]メッセージを表示
        /// </summary>
        public static void DisplayDropItems() {
            var items = _rewards.items;
            if (items.Count > 0)
            {
                DataManager.Self().GetGameMessage().NewPage();
                items.ForEach(item =>
                {
                    var text = TextManager.Format(TextManager.obtainItem, item.Name);
                    DataManager.Self().GetGameMessage().Add(text);
                });
            }
        }

        /// <summary>
        /// 報酬(経験値・お金・アイテム)を返す
        /// </summary>
        public static void GainRewards() {
            GainExp();
            GainGold();
            GainDropItems();
        }

        /// <summary>
        /// [経験値]を返す
        /// </summary>
        public static void GainExp() {
            var exp = _rewards.exp;

            //Uniteでは経験値を人数で割る
            //逃げたメンバーはカウントしない
            int count = DataManager.Self().GetGameParty().AllMembers().Count;
            var members = DataManager.Self().GetGameParty().AllMembers();
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i].IsEscaped) count--;
            }
            exp = Mathf.FloorToInt(exp / count);

            DataManager.Self().GetGameParty().AllMembers().ForEach(actor => { 
                if (!actor.IsEscaped)
                {
                    actor.GainExp(exp);
                }
            });
        }

        /// <summary>
        /// [お金]を返す
        /// </summary>
        public static void GainGold() {
            DataManager.Self().GetGameParty().GainGold(_rewards.gold);
        }

        /// <summary>
        /// [ドロップアイテム]を返す
        /// </summary>
        public static void GainDropItems() {
            var items = _rewards.items;
            items.ForEach(item => { DataManager.Self().GetGameParty().GainItem(item, 1); });
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod()]
        static void Init() {
            IsBattle = false;
        }
#endif
    }

    /// <summary>
    /// 報酬
    /// </summary>
    public class BattleRewards
    {
        public int            exp;
        public int            gold;
        public List<GameItem> items;
    }
}