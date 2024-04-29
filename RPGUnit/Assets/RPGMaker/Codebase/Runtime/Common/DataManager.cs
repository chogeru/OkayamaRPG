using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Animation;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Enemy;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCustom;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.State;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Troop;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.UiSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Vehicle;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.WordDefinition;
using RPGMaker.Codebase.CoreSystem.Knowledge.Misc;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.RuntimeDataManagement;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Common
{
    public class DataManager
    {
        private static DataManager _self;

        //アクターの特徴検索
        public static    int         NotTraits    = -99999999;
        private readonly GameActors  _gameActors  = new GameActors();
        private readonly GameMessage _gameMessage = new GameMessage();

        private List<CharacterActorDataModel> _actorSo;
        private List<AnimationDataModel>      _animationSo;
        private List<ArmorDataModel>          _armorSo;
        private List<ClassDataModel>          _classSo;

        private DatabaseManagementService _databaseManagementService;
        private List<EnemyDataModel>      _enemySo;
        private FlagDataModel             _flagsSo;

        private GameParty           _gameParty;
        private GameTroop           _gameTroop;
        private List<ItemDataModel> _itemSo;

        private RuntimeConfigDataModel       _runtimeConfigCo;
        private RuntimeDataManagementService _runtimeDataManagementService;
        private RuntimeSaveDataModel         _runtimeSaveDataSo;
        private List<SkillCustomDataModel>   _skillCustomSo;
        private List<StateDataModel>         _stateSo;
        private SystemSettingDataModel       _systemSo;
        private List<TroopDataModel>         _troopSo;
        private UiSettingDataModel           _uiSettingDataModel;
        private List<VehiclesDataModel>      _vehicleSo;
        private List<WeaponDataModel>        _weaponSo;
        private WordDefinitionDataModel      _wordDefinitionSo;
        public int BattleResult { get; set; }

        public bool IsGameOverCheck { get; set; } = false;

        /// <summary>ニューゲームからゲームを開始したかどうか</summary>
        public bool IsNewGame { get; private set; } = true;

        //現在コンティニューに使われているデータ
        public int NowLoadId { get; private set; } = 1;

        private void Start() {
            _databaseManagementService = new DatabaseManagementService();
            _runtimeDataManagementService = new RuntimeDataManagementService();

            LoadData();
        }

        public void SetTroopForBattle(GameTroop troop, bool isBattlePreview = false) {
            if (!isBattlePreview)
            {
                GetGameParty();
            }
            else
            {
                _gameParty = new GameParty();
                _gameParty.SetupStartingMembersFromDataBase();
            }

            _gameTroop = troop;
        }

        public void LoadData() {
            _actorSo = _databaseManagementService.LoadCharacterActor();
            _vehicleSo = _databaseManagementService.LoadCharacterVehicles();
            _classSo = _databaseManagementService.LoadCharacterActorClass();
            _skillCustomSo = _databaseManagementService.LoadSkillCustom();
            _systemSo = _databaseManagementService.LoadSystem();
            _uiSettingDataModel = _databaseManagementService.LoadUiSettingDataModel();
            _wordDefinitionSo = _databaseManagementService.LoadWordDefinition();
            _runtimeConfigCo = _runtimeDataManagementService.LoadConfig();
            _itemSo = _databaseManagementService.LoadItem();
            _weaponSo = _databaseManagementService.LoadWeapon();
            _armorSo = _databaseManagementService.LoadArmor();
            _enemySo = _databaseManagementService.LoadEnemy();
            _troopSo = _databaseManagementService.LoadTroop();
            _animationSo = _databaseManagementService.LoadAnimation();
            _stateSo = _databaseManagementService.LoadStateEdit();
            _flagsSo = _databaseManagementService.LoadFlags();
        }

        public RuntimeSaveDataModel LoadSaveData(int saveNo) {
            NowLoadId = saveNo;

            var runtimeDataManagementService = new RuntimeDataManagementService();
            var str = runtimeDataManagementService.LoadSaveData("file" + saveNo);
            if (str == null) return null;

            // 型を合わせる
            var inputString = new TextAsset(str);

            IsNewGame = false;
            var saveDataModel = JsonUtility.FromJson<RuntimeSaveDataModel>(inputString.text);

            // スイッチと変数をゲームデータと整合性をとる
            var flagDataModel = _databaseManagementService.LoadFlags();
            int diffSwitchCount = flagDataModel.switches.Count - saveDataModel.switches.data.Count;
            for (int i = 0; i < diffSwitchCount; i++) {
                saveDataModel.switches.data.Add(false);
            }

            int diffVariableCount = flagDataModel.variables.Count - saveDataModel.variables.data.Count;
            for (int i = 0; i < diffVariableCount; i++) {
                saveDataModel.variables.data.Add("0");
            }

            _runtimeSaveDataSo = saveDataModel;

            return _runtimeSaveDataSo;
        }

        public void CreateGame(BattleSceneTransition battleTest = null) {
            IsNewGame = true;
            _runtimeDataManagementService.StartNewGame(battleTest);
        }

        public RuntimeConfigDataModel CreateConfig() {
            _runtimeConfigCo = _runtimeDataManagementService.NewConfig();
            return _runtimeConfigCo;
        }

        public RuntimeSaveDataModel CreateLoadGame() {
            _runtimeSaveDataSo = _runtimeDataManagementService.StartLoadGame();
            return _runtimeSaveDataSo;
        }

        /// <summary>
        /// 戦闘テスト時にクリアする
        /// </summary>
        public void ClearRuntimeSaveDataModel() {
            _runtimeSaveDataSo = null;
        }

        /// <summary>
        /// ActorData生成
        /// </summary>
        /// <param name="actorData"></param>
        /// <returns></returns>
        public RuntimeActorDataModel CreateActorData(BattleSceneTransition.Actor actorData) {
            return _runtimeDataManagementService.CreateActorData(actorData);
        }

        public static DataManager Self() {
            if (_self == null)
            {
                _self = new DataManager();
                _self.Start();
            }

            return _self;
        }

        public List<CharacterActorDataModel> GetActorDataModels() {
            return _actorSo;
        }

        public CharacterActorDataModel GetActorDataModel(string actorId) {
            return _actorSo.FirstOrDefault(t => t.uuId == actorId);
        }

        public VehiclesDataModel GetVehicleDataModel(string vehicleId) {
            return _vehicleSo.FirstOrDefault(t => t.id == vehicleId);
        }

        public List<ClassDataModel> GetClassDataModels() {
            return _classSo;
        }

        public ClassDataModel GetClassDataModel(string classesId) {
            return _classSo.FirstOrDefault(t => t.id == classesId);
        }

        public List<SkillCustomDataModel> GetSkillCustomDataModels() {
            return _skillCustomSo;
        }

        public SkillCustomDataModel GetSkillCustomDataModel(string id) {
            return _skillCustomSo.FirstOrDefault(t => t.basic.id == id);
        }

        public List<ItemDataModel> GetItemDataModels() {
            return _itemSo;
        }

        public ItemDataModel GetItemDataModel(string id) {
            return _itemSo.FirstOrDefault(t => t.basic.id == id);
        }

        public List<WeaponDataModel> GetWeaponDataModels() {
            return _weaponSo;
        }

        public WeaponDataModel GetWeaponDataModel(string id) {
            return _weaponSo.FirstOrDefault(t => t.basic.id == id);
        }

        public List<ArmorDataModel> GetArmorDataModels() {
            return _armorSo;
        }

        public ArmorDataModel GetArmorDataModel(string id) {
            return _armorSo.FirstOrDefault(t => t.basic.id == id);
        }

        public List<EnemyDataModel> GetEnemyDataModels() {
            return _enemySo;
        }

        public EnemyDataModel GetEnemyDataModel(string enemyId) {
            return _enemySo.FirstOrDefault(t => t.id == enemyId);
        }

        public List<TroopDataModel> GetTroopDataModels() {
            return _troopSo;
        }

        public TroopDataModel GetTroopDataModel(string id) {
            return _troopSo.FirstOrDefault(t => t.id == id);
        }

        public List<StateDataModel> GetStateDataModels() {
            return _stateSo;
        }

        public StateDataModel GetStateDataModel(string id) {
            return _stateSo.FirstOrDefault(t => t.id == id);
        }

        public List<AnimationDataModel> GetAnimationDataModels() {
            return _animationSo;
        }

        public AnimationDataModel GetAnimationDataModel(string id) {
            return _animationSo.FirstOrDefault(t => t.id == id);
        }

        public FlagDataModel GetFlags() {
            return _flagsSo;
        }

        public SystemSettingDataModel GetSystemDataModel() {
            return _systemSo;
        }

        public UiSettingDataModel GetUiSettingDataModel() {
            return _uiSettingDataModel;
        }

        public WordDefinitionDataModel GetWordDefinitionDataModel() {
            return _wordDefinitionSo;
        }

        public RuntimeSaveDataModel GetRuntimeSaveDataModel() {
            return _runtimeSaveDataSo;
        }

        public RuntimeConfigDataModel GetRuntimeConfigDataModel() {
            return _runtimeConfigCo;
        }

        public GameActors GetGameActors() {
            return _gameActors;
        }

        public GameParty GetGameParty() {
            if (_gameParty == null)
            {
                _gameParty = new GameParty();
            }
            return _gameParty;
        }

        public void ReloadGameParty() {
            _gameParty = new GameParty();
        }

        public void SetGamePartyBattleTest(GameParty gameParty) {
            _gameParty = gameParty;
        }

        public GameTroop GetGameTroop() {
            return _gameTroop;
        }

        public GameMessage GetGameMessage() {
            return _gameMessage;
        }

        public static bool CheckStateInParty(int partyNo, int stateSerialNo) {
            var saveData = Self().GetRuntimeSaveDataModel();
            var stateDataModels = Self().GetStateDataModels();
            if (partyNo < saveData.runtimePartyDataModel.actors.Count)
                for (var j = 0; j < saveData.runtimeActorDataModels.Count; j++)
                    if (saveData.runtimeActorDataModels[j].actorId == saveData.runtimePartyDataModel.actors[partyNo])
                        //アクターのステートに設定されている特徴の検索
                        for (var m = 0; m < saveData.runtimeActorDataModels[j].states.Count; m++)
                            if (saveData.runtimeActorDataModels[j].states[m].id == stateDataModels[stateSerialNo].id)
                                return true;
            return false;
        }


#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod()]
        static void Init() {
            _self = null;
        }
#endif
    }
}