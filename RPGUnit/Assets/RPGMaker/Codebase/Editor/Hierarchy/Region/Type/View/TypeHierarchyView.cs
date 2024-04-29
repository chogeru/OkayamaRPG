using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Base.View.Component;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Type.View
{
    public class TypeHierarchyView : AbstractHierarchyView
    {
        // const
        //--------------------------------------------------------------------------------------------------------------
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Hierarchy/Region/Type/Asset/database_type.uxml"; } }

        // 状態
        //--------------------------------------------------------------------------------------------------------------
        //コピーしたデータの保持
        private int _attributeIndex;

        // UI要素
        //--------------------------------------------------------------------------------------------------------------
        private HierarchyItemListView _attributeTypeListView;
        private HierarchyItemListView _equipmentTypeListView;
        private int _equipmentIndex;
        private HierarchyItemListView _skillTypeListView;
        private int _skillIndex;
        private HierarchyItemListView _armorTypeListView;
        private int _armorIndex;
        private HierarchyItemListView _weaponTypeListView;
        private int _weaponIndex;
        private const int foldoutCount = 1;

        // 利用するデータ
        //--------------------------------------------------------------------------------------------------------------
        private SystemSettingDataModel _systemSettingDataModel;

        // ヒエラルキー本体
        //--------------------------------------------------------------------------------------------------------------
        private readonly TypeHierarchy         _typeHierarchy;


        //--------------------------------------------------------------------------------------------------------------
        //
        // methods
        //
        //--------------------------------------------------------------------------------------------------------------

        // 初期化・更新系
        //--------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="typeHierarchy"></param>
        public TypeHierarchyView(TypeHierarchy typeHierarchy) {
            _typeHierarchy = typeHierarchy;
            InitUI();
            InitEventHandlers();
        }

        /// <summary>
        /// 各コンテンツデータの初期化
        /// </summary>
        override protected void InitContentsData() {
            SetFoldout("element_type");
            _attributeTypeListView = new HierarchyItemListView(ViewName + "Element");
            ((VisualElement) UxmlElement.Query<VisualElement>("attribute_type_list")).Add(_attributeTypeListView);

            SetFoldout("skill_type");
            _skillTypeListView = new HierarchyItemListView(ViewName + "Skill");
            ((VisualElement) UxmlElement.Query<VisualElement>("skill_type_list")).Add(_skillTypeListView);

            SetFoldout("weapon_type");
            _weaponTypeListView = new HierarchyItemListView(ViewName + "Weapon");
            ((VisualElement) UxmlElement.Query<VisualElement>("weapon_type")).Add(_weaponTypeListView);

            SetFoldout("armor_type");
            _armorTypeListView = new HierarchyItemListView(ViewName + "Armor");
            ((VisualElement) UxmlElement.Query<VisualElement>("armor_type")).Add(_armorTypeListView);

            SetFoldout("equip_type");
            _equipmentTypeListView = new HierarchyItemListView(ViewName + "Equip");
            ((VisualElement) UxmlElement.Query<VisualElement>("equip_type")).Add(_equipmentTypeListView);

            //Foldoutの開閉状態保持用
            for (int i = 0; i < foldoutCount; i++)
                SetFoldout("foldout_" + (i + 1));
        }

        /// <summary>
        /// イベントの初期設定
        /// </summary>
        private void InitEventHandlers() {
            InitContextMenu(RegistrationLimit.LIMITED_99);
            var dic = new Dictionary<string, List<string>>
            {
                {
                    KeyNameElementType,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_1231"), EditorLocalize.LocalizeText("WORD_1232")
                    }
                },
                {
                    KeyNameSkillType,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_1244"), EditorLocalize.LocalizeText("WORD_1245")
                    }
                },
                {
                    KeyNameWeaponType,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_1250"), EditorLocalize.LocalizeText("WORD_1251")
                    }
                },
                {
                    KeyNameArmorType,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_1266"), EditorLocalize.LocalizeText("WORD_1267")
                    }
                },
                {
                    KeyNameEquipType,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_1276"), EditorLocalize.LocalizeText("WORD_1277")
                    }
                }
            };
            SetParentContextMenu(dic);
            
            // 属性
            _attributeTypeListView.SetEventHandler(
                //属性表示時、「なし」「通常攻撃」を除外して表示する(前から二つを非表示)
                (i, value) => { _typeHierarchy.OpenAttributeTypeInspector(_systemSettingDataModel.elements[i + 2]); },
                (i, value) =>
                {

                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameElementType, new ContextMenuData()
                            {
                                UuId = _systemSettingDataModel.elements[i + 2].id,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_1233"),
                                        EditorLocalize.LocalizeText("WORD_1234")
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.None
                            }
                        }
                    };
                    SetChildContextMenu(childDic);
                });

            // スキルタイプ
            _skillTypeListView.SetEventHandler(
                //属性表示時、「なし」「通常攻撃」を除外して表示する(前から二つを非表示)
                (i, value) => { _typeHierarchy.OpenSkillTypeInspector(_systemSettingDataModel.skillTypes[i]); },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameSkillType, new ContextMenuData()
                            {
                                UuId = _systemSettingDataModel.skillTypes[i].id,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_1246"),
                                        EditorLocalize.LocalizeText("WORD_1247")
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.One
                            }
                        }
                    };
                    SetChildContextMenu(childDic);
                });

            // 武器タイプ
            _weaponTypeListView.SetEventHandler(
                (i, value) => { _typeHierarchy.OpenWeaponTypeInspector(_systemSettingDataModel.weaponTypes[i]); },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameWeaponType, new ContextMenuData()
                            {
                                UuId = _systemSettingDataModel.weaponTypes[i].id,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_1252"),
                                        EditorLocalize.LocalizeText("WORD_1253")
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.One
                            }
                        }
                    };
                    SetChildContextMenu(childDic);
                });

            // 防具タイプ
            _armorTypeListView.SetEventHandler(
                //属性表示時、「なし」「通常攻撃」を除外して表示する(前から二つを非表示)
                (i, value) => { _typeHierarchy.OpenArmorTypeInspector(_systemSettingDataModel.armorTypes[i]); },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameArmorType, new ContextMenuData()
                            {
                                UuId = _systemSettingDataModel.armorTypes[i].id,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_1268"),
                                        EditorLocalize.LocalizeText("WORD_1269")
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.None
                            }
                        }
                    };
                    SetChildContextMenu(childDic);
                });

            // 装備タイプ
            _equipmentTypeListView.SetEventHandler(
                //属性表示時、「なし」「通常攻撃」を除外して表示する(前から二つを非表示)
                (i, value) => { _typeHierarchy.OpenEquipmentTypeInspector(_systemSettingDataModel.equipTypes[i]); },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameEquipType, new ContextMenuData()
                            {
                                UuId = _systemSettingDataModel.equipTypes[i].id,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_1278"),
                                        EditorLocalize.LocalizeText("WORD_1279")
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.Four
                            }
                        }
                    };
                    SetChildContextMenu(childDic);
                });
        }

        protected override VisualElement CreateDataModel(string keyName) {
            VisualElement visualElement = base.CreateDataModel(keyName);

            switch (keyName)
            {
                case KeyNameElementType:
                    _typeHierarchy.CreateAttributeType();
                    visualElement = LastAttributeTypeIndex();
                    break;
                case KeyNameSkillType:
                    _typeHierarchy.CreateSkillType();
                    visualElement = LastSkillTypeIndex();
                    break;
                case KeyNameWeaponType:
                    _typeHierarchy.CreateWeaponType();
                    visualElement = LastWeaponTypeIndex();
                    break;
                case KeyNameArmorType:
                    _typeHierarchy.CreateArmorType();
                    visualElement = LastArmorTypeIndex();
                    break;
                case KeyNameEquipType:
                    _typeHierarchy.CreateEquipmentType();
                    visualElement = LastEquipmentTypeIndex();
                    break;
            }

            return visualElement;
        }

        protected override VisualElement DuplicateDataModel(string keyName, string uuId) {
            VisualElement visualElement = base.DuplicateDataModel(keyName, uuId);
            switch (keyName)
            {
                case KeyNameElementType:
                    SystemSettingDataModel.Element element = _systemSettingDataModel.elements.FirstOrDefault(t => t.id == uuId);
                    if (element != null) _typeHierarchy.DuplicateAttributeType(element);
                    visualElement = LastAttributeTypeIndex();
                    break;
                case KeyNameSkillType:
                    SystemSettingDataModel.SkillType skillType = _systemSettingDataModel.skillTypes.FirstOrDefault(t => t.id == uuId);
                    if (skillType != null)
                        _typeHierarchy.DuplicateSkillType(skillType);
                    visualElement = LastSkillTypeIndex();
                    break;
                case KeyNameWeaponType:
                    SystemSettingDataModel.WeaponType weaponType = _systemSettingDataModel.weaponTypes.FirstOrDefault(t => t.id == uuId);
                    if (weaponType != null)
                        _typeHierarchy.DuplicateWeaponType(weaponType);

                    visualElement = LastWeaponTypeIndex();
                    break;
                case KeyNameArmorType:
                    SystemSettingDataModel.ArmorType armorType = _systemSettingDataModel.armorTypes.FirstOrDefault(t => t.id == uuId);
                    if (armorType != null)
                        _typeHierarchy.DuplicateArmorType(armorType);

                    visualElement = LastArmorTypeIndex();
                    break;
                case KeyNameEquipType:
                    SystemSettingDataModel.EquipType equipType = _systemSettingDataModel.equipTypes.FirstOrDefault(t => t.id == uuId);
                    if (equipType != null)
                        _typeHierarchy.DuplicateEquipmentType(equipType);

                    visualElement = LastEquipmentTypeIndex();
                    break;
            }
            return visualElement;
        }

        protected override VisualElement DeleteDataModel(string keyName, string uuId) {
            VisualElement visualElement = base.DeleteDataModel(keyName, uuId);
            int index = 0;
            List<VisualElement> elements;
            switch (keyName)
            {
                case KeyNameElementType:
                    SystemSettingDataModel.Element element = null;
                    index = 0;
                    for (int i = 0; i < _systemSettingDataModel.elements.Count; i++)
                    {
                        if (_systemSettingDataModel.elements[i].id == uuId)
                        {
                            element = _systemSettingDataModel.elements[i];
                            index = i;
                            break;
                        }
                    }
                    if (element != null) _typeHierarchy.DeleteAttributeType(element);
                    elements = new List<VisualElement>();
                    _attributeTypeListView.Query<Button>().ForEach(button => { elements.Add(button); });
                    visualElement = elements.Count < index
                        ? LastAttributeTypeIndex()
                        : elements.FirstOrDefault(e => e.name == "TypeHierarchyViewElement" + (index - 2));
                    break;
                case KeyNameSkillType:
                    SystemSettingDataModel.SkillType skillType = null;
                    index = 0;
                    for (int i = 0; i < _systemSettingDataModel.skillTypes.Count; i++)
                    {
                        if (_systemSettingDataModel.skillTypes[i].id == uuId)
                        {
                            skillType = _systemSettingDataModel.skillTypes[i];
                            index = i;
                            break;
                        }
                    }
                    if (skillType != null)
                        _typeHierarchy.DeleteSkillType(skillType);
                    elements = new List<VisualElement>();
                    _skillTypeListView.Query<Button>().ForEach(button => { elements.Add(button); });
                    visualElement = elements.Count - 1 < index
                        ? LastSkillTypeIndex()
                        : elements.FirstOrDefault(e => e.name == "TypeHierarchyViewSkill" + index);
                    break;
                case KeyNameWeaponType:
                    SystemSettingDataModel.WeaponType weaponType = null;
                    index = 0;
                    for (int i = 0; i < _systemSettingDataModel.weaponTypes.Count; i++)
                    {
                        if (_systemSettingDataModel.weaponTypes[i].id == uuId)
                        {
                            weaponType = _systemSettingDataModel.weaponTypes[i];
                            index = i;
                            break;
                        }
                    }
                    if (weaponType != null)
                        _typeHierarchy.DeleteWeaponType(weaponType);
                    elements = new List<VisualElement>();
                    _weaponTypeListView.Query<Button>().ForEach(button => { elements.Add(button); });
                    visualElement = elements.Count - 1 < index
                        ? LastWeaponTypeIndex()
                        : elements.FirstOrDefault(e => e.name == "TypeHierarchyViewWeapon" + index);
                    break;
                case KeyNameArmorType:
                    SystemSettingDataModel.ArmorType armorType = null;
                    index = 0;
                    for (int i = 0; i < _systemSettingDataModel.armorTypes.Count; i++)
                    {
                        if (_systemSettingDataModel.armorTypes[i].id == uuId)
                        {
                            armorType = _systemSettingDataModel.armorTypes[i];
                            index = i;
                            break;
                        }
                    }
                    if (armorType != null)
                        _typeHierarchy.DeleteArmorType(armorType);
                    elements = new List<VisualElement>();
                    _armorTypeListView.Query<Button>().ForEach(button => { elements.Add(button); });
                    visualElement = elements.Count - 1 < index
                        ? LastArmorTypeIndex()
                        : elements.FirstOrDefault(e => e.name == "TypeHierarchyViewArmor" + index);
                    break;
                case KeyNameEquipType:
                    SystemSettingDataModel.EquipType equipType = _systemSettingDataModel.equipTypes.FirstOrDefault(t => t.id == uuId);
                    index = 0;
                    for (int i = 0; i < _systemSettingDataModel.equipTypes.Count; i++)
                    {
                        if (_systemSettingDataModel.equipTypes[i].id == uuId)
                        {
                            equipType = _systemSettingDataModel.equipTypes[i];
                            index = i;
                            break;
                        }
                    }
                    if (equipType != null)
                        _typeHierarchy.DeleteEquipmentType(equipType);
                    elements = new List<VisualElement>();
                    _equipmentTypeListView.Query<Button>().ForEach(button => { elements.Add(button); });
                    visualElement = elements.Count - 1 < index
                        ? LastEquipmentTypeIndex()
                        : elements.FirstOrDefault(e => e.name == "TypeHierarchyViewEquip" + index);
                    break;
            }

            return visualElement;
        }

        /// <summary>
        /// データ更新
        /// </summary>
        /// <param name="systemSettingDataModel"></param>
        public void Refresh([CanBeNull] SystemSettingDataModel systemSettingDataModel = null) {
            _systemSettingDataModel = systemSettingDataModel ?? _systemSettingDataModel;
            base.Refresh();
        }

        /// <summary>
        /// データ更新
        /// </summary>
        protected override void RefreshContents() {
            base.RefreshContents();
            //属性表示時、「なし」「通常攻撃」を除外して表示する(前から二つを非表示)
            var elementViewList = new List<SystemSettingDataModel.Element>();
            for (var i = 2; i < _systemSettingDataModel.elements.Count; i++)
                elementViewList.Add(_systemSettingDataModel.elements[i]);


            _attributeTypeListView.Refresh(elementViewList.Select(item => item.value).ToList());
            _skillTypeListView.Refresh(_systemSettingDataModel.skillTypes.Select(item => item.value).ToList());
            _weaponTypeListView.Refresh(_systemSettingDataModel.weaponTypes.Select(item => item.value).ToList());
            _armorTypeListView.Refresh(_systemSettingDataModel.armorTypes.Select(item => item.name).ToList());
            _equipmentTypeListView.Refresh(_systemSettingDataModel.equipTypes.Select(item => item.name).ToList());
            
            NowDataCounts = new Dictionary<string, int>();
            NowDataCounts.Add(KeyNameElementType, _systemSettingDataModel.elements.Count);
            NowDataCounts.Add(KeyNameSkillType, _systemSettingDataModel.skillTypes.Count);
            NowDataCounts.Add(KeyNameWeaponType, _systemSettingDataModel.weaponTypes.Count);
            NowDataCounts.Add(KeyNameArmorType, _systemSettingDataModel.armorTypes.Count);
            NowDataCounts.Add(KeyNameEquipType, _systemSettingDataModel.equipTypes.Count);
        }

        /// <summary>
        /// 最終選択していた属性を返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastAttributeTypeIndex() {
            var elements = new List<VisualElement>();
            _attributeTypeListView.Query<Button>().ForEach(button => { elements.Add(button); });

            //選択可能なVisualElementが存在しない場合には、Inspectorを初期化して終了する
            if (elements.Count == 0)
            {
                Inspector.Inspector.Clear();
                return null;
            }

            return elements[elements.Count - 1];
        }

        /// <summary>
        /// 最終選択していたスキルタイプを返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastSkillTypeIndex() {
            var elements = new List<VisualElement>();
            _skillTypeListView.Query<Button>().ForEach(button => { elements.Add(button); });

            return elements[elements.Count - 1];
        }

        /// <summary>
        /// 最終選択していた武器タイプを返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastWeaponTypeIndex() {
            var elements = new List<VisualElement>();
            _weaponTypeListView.Query<Button>().ForEach(button => { elements.Add(button); });

            return elements[elements.Count - 1];
        }

        /// <summary>
        /// 最終選択していた防具タイプを返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastArmorTypeIndex() {
            var elements = new List<VisualElement>();
            _armorTypeListView.Query<Button>().ForEach(button => { elements.Add(button); });

            //選択可能なVisualElementが存在しない場合には、Inspectorを初期化して終了する
            if (elements.Count == 0)
            {
                Inspector.Inspector.Clear();
                return null;
            }

            return elements[elements.Count - 1];
        }

        /// <summary>
        /// 最終選択していた装備タイプを返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastEquipmentTypeIndex() {
            var elements = new List<VisualElement>();
            _equipmentTypeListView.Query<Button>().ForEach(button => { elements.Add(button); });

            return elements[elements.Count - 1];
        }
    }
}