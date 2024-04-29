using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Scene
{
    public class ShopProcess : AbstractCommandEditor
    {
        //イベントコマンドのUxml
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_shop_process.uxml";

        //追加されるアイテムのuxml
        private const string ItemUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Component/Canvas/QuickEvent/ModalWindow/Uxml/quickevent_tool_shop_items.uxml";

        //カテゴリーのプルダウンの横幅
        private const int _categoryWidth = 70;

        //アイテム名のプルダウンの横幅
        private const int                  _itemNameWidth = 90;
        private       Button               _addButton;
        private       List<ArmorDataModel> _armorDataModels;

        //データ保持用
        private DatabaseManagementService _databaseManagementService;
        private Toggle                    _isBuy;
        private List<ItemDataModel>       _itemDataModels;

        //returnする値
        private readonly List<object> _itemList = new List<object>();

        //表示要素
        private VisualElement         _items;
        private List<WeaponDataModel> _weaponDataModels;

        public ShopProcess(
            VisualElement rootElement,
            List<EventDataModel> eventDataModels,
            int eventIndex,
            int eventCommandIndex
        )
            : base(rootElement, eventDataModels, eventIndex, eventCommandIndex) {
        }

        public override void Invoke() {
            //イベントコマンド
            var commandTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SettingUxml);
            VisualElement commandFromUxml = commandTree.CloneTree();
            EditorLocalize.LocalizeElements(commandFromUxml);
            commandFromUxml.style.flexGrow = 1;
            RootElement.Add(commandFromUxml);

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count == 0)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            //データの取得
            _databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
            _itemDataModels = _databaseManagementService.LoadItem();
            _weaponDataModels = _databaseManagementService.LoadWeapon();
            _armorDataModels = _databaseManagementService.LoadArmor();

            //道具屋に並ぶアイテム部分
            _items = commandFromUxml.Query<VisualElement>("item_list");
            _addButton = commandFromUxml.Query<Button>("add");

            if (_itemDataModels.Count == 0 && _weaponDataModels.Count == 0 && _armorDataModels.Count == 0)
            {
                _addButton.SetEnabled(false);
            }
            
            _addButton.clicked += () =>
            {
                //表示の追加
                VisualElement itemLabelFromUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ItemUxml).CloneTree();
                EditorLocalize.LocalizeElements(itemLabelFromUxml);

                var category = "0";
                var id = "0";
                //アイテムがあったらアイテムを設定
                if (_itemDataModels.Count != 0)
                {
                    category = "0";
                    id = _itemDataModels[0].basic.id;
                }
                //武器があったらアイテムを設定
                else if (_weaponDataModels.Count != 0)
                {
                    category = "1";
                    id = _weaponDataModels[0].basic.id;
                }
                //アイテムがあったらアイテムを設定
                else if (_armorDataModels.Count != 0)
                {
                    category = "2";
                    id = _armorDataModels[0].basic.id;
                }

                ItemPage(itemLabelFromUxml, category, id);
            };

            //購入のみ
            Toggle buyToggle = RootElement.Query<Toggle>("buyToggle");
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] == "1")
                buyToggle.value = true;
            buyToggle.RegisterValueChangedCallback(o =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] =
                    buyToggle.value ? "1" : "0";
                Save(EventDataModels[EventIndex]);
                Save();
            });

            //既にあるアイテムの追加
            for (var i = EventCommandIndex + 1; i < EventDataModels[EventIndex].eventCommands.Count; i++)
                if (EventDataModels[EventIndex].eventCommands[i].code ==
                    (int) EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG_LINE)
                {
                    VisualElement itemLabelFromUxml =
                        AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ItemUxml).CloneTree();
                    EditorLocalize.LocalizeElements(itemLabelFromUxml);

                    ItemPage(itemLabelFromUxml,
                        EventDataModels[EventIndex].eventCommands[i].parameters[0],
                        EventDataModels[EventIndex].eventCommands[i].parameters[1],
                        EventDataModels[EventIndex].eventCommands[i].parameters[2],
                        EventDataModels[EventIndex].eventCommands[i].parameters[3]
                    );
                }
                else
                {
                    break;
                }
        }

        //アイテムの追加uxmlの処理
        //引数uxml そのデータ
        //表示させるアイテムの情報
        ////category.アイテムカテゴリー itemId.アイテムID priceSwitch.標準か指定か priceValue.指定の値段
        private void ItemPage(
            VisualElement Items,
            string category = "0",
            string itemId = "0",
            string priceSwitch = "0",
            string priceValue = "0"
        ) {
            //表示の追加を実施
            _items.Add(Items);
            //返信用のListにデータの追加を実施
            //データの追加(初期値は全部「0」)
            //0.アイテムカテゴリー 1.アイテムID 2.標準か指定か 3.指定の値段
            var data = new List<string>
            {
                category, itemId, priceSwitch, priceValue
            };

            //リストへ追加
            _itemList.Add(data);

            //アイテム名前リスト
            var itemTextList = new List<string>();
            //アイテムIDリスト
            var itemIdList = new List<string>();

            var selectID = -1;
            switch (int.Parse(data[0]))
            {
                case 0:
                    for (int i = 0; i < _itemDataModels.Count; i++)
                    {
                        itemTextList.Add(_itemDataModels[i].basic.name);
                        itemIdList.Add(_itemDataModels[i].basic.id);
                        if (_itemDataModels[i].basic.id == data[1])
                            selectID = i;
                    }

                    break;
                case 1:
                    for (int i = 0; i < _weaponDataModels.Count; i++)
                    {
                        itemTextList.Add(_weaponDataModels[i].basic.name);
                        itemIdList.Add(_weaponDataModels[i].basic.id);
                        if (_weaponDataModels[i].basic.id == data[1])
                            selectID = i;
                    }

                    break;
                case 2:
                    for (int i = 0; i < _armorDataModels.Count; i++)
                    {
                        itemTextList.Add(_armorDataModels[i].basic.name);
                        itemIdList.Add(_armorDataModels[i].basic.id);
                        if (_armorDataModels[i].basic.id == data[1])
                            selectID = i;
                    }

                    break;
            }

            int parse;
            if (selectID == -1)
            {
                if (int.TryParse(data[1], out parse) && itemTextList.Count > int.Parse(data[1]))
                    selectID = int.Parse(data[1]);
                else
                    selectID = 0;
            }

            var itemListPopupField = new PopupFieldBase<string>(itemTextList, selectID);
            VisualElement itemList = Items.Query<VisualElement>("itemList");


            VisualElement classification = Items.Query<VisualElement>("classification");
            classification.style.width = _categoryWidth;
            var classificationList = EditorLocalize.LocalizeTexts(new List<string> {"WORD_0068", "WORD_0128", "WORD_0129"});
            
            var classificationPopupField = new PopupFieldBase<string>(classificationList, int.Parse(data[0]));
            classification.Clear();
            classification.Add(classificationPopupField);
            classificationPopupField.RegisterValueChangedCallback(evt =>
            {
                data[0] = classificationPopupField.index.ToString();
                itemTextList.Clear();
                itemIdList.Clear();
                switch (classificationList.IndexOf(classificationPopupField.value))
                {
                    case 0:
                        foreach (var itemdata in _itemDataModels)
                        {
                            itemTextList.Add(itemdata.basic.name);
                            itemIdList.Add(itemdata.basic.id);
                        }

                        data[1] = _itemDataModels.Count > 0 ? _itemDataModels[0].basic.id : "";

                        break;
                    case 1:
                        foreach (var itemdata in _weaponDataModels)
                        {
                            itemTextList.Add(itemdata.basic.name);
                            itemIdList.Add(itemdata.basic.id);
                        }

                        data[1] = _weaponDataModels.Count > 0 ? _weaponDataModels[0].basic.id : "";

                        break;
                    case 2:
                        foreach (var armordata in _armorDataModels)
                        {
                            itemTextList.Add(armordata.basic.name);
                            itemIdList.Add(armordata.basic.id);
                        }

                        data[1] = _armorDataModels.Count > 0 ? _armorDataModels[0].basic.id : "";
                        break;
                }

                itemListPopupField = new PopupFieldBase<string>(itemTextList, selectID);
                itemList.Clear();
                itemList.Add(itemListPopupField);
                itemListPopupField.RegisterValueChangedCallback(evt =>
                {
                    data[1] = itemIdList[itemListPopupField.index];
                    Save();
                });
                Save();
            });

            // アイテムIDからindexを取得
            //int itemIndex = 0;
            switch (classificationList.IndexOf(classificationPopupField.value))
            {
                case 0:
                    // 値が数値の場合
                    if (int.TryParse(data[1], out parse) && _itemDataModels.Count > int.Parse(data[1]))
                        itemListPopupField.index = int.Parse(data[1]);
                    // 値がID
                    else
                        for (var i = 0; i < _itemDataModels.Count; i++)
                            if (_itemDataModels[i].basic.id == data[1])
                                itemListPopupField.index = i;
                    break;
                case 1:
                    // 値が数値の場合
                    if (int.TryParse(data[1], out parse) && _weaponDataModels.Count > int.Parse(data[1]))
                        itemListPopupField.index = int.Parse(data[1]);
                    // 値がID
                    else
                        for (var i = 0; i < _weaponDataModels.Count; i++)
                            if (_weaponDataModels[i].basic.id == data[1])
                                itemListPopupField.index = i;
                    break;
                case 2:
                    // 値が数値の場合
                    if (int.TryParse(data[1], out parse) && _armorDataModels.Count > int.Parse(data[1]))
                        itemListPopupField.index = int.Parse(data[1]);
                    // 値がID
                    else
                        for (var i = 0; i < _armorDataModels.Count; i++)
                            if (_armorDataModels[i].basic.id == data[1])
                                itemListPopupField.index = i;
                    break;
            }

            itemList.Clear();
            itemList.style.width = _itemNameWidth;
            itemList.Add(itemListPopupField);
            itemListPopupField.RegisterValueChangedCallback(evt =>
            {
                data[1] = itemIdList[itemListPopupField.index];
                Save();
            });

            RadioButton standard = Items.Query<RadioButton>("radioButton-eventCommand-price1");
            RadioButton designation = Items.Query<RadioButton>("radioButton-eventCommand-price2");
            IntegerField num = Items.Query<IntegerField>("num");
            if (data[2] == "0")
                standard.value = true;
            else
                designation.value = true;
            standard.RegisterValueChangedCallback(o =>
            {
                if (!standard.value)
                {
                    standard.value = false;
                    designation.value = true;
                }

                if (standard.value)
                {
                    standard.value = true;
                    designation.value = false;
                    data[2] = "0";
                }

                num.SetEnabled(designation.value);
                Save();
            });
            designation.RegisterValueChangedCallback(o =>
            {
                if (!designation.value)
                {
                    designation.value = false;
                    standard.value = true;
                }

                if (designation.value)
                {
                    designation.value = true;
                    standard.value = false;
                    data[2] = "1";
                }

                num.SetEnabled(designation.value);
                Save();
            });
            num.value = int.Parse(data[3]);
            BaseInputFieldHandler.IntegerFieldCallback(num, evt =>
            {
                data[3] = num.value.ToString();
                Save();
            }, 0, 999999);

            Button delete = Items.Query<Button>("delete");
            delete.clicked += () =>
            {
                //表示の削除を実施
                _items.Remove(Items);
                //データの削除を実施
                _itemList.Remove(data);
                Save();
            };
        }

        //データの保存を行う
        private void Save() {
            //今のイベントデータの中にある商品の削除
            for (var i = EventCommandIndex + 1; i < EventDataModels[EventIndex].eventCommands.Count; i++)
                if (EventDataModels[EventIndex].eventCommands[i].code ==
                    (int) EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG_LINE)
                {
                    EventDataModels[EventIndex].eventCommands.RemoveAt(i);
                    i--;
                }
                else
                {
                    break;
                }

            //今ショップに並んでいる商品を追加
            for (var i = _itemList.Count - 1; i >= 0; i--)
            {
                //アイテムのデータ
                var itemData = (List<string>) _itemList[i];

                // ショップ下に挿入
                EventDataModels[EventIndex].eventCommands
                    .Insert(EventCommandIndex + 1,
                        //挿入するアイテムの生成
                        new EventDataModel.EventCommand(
                            (int) EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG_LINE,
                            new List<string> {itemData[0], itemData[1], itemData[2], itemData[3]},
                            new List<EventDataModel.EventCommandMoveRoute>()
                        )
                    );
            }

            //データモデルの保存
            Save(EventDataModels[EventIndex]);
        }
    }
}