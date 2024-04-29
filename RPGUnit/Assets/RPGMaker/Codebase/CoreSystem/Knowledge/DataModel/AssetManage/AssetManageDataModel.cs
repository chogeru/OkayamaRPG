using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage
{
    [Serializable]
    public class AssetManageDataModel : WithSerialNumberDataModel
    {
        private const int DataSizeDefault       = 1;
        private const int DataSizeMoveCharacter = 5;
        private const int DataSizeSvCharacter   = 18;
        public        int assetTypeId;

        public string             id;
        public List<ImageSetting> imageSettings;
        public string             name;
        public int                sort;
        public int                type;
        public int                weaponTypeId;

        public AssetManageDataModel(
            string id,
            int sort,
            string name,
            int type,
            int weaponTypeId,
            int assetTypeId,
            List<ImageSetting> imageSettings
        ) {
            this.id = id;
            this.sort = sort;
            this.name = name;
            this.type = type;
            this.weaponTypeId = weaponTypeId;
            this.assetTypeId = assetTypeId;
            this.imageSettings = imageSettings;
        }

        public static AssetManageDataModel CreateDefault(int assetTypeId, int otherSameTypeItemAmount) {
            var ret = new AssetManageDataModel(
                Guid.NewGuid().ToString(),
                otherSameTypeItemAmount + 1,
                "",
                0,
                0,
                assetTypeId,
                new List<ImageSetting>()
            );


            switch (assetTypeId)
            {
                case (int) AssetCategoryEnum.MOVE_CHARACTER:
                case (int) AssetCategoryEnum.OBJECT:
                    ret.imageSettings = new List<ImageSetting>();
                    for (var i = 0; i < DataSizeMoveCharacter; i++)
                        ret.imageSettings.Add(new ImageSetting("", 0, 0, 0, 0));

                    break;
                case (int) AssetCategoryEnum.SV_BATTLE_CHARACTER:
                    ret.imageSettings = new List<ImageSetting>();
                    for (var i = 0; i < DataSizeSvCharacter; i++)
                        ret.imageSettings.Add(new ImageSetting("", 0, 0, 0, 0));

                    break;
                case (int) AssetCategoryEnum.POPUP:
                case (int) AssetCategoryEnum.SV_WEAPON:
                case (int) AssetCategoryEnum.SUPERPOSITION:
                    ret.imageSettings = new List<ImageSetting>();
                    for (var i = 0; i < DataSizeDefault; i++) ret.imageSettings.Add(new ImageSetting("", 0, 0, 0, 0));

                    break;
                case (int) AssetCategoryEnum.BATTLE_EFFECT:
                    ret.imageSettings = new List<ImageSetting>();
                    for (var i = 0; i < DataSizeDefault; i++)
                        ret.imageSettings.Add(
                            new ImageSetting("", 0, 0, 0, 0));

                    break;
            }

            return ret;
        }

        public bool isEqual(AssetManageDataModel data) {
            if (imageSettings.Count != data.imageSettings.Count)
                return false;
            for (int i = 0; i < imageSettings.Count; i++)
                if (!imageSettings[i].isEqual(data.imageSettings[i]))
                    return false;

            return assetTypeId == data.assetTypeId &&
                   id == data.id &&
                   name == data.name &&
                   sort == data.sort &&
                   type == data.type &&
                   weaponTypeId == data.weaponTypeId;
        }

        [Serializable]
        public class ImageSetting
        {
            public int    animationFrame;
            public int    animationSpeed;
            public string path;
            public int    sizeX;
            public int    sizeY;

            public ImageSetting(string path, int sizeX, int sizeY, int animationFrame, int animationSpeed) {
                this.path = path;
                this.sizeX = sizeX;
                this.sizeY = sizeY;
                this.animationFrame = animationFrame;
                this.animationSpeed = animationSpeed;
            }

            public bool isEqual(ImageSetting data) {
                return animationFrame == data.animationFrame &&
                       animationSpeed == data.animationSpeed &&
                       path == data.path &&
                       sizeX == data.sizeX &&
                       sizeY == data.sizeY;
            }
        }
    }
}