using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map.Item;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TextMP = TMPro.TextMeshProUGUI;

namespace RPGMaker.Codebase.Runtime.Map.Menu
{
    public class ItemMenu : WindowBase
    {
        [SerializeField] private GameObject _armorArea = null;

        //メッセージの表示部位
        private TextMP _description1;
        private TextMP _description2;

        [SerializeField] private GameObject _importantArea = null;

        //アイテムを使う時に表示されるパーティの部分
        [SerializeField] private GameObject _itemArea = null;

        private List<ItemDataModel> _itemLists;
        private List<ItemsItem> _itemsItems = new List<ItemsItem>();
        private ItemsItem        _itemsItem;
        private string _nowMenu = "";

        //上に表示される項目の部分
        [SerializeField] private GameObject _topMenusArea = null;
        private                           TextMP     _topMenusText;
        [SerializeField] private GameObject _weaponArea = null;

        //クローンのもとになるオブジェクト
        [SerializeField] private GameObject itemsItemObject = null;
        public MenuBase MenuBase { get; private set; }

        public enum Window
        {
            ItemType = 0,
            ItemList,
            WeaponList,
            ArmorList,
            ImportantList,
            Party
        }

        private List<Button> _menu;
        private Window _state;
        private Window _befState;

        public Window State { get { return _state; } }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="manager"></param>
        public void Init(WindowBase manager) {
            _state = Window.ItemType;
            var itemDataModels = DataManager.Self().GetItemDataModels();
            _itemLists = itemDataModels;
            MenuBase = manager as MenuBase;

            //リストの初期化
            _itemsItems = new List<ItemsItem>();

            //メニューの初期化
            _menu = new List<Button>();
            for (var i = 1; i <= _topMenusArea.transform.childCount; i++)
            {
                var button = _topMenusArea.transform.Find("Item" + i).GetComponent<Button>();
                if (_topMenusArea.transform.Find("Item" + i).gameObject.activeSelf)
                    _menu.Add(button);
            }

            //十字キーでの操作登録
            for (var i = 0; i < _menu.Count; i++)
            {
                var nav = _menu[i].navigation;
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnLeft = _menu[i == 0 ? _menu.Count - 1 : i - 1];
                nav.selectOnRight = _menu[(i + 1) % _menu.Count];

                _menu[i].navigation = nav;
                _menu[i].targetGraphic = _menu[i].transform.Find("Highlight").GetComponent<Image>();
            }

            //アイテム、武器、防具、大事なものの初期化
            if (_itemArea.transform.childCount > 0)
                foreach (Transform child in _itemArea.transform)
                    GameObject.Destroy(child.gameObject);

            if (_weaponArea.transform.childCount > 0)
                foreach (Transform child in _weaponArea.transform)
                    GameObject.Destroy(child.gameObject);

            if (_armorArea.transform.childCount > 0)
                foreach (Transform child in _armorArea.transform)
                    GameObject.Destroy(child.gameObject);

            if (_importantArea.transform.childCount > 0)
                foreach (Transform child in _importantArea.transform)
                    GameObject.Destroy(child.gameObject);

            _description1 = transform.Find("MenuArea/Description/DescriptionText1").GetComponent<TextMP>();
            _description2 = transform.Find("MenuArea/Description/DescriptionText2").GetComponent<TextMP>();
            _description1.text = "";
            _description2.text = "";
            TopMenusWords();

            //共通のウィンドウの適応
            Init();

            //フォーカス制御
            ChangeFocusList(true);
        }

        /// <summary>
        /// 表示更新
        /// </summary>
        public void UpdateStatus() {
            MenusEvent(_nowMenu);
        }

        /// <summary>
        /// パーティメンバーの選択画面表示
        /// </summary>
        private void OpenParty() {
            //フォーカス制御
            _befState = _state;
            _state = Window.Party;
            ChangeFocusList(false);
        }

