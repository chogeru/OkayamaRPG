using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Equip.View;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Equip
{
    /// <summary>
    /// 装備のHierarchy
    /// </summary>
    public class EquipHierarchy : AbstractHierarchy
    {
        private List<WeaponDataModel> _weaponDataModels;
        private List<ArmorDataModel> _armorDataModels;
        private List<ItemDataModel> _itemDataModels;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EquipHierarchy() {
            View = new EquipHierarchyView(this);
        }

        /// <summary>
        /// View
        /// </summary>
        public EquipHierarchyView View { get; }

        /// <summary>
        /// データの読込
        /// </summary>
        override protected void LoadData() {
            base.LoadData();
            _weaponDataModels = databaseManagementService.LoadWeapon();
            _armorDataModels = databaseManagementService.LoadArmor();
            _itemDataModels = databaseManagementService.LoadItem();
        }

        /// <summary>
        /// Viewの更新
        /// </summary>
        protected override void UpdateView(string updateData = null) {
            base.UpdateView();
            View.Refresh(
                _weaponDataModels,
                _armorDataModels,
                _itemDataModels
            );
        }

        /// <summary>
        /// 武器のInspector表示
        /// </summary>
        /// <param name="weaponDataModel"></param>
        public void OpenWeaponInspector(WeaponDataModel weaponDataModel) {
            Inspector.Inspector.WeaponEditView(weaponDataModel);
        }

        /// <summary>
        /// 武器の新規作成
        /// </summary>
        public void CreateWeaponDataModel() {
            var newModel = WeaponDataModel.CreateDefault(Guid.NewGuid().ToString());
            newModel.basic.name = "#" + string.Format("{0:D4}", _weaponDataModels.Count + 1) + "　" + 
                                  EditorLocalize.LocalizeText("WORD_1518");
            
            var system = databaseManagementService.LoadSystem();
            //0番目は、武器に設定
            newModel.basic.equipmentTypeId = system.equipTypes[0].id;

            
            //特徴設定「攻撃時属性」「命中率」
            newModel.traits = new List<TraitCommonDataModel>()
            {
                new TraitCommonDataModel(3,1,2,0),
                new TraitCommonDataModel(2,2,0,0)
            };
            
            _weaponDataModels.Add(newModel);
            databaseManagementService.SaveWeapon(_weaponDataModels);

            Refresh();
        }

        /// <summary>
        /// 武器のコピー＆貼り付け処理
        /// </summary>
        /// <param name="weaponDataModel"></param>
        public void DuplicateWeaponDataModel(WeaponDataModel weaponDataModel) {
            var duplicated = weaponDataModel.DataClone();
            duplicated.basic.id = Guid.NewGuid().ToString();
            duplicated.basic.name = CreateDuplicateName(_weaponDataModels.Select(w => w.basic.name).ToList(),
                duplicated.basic.name);
            _weaponDataModels.Add(duplicated);
            databaseManagementService.SaveWeapon(_weaponDataModels);
            Refresh();
        }

        /// <summary>
        /// 武器の削除
        /// </summary>
        /// <param name="weaponDataModel"></param>
        public void DeleteWeaponDataModel(WeaponDataModel weaponDataModel) {
            _weaponDataModels.Remove(weaponDataModel);
            databaseManagementService.SaveWeapon(_weaponDataModels);
            Refresh();
        }

        /// <summary>
        /// 防具のInspector表示
        /// </summary>
        /// <param name="armorDataModel"></param>
        public void OpenArmorInspector(ArmorDataModel armorDataModel) {
            Inspector.Inspector.ArmorEditView(armorDataModel);
        }

        /// <summary>
        /// 防具の新規作成
        /// </summary>
        public void CreateArmorDataModel() {
            var newModel = ArmorDataModel.CreateDefault(Guid.NewGuid().ToString());
            newModel.basic.name = "#" + string.Format("{0:D4}", _armorDataModels.Count + 1) + "　" + 
                                  EditorLocalize.LocalizeText("WORD_1518");
            var system = databaseManagementService.LoadSystem();
            //先頭のデータを設定
            newModel.basic.armorTypeId = system.armorTypes[0].id;
            //0番目は、武器なので、1番目(盾)に設定
            newModel.basic.equipmentTypeId = system.equipTypes[1].id;
            
            //特徴設定「回避率」
            newModel.traits = new List<TraitCommonDataModel>()
            {
                new TraitCommonDataModel(2,2,1,0)
            };
            
            _armorDataModels.Add(newModel);
            databaseManagementService.SaveArmor(_armorDataModels);


            Refresh();
        }

        /// <summary>
        /// 防具のコピー＆貼り付け処理
        /// </summary>
        /// <param name="armorDataModel"></param>
        public void DuplicateArmorDataModel(ArmorDataModel armorDataModel) {
            var duplicated = armorDataModel.DataClone();
            duplicated.basic.id = Guid.NewGuid().ToString();
            duplicated.basic.name = CreateDuplicateName(_armorDataModels.Select(a => a.basic.name).ToList(),
                duplicated.basic.name);
                //duplicated.basic.name + " " + EditorLocalize.LocalizeText("WORD_1462");

            _armorDataModels.Add(duplicated);
            databaseManagementService.SaveArmor(_armorDataModels);
            Refresh();
        }

        /// <summary>
        /// 防具の削除
        /// </summary>
        /// <param name="armorDataModel"></param>
        public void DeleteArmorDataModel(ArmorDataModel armorDataModel) {
            _armorDataModels.Remove(armorDataModel);
            databaseManagementService.SaveArmor(_armorDataModels);
            Refresh();
        }

        /// <summary>
        /// アイテムのInspector表示
        /// </summary>
        /// <param name="itemDataModel"></param>
        public void OpenItemInspector(ItemDataModel itemDataModel) {
            Inspector.Inspector.ItemEditView(itemDataModel);
        }

        /// <summary>
        /// アイテムの新規作成
        /// </summary>
        public void CreateItemDataModel() {
            var newModel = ItemDataModel.CreateDefault(Guid.NewGuid().ToString());
            newModel.basic.name = "#" + string.Format("{0:D4}", _itemDataModels.Count + 1) + "　" + 
                                  EditorLocalize.LocalizeText("WORD_1518");

            //アイテムタイプは通常
            newModel.basic.itemType = 1;
            //範囲は味方の単体
            newModel.targetEffect.targetTeam = 2;
            newModel.targetEffect.targetRange = 0;
            
            _itemDataModels.Add(newModel);
            databaseManagementService.SaveItem(_itemDataModels);
            
            Refresh();
        }

        /// <summary>
        /// アイテムのコピー＆貼り付け処理
        /// </summary>
        /// <param name="itemDataModel"></param>
        public void DuplicateItemDataModel(ItemDataModel itemDataModel) {
            var duplicated = itemDataModel.DataClone();
            duplicated.basic.id = Guid.NewGuid().ToString();
            duplicated.basic.name =
                CreateDuplicateName(_itemDataModels.Select(i => i.basic.name).ToList(), duplicated.basic.name);
            _itemDataModels.Add(duplicated);
            databaseManagementService.SaveItem(_itemDataModels);
            Refresh();
        }

        /// <summary>
        /// アイテムの削除
        /// </summary>
        /// <param name="itemDataModel"></param>
        public void DeleteItemDataModel(ItemDataModel itemDataModel) {
            _itemDataModels.Remove(itemDataModel);
            databaseManagementService.SaveItem(_itemDataModels);
            Refresh();
        }
    }
}