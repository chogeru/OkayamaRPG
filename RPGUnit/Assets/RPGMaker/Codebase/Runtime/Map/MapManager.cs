#define USE_TRY_TO_MOVE_CHARACTER_DIRECTION_ENUM
#define USE_TILEMAP_ORIGIN_AND_SIZE_BY_CREATE_UPPER_LAYER
#define USE_CHARACTER_MOVE_AS   //定義するとA*アルゴリズムで、タップした場所に障害物を回避して移動するようになる。コメントアウトで従来動作。

using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Encounter;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Troop;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Knowledge.Misc;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement;
using RPGMaker.Codebase.Runtime.Battle;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Character;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.GameProgress;
using RPGMaker.Codebase.Runtime.Common.Enum;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using RPGMaker.Codebase.Runtime.Map.Component.Map;
using RPGMaker.Codebase.Runtime.Scene.Map;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime.RuntimePlayerDataModel;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime.RuntimeSystemConfigDataModel;

namespace RPGMaker.Codebase.Runtime.Map
{
    /// <summary>
    /// マップの管理クラス
    /// </summary>
    public static class MapManager
    {
        // マップ内の1タイルの横および縦の表示サイズ。
        public const float RuntimeTileDisplaySizeInMap = 
            CoreSystem.Service.MapManagement.Repository.TileRepository.TileDefaultSize / 100f;

        //キャラクターのダッシュ時の加速倍率
        private const float DASH_SPEED = 2;

        // 利用パッケージクラス
        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// メニュー
        /// </summary>
        public static MenuBase menu = null;
        /// <summary>
        /// 現在表示中のマップの、MapDataModel
        /// </summary>
        public static MapDataModel CurrentMapDataModel { get; set; }
        public static  int IsDisplayName { get; set; }
        

        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// GameObjectの座標とタイル座標の変換
        /// </summary>
        private static TilesOnThePosition       _tilesOnThePosition;
        /// <summary>
        /// エンカウント用のDataModel
        /// </summary>
        private static List<EncounterDataModel> _encounterDataModels;
        /// <summary>
        /// ゲームのConfig設定
        /// </summary>
        private static RuntimeConfigDataModel _runtimeConfigDataModel;
        /// <summary>
        /// デフォルトキャラクター移動速度
        /// </summary>
        private static float _defaultCharacterSpeed;

        /// <summary>
        /// アニメーション
        /// </summary>
        private static List<CharacterAnimation> _characterAnimations;
        public static void AddAnimation(CharacterAnimation characterAnimation) { _characterAnimations.Add(characterAnimation); }

        // イベント
        //--------------------------------------------------------------------------------------------------------------

        // 表示要素プロパティ
        //--------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// MapDataModel
        /// </summary>
        private static MapDataModel     initialMap;
        /// <summary>
        /// マップのRoot
        /// </summary>
        private static GameObject       _rootGameObject;
        /// <summary>
        /// Menu
        /// </summary>
        private static GameObject       _menuGameObject;
        /// <summary>
        /// カメラ
        /// </summary>
        private static Camera           _camera;
        /// <summary>
        /// カメラの座標
        /// </summary>
        private static Vector3          _cameraPos;
        /// <summary>
        /// 操作キャラクター
        /// </summary>
        private static ActorOnMap       _actorOnMap;
        /// <summary>
        /// パーティメンバー
        /// </summary>
        private static List<ActorOnMap> _partyOnMap;

        /// <summary>
        /// 操作中のキャラクター（アクター or 乗り物）
        /// </summary>
        public static CharacterOnMap OperatingCharacter
        {
            get
            {
                if (_moveType == _moveTypeEnum.Vehicle)
                {
                    return _vehicleOnMap;
                }
                return _actorOnMap;
            }
        }

        public static List<ActorOnMap> OperatingParty
        {
            get
            {
                if (!_playerFollow)
                {
                    return null;
                }
                if (_moveType != _moveTypeEnum.Actor)
                {
                    return null;
                }
                if (_partyOnMap?.Count == 0)
                {
                    return null;
                }
                return _partyOnMap;
            }
        }
        /// <summary>操作中のアクターの情報</summary>
        public static ActorOnMap OperatingActor => _actorOnMap;
        /// <summary>パーティメンバーの情報</summary>
        public static List<ActorOnMap> PartyOnMap => _partyOnMap;
        /// <summary>
        /// パーティメンバーの追加（予約）
        /// </summary>
        public static List<string> _partyWaitAdd = new List<string>();
        /// <summary>
        /// 乗り物に乗り降りしている最中かどうか
        /// </summary>
        private static bool               _isRiding;
        /// <summary>
        /// 現在操作中の乗り物
        /// </summary>
        private static VehicleOnMap       _vehicleOnMap;
        /// <summary>
        /// マップ内に存在する乗り物
        /// </summary>
        private static List<VehicleOnMap> _vehiclesOnMap;
        /// <summary>
        /// 乗り物のAction
        /// </summary>
        private static Action             _vehicleAction;
        /// <summary>搭乗中の乗り物のID</summary>
        public static string CurrentVehicleId { get; private set; } = "";

        private static bool _initScene = false;

        /// <summary>
        /// 現在搭乗中の乗り物を返却
        /// </summary>
        /// <returns></returns>
        public static VehicleOnMap GetRideVehicle() {
            return _vehicleOnMap;
        }

        /// <summary>
        /// 現在のマップのTilesOnThePositionを返却
        /// </summary>
        /// <returns></returns>
        public static TilesOnThePosition CurrentTileData(Vector2 targetPositionOnTile) {
            _tilesOnThePosition.InitForRuntime(CurrentMapDataModel, targetPositionOnTile);
            return _tilesOnThePosition;
        }


        private static CharacterMoveDirectionEnum _actorMoveDirectionEnum  = CharacterMoveDirectionEnum.Down;

#if USE_TRY_TO_MOVE_CHARACTER_DIRECTION_ENUM
        private static CharacterMoveDirectionEnum _tryToMoveCharacterDirectionEnum = CharacterMoveDirectionEnum.Down;
#endif
        private static Vector3 _actorPosition;
        private static Vector3 _party1Position;
        private static Vector3 _party2Position;

        private static bool     _isRightAbove = true;
        private static bool     _playerFollow = false;

        /// <summary>
        /// GAMEOVERのシーンへの切り替え中
        /// </summary>
        private static bool _movingGameover = false;

        // Application usecases
        //--------------------------------------------------------------------------------------------------------------
        private static MapManagementService      _mapManagementService;
        private static DatabaseManagementService _databaseManagementService;

        // enum / interface / local class
        //--------------------------------------------------------------------------------------------------------------
        //現在の動作タイプ
        private static _moveTypeEnum _moveType;
        private static bool _isEventBattle = false;

        /// <summary>
        /// マップ上での操作対象のEnum
        /// </summary>
        private enum _moveTypeEnum
        {
            Actor = 0,
            Vehicle
        }

        /// <summary>
        /// パーティーメンバーの集合種別
        /// </summary>
        public enum ReasonForPartyMemberAllIn
        {
            Event = 0,
            Vehicle
        }
        
        //影画像の読み込みPath
        private static string         shadowImagePath = "Assets/RPGMaker/Storage/Images/System/Shadow1.png";
        private static SpriteRenderer _shadowSpriteRenderer;

        private static MapLoop _mapLoop;
        public static MapLoop LoopInstance { get { return _mapLoop; } }

        /// <summary>
        /// 今フレームで全員の移動が完了し、かつ次移動が行われる場合に、次フレームから次の移動を開始するためのフラグ
        /// </summary>
        private static bool _nextWalkFlg = false;


        /// <summary>
        /// エンカウントマネージャー
        /// RPGツクールMVに近い仕組み
        /// </summary>
        public class EncountManager
        {
            private EncounterDataModel _encounterDataModel = null;
            private float _encounterCount = float.MaxValue;

            /// <summary>
            /// 現地点でのエンカウンターデータモデルを取得
            /// </summary>
            /// <param name="saveData"></param>
            /// <returns></returns>
            public EncounterDataModel GetEncounterDataModel(bool saveData = true) {

                if (MapManager.CurrentMapDataModel == null)
                {
                    return null;
                }


                EncounterDataModel encounterDataModel = null;

                // リージョンごとのエンカウント設定情報。
                var regionTileDataModel = _tilesOnThePosition.GetRegionTileDataModel();
                if (regionTileDataModel != null)
                {
                    for (int i = 0; i < MapManager._encounterDataModels.Count; i++)
                    {
                        if (MapManager._encounterDataModels[i].mapId == MapManager.CurrentMapDataModel.id && MapManager._encounterDataModels[i].region == regionTileDataModel.regionId)
                        {
                            encounterDataModel = MapManager._encounterDataModels[i];
                            break;
                        }
                    }
                }

                // マップのエンカウント設定情報。
                if (encounterDataModel == null)
                {
                    for (int i = 0; i < MapManager._encounterDataModels.Count; i++)
                    {
                        if (MapManager._encounterDataModels[i].mapId == MapManager.CurrentMapDataModel.id && MapManager._encounterDataModels[i].region == 0)
                        {
                            encounterDataModel = _encounterDataModels[i];
                            break;
                        }
                    }
                }

                //現在のエンカウント設定を保持
                if (saveData)
                {
                    _encounterDataModel = encounterDataModel;
                }
                return encounterDataModel;
            }

            public void MakeCount() {
                var encounterData = GetEncounterDataModel(true);
                if (encounterData == null)
                {
                    _encounterCount = float.MaxValue;
                    return;
                }

                int n = encounterData.step;
                _encounterCount = UnityEngine.Random.Range(0, n) + UnityEngine.Random.Range(0, n) + 1;
            }

            public void UpdateCount() {
                //以前とマップが変わっている場合
                //以前とRegionが変わっている場合には、再度Makeしなおす
                EncounterDataModel work = GetEncounterDataModel(false);
                if (work != _encounterDataModel)
                {
                    if (work != null && _encounterDataModel != null && work.mapId != _encounterDataModel.mapId)
                    {
                        //このケースは歩数を初期化
                        _encounterCount = float.MaxValue;
                    }
                    _encounterDataModel = work;
                }

                if (_encounterCount == float.MaxValue)
                {
                    _encountManager.MakeCount();
                }
                else if (CanEncounter())
                {
                    _encounterCount -= ProgressValue();
                }
            }

            public bool IsEncount() {
                return _encounterCount <= 0.0f;
            }

            private bool CanEncounter() {
                // RPGツクールMVの場合、以下となるらしい。
                // 飛行船の中、強制移動、デバッグ用スルーの場合はfalse。
                return true;
            }

            private float ProgressValue() {
                // RPGツクールMVの場合、以下となるらしい。
                // 現在地点がブッシュの場合は2、それ以外は1。
                // パーティーがエンカウント半減状態の場合は、値を1/2に。
                // 更に船の中の場合は、値を1/2に。
                return 1.0f;
            }

            public void ClearEncount() {

            }
        }

        private static EncountManager _encountManager;
        public static EncountManager encountManager { get { return _encountManager; } }

        public static bool IsMovingGameOver { get { return _movingGameover; } }

        // 変数
        //--------------------------------------------------------------------------------------------------------------

        // methods
        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// マップ初期化
        /// </summary>
        /// <param name="sceneRootGameObject"></param>
        /// <param name="camera"></param>
        /// <param name="menuObject"></param>
        public static async void InitManager(GameObject sceneRootGameObject, Camera camera, GameObject menuObject) {
            _initScene = true;
            //状態の更新
            GameStateHandler.SetGameState(GameStateHandler.GameState.MAP);

            //入力ハンドリング初期化
            InputDistributor.RemoveInputHandlerWithGameState(GameStateHandler.GameState.TITLE);
            InputDistributor.RemoveInputHandlerWithGameState(GameStateHandler.GameState.MAP);
            InputDistributor.RemoveInputHandlerWithGameState(GameStateHandler.GameState.BATTLE);

            //移動方法の振り分け
            ChangeMoveSubject(_moveTypeEnum.Actor);

            //マウス入力の登録

            // HUD系UIハンドリング
            HudDistributor.Instance.AddHudHandler(new HudHandler(sceneRootGameObject));

            // 表示要素
            _rootGameObject = sceneRootGameObject;
            _menuGameObject = menuObject;
            _camera = camera;
            _camera.orthographic = true;

            //手前にあるものを上に表示する
            _camera.transparencySortMode = TransparencySortMode.CustomAxis;
            _camera.transparencySortAxis = new Vector3(0.0f, 0.0000001f, 1.0f);

            // タイル当たり判定コンポーネント初期化
            _tilesOnThePosition = _rootGameObject.AddComponent<TilesOnThePosition>();

            // userCase
            _mapManagementService = new MapManagementService();
            _databaseManagementService = new DatabaseManagementService();
            
            //ランタイムデータの取得
            _runtimeConfigDataModel = DataManager.Self().GetRuntimeConfigDataModel();

            // エンカウントデータ取得
            _encounterDataModels = _databaseManagementService.LoadEncounter();
            // エンカウントマネージャ。
            _encountManager = new EncountManager();

            // マップ初期表示
            var systemSetting = DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map;
            var initialMap = _mapManagementService.LoadMapById(systemSetting.mapId);
            var partyPosition = new Vector2(
                systemSetting.x,
                systemSetting.y
            );

            _mapLoop = _rootGameObject.AddComponent<MapLoop>();

            var runtimeSystemConfig = DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig;
            
            //サウンド鳴動
            SoundManager.Self().Init();
            SoundCommonDataModel sound = null;

            //BGM自動演奏が有効であり、BGM設定も行われている場合に、そのBGMを読み込む
            if (initialMap.autoPlayBGM && !string.IsNullOrEmpty(initialMap.bgmID))
            {
                sound = new SoundCommonDataModel(initialMap.bgmID, initialMap.bgmState.pan, initialMap.bgmState.pitch, initialMap.bgmState.volume);
            }
            
            //セーブデータにBGMが保存されている場合には、セーブデータのBGMの方を優先する
            if (!string.IsNullOrEmpty(runtimeSystemConfig.bgmOnSave.name))
            {
                sound = new SoundCommonDataModel(runtimeSystemConfig.bgmOnSave.name, runtimeSystemConfig.bgmOnSave.pan,
                    runtimeSystemConfig.bgmOnSave.pitch, runtimeSystemConfig.bgmOnSave.volume);
            }

            //BGMが設定されていたら再生
            if (sound != null)
            {
                SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_BGM, sound);
                _ = SoundManager.Self().PlayBgm(() => {
                    SoundManager.Self().ChangeBgmState(_runtimeConfigDataModel.bgmVolume);
                });
            }

            //BGS自動演奏が有効であり、BGS設定も行われている場合に、そのBGSを読み込む
            sound = null;
            if (initialMap.autoPlayBgs && !string.IsNullOrEmpty(initialMap.bgsID))
            {
                sound = new SoundCommonDataModel(initialMap.bgsID, initialMap.bgsState.pan, initialMap.bgsState.pitch, initialMap.bgsState.volume);
            }

            //セーブデータにBGSが保存されている場合には、セーブデータのBGSの方を優先する
            if (!string.IsNullOrEmpty(runtimeSystemConfig.bgsOnSave.name))
            {
                sound = new SoundCommonDataModel(runtimeSystemConfig.bgsOnSave.name,
                    runtimeSystemConfig.bgsOnSave.pan,
                    runtimeSystemConfig.bgsOnSave.pitch, runtimeSystemConfig.bgsOnSave.volume);
            }

            //BGSが設定されていたら再生
            if (sound != null)
            {
                SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_BGS, sound);
                _ = SoundManager.Self().PlayBgs();
                SoundManager.Self().ChangeBgsState(_runtimeConfigDataModel.bgsVolume);
            }

            //マップを初めて生成した時の初期化処理
            CurrentMapDataModel = null;
            _actorOnMap = null;
            _partyOnMap = null;
            _partyWaitAdd = new List<string>();
            _vehicleOnMap = null;
            _isRightAbove = false;
            _isEventBattle = false;
            _vehiclesOnMap = new List<VehicleOnMap>();
            CurrentVehicleId = "";
            _characterAnimations = new List<CharacterAnimation>();
            MapEventExecutionController.Instance.Initialize();

            
            var eventBattleBack1 = DataManager.Self().GetRuntimeSaveDataModel()
                .runtimePlayerDataModel.map.eventBattleBack1;
            var eventBattleBack2 = DataManager.Self().GetRuntimeSaveDataModel()
                .runtimePlayerDataModel.map.eventBattleBack2;

            //初期マップをロード
            ChangeMapForRuntime(initialMap, partyPosition);
            
            //セーブにイベント背景が設定されている場合、設定
            BattleSceneTransition.Instance.EventMapBackgroundImage1 = eventBattleBack1;
            BattleSceneTransition.Instance.EventMapBackgroundImage2 = eventBattleBack2;


            // マップ配置物設定
            {
                bool playerFlg = false;

                // 設定処理
                void SetMapData(CharacterOnMap character, RuntimeOnMapDataModel.OnMapData dataModel) {
                    character.SetToPositionOnTile(new Vector2(dataModel.x_next, dataModel.y_next));
                    character.ChangeAsset(dataModel.AssetId);
                    character.SetThrough(dataModel.isThrough);
                    character.SetAnimation(dataModel.isAnimation, dataModel.isSteppingAnimation);
                    character.SetIsLockDirection(dataModel.isLockDirection);
                    character.TryChangeCharacterDirection(dataModel.direction, true);
                    character.SetLastMoveDirection(dataModel.lastMoveDirectionEnum);
                    character.SetOpacity(dataModel.opacity);
                }

                void SetMapEventData(EventOnMap character, RuntimeOnMapDataModel.OnMapData dataModel) {
                    character.SetMapEventData(dataModel.eventData);
                }

                void SetMapMoveData(EventOnMap character, RuntimeOnMapDataModel.OnMapData dataModel) {
                    character.SetMapMoveData(dataModel.moveData);
                }

                // イベント設定
                var events = MapEventExecutionController.Instance.GetEvents();
                foreach (var data in events)
                    foreach (var saveEvent in DataManager.Self().GetRuntimeSaveDataModel().RuntimeOnMapDataModel.onMapDatas)
                        if (data.MapDataModelEvent.eventId == saveEvent.id)
                        {
                            SetMapData(data, saveEvent);
                            SetMapEventData(data, saveEvent);
                            SetMapMoveData(data, saveEvent);
                        }

                // パーティ設定
                for (int i = 0; i < PartyOnMap?.Count; i++)
                {
                    var member = GetPartyGameObject(i).GetComponent<ActorOnMap>();
                    foreach (var saveEvent in DataManager.Self().GetRuntimeSaveDataModel().RuntimeOnMapDataModel.onMapDatas)
                        if (saveEvent.isActor == true && saveEvent.id == (i + 1).ToString())
                        {
                            SetMapData(member, saveEvent);
                        }
                }

                // プレイヤー設定
                var player = GetCharacterGameObject().GetComponent<ActorOnMap>();
                foreach (var saveEvent in DataManager.Self().GetRuntimeSaveDataModel().RuntimeOnMapDataModel.onMapDatas)
                    if (saveEvent.isActor == true && saveEvent.id == "0")
                    {
                        SetMapData(player, saveEvent);
                        //Continueの際に、RuntimeOnMapDataModel に保持している Opacity を設定しなおす（データコンバート用）
                        DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.opacity = saveEvent.opacity;
                        playerFlg = true;
                    }

                if (!playerFlg)
                {
                    //プレイヤー設定が存在しない場合は、NEWGAMEによる新規作成
                    //初期設定では、透明ON/OFFのみ設定可能であるため、透明度については1.0fで初期化する
                    DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.opacity = 1.0f;
                }
                else
                {
                    if (DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.actors.Count == 0)
                    {
                        //Continueした際に、パーティメンバーが1名も存在しない場合、先頭キャラクターを透明にする
                        player.SetTransparent(true);
                    }
                }

                // パーティメンバーの透明状態の確認・反映
                if (DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.transparent == 1)
                {
                    foreach (var actor in MapManager.GetAllActorOnMap())
                        actor.SetTransparent(true);
                }

                //不透明設定
                var opacity = (float) (DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.opacity);
                foreach (var actor in MapManager.GetAllActorOnMap())
                    actor.SetOpacity(opacity);
            }

            //マップループ用の座標補正
            partyPosition = _mapLoop.SetInitializePosition(partyPosition);
            