        /// <summary>
        /// パーティメンバーの選択画面終了
        /// </summary>
        private void CloseParty() {
            _state = _befState;
            ChangeFocusList(true);
        }

        /// <summary>
        /// リストのフォーカス位置を変更する
        /// </summary>
        private void ChangeFocusList(bool initialize) {
            if (_state == Window.ItemType)
            {
                //第一階層のメニューを選択可能とする
                int num = 0;
                for (var i = 0; i < _menu.Count; i++)
                {
                    _menu[i].GetComponent<WindowButtonBase>().SetEnabled(true);
                    if (_menu[i].GetComponent<WindowButtonBase>().IsHighlight())
                        num = i;
                }
                //先頭にフォーカスをあてる
                if (_menu.Count > 0)
                {
                    if (!initialize)
                        _menu[num].GetComponent<Button>().Select();
                    else
                        _menu[0].GetComponent<Button>().Select();
                }

                //第二階層のメニューは選択不可とする
                _itemArea.SetActive(false);

                //アイテム枠のクリア
                if (_itemsItems != null && _itemsItems.Count > 0)
                {
                    for (int i = 0; i < _itemsItems.Count; i++)
                    {
                        Object.DestroyImmediate(_itemsItems[i].gameObject);
                    }
                }
                _itemsItems = new List<ItemsItem>();
            }
            else if (_state == Window.ItemList || _state == Window.ImportantList || _state == Window.WeaponList || _state == Window.ArmorList)
            {
                //第一階層のメニューを選択不可とする
                int num = 0;
                for (var i = 0; i < _menu.Count; i++)
                {
                    _menu[i].GetComponent<WindowButtonBase>().SetEnabled(false);
                }

                //第二階層のメニューのうち、アイテムを選択可能とする
                _itemArea.SetActive(true);

                //フォーカス設定
                for (var i = 0; i < _itemsItems.Count; i++)
                {
                    var button = _itemsItems[i].gameObject.GetComponent<WindowButtonBase>();
                    button.GetComponent<WindowButtonBase>().SetEnabled(true);
                    if (button.GetComponent<WindowButtonBase>().IsHighlight())
                        num = i;
                }
                //先頭にフォーカスをあてる
                if (_itemsItems.Count > 0)
                {
                    if (_state != Window.ItemList)
                        if (!initialize)
                            _itemsItems[num].GetComponent<Button>().Select();
                        else
                            _itemsItems[0].GetComponent<Button>().Select();
                    else
                    {
                        //アイテムの場合は最終選択状態のものを初期値で選択する
                        int index = 0;

                        //最終選択したアクターを初期選択状態とする
                        if (DataManager.Self().GetRuntimeConfigDataModel()?.commandRemember == 1)
                        {
                            var runtimePartyDataModel = DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel;
                            for (int i = 0; i < _itemsItems.Count; i++)
                                if (runtimePartyDataModel.lastItem.itemId == _itemsItems[i].ItemId())
                                {
                                    index = i;
                                    break;
                                }
                        }
                        //カーソル記憶が働いてない場合はinitializeフラグを見る
                        else if (!initialize)
                            index = num;

                        _itemsItems[index].GetComponent<Button>().Select();
                    }
                }
            }
            else
            {
                //第一階層のメニューを選択不可とする
                for (var i = 0; i < _menu.Count; i++)
                {
                    _menu[i].GetComponent<WindowButtonBase>().SetEnabled(false);
                }
                //第二階層も選択不可とする
                for (var i = 0; i < _itemsItems.Count; i++)
                {
                    var button = _itemsItems[i].gameObject.GetComponent<WindowButtonBase>();
                    button.GetComponent<WindowButtonBase>().SetEnabled(false);
                }
            }
        }

