using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Knowledge.Misc;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.RuntimeDataManagement;
using RPGMaker.Codebase.Runtime.Battle.Sprites;
using RPGMaker.Codebase.Runtime.Battle.Window;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component;
using RPGMaker.Codebase.Runtime.Map;
using System;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DataManager = RPGMaker.Codebase.Runtime.Common.DataManager;

namespace RPGMaker.Codebase.Runtime.Battle
{
    /// <summary>
    /// 戦闘シーンのコマンドやメッセージのウィンドウ、[敵キャラ]やサイドビューの[アクター]の画像を管理するクラス
    /// </summary>
    public class SceneBattle : SceneBase
    {
        /// <summary>
        /// コマンドウィンドウの作成先
        /// </summary>
        [SerializeField] private GameObject _commandWindowParent;
        /// <summary>
        /// 戻るボタン
        /// </summary>
        [SerializeField] private Button _backButton;
        public static bool BackButton;
        /// <summary>
        /// 戦闘シーン用のスプライトセット。[背景][アクター][敵キャラ]を含む
        /// </summary>
        [SerializeField] private SpritesetBattle _spriteset;
        /// <summary>
        /// 前画面に戻るときの抑制するフラグ
        /// </summary>
        public static bool IsUpdateSuppress;
        /// <summary>
        /// [ステータス]ウィンドウ
        /// </summary>
        private WindowBattleStatus _statusWindow;
        /// <summary>
        /// [パーティ]コマンドウィンドウ
        /// </summary>
        private WindowPartyCommand _partyCommandWindow;
        /// <summary>
        /// [アクター]コマンドウィンドウ
        /// </summary>
        private WindowActorCommand _actorCommandWindow;
        /// <summary>
        /// [アイテム]ウィンドウ
        /// </summary>
        private WindowBattleItem _itemWindow;
        /// <summary>
        /// [アクター]選択ウィンドウ
        /// </summary>
        private WindowBattleActor _actorWindow;
        /// <summary>
        /// [スキル]ウィンドウ
        /// </summary>
        private WindowBattleSkill _skillWindow;
        /// <summary>
        /// [敵キャラ]選択ウィンドウ
        /// </summary>
        private WindowBattleEnemy _enemyWindow;
        /// <summary>
        /// ログウィンドウ
        /// </summary>
        private WindowBattleLog _logWindow;
        /// <summary>
        /// ヘルプウィンドウ
        /// </summary>
        private WindowHelp _helpWindow;
        /// <summary>
        /// メッセージウィンドウ
        /// </summary>
        private WindowMessage _messageWindow;

        /// <summary>
        /// 遷移先の画面
        /// </summary>
        public GameStateHandler.GameState NextScene { get; set; }