            //異なるマップの場合、乗り物の座標も一緒に移動する
            if (_moveType != _moveTypeEnum.Actor)
            {
                _vehicleOnMap.SetToPositionOnTile(partyPosition);
                _actorOnMap.SetToPositionOnTile(partyPosition);
                //パーティメンバーも強制的に同じ座標に移動
                if (_partyOnMap != null)
                {
                    for (int i = 0; i < _partyOnMap.Count; i++)
                        _partyOnMap[i].SetToPositionOnTile(partyPosition);
                }

                //キー入力等を乗り物用に変更
                ChangeMoveSubject(_moveTypeEnum.Vehicle);
                _vehicleOnMap.SetCharacterEnable(true);
            }
            else
            {
                _actorOnMap.SetToPositionOnTile(partyPosition);
                //パーティメンバーも強制的に同じ座標に移動
                if (_partyOnMap != null)
                {
                    for (int i = 0; i < _partyOnMap.Count; i++)
                        _partyOnMap[i].SetToPositionOnTile(partyPosition);
                }
            }
            
            // カメラ位置を復元
            _cameraPos =
                new Vector3(
                    DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.cameraX,
                    DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.cameraY, 0);
            _camera.transform.localPosition = _cameraPos + new Vector3(0.5f, 0.5f, -100.0f);

            // イベントの復元
            // タイマー
            if (DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.timer != -1)
            {
                GameTimer gameTimer = HudDistributor.Instance.NowHudHandler().CreateGameTimer();
                gameTimer.SetGameTimer(true, DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.timer);
            }
            // ピクチャ
            if (DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.picture.pictureData != null &&
                DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.picture.pictureData.Count > 0)
            {
                HudDistributor.Instance.NowHudHandler().LoadPicture();
            }

            //プレイ時間登録
            TimeHandler.Instance.SetPlayTime(DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig);
            

            //マップ描画しきるまでにキー登録をすこし待つ
            await Task.Delay(50);
            _initScene = false;
            //移動方法の振り分け
            ChangeMoveSubject(_moveType);