        /// <summary>
        /// 上の項目による表示切替
        /// </summary>
        /// <param name="menus"></param>
        public void MenusEvent(string menus) {
            //メッセージ枠のクリア
            DescriptionClaer();
            _nowMenu = menus;

            //アイテム枠のクリア
            if (_itemsItems != null && _itemsItems.Count > 0)
            {
                for (int i = 0; i < _itemsItems.Count; i++)
                {
                    Object.DestroyImmediate(_itemsItems[i].gameObject);
                }
            }
            _itemsItems = new List<ItemsItem>();

            //選択された項目ごとにリスト更新
            switch (menus)
            {
                case "item":
                    _item();
                    break;
                case "weapon":
                    _weapon();
                    break;
                case "armor":
                    _armor();
                    break;
                case "important":
                    _important();
                    break;
            }
        }

        /// <summary>
        /// アイテム表示
        /// </summary>
        private void _item() {
            var runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
            for (var i = 0; i < runtimeSaveDataModel.runtimePartyDataModel.items.Count; i++)
            {
                var itemId = runtimeSaveDataModel.runtimePartyDataModel.items[i].itemId;
                var itemDataModel = DataManager.Self().GetItemDataModels()
                    .FirstOrDefault(t => t.basic.id == itemId);

                //アイテムかの判定
                if (itemDataModel != null && _GetItemType(runtimeSaveDataModel.runtimePartyDataModel.items[i].itemId) ==
                    (int) ItemEnums.ItemType.NORMAL && runtimeSaveDataModel.runtimePartyDataModel.items[i].value > 0)
                {
                    //アイテムの表示項目のクローンを生成
                    var item = Instantiate(itemsItemObject);
                    item.transform.SetParent(_itemArea.transform, false);
                    item.SetActive(true);
                    item.name = "item" + (i + 1);
                    //項目の代入を開始
                    _itemsItem = item.AddComponent<ItemsItem>();
                    _itemsItem.Init(runtimeSaveDataModel.runtimePartyDataModel.items[i].itemId,
                        runtimeSaveDataModel.runtimePartyDataModel.items[i].value.ToString(),
                        "item", this, OpenParty, CloseParty);
                    _itemsItems.Add(_itemsItem);
                }
            }

            if (_itemsItems.Count == 0)
            {
                //0件の場合は空のアイテムを1つだけ追加する
                var item = Instantiate(itemsItemObject);
                item.transform.SetParent(_itemArea.transform, false);
                item.SetActive(true);
                item.name = "item1";
                //項目の代入を開始
                _itemsItem = item.AddComponent<ItemsItem>();
                _itemsItem.Init("", "", "", this, null, CloseParty);
                _itemsItems.Add(_itemsItem);
            }

            //十字キーでの操作登録
            var selects = _itemArea.GetComponentsInChildren<Button>();
            if (selects.Length > 1)
            {
                for (var i = 0; i < selects.Length; i++)
                {
                    var nav = selects[i].navigation;
                    nav.mode = Navigation.Mode.Explicit;

                    nav.selectOnLeft = selects[i == 0 ? selects.Length - 1 : i - 1];
                    nav.selectOnUp = selects[i < 2 ? selects.Length - System.Math.Abs(i - 2) : i - 2];
                    nav.selectOnRight = selects[(i + 1) % selects.Length];
                    nav.selectOnDown = selects[(i + 2) % selects.Length];

                    selects[i].navigation = nav;
                    selects[i].targetGraphic = selects[i].transform.Find("Highlight").GetComponent<Image>();
                }
            }
            else if (selects.Length == 1)
            {
                var nav = selects[0].navigation;
                nav.mode = Navigation.Mode.None;
                selects[0].navigation = nav;
            }

            //フォーカス制御
            _state = Window.ItemList;
            ChangeFocusList(true);
        }

