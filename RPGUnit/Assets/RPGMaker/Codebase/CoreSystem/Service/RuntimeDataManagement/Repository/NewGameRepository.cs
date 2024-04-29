using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Vehicle;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Knowledge.Misc;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Runtime.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Service.RuntimeDataManagement.Repository
{
    public partial class NewGameRepository
    {
        private List<ArmorDataModel> _armorDataModels;
        private DatabaseManagementService _databaseManagementService;

        private SystemSettingDataModel  _systemSettingDataModel;
        private List<VehiclesDataModel> _vehiclesDataModel;
        private List<WeaponDataModel>   _weaponDataModels;
        public  RuntimeConfigDataModel  RuntimeConfig;

        public RuntimeSaveDataModel RuntimeSaveData;

        private void Initialize() {
            if (_databaseManagementService != null) return;

            //各データモデル取得
            _databaseManagementService = new DatabaseManagementService();

            _systemSettingDataModel = _databaseManagementService.LoadSystem();
            _vehiclesDataModel = _databaseManagementService.LoadCharacterVehicles();
            _weaponDataModels = _databaseManagementService.LoadWeapon();
            _armorDataModels = _databaseManagementService.LoadArmor();
        }

        /// <summary>
        /// NewGame時に必要なデータの生成
        /// </summary>
        /// <param name="battleTest"></param>
        public void CreateGame(BattleSceneTransition battleTest = null) {
            Initialize();

            //セーブデータを新規作成
            RuntimeSaveData = new RuntimeSaveDataModel();

            //スイッチを作成
            RuntimeSaveData.switches.data = new List<bool>();
            for (var i = 0; i < _databaseManagementService.LoadFlags().switches.Count; i++)
                RuntimeSaveData.switches.data.Add(false);

            //変数を作成
            RuntimeSaveData.variables.data = new List<string>();
            for (var i = 0; i < _databaseManagementService.LoadFlags().variables.Count; i++)
                RuntimeSaveData.variables.data.Add("0");

            //Editorからパーティ情報の代入
            RuntimeSaveData.runtimePartyDataModel.actors = _systemSettingDataModel.initialParty.party;

            //戦闘テストの場合の処理
            if (battleTest != null)
            {
                RuntimeSaveData.runtimePartyDataModel.actors = new List<string>();
                for (int i = 0; i < battleTest.Actors.Length; i++)
                {
                    RuntimeSaveData.runtimePartyDataModel.actors.Add(battleTest.Actors[i].id);
                }
            }

            var actors = battleTest?.Actors;
            actors ??= _systemSettingDataModel.initialParty.party.Select(id => new BattleSceneTransition.Actor
                {id = id, level = -1, equipIds = new string[] { }}).ToArray();

            //アクターの情報を生成
            RuntimeSaveData.runtimeActorDataModels.Clear();
            foreach (var actor in actors)
            {
                RuntimeActorDataModel data = CreateActorData(actor, battleTest);
                if (data != null)
                    RuntimeSaveData.runtimeActorDataModels.Add(data);
            }

            //サウンド情報を設定
            //バトルBGM
            RuntimeSaveData.runtimeSystemConfig.battleBgm = new RuntimeSystemConfigDataModel.Sound(
                _systemSettingDataModel.bgm.battleBgm.name,
                _systemSettingDataModel.bgm.battleBgm.pan,
                _systemSettingDataModel.bgm.battleBgm.pitch,
                _systemSettingDataModel.bgm.battleBgm.volume
            );
            //勝利ME
            RuntimeSaveData.runtimeSystemConfig.victoryMe = new RuntimeSystemConfigDataModel.Sound(
                _systemSettingDataModel.bgm.victoryMe.name,
                _systemSettingDataModel.bgm.victoryMe.pan,
                _systemSettingDataModel.bgm.victoryMe.pitch,
                _systemSettingDataModel.bgm.victoryMe.volume
            );
            //敗北ME
            RuntimeSaveData.runtimeSystemConfig.defeatMe = new RuntimeSystemConfigDataModel.Sound(
                _systemSettingDataModel.bgm.defeatMe.name,
                _systemSettingDataModel.bgm.defeatMe.pan,
                _systemSettingDataModel.bgm.defeatMe.pitch,
                _systemSettingDataModel.bgm.defeatMe.volume
            );

            //初期データ入れ込み(RuntimePlayer編)
            RuntimeSaveData.runtimePlayerDataModel.map = new RuntimePlayerDataModel.PlayerMap();
            RuntimeSaveData.runtimePlayerDataModel.map.mapId =
                _systemSettingDataModel.initialParty.startMap.mapId;
            RuntimeSaveData.runtimePlayerDataModel.map.x =
                _systemSettingDataModel.initialParty.startMap.position[0];
            RuntimeSaveData.runtimePlayerDataModel.map.y =
                _systemSettingDataModel.initialParty.startMap.position[1];
            RuntimeSaveData.runtimePlayerDataModel.map.transparent =
                _systemSettingDataModel.optionSetting.optTransparent;

            //transparent
            //乗り物データ
            RuntimeSaveData.runtimePlayerDataModel.map.vehicles =
                new List<RuntimePlayerDataModel.Vhicle>();
            foreach (var vehicle in _vehiclesDataModel)
            {
                var Vhicle = new RuntimePlayerDataModel.Vhicle();
                Vhicle.id = vehicle.id;
                Vhicle.name = vehicle.name;
                Vhicle.mapId = vehicle.mapId;
                Vhicle.assetId = vehicle.images;
                Vhicle.x = vehicle.initialPos[0];
                Vhicle.y = vehicle.initialPos[1];
                Vhicle.speed = vehicle.speed;
                Vhicle.moveTags = vehicle.moveTags;
                RuntimeSaveData.runtimePlayerDataModel.map.vehicles.Add(Vhicle);
            }

            //隊列歩行の有効状態
            RuntimeSaveData.runtimeSystemConfig.follow = _systemSettingDataModel.optionSetting.optFollowers;

            var str = JsonUtility.ToJson(RuntimeSaveData);

            // 型を合わせる
            var inputString = new TextAsset(str);

            if (inputString == null) return;

            var json = JsonUtility.FromJson<RuntimeSaveDataModel>(inputString.text);

            RuntimeSaveData = json;
        }

        /// <summary>
        /// ActorDataModel生成
        /// </summary>
        /// <param name="actorData"></param>
        /// <param name="battleTest"></param>
        /// <returns></returns>
        public RuntimeActorDataModel CreateActorData(BattleSceneTransition.Actor actorData, BattleSceneTransition battleTest = null) {
            Initialize();

            //パーティ情報生成
            var actorDataModels = _databaseManagementService.LoadCharacterActor();
            var classDataModels = _databaseManagementService.LoadCharacterActorClass();
            var equipTypes = _systemSettingDataModel.equipTypes;

            var adm = actorDataModels.FirstOrDefault(adm =>
                adm.uuId == actorData.id && adm.charaType == (int) ActorTypeEnum.ACTOR);
            if (adm == null) return null;

            var level = actorData.level >= 0 ? actorData.level : adm.basic.initialLevel;

            //ゲーム全体での[最大レベル]以上のレベルが設定されている場合には、ゲーム全体での[最大レベル]の方を優先
            var classCommonData = new DatabaseManagementService().LoadClassCommon()[0];
            var maxLevel = classCommonData.maxLevel;
            if (maxLevel < level)
            {
                level = maxLevel;
            }

            var selectClass = classDataModels.FirstOrDefault(c => c.id == adm.basic.classId);

            //念のためここでパラメータ生成
            selectClass.UpdateGraph();

            //生成したパラメータを元にして、初期パラメータを決める
            var maxHP = selectClass.parameter.maxHp[level];
            var maxMP = selectClass.parameter.maxMp[level];
            var actor1 = new RuntimeActorDataModel(
                adm.uuId,
                adm.basic.name,
                adm.basic.secondName,
                adm.basic.profile,
                adm.basic.classId,
                level,
                adm.image.character,
                adm.image.face,
                adm.image.battler,
                adm.image.adv,
                new RuntimeActorDataModel.Exp(),
                maxHP,
                maxMP,
                0,
                new RuntimeActorDataModel.ParamPlus(),
                0
            );

            actor1.exp.classId = adm.basic.classId;
            var classDataModel = new DatabaseManagementService().LoadClassCommonByClassId(actor1.exp.classId);
            actor1.exp.value = classDataModel.GetExpForLevel(actor1.level);

            //装備の枠ははじめに作成し、以降消去しない
            actor1.equips = new List<RuntimeActorDataModel.Equip>();
            for (var equipIndex = 0; equipIndex < equipTypes.Count; equipIndex++)
            {
                var runtimeEquip = new RuntimeActorDataModel.Equip();
                runtimeEquip.equipType = equipTypes[equipIndex].id;
                runtimeEquip.itemId = "";
                actor1.equips.Add(runtimeEquip);
            }

            //NEWGAME又は、ゲーム実行中
            if (battleTest == null)
            {
                //初期装備の反映
                foreach (var equip in adm.equips)
                {
                    //タイプの特定
                    foreach (var weaponDataModel in _weaponDataModels)
                        if (weaponDataModel.basic.id == equip.value)
                        {
                            for (var equipIndex = 0; equipIndex < actor1.equips.Count; equipIndex++)
                                if (weaponDataModel.basic.equipmentTypeId == actor1.equips[equipIndex].equipType)
                                {
                                    SystemSettingDataModel.EquipType equipTypeData = null;
                                    for (int i = 0; i < equipTypes.Count; i++)
                                        if (equipTypes[i].id == weaponDataModel.basic.equipmentTypeId)
                                        {
                                            equipTypeData = equipTypes[i];
                                            break;
                                        }
                                    // MVMZ同様二刀流の場合でも武器は1つのみ設定
                                    ItemManager.ChangeEquipment(actor1, equipTypeData, equip.value, equipIndex, true);
                                    break;
                                }
                            break;
                        }

                    foreach (var armorDataModel in _armorDataModels)
                        if (armorDataModel.basic.id == equip.value)
                        {
                            for (var equipIndex = 0; equipIndex < actor1.equips.Count; equipIndex++)
                                if (armorDataModel.basic.equipmentTypeId == actor1.equips[equipIndex].equipType)
                                {
                                    SystemSettingDataModel.EquipType equipTypeData = null;
                                    for (int i = 0; i < equipTypes.Count; i++)
                                        if (equipTypes[i].id == armorDataModel.basic.equipmentTypeId)
                                        {
                                            equipTypeData = equipTypes[i];
                                            break;
                                        }
                                    ItemManager.ChangeEquipment(actor1, equipTypeData, equip.value, equipIndex, true);
                                }
                            break;
                        }
                }
            }
            //戦闘テスト
            else
            {
                for (int i = 0; i < battleTest.Actors.Length; i++)
                    if (actor1.actorId == battleTest.Actors[i].id)
                    {
                        for (int j = 0; j < battleTest.Actors[i].equipIds.Length; j++)
                        {
                            bool flg = false;
                            for (int k = 0; k < _weaponDataModels.Count; k++)
                                if (_weaponDataModels[k].basic.id == battleTest.Actors[i].equipIds[j])
                                {
                                    flg = true;

                                    SystemSettingDataModel.EquipType equipTypeData = null;
                                    for (int l = 0; l < equipTypes.Count; l++)
                                        if (equipTypes[l].id == _weaponDataModels[k].basic.equipmentTypeId)
                                        {
                                            equipTypeData = equipTypes[l];
                                            break;
                                        }
                                    ItemManager.ChangeEquipment(actor1, equipTypeData, _weaponDataModels[k].basic.id, j, true);
                                    break;
                                }

                            if (!flg)
                                for (int k = 0; k < _armorDataModels.Count; k++)
                                    if (_armorDataModels[k].basic.id == battleTest.Actors[i].equipIds[j])
                                    {
                                        SystemSettingDataModel.EquipType equipTypeData = null;
                                        for (int l = 0; l < equipTypes.Count; l++)
                                            if (equipTypes[l].id == _armorDataModels[k].basic.equipmentTypeId)
                                            {
                                                equipTypeData = equipTypes[l];
                                                break;
                                            }
                                        ItemManager.ChangeEquipment(actor1, equipTypeData, _armorDataModels[k].basic.id, j, true);
                                        break;
                                    }
                        }
                    }
            }

            // 装備の特性をHP/MPに反映させる
            actor1.hp = actor1.GetCurrentMaxHp(classDataModel);
            actor1.mp = actor1.GetCurrentMaxMp(classDataModel);

            return actor1;
        }


        public RuntimeSaveDataModel CreateLoadGame() {
            return RuntimeSaveData;
        }

        public void SaveData(RuntimeSaveDataModel runtimeSaveDataModel, int id) {
            SaveSaveFile("file" + id, JsonUtility.ToJson(runtimeSaveDataModel));
        }

        public RuntimeSaveDataModel LoadData(int id) {
            // Runtimeの処理
            // テキスト読込
            var jsonString =
                UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(Application.persistentDataPath + "/" +
                                                                       "file1.json");
            var json = JsonUtility.FromJson<RuntimeSaveDataModel>(jsonString);
            RuntimeSaveData = json;
            return RuntimeSaveData;
        }

        public RuntimeConfigDataModel NewConfig() {
            RuntimeConfig = new RuntimeConfigDataModel();

            SaveSaveFile("config", JsonUtility.ToJson(RuntimeConfig));

            return RuntimeConfig;
        }

        /// <summary>
        ///     configの保存
        /// </summary>
        public void SaveConfig() {
            SaveSaveFile("config", JsonUtility.ToJson(RuntimeConfig));
        }

        /// <summary>
        ///     configの読み込み
        /// </summary>
        /// <returns></returns>
#if !UNITY_SWITCH && !UNITY_PS4
        public RuntimeConfigDataModel LoadConfig() {
            RuntimeConfig = new RuntimeConfigDataModel();

            //試しにJSONに変更
            var jsonWk = JsonUtility.ToJson(RuntimeConfig);

#if !UNITY_WEBGL && !UNITY_IOS && !UNITY_ANDROID
            //テキスト読込
            var path = Application.persistentDataPath + "/" + "config.json";
            try
            {
                var streamReader = new StreamReader(path);
                var str = streamReader.ReadToEnd();
                streamReader.Close();
                // 型を合わせる
                var inputString = new TextAsset(str);
                var json = JsonUtility.FromJson<RuntimeConfigDataModel>(inputString.text);

                RuntimeConfig = json;
                return RuntimeConfig;
            }
            catch (Exception)
            {
                return null;
            }
#else
            try {
                var json = JsonUtility.FromJson<RuntimeConfigDataModel>(PlayerPrefs.GetString("config", null));
                RuntimeConfig = json;
                return RuntimeConfig;
            }
            catch (Exception)
            {
                return null;
            }
#endif
        }

        public int GetSaveFileCount() {
#if !UNITY_WEBGL && !UNITY_IOS && !UNITY_ANDROID
            //セーブデータが格納されているフォルダのjsonファイルのファイルパスを配列で取得
            var jsonPathList = Directory.GetFiles(Application.persistentDataPath, "*.json");

            // パスにfileが含まれるjsonファイルの個数を返す
            return jsonPathList.Count(v =>
            {
                //既にファイルが存在している場合は、含めない
                if (v.Contains("file9999"))
                {
                    return false;
                }
                return v.Contains("file");
            });
#else
            return PlayerPrefs.GetInt("SaveFileCount", 0);
#endif
        }
        
        public bool IsAutoSaveFile() {
#if !UNITY_WEBGL && !UNITY_IOS && !UNITY_ANDROID
            //オートセーブ用のファイルがあるか取得
            var jsonPathList = Directory.GetFiles(Application.persistentDataPath, "file0.json");
            return jsonPathList.Length > 0;
#else
            string work = PlayerPrefs.GetString("file0", null);
            if (work != null && work.Length > 0)
                return true;
            return false;
#endif
        }

        public string LoadSaveFile(string filename) {
#if !UNITY_WEBGL && !UNITY_IOS && !UNITY_ANDROID
            if (File.Exists(Application.persistentDataPath + "/" + filename + ".json"))
                return File.ReadAllText(Application.persistentDataPath + "/" + filename + ".json");
            return null;
#else
            return PlayerPrefs.GetString(filename, null);
#endif
        }

        public void SaveSaveFile(string filename, string data) {
#if !UNITY_WEBGL && !UNITY_IOS && !UNITY_ANDROID
            File.WriteAllText(Application.persistentDataPath + "/" + filename + ".json", data);
#else
            PlayerPrefs.SetString(filename, data);

            int cnt = 0;
            for (int i = 1; i <= 20; i++)
            {
                string work = PlayerPrefs.GetString("file" + i, null);
                if (work != null && work != "") cnt++;
            }
            PlayerPrefs.SetInt("SaveFileCount", cnt);
#endif
        }
#endif
    }
}