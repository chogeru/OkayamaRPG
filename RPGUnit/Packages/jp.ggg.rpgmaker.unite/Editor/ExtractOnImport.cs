using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Compilation;

internal static class ExtractOnImport
{
    private static readonly List<string> s_templateNames = new List<string>
    {
        "project_base_v",
        // "masterdata_common_v",
        "masterdata_jp_v",
        "masterdata_en_v",
        "masterdata_ch_v",
        // "defaultgame_common_v",
        "defaultgame_jp_v",
        "defaultgame_en_v",
        "defaultgame_ch_v",
    };
    private static readonly string localTemplatePath = Directory.GetCurrentDirectory() + "/Packages/jp.ggg.rpgmaker.unite/System/Archive/";
    private static readonly string LocalVersionPath = Directory.GetCurrentDirectory() + "/Packages/jp.ggg.rpgmaker.unite/version.txt";
    private static readonly string LocalVersionCodePath = Directory.GetCurrentDirectory() + "/Packages/jp.ggg.rpgmaker.unite/versioncode.txt";
    private static readonly string LocalVersionCodePathOne = Directory.GetCurrentDirectory() + "/Packages/jp.ggg.rpgmaker.unite/versioncode1.txt";
    private static readonly string LocalVersionCodePathTwo = Directory.GetCurrentDirectory() + "/Packages/jp.ggg.rpgmaker.unite/versioncode2.txt";
    private static readonly string LocalVersionCodePathThree = Directory.GetCurrentDirectory() + "/Packages/jp.ggg.rpgmaker.unite/versioncode3.txt";
    private static readonly string LocalVersionCodePathFour = Directory.GetCurrentDirectory() + "/Packages/jp.ggg.rpgmaker.unite/versioncode4.txt";
    private static string kAssetPackageSystemPath = "Packages/jp.ggg.rpgmaker.unite/System";
    private const string VERSION = "1.0.8";
    private const string PROJECT_BASE = "project_base_v";
    // private const string DEFAULTGAME_COMMON = "masterdata_common_v";
    private const string DEFAULTGAME_JP     = "masterdata_jp_v";
    private const string DEFAULTGAME_EN     = "masterdata_en_v";
    private const string DEFAULTGAME_CN     = "masterdata_ch_v";
#if UNITY_EDITOR_WIN
    private static int folderSub = 2;
#else
    private static int folderSub = 4;
#endif

    private static string localVer = "1.0.0";
    private static string templateVer = "1.0.0";