        /// <summary>
        /// 大事なものの表示
        /// </summary>
        private void _important() {
            var runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
            for (var i = 0; i < runtimeSaveDataModel.runtimePartyDataModel.items.Count; i++)
            {
                var itemId = runtimeSaveDataModel.runtimePartyDataModel.items[i].itemId;
                var itemDataModel = DataManager.Self().GetItemDataModels()
                    .FirstOrDefault(t => t.basic.id == itemId);
                //アイテムかの判定
                if (itemDataModel != null && _GetItemType(runtimeSaveDataModel.runtimePartyDataModel.items[i].itemId) ==
                    (int) ItemEnums.ItemType.IMPORTANT && runtimeSaveDataModel.runtimePartyDataModel.items[i].value > 0)
                {
                    //アイテムの表示項目のクローンを生成
                    //Clone(i);
                    var item = Instantiate(itemsItemObject);
                    item.transform.SetParent(_importantArea.transform, false);
                    item.SetActive(true);
                    item.name = "item" + (i + 1);
                    //項目の代入を開始
                    _itemsItem = item.AddComponent<ItemsItem>();
                    _itemsItem.Init(runtimeSaveDataModel.runtimePartyDataModel.items[i].itemId,
                        runtimeSaveDataModel.runtimePartyDataModel.items[i].value.ToString(),
                        "important", this, OpenParty, CloseParty);
                    _itemsItems.Add(_itemsItem);
                }
            }

            if (_itemsItems.Count == 0)
            {
                //0件の場合は空のアイテムを1つだけ追加する
                var item = Instantiate(itemsItemObject);
                item.transform.SetParent(_itemArea.transform, false);
                item.SetActive(true);
                item.name = "item1";
                //項目の代入を開始
                _itemsItem = item.AddComponent<ItemsItem>();
                _itemsItem.Init("", "", "", this, null, CloseParty);
                _itemsItems.Add(_itemsItem);
            }

            //十字キーでの操作登録
            var selects = _importantArea.GetComponentsInChildren<Button>();
            if (selects.Length > 1)
            {
                for (var i = 0; i < selects.Length; i++)
                {
                    var nav = selects[i].navigation;
                    nav.mode = Navigation.Mode.Explicit;

                    nav.selectOnLeft = selects[i == 0 ? selects.Length - 1 : i - 1];
                    nav.selectOnUp = selects[i < 2 ? selects.Length - System.Math.Abs(i - 2) : i - 2];
                    nav.selectOnRight = selects[(i + 1) % selects.Length];
                    nav.selectOnDown = selects[(i + 2) % selects.Length];

                    selects[i].navigation = nav;
                    selects[i].targetGraphic = selects[i].transform.Find("Highlight").GetComponent<Image>();
                }
            }
            else if (selects.Length == 1)
            {
                var nav = selects[0].navigation;
                nav.mode = Navigation.Mode.None;
                selects[0].navigation = nav;
            }

            //フォーカス制御
            _state = Window.ImportantList;
            ChangeFocusList(true);
        }

