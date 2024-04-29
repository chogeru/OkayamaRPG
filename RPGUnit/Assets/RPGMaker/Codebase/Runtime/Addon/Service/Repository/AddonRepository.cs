using System.Collections.Generic;
using System.IO;
using System.Linq;
using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Addon
{
    public class AddonRepository
    {
        private const string JsonFileBasename = "addonSettings";
        private const string JsonFile           = "Assets/RPGMaker/Storage/Addon/Resources/addonSettings.json";
        private const string AddonInfosJsonFileBasename = "addonInfos";
        private const string AddonInfosJsonFile = "Assets/RPGMaker/Storage/Addon/Resources/addonInfos.json";

        private List<AddonDataModel> _addonDataModels;
        private AddonInfoContainer   _addonInfos;

        /**
         * アドオンデータをJSONから読み出す.
         */
        public List<AddonDataModel> GetAddonDataModels() {
            if (_addonDataModels != null) return _addonDataModels;
#if UNITY_EDITOR
            if (!File.Exists(JsonFile)){
                Directory.CreateDirectory(JsonFile.Substring(0, JsonFile.LastIndexOf('/')));
                var JsonFileSub = JsonFile.Replace("Resources", "JSON");
                if (File.Exists(JsonFileSub))
                {
                    File.Copy(JsonFileSub, JsonFile, true);
                } else
                {
                    File.WriteAllText(JsonFile, "[]");
                }
            }
            var jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JsonFile);
#else
            var jsonString = Resources.Load<TextAsset>(JsonFileBasename).text;
#endif
            _addonDataModels = JsonHelper.FromJsonArray<AddonJson>(jsonString)
                .Select(addonJson => { return DataConverter.ConvertAddonToObject(addonJson); })
                .ToList();

            return _addonDataModels;
        }

        /**
         * アドオンデータをJSONに保存する.
         */
        public void StoreAddonDataModels(List<AddonDataModel> addonDataModels) {
            var addonJson =
                addonDataModels.Select(DataConverter.ConvertAddonToJson);

            try
            {
                File.WriteAllText(JsonFile, JsonHelper.ToJsonArray(addonJson));
            }
            catch (IOException)
            {
                Debug.Log($"Failed to save {JsonFile}");
            }
        }

        public AddonInfoContainer GetAddonInfos() {
            if (_addonInfos != null) return _addonInfos;
#if UNITY_EDITOR
            if (!File.Exists(AddonInfosJsonFile))
            {
                Directory.CreateDirectory(AddonInfosJsonFile.Substring(0, AddonInfosJsonFile.LastIndexOf('/')));
                var AddonInfosJsonFileSub = AddonInfosJsonFile.Replace("Resources", "JSON");
                if (File.Exists(AddonInfosJsonFileSub))
                {
                    File.Copy(AddonInfosJsonFileSub, AddonInfosJsonFile, true);
                }
                else
                {
                    File.WriteAllText(AddonInfosJsonFile, "[]");
                }
            }
            var jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(AddonInfosJsonFile);
#else
            var jsonString = Resources.Load<TextAsset>(AddonInfosJsonFileBasename).text;
#endif
            _addonInfos = new AddonInfoContainer(JsonHelper.FromJsonArray<AddonInfoJson>(jsonString)
                .Select(addonInfoJson => { return DataConverter.ConvertAddonInfoToObject(addonInfoJson); })
                .ToList());

            return _addonInfos;
        }

        /**
         * アドオン情報をJSONに保存する.
         */
        public void StoreAddonInfos(AddonInfoContainer addonInfos) {
            var addonInfoJson =
                addonInfos.Select(DataConverter.ConvertAddonInfoToJson);

            try
            {
                File.WriteAllText(AddonInfosJsonFile, JsonHelper.ToJsonArray(addonInfoJson));
            }
            catch (IOException)
            {
                Debug.Log($"Failed to save {AddonInfosJsonFile}");
            }
        }
    }
}
