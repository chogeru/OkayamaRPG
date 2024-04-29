using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventCommon;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using RPGMaker.Codebase.Runtime.Map.Component.Map;
using RPGMaker.Codebase.Runtime.Scene.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using GameState = RPGMaker.Codebase.Runtime.Common.GameStateHandler.GameState;

namespace RPGMaker.Codebase.Runtime.Map
{
    /// <summary>
    /// マップイベントの実行状態を管理するためのクラス
    /// </summary>
    public class MapEventExecutionController
    {
        /// <summary>
        ///     シングルトンのインスタンス
        /// </summary>
        private static MapEventExecutionController instance;

        /// <summary>
        ///     インスタンス返却
        /// </summary>
        public static MapEventExecutionController Instance
        {
            get
            {
                if (instance == null) instance = new MapEventExecutionController();

                return instance;
            }
        }

        /// <summary>
        /// マップ内のイベントリスト
        /// </summary>
        private List<EventOnMap> _eventsOnMap;

        /// <summary>
        /// マップ内のイベントリスト返却
        /// </summary>
        public List<EventOnMap> EventsOnMap { get { return _eventsOnMap; } }

        /// <summary>
        /// 現在、接触又はアクションキーで実行中のイベント
        /// </summary>
        private EventOnMap _executeEventsOnMap;

        /// <summary>
        /// 現在、自動実行中のイベントリスト
        /// </summary>
        private EventOnMap _autoExecuteEventsOnMap;

        /// <summary>
        /// 現在、並列実行中のイベントリスト
        /// </summary>
        private List<EventOnMap> _parallelExecuteEventsOnMap;

        /// <summary>
        /// 同時に1つしか実行できないイベントを実行中かどうか
        /// </summary>
        private bool _isExecuteQueue;

        /// <summary>
        /// 同時に1つしか実行できないイベントの、待ち行列
        /// </summary>
        private List<Action> _eventQueue;

        /// <summary>
        /// MapDataModel
        /// </summary>
        private MapDataModel _mapDataModel;

        /// <summary>
        /// マップ内のEventMapDataModel
        /// </summary>
        private List<EventMapDataModel> _mapEventEntities;

        /// <summary>
        /// マップ内のイベントリスト返却
        /// </summary>
        /// <returns></returns>
        public List<EventOnMap> GetEvents() { return _eventsOnMap; }

        /// <summary>
        /// イベント参照用のEventManagementService
        /// </summary>
        private EventManagementService _eventManagementService;

        /// <summary>
        /// イベントを一時的に中断する場合に、再開する際のCB
        /// </summary>
        private Action _resumeAction;

        /// <summary>
        /// イベントを一時的に中断しているかどうか
        /// </summary>
        private bool _isPause = false;

        /// <summary>
        /// 次のマップへ遷移する際に引き継ぐEventOnMap
        /// </summary>
        private EventOnMap _carryOverEventOnMap;

        /// <summary>
        /// イベント終了から、次のイベント発動までの待ち状態かどうか
        /// </summary>
        private float _waitTime = 0.0f;

