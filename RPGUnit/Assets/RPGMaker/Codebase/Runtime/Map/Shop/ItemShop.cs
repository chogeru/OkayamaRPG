using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Enum;
using RPGMaker.Codebase.Runtime.Event.Party;
using RPGMaker.Codebase.Runtime.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item.ItemDataModel;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime.RuntimePartyDataModel;

namespace RPGMaker.Codebase.Runtime.Shop
{
    public class ItemShop : WindowBase
    {
        [SerializeField] private TradeItemContent _buyTradeContent;
        [SerializeField] private TradeItemContent _sellTradeContent;
        [SerializeField] private ItemShopContent  _itemPrefab = null;
        [SerializeField] private Text             fundage;
        [SerializeField] private Text             currencyUnit;

        [SerializeField] private List<Transform>  contentParent = new List<Transform>();
        [SerializeField] private List<GameObject> panelList     = new List<GameObject>();
        [SerializeField] private List<GameObject> sellPanelList = new List<GameObject>();

        [SerializeField] private Text       _itemDescription       = null;
        [SerializeField] private GameObject _operationButtonParent = null;
        [SerializeField] private GameObject _categoryButtonParent  = null;
        [SerializeField] private Button     _buyButton;
        [SerializeField] private Button     _sellButton;
        
        [SerializeField] private Text buy;
        [SerializeField] private Text sell;
        [SerializeField] private Text stop;
        [SerializeField] private Text item;
        [SerializeField] private Text weapon;
        [SerializeField] private Text armor;
        [SerializeField] private Text important;
        [SerializeField] private Text haveNumber;
        [SerializeField] private Text haveNumber2;
        [SerializeField] private Text haveNumber3;
        
        
        
        public                   Button     BackOperationButton;

        //EventSystemの取得
        private EventSystem _eventSystem;

        //現在選択状態のオブジェクト
        private GameObject _focusObject;

        public enum ShopState
        {
            NONE,
            BUY,
            SELL,
            BACK,
            ITEM,
            WEAPON,
            ARMOR,
            IMPORTANT,
            TRADE_SCENE,
            TRADE_SCENE_SELL,
        }

        private ShopState _nowSelectKind;
        private ShopState _processingKind;
        private ShopState _befProcessingKind;

        // 販売商品List
        private List<ItemShopContent> saleItemList = null;

        //選択された販売商品のIndex
        private int saleItemListIndex = 0;

        // カテゴリーごとの所持品一覧
        Dictionary<ItemShopContent.Type, List<ItemShopContent>> _belongingDic =
            new Dictionary<ItemShopContent.Type, List<ItemShopContent>>();

        // 販売商品リスト
        List<ItemDataModel>   salesItemDataList   = new List<ItemDataModel>();
        List<WeaponDataModel> salesWeaponDataList = new List<WeaponDataModel>();
        List<ArmorDataModel>  salesArmorDataList  = new List<ArmorDataModel>();

        // マスターデータを格納するリスト
        private List<ItemDataModel>   _items;
        private List<WeaponDataModel> _weapons;
        private List<ArmorDataModel>  _armors;

        private ItemShopContent  _selectedItemContent;
        private TradeItemContent _activeTradeContent;
        private int              _tradeNum = 0;
        private List<Action>     _tradeActionList;

        //今持っている物のリスト
        List<RuntimePartyDataModel.Item>   haveItemList   = new List<RuntimePartyDataModel.Item>();
        List<RuntimePartyDataModel.Weapon> haveWeaponList = new List<RuntimePartyDataModel.Weapon>();
        List<RuntimePartyDataModel.Armor>  haveArmorList  = new List<RuntimePartyDataModel.Armor>();

        //セーブデータの保持
        private RuntimeSaveDataModel _runtimeSaveDataModel;

        public List<EventDataModel.EventCommand> ShopBuyItem { get; set; }
        private bool _isChangingFocus = false;

        private readonly int _maxItems = 9999;

        /**
         * 処理開始
         */
        private void Start() {
            //共通UIの適応を開始
            Init();

            //EventSystemの取得
            _eventSystem = GameObject.FindWithTag("EventSystem").GetComponent<EventSystem>();


            //セーブデータの取得
            _runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
            haveItemList = _runtimeSaveDataModel.runtimePartyDataModel.items;
            haveWeaponList = _runtimeSaveDataModel.runtimePartyDataModel.weapons;
            haveArmorList = _runtimeSaveDataModel.runtimePartyDataModel.armors;

            buy.text = TextManager.buy;
            sell.text = TextManager.sell;
            stop.text = TextManager.cancel;
            item.text = TextManager.item;
            weapon.text = TextManager.weapon;
            armor.text = TextManager.armor;
            important.text = TextManager.keyItem;
            haveNumber.text = TextManager.possession;
            haveNumber2.text = TextManager.possession;
            haveNumber3.text = TextManager.possession;

            var _databaseManagementService = new DatabaseManagementService();
            _items = _databaseManagementService.LoadItem();
            _weapons = _databaseManagementService.LoadWeapon();
            _armors = _databaseManagementService.LoadArmor();

            saleItemList = new List<ItemShopContent>();
            _belongingDic = new Dictionary<ItemShopContent.Type, List<ItemShopContent>>()
            {
                {ItemShopContent.Type.ITEM, new List<ItemShopContent>()},
                {ItemShopContent.Type.WEAPON, new List<ItemShopContent>()},
                {ItemShopContent.Type.ARMOR, new List<ItemShopContent>()},
                {ItemShopContent.Type.IMPORTANT, new List<ItemShopContent>()},
            };

            // 操作切り替えボタンのコールバック登録
            var operationButton = _operationButtonParent.GetComponentsInChildren<Button>();
            foreach (var button in operationButton)
            {
                SetupButtonEvent(button);
            }

            operationButton[0].Select();
            // 「購入のみ」の場合は売却用のボタンを非表示にする
            bool flg = false;
            if (ShopBuyItem[0].parameters.Count >= 5) flg = ShopBuyItem[0].parameters[4] == "1";
            operationButton[1].gameObject.SetActive(!flg);

            var selects = _operationButtonParent.GetComponentsInChildren<Button>();
            for (var i = 0; i < selects.Length; i++)
            {
                var nav = selects[i].navigation;
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnUp = selects[i == 0 ? selects.Length - 1 : i - 1];
                nav.selectOnDown = selects[(i + 1) % selects.Length];
                nav.selectOnLeft = selects[i == 0 ? selects.Length - 1 : i - 1];
                nav.selectOnRight = selects[(i + 1) % selects.Length];
                selects[i].navigation = nav;
                if (i == selects.Length - 1)
                {
                    selects[i].GetComponent<WindowButtonBase>().SetSilentClick(true);
                }
            }


            // アイテムのカテゴリー切り替えボタンのコールバック登録
            var categoryButtons = _categoryButtonParent.GetComponentsInChildren<Button>();
            foreach (var button in categoryButtons)
            {
                SetupButtonEvent(button);
            }

            // 購入用のアイテムを非表示
            panelList[1].SetActive(false);
            panelList[1].transform.Find("ItemList/Viewport").gameObject.SetActive(false);
            panelList[1].transform.Find("UsedItemList/Text").gameObject.SetActive(false);
            panelList[1].transform.Find("UsedItemList/Num").gameObject.SetActive(false);

            // 売却用のアイテムを非表示
            sellPanelList[0].transform.Find("Scroll View/Viewport").gameObject.SetActive(false);

            SetupButtonEvent(_buyButton);
            SetupButtonEvent(_sellButton);
            // descriptionを空に
            _itemDescription.text = "";

            InputDistributor.AddInputHandler(
                GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT,
                HandleType.Back, BackWindow);

            // 上下左右の順で個数調整イベントを登録
            if (_tradeActionList == null)
            {
                _tradeActionList = new List<Action>()
                {
                    () => QuantityItem(10),
                    () => QuantityItem(-10),
                    () => QuantityItem(-1),
                    () => QuantityItem(1),
                };
            }

            InputDistributor.AddInputHandler(
                GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT,
                HandleType.UpKeyDown, _tradeActionList[0]);
            InputDistributor.AddInputHandler(
                GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT,
                HandleType.DownKeyDown, _tradeActionList[1]);
            InputDistributor.AddInputHandler(
                GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT,
                HandleType.LeftKeyDown, _tradeActionList[2]);
            InputDistributor.AddInputHandler(
                GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT,
                HandleType.RightKeyDown, _tradeActionList[3]);

            SetSellItem();
            ItemShopWindow();

            //共通設定の色を適用
            Color textColor = new Color(
                DataManager.Self().GetUiSettingDataModel().commonMenus[0].menuFontSetting.color[0] / 255.0f,
                DataManager.Self().GetUiSettingDataModel().commonMenus[0].menuFontSetting.color[1] / 255.0f,
                DataManager.Self().GetUiSettingDataModel().commonMenus[0].menuFontSetting.color[2] / 255.0f, 1.0f);
            _itemDescription.color = textColor;

            //第一階層のボタン
            foreach (var button in operationButton)
            {
                button.transform.Find("Text").GetComponent<Text>().color = textColor;
            }
            //第二階層のボタン
            foreach (var button in categoryButtons)
            {
                button.transform.Find("Text").GetComponent<Text>().color = textColor;
            }

            //所持金及び、Currency
            fundage.color = textColor;
            currencyUnit.color = textColor;

            //第一階層を有効にする
            changeEnabledButton();

            MenuManager.IsShopActive = true;
        }