            //キー入力の登録
            InputDistributor.AddInputHandler(GameStateHandler.GameState.MAP,HandleType.Decide, DesiceKeyEvent);
        }

        private static async void ChangeMoveSubject(_moveTypeEnum type) {
            _moveType = type;
            SetVehicleRide();

            var saveData = DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.vehicles;

            //移動方法の振り分け
            switch (_moveType)
            {
                //キャラクター
                case _moveTypeEnum.Actor:
                    //入力ハンドリング初期化
                    InputDistributor.RemoveInputHandlerWithGameState(GameStateHandler.GameState.MAP);

                    if (!_initScene)
                    {
                        //キー入力の登録
                        InputDistributor.AddInputHandler(GameStateHandler.GameState.MAP, HandleType.Left,
                            TryToMoveLeftCharacter);
                        InputDistributor.AddInputHandler(GameStateHandler.GameState.MAP, HandleType.Right,
                            TryToMoveRightCharacter);
                        InputDistributor.AddInputHandler(GameStateHandler.GameState.MAP, HandleType.Up,
                            TryToMoveUpCharacter);
                        InputDistributor.AddInputHandler(GameStateHandler.GameState.MAP, HandleType.Down,
                            TryToMoveDownCharacter);
                        InputDistributor.AddInputHandler(GameStateHandler.GameState.MAP, HandleType.Decide,
                            DesiceKeyEvent);

                        //マウス入力の登録
                        InputDistributor.AddInputHandler(GameStateHandler.GameState.MAP, HandleType.LeftClick,
                            TryToMoveCharacterClick);
                    }

                    //TilesOnThePositionの読み込み
                    if (_tilesOnThePosition != null && CurrentMapDataModel != null && _actorOnMap != null)
                    {
                        CurrentTileData(_actorOnMap.GetCurrentPositionOnTile());
                    }

                    if (_vehicleOnMap != null)
                    {
                        for (int i = 0; i < saveData.Count; i++)
                        {
                            if (saveData[i].id == _vehicleOnMap.CharacterId)
                            {
                                saveData[i].ride = 0;
                                break;
                            }
                        }
                        _vehicleOnMap = null;
                    }

                    //今のキャラクター達の移動速度を、元に戻す
                    _actorOnMap?.SetCharacterRide(false);
                    _partyOnMap?.ForEach(v => v.SetCharacterRide(false));

                    //ダッシュ可能かどうかの設定
                    if (CurrentMapDataModel != null && !CurrentMapDataModel.forbidDash)
                    {
                        SetCanDashForAllPlayerCharacters(true);
                    }

                    await Task.Delay(500);
                    _isRiding = false;
                    break;

                //乗り物
                case _moveTypeEnum.Vehicle:
                    //入力ハンドリング初期化
                    InputDistributor.RemoveInputHandlerWithGameState(GameStateHandler.GameState.MAP);

                    if (!_initScene)
                    {
                        //登録
                        InputDistributor.AddInputHandler(GameStateHandler.GameState.MAP, HandleType.Left,
                            TryToMoveLeftVehicle);
                        InputDistributor.AddInputHandler(GameStateHandler.GameState.MAP, HandleType.Right,
                            TryToMoveRightVehicle);
                        InputDistributor.AddInputHandler(GameStateHandler.GameState.MAP, HandleType.Up,
                            TryToMoveUpVehicle);
                        InputDistributor.AddInputHandler(GameStateHandler.GameState.MAP, HandleType.Down,
                            TryToMoveDownVehicle);
                        InputDistributor.AddInputHandler(GameStateHandler.GameState.MAP, HandleType.Decide,
                            DesiceKeyEvent);

                        //マウス入力の登録
                        InputDistributor.AddInputHandler(GameStateHandler.GameState.MAP, HandleType.LeftClick,
                            TryToMoveVehicleClick);
                    }

                    //今のキャラクター達を非表示にする
                    _actorOnMap.SetCharacterEnable(false);
                    _partyOnMap?.ForEach(v => v.SetCharacterEnable(false));

                    //今のキャラクター達の移動速度を乗り物に合わせる
                    _actorOnMap.SetCharacterRide(true, _vehicleOnMap.GetCharacterSpeed());
                    _partyOnMap?.ForEach(v => v.SetCharacterRide(true, _vehicleOnMap.GetCharacterSpeed()));

                    //TilesOnThePositionの読み込み
                    if (_tilesOnThePosition != null && CurrentMapDataModel != null && _vehicleOnMap != null)
                    {
                        CurrentTileData(_vehicleOnMap.GetCurrentPositionOnTile());
                    }

                    for (int i = 0; i < saveData.Count; i++)
                    {
                        if (saveData[i].id == _vehicleOnMap.CharacterId)
                        {
                            saveData[i].ride = 1;
                            break;
                        }
                    }

                    //ダッシュは不可
                    if (!CurrentMapDataModel.forbidDash)
                    {
                        SetCanDashForAllPlayerCharacters(false);
                    }

                    await Task.Delay(500);
                    _isRiding = false;
                    break;
            }
        }

        /// <summary>
        /// マップを表示する
        /// </summary>
        /// <param name="mapDataModel"></param>
        /// <param name="partyPosition"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static MapPrefabControl ChangeMapForRuntime(MapDataModel mapDataModel, Vector2 partyPosition, CharacterMoveDirectionEnum direction = CharacterMoveDirectionEnum.Down) {
            // 現在のマップと、移動先のマップが異なる場合にのみ処理する
            if (CurrentMapDataModel == null || CurrentMapDataModel.id != mapDataModel.id)
            {
                DebugUtil.Log($"ChangeMap(MapDataModel{{id={mapDataModel.id}, name={mapDataModel.name}, {mapDataModel.width}x{mapDataModel.height}}})");

                BattleSceneTransition.Instance.EventMapBackgroundImage1 = string.Empty;
                BattleSceneTransition.Instance.EventMapBackgroundImage2 = string.Empty;

                DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.eventBattleBack1 = String.Empty;
                DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.eventBattleBack2 = String.Empty;

                
                // 異なるマップ
                // 保持しているマップを破棄する
                CurrentMapDataModel?.MapPrefabManagerForRuntime.DeletePrefab();

                // 現在のマップを保持する
                CurrentMapDataModel = mapDataModel;
                var mapPrefabManager = CurrentMapDataModel.MapPrefabManagerForRuntime;
                GameObject mapPrefab = mapPrefabManager.LoadPrefab();

                // Prefabの強調表示設定は、Runtimeでは無効であるため、すべて元に戻す
                foreach (var layer in mapDataModel.LayersForRuntime)
                {
                    if (layer.spr != null)
                    {
                        var sprTransform = mapDataModel.GetLayerTransformForRuntime(layer.type);
                        var spr = sprTransform.GetComponent<SpriteRenderer>();
                        spr.color = new Color(1f, 1f, 1f, 1f);
                    }
                    if (layer.tilemap == null) continue;
                    layer.tilemap.color = new Color(1f, 1f, 1f, 1f);
                }

                //遠景のマスクレイヤーがあれば、非アクティブにする
                var maskGameObjetName = $"Layer {nameof(MapDataModel.Layer.LayerType.DistantView)} Mask";
                var maskGameObject = mapPrefab.transform.Find(maskGameObjetName)?.gameObject;
                if (maskGameObject != null)
                {
                    if (maskGameObject.GetComponent<SpriteMask>() != null && maskGameObject.GetComponent<SpriteMask>().enabled)
                    {
                        maskGameObject.GetComponent<SpriteMask>().enabled = false;
                    }
                }

                //全部アクティブにしたあと、エンカウントレイヤーだけ非アクティブに
                var tileMapObjects = mapPrefab.GetComponentsInChildren<Tilemap>();
                foreach (var tilemap in tileMapObjects)
                {
                    tilemap.gameObject.SetActive(true);
                    if (tilemap.gameObject.GetComponent<TilemapRenderer>())
                        tilemap.gameObject.GetComponent<TilemapRenderer>().enabled = true;
                    else if (tilemap.gameObject.GetComponent<SpriteRenderer>())
                        tilemap.gameObject.GetComponent<SpriteRenderer>().enabled = true;

                    //タイルのアニメーション速度等を、最新の状態に更新する
                    tilemap.RefreshAllTiles();
                }
                tileMapObjects[tileMapObjects.Length - 1].gameObject.SetActive(false);
                if (tileMapObjects[tileMapObjects.Length - 1].gameObject.GetComponent<TilemapRenderer>())
                    tileMapObjects[tileMapObjects.Length - 1].gameObject.GetComponent<TilemapRenderer>().enabled = true;
                else if (tileMapObjects[tileMapObjects.Length - 1].gameObject.GetComponent<SpriteRenderer>())
                    tileMapObjects[tileMapObjects.Length - 1].gameObject.GetComponent<SpriteRenderer>().enabled = true;

                mapPrefab.transform.localPosition = new Vector3(mapPrefab.transform.localPosition.x, mapPrefab.transform.localPosition.y, 4.0f);

                //カメラ消されるので。
                _cameraPos = Vector3.zero;
                _camera.transform.SetParent(_rootGameObject.transform.parent);
                _camera.transform.localPosition = _cameraPos + new Vector3(0.5f, 0.5f, -100.0f);

                //イベントデータが存在する場合に、イベントを破棄
                MapEventExecutionController.Instance.DestroyAllEvent();

                //マップ移動前のGameObjectをすべて破棄
                foreach (Transform child in _rootGameObject.transform)
                {
                    //マップ移動後に継続するイベントが存在する場合には、削除を行わず、非表示とする
                    if (MapEventExecutionController.Instance.GetCarryEventOnMap() == child.gameObject)
                    {
                        continue;
                    }
                    //乗り物に乗ったまま移動した場合は、削除を行わない
                    if (_vehicleOnMap != null && _vehicleOnMap.gameObject == child.gameObject)
                    {
                        continue;
                    }
                    GameObject.Destroy(child.gameObject);
                }

                mapPrefab.transform.SetParent(_rootGameObject.transform);

                // 遠景のデータ設定
                mapPrefab.transform.GetChild((int)MapDataModel.Layer.LayerType.DistantView).
                    gameObject.GetComponent<DistantViewManager>().SetData(mapDataModel.Parallax);

                // キャラクター設定
                SetActorOnMap(partyPosition);
                SetVehicleOnMap(partyPosition);

                // イベント関連の初期化
                MapEventExecutionController.Instance.InitEvents(mapDataModel, _rootGameObject);

                // 下をくぐり抜けるタイル表示用の上層レイヤーを追加する (マップのループ設定の前に呼ぶ)。
                AddMapUpperLayerForPassUnderTilesForRuntime(mapDataModel);

                // マップのループ設定
                _mapLoop.SetUp(mapPrefab, mapDataModel, _vehiclesOnMap, MapEventExecutionController.Instance.GetEvents());

                //マップループ用の座標補正
                partyPosition = _mapLoop.SetInitializePosition(partyPosition);

                // マップの各レイヤーの設定 (上層レイヤーを追加前の処理)。
                foreach (MapDataModel.Layer.LayerType layerType in Enum.GetValues(typeof(MapDataModel.Layer.LayerType)))
                {
                    var layerGameObject = mapPrefabManager.GetLayerTransform(layerType).gameObject;

                    // マップのレイヤーを表示する(Collision、Regionレイヤーは非表示)
                    layerGameObject.SetActive(layerType != MapDataModel.Layer.LayerType.BackgroundCollision &&
                                              layerType != MapDataModel.Layer.LayerType.ForRoute &&
                                              layerType != MapDataModel.Layer.LayerType.Region);

                    MapRenderingOrderManager.SetLayerRendererSortingLayer(layerGameObject, layerType);
                }

                // マップの各レイヤーの設定その2 (上層レイヤーを追加後の処理)。
                foreach (MapDataModel.Layer.LayerType layerType in Enum.GetValues(typeof(MapDataModel.Layer.LayerType)))
                {
                    var layerGameObject = mapPrefabManager.GetLayerTransform(layerType).gameObject;

                    // とても動作確認しにくいので設定。
                    // (本来ならnameへの設定が適切だか支障が出る可能性があるのでtagに設定)。
                    layerGameObject.tag = "Map Layer" + layerType.ToString();
                }

                //異なるマップの場合、乗り物の座標も一緒に移動する
                if (_moveType != _moveTypeEnum.Actor)
                {
                    _vehicleOnMap.SetToPositionOnTile(partyPosition);
                    _actorOnMap.SetToPositionOnTile(partyPosition);
                    //パーティメンバーも強制的に同じ座標に移動
                    if (_partyOnMap != null)
                    {
                        for (int i = 0; i < _partyOnMap.Count; i++)
                            _partyOnMap[i].SetToPositionOnTile(partyPosition);
                    }

                    //キー入力等を乗り物用に変更
                    ChangeMoveSubject(_moveTypeEnum.Vehicle);
                    _vehicleOnMap.SetCharacterEnable(true);
                }
                else
                {
                    _actorOnMap.SetToPositionOnTile(partyPosition);
                    //パーティメンバーも強制的に同じ座標に移動
                    if (_partyOnMap != null)
                    {
                        for (int i = 0; i < _partyOnMap.Count; i++)
                            _partyOnMap[i].SetToPositionOnTile(partyPosition);
                    }
                }

                // アニメーション削除
                DeleteAnimation();

                // パーティメンバーの透明状態の確認・反映
                if (DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.transparent == 1)
                {
                    foreach (var actor in MapManager.GetAllActorOnMap())
                        actor.SetTransparent(true);
                }

                //不透明設定
                var opacity = (float) (DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.opacity);
                foreach (var actor in MapManager.GetAllActorOnMap())
                    actor.SetOpacity(opacity);


                // HudHandler生成
                HudDistributor.Instance.NowHudHandler().DisplayInit();

                // 画面の色調変更の反映
                if (DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.tone != null && DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.tone.Count == 4)
                {
                    Color color = new Color(
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.tone[0],
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.tone[1],
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.tone[2],
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.tone[3]
                    );
                    HudDistributor.Instance.NowHudHandler().ChangeColor(null, color, DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.tone[3], 1, false);
                }

                // 天候の反映
                HudDistributor.Instance.NowHudHandler().ChangeWeather(null, 
                    DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.weather.type,
                    DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.weather.power,
                    1, false);
            }
            else
            {
                // 同一マップ
                //同一マップ内の移動では、座標を変更するのみとする
                _mapLoop.MovePointLoopCharacter(partyPosition, _actorOnMap);
            }

            //向きの変更
            _actorOnMap.ChangeCharacterDirection(direction);

            MapPrefabControl mapPrefabControl;
            if (CurrentMapDataModel.MapPrefabManagerForRuntime.mapPrefab.GetComponent<MapPrefabControl>() == null)
            {
                mapPrefabControl =
                    CurrentMapDataModel.MapPrefabManagerForRuntime.mapPrefab.AddComponent<MapPrefabControl>();
            }
            else
            {
                mapPrefabControl =
                    CurrentMapDataModel.MapPrefabManagerForRuntime.mapPrefab.GetComponent<MapPrefabControl>();
            }

            return mapPrefabControl;
        }

        /// <summary>
        /// 下をくぐり抜けるタイル表示用の上層レイヤーを追加する
        /// </summary>
        /// <param name="mapDataModel"></param>
        private static void AddMapUpperLayerForPassUnderTilesForRuntime(MapDataModel mapDataModel)
        {
            GameObject mapPrefab = mapDataModel.MapPrefabManagerForRuntime.LoadPrefab();

            foreach (var sourceLayerType in new MapDataModel.Layer.LayerType[]
                {
                    MapDataModel.Layer.LayerType.A,
                    MapDataModel.Layer.LayerType.B,
                    MapDataModel.Layer.LayerType.C,
                    MapDataModel.Layer.LayerType.D,
                })
            {
                // 参照レイヤー
                var targetLayer =
                    mapDataModel.MapPrefabManagerForRuntime.GetLayerTransform(sourceLayerType).GetComponent<Tilemap>();

                // 上層用レイヤーを作成追加。
                var upperLayerTransform = new GameObject();
                upperLayerTransform.gameObject.transform.position = Vector3.zero; 
                upperLayerTransform.gameObject.AddComponent<Tilemap>();
                var tilemapRenderer = upperLayerTransform.gameObject.AddComponent<TilemapRenderer>();
                tilemapRenderer.sortOrder = targetLayer.gameObject.GetComponent<TilemapRenderer>().sortOrder;
                upperLayerTransform.gameObject.transform.SetParent(mapPrefab.transform);
                upperLayerTransform.name = $"Layer {sourceLayerType}_Upper";

                upperLayerTransform.GetComponent<TilemapRenderer>().sortingLayerID =
                    MapRenderingOrderManager.GetMapUpperLayerSortingLayerId(sourceLayerType);

                var upperLayerTilemap = upperLayerTransform.GetComponent<Tilemap>();

#if USE_TILEMAP_ORIGIN_AND_SIZE_BY_CREATE_UPPER_LAYER
                var xRange = Enumerable.Range(0, mapDataModel.width);
                var yRange = Enumerable.Range(-mapDataModel.height + 1, mapDataModel.height);

                try
                {
                    var xRangeWork = Enumerable.Range(targetLayer.origin.x, targetLayer.size.x);
                    var yRangeWork = Enumerable.Range(targetLayer.origin.y, targetLayer.size.y);
                    xRange = xRangeWork;
                    yRange = yRangeWork;
                } catch (Exception) {}
#else
                var xRange = Enumerable.Range(0, mapDataModel.width);
                var yRange = Enumerable.Range(-mapDataModel.height + 1, mapDataModel.height);
#endif

                foreach (int y in yRange)
                {
                    foreach (int x in xRange)
                    {
                        var tileDataModel = targetLayer.GetTile<TileDataModel>(new Vector3Int(x, y, 0));
                        if (tileDataModel != null)
                        {
                            if (tileDataModel.passType == TileDataModel.PassType.CanPassUnder)
                            {
                                upperLayerTilemap.SetTile(new Vector3Int(x, y, 0), tileDataModel);
                                targetLayer.SetTile(new Vector3Int(x, y, 0), null);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 同Y座標の場合のキャラクターの描画順値を取得する。
        /// </summary>
        /// <param name="characterOnMap">キャラクター。</param>
        /// <returns>0=パーティ隊列外のキャラクター、1=パーティ隊列の最後尾～。値が大きいほど手前。</returns>
        public static int GetCharactorRenderingOrder(CharacterOnMap characterOnMap)
        {
        	// プレイヤーキャラクターを末尾とした、隊列キャラクターリスト。
            var list = _partyOnMap != null ?
                _partyOnMap.
                    Select(actorOnMap => (CharacterOnMap)actorOnMap).
                    Reverse().
                    ToList() :
                new List<CharacterOnMap>();

            if (_actorOnMap != null)
            {
                list.Add(_actorOnMap);
            }

            return list.IndexOf(characterOnMap) + 1;
        }

        private static void SetActorOnMap(Vector2 partyPosition) {
            _actorOnMap = (new GameObject()).AddComponent<ActorOnMap>();
            _actorOnMap.gameObject.transform.SetParent(_rootGameObject.transform);

            string actorId = null;
            if (DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.actors.Count > 0)
                actorId = DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.actors[0];

            var assetId = "";
            foreach (var runtimeActorDataModel in DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels)
                if (runtimeActorDataModel.actorId == actorId)
                    assetId = runtimeActorDataModel.characterImage;

            _actorOnMap.Init(partyPosition, assetId, actorId);
            _actorOnMap.SetAnimation(true, false);
            //デフォルトの移動速度の保持
            _defaultCharacterSpeed = _actorOnMap.GetCharacterSpeed();

            //カメラをActorに紐づけ
            _camera.transform.SetParent(_actorOnMap.gameObject.transform);
            _camera.transform.localPosition = _cameraPos + new Vector3(0.5f, 0.5f, -100.0f);
            _camera.enabled = true;

            var partyDataModel = DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel;

            //隊列歩行用のActorOnMap生成
            _partyOnMap = new List<ActorOnMap>();
            for (int i = 1; i < 4; i++)
            {
                //ActorOnMap生成
                var actor = (new GameObject()).AddComponent<ActorOnMap>();
                actor.gameObject.transform.SetParent(_rootGameObject.transform);
                if (i < partyDataModel.actors.Count)
                {
                    //パーティメンバーが存在する場合、IDを設定
                    actorId = partyDataModel.actors[i];
                    assetId = "";
                    foreach (var runtimeActorDataModel in DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels)
                        if (runtimeActorDataModel.actorId == actorId)
                            assetId = runtimeActorDataModel.characterImage;
                }
                else
                {
                    //パーティメンバーが不在の場合、nullを設定
                    actorId = null;
                    assetId = _actorOnMap.AssetId;
                }
                //ActorOnMap初期化
                actor.Init(partyPosition, assetId, actorId);
                actor.SetAnimation(true, false);

                //管理用配列に追加
                _partyOnMap.Add(actor);

                //隊列歩行のON/OFF設定を反映
                //パーティメンバーが存在しており、隊列歩行がONの場合に設定
                if (i < partyDataModel.actors.Count && _playerFollow)
                    actor.SetCharacterEnable(true);
                else
                    actor.SetCharacterEnable(false);

                //透明状態の設定
                actor.SetTransparent(actor.GetTransparent());
            }

            //ダッシュ可能かどうかの設定
            SetCanDashForAllPlayerCharacters(!CurrentMapDataModel.forbidDash);
        }

       public static void SortActor() {
            var partyDataModel = DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel;
            var actorId = partyDataModel.actors[0];

            var assetId = "";
            foreach (var runtimeActorDataModel in DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels)
                if (runtimeActorDataModel.actorId == actorId)
                    assetId = runtimeActorDataModel.characterImage;

            _actorOnMap.ResetActor(assetId, actorId);
            _actorOnMap.SetTransparent(_actorOnMap.GetTransparent());

            for (int i = 1; i < partyDataModel.actors.Count; i++)
            {
                actorId = partyDataModel.actors[i];

                assetId = "";
                foreach (var runtimeActorDataModel in DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels)
                    if (runtimeActorDataModel.actorId == actorId)
                        assetId = runtimeActorDataModel.characterImage;

                _partyOnMap[i - 1].ResetActor(assetId, actorId);
                _partyOnMap[i - 1].SetTransparent(_partyOnMap[i - 1].GetTransparent());
            }
        }

        public static void SetVehicleOnMap(Vector2 partyPosition) {
            if (_vehiclesOnMap != null)
            {
                for (int i = 0; i < _vehiclesOnMap.Count; i++)
                {
                    //乗り物に乗って移動していた場合、移動時に乗っていた乗り物だけは削除しない
                    if (_vehicleOnMap == null || _vehicleOnMap != _vehiclesOnMap[i])
                        TforuUtility.Destroy(_vehiclesOnMap[i].gameObject);
                }
            }

            var saveData = DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel;
            if (_vehicleOnMap != null)
            {
                for (int i = 0; i < saveData.map.vehicles.Count; i++)
                {
                    if (saveData.map.vehicles[i].id == _vehicleOnMap.CharacterId)
                    {
                        //データ的にもマップを移動
                        saveData.map.vehicles[i].mapId = CurrentMapDataModel.id;
                    }
                }
            }

            if (saveData.map.vehicles.Count != 0)
            {
                _vehiclesOnMap = new List<VehicleOnMap>();
                if (_vehicleOnMap != null)
                    _vehiclesOnMap.Add(_vehicleOnMap);

                for (int i = 0; i < saveData.map.vehicles.Count; i++)
                {
                    var vehicle = saveData.map.vehicles[i];
                    if (CurrentMapDataModel.id != vehicle.mapId)
                    {
                        continue;
                    }
                    if (_vehicleOnMap != null && _vehicleOnMap.CharacterId == vehicle.id)
                    {
                        continue;
                    }

                    var vehicleId = vehicle.id;
                    var assetId = vehicle.assetId;

                    var vehicleOnMap = new GameObject().AddComponent<VehicleOnMap>();
                    vehicleOnMap.gameObject.transform.SetParent(_rootGameObject.transform);
                    vehicleOnMap.Init(new Vector2(vehicle.x, vehicle.y), assetId, vehicleId);
                    vehicleOnMap.SetVehicleSpeed(vehicle.speed);

                    //影の追加
                    var shadowObject = new GameObject().AddComponent<SpriteRenderer>();
                    var shadow = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(shadowImagePath);
                    _shadowSpriteRenderer = shadowObject.GetComponent<SpriteRenderer>();
                    _shadowSpriteRenderer.sprite = shadow;
                    _shadowSpriteRenderer.color = new Color(0, 0, 0, 0);
                    vehicleOnMap.SetVehicleShadow(_shadowSpriteRenderer);
                    shadowObject.gameObject.transform.SetParent(vehicleOnMap.transform);
                    shadowObject.transform.position = new Vector3
                    (
                        0.5f,
                        0.5f,
                        0
                    );

                    _vehiclesOnMap.Add(vehicleOnMap);

                    //追加した乗り物に搭乗していた場合
                    if (vehicle.ride == 1)
                    {
                        //搭乗中の乗り物設定
                        _vehicleOnMap = vehicleOnMap;
                        //真上からの乗り込み
                        _isRightAbove = true;
                        //乗り込み中
                        _isRiding = true;
                        //ここで乗り込む処理の実行
                        PartyMemberAllInCoordinate(ReasonForPartyMemberAllIn.Vehicle);
                    }
                }
            }
            else
            {
                if (_vehicleOnMap != null)
                {
                    _vehiclesOnMap = new List<VehicleOnMap>();
                    _vehiclesOnMap.Add(_vehicleOnMap);
                }
            }
        
            SetVehicleRide();
        }

        public static void SetVehicleOnMap(Vector2 pos, Vhicle vehicle) {
            bool flg = false;
            for (int i = 0; i < _vehiclesOnMap.Count; i++)
            {
                if (vehicle.id == _vehiclesOnMap[i].CharacterId)
                {
                    flg = true;
                    if (vehicle.mapId != CurrentMapDataModel.id)
                    {
                        //マップ内からマップ外に移動した場合には、削除する
                        TforuUtility.Destroy(_vehiclesOnMap[i].gameObject);
                        _vehiclesOnMap.RemoveAt(i);
                    }
                    else
                    {
                        //同一マップ内で移動した場合
                        //ループを考慮した座標に変換する
                        pos = MapManager.LoopInstance.MovePointLoopEvent(pos);
                        _vehiclesOnMap[i].SetToPositionOnTile(pos);

                        if (_vehicleOnMap.CharacterId == _vehiclesOnMap[i].CharacterId)
                        {
                            //現在、この乗り物に搭乗中の場合は、キャラクターも移動する
                            _actorOnMap.SetToPositionOnTile(pos);
                            //パーティメンバーも強制的に同じ座標に移動
                            if (_partyOnMap != null)
                            {
                                for (int j = 0; j < _partyOnMap.Count; j++)
                                    _partyOnMap[j].SetToPositionOnTile(pos);
                            }
                        }
                    }
                    break;
                }
            }
            //別のマップから、現在のマップに移動してきた場合
            if (!flg && vehicle.mapId == CurrentMapDataModel.id)
            {
                //ループを考慮した座標に変換する
                pos = MapManager.LoopInstance.MovePointLoopEvent(pos);

                var vehicleId = vehicle.id;
                var assetId = DataManager.Self().GetVehicleDataModel(vehicleId).images;
                var vehicleOnMap = new GameObject().AddComponent<VehicleOnMap>();
                vehicleOnMap.gameObject.transform.SetParent(_rootGameObject.transform);
                vehicleOnMap.Init(pos, assetId, vehicleId);

                //影の追加
                var shadowObject = new GameObject().AddComponent<SpriteRenderer>();
                var shadow = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(shadowImagePath);
                _shadowSpriteRenderer = shadowObject.GetComponent<SpriteRenderer>();
                _shadowSpriteRenderer.sprite = shadow;
                _shadowSpriteRenderer.color = new Color(0, 0, 0, 0);
                vehicleOnMap.SetVehicleShadow(_shadowSpriteRenderer);
                shadowObject.gameObject.transform.SetParent(vehicleOnMap.transform);
                shadowObject.transform.position = new Vector3
                (
                    0.5f,
                    0.5f,
                    0
                );

                _vehiclesOnMap.Add(vehicleOnMap);
            }
        }

        private static void SetVehicleRide()
        {
            _vehicleOnMap?.SetRide(_moveType == _moveTypeEnum.Vehicle);
        }

        public static void AddPartyOnMap(string actorID) {
            if (GameStateHandler.IsBattle())
            {
                if (!_partyWaitAdd.Contains(actorID))
                    _partyWaitAdd.Add(actorID);
                return;
            }

            int index = -1;
            for (int i = 0; i < _partyOnMap.Count; i++)
            {
                if (_partyOnMap[i].CharacterId == null)
                {
                    index = i;
                    break;
                }
            }
            var saveData = DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel;
            var assetId = "";
            foreach (var runtimeActorDataModel in DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels)
                if (runtimeActorDataModel.actorId == actorID)
                    assetId = runtimeActorDataModel.characterImage;
            var name = DataManager.Self().GetActorDataModel(actorID).name;

            ActorOnMap actor = _actorOnMap;
            if (_actorOnMap.CharacterId != null)
            {
                actor = _partyOnMap[index];
            }
            actor.gameObject.name = name;
            actor.ResetActor(assetId, actorID);
            // 透明状態の反映
            actor.SetTransparent(saveData.map.transparent == 1);

            //隊列歩行のON/OFF設定を反映
            _partyOnMap?.ForEach(v => v.SetCharacterEnable(_playerFollow));

            //ダッシュ可能かどうかの設定
            SetCanDashForAllPlayerCharacters(!CurrentMapDataModel.forbidDash);
        }

        public static void SubPartyOnMap(string actorID) {
            if (GameStateHandler.IsBattle())
            {
                if (_partyWaitAdd.Contains(actorID))
                {
                    //バトル中に追加したキャラクターで、かつマップにまだ戻ってきていない場合は、
                    //マップに戻ってきた時に追加予定だったリストから、該当のキャラクターを除外して終了
                    _partyWaitAdd.Remove(actorID);
                    return;
                }
            }

            var runtimeSaveData = DataManager.Self().GetRuntimeSaveDataModel();

            int index = runtimeSaveData.runtimePartyDataModel.actors.IndexOf(actorID);
            if (index >= 0)
                runtimeSaveData.runtimePartyDataModel.actors.RemoveAt(index);

            RuntimeActorDataModel runtimeActorDataModel = null;
            for (int i = 0; i < runtimeSaveData.runtimeActorDataModels.Count; i++)
                if (runtimeSaveData.runtimeActorDataModels[i].actorId == actorID)
                {
                    runtimeActorDataModel = runtimeSaveData.runtimeActorDataModels[i];
                    break;
                }

            if (_actorOnMap.CharacterId == actorID)
            {
                if (_partyOnMap.Count > 0)
                {
                    //隊列歩行の先頭キャラクターを、操作キャラクターに変更する
                    _actorOnMap.ResetActor(_partyOnMap[0].AssetId, _partyOnMap[0].CharacterId);
                    _actorOnMap.SetTransparent(_actorOnMap.GetTransparent());

                    for (int i = 0; i < _partyOnMap.Count - 1; i++)
                    {
                        _partyOnMap[i].ResetActor(_partyOnMap[i + 1].AssetId, _partyOnMap[i + 1].CharacterId);
                        _partyOnMap[i].SetTransparent(_partyOnMap[i + 1].GetTransparent());
                    }
                    _partyOnMap[2].ResetActor(_actorOnMap.AssetId, null);
                    _partyOnMap[2].SetTransparent(_partyOnMap[2].GetTransparent());
                }
            }
            else
            {
                var data = _partyOnMap.FirstOrDefault(c => c.CharacterId == actorID);
                if (data != null)
                {
                    //対象のキャラクターを初期化して、配列の一番後ろに移動する
                    index = _partyOnMap.IndexOf(data);
                    for (int i = index; i < _partyOnMap.Count - 1; i++)
                    {
                        _partyOnMap[i].ResetActor(_partyOnMap[i + 1].AssetId, _partyOnMap[i + 1].CharacterId);
                        _partyOnMap[i].SetTransparent(_partyOnMap[i + 1].GetTransparent());
                    }
                    _partyOnMap[2].ResetActor(_actorOnMap.AssetId, null);
                    _partyOnMap[2].SetTransparent(_partyOnMap[2].GetTransparent());
                }
            }
        }

        /// <summary>
        /// 隊列歩行を行うかどうかを設定する
        /// </summary>
        /// <param name="playerFollow">trueで隊列歩行を行う。falseで自機のみが歩行を行う</param>
        public static void ChangePlayerFollowers(bool playerFollow) {
            _playerFollow = playerFollow;

            if (_playerFollow)
                DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.follow = 1;
            else
                DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.follow = 0;

            // 乗り物に乗っていなければ即座に反映する
            if (_vehicleOnMap == null)
            {
                _partyOnMap?.ForEach(v => v.SetCharacterEnable(_playerFollow));
            }
        }

        /// <summary>
        /// 特定のイベント更新
        /// </summary>
        public static void UpdateEventWatch() {
            if (IsActiveMap() == false) return;

            if (DataManager.Self().IsGameOverCheck)
            {
                CheckGameOver();
            }

            //まだプレイヤーが配置されていないようなケースでは処理しない
            if (_actorOnMap == null) return;
            //状態を更新する
            MapEventExecutionController.Instance.UpdateGameState();
            //イベント実行
            MapEventExecutionController.Instance.UpdateEventWatch(_actorOnMap.GetCurrentPositionOnTile());

            //ダッシュ判定
            ChangeDashCharacter(InputHandler.GetInputKey(KeyCode.LeftShift, InputType.Press));
            //パーティのUpdate処理
            UpdateActorOnMap();
        }

        /// <summary>
        /// プレイヤー及び隊列歩行キャラクターの更新
        /// </summary>
        private static void UpdateActorOnMap() {
            _nextWalkFlg = false;
            _actorOnMap.ExecUpdateTimeHandler();
            if (_partyOnMap != null && !_nextWalkFlg)
            {
                for (int i = 0; i < _partyOnMap.Count && !_nextWalkFlg; i++)
                    _partyOnMap[i].ExecUpdateTimeHandler();
            }
        }

        /// <summary>
        /// 乗り物から降りる
        /// </summary>
        /// <param name="allDirection"></param>
        public static void TryToGetOffFromTheVehicleToPlayer(bool allDirection = false) {
            if (_actorOnMap != null && _actorOnMap.IsMoving()) return;
            if (_isRiding) return;

            //降りれるかの判定を行う
            //まずは現在の向きで判断する
            //VehicleGetCurrentDirection
            var targetPositionOnTile = _vehicleOnMap.GetCurrentDirection() switch
            {
                CharacterMoveDirectionEnum.Left => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.left,
                CharacterMoveDirectionEnum.Right => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.right,
                CharacterMoveDirectionEnum.Up => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.up,
                CharacterMoveDirectionEnum.Down => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.down,
                _ => _vehicleOnMap.GetCurrentPositionOnTile()
            };

            CurrentTileData(targetPositionOnTile);

            //降りられるか判定
            if (_tilesOnThePosition.CanEnterThisTiles(_vehicleOnMap.GetCurrentDirection()))
            {
                //地形的には降りられるため、その場所にプレイヤーと同じ高さのイベントが存在するかどうかの確認
                if (MapEventExecutionController.Instance.GetEventPoint(targetPositionOnTile) == null)
                {
                    //ここで降りる処理の実行
                    GetOffVehicle(_vehicleOnMap.GetCurrentDirection());
                    //「TilesOnThePosition」を乗り物へ更新
                    //タイミング的に、降りた直後に来る可能性もあるため、nullチェックを実施
                    if (_vehicleOnMap != null)
                        CurrentTileData(_vehicleOnMap.GetCurrentPositionOnTile());

                    return;
                }
            }

            //上記で降りられなかった場合、全周囲で、降りられるかどうかの判定が必要な場合は、チェックする
            if (allDirection)
            {
                foreach (CharacterMoveDirectionEnum Direction in Enum.GetValues(typeof(CharacterMoveDirectionEnum)))
                {
                    targetPositionOnTile = Direction switch
                    {
                        CharacterMoveDirectionEnum.Left => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.left,
                        CharacterMoveDirectionEnum.Right => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.right,
                        CharacterMoveDirectionEnum.Up => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.up,
                        CharacterMoveDirectionEnum.Down => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.down,
                        _ => _vehicleOnMap.GetCurrentPositionOnTile()
                    };

                    CurrentTileData(targetPositionOnTile);

                    //降りられるか判定
                    if (_tilesOnThePosition.CanEnterThisTiles(Direction))
                    {
                        //地形的には降りられるため、その場所にプレイヤーと同じ高さのイベントが存在するかどうかの確認
                        if (MapEventExecutionController.Instance.GetEventPoint(targetPositionOnTile) == null)
                        {
                            //ここで降りる処理の実行
                            GetOffVehicle(Direction);
                            break;
                        }
                    }
                }

                //「TilesOnThePosition」を乗り物へ更新
                if (_vehicleOnMap != null)
                    CurrentTileData(_vehicleOnMap.GetCurrentPositionOnTile());
            }
        }

        /// <summary>
        /// 乗り物に乗る
        /// </summary>
        public static async void TryToRideFromThePlayerToVehicle() {
            if (_actorOnMap != null && _actorOnMap.IsMoving()) return;
            if (_isRiding) return;

            var targetPositionOnTile = _actorOnMap.GetCurrentDirection() switch
            {
                CharacterMoveDirectionEnum.Left => _actorOnMap.GetCurrentPositionOnTile() + Vector2.left,
                CharacterMoveDirectionEnum.Right => _actorOnMap.GetCurrentPositionOnTile() + Vector2.right,
                CharacterMoveDirectionEnum.Up => _actorOnMap.GetCurrentPositionOnTile() + Vector2.up,
                CharacterMoveDirectionEnum.Down => _actorOnMap.GetCurrentPositionOnTile() + Vector2.down,
                _ => throw new ArgumentOutOfRangeException()
            };

            //今のMAPにあるの

            //真上にいる時
            for (int i = 0; i < _vehiclesOnMap.Count; i++)
                if (_vehiclesOnMap[i].GetCurrentPositionOnTile() == _actorOnMap.GetCurrentPositionOnTile())
                {
                    _vehicleOnMap = _vehiclesOnMap[i];
                    _isRightAbove = true;
                }

            //隣にいる時
            if (_vehicleOnMap == null)
                for (int i = 0; i < _vehiclesOnMap.Count; i++)
                    if (_vehiclesOnMap[i].GetCurrentPositionOnTile() == targetPositionOnTile)
                    {
                        _vehicleOnMap = _vehiclesOnMap[i];
                        _isRightAbove = false;
                    }

            //あったら実行
            if (_vehicleOnMap != null)
            {
                //乗り込み中
                _isRiding = true;
                //ここで乗り込む処理の実行
                PartyMemberAllInCoordinate(ReasonForPartyMemberAllIn.Vehicle);

                // BGM再生
                //セーブデータにBGMが保存されている場合には、セーブデータのBGMの方を優先する
                var runtimeSystemConfig = DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig;
                if (runtimeSystemConfig.vehicleSound == null)
                    runtimeSystemConfig.vehicleSound = new List<VehicleSound>();

                bool flg = false;
                for (int i = 0; i < runtimeSystemConfig.vehicleSound.Count; i++)
                {
                    if (runtimeSystemConfig.vehicleSound[i].id == _vehicleOnMap.CharacterId)
                    {
                        SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_BGM,
                            new SoundCommonDataModel(
                            runtimeSystemConfig.vehicleSound[i].sound.name,
                            runtimeSystemConfig.vehicleSound[i].sound.pan,
                            runtimeSystemConfig.vehicleSound[i].sound.pitch,
                            runtimeSystemConfig.vehicleSound[i].sound.volume
                        ));
                        await SoundManager.Self().PlayBgm();
                        flg = true;
                        break;
                    }
                }
                // 保存されていないならマスタデータに登録されているBGMを再生する
                if (!flg)
                {
                    var bgm = DataManager.Self().GetVehicleDataModel(_vehicleOnMap.CharacterId)?.bgm;
                    if (bgm != null)
                    {
                        SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_BGM,
                            new SoundCommonDataModel(
                            bgm.name,
                            bgm.pan,
                            bgm.pitch,
                            bgm.volume
                        ));
                        await SoundManager.Self().PlayBgm();
                    }
                    else
                    {
                        SoundManager.Self().StopBgm();
                    }
                }

                var runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
                //アクター座標
                runtimeSaveDataModel.runtimePlayerDataModel.map.x = (int) _actorOnMap.x_next;
                runtimeSaveDataModel.runtimePlayerDataModel.map.y = (int) _actorOnMap.y_next;
            }
        }

        /// <summary>
        /// 向きからVector2を取得
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private static Vector2 DirectionToVector2(CharacterMoveDirectionEnum direction) {
            return direction switch
            {
                CharacterMoveDirectionEnum.Left => Vector2.left,
                CharacterMoveDirectionEnum.Right => Vector2.right,
                CharacterMoveDirectionEnum.Up => Vector2.up,
                CharacterMoveDirectionEnum.Down => Vector2.down,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <summary>
        /// 向きからVector2Intを取得
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private static Vector2Int DirectionToVector2Int(CharacterMoveDirectionEnum direction) {
            return direction switch
            {
                CharacterMoveDirectionEnum.Left => Vector2Int.left,
                CharacterMoveDirectionEnum.Right => Vector2Int.right,
                CharacterMoveDirectionEnum.Up => Vector2Int.up,
                CharacterMoveDirectionEnum.Down => Vector2Int.down,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <summary>
        /// ループマップでのタイル位置を取得
        /// </summary>
        /// <param name="positionOnTile"></param>
        /// <returns></returns>
        private static Vector2Int PositionOnTileToPositionOnLoopMapTile(Vector2Int positionOnTile) {
            int x = positionOnTile.x;
            if (x < 0) x = (x % CurrentMapDataModel.width + CurrentMapDataModel.width) % CurrentMapDataModel.width;
            else x = x % CurrentMapDataModel.width;

            int y = positionOnTile.y;
            if (y > 0) y = (y % CurrentMapDataModel.height == 0 ? 0 : y % CurrentMapDataModel.height - CurrentMapDataModel.height);
            else y = y % CurrentMapDataModel.height;

            return new Vector2Int(x, y);
        }

        /// <summary>
        /// 目の前にイベントがあり、その発動契機がTalkであればイベントを開始する
        /// </summary>
        private static void DesiceKeyEvent() {
            if (GameStateHandler.CurrentGameState() != GameStateHandler.GameState.MAP) return;
            if (MapEventExecutionController.Instance.CheckRunningEvent() == true) return;
            if (_isRiding) return;

#if USE_CHARACTER_MOVE_AS
            CancelMove();
#endif

            //歩行キャラの向きから、チェックするイベントを確認
            var targetPositionOnTile =
                _actorOnMap.GetCurrentPositionOnTile() + DirectionToVector2(_actorOnMap.GetCurrentDirection());
            var counterOverTargetPositionOnTile = _actorOnMap.GetCurrentDirection() switch
            {
                CharacterMoveDirectionEnum.Left => _actorOnMap.GetCurrentPositionOnTile() + Vector2.left * 2,
                CharacterMoveDirectionEnum.Right => _actorOnMap.GetCurrentPositionOnTile() + Vector2.right * 2,
                CharacterMoveDirectionEnum.Up => _actorOnMap.GetCurrentPositionOnTile() + Vector2.up * 2,
                CharacterMoveDirectionEnum.Down => _actorOnMap.GetCurrentPositionOnTile() + Vector2.down * 2,
                _ => throw new ArgumentOutOfRangeException()
            };

            //乗り物に乗っている場合は、座標を乗り物に挿げ替え
            if (_moveType == _moveTypeEnum.Vehicle)
            {
                targetPositionOnTile =
                    _vehicleOnMap.GetCurrentPositionOnTile() + DirectionToVector2(_vehicleOnMap.GetCurrentDirection());
                counterOverTargetPositionOnTile = _vehicleOnMap.GetCurrentDirection() switch
                {
                    CharacterMoveDirectionEnum.Left => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.left * 2,
                    CharacterMoveDirectionEnum.Right => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.right * 2,
                    CharacterMoveDirectionEnum.Up => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.up * 2,
                    CharacterMoveDirectionEnum.Down => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.down * 2,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            //アクションキーでイベントを実行する
            MapEventExecutionController.Instance.TryToTalkToEvent(targetPositionOnTile, counterOverTargetPositionOnTile, _tilesOnThePosition);

            //上記を実行して、イベントが実行できなかった時
            if (GameStateHandler.CurrentGameState() == GameStateHandler.GameState.MAP)
            {
                //同じ座標で重なっているアクションイベントがあれば、実行する
                TryToTalkToEventSamePoint();
            }

            //上記を実行して、イベントが実行できなかった時
            if (GameStateHandler.CurrentGameState() == GameStateHandler.GameState.MAP)
            {
                //この場合は乗り物の乗り降りを試行する
                if (_moveType == _moveTypeEnum.Vehicle)
                {
                    TryToGetOffFromTheVehicleToPlayer(true);
                }
                else
                {
                    TryToRideFromThePlayerToVehicle();
                }
            }
        }

        private static void TryToTalkToEventSamePoint() {
            if (GameStateHandler.CurrentGameState() != GameStateHandler.GameState.MAP) return;

            //移動中は処理しない
            if (_moveType == _moveTypeEnum.Actor && _actorOnMap.IsMoving()) return;
            if (_moveType == _moveTypeEnum.Vehicle && _vehicleOnMap.IsMoving()) return;

            if (_moveType == _moveTypeEnum.Actor)
            {
                //同一の座標での判断
                var targetPositionOnTile = _actorOnMap.GetCurrentPositionOnTile();
                //アクションキーでイベントを実行する
                MapEventExecutionController.Instance.TryToTalkToEventSamePoint(targetPositionOnTile, _tilesOnThePosition);
            }
            else
            {
                //同一の座標での判断
                var targetPositionOnTile = _vehicleOnMap.GetCurrentPositionOnTile();
                //アクションキーでイベントを実行する
                MapEventExecutionController.Instance.TryToTalkToEventSamePoint(targetPositionOnTile, _tilesOnThePosition);
            }

            //上記を実行して、イベントが実行できなかった時
            if (GameStateHandler.CurrentGameState() == GameStateHandler.GameState.MAP)
            {
                //この場合は乗り物の乗り降りを試行する
                if (_moveType == _moveTypeEnum.Vehicle)
                {
                    TryToGetOffFromTheVehicleToPlayer(true);
                }
                else
                {
                    TryToRideFromThePlayerToVehicle();
                }
            }
        }

        /// <summary>
        /// イベントがあり、その発動契機がContactFromThePlayerであればイベントを開始する
        /// </summary>
        /// <param name="checkForward"></param>
        private static void TryToContactFromThePlayerToEvent(bool checkForward = false) {
            if (GameStateHandler.CurrentGameState() != GameStateHandler.GameState.MAP) return;

            //移動中は処理しない
            if (_moveType == _moveTypeEnum.Actor && _actorOnMap.IsMoving()) return;
            if (_moveType == _moveTypeEnum.Vehicle && _vehicleOnMap.IsMoving()) return;

            var targetPositionOnTile = _actorOnMap.GetCurrentPositionOnTile();
            var targetPositionOnTile2 = _actorOnMap.GetCurrentDirection() switch
            {
                CharacterMoveDirectionEnum.Left => _actorOnMap.GetCurrentPositionOnTile() + Vector2.left,
                CharacterMoveDirectionEnum.Right => _actorOnMap.GetCurrentPositionOnTile() + Vector2.right,
                CharacterMoveDirectionEnum.Up => _actorOnMap.GetCurrentPositionOnTile() + Vector2.up,
                CharacterMoveDirectionEnum.Down => _actorOnMap.GetCurrentPositionOnTile() + Vector2.down,
                _ => throw new ArgumentOutOfRangeException()
            };

            //乗り物に乗っている場合は、座標を乗り物に挿げ替え
            if (_moveType == _moveTypeEnum.Vehicle)
            {
                targetPositionOnTile = _vehicleOnMap.GetCurrentPositionOnTile();
                targetPositionOnTile2 = _vehicleOnMap.GetCurrentDirection() switch
                {
                    CharacterMoveDirectionEnum.Left => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.left,
                    CharacterMoveDirectionEnum.Right => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.right,
                    CharacterMoveDirectionEnum.Up => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.up,
                    CharacterMoveDirectionEnum.Down => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.down,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            //接触でイベントを実行する
            MapEventExecutionController.Instance.TryToContactFromThePlayerToEvent(targetPositionOnTile, targetPositionOnTile2, _tilesOnThePosition, checkForward);
        }

        // Actionへの代入時に内容が確認しやすいように、各向き分メソッドを用意
        private static void TryToMoveLeftVehicle() {
#if USE_CHARACTER_MOVE_AS
            CancelMove();
#endif
            TryToMoveVehicle(CharacterMoveDirectionEnum.Left);
        }
        private static void TryToMoveRightVehicle() {
#if USE_CHARACTER_MOVE_AS
            CancelMove();
#endif
            TryToMoveVehicle(CharacterMoveDirectionEnum.Right);
        }
        private static void TryToMoveUpVehicle() {
#if USE_CHARACTER_MOVE_AS
            CancelMove();
#endif
            TryToMoveVehicle(CharacterMoveDirectionEnum.Up);
        }
        private static void TryToMoveDownVehicle() {
#if USE_CHARACTER_MOVE_AS
            CancelMove();
#endif
            TryToMoveVehicle(CharacterMoveDirectionEnum.Down);
        }
        
        /// <summary>
        /// 乗り物の移動を試行する
        /// （対象位置が進入不可の場合は向きだけ変える）
        /// </summary>
        /// <param name="directionEnum"></param>
        /// <param name="moveRoute"></param>
        private static void TryToMoveVehicle(CharacterMoveDirectionEnum directionEnum, bool moveRoute = false) {
            if (GameStateHandler.CurrentGameState() != GameStateHandler.GameState.MAP) return;
            if (IsActiveMap() == false) return;
            if (_actorOnMap == null) return;
            if (_vehicleOnMap == null) return;
            if (MapEventExecutionController.Instance.CheckRunningEvent() == true) return;

            //移動ルートでの移動ではなかった場合、かつ移動ルートが残っている場合
            //キーボードでの差し込みを優先するため、移動ルートを破棄する
            if (!moveRoute)
            {
                //現在移動ルートを保持している場合は、移動ルートを破棄
                _vehicleOnMap.ResetMoveRoute();
                _actorOnMap.ResetMoveRoute();

                //移動中だった場合、設定を元に戻す
                _vehicleOnMap.ResetSetting();
                _actorOnMap.ResetSetting();
                //現在移動中かどうかの取得
                for (int i = 0; i < _partyOnMap?.Count; i++)
                {
                    //移動中だった場合、設定を元に戻す
                    _partyOnMap[i].ResetSetting();
                }
            }

            //現在移動中かどうかの取得
            bool movingflg = _vehicleOnMap.IsMoving() || _actorOnMap.IsMoving();
            if (_playerFollow)
            {
                for (int i = 0; i < _partyOnMap?.Count; i++)
                {
                    var nextRouteParty = _partyOnMap[i].IsMoving();
                    if (nextRouteParty)
                    {
                        movingflg = true;
                        break;
                    }
                }
            }

            var targetPositionOnTile = directionEnum switch
            {
                CharacterMoveDirectionEnum.Left => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.left,
                CharacterMoveDirectionEnum.Right => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.right,
                CharacterMoveDirectionEnum.Up => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.up,
                CharacterMoveDirectionEnum.Down => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.down,
                _ => throw new ArgumentOutOfRangeException()
            };

            CurrentTileData(targetPositionOnTile);

            if (_actorOnMap.GetCharacterThrough() && !IsOutOfMap(targetPositionOnTile))
            {
                // タイルチェック不要。
            }
            else if (!CheckCanMove(directionEnum))
            {
                //進入できない場合は向きだけ変える
                _vehicleOnMap.ChangeCharacterDirection(directionEnum);

                //イベント発動するかどうかチェック
                //接触イベント
                TryToContactFromThePlayerToEvent(true);

#if USE_CHARACTER_MOVE_AS
                //移動のケースで、移動先が隣接タイルの場合には、話しかけた際のイベント実行も行う
                if (GameStateHandler.CurrentGameState() == GameStateHandler.GameState.MAP)
                {
                    if (moveRoute && _vehicleOnMap.MoveRouteCount() == 1)
                    {
                        DesiceKeyEvent();
                    }
                }
#endif
                return;
            }

            //イベントとのプライオリティチェック
            var targetPosOnTile = new Vector2Int(_actorOnMap.x_now, _actorOnMap.y_now) + DirectionToVector2Int(directionEnum);
            if (_actorOnMap.GetCharacterThrough())
            {
                // すり抜けONの場合。
                foreach (var _event in MapEventExecutionController.Instance.EventsOnMap)
                {
                    if (_event.IsPriorityNormal() && _event.isValid)
                    {
                        if (_event.IsMoveCheck(targetPosOnTile.x, targetPosOnTile.y))
                        {
                            //進入できない場合は向きだけ変える
                            _vehicleOnMap.ChangeCharacterDirection(directionEnum);

                            //イベント発動チェック
                            if (_vehicleOnMap.x_now == _vehicleOnMap.x_next && _vehicleOnMap.y_now == _vehicleOnMap.y_next)
                            {
                                TryToContactFromThePlayerToEvent(true);
                            }

                            if (GameStateHandler.CurrentGameState() != GameStateHandler.GameState.MAP
                            || MapEventExecutionController.Instance.CheckRunningEvent() == true)
                            {
                                //イベントが開始したなら、移動はストップ。
                                return;
                            }
                        }
                    }
                }
            } 
            else
            {
                foreach (var _event in MapEventExecutionController.Instance.EventsOnMap)
                {
                    if (_event.IsPriorityNormal() && _event.isValid)
                    {
                        if (_event.GetTrough() == false &&
                            _event.IsMoveCheck(targetPosOnTile.x, targetPosOnTile.y))
                        {
                            //進入できない場合は向きだけ変える
                            _vehicleOnMap.ChangeCharacterDirection(directionEnum);

                            //イベント発動チェック
                            if (_vehicleOnMap.x_now == _vehicleOnMap.x_next && _vehicleOnMap.y_now == _vehicleOnMap.y_next)
                            {
                                TryToContactFromThePlayerToEvent(true);
                            }

#if USE_CHARACTER_MOVE_AS
                            //移動のケースで、移動先が隣接タイルの場合には、話しかけた際のイベント実行も行う
                            if (moveRoute && _vehicleOnMap.MoveRouteCount() == 1)
                            {
                                DesiceKeyEvent();
                            }
#endif
                            return;
                        }
                    }
                }
            }

#if USE_TRY_TO_MOVE_CHARACTER_DIRECTION_ENUM
            _tryToMoveCharacterDirectionEnum = directionEnum;
#endif

            //現在移動中の場合は処理しない
            if (movingflg)
            {
                return;
            }

            switch (directionEnum)
            {
                case CharacterMoveDirectionEnum.Up:
                    _vehicleOnMap.MoveUpOneUnit(CheckProcess);
                    _actorOnMap.MoveUpOneUnit();
                    _partyOnMap?.ForEach(v => v.MoveUpOneUnit());
                    break;

                case CharacterMoveDirectionEnum.Down:
                    _vehicleOnMap.MoveDownOneUnit(CheckProcess);
                    _actorOnMap.MoveDownOneUnit();
                    _partyOnMap?.ForEach(v => v.MoveDownOneUnit());
                    break;

                case CharacterMoveDirectionEnum.Left:
                    _vehicleOnMap.MoveLeftOneUnit(CheckProcess);
                    _actorOnMap.MoveLeftOneUnit();
                    _partyOnMap?.ForEach(v => v.MoveLeftOneUnit());
                    break;

                case CharacterMoveDirectionEnum.Right:
                    _vehicleOnMap.MoveRightOneUnit(CheckProcess);
                    _actorOnMap.MoveRightOneUnit();
                    _partyOnMap?.ForEach(v => v.MoveRightOneUnit());
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(directionEnum), directionEnum, null);
            }

            //マップ情報更新
            var runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
            runtimeSaveDataModel.runtimePlayerDataModel.map.mapId = CurrentMapDataModel.id;

            //アクター座標
            runtimeSaveDataModel.runtimePlayerDataModel.map.x = (int) _actorOnMap.x_next;
            runtimeSaveDataModel.runtimePlayerDataModel.map.y = (int) _actorOnMap.y_next;

            //現在のっている乗り物の保存
            RuntimePlayerDataModel.Vhicle vehicle = null;
            for (int i = 0; i < runtimeSaveDataModel.runtimePlayerDataModel.map.vehicles.Count; i++)
                if (runtimeSaveDataModel.runtimePlayerDataModel.map.vehicles[i].id == _vehicleOnMap.CharacterId)
                {
                    vehicle = runtimeSaveDataModel.runtimePlayerDataModel.map.vehicles[i];
                    break;
                }
            vehicle.x = (int) _actorOnMap.x_next;
            vehicle.y = (int) _actorOnMap.y_next;
        }

        // Actionへの代入時に内容が確認しやすいように、各向き分メソッドを用意。
        private static void TryToMoveLeftCharacter() {
#if USE_CHARACTER_MOVE_AS
            CancelMove();
#endif
            TryToMoveCharacter(CharacterMoveDirectionEnum.Left);
        }
        private static void TryToMoveRightCharacter() {
#if USE_CHARACTER_MOVE_AS
            CancelMove();
#endif
            TryToMoveCharacter(CharacterMoveDirectionEnum.Right);
        }
        private static void TryToMoveUpCharacter() {
#if USE_CHARACTER_MOVE_AS
            CancelMove();
#endif
            TryToMoveCharacter(CharacterMoveDirectionEnum.Up);
        }
        private static void TryToMoveDownCharacter() {
#if USE_CHARACTER_MOVE_AS
            CancelMove();
#endif
            TryToMoveCharacter(CharacterMoveDirectionEnum.Down);
        }

        public static bool IsCharacterMove() {
            //移動中は処理しない
            if (_moveType == _moveTypeEnum.Actor && _actorOnMap.IsMoving()) return true;
            if (_moveType == _moveTypeEnum.Vehicle && _vehicleOnMap.IsMoving()) return true;
            return false;
        }

        /// <summary>
        /// キャラクターの移動を試行する
        /// （対象位置が進入不可の場合は向きだけ変える）
        /// </summary>
        /// <param name="directionEnum"></param>
        /// <param name="moveRoute"></param>
        private static void TryToMoveCharacter(CharacterMoveDirectionEnum directionEnum, bool moveRoute = false) {
            if (GameStateHandler.CurrentGameState() != GameStateHandler.GameState.MAP) return;
            if (_isRiding) return;

            if (IsActiveMap() == false) return;
            if (_actorOnMap == null) return;
            if (MapEventExecutionController.Instance.CheckRunningEvent() == true) return;

            //移動ルートでの移動ではなかった場合、かつ移動ルートが残っている場合
            //キーボードでの差し込みを優先するため、移動ルートを破棄する
            if (!moveRoute)
            {
                //現在移動ルートを保持している場合は、移動ルートを破棄
                _actorOnMap.ResetMoveRoute();

                //移動中だった場合、設定を元に戻す
                _actorOnMap.ResetSetting();
                //現在移動中かどうかの取得
                for (int i = 0; i < _partyOnMap?.Count; i++)
                {
                    //移動中だった場合、設定を元に戻す
                    _partyOnMap[i].ResetSetting();
                }
            }

            //現在移動中かどうかの取得
            bool movingflg = _actorOnMap.IsMoving();
            if (_playerFollow)
            {
                for (int i = 0; i < _partyOnMap?.Count; i++)
                {
                    var nextRouteParty = _partyOnMap[i].IsMoving();
                    if (nextRouteParty)
                    {
                        movingflg = true;
                        break;
                    }
                }
            }

            var targetPositionOnTile = directionEnum switch
            {
                CharacterMoveDirectionEnum.Left => _actorOnMap.GetCurrentPositionOnTile() + Vector2.left,
                CharacterMoveDirectionEnum.Right => _actorOnMap.GetCurrentPositionOnTile() + Vector2.right,
                CharacterMoveDirectionEnum.Up => _actorOnMap.GetCurrentPositionOnTile() + Vector2.up,
                CharacterMoveDirectionEnum.Down => _actorOnMap.GetCurrentPositionOnTile() + Vector2.down,
                _ => throw new ArgumentOutOfRangeException()
            };

            CurrentTileData(targetPositionOnTile);

            if (_actorOnMap.GetCharacterThrough() && !IsOutOfMap(targetPositionOnTile))
            {
                //タイルチェック不要。
            } 
            else if (!CheckCanMove(directionEnum))
            {
                //進入できない場合は向きだけ変える
                if (!movingflg)
                    _actorOnMap.ChangeCharacterDirection(directionEnum);

                //イベント発動するかどうかチェック
                //接触イベント
                TryToContactFromThePlayerToEvent(true);

                //移動のケースで、移動先が最終の移動場所の場合には、話しかけた際のイベント実行も行う
                if (moveRoute && _actorOnMap.MoveRouteCount() == 1)
                {
                    DesiceKeyEvent();
                }

                //現在移動ルートを保持している場合は、移動ルートを破棄
                _actorOnMap.ResetMoveRoute();

                return;
            }

            //イベントとのプライオリティチェック
            var targetPosOnTile = new Vector2Int(_actorOnMap.x_now, _actorOnMap.y_now) + DirectionToVector2Int(directionEnum);
            if (_actorOnMap.GetCharacterThrough())
            {
                // すり抜けONの場合。
                foreach (var _event in MapEventExecutionController.Instance.EventsOnMap)
                {
                    if (_event.IsPriorityNormal() && _event.isValid)
                    {
                        if (_event.IsMoveCheck(targetPosOnTile.x, targetPosOnTile.y))
                        {
                            //接触イベント
                            //現在移動していない場合にのみ行う
                            if (_actorOnMap.x_now == _actorOnMap.x_next && _actorOnMap.y_now == _actorOnMap.y_next)
                            {
                                TryToContactFromThePlayerToEvent(true);
                            }

                            //移動のケースで、移動先が最終の移動場所の場合には、話しかけた際のイベント実行も行う
                            if (moveRoute && _actorOnMap.MoveRouteCount() == 1)
                            {
                                DesiceKeyEvent();
                            }
                            if (GameStateHandler.CurrentGameState() != GameStateHandler.GameState.MAP
                            || MapEventExecutionController.Instance.CheckRunningEvent() == true)
                            {
                                //イベントが開始したなら、移動はストップ。
                                return;
                            }
                        }
                    }
                }
            } 
            else
            {
                foreach (var _event in MapEventExecutionController.Instance.EventsOnMap)
                {
                    if (_event.IsPriorityNormal() && _event.isValid)
                    {
                        if (_event.GetTrough() == false &&
                            _event.IsMoveCheck(targetPosOnTile.x, targetPosOnTile.y))
                        {
                            //進入できない場合は向きだけ変える
                            if (!movingflg)
                            {
                                // 梯子タイルは上固定
                                if (_tilesOnThePosition.GetLadderTile() == true)
                                    _actorOnMap.ChangeCharacterDirection(CharacterMoveDirectionEnum.Up);
                                else
                                    _actorOnMap.ChangeCharacterDirection(directionEnum);
                            }

                            //接触イベント
                            //現在移動していない場合にのみ行う
                            if (_actorOnMap.x_now == _actorOnMap.x_next && _actorOnMap.y_now == _actorOnMap.y_next)
                            {
                                TryToContactFromThePlayerToEvent(true);
                            }

                            //移動のケースで、移動先が最終の移動場所の場合には、話しかけた際のイベント実行も行う
                            if (moveRoute && _actorOnMap.MoveRouteCount() == 1)
                            {
                                DesiceKeyEvent();
                            }
                            return;
                        }
                    }
                }
            }

#if USE_CHARACTER_MOVE_AS
            //移動のケースで、移動先に隣接タイルを指定している場合には、乗り物に乗る判定を行う。
            if (moveRoute && _actorOnMap.MoveRouteCount() == 1)
            {
                if (GameStateHandler.CurrentGameState() == GameStateHandler.GameState.MAP)
                {
                    TryToRideFromThePlayerToVehicle();
                    if (_isRiding) return;
                }
            }
#endif

            _actorMoveDirectionEnum = _actorOnMap.GetCurrentDirection();
            _actorPosition = _actorOnMap.GetCurrentPositionOnTile();

#if USE_TRY_TO_MOVE_CHARACTER_DIRECTION_ENUM
            _tryToMoveCharacterDirectionEnum = directionEnum;
#endif
            
            //現在移動中の場合は処理しない
            if (movingflg)
            {
                return;
            }

            // パーティメンバーの移動
            PartyMoveCharacter(false);

            // 先頭キャラクターの移動
            switch (directionEnum)
            {
                case CharacterMoveDirectionEnum.Up:
                    _actorOnMap.MoveUpOneUnit(CheckProcess);
                    break;
                case CharacterMoveDirectionEnum.Down:
                    _actorOnMap.MoveDownOneUnit(CheckProcess);
                    break;
                case CharacterMoveDirectionEnum.Left:
                    _actorOnMap.MoveLeftOneUnit(CheckProcess);
                    break;
                case CharacterMoveDirectionEnum.Right:
                    _actorOnMap.MoveRightOneUnit(CheckProcess);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(directionEnum), directionEnum, null);
            }
            
            //マップ情報更新
            var runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
            runtimeSaveDataModel.runtimePlayerDataModel.map.mapId = CurrentMapDataModel.id;

            if (CurrentMapDataModel.scrollType != MapDataModel.MapScrollType.NoLoop)
            {                
                runtimeSaveDataModel.runtimePlayerDataModel.map.x = PositionOnTileToPositionOnLoopMapTile(new Vector2Int(_actorOnMap.x_next, _actorOnMap.y_next)).x;
                runtimeSaveDataModel.runtimePlayerDataModel.map.y = PositionOnTileToPositionOnLoopMapTile(new Vector2Int(_actorOnMap.x_next, _actorOnMap.y_next)).y;
            }
            else
            {
                runtimeSaveDataModel.runtimePlayerDataModel.map.x = (int) _actorOnMap.x_next;
                runtimeSaveDataModel.runtimePlayerDataModel.map.y = (int) _actorOnMap.y_next;
            }

            //隊列歩行中、かつ乗り物に乗っていない場合にフラグを立てる
            if (_playerFollow && _moveType == _moveTypeEnum.Actor)
            {
                _nextWalkFlg = true;
            }
        }

        /// <summary>
        /// RuntimeConfigDataModelが更新された時に入れ直す
        /// </summary>
        /// <param name="runtimeActorDataModel"></param>
        public static void SetRuntimeConfigDataModel(RuntimeConfigDataModel runtimeActorDataModel) {
            _runtimeConfigDataModel = runtimeActorDataModel;
        }

        /// <summary>
        /// 操作中のアクターを含めたパーティメンバー全員の情報を取得する
        /// </summary>
        /// <returns>アクター情報リスト</returns>
        public static IReadOnlyList<ActorOnMap> GetAllActorOnMap()
        {
            List<ActorOnMap> partyOnMapWork = _partyOnMap;
            if (partyOnMapWork == null) partyOnMapWork = new List<ActorOnMap>();
            var tmpList = new List<ActorOnMap>(partyOnMapWork);
            tmpList.Add(_actorOnMap);
            return tmpList;
        }

        /// <summary>
        /// キャラクターのダッシュ、非ダッシュ切り替え
        /// </summary>
        /// <param name="isShiftDown"></param>
        private static void ChangeDashCharacter(bool isShiftDown) {
            SetDashForAllPlayerCharacters(_runtimeConfigDataModel.alwaysDash == 0 ? isShiftDown : !isShiftDown);
        }

        /// <summary>
        /// ダッシュ中かどうかの設定
        /// </summary>
        /// <param name="isDash"></param>
        private static void SetDashForAllPlayerCharacters(bool isDash)
        {
            _actorOnMap.SetDash(isDash);
            _partyOnMap?.ForEach(v => v.SetDash(isDash));
        }

        /// <summary>
        /// ダッシュ可能かどうかの設定
        /// </summary>
        /// <param name="canDash"></param>
        private static void SetCanDashForAllPlayerCharacters(bool canDash)
        {
            _actorOnMap.CanDash(canDash);
            _partyOnMap?.ForEach(v => v.CanDash(canDash));
        }

        /// <summary>
        /// キャラクターの移動を試行する
        /// （対象位置が進入不可の場合は向きだけ変える）
        /// </summary>
        private static void TryToMoveCharacterClick() {
            DebugUtil.LogMethodParameters();
            using var indentLog = new DebugUtil.IndentLog();

            //移動可否判定
            if (GameStateHandler.CurrentGameState() != GameStateHandler.GameState.MAP) return;
            if (IsActiveMap() == false) return;
            if (MapEventExecutionController.Instance.CheckRunningEvent() == true) return;
            if (MenuManager.IsMenuActive ) return; 
            if ((DateTime.Now - MenuManager.dateTime).TotalMilliseconds < 200) return;
            if (CurrentMapDataModel == null || CurrentMapDataModel.MapPrefabManagerForRuntime == null) return;

            //移動処理
            //1.移動可否の判断は、後続処理で実施

            //2.移動ルート算出
            //2.1.マウス座標取得
            Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            //2.2.タイルの座標に変換
            Vector2 clickPoint = GetTilePositionByWorldPositionForRuntime(new Vector2(mouseWorldPos.x, mouseWorldPos.y));
            //2.3.タイルの座標をintに変換
            //現在の座標をint型で保持しておく
            int click_x = (int) clickPoint.x;
            int click_y = (int) clickPoint.y;

            //3.CharacterOnMap に SetMoveRoute
            //3.1.移動ルート配列の初期化
            List<CharacterMoveDirectionEnum> moveRoute = new List<CharacterMoveDirectionEnum>();
            //3.2.xとyの差を算出（何歩あるくか）
            int move_x = click_x - _actorOnMap.x_now;
            int move_y = click_y - _actorOnMap.y_now;
            if (move_x == 0 && move_y == 0)
            {
                //移動は行われない
                //そのため、重なっているイベントが存在する場合には発動する
                TryToTalkToEventSamePoint();
                return;
            }
#if !USE_CHARACTER_MOVE_AS
            //以下、移動ルート計算
            //計算用の係数
            float coefficient = 0.0f;
            //歩いた歩数を記録する用の一時変数
            int calc_x = 0;
            int calc_y = 0;

            //以下、ログ

            if (Mathf.Abs(move_x) <= Mathf.Abs(move_y))
            {
                //3.3.係数算出
                coefficient = Mathf.Abs(1.0f * move_x / move_y);
                //3.4.ルートを出す
                while (!(calc_x == move_x && calc_y == move_y))
                {
                    //y座標に1歩進む
                    int bef_y = calc_y;
                    if (move_y < 0)
                    {
                        calc_y--;
                        moveRoute.Add(CharacterMoveDirectionEnum.Down);
                    }
                    else
                    {
                        calc_y++;
                        moveRoute.Add(CharacterMoveDirectionEnum.Up);
                    }
                    //次にx座標を動かす必要があるかどうかの確認
                    if (Math.Round(Mathf.Abs(bef_y * coefficient)) < Math.Round(Mathf.Abs(calc_y * coefficient)) || calc_y == move_y)
                    {
                        if (calc_x == move_x)
                        {
                            continue;
                        }
                        //一歩ずらす
                        if (move_x < 0)
                        {
                            calc_x--;
                            moveRoute.Add(CharacterMoveDirectionEnum.Left);
                        }
                        else
                        {
                            calc_x++;
                            moveRoute.Add(CharacterMoveDirectionEnum.Right);
                        }
                        //フェールセーフ
                        if (calc_y == move_y) break;
                    }
                }
            }
            else
            {
                //3.3.係数算出
                coefficient = Mathf.Abs(1.0f * move_y / move_x);
                //3.4.ルートを出す
                while (!(calc_x == move_x && calc_y == move_y))
                {
                    //x座標に1歩進む
                    int bef_x = calc_x;
                    if (move_x < 0)
                    {
                        calc_x--;
                        moveRoute.Add(CharacterMoveDirectionEnum.Left);
                    }
                    else
                    {
                        calc_x++;
                        moveRoute.Add(CharacterMoveDirectionEnum.Right);
                    }
                    //次にx座標を動かす必要があるかどうかの確認
                    if (Math.Round(Mathf.Abs(bef_x * coefficient)) < Math.Round(Mathf.Abs(calc_x * coefficient)) || calc_x == move_x)
                    {
                        if (calc_y == move_y)
                        {
                            continue;
                        }
                        //一歩ずらす
                        if (move_y < 0)
                        {
                            calc_y--;
                            moveRoute.Add(CharacterMoveDirectionEnum.Down);
                        }
                        else
                        {
                            calc_y++;
                            moveRoute.Add(CharacterMoveDirectionEnum.Up);
                        }
                        //フェールセーフ
                        if (calc_x == move_x) break;
                    }
                }
            }
            //3.5.移動ルート算出したため、セットする
            _actorOnMap.SetMoveRoute(moveRoute);

            //4.1マス目のTryToMoveCharacter実行
            TryToMoveCharacter(moveRoute[0], true);
#else
            _actorTarget = true;
            _vehicleId = (_moveType == _moveTypeEnum.Vehicle ? MapManager.GetRideVehicle().CharacterId : null);

            // タイル情報取得用及びハイライト表示用。
            _moveAsTilesOnThePosition = GetTilesOnThePosition();
            if (_moveAsTilesOnThePosition == null) return;
            //クリックしたタイルをハイライトする。（ループを想定して近い位置に設定。）
            _moveAsTilesOnThePosition.gameObject.transform.position = new Vector3(_actorOnMap.x_now + GetVectorX(click_x, _actorOnMap.x_now), _actorOnMap.y_now + GetVectorY(click_y, _actorOnMap.y_now), 0);

            GetCurrentMapInfo();

            //スタート座標設定。
            _startX = _actorOnMap.x_now;
            _startY = _actorOnMap.y_now;

            //ゴール座標設定。
            _goalX = GetMapX(click_x);
            _goalY = GetMapY(click_y);

            MenuManager.MenuActiveEvent += CancelMove;
            //毎フレーム継続処理させる。
            TimeHandler.Instance.AddTimeActionEveryFrame(UpdateMove);   // Call every frame.
                                                                        //初回移動処理呼び出し。
            UpdateMove();
#endif
        }

        /// <summary>
        /// キャラクターの移動を試行する
        /// （対象位置が進入不可の場合は向きだけ変える）
        /// </summary>
        private static void TryToMoveVehicleClick() {
            DebugUtil.LogMethodParameters();
            using var indentLog = new DebugUtil.IndentLog();

            //移動可否判定
            if (GameStateHandler.CurrentGameState() != GameStateHandler.GameState.MAP) return;
            if (IsActiveMap() == false) return;
            if (MapEventExecutionController.Instance.CheckRunningEvent() == true) return;
            if (MenuManager.IsMenuActive) return;
            if ((DateTime.Now - MenuManager.dateTime).TotalMilliseconds < 200) return;

            //移動処理
            //1.移動可否の判断は、後続処理で実施

            //2.移動ルート算出
            //2.1.マウス座標取得
            Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            //2.2.タイルの座標に変換
            Vector2 clickPoint = GetTilePositionByWorldPositionForRuntime(new Vector2(mouseWorldPos.x, mouseWorldPos.y));
            //2.3.タイルの座標をintに変換
            //現在の座標をint型で保持しておく
            int click_x = (int) clickPoint.x;
            int click_y = (int) clickPoint.y;

            //3.CharacterOnMap に SetMoveRoute
            //3.1.移動ルート配列の初期化
            List<CharacterMoveDirectionEnum> moveRoute = new List<CharacterMoveDirectionEnum>();
            //3.2.xとyの差を算出（何歩あるくか）
            int move_x = click_x - _vehicleOnMap.x_now;
            int move_y = click_y - _vehicleOnMap.y_now;
            if (move_x == 0 && move_y == 0)
            {
                //移動は行われない
                //そのため、重なっているイベントが存在する場合には発動する
                TryToTalkToEventSamePoint();
                return;
            }
#if !USE_CHARACTER_MOVE_AS
            //以下、移動ルート計算
            //計算用の係数
            float coefficient = 0.0f;
            //歩いた歩数を記録する用の一時変数
            int calc_x = 0;
            int calc_y = 0;

            //以下、ログ

            if (Mathf.Abs(move_x) <= Mathf.Abs(move_y))
            {
                //3.3.係数算出
                coefficient = Mathf.Abs(1.0f * move_x / move_y);
                //3.4.ルートを出す
                while (!(calc_x == move_x && calc_y == move_y))
                {
                    //y座標に1歩進む
                    int bef_y = calc_y;
                    if (move_y < 0)
                    {
                        calc_y--;
                        moveRoute.Add(CharacterMoveDirectionEnum.Down);
                    }
                    else
                    {
                        calc_y++;
                        moveRoute.Add(CharacterMoveDirectionEnum.Up);
                    }
                    //次にx座標を動かす必要があるかどうかの確認
                    if (Math.Round(Mathf.Abs(bef_y * coefficient)) < Math.Round(Mathf.Abs(calc_y * coefficient)) || calc_y == move_y)
                    {
                        if (calc_x == move_x)
                        {
                            continue;
                        }
                        //一歩ずらす
                        if (move_x < 0)
                        {
                            calc_x--;
                            moveRoute.Add(CharacterMoveDirectionEnum.Left);
                        }
                        else
                        {
                            calc_x++;
                            moveRoute.Add(CharacterMoveDirectionEnum.Right);
                        }
                        //フェールセーフ
                        if (calc_y == move_y) break;
                    }
                }
            }
            else
            {
                //3.3.係数算出
                coefficient = Mathf.Abs(1.0f * move_y / move_x);
                //3.4.ルートを出す
                while (!(calc_x == move_x && calc_y == move_y))
                {
                    //x座標に1歩進む
                    int bef_x = calc_x;
                    if (move_x < 0)
                    {
                        calc_x--;
                        moveRoute.Add(CharacterMoveDirectionEnum.Left);
                    }
                    else
                    {
                        calc_x++;
                        moveRoute.Add(CharacterMoveDirectionEnum.Right);
                    }
                    //次にx座標を動かす必要があるかどうかの確認
                    if (Math.Round(Mathf.Abs(bef_x * coefficient)) < Math.Round(Mathf.Abs(calc_x * coefficient)) || calc_x == move_x)
                    {
                        if (calc_y == move_y)
                        {
                            continue;
                        }
                        //一歩ずらす
                        if (move_y < 0)
                        {
                            calc_y--;
                            moveRoute.Add(CharacterMoveDirectionEnum.Down);
                        }
                        else
                        {
                            calc_y++;
                            moveRoute.Add(CharacterMoveDirectionEnum.Up);
                        }
                        //フェールセーフ
                        if (calc_x == move_x) break;
                    }
                }
            }
            //3.5.移動ルート算出したため、セットする
            _vehicleOnMap.SetMoveRoute(moveRoute);

            //4.1マス目のTryToMoveCharacter実行
            TryToMoveVehicle(moveRoute[0], true);
#else
            _actorTarget = false;
            _vehicleId = (_moveType == _moveTypeEnum.Vehicle ? MapManager.GetRideVehicle().CharacterId : null);

            // タイル情報取得用及びハイライト表示用。
            _moveAsTilesOnThePosition = GetTilesOnThePosition();
            if (_moveAsTilesOnThePosition == null) return;
            //クリックしたタイルをハイライトする。（ループを想定して近い位置に設定。）
            _moveAsTilesOnThePosition.gameObject.transform.position = new Vector3(_vehicleOnMap.x_now + GetVectorX(click_x, _vehicleOnMap.x_now), _vehicleOnMap.y_now + GetVectorY(click_y, _vehicleOnMap.y_now), 0);

            GetCurrentMapInfo();

            //スタート座標設定。
            _startX = _vehicleOnMap.x_now;
            _startY = _vehicleOnMap.y_now;

            //ゴール座標設定。
            _goalX = GetMapX(click_x);
            _goalY = GetMapY(click_y);

            MenuManager.MenuActiveEvent += CancelMove;
            //毎フレーム継続処理させる。
            TimeHandler.Instance.AddTimeActionEveryFrame(UpdateMove);   // Call every frame.
                                                                        //初回移動処理呼び出し。
            UpdateMove();
#endif
        }

        /// <summary>
        /// パーティメンバーの移動
        /// </summary>
        public static void PartyMoveCharacter(bool loop = true) {
            if (_partyOnMap == null || _partyOnMap.Count == 0)
            {
                return;
            }

            List<ActorOnMap> work = new List<ActorOnMap>();
            work.Add(_actorOnMap);
            for (int i = 0; i < _partyOnMap.Count; i++)
            {
                work.Add(_partyOnMap[i]);
            }

            if (loop)
            {
                //全員の移動が終わるまでは処理しない
                for (int i = 0; i < _partyOnMap.Count; i++)
                {
                    if (_partyOnMap[i].IsMoving()) return;
                }
            }

            bool isMoved = false;
            for (int i = work.Count - 1; i > 0; i--)
            {
                //1つ前のキャラクターの最終移動方向
                var direction = work[i - 1].GetLastMoveDirection();

                //あり得ないが、向きが取れなければcontinue
                if (direction == CharacterMoveDirectionEnum.Max)
                    continue;

                //同一座標に居た場合にはcontinue
                if (work[i].GetCurrentPositionOnTile() == work[i - 1].GetCurrentPositionOnTile())
                    continue;

                //1つ前のキャラクターのpositionと、最終の移動方向に移動
                if (!loop)
                {
                    work[i].MoveToPositionOnTile(direction, work[i - 1].GetCurrentPositionOnTile());
                }
                else
                {
                    work[i].MoveToPositionOnTile(direction, work[i - 1].GetCurrentPositionOnTile(), () => { PartyMoveCharacter(); });
                }

                isMoved = true;
            }

            if (!isMoved && loop)
            {
                //全員集合済み
                AllMemberAssembly();
            }
        }

        /// <summary>
        /// パーティメンバーの移動 乗り物搭乗時
        /// </summary>
        public static void PartyMoveCharacterVehicle() {
            //乗り物の最終移動方向
            var direction = _vehicleOnMap.GetLastMoveDirection();

            //乗り物の移動先と、向きに移動
            _actorOnMap.MoveToPositionOnTile(direction, _vehicleOnMap.GetDestinationPositionOnTile());

            //パーティメンバーの移動
            PartyMoveCharacter(false);
        }

        /// <summary>
        /// ワールド座標をタイル座標に変換
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public static Vector2 GetTilePositionByWorldPositionForRuntime(Vector2 worldPosition) {
            var mapPrefabManager = CurrentMapDataModel.MapPrefabManagerForRuntime;
            try
            {
                var tilePosition = mapPrefabManager.layers[1].tilemap.WorldToCell(worldPosition);
                return new Vector2(tilePosition.x, tilePosition.y);
            }
            catch (Exception)
            {
                try
                {
                    var tilePosition = mapPrefabManager.LoadPrefab().transform.GetComponent<Grid>().WorldToCell(worldPosition);
                    return new Vector2(tilePosition.x, tilePosition.y);
                }
                catch (Exception) {}
            }
            return new Vector2(0, 0);
        }

        /// <summary>
        /// タイル座標をワールド座標に変換
        /// </summary>
        /// <param name="tilePosition"></param>
        /// <returns></returns>
        public static Vector2 GetWorldPositionByTilePositionForRuntime(Vector2 tilePosition) {
            var tilePosition3D = new Vector3Int((int) tilePosition.x, (int) tilePosition.y, 0);
            // Aレイヤーを参照する
            var worldPosition = CurrentMapDataModel.MapPrefabManagerForRuntime.layers[(int) MapDataModel.Layer.LayerType.A]
                .tilemap.CellToWorld(tilePosition3D);

            return new Vector2(worldPosition.x, worldPosition.y);
        }

        /// <summary>
        /// 操作中のキャラクター（アクター or 乗り物）のGameObject
        /// </summary>
        /// <returns></returns>
        public static GameObject GetOperatingCharacterGameObject() {
            if (_moveType == _moveTypeEnum.Vehicle)
            {
                return _vehicleOnMap.gameObject;
            }
            return _actorOnMap.gameObject;
        }
        
        /// <summary>
        /// 操作中のアクターを取得
        /// </summary>
        /// <returns></returns>
        public static GameObject GetCharacterGameObject() {
            return _actorOnMap.gameObject;
        }

        public static int GetPartyMemberNum() {
            if (_partyOnMap != null)
                return _partyOnMap.Count;
            else
                return 0;
        }
        public static GameObject GetPartyGameObject(int index) {
            if (_partyOnMap != null)
                return _partyOnMap[index].gameObject == null ? null : _partyOnMap[index].gameObject;
            else
                return null;
        }

        public static GameObject GetVehicleGameObject(string VehicleId) {
            foreach (var data in _vehiclesOnMap)
            {
                if (data.CharacterId == VehicleId)
                    return data.gameObject;
            }

            return null;
        }

        static bool IsOutOfMap(Vector2 positionOnTile) {
            var mapWidth = CurrentMapDataModel.width;
            var mapHeight = CurrentMapDataModel.height;
            var scrollType = CurrentMapDataModel.scrollType;
            var mapVerticalLoop = (scrollType == MapDataModel.MapScrollType.LoopVertical || scrollType == MapDataModel.MapScrollType.LoopBoth);
            var mapHorizontalLoop = (scrollType == MapDataModel.MapScrollType.LoopHorizontal || scrollType == MapDataModel.MapScrollType.LoopBoth);
            if (!mapHorizontalLoop && (positionOnTile.x < 0 || positionOnTile.x >= mapWidth)) return true;
            if (!mapVerticalLoop && (positionOnTile.y > 0 || positionOnTile.y <= -mapHeight)) return true;
            return false;
        }

        public static bool CheckCanMove(CharacterMoveDirectionEnum directionEnum) {
            return
            	 _tilesOnThePosition.CanEnterThisTiles(
                	directionEnum,
                	_moveType == _moveTypeEnum.Vehicle ? _vehicleOnMap.CharacterId: null);
        }

        public static Vector3 ActorCameraWorldScreen() {
            Vector3 pos = _camera.WorldToScreenPoint(new Vector3(_actorOnMap.transform.position.x, _actorOnMap.transform.position.y));
            return pos;
        }

        public static Vector3 EventCameraWorldScreen(Vector2 eventPos) {
            Vector3 pos = _camera.WorldToScreenPoint(new Vector3(eventPos.x, eventPos.y));
            return pos;
        }

        public static Camera GetCamera() {
            return _camera;
        }

        public static Vector3 GetCameraPosition() {
            return _cameraPos;
        }

        public static void SetCameraPosition(Vector3 pos) {
            _cameraPos = pos;
            _camera.transform.localPosition = _cameraPos + new Vector3(0.5f, 0.5f, -100.0f);
        }

        /// <summary>
        /// どのレイヤーのTilemapを取得するか、イベント285で仕様
        /// </summary>
        /// <returns></returns>
        public static Tilemap GetTileMapForRuntime(MapDataModel.Layer.LayerType layerType) {
            return CurrentMapDataModel.MapPrefabManagerForRuntime.layers[(int)layerType].tilemap;
        }

        private static void ActiveMap(bool isShow) {
            _rootGameObject.SetActive(isShow);
        }
        private static void ActiveMenu(bool isShow) {
            _menuGameObject.SetActive(isShow);
            if (_partyWaitAdd.Count > 0)
            {
                for (int i = 0; i < _partyWaitAdd.Count; i++)
                {
                    AddPartyOnMap(_partyWaitAdd[i]);
                }
                _partyWaitAdd.Clear();
            }
            if (_isEventBattle)
            {
                GameStateHandler.SetGameState(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT);
                _isEventBattle = false;
                NextEvent();
            }
            else if (isShow)
            {
                //マップに戻ってきたため、イベント側にも通知
                MapEventExecutionController.Instance.ResumeEvent();
            }

        }

        /// <summary>
        /// バトルからマップへ遷移
        /// </summary>
        public static void BattleToMap() {
            //バトル側でフェードしているため、マップ側に戻ったときに黒くする
            HudDistributor.Instance.StaticHudHandler().DisplayInit();
            HudDistributor.Instance.StaticHudHandler().FadeOut(UnloadBattle, Color.black, 0f);
            async void UnloadBattle() {
                //マップを表示
                ActiveMap(true);

                //バトルをUnload
                SceneManager.UnloadSceneAsync("Battle");
                
                //すこし待つ
                await Task.Delay(500);
                
                //フェードあける
                HudDistributor.Instance.StaticHudHandler().FadeIn(() =>
                {
                    ActiveMenu(true);
                }, true, 0f);
            }
        }

        public static async void NextEvent() {
            await Task.Delay(500);
            //次のイベントに進める
            MapEventExecutionController.Instance.ResumeEvent();
        }

        public static bool IsActiveMap() {
            if (_rootGameObject == null)
            {
                return false;
            }

            return _rootGameObject.activeSelf;
        }

        // private methods
        //--------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 移動後の各判定処理
        /// </summary>
        private static void CheckProcess() {
            //移動した数をインクリメント
            DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.steps++;

#if USE_TRY_TO_MOVE_CHARACTER_DIRECTION_ENUM
            var direction = _tryToMoveCharacterDirectionEnum;
#else
            var direction = _actorMoveDirectionEnum;
#endif

            //マップのループ処理
            if (_moveType == _moveTypeEnum.Actor)
            {
                _mapLoop.MapLoopDirection(_actorOnMap, direction);
            }
            else
            {
                _mapLoop.MapLoopDirection(_vehicleOnMap, direction);
            }

            //ダメージ床の上を歩いているか
            var tileDataModel = _actorOnMap.GetComponent<CharacterOnMap>().GetDamageTileData();
            if (tileDataModel != null)
            {
                var actors = DataManager.Self().GetGameParty();
                switch (tileDataModel.damageFloorType)
                {
                    //最大HPに対する割合率
                    case TileDataModel.DamageFloorType.Rate:
                        for (int i = 0; i < actors.Actors.Count; i++)
                        {
                            var value = (int )(actors.Actors[i].Mhp * (tileDataModel.damageFloorValue / 100f) * actors.Actors[i].Fdr);
                            actors.Actors[i].ExecuteFloorDamage(value);
                        }
                        break;
                    //定数
                    case TileDataModel.DamageFloorType.Fix:
                        for (int i = 0; i < actors.Actors.Count; i++)
                        {
                            var value = tileDataModel.damageFloorValue * actors.Actors[i].Fdr;
                            actors.Actors[i].ExecuteFloorDamage((int)value);
                        }
                        break;
                }

                //パーティメンバー全員がHP1、床ダメージ率が0％だったら、フラッシュはしない。
                int count = 0;
                for (int i = 0; i < actors.Actors.Count; i++)
                {
                    if (actors.Actors[i].Hp == 1 ||  (int)actors.Actors[i].Fdr == 0)
                    {
                        count++;
                    }
                }
                
                if (count != actors.Actors.Count)
                {
                    HudDistributor.Instance.NowHudHandler().DisplayInit();
                    HudDistributor.Instance.NowHudHandler().Flash(() => { }, new Color(255, 0, 0), 160, 15, false, "Damage_Flash");
                }

                //GameOverのチェック
                count = 0;
                for (int i = 0; i < actors.Actors.Count; i++)
                {
                    if (actors.Actors[i].Hp == 0)
                    {
                        count++;
                    }
                }
                if (count == actors.Actors.Count)
                {
                    //GAMEOVER表示
                    _movingGameover = true;
                    ShowGameOver();
                    return;
                }
            }

            // 接触イベント確認
            // このタイミングでは、プレイヤーが該当のイベントと重なっている場合のみ処理
            TryToContactFromThePlayerToEvent();

            // 接触イベント確認後、自動実行イベント、並列実行イベントの発動条件を満たしていた場合には即処理する
            MapEventExecutionController.Instance.TryToAutoEventMoveEnd(_actorOnMap.GetCurrentPositionOnTile());

            // 状態確認
            CheckState();

            // イベントが発動しなかった場合
            if (MapEventExecutionController.Instance.CheckRunningEvent() == false)
            {
                // エンカウントのチェック
                CheckEncount();
                
                //移動継続するかどうかの判定
                if (IsActiveMap() == true)
                {
                    //移動のケースで、ここが最終の移動場所の場合には、話しかけた際のイベント実行も行う
                    //このケースでは、キャラクターと重なっているイベントのみを対象とする
                    if (_actorOnMap.MoveRouteCount() == 1)
                    {
                        TryToTalkToEventSamePoint();
                    }

                    //エンカウントしておらず、イベント実行も行っておらず、移動完了前にメニューを開こうとしていた場合は、メニューを開く
                    if (menu.MenuWillOpen)
                    {
                        menu.MenuOpen();
                    }

                    //メニューが開かれなかった場合
                    if (!MenuManager.IsMenuActive)
                    {
                        //移動ルート設定がある場合には、次の移動を自動的に行う
                        CharacterMoveDirectionEnum nextRoute = _actorOnMap.GetNextRoute();
                        if (nextRoute != CharacterMoveDirectionEnum.Max)
                        {
                            //await Task.Delay(10);
                            TryToMoveCharacter(nextRoute, true);
                        }
                        if (_vehicleOnMap != null)
                        {
                            nextRoute = _vehicleOnMap.GetNextRoute();
                            if (nextRoute != CharacterMoveDirectionEnum.Max)
                            {
                                //await Task.Delay(10);
                                TryToMoveVehicle(nextRoute, true);
                            }
                        }

                        //移動ルート設定に従った移動が行われておらず、
                        //移動完了時点でキー入力が行われている場合、次の移動を試みる
                        if (_moveType == _moveTypeEnum.Actor)
                        {
                            if (InputHandler.OnPress(Common.Enum.HandleType.Left))
                                TryToMoveCharacter(CharacterMoveDirectionEnum.Left);
                            else if (InputHandler.OnPress(Common.Enum.HandleType.Right))
                                TryToMoveCharacter(CharacterMoveDirectionEnum.Right);
                            else if (InputHandler.OnPress(Common.Enum.HandleType.Up))
                                TryToMoveCharacter(CharacterMoveDirectionEnum.Up);
                            else if (InputHandler.OnPress(Common.Enum.HandleType.Down))
                                TryToMoveCharacter(CharacterMoveDirectionEnum.Down);
                        }
                        else
                        {
                            if (InputHandler.OnPress(Common.Enum.HandleType.Left))
                                TryToMoveVehicle(CharacterMoveDirectionEnum.Left);
                            else if (InputHandler.OnPress(Common.Enum.HandleType.Right))
                                TryToMoveVehicle(CharacterMoveDirectionEnum.Right);
                            else if (InputHandler.OnPress(Common.Enum.HandleType.Up))
                                TryToMoveVehicle(CharacterMoveDirectionEnum.Up);
                            else if (InputHandler.OnPress(Common.Enum.HandleType.Down))
                                TryToMoveVehicle(CharacterMoveDirectionEnum.Down);
                        }
                    }
                }
            }

            //歩き終わった場合には、メニューを開こうとしていたフラグを落とす
            menu.MenuWillOpen = false;
        }

        // エンカウント判定用
        private static bool _skipEncount = false;
        private static void CheckEncount() {
            _encountManager.UpdateCount();
            if (_encountManager.IsEncount())
            {
                _encountManager.MakeCount();
                var encounterDataModel = _encountManager.GetEncounterDataModel(false);
                if (encounterDataModel == null)
                {
                    return;
                }

                //敵も敵グループも存在しない場合は、エンカウントしない
                if (encounterDataModel.enemyList.Count <= 0 && encounterDataModel.troopList.Count <= 0)
                {
                    //出現するべき敵がいないため、処理終了
                    return;
                }

                //エンカウント禁止になっているときはエンカウントしない
                var party = DataManager.Self().GetGameParty();
                if (party.HasEncounterNone()) return;
                if (party.HasEncounterHalf()) 
                {
                    _skipEncount = !_skipEncount;
                    if (_skipEncount) return;
                }

                //システム設定でエンカウント無しが指定されている場合はエンカウントしない
                if (DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.encounterEnabled == 0)
                {
                    return;
                }

                //バトル開始
                StartBattle(encounterDataModel);
            }
        }
        
        public static void StartBattle(EncounterDataModel encounterDataModel, string troopId = null, bool canEscape = true, bool canLose = false) {
            //今のオブジェクト全削除
            HudDistributor.Instance.AllDestroyNowHudHandler();

            var eventBack1 = BattleSceneTransition.Instance.EventMapBackgroundImage1;
            var eventBack2 = BattleSceneTransition.Instance.EventMapBackgroundImage2;
            
            //バトル開始
            BattleSceneTransition.Initialize();
            
            BattleSceneTransition.Instance.EventMapBackgroundImage1 = eventBack1;
            BattleSceneTransition.Instance.EventMapBackgroundImage2 = eventBack2;


            //エンカウンター情報を設定
            BattleSceneTransition.Instance.EncounterDataModel = encounterDataModel;
            
            //どの敵 or 敵グループが出るかどうかの抽選処理
            int encountType = 0;
            if (troopId == null)
            {
                //0: 敵
                //1: 敵グループとする
                if (encounterDataModel.enemyList.Count == 0)
                {
                    //敵未設定のため、敵グループ確定
                    encountType = 1;
                }
                else if (encounterDataModel.troopList.Count == 0)
                {
                    //敵グループ未設定のため、敵確定
                    encountType = 0;
                }
                else
                {
                    //乱数で決める
                    //100%のうち、何パーセントが敵グループかの設定値を持っているため、その数値から判断する
                    //本来はデータベースで正しい値が設定されるべきだが、万が一範囲外だった場合は丸め込む
                    int encountTypeWork = encounterDataModel.troopPer;
                    if (encountTypeWork < 0)
                    {
                        encountTypeWork = 0;
                    }
                    else if (encountTypeWork > 100)
                    {
                        encountTypeWork = 100;
                    }
                    int encountTypeRand = UnityEngine.Random.Range(0, 101);
                    if (encountTypeRand < encountTypeWork)
                    {
                        //敵グループ
                        encountType = 1;
                    }
                    else
                    {
                        //自動マッチング
                        encountType = 0;
                    }
                }
            }
            else
            {
                encountType = 1;
            }
            

            //敵グループの場合で、敵グループIDに指定がある場合
            if (encountType == 1 && troopId != "EVENTDATA" && !string.IsNullOrEmpty(troopId))
            {
                BattleSceneTransition.Instance.SelectTroopId = troopId;
            }
            //敵グループの場合の処理
            else if (encountType == 1)
            {
                //敵グループごとに設定されている重みに従って処理する
                //重みを全部足す
                int weight = 0;
                for (int i = 0; i < encounterDataModel.troopList.Count; i++) {
                    weight += encounterDataModel.troopList[i].weight;
                }
                //乱数
                int selectWeight = UnityEngine.Random.Range(1, weight + 1);
                //出現する敵を決定
                bool flg = false;
                weight = 0;
                for (int i = 0; i < encounterDataModel.troopList.Count; i++)
                {
                    weight += encounterDataModel.troopList[i].weight;
                    if (weight > selectWeight)
                    {
                        BattleSceneTransition.Instance.SelectTroopId = encounterDataModel.troopList[i].troopId;
                        flg = true;
                        break;
                    }
                }
                //上記に不備があり、決まらないことが万が一あったら先頭のものにする
                if (!flg)
                {
                    BattleSceneTransition.Instance.SelectTroopId = encounterDataModel.troopList[0].troopId;
                }
            }
            //敵（自動マッチング）の場合
            else
            {
                //自動マッチング専用のtroopIdを設定する
                BattleSceneTransition.Instance.SelectTroopId = TroopDataModel.TROOP_AUTOMATCHING;

                //現在既に、そのデータが存在する場合には、そのデータを上書きするため、検索を行う
                TroopDataModel troop = DataManager.Self().GetTroopDataModels().FirstOrDefault(t => t.id == TroopDataModel.TROOP_AUTOMATCHING);
                
                if (troop == null)
                {
                    troop = _databaseManagementService.CreateEnemyToTroopDataModel(TroopDataModel.TROOP_AUTOMATCHING,
                        DataManager.Self().GetSystemDataModel().battleScene.viewType, encounterDataModel);
                    DataManager.Self().GetTroopDataModels().Add(troop);
                }
                else
                {
                    troop = _databaseManagementService.CreateEnemyToTroopDataModel(TroopDataModel.TROOP_AUTOMATCHING,
                        DataManager.Self().GetSystemDataModel().battleScene.viewType, encounterDataModel);
                    DataManager.Self().GetTroopDataModels()[^1] = troop;
                }
                
            }

            //バトル背景設定
            if (encounterDataModel.backImage1 != "")
            {
                BattleSceneTransition.Instance.EncounterDataBackgroundImage1 = encounterDataModel.backImage1;
            }
            else
            {
                //画像が設定されていなければ、固定の画像名。
                BattleSceneTransition.Instance.EncounterDataBackgroundImage1 = "";
            }

            if (encounterDataModel.backImage2 != "")
            {
                BattleSceneTransition.Instance.EncounterDataBackgroundImage2 = encounterDataModel.backImage2;
            }
            else
            {
                //画像が設定されていなければ、固定の画像名。
                BattleSceneTransition.Instance.EncounterDataBackgroundImage2 = "";
            }

            if (!BattleManager.IsBattle)
            {
                BattleManager.IsBattle = true;

                BattleSceneTransition.Instance.CanLose = canLose;
                BattleSceneTransition.Instance.CanEscape = canEscape;

                //Application.LoadLevelAdditive("Battle");
                SceneManager.LoadScene("Battle", LoadSceneMode.Additive);
            }

            //マップを非アクティブにする
            ActiveMap(false);
            ActiveMenu(false);

            //引数がある場合、イベントバトル
            if (troopId != null)
            {
                //イベント実行中の場合、イベント側への通知は必要無い
                //マップ内でフラグのみ立てる
                _isEventBattle = true;
            }
            else
            {
                //バトルに遷移するので、イベント側にも通知
                MapEventExecutionController.Instance.PauseEvent();
            }
        }

        /// <summary>
        /// 状態異常処理
        /// </summary>
        private static void CheckState() {
            // アクターデータ取得
            var actorDataModels = DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels;
            // ステートデータ取得
            var stateDataModels = DataManager.Self().GetStateDataModels();

            // アクター分繰り返し
            for (int i = 0; i < actorDataModels.Count; i++)
            {
                // 現在のステート分繰り返し
                for (int i2 = 0; i2 < actorDataModels[i].states.Count; i2++)
                {
                    // ステート検索
                    for (int i3 = 0; i3 < stateDataModels.Count; i3++)
                    {
                        // ステートが一致
                        if (actorDataModels[i].states[i2].id == stateDataModels[i3].id)
                        {
                            var statesCount = actorDataModels[i].states.Count;
                            // ステートの処理
                            var actor = StateBegin(actorDataModels[i], i2, stateDataModels[i3]);
                            actorDataModels[i] = actor;
                            //ステートの数が減っていたら
                            if(statesCount != actorDataModels[i].states.Count) 
                                i2--;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ステートの処理（肥大化しそうなため分割）
        /// 引数は対象のアクター、配列番号、ステート
        /// </summary>
        /// <param name="actorData"></param>
        /// <param name="num"></param>
        /// <param name="stateData"></param>
        /// <returns></returns>
        private static RuntimeActorDataModel StateBegin(
            RuntimeActorDataModel actorData,
            int num,
            CoreSystem.Knowledge.DataModel.State.StateDataModel stateData
        ) {
            // 歩数を加算
            actorData.states[num].walkingCount++;

            // バトル中のみなら
            if (stateData.stateOn == 0) return actorData;

            // ダメージ処理
            if (stateData.stepGeneration == 1)
            {
                if (stateData.occurrenceFrequencyStep <= 1 || actorData.states[num].walkingCount % stateData.occurrenceFrequencyStep == 0)
                {
                    for (int i = 0; i < stateData.traits.Count; i++)
                    {
                        switch (stateData.traits[i].categoryId)
                        {
                            case (int) TraitsEnums.TraitsCategory.RESISTANCE:
                                break;
                            case (int) TraitsEnums.TraitsCategory.ABILITY_SCORE:
                                // 能力値関連のみ処理
                                if (stateData.traits[i].traitsId == (int) TraitsAbilityScore.NORMAL_ABILITY_SCORE)
                                {
                                    // HP
                                    if (stateData.traits[i].effectId == 0)
                                    {
                                        actorData.hp += stateData.traits[i].value;
                                    }
                                    // MP
                                    else if (stateData.traits[i].effectId == 1)
                                    {
                                        actorData.mp += stateData.traits[i].value;
                                    }
                                    // 攻撃力
                                    else if (stateData.traits[i].effectId == 2)
                                    {
                                        actorData.paramPlus.attack += stateData.traits[i].value;
                                    }
                                    // 守備力
                                    else if (stateData.traits[i].effectId == 3)
                                    {
                                        actorData.paramPlus.defense += stateData.traits[i].value;
                                    }
                                    // 魔力
                                    else if (stateData.traits[i].effectId == 4)
                                    {
                                        actorData.paramPlus.magicAttack += stateData.traits[i].value;
                                    }
                                    // 魔防
                                    else if (stateData.traits[i].effectId == 5)
                                    {
                                        actorData.paramPlus.magicDefence += stateData.traits[i].value;
                                    }
                                    // 敏捷
                                    else if (stateData.traits[i].effectId == 6)
                                    {
                                        actorData.paramPlus.speed += stateData.traits[i].value;
                                    }
                                    // 運
                                    else if (stateData.traits[i].effectId == 7)
                                    {
                                        actorData.paramPlus.luck += stateData.traits[i].value;
                                    }

                                    // 減算の場合はフラッシュエフェクト再生
                                    if (stateData.traits[i].value < 0)
                                    {
                                        // 画面フラッシュの処理
                                        EventCommandChainLauncher launcher = new EventCommandChainLauncher();
                                        EventDataModel.EventCommand evCommand
                                            = new EventDataModel.EventCommand(224,
                                                new List<string>() { "255", "0", "0", "255", "255", "0" },
                                                new List<EventDataModel.EventCommandMoveRoute>());
                                        EventDataModel ev = new EventDataModel("", 0, 0,
                                            new List<EventDataModel.EventCommand>() { evCommand });
                                        launcher.LaunchCommandChain(new EventMapDataModel(), ev);
                                    }
                                }

                                break;
                            case (int) TraitsEnums.TraitsCategory.ATTACK:
                                break;
                            case (int) TraitsEnums.TraitsCategory.SKILL:
                                break;
                            case (int) TraitsEnums.TraitsCategory.EQUIPMENT:
                                break;
                            case (int) TraitsEnums.TraitsCategory.OTHER:
                                break;
                        }
                    }
                }
            }

            // ステート回復判定
            if (stateData.removeByWalking == 1)
            {
                if (stateData.stepsToRemove <= actorData.states[num].walkingCount)
                {
                    GameActor gameActor = null;
                    for (int i = 0; i < DataManager.Self().GetGameParty().Actors.Count; i++)
                    {
                        if (actorData.actorId == DataManager.Self().GetGameParty().Actors[i].Actor.actorId)
                        {
                            gameActor = DataManager.Self().GetGameParty().Actors[i];
                            break;
                        }
                    }

                    var stateId = actorData.states[num].id;
                    actorData.states.RemoveAt(num);
                    gameActor?.RemoveState(stateId);
                }
            }

            return actorData;
        }

        //パーティメンバーが一か所に集合
        private static Vector2                   _targetPosition;
        private static ReasonForPartyMemberAllIn _reason;

        public static async void PartyMemberAllInCoordinate(ReasonForPartyMemberAllIn reason,[CanBeNull] Action  callback = null) {
            //現在移動途中であった場合には、移動するまで待つ
            bool isMoving = false;
            CharacterOnMap characterOnMap = OperatingCharacter;
            if (characterOnMap.IsMoving())
            {
                isMoving = true;
            }
            List<ActorOnMap> operatingParty = OperatingParty;
            if (operatingParty != null)
            {
                for (int i = 0; i < operatingParty.Count; i++)
                {
                    if (operatingParty[i].IsMoving())
                    {
                        isMoving = true;
                    }
                }
            }
            if (isMoving)
            {
                await Task.Delay(1000/60);
                PartyMemberAllInCoordinate(reason, callback);
                return;
            }

            //今の操作キャラの場所と向き
            _actorPosition = _actorOnMap.GetCurrentPositionOnTile();
            
            //集合箇所の指定
            _reason = reason;
            switch (reason)
            {
                case ReasonForPartyMemberAllIn.Event:
                    _targetPosition = _actorOnMap.GetCurrentPositionOnTile();
                    break;
                case ReasonForPartyMemberAllIn.Vehicle:
                    _targetPosition = _vehicleOnMap.GetCurrentPositionOnTile();
                    //乗り物に乗り込む処理(同じ座標から乗り込んだ際は移動しない)
                    if(!_isRightAbove)
                    {
                        switch (_actorOnMap.GetCurrentDirection())
                        {
                            case CharacterMoveDirectionEnum.Up:
                                _actorOnMap.MoveUpOneUnit();
                                break;
                            case CharacterMoveDirectionEnum.Down:
                                _actorOnMap.MoveDownOneUnit();
                                break;
                            case CharacterMoveDirectionEnum.Left:
                                _actorOnMap.MoveLeftOneUnit();
                                break;
                            case CharacterMoveDirectionEnum.Right:
                                _actorOnMap.MoveRightOneUnit();
                                break;
                        }
                    }
                    CurrentVehicleId = _vehicleOnMap.CharacterId;
                    break;
            }
            
            //2人目のパーティーメンバーの存在確認
            if (_partyOnMap == null)
            {
                //いなければ全員集合済み
                AllMemberAssembly();
            }
            else
            {
                //いれば次へ
                PartyMoveCharacter();
            }
        }

        public static void SetTargetPosition(Vector3 pos) {
            _targetPosition = pos;

            //セーブデータ側へも反映する
            //ここに来るタイミングでは、既に移動済みである
            if (CurrentMapDataModel.scrollType != MapDataModel.MapScrollType.NoLoop)
            {
                DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.x = PositionOnTileToPositionOnLoopMapTile(new Vector2Int(_actorOnMap.x_now, _actorOnMap.y_now)).x;
                DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.y = PositionOnTileToPositionOnLoopMapTile(new Vector2Int(_actorOnMap.x_now, _actorOnMap.y_now)).y;
            }
            else
            {
                DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.x = (int) _actorOnMap.x_now;
                DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.y = (int) _actorOnMap.y_now;
            }
        }


        //全員集したにで操作切り替え
        private static void AllMemberAssembly() {
            switch (_reason)
            {
                case ReasonForPartyMemberAllIn.Event:
                    break;
                case ReasonForPartyMemberAllIn.Vehicle:
                    //乗り物に乗り込む
                    //操作対象の切り替え
                    switch (_moveType)
                    {
                        case _moveTypeEnum.Actor:
                            //乗り物に切り替え
                            //乗ったのが飛行船かどうか
                            ChangeMoveSubject(_moveTypeEnum.Vehicle);
                            break;
                        case _moveTypeEnum.Vehicle:
                            //アクターに切り替え
                            ChangeMoveSubject(_moveTypeEnum.Actor);
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// 乗り物降りるメソッド
        /// 降りれる方向が入ってくる
        /// </summary>
        /// <param name="Direction"></param>
        private static async void GetOffVehicle(CharacterMoveDirectionEnum Direction) {
            //アクター表示
            _actorOnMap.SetCharacterEnable(true);
            if(_playerFollow)
                _partyOnMap?.ForEach(v => v.SetCharacterEnable(true));

            //アクター降りる
            switch (Direction)
            {
                case CharacterMoveDirectionEnum.Up:
                    _actorOnMap.MoveUpOneUnit();
                    break;
                case CharacterMoveDirectionEnum.Down:
                    _actorOnMap.MoveDownOneUnit();
                    break;
                case CharacterMoveDirectionEnum.Left:
                    _actorOnMap.MoveLeftOneUnit();
                    break;
                case CharacterMoveDirectionEnum.Right:
                    _actorOnMap.MoveRightOneUnit();
                    break;
            }

            //パーティメンバーも全員降りる
            if (_partyOnMap != null)
            {
                foreach (var member in _partyOnMap)
                {
                    //強制的にActorと同じ位置とする
                    member.SetToPositionOnTile(_actorOnMap.GetCurrentPositionOnTile());

                    //同じ方向に移動する
                    switch (Direction)
                    {
                        case CharacterMoveDirectionEnum.Up:
                            member.MoveUpOneUnit();
                            break;
                        case CharacterMoveDirectionEnum.Down:
                            member.MoveDownOneUnit();
                            break;
                        case CharacterMoveDirectionEnum.Left:
                            member.MoveLeftOneUnit();
                            break;
                        case CharacterMoveDirectionEnum.Right:
                            member.MoveRightOneUnit();
                            break;
                    }
                }
            }

            //操作切り替え
            ChangeMoveSubject(_moveTypeEnum.Actor);
            CurrentVehicleId = "";

            // MapのBGM再生
            if (CurrentMapDataModel.autoPlayBGM && !string.IsNullOrEmpty(CurrentMapDataModel.bgmID))
            {
                SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_BGM,
                new SoundCommonDataModel(
                    CurrentMapDataModel.bgmID,
                    CurrentMapDataModel.bgmState.pan,
                    CurrentMapDataModel.bgmState.pitch,
                    CurrentMapDataModel.bgmState.volume
                ));
                await SoundManager.Self().PlayBgm();
            }
            else
            {
                SoundManager.Self().StopBgm();
            }
        }

        private static void RideOnAirship(bool GettingOnAndOff) {
            if (_animationSpeed <= 0)
            {
                if (GettingOnAndOff)
                {
                    _animationSpeed *= -1f;
                }
            }

            //アニメーション始動
            //飛ぶ
            TforuUtility.Instance.StartCoroutine(VehicleUpAnimation());
            //影
            TforuUtility.Instance.StartCoroutine(VehicleShadowsFallAnimation());
        }

        //アニメーションのスピード
        private static float _animationSpeed = 0.05f;

        //飛ぶアニメーション
        private static IEnumerator VehicleUpAnimation([CanBeNull] Action callBack = null) {
            float Y = _vehicleOnMap.transform.position.y + 1;
            while (Y > _vehicleOnMap.transform.position.y)
            {
                yield return new WaitForSeconds(0.05f);
                _vehicleOnMap.transform.position =
                    new Vector2(
                        _vehicleOnMap.transform.position.x,
                        _vehicleOnMap.transform.position.y + _animationSpeed
                    );
            }

            //アニメーション終了
            callBack?.Invoke();
        }

        //影のアニメーション
        private static IEnumerator VehicleShadowsFallAnimation([CanBeNull] Action callBack = null) {
            _shadowSpriteRenderer = _vehicleOnMap.GetVehicleShadow();
            Color color = _shadowSpriteRenderer.color;
            float alpha = 1f;
            while (alpha >= _shadowSpriteRenderer.color.a)
            {
                yield return new WaitForSeconds(0.05f);
                color.a = +_animationSpeed;
                _shadowSpriteRenderer.color = color;
            }

            //アニメーション終了
            callBack?.Invoke();
        }

        /// <summary>
        /// 全滅判定
        /// </summary>
        public static void CheckGameOver() {
            //既に処理中であればチェックしない
            if (_movingGameover) return;

            //全員戦闘不能の場合は、全滅
            var saveData = DataManager.Self().GetRuntimeSaveDataModel();
            for (int i = 0; i < saveData.runtimePartyDataModel.actors.Count; i++)
            {
                //戦闘不能は固定で0
                bool ret = DataManager.CheckStateInParty(i, 0);
                if (!ret)
                {
                    DataManager.Self().IsGameOverCheck = false;
                    return;
                }
            }
            //GAMEOVER表示
            _movingGameover = true;

            ShowGameOver();
        }

        public static void ShowGameOver() {
            //メニューの非表示
            MapManager.menu.MenuClose(false);
            //画面をフェードアウトする
            HudDistributor.Instance.NowHudHandler().DisplayInit();
            HudDistributor.Instance.NowHudHandler().FadeOut(GameOver, UnityEngine.Color.black);

            void GameOver() {
                //状態の更新
                _movingGameover = false;
                GameStateHandler.SetGameState(GameStateHandler.GameState.GAME_OVER);
                SceneManager.LoadScene("GameOver");
            }
        }

        public static void MapLoop(CharacterMoveDirectionEnum direction) {
            //マップのループ処理
            _mapLoop.MapLoopDirection(_actorOnMap, direction);
        }

        //以降、セーブデータ関連
        private static void DeleteAnimation() {
            foreach (var anim in _characterAnimations)
                if (anim != null && anim.gameObject != null)
                {
                    anim.StopAnimation();
                    
                    try
                    {
                        UnityEngine.Object.DestroyImmediate(anim.gameObject);
                    }
                    catch
                    {
                        //すでに消されてる
                    }
                }

            _characterAnimations = new List<CharacterAnimation>();
        }

#if USE_CHARACTER_MOVE_AS
        static int _searchLimit = 12;
        static bool _playTest = false;
        static bool _actorTarget = true;
        static int _startX = -1;
        static int _startY = -1;
        static int _goalX = -1;
        static int _goalY = -1;
        static string _vehicleId = null;
        static int _mapWidth;
        static int _mapHeight;
        static bool _mapVerticalLoop;
        static bool _mapHorizontalLoop;
        private static TilesOnThePosition _moveAsTilesOnThePosition;

        static string _mapClickHighlightObjectName = "MapClickHighlight";

        public static void SetSearchLimit(int searchLimit) {
            _searchLimit = searchLimit;
        }

        public static void SetPlayTest(bool playTest) {
            _playTest = playTest;
        }

        static TilesOnThePosition GetTilesOnThePosition() {
            var scene = SceneManager.GetActiveScene();
            {
                foreach (var obj in scene.GetRootGameObjects())
                {
                    if (obj.name == "Root")
                    {
                        var transform = obj.transform.Find(_mapClickHighlightObjectName);
                        if (transform == null)
                        {
                            var loadPrefab = UnityEditorWrapper.PrefabUtilityWrapper.LoadPrefabContents(
                                "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/MapClickHighlight.prefab");
                            var o = UnityEngine.Object.Instantiate(loadPrefab);
                            UnityEditorWrapper.PrefabUtilityWrapper.UnloadPrefabContents(loadPrefab);
                            o.SetActive(true);
                            o.transform.SetParent(obj.transform);
                            o.name = _mapClickHighlightObjectName;
                            o.transform.Find("Highlight").GetComponent<Canvas>().sortingLayerID = UnityUtil.SortingLayerManager.GetId(UnityUtil.SortingLayerManager.SortingLayerIndex.Runtime_MapCharacterPriorityNormal);
                            transform = o.transform;
                        }
                        return transform.GetComponent<TilesOnThePosition>();
                    }
                }
            }
            return null;
        }

        static void GetCurrentMapInfo() {
            _mapWidth = MapManager.CurrentMapDataModel.width;
            _mapHeight = MapManager.CurrentMapDataModel.height;
            var scrollType = MapManager.CurrentMapDataModel.scrollType;
            _mapVerticalLoop = (scrollType == MapDataModel.MapScrollType.LoopVertical || scrollType == MapDataModel.MapScrollType.LoopBoth);
            _mapHorizontalLoop = (scrollType == MapDataModel.MapScrollType.LoopHorizontal || scrollType == MapDataModel.MapScrollType.LoopBoth);

        }

        static void CancelMove() {
            if (_moveAsTilesOnThePosition == null) return;
            TimeHandler.Instance.RemoveTimeAction(UpdateMove);
            UnityEngine.Object.Destroy(_moveAsTilesOnThePosition.gameObject);
            _moveAsTilesOnThePosition = null;
            MenuManager.MenuActiveEvent -= CancelMove;
        }

        static void UpdateMove() {
            if (GameStateHandler.CurrentGameState() != GameStateHandler.GameState.MAP
            || MapManager.IsActiveMap() == false
            || MapEventExecutionController.Instance.CheckRunningEvent() == true
            || MenuManager.IsMenuActive)
            {
                CancelMove();
                return;
            }

            var isThrough = _actorOnMap.GetCharacterThrough();
            if (
#if !UNITY_EDITOR
            _playTest &&
#endif
                (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                isThrough = true;
            }
            bool moveEnd = false;
            bool endAction = false;
            var dir = GetNextDirection(isThrough, out moveEnd, out endAction);
            if (dir == 0 || moveEnd)
            {
                if (_actorOnMap.IsMoving()) return;
                for (int i = 0; i < _partyOnMap?.Count; i++)
                {
                    if (_partyOnMap[i].IsMoving()) return;
                }
            }
            if (dir == 0)
            {
                CancelMove();
                return;
            }
            if (moveEnd)    //これが最後の移動。
            {
                CancelMove();
            }
            var moveDir = GetMoveDirByDirection(dir);

            if (endAction)
            {
                //移動先で、ゴール地点にイベントがあれば、反応させる。
                var moveRoute = new List<CharacterMoveDirectionEnum>() { moveDir };
                if (_actorTarget)
                {
                    _actorOnMap.SetMoveRoute(moveRoute);
                }
                else
                {
                    _vehicleOnMap.SetMoveRoute(moveRoute);
                }
            }

            if (!isThrough || endAction)
            {
                if (_actorTarget)
                {
                    TryToMoveCharacter(moveDir, endAction);
                }
                else
                {
                    TryToMoveVehicle(moveDir, endAction);
                }
            }
            else
            {
                TryToMoveThrough(moveDir);
            }
        }

        static void TryToMoveThrough(CharacterMoveDirectionEnum directionEnum) {
            if (GameStateHandler.CurrentGameState() != GameStateHandler.GameState.MAP) return;
            if (_actorTarget)
            {
                if (_isRiding) return;
            }

            if (MapManager.IsActiveMap() == false) return;
            if (_actorOnMap == null) return;
            if (!_actorTarget) {
                if (_vehicleOnMap == null) return;
            }
            if (MapEventExecutionController.Instance.CheckRunningEvent() == true) return;

            //現在移動ルートを保持している場合は、移動ルートを破棄
            if (_actorTarget)
            {
                _actorOnMap.ResetMoveRoute();
            }
            else
            {
                _vehicleOnMap.ResetMoveRoute();
            }

            //移動中だった場合、設定を元に戻す
            _actorOnMap.ResetSetting();
            //現在移動中かどうかの取得
            for (int i = 0; i < _partyOnMap?.Count; i++)
            {
                //移動中だった場合、設定を元に戻す
                _partyOnMap[i].ResetSetting();
            }

            Vector2 targetPositionOnTile = Vector2.zero;
            if (_actorTarget)
            {
                targetPositionOnTile = directionEnum switch
                {
                    CharacterMoveDirectionEnum.Left => _actorOnMap.GetCurrentPositionOnTile() + Vector2.left,
                    CharacterMoveDirectionEnum.Right => _actorOnMap.GetCurrentPositionOnTile() + Vector2.right,
                    CharacterMoveDirectionEnum.Up => _actorOnMap.GetCurrentPositionOnTile() + Vector2.up,
                    CharacterMoveDirectionEnum.Down => _actorOnMap.GetCurrentPositionOnTile() + Vector2.down,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            else
            {
                targetPositionOnTile = directionEnum switch
                {
                    CharacterMoveDirectionEnum.Left => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.left,
                    CharacterMoveDirectionEnum.Right => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.right,
                    CharacterMoveDirectionEnum.Up => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.up,
                    CharacterMoveDirectionEnum.Down => _vehicleOnMap.GetCurrentPositionOnTile() + Vector2.down,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            CurrentTileData(targetPositionOnTile);

            // 先頭キャラクターの移動
            switch (directionEnum)
            {
                case CharacterMoveDirectionEnum.Up:
                    if (_actorTarget)
                    {
                        _actorOnMap.MoveUpOneUnit(CheckProcess);
                    }
                    else
                    {
                        _vehicleOnMap.MoveUpOneUnit(CheckProcess);
                        _actorOnMap.MoveUpOneUnit();
                        _partyOnMap?.ForEach(v => v.MoveUpOneUnit());
                    }
                    break;
                case CharacterMoveDirectionEnum.Down:
                    if (_actorTarget)
                    {
                        _actorOnMap.MoveDownOneUnit(CheckProcess);
                    }
                    else
                    {
                        _vehicleOnMap.MoveDownOneUnit(CheckProcess);
                        _actorOnMap.MoveDownOneUnit();
                        _partyOnMap?.ForEach(v => v.MoveDownOneUnit());
                    }
                    break;
                case CharacterMoveDirectionEnum.Left:
                    if (_actorTarget)
                    {
                        _actorOnMap.MoveLeftOneUnit(CheckProcess);
                    }
                    else
                    {
                        _vehicleOnMap.MoveLeftOneUnit(CheckProcess);
                        _actorOnMap.MoveLeftOneUnit();
                        _partyOnMap?.ForEach(v => v.MoveLeftOneUnit());
                    }
                    break;
                case CharacterMoveDirectionEnum.Right:
                    if (_actorTarget)
                    {
                        _actorOnMap.MoveRightOneUnit(CheckProcess);
                    }
                    else
                    {
                        _vehicleOnMap.MoveRightOneUnit(CheckProcess);
                        _actorOnMap.MoveRightOneUnit();
                        _partyOnMap?.ForEach(v => v.MoveRightOneUnit());
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(directionEnum), directionEnum, null);
            }

            if (_actorTarget)
            {
                // パーティメンバーの移動
                MapManager.PartyMoveCharacter(false);
            }
                
            //マップ情報更新
            var runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
            runtimeSaveDataModel.runtimePlayerDataModel.map.mapId = MapManager.CurrentMapDataModel.id;

            if (_actorTarget)
            {
                if (CurrentMapDataModel.scrollType != MapDataModel.MapScrollType.NoLoop)
                {
                    runtimeSaveDataModel.runtimePlayerDataModel.map.x = PositionOnTileToPositionOnLoopMapTile(new Vector2Int(_actorOnMap.x_next, _actorOnMap.y_next)).x;
                    runtimeSaveDataModel.runtimePlayerDataModel.map.y = PositionOnTileToPositionOnLoopMapTile(new Vector2Int(_actorOnMap.x_next, _actorOnMap.y_next)).y;
                }
                else
                {
                    runtimeSaveDataModel.runtimePlayerDataModel.map.x = (int) _actorOnMap.x_next;
                    runtimeSaveDataModel.runtimePlayerDataModel.map.y = (int) _actorOnMap.y_next;
                }
            }
            else
            {
                //アクター座標
                runtimeSaveDataModel.runtimePlayerDataModel.map.x = (int) _actorOnMap.x_next;
                runtimeSaveDataModel.runtimePlayerDataModel.map.y = (int) _actorOnMap.y_next;

                //現在のっている乗り物の保存
                var vehicle =
                    runtimeSaveDataModel.runtimePlayerDataModel.map.vehicles.Find(v => v.id == _vehicleOnMap.CharacterId);
                vehicle.x = (int) _actorOnMap.x_next;
                vehicle.y = (int) _actorOnMap.y_next;
            }
        }


        static int GetVectorX(int end, int start) {
            var v = end - start;
            if (_mapHorizontalLoop && Math.Abs(v) > _mapWidth / 2)
            {
                if (v < 0)
                {
                    v += _mapWidth;
                }
                else
                {
                    v -= _mapWidth;
                }
            }
            return v;
        }

        static int GetVectorY(int end, int start) {
            var v = end - start;
            if (_mapVerticalLoop && Math.Abs(v) > _mapHeight / 2)
            {
                if (v < 0)
                {
                    v += _mapHeight;
                }
                else
                {
                    v -= _mapHeight;
                }
            }
            return v;
        }

        static int GetMapX(int x) {
            if (_mapHorizontalLoop)
            {
                x = x % _mapWidth;
            }
            return x;
        }

        static int GetMapY(int y) {
            if (_mapVerticalLoop)
            {
                y = -(((-y) + (y + _mapHeight - 1) / _mapHeight * _mapHeight) % _mapHeight);
            }
            return y;
        }

        static int GetManhattanDistance(int x1, int y1, int x2, int y2) {
            return Math.Abs(GetVectorX(x1, x2)) + Math.Abs(GetVectorY(y1, y2));
        }

        static int GetHeuristicValue(int x1, int y1, int x2, int y2) {
            var value = GetManhattanDistance(x1, y1, x2, y2);
            if (value > 1)
            {
                value += _mapWidth + _mapHeight;
            }
            return value;
        }


        static Vector2Int GetVecByDirection(int direction) {
            return new Vector2Int((direction == 6 ? 1 : direction == 4 ? -1 : 0), (direction == 2 ? -1 : direction == 8 ? 1 : 0));
        }

        static CharacterMoveDirectionEnum GetMoveDirByDirection(int dir) {
            var moveDir = (dir == 2 ? CharacterMoveDirectionEnum.Down : dir == 4 ? CharacterMoveDirectionEnum.Left : dir == 6 ? CharacterMoveDirectionEnum.Right : dir == 2 ? CharacterMoveDirectionEnum.Down : dir == 8 ? CharacterMoveDirectionEnum.Up : CharacterMoveDirectionEnum.None);
            return moveDir;
        }


        static bool CanEnter(int x, int y, int direction, bool checkActor = false) {
            _moveAsTilesOnThePosition.InitForRuntime(MapManager.CurrentMapDataModel, new Vector2(x, y));
            CharacterMoveDirectionEnum moveDirectionEnum = GetMoveDirByDirection(direction);
            return _moveAsTilesOnThePosition.CanEnterThisTiles(moveDirectionEnum, checkActor ? null : _vehicleId);
        }

        static bool OutOfMap(int x, int y) {
            if (!_mapHorizontalLoop && (x < 0 || x >= _mapWidth)) return true;
            if (!_mapVerticalLoop && (y > 0 || y <= -_mapHeight)) return true;
            return false;
        }

        static HashSet<int> GetUnthroughableEventKeySet() {
            var unthroughableEventKeySet = new HashSet<int>();
            foreach (var eventOnMap in MapEventExecutionController.Instance.EventsOnMap)
            {
                if (!eventOnMap.IsPriorityNormal() || !eventOnMap.isValid) continue;

                if (eventOnMap.GetTrough())
                {
                    continue;
                }

                var pos = eventOnMap.GetCurrentPositionOnTile();
                var key = Node.GetKey((int) pos.x, (int) pos.y);
                unthroughableEventKeySet.Add(key);
            }
            return unthroughableEventKeySet;
        }

        class Node
        {
            public int x;
            public int y;
            public int f;
            public int g;
            public Node parent = null;
            public int GetKey() {
                return (-this.y) * _mapWidth + this.x;
            }
            public static int GetKey(int x, int y) {
                return (-y) * _mapWidth + x;
            }
        }
        static int GetNextDirection(bool isThrough, out bool moveEnd, out bool endAction) {
            moveEnd = false;
            endAction = false;
            var nodeDic = new Dictionary<int, Node>();
            var openList = new List<int>();
            var closedList = new List<int>();
            var start = new Node();

            var actor_x_now = _actorTarget ? _actorOnMap.x_now : _vehicleOnMap.x_now;
            var actor_y_now = _actorTarget ? _actorOnMap.y_now : _vehicleOnMap.y_now;
            var x_now = GetMapX(actor_x_now);
            var y_now = GetMapY(actor_y_now);
            if (x_now == _goalX && y_now == _goalY)
            {
                moveEnd = true;
                endAction = true;
                return 0;
            }
            var dir = 0;
            if (!OutOfMap(_goalX, _goalY) && GetManhattanDistance(x_now, y_now, _goalX, _goalY) <= 1)
            {
                moveEnd = true;
                var dx1 = GetVectorX(_goalX, x_now);
                var dy1 = GetVectorY(_goalY, y_now);
                if (dy1 < 0)
                {
                    dir = 2;
                }
                else if (dx1 < 0)
                {
                    dir = 4;
                }
                else if (dx1 > 0)
                {
                    dir = 6;
                }
                else if (dy1 > 0)
                {
                    dir = 8;
                }
                _moveAsTilesOnThePosition.InitForRuntime(MapManager.CurrentMapDataModel, new Vector2(_goalX, _goalY));
                var goalIsCounter = _moveAsTilesOnThePosition.GetHasCounterTile() == true;
                var canEnterGoal = CanEnter(_goalX, _goalY, dir, _actorTarget);
                foreach (var eventOnMap in MapEventExecutionController.Instance.EventsOnMap)
                {
                    if (!eventOnMap.IsPriorityNormal() || !eventOnMap.isValid) continue;

                    var pos = eventOnMap.GetCurrentPositionOnTile();
                    if ((int) pos.x == _goalX && (int) pos.y == _goalY)
                    {
                        //ゴールにイベントがあればアクション実行。
                        endAction = true;
                        break;
                    }
                    if (!canEnterGoal && goalIsCounter && (int) pos.x == x_now + dx1 * 2 && (int) pos.y == y_now + dy1 * 2)
                    {
                        //ゴールに進入不可で、ゴールがカウンター属性で、その先がイベントならアクション実行。
                        endAction = true;
                        break;
                    }
                }
                if (GetManhattanDistance(_startX, _startY, _goalX, _goalY) <= 1)
                {
                    if (_actorTarget)
                    {
                        if (!CanEnter(_goalX, _goalY, dir))
                        {
                            //ゴールに進入できない。
                            for (int i = 0; i < _vehiclesOnMap.Count; i++)
                            {
                                var pos = _vehiclesOnMap[i].GetCurrentPositionOnTile();
                                if ((int) pos.x == _goalX && (int) pos.y == _goalY)
                                {
                                    //ゴールに乗り物があればアクション実行。
                                    endAction = true;
                                    break;
                                }
                            }
                        }

                    }
                    else
                    {
                        if (CanEnter(_goalX, _goalY, dir, true))
                        {
                            //乗り物に乗っていて、ゴールに進入可能なら、降りるかをチェック。
                            endAction = true;
                        }
                    }
                }
                return dir;
            }
            if (!OutOfMap(_goalX, _goalY) && GetManhattanDistance(x_now, y_now, _goalX, _goalY) == 2)
            {
                var dx1 = GetVectorX(_goalX, x_now);
                var dy1 = GetVectorY(_goalY, y_now);
                if (dy1 < 0)
                {
                    dir = 2;
                }
                else if (dx1 < 0)
                {
                    dir = 4;
                }
                else if (dx1 > 0)
                {
                    dir = 6;
                }
                else if (dy1 > 0)
                {
                    dir = 8;
                }
                var canEnter = CanEnter(x_now + dx1 / 2, y_now + dy1 / 2, dir, _actorTarget);
                _moveAsTilesOnThePosition.InitForRuntime(MapManager.CurrentMapDataModel, new Vector2(x_now + dx1 / 2, y_now + dy1 / 2));
                var adjacentIsCounter = _moveAsTilesOnThePosition.GetHasCounterTile() == true;
                if (!canEnter && adjacentIsCounter)
                {
                    //移動方向のタイルが進入不可で、カウンター属性の場合。
                    foreach (var eventOnMap in MapEventExecutionController.Instance.EventsOnMap)
                    {
                        if (!eventOnMap.IsPriorityNormal() || !eventOnMap.isValid) continue;

                        var pos = eventOnMap.GetCurrentPositionOnTile();
                        if ((int) pos.x == _goalX && (int) pos.y == _goalY)
                        {
                            //ゴールにイベントがあればアクション実行。
                            moveEnd = true;
                            endAction = true;
                            return dir;
                        }
                    }
                }
            }


            HashSet<int> unthroughableEventKeySet = null;
            if (!isThrough || moveEnd)
            {
                unthroughableEventKeySet = GetUnthroughableEventKeySet();
            }

            start.parent = null;
            start.x = x_now;
            start.y = y_now;
            start.g = 0;
            var h = GetHeuristicValue(start.x, start.y, _goalX, _goalY);
            start.f = start.g + h;
            var key = start.GetKey();
            nodeDic.Add(key, start);
            openList.Add(key);

            Node best = start;
            while (openList.Count > 0)
            {
                Node bestNode = null;
                foreach (var key2 in openList)
                {
                    var node2 = nodeDic[key2];
                    if (bestNode == null || node2.f < bestNode.f)
                    {
                        bestNode = node2;
                    }
                }

                var current = bestNode;
                var x1 = current.x;
                var y1 = current.y;
                var key1 = current.GetKey();

                openList.Remove(key1);
                closedList.Add(key1);

                if (current.g >= _searchLimit) //探索制限。
                {
                    continue;
                }

                for (int j = 0; j < 4; j++)
                {
                    var dir2 = 2 + j * 2;
                    var vec = GetVecByDirection(dir2);
                    var x2 = GetMapX(x1 + vec.x);
                    var y2 = GetMapY(y1 + vec.y);
                    var key2 = Node.GetKey(x2, y2);
                    if (OutOfMap(x2, y2)) continue;
                    if (!(x2 == _goalX && y2 == _goalY) && !isThrough && (unthroughableEventKeySet.Contains(key2) || !CanEnter(actor_x_now + GetVectorX(x2, x_now), actor_y_now + GetVectorY(y2, y_now), dir2)))
                    {
                        continue;
                    }


                    h = GetHeuristicValue(x2, y2, _goalX, _goalY);
                    var g = current.g + 1;
                    var f = g + h;

                    Node neighbor = null;
                    var openIndex = openList.IndexOf(key2);
                    var closedIndex = closedList.IndexOf(key2);
                    if (openIndex < 0 && closedIndex < 0)
                    {
                        neighbor = new Node();
                        neighbor.x = x2;
                        neighbor.y = y2;
                        neighbor.f = f;
                        neighbor.g = g;
                        neighbor.parent = current;
                        nodeDic.Add(key2, neighbor);
                        openList.Add(key2);
                    }
                    else if (openIndex >= 0)
                    {
                        neighbor = nodeDic[key2];
                        if (f < neighbor.f)
                        {
                            openList.RemoveAt(openIndex);
                            neighbor.f = f;
                            neighbor.g = g;
                            neighbor.parent = current;
                            openList.Add(key2);
                        }
                    }
                    else if (closedIndex >= 0)
                    {
                        neighbor = nodeDic[key2];
                        if (f < neighbor.f)
                        {
                            closedList.RemoveAt(closedIndex);
                            neighbor.f = f;
                            neighbor.g = g;
                            neighbor.parent = current;
                            openList.Add(key2);
                        }
                    }
                    if (neighbor != null && (best == null || (neighbor.f < best.f || (neighbor.f == best.f && neighbor.g > best.g))))
                    {
                        best = neighbor;
                    }

                }
            }

            //ゴールあるいは、一番マシな到達点から、開始位置の１つ前まで遡る。
            var node = best;
            while (node.parent != null && node.parent != start)
            {
                node = node.parent;
            }

            {
                var dx1 = GetVectorX(node.x, start.x);
                var dy1 = GetVectorY(node.y, start.y);
                if (dy1 < 0)
                {
                    dir = 2;
                }
                else if (dx1 < 0)
                {
                    dir = 4;
                }
                else if (dx1 > 0)
                {
                    dir = 6;
                }
                else if (dy1 > 0)
                {
                    dir = 8;
                }
            }

            if (dir == 0)
            {
                //決められない場合は、ゴール方向に向かう。
                var vx2 = GetVectorX(x_now, _goalX);
                var vy2 = GetVectorY(y_now, _goalY);
                if (Math.Abs(vx2) > Math.Abs(vy2))
                {
                    dir = vx2 > 0 ? 4 : 6;
                }
                else if (vy2 != 0)
                {
                    dir = vy2 < 0 ? 8 : 2;
                }
                var vec = GetVecByDirection(dir);
                var cx = actor_x_now + vec.x;
                var cy = actor_y_now + vec.y;
                if (OutOfMap(cx, cy))
                {
                    dir = 0;
                }
            }

            if (dir != 0)
            {
                var vec = GetVecByDirection(dir);
                var cx = actor_x_now + vec.x;
                var cy = actor_y_now + vec.y;
                if (OutOfMap(cx, cy) || !isThrough && (unthroughableEventKeySet.Contains(Node.GetKey(GetMapX(cx), GetMapY(cy))) || !CanEnter(cx, cy, dir)))
                {
                    //その方向のタイルに進入不可なら、移動完了。
                    moveEnd = true;
                }
            }

            return dir;
        }
#endif
    }
}