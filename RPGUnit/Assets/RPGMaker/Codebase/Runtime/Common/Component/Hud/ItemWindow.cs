using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Runtime.Common.Enum;
using RPGMaker.Codebase.Runtime.Shop;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item.ItemDataModel;

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud
{
    public class ItemWindow : WindowBase
    {
        [SerializeField] private ItemShopContent _itemPrefab = null;
        [SerializeField] private List<Transform> contentParent = new List<Transform>();
        [SerializeField] private List<GameObject> panelList = new List<GameObject>();
        [SerializeField] private List<GameObject> sellPanelList = new List<GameObject>();

        /// <summary>
        /// 戻るボタン
        /// </summary>
        public Button BackOperationButton;

        /// <summary>
        /// 所持アイテムのうち選択可能なアイテムのリスト
        /// </summary>
        private List<ItemShopContent> itemList = null;

        /// <summary>
        /// マスターデータを格納するリスト
        /// </summary>
        private List<ItemDataModel> _items;

        /// <summary>
        /// アイテム種別
        /// </summary>
        private int _itemType;

        /// <summary>
        /// 現在選択中のアイテム
        /// </summary>
        private ItemShopContent _selectedItemContent;

        /// <summary>
        /// 所持アイテムリスト
        /// </summary>
        List<RuntimePartyDataModel.Item> haveItemList = new List<RuntimePartyDataModel.Item>();

        /// <summary>
        /// セーブデータ
        /// </summary>
        private RuntimeSaveDataModel _runtimeSaveDataModel;

        /// <summary>
        /// スクロール関連
        /// </summary>
        private ScrollRect _nowScrollRect;

        /// <summary>
        /// 処理開始
        /// </summary>
        private void Start() {
            //共通UIの適応を開始
            Init();

            //セーブデータの取得
            _runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
            //所持アイテム初期化
            haveItemList = _runtimeSaveDataModel.runtimePartyDataModel.items;

            //アイテムのマスタデータ取得
            var _databaseManagementService = new DatabaseManagementService();
            _items = _databaseManagementService.LoadItem();

            //所持アイテムのうち、選択可能なアイテムリストの初期化
            itemList = new List<ItemShopContent>();

            //戻るボタン、右クリックイベント登録
            InputDistributor.AddInputHandler(
                GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT,
                HandleType.Back, BackWindow);

            //Windowの表示位置を更新
            switch (DataManager.Self().GetUiSettingDataModel().talkMenu.itemSelectMenu.positionItemWindow)
            {
                case 0:
                    transform.Find("ItemSelect").GetComponent<RectTransform>().localPosition = new Vector2(0, 250.0f);
                    transform.Find("ItemSelect/Back").GetComponent<RectTransform>().localPosition = new Vector2(920.0f, -250.0f);
                    break;
                case 1:
                    transform.Find("ItemSelect").GetComponent<RectTransform>().localPosition = new Vector2(0, 0.0f);
                    transform.Find("ItemSelect/Back").GetComponent<RectTransform>().localPosition = new Vector2(920.0f, 320.0f);
                    break;
                case 2:
                    transform.Find("ItemSelect").GetComponent<RectTransform>().localPosition = new Vector2(0, -250.0f);
                    transform.Find("ItemSelect/Back").GetComponent<RectTransform>().localPosition = new Vector2(920.0f, 320.0f);
                    break;
            }

            foreach (Transform child in contentParent[0].transform)
            {
                GameObject.DestroyImmediate(child.gameObject);
            }

            //アイテム選択Window表示
            SetupBelongingList();

            BackOperationButton?.GetComponent<WindowButtonBase>().SetSilentClick(true);
        }

        /// <summary>
        /// アイテムタイプを設定し、アイテム選択Windowを表示する
        /// </summary>
        public void SetItemType(int itemType) {
            _itemType = itemType;
        }

        private void DestroyObject() {
            //戻るボタン、右クリックイベントの破棄
            InputDistributor.RemoveInputHandler(
                GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT,
                HandleType.Back, BackWindow);
        }

        /// <summary>
        /// 所持品一覧の初期化
        /// </summary>
        private void SetupBelongingList() {
            itemList.ForEach(v => Destroy(v.gameObject));
            itemList.Clear();

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
                    if (item.basic.itemType == _itemType)
                        itemList.Add(
                            CreateContent(i, contentParent[0], item)
                        );
                }
            }

            // アイテム所持数が0の場合に空アイテム追加
            if (itemList.Count == 0)
            {
                itemList.Add(CreateContent(0, contentParent[0], ItemDataModel.CreateDefault("")));
                itemList[0].SetText("","","");
            }

            // ボタンのNavgationを設定。所持品は2列で表示されるので上下左右キーに対応
            //ボタンが1つしかない場合にはnavigationの設定をしない
            if (itemList.Count > 1)
            {
                for (int i = 0; i < itemList.Count; i++)
                {
                    var selectable = itemList[i].ContentButton;
                    selectable.targetGraphic = selectable.transform.Find("Highlight").GetComponent<Image>();
                    var nav = selectable.navigation;
                    nav.mode = Navigation.Mode.Explicit;
                    nav.selectOnLeft = itemList[i == 0 ? itemList.Count - 1 : i - 1].ContentButton;
                    nav.selectOnUp = itemList[i < 2 ? itemList.Count - Math.Abs(i - 2) : i - 2].ContentButton;
                    nav.selectOnRight = itemList[(i + 1) % itemList.Count].ContentButton;
                    nav.selectOnDown = itemList[(i + 2) % itemList.Count].ContentButton;
                    selectable.navigation = nav;
                }
            }
            if (itemList.Count > 0)
            {
                //最初のアイテムを選択状態にする
                itemList[0].ContentButton.Select();
            }

            contentParent[0].GetComponent<RectTransform>().sizeDelta =
                new Vector2(contentParent[0].GetComponent<RectTransform>().sizeDelta.x, (itemList.Count % 2) * 70);
        }

        /// <summary>
        /// アイテム要素の作成 表示要素はショップと同じものを利用する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="i"></param>
        /// <param name="parent"></param>
        /// <param name="itemData"></param>
        /// <returns></returns>
        public ItemShopContent CreateContent<T>(
            int i,
            Transform parent,
            T itemData
        ) {
            if (parent == null)
                parent = contentParent[0];
            ItemShopContent itemContent = Instantiate(_itemPrefab, parent);
            _itemPrefab.gameObject.SetActive(false);
            itemContent.gameObject.SetActive(true);
            itemContent.ContentButton.GetComponent<WindowButtonBase>().OnFocus = new UnityEngine.Events.UnityEvent();
            itemContent.ContentButton.GetComponent<WindowButtonBase>().OnFocus.AddListener(() => ChangeFocus(itemContent));
            itemContent.ContentButton.GetComponent<WindowButtonBase>().OnClick = new Button.ButtonClickedEvent();
            itemContent.ContentButton.GetComponent<WindowButtonBase>().OnClick.AddListener(() => ItemClicked(itemContent));
            //itemContent.ContentButton.onClick.AddListener(() => ItemClicked(itemContent));

            ItemDataSetting(itemContent, i, itemData);
            return itemContent;
        }

        /// <summary>
        /// 所持アイテムの設定
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemContent"></param>
        /// <param name="i"></param>
        /// <param name="itemData"></param>
        void ItemDataSetting<T>(
            ItemShopContent itemContent,
            int i,
            T itemData
        ) {
            // アイテムの情報を設定
            itemContent.SettingItems = itemData as ItemDataModel;
            ItemBasic itemBasic = itemContent.SettingItems.basic;
            if (haveItemList != null && haveItemList.Count > i) 
                itemContent.SetItemCount(haveItemList[i].value, ItemShopContent.Type.ITEM);
            else
                itemContent.SetItemCount(1, ItemShopContent.Type.ITEM);

            // テキストの色を指定して表示
            itemContent.SetTextColor(itemBasic.name, itemContent.ItemCount.ToString(), itemBasic.iconId,
                new Color(DataManager.Self().GetUiSettingDataModel().talkMenu.itemSelectMenu.menuFontSetting.color[0],
                    DataManager.Self().GetUiSettingDataModel().talkMenu.itemSelectMenu.menuFontSetting.color[1],
                    DataManager.Self().GetUiSettingDataModel().talkMenu.itemSelectMenu.menuFontSetting.color[2],
                    255), itemBasic.itemType);
        }

        /// <summary>
        /// 戻る操作
        /// </summary>
        public void BackWindow() {
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.cancel);
            SoundManager.Self().PlaySe();

            _selectedItemContent = null;
            DestroyObject();
            Destroy(gameObject);
            _eventEndProcess?.Invoke();
        }

        private void ChangeFocus(ItemShopContent item) {
            _selectedItemContent = item;
        }

        /// <summary>
        /// 要素選択時の処理
        /// </summary>
        /// <param name="item"></param>
        private void ItemClicked(ItemShopContent item) {
            //このアイテムの選択処理
            DestroyObject();
            Destroy(gameObject);
            _eventEndProcess?.Invoke();

            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.ok);
        }

        public int GetSelectedItem() {
            if (_selectedItemContent != null)
            {
                return _selectedItemContent.SettingItems.SerialNumber;
            }
            //キャンセル操作の場合は 0 を返却する
            return 0;
        }

        /// <summary>
        /// プレビュー表示用 ItemShopContent削除
        /// </summary>
        public void ClearItemList() {
            //Windowの表示位置を更新
            switch (DataManager.Self().GetUiSettingDataModel().talkMenu.itemSelectMenu.positionItemWindow)
            {
                case 0:
                    transform.Find("ItemSelect").GetComponent<RectTransform>().localPosition = new Vector2(0, 250.0f);
                    transform.Find("ItemSelect/Back").GetComponent<RectTransform>().localPosition = new Vector2(920.0f, -250.0f);
                    break;
                case 1:
                    transform.Find("ItemSelect").GetComponent<RectTransform>().localPosition = new Vector2(0, 0.0f);
                    transform.Find("ItemSelect/Back").GetComponent<RectTransform>().localPosition = new Vector2(920.0f, 320.0f);
                    break;
                case 2:
                    transform.Find("ItemSelect").GetComponent<RectTransform>().localPosition = new Vector2(0, -250.0f);
                    transform.Find("ItemSelect/Back").GetComponent<RectTransform>().localPosition = new Vector2(920.0f, 320.0f);
                    break;
            }

            foreach (Transform child in contentParent[0].transform)
            {
                GameObject.DestroyImmediate(child.gameObject);
            }
        }

        //イベント終了のコールバック
        private Action _eventEndProcess;

        public void CloseItemWindow(Action endProcess) {
            _eventEndProcess = endProcess;
        }
    }
}