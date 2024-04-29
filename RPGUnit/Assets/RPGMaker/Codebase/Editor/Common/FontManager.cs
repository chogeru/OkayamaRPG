using RPGMaker.Codebase.Editor.DatabaseEditor.View.Preview;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.Common
{
    /// <summary>
    ///     フォント用
    /// </summary>
    public static class FontManager
    {
        public enum FONT_TYPE
        {
            NORMAL = 0,
            TITLE  = 1,
            MENU   = 2,
            MESSAGE,
            MESSAGE_NAME,
            MESSAGE_SELECT,
            MESSAGE_NUM,
            MESSAGE_ITEM,
        }

        // デフォルトフォント
        private static readonly string DEFAULT_FONT    = "mplus-1m-regular";
        private static readonly string DEFAULT_CN_FONT = "SourceHanSansCN-Regular";
        private static readonly string BASE_FONT       = "default";
        private static readonly string TITLE_FONT      = "title";
        private static readonly string MENU_FONT = "menu";
        private static readonly string MESSAGE_FONT = "message";
        private static readonly string MESSAGE_NAME_FONT = "message_name";
        private static readonly string MESSAGE_SELECT_FONT = "message_select";
        private static readonly string MESSAGE_NUM_FONT = "message_num";
        private static readonly string MESSAGE_ITEM_FONT = "message_item";

        private static readonly string FONT_PATH       = "Assets/TextMesh Pro/Fonts/";

        private static List<string>    _fontList;
        private static List<string>    _pathList;

        /// <summary>
        /// フォントの初期設定
        /// </summary>
        static FontManager() {
            _fontList = new List<string>();
            _pathList = new List<string>();

            // デフォルト設定、使用可能フォント追加
            _pathList.Add(DEFAULT_FONT + ".ttf");
            _pathList.AddRange(Font.GetPathsToOSFonts().ToList());

            // ファイル名の取り出し
            foreach(var path in _pathList)
                _fontList.Add(path.Split("/")[path.Split("/").Length - 1].Split(".")[0]);
        }

        /// <summary>
        /// フォントの取得
        /// </summary>
        public static List<string> GetFontList() {
            return _fontList;
        }

        /// <summary>
        /// フォントのリセット
        /// </summary>
        public static void ResetFonts() {
            var dir = new DirectoryInfo(FONT_PATH);
            var info = dir.GetFiles("*.asset");
            var fileNames = new List<string>();
            foreach (var f in info) fileNames.Add(f.Name);

            foreach (var fileName in fileNames)
            {
                try
                {
                    TMP_FontAsset tmFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATH + fileName);
                    tmFont.ClearFontAssetData();
                }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// フォントの作成
        /// </summary>
        public static void CreateFont (int index, FONT_TYPE type = FONT_TYPE.NORMAL) {
            AssetDatabase.StartAssetEditing();
            string file = _pathList[index].Split("/")[_pathList[index].Split("/").Length - 1];
            string extension = file.Split(".")[file.Split(".").Length - 1];
            string fileName = file.Substring(0, file.Length - extension.Length - 1);

            // フォントコピー
            if (File.Exists(FONT_PATH + fileName + "." + extension) == false)
            {
                AssetDatabase.StopAssetEditing();
                File.Copy(_pathList[index], FONT_PATH + fileName + "." + extension);
                AssetDatabase.Refresh();
                AssetDatabase.StartAssetEditing();
            }

            // TextMeshフォント作成
            string tmPath = fileName + " SDF";
            
            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath(FONT_PATH + tmPath + ".asset", typeof(TMP_FontAsset)) as TMP_FontAsset;
            var atlasWidth = 4096;
            var atlasHeight = 4096;
            if (fontAsset == null)
            {
                //フォント作成
                Font font = AssetDatabase.LoadAssetAtPath<Font>(FONT_PATH + fileName + "." + extension);
                //sourceFontFileがinternalのため
                fontAsset = TMPro.TMP_FontAsset.CreateFontAsset(font, 60, 5,
                    UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, atlasWidth, atlasHeight, TMPro.AtlasPopulationMode.Dynamic);

                AssetDatabase.CreateAsset(fontAsset, FONT_PATH + tmPath + ".asset");
                
                //再度ここで取得しないと、アトラスとマテリアル作成できない
                fontAsset = AssetDatabase.LoadAssetAtPath(FONT_PATH + tmPath + ".asset", typeof(TMP_FontAsset)) as TMP_FontAsset;
                
                fontAsset.isMultiAtlasTexturesEnabled = true;
                
                // Initialize array for the font atlas textures.
                fontAsset.atlasTextures = new Texture2D[1];
                Texture2D texture = new Texture2D(atlasWidth, atlasHeight, TextureFormat.Alpha8, false, true)
                {
                    name = tmPath + " Atlas"
                };
                fontAsset.atlasTextures[0] = texture;
                AssetDatabase.AddObjectToAsset(texture, fontAsset);

                Material tmpMaterial = new Material(Shader.Find("TextMeshPro/Distance Field"))
                {
                    name = tmPath + " Material"
                };

                tmpMaterial.SetTexture(ShaderUtilities.ID_MainTex, texture);
                tmpMaterial.SetFloat(ShaderUtilities.ID_TextureWidth, atlasWidth);
                tmpMaterial.SetFloat(ShaderUtilities.ID_TextureHeight, atlasHeight);

                fontAsset.material = tmpMaterial;
                
                AssetDatabase.AddObjectToAsset(tmpMaterial, fontAsset);
                EditorUtility.SetDirty(fontAsset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            ChangeFont(FONT_PATH + fileName + "." + extension, FONT_PATH + tmPath + ".asset", type);
            AssetDatabase.StopAssetEditing();
        }

        /// <summary>
        /// フォントの設定
        /// </summary>
        private static void ChangeFont(string fontPath, string fontTMPath, FONT_TYPE type) {
            string baseName = type switch
            {
                FONT_TYPE.NORMAL => BASE_FONT,
                FONT_TYPE.TITLE => TITLE_FONT,
                FONT_TYPE.MENU => MENU_FONT,
                FONT_TYPE.MESSAGE => MESSAGE_FONT,
                FONT_TYPE.MESSAGE_NAME => MESSAGE_NAME_FONT,
                FONT_TYPE.MESSAGE_SELECT => MESSAGE_SELECT_FONT,
                FONT_TYPE.MESSAGE_NUM => MESSAGE_NUM_FONT,
                FONT_TYPE.MESSAGE_ITEM => MESSAGE_ITEM_FONT,
                _ => throw null
            };

            // フォント読込
            Font baseFont = AssetDatabase.LoadAssetAtPath<Font>(FONT_PATH + baseName + ".ttf");
            var trueTypeFontImporter = AssetImporter.GetAtPath(FONT_PATH + baseName + ".ttf") as TrueTypeFontImporter;
            Font cnFont = AssetDatabase.LoadAssetAtPath<Font>(FONT_PATH + DEFAULT_CN_FONT + ".ttf");
            Font defaultFont = AssetDatabase.LoadAssetAtPath<Font>(FONT_PATH + DEFAULT_FONT + ".ttf");
            Font targetFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            TMPro.TMP_FontAsset baseTMFont = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(FONT_PATH + baseName + " SDF.asset");
            TMPro.TMP_FontAsset cnTMFont = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(FONT_PATH + DEFAULT_CN_FONT + " SDF.asset");
            TMPro.TMP_FontAsset defaultTMFont = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(FONT_PATH + DEFAULT_FONT + " SDF.asset");
            TMPro.TMP_FontAsset targetTMFont = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(fontTMPath);

            // 先頭が優先
            trueTypeFontImporter.fontNames = new string[3]{targetFont.fontNames[0], defaultFont.fontNames[0], cnFont.fontNames[0]};
            
            baseTMFont.fallbackFontAssetTable = new List<TMPro.TMP_FontAsset>();
            baseTMFont.fallbackFontAssetTable.Add(targetTMFont);
            baseTMFont.fallbackFontAssetTable.Add(defaultTMFont);
            baseTMFont.fallbackFontAssetTable.Add(cnTMFont);

            EditorUtility.SetDirty(trueTypeFontImporter);
            EditorUtility.SetDirty(baseTMFont);
            trueTypeFontImporter.SaveAndReimport();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// フォントの色設定
        /// </summary>
        public static void ChangeFontColor(FONT_TYPE type, Color color) {
            string baseName = type switch
            {
                FONT_TYPE.NORMAL => BASE_FONT,
                FONT_TYPE.TITLE => TITLE_FONT,
                FONT_TYPE.MENU => MENU_FONT,
                _ => throw null
            };

            // フォント読込
            TMPro.TMP_FontAsset baseTMFont = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(FONT_PATH + baseName + " SDF.asset");
            baseTMFont.material.SetColor("_OutlineColor", new Color(0, 0, 0, 0));
            baseTMFont.material.SetColor("_FaceColor", color);

#if ENABLE_DEVELOPMENT_FIX
            baseTMFont.fallbackFontAssetTable[0].material.shader = Shader.Find("TextMeshPro/Distance Field");
            baseTMFont.fallbackFontAssetTable[1].material.shader = Shader.Find("TextMeshPro/Distance Field");
            baseTMFont.fallbackFontAssetTable[2].material.shader = Shader.Find("TextMeshPro/Distance Field");
            baseTMFont.fallbackFontAssetTable[0].material.SetColor("_OutlineColor", new Color(0, 0, 0, 0));
            baseTMFont.fallbackFontAssetTable[0].material.SetColor("_FaceColor", color);
            baseTMFont.fallbackFontAssetTable[1].material.SetColor("_OutlineColor", new Color(0, 0, 0, 0));
            baseTMFont.fallbackFontAssetTable[1].material.SetColor("_FaceColor", color);
            baseTMFont.fallbackFontAssetTable[2].material.SetColor("_OutlineColor", new Color(0, 0, 0, 0));
            baseTMFont.fallbackFontAssetTable[2].material.SetColor("_FaceColor", color);
#endif
            EditorUtility.SetDirty(baseTMFont);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// フォントを初期化し、Prefabを作り直す
        /// </summary>
        public static void InitializeFont() {
            //利用しているPC環境によって、入っているフォントが異なるため、PJを開いた後、利用できるフォントを設定しなおす
            var databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
            var uiSettingDataModel = databaseManagementService.LoadUiSettingDataModel();
            var commonData = uiSettingDataModel.commonMenus[0];

            //フォントの一覧を取得
            var fontSelectList = GetFontList();
            //設定されているフォント名を探し、無ければ0にする
            var titleFontIndex = fontSelectList.IndexOf(commonData.menuFontSetting.font) >= 0
                ? fontSelectList.IndexOf(commonData.menuFontSetting.font)
                : 0;

            //フォントを適用しなおし
            commonData.menuFontSetting.font = fontSelectList[titleFontIndex];
            FontManager.CreateFont(titleFontIndex);

            //適用したフォントを保存
            databaseManagementService.SaveUiSettingDataModel(uiSettingDataModel);

            //MenuPreview作成
            MenuPreview menuPreview = new MenuPreview();
            menuPreview.SetUiData(databaseManagementService.LoadUiSettingDataModel());
            menuPreview.InitUi(null, true);

            //UIパターン適用
            //menuPreview.ImageSettingApply();

            //オブジェクトを破棄
            menuPreview.DestroyLocalData();
        }
    }
}