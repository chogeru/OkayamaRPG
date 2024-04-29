using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.Misc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement
{
    public static class AssetManageImporter
    {
        // 画像最大サイズ
        static readonly ReadOnlyDictionary<string, string> LIMIT_IMAGE_SIZE = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()
        {
            { PathManager.IMAGE_CHARACTER    , "1024,1024" },
            { PathManager.IMAGE_OBJECT       , "1024,1024" },
            { PathManager.IMAGE_BALLOON      , "4096,4096" },
            { PathManager.IMAGE_SV_CHARACTER , "4096,4096" },
            { PathManager.IMAGE_WEAPON       , "4096,4096" },
            { PathManager.IMAGE_OVERLAP      , "4096,4096" },
            { PathManager.IMAGE_ANIMATION    , "1000,1000" },
            { PathManager.IMAGE_ENEMY        , "1980,1024" },
            { PathManager.IMAGE_ICON         , "66,66" },
            { PathManager.IMAGE_FACE         , "288,288" },
            { PathManager.IMAGE_ADV          , "1024,1024" },

            { PathManager.MAP_PARALLAX       , "2048,2048" },
            { PathManager.MAP_BACKGROUND     , "4096,4096" },
            { PathManager.MAP_TILE_IMAGE     , "4096,4096" },

            { PathManager.BATTLE_BACKGROUND_1, "2304,1080" },
            { PathManager.BATTLE_BACKGROUND_2, "2304,1080" },

            { PathManager.UI_TITLE_BG        , "1920,1080" },
            { PathManager.UI_TITLE_FRAME     , "1920,1080" },
            { PathManager.UI_TITLE_NAME      , "1920,1080" },
            { PathManager.UI_BG              , "96,96" },
            { PathManager.UI_WINDOW          , "192,192" },
            { PathManager.UI_BUTTON          , "96,96" },
        });

        private const string PROCESS_TEXT           = "WORD_0001";
        private const string CLOSE_TEXT             = "WORD_0002";
        private const string PROCESS_SUCCESS_TEXT   = "WORD_0003";
        private const string PROCESS_FAIL_TEXT      = "WORD_0004";
        private const string PROCESS_CANCEL_TEXT    = "WORD_0005";
        private const string PROCESS_OVERWRITE_TEXT = "WORD_0006";
        private const string FILE_NOT_FOUND_TEXT    = "WORD_0007";
        private const string FILE_CONFLICT_TEXT     = "WORD_0008";
        private const string YES_TEXT               = "WORD_0009";
        private const string NO_TEXT                = "WORD_0010";
        private const string IMAGE_LIMIT_TEXT       = "WORD_0011";

        //pivot初期値
        private static Vector2 defaultPivot => new Vector2(0.5f, 0.5f);

        // private関数
        //------------------------------------------------------------------
        // ダイアログ表示用
        private static bool DisplayDialog(string process, string addText = "") {
#if UNITY_EDITOR
            switch (process)
            {
                case PROCESS_SUCCESS_TEXT:
                    EditorUtility.DisplayDialog(CoreSystemLocalize.LocalizeText(PROCESS_TEXT),
                        CoreSystemLocalize.LocalizeText(PROCESS_SUCCESS_TEXT),
                        CoreSystemLocalize.LocalizeText(CLOSE_TEXT));
                    break;
                case PROCESS_FAIL_TEXT:
                    EditorUtility.DisplayDialog(CoreSystemLocalize.LocalizeText(PROCESS_TEXT),
                        addText + CoreSystemLocalize.LocalizeText(PROCESS_FAIL_TEXT),
                        CoreSystemLocalize.LocalizeText(CLOSE_TEXT));
                    break;
                case PROCESS_CANCEL_TEXT:
                    break;
                case PROCESS_OVERWRITE_TEXT:
                    return EditorUtility.DisplayDialog(CoreSystemLocalize.LocalizeText(PROCESS_TEXT),
                        CoreSystemLocalize.LocalizeText(PROCESS_OVERWRITE_TEXT) + addText,
                        CoreSystemLocalize.LocalizeText(YES_TEXT), CoreSystemLocalize.LocalizeText(NO_TEXT));
                case FILE_NOT_FOUND_TEXT:
                    return EditorUtility.DisplayDialog(CoreSystemLocalize.LocalizeText(PROCESS_TEXT),
                        addText + CoreSystemLocalize.LocalizeText(FILE_NOT_FOUND_TEXT),
                        CoreSystemLocalize.LocalizeText(YES_TEXT), CoreSystemLocalize.LocalizeText(NO_TEXT));
                case FILE_CONFLICT_TEXT:
                    EditorUtility.DisplayDialog(CoreSystemLocalize.LocalizeText(PROCESS_TEXT),
                        CoreSystemLocalize.LocalizeText(FILE_CONFLICT_TEXT),
                        CoreSystemLocalize.LocalizeText(CLOSE_TEXT));
                    break;
                case IMAGE_LIMIT_TEXT:
                    return EditorUtility.DisplayDialog(CoreSystemLocalize.LocalizeText(PROCESS_TEXT),
                        CoreSystemLocalize.LocalizeText(IMAGE_LIMIT_TEXT),
                        CoreSystemLocalize.LocalizeText(YES_TEXT), CoreSystemLocalize.LocalizeText(NO_TEXT));
            }
#endif
            return true;
        }

        // ZIP内に指定拡張子ファイルがあるか確認
        private static bool DataFoundInZip(ZipArchive archive, string extension) {
            if (archive.Entries
                .Where(e => e.FullName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.FullName).ToList().Count == 0)
                return false;
            return true;
        }

        // ZIP内の指定拡張子ファイルを取得
        private static IOrderedEnumerable<ZipArchiveEntry> DataGetInZip(ZipArchive archive, string extension) {
            return archive.Entries
                .Where(e => e.FullName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.FullName);
        }

        // public関数
        //------------------------------------------------------------------
        // JSON読み込み
        // 読み込んだ引数のクラスを返す
        public static T StartToJson<T>() {
#if UNITY_EDITOR
            var savePath = EditorUtility.OpenFolderPanel("Open Import Directory", "", "");
            try
            {
                // ディレクトリ内のファイル全取得
                var data_path =
                    Directory.GetFiles(savePath, "*.json", SearchOption.TopDirectoryOnly);
                for (var i = 0; i < data_path.Length;)
                {
                    data_path[i] = data_path[i].Replace("\\", "/");

                    // 読み込んだテキストを代入
                    var data = File.ReadAllText(data_path[i]);
                    if (data == null)
                        break;

                    var importData = JsonUtility.FromJson<T>(data);
                    if (importData == null)
                        break;

                    // ダイアログ表示
                    DisplayDialog(PROCESS_SUCCESS_TEXT);
                    return importData;
                }
            }
            catch (IOException)
            {
            } // 処理が止まらないようエラーキャッチ（何もしない）
            catch (ArgumentException)
            {
            }

            // エラーダイアログ表示
            DisplayDialog(PROCESS_FAIL_TEXT);
#endif
            return default;
        }

        public static List<string> ImportMapEventJsons() {
            List<string> ret = new List<string>();
#if UNITY_EDITOR
            var savePath = EditorUtility.OpenFolderPanel("Open Import Directory", "", "");
            if (savePath.Length == 0)
            {
                // ダイアログ表示
                DisplayDialog(PROCESS_CANCEL_TEXT);
                return ret;
            }
            try
            {
                // ディレクトリ内のファイル全取得
                var data_path =
                    Directory.GetFiles(savePath, "*.json", SearchOption.TopDirectoryOnly);

                // 最初の1ファイルが EventMapDataModel
                if (data_path.Length <= 0)
                {
                    // エラーダイアログ表示
                    DisplayDialog(PROCESS_FAIL_TEXT);
                    return ret;
                }
                ret.Add(data_path[0]);

                // ディレクトリが存在する場合は、その中身が EventDataModel
                var data_path2 = Directory.GetDirectories(savePath);
                if (data_path2.Length >= 1)
                {
                    var data_path3 =
                        Directory.GetFiles(data_path2[0], "*.json", SearchOption.TopDirectoryOnly);

                    //最初の1ファイルが EventDataModel
                    if (data_path3.Length > 0)
                    {
                        ret.Add(data_path3[0]);
                    }
                }

                // ダイアログ表示
                // DisplayDialog(PROCESS_SUCCESS_TEXT);
                return ret;
            }
            catch (IOException)
            {
            } // 処理が止まらないようエラーキャッチ（何もしない）
            catch (ArgumentException)
            {
            }

            // エラーダイアログ表示
            // DisplayDialog(PROCESS_FAIL_TEXT);
#endif
            return ret;
        }

        /// <summary>
        /// 成功又は失敗のダイアログ表示
        /// </summary>
        public static void ShowDialog(bool success) {
            // ダイアログ表示
            if (success)
            {
                DisplayDialog(PROCESS_SUCCESS_TEXT);
            }
            else
            {
                DisplayDialog(PROCESS_FAIL_TEXT);
            }
        }

        // JSON読み込み
        // 読み込んだ引数のクラスを返す
        public static T ReadJsonToDataModel<T>(string path) {
#if UNITY_EDITOR
            // 読み込んだテキストを代入
            var data = File.ReadAllText(path);
            if (data == null)
                return default;

            var importData = JsonUtility.FromJson<T>(data);
            if (importData != null)
                return importData;
#endif
            return default;
        }

        // データ読み込み
        // 読み込んだデータを指定パスにコピーする
        // 基本設定ではSpriteに変換する
        public static string StartToFile(
            string fileType,
            string savePath,
            Vector2? pivot = null,
            bool textureSprite = true,
            bool textureReadable = false,
            bool textureWrap = false,
            bool setPixelsPerUnit = false,
            bool setLargeSize = false
        ) {
#if UNITY_EDITOR
            string path;

            //.oggが指定された時に.wavもインポートできるようにする
            if (fileType == "ogg")
                path = EditorUtility.OpenFilePanelWithFilters("Overwrite with " + fileType, "",
                    new[] {fileType, fileType, "wav", "wav"});
            else
                path = EditorUtility.OpenFilePanelWithFilters("Overwrite with " + fileType, "",
                    new[] {fileType, fileType});

            if (pivot == null) pivot = defaultPivot;

            // パスがある
            if (path.Length != 0)
            {
                var fileName = Path.GetFileName(path);
                var saveFilePath = Path.Combine(savePath, fileName).Replace('\\', '/');

                // サイズ確認
                if (!CheckSize(fileType, savePath, path))
                    if (!DisplayDialog(IMAGE_LIMIT_TEXT))
                    {
                        // ダイアログ表示
                        DisplayDialog(PROCESS_CANCEL_TEXT);
                        return "";
                    }

                // 既にフォルダがあるか確認する
                if (File.Exists(saveFilePath))
                {
                    if (!DisplayDialog(PROCESS_OVERWRITE_TEXT, fileName))
                    {
                        // ダイアログ表示
                        DisplayDialog(PROCESS_CANCEL_TEXT);
                        return "";
                    }

                    //同じファイルをインポートしたとき、returnで返す
                    //pathを絶対パスからAssetsパスに変更
                    if (path.Replace("\\", "/").Replace(Application.dataPath, "Assets") == saveFilePath)
                    {
                        DisplayDialog(PROCESS_SUCCESS_TEXT);
                        return saveFilePath;
                    }
                    File.Delete(saveFilePath);
                }

                File.Copy(path, saveFilePath);
                AssetDatabase.Refresh();
                // パス登録（Refreshしてからでないとファイルが見つからない）
                AddressableManager.Path.SetAddressToAsset(saveFilePath);
                // ダイアログ表示
                DisplayDialog(PROCESS_SUCCESS_TEXT);
                // 画像形式の場合にテクスチャ設定を適用する
                if (fileType == "png")
                    ChangeTextureType(saveFilePath, textureSprite, textureReadable, textureWrap, pivot,
                        setPixelsPerUnit, setLargeSize);
                return saveFilePath;
            }

            // ダイアログ表示
            DisplayDialog(PROCESS_CANCEL_TEXT);
            return "";
#else
            return "";
#endif
        }

        public static bool ImportFile(
            string path,
            string saveFilePath,
            Vector2? pivot = null,
            bool textureSprite = true,
            bool textureReadable = false,
            bool textureWrap = false,
            bool setPixelsPerUnit = false,
            bool setLargeSize = false
        ) {
#if UNITY_EDITOR
            if (pivot == null) pivot = defaultPivot;

            // サイズ確認は行わない。

            // 既にファイルがあるかの確認不要。
            // AssetDatabaseを一時停止
            AssetDatabase.StartAssetEditing();


            File.Copy(path, saveFilePath);
            if (!File.Exists($"{saveFilePath}.meta"))
            {
                AssetDatabase.ImportAsset(saveFilePath);
            }
            // パス登録
            AddressableManager.Path.SetAddressToAsset(saveFilePath);

            // AssetDatabaseを再開
            AssetDatabase.StopAssetEditing();

            // 画像形式の場合にテクスチャ設定を適用する
            if (saveFilePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                AssetManageImporter.ChangeTextureType(saveFilePath, textureSprite, textureReadable, textureWrap, pivot,
                        setPixelsPerUnit, setLargeSize);
            }
            return true;
#else
            return false;
#endif
        }

        public static List<bool> ImportFile(
            List<ImportFileData> importFileData
        ) {
#if UNITY_EDITOR
            // AssetDatabaseを一時停止
            AssetDatabase.StartAssetEditing();
            for (int i = 0; i < importFileData.Count; i++)
            {
                var path = importFileData[i].Path;
                var saveFilePath = importFileData[i].SavePath;

                File.Copy(path, saveFilePath);
            }
            // AssetDatabaseを再開
            AssetDatabase.StopAssetEditing();

            // Refresh
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh2();

            // AssetDatabaseを一時停止
            AssetDatabase.StartAssetEditing();

            // パス登録
            for (int i = 0; i < importFileData.Count; i++)
            {
                var saveFilePath = importFileData[i].SavePath;
                if (!File.Exists($"{saveFilePath}.meta"))
                {
                    AssetDatabase.ImportAsset(saveFilePath);
                }

                AddressableManager.Path.SetAddressToAsset(saveFilePath);
            }

            // AssetDatabaseを再開
            AssetDatabase.StopAssetEditing();

            // Refresh
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh2();

            // AssetDatabaseを一時停止
            AssetDatabase.StartAssetEditing();

            var textureList = new List<bool>();

            for (int i = 0; i < importFileData.Count; i++)
            {
                var saveFilePath = importFileData[i].SavePath;
                var pivot = importFileData[i].Pivot;
                var textureSprite = importFileData[i].TextureSprite;
                var textureReadable = importFileData[i].TextureReadable;
                var textureWrap = importFileData[i].TextureWrap;
                var setPixelsPerUnit = importFileData[i].SetPixelsPerUnit;
                var setLargeSize = importFileData[i].SetPixelsPerUnit;

                if (pivot == null) pivot = defaultPivot;


                // 画像形式の場合にテクスチャ設定を適用する
                if (saveFilePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    var isSuccess = ChangeTextureAndReturnSuccess(saveFilePath, textureSprite, textureReadable, textureWrap,
                        pivot,
                        setPixelsPerUnit, setLargeSize);
                    textureList.Add(isSuccess);
                }
            }
            // AssetDatabaseを再開
            AssetDatabase.StopAssetEditing();

            // Refresh
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh2();

            return textureList;
#else
            return new List<bool>() {false};
#endif
        }

        // データ読み込み（ZIP）
        // 汎用
        public static void StartToZip(string[] fileType, string[] savePath) {
#if UNITY_EDITOR
            string path;

            path = EditorUtility.OpenFilePanelWithFilters("Overwrite with " + "zip", "", new[] {"zip", "zip"});

            // パスがある
            if (path.Length != 0)
            {
                var pathSplit = path.Split('/');

                // ZIPファイルを開いてZipArchiveオブジェクトを作る
                using (var archive = ZipFile.OpenRead(path))
                {
                    // 無視したデータ数
                    var skipCount = 0;

                    for (var i = 0; i < fileType.Length; i++)
                    {
                        // ZIPからデータ取得
                        var targetFile = DataGetInZip(archive, "." + fileType[i]);

                        if (targetFile.Count() < 1)
                        {
                            // ファイルが見つからない
                            // ダイアログ表示
                            if (!DisplayDialog(FILE_NOT_FOUND_TEXT, fileType[i]))
                            {
                                // ダイアログ表示
                                DisplayDialog(PROCESS_CANCEL_TEXT);
                                return;
                            }

                            skipCount++;
                        }
                    }

                    // 全てのデータが無い
                    if (skipCount == fileType.Length)
                    {
                        // エラーダイアログ表示
                        DisplayDialog(PROCESS_FAIL_TEXT);
                        return;
                    }

                    for (var i = 0; i < fileType.Length; i++)
                    {
                        // ZIPからデータ取得
                        var targetFile = DataGetInZip(archive, "." + fileType[i]);

                        foreach (var entry in targetFile)
                        {
                            // 既にフォルダがあるか確認する
                            if (File.Exists(savePath[i] + entry.FullName))
                            {
                                if (!DisplayDialog(PROCESS_OVERWRITE_TEXT, entry.FullName))
                                {
                                    // ダイアログ表示
                                    DisplayDialog(PROCESS_CANCEL_TEXT);
                                    return;
                                }

                                File.Delete(savePath[i] + entry.FullName);
                            }

                            // ZipArchiveEntryオブジェクトのExtractToFileメソッドにフルパスを渡す
                            entry.ExtractToFile(Path.Combine(savePath[i], entry.FullName));
                            AssetDatabase.Refresh();
                            // パス登録（Refreshしてからでないとファイルが見つからない）
                            AddressableManager.Path.SetAddressToAsset(Path.Combine(savePath[i], entry.FullName));
                        }
                    }
                }

                // ダイアログ表示
                DisplayDialog(PROCESS_SUCCESS_TEXT);
            }
            else
            {
                // ダイアログ表示
                DisplayDialog(PROCESS_CANCEL_TEXT);
            }
#else
            return;
#endif
        }

        // データ読み込み（ZIP）
        // エフェクト用
        public static List<T> StartToZip_Effect<T>(
            List<string> fileType,
            List<string> savePath,
            List<string> effectSavePath
        ) {
#if UNITY_EDITOR
            var jsonData = new List<T>();
            var prefab = true;
            string path;

            path = EditorUtility.OpenFilePanelWithFilters("Overwrite with " + "zip", "", new[] {"zip", "zip"});

            // パスがある
            if (path.Length != 0)
            {
                var pathSplit = path.Split('/');

                // ZIPファイルを開いてZipArchiveオブジェクトを作る
                using (var archive = ZipFile.OpenRead(path))
                {
                    // エフェクトデータがあるか
                    if (!DataFoundInZip(archive, ".prefab") && !DataFoundInZip(archive, ".efkefc"))
                    {
                        // ファイルが見つからない
                        // ダイアログ表示
                        if (!DisplayDialog(FILE_NOT_FOUND_TEXT, "エフェクト"))
                        {
                            // ダイアログ表示
                            DisplayDialog(PROCESS_CANCEL_TEXT);
                            return null;
                        }
                    }
                    else if (DataFoundInZip(archive, ".prefab") && DataFoundInZip(archive, ".efkefc"))
                    {
                        // ダイアログ表示
                        DisplayDialog(FILE_CONFLICT_TEXT);
                        return null;
                    }
                    else
                    {
                        if (!DataFoundInZip(archive, ".prefab"))
                        {
                            effectSavePath[0] = effectSavePath[1];
                            savePath[0] = effectSavePath[1] + "Texture/";
                            prefab = false;

                            savePath.Add(effectSavePath[1] + "Model/");
                            fileType.Add("efkmodel");
                        }

                        // エフェクトのパス等を追加する
                        savePath.Add(effectSavePath[0]);
                        savePath.Add(effectSavePath[0]);
                        if (prefab)
                        {
                            fileType.Add("prefab");
                            fileType.Add("prefab.meta");
                        }
                        else
                            fileType.Add("efkefc");
                    }

                    // 無視したデータ数
                    var skipCount = 0;

                    for (var i = 0; i < fileType.Count; i++)
                    {
                        // ZIPからデータ取得
                        var targetFile = DataGetInZip(archive, "." + fileType[i]);

                        if (targetFile.Count() < 1)
                        {
                            // ファイルが見つからない
                            // ダイアログ表示
                            if (!DisplayDialog(FILE_NOT_FOUND_TEXT, fileType[i]))
                            {
                                // ダイアログ表示
                                DisplayDialog(PROCESS_CANCEL_TEXT);
                                return null;
                            }

                            skipCount++;
                        }
                    }

                    // 全てのデータが無い
                    if (skipCount == fileType.Count)
                    {
                        // エラーダイアログ表示
                        DisplayDialog(PROCESS_FAIL_TEXT);
                        return null;
                    }

                    for (var i = 0; i < fileType.Count; i++)
                    {
                        // ZIPからデータ取得
                        var targetFile = DataGetInZip(archive, "." + fileType[i]);

                        foreach (var entry in targetFile)
                        {
                            // jsonファイルの処理
                            if (fileType[i] == "json")
                            {
                                using (var sr = new StreamReader(archive.GetEntry(entry.FullName).Open(),
                                    Encoding.GetEncoding("shift_jis")))
                                {
                                    var str = sr.ReadToEnd();
                                    if (str == null)
                                        continue;

                                    try
                                    {
                                        var importData = JsonUtility.FromJson<T>(str);
                                        if (importData == null)
                                            continue;

                                        jsonData.Add(importData);
                                    }
                                    catch
                                    {
                                        // キャスト失敗
                                        // ダイアログ表示
                                        DisplayDialog(PROCESS_FAIL_TEXT, entry.FullName);
                                    }
                                }

                                continue;
                            }

                            // 既にフォルダがあるか確認する
                            if (File.Exists(savePath[i] + entry.FullName))
                            {
                                if (!DisplayDialog(PROCESS_OVERWRITE_TEXT, entry.FullName))
                                {
                                    // ダイアログ表示
                                    DisplayDialog(PROCESS_CANCEL_TEXT);
                                    return null;
                                }

                                File.Delete(savePath[i] + entry.FullName);
                            }

                            // ZipArchiveEntryオブジェクトのExtractToFileメソッドにフルパスを渡す
                            entry.ExtractToFile(Path.Combine(savePath[i], entry.FullName));
                            AssetDatabase.Refresh();
                            // パス登録（Refreshしてからでないとファイルが見つからない）
                            AddressableManager.Path.SetAddressToAsset(Path.Combine(savePath[i], entry.FullName));
                        }
                    }
                }

                // ダイアログ表示
                DisplayDialog(PROCESS_SUCCESS_TEXT);
                return jsonData;
            }

            // ダイアログ表示
            DisplayDialog(PROCESS_CANCEL_TEXT);
            return null;
#else
            return null;
#endif
        }

        public static List<T> ImportZip_Effect<T>(
            string path,
            List<string> fileType,
            List<string> savePath,
            List<string> effectSavePath
        ) {
#if UNITY_EDITOR
            var jsonData = new List<T>();
            var prefab = true;

            var pathSplit = path.Split('/');

            // ZIPファイルを開いてZipArchiveオブジェクトを作る
            using (var archive = ZipFile.OpenRead(path))
            {
                // エフェクトデータがあるか
                if (!DataFoundInZip(archive, ".prefab") && !DataFoundInZip(archive, ".efkefc"))
                {
                    // ファイルが見つからない
                    return null;
                }
                else if (DataFoundInZip(archive, ".prefab") && DataFoundInZip(archive, ".efkefc"))
                {
                    // ダイアログ表示
                    return null;
                }
                else
                {
                    if (!DataFoundInZip(archive, ".prefab"))
                    {
                        effectSavePath[0] = effectSavePath[1];
                        savePath[0] = effectSavePath[1] + "Texture/";
                        prefab = false;

                        savePath.Add(effectSavePath[1] + "Model/");
                        fileType.Add("efkmodel");
                    }

                    // エフェクトのパス等を追加する
                    savePath.Add(effectSavePath[0]);
                    if (prefab)
                        fileType.Add("prefab");
                    else
                        fileType.Add("efkefc");
                }

                // 無視したデータ数
                var skipCount = 0;

                for (var i = 0; i < fileType.Count; i++)
                {
                    // ZIPからデータ取得
                    var targetFile = DataGetInZip(archive, "." + fileType[i]);

                    if (targetFile.Count() < 1)
                    {
                        // ファイルが見つからない
                        skipCount++;
                    }
                }

                // 全てのデータが無い
                if (skipCount == fileType.Count)
                {
                    // エラーダイアログ表示
                    return null;
                }

                for (var i = 0; i < fileType.Count; i++)
                {
                    // ZIPからデータ取得
                    var targetFile = DataGetInZip(archive, "." + fileType[i]);

                    foreach (var entry in targetFile)
                    {
                        // jsonファイルの処理
                        if (fileType[i] == "json")
                        {
                            using (var sr = new StreamReader(archive.GetEntry(entry.FullName).Open(),
                                Encoding.GetEncoding("shift_jis")))
                            {
                                var str = sr.ReadToEnd();
                                if (str == null)
                                    continue;

                                try
                                {
                                    var importData = JsonUtility.FromJson<T>(str);
                                    if (importData == null)
                                        continue;

                                    jsonData.Add(importData);
                                }
                                catch
                                {
                                    // キャスト失敗
                                    // ダイアログ表示
                                }
                            }

                            continue;
                        }

                        // 既にフォルダがあるか確認する
                        if (File.Exists(savePath[i] + entry.FullName))
                        {
                            File.Delete(savePath[i] + entry.FullName);
                        }

                        // ZipArchiveEntryオブジェクトのExtractToFileメソッドにフルパスを渡す
                        entry.ExtractToFile(Path.Combine(savePath[i], entry.FullName));
                        AssetDatabase.Refresh();
                        // パス登録（Refreshしてからでないとファイルが見つからない）
                        AddressableManager.Path.SetAddressToAsset(Path.Combine(savePath[i], entry.FullName));
                    }
                }

                return jsonData;
            }
#else
            return null;
#endif
        }


        // データ読み込み（ZIP）
        // jsonデータを返す
        public static List<T> StartToZip_GetJson<T>(string[] fileType, string[] savePath) {
#if UNITY_EDITOR
            var jsonData = new List<T>();
            string path;

            path = EditorUtility.OpenFilePanelWithFilters("Overwrite with " + "zip", "", new[] {"zip", "zip"});

            // パスがある
            if (path.Length != 0)
            {
                var pathSplit = path.Split('/');

                // ZIPファイルを開いてZipArchiveオブジェクトを作る
                using (var archive = ZipFile.OpenRead(path))
                {
                    // 無視したデータ数
                    var skipCount = 0;

                    for (var i = 0; i < fileType.Length; i++)
                    {
                        // ZIPからデータ取得
                        var targetFile = DataGetInZip(archive, "." + fileType[i]);

                        if (targetFile.Count() < 1)
                        {
                            // ファイルが見つからない
                            // ダイアログ表示
                            if (!DisplayDialog(FILE_NOT_FOUND_TEXT, fileType[i]))
                            {
                                // ダイアログ表示
                                DisplayDialog(PROCESS_CANCEL_TEXT);
                                return null;
                            }

                            skipCount++;
                        }
                    }

                    // 全てのデータが無い
                    if (skipCount == fileType.Length)
                    {
                        // エラーダイアログ表示
                        DisplayDialog(PROCESS_FAIL_TEXT);
                        return null;
                    }

                    for (var i = 0; i < fileType.Length; i++)
                    {
                        // ZIPからデータ取得
                        var targetFile = DataGetInZip(archive, "." + fileType[i]);

                        foreach (var entry in targetFile)
                        {
                            // jsonファイルの処理
                            if (fileType[i] == "json")
                            {
                                using (var sr = new StreamReader(archive.GetEntry(entry.FullName).Open(),
                                    Encoding.GetEncoding("shift_jis")))
                                {
                                    var str = sr.ReadToEnd();
                                    if (str == null)
                                        continue;

                                    try
                                    {
                                        var importData = JsonUtility.FromJson<T>(str);
                                        if (importData == null)
                                            continue;

                                        jsonData.Add(importData);
                                    }
                                    catch
                                    {
                                        // キャスト失敗
                                        // ダイアログ表示
                                        DisplayDialog(PROCESS_FAIL_TEXT, entry.FullName);
                                    }
                                }

                                continue;
                            }

                            // 既にフォルダがあるか確認する
                            if (File.Exists(savePath[i] + entry.FullName))
                            {
                                if (!DisplayDialog(PROCESS_OVERWRITE_TEXT, entry.FullName))
                                {
                                    // ダイアログ表示
                                    DisplayDialog(PROCESS_CANCEL_TEXT);
                                    return null;
                                }

                                File.Delete(savePath[i] + entry.FullName);
                            }

                            // ZipArchiveEntryオブジェクトのExtractToFileメソッドにフルパスを渡す
                            entry.ExtractToFile(Path.Combine(savePath[i], entry.FullName));
                            AssetDatabase.Refresh();
                            // パス登録（Refreshしてからでないとファイルが見つからない）
                            AddressableManager.Path.SetAddressToAsset(Path.Combine(savePath[i], entry.FullName));
                        }
                    }
                }

                // ダイアログ表示
                DisplayDialog(PROCESS_SUCCESS_TEXT);
                return jsonData;
            }

            // ダイアログ表示
            DisplayDialog(PROCESS_CANCEL_TEXT);
            return null;
#else
            return null;
#endif
        }

        // テクスチャ設定を変更する
        // path             = ファイルパス
        // textureSprite    = TexturetypeをSpriteにするか
        // textureReadble   = Read/Writeにチェックを入れるか
        // textureWrap      = WrapModeをRepeatにするか
        private static void ChangeTextureType(
            string path,
            bool textureSprite = false,
            bool textureReadable = false,
            bool textureWrap = false,
            Vector2? pivot = null,
            bool setPixelsPerUnit = false,
            bool setLargeSize = false
        ) {
#if UNITY_EDITOR
            var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (textureImporter == null) return;

            // 各パラメータ設定
            if (textureSprite)
                textureImporter.textureType = TextureImporterType.Sprite; // テクスチャタイプ変更
            if (textureReadable)
                textureImporter.isReadable = true;
            if (textureWrap)
                textureImporter.wrapMode = TextureWrapMode.Repeat;
            if (setPixelsPerUnit)
                textureImporter.spritePixelsPerUnit = 96;
            if (setLargeSize)
                textureImporter.maxTextureSize = 4096;

            var texSettings = new TextureImporterSettings();
            textureImporter.ReadTextureSettings(texSettings);
            texSettings.spriteAlignment = (int) SpriteAlignment.Custom;
            textureImporter.SetTextureSettings(texSettings);
            textureImporter.spritePivot = (Vector2) pivot;

            textureImporter.SaveAndReimport();
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Textureのタイプを変更
        /// </summary>
        /// <param name="path"></param>
        /// <param name="textureSprite"></param>
        /// <param name="textureReadable"></param>
        /// <param name="textureWrap"></param>
        /// <param name="pivot"></param>
        /// <param name="setPixelsPerUnit"></param>
        /// <param name="setLargeSize"></param>
        /// <returns>成功かどうか</returns>
        private static bool ChangeTextureAndReturnSuccess(
            string path,
            bool textureSprite = false,
            bool textureReadable = false,
            bool textureWrap = false,
            Vector2? pivot = null,
            bool setPixelsPerUnit = false,
            bool setLargeSize = false
        ) {
            var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (textureImporter == null) return false;

            // 各パラメータ設定
            if (textureSprite)
                textureImporter.textureType = TextureImporterType.Sprite; // テクスチャタイプ変更
            if (textureReadable)
                textureImporter.isReadable = true;
            if (textureWrap)
                textureImporter.wrapMode = TextureWrapMode.Repeat;
            if (setPixelsPerUnit)
                textureImporter.spritePixelsPerUnit = 96;
            if (setLargeSize)
                textureImporter.maxTextureSize = 4096;

            var texSettings = new TextureImporterSettings();
            textureImporter.ReadTextureSettings(texSettings);
            texSettings.spriteAlignment = (int) SpriteAlignment.Custom;
            textureImporter.SetTextureSettings(texSettings);
            textureImporter.spritePivot = (Vector2) pivot;

            textureImporter.SaveAndReimport();

            return true;
        }
#endif

#if ENABLE_DEVELOPMENT_FIX
        public static void ChangeTextureTypeFix(
            string path,
            bool textureReadble = false,
            bool none = false,
            bool changeMeshType = false
        ) {
#if UNITY_EDITOR
            var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (textureImporter == null) return;

            // 各パラメータ設定
            if (textureReadble)
                textureImporter.isReadable = true;
            else
                textureImporter.isReadable = false;

            if (none)
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            else
                textureImporter.textureCompression = TextureImporterCompression.Compressed;

            textureImporter.spritePixelsPerUnit = 96;

            var texSettings = new TextureImporterSettings();
            if (changeMeshType)
                texSettings.spriteMeshType = SpriteMeshType.FullRect;

            textureImporter.ReadTextureSettings(texSettings);
            texSettings.spriteAlignment = (int) SpriteAlignment.Custom;
            textureImporter.SetTextureSettings(texSettings);

            textureImporter.SaveAndReimport();
#endif
        }
#endif

        public static string StartToFileBySprite(
            string fileType,
            string savePath
        ) {
#if UNITY_EDITOR
            string path = EditorUtility.OpenFilePanelWithFilters("Overwrite with " + fileType, "", new[] {fileType, fileType});

            // パスがある
            if (path.Length != 0)
            {
                var fileName = Path.GetFileName(path);
                var saveFilePath = Path.Combine(savePath, fileName).Replace('\\', '/');

                // 既にフォルダがあるか確認する
                if (File.Exists(saveFilePath))
                {
                    if (!DisplayDialog(PROCESS_OVERWRITE_TEXT, fileName))
                    {
                        // ダイアログ表示
                        DisplayDialog(PROCESS_CANCEL_TEXT);
                        return "";
                    }

                    //同じファイルをインポートしたとき、returnで返す
                    //pathを絶対パスからAssetsパスに変更
                    if (path.Replace("\\", "/").Replace(Application.dataPath, "Assets") == saveFilePath)
                    {
                        DisplayDialog(PROCESS_SUCCESS_TEXT);
                        return saveFilePath;
                    }

                    File.Delete(saveFilePath);
                }

                File.Copy(path, saveFilePath);
                AssetDatabase.Refresh();
                // パス登録（Refreshしてからでないとファイルが見つからない）
                AddressableManager.Path.SetAddressToAsset(saveFilePath);
                // ダイアログ表示
                DisplayDialog(PROCESS_SUCCESS_TEXT);
                // 画像形式の場合にテクスチャ設定を適用する
                if (fileType == "png")
                {
                    var textureImporter = AssetImporter.GetAtPath(savePath + fileName) as TextureImporter;
                    if (textureImporter == null) return "";


                    // 各パラメータ設定
                    textureImporter.textureType = TextureImporterType.Sprite; // テクスチャタイプ変更
                    textureImporter.isReadable = true;
                    textureImporter.wrapMode = TextureWrapMode.Repeat;
                    textureImporter.spritePixelsPerUnit = 96;

                    var texSettings = new TextureImporterSettings();
                    textureImporter.ReadTextureSettings(texSettings);
                    texSettings.spriteAlignment = (int) SpriteAlignment.Custom;
                    textureImporter.SetTextureSettings(texSettings);
                    textureImporter.spritePivot = (Vector2) defaultPivot;

                    textureImporter.SaveAndReimport();

                    //サイズが変わるのでここで9スライスのBorderを決める
                    var texture = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(savePath + fileName);
                    var borderSize = texture.width / 3f;
                    texSettings.spriteBorder = new Vector4(borderSize,borderSize,borderSize,borderSize);
                    textureImporter.SetTextureSettings(texSettings);
                    textureImporter.SaveAndReimport();
                }

                return saveFilePath;
            }

            // ダイアログ表示
            DisplayDialog(PROCESS_CANCEL_TEXT);
            return "";
#else
            return "";
#endif
        }

        public static bool CheckSize(string fileType, string basePath, string filePath) {
            if (fileType != "png") return true;

            // 対象の最大サイズを取得
            var sizeStr = LIMIT_IMAGE_SIZE[basePath];
            var size = new Vector2Int(int.Parse(sizeStr.Split(",")[0]), int.Parse(sizeStr.Split(",")[1]));

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                BinaryReader bin = new BinaryReader(fileStream);
                byte[] values = bin.ReadBytes((int) bin.BaseStream.Length);
                bin.Close();
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(values);
                if (texture.width > size.x || texture.height > size.y) return false;
            }

            return true;
        }
    }

    public class ImportFileData
    {
        public string Path;
        public string SavePath;
        public Vector2? Pivot = null;
        public bool TextureSprite = true;
        public bool TextureReadable = false;
        public bool TextureWrap = false;
        public bool SetPixelsPerUnit = false;
        public bool SetLargeSize = false;

        public ImportFileData(
            string path,
            string savePath
        ) {
            Path = path;
            SavePath = savePath;
        }
    }
}