        /// <summary>
        /// シーンの開始
        /// </summary>
        protected override void Start() {
            base.Start();
            base.Init();
            
            //状態の更新
            GameStateHandler.SetGameState(GameStateHandler.GameState.BATTLE);
            //HUD系UIハンドリング
            HudDistributor.Instance.AddHudHandler(new HudHandler(gameObject));
            //音関連の初期化
            SoundManager.Self().Init();

            //indowの作成
            var window = new WindowInitialize().Create(_commandWindowParent);

            //要素の取得
            _logWindow = window.transform.Find("WindowBattleLog").GetComponent<WindowBattleLog>();
            _statusWindow = window.transform.Find("WindowBattleStatus").GetComponent<WindowBattleStatus>();
            _partyCommandWindow = window.transform.Find("WindowPartyCommand").GetComponent<WindowPartyCommand>();
            _actorCommandWindow = window.transform.Find("WindowActorCommand").GetComponent<WindowActorCommand>();
            _helpWindow = window.transform.Find("WindowHelp").GetComponent<WindowHelp>();
            _skillWindow = window.transform.Find("WindowBattleSkill").GetComponent<WindowBattleSkill>();
            _itemWindow = window.transform.Find("WindowBattleItem").GetComponent<WindowBattleItem>();
            _actorWindow = window.transform.Find("WindowBattleActor").GetComponent<WindowBattleActor>();
            _enemyWindow = window.transform.Find("WindowBattleEnemy").GetComponent<WindowBattleEnemy>();
            _messageWindow = window.transform.Find("WindowMessage").GetComponent<WindowMessage>();

            //初期状態設定
            _actorCommandWindow.gameObject.SetActive(true);
            _statusWindow.gameObject.SetActive(true);
            _skillWindow.gameObject.SetActive(true);
            _itemWindow.gameObject.SetActive(true);
            //Uniteではコマンド選択終了後は、ログWindowをActive、ステータスWindowを非Activeとする
            _logWindow.gameObject.SetActive(false);

            bool battleTest = false;
#if UNITY_EDITOR
            battleTest  = BattleTest.Instance.TryInitialize();
            if (battleTest)
            {
                var inputSystemObj = Resources.Load("InputSystem");
                Instantiate(inputSystemObj);
            }
#endif

#if DEBUG
            if (string.IsNullOrWhiteSpace(BattleSceneTransition.Instance.SelectTroopId))
            {
                var encounterTroopId =
                    BattleSceneTransition.Instance.EncounterDataModel?.troopList?.Count > 0
                        ? BattleSceneTransition.Instance.EncounterDataModel.troopList[0].troopId
                        : null;

                var (troopId, warningString) =
                    !string.IsNullOrEmpty(encounterTroopId)
                        ? (
                            encounterTroopId,
                            "このバトル用に設定されたエンカウンター情報から"
                        )
                        : (
                            new DatabaseManagementService().LoadTroop()[0].id,
                            "データベースの敵グループ情報から"
                        );

                BattleSceneTransition.Instance.SelectTroopId = troopId;
                DebugUtil.LogWarning(string.Concat(
                    "現在 (2022/04/12) 敵グループ相手のバトルしかできないので、",
                    $"{warningString}最初の敵グループid ({troopId}) を、",
                    "敵として設定しました。"));
            }
#endif
            //アクターの逃走状態を初期化
            var battlers = DataManager.Self().GetGameParty().BattleMembers();
            for (int i = 0; i < battlers.Count; i++)
            {
                battlers[i].IsEscaped = false;
            }
            //TroopID
            var id = BattleSceneTransition.Instance.SelectTroopId;
            //逃走可否
            var canEscape = BattleSceneTransition.Instance.CanEscape;
            //敗北可否
            var canLose = BattleSceneTransition.Instance.CanLose;
            //戦闘背景1
            var backImage1 = BattleSceneTransition.Instance.EncounterDataBackgroundImage1;
            //戦闘背景2
            var backImage2 = BattleSceneTransition.Instance.EncounterDataBackgroundImage2;

            //idが空の場合には、先頭の敵グループIDを設定
            if (id == "") id = new DatabaseManagementService().LoadTroop()[0].id;

            //画角対応として、バトル画面内に存在する全てのUIに対して、Scale設定を行う
            var scales = _spriteset.transform.parent.GetComponentsInChildren<CanvasScaler>();
            var displaySize = DataManager.Self().GetSystemDataModel()
                .DisplaySize[DataManager.Self().GetSystemDataModel().displaySize];
            foreach (var scale in scales)
            {
                scale.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                //scale.referenceResolution = displaySize;
            }

            //逃走可否、敗北可否をBattleManagerに登録
            BattleManager.Setup(id, canEscape, canLose, this, battleTest);
            //先制攻撃、不意打ち設定
            BattleManager.OnEncounter();

            //戻るボタンは初期状態で非表示
            _backButton.gameObject.SetActive(false);
            //戻るボタンのCB登録
            _backButton.onClick.RemoveAllListeners();
            _backButton.onClick.AddListener(OnClickBackButton);

            //表示に必要なオブジェクトを生成
            CreateDisplayObjects();
            //バトル開始用のフェードイン処理
            FadeIn();
            //フォーカス補正用クラスを追加
            gameObject.AddComponent<SelectedGameObjectManager>();

            //BGM再生
            SoundCommonDataModel bgm = new SoundCommonDataModel(
                DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.battleBgm.name,
                DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.battleBgm.pan,
                DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.battleBgm.pitch,
                DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.battleBgm.volume
            );
            if (BattleSceneTransition.Instance.EncounterDataModel != null)
            {
                if (BattleSceneTransition.Instance.EncounterDataModel.bgm.name != "")
                {
                    bgm = new SoundCommonDataModel(
                        BattleSceneTransition.Instance.EncounterDataModel.bgm.name,
                        BattleSceneTransition.Instance.EncounterDataModel.bgm.pan,
                        BattleSceneTransition.Instance.EncounterDataModel.bgm.pitch,
                        BattleSceneTransition.Instance.EncounterDataModel.bgm.volume
                    );
                }
            }

            //鳴動していたサウンドを停止
            SoundManager.Self().StopBgs();
            SoundManager.Self().StopBgm();
            SoundManager.Self().StopMe();
            SoundManager.Self().StopSe();

            //バトル用BGM鳴動開始
            BattleManager.PlayBattleBgm(bgm);

            //バトルシーケンス開始
            BattleManager.StartBattle();

#if UNITY_EDITOR
            if (battleTest)
            {
                //戦闘テストであることを設定
                BattleManager.SetBattleTest(true);
            }
#endif

            //TimeHandlerによるUpdate処理を開始
            TimeHandler.Instance.AddTimeActionEveryFrame(UpdateTimeHandler);
        }

