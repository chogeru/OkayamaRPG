using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TextMP = TMPro.TextMeshProUGUI;

namespace RPGMaker.Codebase.Runtime.Map.Item
{
    public class ItemsItem : MonoBehaviour
    {
        private ArmorDataModel       _armorDataModel;
        private List<ArmorDataModel> _armorDataModels;

        private GameItem _gameItem;

        private Image _icon;

        private ItemDataModel _itemDataModel;

        private List<ItemDataModel> _itemDataModels;
        private ItemMenu            _itemMenu;

        //今何の項目を使っているかを保持しておく
        private string _menus;
        private string _itemId;
        private string _itemValue;
        private TextMP _name;
        private TextMP _space;
        private TextMP _value;
        private List<WeaponDataModel> _weaponDataModels;
        private WeaponDataModel _weaponList;

        private Action _openAction;
        private Action _closeAction;

        private bool isUse = false;

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="itemValue"></param>
        /// <param name="menus"></param>
        /// <param name="itemMenu"></param>
        /// <param name="openAction"></param>
        /// <param name="closeAction"></param>
        public void Init(string itemId, string itemValue, string menus, ItemMenu itemMenu, Action openAction, Action closeAction) {
            _menus = menus;
            _itemId = itemId;
            _itemValue = itemValue;
            _itemMenu = itemMenu;
            _itemDataModels =
                DataManager.Self().GetItemDataModels();
            _weaponDataModels =
                DataManager.Self().GetWeaponDataModels();
            _armorDataModels =
                DataManager.Self().GetArmorDataModels();

            _icon = transform.Find("Icon").GetComponent<Image>();
            _name = transform.Find("Name").GetComponent<TextMP>();
            _space = transform.Find("Space").GetComponent<TextMP>();
            _value = transform.Find("Value").GetComponent<TextMP>();

            if (transform.GetComponent<WindowButtonBase>() != null)
            {
                transform.GetComponent<WindowButtonBase>().OnFocus = new UnityEngine.Events.UnityEvent();
                transform.GetComponent<WindowButtonBase>().OnFocus.AddListener(ItemEvent);
                transform.GetComponent<WindowButtonBase>().OnClick = new Button.ButtonClickedEvent();
                transform.GetComponent<WindowButtonBase>().OnClick.AddListener(ButtonEvent);
            }

            _space.enabled = true;
            _value.enabled = true;

            switch (menus)
            {
                case "item":
                    _itemDataModel = DataManager.Self().GetItemDataModels()
                        .FirstOrDefault(t => t.basic.id == itemId);
                    _name.text = _itemDataModel.basic.name;
                    _icon.sprite = GetItemImage(_itemDataModel.basic.iconId);
                    if (_gameItem == null) _gameItem = new GameItem(itemId, GameItem.DataClassEnum.Item);

                    break;
                case "weapon":
                    _weaponList = DataManager.Self().GetWeaponDataModels()
                        .FirstOrDefault(t => t.basic.id == itemId);
                    _name.text = _weaponList.basic.name;
                    _icon.sprite = GetItemImage(_weaponList.basic.iconId);
                    if (_gameItem == null) _gameItem = new GameItem(itemId, GameItem.DataClassEnum.Weapon);

                    break;
                case "armor":
                    _armorDataModel = DataManager.Self().GetArmorDataModels()
                        .FirstOrDefault(t => t.basic.id == itemId);
                    _name.text = _armorDataModel.basic.name;
                    _icon.sprite = GetItemImage(_armorDataModel.basic.iconId);
                    if (_gameItem == null) _gameItem = new GameItem(itemId, GameItem.DataClassEnum.Armor);

                    break;
                case "important":
                    _itemDataModel = DataManager.Self().GetItemDataModels()
                        .FirstOrDefault(t => t.basic.id == itemId);
                    _name.text = _itemDataModel.basic.name;
                    _icon.sprite = GetItemImage(_itemDataModel.basic.iconId);
                    if (_gameItem == null) _gameItem = new GameItem(itemId, GameItem.DataClassEnum.Item);
                    _space.enabled = DataManager.Self().GetSystemDataModel().optionSetting.showKeyItemNum == 1;
                    _value.enabled = DataManager.Self().GetSystemDataModel().optionSetting.showKeyItemNum == 1;
                    break;
                default:
                    _icon.gameObject.SetActive(false);
                    break;
            }

            _openAction = openAction;
            _closeAction = closeAction;

            _value.text = itemValue;

            if (itemId == "")
            {
                transform.Find("Space").gameObject.SetActive(false);
            }
            else
            {
                transform.Find("Space").gameObject.SetActive(true);
            }

            UpdateStatus();
        }

