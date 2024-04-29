using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Runtime.Map;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using RPGMaker.Codebase.Runtime.Map.Component.Map;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using _Tuple = System.Tuple<RPGMaker.Codebase.CoreSystem.Knowledge.Enum.CharacterMoveDirectionEnum, UnityEngine.Vector2>;
using Random = UnityEngine.Random; //バトルでは本コマンドは利用しない

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud.Character
{
    /// <summary>
    ///     キャラクターの座標が必要
    ///     キャラクターの画像を変える必要がある
    /// </summary>
    public class StepMoveController : MonoBehaviour
    {
        // const
        //--------------------------------------------------------------------------------------------------------------
        private const float MoveSpeedRate = 0.05f;
        private const float MinJumpHeight = 0.5f;
        private const float JumpHeightParDistance = 0.25f;

        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        private GameObject                  _actorObj;
        private Queue<float>                _beforeMoveSpeeds = new Queue<float>(); // 移動速度保持
        private Action                      _closeAction;

        // 関数プロパティ
        //--------------------------------------------------------------------------------------------------------------
        private Action                  _endAction;
        private Commons.TargetCharacter _targetCharacer;    // 対象キャラクター。

        private MapDataModel _mapDataModel;
        private bool         _moveSkip; //移動できないときは飛ばす
        private EventOnMap   _parent;
        private CharacterOnMap _characterOnMap;
        private List<CharacterOnMap> _partyCharactersOnMap;
        private EventMoveEnum _eventMove;

        // 表示要素プロパティ
        //--------------------------------------------------------------------------------------------------------------
        private EventMapDataModel.EventMapPage.PriorityType
                                    _priority; //プライオリティ
        private GameObject         _targetObj;
        private TilesOnThePosition _tilesOnThePosition;

        private Vector2 _destTilePos;

        private List<JumpCharacter> _jumpCharacters;
        private CharacterOnMap firstJumpCharacterOnMap => _jumpCharacters[0].CharacterOnMap;

        // initialize methods
        //--------------------------------------------------------------------------------------------------------------
        /**
         * 初期化
         */
        public void Init() {
            _actorObj = MapManager.GetOperatingCharacterGameObject();
            _tilesOnThePosition = gameObject.AddComponent<TilesOnThePosition>();
        }

        public void StartStepMove(
            Action endAction,
            Action closeAction,
            string thisEventId,
            EventMoveEnum eventMove,
            int positionX,
            int positionY,
            bool skipIfUnableToMove
        ) {
            Init();

            _targetCharacer = new Commons.TargetCharacter(thisEventId);

            _moveSkip = skipIfUnableToMove;
            _endAction = endAction;
            _closeAction = closeAction;
            _targetObj = _targetCharacer.GetGameObject();
            _mapDataModel = MapManager.CurrentMapDataModel;
            _parent = _targetObj.GetComponent<EventOnMap>();
            _priority = (_parent != null) ? _parent.MapDataModelEvent.pages[_parent.page].Priority : EventMapDataModel.EventMapPage.PriorityType.Normal;
            _characterOnMap = _targetObj.GetComponent<CharacterOnMap>();
            _partyCharactersOnMap = new List<CharacterOnMap> { _actorObj.GetComponent<CharacterOnMap>() };
            foreach (var partyIndex in Enumerable.Range(0, MapManager.GetPartyMemberNum()))
            {
                _partyCharactersOnMap.Add(MapManager.GetPartyGameObject(partyIndex).GetComponent<CharacterOnMap>());
            }

            _eventMove = eventMove;

            if (_eventMove != EventMoveEnum.MOVEMENT_JUMP)
            {
                MoveProcess();
            } else
            {
                _jumpCharacters = new()
                {
                    new JumpCharacter(this, _characterOnMap)
                };

                if (_targetCharacer.IsPlayer)
                {
                    foreach (var partyIndex in Enumerable.Range(0, MapManager.GetPartyMemberNum()))
                    {
                        _jumpCharacters.Add(
                            new JumpCharacter(
                                this,
                                _partyCharactersOnMap[partyIndex + 1]));
                    }
                }

                _destTilePos = new Vector2(positionX, -positionY);

                StartCoroutine(JumpProcessCoroutine());
            }
        }

        public void UpdateMove() {
            //_moveSkip = false;
            //MoveProcess();
        }

       
        private Vector2[] _directionEnumVector2 = new Vector2[] {
            Vector2.up,//Up
            Vector2.down,//Down
            Vector2.left,//Left
            Vector2.right,//Right
        };

        private Vector2 GetDirectionEnumVector2(CharacterMoveDirectionEnum directionEnum) {
            return _directionEnumVector2[(int) directionEnum];
        }

        private void MoveProcess() {
            if (_targetObj == null)
                return;

            var directionEnum = CharacterMoveDirectionEnum.Down;

            var targetPositionOnTile = new Vector2(0, 0);
            var currentPotision = new Vector2(0, 0);
            var events = MapEventExecutionController.Instance.GetEvents();
            var isThrough = (_parent != null) ? _parent.GetTrough() : _characterOnMap.GetCharacterThrough();
            switch (_eventMove) {
                case EventMoveEnum.MOVEMENT_MOVE_DOWN:
                case EventMoveEnum.MOVEMENT_MOVE_LEFT:
                case EventMoveEnum.MOVEMENT_MOVE_RIGHT:
                case EventMoveEnum.MOVEMENT_MOVE_UP:
                case EventMoveEnum.MOVEMENT_MOVE_LOWER_LEFT:
                case EventMoveEnum.MOVEMENT_MOVE_LOWER_RIGHT:
                case EventMoveEnum.MOVEMENT_MOVE_UPPER_LEFT:
                case EventMoveEnum.MOVEMENT_MOVE_UPPER_RIGHT:
                    //目標タイルに達するための、タイル的に移動する順序（複数の場合あり）を求める。
                    var tupleList = _eventMove switch
                    {
                        EventMoveEnum.MOVEMENT_MOVE_LEFT => new List<_Tuple>() { new _Tuple(CharacterMoveDirectionEnum.Left, Vector2.left), null },
                        EventMoveEnum.MOVEMENT_MOVE_RIGHT => new List<_Tuple>() { new _Tuple(CharacterMoveDirectionEnum.Right, Vector2.right), null },
                        EventMoveEnum.MOVEMENT_MOVE_UP => new List<_Tuple>() { new _Tuple(CharacterMoveDirectionEnum.Up, Vector2.up), null },
                        EventMoveEnum.MOVEMENT_MOVE_DOWN => new List<_Tuple>() { new _Tuple(CharacterMoveDirectionEnum.Down, Vector2.down), null },
                        EventMoveEnum.MOVEMENT_MOVE_LOWER_LEFT => new List<_Tuple>() { new _Tuple(CharacterMoveDirectionEnum.Down, Vector2.down), new _Tuple(CharacterMoveDirectionEnum.Left, Vector2.left), null, new _Tuple(CharacterMoveDirectionEnum.Left, Vector2.left), new _Tuple(CharacterMoveDirectionEnum.Down, Vector2.down), null },
                        EventMoveEnum.MOVEMENT_MOVE_LOWER_RIGHT => new List<_Tuple>() { new _Tuple(CharacterMoveDirectionEnum.Down, Vector2.down), new _Tuple(CharacterMoveDirectionEnum.Right, Vector2.right), null, new _Tuple(CharacterMoveDirectionEnum.Right, Vector2.right), new _Tuple(CharacterMoveDirectionEnum.Down, Vector2.down), null },
                        EventMoveEnum.MOVEMENT_MOVE_UPPER_LEFT => new List<_Tuple>() { new _Tuple(CharacterMoveDirectionEnum.Up, Vector2.up), new _Tuple(CharacterMoveDirectionEnum.Left, Vector2.left), null, new _Tuple(CharacterMoveDirectionEnum.Left, Vector2.left), new _Tuple(CharacterMoveDirectionEnum.Up, Vector2.up), null },
                        EventMoveEnum.MOVEMENT_MOVE_UPPER_RIGHT => new List<_Tuple>() { new _Tuple(CharacterMoveDirectionEnum.Up, Vector2.up), new _Tuple(CharacterMoveDirectionEnum.Right, Vector2.right), null, new _Tuple(CharacterMoveDirectionEnum.Right, Vector2.right), new _Tuple(CharacterMoveDirectionEnum.Up, Vector2.up), null },
                        _ => new List<_Tuple>() { null }
                    };
                    int startIndex = 0;
                    //すり抜けON
                    if (!isThrough)
                    {
                        var currentPositionOnTile = _characterOnMap.GetCurrentPositionOnTile();

                        var throughable = true;
                        var posOnTile = currentPositionOnTile;
                        for (int i = 0; i < tupleList.Count; i++) {
                            var tuple = tupleList[i];
                            if (!throughable && tuple != null) continue;
                            if (tuple == null)
                            {
                                if (throughable) break; //目標タイルに移動可能。
                                if (i + 1 < tupleList.Count)
                                {
                                    //次の順序をトライ。
                                    startIndex = i + 1;
                                    throughable = true;
                                    posOnTile = currentPositionOnTile;
                                }
                                continue;
                            }
                            posOnTile += tuple.Item2;

                            if (UnableToEnter(posOnTile, tuple.Item1, events))
                            {
                                //通れない
                                throughable = false;
                                continue;
                            }
                        }
                        if (!throughable)
                        {
                            // スキップが有効の場合は処理終了
                            if (_moveSkip)
                            {
                                MoveEnd();
                                return;
                            }

                            // 飛ばさない場合はリトライ
                            _closeAction.Invoke();
                            TimeHandler.Instance.AddTimeActionEveryFrame(RetryMove);
                            return;
                        }
                    }

                    switch (_eventMove)
                    {
                        case EventMoveEnum.MOVEMENT_MOVE_DOWN:
                            _characterOnMap.MoveDownOneUnit(MoveEnd);
                            break;
                        case EventMoveEnum.MOVEMENT_MOVE_LEFT:
                            _characterOnMap.MoveLeftOneUnit(MoveEnd);
                            break;
                        case EventMoveEnum.MOVEMENT_MOVE_RIGHT:
                            _characterOnMap.MoveRightOneUnit(MoveEnd);
                            break;
                        case EventMoveEnum.MOVEMENT_MOVE_UP:
                            _characterOnMap.MoveUpOneUnit(MoveEnd);
                            break;
                        case EventMoveEnum.MOVEMENT_MOVE_LOWER_LEFT:
                        case EventMoveEnum.MOVEMENT_MOVE_LOWER_RIGHT:
                        case EventMoveEnum.MOVEMENT_MOVE_UPPER_LEFT:
                        case EventMoveEnum.MOVEMENT_MOVE_UPPER_RIGHT:
                            directionEnum = _characterOnMap.GetCurrentDirection();
                            var matched = false;
                            bool changeDirection = false;
                            Vector2 offset = Vector2.zero;
                            var dirList = new List<CharacterMoveDirectionEnum>();
                            for (int i = startIndex; i < tupleList.Count; i++) {
                                var tuple = tupleList[i];
                                if (tuple == null) break;
                                if (tuple.Item1 == directionEnum)
                                {
                                    matched = true;
                                }
                                offset += tuple.Item2;
                                dirList.Add(tuple.Item1);
                            }
                            if (!matched)
                            {
                                //向きを逆に。
                                directionEnum = directionEnum switch {
                                    CharacterMoveDirectionEnum.Down => CharacterMoveDirectionEnum.Up,
                                    CharacterMoveDirectionEnum.Left => CharacterMoveDirectionEnum.Right,
                                    CharacterMoveDirectionEnum.Right => CharacterMoveDirectionEnum.Left,
                                    CharacterMoveDirectionEnum.Up => CharacterMoveDirectionEnum.Down,
                                    _ => directionEnum
                                };
                                changeDirection = true;
                            }
                            var dirEnum2 = (dirList[0] == directionEnum) ? dirList[1] : dirList[0];
                            _characterOnMap.MoveDinagonalOneUnit(directionEnum, dirEnum2, offset, MoveEnd, changeDirection);
                            break;

                    }
                    break;
                case EventMoveEnum.MOVEMENT_MOVE_AT_RANDOM:
                    //ランダム
                    var rand = Random.Range(0, 4);
                    directionEnum = (CharacterMoveDirectionEnum) rand;
                    targetPositionOnTile = _characterOnMap.GetCurrentPositionOnTile() + GetDirectionEnumVector2(directionEnum);

                    if (!isThrough && UnableToEnter(targetPositionOnTile, directionEnum, events))
                    {
                        // スキップが有効の場合は処理終了
                        if (_moveSkip)
                        {
                            MoveEnd();
                            return;
                        }

                        // 飛ばさない場合はリトライ
                        _closeAction.Invoke();
                        TimeHandler.Instance.AddTimeActionEveryFrame(RetryMove);
                        return;
                    }

                    switch (directionEnum)
                    {
                        case CharacterMoveDirectionEnum.Down:
                            _characterOnMap.MoveDownOneUnit(MoveEnd);
                            break;
                        case CharacterMoveDirectionEnum.Up:
                            _characterOnMap.MoveUpOneUnit(MoveEnd);
                            break;
                        case CharacterMoveDirectionEnum.Left:
                            _characterOnMap.MoveLeftOneUnit(MoveEnd);
                            break;
                        case CharacterMoveDirectionEnum.Right:
                            _characterOnMap.MoveRightOneUnit(MoveEnd);
                            break;
                        default:
                            // 飛ばさない場合はリトライ
                            TimeHandler.Instance.AddTimeActionEveryFrame(RetryMove);
                            break;
                    }
                    break;
                case EventMoveEnum.MOVEMENT_MOVE_TOWARD_PLAYER:
                    //プレイヤーに近づく
                    if (_targetCharacer.IsPlayer)
                    {
                        MoveEnd();
                        return;
                    }

                    // 方向取得
                    List<CharacterMoveDirectionEnum> directionEnumList = new List<CharacterMoveDirectionEnum>();

                    if (Mathf.Abs(transform.localPosition.x - _actorObj.transform.localPosition.x) > Mathf.Abs(transform.localPosition.y - _actorObj.transform.localPosition.y))
                    {
                        if (transform.localPosition.x < _actorObj.transform.localPosition.x)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Right);
                        else if (transform.localPosition.x > _actorObj.transform.localPosition.x)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Left);
                        if (transform.localPosition.y < _actorObj.transform.localPosition.y)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Up);
                        else if (transform.localPosition.y > _actorObj.transform.localPosition.y)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Down);

                        if (directionEnumList.Count == 0)
                            TimeHandler.Instance.AddTimeActionEveryFrame(RetryMove);
                    }
                    else
                    {
                        if (transform.localPosition.y < _actorObj.transform.localPosition.y)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Up);
                        else if (transform.localPosition.y > _actorObj.transform.localPosition.y)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Down);
                        if (transform.localPosition.x < _actorObj.transform.localPosition.x)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Right);
                        else if (transform.localPosition.x > _actorObj.transform.localPosition.x)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Left);

                        if (directionEnumList.Count == 0)
                            TimeHandler.Instance.AddTimeActionEveryFrame(RetryMove);
                    }

                    bool flg = false;
                    for (int i = 0; i < directionEnumList.Count; i++)
                    {
                        targetPositionOnTile = _characterOnMap.GetCurrentPositionOnTile() + GetDirectionEnumVector2(directionEnumList[i]);

                        if (!isThrough && UnableToEnter(targetPositionOnTile, directionEnumList[i], events))
                        {
                            continue;
                        }

                        flg = true;
                        directionEnum = directionEnumList[i];
                        break;
                    }

                    if (!flg)
                    {
                        // スキップが有効の場合は処理終了
                        if (_moveSkip)
                        {
                            MoveEnd();
                            return;
                        }

                        // 飛ばさない場合はリトライ
                        _closeAction.Invoke();
                        TimeHandler.Instance.AddTimeActionEveryFrame(RetryMove);
                        return;
                    }

                    if (directionEnum == CharacterMoveDirectionEnum.Left)
                        _characterOnMap.MoveLeftOneUnit(MoveEnd);
                    else if (directionEnum == CharacterMoveDirectionEnum.Right)
                        _characterOnMap.MoveRightOneUnit(MoveEnd);
                    else if (directionEnum == CharacterMoveDirectionEnum.Up)
                        _characterOnMap.MoveUpOneUnit(MoveEnd);
                    else if (directionEnum == CharacterMoveDirectionEnum.Down)
                        _characterOnMap.MoveDownOneUnit(MoveEnd);

                    break;
                case EventMoveEnum.MOVEMENT_MOVE_AWAY_FROM_PLAYER: //プレイヤーから遠ざかる
                    if (_targetCharacer.IsPlayer)
                    {
                        MoveEnd();
                        return;
                    }

                    // 方向取得
                    directionEnumList = new List<CharacterMoveDirectionEnum>();

                    if (Mathf.Abs(transform.localPosition.x - _actorObj.transform.localPosition.x) > Mathf.Abs(transform.localPosition.y - _actorObj.transform.localPosition.y))
                    {
                        if (transform.localPosition.y < _actorObj.transform.localPosition.y)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Down);
                        else if (transform.localPosition.y > _actorObj.transform.localPosition.y)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Up);
                        if (transform.localPosition.x < _actorObj.transform.localPosition.x)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Left);
                        else if (transform.localPosition.x > _actorObj.transform.localPosition.x)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Right);

                        if (directionEnumList.Count == 0)
                            TimeHandler.Instance.AddTimeActionEveryFrame(RetryMove);
                    }
                    else
                    {
                        if (transform.localPosition.x < _actorObj.transform.localPosition.x)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Left);
                        else if (transform.localPosition.x > _actorObj.transform.localPosition.x)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Right);
                        if (transform.localPosition.y < _actorObj.transform.localPosition.y)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Down);
                        else if (transform.localPosition.y > _actorObj.transform.localPosition.y)
                            directionEnumList.Add(CharacterMoveDirectionEnum.Up);

                        if (directionEnumList.Count == 0)
                            TimeHandler.Instance.AddTimeActionEveryFrame(RetryMove);
                    }

                    flg = false;
                    for (int i = 0; i < directionEnumList.Count; i++)
                    {
                        targetPositionOnTile = _characterOnMap.GetCurrentPositionOnTile() + GetDirectionEnumVector2(directionEnumList[i]);

                        if (!isThrough && UnableToEnter(targetPositionOnTile, directionEnumList[i], events))
                        {
                            continue;
                        }

                        flg = true;
                        directionEnum = directionEnumList[i];
                        break;
                    }

                    if (!flg)
                    {
                        // スキップが有効の場合は処理終了
                        if (_moveSkip)
                        {
                            MoveEnd();
                            return;
                        }

                        // 飛ばさない場合はリトライ
                        _closeAction.Invoke();
                        TimeHandler.Instance.AddTimeActionEveryFrame(RetryMove);
                        return;
                    }


                    if (directionEnum == CharacterMoveDirectionEnum.Left)
                        _characterOnMap.MoveLeftOneUnit(MoveEnd);
                    else if (directionEnum == CharacterMoveDirectionEnum.Right)
                        _characterOnMap.MoveRightOneUnit(MoveEnd);
                    else if (directionEnum == CharacterMoveDirectionEnum.Up)
                        _characterOnMap.MoveUpOneUnit(MoveEnd);
                    else if (directionEnum == CharacterMoveDirectionEnum.Down)
                        _characterOnMap.MoveDownOneUnit(MoveEnd);

                    break;
            }

            //ここに到達している場合は、移動しているケース
            //対象がアクターであり、かつイベント実行中の場合には、パーティメンバーも移動する
            if (_targetCharacer.IsPlayer)
            {
                MapManager.PartyMoveCharacter(false);
            }
        }

        private bool UnableToEnter(Vector2 posOnTile, CharacterMoveDirectionEnum directionEnum, List<EventOnMap> events) {
            if (_tilesOnThePosition == null)
            {
                _tilesOnThePosition = gameObject.AddComponent<TilesOnThePosition>();
            }
            _tilesOnThePosition.InitForRuntime(_mapDataModel, posOnTile);
            if (!_tilesOnThePosition.CanEnterThisTiles(directionEnum))
            {
                return true;
            }

            if (_targetCharacer.TargetType != Commons.TargetType.Player)
            {
                //進行可能判定（プレイヤーと接触しているかどうか）
                if (!CanMove(posOnTile)) return true;

                foreach (var characterOnMap in _partyCharactersOnMap)
                {
                    var pos = characterOnMap.GetCurrentPositionOnTile();
                    if (pos == posOnTile) return true;
                }
            }

            // 進行可能判定（イベント）
            for (var j = 0; j < events.Count; j++)
            {
                var event_ = events [j];
                if (event_.isValid == false) continue;
                if (event_.GetTrough() == false &&
                    (new Vector2(event_.x_now, event_.y_now) == posOnTile ||
                     new Vector2(event_.x_next, event_.y_next) == posOnTile) &&
                    event_.GetPriority() == _priority)
                {
                    return true;
                }
            }
            return false;

        }

        private void RetryMove() {
            TimeHandler.Instance.RemoveTimeAction(RetryMove);
            MoveProcess();
        }


        private void MoveEnd() {
            _endAction?.Invoke();
        }

        // 移動頻度を制御。
        private bool CanMove(Vector2 posOnTile) {
            if (_priority != EventMapDataModel.EventMapPage.PriorityType.Normal) return true;

            var characterOnMap = MapManager.GetOperatingCharacterGameObject().GetComponent<CharacterOnMap>();
            if (characterOnMap.IsMoveCheck((int)posOnTile.x, (int) posOnTile.y))
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// 一連のジャンプ処理コルーチン。
        /// </summary>
        /// <returns>コルーチンの戻り値。</returns>
        private IEnumerator JumpProcessCoroutine() {
            Vector2 destPos = MapManager.GetWorldPositionByTilePositionForRuntime(_destTilePos);
            Vector2 tilePos = MapManager.GetTilePositionByWorldPositionForRuntime(firstJumpCharacterOnMap.transform.position);

            foreach (var jumpCharacter in _jumpCharacters)
            {
                jumpCharacter.Init();
            }

            firstJumpCharacterOnMap.SetMoving(true);

            float moveRate = 0f;

            var moveSpeed = _characterOnMap.GetCharacterSpeed() / 6f * MoveSpeedRate;

            do
            {
                moveRate = Mathf.Min(moveRate + moveSpeed, 1f);

                foreach (var jumpCharacter in _jumpCharacters)
                {
                    jumpCharacter.Update(moveRate);
                }

                if (_targetCharacer.IsPlayer)
                {
                    // 移動に応じてループ
                    var newTilePos = MapManager.GetTilePositionByWorldPositionForRuntime(
                        firstJumpCharacterOnMap.transform.position) + new Vector2(1, -1);
                    var deltaX = newTilePos.x - tilePos.x;
                    var deltaY = newTilePos.y - tilePos.y;
                    for (int i = 0; i < deltaX; i++)
                        MapManager.MapLoop(CharacterMoveDirectionEnum.Right);
                    for (int i = 0; i > deltaX; i--)
                        MapManager.MapLoop(CharacterMoveDirectionEnum.Left);
                    for (int i = 0; i < deltaY; i++)
                        MapManager.MapLoop(CharacterMoveDirectionEnum.Up);
                    for (int i = 0; i > deltaY; i--)
                        MapManager.MapLoop(CharacterMoveDirectionEnum.Down);
                    tilePos.x = newTilePos.x;
                    tilePos.y = newTilePos.y;
                }

                yield return null;
            }
            while (moveRate < 1f);

            firstJumpCharacterOnMap.SetMoving(false);

            foreach (var jumpCharacter in _jumpCharacters)
            {
                jumpCharacter.Term();
            }

            firstJumpCharacterOnMap.SetCurrentPositionOnTile(new Vector2(destPos.x, destPos.y));

            // パーティ更新
            if (_targetCharacer.IsPlayer)
            {
                MapManager.SetTargetPosition(new Vector2(destPos.x, destPos.y));
            }

            _endAction?.Invoke();
        }

        private class JumpCharacter
        {
            private readonly StepMoveController controller;
            private readonly CharacterOnMap characterOnMap;
            private Vector2 startPos;
            private float maxJumpHeight;

            public CharacterOnMap CharacterOnMap => this.characterOnMap;

            public JumpCharacter(StepMoveController controller, CharacterOnMap characterOnMap) {
                this.controller = controller;
                this.characterOnMap = characterOnMap;
            }

            public void Init() {
                // 開始位置。
                this.startPos = this.characterOnMap.transform.position;

                // 最大ジャンプ高。
                var distance = Vector2.Distance(this.startPos, MapManager.GetWorldPositionByTilePositionForRuntime(this.controller._destTilePos));
                this.maxJumpHeight = MinJumpHeight + distance * JumpHeightParDistance;

                // 移動先に向く。
                this.characterOnMap.TryChangeCharacterDirection(
                    Commons.GetDirection(
                        MapManager.GetTilePositionByWorldPositionForRuntime(this.startPos), this.controller._destTilePos),
                    !this.characterOnMap.GetIsLockDirection());
            }

            public void Update(float moveRate) {
                try
                {
                    var newPos = Vector3.Lerp(
                        this.startPos, MapManager.GetWorldPositionByTilePositionForRuntime(this.controller._destTilePos), moveRate);
                    var jumpHeight = Mathf.Sin(Mathf.PI * moveRate) * this.maxJumpHeight;
                    this.characterOnMap.SetGameObjectPositionWithRenderingOrder(newPos, jumpHeight);
                }
                catch (Exception)
                {
                    //マップ移動時に、このオブジェクト自体が破棄された場合、タイミングによってはここに入ってくる可能性がある
                    //nullチェック等での判定が出来ないため、ここで破棄済みの場合の処理を実施する
                }
            }

            public void Term() {
                this.characterOnMap.SetToPositionOnTile(
                    MapManager.GetWorldPositionByTilePositionForRuntime(this.controller._destTilePos));
            }
        }
    }
}