        /// <summary>
        /// 武器表示
        /// </summary>
        private void _weapon() {
            var runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
            for (var i = 0; i < runtimeSaveDataModel.runtimePartyDataModel.weapons.Count; i++)
            {
                var itemId = runtimeSaveDataModel.runtimePartyDataModel.weapons[i].weaponId;
                var weaponDataModel = DataManager.Self().GetWeaponDataModels()
                    .FirstOrDefault(t => t.basic.id == itemId);

                //IDが空の場合にクローンしない
                if (weaponDataModel != null && runtimeSaveDataModel.runtimePartyDataModel.weapons[i].weaponId != "" &&
                    runtimeSaveDataModel.runtimePartyDataModel.weapons[i].value > 0)
                {
                    //武器の表示項目のクローンを生成
                    //Clone(i);
                    var item = Instantiate(itemsItemObject);
                    item.transform.SetParent(_weaponArea.transform, false);
                    item.SetActive(true);
                    item.name = "item" + (i + 1);
                    //項目の代入を開始
                    _itemsItem = item.AddComponent<ItemsItem>();
                    _itemsItem.Init(runtimeSaveDataModel.runtimePartyDataModel.weapons[i].weaponId,
                        runtimeSaveDataModel.runtimePartyDataModel.weapons[i].value.ToString(),
                        "weapon", this, OpenParty, CloseParty);
                    _itemsItems.Add(_itemsItem);
                }
            }

            if (_itemsItems.Count == 0)
            {
                //0件の場合は空のアイテムを1つだけ追加する
                var item = Instantiate(itemsItemObject);
                item.transform.SetParent(_itemArea.transform, false);
                item.SetActive(true);
                item.name = "item1";
                //項目の代入を開始
                _itemsItem = item.AddComponent<ItemsItem>();
                _itemsItem.Init("", "", "", this, null, CloseParty);
                _itemsItems.Add(_itemsItem);
            }

            //十字キーでの操作登録
            var selects = _weaponArea.GetComponentsInChildren<Button>();
            if (selects.Length > 1)
            {
                for (var i = 0; i < selects.Length; i++)
                {
                    var nav = selects[i].navigation;
                    nav.mode = Navigation.Mode.Explicit;

                    nav.selectOnLeft = selects[i == 0 ? selects.Length - 1 : i - 1];
                    nav.selectOnUp = selects[i < 2 ? selects.Length - System.Math.Abs(i - 2) : i - 2];
                    nav.selectOnRight = selects[(i + 1) % selects.Length];
                    nav.selectOnDown = selects[(i + 2) % selects.Length];

                    selects[i].navigation = nav;
                    selects[i].targetGraphic = selects[i].transform.Find("Highlight").GetComponent<Image>();
                }
            }
            else if (selects.Length == 1)
            {
                var nav = selects[0].navigation;
                nav.mode = Navigation.Mode.None;
                selects[0].navigation = nav;
            }

            //フォーカス制御
            _state = Window.WeaponList;
            ChangeFocusList(true);
        }

        /// <summary>
        /// 防具表示
        /// </summary>
        private void _armor() {
            var runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
            for (var i = 0; i < runtimeSaveDataModel.runtimePartyDataModel.armors.Count; i++)
            {
                var itemId = runtimeSaveDataModel.runtimePartyDataModel.armors[i].armorId;
                var armorDataModel = DataManager.Self().GetArmorDataModels()
                    .FirstOrDefault(t => t.basic.id == itemId);
                //IDが空の場合にクローンしない
                if (armorDataModel != null && runtimeSaveDataModel.runtimePartyDataModel.armors[i].armorId != "" &&
                    runtimeSaveDataModel.runtimePartyDataModel.armors[i].value > 0)
                {
                    //防具の表示項目のクローンを生成
                    //Clone(i);
                    var item = Instantiate(itemsItemObject);
                    item.transform.SetParent(_armorArea.transform, false);
                    item.SetActive(true);
                    item.name = "item" + (i + 1);
                    //項目の代入を開始
                    _itemsItem = item.AddComponent<ItemsItem>();
                    _itemsItem.Init(runtimeSaveDataModel.runtimePartyDataModel.armors[i].armorId,
                        runtimeSaveDataModel.runtimePartyDataModel.armors[i].value.ToString(), "armor",
                        this, OpenParty, CloseParty);
                    _itemsItems.Add(_itemsItem);
                }
            }

            if (_itemsItems.Count == 0)
            {
                //0件の場合は空のアイテムを1つだけ追加する
                var item = Instantiate(itemsItemObject);
                item.transform.SetParent(_itemArea.transform, false);
                item.SetActive(true);
                item.name = "item1";
                //項目の代入を開始
                _itemsItem = item.AddComponent<ItemsItem>();
                _itemsItem.Init("", "", "", this, null, CloseParty);
                _itemsItems.Add(_itemsItem);
            }

            //十字キーでの操作登録
            var selects = _armorArea.GetComponentsInChildren<Button>();
            if (selects.Length > 1)
            {
                for (var i = 0; i < selects.Length; i++)
                {
                    var nav = selects[i].navigation;
                    nav.mode = Navigation.Mode.Explicit;

                    nav.selectOnLeft = selects[i == 0 ? selects.Length - 1 : i - 1];
                    nav.selectOnUp = selects[i < 2 ? selects.Length - System.Math.Abs(i - 2) : i - 2];
                    nav.selectOnRight = selects[(i + 1) % selects.Length];
                    nav.selectOnDown = selects[(i + 2) % selects.Length];

                    selects[i].navigation = nav;
                    selects[i].targetGraphic = selects[i].transform.Find("Highlight").GetComponent<Image>();
                }
            }
            else if (selects.Length == 1)
            {
                var nav = selects[0].navigation;
                nav.mode = Navigation.Mode.None;
                selects[0].navigation = nav;
            }

            //フォーカス制御
            _state = Window.ArmorList;
            ChangeFocusList(true);
        }

