using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace RPGMaker.Codebase.CoreSystem.Helper
{
    public class ImageUtility
    {
        private static ImageUtility mInstance;
        private        int          partHeight;


        private int partWidth;

        private ImageUtility() {
        }

        public static ImageUtility Instance
        {
            get
            {
                if (mInstance == null) mInstance = new ImageUtility();

                return mInstance;
            }
        }

        public List<Texture2D> SliceImage(string path, int x, int y) {
            // 指定パスの画像をバイトデータで読み込む
            var texture = ReadImage(path);

            partWidth = texture.width / x;
            partHeight = texture.height / y;
            var fileNameIndex = 0;

            var sliceBitmaps = new List<Texture2D>();
            for (var partY = y; partY > 0; partY--)
            for (var partX = 0; partX < x; partX++)
            {
                var partPixels = ReadPartPixels(partX, partY, texture);


                var partTexture = new Texture2D(partWidth, partHeight, TextureFormat.RGBA32, false);
                partTexture.SetPixels(partPixels);
                partTexture.Apply();

                sliceBitmaps.Add(partTexture);


                fileNameIndex++;
            }

            return sliceBitmaps;
        }

        public List<Sprite> SliceImageToSprite(Texture2D texture, int x, int y) {
            try
            {
                partWidth = texture.width / x;
                partHeight = texture.height / y;
                var fileNameIndex = 0;

                var sliceBitmaps = new List<Sprite>();
                for (var partY = y; partY > 0; partY--)
                for (var partX = 0; partX < x; partX++)
                {
                    var partPixels = ReadPartPixels(partX, partY, texture);


                    var partTexture = new Texture2D(partWidth, partHeight, TextureFormat.RGBA32, false);
                    partTexture.SetPixels(partPixels);
                    partTexture.Apply();
                    var sp = Sprite.Create(partTexture, new Rect(0, 0, partTexture.width, partTexture.height),
                        Vector2.zero);
                    sliceBitmaps.Add(sp);


                    fileNameIndex++;
                }

                return sliceBitmaps;
            }
            catch (Exception)
            {
                var sliceBitmaps = new List<Sprite>();
                var partTexture = new Texture2D(96, 96, TextureFormat.RGBA32, false);
                var sp = Sprite.Create(partTexture, new Rect(0, 0, partTexture.width, partTexture.height),
                    Vector2.zero);
                sliceBitmaps.Add(sp);
                return sliceBitmaps;
            }
        }

        private Color[] ReadPartPixels(int partX, int partY, Texture2D texture) {
            var pixels = new Color[partWidth * partHeight];
            var offsetX = partX * partWidth;
            var offsetY = partY * partHeight;

            for (int y = offsetY, y_ = partHeight - 1; y > offsetY - partHeight; y--, y_--)
            for (int x = offsetX, x_ = 0; x < offsetX + partWidth; x++, x_++)
                pixels[y_ * partWidth + x_] = texture.GetPixel(x, y);

            return pixels;
        }

        /*
         * 指定テクスチャから指定矩形位置のテクスチャを取得する。
         * 
         * Texture2Dのメソッドと違い、y座標は上端を0とする。
         */
        public Texture2D GetTextureRect(Texture2D texture, RectInt rect) {
            var resultTexture = new Texture2D(rect.width, rect.height, texture.format, false);
            resultTexture.SetPixels(GetPixelsTopOrigin(texture, rect));
            resultTexture.Apply();
            return resultTexture;
        }

        /*
         * 指定テクスチャから指定矩形位置のテクスチャを取得する。
         * 
         * Texture2Dのメソッドと違い、y座標は上端を0とする。
         */
        public void CopyTextureRect(
            Texture2D sourceTexture,
            RectInt sourceRect,
            Texture2D destinationTexture,
            Vector2Int destinationPosition
        ) {
            var destinationRect = new RectInt(destinationPosition, sourceRect.size);
            destinationTexture.SetPixels(
                destinationPosition.x,
                GetTopOriginY(destinationTexture, destinationRect),
                sourceRect.width,
                sourceRect.height,
                GetPixelsTopOrigin(sourceTexture, sourceRect));
        }

        /*
         * テクスチャからピクセル色列を取得する。
         * 
         * Texture2Dのメソッドと違い、y座標は上端を0とする。
         */
        private Color[] GetPixelsTopOrigin(Texture2D texture, RectInt rect) {
            return texture.GetPixels(rect.x, GetTopOriginY(texture, rect), rect.width, rect.height);
        }

        /*
         * テクスチャからピクセル色列を取得する。
         * 
         * Texture2Dのメソッドと違い、y座標は上端を0とする。
         */
        private int GetTopOriginY(Texture2D texture, RectInt rect) {
            return texture.height - rect.y - rect.height;
        }

        /*
         * テクスチャを単色で埋める。
         */
        public void FillTexture(Texture2D texture, Color color) {
            var pixels = texture.GetPixels();
            for (var i = 0; i < pixels.Length; i++) pixels[i] = color;
            texture.SetPixels(pixels);
            texture.Apply();
        }

        /*
         * 画像の読み込み
         */
        private static Texture2D ReadImage(string path) {
            return ReadPng(path);
        }

        /*
         * バイトデータからTexture2Dに変換
         */
        private static Texture2D ReadPng(string path) {
            var readBinary = ReadPngFile(path);

            var pos = 16; // 16バイトから開始

            var width = 0;
            for (var i = 0; i < 4; i++) width = width * 256 + readBinary[pos++];

            var height = 0;
            for (var i = 0; i < 4; i++) height = height * 256 + readBinary[pos++];

            var texture = new Texture2D(width, height);
            texture.LoadImage(readBinary);

            return texture;
        }

        /*
         * 画像をバイトで開く
         */
        private static byte[] ReadPngFile(string path) {
            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var bin = new BinaryReader(fileStream);
            var values = bin.ReadBytes((int) bin.BaseStream.Length);

            bin.Close();

            return values;
        }

        /// <summary>
        ///     PNG/JPG (またはサポートされている形式) の画像ファイルをロードして、そのテクスチャを取得します。
        /// </summary>
        /// <param name="imageFilePath">画像ファイルパス</param>
        /// <returns></returns>
        public static Texture2D LoadImageFileToTexture(string imageFilePath) {
            var texture = new Texture2D(1, 1);
            texture.LoadImage(File.ReadAllBytes(imageFilePath));
            return texture;
        }

        public static void SaveAndDestroyTexture(string filePath, Texture2D texture2d) {
            SaveTexture(filePath, texture2d);
            Object.DestroyImmediate(texture2d);
        }

        public static void SaveTexture(string filePath, Texture2D texture2d) {
            // Texture2Dをバイト列に変換。
            var bytes = texture2d.EncodeToPNG();

            // ファイルにセーブ。
            File.WriteAllBytes(filePath, bytes);
        }

#if UNITY_EDITOR
        /// <summary>
        /// TextureからRGBA32のTexture2Dを取得する。
        /// </summary>
        /// <param name="texture">変換元のTexture</param>
        /// <param name="coroutineOwner">
        /// コルーチン用オーナーオブジェクト (コルーチンを自動で停止させる為に必要)。
        /// </param>
        /// <returns>変換後のTexture2D</returns>
        public static Texture2D ToTexture2D(Texture texture, object coroutineOwner)
        {
            // テクスチャを自前のレンダーテクスチャにコピー。
            var renderTexture = new RenderTexture(texture.width, texture.height, 32);
            renderTexture.Create();

            Graphics.Blit(texture, renderTexture);

            // アクティブなレンダーテクスチャを自前のレンダーテクスチャに変更。
            var originalActiveRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;

            // アクティブなレンダーテクスチャからTexture2Dにコピー。
            var texture2d = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            texture2d.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2d.Apply();

            // アクティブなレンダーテクスチャを元に戻す。
            RenderTexture.active = originalActiveRenderTexture;

            // 自前のレンダーテクスチャを次フレームに解放。
            // 即時の解放は以下の警告が表示される。
            //   Releasing render texture that is set to be RenderTexture.active!
            EditorCoroutineUtility.StartCoroutine(
                ReleaseRenderTexture(renderTexture), coroutineOwner);
            static IEnumerator ReleaseRenderTexture(RenderTexture renderTexture)
            {
                yield return new WaitForEndOfFrame();
                renderTexture.Release();
            }

            return texture2d;
        }
#endif

        public static Texture2D CreateFrameTexture(
            int width,
            int height,
            Color baseColor,
            Color frameColor,
            int frameSize,
            bool mipChain = false
        ) {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, mipChain);

            foreach (var y in Enumerable.Range(0, height)) 
            foreach (var x in Enumerable.Range(0, width))
            {
                var color = x < frameSize || x >= width - frameSize || y < frameSize || y >= height - frameSize ?
                    frameColor :
                    baseColor;
                texture.SetPixel(x, y, color);
            }

            texture.Apply();
            return texture;
        }

        public static Color32 ToColor32(uint rgba) {
            return new Color32(
                (byte) ((rgba >> (8 * 3)) & 0xff),
                (byte) ((rgba >> (8 * 2)) & 0xff),
                (byte) ((rgba >> (8 * 1)) & 0xff),
                (byte) ((rgba >> (8 * 0)) & 0xff));
        }
    }
}