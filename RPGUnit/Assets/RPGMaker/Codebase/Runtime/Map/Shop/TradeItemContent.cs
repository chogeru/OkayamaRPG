using RPGMaker.Codebase.CoreSystem.Helper;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Shop
{
    /// <summary>
    ///     取引決済時にアイテムの情報を表示する項目
    /// </summary>
    public class TradeItemContent : MonoBehaviour
    {
        [SerializeField] private Image _iconImage = null;
        [SerializeField] private Text  _nameText  = null;
        [SerializeField] private Text  _numText   = null;
        [SerializeField] private Text  _priseText = null;


        /// <summary>
        ///     表示するアイテムに関する情報をすべて設定する
        /// </summary>
        /// <param name="iconId">アイテムのアイコンID</param>
        /// <param name="itemName">アイテム名</param>
        /// <param name="num">アイテムの個数</param>
        /// <param name="price">アイテムの価格</param>
        public void SetTradeItemInfo(string iconId, string itemName, int num, int price) {
            _iconImage.sprite = GetItemImage(iconId);
            _nameText.text = itemName;
            _numText.text = num.ToString();
            _priseText.text = price.ToString();
        }

        /// <summary>
        ///     取引に関連する数量の表示を設定し直す
        /// </summary>
        /// <param name="num">アイテムの個数</param>
        /// <param name="price">アイテムの価格</param>
        public void SetTradeNum(int num, int price) {
            _numText.text = num.ToString();
            _priseText.text = price.ToString();
        }

        /// <summary>
        ///     アイコンの設定
        /// </summary>
        /// <param name="iconId">アイコン用画像のID</param>
        /// <returns>生成されたスプライト</returns>
        public Sprite GetItemImage(string iconId) {
            var iconSetTexture =
                UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                    "Assets/RPGMaker/Storage/Images/System/IconSet/" + iconId + ".png");

            var sprite = Sprite.Create(
                iconSetTexture,
                new Rect(0, 0, iconSetTexture.width, iconSetTexture.height),
                new Vector2(0.5f, 0.5f)
            );

            return sprite;
        }
    }
}