        /// <summary>
        /// アイテムIDを返却する
        /// </summary>
        /// <returns></returns>
        public string ItemId() {
            return _itemId;
        }

        /// <summary>
        /// アイテムのデータを更新
        /// </summary>
        private void UpdateStatus() {
            //アイテム利用可否判定のために、パーティメンバーの1人目を取得
            var party = DataManager.Self().GetGameParty();
            if (party.Members().Count == 0)
            {
                return;
            }
            var actor = party.Members()[0] as GameActor;
            var gameAction = new GameAction(actor);

            //アイテムの種類設定
            ItemEnums.ItemCanUseTiming canUseTiming = 0;
            switch (_menus)
            {
                case "item":
                case "important":
                    gameAction.SetItem(_itemDataModel.basic.id);
                    canUseTiming = (ItemEnums.ItemCanUseTiming)_itemDataModel.basic.canUseTiming;
                    break;
                case "weapon":
                    canUseTiming = ItemEnums.ItemCanUseTiming.NONE;
                    gameAction.SetItem(_weaponList.basic.id);
                    break;
                case "armor":
                    canUseTiming = ItemEnums.ItemCanUseTiming.NONE;
                    gameAction.SetItem(_armorDataModel.basic.id);
                    break;
            }

            //アイテムを使用可能かどうか
            isUse = false;
            switch (canUseTiming)
            {
                case ItemEnums.ItemCanUseTiming.ALL:
                case ItemEnums.ItemCanUseTiming.MENU:
                    isUse = true;
                    break;
                case ItemEnums.ItemCanUseTiming.BATTLE:
                case ItemEnums.ItemCanUseTiming.NONE:
                    isUse = false;
                    break;
            }

            //メニューで使用可能である
            if (isUse)
            {
                //対象者が味方を含んでいるかどうか
                isUse = CanUse();

                //対象者に味方が含まれていないケースであっても、コモンイベントが設定されている場合には使用可能
                if (!isUse)
                {
                    //コモンイベントが設定されているかどうか
                    bool isCommonEvent = false;
                    bool isCommonEventForUser = false;

                    if (_gameItem != null)
                    {

                        //コモンイベントが設定されているか
                        foreach (var effect in _gameItem.Effects)
                        {
                            if (isCommonEvent) break;
                            isCommonEvent = gameAction.IsEffectCommonEvent(effect);
                        }

                        //使用者への影響にチェックが入っている場合、使用者への影響側のコモンイベントも確認
                        if (gameAction.IsForUser())
                            foreach (var effect in gameAction.Item.EffectsMyself)
                            {
                                if (isCommonEventForUser) break;
                                isCommonEventForUser = gameAction.IsEffectCommonEvent(effect);
                            }

                        if (isCommonEvent || isCommonEventForUser) isUse = true;
                    }
                }
            }

            //アイテムを使用可能かどうかによって、表示の切り替え
            if (!isUse)
            {
                _name.color = Color.gray;
                _icon.color = Color.gray;
                _value.color = Color.gray;
                transform.GetComponent<WindowButtonBase>().SetGray(true);
            }
            else
            {
                _name.color = Color.white;
                _icon.color = Color.white;
                _value.color = Color.white;
                transform.GetComponent<WindowButtonBase>().SetGray();
            }
        }

        /// <summary>
        /// アイテム画像取得
        /// </summary>
        /// <param name="iconName"></param>
        /// <returns></returns>
        public Sprite GetItemImage(string iconName) {
            var iconSetTexture =
                UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                    "Assets/RPGMaker/Storage/Images/System/IconSet/" + iconName + ".png");

            var iconTexture = iconSetTexture;
            if (iconTexture == null)
            {
                _icon.gameObject.SetActive(false);
                return null;
            }

            var sprite = Sprite.Create(
                iconTexture,
                new Rect(0, 0, iconTexture.width, iconTexture.height),
                new Vector2(0.5f, 0.5f)
            );

