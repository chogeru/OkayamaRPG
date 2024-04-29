using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Event;
using RPGMaker.Codebase.Runtime.Map;
using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.Runtime.Scene.Map
{
    /// <summary>
    /// イベント実行処理
    /// </summary>
    public class EventCommandChainLauncher : AbstractEventCommandChainLauncher
    {
        /// <summary>
        /// 自動実行又は、並列処理のイベントかどうかのフラグ
        /// </summary>
        private bool _autoEvent;
        /// <summary>
        /// 実行しているEventMapDataModel
        /// </summary>
        private EventMapDataModel _eventMapDataModel;
        /// <summary>
        /// 実行しているEventDataModel
        /// </summary>
        private EventDataModel _eventDataModel;
        // properties
        /// <summary>
        /// ショップに陳列するアイテムリスト
        /// </summary>
        private List<EventDataModel.EventCommand> _shopItemList;
        /// <summary>
        /// イベントキューへ登録中かどうか
        /// </summary>
        private bool _isCallback = false;
        /// <summary>
        /// ウェイトを挟まずに実行しているカウント数
        /// </summary>
        protected int _loopCount;
        /// <summary>
        /// 連続で実行する最大のイベント数
        /// 本数値に達した場合は1ms待ってから続きのイベントを実行する
        /// この数値はあまり上げずに、並列で実行されているイベントを短時間（1msの範囲内）で切り替える方が良い
        /// </summary>
        const int MAX_LOOP_PER_FRAME = 100;
        /// <summary>
        /// イベント終了時に実行するCB
        /// </summary>
        protected Func<bool> _checkValid;

        /// <summary>
        ///     コンストラクタ
        /// </summary>
        public EventCommandChainLauncher(Func<bool> checkValid = null) {
            _eventManagementService = new EventManagementService();
            _eventCommonDataModels = _eventManagementService.LoadEventCommon();
            _checkValid = checkValid;
        }

        /// <summary>
        ///     ショップの陳列データを送る
        /// </summary>
        /// <returns></returns>
        public List<EventDataModel.EventCommand> ShopItemList() {
            return _shopItemList;
        }

        /// <summary>
        ///     イベントコマンドチェーンを開始する
        /// </summary>
        /// <param name="mapDataModelEvent">EventMapDataModel</param>
        /// <param name="evt">EventDataModel</param>
        /// <param name="callback">イベント終了時に実行するコールバック</param>
        /// <param name="autoEvent">自動実行のイベントかどうかのフラグ</param>
        public bool LaunchCommandChain(
            EventMapDataModel mapDataModelEvent,
            EventDataModel evt,
            [CanBeNull] Action<EventMapDataModel, EventDataModel> callback = null,
            bool autoEvent = false,
            [CanBeNull] Action<string> executeCommonEvent = null
        ) {
            // 実行中のイベントが再度登録された際は無視する
            if (_eventMapDataModel != null && _eventMapDataModel.eventId == mapDataModelEvent.eventId && _running) 
                return false;

            // 実行するイベントの内容が無ければ無視する
            if (evt.eventCommands == null)
                return false;

            // 実行するイベントの中身が全てcode=0の場合も無視する
            bool flg = false;
            for (int i = 0; i < evt.eventCommands.Count; i++)
            {
                if (evt.eventCommands[i].code != 0)
                {
                    flg = true;
                    break;
                }
            }
            if (!flg)
                return false;

            //イベントコマンドに対して、indentを設定
            _eventManagementService.SetEventIndent(evt);

            //イベント初期化
            _eventMapDataModel = mapDataModelEvent;
            _eventDataModel = evt;
            _autoEvent = autoEvent;
            _callback = callback;
            _executeCommonEvent = executeCommonEvent;
            _commandsInQueue = evt.eventCommands.DataClone();
            _commandEventID = evt.id;
            _currentCommandIndex = -1;
            if (_running == false)
            {
                _running = true;
                _loopCount = 0;
                ProcessCommand();
            }
            return true;
        }

        /// <summary>
        ///     イベントコマンドを実行する.
        /// </summary>
        /// <param name="eventId">イベントID</param>
        public override void ProcessCommand(string eventId) {
            if (_commandEventID != eventId) return;

            //1フレーム当たりで連続実行するイベント数を限定する
            if (_loopCount > MAX_LOOP_PER_FRAME)
            {
                TimeHandler.Instance.WaitMillisec(ProcessCommand);
                return;
            }
            _loopCount++;

            _currentCommandIndex++;
            if (_currentCommandIndex >= _commandsInQueue.Count ||
                _running == false)
            {
                // イベントコマンドチェーンの終了
                if (_autoEvent == false)
                    EndDelayCommandChain();
                else
                    EndAutoCommandChain();

                // もしもキューが削除できていなければ、削除する
                if (_isCallback)
                {
                    MapEventExecutionController.Instance.RemoveEventQueue(ProcessCommandExecute);
                    _isCallback = false;
                }
                return;
            }

            var targetCommand = _commandsInQueue[_currentCommandIndex];
            var code = (EventEnum) Enum.ToObject(typeof(EventEnum), targetCommand.code);
            _eventCode = (int) code;
            var nowIndent = _commandsInQueue[_currentCommandIndex].indent;

            //直前に実行していたイベント
            int eventCodeBef = -1;
            if (_currentCommandIndex >= 1)
                eventCodeBef = (int) _commandsInQueue[_currentCommandIndex - 1].code;

            //直前に実行していたイベントが、1つしか同時に実行できないイベントかどうか
            if (_isCallback && 
                (eventCodeBef == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_SELECTED || //選択肢の分岐
                eventCodeBef == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_CANCELED || //選択肢の分岐（キャンセル）
                eventCodeBef == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_END || //選択肢の終了
                eventCodeBef == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG || //バトル開始
                eventCodeBef == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_WIN || //バトル勝利
                eventCodeBef == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_LOSE || //バトル敗北
                eventCodeBef == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_ESCAPE || //バトル逃走
                eventCodeBef == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END || //バトル終了
                eventCodeBef == (int) EventEnum.EVENT_CODE_MESSAGE_TEXT_SCROLL_ONE_LINE || //メッセージのスクロールの本文
                eventCodeBef == (int) EventEnum.EVENT_CODE_MESSAGE_TEXT_ONE_LINE || //文章表示の本文
                eventCodeBef == (int) EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG_LINE || //ショップの購入物
                eventCodeBef == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_NUMBER || //数値入力
                eventCodeBef == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_ITEM ||  //アイテム選択
                eventCodeBef == (int) EventEnum.EVENT_CODE_SCENE_INPUT_NAME || //名前入力
                eventCodeBef == (int) EventEnum.EVENT_CODE_SCENE_MENU_OPEN || //メニュー表示
                eventCodeBef == (int) EventEnum.EVENT_CODE_SCENE_SAVE_OPEN || //セーブ画面表示
                eventCodeBef == (int) EventEnum.EVENT_CODE_SCENE_GAME_OVER || //GAMEOVER表示
                eventCodeBef == (int) EventEnum.EVENT_CODE_SCENE_GOTO_TITLE //タイトルに戻る
                //eventCodeBef == (int) EventEnum.EVENT_CODE_MOVE_PLACE //場所移動
            ))
            {
                //このようなイベントの場合には、イベントの終了を通知する
                MapEventExecutionController.Instance.RemoveEventQueue(ProcessCommandExecute);
                _isCallback = false;
            }
            //いずれか1つしか同時に実行できないイベントかどうか
            if (!_isCallback &&
                (_eventCode == (int) EventEnum.EVENT_CODE_MESSAGE_TEXT ||
                _eventCode == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT ||
                _eventCode == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_NUMBER ||
                _eventCode == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_ITEM ||
                _eventCode == (int) EventEnum.EVENT_CODE_MESSAGE_TEXT_SCROLL ||
                _eventCode == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG ||
                _eventCode == (int) EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG ||
                _eventCode == (int) EventEnum.EVENT_CODE_SCENE_INPUT_NAME ||
                _eventCode == (int) EventEnum.EVENT_CODE_SCENE_MENU_OPEN ||
                _eventCode == (int) EventEnum.EVENT_CODE_SCENE_SAVE_OPEN ||
                _eventCode == (int) EventEnum.EVENT_CODE_SCENE_GAME_OVER ||
                _eventCode == (int) EventEnum.EVENT_CODE_SCENE_GOTO_TITLE ||
                _eventCode == (int) EventEnum.EVENT_CODE_MOVE_PLACE
            ))
            {
                //このようなイベントの場合には、イベントの開始タイミングが来るまで待つ
                MapEventExecutionController.Instance.AddEventQueue(ProcessCommandExecute);
                _isCallback = true;
                return;
            }

            //マップで、実行しないイベントがある
            if (!EventCodeList.CheckEventCodeExecute(_eventCode, EventCodeList.EventType.Map, true))
            {
                ProcessCommand(_commandEventID);
                return;
            }

            if (FlowControl(code, _eventMapDataModel.eventId) == false)
            {
                // 選択肢
                if (code == EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT)
                {
                    _commandProcessors[code].Invoke(this, _commandEventID, targetCommand, id => { ProcessCommand(id); },
                        _commandsInQueue);
                }
                // 選択肢関係
                else if (code == EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_CANCELED ||
                         code == EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_END ||
                         code == EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_SELECTED)
                {
                    for (var i = _currentCommandIndex; i < _commandsInQueue.Count; i++)
                        if (_commandsInQueue[i].code == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_END &&
                            _commandsInQueue[i].indent == nowIndent)
                        {
                            _currentCommandIndex = i;
                            break;
                        }

                    ProcessCommand(_commandEventID);
                }
                // 一旦ショップのアイテムリストは飛ばす
                else if (code == EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG_LINE)
                {
                    ProcessCommand(_commandEventID);
                }
                else
                {
                    try
                    {
                        _commandProcessors[code].Invoke(this, _commandEventID, targetCommand, ProcessCommand);
                    }
                    catch (Exception)
                    {
                        //イベントコマンドチェーンの終了
                        EndDelayCommandChain();
                    }
                }
            }
        }

        public override void ProcessCommandExecute() {
            if (_running == false)
            {
                MapEventExecutionController.Instance.RemoveEventQueue(ProcessCommandExecute);
                return;
            }
            var targetCommand = _commandsInQueue[_currentCommandIndex];
            var code = (EventEnum) Enum.ToObject(typeof(EventEnum), targetCommand.code);
            var nowIndent = _commandsInQueue[_currentCommandIndex].indent;

            if (FlowControl(code, _eventMapDataModel.eventId) == false)
            {
                // 選択肢
                if (code == EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT)
                {
                    _commandProcessors[code].Invoke(this, _commandEventID, targetCommand, id => { ProcessCommand(id); },
                        _commandsInQueue);
                }
                // 選択肢関係
                else if (code == EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_CANCELED ||
                         code == EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_END ||
                         code == EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_SELECTED)
                {
                    for (var i = _currentCommandIndex; i < _commandsInQueue.Count; i++)
                        if (_commandsInQueue[i].code == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_END &&
                            _commandsInQueue[i].indent == nowIndent)
                        {
                            _currentCommandIndex = i;
                            break;
                        }

                    ProcessCommand(_commandEventID);
                }
                // 一旦ショップのアイテムリストは飛ばす
                else if (code == EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG_LINE)
                {
                    ProcessCommand(_commandEventID);
                }
                else
                {
                    try
                    {
                        _commandProcessors[code].Invoke(this, _commandEventID, targetCommand, ProcessCommand);
                    }
                    catch (Exception)
                    {
                        //イベントコマンドチェーンの終了
                        EndDelayCommandChain();
                    }
                }
            }
        }

        /// <summary>
        ///     イベント終了処理. 待ち時間あり.
        /// </summary>
        private void EndDelayCommandChain() {
            // マップ移動時にこのイベントを保持していた場合は破棄
            if (MapEventExecutionController.Instance.CheckCarryEventOnMap(_commandEventID))
            {
                MapEventExecutionController.Instance.RemoveCarryEventOnMap();
            }
            //次イベント発動まで待ち
            MapEventExecutionController.Instance.SetWait(0.1f);
            //このイベントは終了
            _running = false;
            //CB実施
            _callback?.Invoke(_eventMapDataModel, _eventDataModel);
        }

        /// <summary>
        ///     自動実行、並列実行処理終了
        /// </summary>
        public void EndAutoCommandChain() {
            //このイベントが現在も有効かどうかのチェック
            bool isValid = true;
            if (_checkValid != null)
            {
                isValid = _checkValid();
            }
            // マップ移動時にこのイベントを保持していた場合は破棄
            if (MapEventExecutionController.Instance.CheckCarryEventOnMap(_commandEventID))
            {
                MapEventExecutionController.Instance.RemoveCarryEventOnMap();
            }

            //このイベントは終了
            _running = false;
            //CB実施
            _callback?.Invoke(_eventMapDataModel, _eventDataModel);
        }

        /// <summary>
        ///     マップを移動した等の理由で、イベントを完全に消去する
        /// </summary>
        public void ExitCommandChain() {
            // マップ移動時にこのイベントを保持していた場合は破棄
            _currentCommandIndex = 999999999;
            _running = false;
        }

        /// <summary>
        ///     ショップに関するイベント
        /// </summary>
        public void ShopEvent() {
            var shopItemList = new List<EventDataModel.EventCommand>();
            shopItemList.Clear();

            //ショップの開始地点から陳列リスト終了までを読み込む
            for (var i = _currentCommandIndex; i < _commandsInQueue.Count; i++)
            {
                //ショップイベントかの判定
                var codeTmp = _commandsInQueue[i].code;
                if ((codeTmp == (int) EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG && i == _currentCommandIndex) ||
                    codeTmp == (int) EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG_LINE)
                {
                    //商品の仕入れ
                    shopItemList.Add(_commandsInQueue[i]);
                }
                else
                {
                    //仕入れの終了
                    _currentCommandIndex = i - 1;
                    break;
                }
            }

            _shopItemList = shopItemList;
        }

        //イベントのcode「0」の時に「EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END(604)」までスキップする
        public void EventGapExecution() {
            //呼ばれた地点から終了の部分までを読み込む
            for (var i = _currentCommandIndex; i < _commandsInQueue.Count; i++)
            {
                //「EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END」かの判定
                var codeTmp = _commandsInQueue[i].code;
                if (codeTmp == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END)
                    //EVENT_CODE_SCENE_SET_BATTLE_CONFIG_ENDにイベントを移行
                    SetIndex(i);
            }
        }

        /// <summary>
        /// ループカウントの初期化
        /// </summary>
        public void InitializeCount() {
            _loopCount = 0;
        }

        public bool IsCustomMoveSkipIfUnabletoMove() {
            if (_eventDataModel.eventCommands.Count > 0 && _eventDataModel.eventCommands[0].code == (int) EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE)
            {
                var parameters = _eventDataModel.eventCommands[0].parameters;
                if (parameters.Count >= 6)
                {
                    return (parameters[3] == "1");
                }
            }
            return true;
        }

        public string GetCustomMoveTargetEvent() {
            if (_eventDataModel.eventCommands.Count > 0 && _eventDataModel.eventCommands[0].code == (int) EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE)
            {
                var parameters = _eventDataModel.eventCommands[0].parameters;
                return parameters[1];
            }
            return null;
        }

        /// <summary>
        ///     イベントコマンドチェーンを復元する
        /// </summary>
        /// <param name="mapDataModelEvent">EventMapDataModel</param>
        /// <param name="evt">EventDataModel</param>
        /// <param name="callback">イベント終了時に実行するコールバック</param>
        /// <param name="autoEvent">自動実行のイベントかどうかのフラグ</param>
        public bool ResumeCommandChain(
            EventMapDataModel mapDataModelEvent,
            EventDataModel evt,
            [CanBeNull] Action<EventMapDataModel, EventDataModel> callback,
            bool autoEvent,
            int index
        ) {
            // 実行中のイベントが再度登録された際は無視する
            if (_eventMapDataModel != null && _eventMapDataModel.eventId == mapDataModelEvent.eventId && _running)
                return false;

            // 実行するイベントの内容が無ければ無視する
            if (evt.eventCommands == null)
                return false;

            // 実行するイベントの中身が全てcode=0の場合も無視する
            bool flg = false;
            for (int i = 0; i < evt.eventCommands.Count; i++)
            {
                if (evt.eventCommands[i].code != 0)
                {
                    flg = true;
                    break;
                }
            }
            if (!flg)
                return false;

            //イベントコマンドに対して、indentを設定
            _eventManagementService.SetEventIndent(evt);

            //イベント初期化
            _eventMapDataModel = mapDataModelEvent;
            _eventDataModel = evt;
            _autoEvent = autoEvent;
            _callback = callback;
            _commandsInQueue = evt.eventCommands.DataClone();
            _commandEventID = evt.id;
            _currentCommandIndex = index;
            if (_running == false)
            {
                _running = true;
                _loopCount = 0;
                ProcessCommand();
            }

            return true;
        }
    }
}