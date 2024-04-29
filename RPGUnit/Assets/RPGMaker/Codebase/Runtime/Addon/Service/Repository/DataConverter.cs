using System;
using System.Linq;
using RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Addon
{
    public static class DataConverter
    {
        public static AddonDataModel ConvertAddonToObject(
            AddonJson jsonAddonJson
        ) {
            return new AddonDataModel(
                jsonAddonJson.name,
                jsonAddonJson.status,
                jsonAddonJson.description,
                new AddonParameterContainer(jsonAddonJson.parameters.ToList())
            );
        }


        public static AddonJson ConvertAddonToJson(AddonDataModel addonDataModel) {
            return new AddonJson(
                addonDataModel.Name,
                addonDataModel.Status,
                addonDataModel.Description,
                addonDataModel.Parameters.ToArray()
            );
        }

        public static AddonInfo ConvertAddonInfoToObject(
            AddonInfoJson addonInfoJson
        ) {
            return new AddonInfo(addonInfoJson);
        }


        public static AddonInfoJson ConvertAddonInfoToJson(AddonInfo addonInfo) {
            return new AddonInfoJson(addonInfo);
        }

        public static string GetJsonString(string str) {
            var noteJson = JsonUtility.ToJson(new AddonNoteJson(str));
            // {"str":???}
            return noteJson.Substring(7, noteJson.Length - (7 + 1));
        }

        public static string GetStringFromJson(string jsonStr) {
            if (jsonStr.Length >= 2 && jsonStr.Trim().Substring(0, 1) != "\"") return null;
            try
            {
                var obj = JsonUtility.FromJson<AddonNoteJson>("{\"str\":" + jsonStr + "}");
                if (obj != null) return obj.str;
            }
            catch (Exception)
            {
            }

            return null;
        }

        public static string GetJsonStringArray(string[] arr) {
            var arrJson = JsonUtility.ToJson(new AddonArrayJson(arr));
            // {"arr":[:???]}
            return arrJson.Substring(7, arrJson.Length - (7 + 1));
        }

        public static string[] GetStringArrayFromJson(string jsonStr) {
            if (jsonStr == null || jsonStr.Length < 2 || jsonStr.Trim().Substring(0, 1) != "[") return null;
            try
            {
                var obj = JsonUtility.FromJson<AddonArrayJson>("{\"arr\":" + jsonStr + "}");
                if (obj != null) return obj.arr;
            }
            catch (Exception)
            {
            }

            return null;
        }
    }
}