    private static bool IsTemplateAvailableForInstall() {
        string folderPath = Application.persistentDataPath;
        string[] folderSplit = folderPath.Split("/");
        folderPath = "";
        for (int i = 0; i < folderSplit.Length - folderSub; i++)
            folderPath += folderSplit[i] + "/";
        folderPath += ".RPGMaker/";

        int cnt = 0;
        foreach (var t in s_templateNames)
        {
            if (File.Exists(folderPath + GetFileName(t)) == false)
            {
                if (File.Exists(localTemplatePath + t))
                {
                    cnt++;
                }
            }
        }
        if (cnt == s_templateNames.Count)
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// テンプレート配置処理/Install template files to Unity
    /// </summary>
    public static void InstallRpgMakerUniteTemplates() {
        if (IsTemplateAvailableForInstall())
        {
            return;
        }
        string folderPath = Application.persistentDataPath;
        string[] folderSplit = folderPath.Split("/");
        folderPath = "";
        for (int i = 0; i < folderSplit.Length - folderSub; i++)
            folderPath += folderSplit[i] + "/";
        folderPath += ".RPGMaker";

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        //異なるVersionのStorageを削除する
        string[] files = Directory.GetFiles(folderPath);
        string[] fileNames = new string[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            fileNames[i] = Path.GetFileName(files[i]);
        }
        folderPath += "/";
        for (var i = 0; i < fileNames.Length; i++)
        {
            bool flg = false;
            for (var j = 0; j < s_templateNames.Count; j++)
            {
                if (fileNames[i] == GetFileName(s_templateNames[j]))
                {
                    flg = true;
                    break;
                }
            }
            if (!flg)
            {
                File.Delete(folderPath + fileNames[i]);
            }
        }
        //Storageをユーザーフォルダにコピーする
        for (var i = 0; i < s_templateNames.Count; i++)
        {
            if (File.Exists(folderPath + GetFileName(s_templateNames[i])) == false)
            {
                if (File.Exists(localTemplatePath + GetFileName(s_templateNames[i])))
                {
                    File.Copy(localTemplatePath + GetFileName(s_templateNames[i]),
                        folderPath + GetFileName(s_templateNames[i]));
                }
            }
        }
    }
    private static bool UpdateContents() {
        localVer = "1.0.0";
        templateVer = "1.0.0";
        if (File.Exists(LocalVersionPath))
        {
            using var fs = new FileStream(LocalVersionPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fs, Encoding.UTF8);
            localVer = reader.ReadToEnd();
        }

        if (File.Exists(localTemplatePath + GetFileName(PROJECT_BASE)))
        {
            templateVer = VERSION;
        }

        var v1 = new Version(templateVer);
        var v2 = new Version(localVer);
        int comparisonResult = v1.CompareTo(v2);

        if (comparisonResult > 0)
        {
            return true;
        }
        return false;
    }

    [InitializeOnLoadMethod]
    private static async void ImportRPGMakerAssetsOnImport() {
        // zipファイルが存在しない場合は、新規PJ作成又は、既にアップデート処理済み
        // そのケースでは、新規PJ作成時に、最新バージョンのプログラムとStorageになっているため、本処理自体が不要である
        if (!Directory.Exists(Directory.GetCurrentDirectory() + "/Packages/jp.ggg.rpgmaker.unite/System/Archive"))
        {
            return;
        }

        // プログラムが同一バージョンであれば処理しない
        if (!UpdateContents())
        {
            return;
        }

        // アップデート処理中である場合にInitializeOnLoadが再度実行された場合には処理しない
        // このケースは、コンパイル要求実施から、コンパイル終了までの間になるため、
        // この条件を満たす = コンパイル途中で強制的にUnityを終了して再起動した、などの特殊なケースになるため、
        // コンパイルが終了したものとみなして、後続の処理を実施する
        if (File.Exists(LocalVersionCodePath))
        {
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            RemoveUpdateProgram();
            EditorUtility.ClearProgressBar();
            EditorApplication.update -= Update;
            return;
        }

        // アップデートが必要
        EditorUtility.DisplayProgressBar("Update", "Now progressing...", 0);

        // プログラム配下をアップデートするかどうか
        if (localVer == "1.0.0")
        {
            // 通常のUniteの起動シーケンスが終了するのを待つ
            await Task.Delay(20000);
        }
        EditorUtility.DisplayProgressBar("Update", "Now progressing...", 0.25f);

        if (!Directory.Exists(Directory.GetCurrentDirectory()))
        {
            Directory.CreateDirectory(Directory.GetCurrentDirectory());
        }

        // プログラム配下をアップデートするかどうか
        // 指定されたフォルダ位置に、プログラムファイル一式を解凍する
        EditorApplication.LockReloadAssemblies();
        if (!File.Exists(LocalVersionCodePathFour))
        {
            if (Directory.Exists(Directory.GetCurrentDirectory() + "/Assets/RPGMaker/Codebase"))
            {
                // DLLファイル
                bool zipFlg = false;
                EditorUtility.DisplayProgressBar("Update", "Now progressing...", 0.4f);
                if (!File.Exists(LocalVersionCodePathOne))
                {
                    if (Directory.Exists(localTemplatePath))
                    {
                        if (!zipFlg) {
                            zipFlg = true;
                            ZipFile.ExtractToDirectory(localTemplatePath + GetFileName(PROJECT_BASE), Directory.GetCurrentDirectory() + "/rpgmaker", true);
                        }
                        MoveDirectory(Directory.GetCurrentDirectory() + "/rpgmaker", Directory.GetCurrentDirectory(), 1);
                    }

                    // 解凍したことを書きこむ
                    var fs = File.Create(LocalVersionCodePathOne);
                    var writer = new StreamWriter(fs, Encoding.UTF8);
                    writer.Write("Program Update");
                    writer.Close();
                    fs.Close();
                }

                // CSファイル
                EditorUtility.DisplayProgressBar("Update", "Now progressing...", 0.5f);
                if (!File.Exists(LocalVersionCodePathTwo))
                {
                    if (Directory.Exists(localTemplatePath))
                    {
                        if (!zipFlg) {
                            zipFlg = true;
                            ZipFile.ExtractToDirectory(localTemplatePath + GetFileName(PROJECT_BASE), Directory.GetCurrentDirectory() + "/rpgmaker", true);
                        }
                        MoveDirectory(Directory.GetCurrentDirectory() + "/rpgmaker", Directory.GetCurrentDirectory(), 2);
                    }

                    // 解凍したことを書きこむ
                    var fs = File.Create(LocalVersionCodePathTwo);
                    var writer = new StreamWriter(fs, Encoding.UTF8);
                    writer.Write("Program Update");
                    writer.Close();
                    fs.Close();
                }

                // inputactionsファイル
                EditorUtility.DisplayProgressBar("Update", "Now progressing...", 0.6f);
                if (!File.Exists(LocalVersionCodePathThree))
                {
                    if (Directory.Exists(localTemplatePath))
                    {
                        if (!zipFlg) {
                            zipFlg = true;
                            ZipFile.ExtractToDirectory(localTemplatePath + GetFileName(PROJECT_BASE), Directory.GetCurrentDirectory() + "/rpgmaker", true);
                        }
                        MoveDirectory(Directory.GetCurrentDirectory() + "/rpgmaker", Directory.GetCurrentDirectory(), 3);
                    }

                    // 解凍したことを書きこむ
                    var fs = File.Create(LocalVersionCodePathThree);
                    var writer = new StreamWriter(fs, Encoding.UTF8);
                    writer.Write("Program Update");
                    writer.Close();
                    fs.Close();
                }
            }
            else {
                // 全て展開して書き込む
                ZipFile.ExtractToDirectory(localTemplatePath + GetFileName(PROJECT_BASE), Directory.GetCurrentDirectory() + "/rpgmaker", true);
                MoveDirectory(Directory.GetCurrentDirectory() + "/rpgmaker", Directory.GetCurrentDirectory(), 1);

                // 解凍したことを書きこむ
                var fs = File.Create(LocalVersionCodePathOne);
                var writer = new StreamWriter(fs, Encoding.UTF8);
                writer.Write("Program Update");
                writer.Close();
                fs.Close();

                // 解凍したことを書きこむ
                fs = File.Create(LocalVersionCodePathTwo);
                writer = new StreamWriter(fs, Encoding.UTF8);
                writer.Write("Program Update");
                writer.Close();
                fs.Close();

                // 解凍したことを書きこむ
                fs = File.Create(LocalVersionCodePathThree);
                writer = new StreamWriter(fs, Encoding.UTF8);
                writer.Write("Program Update");
                writer.Close();
                fs.Close();
            }

            // プログラムの更新を行ったことを一時ファイルに保存
            EditorUtility.DisplayProgressBar("Update", "Now progressing...", 0.7f);
            var fsend = File.Create(LocalVersionCodePathFour);
            var writerend = new StreamWriter(fsend, Encoding.UTF8);
            writerend.Write("Program Update");
            writerend.Close();
            fsend.Close();
        }

        // 不要ディレクトリの削除
        try {
            Directory.Delete(Directory.GetCurrentDirectory() + "/rpgmaker", true);
            File.Delete(Directory.GetCurrentDirectory() + "/rpgmaker" + ".meta");
        }
        catch (Exception)
        {
        }

        // Storage関連処理
        ImportRPGMakerAssetsOnImportStorage();
    }

    private static void ImportRPGMakerAssetsOnImportStorage() {
        // プログラムをアップデート後、かつ zip ファイルが localTemplatePath に存在する場合
        // zipファイル配置処理
        InstallRpgMakerUniteTemplates();

        // Storageは、すでに存在する場合には上書きしない
        if (!Directory.Exists(Directory.GetCurrentDirectory() + "/Assets/RPGMaker/Storage"))
        {
            // AssetDatabaseを一時停止
            AssetDatabase.StartAssetEditing();

            // 各zipファイルの保存場所を取得
            var _folderPath = Application.persistentDataPath;
            string[] folderSplit = _folderPath.Split("/");
            _folderPath = "";

            for (int i = 0; i < folderSplit.Length - folderSub; i++)
                _folderPath += folderSplit[i] + "/";
            _folderPath += ".RPGMaker/";

            // 指定されたフォルダ内の、Storage領域に、共通Storageを解凍する
            // ZipFile.ExtractToDirectory(_folderPath + GetFileName(DEFAULTGAME_COMMON),
            //     Directory.GetCurrentDirectory() + "/Assets/RPGMaker", true);
            ZipFile.ExtractToDirectory(_folderPath + GetFileName(DEFAULTGAME_JP),
                Directory.GetCurrentDirectory() + "/Assets/RPGMaker/Storage", true);

            // 現在の言語設定
            var assembly2 = typeof(EditorWindow).Assembly;
            var localizationDatabaseType2 = assembly2.GetType("UnityEditor.LocalizationDatabase");
            var currentEditorLanguageProperty2 = localizationDatabaseType2.GetProperty("currentEditorLanguage");
            var lang2 = (SystemLanguage) currentEditorLanguageProperty2.GetValue(null);
            // 指定されたフォルダ内の、Storage領域に、言語Storageを解凍する
            switch (lang2){
                case SystemLanguage.Japanese:
                    break;
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                    ZipFile.ExtractToDirectory(_folderPath + GetFileName(DEFAULTGAME_CN),
                        Directory.GetCurrentDirectory() + "/Assets/RPGMaker/Storage", true);
                    break;
                default:
                    ZipFile.ExtractToDirectory(_folderPath + GetFileName(DEFAULTGAME_EN),
                        Directory.GetCurrentDirectory() + "/Assets/RPGMaker/Storage", true);
                    break;
            }
            // AssetDatabaseを再開
            AssetDatabase.StopAssetEditing();
        }

        // コンパイル完了待ち
        // 一時的にコメントアウトして、コンパイルを実行させないようにして、ログを残すようにする
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayProgressBar("Update", "Now progressing...", 0.8f);
        var fs = File.Create(LocalVersionCodePath);
        var writer = new StreamWriter(fs, Encoding.UTF8);
        writer.Write("Program Update");
        writer.Close();
        fs.Close();

        // リコンパイル
        EditorUtility.DisplayProgressBar("Update", "Now progressing...", 0.9f);
        EditorApplication.UnlockReloadAssemblies();
        AssetDatabase.Refresh();
        CompilationPipeline.RequestScriptCompilation();
        CompilationPipeline.compilationFinished += OnCompilationFinished;

        // プログレスバー表示処理
        EditorApplication.update += Update;
    }

    private static void Update() {
        if (File.Exists(LocalVersionCodePath))
        {
            EditorUtility.DisplayProgressBar("Update", "Now progressing...", 0.9f);
        }
        else
        {
            EditorUtility.ClearProgressBar();
            EditorApplication.update -= Update;
        }
    }

    private static void OnCompilationFinished(object obj) {
        if (File.Exists(LocalVersionCodePath))
        {
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            RemoveUpdateProgram();
            EditorUtility.ClearProgressBar();
            EditorApplication.update -= Update;
        }
    }

    private static void RemoveUpdateProgram() {
        try
        {
            Directory.Delete(kAssetPackageSystemPath, true);
            File.Delete(kAssetPackageSystemPath + ".meta");
        }
        catch (Exception)
        {
        }
        try {
            File.Delete(LocalVersionCodePath);
            File.Delete(LocalVersionCodePath + ".meta");
        }
        catch (Exception)
        {
        }
        try {
            File.Delete(LocalVersionCodePathOne);
            File.Delete(LocalVersionCodePathOne + ".meta");
        }
        catch (Exception)
        {
        }
        try {
            File.Delete(LocalVersionCodePathTwo);
            File.Delete(LocalVersionCodePathTwo + ".meta");
        }
        catch (Exception)
        {
        }
        try {
            File.Delete(LocalVersionCodePathThree);
            File.Delete(LocalVersionCodePathThree + ".meta");
        }
        catch (Exception)
        {
        }
        try
        {
            File.Delete(LocalVersionCodePathFour);
            File.Delete(LocalVersionCodePathFour + ".meta");
        }
        catch (Exception)
        {
        }

        if (File.Exists(LocalVersionPath))
        {
            using var fs = new FileStream(LocalVersionPath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
            using var writer = new StreamWriter(fs, Encoding.UTF8);
            writer.Write(VERSION);
        }
        else
        {
            using var fs = File.Create(LocalVersionPath);
            using var writer = new StreamWriter(fs, Encoding.UTF8);
            writer.Write(VERSION);
        }
    }
    private static string GetFileName(string name) {
        return name + VERSION + ".zip";
    }
    private static void MoveDirectory(string source, string destination, int type) {
        // 移動元のディレクトリとその中身を取得
        DirectoryInfo sourceDirectory = new DirectoryInfo(source);

        // 移動先のディレクトリが存在しない場合は作成
        if (!Directory.Exists(destination))
        {
            Directory.CreateDirectory(destination);
        }

        // 移動元のディレクトリ内のファイルをすべて移動
        foreach (FileInfo file in sourceDirectory.GetFiles())
        {
            string tempPath = Path.Combine(destination, file.Name);
            if (File.Exists(tempPath))
            {
                if (file.Name == "UpdateScene.unity" || 
                    file.Name == "ExtractOnImport.cs" ||
                    file.Name == "Title.unity" ||
                    file.Name == "TitleCanvas.prefab") 
                {
                    //バージョンアップに利用しているファイルは上書きしない
                    continue;
                }

                string [] tempExtention = file.Name.Split(".");
                if (type == 1) {
                    // v1.0.7からの更新処理：フォントファイル更新
                    if (localVer == "1.0.7" && file.Name == "BIZ-UDGOTHICB SDF.asset")
                    {
                    }else if( tempExtention[tempExtention.Length - 1] != "dll")
                    {
                        //更新可能な拡張子以外のファイルは上書きしない
                        continue;
                    }
                }
                else if (type == 2) {
                    if (tempExtention[tempExtention.Length - 1] != "cs" &&
                        tempExtention[tempExtention.Length - 1] != "uxml" &&
                        tempExtention[tempExtention.Length - 1] != "uss" &&
                        tempExtention[tempExtention.Length - 1] != "prefab")
                    {
                        //更新可能な拡張子以外のファイルは上書きしない
                        continue;
                    }
                }
                else if (type == 3) {
                    if (tempExtention[tempExtention.Length - 1] != "inputactions")
                    {
                        //更新可能な拡張子以外のファイルは上書きしない
                        continue;
                    }
                }

                try {
                    File.Replace(file.FullName, tempPath, null, true);
                }
                catch (Exception e) {
                    UnityEngine.Debug.Log("Failed to replace file [" + file.Name + "]");
                    throw e;
                }
            }
            else
            {
                file.MoveTo(tempPath);
            }
        }

        // サブディレクトリの移動（再帰呼び出し）
        foreach (DirectoryInfo subDirectory in sourceDirectory.GetDirectories())
        {
            if (subDirectory.Name == "Packages")
            {
                continue;
            }
            string tempPath = Path.Combine(destination, subDirectory.Name);
            MoveDirectory(subDirectory.FullName, tempPath, type);
        }
    }
}
