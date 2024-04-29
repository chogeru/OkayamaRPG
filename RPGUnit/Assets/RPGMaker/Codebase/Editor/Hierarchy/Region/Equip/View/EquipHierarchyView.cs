using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Base.View.Component;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Equip.View
{
    /// <summary>
    /// 装備のpHierarchyView
    /// </summary>
    public class EquipHierarchyView : AbstractHierarchyView
    {
        // const
        //--------------------------------------------------------------------------------------------------------------
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Hierarchy/Region/Equip/Asset/database_equip.uxml"; } }

        // ヒエラルキー本体クラス
        //--------------------------------------------------------------------------------------------------------------
        private readonly EquipHierarchy        _equipHierarchy;

        // 利用するデータ
        //--------------------------------------------------------------------------------------------------------------
        private List<WeaponDataModel> _weaponDataModels;

        // 状態
        //--------------------------------------------------------------------------------------------------------------

        // UI要素
        //--------------------------------------------------------------------------------------------------------------
        private HierarchyItemListView _weaponListView;
        private List<ArmorDataModel> _armorDataModels;
        private HierarchyItemListView _armorListView;
        private List<ItemDataModel> _itemDataModels;
        private HierarchyItemListView _itemListView;
        private const int foldoutCount = 1;

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
        /// <param name="equipHierarchy"></param>
        public EquipHierarchyView(EquipHierarchy equipHierarchy) {
            _equipHierarchy = equipHierarchy;
            InitUI();
        }

        /// <summary>
        /// 各コンテンツデータの初期化
        /// </summary>
        override protected void InitContentsData() {
            SetFoldout("weaponFoldout");
            _weaponListView = new HierarchyItemListView(ViewName + "Weapon");
            ((VisualElement) UxmlElement.Query<VisualElement>("weapon_item_list")).Add(_weaponListView);

            SetFoldout("armorFoldout");
            _armorListView = new HierarchyItemListView(ViewName + "Armor");
            ((VisualElement) UxmlElement.Query<VisualElement>("armor_item_list")).Add(_armorListView);

            SetFoldout("itemFoldout");
            _itemListView = new HierarchyItemListView(ViewName + "Item");
            ((VisualElement) UxmlElement.Query<VisualElement>("item_list")).Add(_itemListView);

            //Foldoutの開閉状態保持用
            for (int i = 0; i < foldoutCount; i++)
                SetFoldout("foldout_" + (i + 1));

            InitEventHandlers();
        }

        /// <summary>
        /// イベントの初期設定
        /// </summary>
        private void InitEventHandlers() {
            InitContextMenu(RegistrationLimit.None);
            var dic = new Dictionary<string, List<string>>
            {
                {
                    KeyNameWeapon,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_0512"), EditorLocalize.LocalizeText("WORD_0513")
                    }
                },
                {
                    KeyNameArmor,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_0516"), EditorLocalize.LocalizeText("WORD_0517")
                    }
                },
                {
                    KeyNameItem,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_0520"), EditorLocalize.LocalizeText("WORD_0521")
                    }
                },
            };
            SetParentContextMenu(dic);

            // 武器リストアイテムクリック時
            _weaponListView.SetEventHandler(
                (i, value) => { _equipHierarchy.OpenWeaponInspector(_weaponDataModels[i]); },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameWeapon, new ContextMenuData()
                            {
                                UuId = _weaponDataModels[i].basic.id,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_0514"),
                                        EditorLocalize.LocalizeText("WORD_0515")
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.None
                            }
                        }
                    };
                    SetChildContextMenu(childDic);
                });
            // 防具リストアイテムクリック時
            _armorListView.SetEventHandler(
                (i, value) => { _equipHierarchy.OpenArmorInspector(_armorDataModels[i]); },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameArmor, new ContextMenuData()
                            {
                                UuId = _armorDataModels[i].basic.id,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_0518"),
                                        EditorLocalize.LocalizeText("WORD_0519")
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.None
                            }
                        }
                    };
                    SetChildContextMenu(childDic);

                });

            // アイテムリストアイテムクリック時
            _itemListView.SetEventHandler(
                (i, value) => { _equipHierarchy.OpenItemInspector(_itemDataModels[i]); },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameItem, new ContextMenuData()
                            {
                                UuId = _itemDataModels[i].basic.id,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_0522"),
                                        EditorLocalize.LocalizeText("WORD_0523")
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.None
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
                case KeyNameWeapon:
                    _equipHierarchy.CreateWeaponDataModel();
                    visualElement = LastWeaponIndex();
                    break;
                case KeyNameArmor:
                    _equipHierarchy.CreateArmorDataModel();
                    visualElement = LastArmorIndex();
                    break;
                case KeyNameItem:
                    _equipHierarchy.CreateItemDataModel();
                    visualElement = LastItemIndex();
                    break;
            }

            return visualElement;
        }

        protected override VisualElement DuplicateDataModel(string keyName, string uuId) {
            VisualElement visualElement = base.DuplicateDataModel(keyName, uuId);
            switch (keyName)
            {
                case KeyNameWeapon:
                    var weaponDataModel = _weaponDataModels.FirstOrDefault(w => w.basic.id == uuId);
                    _equipHierarchy.DuplicateWeaponDataModel(weaponDataModel);
                    visualElement = LastWeaponIndex();
                    break;
                case KeyNameArmor:
                    var armorDataModel = _armorDataModels.FirstOrDefault(a => a.basic.id == uuId);
                    _equipHierarchy.DuplicateArmorDataModel(armorDataModel);
                    visualElement = LastArmorIndex();
                    break;
                case KeyNameItem:
                    var itemDataModel = _itemDataModels.FirstOrDefault(i => i.basic.id == uuId);
                    _equipHierarchy.DuplicateItemDataModel(itemDataModel);
                    visualElement = LastItemIndex();
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
                case KeyNameWeapon:
                    WeaponDataModel weaponDataModel = null;
                    index = 0;
                    for (int i = 0; i < _weaponDataModels.Count; i++)
                    {
                        if (_weaponDataModels[i].basic.id == uuId)
                        {
                            weaponDataModel = _weaponDataModels[i];
                            index = i;
                            break;
                        }
                    }

                    _equipHierarchy.DeleteWeaponDataModel(weaponDataModel);
                    elements = new List<VisualElement>();
                    _weaponListView.Query<Button>().ForEach(button => { elements.Add(button); });
                    visualElement = elements.Count - 1 < index
                        ? LastWeaponIndex()
                        : elements.FirstOrDefault(e => e.name == "EquipHierarchyViewWeapon" + index);
                    break;
                case KeyNameArmor:
                    ArmorDataModel armorDataModel = null;
                    index = 0;
                    for (int i = 0; i < _armorDataModels.Count; i++)
                    {
                        if (_armorDataModels[i].basic.id == uuId)
                        {
                            armorDataModel = _armorDataModels[i];
                            index = i;
                            break;
                        }
                    }

                    _equipHierarchy.DeleteArmorDataModel(armorDataModel);
                    elements = new List<VisualElement>();
                    _armorListView.Query<Button>().ForEach(button => { elements.Add(button); });
                    visualElement = elements.Count - 1 < index
                        ? LastArmorIndex()
                        : elements.FirstOrDefault(e => e.name == "EquipHierarchyViewArmor" + index);
                    break;
                case KeyNameItem:
                    ItemDataModel itemDataModel = null;
                    index = 0;
                    for (int i = 0; i < _itemDataModels.Count; i++)
                    {
                        if (_itemDataModels[i].basic.id == uuId)
                        {
                            itemDataModel = _itemDataModels[i];
                            index = i;
                            break;
                        }
                    }
                    _equipHierarchy.DeleteItemDataModel(itemDataModel);
                    elements = new List<VisualElement>();
                    _itemListView.Query<Button>().ForEach(button => { elements.Add(button); });
                    visualElement = elements.Count - 1 < index
                        ? LastItemIndex()
                        : elements.FirstOrDefault(e => e.name == "EquipHierarchyViewItem" + index);
                    break;
            }

            return visualElement;
        }

        /// <summary>
        /// データ更新
        /// </summary>
        /// <param name="weaponDataModels"></param>
        /// <param name="armorDataModels"></param>
        /// <param name="itemDataModels"></param>
        public void Refresh(
            [CanBeNull] List<WeaponDataModel> weaponDataModels = null,
            [CanBeNull] List<ArmorDataModel> armorDataModels = null,
            [CanBeNull] List<ItemDataModel> itemDataModels = null
        ) {
            _weaponDataModels = weaponDataModels ?? _weaponDataModels;
            _armorDataModels = armorDataModels ?? _armorDataModels;
            _itemDataModels = itemDataModels ?? _itemDataModels;
            base.Refresh();
        }

        /// <summary>
        /// データ更新
        /// </summary>
        protected override void RefreshContents() {
            base.RefreshContents();
            _weaponListView.Refresh(_weaponDataModels.Select(item => item.basic.name).ToList());
            _armorListView.Refresh(_armorDataModels.Select(item => item.basic.name).ToList());
            _itemListView.Refresh(_itemDataModels.Select(item => item.basic.name).ToList());
        }

        /// <summary>
        /// 最終選択していた武器を返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastWeaponIndex() {
            var elements = new List<VisualElement>();
            _weaponListView.Query<Button>().ForEach(button => { elements.Add(button); });

            //選択可能なVisualElementが存在しない場合には、Inspectorを初期化して終了する
            if (elements.Count == 0)
            {
                Inspector.Inspector.Clear();
                (WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.DatabaseSceneWindow) as DatabaseEditor.Window.SceneWindow)?.Clear();
                return null;
            }

            return elements[elements.Count - 1];
        }

        /// <summary>
        /// 最終選択していた防具を返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastArmorIndex() {
            var elements = new List<VisualElement>();
            _armorListView.Query<Button>().ForEach(button => { elements.Add(button); });

            //選択可能なVisualElementが存在しない場合には、Inspectorを初期化して終了する
            if (elements.Count == 0)
            {
                Inspector.Inspector.Clear();
                return null;
            }

            return elements[elements.Count - 1];
        }

        /// <summary>
        /// 最終選択していたアイテムを返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastItemIndex() {
            var elements = new List<VisualElement>();
            _itemListView.Query<Button>().ForEach(button => { elements.Add(button); });

            //選択可能なVisualElementが存在しない場合には、Inspectorを初期化して終了する
            if (elements.Count == 0)
            {
                Inspector.Inspector.Clear();
                return null;
            }

            return elements[elements.Count - 1];
        }
    }
}