        /// <summary>
        /// 破棄処理
        /// </summary>
        private void OnDestroy() {
            TimeHandler.Instance?.RemoveTimeAction(UpdateTimeHandler);
        }
        
        /// <summary>
        /// Update処理
        /// </summary>
        public void UpdateTimeHandler() {
            IsUpdateSuppress = false;
            var active = IsActive();

            //元々は非Activeかつ、バトルイベントでAbortが呼ばれていた場合には、ここでAbortする処理が存在したが、Uniteではイベントから直接実施
            //フェード、色調変更、フラッシュ、画面の揺れ、天候、画像表示のUpdateもここで実施していたが、Uniteではイベントで直接実施

            //ステータスWindowの更新
            UpdateStatusWindow();

            //BattleManager側の更新
            if (active && !IsBusy()) UpdateBattleProcess();

            
            //各WindowのUpdate処理
            _spriteset.UpdateTimeHandler();
            _logWindow.UpdateTimeHandler();
            _statusWindow.UpdateTimeHandler();
            _partyCommandWindow.UpdateTimeHandler();
            _actorCommandWindow.UpdateTimeHandler();
            _helpWindow.UpdateTimeHandler();
            _skillWindow.UpdateTimeHandler();
            _itemWindow.UpdateTimeHandler();
            _actorWindow.UpdateTimeHandler();
            _enemyWindow.UpdateTimeHandler();
            _messageWindow.UpdateTimeHandler();

            //バトル時の入力操作更新
            //UniteではInputSystemを利用
            InputHandler.Watch();

#if UNITY_EDITOR && DEBUG
            // 戦闘テストの終了テスト用。
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                if (Input.GetKeyUp(KeyCode.X))
                    if (BattleTest.Instance.TryTerminate())
                        return;
#endif
        }

        private void LateUpdate() {
            if (BackButton)
            {
                BackButton = false;
            }
        }

        /// <summary>
        /// 戦闘段階のアップデート
        /// </summary>
        private void UpdateBattleProcess() {
            if (!IsAnyInputWindowActive() || BattleManager.IsAborting() || BattleManager.IsBattleEnd())
            {
                BattleManager.UpdateBattleProcess();
                ChangeInputWindow();
            }
        }

        /// <summary>
        /// 入力ウィンドウがアクティブか
        /// </summary>
        /// <returns></returns>
        private bool IsAnyInputWindowActive() {
            return _partyCommandWindow.Active ||
                   _actorCommandWindow.Active ||
                   _skillWindow.Active ||
                   _itemWindow.Active ||
                   _actorWindow.Active ||
                   _enemyWindow.Active;
        }

        /// <summary>
        /// [パーティ]か[アクター]のコマンドウィンドウの選択、非選択を状態に応じて切り替え
        /// </summary>
        private void ChangeInputWindow() {
            if (BattleManager.IsInputting())
            {
                if (BattleManager.Actor() != null)
                {
                    StartActorCommandSelection();
                }
                else
                {
                    StartPartyCommandSelection();
                }
            }
            else
            {
                EndCommandSelection();
            }
        }

        /// <summary>
        /// シーンの停止
        /// </summary>
        public override void Stop() {
            base.Stop();

            //フェードアウト処理
            //フェードアウト後にTerminateを実行する
            FadeOut(Terminate);

            _statusWindow.Close();
            _partyCommandWindow.Close();
            _actorCommandWindow.Close();
        }

