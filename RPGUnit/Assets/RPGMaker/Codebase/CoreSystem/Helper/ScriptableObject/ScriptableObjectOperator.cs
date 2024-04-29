#define USE_PARTIAL_LOOP
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RPGMaker.Codebase.CoreSystem.Helper.SO;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Animation;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Encounter;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Enemy;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventBattle;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventCommon;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCommon;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCustom;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.State;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Troop;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.UiSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Vehicle;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.WordDefinition;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement.Repository;
using UnityEngine;
#if USE_PARTIAL_LOOP
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Sound;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RPGMaker.Codebase.CoreSystem.Helper
{
    public static class ScriptableObjectOperator
    {
        //====================================================================================================
        // 読込パス
        //====================================================================================================
        private const string JSON_PATH_ANIMATION = "Assets/RPGMaker/Storage/Animation/JSON/animation.json";
        private const string SO_PATH_ANIMATION = "Assets/RPGMaker/Storage/Animation/SO/animation.asset";
        private const string JSON_PATH_ARMOR = "Assets/RPGMaker/Storage/Initializations/JSON/armor.json";
        private const string SO_PATH_ARMOR = "Assets/RPGMaker/Storage/Initializations/SO/armor.asset";
        private const string JSON_PATH_ASSETMANAGE = "Assets/RPGMaker/Storage/AssetManage/JSON";
        private const string SO_PATH_ASSETMANAGE = "Assets/RPGMaker/Storage/AssetManage/SO";
        private const string JSON_PATH_ASSETMANAGE_DATA = "Assets/RPGMaker/Storage/AssetManage/JSON/assetsData.json";
        private const string SO_PATH_ASSETMANAGE_DATA = "Assets/RPGMaker/Storage/AssetManage/SO/assetsData.asset";
        private const string JSON_PATH_AUTOGUID = "Assets/RPGMaker/Storage/Initializations/JSON/autoGuide.json";
        private const string SO_PATH_AUTOGUID = "Assets/RPGMaker/Storage/Initializations/SO/autoGuide.asset";
        private const string JSON_PATH_CHARACTERACTOR = "Assets/RPGMaker/Storage/Character/JSON/characterActor.json";
        private const string SO_PATH_CHARACTERACTOR = "Assets/RPGMaker/Storage/Character/SO/characterActor.asset";
        private const string JSON_PATH_CLASS = "Assets/RPGMaker/Storage/Character/JSON/class.json";
        private const string SO_PATH_CLASS = "Assets/RPGMaker/Storage/Character/SO/class.asset";
        private const string JSON_PATH_ENCOUNTER = "Assets/RPGMaker/Storage/Encounter/JSON/encounter.json";
        private const string SO_PATH_ENCOUNTER = "Assets/RPGMaker/Storage/Encounter/SO/encounter.asset";
        private const string JSON_PATH_ENEMY = "Assets/RPGMaker/Storage/Character/JSON/enemy.json";
        private const string SO_PATH_ENEMY = "Assets/RPGMaker/Storage/Character/SO/enemy.asset";
        private const string JSON_PATH_FLAGS = "Assets/RPGMaker/Storage/Flags/JSON/flags.json";
        private const string SO_PATH_FLAGS = "Assets/RPGMaker/Storage/Flags/SO/flags.asset";
        private const string JSON_PATH_ITEM = "Assets/RPGMaker/Storage/Item/JSON/item.json";
        private const string SO_PATH_ITEM = "Assets/RPGMaker/Storage/Item/SO/item.asset";
        private const string JSON_PATH_SKILL = "Assets/RPGMaker/Storage/Initializations/JSON/skill.json";
        private const string SO_PATH_SKILL = "Assets/RPGMaker/Storage/Initializations/SO/skill.asset";
        private const string JSON_PATH_SKILLCUSTOM = "Assets/RPGMaker/Storage/Initializations/JSON/skillCustom.json";
        private const string SO_PATH_SKILLCUSTOM = "Assets/RPGMaker/Storage/Initializations/SO/skillCustom.asset";
        private const string JSON_PATH_STATE = "Assets/RPGMaker/Storage/Initializations/JSON/state.json";
        private const string SO_PATH_STATE = "Assets/RPGMaker/Storage/Initializations/SO/state.asset";
        private const string JSON_PATH_SYSTEM = "Assets/RPGMaker/Storage/Initializations/JSON/system.json";
        private const string SO_PATH_SYSTEM = "Assets/RPGMaker/Storage/Initializations/SO/system.asset";
        private const string JSON_PATH_TITLE = "Assets/RPGMaker/Storage/Initializations/JSON/title.json";
        private const string SO_PATH_TITLE = "Assets/RPGMaker/Storage/Initializations/SO/title.asset";
        private const string JSON_PATH_TROOP = "Assets/RPGMaker/Storage/Character/JSON/troop.json";
        private const string SO_PATH_TROOP = "Assets/RPGMaker/Storage/Character/SO/troop.asset";
        private const string JSON_PATH_UI = "Assets/RPGMaker/Storage/Ui/JSON/ui.json";
        private const string SO_PATH_UI = "Assets/RPGMaker/Storage/Ui/SO/ui.asset";
        private const string JSON_PATH_VEHICLES = "Assets/RPGMaker/Storage/Character/JSON/vehicles.json";
        private const string SO_PATH_VEHICLES = "Assets/RPGMaker/Storage/Character/SO/vehicles.asset";
        private const string JSON_PATH_WEAPON = "Assets/RPGMaker/Storage/Initializations/JSON/weapon.json";
        private const string SO_PATH_WEAPON = "Assets/RPGMaker/Storage/Initializations/SO/weapon.asset";
        private const string JSON_PATH_WORDS = "Assets/RPGMaker/Storage/Word/JSON/words.json";
        private const string SO_PATH_WORDS = "Assets/RPGMaker/Storage/Word/SO/words.asset";

        private const string JSON_PATH_MAPBASE = "Assets/RPGMaker/Storage/Map/JSON/mapbase.json";
        private const string SO_PATH_MAPBASE = "Assets/RPGMaker/Storage/Map/SO/mapbase.asset";

        private const string JSON_PATH_EVENT = "Assets/RPGMaker/Storage/Event/JSON/Event";
        private const string SO_PATH_EVENT = "Assets/RPGMaker/Storage/Event/SO/Event";
        private const string JSON_PATH_EVENTMAP = "Assets/RPGMaker/Storage/Event/JSON/eventMap.json";
        private const string SO_PATH_EVENTMAP = "Assets/RPGMaker/Storage/Event/SO/eventMap.asset";
        private const string JSON_PATH_EVENTCOMMON = "Assets/RPGMaker/Storage/Event/JSON/eventCommon.json";
        private const string SO_PATH_EVENTCOMMON = "Assets/RPGMaker/Storage/Event/SO/eventCommon.asset";
        private const string JSON_PATH_EVENTBATTLE = "Assets/RPGMaker/Storage/Event/JSON/eventBattle.json";
        private const string SO_PATH_EVENTBATTLE = "Assets/RPGMaker/Storage/Event/SO/eventBattle.asset";

        private const string JSON_PATH_MAP = "Assets/RPGMaker/Storage/Map/JSON/Map";
        private const string SO_PATH_MAP = "Assets/RPGMaker/Storage/Map/SO/Map";
        private const string SO_PATH_MAP_FOLDER = "Assets/RPGMaker/Storage/Map/SO";
        private const string ASSET_PATH_TILE = "Assets/RPGMaker/Storage/Map/TileAssets";
        private const string SO_PATH_TILE = "Assets/RPGMaker/Storage/Map/SO/TileAssets.asset";
        private const string ASSET_PATH_TILE_TABLE = "Assets/RPGMaker/Storage/Map/JSON/tileTable.json";
        private const string SO_PATH_TILE_TABLE = "Assets/RPGMaker/Storage/Map/SO/tileTable.asset";

#if USE_PARTIAL_LOOP
        private const string JSON_PATH_BGM_LOOP = "Assets/RPGMaker/Storage/Sounds/bgmLoopInfo.json";
        private const string SO_PATH_BGM_LOOP = "Assets/RPGMaker/Storage/Sounds/bgmLoopInfo.asset";
        private const string JSON_PATH_BGS_LOOP = "Assets/RPGMaker/Storage/Sounds/bgsLoopInfo.json";
        private const string SO_PATH_BGS_LOOP = "Assets/RPGMaker/Storage/Sounds/bgsLoopInfo.asset";
#endif

        //====================================================================================================
        // action
        //====================================================================================================
        private static List<System.Action> _firstActionList;
        private static List<System.Action> _secondActionList;

        //====================================================================================================
        // JSON→SO変換処理
        //====================================================================================================
        public static void CreateSO() {
            _firstActionList = null;
            _secondActionList = null;
            _firstActionList = new List<System.Action>();
            _secondActionList = new List<System.Action>();

            // アニメーション
            var jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH_ANIMATION);
            var anim = JsonHelper.FromJsonArray<AnimationDataModel>(jsonString);
            var animSo = ScriptableObject.CreateInstance<AnimationSO>();
            animSo.dataModels = anim;
            CreateAsset(animSo, SO_PATH_ANIMATION);

            // アーマー
            jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH_ARMOR);
            var armor = JsonHelper.FromJsonArray<ArmorDataModel>(jsonString);
            var armorSo = ScriptableObject.CreateInstance<ArmorSO>();
            armorSo.dataModels = armor;
            CreateAsset(armorSo, SO_PATH_ARMOR);

            // 素材管理
            // 管理用ファイル
            jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH_ASSETMANAGE_DATA);
            var assetData = JsonHelper.FromJsonArray<AssetManageDataModel>(jsonString);
            var assetDataSo = ScriptableObject.CreateInstance<AssetManagesSO>();
            assetDataSo.dataModels = assetData;
            CreateAsset(assetDataSo, SO_PATH_ASSETMANAGE_DATA);

            // 個々のファイル
            // ディレクトリ内のファイル全取得
            var dataPath =
                Directory.GetFiles(JSON_PATH_ASSETMANAGE + "/Assets/", "*.json", SearchOption.AllDirectories);
            for (var i = 0; i < dataPath.Length; i++)
            {
                dataPath[i] = dataPath[i].Replace("\\", "/");

                // 取得したJSONデータを読み込む
                jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(dataPath[i]);
                var asset = JsonHelper.FromJson<AssetManageDataModel>(jsonString);
                var assetSo = ScriptableObject.CreateInstance<AssetManageSO>();
                assetSo.dataModel = asset;
                CreateAsset(assetSo, SO_PATH_ASSETMANAGE + "/" + asset.id + ".asset");
            }

            // オートガイド
            jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH_AUTOGUID);
            var guide = JsonHelper.FromJsonArray<AutoGuideDataModel>(jsonString);
            var guideSo = ScriptableObject.CreateInstance<AutoGuideSO>();
            guideSo.dataModels = guide;
            CreateAsset(guideSo, SO_PATH_AUTOGUID);

            // アクター
            jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH_CHARACTERACTOR);
            var actor = JsonHelper.FromJsonArray<CharacterActorDataModel>(jsonString);
            var actorSo = ScriptableObject.CreateInstance<CharacterActorSO>();
            actorSo.dataModels = actor;
            CreateAsset(actorSo, SO_PATH_CHARACTERACTOR);

            // クラス
            jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH_CLASS);
            var classdata = JsonHelper.FromJsonArray<ClassDataModel>(jsonString);
            var classSo = ScriptableObject.CreateInstance<ClassSO>();
            classSo.dataModels = classdata;
            CreateAsset(classSo, SO_PATH_CLASS);

            // エンカウント
            jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH_ENCOUNTER);
            var encount = JsonHelper.FromJsonArray<EncounterDataModel>(jsonString);
            var encountSo = ScriptableObject.CreateInstance<EncounterSO>();
            encountSo.dataModels = encount;
            CreateAsset(encountSo, SO_PATH_ENCOUNTER);

            // エネミー
            jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH_ENEMY);
            var enemy = JsonHelper.FromJsonArray<EnemyDataModel>(jsonString);
            var enemySo = ScriptableObject.CreateInstance<EnemySO>();
            enemySo.dataModels = enemy;
            CreateAsset(enemySo, SO_PATH_ENEMY);

            // フラグ
            jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH_FLAGS);
            var flags = JsonHelper.FromJson<FlagDataModel>(jsonString);
            var flagsSo = ScriptableObject.CreateInstance<FlagsSO>();
            flagsSo.dataModel = flags;
            CreateAsset(flagsSo, SO_PATH_FLAGS);

            // アイテム
            jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH_ITEM);
            var item = JsonHelper.FromJsonArray<ItemDataModel>(jsonString);
            var itemSo = ScriptableObject.CreateInstance<ItemSO>();
            itemSo.dataModels = item;
            CreateAsset(itemSo, SO_PATH_ITEM);

            // スキルカスタム
            jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH_SKILLCUSTOM);
            var skillCustom = JsonHelper.FromJsonArray<SkillCustomDataModel>(jsonString);
            var skillCustomSo = ScriptableObject.CreateInstance<SkillCustomSO>();
            skillCustomSo.dataModels = skillCustom;
            CreateAsset(skillCustomSo, SO_PATH_SKILLCUSTOM);

            // スキルコモン
            jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH_SKILL);
            var skill = JsonHelper.FromJsonArray<SkillCommonDataModel>(jsonString);
            var skillSo = ScriptableObject.CreateInstance<SkillCommonSO>();
            skillSo.dataModels = skill;
            CreateAsset(skillSo, SO_PATH_SKILL);

            // ステート
            jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH_STATE);
            var state = JsonHelper.FromJsonArray<StateDataModel>(jsonString);
            var stateSo = ScriptableObject.CreateInstance<StateSO>();
            stateSo.dataModels = state;
            CreateAsset(stateSo, SO_PATH_STATE);

            // システム
            jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH_SYSTEM);
            var system = JsonHelper.FromJson<SystemSettingDataModel>(jsonString);
            var systemSo = ScriptableObject.CreateInstance<SystemSO>();
            systemSo.dataModels = system;
            CreateAsset(systemSo, SO_PATH_SYSTEM);

            // タイトル
            jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH_TITLE);
            var title = JsonHelper.FromJson<RuntimeTitleDataModel>(jsonString);
            var titleSo = ScriptableObject.CreateInstance<TitleSO>();
            titleSo.dataModel = title;
            CreateAsset(titleSo, SO_PATH_TITLE);

            // グループ
            jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH_TROOP);
            var troop = JsonHelper.FromJsonArray<TroopDataModel>(jsonString);
            var troopSo = ScriptableObject.CreateInstance<TroopSO>();
            troopSo.dataModels = troop;
            CreateAsset(troopSo, SO_PATH_TROOP);

            // UI
            jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH_UI);
            var ui = JsonHelper.FromJson<UiSettingDataModel>(jsonString);
            var uiSo = ScriptableObject.CreateInstance<UiSettingSO>();
            uiSo.dataModel = ui;
            CreateAsset(uiSo, SO_PATH_UI);

            // 乗り物
            jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH_VEHICLES);
            var vehicle = JsonHelper.FromJsonArray<VehiclesDataModel>(jsonString);
            var vehicleSo = ScriptableObject.CreateInstance<VehicleSO>();
            vehicleSo.dataModels = vehicle;
            CreateAsset(vehicleSo, SO_PATH_VEHICLES);

            // 武器
            jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH_WEAPON);
            var weapon = JsonHelper.FromJsonArray<WeaponDataModel>(jsonString);
            var weaponSo = ScriptableObject.CreateInstance<WeaponSO>();
            weaponSo.dataModels = weapon;
            CreateAsset(weaponSo, SO_PATH_WEAPON);

            // 文章
            jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH_WORDS);
            var words = JsonHelper.FromJson<WordDefinitionDataModel>(jsonString);
            var wordsSo = ScriptableObject.CreateInstance<WordSO>();
            wordsSo.dataModel = words;
            CreateAsset(wordsSo, SO_PATH_WORDS);

            // イベント系
            //--------------------------------------------------------------------------------------
            // マップ
            jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH_EVENTMAP);
            var eventMap = JsonHelper.FromJsonArray<EventMapDataModel>(jsonString);
            var eventMapSo = ScriptableObject.CreateInstance<EventMapSO>();
            eventMapSo.dataModels = eventMap;
            CreateAsset(eventMapSo, SO_PATH_EVENTMAP);

            // コモン
            jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH_EVENTCOMMON);
            var eventCommon = JsonHelper.FromJsonArray<EventCommonDataModel>(jsonString);
            var eventCommonSo = ScriptableObject.CreateInstance<EventCommonSO>();
            eventCommonSo.dataModels = eventCommon;
            CreateAsset(eventCommonSo, SO_PATH_EVENTCOMMON);

            // バトル
            jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH_EVENTBATTLE);
            var eventBattle = JsonHelper.FromJsonArray<EventBattleDataModel>(jsonString);
            var eventBattleSo = ScriptableObject.CreateInstance<EventBattleSO>();
            eventBattleSo.dataModels = eventBattle;
            CreateAsset(eventBattleSo, SO_PATH_EVENTBATTLE);

            // イベント
            // ディレクトリ内のファイル全取得
            dataPath = Directory.GetFiles(JSON_PATH_EVENT, "*.json", SearchOption.TopDirectoryOnly);
            for (var i = 0; i < dataPath.Length; i++)
            {
                dataPath[i] = dataPath[i].Replace("\\", "/");

                jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(dataPath[i]);
                var eventData = JsonHelper.FromJson<EventDataModel>(jsonString);
                var eventDataSo = ScriptableObject.CreateInstance<EventSO>();
                eventDataSo.dataModel = eventData;
                if (eventData.id != "")
                {
                    CreateAsset(eventDataSo, SO_PATH_EVENT + "/" + eventData.id + "-" + eventData.page + ".asset");
#if UNITY_EDITOR
                    AddressableManager.Path.SetAddressToAsset(
                        SO_PATH_EVENT + "/" + eventData.id + "-" + eventData.page + ".asset");
#endif
                }
            }

            // マップ系
            //--------------------------------------------------------------------------------------
            // マップ
            // ディレクトリ内のファイル全取得
            var mapBaseDataSo = ScriptableObject.CreateInstance<MapBaseSO>();
            mapBaseDataSo.dataModel = new List<MapBaseDataModel>();

            //SerialNumberがずれないように、同一の読み込み方法を用いる
            List<MapDataModel> mapDataModels = new MapRepository().LoadMapDataModels();
            for (int i = 0; i < mapDataModels.Count; i++)
            {
                var mapDataSo = ScriptableObject.CreateInstance<MapSO>();
                mapDataSo.dataModel = mapDataModels[i];
                if (mapDataModels[i].id != "")
                {
                    CreateAsset(mapDataSo, SO_PATH_MAP + "/" + mapDataModels[i].id + ".asset");
#if UNITY_EDITOR
                    AddressableManager.Path.SetAddressToAsset(SO_PATH_MAP + "/" + mapDataModels[i].id + ".asset");
#endif
                }

                MapBaseDataModel work = new MapBaseDataModel(mapDataModels[i].id, mapDataModels[i].name, mapDataModels[i].SerialNumber);
                mapBaseDataSo.dataModel.Add(work);
            }

            CreateAsset(mapBaseDataSo, SO_PATH_MAPBASE);
