using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Character;
using RPGMaker.Codebase.Runtime.Map.Component.Map;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Map.Component.Character
{
    public abstract class CharacterOnMap : MonoBehaviour
    {
        private const float DashMoveSpeedMultiplier = 2.0f;

        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------

        // 状態プロパティ
        //--------------------------------------------------------------------------------------------------------------
        protected Vector2 _currentPositionOnTile;
        protected bool _isAnimation;
        protected bool _isLockDirection;
        protected bool _isMoving;
        protected bool _isSteppingAnimation;
        protected bool _isStop = true;
        protected bool _isThrough;
        protected Vector2 _destinationPositionOnTile;
        protected Action _callback;
        protected TileDataModel _tileDataModel = null;
        protected float _opacity = 1f;

        /// <summary>
        /// 最終の移動方向
        /// </summary>
        protected CharacterMoveDirectionEnum _lastMoveDirectionEnum = CharacterMoveDirectionEnum.Down;

        // 関数プロパティ
        //--------------------------------------------------------------------------------------------------------------
        protected IEnumerator _moveEnumerator;

        //マウスによる移動時のルート設定
        private   List<CharacterMoveDirectionEnum> _moveRoute = new List<CharacterMoveDirectionEnum>();

        /// <summary>
        /// MVの「標準」速度は16フレームで1マス進む。1秒間では3.75マス進む
        /// </summary>
        protected float _moveSpeed = 3.75f;
        protected float _moveSpeedVehicle;
        protected bool _isRide = false;

        private bool _isDash = false;
        private bool _canDash = false;
        public int x_next;

        //移動判定用のプロパティ
        public int x_now;
        public int y_next;
        public int y_now;

        //透明状態
        private bool _isTransparent = false;

        //シェーダー
        private List<Shader> _shaders = new List<Shader>();

        public string AssetId { get; private set; }

        public string CharacterId { get; private set; }

        // 表示要素プロパティ
        //--------------------------------------------------------------------------------------------------------------
        public CharacterGraphic Graphic { get; private set; }

        private Transform actorTransformInternal;
        protected Transform actorTransform
        {
            get
            {
                actorTransformInternal ??= transform.Find("actor");
                return actorTransformInternal;
            }
        }

        // enum / interface / local class
        //--------------------------------------------------------------------------------------------------------------

        // initialize methods
        //--------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="positionOnTile"></param>
        /// <param name="assetId"></param>
        /// <param name="id"></param>
        public void Init(Vector2 positionOnTile, string assetId, string id) {
            _isAnimation = false;
            _isSteppingAnimation = false;
            AssetId = assetId;
            Graphic = new GameObject().AddComponent<CharacterGraphic>();
            Graphic.gameObject.transform.SetParent(transform);
            if (assetId != null)
                Graphic.Init(assetId);

            //現在位置
            _currentPositionOnTile = positionOnTile;
            //移動先の位置も、現在位置と同じもので初期化
            _destinationPositionOnTile = _currentPositionOnTile;
            SetGameObjectPositionWithRenderingOrder(MapManager.GetWorldPositionByTilePositionForRuntime(_currentPositionOnTile));

            CharacterId = id;

            //現在の座標をint型で保持しておく
            x_now = (int) positionOnTile.x;
            y_now = (int) positionOnTile.y;
            x_next = x_now;
            y_next = y_now;
            _lastMoveDirectionEnum = GetLastMoveDirection();

            SetSortingLayer(isFlying: false);

            var spriteRenderer = gameObject.transform.GetChild(0).GetComponent<Image>();
            _shaders.Add(Shader.Find("UI/Default"));
            _shaders.Add(Shader.Find("Legacy Shaders/Particles/Additive"));
            _shaders.Add(Shader.Find("Legacy Shaders/Particles/Multiply"));
            _shaders.Add(Shader.Find("UI/Screen"));

            //現在の座標のタイル情報
            TilesOnThePosition nowTilesOnThePosition = MapManager.CurrentTileData(_currentPositionOnTile);

            //現在いる場所が梯子
            if (nowTilesOnThePosition.GetLadderTile() == true)
            {
                if (!_isLockDirection)
                {
                    ChangeCharacterDirection(CharacterMoveDirectionEnum.Up);
                }
            }

            //現在いる場所が茂み
            if (nowTilesOnThePosition.GetBushTile() == true)
            {
                //半透明処理
                Graphic.SetBush(true);
            }

            //ダメージ床
            _tileDataModel = nowTilesOnThePosition.GetDamageTile(GetCurrentDirection());
        }

        public void ResetActor(string assetId, string id) {
            AssetId = assetId;
            Graphic.ChangeAsset(assetId);
            CharacterId = id;

            //元々画像設定が無く、ここで初めて描画するケースでは、Sortしなおす必要がある
            if (Graphic.IsEnabledImage()) {
                SetSortingLayer(isFlying: false);
                SetGameObjectPositionWithRenderingOrder(MapManager.GetWorldPositionByTilePositionForRuntime(_currentPositionOnTile));
            }
        }

        public void ChangeAsset(string assetId) {
            AssetId = assetId;
            Graphic.ChangeAsset(assetId);

            //元々画像設定が無く、ここで初めて描画するケースでは、Sortしなおす必要がある
            if (Graphic.IsEnabledImage())
            {
                SetSortingLayer(isFlying: false);
                SetGameObjectPositionWithRenderingOrder(MapManager.GetWorldPositionByTilePositionForRuntime(_currentPositionOnTile));
            }
        }

        public bool GetTransparent() {
            return _isTransparent;
        }

        public void SetTransparent(bool flg) {
            _isTransparent = flg;
            if (CharacterId == null)
                Graphic.SetTransparent(true);
            else
            {
                Graphic.SetTransparent(flg);
                if (!flg)
                {
                    SetOpacity(_opacity);
                }
            }
        }
        
        
        // 状態取得系I/F
        //--------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 現在のタイル位置を取得
        /// </summary>
        /// <returns></returns>
        public Vector2 GetCurrentPositionOnTile() {
            return _currentPositionOnTile;
        }

        /// <summary>
        /// 移動先のタイル位置を取得
        /// </summary>
        /// <returns></returns>
        public Vector2 GetDestinationPositionOnTile() {
            return _destinationPositionOnTile;
        }

        /// <summary>
        /// ループマップでの現在のタイルの元の位置を取得 (イベント用)
        /// </summary>
        /// <returns></returns>
        public Vector2 GetCurrentPositionOnLoopMapTile() {
            int x = (int)_currentPositionOnTile.x;
            int width = MapManager.CurrentMapDataModel.width;
            if (x < 0) x = (x % width + width) % width;
            else x = x % width;

            int y = (int)_currentPositionOnTile.y;
            int height = MapManager.CurrentMapDataModel.height;
            if (y > 0) y = (y % height == 0 ? 0 : y % height - height);
            else y = y % height;

            return new Vector2Int(x, y);
        }

        public bool IsMoveCheck(int x, int y) {
            if (CharacterId == null)
            {
                //キャラクターIDがnullの場合、存在しないものとして扱う
                return false;
            }

            //向かおうとしている先に、既にいるかどうか
            //移動中の場合は、移動先がいる場所
            if (IsMoving())
            {
                if (x_next == x && y_next == y)
                    return true;
            }
            //移動していない場合は、現在位置がいる場所
            else
            {
                if (x_now == x && y_now == y)
                    return true;
            }

            //いない
            return false;
        }

        public bool IsAroundCheck(int x, int y) {
            if ((x_now - 1 == x || x_now == x || x_now + 1 == x) && y_now == y ||
                (y_now - 1 == y || y_now == y || y_now + 1 == y) && x_now == x)
                //周囲にいる
                return true;

            //隣にはいない
            return false;
        }

        public bool IsSameCheck(int x, int y) {
            //渡されてきた座標と同一マスにいるかどうかを返却する
            if (x == x_now && y == y_now)
                //同一
                return true;
            //異なる
            return false;
        }

        /// <summary>
        ///     パーティ用
        /// </summary>
        /// <param name="pos"></param>
        public void SetCurrentPositionOnTile(Vector2 pos) {
            _currentPositionOnTile = pos;
            _destinationPositionOnTile = _currentPositionOnTile;
        }

        /// <summary>
        /// 現在の向きを取得
        /// </summary>
        /// <returns></returns>
        public CharacterMoveDirectionEnum GetCurrentDirection() {
            return Graphic.GetCurrentDirection();
        }

        /// <summary>
        /// 最終の移動方向を取得
        /// </summary>
        /// <returns></returns>
        public CharacterMoveDirectionEnum GetLastMoveDirection() {
            return _lastMoveDirectionEnum;
        }

        /// <summary>
        /// セーブデータから最終の移動方向を設定
        /// </summary>
        /// <param name="lastMoveDirection"></param>
        public void SetLastMoveDirection(CharacterMoveDirectionEnum lastMoveDirection) {
            _lastMoveDirectionEnum = lastMoveDirection;
        }

        /// <summary>
        /// 表示プライオリティを取得を取得。
        /// </summary>
        /// <returns>表示プライオリティ</returns>
        /// <remarks>
        /// イベントに設定されいる情報なので、イベントで作成されたキャラクターでなければ『通常』とする。
        /// </remarks>
        public virtual EventMapDataModel.EventMapPage.PriorityType GetPriority()
        {
            return EventMapDataModel.EventMapPage.PriorityType.Normal;
        }

        /// <summary>
        /// ソートレイヤーを設定。
        /// </summary>
        /// <param name="isFlying">飛行中フラグ</param>
        protected void SetSortingLayer(bool isFlying)
        {
            SetSortingLayer(actorTransform, GetPriority(), isFlying);

            static void SetSortingLayer(
                Transform actorTransform, EventMapDataModel.EventMapPage.PriorityType priorityType, bool isFlying)
            {
                if (actorTransform == null)
                {
                    return;
                }

                var actorCanvas = actorTransform.GetComponent<Canvas>();
                if (actorCanvas == null)
                {
                    return;
                }

                actorCanvas.sortingLayerID =
                    MapRenderingOrderManager.GetCharacterSortingLayerId(priorityType, isFlying);
            }
        }

        /// <summary>
        /// 1マス上に移動する
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="changeDirection"></param>
        public void MoveUpOneUnit([CanBeNull] Action callback = null) {
            _lastMoveDirectionEnum = CharacterMoveDirectionEnum.Up;
            MoveToPositionOnTile(CharacterMoveDirectionEnum.Up, _currentPositionOnTile + Vector2.up, callback);
        }

        /// <summary>
        /// 1マス下に移動する
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="changeDirection"></param>
        public void MoveDownOneUnit([CanBeNull] Action callback = null) {
            _lastMoveDirectionEnum = CharacterMoveDirectionEnum.Down;
            MoveToPositionOnTile(CharacterMoveDirectionEnum.Down, _currentPositionOnTile + Vector2.down, callback);
        }

        /**
         * 1マス左に移動する
         */
        public void MoveLeftOneUnit([CanBeNull] Action callback = null) {
            _lastMoveDirectionEnum = CharacterMoveDirectionEnum.Left;
            MoveToPositionOnTile(CharacterMoveDirectionEnum.Left, _currentPositionOnTile + Vector2.left, callback);
        }

        /// <summary>
        /// 1マス右に移動する
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="changeDirection"></param>
        public void MoveRightOneUnit([CanBeNull] Action callback = null) {
            _lastMoveDirectionEnum = CharacterMoveDirectionEnum.Right;
            MoveToPositionOnTile(CharacterMoveDirectionEnum.Right, _currentPositionOnTile + Vector2.right, callback);
        }

        /**
         * 1マス斜めに移動する
         */
        public void MoveDinagonalOneUnit(CharacterMoveDirectionEnum moveDirection, CharacterMoveDirectionEnum moveDir2, Vector2 offset, [CanBeNull] Action callback = null, bool changeDirection = true) {
            if (_isMoving) return;
            if (moveDir2 == CharacterMoveDirectionEnum.Up) y_next = y_now + 1;
            if (moveDir2 == CharacterMoveDirectionEnum.Down) y_next = y_now - 1;
            if (moveDir2 == CharacterMoveDirectionEnum.Left) x_next = x_now - 1;
            if (moveDir2 == CharacterMoveDirectionEnum.Right) x_next = x_now + 1;
            MoveToPositionOnTile(moveDirection, _currentPositionOnTile + offset, callback);
        }

        /// <summary>
        /// 指定したタイルの位置へ移動する
        /// </summary>
        /// <param name="directionEnum"></param>
        /// <param name="destinationPositionOnTile"></param>
        /// <param name="callback"></param>
        /// <param name="changeDirection"></param>
        public virtual void MoveToPositionOnTile(
            CharacterMoveDirectionEnum directionEnum,
            Vector2 destinationPositionOnTile,
            [CanBeNull] Action callback = null
        ) {
            if (_isMoving) return;
            _isMoving = true;
            _isStop = false;

            //移動先を保持しておく
            if (directionEnum == CharacterMoveDirectionEnum.Up) y_next = y_now + 1;
            if (directionEnum == CharacterMoveDirectionEnum.Down) y_next = y_now - 1;
            if (directionEnum == CharacterMoveDirectionEnum.Left) x_next = x_now - 1;
            if (directionEnum == CharacterMoveDirectionEnum.Right) x_next = x_now + 1;
            _lastMoveDirectionEnum = directionEnum;

            //現在の座標のタイル情報
            TilesOnThePosition nowTilesOnThePosition = MapManager.CurrentTileData(_destinationPositionOnTile);
            //移動先の座標のタイル情報
            TilesOnThePosition nextTilesOnThePosition = MapManager.CurrentTileData(destinationPositionOnTile);

            //現在いる場所が梯子、又は移動先の場所が梯子
            if (nowTilesOnThePosition.GetLadderTile() == true || nextTilesOnThePosition.GetLadderTile() == true)
            {
                if (!_isLockDirection)
                {
                    ChangeCharacterDirection(CharacterMoveDirectionEnum.Up);
                }
                else if (GetCurrentDirection() == CharacterMoveDirectionEnum.Damage)
                {
                    //向き固定かつ、現在がダメージ絵のケースでは、下向きとする
                    ChangeCharacterDirection(CharacterMoveDirectionEnum.Down, true);
                }
            }
            //それ以外の場合
            else
            {
                if (!_isLockDirection)
                {
                    ChangeCharacterDirection(directionEnum);
                }
                else if (GetCurrentDirection() == CharacterMoveDirectionEnum.Damage)
                {
                    //向き固定かつ、現在がダメージ絵のケースでは、下向きとする
                    ChangeCharacterDirection(CharacterMoveDirectionEnum.Down, true);
                }
            }

            //現在いる場所が茂みかつ、移動先も茂み
            if (nowTilesOnThePosition.GetBushTile() == true && nextTilesOnThePosition.GetBushTile() == true)
            {
                //半透明処理
                Graphic.SetBush(true);
            }
            else
            {
                //通常のグラフィック
                Graphic.SetBush(false);
            }
            
            //ダメージ床
            _tileDataModel = nowTilesOnThePosition.GetDamageTile(GetCurrentDirection());


            _destinationPositionOnTile = destinationPositionOnTile;
            _callback = callback;
        }

        /// <summary>
        /// 指定したタイルの位置を設定
        /// </summary>
        /// <param name="destinationPositionOnTile"></param>
        public virtual void SetToPositionOnTile(Vector2 destinationPositionOnTile) {
            _isMoving = false;

            var worldPosition = MapManager.GetWorldPositionByTilePositionForRuntime(destinationPositionOnTile);
            _currentPositionOnTile = destinationPositionOnTile;

            // 移動先設定
            x_now = (int) destinationPositionOnTile.x;
            y_now = (int) destinationPositionOnTile.y;
            y_next = (int) destinationPositionOnTile.y;
            x_next = (int) destinationPositionOnTile.x;

            SetGameObjectPositionWithRenderingOrder(worldPosition);
        }

        /// <summary>
        /// 移動状態の取得
        /// </summary>
        /// <returns></returns>
        public bool IsMoving() {
            return _isMoving;
        }

        /// <summary>
        /// 移動状態の設定
        /// </summary>
        /// <param name="moving"></param>
        public void SetMoving(bool moving) {
            _isMoving = moving;
            _isStop = false;
        }

        /// <summary>
        /// 目的位置へ毎フレーム移動していく
        /// </summary>
        /// <param name="destinationPositionOnWorld"></param>
        /// <param name="callBack"></param>
        protected virtual void MoveToPositionOnTileByFrame(Vector2 destinationPositionOnWorld, Action callBack) {
            Vector2 currentPos = gameObject.transform.position;
            if (currentPos != destinationPositionOnWorld)
            {
                //メニュー開き中は進まないようにしておく
                if (MenuManager.IsMenuActive) return;

                var newPos = Vector2.MoveTowards(
                    currentPos,
                    destinationPositionOnWorld,
                    Time.deltaTime * (_isRide ? _moveSpeedVehicle : _moveSpeed) * (_canDash && _isDash && !_isRide ? DashMoveSpeedMultiplier : 1.0f));

                SetGameObjectPositionWithRenderingOrder(newPos);
            }
            else
            {
                callBack();
            }
        }

        /// <summary>
        /// 向き変更を試行する。
        /// </summary>
        /// <param name="directionEnum"></param>
        /// <param name="lockIgnore"></param>
        public void TryChangeCharacterDirection(CharacterMoveDirectionEnum directionEnum, bool lockIgnore = false)
        {
            if (directionEnum != CharacterMoveDirectionEnum.None)
            {
                ChangeCharacterDirection(directionEnum, lockIgnore);
            }
        }

        /// <summary>
        /// 向きを変える
        /// </summary>
        /// <param name="directionEnum"></param>
        /// <param name="lockIgnore"></param>
        public void ChangeCharacterDirection(CharacterMoveDirectionEnum directionEnum, bool lockIgnore = false) {
            if (!_isLockDirection || lockIgnore) Graphic.ChangeDirection(directionEnum);
        }

        // 状態操作系I/F
        //--------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// アニメーションの設定
        /// </summary>
        /// <param name="animation"></param>
        /// <param name="step"></param>
        public void SetAnimation(bool animation, bool step) {
            if (!isActiveAndEnabled) return;

            _isAnimation = animation;
            _isSteppingAnimation = step;
        }

        public virtual void SetAnimationSettings(bool animation, bool step) {
            if (!isActiveAndEnabled) return;

            _isAnimation = animation;
            _isSteppingAnimation = step;
        }


        public bool GetAnimation() {
            return _isAnimation;
        }

        public bool GetStepAnimation() {
            return _isSteppingAnimation;
        }

        public void SteppingAnimation() {
            Application.targetFrameRate = 60;

            //パーティに1名も存在しない場合などで、Graphicが無い場合は終了
            if (Graphic == null)
            {
                return;
            }

            //ダメージ絵の場合には、歩行アニメや足踏みアニメとは別で処理する
            if (Graphic.GetCurrentDirection() == CharacterMoveDirectionEnum.Damage)
            {
                Graphic.DamageAnimation();
                return;
            }

            //歩行アニメ又は、足踏みアニメがONの場合は処理する
            if (_isAnimation || _isSteppingAnimation || _isRide)
            {
                if (MenuManager.IsMenuActive)
                    return;

                //乗り物に乗ってる状態なら、アニメーションする（アクターは関係ない）
                if (_isRide) Graphic.StepAnimation();
                //移動していない場合は、足踏みアニメがONの場合にアニメーションする
                else if (!_isMoving && _isSteppingAnimation)
                    Graphic.StepAnimation();
                //移動中は、歩行アニメがONの場合にアニメーションする
                else if (_isMoving && _isAnimation)
                    Graphic.StepAnimation();
                // 移動完了時は停止状態にする
                else if (_isMoving == false && _isStop == false)
                    _isStop = true;
                // 処理していない場合は静止画を設定
                else if (_isStop) Graphic.StopTexture();
            }
        }

        public void SetIsLockDirection(bool flg) {
            _isLockDirection = flg;
        }

        public bool GetIsLockDirection() {
            return _isLockDirection;
        }

        public void SetThrough(bool flg) {
            //gameObject.GetComponent<BoxCollider2D>().enabled = flg;
            _isThrough = flg;
        }

        public bool GetCharacterThrough() {
            return _isThrough;
        }

        public float GetCharacterOpacity() {
            return _opacity;
        }

        public void SetCharacterSpeed(float speed) {
            _moveSpeed = speed;
        }
        public void SetVehicleSpeed(float speed) {
            _moveSpeed = speed / 40 * 3.75f;
        }

        public float GetCharacterSpeed() {
            return _moveSpeed;
        }

        public void SetCharacterRide(bool isRide, float speed = 0.0f) {
            _isRide = isRide;
            _moveSpeedVehicle = speed;
        }

        public void SetDash(bool isDash) {
            _isDash = isDash;
        }

        public void CanDash(bool canDash) {
            _canDash = canDash;
        }

        public void SetOpacity(float opacity) {
            if (CharacterId != null)
            {
                _opacity = opacity;
                Graphic.SetOpacity(_isTransparent ? 0 : opacity);
            }
        }
        
        public void SetOpacityToNpc(float opacity) {
            _opacity = opacity;
            Graphic.SetOpacity(_isTransparent ? 0 : opacity);
        }

        public void SetChangeBlendMode(int selectIndex) {
            var spriteRenderer = gameObject.transform.GetChild(0).GetComponent<Image>();
            var synthesis =
                selectIndex switch
                {
                    0 => _shaders[0],
                    1 => _shaders[1],
                    2 => _shaders[2],
                    _ => _shaders[3]
                };
            spriteRenderer.material.shader = synthesis;
        }

        public void SetCharacterEnable(bool enable) {
            Graphic.SetImageEnable(enable);
        }

        public int MoveRouteCount() {
            //現在、移動ルートに従った移動中かどうかを返却する
            return _moveRoute.Count;
        }

        public void SetMoveRoute(List<CharacterMoveDirectionEnum> moveRoute) {
            //移動ルート設定
            _moveRoute = moveRoute;
        }

        public void ResetMoveRoute() {
            //移動ルート初期化
            _moveRoute.Clear();
        }

        public CharacterMoveDirectionEnum GetNextRoute() {
            //配列要素が無い場合は移動しない
            if (_moveRoute.Count == 0) return CharacterMoveDirectionEnum.Max;
            //移動終了したため配列の0番目を削除
            _moveRoute.RemoveAt(0);
            //配列要素が無くなった場合は処理終了
            if (_moveRoute.Count == 0) return CharacterMoveDirectionEnum.Max;
            //まだ移動先がある場合は、次の移動先を返却する
            return _moveRoute[0];
        }

        public virtual void UpdateTimeHandler() {
            // 歩行アニメーションの更新
            SteppingAnimation();

            // 現在移動中の場合は先に進める
            if (_isMoving && gameObject.activeSelf)
            {
                MoveToPositionOnTileByFrame(
                    MapManager.GetWorldPositionByTilePositionForRuntime(_destinationPositionOnTile), MoveEnd);
            }
        }

        public void MoveEnd() {
            _currentPositionOnTile = _destinationPositionOnTile;
            //移動完了
            _isMoving = false;
            //移動元を移動先にする
            x_now = x_next;
            y_now = y_next;

            //移動対象が操作キャラクターであれば、セーブデータへ反映
            if (MapManager.OperatingCharacter == this)
            {
                MapManager.SetTargetPosition(new Vector2(MapManager.OperatingCharacter.x_next, MapManager.OperatingCharacter.y_next));
            }

            //移動後の座標のタイル情報
            TilesOnThePosition nowTilesOnThePosition = MapManager.CurrentTileData(_destinationPositionOnTile);

            //現在いる場所が茂み
            if (nowTilesOnThePosition.GetBushTile() == true)
            {
                //半透明処理の付与
                Graphic.SetBush(true);
            }
            else
            {
                //半透明処理の付与
                Graphic.SetBush(false);
            }

            //ダメージ床
            _tileDataModel = nowTilesOnThePosition.GetDamageTile(GetCurrentDirection());

            //先頭のキャラクターのタイル情報に戻す
            MapManager.CurrentTileData(MapManager.OperatingCharacter.GetCurrentPositionOnTile());

            _callback?.Invoke();
        }

        public void ResetSetting() {
            //移動ルート指定のイベント実行中だった場合、初期状態に戻す
            MoveSetMovePoint work = GetComponent<MoveSetMovePoint>();
            if (work != null)
            {
                work.ResetSetting();
            }
        }

        /// <summary>
        /// 茂み再設定処理
        /// </summary>
        /// <param name="isForce">タイル情報を無視して強制的に引数の設定を適用する</param>
        /// <param name="isBush">強制的に適用する際の、茂み設定</param>
        public void ResetBush(bool isForce, bool isBush) {
            if (!isForce)
            {
                //現在の座標のタイル情報
                TilesOnThePosition nowTilesOnThePosition = MapManager.CurrentTileData(_currentPositionOnTile);

                //現在いる場所が梯子
                if (nowTilesOnThePosition.GetLadderTile() == true)
                {
                    if (!_isLockDirection)
                    {
                        ChangeCharacterDirection(CharacterMoveDirectionEnum.Up);
                    }
                }

                //現在いる場所が茂み
                if (nowTilesOnThePosition.GetBushTile() == true)
                {
                    //半透明処理
                    Graphic.SetBush(true);
                }
                else
                {
                    //半透明解除
                    Graphic.SetBush(false);
                }
            }
            else
            {
                //半透明処理
                Graphic.SetBush(isBush);
            }
        }

        // 表示位置と描画順調整値の設定。
        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 描画順調整値も含めた表示位置設定。
        /// </summary>
        /// <param name="position">xy位置。</param>
        /// <param name="height">高さ (省略時は変更なし。ジャンプで使用する)。</param>
        public void SetGameObjectPositionWithRenderingOrder(Vector2 position, float height = float.NaN) {
            // 表示位置を設定する。
            transform.SetPositionXY(position);

            // 描画順調整用Z値を設定する。
            {
                // Y座標値の下の桁の情報として加算するための割合。
                // マップシーン内の座標空間での1タイルのサイズは、横0.98 縦0.98 (横1 縦1に変わった？)。
                // charactorOrderは、0～最大パーティ人数の値をとる。
                const float CharactorRenderingOrderRate = 0.001f;

                // このキャラクターの描画順値。
                int charactorRenderingOrder = MapManager.GetCharactorRenderingOrder(this);
                transform.SetLocalPositionZ(position.y - charactorRenderingOrder * CharactorRenderingOrderRate);

                Transform work = transform.Find("actor");
                if (work != null)
                {
                    //Actor下のz-indexは0
                    work.SetLocalPositionZ(0);
                }
            }

            if (actorTransform == null)
            {
                return;
            }

            // 高さ。
            if (!float.IsNaN(height))
            {
                actorTransform.SetLocalPositionY(height);
            }
        }

        /// <summary>
        /// キャラクターの用途を文字列で取得する (動作確認用)。
        /// </summary>
        /// <param name="characterOnMap">キャラクター。</param>
        /// <returns>用途を表す文字列。</returns>
        private static string GetCharacterUsageString(CharacterOnMap characterOnMap)
        {
            string name = null;
            int index = -1;

            if (characterOnMap == MapManager.OperatingActor)
            {
                return "OperatingActor";
            }

            if (name == null)
            {
                index = MapManager.PartyOnMap.IndexOf(characterOnMap as ActorOnMap);
                if (index >= 0)
                {
                    name = "PartyOnMap";
                }
            }

            if (name == null)
            {
                index = MapEventExecutionController.Instance.EventsOnMap.IndexOf(characterOnMap as EventOnMap);
                if (index >= 0)
                {
                    name = "EventsOnMap";
                }
            }

            if (name == null)
            {
                return "?";
            }

            return $"{name}[{index}]";
        }
        
        /// <summary>
        /// ダメージ床の取得
        /// </summary>
        /// <returns></returns>
        public TileDataModel GetDamageTileData() {
            return _tileDataModel;
        }
    }
}