        public void DestroyObject() {
            InputDistributor.RemoveInputHandler(
                GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT,
                HandleType.Back, BackWindow);

            InputDistributor.RemoveInputHandler(
                GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT,
                HandleType.UpKeyDown, _tradeActionList[0]);
            InputDistributor.RemoveInputHandler(
                GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT,
                HandleType.DownKeyDown, _tradeActionList[1]);
            InputDistributor.RemoveInputHandler(
                GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT,
                HandleType.LeftKeyDown, _tradeActionList[2]);
            InputDistributor.RemoveInputHandler(
                GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT,
                HandleType.RightKeyDown, _tradeActionList[3]);

            MenuManager.IsShopActive = false;
        }

        //陳列するアイテムのSet
        private void SetSellItem() {
            //持ってきた並べる商品のデータを回していく
            foreach (var item in ShopBuyItem)
            {
                switch (item.parameters[0])
                {
                    //アイテムのデータをセットする
                    case "0":
                        //IDが一致するアイテムを探す
                        var addItem = _items.FirstOrDefault(v => v.basic.id == item.parameters[1]);
                        if (addItem == null)
                            break;

                        //金額指定があるかの確認
                        int itemPrice = addItem.basic.price;
                        if (item.parameters[2] == "1")
                            itemPrice = int.Parse(item.parameters[3]);

                        ItemDataModel cloneItem = new ItemDataModel(
                            addItem.DataClone().basic,
                            addItem.DataClone().targetEffect,
                            addItem.DataClone().userEffect,
                            addItem.DataClone().memo
                        );

                        //金額変更
                        cloneItem.basic.price = itemPrice;

                        //設定が終わり陳列棚に並べる
                        salesItemDataList.Add(cloneItem);
                        break;

                    case "1":
                        //IDが一致する武器を探す
                        var addWeapon = _weapons.FirstOrDefault(v => v.basic.id == item.parameters[1]);
                        if (addWeapon == null)
                            break;

                        //金額指定があるかの確認
                        int weaponPrice = addWeapon.basic.price;
                        if (item.parameters[2] == "1")
                            weaponPrice = int.Parse(item.parameters[3]);

                        WeaponDataModel cloneWeapon = new WeaponDataModel(
                            addWeapon.DataClone().basic,
                            addWeapon.DataClone().parameters,
                            addWeapon.DataClone().traits,
                            addWeapon.DataClone().memo
                        );

                        //金額変更
                        cloneWeapon.basic.price = weaponPrice;

                        //設定が終わり陳列棚に並べる
                        salesWeaponDataList.Add(cloneWeapon);
                        break;

                    case "2":
                        //IDが一致する防具を探す
                        var addArmor = _armors.FirstOrDefault(v => v.basic.id == item.parameters[1]);
                        if (addArmor == null)
                            break;

                        //金額指定があるかの確認
                        int armorPrice = addArmor.basic.price;
                        if (item.parameters[2] == "1")
                            armorPrice = int.Parse(item.parameters[3]);

                        ArmorDataModel cloneArmor = new ArmorDataModel(
                            addArmor.DataClone().basic,
                            addArmor.DataClone().parameters,
                            addArmor.DataClone().traits,
                            addArmor.DataClone().memo
                        );

                        //金額変更
                        cloneArmor.basic.price = armorPrice;

                        //設定が終わり陳列棚に並べる
                        salesArmorDataList.Add(cloneArmor);
                        break;
                }
            }
        }


        /// <summary>
        /// ウィンドウ作成
        /// </summary>
        void ItemShopWindow() {
            _nowSelectKind = ShopState.BUY;

            SetupSaleItemList();
            SetupBelongingList();

            FundageSetting();
            CurrencyUnitSetting();
        }

        /// <summary>
        /// 販売商品一覧の初期化
        /// </summary>
        private void SetupSaleItemList() {
            saleItemList.ForEach(v => Destroy(v.gameObject));
            saleItemList.Clear();

            // 販売アイテム
            for (int i = 0; i < salesItemDataList.Count; i++)
            {
                if (salesItemDataList[i] != null &&
                    salesItemDataList[i].basic.itemType != (int) ItemEnums.ItemType.NONE)
                    saleItemList.Add(
                        CreateContent(i, contentParent[0], ShopState.BUY, ItemShopContent.Type.ITEM,
                            salesItemDataList[i])
                    );
            }

            // 販売武器
            for (int i = 0; i < salesWeaponDataList.Count; i++)
            {
                if (salesWeaponDataList[i] != null)
                    saleItemList.Add(
                        CreateContent(i, contentParent[0], ShopState.BUY, ItemShopContent.Type.WEAPON,
                            salesWeaponDataList[i])
                    );
            }

            // 販売防具
            for (int i = 0; i < salesArmorDataList.Count; i++)
            {
                if (salesArmorDataList[i] != null)
                    saleItemList.Add(
                        CreateContent(i, contentParent[0], ShopState.BUY, ItemShopContent.Type.ARMOR,
                            salesArmorDataList[i])
                    );
            }

            // ボタンのNavgationを設定
            for (int i = 0; i < saleItemList.Count; i++)
            {
                var selectable = saleItemList[i].ContentButton;
                SetupButtonEvent(selectable);

                var nav = selectable.navigation;
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnUp = saleItemList[i == 0 ? saleItemList.Count - 1 : i - 1].ContentButton;
                nav.selectOnDown = saleItemList[(i + 1) % saleItemList.Count].ContentButton;
                selectable.navigation = nav;
            }

            contentParent[0].GetComponent<RectTransform>().sizeDelta =
                new Vector2(contentParent[0].GetComponent<RectTransform>().sizeDelta.x, saleItemList.Count * 70);
        }

        /// <summary>
        /// 所持品一覧の初期化
        /// </summary>
        private void SetupBelongingList() {
            foreach (var list in _belongingDic.Values)
            {
                list.ForEach(v => Destroy(v.gameObject));
                list.Clear();
            }

            // 所持アイテム
            for (int i = 0; i < haveItemList.Count; i++)
            {
                ItemDataModel item = null;
                for (int i2 = 0; i2 < _items.Count; i2++)
                    if (_items[i2].basic.id == haveItemList[i].itemId)
                    {
                        item = _items[i2];
                        break;
                    }
                if (item != null && haveItemList[i].value > 0)
                {
                    if (item.basic.itemType == (int) ItemEnums.ItemType.IMPORTANT)
                        _belongingDic[ItemShopContent.Type.IMPORTANT].Add(
                            CreateContent(i, contentParent[4], ShopState.IMPORTANT, ItemShopContent.Type.IMPORTANT,
                                item)
                        );
                    else if (item.basic.itemType != (int) ItemEnums.ItemType.NONE)
                        _belongingDic[ItemShopContent.Type.ITEM].Add(
                            CreateContent(i, contentParent[1], ShopState.ITEM, ItemShopContent.Type.ITEM, item)
                        );
                }
            }

            // 所持武器
            for (int i = 0; i < haveWeaponList.Count; i++)
            {
                WeaponDataModel weapon = null;
                for (int i2 = 0; i2 < _weapons.Count; i2++)
                    if (_weapons[i2].basic.id == haveWeaponList[i].weaponId)
                    {
                        weapon = _weapons[i2];
                        break;
                    }
                if (weapon != null && haveWeaponList[i].value > 0)
                    _belongingDic[ItemShopContent.Type.WEAPON].Add(
                        CreateContent(i, contentParent[2], ShopState.WEAPON, ItemShopContent.Type.WEAPON, weapon)
                    );
            }

            // 所持防具
            for (int i = 0; i < haveArmorList.Count; i++)
            {
                ArmorDataModel armor = null;
                for (int i2 = 0; i2 < _armors.Count; i2++)
                    if (_armors[i2].basic.id == haveArmorList[i].armorId)
                    {
                        armor = _armors[i2];
                        break;
                    }
                if (armor != null && haveArmorList[i].value > 0)
                    _belongingDic[ItemShopContent.Type.ARMOR].Add(
                        CreateContent(i, contentParent[3], ShopState.ARMOR, ItemShopContent.Type.ARMOR, armor)
                    );
            }

            // ボタンのNavgationを設定。所持品は2列で表示されるので上下左右キーに対応
            foreach (var list in _belongingDic.Values)
            {
                //ボタンが1つしかない場合にはnavigationの設定をしない
                if (list.Count > 1)
                    for (int i = 0; i < list.Count; i++)
                    {
                        var selectable = list[i].ContentButton;
                        SetupButtonEvent(selectable);
                        var nav = selectable.navigation;
                        nav.mode = Navigation.Mode.Explicit;
                        nav.selectOnLeft = list[i == 0 ? list.Count - 1 : i - 1].ContentButton;
                        nav.selectOnUp = list[i < 2 ? list.Count - Math.Abs(i - 2) : i - 2].ContentButton;
                        nav.selectOnRight = list[(i + 1) % list.Count].ContentButton;
                        nav.selectOnDown = list[(i + 2) % list.Count].ContentButton;
                        selectable.navigation = nav;
                    }
            }

            float itemHeight = _itemPrefab.GetComponent<RectTransform>().sizeDelta.y;
            contentParent[1].GetComponent<RectTransform>().sizeDelta =
                new Vector2(contentParent[1].GetComponent<RectTransform>().sizeDelta.x, (_belongingDic[ItemShopContent.Type.ITEM].Count / 2 + _belongingDic[ItemShopContent.Type.ITEM].Count % 2) * itemHeight);
            contentParent[2].GetComponent<RectTransform>().sizeDelta =
                new Vector2(contentParent[2].GetComponent<RectTransform>().sizeDelta.x, (_belongingDic[ItemShopContent.Type.WEAPON].Count / 2 + _belongingDic[ItemShopContent.Type.WEAPON].Count % 2) * itemHeight);
            contentParent[3].GetComponent<RectTransform>().sizeDelta =
                new Vector2(contentParent[3].GetComponent<RectTransform>().sizeDelta.x, (_belongingDic[ItemShopContent.Type.ARMOR].Count / 2 + _belongingDic[ItemShopContent.Type.ARMOR].Count % 2) * itemHeight);
            contentParent[4].GetComponent<RectTransform>().sizeDelta =
                new Vector2(contentParent[4].GetComponent<RectTransform>().sizeDelta.x, (_belongingDic[ItemShopContent.Type.IMPORTANT].Count / 2 + _belongingDic[ItemShopContent.Type.IMPORTANT].Count % 2) * itemHeight);

        }

