using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Common
{
    /// <summary>
    ///     背景画像の設定用補助クラス
    /// </summary>
    public static class BackgroundImageHelper
    {
        /// <summary>
        ///     背景画像の設定
        ///     引数1:設定対象(VisualElement)
        ///     引数2:表示領域の幅、高さ(Vector2Int)
        ///     引数3:対象のテクスチャ
        ///     引数4:画像サイズをピクセル数で指定するかパーセントで指定するか（デフォルトでパーセント）
        /// </summary>
        public static void SetBackground(
            VisualElement element,
            Vector2 windowSize,
            Texture2D tex2D,
            LengthUnit unit = LengthUnit.Percent
        ) {
            // 画像を設定
            element.style.backgroundImage = tex2D;

            if (tex2D == null) return;
            FixAspectRatio(element, windowSize, new Vector2(tex2D.width, tex2D.height), unit);
        }

        /// <summary>
        ///     背景画像の設定
        ///     引数1:設定対象(VisualElement)
        ///     引数2:表示領域の幅、高さ(Vector2Int)
        ///     引数3:対象のテクスチャ
        ///     引数4:画像サイズをピクセル数で指定するかパーセントで指定するか（デフォルトでパーセント）
        /// </summary>
        public static void FixAspectRatio(
            VisualElement element,
            Vector2 windowSize,
            Vector2 texSize,
            LengthUnit unit = LengthUnit.Percent
        ) {
            float widthRate = 0;
            float heightRate = 0;

            // テクスチャの割合を取得
            if (texSize.x < texSize.y)
            {
                widthRate = 1.0f;
                heightRate = texSize.x / texSize.y;
            }
            else
            {
                widthRate = texSize.y / texSize.x;
                heightRate = 1.0f;
            }

            // ウィンドウの表示割合を取得
            if (windowSize.x < windowSize.y)
                widthRate = widthRate * windowSize.x / windowSize.y;
            else
                heightRate = heightRate * windowSize.y / windowSize.x;

            if (windowSize.x < texSize.x || windowSize.y < texSize.y)
            {
                // 幅に合わせる
                if (widthRate > heightRate)
                {
                    if (unit == LengthUnit.Percent)
                    {
                        element.style.height = Length.Percent(100);
                        element.style.width = Length.Percent(100 * (heightRate / widthRate));
                    }
                    else
                    {
                        element.style.height = windowSize.y;
                        element.style.width = windowSize.x * (heightRate / widthRate);
                    }
                }
                // 高さ合わせる
                else
                {
                    if (unit == LengthUnit.Percent)
                    {
                        element.style.height = Length.Percent(100 * (widthRate / heightRate));
                        element.style.width = Length.Percent(100);
                    }
                    else
                    {
                        element.style.height = windowSize.y * (widthRate / heightRate);
                        element.style.width = windowSize.x;
                    }
                }
            }
            else
            {
                element.style.width = texSize.x;
                element.style.height = texSize.y;
            }
        }
    }
}