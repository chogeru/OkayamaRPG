using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Troop;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Vehicle;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Character.View;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Character
{
    /// <summary>
    /// キャラクター及び、敵のHierarchy
    /// </summary>
    public class CharacterHierarchy : AbstractHierarchy
    {
        private List<CharacterActorDataModel> _characterActorDataModels;
        private List<ClassDataModel>          _classDataModels;
        private List<TroopDataModel>          _troopDataModels;
        private List<VehiclesDataModel>       _vehiclesDataModels;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CharacterHierarchy() {
            View = new CharacterHierarchyView(this);
            //Refresh();
        }

        /// <summary>
        /// View
        /// </summary>
        public CharacterHierarchyView View { get; }


        /// <summary>
        /// データ読込
        /// </summary>
        override protected void LoadData() {
            base.LoadData();

            //各Modelデータの再読込
            _characterActorDataModels = databaseManagementService.LoadCharacterActor();
            _vehiclesDataModels = databaseManagementService.LoadCharacterVehicles();
            _classDataModels = databaseManagementService.LoadCharacterActorClass();
            var troops = databaseManagementService.LoadTroop();
            _troopDataModels = new List<TroopDataModel>();
            foreach (var troop in troops)
            {
                if (troop.id == TroopDataModel.TROOP_PREVIEW) continue;
                if (troop.id == TroopDataModel.TROOP_BTATLE_TEST) continue;
                if (troop.id == TroopDataModel.TROOP_AUTOMATCHING) continue;
                _troopDataModels.Add(troop);
            }
        }

        /// <summary>
        /// Viewのアップデート
        /// </summary>
        protected override void UpdateView(string updateData = null) {
            base.UpdateView();
            View.Refresh(
                _characterActorDataModels.FindAll(item => item.charaType == (int) ActorTypeEnum.ACTOR),
                _characterActorDataModels.FindAll(item => item.charaType == (int) ActorTypeEnum.NPC),
                _vehiclesDataModels,
                _classDataModels
            );
        }

        /// <summary>
        /// アクターデータ作成
        /// </summary>
        /// <param name="characterHierarchyView"></param>
        public void CreateCharacterActorDataModel(CharacterHierarchyView characterHierarchyView) {
            var createNum = 0;
            foreach (var dataModel in _characterActorDataModels)
                if (dataModel.charaType == (int) ActorTypeEnum.ACTOR)
                    createNum += 1;

            var newDataModel =
                CharacterActorDataModel.CreateDefault(Guid.NewGuid().ToString(),
                    "#" + string.Format("{0:D4}", createNum + 1) + "　" +  EditorLocalize.LocalizeText("WORD_1518"),
                    (int) ActorTypeEnum.ACTOR);
            
            //職業の初期値設定
            newDataModel.basic.classId = databaseManagementService.LoadCharacterActorClass()[0].id;
            //画像の初期値設定
            newDataModel.image.face = ImageManager.GetImageNameList(PathManager.IMAGE_FACE)[0];
            newDataModel.image.character = ImageManager.GetSvIdList(AssetCategoryEnum.MOVE_CHARACTER).Count > 0
                ? ImageManager.GetSvIdList(AssetCategoryEnum.MOVE_CHARACTER)[0].id
                : "";
            newDataModel.image.battler = ImageManager.GetSvIdList(AssetCategoryEnum.SV_BATTLE_CHARACTER).Count > 0
                ? ImageManager.GetSvIdList(AssetCategoryEnum.SV_BATTLE_CHARACTER)[0].id
                : "";
            newDataModel.image.adv = ImageManager.GetImageNameList(PathManager.IMAGE_ADV)[0];

            _characterActorDataModels.Add(newDataModel);
            databaseManagementService.SaveCharacterActor(_characterActorDataModels);

            // キャラクター領域のみリフレッシュ
            Refresh();
        }

        /// <summary>
        /// アクター削除
        /// </summary>
        /// <param name="characterActorDataModel"></param>
        public void DeleteCharacterActorDataModel(CharacterActorDataModel characterActorDataModel) {
            _characterActorDataModels.Remove(characterActorDataModel);
            databaseManagementService.SaveCharacterActor(_characterActorDataModels);

            //初期パーティの数が、アクターデータの数より超えていたら、超えた分削る
            var system = databaseManagementService.LoadSystem();
            if (_characterActorDataModels.Count < system.initialParty.partyMax)
            {
                for (int i = 0; i < system.initialParty.party.Count; i++)
                {
                    bool flag = false;
                    for (int j = 0; j < _characterActorDataModels.Count; j++)
                    {
                        if (_characterActorDataModels[j].uuId == system.initialParty.party[i])
                        {
                            flag = true;
                        }
                    }

                    if (!flag)
                    {
                        system.initialParty.party.RemoveAt(i);
                    }
                }
                system.initialParty.partyMax = system.initialParty.party.Count;
            }
            else
            {
                //削除したアクターが初期パーティに選択していたら、データから取得する
                string workUuId = "";
                for (int i = 0; i < _characterActorDataModels.Count; i++)
                {
                    bool flg = false;
                    for (int j = 0; j < system.initialParty.party.Count; j++)
                    {
                        if (_characterActorDataModels[i].uuId == system.initialParty.party[j])
                        {
                            flg = true;
                            break;
                        }
                    }
                    if (!flg)
                    {
                        workUuId = _characterActorDataModels[i].uuId;
                        break;
                    }
                }
                
                for (int i = 0; i < system.initialParty.party.Count; i++)
                    if (system.initialParty.party[i] == characterActorDataModel.uuId &&
                        _characterActorDataModels.Count > 0)
                    {
                        system.initialParty.party[i] = workUuId;
                    }
            }

            databaseManagementService.SaveSystem(system);

            // キャラクター領域のみリフレッシュ
            Refresh();
        }

        /// <summary>
        /// NPCデータ作成
        /// </summary>
        /// <param name="characterHierarchyView"></param>
        public void CreateNpcCharacterActorDataModel(CharacterHierarchyView characterHierarchyView) {
            var createNum = 0;
            foreach (var dataModel in _characterActorDataModels)
                if (dataModel.charaType == (int) ActorTypeEnum.NPC)
                    createNum += 1;

            var newDataModel =
                CharacterActorDataModel.CreateDefault(Guid.NewGuid().ToString(),
                    "#" + string.Format("{0:D4}", createNum + 1) + "　" +  EditorLocalize.LocalizeText("WORD_1518"),
                    (int) ActorTypeEnum.NPC);
            //画像の初期値設定
            newDataModel.image.face = ImageManager.GetImageNameList(PathManager.IMAGE_FACE)[0];
            newDataModel.image.character = ImageManager.GetSvIdList(AssetCategoryEnum.MOVE_CHARACTER).Count > 0
                ? ImageManager.GetSvIdList(AssetCategoryEnum.MOVE_CHARACTER)[0].id
                : "";
            newDataModel.image.battler = ImageManager.GetSvIdList(AssetCategoryEnum.SV_BATTLE_CHARACTER).Count > 0
                ? ImageManager.GetSvIdList(AssetCategoryEnum.SV_BATTLE_CHARACTER)[0].id
                : "";
            newDataModel.image.adv = ImageManager.GetImageNameList(PathManager.IMAGE_ADV)[0];

            _characterActorDataModels.Add(newDataModel);
            databaseManagementService.SaveCharacterActor(_characterActorDataModels);

            // キャラクター領域のみリフレッシュ
            Refresh();
        }

        /// <summary>
        /// NPC削除
        /// </summary>
        /// <param name="characterActorDataModel"></param>
        public void DeleteNpcCharacterActorDataModel(CharacterActorDataModel characterActorDataModel) {
            _characterActorDataModels.Remove(characterActorDataModel);
            databaseManagementService.SaveCharacterActor(_characterActorDataModels);
            Refresh();
        }

        /// <summary>
        /// コピー＆貼り付け処理
        /// </summary>
        /// <param name="characterHierarchyView"></param>
        /// <param name="characterActorDataModel"></param>
        public void PasteActorOrNpcDataModel(
            CharacterHierarchyView characterHierarchyView,
            CharacterActorDataModel characterActorDataModel
        ) {
            var uuid = Guid.NewGuid().ToString();
            var name = characterActorDataModel.DataClone().basic.name;
            var newModel = characterActorDataModel.DataClone();
            newModel.uuId = uuid;
            newModel.basic.name = CreateDuplicateName(_characterActorDataModels.Select(c=>c.basic.name).ToList(), name);

            _characterActorDataModels.Add(newModel);
            databaseManagementService.SaveCharacterActor(_characterActorDataModels);
            Refresh();
        }

        /// <summary>
        /// 初期パーティ表示
        /// </summary>
        public void OpenInitialPartySettingInspector() {
            Inspector.Inspector.CharacterEarlyPartyView();
        }

        /// <summary>
        /// 乗り物データ作成
        /// </summary>
        public void CreateVehicleDataModel() {
            var newModel = VehiclesDataModel.CreateDefault();
            newModel.name = "#" + string.Format("{0:D4}", _vehiclesDataModels.Count + 1) + "　" + 
                            EditorLocalize.LocalizeText("WORD_1518");
            _vehiclesDataModels.Add(newModel);
            databaseManagementService.SaveCharacterVehicles(_vehiclesDataModels);

            Refresh();
        }

        /// <summary>
        /// 乗り物のInspector表示
        /// </summary>
        /// <param name="vehiclesDataModel"></param>
        public void OpenVehicleInspector(VehiclesDataModel vehiclesDataModel) {
            Inspector.Inspector.VehiclesView(vehiclesDataModel.id);
        }

        /// <summary>
        /// 乗り物削除
        /// </summary>
        /// <param name="vehiclesDataModel"></param>
        public void DeleteVehicleDataModel(VehiclesDataModel vehiclesDataModel) {
            _vehiclesDataModels.Remove(vehiclesDataModel);
            databaseManagementService.SaveCharacterVehicles(_vehiclesDataModels);
            Refresh();
        }

        /// <summary>
        /// 乗り物のコピー＆貼り付け処理
        /// </summary>
        /// <param name="vehiclesDataModel"></param>
        public void PasteVehicleDataModel(VehiclesDataModel vehiclesDataModel) {
            var newModel = VehiclesDataModel.CreateDefault();
            var uuid = newModel.id;
            newModel = vehiclesDataModel.DataClone();
            newModel.id = uuid;
            newModel.name = CreateDuplicateName(_vehiclesDataModels.Select(v => v.name).ToList(), newModel.name);
            _vehiclesDataModels.Add(newModel);
            databaseManagementService.SaveCharacterVehicles(_vehiclesDataModels);
            
            Refresh();
        }

        /// <summary>
        /// 職業データ作成
        /// </summary>
        public void CreateClassDataModel() {
            var newModel = ClassDataModel.CreateDefault(Guid.NewGuid().ToString(),
                "#" + string.Format("{0:D4}", _classDataModels.Count + 1) + "　" + 
                EditorLocalize.LocalizeText("WORD_1518"));
            newModel.abilityScore.maxHp.paramPeakLv = _classDataModels[0].clearLevel;
            newModel.abilityScore.maxMp.paramPeakLv = _classDataModels[0].clearLevel;
            newModel.abilityScore.attack.paramPeakLv = _classDataModels[0].clearLevel;
            newModel.abilityScore.defense.paramPeakLv = _classDataModels[0].clearLevel;
            newModel.abilityScore.magicAttack.paramPeakLv = _classDataModels[0].clearLevel;
            newModel.abilityScore.magicDefense.paramPeakLv = _classDataModels[0].clearLevel;
            newModel.abilityScore.speed.paramPeakLv = _classDataModels[0].clearLevel;
            newModel.abilityScore.luck.paramPeakLv = _classDataModels[0].clearLevel;

            //固定特徴
            newModel.traits.Add(new TraitCommonDataModel(2, 2, 0, 100));
            newModel.traits.Add(new TraitCommonDataModel(2, 2, 1, 5));
            newModel.traits.Add(new TraitCommonDataModel(2, 2, 2, 6));
            newModel.traits.Add(new TraitCommonDataModel(2, 3, 1, 100));

            _classDataModels.Add(newModel);
            databaseManagementService.SaveCharacterActorClass(_classDataModels);
            Refresh();
        }

        /// <summary>
        /// 職業のInspector表示
        /// </summary>
        /// <param name="classDataModel"></param>
        public void OpenClassInspector(ClassDataModel classDataModel) {
            if (classDataModel == null)
                Inspector.Inspector.JobCommonView();
            else
                Inspector.Inspector.ClassView(classDataModel.id, null);
        }

        /// <summary>
        /// 職業削除
        /// </summary>
        /// <param name="classDataModel"></param>
        public void DeleteClassDataModel(ClassDataModel classDataModel) {
            _classDataModels.Remove(classDataModel);
            databaseManagementService.SaveCharacterActorClass(_classDataModels);
            Refresh();
        }

        /// <summary>
        /// 職業のコピー＆ペースト
        /// </summary>
        /// <param name="classDataModel"></param>
        public void PasteClassDataModel(ClassDataModel classDataModel) {
            var uuid = Guid.NewGuid().ToString();
            var newModel = classDataModel.DataClone();
            newModel.id = uuid;
            newModel.basic.name =
                CreateDuplicateName(_classDataModels.Select(c => c.basic.name).ToList(), newModel.basic.name);

            _classDataModels.Add(newModel);
            databaseManagementService.SaveCharacterActorClass(_classDataModels);
            Refresh();
        }
    }
}