            var aspect = ImageManager.FixAspect( new Vector2(66f,66f), new Vector2(iconTexture.width, iconTexture.height));
            var aspectRatio = _icon.GetComponent<AspectRatioFitter>();
            if (aspectRatio == null)
            {
                aspectRatio = _icon.gameObject.AddComponent<AspectRatioFitter>();
                aspectRatio.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
            }
            aspectRatio.aspectRatio = aspect;

            return sprite;
        }

        /// <summary>
        /// アイテムの説明文表示
        /// </summary>
        public void ItemEvent() {
            switch (_menus)
            {
                case "item":
                    _itemMenu.Description(_itemDataModel.basic.description);
                    break;
                case "weapon":
                    _itemMenu.Description(_weaponList.basic.description);
                    break;
                case "armor":
                    _itemMenu.Description(_armorDataModel.basic.description);
                    break;
                case "important":
                    _itemMenu.Description(_itemDataModel.basic.description);
                    break;
            }
        }

        /// <summary>
        /// アイテムを使用する
        /// </summary>
        public void ButtonEvent() {
            if (_menus == "item" || _menus == "important")
            {
                //アイテム利用可否判定のために、パーティメンバーの1人目を取得
                var party = DataManager.Self().GetGameParty();
                if (party.Members().Count == 0)
                {
                    //パーティメンバーが1人もいない場合は、アイテム使用不可
                    SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.buzzer);
                    SoundManager.Self().PlaySe();
                    return;
                }
                var actor = party.Members()[0] as GameActor;
                var gameAction = new GameAction(actor);
                gameAction.SetItem(_itemDataModel.basic.id);

                //敵が対象で、使用者への影響も設定されていない場合
                if ((gameAction.IsForOpponent() || _gameItem.Scope == 0) && !gameAction.IsForUser())
                {
                    //このケースであっても、コモンイベントが設定されており、かつ個数が足りている場合は、
                    //コモンイベントだけを実行する
                    var gameBattlers = gameAction.MakeTargets();
                    foreach (var battler in gameBattlers)
                    {
                        battler.Result ??= new GameActionResult();
                    }

                    bool isCommonEvent = false;
                    bool isCommonEventForUser = false;

                    //コモンイベントが設定されているか
                    foreach (var effect in _gameItem.Effects)
                    {
                        if (isCommonEvent) break;
                        isCommonEvent = gameAction.IsEffectCommonEvent(effect);
                    }

                    //使用者への影響にチェックが入っている場合、使用者への影響側のコモンイベントも確認
                    if (gameAction.IsForUser())
                        foreach (var effect in gameAction.Item.EffectsMyself)
                        {
                            if (isCommonEventForUser) break;
                            isCommonEventForUser = gameAction.IsEffectCommonEvent(effect);
                        }

                    if (isCommonEvent || isCommonEventForUser) {
                        //コモンイベントが設定されている場合、個数が足りているか
                        if (party.HasItem(_gameItem))
                        {
                            //コモンイベントを実行して終了する
                            actor.UseItem(_gameItem);
                            gameAction.SetCommonEvent(isCommonEventForUser);

                            //アイテム使用時のSE鳴動
                            gameAction.UseItemPlaySe(0);
                            MenuManager.MenuBase.AllUpdateStatus();
                            return;
                        }
                    }
                }
                else
                {
                    //対象が味方なので、選択Windowを表示して終了する
                    _itemMenu.MenuBase.OpenPartyWindow(PartyWindow.PartyType.Item, "", _itemDataModel.basic.id, _gameItem, _closeAction);
                    _openAction?.Invoke();

                    //最終選択したアイテム情報を保持
                    var runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
                    runtimeSaveDataModel.runtimePartyDataModel.lastItem.itemId = _itemDataModel.basic.id;

                    return;
                }

                //ブザー音鳴動
                SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.buzzer);
                SoundManager.Self().PlaySe();
            }
        }

        /// <summary>
        /// 使用可能かチェック
        /// </summary>
        /// <returns></returns>
        public bool CanUse() {
            if (_gameItem != null)
            {
                if (_gameItem.IsItem() && _gameItem.Scope >= 7)
                    return true;

                if (_gameItem.IsItem() && _gameItem.UserScope == 1)
                    return true;
            }

            return false;
        }
    }
}