        /// <summary>
        /// 遷移前のシーン中断
        /// </summary>
        public void Terminate() {
            //GameParty終期化処理
            DataManager.Self().GetGameParty().OnBattleEnd();
            //GameTroop終期化処理
            DataManager.Self().GetGameTroop().OnBattleEnd();

            //サウンド停止
            SoundManager.Self().StopBgs();
            SoundManager.Self().StopBgm();
            SoundManager.Self().StopMe();
            SoundManager.Self().StopSe();

            //バトル終了
            BattleManager.IsBattle = false;

#if UNITY_EDITOR
            if (BattleTest.Instance.TryTerminate()) return;
#endif

            //マップへ戻る処理 (Unite固有）
            if (NextScene == GameStateHandler.GameState.MAP)
            {
                // HUD系UIハンドリング
                HudDistributor.Instance.RemoveHudHandler();

                //状態の更新
                GameStateHandler.SetGameState(GameStateHandler.GameState.MAP);

                //MAPを有効にする
                MapManager.BattleToMap();

                // オートセーブの実施
                var systemSettingDataModel = DataManager.Self().GetSystemDataModel();
                if (systemSettingDataModel.optionSetting.enabledAutoSave == 1)
                {
                    var runtimeDataManagementService = new RuntimeDataManagementService();
                    var data = DataManager.Self().GetRuntimeSaveDataModel();
                    runtimeDataManagementService.SaveAutoSaveData(data);
                }

                //バトルをUnloadする
                //SceneManager.UnloadSceneAsync("Battle");

                //マップのBGM、BGSに戻す
                BattleManager.ReplayBgmAndBgs();

                //初期化する
                BattleManager.InitMembers();
            }
            else if (NextScene == GameStateHandler.GameState.TITLE)
            {
                SceneManager.LoadScene("Title");
            }
            //GAMEOVER
            else
            {
                SceneManager.LoadScene("GameOver");
            }
        }

        /// <summary>
        /// GAMEOVER処理
        /// イベントから実行する
        /// </summary>
        public static void GameOver() {
            BattleManager.SceneBattle.NextScene = GameStateHandler.GameState.GAME_OVER;
            BattleManager.SceneBattle.Stop();
        }

        /// <summary>
        /// タイトルへ戻る
        /// イベントから実行する
        /// </summary>
        public static void BackTitle() {
            BattleManager.SceneBattle.NextScene = GameStateHandler.GameState.TITLE;
            BattleManager.SceneBattle.Stop();
        }

        /// <summary>
        /// [ステータス]ウィンドウのアップデート
        /// </summary>
        public void UpdateStatusWindow() {
            if (DataManager.Self().GetGameMessage().IsBusy())
            {
                _statusWindow.Close();
                _partyCommandWindow.Close();
                _actorCommandWindow.Close();
            }
            else if (IsActive() && !_messageWindow.IsClosing())
            {
                _statusWindow.Open();
            }
        }

        /// <summary>
        /// 表示に必要なオブジェクトを生成。 スプライトセット、ウィンドウレイヤー、ウィンドウなど
        /// </summary>
        public void CreateDisplayObjects() {
            CreateSpriteset();
            CreateAllWindows();
            BattleManager.SetLogWindow(_logWindow);
            BattleManager.SetStatusWindow(_statusWindow);
            BattleManager.SetCommandWindowParent(_commandWindowParent);
            BattleManager.SetSpriteset(_spriteset);
        }

        /// <summary>
        /// 戦闘シーンに必要なスプライトセットを生成。 [アクター][敵キャラ]など
        /// </summary>
        public void CreateSpriteset() {
            _spriteset.Initialize();
        }

        /// <summary>
        /// 戦闘シーンに必要なすべてのウィンドウを生成
        /// </summary>
        public void CreateAllWindows() {
            CreateLogWindow();
            CreateStatusWindow();
            CreatePartyCommandWindow();
            CreateActorCommandWindow();
            CreateHelpWindow();
            CreateSkillWindow();
            CreateItemWindow();
            CreateActorWindow();
            CreateEnemyWindow();
            CreateMessageWindow();
        }

        /// <summary>
        /// ログウィンドウ(Window_BattleLog)を生成
        /// </summary>
        public void CreateLogWindow() {
            _logWindow.Initialize();
        }