#if UNITY_EDITOR
            AddressableManager.Path.SetAddressToAsset(SO_PATH_MAPBASE);
#endif

            // タイル(アセットを直接探してSOに変換)
            var tileDataModels = Directory.GetFiles(ASSET_PATH_TILE)
                .Select(assetPath => UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<TileDataModel>(assetPath))
                .Where(tileAsset => tileAsset != null)
                .ToList();
            var tileSo = ScriptableObject.CreateInstance<TileSO>();
            tileSo.dataModels = tileDataModels;
            CreateAsset(tileSo, SO_PATH_TILE);

            // タイルテーブル
            jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(ASSET_PATH_TILE_TABLE);
            var tileTable = JsonHelper.FromJsonArray<TileDataModelInfo>(jsonString);
            var tileTableSo = ScriptableObject.CreateInstance<TileTableSO>();
            tileTableSo.dataModels = tileTable;
            CreateAsset(tileTableSo, SO_PATH_TILE_TABLE);

#if USE_PARTIAL_LOOP
            //　BGM/BGSループ情報
            for (int i = 0; i < 2; i++)
            {
                var jsonFilename = (i == 0) ? JSON_PATH_BGM_LOOP : JSON_PATH_BGS_LOOP;
                var soFilename = (i == 0) ? SO_PATH_BGM_LOOP : SO_PATH_BGS_LOOP;
                var jsonStr = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(jsonFilename);
                var loopInfos = JsonHelper.FromJsonArray<LoopInfoModel>(jsonStr);
                var loopInfoSo = ScriptableObject.CreateInstance<LoopInfoSO>();
                loopInfoSo.dataModels = loopInfos;
                CreateAsset(loopInfoSo, soFilename);
            }
