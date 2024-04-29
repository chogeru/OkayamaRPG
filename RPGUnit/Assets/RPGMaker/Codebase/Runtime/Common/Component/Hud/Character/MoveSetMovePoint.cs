using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Runtime.Map;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using RPGMaker.Codebase.Runtime.Map.Component.Map;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime.RuntimeOnMapDataModel;
using Random = UnityEngine.Random;

//バトルでは本コマンドは利用しない

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud.Character
{
    /// <summary>
    ///     キャラクターの座標が必要
    ///     キャラクターの画像を変える必要がある
    /// </summary>
    public class MoveSetMovePoint : MonoBehaviour
    {
        // const
        //--------------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// MVの「標準」速度は16フレームで1マス進む。1秒間では3.75マス進む
        /// </summary>
        public const float BaseMoveSpeed = 3.75f;

        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        private int                         _animation; //アニメーション
        private Queue<float>                _beforMoveSpeeds = new Queue<float>(); // 移動速度保持
        private Action                      _closeAction;
        private EventDataModel.EventCommand _command;
        private Commons.Direction.Id        _directionId; //向き
        private bool                        _isActor;
        private bool                        _beforeSlidingThrough;

        // 関数プロパティ
        //--------------------------------------------------------------------------------------------------------------
        private bool                    _endActionIsEvent;
        private Action                  _endAction;
        private Commons.TargetCharacter _targetCharacer; // 対象キャラクター。
        private List<EventDataModel>    _eventList;
        private List<EventMapDataModel> _eventMapList;

        // 初期パラメータ
        private EventDataModel.EventCommand
            _defaultCommand;

        private int _defaultMoveKind;
        private int _defaultMoveSpeed;
        private int  _defaultIndex;
        private int  _defaultIndexMax;
        private bool _defaultRepeatOperation;
        private bool _defaultSave;
        private bool _defaultMoveSkip;
        private Action _defaultEndAction;
        private Action _defaultCloseAction;
        private Action<CharacterMoveDirectionEnum> _checkEventAction;

        private int          _execEventIndex = 0;
        private int          _index;
        private int          _indexMax;
        private MapDataModel _mapDataModel;
        private int          _moveFrequency; //移動頻度
        private int          _moveKind; //ルート指定
        private bool         _moveSkip; //移動できないときは飛ばす
        private int          _moveSpeed; //移動速度インデックス
        private EventOnMap   _parent;
        private bool         _moveSkipNext; //進行方向への移動が出来なかった際に次の移動まで処理を飛ばす 通常の移動用

        // 表示要素プロパティ
        //--------------------------------------------------------------------------------------------------------------
        private GameObject _prefab;

        private EventMapDataModel.EventMapPage.PriorityType
            _priority; //プライオリティ

        private bool               _repeatOperation; //動作を繰り返す
        private bool               _slidingThrough; //すり抜け
        private GameObject         _targetObj;
        private TilesOnThePosition _tilesOnThePosition;

        private bool _waitToggle; //完了までウェイト
        private int _eventCount;
        // enum / interface / local class
        //--------------------------------------------------------------------------------------------------------------

        public static float GetMoveFrequencyWaitTime(int moveFrequencyIndex) {
            float time = 1;
            switch (moveFrequencyIndex)
            {
                case 0:
                    time = time * 2;
                    break;
                case 1:
                    time = time * 1.5f;
                    break;
                case 2:
                    break;
                case 3:
                    time = time / 1.5f;
                    break;
                case 4:
                    time = 0f;
                    break;
            }

            return time;
        }

        // initialize methods
        //--------------------------------------------------------------------------------------------------------------
        /**
         * 初期化
         */
        public void Init() {
            if (_prefab != null) return;
            _prefab = new GameObject();
            _prefab.transform.SetParent(gameObject.transform);
            _tilesOnThePosition = gameObject.AddComponent<TilesOnThePosition>();
        }

        public void SetData(EventDataModel eventData, EventMapDataModel eventMap, EventOnMap parent) {
            _eventList = new List<EventDataModel>();
            _eventMapList = new List<EventMapDataModel>();

            _eventList.Add(eventData);
            _eventMapList.Add(eventMap);

            _parent = parent;
        }

        /// <summary>
        /// イベントコマンドから設定
        /// </summary>
        /// <param name="endAction"></param>
        /// <param name="closeAction"></param>
        /// <param name="command"></param>
        /// <param name="thisEventId"></param>
        /// <param name="isActor"></param>
        public async void SetMovePointProcess(
            Action endAction,
            Action closeAction,
            EventDataModel.EventCommand command,
            string thisEventId,
            bool isActor = false
        ) {
            try 
            {
                _eventCount++;
                int eventCount = _eventCount;

                await Task.Delay(1);

                //別のイベントによって上書きされていた場合には、最初の一歩を行わない
                if (eventCount != _eventCount)
                {
                    return;
                }

                Init();

                _targetCharacer = new Commons.TargetCharacter(command.parameters[0], thisEventId);
                _moveKind = int.Parse(command.parameters[1]);

                if (command.parameters[2] != "-1")
                    _moveSpeed = int.Parse(command.parameters[2]);
                _moveFrequency = int.Parse(command.parameters[3]);
                _directionId = (Commons.Direction.Id) int.Parse(command.parameters[4]);

                _animation = int.Parse(command.parameters[5]);
                _slidingThrough = command.parameters[6] != "0"; //すり抜け

                _repeatOperation = command.parameters[7] != "0"; //繰り返し
                _moveSkip = command.parameters[8] != "0"; //飛ばす
                _waitToggle = command.parameters[9] != "0"; //ウエイト
                _command = command;
                _indexMax = _command.route.Count;
                // 実行中のindexを保存
                if (_defaultSave == false)
                {
                    _defaultSave = true;
                    _defaultIndex = _index;
                }

                TimeHandler.Instance.RemoveTimeAction(CheckMoveProcess);
                TimeHandler.Instance.RemoveTimeAction(RetryMove);
                _execEventIndex++;
                _index = 0;
                _endAction = endAction;
                _endActionIsEvent = true;
                _closeAction = closeAction;
                _targetObj = _targetCharacer.GetGameObject();
                _mapDataModel = MapManager.CurrentMapDataModel;
                _isActor = isActor;

                if (_isActor)
                    _priority = EventMapDataModel.EventMapPage.PriorityType.Normal;

                if (command.parameters[2] != "-1")
                    SetCharactersSpeed();

                SetDirection();
                AnimationSettings();
                ThroughSetting();

                MoveProcess();
            }
            catch (Exception) {}
        }

        /// <summary>
        /// 自律移動から設定
        /// </summary>
        /// <param name="eventMapPage"></param>
        /// <param name="endAction"></param>
        /// <param name="closeAction"></param>
        /// <param name="thisEventId"></param>
        /// <param name="mapDataModel"></param>
        public void SetNpcMove(
            EventMapDataModel.EventMapPage eventMapPage,
            Action endAction,
            Action closeAction,
            Action<CharacterMoveDirectionEnum> checkEventAction,
            string thisEventId,
            MapDataModel mapDataModel
        ) {
            Init();
            _directionId = (Commons.Direction.Id) eventMapPage.image.direction;
            _repeatOperation = eventMapPage.move.repeat == 1;
            _defaultRepeatOperation = _repeatOperation;
            _moveSkip = eventMapPage.move.skip == 1;
            _defaultMoveSkip = _moveSkip;
            _moveSpeed = eventMapPage.move.speed;
            _defaultEndAction = endAction;
            _defaultCloseAction = closeAction;
            _checkEventAction = checkEventAction;

            //Editorでは、0：固定、1：ランダム、2：近づく、3：カスタム
            //Runtimeでは、0：ルート指定、1：ランダム、2：プレイヤーに近づく、3：プレイヤーから遠ざかるとなる
            //書き換える
            if (eventMapPage.move.moveType == 0)
                _moveKind = -1;
            else if (eventMapPage.move.moveType == 3)
                _moveKind = 0;
            else
                _moveKind = eventMapPage.move.moveType;
            _defaultMoveKind = _moveKind;
            _defaultMoveSpeed = _moveSpeed;

            TimeHandler.Instance.RemoveTimeAction(CheckMoveProcess);
            TimeHandler.Instance.RemoveTimeAction(RetryMove);
            _execEventIndex++;

            _priority = eventMapPage.Priority;
            _slidingThrough = _parent.GetTrough();
            _moveFrequency = eventMapPage.move.frequency;
            _targetCharacer = new Commons.TargetCharacter(Commons.TargetType.ThisEvent, thisEventId);
            _targetObj = _targetCharacer.GetGameObject();
            _mapDataModel = mapDataModel;
            _endAction = endAction;
            _endActionIsEvent = false;
            _closeAction = closeAction;
            _isActor = false;

            //詰め変える
            _command = new EventDataModel.EventCommand(0, new List<string>(), eventMapPage.move.route);
            _defaultCommand = _command;
            _indexMax = _command.route.Count;
            _defaultIndexMax = _command.route.Count;
            _defaultSave = false;

            // アニメーションの設定
            if (eventMapPage.walk.walking == 0 && eventMapPage.walk.stepping == 1) _animation = 0;
            else if (eventMapPage.walk.walking == 1 && eventMapPage.walk.stepping == 0) _animation = 1;
            else if (eventMapPage.walk.walking == 1 && eventMapPage.walk.stepping == 1) _animation = 2;
            else _animation = 3;

            SetCharactersSpeed();
            SetDirection();
            AnimationSettings();
            ThroughSetting();

            MoveProcess();
        }

        public void UpdateMove() {
            MoveFrequencyControl();
        }

        private List<CharacterOnMap> GetCharacterOnMaps() {
            List<CharacterOnMap> characterOnMaps = new () {_targetObj?.GetComponent<CharacterOnMap>()};

            // ルート指定＆対象がプレイヤー＆パーティキャラクター有り？
            if (_moveKind == 0 && _targetCharacer != null && _targetCharacer.IsPlayer && MapManager.PartyOnMap != null)
            {
                characterOnMaps.AddRange(MapManager.PartyOnMap);
            }

            return characterOnMaps;
        }

        private void SetCharactersSpeed() {
            var speed = Commons.SpeedMultiple.GetValue((Commons.SpeedMultiple.Id) _moveSpeed) * BaseMoveSpeed;

            _beforMoveSpeeds.Clear();
            foreach (var characterOnMap in GetCharacterOnMaps())
            {
                if (characterOnMap != null)
                {
                    _beforMoveSpeeds.Enqueue(characterOnMap.GetCharacterSpeed());
                    characterOnMap.SetCharacterSpeed(speed);
                }
            }
        }

        public void SetMoveSpeed(int moveSpeed) {
            _moveSpeed = moveSpeed;
        }

        private void RestoreCharactersSpeed() {
            foreach (var characterOnMap in GetCharacterOnMaps())
            {
                if (characterOnMap != null && _beforMoveSpeeds.Count != 0)
                {
                    characterOnMap.SetCharacterSpeed(_beforMoveSpeeds.Dequeue());
                }
            }
        }

        public static float GetMoveSpeed(Commons.SpeedMultiple.Id speedMultipleId) {
            return Commons.SpeedMultiple.GetValue(speedMultipleId) * BaseMoveSpeed;
        }

        private void SetDirection() {
            var characterMoveDirection = Commons.Direction.GetCharacterMoveDirection(_directionId, _targetObj,
                MapManager.GetOperatingCharacterGameObject());
            if (characterMoveDirection != CharacterMoveDirectionEnum.None)
            {
                _targetObj.GetComponent<CharacterOnMap>().ChangeCharacterDirection(characterMoveDirection);
            }
        }

        public void SetAnimationSettings(int walking, int stepping) {
            if (walking == 0 && stepping == 1) _animation = 0;
            else if (walking == 1 && stepping == 0) _animation = 1;
            else if (walking == 1 && stepping == 1) _animation = 2;
            else _animation = 3;

            AnimationSettings();
        }

        private void AnimationSettings() {
            switch (_animation)
            {
                case 0:
                    _targetObj.GetComponent<CharacterOnMap>().SetAnimation(true, false);
                    break;
                case 1:
                    _targetObj.GetComponent<CharacterOnMap>().SetAnimation(false, true);
                    break;
                case 2:
                    _targetObj.GetComponent<CharacterOnMap>().SetAnimation(false, false);
                    break;
                case 3:
                    _targetObj.GetComponent<CharacterOnMap>().SetAnimation(true, true);
                    break;
            }
        }

        private void ThroughSetting() {
            _beforeSlidingThrough = _slidingThrough;
            _targetObj.GetComponent<CharacterOnMap>().SetThrough(_slidingThrough);
        }

        private void RestoreThroughSetting() {
            _targetObj?.GetComponent<CharacterOnMap>().SetThrough(_beforeSlidingThrough);
        }

        private void MoveProcess() {
            if (_targetObj == null)
            {
                return;
            }
            if (_parent?.isValid == false)
            {
                return;
            }

            // タイミングによって以前のマップの移動処理が行われる為判定
            if (_mapDataModel.id != MapManager.CurrentMapDataModel.id)
            {
                return;
            }

            // ルート指定かつ、ルート未設定の場合は、移動処理を行わない
            if (_indexMax == 0 && _moveKind == 0)
            {
                //このケースでは、CBを実行して終了する
                if (_waitToggle)
                {
                    //EndActionを1度でも実行した場合は、2度目以降は実行しない
                    _waitToggle = false;
                    _endAction?.Invoke();
                }
                return;
            }

            var directionEnum = CharacterMoveDirectionEnum.Down;
            var targetPositionOnTile = new Vector2(0, 0);
            var currentPotision = new Vector2(0, 0);
            var events = MapEventExecutionController.Instance.GetEvents();

            string vehicleId = null;
            if (_isActor && MapManager.CurrentVehicleId != "")
            {
                vehicleId = MapManager.CurrentVehicleId;
            }

            switch (_moveKind)
            {
                case 0: //ルート指定
                    if (_index >= _indexMax)
                    {
                        MoveRouteEnd();
                        break;
                    }

                    //すり抜けON
                    if (!_slidingThrough)
                    {
                        var eventMoveEnum = (EventMoveEnum) _command?.route[_index].code;
                        targetPositionOnTile = eventMoveEnum switch
                        {
                            EventMoveEnum.MOVEMENT_MOVE_LEFT => _targetObj.GetComponent<CharacterOnMap>()
                                .GetCurrentPositionOnTile() + Vector2.left,
                            EventMoveEnum.MOVEMENT_MOVE_RIGHT => _targetObj.GetComponent<CharacterOnMap>()
                                .GetCurrentPositionOnTile() + Vector2.right,
                            EventMoveEnum.MOVEMENT_MOVE_UP => _targetObj.GetComponent<CharacterOnMap>()
                                .GetCurrentPositionOnTile() + Vector2.up,
                            EventMoveEnum.MOVEMENT_MOVE_DOWN => _targetObj.GetComponent<CharacterOnMap>()
                                .GetCurrentPositionOnTile() + Vector2.down,
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        if (_tilesOnThePosition == null)
                            _tilesOnThePosition = gameObject.AddComponent<TilesOnThePosition>();

                        _tilesOnThePosition.InitForRuntime(_mapDataModel, targetPositionOnTile);

                        //次に進行する方向（向きではない）
                        directionEnum = eventMoveEnum switch
                        {
                            EventMoveEnum.MOVEMENT_MOVE_LEFT => CharacterMoveDirectionEnum.Left,
                            EventMoveEnum.MOVEMENT_MOVE_RIGHT => CharacterMoveDirectionEnum.Right,
                            EventMoveEnum.MOVEMENT_MOVE_UP => CharacterMoveDirectionEnum.Up,
                            EventMoveEnum.MOVEMENT_MOVE_DOWN => CharacterMoveDirectionEnum.Down,
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        if (!_tilesOnThePosition.CanEnterThisTiles(directionEnum, vehicleId))
                        {
                            //飛ばす
                            if (_moveSkip)
                            {
                                _index++;
                                TimeHandler.Instance.AddTimeActionFrame(6, RetryMove, false);
                                return;
                            }

                            // 飛ばさない場合はリトライ
                            _moveSkipNext = true;
                            _checkEventAction?.Invoke(directionEnum);
                            TimeHandler.Instance.AddTimeActionFrame(6, RetryMove, false);
                            return;
                        }

                        //進行可能判定（プレイヤーと接触しているかどうか）
                        if (!_isActor && !CanMove(directionEnum))
                        {
                            //飛ばす
                            if (_moveSkip)
                            {
                                _index++;
                                TimeHandler.Instance.AddTimeActionFrame(6, RetryMove, false);
                                return;
                            }

                            // 飛ばさない場合はリトライ
                            _moveSkipNext = true;
                            _checkEventAction?.Invoke(directionEnum);
                            TimeHandler.Instance.AddTimeActionFrame(6, RetryMove, false);
                            return;
                        }

                        // 進行可能判定（イベント）
                        for (var i = 0; i < events.Count; i++)
                        {
                            if (events[i].isValid == false) continue;
                            if (this.GetComponent<CharacterOnMap>() != events[i].GetComponent<CharacterOnMap>() &&
                                events[i].GetTrough() == false &&
                                ((!events[i].IsMoving() && new Vector2(events[i].x_now, events[i].y_now) == targetPositionOnTile) ||
                                 (events[i].IsMoving() && new Vector2(events[i].x_next, events[i].y_next) == targetPositionOnTile)) &&
                                events[i].GetPriority() == _priority)
                            {
                                //飛ばす
                                if (_moveSkip)
                                {
                                    _index++;
                                    TimeHandler.Instance.AddTimeActionFrame(6, RetryMove, false);
                                    return;
                                }

                                // 飛ばさない場合はリトライ
                                _moveSkipNext = true;
                                _checkEventAction?.Invoke(directionEnum);
                                TimeHandler.Instance.AddTimeActionFrame(6, RetryMove, false);
                                return;
                            }
                        }
                    }

                    if (_endActionIsEvent && _targetObj.GetComponent<CharacterOnMap>().IsMoving())
                    {
                        MoveEnd();
                    } else
                    switch (_command?.route[_index].code)
                    {
                        case (int) EventMoveEnum.MOVEMENT_MOVE_DOWN:
                            _targetObj.GetComponent<CharacterOnMap>().MoveDownOneUnit(MoveEnd);
                            break;
                        case (int) EventMoveEnum.MOVEMENT_MOVE_LEFT:
                            _targetObj.GetComponent<CharacterOnMap>().MoveLeftOneUnit(MoveEnd);
                            break;
                        case (int) EventMoveEnum.MOVEMENT_MOVE_RIGHT:
                            _targetObj.GetComponent<CharacterOnMap>().MoveRightOneUnit(MoveEnd);
                            break;
                        case (int) EventMoveEnum.MOVEMENT_MOVE_UP:
                            _targetObj.GetComponent<CharacterOnMap>().MoveUpOneUnit(MoveEnd);
                            break;
                    }

                    SetDirection();
                    _index++;
                    break;
                case 1: //random
                    if (_moveSkipNext) return;
                    var rand = Random.Range(0, 4);
                    directionEnum = (CharacterMoveDirectionEnum) rand;
                    targetPositionOnTile = directionEnum switch
                    {
                        CharacterMoveDirectionEnum.Left => _targetObj.GetComponent<CharacterOnMap>()
                            .GetCurrentPositionOnTile() + Vector2.left,
                        CharacterMoveDirectionEnum.Right => _targetObj.GetComponent<CharacterOnMap>()
                            .GetCurrentPositionOnTile() + Vector2.right,
                        CharacterMoveDirectionEnum.Up => _targetObj.GetComponent<CharacterOnMap>()
                            .GetCurrentPositionOnTile() + Vector2.up,
                        CharacterMoveDirectionEnum.Down => _targetObj.GetComponent<CharacterOnMap>()
                            .GetCurrentPositionOnTile() + Vector2.down,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    // イベントが持っている座標データ
                    currentPotision = new Vector2(_targetObj.GetComponent<CharacterOnMap>().x_now,
                        _targetObj.GetComponent<CharacterOnMap>().y_now);
                    currentPotision += directionEnum switch
                    {
                        CharacterMoveDirectionEnum.Left => Vector2.left,
                        CharacterMoveDirectionEnum.Right => Vector2.right,
                        CharacterMoveDirectionEnum.Up => Vector2.up,
                        CharacterMoveDirectionEnum.Down => Vector2.down,
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    if (_tilesOnThePosition == null)
                        _tilesOnThePosition = gameObject.AddComponent<TilesOnThePosition>();
                    _tilesOnThePosition.InitForRuntime(_mapDataModel, targetPositionOnTile);

                    if (!_tilesOnThePosition.CanEnterThisTiles(directionEnum, vehicleId))
                    {
                        // スキップが有効の場合は処理終了
                        if (_moveSkip && _endActionIsEvent)
                        {
                            MoveEnd();
                            return;
                        }

                        // 飛ばさない場合はリトライ
                        _moveSkipNext = true;
                        _checkEventAction?.Invoke(directionEnum);
                        _closeAction.Invoke();
                        TimeHandler.Instance.AddTimeActionFrame(6, RetryMove, false);
                        return;
                    }

                    //進行可能判定（プレイヤーと接触しているかどうか）
                    if (!CanMove(directionEnum))
                    {
                        // スキップが有効の場合は処理終了
                        if (_moveSkip && _endActionIsEvent)
                        {
                            MoveEnd();
                            return;
                        }

                        // 飛ばさない場合はリトライ
                        _moveSkipNext = true;
                        _checkEventAction?.Invoke(directionEnum);
                        _closeAction.Invoke();
                        TimeHandler.Instance.AddTimeActionFrame(6, RetryMove, false);
                        return;
                    }

                    // 進行可能判定（イベント）
                    for (var i = 0; i < events.Count; i++)
                    {
                        if (events[i].isValid == false) continue;

                        if (events[i].GetTrough() == false &&
                            (new Vector2(events[i].x_now, events[i].y_now) == currentPotision ||
                             new Vector2(events[i].x_next, events[i].y_next) == currentPotision) &&
                            events[i].GetPriority() == _priority)
                        {
                            // スキップが有効の場合は処理終了
                            if (_moveSkip && _endActionIsEvent)
                            {
                                MoveEnd();
                                return;
                            }

                            // 飛ばさない場合はリトライ
                            _moveSkipNext = true;
                            _checkEventAction?.Invoke(directionEnum);
                            _closeAction.Invoke();
                            TimeHandler.Instance.AddTimeActionFrame(6, RetryMove, false);
                            return;
                        }
                    }

                    if (_endActionIsEvent && _targetObj.GetComponent<CharacterOnMap>().IsMoving())
                    {
                        MoveEnd();
                    } else
                    switch (directionEnum)
                    {
                        case CharacterMoveDirectionEnum.Down:
                            _targetObj.GetComponent<CharacterOnMap>().MoveDownOneUnit(MoveEnd);
                            break;
                        case CharacterMoveDirectionEnum.Up:
                            _targetObj.GetComponent<CharacterOnMap>().MoveUpOneUnit(MoveEnd);
                            break;
                        case CharacterMoveDirectionEnum.Left:
                            _targetObj.GetComponent<CharacterOnMap>().MoveLeftOneUnit(MoveEnd);
                            break;
                        case CharacterMoveDirectionEnum.Right:
                            _targetObj.GetComponent<CharacterOnMap>().MoveRightOneUnit(MoveEnd);
                            break;
                        default:
                            // 飛ばさない場合はリトライ
                            TimeHandler.Instance.AddTimeActionFrame(6, RetryMove, false);
                            break;
                    }

                    SetDirection();
                    break;
                case 2: //プレイヤーに近づく
                    if (_moveSkipNext) return;
                    if (_targetCharacer.IsPlayer) break;

                    // 方向取得
                    List<CharacterMoveDirectionEnum> directionEnumList = new List<CharacterMoveDirectionEnum>();

                    if (Mathf.Abs(transform.localPosition.x - MapManager.OperatingCharacter.transform.localPosition.x) >
                        Mathf.Abs(transform.localPosition.y - MapManager.OperatingCharacter.transform.localPosition.y))
                    {
                        if (transform.localPosition.x < MapManager.OperatingCharacter.transform.localPosition.x)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Right);
                        else if (transform.localPosition.x > MapManager.OperatingCharacter.transform.localPosition.x)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Left);
                        if (transform.localPosition.y < MapManager.OperatingCharacter.transform.localPosition.y)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Up);
                        else if (transform.localPosition.y > MapManager.OperatingCharacter.transform.localPosition.y)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Down);

                        if (directionEnumList.Count == 0)
                            TimeHandler.Instance.AddTimeActionFrame(6, RetryMove, false);
                    }
                    else
                    {
                        if (transform.localPosition.y < MapManager.OperatingCharacter.transform.localPosition.y)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Up);
                        else if (transform.localPosition.y > MapManager.OperatingCharacter.transform.localPosition.y)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Down);
                        if (transform.localPosition.x < MapManager.OperatingCharacter.transform.localPosition.x)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Right);
                        else if (transform.localPosition.x > MapManager.OperatingCharacter.transform.localPosition.x)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Left);

                        if (directionEnumList.Count == 0)
                            TimeHandler.Instance.AddTimeActionFrame(6, RetryMove, false);
                    }

                    bool flg = false;
                    for (int i = 0; i < directionEnumList.Count; i++)
                    {
                        targetPositionOnTile = directionEnumList[i] switch
                        {
                            CharacterMoveDirectionEnum.Left => _targetObj.GetComponent<CharacterOnMap>()
                                .GetCurrentPositionOnTile() + Vector2.left,
                            CharacterMoveDirectionEnum.Right => _targetObj.GetComponent<CharacterOnMap>()
                                .GetCurrentPositionOnTile() + Vector2.right,
                            CharacterMoveDirectionEnum.Up => _targetObj.GetComponent<CharacterOnMap>()
                                .GetCurrentPositionOnTile() + Vector2.up,
                            CharacterMoveDirectionEnum.Down => _targetObj.GetComponent<CharacterOnMap>()
                                .GetCurrentPositionOnTile() + Vector2.down,
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        // イベントが持っている座標データ
                        currentPotision = new Vector2(_targetObj.GetComponent<CharacterOnMap>().x_now,
                            _targetObj.GetComponent<CharacterOnMap>().y_now);
                        currentPotision += directionEnumList[i] switch
                        {
                            CharacterMoveDirectionEnum.Left => Vector2.left,
                            CharacterMoveDirectionEnum.Right => Vector2.right,
                            CharacterMoveDirectionEnum.Up => Vector2.up,
                            CharacterMoveDirectionEnum.Down => Vector2.down,
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        if (_tilesOnThePosition == null)
                            _tilesOnThePosition = gameObject.AddComponent<TilesOnThePosition>();
                        _tilesOnThePosition.InitForRuntime(_mapDataModel, targetPositionOnTile);

                        if (!_tilesOnThePosition.CanEnterThisTiles(directionEnumList[i], vehicleId))
                        {
                            continue;
                        }

                        //進行可能判定（プレイヤーと接触しているかどうか）
                        if (!CanMove(directionEnumList[i]))
                            continue;

                        // 進行可能判定（イベント）
                        bool flgEvent = false;
                        for (var j = 0; j < events.Count; j++)
                        {
                            if (events[j].isValid == false) continue;
                            if (events[j].GetTrough() == false &&
                                (new Vector2(events[j].x_now, events[j].y_now) == currentPotision ||
                                 new Vector2(events[j].x_next, events[j].y_next) == currentPotision) &&
                                events[j].GetPriority() == _priority)
                            {
                                flgEvent = true;
                                continue;
                            }
                        }

                        if (flgEvent)
                            continue;

                        flg = true;
                        directionEnum = directionEnumList[i];
                        break;
                    }

                    if (!flg)
                    {
                        //向きを変更
                        SetDirection();

                        // スキップが有効の場合は処理終了
                        if (_moveSkip && _endActionIsEvent)
                        {
                            MoveEnd();
                            return;
                        }

                        // 飛ばさない場合はリトライ
                        _moveSkipNext = true;
                        _checkEventAction?.Invoke(CharacterMoveDirectionEnum.Max);
                        _closeAction.Invoke();
                        TimeHandler.Instance.AddTimeActionFrame(6, RetryMove, false);
                        return;
                    }

                    if (_endActionIsEvent && _targetObj.GetComponent<CharacterOnMap>().IsMoving())
                    {
                        MoveEnd();
                    } else
                    if (directionEnum == CharacterMoveDirectionEnum.Left)
                        _targetObj.GetComponent<CharacterOnMap>().MoveLeftOneUnit(MoveEnd);
                    else if (directionEnum == CharacterMoveDirectionEnum.Right)
                        _targetObj.GetComponent<CharacterOnMap>().MoveRightOneUnit(MoveEnd);
                    else if (directionEnum == CharacterMoveDirectionEnum.Up)
                        _targetObj.GetComponent<CharacterOnMap>().MoveUpOneUnit(MoveEnd);
                    else if (directionEnum == CharacterMoveDirectionEnum.Down)
                        _targetObj.GetComponent<CharacterOnMap>().MoveDownOneUnit(MoveEnd);

                    SetDirection();
                    break;
                case 3: //プレイヤーから遠ざかる
                    if (_moveSkipNext) return;
                    if (_targetCharacer.IsPlayer)
                        break;

                    // 方向取得
                    directionEnumList = new List<CharacterMoveDirectionEnum>();

                    if (Mathf.Abs(transform.localPosition.x - MapManager.OperatingCharacter.transform.localPosition.x) >
                        Mathf.Abs(transform.localPosition.y - MapManager.OperatingCharacter.transform.localPosition.y))
                    {
                        if (transform.localPosition.y < MapManager.OperatingCharacter.transform.localPosition.y)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Down);
                        else if (transform.localPosition.y > MapManager.OperatingCharacter.transform.localPosition.y)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Up);
                        if (transform.localPosition.x < MapManager.OperatingCharacter.transform.localPosition.x)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Left);
                        else if (transform.localPosition.x > MapManager.OperatingCharacter.transform.localPosition.x)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Right);

                        if (directionEnumList.Count == 0)
                            TimeHandler.Instance.AddTimeActionFrame(6, RetryMove, false);
                    }
                    else
                    {
                        if (transform.localPosition.x < MapManager.OperatingCharacter.transform.localPosition.x)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Left);
                        else if (transform.localPosition.x > MapManager.OperatingCharacter.transform.localPosition.x)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Right);
                        if (transform.localPosition.y < MapManager.OperatingCharacter.transform.localPosition.y)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Down);
                        else if (transform.localPosition.y > MapManager.OperatingCharacter.transform.localPosition.y)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Up);

                        if (directionEnumList.Count == 0)
                            TimeHandler.Instance.AddTimeActionFrame(6, RetryMove, false);
                    }

                    flg = false;
                    for (int i = 0; i < directionEnumList.Count; i++)
                    {
                        targetPositionOnTile = directionEnumList[i] switch
                        {
                            CharacterMoveDirectionEnum.Left => _targetObj.GetComponent<CharacterOnMap>()
                                .GetCurrentPositionOnTile() + Vector2.left,
                            CharacterMoveDirectionEnum.Right => _targetObj.GetComponent<CharacterOnMap>()
                                .GetCurrentPositionOnTile() + Vector2.right,
                            CharacterMoveDirectionEnum.Up => _targetObj.GetComponent<CharacterOnMap>()
                                .GetCurrentPositionOnTile() + Vector2.up,
                            CharacterMoveDirectionEnum.Down => _targetObj.GetComponent<CharacterOnMap>()
                                .GetCurrentPositionOnTile() + Vector2.down,
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        // イベントが持っている座標データ
                        currentPotision = new Vector2(_targetObj.GetComponent<CharacterOnMap>().x_now,
                            _targetObj.GetComponent<CharacterOnMap>().y_now);
                        currentPotision += directionEnumList[i] switch
                        {
                            CharacterMoveDirectionEnum.Left => Vector2.left,
                            CharacterMoveDirectionEnum.Right => Vector2.right,
                            CharacterMoveDirectionEnum.Up => Vector2.up,
                            CharacterMoveDirectionEnum.Down => Vector2.down,
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        if (_tilesOnThePosition == null)
                            _tilesOnThePosition = gameObject.AddComponent<TilesOnThePosition>();
                        _tilesOnThePosition.InitForRuntime(_mapDataModel, targetPositionOnTile);

                        if (!_tilesOnThePosition.CanEnterThisTiles(directionEnumList[i], vehicleId))
                        {
                            continue;
                        }

                        //進行可能判定（プレイヤーと接触しているかどうか）
                        if (!CanMove(directionEnumList[i]))
                            continue;

                        // 進行可能判定（イベント）
                        bool flgEvent = false;
                        for (var j = 0; j < events.Count; j++)
                        {
                            if (events[j].isValid == false) continue;
                            if (events[j].GetTrough() == false &&
                                (new Vector2(events[j].x_now, events[j].y_now) == currentPotision ||
                                 new Vector2(events[j].x_next, events[j].y_next) == currentPotision) &&
                                events[j].GetPriority() == _priority)
                            {
                                flgEvent = true;
                                continue;
                            }
                        }

                        if (flgEvent)
                            continue;

                        flg = true;
                        directionEnum = directionEnumList[i];
                        break;
                    }

                    if (!flg)
                    {
                        //向きを変更
                        SetDirection();

                        // スキップが有効の場合は処理終了
                        if (_moveSkip && _endActionIsEvent)
                        {
                            MoveEnd();
                            return;
                        }

                        // 飛ばさない場合はリトライ
                        _moveSkipNext = true;
                        _closeAction.Invoke();
                        TimeHandler.Instance.AddTimeActionFrame(6, RetryMove, false);
                        return;
                    }


                    if (_endActionIsEvent && _targetObj.GetComponent<CharacterOnMap>().IsMoving())
                    {
                        MoveEnd();
                    } else
                    if (directionEnum == CharacterMoveDirectionEnum.Left)
                        _targetObj.GetComponent<CharacterOnMap>().MoveLeftOneUnit(MoveEnd);
                    else if (directionEnum == CharacterMoveDirectionEnum.Right)
                        _targetObj.GetComponent<CharacterOnMap>().MoveRightOneUnit(MoveEnd);
                    else if (directionEnum == CharacterMoveDirectionEnum.Up)
                        _targetObj.GetComponent<CharacterOnMap>().MoveUpOneUnit(MoveEnd);
                    else if (directionEnum == CharacterMoveDirectionEnum.Down)
                        _targetObj.GetComponent<CharacterOnMap>().MoveDownOneUnit(MoveEnd);

                    SetDirection();
                    break;
            }

            //ここに到達している場合は、移動しているケース
            //対象がアクターであり、かつイベント実行中の場合には、パーティメンバーも移動する
            if (_isActor && _endActionIsEvent)
            {
                if (MapManager.CurrentVehicleId == "")
                {
                    MapManager.PartyMoveCharacter(false);
                }
                else
                {
                    MapManager.PartyMoveCharacterVehicle();
                }

                //アクターの場合には、マップのループ処理を実施
                MapManager.LoopInstance.MapLoopDirection(
                    MapManager.OperatingCharacter.GetComponent<CharacterOnMap>(),
                    MapManager.OperatingCharacter.GetComponent<CharacterOnMap>().GetLastMoveDirection());
            }
        }

        private void MoveRouteEnd() {
            //繰り返しON
            if (_repeatOperation)
            {
                SetDirection();
                //もう一度最初に
                _index = 0;
                MoveProcess();
                return;
            }

            _endActionIsEvent = false;

            if (_waitToggle)
            {
                //EndActionを1度でも実行した場合は、2度目以降は実行しない
                _waitToggle = false;
                _endAction?.Invoke();
            }

            DefaultSetting();
            RestoreCharactersSpeed();
            RestoreThroughSetting();
        }

        private void RetryMove() {
            TimeHandler.Instance.RemoveTimeAction(RetryMove);
            MoveFrequencyControl();
        }


        private void MoveEnd() {
            //通常の移動時
            if (!_endActionIsEvent)
            {
                MoveFrequencyControl();
            }
            //イベントでの移動時
            else
            {
                //ルート指定の場合は、動作を繰り返す判定も移動時に実施しているため、無条件で呼ぶ
                if (_moveKind == 0)
                {
                    MoveFrequencyControl();
                }
                //それ以外の動作では、動作を繰り返す場合にのみループする
                else if (_repeatOperation)
                {
                    _endActionIsEvent = false;
                    MoveFrequencyControl();
                    //完了までウェイトの場合
                    if (_waitToggle)
                    {
                        //EndActionを1度でも実行した場合は、2度目以降は実行しない
                        _waitToggle = false;
                        _endAction?.Invoke();
                        DefaultSetting();
                    }
                }
                else
                {
                    _endActionIsEvent = false;
                    //完了までウェイトの場合
                    if (_waitToggle)
                    {
                        //EndActionを1度でも実行した場合は、2度目以降は実行しない
                        _waitToggle = false;
                        _endAction?.Invoke();
                        DefaultSetting();
                    }
                }
            }
        }

        // 初期設定に戻す
        private void DefaultSetting() {
            if (_defaultSave == false || _endActionIsEvent == true)
                return;

            _command = _defaultCommand;
            _moveKind = _defaultMoveKind;
            _moveSpeed = _defaultMoveSpeed;
            _index = _defaultIndex;
            _indexMax = _defaultIndexMax;
            _repeatOperation = _defaultRepeatOperation;
            _moveSkip = _defaultMoveSkip;
            _endAction = _defaultEndAction;
            _closeAction = _defaultCloseAction;
            _defaultSave = false;
            MoveFrequencyControl();
        }

        // 移動頻度を制御。
        private void MoveFrequencyControl() {
            float time = GetMoveFrequencyWaitTime(_moveFrequency);

            //移動頻度が最高、かつ前回移動に失敗していない場合
            if (time == 0 && !_moveSkipNext)
            {
                //次の移動が可能な場合、即実行
                //アクターの場合は隊列歩行があるためこの処理を通さない
                if (!(_parent != null && _parent.MapDataModelEvent != null &&
                    MapEventExecutionController.Instance.GetEventExecuting(_parent.MapDataModelEvent.eventId) &&
                    !_endActionIsEvent))
                {
                    if (!_isActor) {
                        MoveProcess();
                    }
                    else
                    {
                        //次の移動が今時点では出来ない場合、若干の待ちを発生させる
                        time = 0.0001f;

                        //わずかな時間のみ待ちたいため、WaitMillisecの方を利用する
                        WaitMillSec(_execEventIndex);
                    }
                }
                else
                {
                    //移動できないことがわかっているため、Retryする
                    TimeHandler.Instance.AddTimeAction(0.1f, RetryMove, false);
                }
                return;
            }

            _moveSkipNext = false;
            TimeHandler.Instance.AddTimeAction(time, CheckMoveProcess, false);
        }

        private async void WaitMillSec(int nowEventIndex) {
            await Task.Delay(1);

            if (nowEventIndex == _execEventIndex)
            {
                CheckMoveProcess();
            }
        }

        public void SetMoveFrequency(int moveFrequency) {
            _moveFrequency = moveFrequency;
        }

        private void CheckMoveProcess() {
            TimeHandler.Instance.RemoveTimeAction(CheckMoveProcess);
            if (_parent != null && _parent.MapDataModelEvent != null &&
                MapEventExecutionController.Instance.GetEventExecuting(_parent.MapDataModelEvent.eventId) &&
                !_endActionIsEvent)
            {
                TimeHandler.Instance.AddTimeActionFrame(6, RetryMove, false);
                return;
            }

            MoveProcess();
        }

        private bool CanMove(CharacterMoveDirectionEnum directionEnum) {
            if (_priority != EventMapDataModel.EventMapPage.PriorityType.Normal) return true;

            var x_move = _parent.x_now;
            var y_move = _parent.y_now;
            if (directionEnum == CharacterMoveDirectionEnum.Left) x_move--;
            if (directionEnum == CharacterMoveDirectionEnum.Right) x_move++;
            if (directionEnum == CharacterMoveDirectionEnum.Up) y_move++;
            if (directionEnum == CharacterMoveDirectionEnum.Down) y_move--;

            var characterOnMap = MapManager.OperatingCharacter.GetComponent<CharacterOnMap>();
            if (characterOnMap.IsMoveCheck(x_move, y_move) && !_slidingThrough)
            {
                return false;
            }

            var partyOnMap = MapManager.OperatingParty;
            if (partyOnMap != null)
            {
                for (int i = 0; i < partyOnMap.Count; i++)
                {
                    var partyChara = partyOnMap[i].GetComponent<CharacterOnMap>();
                    if (partyChara.IsMoveCheck(x_move, y_move) && !_slidingThrough)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void SetThrough(bool isThrough) {
            _slidingThrough = isThrough;
            _beforeSlidingThrough = isThrough; //移動ルートの指定完了後も、すり抜け状態を維持させる。
        }

        public void ResetSetting() {
            DefaultSetting();
            RestoreCharactersSpeed();
            RestoreThroughSetting();
        }

        public void RestartMove() {
            //通常の移動時
            if (!_endActionIsEvent)
            {
                MoveFrequencyControl();
            }
            //イベントでの移動時
            else
            {
                //ルート指定の場合は、動作を繰り返す判定も移動時に実施しているため、無条件で呼ぶ
                if (_moveKind == 0)
                {
                    MoveFrequencyControl();
                }
                //それ以外の動作では、動作を繰り返す場合にのみループする
                else if (_repeatOperation)
                {
                    _endActionIsEvent = false;
                    MoveFrequencyControl();
                    //完了までウェイトの場合
                    if (_waitToggle)
                    {
                        //EndActionを1度でも実行した場合は、2度目以降は実行しない
                        _waitToggle = false;
                        _endAction?.Invoke();
                        DefaultSetting();
                    }
                }
                else
                {
                    _endActionIsEvent = false;
                    //完了までウェイトの場合
                    if (_waitToggle)
                    {
                        //EndActionを1度でも実行した場合は、2度目以降は実行しない
                        _waitToggle = false;
                        _endAction?.Invoke();
                        DefaultSetting();
                    }
                }
            }
        }

        /// <summary>
        /// セーブデータ用の移動データ取得
        /// </summary>
        /// <returns></returns>
        public RuntimeOnMapMoveDataModel GetMapMoveData() {
            RuntimeOnMapMoveDataModel data = new RuntimeOnMapMoveDataModel();

            data.defaultCommand = _defaultCommand;
            data.defaultIndex = _defaultIndex;
            data.defaultIndexMax = _defaultIndexMax;
            data.defaultMoveKind = _defaultMoveKind;
            data.defaultMoveSpeed = _defaultMoveSpeed;
            data.defaultMoveSkip = _defaultMoveSkip;
            data.defaultRepeatOperation = _defaultRepeatOperation;
            data.defaultSave = _defaultSave;

            data.command = _command;
            data.index = _index;
            data.indexMax = _indexMax;
            data.moveKind = _moveKind;
            data.moveSpeed = _moveSpeed;
            data.moveSkip = _moveSkip;
            data.moveSkipNext = _moveSkipNext;
            data.repeatOperation = _repeatOperation;

            return data;
        }

        /// <summary>
        /// コンティニュー用の移動データ復元
        /// </summary>
        /// <param name="data"></param>
        public void SetMapMoveData(RuntimeOnMapMoveDataModel data, EventOnMap eventData, GameObject targetObject) {
            _defaultCommand = data.defaultCommand;
            _defaultIndex = data.defaultIndex;
            _defaultIndexMax = data.defaultIndexMax;
            _defaultMoveKind = data.defaultMoveKind;
            _defaultMoveSkip = data.defaultMoveSkip;
            _defaultRepeatOperation = data.defaultRepeatOperation;
            _defaultSave = data.defaultSave;

            _command = data.command;
            _index = data.index;
            _indexMax = data.indexMax;
            _moveKind = data.moveKind;
            _moveSpeed = data.moveSpeed;
            _moveSkip = data.moveSkip;
            _moveSkipNext = data.moveSkipNext;
            _repeatOperation = data.repeatOperation;

            _mapDataModel = MapManager.CurrentMapDataModel;
            _targetObj = targetObject;
            _parent = eventData;

            MoveFrequencyControl();
        }
    }
}