        /// <summary>
        /// [ステータス]ウィンドウ(Window_BattleStatus)を生成
        /// </summary>
        public void CreateStatusWindow() {
            _statusWindow.Initialize();
        }

        /// <summary>
        /// [パーティ]コマンドウィンドウ(Window_PartyCommand)を生成
        /// </summary>
        public void CreatePartyCommandWindow() {
            _partyCommandWindow.Initialize();
            _partyCommandWindow.SetHandler("fight", CommandFight);
            _partyCommandWindow.SetHandler("escape", CommandEscape);
            _partyCommandWindow.Deselect();
        }

        /// <summary>
        /// [アクター]コマンドウィンドウ(Window_ActorCommand)を生成
        /// </summary>
        public void CreateActorCommandWindow() {
            _actorCommandWindow.Initialize();
            _actorCommandWindow.SetHandler("attack", CommandAttack);
            _actorCommandWindow.SetHandler("skill", CommandSkill);
            _actorCommandWindow.SetHandler("defence", CommandGuard);
            _actorCommandWindow.SetHandler("item", CommandItem);
            _actorCommandWindow.SetHandler("cancel", SelectPreviousCommand);
        }

        /// <summary>
        /// ヘルプウィンドウ(Window_Help)を生成
        /// </summary>
        public void CreateHelpWindow() {
            _helpWindow.Initialize();
            _helpWindow.Visible = false;
        }

        /// <summary>
        /// [スキル]ウィンドウ(Window_BattleSkill)を生成
        /// </summary>
        public void CreateSkillWindow() {
            _skillWindow.Initialize();
            _skillWindow.SetHelpWindow(_helpWindow);
            _skillWindow.SetHandler("ok", OnSkillOk);
            _skillWindow.SetHandler("cancel", OnSkillCancel);
        }

        /// <summary>
        /// [アイテム]ウィンドウ(Window_BattleItem)を生成
        /// </summary>
        public void CreateItemWindow() {
            _itemWindow.Initialize();
            _itemWindow.SetHelpWindow(_helpWindow);
            _itemWindow.SetHandler("ok", OnItemOk);
            _itemWindow.SetHandler("cancel", OnItemCancel);
        }

        /// <summary>
        /// [アクター]選択ウィンドウ(Window_BattleActor)を生成
        /// </summary>
        public void CreateActorWindow() {
            _actorWindow.Initialize();
            _actorWindow.SetHandler("ok", OnActorOk);
            _actorWindow.SetHandler("cancel", OnActorCancel);
        }

        /// <summary>
        /// [敵キャラ]選択ウィンドウ(Window_BattleEnemy)を生成
        /// </summary>
        public void CreateEnemyWindow() {
            _enemyWindow.Initialize();
            _enemyWindow.SetHandler("ok", OnEnemyOk);
            _enemyWindow.SetHandler("cancel", OnEnemyCancel);
       }

        /// <summary>
        /// メッセージウィンドウ(Window_Message)を生成
        /// </summary>
        public void CreateMessageWindow() {
            _messageWindow.Initialize();
        }

        /// <summary>
        /// [ステータス]の回復
        /// </summary>
        public void RefreshStatus() {
            _statusWindow.Refresh();
        }

        /// <summary>
        /// [パーティ]コマンドの選択開始
        /// </summary>
        public void StartPartyCommandSelection() {
            RefreshStatus();
            _statusWindow.Deselect();
            _statusWindow.Open();
            _actorCommandWindow.Close();
            _partyCommandWindow.Setup();

            //以降、UniteのWindowやボタン制御関連処理
            //直前のWindowを表示
            _partyCommandWindow.gameObject.SetActive(true);
            _statusWindow.gameObject.SetActive(true);
            _backButton.gameObject.SetActive(false);
            _logWindow.gameObject.SetActive(false);

            //ボタンは全て有効にする
            _partyCommandWindow.Show();
        }

        /// <summary>
        /// [戦う]コマンドのハンドラ
        /// </summary>
        public void CommandFight() {
            SelectNextCommand();
        }

        /// <summary>
        /// [逃げる]コマンドのハンドラ
        /// </summary>
        public void CommandEscape() {
            BattleManager.ProcessEscape();
            ChangeInputWindow();
        }

