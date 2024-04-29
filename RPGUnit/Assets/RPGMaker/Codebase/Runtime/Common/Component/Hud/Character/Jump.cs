using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Runtime.Map;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine; //バトルでは本コマンドは利用しない

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud.Character
{
    /// <summary>
    /// イベントコマンド『ジャンプ』用コンポーネント。
    /// </summary>
    public class Jump : MonoBehaviour
    {
        private const bool CharacterDirectionLockIgnore = true;

        private const float MoveSpeedRate = 0.05f;
        private const float MinJumpHeight = 0.5f;
        private const float JumpHeightParDistance = 0.25f;

        private GameObject _actorObj;

        private int _animation; //歩行、足踏み、なし


        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        private Commons.Direction.Id     _directionId; //向き
        private Commons.TargetCharacter _targetCharacer;    // 対象キャラクター。
        private bool                    _isMove;    // 座標指定(移動する)か？ 
        private float                   _moveSpeed; //移動速度 (実態は移動にかかるフレーム数の逆数)

        private Action                  _nextAction;
        private Action<string>          _closeAction;

        // 表示要素プロパティ
        //--------------------------------------------------------------------------------------------------------------
        private bool       _repeatOperation; //動作を繰り返す
        private Vector2Int _offsettTilePos;
        private Vector2    _destTilePos;
        private bool       _waitToggle; //完了までウェイト
        private string     _eventId;
        private Coroutine  _coroutine;

        private List<JumpCharacter> _jumpCharacters;
        private CharacterOnMap jumpFirstCharacterOnMap => _jumpCharacters[0].CharacterOnMap;

        // enum / interface / local class
        //--------------------------------------------------------------------------------------------------------------

        public static float GetMaxJumpHeight(float distance)
        {
            return MinJumpHeight + distance * JumpHeightParDistance;
        }

        /**
         * 初期化
         */
        public void Init() {
            _actorObj = MapManager.GetOperatingCharacterGameObject();
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }

        public void MoveJumpProcess(
            string thisEventId,
            EventDataModel.EventCommand command,
            Action<string> closeAction,
            Action nextAction,
            string eventId
        ) {
            _targetCharacer = new Commons.TargetCharacter(command.parameters[0], thisEventId);
            _isMove = command.parameters[1] == "0";
            _moveSpeed = GetMoveSpeed((Commons.SpeedMultiple.Id)int.Parse(command.parameters[2]));
            _directionId = (Commons.Direction.Id)int.Parse(command.parameters[3]);
            _animation = int.Parse(command.parameters[4]);
            _repeatOperation = command.parameters[6] != "0";
            _waitToggle = command.parameters[8] != "0";

            _closeAction = closeAction;
            _nextAction = nextAction;
            _eventId = eventId;

            _jumpCharacters = new()
            {
                new JumpCharacter(this, _targetCharacer.GetGameObject().GetComponent<CharacterOnMap>())
            };

            if (_targetCharacer.IsPlayer)
            {
                if (MapManager.CurrentVehicleId != "")
                {
                    //乗り物に搭乗中は、先頭のアクターを追加
                    _jumpCharacters.Add(
                        new JumpCharacter(
                            this,
                            MapManager.OperatingActor.GetComponent<CharacterOnMap>()));
                }
                foreach (var partyIndex in Enumerable.Range(0, MapManager.GetPartyMemberNum()))
                {
                    _jumpCharacters.Add(
                        new JumpCharacter(
                            this,
                            MapManager.GetPartyGameObject(partyIndex).GetComponent<CharacterOnMap>())); 
                }
            }

            _offsettTilePos = new Vector2Int(int.Parse(command.parameters[10]), int.Parse(command.parameters[11]));

            AnimationSettings();

            //茂みの設定情報を初期化
            _targetCharacer.GetGameObject().GetComponent<CharacterOnMap>().ResetBush(true, false);

            //CB実行
            if (!_waitToggle)
            {
                _nextAction?.Invoke();
            }

            //CB実行後、本GameObjectが未だ破棄されていなかった場合、コルーチンの処理を実施
            if (this.gameObject != null)
            {
                _coroutine = StartCoroutine(JumpProcessCoroutine());
            }
        }

        public static float GetMoveSpeed(Commons.SpeedMultiple.Id speedMultipleId)
        {
            return Commons.SpeedMultiple.GetValue(speedMultipleId) * MoveSpeedRate;
        }

        private void AnimationSettings() {
            switch (_animation)
            {
                case 0:
                    jumpFirstCharacterOnMap.SetAnimation(true, false);
                    break;
                case 1:
                    jumpFirstCharacterOnMap.SetAnimation(false, true);
                    break;
                case 2:
                    break;
            }
        }

        /// <summary>
        /// 一連のジャンプ処理コルーチン。
        /// </summary>
        /// <returns>コルーチンの戻り値。</returns>
        private IEnumerator JumpProcessCoroutine()
        {
            do
            {
                _destTilePos = MapManager.GetTilePositionByWorldPositionForRuntime(jumpFirstCharacterOnMap.transform.position);
                if (_isMove)
                {
                    _destTilePos += _offsettTilePos;
                }

                Vector2 destPos = MapManager.GetWorldPositionByTilePositionForRuntime(_destTilePos);
                Vector2 tilePos = MapManager.GetTilePositionByWorldPositionForRuntime(jumpFirstCharacterOnMap.transform.position);

                foreach (var jumpCharacter in _jumpCharacters)
                {
                    jumpCharacter.Init();
                }

                jumpFirstCharacterOnMap.SetMoving(true);

                float moveRate = 0f;

                do
                {
                    //GameObjectが破棄済みであれば処理しない
                    if (this.gameObject == null)
                    {
                        yield break;
                    }

                    //ステートがMAP、イベントの場合には時間を進める
                    if (GameStateHandler.CurrentGameState() == GameStateHandler.GameState.MAP ||
                        GameStateHandler.CurrentGameState() == GameStateHandler.GameState.EVENT)
                    {
                        moveRate = Mathf.Min(moveRate + _moveSpeed, 1f);
                    }

                    foreach (var jumpCharacter in _jumpCharacters)
                    {
                        jumpCharacter.Update(moveRate);
                    }

                    if (_targetCharacer.IsPlayer)
                    {
                        // 移動に応じてループ
                        var newTilePos = MapManager.GetTilePositionByWorldPositionForRuntime(
                            jumpFirstCharacterOnMap.transform.position); // + new Vector2(1, -1);
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

                jumpFirstCharacterOnMap.SetMoving(false);

                foreach (var jumpCharacter in _jumpCharacters)
                {
                    jumpCharacter.Term();
                }

                jumpFirstCharacterOnMap.SetCurrentPositionOnTile(new Vector2(destPos.x, destPos.y));

                // パーティ更新
                if (_targetCharacer.IsPlayer)
                {
                    MapManager.SetTargetPosition(new Vector2(destPos.x, destPos.y));
                }
            }
            while (_repeatOperation);

            //茂み情報を、着地地点に合わせて再設定
            _targetCharacer.GetGameObject().GetComponent<CharacterOnMap>().ResetBush(false, false);

            _closeAction?.Invoke(_eventId);

            if (_waitToggle)
                _nextAction?.Invoke();
        }

        private class JumpCharacter
        {
            private readonly Jump jump;
            private readonly CharacterOnMap characterOnMap;
            private Vector2 startPos;
            private float maxJumpHeight;

            public CharacterOnMap CharacterOnMap => this.characterOnMap;

            public JumpCharacter(Jump jump, CharacterOnMap characterOnMap)
            {
                this.jump = jump;
                this.characterOnMap = characterOnMap;
            }

            public void Init()
            {
                // 開始位置。
                this.startPos = this.characterOnMap.transform.position;

                // 最大ジャンプ高。
                var distance = Vector2.Distance(
                    this.startPos, MapManager.GetWorldPositionByTilePositionForRuntime(this.jump._destTilePos));
                this.maxJumpHeight = GetMaxJumpHeight(distance);

                // 移動先に向く。
                this.characterOnMap.TryChangeCharacterDirection(
                    Commons.GetDirection(
                        MapManager.GetTilePositionByWorldPositionForRuntime(this.startPos), this.jump._destTilePos),
                    CharacterDirectionLockIgnore);
            }

            public void Update(float moveRate)
            {
                try
                {
                    var newPos = Vector3.Lerp(
                        this.startPos, MapManager.GetWorldPositionByTilePositionForRuntime(this.jump._destTilePos), moveRate);
                    var jumpHeight = Mathf.Sin(Mathf.PI * moveRate) * this.maxJumpHeight;
                    this.characterOnMap.SetGameObjectPositionWithRenderingOrder(newPos, jumpHeight);

                    this.characterOnMap.TryChangeCharacterDirection(
                        Commons.Direction.GetCharacterMoveDirection(
                            this.jump._directionId, characterOnMap.gameObject, this.jump._actorObj),
                        CharacterDirectionLockIgnore);
                }
                catch (Exception)
                {
                }
            }

            public void Term()
            {
                this.characterOnMap.SetToPositionOnTile(
                    MapManager.GetWorldPositionByTilePositionForRuntime(this.jump._destTilePos));
            }
        }
    }
}