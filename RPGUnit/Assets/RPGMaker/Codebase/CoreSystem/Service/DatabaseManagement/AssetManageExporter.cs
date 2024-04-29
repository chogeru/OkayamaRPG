using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.Misc;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement
{
    public static class AssetManageExporter
    {
        private const string PROCESS_TEXT            = "WORD_0051";
        private const string CLOSE_TEXT              = "WORD_0052";
        private const string PROCESS_SUCCESS_TEXT    = "WORD_0053";
        private const string PROCESS_CANCEL_TEXT     = "WORD_0054";
        private const string PROCESS_OVERWRITE_TEXT  = "WORD_0055";
        private const string PROCESS_SAVEPATH_TEXT   = "WORD_0056";
        private const string YES_TEXT                = "WORD_0057";
        private const string NO_TEXT                 = "WORD_0058";

        // private関数
        //------------------------------------------------------------------
        // ダイアログ表示用
        private static bool DisplayDialog(string process, string addText = "") {
#if UNITY_EDITOR
            switch (process)
            {
                case PROCESS_SUCCESS_TEXT:
                    EditorUtility.DisplayDialog(CoreSystemLocalize.LocalizeText(PROCESS_TEXT), CoreSystemLocalize.LocalizeText(PROCESS_SUCCESS_TEXT), CoreSystemLocalize.LocalizeText(CLOSE_TEXT));
                    break;
                case PROCESS_CANCEL_TEXT:
                    EditorUtility.DisplayDialog(CoreSystemLocalize.LocalizeText(PROCESS_TEXT), CoreSystemLocalize.LocalizeText(PROCESS_CANCEL_TEXT), CoreSystemLocalize.LocalizeText(CLOSE_TEXT));
                    break;
                case PROCESS_OVERWRITE_TEXT:
                    return EditorUtility.DisplayDialog(CoreSystemLocalize.LocalizeText(PROCESS_TEXT), CoreSystemLocalize.LocalizeText(PROCESS_OVERWRITE_TEXT), CoreSystemLocalize.LocalizeText(YES_TEXT), CoreSystemLocalize.LocalizeText(NO_TEXT));
            }
#endif
            return true;
        }

        // public関数
        //------------------------------------------------------------------
        // JSON書き出し
        // 読み込んだ引数のクラスデータを出力する
        public static string StartToJson<T>(T exportData, string folderName, string fileName = null) {
#if UNITY_EDITOR
            if (fileName == null)
                fileName = folderName;

            var savePath = EditorUtility.OpenFolderPanel(PROCESS_SAVEPATH_TEXT, "", "");

            // 既にフォルダがあるか確認する
            if (Directory.Exists(savePath + "/" + folderName))
                if (!DisplayDialog(PROCESS_OVERWRITE_TEXT))
                {
                    // ダイアログ表示
                    DisplayDialog(PROCESS_CANCEL_TEXT);
                    return "";
                }

            // フォルダ作成＆出力
            Directory.CreateDirectory(savePath + "/" + folderName);
            File.WriteAllText(savePath + "/" + folderName + "/" + fileName + ".json", JsonUtility.ToJson(exportData));

            // ダイアログ表示
            DisplayDialog(PROCESS_SUCCESS_TEXT);

            return savePath;
#else
            return "";
#endif
        }

        // public関数
        //------------------------------------------------------------------
        // JSON書き出し
        // 読み込んだ引数のクラスデータを出力する
        public static string ExportMapEventJsons(EventMapDataModel eventMapData, EventDataModel eventData, string folderName, string fileName = null) {
#if UNITY_EDITOR
            if (fileName == null)
                fileName = folderName;

            var savePath = EditorUtility.OpenFolderPanel(PROCESS_SAVEPATH_TEXT, "", "");
            if (savePath.Length == 0)
            {
                // ダイアログ表示
                DisplayDialog(PROCESS_CANCEL_TEXT);
                return "";
            }

            // 既にフォルダがあるか確認する
            if (Directory.Exists(savePath + "/" + folderName))
                if (!DisplayDialog(PROCESS_OVERWRITE_TEXT))
                {
                    // ダイアログ表示
                    DisplayDialog(PROCESS_CANCEL_TEXT);
                    return "";
                }

            // フォルダ作成＆出力
            Directory.CreateDirectory(savePath + "/" + folderName);
            File.WriteAllText(savePath + "/" + folderName + "/" + fileName + ".json", JsonUtility.ToJson(eventMapData));

            Directory.CreateDirectory(savePath + "/" + folderName + "/Event");
            File.WriteAllText(savePath + "/" + folderName + "/Event/" + fileName + "-" + eventData.page.ToString() + ".json", JsonUtility.ToJson(eventData));

            // ダイアログ表示
            DisplayDialog(PROCESS_SUCCESS_TEXT);

            return savePath;
#else
            return "";
#endif
        }
    }
}