        /**
         * アイテム要素の作成
         */
        ItemShopContent CreateContent<T>(
            int i,
            Transform parent,
            ShopState kind,
            ItemShopContent.Type myKind,
            T itemData
        ) {
            ItemShopContent itemContent = Instantiate(_itemPrefab, parent);
            itemContent.ContentButton.GetComponent<WindowButtonBase>().OnFocus = new UnityEngine.Events.UnityEvent();
            itemContent.ContentButton.GetComponent<WindowButtonBase>().OnFocus.AddListener(() => ChangeFocus(itemContent));
            itemContent.ContentButton.GetComponent<WindowButtonBase>().OnClick = new Button.ButtonClickedEvent();
            itemContent.ContentButton.GetComponent<WindowButtonBase>().OnClick.AddListener(() => ItemClicked(itemContent));
            //itemContent.ContentButton.onClick.AddListener(() => ItemClicked(itemContent));

            //ScrollViewとContentを設定する
            itemContent.GetComponent<WindowButtonBase>().ScrollView = parent.parent.parent.gameObject;
            itemContent.GetComponent<WindowButtonBase>().Content = parent.gameObject;

            ItemDataSetting(kind, itemContent, i, myKind, itemData);
            return itemContent;
        }

        /**
         * 所持アイテムの設定
         */
        void ItemDataSetting<T>(
            ShopState kind,
            ItemShopContent itemContent,
            int i,
            ItemShopContent.Type myKind,
            T itemData
        ) {
            ItemBasic itemBasic = null;
            WeaponDataModel.Basic weaponBasic = null;
            ArmorDataModel.Basic armorBasic = null;

            switch (kind)
            {
                // 購入画面
                case ShopState.BUY:
                    string id = "";
                    switch (myKind)
                    {
                        case ItemShopContent.Type.ITEM:
                            itemContent.SettingItems = itemData as ItemDataModel;
                            itemBasic = itemContent.SettingItems.basic;
                            id = itemBasic.id;

                            itemContent.SetText(itemBasic.name, itemBasic.price.ToString(), itemBasic.iconId);
                            break;

                        case ItemShopContent.Type.WEAPON:
                            itemContent.SettingWeapons = itemData as WeaponDataModel;
                            weaponBasic = itemContent.SettingWeapons.basic;
                            id = weaponBasic.id;

                            itemContent.SetText(weaponBasic.name, weaponBasic.price.ToString(), weaponBasic.iconId);
                            break;

                        case ItemShopContent.Type.ARMOR:
                            itemContent.SettingArmors = itemData as ArmorDataModel;
                            armorBasic = itemContent.SettingArmors.basic;
                            id = armorBasic.id;

                            itemContent.SetText(armorBasic.name, armorBasic.price.ToString(), armorBasic.iconId);
                            break;
                    }

                    var tmpItemData = haveItemList.FirstOrDefault(v => id == v.itemId);
                    itemContent.SetItemCount(tmpItemData != null ? tmpItemData.value : 0, myKind);

                    break;

                // 売却画面
                case ShopState.ITEM:
                    // アイテムの情報を設定
                    itemContent.SettingItems = itemData as ItemDataModel;
                    itemBasic = itemContent.SettingItems.basic;
                    itemContent.SetItemCount(haveItemList[i].value, ItemShopContent.Type.ITEM);
                    itemContent.SetText(itemBasic.name, itemContent.ItemCount.ToString(), itemBasic.iconId);
                    //売却不可のチェックが入っている場合には、売却できない
                    itemContent.Sellable = itemContent.SettingItems.basic.canSell == 0 ? true : false;
                    break;
                case ShopState.WEAPON:
                    // 武器の情報を設定
                    itemContent.SettingWeapons = itemData as WeaponDataModel;
                    weaponBasic = itemContent.SettingWeapons.basic;
                    itemContent.SetItemCount(haveWeaponList[i].value, ItemShopContent.Type.WEAPON);
                    itemContent.SetText(weaponBasic.name, itemContent.ItemCount.ToString(), weaponBasic.iconId);
                    //売却不可のチェックが入っている場合には、売却できない
                    itemContent.Sellable = itemContent.SettingWeapons.basic.canSell == 0 ? true : false;
                    break;
                case ShopState.ARMOR:
                    // 防具の情報を設定
                    itemContent.SettingArmors = itemData as ArmorDataModel;
                    armorBasic = itemContent.SettingArmors.basic;
                    itemContent.SetItemCount(haveArmorList[i].value, ItemShopContent.Type.ARMOR);
                    itemContent.SetText(armorBasic.name, itemContent.ItemCount.ToString(), armorBasic.iconId);
                    //売却不可のチェックが入っている場合には、売却できない
                    itemContent.Sellable = itemContent.SettingArmors.basic.canSell == 0 ? true : false;
                    break;
                case ShopState.IMPORTANT:
                    // 大切なものの情報を設定
                    itemContent.SettingItems = itemData as ItemDataModel;
                    itemBasic = itemContent.SettingItems.basic;
                    itemContent.SetItemCount(haveItemList[i].value, ItemShopContent.Type.IMPORTANT);
                    itemContent.SetText(itemBasic.name, itemContent.ItemCount.ToString(), itemBasic.iconId, itemBasic.itemType);
                    break;
            }
        }

        /// <summary>
        /// 所持金の反映
        /// </summary>
        void FundageSetting() {
            if (_runtimeSaveDataModel != null)
                fundage.text = _runtimeSaveDataModel.runtimePartyDataModel.gold.ToString();
        }

        /**
         * 通貨単位の反映
         */
        void CurrencyUnitSetting() {
            if (_runtimeSaveDataModel != null)
                currencyUnit.text = TextManager.money;
            else
            {
                currencyUnit.text = "G";
            }
        }

        /// <summary>
        /// どの操作を行うかを決定するボタン押下のコールバック
        /// </summary>
        /// <param name="state">どの状態にするか</param>
        public void OnClickOperationButton(int state) {
            if (_isChangingFocus)
                return;
            //カーソル押下制御
            switch (state)
            {
                case 1:
                    CanMouseClick(ShopState.BUY, false);
                    break;
                case 2:
                    CanMouseClick(ShopState.SELL, false);
                    break;
                case 3:
                    DestroyObject();
                    break;
                case 4:
                    CanMouseClick(ShopState.ITEM, false);
                    break;
                case 5:
                    CanMouseClick(ShopState.WEAPON, false);
                    break;
                case 6:
                    CanMouseClick(ShopState.ARMOR, false);
                    break;
                case 7:
                    CanMouseClick(ShopState.IMPORTANT, false);
                    break;
            }

            ClickStatusCheck((ShopState) state);
            PanelProcess();
        }

        /// <summary>
        /// どのカテゴリの装備アイテムを表示するかを決定するボタン押下のコールバック
        /// </summary>
        /// <param name="state">どの状態にするか</param>
        public void OnClickCategoryButton(int state) {
            ClickStatusCheck((ShopState) state);
            PanelProcess();
        }

