using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Type.View;
using System;
using System.Linq;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Type
{
    /// <summary>
    /// タイプのHierarchy
    /// </summary>
    public class TypeHierarchy : AbstractHierarchy
    {
        private SystemSettingDataModel _systemSettingDataModel;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TypeHierarchy() {
            View = new TypeHierarchyView(this);
        }

        /// <summary>
        /// View
        /// </summary>
        public TypeHierarchyView View { get; }

        /// <summary>
        /// データの読込
        /// </summary>
        override protected void LoadData() {
            base.LoadData();
            _systemSettingDataModel = databaseManagementService.LoadSystem();
        }

        /// <summary>
        /// Viewの更新
        /// </summary>
        protected override void UpdateView(string updateData = null) {
            base.UpdateView();
            View.Refresh(_systemSettingDataModel);
        }

        /// <summary>
        /// 属性のInspector表示
        /// </summary>
        /// <param name="element"></param>
        public void OpenAttributeTypeInspector(SystemSettingDataModel.Element element) {
            Inspector.Inspector.AttributeTypeEditView(element);
        }

        /// <summary>
        /// 属性の新規作成
        /// </summary>
        public void CreateAttributeType() {
            var newModel = SystemSettingDataModel.Element.CreateDefault();
            newModel.value = "#" + string.Format("{0:D4}", _systemSettingDataModel.elements.Count + 1) + "　" + 
                             EditorLocalize.LocalizeText("WORD_1518");
            ;
            _systemSettingDataModel.elements.Add(newModel);
            databaseManagementService.SaveSystem(_systemSettingDataModel);


            Refresh();
        }

        /// <summary>
        /// 属性のコピー＆貼り付け処理
        /// </summary>
        /// <param name="element"></param>
        public void DuplicateAttributeType(SystemSettingDataModel.Element element) {
            var duplicated = element.DataClone();
            duplicated.id = Guid.NewGuid().ToString();
            duplicated.value = CreateDuplicateName(_systemSettingDataModel.elements.Select(e => e.value).ToList(),
                duplicated.value);
            _systemSettingDataModel.elements.Add(duplicated);
            databaseManagementService.SaveSystem(_systemSettingDataModel);
            Refresh();
        }

        /// <summary>
        /// 属性の削除
        /// </summary>
        /// <param name="element"></param>
        public void DeleteAttributeType(SystemSettingDataModel.Element element) {
            var classes = databaseManagementService.LoadCharacterActorClass();
            var items = databaseManagementService.LoadItem();
            var skillCustoms = databaseManagementService.LoadSkillCustom();
            var enemies = databaseManagementService.LoadEnemy();
            var actors = databaseManagementService.LoadCharacterActor();
            var weapons = databaseManagementService.LoadWeapon();
            var armors = databaseManagementService.LoadArmor();


            //属性のINDEXの取得
            var elementIndex = _systemSettingDataModel.elements.IndexOf(element);

            //クラスの属性に入っていた際に初期値に変更
            foreach (var classData in classes)
            {
                if (classData.element == element.id) classData.element = _systemSettingDataModel.elements[0].id;
                //特徴の削除
                for (var i = 0; i < classData.traits.Count; i++)
                    if (classData.traits[i].categoryId == 1 || classData.traits[i].categoryId == 3)
                        if (classData.traits[i].traitsId == 1)
                            if (classData.traits[i].effectId == elementIndex)
                            {
                                classData.traits.RemoveAt(i);
                                i--;
                            }
            }

            //アイテムの属性に入っていた際に初期値に変更
            foreach (var item in items)
            {
                //アイテムの属性は１つしか選べない為[0]
                //使用者
                if (item.userEffect.damage.elements.Count != 0 && item.userEffect.damage.elements[0] == elementIndex)
                    item.userEffect.damage.elements[0] = 0;
                //対象者
                if (item.targetEffect.damage.elements.Count != 0 && item.targetEffect.damage.elements[0] == elementIndex
                ) item.targetEffect.damage.elements[0] = 0;
            }

            //カスタムスキルの属性に入っていた際に初期値に変更
            foreach (var skillCustom in skillCustoms)
            {
                //カスタムスキルの属性は１つしか選べない為[0]
                //使用者
                if (skillCustom.userEffect.damage.elements.Count != 0 &&
                    skillCustom.userEffect.damage.elements[0] == elementIndex)
                    skillCustom.userEffect.damage.elements[0] = 0;
                //対象者
                if (skillCustom.targetEffect.damage.elements.Count != 0 &&
                    skillCustom.targetEffect.damage.elements[0] == elementIndex)
                    skillCustom.targetEffect.damage.elements[0] = 0;
            }

            //エネミーの属性に入っていた際に初期値に変更
            foreach (var enemy in enemies)
            {
                if (enemy.elements.Count != 0 && enemy.elements[0] == elementIndex) enemy.elements[0] = 0;
                //特徴の削除
                for (var i = 0; i < enemy.traits.Count; i++)
                    if (enemy.traits[i].categoryId == 1 || enemy.traits[i].categoryId == 3)
                        if (enemy.traits[i].traitsId == 1)
                            if (enemy.traits[i].effectId == elementIndex)
                            {
                                enemy.traits.RemoveAt(i);
                                i--;
                            }
            }

            //属性の優勢劣勢に入っていた際に初期値に変更
            foreach (var a in _systemSettingDataModel.elements)
            {
                for (var i = 0; i < _systemSettingDataModel.elements[i].advantageous.Count; i++)
                    if (_systemSettingDataModel.elements[i].advantageous.Count != 0 &&
                        _systemSettingDataModel.elements[i].advantageous[i].element == elementIndex)
                    {
                        _systemSettingDataModel.elements.RemoveAt(i);
                        i--;
                    }

                for (var i = 0; i < _systemSettingDataModel.elements[i].disadvantage.Count; i++)
                    if (_systemSettingDataModel.elements[i].disadvantage.Count != 0 &&
                        _systemSettingDataModel.elements[i].disadvantage[i].element == elementIndex)
                    {
                        _systemSettingDataModel.elements.RemoveAt(i);
                        i--;
                    }
            }

            //アクターの特徴にその属性があったら削除
            foreach (var actor in actors)
                //アクターに絞り込み
                if (actor.charaType == (int) ActorTypeEnum.ACTOR)
                    for (var i = 0; i < actor.traits.Count; i++)
                        if (actor.traits[i].categoryId == 1 || actor.traits[i].categoryId == 3)
                            if (actor.traits[i].traitsId == 1)
                                if (actor.traits[i].effectId == elementIndex)
                                {
                                    actor.traits.RemoveAt(i);
                                    i--;
                                }

            //武器の特性にその属性があったら削除を実施
            foreach (var weapon in weapons)
                for (var i = 0; i < weapon.traits.Count; i++)
                    if (weapon.traits[i].categoryId == 1 || weapon.traits[i].categoryId == 3)
                        if (weapon.traits[i].traitsId == 1)
                            if (weapon.traits[i].effectId == elementIndex)
                            {
                                weapon.traits.RemoveAt(i);
                                i--;
                            }

            //防具の特性にその属性があったら削除を実施
            foreach (var armor in armors)
                for (var i = 0; i < armor.traits.Count; i++)
                    if (armor.traits[i].categoryId == 1 || armor.traits[i].categoryId == 3)
                        if (armor.traits[i].traitsId == 1)
                            if (armor.traits[i].effectId == elementIndex)
                            {
                                armor.traits.RemoveAt(i);
                                i--;
                            }

            //データの削除
            databaseManagementService.SaveCharacterActorClass(classes);
            databaseManagementService.SaveItem(items);
            databaseManagementService.SaveSkillCustom(skillCustoms);
            databaseManagementService.SaveEnemy(enemies);
            databaseManagementService.SaveCharacterActor(actors);
            databaseManagementService.SaveWeapon(weapons);
            databaseManagementService.SaveArmor(armors);

            _systemSettingDataModel.elements.Remove(element);
            databaseManagementService.SaveSystem(_systemSettingDataModel);
            Refresh();
        }

        /// <summary>
        /// スキルタイプのInspector表示
        /// </summary>
        /// <param name="skillType"></param>
        public void OpenSkillTypeInspector(SystemSettingDataModel.SkillType skillType) {
            Inspector.Inspector.SkillTypeEditView(skillType);
        }

        /// <summary>
        /// スキルタイプの新規作成
        /// </summary>
        public void CreateSkillType() {
            var newModel = SystemSettingDataModel.SkillType.CreateDefault();
            newModel.value = "#" + string.Format("{0:D4}", _systemSettingDataModel.skillTypes.Count + 1) + "　" + 
                             EditorLocalize.LocalizeText("WORD_1518");
            _systemSettingDataModel.skillTypes.Add(newModel);
            databaseManagementService.SaveSystem(_systemSettingDataModel);

            Refresh();
        }

        /// <summary>
        /// スキルタイプのコピー＆貼り付け処理
        /// </summary>
        /// <param name="skillType"></param>
        public void DuplicateSkillType(SystemSettingDataModel.SkillType skillType) {
            var duplicated = skillType.DataClone();
            duplicated.id = Guid.NewGuid().ToString();
            duplicated.value = CreateDuplicateName(_systemSettingDataModel.skillTypes.Select(s => s.value).ToList(),
                duplicated.value);
            _systemSettingDataModel.skillTypes.Add(duplicated);
            databaseManagementService.SaveSystem(_systemSettingDataModel);
            Refresh();
        }

        /// <summary>
        /// スキルタイプの削除
        /// </summary>
        /// <param name="skillType"></param>
        public void DeleteSkillType(SystemSettingDataModel.SkillType skillType) {
            _systemSettingDataModel.skillTypes.Remove(skillType);
            databaseManagementService.SaveSystem(_systemSettingDataModel);
            Refresh();
        }

        /// <summary>
        /// 武器タイプのInspector表示
        /// </summary>
        /// <param name="weaponType"></param>
        public void OpenWeaponTypeInspector(SystemSettingDataModel.WeaponType weaponType) {
            Inspector.Inspector.WeaponTypeEditView(weaponType);
        }

        /// <summary>
        /// 武器タイプの新規作成
        /// </summary>
        public void CreateWeaponType() {
            var newModel = SystemSettingDataModel.WeaponType.CreateDefault();
            newModel.value = "#" + string.Format("{0:D4}", _systemSettingDataModel.weaponTypes.Count + 1) + "　" + 
                             EditorLocalize.LocalizeText("WORD_1518");

            _systemSettingDataModel.weaponTypes.Add(newModel);
            databaseManagementService.SaveSystem(_systemSettingDataModel);

            Refresh();
        }

        /// <summary>
        /// 武器タイプのコピー＆貼り付け処理
        /// </summary>
        /// <param name="weaponType"></param>
        public void DuplicateWeaponType(SystemSettingDataModel.WeaponType weaponType) {
            var duplicated = weaponType.DataClone();
            duplicated.id = Guid.NewGuid().ToString();
            duplicated.value = CreateDuplicateName(_systemSettingDataModel.weaponTypes.Select(w => w.value).ToList(),
                duplicated.value);
            _systemSettingDataModel.weaponTypes.Add(duplicated);
            databaseManagementService.SaveSystem(_systemSettingDataModel);
            Refresh();
        }

        /// <summary>
        /// 武器タイプの削除
        /// </summary>
        /// <param name="weaponType"></param>
        public void DeleteWeaponType(SystemSettingDataModel.WeaponType weaponType) {
            var weapons = databaseManagementService.LoadWeapon();
            var classes = databaseManagementService.LoadCharacterActorClass();
            var actors = databaseManagementService.LoadCharacterActor();

            //武器のウェポンタイプに入っていた際に初期値を入れる
            foreach (var weapon in weapons)
                if (weapon.basic.weaponTypeId == weaponType.id)
                {
                    weapon.basic.weaponTypeId = _systemSettingDataModel.weaponTypes[0].id;

                    //アクターがその武器を持っていた際に取り上げる
                    foreach (var actor in actors)
                        //アクターに絞り込み
                        if (actor.charaType == (int) ActorTypeEnum.ACTOR)
                            for (var i = 0; i < actor.equips.Count; i++)
                                if (actor.equips[i].value == weapon.basic.id)
                                    actor.equips[i].value = "";
                }

            //クラスのウェポンタイプに入っていた際に消す
            foreach (var classData in classes)
                for (var i = 0; i < classData.weaponTypes.Count; i++)
                    if (classData.weaponTypes[i] == weaponType.id)
                    {
                        classData.weaponTypes.RemoveAt(i);
                        i--;
                    }

            //データの削除
            databaseManagementService.SaveWeapon(weapons);
            databaseManagementService.SaveCharacterActorClass(classes);
            databaseManagementService.SaveCharacterActor(actors);

            _systemSettingDataModel.weaponTypes.Remove(weaponType);
            databaseManagementService.SaveSystem(_systemSettingDataModel);
            Refresh();
        }

        /// <summary>
        /// 防具タイプのInspector表示
        /// </summary>
        /// <param name="armorType"></param>
        public void OpenArmorTypeInspector(SystemSettingDataModel.ArmorType armorType) {
            Inspector.Inspector.ArmorTypeEditView(armorType);
        }

        /// <summary>
        /// 防具タイプの新規作成
        /// </summary>
        public void CreateArmorType() {
            var newModel = SystemSettingDataModel.ArmorType.CreateDefault();
            newModel.name = "#" + string.Format("{0:D4}", _systemSettingDataModel.armorTypes.Count + 1) + "　" + 
                            EditorLocalize.LocalizeText("WORD_1518");

            _systemSettingDataModel.armorTypes.Add(newModel);
            databaseManagementService.SaveSystem(_systemSettingDataModel);

            Refresh();
        }

        /// <summary>
        /// 防具タイプのコピー＆貼り付け処理
        /// </summary>
        /// <param name="armorType"></param>
        public void DuplicateArmorType(SystemSettingDataModel.ArmorType armorType) {
            var duplicated = armorType.DataClone();
            duplicated.id = Guid.NewGuid().ToString();
            duplicated.name = CreateDuplicateName(_systemSettingDataModel.armorTypes.Select(a => a.name).ToList(),
                duplicated.name);
            _systemSettingDataModel.armorTypes.Add(duplicated);
            databaseManagementService.SaveSystem(_systemSettingDataModel);
            Refresh();
        }

        /// <summary>
        /// 防具タイプの削除
        /// </summary>
        /// <param name="armorType"></param>
        public void DeleteArmorType(SystemSettingDataModel.ArmorType armorType) {
            var armors = databaseManagementService.LoadArmor();
            var classes = databaseManagementService.LoadCharacterActorClass();
            var actors = databaseManagementService.LoadCharacterActor();

            //防具のアーマータイプに入っていた際に初期値を入れる
            foreach (var armor in armors)
                if (armor.basic.armorTypeId == armorType.id)
                {
                    armor.basic.armorTypeId = _systemSettingDataModel.armorTypes[0].id;

                    //アクターがその防具を持っていた際に取り上げる
                    foreach (var actor in actors)
                        //アクターに絞り込み
                        if (actor.charaType == (int) ActorTypeEnum.ACTOR)
                            for (var i = 0; i < actor.equips.Count; i++)
                                if (actor.equips[i].value == armor.basic.id)
                                    actor.equips[i].value = "";
                }

            //クラスのアーマータイプに入っていた際に消す
            foreach (var classData in classes)
                for (var i = 0; i < classData.armorTypes.Count; i++)
                    if (classData.armorTypes[i] == armorType.id)
                    {
                        classData.armorTypes.RemoveAt(i);
                        i--;
                    }

            //データの削除
            databaseManagementService.SaveArmor(armors);
            databaseManagementService.SaveCharacterActorClass(classes);
            databaseManagementService.SaveCharacterActor(actors);

            _systemSettingDataModel.armorTypes.Remove(armorType);
            databaseManagementService.SaveSystem(_systemSettingDataModel);
            Refresh();
        }

        /// <summary>
        /// 装備タイプのInspector表示
        /// </summary>
        /// <param name="equipmentType"></param>
        public void OpenEquipmentTypeInspector(SystemSettingDataModel.EquipType equipmentType) {
            Inspector.Inspector.EquipmentTypeEditView(equipmentType);
        }

        /// <summary>
        /// 装備タイプの新規作成
        /// </summary>
        public void CreateEquipmentType() {
            var newModel = SystemSettingDataModel.EquipType.CreateDefault();
            newModel.name = "#" + string.Format("{0:D4}", _systemSettingDataModel.equipTypes.Count + 1) + "　" + 
                            EditorLocalize.LocalizeText("WORD_1518");

            _systemSettingDataModel.equipTypes.Add(newModel);
            databaseManagementService.SaveSystem(_systemSettingDataModel);

            //Inspector側への反映
            //Inspector.Inspector.Refresh();
            //マップエディタ
            //MapEditor.MapEditor.EventRefresh(true);

            Refresh();
            //Hierarchy.InvokeSelectableElementAction(View.LastEquipmentTypeIndex());
        }

        /// <summary>
        /// 装備タイプのコピー＆貼り付け処理
        /// </summary>
        /// <param name="equipmentType"></param>
        public void DuplicateEquipmentType(SystemSettingDataModel.EquipType equipmentType) {
            var duplicated = equipmentType.DataClone();
            duplicated.id = Guid.NewGuid().ToString();
            duplicated.name = CreateDuplicateName(_systemSettingDataModel.equipTypes.Select(e => e.name).ToList(),
                duplicated.name);
            _systemSettingDataModel.equipTypes.Add(duplicated);
            databaseManagementService.SaveSystem(_systemSettingDataModel);
            Refresh();
        }

        /// <summary>
        /// 装備タイプの削除
        /// </summary>
        /// <param name="equipmentType"></param>
        public void DeleteEquipmentType(SystemSettingDataModel.EquipType equipmentType) {
            _systemSettingDataModel.equipTypes.Remove(equipmentType);
            databaseManagementService.SaveSystem(_systemSettingDataModel);
            Refresh();
        }
    }
}