        /// <summary>
        /// アイテム種別を返却
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private int _GetItemType(string id) {
            for (var i = 0; i < _itemLists.Count; i++)
                if (_itemLists[i].basic.id == id)
                    return _itemLists[i].basic.itemType;

            return 0;
        }

        /// <summary>
        /// 説明文表示
        /// </summary>
        /// <param name="descriptionText"></param>
        public void Description(string descriptionText) {
            var width = _description1.transform.parent.GetComponent<RectTransform>().sizeDelta + _description1.GetComponent<RectTransform>().sizeDelta;
            _description1.text = "";
            _description2.text = "";
            if (descriptionText.Contains("\\n"))
            {
                var textList = descriptionText.Split("\\n");
                _description1.text = textList[0];
                if (width.x >= _description1.preferredWidth)
                {
                    _description2.text = textList[1];
                    return;
                }
                descriptionText = textList[0];
            }
            var isNextLine = false;
            _description1.text = "";
            _description2.text = "";
            for (int i = 0; i < descriptionText.Length; i++)
            {
                if (!isNextLine)
                {
                    _description1.text += descriptionText[i];
                    if (width.x <= _description1.preferredWidth)
                    {
                        var lastChara = _description1.text.Substring(_description1.text.Length - 1);
                        _description2.text += lastChara;
                        _description1.text = _description1.text.Remove(_description1.text.Length - 1);
                        isNextLine = true;
                    }
                }
                else
                {
                    _description2.text += descriptionText[i];
                    if (width.x <= _description2.preferredWidth)
                    {
                        _description2.text = _description2.text.Remove(_description2.text.Length - 1);
                        break;
                    }
                }

            }
        }

        /// <summary>
        /// 説明文初期化
        /// </summary>
        public void DescriptionClaer() {
            _description1.text = "";
            _description2.text = "";
        }

        /// <summary>
        /// 戻る処理
        /// </summary>
        public new void Back() {
            if (_state == Window.ItemList || _state == Window.WeaponList || _state == Window.ArmorList || _state == Window.ImportantList)
            {
                _state = Window.ItemType;
                ChangeFocusList(false);
            }
            else if (_state == Window.Party)
            {
                //あり得ないが、万が一入ってきた場合は各リスト表示の状態に戻る
                _state = _befState;
                ChangeFocusList(false);
            }
            else
            {
                //あり得ないが、万が一入ってきたらメインメニューに戻る
                MenuBase.BackMenu();
            }
        }

        /// <summary>
        /// 上に表示される項目の部分の用語
        /// </summary>
        private void TopMenusWords() {
            for (var i = 1; i <= _topMenusArea.transform.childCount; i++)
            {
                _topMenusText = _topMenusArea.transform.Find("Item" + i + "/Name").GetComponent<TextMP>();
                switch (i)
                {
                    case 1:
                        _topMenusText.text = TextManager.item;
                        break;
                    case 2:
                        _topMenusText.text = TextManager.weapon;
                        break;
                    case 3:
                        _topMenusText.text = TextManager.armor;
                        break;
                    case 4:
                        _topMenusText.text = TextManager.keyItem;
                        break;
                }
            }
        }
    }
}