        /// <summary>
        /// 並列処理中のカスタム移動のを管理。
        /// </summary>
        private CustomMoveEventManager _customMoveEventManager;
        public CustomMoveEventManager GetCustomMoveEventManager() { return _customMoveEventManager; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize() {
            _resumeAction = null;
            _isPause = false;
            _carryOverEventOnMap = null;
        }

        /// <summary>
        /// 使用効果から実行されたか
        /// </summary>
        private bool _isTraitsCommonEvent = false;


        /// <summary>
        /// マップ内のイベントを初期化
        /// </summary>
        public void InitEvents(MapDataModel mapDataModel, GameObject rootGameObject) {
            //既にマップデータが存在する場合、マップ移動が行われたということ
            //現在実行中のイベントを全て止める
            DestroyAllEvent();

            //初期化処理
            _executeEventsOnMap = null;
            _mapDataModel = mapDataModel;
            _eventManagementService = new EventManagementService();

            _autoExecuteEventsOnMap = null;

            if (_parallelExecuteEventsOnMap == null)
                _parallelExecuteEventsOnMap = new List<EventOnMap>();
            else
                _parallelExecuteEventsOnMap.Clear();

            if (_eventQueue == null)
                _eventQueue = new List<Action>();
            else
                _eventQueue.Clear();

            if (_customMoveEventManager == null)
            {
                _customMoveEventManager = new CustomMoveEventManager();
            } else
            {
                _customMoveEventManager.Init();
            }

            _resumeAction = null;
            _isExecuteQueue = false;

            //イベントデータの読込
            SetEventsOnMap(rootGameObject);
            foreach (var eventOnMap in _eventsOnMap) eventOnMap.SetUp(mapDataModel);
        }

        /// <summary>
        /// マップに存在するイベントを全て生成する
        /// </summary>
        /// <param name="rootGameObject"></param>
        public void SetEventsOnMap(GameObject rootGameObject) {
            _mapEventEntities = _eventManagementService.LoadEventMapEntitiesByMapId(_mapDataModel.id);
            _eventsOnMap = _mapEventEntities.Where(mapEvent => mapEvent.pages.Count > 0).Select(mapEvent =>
            {
                var positionOnTile = new Vector2(mapEvent.x, mapEvent.y);
                var evt = _eventManagementService.LoadEventById(mapEvent.eventId);
                var eventOnMap = new GameObject().AddComponent<EventOnMap>();
                eventOnMap.Init(positionOnTile, mapEvent, evt);
                eventOnMap.gameObject.transform.SetParent(rootGameObject.transform);
                return eventOnMap;
            }).ToList();

            //コモンイベントで、かつ「自動実行」「並列実行」のものは、条件を満たせばあらゆるマップで実行される
            //そのため、_eventOnMap に同様に詰める
            List<EventCommonDataModel> eventCommonDataModels = _eventManagementService.LoadEventCommon();
            foreach (var data in eventCommonDataModels)
            {
                if (data.conditions[0].trigger != (int) CommonEventTriggerEnum.None)
                {
                    //マップに存在するものとして扱う
                    //同マップでは絶対に実行されない位置に移動
                    var positionOnTile = new Vector2(99999999, 99999999);
                    var eventOnMap = new GameObject().AddComponent<EventOnMap>();

                    //EventMapDataModelを新規作成
                    EventMapDataModel eventMapDataModel = new EventMapDataModel();
                    eventMapDataModel.mapId = _mapDataModel.id;
                    eventMapDataModel.eventId = data.eventId;
                    eventMapDataModel.name = data.name;

                    //ページを新規作成
                    eventMapDataModel.pages = new List<EventMapDataModel.EventMapPage>();
                    eventMapDataModel.pages.Add(EventMapDataModel.EventMapPage.CreateDefault());

                    //トリガーを変換して設定
                    if (data.conditions[0].trigger == (int) CommonEventTriggerEnum.Auto)
                        eventMapDataModel.pages[0].eventTrigger = (int) EventTriggerEnum.Auto;
                    else if (data.conditions[0].trigger == (int) CommonEventTriggerEnum.Parallel)
                        eventMapDataModel.pages[0].eventTrigger = (int) EventTriggerEnum.Parallel;

                    //表示する画像は無し
                    eventMapDataModel.pages[0].condition.image.enabled = -1;

                    //発動条件を設定
                    eventMapDataModel.pages[0].condition.switchOne.enabled = 1;
                    eventMapDataModel.pages[0].condition.switchOne.switchId = data.conditions[0].switchId;

                    //コモンイベントのEventDataModel取得
                    var evt = _eventManagementService.LoadEventById(data.eventId);

                    //マップに配置
                    eventOnMap.Init(positionOnTile, eventMapDataModel, evt);
                    eventOnMap.gameObject.transform.SetParent(rootGameObject.transform);
                    eventOnMap.x_now = 99999999;
                    eventOnMap.y_now = 99999999;
                    eventOnMap.x_next = 99999999;
                    eventOnMap.y_next = 99999999;

                    //リストに追加
                    _eventsOnMap.Add(eventOnMap);
                }
            }

            if (_carryOverEventOnMap != null)
            {
                _eventsOnMap.Add(_carryOverEventOnMap);

                //同マップでは絶対に実行されない位置に移動
                _carryOverEventOnMap.SetToPositionOnTile(new Vector2(9999999.9f, 99999999.9f));
                _carryOverEventOnMap.x_now = 99999999;
                _carryOverEventOnMap.y_now = 99999999;
                _carryOverEventOnMap.x_next = 99999999;
                _carryOverEventOnMap.y_next = 99999999;
            }
        }

        /// <summary>
        /// 現在実行中のイベントを一時中断
        /// メニュー表示やバトル表示など、別の画面へ遷移する際に利用
        /// </summary>
        public void PauseEvent(Action callback) {
            _isPause = true;
            _resumeAction = callback;
        }

        /// <summary>
        /// 現在実行中のイベントを一時中断
        /// イベント外からの要求で停止する
        /// </summary>
        public void PauseEvent() {
            _isPause = true;
        }

        /// <summary>
        /// イベントを一時中断中かどうかを返却する
        /// </summary>
        /// <returns></returns>
        public bool IsPauseEvent() {
            return _resumeAction != null;
        }

        /// <summary>
        /// 実行を中断していたイベントの再開
        /// </summary>
        public void ResumeEvent() {
            //イベント再開時に状態を復帰する
            if (_isPause)
            {
                //先にフラグを落とす
                _isPause = false;
                if (_resumeAction != null)
                {
                    //復帰するので、GameStateをEVENTに変更する
                    GameStateHandler.SetGameState(GameState.EVENT);

                    //先にメンバ変数を解放する
                    Action resumeAction = _resumeAction;
                    _resumeAction = null;

                    //実行する
                    resumeAction();
                }
                else
                {
                    if (_eventQueue.Count > 0)
                    {
                        //復帰するので、GameStateをEVENTに変更する
                        GameStateHandler.SetGameState(GameState.EVENT);
                        _eventQueue[0]();
                    }
                }
            }
            else
            {
                //フラグを落とす
                _isPause = false;
            }
        }

        /// <summary>
        /// 同時に1つしか実行できないイベントをキューに貯める
        /// 現在1つも実行していない場合は、即実行する
        /// </summary>
        /// <param name="callback"></param>
        public void AddEventQueue(Action callback) {
            _eventQueue.Add(callback);
            if (!_isExecuteQueue && !IsPauseEvent())
            {
                _isExecuteQueue = true;
                callback();
            }
            else
            {
                _isExecuteQueue = true;
            }
            UpdateGameState();
        }

        /// <summary>
        /// 同時に1つしか実行できないイベントをキューから削除する
        /// キューにイベントがまだある場合には、そのイベントを実行する
        /// </summary>
        /// <param name="callback"></param>
        public void RemoveEventQueue(Action callback) {
            if (_eventQueue.Count > 0)
            {
                if (_eventQueue[0] == callback)
                {
                    _eventQueue.RemoveAt(0);

                    if (_eventQueue.Count == 0)
                    {
                        _isExecuteQueue = false;
                    }
                    else
                    {
                        _eventQueue[0]();
                    }

                    return;
                }
            }

            if (_eventQueue.Contains(callback))
            {
                _eventQueue.Remove(callback);
                if (_eventQueue.Count == 0)
                {
                    _isExecuteQueue = false;
                }
            }
        }

        /// <summary>
        /// 次のマップに遷移する際に、削除しないイベントデータを設定
        /// </summary>
        /// <param name="carryOverEventOnMap"></param>
        public void SetCarryEventOnMap(string eventID) {
            foreach (var data in _eventsOnMap)
            {
                if (data != null && data.EventDataModel != null && data.EventDataModel.id == eventID)
                {
                    _carryOverEventOnMap = data;
                    _carryOverEventOnMap.SetNotEventExecute();
                    SetWait(0.5f);
                    break;
                }
            }
        }

        /// <summary>
        /// 次のマップに遷移する際に、削除しないイベントのGameObjectを返却
        /// </summary>
        /// <returns></returns>
        public bool CheckCarryEventOnMap(string eventID) {
            if (_carryOverEventOnMap == null) return false;

            foreach (var data in _eventsOnMap)
            {
                if (data != null && data.EventDataModel != null && data.EventDataModel.id == eventID)
                {
                    if (_carryOverEventOnMap.EventDataModel.id == data.EventDataModel.id)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }

        /// <summary>
        /// 次のマップに遷移する際に、削除しないイベントのGameObjectを返却
        /// </summary>
        /// <returns></returns>
        public GameObject GetCarryEventOnMap() {
            if (_carryOverEventOnMap == null) return null;
            return _carryOverEventOnMap.gameObject;
        }

        /// <summary>
        /// 次のマップに遷移する際に、削除しないイベントデータを破棄
        /// </summary>
        public void RemoveCarryEventOnMap() {
            if (_executeEventsOnMap == _carryOverEventOnMap)
                _executeEventsOnMap = null;
            if (_autoExecuteEventsOnMap == _carryOverEventOnMap)
                _autoExecuteEventsOnMap = null;
            _carryOverEventOnMap = null;
        }

        /// <summary>
        /// マップに存在するイベントのEventOnMapを返却する
        /// </summary>
        public EventOnMap GetEventOnMap(string eventID) {
            foreach (var data in _eventsOnMap)
            {
                if (data != null && data.EventDataModel != null && data.EventDataModel.id == eventID)
                    return data;
            }

            return null;
        }

        /// <summary>
        /// マップに存在するイベントのGameObjectを返却する
        /// </summary>
        public GameObject GetEventMapGameObject(string eventID) {
            //EventMapDataModel で指定されているケース
            foreach (var data in _eventsOnMap)
            {
                if (data != null && data.MapDataModelEvent != null && data.MapDataModelEvent.eventId == eventID)
                    return data.gameObject;
            }
            //EventDataModel で指定されているケース
            foreach (var data in _eventsOnMap)
            {
                if (data != null && data.EventDataModel != null && data.EventDataModel.id == eventID)
                    return data.gameObject;
            }
            //どちらにも該当がない
            return null;
        }

        /// <summary>
        /// イベントが実行中かどうかの返却
        /// </summary>
        public bool GetEventExecuting(string eventID) {
            //EventMapDataModel で指定されているケース
            foreach (var data in _eventsOnMap)
            {
                if (data != null && data.MapDataModelEvent != null && data.MapDataModelEvent.eventId == eventID)
                    if (_executeEventsOnMap == data || _autoExecuteEventsOnMap == data)
                        return true;
                    else
                        return false;
            }
            //EventDataModel で指定されているケース
            foreach (var data in _eventsOnMap)
            {
                if (data != null && data.EventDataModel != null && data.EventDataModel.id == eventID)
                    if (_executeEventsOnMap == data || _autoExecuteEventsOnMap == data)
                        return true;
                    else
                        return false;
            }
            //どちらにも該当がない
            return false;
        }

        /// <summary>
        /// 待ち時間の初期化
        /// </summary>
        public void SetWait(float time = 0.5f) {
            if (_waitTime < time)
                _waitTime = time;
        }

        /// <summary>
        /// イベントの実行状態更新
        /// </summary>
        public void UpdateEventWatch(Vector2 targetPositionOnTile) {
            if (_eventsOnMap == null)
                return;

            //全イベントのカウント数を初期化する
            for (int i = 0; i < _eventsOnMap.Count; i++)
                if (_eventsOnMap[i] != null) _eventsOnMap[i].InitializeCount();
            _customMoveEventManager.InitializeCount();

            //次イベント実行待ち状態の場合は処理しない
            if (_waitTime > 0.0f)
            {
                _waitTime -= Time.deltaTime;
                if (_waitTime < 0.0f)
                {
                    _waitTime = 0.0f;
                }
                return;
            }

            // 自動実行イベント
            TryToAutoEvent(targetPositionOnTile);

            // 並列処理イベント
            TryToParallelEvent(targetPositionOnTile);

            // コモンイベント
            for (int i = 0; i < _eventsOnMap.Count; i++)
            {
                if (_eventsOnMap[i] != null) _eventsOnMap[i].UpdateCommonEvents();
            }
        }

        /// <summary>
        /// 移動直後に、自動実行イベント、並列処理イベントの発動条件を満たしていた場合には、実行処理を行う
        /// </summary>
        public void TryToAutoEventMoveEnd(Vector2 targetPositionOnTile) {
            // ページ番号の切り替え
            for (int i = 0; i < _eventsOnMap.Count; i++)
            {
                _eventsOnMap[i].UpdateTimeHandler();
            }

            // 自動実行イベント
            TryToAutoEvent(targetPositionOnTile);

            // 並列処理イベント
            TryToParallelEvent(targetPositionOnTile);
        }

        /// <summary>
        /// アクションキーでイベントを実行する
        /// </summary>
        public void TryToTalkToEvent(Vector2 targetPositionOnTile, Vector2 counterOverTargetPositionOnTile, TilesOnThePosition tilesOnThePosition) {
            //次イベント実行待ち状態の場合は処理しない
            if (_waitTime > 0.0f)
            {
                return;
            }

            EventOnMap targetEventOnMap = null;
            for (int i = 0; i < _eventsOnMap.Count; i++)
            {
                if (_eventsOnMap[i].isValid == false)
                {
                    continue;
                }

                // トークイベント
                // 接触イベントについても、プライオリティがプレイヤーと同じイベントは、アクションキーでも動作する
                if ((_eventsOnMap[i].MapDataModelEvent.pages[_eventsOnMap[i].page].eventTrigger == (int) EventTriggerEnum.Talk ||
                     _eventsOnMap[i].MapDataModelEvent.pages[_eventsOnMap[i].page].eventTrigger == (int) EventTriggerEnum.ContactFromThePlayer ||
                     _eventsOnMap[i].MapDataModelEvent.pages[_eventsOnMap[i].page].eventTrigger == (int) EventTriggerEnum.ContactFromTheEvent) &&
                    _eventsOnMap[i].IsPriorityNormal() &&
                    _eventsOnMap[i].CanEventExecute())
                {
                    // キャラと重ならない
                    if (_eventsOnMap[i].IsPriorityNormal() && _eventsOnMap[i].GetTrough() == false &&
                        (_eventsOnMap[i].IsMoving() && _eventsOnMap[i].GetDestinationPositionOnTile() == targetPositionOnTile ||
                         !_eventsOnMap[i].IsMoving() && _eventsOnMap[i].GetCurrentPositionOnTile() == targetPositionOnTile))
                    {
                        targetEventOnMap = _eventsOnMap[i];
                        break;
                    }
                    // キャラと重なる
                    else if (_eventsOnMap[i].IsMoving() && _eventsOnMap[i].GetDestinationPositionOnTile() == targetPositionOnTile ||
                         !_eventsOnMap[i].IsMoving() && _eventsOnMap[i].GetCurrentPositionOnTile() == targetPositionOnTile)
                    {
                        targetEventOnMap = _eventsOnMap[i];
                        break;
                    }
                    // カウンター属性の判定
                    else if (_eventsOnMap[i].IsMoving() && _eventsOnMap[i].GetDestinationPositionOnTile() == counterOverTargetPositionOnTile ||
                         !_eventsOnMap[i].IsMoving() && _eventsOnMap[i].GetCurrentPositionOnTile() == counterOverTargetPositionOnTile)
                    {
                        tilesOnThePosition.InitForRuntime(MapManager.CurrentMapDataModel, targetPositionOnTile);
                        if (tilesOnThePosition.GetHasCounterTile() == true)
                        {
                            targetEventOnMap = _eventsOnMap[i];
                            break;
                        }
                    }
                }
            }

            if (targetEventOnMap != null && _executeEventsOnMap == null)
            {
                //イベント実行
                _executeEventsOnMap = targetEventOnMap;
                //イベントを実行した場合は、対象イベントの向き更新
                targetEventOnMap.LookToPlayerDirection();
                if (targetEventOnMap.ExecuteEvent(EndTriggerEvent, false) == false)
                {
                    //イベントを実行しなかった時は、null初期化
                    //向きを戻す
                    if (_executeEventsOnMap.transform.GetChild(0) != null)
                    {
                        _executeEventsOnMap.ChangeCharacterDirection(_executeEventsOnMap.transform.GetChild(0).GetComponent<CharacterGraphic>().GetTempDirection());
                    }
                    _executeEventsOnMap = null;
                }

                //実行したイベントが終了している場合は、待ちを発生させない
                if (_executeEventsOnMap == null || !targetEventOnMap.IsEventRunning())
                {
                    _waitTime = 0.0f;
                }

                //状態を更新
                UpdateGameState();
            }
        }

        /// <summary>
        /// アクションキーでイベントを実行する
        /// 同一の座標のもののみで判定する
        /// </summary>
        public void TryToTalkToEventSamePoint(Vector2 targetPositionOnTile, TilesOnThePosition tilesOnThePosition) {
            //次イベント実行待ち状態の場合は処理しない
            if (_waitTime > 0.0f)
            {
                return;
            }

            EventOnMap targetEventOnMap = null;
            for (int i = 0; i < _eventsOnMap.Count; i++)
            {
                if (_eventsOnMap[i].isValid == false) continue;

                // キャラと重なるもののみで判定する
                if (!_eventsOnMap[i].IsPriorityNormal() || _eventsOnMap[i].GetTrough() == true)
                {
                    if ((_eventsOnMap[i].IsMoving() && _eventsOnMap[i].GetDestinationPositionOnTile() == targetPositionOnTile ||
                         !_eventsOnMap[i].IsMoving() && _eventsOnMap[i].GetCurrentPositionOnTile() == targetPositionOnTile) &&
                        _eventsOnMap[i].MapDataModelEvent.pages[_eventsOnMap[i].page].eventTrigger == (int) EventTriggerEnum.Talk)
                    {
                        targetEventOnMap = _eventsOnMap[i];
                    }
                }
            }

            if (targetEventOnMap != null && _executeEventsOnMap == null)
            {
                //イベント実行
                _executeEventsOnMap = targetEventOnMap;
                if (targetEventOnMap.ExecuteEvent(EndTriggerEvent, false) == false)
                    _executeEventsOnMap = null;

                //実行したイベントが終了している場合は、待ちを発生させない
                if (_executeEventsOnMap == null || !targetEventOnMap.IsEventRunning())
                {
                    _waitTime = 0.0f;
                }

                //状態を更新
                UpdateGameState();
            }
        }

        /// <summary>
        /// 接触でのイベントを実行する
        /// </summary>
        public void TryToContactFromThePlayerToEvent(Vector2 targetPositionOnTile, Vector2 targetPositionOnTile2, TilesOnThePosition tilesOnThePosition, bool checkForward) {
            EventOnMap targetEventOnMap = null;

            for (int i = 0; i < _eventsOnMap.Count; i++)
            {
                if (_eventsOnMap[i].isValid == false)
                    continue;

                if (_eventsOnMap[i].IsMoving() && _eventsOnMap[i].GetDestinationPositionOnTile() == targetPositionOnTile ||
                    !_eventsOnMap[i].IsMoving() && _eventsOnMap[i].GetCurrentPositionOnTile() == targetPositionOnTile)
                {
                    if (!checkForward && 
                        (_eventsOnMap[i].MapDataModelEvent.pages[_eventsOnMap[i].page].eventTrigger == (int) EventTriggerEnum.ContactFromThePlayer ||
                         _eventsOnMap[i].MapDataModelEvent.pages[_eventsOnMap[i].page].eventTrigger == (int) EventTriggerEnum.ContactFromTheEvent) &&
                        _eventsOnMap[i].CanEventExecute())
                    {
                        targetEventOnMap = _eventsOnMap[i];
                        break;
                    }
                }
                else
                {
                    if ((_eventsOnMap[i].IsMoving() && _eventsOnMap[i].GetDestinationPositionOnTile() == targetPositionOnTile2 ||
                         !_eventsOnMap[i].IsMoving() && _eventsOnMap[i].GetCurrentPositionOnTile() == targetPositionOnTile2) &&
                        checkForward &&
                        (_eventsOnMap[i].MapDataModelEvent.pages[_eventsOnMap[i].page].eventTrigger == (int) EventTriggerEnum.ContactFromThePlayer ||
                         _eventsOnMap[i].MapDataModelEvent.pages[_eventsOnMap[i].page].eventTrigger == (int) EventTriggerEnum.ContactFromTheEvent) &&
                        _eventsOnMap[i].CanEventExecute())
                    {
                        targetEventOnMap = _eventsOnMap[i];
                        break;
                    }
                }
            }

            if (targetEventOnMap != null && _executeEventsOnMap == null)
            {
                //対象イベントの向き更新
                targetEventOnMap.LookToPlayerDirection();

                //イベント実行
                _executeEventsOnMap = targetEventOnMap;
                if (targetEventOnMap.ExecuteEvent(EndTriggerEvent, false) == false)
                {
                    // 向きを戻す
                    if (_executeEventsOnMap.transform.GetChild(0) != null)
                        _executeEventsOnMap.ChangeCharacterDirection(_executeEventsOnMap.transform.GetChild(0).GetComponent<CharacterGraphic>().GetTempDirection());

                    _executeEventsOnMap = null;
                }

                //実行したイベントが終了している場合は、待ちを発生させない
                if (_executeEventsOnMap == null || !targetEventOnMap.IsEventRunning())
                {
                    _waitTime = 0.0f;
                }

                //状態を更新
                UpdateGameState();
            }
        }

        /// <summary>
        /// 接触またはアクションキーでの実行の終了時処理
        /// </summary>
        public void EndTriggerEvent(EventMapDataModel eventMapDataModel, EventDataModel eventDataModel) {
            //接触またはアクションキーイベント終了
            if (_executeEventsOnMap != null)
            {
                if (_executeEventsOnMap.MapDataModelEvent == eventMapDataModel &&
                    _executeEventsOnMap.EventDataModel == eventDataModel)
                {
                    // 向きを戻す
                    if (_executeEventsOnMap.transform.GetChild(0) != null)
                    {
                        _executeEventsOnMap.ChangeCharacterDirection(_executeEventsOnMap.transform.GetChild(0).GetComponent<CharacterGraphic>().GetTempDirection());
                    }
                    // 移動処理がある場合には再開する
                    _executeEventsOnMap.RestartMove();

                    _executeEventsOnMap = null;
                }
            }

            //使用効果から実行されたイベントは配列から削除する
            if (_isTraitsCommonEvent)
            {
                _isTraitsCommonEvent = false;
                _eventsOnMap.Remove(_executeEventsOnMap);
            }

            //状態を更新
            UpdateGameState();
        }

        /// <summary>
        /// イベント実行中で、MAPやバトル側でキー操作を受け付け可能かどうかを返却する
        /// </summary>
        /// <returns></returns>
        public bool CheckRunningEvent() {
            //更新した結果、イベント実行中かどうかを返却する
            if (GameStateHandler.CurrentGameState() == GameState.EVENT) return true;
            return false;
        }

        /// <summary>
        /// 自動実行イベントがあればイベントを開始する
        /// </summary>
        public void TryToAutoEvent(Vector2 targetPositionOnTile) {
            if (_eventsOnMap == null)
                return;

            if (_carryOverEventOnMap != null)
                return;

            for (int i = 0; i < _eventsOnMap.Count; i++)
            {
                //現在有効なイベントでなければ処理終了
                if (_eventsOnMap[i].isValid == false) continue;
                if (!_eventsOnMap[i].CanEventExecute()) continue;
                if (_eventsOnMap[i].CheckValid() == false) continue;
                if (_eventsOnMap[i].MapDataModelEvent.mapId != _mapDataModel.id) continue;

                //トリガーが自動実行でなければ処理終了
                if (!(_eventsOnMap[i].MapDataModelEvent.pages[_eventsOnMap[i].page].eventTrigger == (int) EventTriggerEnum.Auto))
                {
                    //現在有効なイベントであり、自動実行ではない場合、ページが変わって自動実行のイベントではなくなった可能性がある
                    if (_autoExecuteEventsOnMap == _eventsOnMap[i])
                    {
                        _autoExecuteEventsOnMap = null;
                    }
                    continue;
                }

                //既に実行中であれば処理終了
                if (_autoExecuteEventsOnMap == _eventsOnMap[i])
                {
                    break;
                }

                //ここに到達するのは、トリガーが自動実行で、データ配列上最も小さい添え字のもの、かつ現在未実行状態
                //自動実行を行う
                if (!_eventsOnMap[i].ExecuteEvent(EndAutoEvent, true))
                    continue;

                //イベントを実行した結果、そのイベントがすべて実行を終えて終了していた場合は処理を継続
                if (_eventsOnMap[i].isValid == false) 
                    continue;
                if (_eventsOnMap[i].IsEventRunning() == false)
                    continue;

                //イベントを実行した場合には、保持する
                _autoExecuteEventsOnMap = _eventsOnMap[i];

                //状態を変更
                UpdateGameState();

                //自動実行は1つしか行えないため、最初のデータを実行した時点で終了する
                break;
            }
        }

        /// <summary>
        /// スキルやアイテムに使用効果でコモンイベントが付与された場合に実行する
        /// </summary>
        /// <param name="mapDataModelEvent"></param>
        /// <param name="eventDataModel"></param>
        public void TryToTraitsToCommonEvent(EventMapDataModel mapDataModelEvent, EventDataModel eventDataModel) {
            //次イベント実行待ち状態の場合は処理しない
            if (_waitTime > 0.0f)
            {
                return;
            }

            EventOnMap targetEventOnMap = new GameObject().AddComponent<EventOnMap>();
            _eventsOnMap.Add(targetEventOnMap);
            targetEventOnMap.Init(new Vector2(9999, 9999), mapDataModelEvent,eventDataModel);
            
            MenuManager.MenuBase.AllClose();

            if (_executeEventsOnMap == null)
            {
                _isTraitsCommonEvent = true;
                //イベント実行
                _executeEventsOnMap = targetEventOnMap;
                if (targetEventOnMap.ExecuteCommonEvent(EndTriggerEvent, false) == false)
                    _executeEventsOnMap = null;

                //状態を更新
                UpdateGameState();
            }
        }

        /// <summary>
        /// 自動実行の終了時処理
        /// </summary>
        public void EndAutoEvent(EventMapDataModel eventMapDataModel, EventDataModel eventDataModel) {
            //自動実行イベント終了
            if (_autoExecuteEventsOnMap != null)
            {
                //イベントマップが一致している場合、初期化処理を実施
                if (_autoExecuteEventsOnMap.MapDataModelEvent == eventMapDataModel)
                {
                    // 移動処理がある場合には再開する
                    _autoExecuteEventsOnMap.RestartMove();

                    // 初期化
                    _autoExecuteEventsOnMap = null;
                }
            }

            //状態を更新
            UpdateGameState();
        }

        /**
         * 並列実行イベントがあればイベントを開始する
         */
        private void TryToParallelEvent(Vector2 targetPositionOnTile) {
            if (_eventsOnMap == null)
                return;

            for (int i = 0; i < _eventsOnMap.Count; i++)
            {
                //現在有効なイベントでなければ処理終了
                if (_eventsOnMap[i].isValid == false) continue;
                if (!_eventsOnMap[i].CanEventExecute()) continue;
                if (_eventsOnMap[i].CheckValid() == false) continue;
                if (_eventsOnMap[i].MapDataModelEvent.mapId != _mapDataModel.id) continue;

                //トリガーが並列実行でなければ処理終了
                if (!(_eventsOnMap[i].MapDataModelEvent.pages[_eventsOnMap[i].page].eventTrigger == (int) EventTriggerEnum.Parallel))
                {
                    //現在有効なイベントであり、並列実行ではない場合、ページが変わって並列実行のイベントではなくなった可能性がある
                    if (_parallelExecuteEventsOnMap.Contains(_eventsOnMap[i]))
                    {
                        _parallelExecuteEventsOnMap.Remove(_eventsOnMap[i]);
                    }
                    continue;
                }

                //既に実行中であれば処理終了
                if (_parallelExecuteEventsOnMap.Contains(_eventsOnMap[i])) continue;

                //ここに到達するのは、トリガーが並列実行で、現在未実行状態
                if (!_eventsOnMap[i].ExecuteEvent(EndParallelEvent, true))
                    continue;

                //イベントを実行した結果、そのイベントがすべて実行を終えて終了していた場合は処理を継続
                if (_eventsOnMap[i].isValid == false)
                    continue;
                if (_eventsOnMap[i].IsEventRunning() == false)
                    continue;

                //イベントを実行した場合には、保持する
                _parallelExecuteEventsOnMap.Add(_eventsOnMap[i]);

                //状態を更新
                UpdateGameState();
            }
        }

        /// <summary>
        /// 並列実行の終了時処理
        /// </summary>
        /// <param name="eventMapDataModel"></param>
        /// <param name="eventDataModel"></param>
        public void EndParallelEvent(EventMapDataModel eventMapDataModel, EventDataModel eventDataModel) {
            //終了して良ければ、配列から除去
            bool flg = false;
            int num = -1;
            for (int i = 0; i < _eventsOnMap.Count; i++)
            {
                if (_eventsOnMap[i].MapDataModelEvent != eventMapDataModel) continue;

                num = i;

                //並列実行を継続する場合は、indexを初期化してそのまま再実行
                //現在有効なイベントでなければ処理終了
                if (_eventsOnMap[i].isValid == false) break;
                if (!_eventsOnMap[i].CanEventExecute()) break;
                if (_eventsOnMap[i].CheckValid() == false) break;
                if (_eventsOnMap[i].MapDataModelEvent.mapId != _mapDataModel.id) break;

                //トリガーが並列実行でなければ処理終了
                if (!(_eventsOnMap[i].MapDataModelEvent.pages[_eventsOnMap[i].page].eventTrigger == (int) EventTriggerEnum.Parallel))
                {
                    //現在有効なイベントであり、並列実行ではない場合、ページが変わって並列実行のイベントではなくなった可能性がある
                    if (_parallelExecuteEventsOnMap.Contains(_eventsOnMap[i]))
                    {
                        _parallelExecuteEventsOnMap.Remove(_eventsOnMap[i]);
                    }
                    break;
                }

                //ここに到達するのは、トリガーが並列実行で、現在未実行状態
                flg = true;
                break;
            }

            if (num != -1)
            {
                //イベントを削除
                _parallelExecuteEventsOnMap.Remove(_eventsOnMap[num]);
                if (flg)
                {
                    //並列イベントを、少し待ってから再実行
                    RestartParallelEvent(eventMapDataModel, eventDataModel);
                }
            }

            //状態を更新
            UpdateGameState();
        }

        private async void RestartParallelEvent(EventMapDataModel eventMapDataModel, EventDataModel eventDataModel) {
            //並列イベントは同時に何個も動作するため、TimeHandlerを利用せずに直接awaitする
            await Task.Delay(1);

            //少し待った後、イベント実行不可であれば処理しない
            try
            {
                if (TimeHandler.Instance != null || !TimeHandler.Instance.CanExecute() || _eventsOnMap == null)
                {
                    return;
                }
            } catch (Exception)
            {
                return;
            }

            //イベントを探す
            for (int i = 0; i < _eventsOnMap.Count; i++)
            {
                if (_eventsOnMap[i].MapDataModelEvent != eventMapDataModel) continue;
                if (_eventsOnMap[i].EventDataModel != eventDataModel) continue;

                //メニュー表示中のケースでは、並列処理を一旦配列から削除する
                if (GameStateHandler.IsMenu())
                {
                    _parallelExecuteEventsOnMap.Remove(_eventsOnMap[i]);
                }

                //イベントを再実行する
                if (_eventsOnMap[i].ExecuteEvent(EndParallelEvent, true))
                {
                    //イベントを実行した場合には、保持する
                    _parallelExecuteEventsOnMap.Add(_eventsOnMap[i]);
                }
                break;
            }
        }

        /// <summary>
        /// ゲームの状態を更新する
        /// </summary>
        public void UpdateGameState() {
            //イベントからメニューを一時的に表示しているケース等では、GameStateは変更しない
            if (GameStateHandler.CurrentGameState() != GameState.MAP &&
                GameStateHandler.CurrentGameState() != GameState.EVENT)
            {
                return;
            }

            //イベントでユーザー入力待ち状態が発生している場合は、EVENT
            //これには並列処理のイベントも含まれる
            if (_isExecuteQueue)
            {
                GameStateHandler.SetGameState(GameState.EVENT);
                return;
            }

            //接触又はアクションキーで実行中のイベントがある or 自動実行中のイベントがある場合は、EVENT
            if (_executeEventsOnMap != null || _autoExecuteEventsOnMap != null)
            {
                if (GameStateHandler.IsMap())
                {
                    GameStateHandler.SetGameState(GameState.EVENT);
                    return;
                }
            }

            //前マップから継続中のEVENTがある場合は、EVENT
            if (_carryOverEventOnMap != null)
            {
                GameStateHandler.SetGameState(GameState.EVENT);
                return;
            }

            //イベント終了後の待ち状態の場合は、未だEVENT
            if (_waitTime > 0.0f)
            {
                GameStateHandler.SetGameState(GameState.EVENT);
                return;
            }

            //その他のケースはMAP
            GameStateHandler.SetGameState(GameState.MAP);
        }

        /// <summary>
        /// 指定座標に存在するイベントを返却する
        /// プレイヤーと統一の高さのイベントのみを対象とする
        /// </summary>
        public EventOnMap GetEventPoint(Vector2 targetPositionOnTile) {
            for (int i = 0; i < _eventsOnMap.Count; i++)
            {
                if (_eventsOnMap[i].isValid && _eventsOnMap[i].GetTrough() == false &&
                    _eventsOnMap[i].IsPriorityNormal() &&
                    (_eventsOnMap[i].IsMoving() &&
                     (int)_eventsOnMap[i].GetDestinationPositionOnTile().x == (int)targetPositionOnTile.x &&
                     (int) _eventsOnMap[i].GetDestinationPositionOnTile().y == (int) targetPositionOnTile.y ||
                     !_eventsOnMap[i].IsMoving() &&
                     (int) _eventsOnMap[i].GetCurrentPositionOnTile().x == (int) targetPositionOnTile.x &&
                     (int) _eventsOnMap[i].GetCurrentPositionOnTile().y == (int) targetPositionOnTile.y))
                {
                    return _eventsOnMap[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 全てのイベントを破棄する
        /// </summary>
        public void DestroyAllEvent() {
            if (_eventsOnMap == null) return;

            //先に初期化処理
            if (_carryOverEventOnMap != _executeEventsOnMap)
                _executeEventsOnMap = null;
            _eventManagementService = new EventManagementService();

            if (_carryOverEventOnMap != _autoExecuteEventsOnMap)
                _autoExecuteEventsOnMap = null;
            _parallelExecuteEventsOnMap.Clear();
            _eventQueue.Clear();

            _resumeAction = null;
            if (_carryOverEventOnMap == null)
                _isExecuteQueue = false;

            //イベントデータ破棄
            foreach (var eventOnMap in _eventsOnMap)
                if (eventOnMap != _carryOverEventOnMap)
                    eventOnMap.DestroyEvent();

            _customMoveEventManager.DestroyEvent();
        }

        /// <summary>
        /// コンティニュー用の実行中イベント復元
        /// </summary>
        /// <param name="data"></param>
        public void SetExecuteEventsOnMap(EventOnMap data) {
            _executeEventsOnMap = data;
        }

        /// <summary>
        /// コンティニュー用の自動実行イベント復元
        /// </summary>
        /// <param name="data"></param>
        public void SetAutoExecuteEventsOnMap(EventOnMap data) {
            _autoExecuteEventsOnMap = data;
        }

        /// <summary>
        /// コンティニュー用の並列実行イベント復元
        /// </summary>
        /// <param name="data"></param>
        public void SetParallelEventsOnMap(EventOnMap data) {
            _parallelExecuteEventsOnMap.Add(data);
        }

        /// <summary>
        /// セーブ、コンティニュー用の、現在実行中のイベント種別取得
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int GetEventType(EventOnMap data) {
            if (_executeEventsOnMap == data)
            {
                return 1;
            }
            else if (_autoExecuteEventsOnMap == data)
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }
    }
}