        /// <summary>
        /// 戻る
        /// </summary>
        public void BackWindow() {
            if (_processingKind == ShopState.NONE && _nowSelectKind == ShopState.BUY ||
                _nowSelectKind == ShopState.SELL)
            {
                DestroyObject();
                Destroy(this?.gameObject);
                _eventEndProcess?.Invoke();
            }
            else
            {
                Button button = null;

                switch (_processingKind)
                {
                    case ShopState.SELL:
                    case ShopState.NONE:
                        _nowSelectKind = ShopState.SELL;
                        _befProcessingKind = _processingKind;
                        _processingKind = ShopState.NONE;
                        //操作を戻す
                        CanMouseClick(ShopState.SELL, true);
                        for (int i = 0; i < panelList.Count; i++)
                        {
                            panelList[i].SetActive(false);
                        }

                        panelList[0].SetActive(true);

                        // 購入用のアイテムを非表示
                        panelList[1].transform.Find("ItemList/Viewport").gameObject.SetActive(false);
                        panelList[1].transform.Find("UsedItemList/Text").gameObject.SetActive(false);
                        panelList[1].transform.Find("UsedItemList/Num").gameObject.SetActive(false);
                        panelList[1].transform.Find("UsedItemList/Num").GetComponent<Text>().text = "0";

                        // 売却用のアイテムは非表示
                        sellPanelList[0].transform.Find("Scroll View/Viewport").gameObject.SetActive(false);
                        sellPanelList[0].gameObject.SetActive(false);
                        
                        button = _operationButtonParent.transform.GetChild(1).gameObject.GetComponent<Button>();
                        button?.Select();

                        // 選択アイテムを空に
                        _selectedItemContent = null;

                        // descriptionを空に
                        _itemDescription.text = "";

                        break;

                    case ShopState.BUY:
                        _befProcessingKind = _processingKind;
                        _processingKind = ShopState.NONE;
                        //操作を戻す
                        CanMouseClick(ShopState.BUY, true);

                        for (int i = 0; i < panelList.Count; i++)
                        {
                            panelList[i].SetActive(false);
                        }

                        panelList[0].SetActive(true);

                        // 購入用のアイテムを表示
                        panelList[1].transform.Find("ItemList/Viewport").gameObject.SetActive(true);
                        panelList[1].transform.Find("UsedItemList/Text").gameObject.SetActive(true);
                        panelList[1].transform.Find("UsedItemList/Num").gameObject.SetActive(true);
                        panelList[1].transform.Find("UsedItemList/Num").GetComponent<Text>().text = "0";

                        // 売却用のアイテムを非表示
                        sellPanelList[0].transform.Find("Scroll View/Viewport").gameObject.SetActive(false);
                        
                        button = _operationButtonParent.transform.GetChild(0).gameObject.GetComponent<Button>();
                        button?.Select();

                        // 選択アイテムを空に
                        _selectedItemContent = null;

                        // descriptionを空に
                        _itemDescription.text = "";

                        break;

                    case ShopState.ITEM:
                        //操作を戻す
                        CanMouseClick(SellWindow(ShopState.ITEM), true);

                        // 選択アイテムを空に
                        _selectedItemContent = null;

                        // descriptionを空に
                        _itemDescription.text = "";
                        break;
                    case ShopState.WEAPON:
                        //操作を戻す
                        CanMouseClick(SellWindow(ShopState.WEAPON), true);

                        // 選択アイテムを空に
                        _selectedItemContent = null;

                        // descriptionを空に
                        _itemDescription.text = "";
                        break;
                    case ShopState.ARMOR:
                        //操作を戻す
                        CanMouseClick(SellWindow(ShopState.ARMOR), true);

                        // 選択アイテムを空に
                        _selectedItemContent = null;

                        // descriptionを空に
                        _itemDescription.text = "";
                        break;
                    case ShopState.IMPORTANT:
                        //操作を戻す
                        CanMouseClick(SellWindow(ShopState.IMPORTANT), true);

                        // 選択アイテムを空に
                        _selectedItemContent = null;

                        // descriptionを空に
                        _itemDescription.text = "";
                        break;

                    case ShopState.TRADE_SCENE:
                        panelList.ForEach(v => v.SetActive(false));
                        panelList[0].SetActive(true);
                        panelList[1].SetActive(true);
                        _befProcessingKind = _processingKind;
                        _processingKind = ShopState.BUY;
                        PanelProcess();
                        button = contentParent[0].GetChild(saleItemListIndex).gameObject.GetComponent<Button>();
                        button?.Select();
                        break;

                    case ShopState.TRADE_SCENE_SELL:
                        panelList.ForEach(v => v.SetActive(false));
                        panelList[0].SetActive(true);
                        panelList[2].SetActive(true);

                        //修正戻し
                        _befProcessingKind = _processingKind;
                        _processingKind = _selectedItemContent.CurrentType switch
                        {
                            ItemShopContent.Type.ITEM => ShopState.ITEM,
                            ItemShopContent.Type.WEAPON => ShopState.WEAPON,
                            ItemShopContent.Type.ARMOR => ShopState.ARMOR,
                            ItemShopContent.Type.IMPORTANT => ShopState.IMPORTANT,
                            _ => throw new InvalidOperationException()
                        };
                        PanelProcess();
                        break;
                }

                ShopState SellWindow(ShopState num) {
                    _befProcessingKind = _processingKind;
                    _processingKind = ShopState.SELL;
                    sellPanelList.ForEach(v => v.SetActive(false));

                    button = _categoryButtonParent.transform.GetChild((int) num - 4).gameObject.GetComponent<Button>();
                    button?.Select();
                    return num;
                }
            }

            //ボタンの有効無効状態を切り替え
            changeEnabledButton();

            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.cancel);
            SoundManager.Self().PlaySe();
        }
        public void SellBackWindow() {
            panelList.ForEach(v => v.SetActive(false));
            panelList[0].SetActive(true);
            panelList[2].SetActive(true);

            _befProcessingKind = _processingKind;
            _processingKind = _selectedItemContent.CurrentType switch
            {
                ItemShopContent.Type.ITEM => ShopState.ITEM,
                ItemShopContent.Type.WEAPON => ShopState.WEAPON,
                ItemShopContent.Type.ARMOR => ShopState.ARMOR,
                ItemShopContent.Type.IMPORTANT => ShopState.IMPORTANT,
                _ => throw new InvalidOperationException()
            };
            PanelProcess();
            //ボタンの有効無効状態を切り替え
            changeEnabledButton();
        }

        /**
         * 現在のクリック状態を確認
         * （使用していないが必要になるかもしれないので残しておく）
         */
        bool IsCheckClick(ShopState triedKind) {
            if (_processingKind == ShopState.BUY)
                return false;

            if (_processingKind == ShopState.SELL)
                if (triedKind == ShopState.BUY || triedKind == ShopState.SELL ||
                    triedKind == ShopState.BACK)
                    return false;

            if (_processingKind == ShopState.ITEM ||
                _processingKind == ShopState.WEAPON ||
                _processingKind == ShopState.ARMOR ||
                _processingKind == ShopState.IMPORTANT)
                return false;

            return true;
        }

        void ClickStatusCheck(ShopState kind) {
            _nowSelectKind = kind;
            _befProcessingKind = _processingKind;
            _processingKind = _nowSelectKind;
        }

