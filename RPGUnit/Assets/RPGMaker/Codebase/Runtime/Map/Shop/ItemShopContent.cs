using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.Runtime.Common;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Shop
{
    /// <summary>
    ///     ショップにアイテム単体の情報を表示する
    /// </summary>
    public class ItemShopContent : MonoBehaviour
    {
        public enum Type
        {
            ITEM,
            WEAPON,
            ARMOR,
            IMPORTANT
        }

        [SerializeField] private Button       contentButton = null;
        [SerializeField] private Image        iconImage     = null;
        [SerializeField] private Text         nameText      = null;
        [SerializeField] private Text         numText       = null;

        public Type CurrentType { get; private set; }
        public int ItemCount { get; private set; }
        public Button ContentButton => contentButton;
        public bool Buyable { get; private set; }
        public bool Sellable { get; set; }

        //各データモデルの受け渡し用
        public ItemDataModel SettingItems { get; set; }
        public WeaponDataModel SettingWeapons { get; set; }
        public ArmorDataModel SettingArmors { get; set; }

        /// <summary>
        ///     購入可能かどうかの設定
        /// </summary>
        /// <param name="gold">所持金</param>
        public void SetupBuyable(int gold) {
            switch (CurrentType)
            {
                case Type.ITEM:
                    Buyable = gold >= SettingItems.basic.price;
                    break;

                case Type.WEAPON:
                    Buyable = gold >= SettingWeapons.basic.price;
                    break;

                case Type.ARMOR:
                    Buyable = gold >= SettingArmors.basic.price;
                    break;
            }
        }

        /// <summary>
        ///     各Textに情報を設定する
        /// </summary>
        /// <param name="name">アイテム名</param>
        /// <param name="count">個数</param>
        /// <param name="iconId">アイテムのID</param>
        /// <param name="itemType">アイテムの種別</param>
        public void SetText(string name, string count, string iconId, int itemType = -1) {
            nameText.text = name;
            numText.text = count;
            iconImage.sprite = GetItemImage(iconId);
            if (iconImage.sprite == null)
                iconImage.enabled = false;

            //共通設定の色を適用
            nameText.color = new Color(
                DataManager.Self().GetUiSettingDataModel().commonMenus[0].menuFontSetting.color[0] / 255.0f,
                DataManager.Self().GetUiSettingDataModel().commonMenus[0].menuFontSetting.color[1] / 255.0f,
                DataManager.Self().GetUiSettingDataModel().commonMenus[0].menuFontSetting.color[2] / 255.0f, nameText.color.a);
            numText.color = new Color(
                DataManager.Self().GetUiSettingDataModel().commonMenus[0].menuFontSetting.color[0] / 255.0f,
                DataManager.Self().GetUiSettingDataModel().commonMenus[0].menuFontSetting.color[1] / 255.0f,
                DataManager.Self().GetUiSettingDataModel().commonMenus[0].menuFontSetting.color[2] / 255.0f, numText.color.a);

            //アイテムの種別が、大事なもの
            //基本、売る画面以外では呼ばれない
            if (itemType == 2)
            {
                numText.enabled = DataManager.Self().GetSystemDataModel().optionSetting.showKeyItemNum == 1;
            }
        }

        /// <summary>
        ///     各Textに情報を設定する
        /// </summary>
        /// <param name="name">アイテム名</param>
        /// <param name="count">個数</param>
        /// <param name="iconId">アイテムのID</param>
        /// <param name="color">テキストカラー</param>
        /// <param name="itemType">アイテムの種別</param>
        public void SetTextColor(string name, string count, string iconId, Color color, int itemType) {
            nameText.text = name;
            numText.text = count;
            iconImage.sprite = GetItemImage(iconId);

            //共通設定の色を適用
            nameText.color = new Color(
                color.r / 255.0f,
                color.g / 255.0f,
                color.b / 255.0f, nameText.color.a);
            numText.color = new Color(
                color.r / 255.0f,
                color.g / 255.0f,
                color.b / 255.0f, numText.color.a);
            
            //アイテムの種別が、大事なもの
            if (itemType == 2)
            {
                numText.enabled = DataManager.Self().GetSystemDataModel().optionSetting.showKeyItemNum == 1;
            }
        }

        public void SetItemCount(int num, Type type) {
            ItemCount = num;
            CurrentType = type;
        }

        public void SetFontSize(int size) {
            nameText.fontSize = size;
            numText.fontSize = size;
        }

        /// <summary>
        ///     保存されている画像を取得する
        /// </summary>
        /// <param name="iconName">アイコン名</param>
        /// <returns>アイコン画像</returns>
        private Sprite GetItemImage(string iconName) {
            var iconSetTexture =
                UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                    "Assets/RPGMaker/Storage/Images/System/IconSet/" + iconName + ".png");

            if (iconSetTexture == null)
                return null;

            var sprite = Sprite.Create(
                iconSetTexture,
                new Rect(0, 0, iconSetTexture.width, iconSetTexture.height),
                new Vector2(0.5f, 0.5f)
            );

            return sprite;
        }

        /// <summary>
        ///     ショップの状態から表示されているはずのContentの種類を返す
        /// </summary>
        /// <param name="state">ショップの状態</param>
        /// <returns>Contentの種類</returns>
        public static Type StateToType(ItemShop.ShopState state) {
            return state switch
            {
                ItemShop.ShopState.ITEM => Type.ITEM,
                ItemShop.ShopState.WEAPON => Type.WEAPON,
                ItemShop.ShopState.ARMOR => Type.ARMOR,
                ItemShop.ShopState.IMPORTANT => Type.IMPORTANT,
                _ => throw new InvalidOperationException()
            };
        }
    }
}