        /// <summary>
        /// [アクター]コマンドの選択開始
        /// </summary>
        public void StartActorCommandSelection() {
            _statusWindow.Select(BattleManager.Actor().Index());
            _partyCommandWindow.Close();
            _actorCommandWindow.gameObject.SetActive(true);
            _actorCommandWindow.Setup(BattleManager.Actor());
            _actorCommandWindow.Show();

            //以降、UniteのWindowやボタン制御関連処理
            //対象のWindowを表示
            _statusWindow.gameObject.SetActive(true);

            //戻るボタン制御
            _backButton.gameObject.SetActive(true);
        }

        /// <summary>
        /// [攻撃]コマンドのハンドラ
        /// </summary>
        public void CommandAttack() {
            BattleManager.InputtingAction().SetAttack();
            SelectEnemySelection();
        }

        /// <summary>
        /// [スキル]コマンドのハンドラ
        /// </summary>
        public void CommandSkill() {
            _skillWindow.SetActor(BattleManager.Actor());
            _skillWindow.SetStypeId(_actorCommandWindow.CurrentExt());
            //_skillWindow.Refresh();
            _skillWindow.Show();
            _skillWindow.Activate();

            //以降、UniteのWindowやボタン制御関連処理
            _actorCommandWindow.gameObject.transform.localScale = new Vector3(0f,0f,0f);
            _partyCommandWindow.Close();
            _statusWindow.Close();

            //_actorCommandWindow.SetHandler("cancel", OnSkillCancel);
            _skillWindow.SetHandler("cancel", OnSkillCancel);

            //対象のWindowを表示
            _skillWindow.gameObject.SetActive(true);

            //直前のWindowは非表示
            _statusWindow.gameObject.SetActive(false);
            _actorCommandWindow.gameObject.SetActive(false);
        }

        /// <summary>
        /// [防御]コマンドのハンドラ
        /// </summary>
        public void CommandGuard() {
            BattleManager.InputtingAction()?.SetGuard();
            SelectNextCommand();
        }

        /// <summary>
        /// [アイテム]コマンドのハンドラ
        /// </summary>
        public void CommandItem() {
            _itemWindow.Refresh();
            _itemWindow.Show();
            _itemWindow.Activate();

            //以降、UniteのWindowやボタン制御関連処理
            //_actorCommandWindow.SetHandler("cancel", OnItemCancel);
            _itemWindow.SetHandler("cancel", OnItemCancel);

            //対象のWindowを表示
            _itemWindow.gameObject.SetActive(true);

            //直前のWindowは非表示
            _statusWindow.gameObject.SetActive(false);
            _actorCommandWindow.gameObject.SetActive(false);
        }

        /// <summary>
        /// ひとつ先のコマンドを選択
        /// </summary>
        public void SelectNextCommand() {
            BattleManager.SelectNextCommand();
            ChangeInputWindow();
        }

        /// <summary>
        /// ひとつ前のコマンドを選択
        /// </summary>
        public void SelectPreviousCommand() {
            BattleManager.SelectPreviousCommand();
            ChangeInputWindow();
        }

        /// <summary>
        /// [アクター]選択ウィンドウの準備
        /// </summary>
        public void SelectActorSelection() {
            _actorWindow.Refresh();
            _actorWindow.Show();
            _actorWindow.Activate();

            //直前のWindowは非表示
            _actorCommandWindow.gameObject.SetActive(false);
            _statusWindow.gameObject.SetActive(false);
            _skillWindow.gameObject.SetActive(false);
            _itemWindow.gameObject.SetActive(false);
        }

        /// <summary>
        /// [アクター]選択ウィンドウで[OK]が選択された時のハンドラ
        /// </summary>
        public void OnActorOk() {
            var action = BattleManager.InputtingAction();
            if (action != null)
                action.SetTarget(_actorWindow.Index());
            _actorWindow.Hide();
            _skillWindow.Hide();
            _itemWindow.Hide();
            SelectNextCommand();
        }

        /// <summary>
        /// [アクター]選択ウィンドウで[キャンセル]が選択された時のハンドラ
        /// </summary>
        public void OnActorCancel() {
            _actorWindow.Hide();
            switch (_actorCommandWindow.CurrentSymbol())
            {
                case "skill":
                    //直前のWindowを表示
                    _skillWindow.gameObject.SetActive(true);
                    _skillWindow.Show();
                    _skillWindow.Activate();
                    break;
                case "item":
                    //直前のWindowを表示
                    _itemWindow.gameObject.SetActive(true);
                    _itemWindow.Show();
                    _itemWindow.Activate();
                    break;
            }
        }