        void PanelProcess() {
            switch (_processingKind)
            {
                case ShopState.BUY:
                case ShopState.SELL:
                    for (int i = 0; i < panelList.Count; i++)
                    {
                        panelList[i].SetActive(false);
                    }

                    panelList[(int) _processingKind].SetActive(true);
                    if (_processingKind == ShopState.SELL)
                    {
                        _nowSelectKind = ShopState.ITEM;
                        var categoryButtons = _categoryButtonParent.GetComponentInChildren<Button>();
                        categoryButtons.Select();

                        // 選択アイテムを空に
                        _selectedItemContent = null;

                        // descriptionを空に
                        _itemDescription.text = "";

                        // 所持数の初期化
                        panelList[1].transform.Find("UsedItemList/Num").GetComponent<Text>().text = "0";
                    }
                    else
                    {
                        if (saleItemList.Count != 0)
                        {
                            saleItemList[0].ContentButton.Select();

                            // 選択アイテム
                            _selectedItemContent = saleItemList[0];

                            int itemNum = 0;
                            switch (_selectedItemContent.CurrentType)
                            {
                                case ItemShopContent.Type.ITEM:
                                    if (_selectedItemContent.SettingItems == null) return;
                                    _itemDescription.text = _selectedItemContent.SettingItems.basic.description;
                                    var haveItem = haveItemList.FirstOrDefault(v =>
                                        v.itemId == _selectedItemContent.SettingItems.basic.id);
                                    if (haveItem != null)
                                        itemNum = haveItem.value;
                                    break;

                                case ItemShopContent.Type.WEAPON:
                                    if (_selectedItemContent.SettingWeapons == null) return;
                                    _itemDescription.text = _selectedItemContent.SettingWeapons.basic.description;
                                    var haveWeapon = haveWeaponList.FirstOrDefault(v =>
                                        v.weaponId == _selectedItemContent.SettingWeapons.basic.id);
                                    if (haveWeapon != null)
                                        itemNum = haveWeapon.value;
                                    break;

                                case ItemShopContent.Type.ARMOR:
                                    if (_selectedItemContent.SettingArmors == null) return;
                                    _itemDescription.text = _selectedItemContent.SettingArmors.basic.description;
                                    var haveArmor = haveArmorList.FirstOrDefault(v =>
                                        v.armorId == _selectedItemContent.SettingArmors.basic.id);
                                    if (haveArmor != null)
                                        itemNum = haveArmor.value;
                                    break;

                                case ItemShopContent.Type.IMPORTANT:
                                    if (_selectedItemContent.SettingItems == null) return;
                                    _itemDescription.text = _selectedItemContent.SettingItems.basic.description;
                                    haveItem = haveItemList.FirstOrDefault(v =>
                                        v.itemId == _selectedItemContent.SettingItems.basic.id);
                                    if (haveItem != null)
                                        itemNum = haveItem.value;
                                    break;
                            }

                            panelList[1].transform.Find("UsedItemList").Find("Num").GetComponent<Text>().text =
                                itemNum.ToString();
                            panelList[3].transform.Find("UsedItemList").Find("Num").GetComponent<Text>().text =
                                itemNum.ToString();
                            panelList[4].transform.Find("UsedItemList").Find("Num").GetComponent<Text>().text =
                                itemNum.ToString();
                        }
                        else
                        {
                            // 選択アイテムを空に
                            _selectedItemContent = null;

                            // descriptionを空に
                            _itemDescription.text = "";

                            // 所持数の初期化
                            panelList[1].transform.Find("UsedItemList/Num").GetComponent<Text>().text = "0";
                        }
                    }

                    panelList[0].SetActive(true);

                    // 購入用のアイテムを表示
                    panelList[1].transform.Find("ItemList/Viewport").gameObject.SetActive(true);
                    panelList[1].transform.Find("UsedItemList/Text").gameObject.SetActive(true);
                    panelList[1].transform.Find("UsedItemList/Num").gameObject.SetActive(true);

                    for (int i = 0; i < sellPanelList.Count; i++)
                        sellPanelList[i].SetActive(false);
                    sellPanelList[0].gameObject.SetActive(false);
                    break;

                case ShopState.BACK:
                    BackWindow();
                    break;

                case ShopState.ITEM:
                case ShopState.WEAPON:
                case ShopState.ARMOR:
                case ShopState.IMPORTANT:
                    // 売却用のアイテムを表示
                    sellPanelList[0].transform.Find("Scroll View/Viewport").gameObject.SetActive(true);
                    sellPanelList[0].gameObject.SetActive(true);

                    // 購入、売却ウィンドウ非表示
                    panelList[3].SetActive(false);
                    panelList[4].SetActive(false);

                    // descriptionを空に
                    _itemDescription.text = "";

                    // 選択アイテムを空に
                    _selectedItemContent = null;

                    for (int i = 0; i < sellPanelList.Count; i++)
                        sellPanelList[i].SetActive(false);
                    sellPanelList[(int) _processingKind - 4].SetActive(true);

                    //売却する物が無い場合に入らない
                    if (_belongingDic[ItemShopContent.StateToType(_processingKind)].Count != 0)
                    {
                        var showList = _belongingDic[ItemShopContent.StateToType(_processingKind)].First();
                        if (showList != null)
                        {
                            showList.ContentButton.Select();

                            // 選択アイテム
                            _selectedItemContent = showList;

                            // description
                            if (_processingKind == ShopState.ITEM)
                            {
                                _itemDescription.text = showList.SettingItems.basic.description;
                            }
                            else if (_processingKind == ShopState.WEAPON)
                            {
                                _itemDescription.text = showList.SettingWeapons.basic.description;
                            }
                            else if (_processingKind == ShopState.ARMOR)
                            {
                                _itemDescription.text = showList.SettingArmors.basic.description;
                            }
                            else if (_processingKind == ShopState.IMPORTANT)
                            {
                                _itemDescription.text = showList.SettingItems.basic.description;
                            }
                        }
                    }
                    break;
            }

            if (_processingKind == ShopState.ITEM || _processingKind == ShopState.WEAPON || _processingKind == ShopState.ARMOR)
            {
                //売却可能かどうかによって、選択可否の色を変更
                StatusCheckSell();
            }
            else
            {
                //その他
                StatusCheck();
            }

            //各ボタンの有効状態の切り替え
            changeEnabledButton();
        }

        private void ChangeFocus(ItemShopContent item) {
            //選択されたアイテムのIndexの取得
            for (int i = 0; i < saleItemList.Count; i++)
                if (saleItemList[i] == item)
                    saleItemListIndex = i;

            int itemNum = 0;
            switch (item.CurrentType)
            {
                case ItemShopContent.Type.ITEM:
                    if (item.SettingItems == null) return;

                    _itemDescription.text = item.SettingItems.basic.description;
                    var haveItem = haveItemList.FirstOrDefault(v => v.itemId == item.SettingItems.basic.id);
                    if (haveItem != null)
                        itemNum = haveItem.value;
                    break;

                case ItemShopContent.Type.WEAPON:
                    if (item.SettingWeapons == null) return;

                    _itemDescription.text = item.SettingWeapons.basic.description;
                    var haveWeapon = haveWeaponList.FirstOrDefault(v => v.weaponId == item.SettingWeapons.basic.id);
                    if (haveWeapon != null)
                        itemNum = haveWeapon.value;
                    break;

                case ItemShopContent.Type.ARMOR:
                    if (item.SettingArmors == null) return;

                    _itemDescription.text = item.SettingArmors.basic.description;
                    var haveArmor = haveArmorList.FirstOrDefault(v => v.armorId == item.SettingArmors.basic.id);
                    if (haveArmor != null)
                        itemNum = haveArmor.value;
                    break;

                case ItemShopContent.Type.IMPORTANT:
                    if (item.SettingItems == null) return;
                    _itemDescription.text = item.SettingItems.basic.description;
                    haveItem = haveItemList.FirstOrDefault(v => v.itemId == item.SettingItems.basic.id);
                    if (haveItem != null)
                        itemNum = haveItem.value;
                    break;
            }

            // 個数を各Textに反映
            panelList[1].transform.Find("UsedItemList").Find("Num").GetComponent<Text>().text = itemNum.ToString();
            panelList[3].transform.Find("UsedItemList").Find("Num").GetComponent<Text>().text = itemNum.ToString();
            panelList[4].transform.Find("UsedItemList").Find("Num").GetComponent<Text>().text = itemNum.ToString();
            _selectedItemContent = item;
            return;
        }

        /**
         * 要素選択時の処理
         */
        private void ItemClicked(ItemShopContent item) {
            //購入の場合には、OnFocusが来ずにClickイベントが来るケースがあるため、先にOnFocusを通す
            if (_processingKind == ShopState.BUY)
            {
                ChangeFocus(item);
            }

            //大事なものを売ることは出来ない
            if (item.CurrentType == ItemShopContent.Type.IMPORTANT) return;

            //購入の場合所持数が最大値の時は何もしない
            if (_processingKind == ShopState.BUY) {
                int itemNum = 0;
                Item haveItem = null;
                switch (item.CurrentType)
                {
                    case ItemShopContent.Type.ITEM:
                        haveItem = haveItemList.FirstOrDefault(v => v.itemId == item.SettingItems.basic.id); break;
                    case ItemShopContent.Type.WEAPON:
                        haveItem = haveItemList.FirstOrDefault(v => v.itemId == item.SettingWeapons.basic.id); break;
                    case ItemShopContent.Type.ARMOR:
                        haveItem = haveItemList.FirstOrDefault(v => v.itemId == item.SettingArmors.basic.id); break;
                }
                if (haveItem != null)
                    itemNum = haveItem.value;
                if (_maxItems <= itemNum) return;
            }

            panelList.ForEach(v => v.SetActive(false));
            panelList[0].SetActive(true);

            // 購入、売却ウィンドウの表示切り替え
            if (_processingKind == ShopState.BUY)
            {
                panelList[3].SetActive(true);

                _sellTradeContent.gameObject.SetActive(false);
                _activeTradeContent = _buyTradeContent;
                _befProcessingKind = _processingKind;
                _processingKind = ShopState.TRADE_SCENE;
                _buyButton.Select();
            }
            else
            {
                sellPanelList.ForEach(v => v.SetActive(false));
                panelList[2].SetActive(true);
                panelList[4].SetActive(true);

                _buyTradeContent.gameObject.SetActive(false);
                _activeTradeContent = _sellTradeContent;
                _befProcessingKind = _processingKind;
                _processingKind = ShopState.TRADE_SCENE_SELL;
                _sellButton.Select();
            }

            _activeTradeContent.gameObject.SetActive(true);
            _tradeNum = 1;

            switch (item.CurrentType)
            {
                case ItemShopContent.Type.ITEM:
                    var itemBasic = item.SettingItems.basic;
                    if (_processingKind == ShopState.TRADE_SCENE)
                        _activeTradeContent.SetTradeItemInfo(itemBasic.iconId, itemBasic.name, _tradeNum, itemBasic.price);
                    else
                        _activeTradeContent.SetTradeItemInfo(itemBasic.iconId, itemBasic.name, _tradeNum, itemBasic.sell);
                    break;
                case ItemShopContent.Type.WEAPON:
                    var weaponBasic = item.SettingWeapons.basic;
                    if (_processingKind == ShopState.TRADE_SCENE)
                        _activeTradeContent.SetTradeItemInfo(weaponBasic.iconId, weaponBasic.name, _tradeNum, weaponBasic.price);
                    else
                        _activeTradeContent.SetTradeItemInfo(weaponBasic.iconId, weaponBasic.name, _tradeNum, weaponBasic.sell);
                    break;
                case ItemShopContent.Type.ARMOR:
                    var armorBasic = item.SettingArmors.basic;
                    if (_processingKind == ShopState.TRADE_SCENE)
                        _activeTradeContent.SetTradeItemInfo(armorBasic.iconId, armorBasic.name, _tradeNum, armorBasic.price);
                    else
                        _activeTradeContent.SetTradeItemInfo(armorBasic.iconId, armorBasic.name, _tradeNum, armorBasic.sell);
                    break;
            }
        }

