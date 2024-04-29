#if UNITY_EDITOR
using RPGMaker.Codebase.CoreSystem.Lib.Migration.ExecutionClasses;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Lib.Migration
{
    public static class MigrationCore
    {
        //----------------------------------------------------------------------------------------------------------------------------------

        // マイグレーション履歴ファイルへのパス
        private const string MigrationHistoryDir = "Assets/RPGMaker/Storage/Migration";
        private const string MigrationHistoryFile = MigrationHistoryDir + "/MigrationHistoryCore.json";

        // Storageのマイグレーションが必要な場合に利用するパス
        private static readonly string LocalVersionPath = Directory.GetCurrentDirectory() + "/Packages/jp.ggg.rpgmaker.unite/version.txt";
        private const string DEFAULTGAME_COMMON = "defaultgame_common_v";
        private static string VERSION = "1.0.0";

        //----------------------------------------------------------------------------------------------------------------------------------
        // 実行するマイグレーション処理クラス一覧
        // マイグレーション処理クラスが増えたらここに追加すること
        // このリスト順でマイグレーション処理が実行される
        private static readonly List<IExecutionClassCore> ExecutionClasses = new List<IExecutionClassCore>
        {
            new Migration_1_0_1_Class(),
            new Migration_1_0_2_Class(),
            new Migration_1_0_3_Class(),
            new Migration_1_0_4_Class(),
            new Migration_1_0_6_Class(),
            new Migration_1_0_7_Class(),
        };

        // マイグレーション履歴
        private static List<MigrationHistoryDataModel> _migrationHistoryDataModels;
        private static bool _isStorageUpdate;

        /**
         * マイグレーションを実行する
         */
        public static void Migrate() {
            if (!Auth.Auth.IsAuthenticated)
                return;

            try
            {
                // マイグレーション履歴ファイルが存在しない場合は新規作成
                if (!Directory.Exists(MigrationHistoryDir)) Directory.CreateDirectory(MigrationHistoryDir);

                if (!File.Exists(MigrationHistoryFile)) File.WriteAllText(MigrationHistoryFile, "[]");

                // マイグレーション履歴を取得
                _migrationHistoryDataModels = LoadMigrationHistoryDataModels();
            }
            catch (Exception)
            {
                // マイグレーション履歴を正常に取得できなかった場合はエラーを出力し処理を中断する
                return;
            }

            // マイグレーション対象の処理を判定・取得
            var loadExecutionClassesForThisTime = LoadExecutionClassesForThisTime();
            if (loadExecutionClassesForThisTime.Count == 0)
            {
                return;
            }

            // マイグレーション実行
            foreach (var executionClass in loadExecutionClassesForThisTime)
            {
                AssetDatabase.StartAssetEditing();

                try
                {
                    //Storageの一時解凍が必要なマイグレーションが含まれている場合には、ユーザーフォルダ下に一時的に展開する
                    if (!_isStorageUpdate)
                    {
                        _isStorageUpdate = executionClass.IsStorageUpdate();
                        if (_isStorageUpdate)
                        {
                            UncompressStorage();
                        }
                    }

                    //マイグレーション処理実行
                    executionClass.Execute();

                    //Storageのマイグレーション処理実行
                    if (executionClass.IsStorageUpdate())
                    {
                        StorageCopy(executionClass.ListStorageCopy());
                        StorageDelete(executionClass.ListStorageDelete());
                    }
                }
                catch (Exception)
                {
                    // マイグレーション処理に失敗、ロールバックする
                    executionClass.Rollback();
                    return;
                }

                try
                {
                    SaveExecutionHistory(executionClass);
                }
                catch (Exception)
                {
                    // マイグレーション履歴作成に失敗、この場合も処理をロールバックする（次回マイグレーションで不整合が起きるため）
                    executionClass.Rollback();
                    return;
                }

                //Storageの一時解凍が必要なマイグレーションが含まれている場合、マイグレーション処理終了時に、ユーザーフォルダ下に一時的に展開していたStorageを削除する
                if (_isStorageUpdate)
                {
                    RemoveStorage();
                    _isStorageUpdate = false;
                }

                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }
        }

        /**
         * マイグレーション処理を履歴に記録する
         */
        private static void SaveExecutionHistory(IExecutionClassCore executionClass) {
            _migrationHistoryDataModels.Add(
                new MigrationHistoryDataModel
                {
                    id = Guid.NewGuid().ToString(),
                    executionClassIdentifier = executionClass.GetIdentifier(),
                    executedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                }
            );
            File.WriteAllText(MigrationHistoryFile, JsonHelperForRepositoryCore.ToJsonArray(_migrationHistoryDataModels));
        }

        /**
         * 履歴から判断して、今回実行するべきマイグレーション処理一覧を取得する
         */
        private static List<IExecutionClassCore> LoadExecutionClassesForThisTime() {
            // 履歴がゼロの場合は全処理を返す
            if (_migrationHistoryDataModels.Count == 0) return ExecutionClasses;

            // 履歴の最後にある処理クラス名を基準にし、実行順を考慮する
            // （仮に、実行済の処理より手前の処理が履歴になかったとしてもそれは実行しないようにする）
            var lastExecutedExecutionClassName = _migrationHistoryDataModels.Last().executionClassIdentifier;

            var ret = new List<IExecutionClassCore>();
            var foundLastExecuted = false;
            foreach (var executionClass in ExecutionClasses)
                if (foundLastExecuted)
                    ret.Add(executionClass);
                else if (executionClass.GetIdentifier() == lastExecutedExecutionClassName) foundLastExecuted = true;

            return ret;
        }

        /**
         * マイグレーション履歴を取得
         */
        private static List<MigrationHistoryDataModel> LoadMigrationHistoryDataModels() {
            var fs = new FileStream(MigrationHistoryFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(fs, Encoding.GetEncoding("UTF-8"));
            var jsonString = sr.ReadToEnd();
            return JsonHelperForRepositoryCore.FromJsonArray<MigrationHistoryDataModel>(jsonString);
        }

        private static string GetFolderPath()
        {
            //DLL内でWindows、Macでのコンパイルスイッチは利用不可能なため、ファイルの有無をそれぞれチェックすることで、所定の場所に解凍する
            string folderPath = Application.persistentDataPath;

            string[] folderSplit = folderPath.Split("/");
            folderPath = "";

            //Windowsの場合には、以下のフォルダが存在するはず
            for (int i = 0; i < folderSplit.Length - 2; i++)
                folderPath += folderSplit[i] + "/";
            folderPath += ".RPGMaker";

            if (!Directory.Exists(folderPath))
            {
                //Macの場合には、以下のフォルダが存在するはず
                folderPath = "";
                for (int i = 0; i < folderSplit.Length - 4; i++)
                    folderPath += folderSplit[i] + "/";
                folderPath += ".RPGMaker";

                if (!Directory.Exists(folderPath))
                {
                    //存在しない場合には、マイグレーション処理を実行できない
                    throw new Exception("Package file could not be found.");
                }
            }

            folderPath += "/";
            return folderPath;
        }

        /**
         * マイグレーション用にStorageを一時的に解凍する
         */
        private static void UncompressStorage()
        {
            string folderPath = GetFolderPath();
            if (!Directory.Exists(folderPath + "Migration"))
            {
                Directory.CreateDirectory(folderPath + "Migration");
            }

            //バージョン情報を、現在のプロジェクトのバージョンとする
            VERSION = "1.0.0";

            if (File.Exists(LocalVersionPath))
            {
                using var fs = new FileStream(LocalVersionPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fs, Encoding.UTF8);
                VERSION = reader.ReadToEnd();
            }

            if (!File.Exists(folderPath + GetFileName(DEFAULTGAME_COMMON)))
            {
                throw new Exception("Package file could not be found.");
            }

            //指定されたフォルダ内の、Storage領域に、共通Storageを解凍する
            ZipFile.ExtractToDirectory(folderPath + GetFileName(DEFAULTGAME_COMMON), folderPath + "Migration", true);

            //言語ごとに異なるファイルについては、ファイル置換によるマイグレーションは不可で、個別処理が必要
        }

        private static void StorageCopy(List<string> list)
        {
            if (list == null) return;
            string folderPath = GetFolderPath() + "Migration/Storage/";
            string localPath = Directory.GetCurrentDirectory() + "/Assets/RPGMaker/Storage/";
            for (int i = 0; i < list.Count; i++)
            {
                File.Copy(folderPath + list[i], localPath + list[i], true);
            }
        }

        private static void StorageDelete(List<string> list)
        {
            if (list == null) return;
            string localPath = Directory.GetCurrentDirectory() + "/Assets/RPGMaker/Storage/";
            for (int i = 0; i < list.Count; i++)
            {
                File.Delete(localPath + list[i]);
            }
        }

        /**
         * マイグレーション用にStorageを一時的に解凍していたディレクトリを削除する
         */
        private static void RemoveStorage()
        {
            string folderPath = GetFolderPath();

            //Migrationディレクトリが残っている場合には削除する
            if (Directory.Exists(folderPath + "Migration"))
            {
                Directory.Delete(folderPath + "Migration", true);
            }
        }

        private static string GetFileName(string name)
        {
            return name + VERSION + ".zip";
        }
    }
}

#else

namespace RPGMaker.Codebase.CoreSystem.Lib.Migration
{
    public static class Migration
    {
        public static void Migrate() {
        }
    }
}

#endif