        /// <summary>
        /// [敵キャラ]選択ウィンドウの準備
        /// </summary>
        public void SelectEnemySelection() {
            _enemyWindow.Refresh();
            _enemyWindow.Show();
            _enemyWindow.Select(0);
            _enemyWindow.Activate();
            _enemyWindow.SetHandler("cancel", OnEnemyCancel);

            //直前のWindowは非表示
            _actorCommandWindow.gameObject.SetActive(false);
            _statusWindow.gameObject.SetActive(false);
            _skillWindow.gameObject.SetActive(false);
            _itemWindow.gameObject.SetActive(false);
        }

        /// <summary>
        /// [敵キャラ]選択ウィンドウで[OK]が選択された時のハンドラ
        /// </summary>
        public void OnEnemyOk() {
            var action = BattleManager.InputtingAction();
            action?.SetTarget(_enemyWindow.EnemyIndex());
            _enemyWindow.Hide();
            _skillWindow.Hide();
            _itemWindow.Hide();
            SelectNextCommand();
        }

        /// <summary>
        /// [敵キャラ]選択ウィンドウで[キャンセル]が選択された時のハンドラ
        /// </summary>
        public void OnEnemyCancel() {
            _actorCommandWindow.SetHandler("cancel", SelectPreviousCommand);
            _enemyWindow.Hide();
            _enemyWindow.Deactivate();
            switch (_actorCommandWindow.CurrentSymbol())
            {
                case "attack":
                    //直前のWindowを表示
                    _statusWindow.gameObject.SetActive(true);
                    _actorCommandWindow.gameObject.SetActive(true);
                    _actorCommandWindow.Show();
                    _actorCommandWindow.Activate();
                    _actorCommandWindow.Select(0);
                    break;
                case "skill":
                    _skillWindow.gameObject.SetActive(true);
                    _skillWindow.Show();
                    _skillWindow.Activate();
                    break;
                case "item":
                    _itemWindow.gameObject.SetActive(true);
                    _itemWindow.Show();
                    _itemWindow.Activate();
                    break;
            }
        }

        /// <summary>
        /// [スキル]ウィンドウで[OK]が選択された時のハンドラ
        /// </summary>
        public void OnSkillOk() {
            var skill = _skillWindow.Item();
            var action = BattleManager.InputtingAction();
            action.SetSkill(skill.ItemId);
            BattleManager.Actor().SetLastBattleSkill(skill);
            OnSelectAction();
        }

        /// <summary>
        /// [スキル]ウィンドウで[キャンセル]が選択された時のハンドラ
        /// </summary>
        public void OnSkillCancel() {
            _skillWindow.Hide();
            _skillWindow.Deactivate();
            _actorCommandWindow.gameObject.transform.localScale = new Vector3(1f,1f,1f);
            _actorCommandWindow.Setup(BattleManager.Actor());
            _actorCommandWindow.Select(0);
            _actorCommandWindow.Activate();
            _actorCommandWindow.SetHandler("cancel", SelectPreviousCommand);
            //直前のWindowを表示
            _actorCommandWindow.gameObject.SetActive(true);
            _actorCommandWindow.Show();
            _statusWindow.gameObject.SetActive(true);
        }

        /// <summary>
        /// [アイテム]ウィンドウで[OK]が選択された時のハンドラ
        /// </summary>
        public void OnItemOk() {
            var item = _itemWindow.Item();
            var action = BattleManager.InputtingAction();
            action.SetItem(item.ItemId);
            DataManager.Self().GetGameParty().SetLastItem(item);
            OnSelectAction();
        }

        /// <summary>
        /// [アイテム]ウィンドウで[キャンセル]が選択された時のハンドラ
        /// </summary>
        public void OnItemCancel() {
            _itemWindow.Hide();
            _itemWindow.Deactivate();

            _actorCommandWindow.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            _actorCommandWindow.Setup(BattleManager.Actor());
            _actorCommandWindow.Select(0);
            _actorCommandWindow.Activate();
            _actorCommandWindow.SetHandler("cancel", SelectPreviousCommand);

            //直前のWindowを表示
            _actorCommandWindow.gameObject.SetActive(true);
            _actorCommandWindow.Show();
            _statusWindow.gameObject.SetActive(true);
        }