        /**
         * アイテム数の設定
         */
        public void QuantityItem(int num) {
            if (_selectedItemContent == null) return;
            if (_activeTradeContent == null || !_activeTradeContent.isActiveAndEnabled) return;

            int priceNum = _selectedItemContent.CurrentType switch
            {
                ItemShopContent.Type.ITEM => _processingKind == ShopState.TRADE_SCENE ? _selectedItemContent.SettingItems.basic.price : _selectedItemContent.SettingItems.basic.sell,
                ItemShopContent.Type.WEAPON => _processingKind == ShopState.TRADE_SCENE ? _selectedItemContent.SettingWeapons.basic.price : _selectedItemContent.SettingWeapons.basic.sell,
                ItemShopContent.Type.ARMOR => _processingKind == ShopState.TRADE_SCENE ? _selectedItemContent.SettingArmors.basic.price : _selectedItemContent.SettingArmors.basic.sell,
                _ => 0
            };

            int haveItemNum = 0;
            int i;
            if (_selectedItemContent.CurrentType == ItemShopContent.Type.ITEM)
            {
                for (i = 0; i < _runtimeSaveDataModel.runtimePartyDataModel.items.Count; i++)
                {
                    if (_runtimeSaveDataModel.runtimePartyDataModel.items[i].itemId == _selectedItemContent.SettingItems.basic.id)
                    {
                        haveItemNum = _runtimeSaveDataModel.runtimePartyDataModel.items[i].value;
                        break;
                    }
                }
            }
            else if (_selectedItemContent.CurrentType == ItemShopContent.Type.WEAPON)
            {
                for (i = 0; i < _runtimeSaveDataModel.runtimePartyDataModel.weapons.Count; i++)
                {
                    if (_runtimeSaveDataModel.runtimePartyDataModel.weapons[i].weaponId == _selectedItemContent.SettingWeapons.basic.id)
                    {
                        haveItemNum = _runtimeSaveDataModel.runtimePartyDataModel.weapons[i].value;
                        break;
                    }
                }
            }
            else if (_selectedItemContent.CurrentType == ItemShopContent.Type.ARMOR)
            {
                for (i = 0; i < _runtimeSaveDataModel.runtimePartyDataModel.armors.Count; i++)
                {
                    if (_runtimeSaveDataModel.runtimePartyDataModel.armors[i].armorId == _selectedItemContent.SettingArmors.basic.id)
                    {
                        haveItemNum = _runtimeSaveDataModel.runtimePartyDataModel.armors[i].value;
                        break;
                    }
                }
            }

            _tradeNum += num;
            if (_processingKind == ShopState.TRADE_SCENE)
            {
                if (priceNum > 0)
                    _tradeNum = Math.Min(Math.Min(Math.Max(_tradeNum, 1), _maxItems - haveItemNum), Mathf.FloorToInt(_runtimeSaveDataModel.runtimePartyDataModel.gold / priceNum));
                else
                    _tradeNum = Math.Min(Math.Max(_tradeNum, 1), _maxItems - haveItemNum);
            }
            else
            {
                _tradeNum = Math.Min(Math.Max(_tradeNum, 0), haveItemNum);
            }

            _activeTradeContent.SetTradeNum(_tradeNum, priceNum * _tradeNum);

            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.cursor);
            SoundManager.Self().PlaySe();
        }

        /**
         * 確定ボタン
         */
        public void BuyAndSell() {
            if (_selectedItemContent == null) return;
            if (_runtimeSaveDataModel == null) return;

            int price = _selectedItemContent.CurrentType switch
            {
                ItemShopContent.Type.ITEM => _processingKind == ShopState.TRADE_SCENE ? _selectedItemContent.SettingItems.basic.price : _selectedItemContent.SettingItems.basic.sell,
                ItemShopContent.Type.WEAPON => _processingKind == ShopState.TRADE_SCENE ? _selectedItemContent.SettingWeapons.basic.price : _selectedItemContent.SettingWeapons.basic.sell,
                ItemShopContent.Type.ARMOR => _processingKind == ShopState.TRADE_SCENE ? _selectedItemContent.SettingArmors.basic.price : _selectedItemContent.SettingArmors.basic.sell,
                _ => 0
            };

            int totalPrice = price * _tradeNum;
            if (_processingKind == ShopState.TRADE_SCENE)
            {
                // 所持金が足りているか
                if (_runtimeSaveDataModel.runtimePartyDataModel.gold < totalPrice)
                    return;

                _runtimeSaveDataModel.runtimePartyDataModel.gold -= totalPrice;
                FundageSetting();

                // タイプ毎に各値設定
                EventDataModel.EventCommand command;
                switch (_selectedItemContent.CurrentType)
                {
                    case ItemShopContent.Type.ITEM:
                        // 購入したアイテムを追加し、所持品リストを更新する
                        command = new EventDataModel.EventCommand(0, new List<string>()
                            {_selectedItemContent.SettingItems.basic.id, "0", "0", _tradeNum.ToString()}, null);
                        new PartyItemProcess().Invoke(null, ((int) EventEnum.EVENT_CODE_PARTY_ITEM).ToString(), command,
                            null);

                        _runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
                        haveItemList = _runtimeSaveDataModel.runtimePartyDataModel.items;
                        break;

                    case ItemShopContent.Type.WEAPON:
                        // 購入した武器を追加し、所持品リストを更新する
                        command = new EventDataModel.EventCommand(0, new List<string>()
                            {_selectedItemContent.SettingWeapons.basic.id, "0", "0", _tradeNum.ToString()}, null);
                        new PartyWeaponProcess().Invoke(null, ((int) EventEnum.EVENT_CODE_PARTY_WEAPON).ToString(),
                            command, null);

                        _runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
                        haveWeaponList = _runtimeSaveDataModel.runtimePartyDataModel.weapons;
                        break;

                    case ItemShopContent.Type.ARMOR:
                        // 購入した防具を追加し、所持品リストを更新する
                        command = new EventDataModel.EventCommand(0, new List<string>()
                            {_selectedItemContent.SettingArmors.basic.id, "0", "0", _tradeNum.ToString()}, null);
                        new PartyArmsProcess().Invoke(null, ((int) EventEnum.EVENT_CODE_PARTY_ARMS).ToString(), command,
                            null);

                        _runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
                        haveArmorList = _runtimeSaveDataModel.runtimePartyDataModel.armors;
                        break;
                }

                // 所持品一覧を更新
                SetupBelongingList();

                panelList[1].transform.Find("UsedItemList").Find("Num").GetComponent<Text>().text =
                    (int.Parse(panelList[1].transform.Find("UsedItemList").Find("Num").GetComponent<Text>().text)
                     + _tradeNum).ToString();
                panelList[3].transform.Find("UsedItemList").Find("Num").GetComponent<Text>().text =
                    (int.Parse(panelList[3].transform.Find("UsedItemList").Find("Num").GetComponent<Text>().text)
                     + _tradeNum).ToString();

                StatusCheck();

                //購入音
                SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.shop);
                SoundManager.Self().PlaySe();

                int priceNum = _selectedItemContent.CurrentType switch
                {
                    ItemShopContent.Type.ITEM => _processingKind == ShopState.TRADE_SCENE ? _selectedItemContent.SettingItems.basic.price : _selectedItemContent.SettingItems.basic.sell,
                    ItemShopContent.Type.WEAPON => _processingKind == ShopState.TRADE_SCENE ? _selectedItemContent.SettingWeapons.basic.price : _selectedItemContent.SettingWeapons.basic.sell,
                    ItemShopContent.Type.ARMOR => _processingKind == ShopState.TRADE_SCENE ? _selectedItemContent.SettingArmors.basic.price : _selectedItemContent.SettingArmors.basic.sell,
                    _ => 0
                };
                int haveItemNum = _selectedItemContent.ItemCount + _tradeNum;
                _selectedItemContent.SetItemCount(haveItemNum, _selectedItemContent.CurrentType);

                if (priceNum > 0)
                    _tradeNum = Math.Min(Math.Min(Math.Max(_tradeNum, 0), _maxItems - haveItemNum), Mathf.FloorToInt(_runtimeSaveDataModel.runtimePartyDataModel.gold / priceNum));
                else
                    _tradeNum = Math.Min(Math.Max(_tradeNum, 0), _maxItems - haveItemNum);

                if (_tradeNum<=0) {
                    BackWindow();
                } else {
                    _activeTradeContent.SetTradeNum(_tradeNum, priceNum * _tradeNum);
                }
            }
            // 売却
            else if (_processingKind == ShopState.TRADE_SCENE_SELL)
            {
                string id = "";
                if (_selectedItemContent.CurrentType == ItemShopContent.Type.ITEM)
                    id = _selectedItemContent.SettingItems.basic.id;
                else if (_selectedItemContent.CurrentType == ItemShopContent.Type.WEAPON)
                    id = _selectedItemContent.SettingWeapons.basic.id;
                else if (_selectedItemContent.CurrentType == ItemShopContent.Type.ARMOR)
                    id = _selectedItemContent.SettingArmors.basic.id;

                bool isReSelectItem = false;
                _runtimeSaveDataModel.runtimePartyDataModel.gold += totalPrice;
                FundageSetting();

                // タイプ毎に各値設定
                int itemNum = _selectedItemContent.ItemCount - _tradeNum;
                EventDataModel.EventCommand command;
                switch (_selectedItemContent.CurrentType)
                {
                    case ItemShopContent.Type.ITEM:
                        // 売却したアイテムを減らす
                        command = new EventDataModel.EventCommand(0, new List<string>()
                            {_selectedItemContent.SettingItems.basic.id, "1", "0", _tradeNum.ToString()}, null);
                        new PartyItemProcess().Invoke(null, ((int) EventEnum.EVENT_CODE_PARTY_ITEM).ToString(), command,
                            null);

                        // アイテム更新
                        _selectedItemContent.SetItemCount(itemNum, _selectedItemContent.CurrentType);
                        QuantityItem(-_tradeNum);
                        if (_selectedItemContent.ItemCount <= 0)
                        {
                            Destroy(_selectedItemContent);
                            _selectedItemContent = null;

                            //カーソルを戻す
                            CanMouseClick(ShopState.ITEM, true);
                            OnClickOperationButton((int) ShopState.ITEM);

                            isReSelectItem = true;
                        }
                        else
                        {
                            var dataModel = _selectedItemContent.SettingItems.basic;
                            _selectedItemContent.SetText(dataModel.name, _selectedItemContent.ItemCount.ToString(),
                                dataModel.iconId);
                            panelList[4].transform.Find("UsedItemList").Find("Num").GetComponent<Text>().text =
                                _selectedItemContent.ItemCount.ToString();
                        }

                        _runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
                        haveItemList = _runtimeSaveDataModel.runtimePartyDataModel.items;
                        break;

                    case ItemShopContent.Type.WEAPON:
                        // 売却した武器を減らす
                        command = new EventDataModel.EventCommand(0, new List<string>()
                            {_selectedItemContent.SettingWeapons.basic.id, "1", "0", _tradeNum.ToString()}, null);
                        new PartyWeaponProcess().Invoke(null, ((int) EventEnum.EVENT_CODE_PARTY_WEAPON).ToString(),
                            command, null);

                        // アイテム更新
                        _selectedItemContent.SetItemCount(itemNum, _selectedItemContent.CurrentType);
                        QuantityItem(-_tradeNum);
                        if (_selectedItemContent.ItemCount <= 0)
                        {
                            Destroy(_selectedItemContent);
                            _selectedItemContent = null;

                            //カーソルを戻す
                            CanMouseClick(ShopState.WEAPON, true);
                            OnClickOperationButton((int) ShopState.WEAPON);

                            isReSelectItem = true;
                        }
                        else
                        {
                            var dataModel = _selectedItemContent.SettingWeapons.basic;
                            _selectedItemContent.SetText(dataModel.name, _selectedItemContent.ItemCount.ToString(),
                                dataModel.iconId);
                            panelList[4].transform.Find("UsedItemList").Find("Num").GetComponent<Text>().text =
                                _selectedItemContent.ItemCount.ToString();
                        }

                        _runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
                        haveWeaponList = _runtimeSaveDataModel.runtimePartyDataModel.weapons;
                        break;

                    case ItemShopContent.Type.ARMOR:
                        // 売却した防具を減らす
                        command = new EventDataModel.EventCommand(0, new List<string>()
                            {_selectedItemContent.SettingArmors.basic.id, "1", "0", _tradeNum.ToString()}, null);
                        new PartyArmsProcess().Invoke(null, ((int) EventEnum.EVENT_CODE_PARTY_ARMS).ToString(), command,
                            null);

                        // アイテム更新
                        _selectedItemContent.SetItemCount(itemNum, _selectedItemContent.CurrentType);
                        QuantityItem(-_tradeNum);
                        if (_selectedItemContent.ItemCount <= 0)
                        {
                            Destroy(_selectedItemContent);
                            _selectedItemContent = null;

                            //カーソルを戻す
                            CanMouseClick(ShopState.ARMOR, true);
                            OnClickOperationButton((int) ShopState.ARMOR);

                            isReSelectItem = true;
                        }
                        else
                        {
                            var dataModel = _selectedItemContent.SettingArmors.basic;
                            _selectedItemContent.SetText(dataModel.name, _selectedItemContent.ItemCount.ToString(),
                                dataModel.iconId);
                            panelList[4].transform.Find("UsedItemList").Find("Num").GetComponent<Text>().text =
                                _selectedItemContent.ItemCount.ToString();
                        }

                        _runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
                        haveArmorList = _runtimeSaveDataModel.runtimePartyDataModel.armors;
                        break;
                }

                if (_selectedItemContent == null)
                {
                    BackWindow();
                }

                // 所持品一覧を更新
                SetupBelongingList();

                if (isReSelectItem)
                {
                    //アイテム選択しなおし
                    ReSelectItem();
                }

                //売却可能かどうかによって、選択可否の色を変更
                StatusCheckSell();

                //購入音（売却音）
                SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.shop);
                SoundManager.Self().PlaySe();

                // 一覧に戻らない場合、選択中のコンテンツを再設定する
                if (!isReSelectItem)
                {
                    //選択されていたアイテムを検索し、フォーカスを設定しなおす
                    //_belongingDic[ItemShopContent.Type.WEAPON]
                    List<ItemShopContent> itemShopContents = new List<ItemShopContent>();
                    if (_selectedItemContent.CurrentType == ItemShopContent.Type.ITEM)
                        itemShopContents = _belongingDic[ItemShopContent.Type.ITEM];
                    else if (_selectedItemContent.CurrentType == ItemShopContent.Type.WEAPON)
                        itemShopContents = _belongingDic[ItemShopContent.Type.WEAPON];
                    else if (_selectedItemContent.CurrentType == ItemShopContent.Type.ARMOR)
                        itemShopContents = _belongingDic[ItemShopContent.Type.ARMOR];

                    for (int i = 0; i < itemShopContents.Count; i++)
                    {
                        string idwork = "";
                        if (itemShopContents[i].CurrentType == ItemShopContent.Type.ITEM)
                            idwork = itemShopContents[i].SettingItems.basic.id;
                        else if (itemShopContents[i].CurrentType == ItemShopContent.Type.WEAPON)
                            idwork = itemShopContents[i].SettingWeapons.basic.id;
                        else if (itemShopContents[i].CurrentType == ItemShopContent.Type.ARMOR)
                            idwork = itemShopContents[i].SettingArmors.basic.id;

                        if (id == idwork)
                        {
                            //以下の一連の処理を行うことで、一覧でアイテムを再選択したのと同等の状態に変化させる
                            //Windowを閉じる
                            SellBackWindow();
                            //フォーカスを移動する
                            ChangeFocus(itemShopContents[i]);
                            //アイテムを選択する
                            ItemClicked(itemShopContents[i]);
                            break;
                        }
                    }
                }
            }
        }

        private void ReSelectItem() {
            //アイテムを選択しなおし
            if (_belongingDic[ItemShopContent.StateToType(_processingKind)].Count != 0)
            {
                var showList = _belongingDic[ItemShopContent.StateToType(_processingKind)].First();
                if (showList != null)
                {
                    showList.ContentButton.Select();

                    //選択アイテムの更新
                    _selectedItemContent = showList;
                }
            }
        }

        /// <summary>
        /// 購入可能かチェック
        /// </summary>
        void StatusCheck() {
            for (int i = 0; i < saleItemList.Count; i++)
            {
                saleItemList[i].SetupBuyable(_runtimeSaveDataModel.runtimePartyDataModel.gold);

                //一旦グレー設定を外す
                saleItemList[i].gameObject.GetComponent<WindowButtonBase>().SetGray(false);
                //そのうえでグレー設定が必要なら設定
                if (!saleItemList[i].Buyable)
                {
                    saleItemList[i].gameObject.GetComponent<WindowButtonBase>().SetGray(true);
                }
            }
        }

        /// <summary>
        /// 売却可能かどうかでUIの色を変更
        /// </summary>
        void StatusCheckSell() {
            try
            {
                List<ItemShopContent> list = _belongingDic[ItemShopContent.StateToType(_processingKind)];
                for (int i = 0; i < list.Count; i++)
                {
                    //一旦グレー設定を外す
                    list[i].gameObject.GetComponent<WindowButtonBase>().SetGray(false);
                    //そのうえでグレー設定が必要なら設定
                    if (!list[i].Sellable)
                    {
                        list[i].gameObject.GetComponent<WindowButtonBase>().SetGray(true);
                    }
                }
            } catch (Exception) { }
        }

        /// <summary>
        /// ボタン群にハイライトアニメーションのコールバックを登録する
        /// </summary>
        /// <param name="selectables">ボタン群</param>
        private void SetupButtonEvent(Button selectable) {
            EventTrigger trigger = selectable.GetComponent<EventTrigger>();
            trigger.triggers.Clear();

            EventTrigger.Entry selectEntry = new EventTrigger.Entry();
            selectEntry.eventID = EventTriggerType.Select;
            selectEntry.callback.AddListener((eventDate) =>
            {
                //アイテムの説明表示を行う
                SetFocusObject();
            });
            
            trigger.triggers.Add(selectEntry);
        }

        //イベント終了のコールバック
        private Action _eventEndProcess;

        public void CloseShop(Action endProcess) {
            _eventEndProcess = endProcess;
        }

        //今フォーカスが当たっているゲームオブジェクトの取得
        private void SetFocusObject() {
            ItemShopContent itemShopContent;
            try
            {
                _focusObject = _eventSystem.currentSelectedGameObject.gameObject;

                //陳列されているアイテムの説明の表示を行う
                if (_focusObject.name == "ItemPrefab(Clone)")
                {
                    itemShopContent = _focusObject.GetComponent<ItemShopContent>();
                    int itemNum = 0;
                    switch (itemShopContent.CurrentType)
                    {
                        case ItemShopContent.Type.ITEM:
                            if (itemShopContent.SettingItems == null) return;
                            _itemDescription.text = itemShopContent.SettingItems.basic.description;
                            var haveItem =
                                haveItemList.FirstOrDefault(v => v.itemId == itemShopContent.SettingItems.basic.id);
                            if (haveItem != null)
                                itemNum = haveItem.value;
                            break;

                        case ItemShopContent.Type.WEAPON:
                            if (itemShopContent.SettingWeapons == null) return;
                            _itemDescription.text = itemShopContent.SettingWeapons.basic.description;
                            var haveWeapon = haveWeaponList.FirstOrDefault(v =>
                                v.weaponId == itemShopContent.SettingWeapons.basic.id);
                            if (haveWeapon != null)
                                itemNum = haveWeapon.value;
                            break;

                        case ItemShopContent.Type.ARMOR:
                            if (itemShopContent.SettingArmors == null) return;
                            _itemDescription.text = itemShopContent.SettingArmors.basic.description;
                            var haveArmor = haveArmorList.FirstOrDefault(v =>
                                v.armorId == itemShopContent.SettingArmors.basic.id);
                            if (haveArmor != null)
                                itemNum = haveArmor.value;
                            break;

                        case ItemShopContent.Type.IMPORTANT:
                            if (itemShopContent.SettingItems == null) return;
                            _itemDescription.text = itemShopContent.SettingItems.basic.description;
                            haveItem = haveItemList.FirstOrDefault(v =>
                                v.itemId == itemShopContent.SettingItems.basic.id);
                            if (haveItem != null)
                                itemNum = haveItem.value;
                            break;
                    }

                    panelList[1].transform.Find("UsedItemList").Find("Num").GetComponent<Text>().text =
                        itemNum.ToString();
                    panelList[3].transform.Find("UsedItemList").Find("Num").GetComponent<Text>().text =
                        itemNum.ToString();
                    panelList[4].transform.Find("UsedItemList").Find("Num").GetComponent<Text>().text =
                        itemNum.ToString();
                }
            }
            catch
            {
            }
        }

        //カーソルのあっていないウィンドウのボタンコンポーネントをonとoffにする
        private void CanMouseClick(ShopState kind, bool isClick) {
            switch (kind)
            {
                //購入売却選択時
                case ShopState.BUY:
                    break;
                case ShopState.SELL:
                    break;

                //売却側のアイテムが選択された際
                case ShopState.ITEM:
                    break;
                case ShopState.WEAPON:
                    break;
                case ShopState.ARMOR:
                    break;
                case ShopState.IMPORTANT:
                    break;
            }
        }

        /// <summary>
        /// ボタンの有効無効状態を、現在のショップの状態に応じて切り替える
        /// </summary>
        private void changeEnabledButton() {
            _isChangingFocus = true;

            //切り替え先がNONEの場合
            if (_processingKind == ShopState.NONE)
            {
                //第一階層のボタンを有効にする
                var operationButton = _operationButtonParent.GetComponentsInChildren<Button>();
                foreach (var button in operationButton)
                {
                    button.GetComponent<WindowButtonBase>().SetEnabled(true);
                }
                //第二階層のボタンは無効にする
                var categoryButtons = _categoryButtonParent.GetComponentsInChildren<Button>();
                foreach (var button in categoryButtons)
                {
                    button.GetComponent<WindowButtonBase>().SetEnabled(false);
                }

                if (_befProcessingKind == ShopState.SELL)
                {
                    //売却ボタンを選択状態にする
                    operationButton[1].GetComponent<Button>().Select();
                }
                else
                {
                    //購入ボタンを選択状態にする
                    operationButton[0].GetComponent<Button>().Select();
                }
            }
            //切り替え先の状態が、購入の場合
            if (_processingKind == ShopState.BUY)
            {
                //第一階層のボタンは無効にする
                var operationButton = _operationButtonParent.GetComponentsInChildren<Button>();
                foreach (var button in operationButton)
                {
                    button.GetComponent<WindowButtonBase>().SetEnabled(false);
                }
                //第二階層のボタンも無効にする
                var categoryButtons = _categoryButtonParent.GetComponentsInChildren<Button>();
                foreach (var button in categoryButtons)
                {
                    button.GetComponent<WindowButtonBase>().SetEnabled(false);
                }
            }
            //切り替え先の状態が、売却の場合
            else if (_processingKind == ShopState.SELL)
            {
                //第一階層のボタンは無効にする
                var operationButton = _operationButtonParent.GetComponentsInChildren<Button>();
                foreach (var button in operationButton)
                {
                    button.GetComponent<WindowButtonBase>().SetEnabled(false);
                }
                //第二階層のボタンを有効にする
                var categoryButtons = _categoryButtonParent.GetComponentsInChildren<Button>();
                foreach (var button in categoryButtons)
                {
                    button.GetComponent<WindowButtonBase>().SetEnabled(true);
                }
                if (_befProcessingKind == ShopState.ITEM)
                {
                    //アイテムボタンを選択状態にする
                    categoryButtons[0].GetComponent<Button>().Select();
                }
                else if (_befProcessingKind == ShopState.WEAPON)
                {
                    //武器ボタンを選択状態にする
                    categoryButtons[1].GetComponent<Button>().Select();
                }
                else if (_befProcessingKind == ShopState.ARMOR)
                {
                    //防具ボタンを選択状態にする
                    categoryButtons[2].GetComponent<Button>().Select();
                }
                else if (_befProcessingKind == ShopState.IMPORTANT)
                {
                    //大事なものボタンを選択状態にする
                    categoryButtons[3].GetComponent<Button>().Select();
                }
                else
                {
                    //アイテムボタンを選択状態にする
                    categoryButtons[0].GetComponent<Button>().Select();
                }
            }
            //切り替え先の状態が、販売アイテム選択状態の場合
            else if (_processingKind == ShopState.ITEM || _processingKind == ShopState.WEAPON || _processingKind == ShopState.ARMOR || _processingKind == ShopState.IMPORTANT)
            {
                //第一階層のボタンは据え置き
                //第二階層のボタンを無効にするにする
                var categoryButtons = _categoryButtonParent.GetComponentsInChildren<Button>();
                foreach (var button in categoryButtons)
                {
                    button.GetComponent<WindowButtonBase>().SetEnabled(false);
                }
            }
            //切り替え先が、それ以外の時
            else
            {
                //特に何も行わない
            }

            _isChangingFocus = false;
        }
    }
}