#endif

#if UNITY_EDITOR
            // フォルダ作成
            AssetDatabase.StartAssetEditing();
            for (int i = 0; i < _firstActionList.Count; i++)
                _firstActionList[i].Invoke();
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
            
            // SO作成
            AssetDatabase.StartAssetEditing();
            for (int i = 0; i < _secondActionList.Count; i++)
                _secondActionList[i].Invoke();
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();

            _firstActionList.Clear();
            _secondActionList.Clear();
#endif
        }

        //====================================================================================================
        // 対応したクラス名を返す
        //====================================================================================================
        public static object GetClass<T>(string path) {
            switch (typeof(T).ToString().Split('.')[typeof(T).ToString().Split('.').Length - 1])
            {
                case "AnimationDataModel":
                    return AddressableManager.Load.LoadAssetSync<AnimationSO>(path)?.dataModels;
                case "ArmorDataModel":
                    return AddressableManager.Load.LoadAssetSync<ArmorSO>(path)?.dataModels;
                case "AssetManageDataModel":
                    return AddressableManager.Load.LoadAssetSync<AssetManageSO>(path)?.dataModel;
                case "AutoGuideDataModel":
                    return AddressableManager.Load.LoadAssetSync<AutoGuideSO>(path)?.dataModels;
                case "CharacterActorDataModel":
                    return AddressableManager.Load.LoadAssetSync<CharacterActorSO>(path)?.dataModels;
                case "ClassDataModel":
                    return AddressableManager.Load.LoadAssetSync<ClassSO>(path)?.dataModels;
                case "EncounterDataModel":
                    return AddressableManager.Load.LoadAssetSync<EncounterSO>(path)?.dataModels;
                case "EnemyDataModel":
                    return AddressableManager.Load.LoadAssetSync<EnemySO>(path)?.dataModels;
                case "FlagDataModel":
                    return AddressableManager.Load.LoadAssetSync<FlagsSO>(path)?.dataModel;
                case "ItemDataModel":
                    return AddressableManager.Load.LoadAssetSync<ItemSO>(path)?.dataModels;
                case "SkillCustomDataModel":
                    return AddressableManager.Load.LoadAssetSync<SkillCustomSO>(path)?.dataModels;
                case "SkillCommonDataModel":
                    return AddressableManager.Load.LoadAssetSync<SkillCommonSO>(path)?.dataModels;
                case "StateDataModel":
                    return AddressableManager.Load.LoadAssetSync<StateSO>(path)?.dataModels;
                case "SystemSettingDataModel":
                    return AddressableManager.Load.LoadAssetSync<SystemSO>(path)?.dataModels;
                case "RuntimeTitleDataModel":
                    return AddressableManager.Load.LoadAssetSync<TitleSO>(path)?.dataModel;
                case "TroopDataModel":
                    return AddressableManager.Load.LoadAssetSync<TroopSO>(path)?.dataModels;
                case "UiSettingDataModel":
                    return AddressableManager.Load.LoadAssetSync<UiSettingSO>(path)?.dataModel;
                case "VehiclesDataModel":
                    return AddressableManager.Load.LoadAssetSync<VehicleSO>(path)?.dataModels;
                case "WeaponDataModel":
                    return AddressableManager.Load.LoadAssetSync<WeaponSO>(path)?.dataModels;
                case "WordDefinitionDataModel":
                    return AddressableManager.Load.LoadAssetSync<WordSO>(path)?.dataModel;

                // イベント系
                case "EventDataModel":
                    return AddressableManager.Load.LoadAssetSync<EventSO>(path)?.dataModel;
                case "EventMapDataModel":
                    return AddressableManager.Load.LoadAssetSync<EventMapSO>(path)?.dataModels;
                case "EventCommonDataModel":
                    return AddressableManager.Load.LoadAssetSync<EventCommonSO>(path)?.dataModels;
                case "EventBattleDataModel":
                    return AddressableManager.Load.LoadAssetSync<EventBattleSO>(path)?.dataModels;

                // マップ系
                case "MapBaseDataModel":
                    return AddressableManager.Load.LoadAssetSync<MapBaseSO>(path)?.dataModel;
                case "MapDataModel":
                    return AddressableManager.Load.LoadAssetSync<MapSO>(path)?.dataModel;
                case "TileDataModel":
                    return AddressableManager.Load.LoadAssetSync<TileSO>(path)?.dataModels;
                case "TileDataModelInfo":
                    return AddressableManager.Load.LoadAssetSync<TileTableSO>(path)?.dataModels;

                default:
                    return "";
            }
        }

        //====================================================================================================
        // SO出力
        //====================================================================================================
        private static void CreateAsset(Object so, string path) {
            // 拡張子を除いたパスを取得する
            var extension = Path.GetExtension(path);
            var folderPath = path;
            if (!string.IsNullOrEmpty(extension))
            {
                folderPath = path.Replace(extension, string.Empty);
                folderPath = folderPath.Replace("/" + folderPath.Split('/')[folderPath.Split('/').Length - 1],
                    string.Empty);
            }

            // 既にSOがあり変更がなければ処理しない
            if (File.Exists(path))
            {
                var currentSO = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath(path, so.GetType());

                // 共通関数の呼び出し
                System.Type type = currentSO.GetType();
                var method = type.GetMethod("isEquals");
                object[] arg = new object[] { so };
                if (method != null)
                    if ((bool) method.Invoke(currentSO, arg))
                        return;
            }

            // フォルダがなければ作成
            if (Directory.Exists(folderPath) == false)
            {
                _firstActionList.Add(() =>
                {
                    Directory.CreateDirectory(folderPath);
                    UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
                });
            }

            _secondActionList.Add(() =>
            {
                UnityEditorWrapper.AssetDatabaseWrapper.CreateAsset(so, path);
            });
        }
    }
}