        /// <summary>
        /// アイテムかスキルが選択された時のハンドラ
        /// </summary>
        public void OnSelectAction() {
            var action = BattleManager.InputtingAction();
            _skillWindow.Hide();
            _itemWindow.Hide();

            if (!action.NeedsSelection())
                SelectNextCommand();
            else if (action.IsForOpponent())
                SelectEnemySelection();
            else
                SelectActorSelection();
        }

        /// <summary>
        /// コマンド選択の終了処理
        /// </summary>
        public void EndCommandSelection() {
            _partyCommandWindow.Close();
            _actorCommandWindow.Close();
            _statusWindow.Deselect();

            //元々は全てActiveであったため、コマンド入力が一通り完了後は元の状態に戻す
            _actorCommandWindow.gameObject.SetActive(true);
            _skillWindow.gameObject.SetActive(true);
            _itemWindow.gameObject.SetActive(true);
            _backButton.gameObject.SetActive(false);

            //ボタンは全て無効にする
            _partyCommandWindow.Hide();
            _actorCommandWindow.Hide();
            _enemyWindow.Hide();
        }

        /// <summary>
        /// フェードイン
        /// </summary>
        /// <param name="callBack"></param>
        public void FadeIn([CanBeNull] Action callBack = null) {
            HudDistributor.Instance.NowHudHandler().DisplayInit();
            HudDistributor.Instance.NowHudHandler().FadeIn(callBack, true);
        }

        /// <summary>
        /// フェードアウト
        /// </summary>
        /// <param name="callBack"></param>
        public void FadeOut([CanBeNull] Action callBack = null) {
            HudDistributor.Instance.NowHudHandler().DisplayInit();
            HudDistributor.Instance.NowHudHandler().FadeOut(callBack, Color.black);
        }

        public void OnClickBackButton() {
            BackButton = true;
        }

#if UNITY_EDITOR
        /// <summary>
        ///     戦闘テスト。
        /// </summary>
        public class BattleTest
        {
            private static BattleTest _instance;
            private        bool       _isBattleTest;

            private BattleTest() {
            }

            public static BattleTest Instance
            {
                get
                {
                    _instance ??= new BattleTest();
                    return _instance;
                }
            }

            public static event Action<string> ScenePlayEndEvent;

            /// <summary>
            ///     戦闘テストの初期化を試行。
            /// </summary>
            /// <returns>戦闘テストフラグ</returns>
            public bool TryInitialize() {
                // タイトルシーンを経由せずにバトルシーンを再生した？
                _isBattleTest = DataManager.Self().GetRuntimeSaveDataModel() == null;

                if (!_isBattleTest)
                {
                    return false;
                }

                DataManager.Self().CreateGame(BattleSceneTransition.Instance);
                DataManager.Self().CreateLoadGame();

                BattleSceneTransition.Instance.CanEscape = true;
                BattleSceneTransition.Instance.CanLose = true;

                // 非アクティブになっているのでアクティブ化。
                SceneManager.GetActiveScene().GetRootGameObjects().Single(go => go.name == "EventSystem").SetActive(true);

                // キー等の割り当てをUnite用に差し替え
                string jsonData = File.ReadAllText(Application.dataPath + "/RPGMaker/InputSystem/rpgmaker.inputactions");
                InputActionAsset inputAction = new InputActionAsset();
                inputAction.LoadFromJson(jsonData);
                SceneManager.GetActiveScene().GetRootGameObjects().Single(go => go.name == "EventSystem").GetComponent<InputSystemUIInputModule>().actionsAsset = inputAction;

                //プレイ時間初期化
                TimeHandler.Instance.SetPlayTime(null);
                

                return true;
            }

            /// <summary>
            ///     戦闘テストの終期化を試行。
            /// </summary>
            /// <returns>戦闘テストだったフラグ</returns>
            public bool TryTerminate() {
                if (!_isBattleTest) return false;

                ScenePlayEndEvent?.Invoke(SceneManager.GetActiveScene().name);
                _instance = null;
                return true;
            }
        }
#endif
    }
}