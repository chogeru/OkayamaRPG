using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventCommon;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Event.Actor;
using RPGMaker.Codebase.Runtime.Event.AudioVideo;
using RPGMaker.Codebase.Runtime.Event.Battle;
using RPGMaker.Codebase.Runtime.Event.Character;
using RPGMaker.Codebase.Runtime.Event.Display;
using RPGMaker.Codebase.Runtime.Event.FlowControl;
using RPGMaker.Codebase.Runtime.Event.GameProgress;
using RPGMaker.Codebase.Runtime.Event.Map;
using RPGMaker.Codebase.Runtime.Event.Message;
using RPGMaker.Codebase.Runtime.Event.Party;
using RPGMaker.Codebase.Runtime.Event.Picture;
using RPGMaker.Codebase.Runtime.Event.Screen;
using RPGMaker.Codebase.Runtime.Event.Systems;
using RPGMaker.Codebase.Runtime.Event.Wait;
using RPGMaker.Codebase.Runtime.Map;
using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.Runtime.Event
{
    /// <summary>
    ///     イベントのChainLauncherの基底クラス
    /// </summary>
    public abstract class AbstractEventCommandChainLauncher
    {
        /// <summary>
        ///     各イベントコードで実行する、各イベントプロセッサーの定義
        /// </summary>
        protected readonly Dictionary<EventEnum, AbstractEventCommandProcessor> _commandProcessors =
            new Dictionary<EventEnum, AbstractEventCommandProcessor>
            {
                //メッセージ
                {EventEnum.EVENT_CODE_MESSAGE_TEXT, new MessageTextProcessor()},
                {EventEnum.EVENT_CODE_MESSAGE_TEXT_ONE_LINE, new MessageTextOnLineProcessor()},
                {EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT, new MessageInputSelectProcessor()},
                {EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_SELECTED, new MessageInputSelectedProcessor()},
                {EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_CANCELED, new MessageInputSelectCanceld()},
                {EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_END, new MessageInputSelectEndProcessor()},
                {EventEnum.EVENT_CODE_MESSAGE_INPUT_NUMBER, new MessageInputNumberProcessor()},
                {EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_ITEM, new MessageInputSelectItemProcessor()},
                {EventEnum.EVENT_CODE_MESSAGE_TEXT_SCROLL, new MessageTextScrollProcessor()},
                {EventEnum.EVENT_CODE_MESSAGE_TEXT_SCROLL_ONE_LINE, new MessageTextScrollOnLineProcessor()},

                //キャラクター
                {EventEnum.EVENT_CODE_CHARACTER_SHOW_ANIMATION, new ShowAnimationProcessor()},
                {EventEnum.EVENT_CODE_CHARACTER_SHOW_ICON, new ShowIconProcessor()},
                {EventEnum.EVENT_CODE_CHARACTER_IS_EVENT, new IsEventProcessor()},
                {EventEnum.MOVEMENT_WALKING_ANIMATION_ON, new AnimationSettingsProcessor()},
                {EventEnum.MOVEMENT_WALKING_ANIMATION_OFF, new AnimationSettingsProcessor()},
                {EventEnum.MOVEMENT_STEPPING_ANIMATION_ON, new AnimationSettingsProcessor()},
                {EventEnum.MOVEMENT_STEPPING_ANIMATION_OFF, new AnimationSettingsProcessor()},
                {EventEnum.MOVEMENT_CHANGE_IMAGE, new ChangeImageProcessor()},

                {EventEnum.EVENT_CODE_MOVE_PLACE, new MovePlaceProcessor()},
                {EventEnum.EVENT_CODE_MOVE_SET_MOVE_POINT, new MoveSetMovePointProcessor()},
                {EventEnum.EVENT_CODE_MOVE_SET_EVENT_POINT, new MoveSetMovePointProcessor()},
                {EventEnum.MOVEMENT_MOVE_AT_RANDOM, new MoveSetMovePointProcessor()},
                {EventEnum.MOVEMENT_MOVE_TOWARD_PLAYER, new MoveSetMovePointProcessor()},
                {EventEnum.MOVEMENT_MOVE_AWAY_FROM_PLAYER, new MoveSetMovePointProcessor()},
                {EventEnum.MOVEMENT_ONE_STEP_FORWARD, new OneStepForwardProcessor()},
                {EventEnum.MOVEMENT_ONE_STEP_BACKWARD, new OneStepBackwardProcessor()},
                {EventEnum.EVENT_CODE_STEP_MOVE, new StepMoveProcessor() },
                {EventEnum.EVENT_CODE_CHANGE_MOVE_SPEED, new ChangeMoveSpeedProcessor() },
                {EventEnum.EVENT_CODE_CHANGE_MOVE_FREQUENCY, new ChangeMoveFrequencyProcessor() },
                {EventEnum.EVENT_CODE_PASS_THROUGH, new PassThroughProcessor() },
                {EventEnum.MOVEMENT_JUMP, new JumpProcessor()},
                {EventEnum.MOVEMENT_TURN_DOWN, new CharacterDirectionProcessor()},
                {EventEnum.MOVEMENT_TURN_LEFT, new CharacterDirectionProcessor()},
                {EventEnum.MOVEMENT_TURN_RIGHT, new CharacterDirectionProcessor()},
                {EventEnum.MOVEMENT_TURN_UP, new CharacterDirectionProcessor()},
                {EventEnum.MOVEMENT_TURN_90_RIGHT, new CharacterDirectionProcessor()},
                {EventEnum.MOVEMENT_TURN_90_LEFT, new CharacterDirectionProcessor()},
                {EventEnum.MOVEMENT_TURN_180, new CharacterDirectionProcessor()},
                {EventEnum.MOVEMENT_TURN_90_RIGHT_OR_LEFT, new CharacterDirectionProcessor()},
                {EventEnum.MOVEMENT_TURN_AT_RANDOM, new CharacterDirectionProcessor()},
                {EventEnum.MOVEMENT_TURN_TOWARD_PLAYER, new CharacterDirectionProcessor()},
                {EventEnum.MOVEMENT_TURN_AWAY_FROM_PLAYER, new CharacterDirectionProcessor()},
                {EventEnum.EVENT_CODE_MOVE_PLACE_SHIP, new MovePlaceShipProcessor()},
                {EventEnum.EVENT_CODE_MOVE_RIDE_SHIP, new MoveRideShipProcessor()},

                //タイミング
                {EventEnum.EVENT_CODE_TIMING_WAIT, new TimingWaitProcessor()},

                //パーティ
                {EventEnum.EVENT_CODE_PARTY_CHANGE, new PartyChangeProcess()},
                {EventEnum.EVENT_CODE_CHARACTER_CHANGE_ALPHA, new PartyAlphaProcess()},
                {EventEnum.EVENT_CODE_CHARACTER_CHANGE_WALK, new PartyChangeWalkProcess()},
                {EventEnum.EVENT_CODE_CHARACTER_CHANGE_PARTY, new PartyCharacterChangeProcess()},
                {EventEnum.EVENT_CODE_PARTY_GOLD, new PartyGoldProcess()},
                {EventEnum.EVENT_CODE_PARTY_ITEM, new PartyItemProcess()},
                {EventEnum.EVENT_CODE_PARTY_WEAPON, new PartyWeaponProcess()},
                {EventEnum.EVENT_CODE_PARTY_ARMS, new PartyArmsProcess()},

                //アクター
                {EventEnum.EVENT_CODE_ACTOR_CHANGE_HP, new ActorChangeHpProcessor()},
                {EventEnum.EVENT_CODE_ACTOR_CHANGE_MP, new ActorChangeMpProcessor()},
                {EventEnum.EVENT_CODE_ACTOR_CHANGE_TP, new ActorChangeTpProcessor()},
                {EventEnum.EVENT_CODE_ACTOR_CHANGE_STATE, new ActorChangeStateProcessor()},
                {EventEnum.EVENT_CODE_ACTOR_HEAL, new ActorHealProcessor()},
                {EventEnum.EVENT_CODE_ACTOR_CHANGE_EXP, new ActorChangeExpProcessor()},
                {EventEnum.EVENT_CODE_ACTOR_CHANGE_LEVEL, new ActorChangeLevelProcessor()},
                {EventEnum.EVENT_CODE_ACTOR_CHANGE_PARAMETER, new ActorChangeParameterProcessor()},
                {EventEnum.EVENT_CODE_ACTOR_CHANGE_SKILL, new ActorChangeSkillProcessor()},
                {EventEnum.EVENT_CODE_ACTOR_CHANGE_EQUIPMENT, new ActorChangeEquipmentProcessor()},
                {EventEnum.EVENT_CODE_ACTOR_CHANGE_CLASS, new ActorChangeClassProcessor()},
                {EventEnum.EVENT_CODE_ACTOR_CHANGE_NAME, new ActorSettingsProcessor()},
                {EventEnum.EVENT_CODE_ACTOR_CHANGE_NICKNAME, new ActorSettingsProcessor()},
                {EventEnum.EVENT_CODE_ACTOR_CHANGE_PROFILE, new ActorSettingsProcessor()},


                //ゲーム進行
                {EventEnum.EVENT_CODE_GAME_SWITCH, new GameSwitchProcessor()},
                {EventEnum.EVENT_CODE_GAME_VAL, new GameValProcessor()},
                {EventEnum.EVENT_CODE_GAME_SELF_SWITCH, new GameSelfSwitchProcessor()},
                {EventEnum.EVENT_CODE_GAME_TIMER, new GameTimerProcessor()},

                //ピクチャ
                {EventEnum.EVENT_CODE_PICTURE_SHOW, new PictureShowProcessor()},
                {EventEnum.EVENT_CODE_PICTURE_MOVE, new PictureMoveProcessor()},
                {EventEnum.EVENT_CODE_PICTURE_ROTATE, new PictureRotateProcessor()},
                {EventEnum.EVENT_CODE_PICTURE_CHANGE_COLOR, new PictureChangeColorProcessor()},
                {EventEnum.EVENT_CODE_PICTURE_ERASE, new PictureErase()},

                //画面
                {EventEnum.EVENT_CODE_DISPLAY_FADEOUT, new DisplayFadeOutProcessor()},
                {EventEnum.EVENT_CODE_DISPLAY_FADEIN, new DisplayFadeInProcessor()},
                {EventEnum.EVENT_CODE_DISPLAY_CHANGE_COLOR, new DisplayChangeColorProcessor()},
                {EventEnum.EVENT_CODE_DISPLAY_FLASH, new DisplayFlashProcessor()},
                {EventEnum.EVENT_CODE_DISPLAY_SHAKE, new DisplayShakeProcessor()},
                {EventEnum.EVENT_CODE_DISPLAY_WEATHER, new DisplayChangeWeatherProcessor()},

                //マップ
                {EventEnum.EVENT_CODE_MAP_CHANGE_NAME, new MapChangeNameProcessor()},
                {EventEnum.EVENT_CODE_MAP_CHANGE_BATTLE_BACKGROUND, new MapChangeBattleBackground()},
                {EventEnum.EVENT_CODE_MAP_GET_POINT, new MapGetPoint()},
                {EventEnum.EVENT_CODE_MAP_CHANGE_DISTANT_VIEW, new MapChangeDistantView()},
                {EventEnum.EVENT_CODE_MOVE_MAP_SCROLL, new MapScrollProcessor()},

                //システム
                {EventEnum.EVENT_CODE_SYSTEM_BATTLE_BGM, new SystemBattleBgmProcessor()},
                {EventEnum.EVENT_CODE_SYSTEM_BATTLE_WIN, new SystemBattleWinProcessor()},
                {EventEnum.EVENT_CODE_SYSTEM_BATTLE_LOSE, new SystemBattleLoseProcessor()},
                {EventEnum.EVENT_CODE_SYSTEM_SHIP_BGM, new SystemShipBgmProcessor()},
                {EventEnum.EVENT_CODE_SYSTEM_CHANGE_ACTOR_IMAGE, new SystemChangeActorImageProcessor()},
                {EventEnum.EVENT_CODE_SYSTEM_CHANGE_SHIP_IMAGE, new SystemChangeShipImageProcessor()},
                {EventEnum.EVENT_CODE_SYSTEM_IS_MENU, new SystemIsMenuProcessor()},
                {EventEnum.EVENT_CODE_SYSTEM_IS_SAVE, new SystemIsSaveProcessor()},
                {EventEnum.EVENT_CODE_SYSTEM_IS_ENCOUNT, new SystemIsEncountProcessor()},
                {EventEnum.EVENT_CODE_SYSTEM_IS_SORT, new SystemIsSortProcessor()},
                {EventEnum.EVENT_CODE_SYSTEM_WINDOW_COLOR, new SystemWindowColorProcessor()},

                //オーディオ・ビデオ
                {EventEnum.EVENT_CODE_AUDIO_BGM_PLAY, new BgmPlayProcessor()},
                {EventEnum.EVENT_CODE_AUDIO_BGS_PLAY, new BgsPlayProcessor()},
                {EventEnum.EVENT_CODE_AUDIO_ME_PLAY, new MePlayProcessor()},
                {EventEnum.EVENT_CODE_AUDIO_SE_PLAY, new SePlayProcessor()},
                {EventEnum.EVENT_CODE_AUDIO_SE_STOP, new SeStopProcessor()},
                {EventEnum.EVENT_CODE_AUDIO_BGM_FADEOUT, new BgmFadeoutProcessor()},
                {EventEnum.EVENT_CODE_AUDIO_BGS_FADEOUT, new BgsFadeoutProcessor()},
                {EventEnum.EVENT_CODE_AUDIO_BGM_SAVE, new BgmSaveProcessor()},
                {EventEnum.EVENT_CODE_AUDIO_BGM_CONTINUE, new BgmContinueProcessor()},
                {EventEnum.EVENT_CODE_AUDIO_MOVIE_PLAY, new MoviePlayProcessor()},

                //シーン制御
                {EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG, new EncountEnemyProcessor()},
                {EventEnum.EVENT_CODE_SCENE_GAME_OVER, new GameOverProcessor()},
                {EventEnum.EVENT_CODE_SCENE_GOTO_TITLE, new GoToTitleProcessor()},
                {EventEnum.EVENT_CODE_SCENE_INPUT_NAME, new InputNameProcessor()},
                {EventEnum.EVENT_CODE_SCENE_MENU_OPEN, new OpenMenuWindowProcessor()},
                {EventEnum.EVENT_CODE_SCENE_SAVE_OPEN, new OpenSaveWindowProcessor()},
                {EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG, new OpenShopProcessor()},
                //{EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG_LINE,new OpenShopProcessor()},
                {EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_WIN, new WinProcessor()},
                {EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_ESCAPE, new EscapeProcessor()},
                {EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_LOSE, new LoseProcessor()},
                {EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END, new EndProcessor()},

                //バトル
                {EventEnum.EVENT_CODE_BATTLE_CHANGE_STATUS, new BattleChangeStatus()},
                {EventEnum.EVENT_CODE_BATTLE_CHANGE_STATE, new BattleChangeState()},
                {EventEnum.EVENT_CODE_BATTLE_APPEAR, new BattleAppear()},
                {EventEnum.EVENT_CODE_BATTLE_TRANSFORM, new BattleTransform()},
                {EventEnum.EVENT_CODE_BATTLE_SHOW_ANIMATION, new BattleShowAnimation()},
                {EventEnum.EVENT_CODE_BATTLE_EXEC_COMMAND, new BattleExecCommand()},
                {EventEnum.EVENT_CODE_BATTLE_STOP, new BattleStop()},

                //アドオン
                {EventEnum.EVENT_CODE_ADDON_COMMAND, new AddonCommandProcessor()}
            };

        /// <summary>
        ///     イベント実行完了後のコールバック
        /// </summary>
        protected Action<EventMapDataModel, EventDataModel> _callback;

        /// <summary>
        /// コモンイベントを実行する際のコールバック（マップ用）
        /// </summary>
        protected Action<string> _executeCommonEvent;

        /// <summary>
        ///     現在実行中のイベントID
        /// </summary>
        protected string _commandEventID;

        /// <summary>
        ///     現在実行中のイベントコマンドリスト
        /// </summary>
        protected List<EventDataModel.EventCommand> _commandsInQueue;

        /// <summary>
        ///     イベントリスト内で、現在実行中のイベントのIndex
        /// </summary>
        protected int _currentCommandIndex;

        /// <summary>
        ///     現在実行中のイベントコード
        /// </summary>
        protected int _eventCode;

        /// <summary>
        ///     イベント用のDataModelリスト
        /// </summary>
        protected List<EventCommonDataModel> _eventCommonDataModels = null;

        /// <summary>
        ///     イベント用のService
        /// </summary>
        protected EventManagementService _eventManagementService = null;

        /// <summary>
        ///     イベントが実行中かどうかのフラグ
        /// </summary>
        protected bool _running;


        /// <summary>
        ///     現在実行中のCommandIndex取得
        /// </summary>
        /// <returns>現在実行中のCommandIndex</returns>
        public int GetIndex() {
            return _currentCommandIndex;
        }

        /// <summary>
        ///     現在実行中のCommandIndexを指定位置まで飛ばす
        /// </summary>
        /// <param name="index">設定するCommandIndex</param>
        public void SetIndex(int index) {
            _currentCommandIndex = index;
        }

        /// <summary>
        ///     現在イベントが実行中かどうかを返却する
        /// </summary>
        /// <returns>実行中の場合true</returns>
        public bool IsRunning() {
            return _running;
        }

        /// <summary>
        ///     現在実行中のイベントコードを返却する
        /// </summary>
        /// <returns>現在実行中のイベントコード</returns>
        public int GetRunningEventCode() {
            return _eventCode;
        }

        /// <summary>
        ///     次に実行するイベントコードを取得
        /// </summary>
        public EventEnum GetNextEventCode() {
            if (_currentCommandIndex == _commandsInQueue.Count - 1)
                return EventEnum.EVENT_CODE_MAX;
            
            return (EventEnum) _commandsInQueue[_currentCommandIndex + 1].code;
        }

        /// <summary>
        ///     イベントコマンドを実行する. 継承先で必要な処理を記載する.
        /// </summary>
        /// <param name="eventId">イベントID</param>
        public virtual void ProcessCommand(string eventId) {
        }

        public virtual void ProcessCommandExecute() {
        }

        /// <summary>
        ///     引数無しでイベントコマンドを実行する. この場合は同一のイベントIDで処理を継続する.
        /// </summary>
        public void ProcessCommand() {
            ProcessCommand(_commandEventID);
        }

        /// <summary>
        ///     イベントのフロー制御.
        ///     ソースを分けるとインデックス周りが複雑になるのでここで処理.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        protected bool FlowControl(EventEnum code, string eventId) {
            var nowIndent = _commandsInQueue[_currentCommandIndex].indent;
            switch (code)
            {
                case EventEnum.EVENT_CODE_FLOW_IF:
                    // 条件分岐
                    // 一致
                    if (new FlowIf().FrowIf(eventId, _commandsInQueue[_currentCommandIndex]))
                    {
                        // そのまま次へ
                    }
                    // 不一致
                    else
                    {
                        // else分岐がある場合
                        if (_commandsInQueue[_currentCommandIndex].parameters[0] == "1")
                        {
                            // elseへ飛ぶ
                            for (var i = _currentCommandIndex; i < _commandsInQueue.Count; i++)
                                if (_commandsInQueue[i].code == (int) EventEnum.EVENT_CODE_FLOW_ELSE &&
                                    _commandsInQueue[i].indent == nowIndent ||
                                    _commandsInQueue[i].code == (int) EventEnum.EVENT_CODE_FLOW_ENDIF &&
                                    _commandsInQueue[i].indent == nowIndent)
                                {
                                    _currentCommandIndex = i;
                                    break;
                                }
                        }
                        else
                        {
                            // endifへ飛ぶ
                            for (var i = _currentCommandIndex; i < _commandsInQueue.Count; i++)
                                if (_commandsInQueue[i].code == (int) EventEnum.EVENT_CODE_FLOW_ENDIF &&
                                    _commandsInQueue[i].indent == nowIndent)
                                {
                                    _currentCommandIndex = i;
                                    break;
                                }
                        }
                    }

                    break;
                case EventEnum.EVENT_CODE_FLOW_OR:
                case EventEnum.EVENT_CODE_FLOW_AND:
                    //そのまま次へ
                    break;
                case EventEnum.EVENT_CODE_FLOW_ELSE:
                    // ENDへ飛ぶ
                    for (var i = _currentCommandIndex; i < _commandsInQueue.Count; i++)
                        if (_commandsInQueue[i].code == (int) EventEnum.EVENT_CODE_FLOW_ENDIF &&
                            _commandsInQueue[i].indent == nowIndent)
                        {
                            _currentCommandIndex = i;
                            break;
                        }

                    break;
                case EventEnum.EVENT_CODE_FLOW_LOOP:
                    // ループ
                    break;
                case EventEnum.EVENT_CODE_FLOW_LOOP_END:
                    // ループ終了（最初に戻る）
                    // 現在から上方向に戻り、ループ開始かつ同一のindent のところを探す
                    for (var i = _currentCommandIndex; i >= 0; i--)
                        if (_commandsInQueue[i].code == (int) EventEnum.EVENT_CODE_FLOW_LOOP &&
                            _commandsInQueue[i].indent == nowIndent)
                        {
                            _currentCommandIndex = i;
                            break;
                        }

                    break;
                case EventEnum.EVENT_CODE_FLOW_LOOP_BREAK:
                    // ループ中断
                    // 現在から下方向に進み、ループ終了かつ同一のindent のところを探す
                    for (var i = _currentCommandIndex; i < _commandsInQueue.Count; i++)
                        if (_commandsInQueue[i].code == (int) EventEnum.EVENT_CODE_FLOW_LOOP_END &&
                            _commandsInQueue[i].indent == nowIndent)
                        {
                            _currentCommandIndex = i;
                            break;
                        }

                    // ループ終了が見つからない場合
                    // ループ元のindentを取得し次の現在のイベント以下のindentまで飛ばす
                    var roopIndent = 0;
                    for (var i = _currentCommandIndex; i > 0; i--)
                        if (_commandsInQueue[i].code == (int) EventEnum.EVENT_CODE_FLOW_LOOP)
                            roopIndent = _commandsInQueue[i].indent;

                    for (var i = _currentCommandIndex; i < _commandsInQueue.Count; i++)
                        if (_commandsInQueue[i].indent == roopIndent)
                        {
                            _currentCommandIndex = i;
                            break;
                        }

                    break;
                case EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE:
                    // カスタム移動
                    TimeHandler.Instance.RemoveTimeAction(ProcessCommandExecute);
                    if (_commandsInQueue[_currentCommandIndex].parameters.Count >= 6 && _commandsInQueue[_currentCommandIndex].parameters[5] == "1")
                    {
                        //カスタム移動化済み。
                        break;
                    }
                    if (!_running) return false;
                    var endIndex = _commandsInQueue.Count - 1;
                    for (var i = _currentCommandIndex; i < _commandsInQueue.Count; i++)
                    {
                        if (_commandsInQueue[i].code == (int) EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE_END &&
                            _commandsInQueue[i].indent == nowIndent)
                        {
                            endIndex = i;
                            break;
                        }
                    }
                    var parallelProcess = (_commandsInQueue[_currentCommandIndex].parameters[4] != "1");
                    if (!parallelProcess)
                    {
                        // 完了までウェイト。終了検知フラグを初期化。
                        if (_commandsInQueue[endIndex].parameters.Count == 0)
                        {
                            _commandsInQueue[endIndex].parameters.Add("0");
                        }
                        else
                        {
                            _commandsInQueue[endIndex].parameters[0] = "0";
                        }
                    }
                    MapEventExecutionController.Instance.GetCustomMoveEventManager().RegisterCustomMove(_commandEventID, _commandsInQueue, _currentCommandIndex, endIndex, parallelProcess);
                    if (!parallelProcess)
                    {
                        // 完了までウェイト。
                        _currentCommandIndex = endIndex;
                        TimeHandler.Instance.AddTimeActionEveryFrame(ProcessCommandExecute);
                        return true;    //ProcessCommand()を呼びたくない。
                    } else
                    {
                        // 完了までウェイトがOFF。
                        _currentCommandIndex = endIndex;    //ProcessCommand()で+1される。
                    }
                    break;
                case EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE_END:
                    TimeHandler.Instance.RemoveTimeAction(ProcessCommandExecute);
                    if (!_running) return false;
                    if (_commandsInQueue[_currentCommandIndex].parameters.Count == 1 && _commandsInQueue[_currentCommandIndex].parameters[0] != "1")
                    {
                        //カスタム移動が完了していない。完了になるまで繰り返しチェック。
                        TimeHandler.Instance.AddTimeActionEveryFrame(ProcessCommandExecute);
                        return true;    //ProcessCommand()を呼びたくない。
                    }
                    // カスタム移動終了（繰り返す場合は、最初に戻る）
                    // 現在から上方向に戻り、カスタム移動開始かつ同一のindent のところを探す
                    for (var i = _currentCommandIndex; i >= 0; i--)
                        if (_commandsInQueue[i].code == (int) EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE &&
                            _commandsInQueue[i].indent == nowIndent)
                        {
                            if (_commandsInQueue[i].parameters[2] == "1")
                            {
                                //「動作を繰り返す」がON
                                _currentCommandIndex = i;
                                TimeHandler.Instance.AddTimeActionEveryFrame(ProcessCommandExecute);
                                return true;    //ProcessCommand()を呼びたくない。
                            }
                            else
                            {
                                //「動作を繰り返す」がOFF
                                //_currentCommandIndexは、ProcessCommand()で+1されるので、変更不要。
                                break;
                            }
                        }

                    break;
                case EventEnum.EVENT_CODE_FLOW_EVENT_BREAK:
                    // イベント中断
                    // コモンイベント実行中であればコモンイベント終了まで飛ばす
                    bool commonEnd = false;

                    // コモンイベント中断の場合、挿入するロジック的に、必ず最初に見つかった箇所になる
                    for (var i = _currentCommandIndex; i < _commandsInQueue.Count; i++)
                        if (_commandsInQueue[i].code == (int) EventEnum.EVENT_CODE_FLOW_COMMON_END)
                        {
                            _currentCommandIndex = i;
                            commonEnd = true;
                            break;
                        }

                    if (commonEnd == true)
                        break;

                    // インデックスを最後まで進める
                    _currentCommandIndex = _commandsInQueue.Count;
                    break;
                case EventEnum.EVENT_CODE_FLOW_JUMP_COMMON:
                    // コモンイベント実行
                    EventCommonDataModel eventCommonData = null;
                    for (int i = 0; i < _eventCommonDataModels.Count; i++)
                        if (_eventCommonDataModels[i].eventId == _commandsInQueue[_currentCommandIndex].parameters[0])
                        {
                            eventCommonData = _eventCommonDataModels[i];
                            break;
                        }
                    if (eventCommonData != null)
                    {
                        //トリガーが0以外であっても、他のイベントから指定された場合には、そのまま単発で実行する
                        //実行したからと言って、並列実行や自動実行にはならない
                        //そのため、ここではトリガーを見ずに実行を行う

                        // 現在のイベントに追加
                        var eventData = _eventManagementService.LoadEventById(_commandsInQueue[_currentCommandIndex].parameters[0]).DataClone<EventDataModel>();
                        // Indent設定する
                        _eventManagementService.SetEventIndent(eventData);

                        // コモンイベントに先頭末尾用コード追加
                        eventData.eventCommands.Insert(0, new EventDataModel.EventCommand((int) EventEnum.EVENT_CODE_FLOW_COMMON_START,
                            new List<string>(), new List<EventDataModel.EventCommandMoveRoute>()));
                        eventData.eventCommands.Add(new EventDataModel.EventCommand((int) EventEnum.EVENT_CODE_FLOW_COMMON_END,
                            new List<string>(), new List<EventDataModel.EventCommandMoveRoute>()));
                        _commandsInQueue.InsertRange(_currentCommandIndex + 1, eventData.eventCommands);

                        // インデントを適用しなおす
                        int indent = _commandsInQueue[_currentCommandIndex].indent;
                        for (int i = _currentCommandIndex + 1; i <= _currentCommandIndex + eventData.eventCommands.Count; i++)
                        {
                            _commandsInQueue[i].indent += indent;
                        }

                        // 追加した後はコモンイベント自体は削除する
                        _commandsInQueue.RemoveAt(_currentCommandIndex);
                        _currentCommandIndex--;
                    }

                    break;
                case EventEnum.EVENT_CODE_FLOW_JUMP_LABEL:
                    // ラベルジャンプ
                    for (var i = 0; i < _commandsInQueue.Count; i++)
                        // 一致したラベルに飛ぶ（indexを変更する）
                        if (_commandsInQueue[i].code == (int) EventEnum.EVENT_CODE_FLOW_LABEL &&
                            _commandsInQueue[i].parameters[0] == _commandsInQueue[_currentCommandIndex].parameters[0])
                        {
                            _currentCommandIndex = i;
                            break;
                        }

                    break;

                // 何もしない
                case 0:
                // 空
                case EventEnum.EVENT_CODE_FLOW_ANNOTATION:
                // 注釈
                case EventEnum.EVENT_CODE_FLOW_LABEL:
                // 判定終了
                case EventEnum.EVENT_CODE_FLOW_ENDIF:
                    break;
                // バトル関連
                case EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_WIN:
                    //バトル終了後で「バトル勝利時」が存在した場合は、バトルの結果に応じた条件分岐がある
                    //バトルの結果を取得
                    var battleResult = DataManager.Self().BattleResult;
                    if (battleResult == 0)
                    {
                        //勝利時
                        //何もしない
                    }
                    else if (battleResult == 1)
                    {
                        //逃走時
                        //逃走または、バトル終了まで飛ぶ
                        for (var i = _currentCommandIndex; i < _commandsInQueue.Count; i++)
                            if ((_commandsInQueue[i].code ==
                                 (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_ESCAPE ||
                                 _commandsInQueue[i].code == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END) &&
                                _commandsInQueue[i].indent == nowIndent
                            )
                            {
                                _currentCommandIndex = i;
                                break;
                            }
                    }
                    else
                    {
                        //敗北時
                        //敗北または、バトル終了まで飛ぶ
                        for (var i = _currentCommandIndex; i < _commandsInQueue.Count; i++)
                            if ((_commandsInQueue[i].code == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_LOSE ||
                                 _commandsInQueue[i].code == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END) &&
                                _commandsInQueue[i].indent == nowIndent
                            )
                            {
                                _currentCommandIndex = i;
                                break;
                            }
                    }

                    break;
                case EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_ESCAPE:
                case EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_LOSE:
                    // ENDへ飛ぶ
                    for (var i = _currentCommandIndex; i < _commandsInQueue.Count; i++)
                        if (_commandsInQueue[i].code == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END &&
                            _commandsInQueue[i].indent == nowIndent)
                        {
                            _currentCommandIndex = i;
                            break;
                        }

                    break;
                case EventEnum.EVENT_CODE_FLOW_COMMON_START:
                case EventEnum.EVENT_CODE_FLOW_COMMON_END:
                    break;
                default:
                    return false;
            }

            ProcessCommand(_commandEventID);
            return true;
        }
    }
}