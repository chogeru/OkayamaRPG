using System.Collections.Generic;
using System.IO;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Knowledge.Misc;

#if UNITY_EDITOR
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository;
#endif

using UnityEngine;

// 読み込み
namespace RPGMaker.Codebase.CoreSystem.Helper
{
    public static class ImageManager {
        public const int OBJECT_WIDTH = 98;
        private static List<string> DefaultBattlebackName = new List<string>
        {
            "battlebacks1_nature_008",
            "battlebacks2_nature_008"
        };

        public static Sprite LoadBattleback1(string filename, int? hue = null) {
            if (!string.IsNullOrEmpty(filename))
            {
                return UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                    PathManager.BATTLE_BACKGROUND_1 + filename + ".png");
            }
            return UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                PathManager.SYSTEM_BATTLE_BACKGROUND_1 + "battlebacks1_nature_008.png");
        }

        public static Sprite LoadBattleback2(string filename, int? hue = null) {
            if (!string.IsNullOrEmpty(filename))
            {
                return UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                    PathManager.BATTLE_BACKGROUND_2 + filename + ".png");
            }
            return UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                PathManager.SYSTEM_BATTLE_BACKGROUND_2 + "battlebacks2_nature_008.png");
        }

        public static string GetBattlebackName(string filename, int backNumber) {
            if (!string.IsNullOrEmpty(filename))
            {
                return filename;
            }
            else
            {
                if (backNumber == 1)
                {
                    return DefaultBattlebackName[0];
                }
                else
                {
                    return DefaultBattlebackName[1];
                }
            }
        }

        public static Bitmap LoadEnemy(string filename, int? hue = null) {
            return LoadBitmap(PathManager.IMAGE_ENEMY, filename, hue, true);
        }

        public static Texture2D LoadEnemyByTexture(string filename, int? hue = null) {
            return UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                PathManager.IMAGE_ENEMY + filename + ".png");
        }

        public static Texture2D LoadFace(string filename, int? hue = null) {
            return UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                PathManager.IMAGE_FACE + filename + ".png");
        }

        public static Texture2D LoadPicture(string filename, int? hue = null) {
            return UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                PathManager.IMAGE_ADV + filename + ".png");
        }

        public static OverrideTexture LoadSvActor(string filename, int? hue = null) {
            return LoadTexture(PathManager.IMAGE_SV_CHARACTER, filename, hue, false);
        }
        
        public static Bitmap LoadSystem(string filename, int? hue = null) {
            return LoadBitmap(PathManager.IMAGE_SYSTEM, filename, hue, false);
        }

        public static Sprite LoadDamage(string filename, int? hue = null) {
            return UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                PathManager.SYSTEM_DAMAGE + filename + ".png");
        }

        private static Bitmap LoadBitmap(string folder, string filename, int? hue, bool smooth) {
            if (filename != "")
            {
                var path = folder + filename;
                var bitmap = LoadNormalBitmap(path, hue ?? 0);

                return bitmap;
            }

            return null;
        }

        private static OverrideTexture LoadTexture(string folder, string filename, int? hue, bool smooth) {
            if (filename != "")
            {
                var path = folder + filename;
                var texture = LoadNormalTexture(path, hue ?? 0);
                return texture;
            }

            return null;
        }

        private static Bitmap LoadNormalBitmap(string path, int hue) {
            return new Bitmap(UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path + ".png"));
        }

        private static OverrideTexture LoadNormalTexture(string path, int hue) {
            return new OverrideTexture(
                (Texture) UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath(path + ".png", typeof(Texture)));
        }

        public static Texture2D LoadPopIcon(string filename, int frame) {
            var texture2Ds =
                ImageUtility.Instance.SliceImage(PathManager.IMAGE_BALLOON + filename, frame == 0 ? 1 : frame, 1);
            return texture2Ds[texture2Ds.Count - 1];
        }

        public static Texture2D LoadSvCharacter(string assetId) {
            // 渡されたIDが空なら抜ける
            if (string.IsNullOrEmpty(assetId)) return null;

#if UNITY_EDITOR
            var inputString =
                UnityEditorWrapper.AssetDatabaseWrapper
                    .LoadAssetAtPath<TextAsset>(
                        PathManager.JSON_ASSETS + assetId + ".json");
            if (inputString == null)
                return null;
            var assetManageData = JsonHelper.FromJson<AssetManageDataModel>(inputString.text);
#else
            var assetManageData =
 ScriptableObjectOperator.GetClass<AssetManageDataModel>("SO/" + assetId + ".asset") as AssetManageDataModel;
#endif
            // アセットタイプによってパス設定
            var path = "";
            switch (assetManageData.assetTypeId)
            {
                case (int) AssetCategoryEnum.MOVE_CHARACTER:
                    path = PathManager.IMAGE_CHARACTER;
                    break;
                case (int) AssetCategoryEnum.SV_BATTLE_CHARACTER:
                    path = PathManager.IMAGE_SV_CHARACTER;
                    break;
                case (int) AssetCategoryEnum.OBJECT:
                    path = PathManager.IMAGE_OBJECT;
                    break;
            }

            if (!File.Exists(path + assetManageData.imageSettings[0].path))
                return null;
            var texture2Ds = ImageUtility.Instance.SliceImage(
                path + assetManageData.imageSettings[0].path,
                assetManageData.imageSettings[0].animationFrame == 0
                    ? 1
                    : assetManageData.imageSettings[0].animationFrame, 1);
            return texture2Ds[0];
        }

        public static float FixAspect(
            Vector2 windowSize,
            Vector2 texSize
        ) {
            float widthRate = windowSize.x / texSize.x;
            float heightRate = windowSize.y / texSize.y;
            
            return widthRate > heightRate ? heightRate : widthRate;
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// 画像のリスト取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<string> GetImageNameList(string path) {
            //PathManager.IMAGE_FACE
            var dir = new DirectoryInfo(path);
            var fileInfoList = dir.GetFiles("*.png");
            var fileNames = new List<string>();
            for (int i = 0; i < fileInfoList.Length; i++)
            {
                var name = fileInfoList[i].Name.Replace(".png", "");
                fileNames.Add(name);
            }
            return fileNames;
        }

        /// <summary>
        /// SV関連のリストを取得
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<AssetManageDataModel> GetSvIdList(AssetCategoryEnum type) {
            var orderData = AssetManageRepository.OrderManager.Load();
            var assetManageData = new List<AssetManageDataModel>();
            var databaseManagementService = new DatabaseManagementService();
            var manageData = databaseManagementService.LoadAssetManage();

            for (var i = 0; i < orderData.orderDataList.Length; i++)
            {
                if (orderData.orderDataList[i].idList == null)
                    continue;
                if (type == (AssetCategoryEnum) orderData.orderDataList[i].assetTypeId)
                {
                    for (var i2 = 0; i2 < orderData.orderDataList[i].idList.Count; i2++)
                    {
                        AssetManageDataModel data = null;
                        for (int i3 = 0; i3 < manageData.Count; i3++)
                            if (manageData[i3].id == orderData.orderDataList[i].idList[i2])
                            {
                                data = manageData[i3];
                                break;
                            }

                        var count = 0;
                        if (data == null)
                            count++;
                        assetManageData.Add(data);
                    }
                }
            }

            return assetManageData;
        }
#endif
    }
}