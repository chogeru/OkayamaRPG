using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Helper;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif


namespace RPGMaker.Codebase.CoreSystem.Lib.Migration.ExecutionClasses
{
    /// <summary>
    /// Version1.0.6へのMigration用クラス
    /// </summary>
    internal class Migration_1_0_6_Class : IExecutionClassCore
    {
        public string GetIdentifier()
        {
            return "Migration106Class";
        }

        public void Execute()
        {
            TileC_Convert();
        }

        public void Rollback()
        {
        }

        public bool IsStorageUpdate()
        {
            return false;
        }

        public List<string> ListStorageCopy()
        {
            return null;
        }

        public List<string> ListStorageDelete()
        {
            return null;
        }

        /// <summary>
        /// タイルCのピボット修正
        /// </summary>
        private void TileC_Convert() {
#if UNITY_EDITOR
            string TileAssetFolderPath = "Assets/RPGMaker/Storage/Map/TileAssets/AutoTileC/";


            var tiletable = new List<TileDataModelInfo>();

            // タイルCの画像が入ってるフォルダを全取得
            var tileFolder = Directory.GetDirectories(TileAssetFolderPath, "*", SearchOption.AllDirectories).ToList();
            for (int i = 0; i < tileFolder.Count; i++)
            {
                //画像を取得
                var pngs = Directory.GetFiles(tileFolder[i].ToString(),"*.png", SearchOption.AllDirectories).ToList();
                for (int j = 0; j < pngs.Count; j++)
                {
                    var textureImporter = AssetImporter.GetAtPath(pngs[j]) as TextureImporter;

                    if (textureImporter != null)
                    {
                        //画像ファイルを抜き取る
                        var names = pngs[j].Split('\\');
                        switch (names[^1])
                        {
                            case "defaultSprite.png":
                            case "shape0.png":
                            case "shape1.png":
                            case "shape2.png":
                            case "shape3.png":
                            case "shape4.png":
                            case "shape5.png":
                            case "shape6.png":
                            case "shape7.png":
                                var texSettings = new TextureImporterSettings();
                                textureImporter.ReadTextureSettings(texSettings);
                                if (texSettings.spriteAlignment == (int) SpriteAlignment.Custom)
                                {
                                    texSettings.spriteAlignment = (int) SpriteAlignment.Center;
                                    textureImporter.SetTextureSettings(texSettings);
                                    textureImporter.spritePivot = new Vector2(0.5f, 0.5f);
                                    EditorUtility.SetDirty(textureImporter);
                                    textureImporter.SaveAndReimport();
                                }
                                break;
                            default:
                                if (textureImporter.spritePivot.y < 0.56f)
                                {
                                    textureImporter.spritePivot = new Vector2(0.5f, 0.57f);
                                    EditorUtility.SetDirty(textureImporter);
                                    textureImporter.SaveAndReimport();
                                }
                                break;
                        }
                    }
                }
            }

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh2();
#endif
        }
    }
}