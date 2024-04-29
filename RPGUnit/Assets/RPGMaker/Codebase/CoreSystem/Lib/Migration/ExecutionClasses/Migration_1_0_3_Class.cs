using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Helper;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RPGMaker.Codebase.CoreSystem.Lib.Migration.ExecutionClasses
{
    /// <summary>
    /// Version1.0.3へのMigration用クラス
    /// </summary>
    internal class Migration_1_0_3_Class : IExecutionClassCore
    {
        public string GetIdentifier()
        {
            return "Migration103Class";
        }

        public void Execute()
        {
            Tile_Convert();
        }

        public void Rollback()
        {
        }

        public bool IsStorageUpdate()
        {
            return true;
        }

        public List<string> ListStorageCopy()
        {
            return new List<string>
            {
                "Map/TileImages/maptree007.png",
                "Map/TileImages/maptree008.png",
                "Map/TileImages/maptree009.png",
                "Map/TileImages/maptree015_a.png",
                "Map/TileImages/maptree015_b.png"
            };
        }

        public List<string> ListStorageDelete()
        {
            return null;
        }

        /// <summary>
        /// タイルのフォルダ構造修正
        /// タイル管理用jsonの生成
        /// </summary>
        private void Tile_Convert() {
#if UNITY_EDITOR
            string TileTableJsonPath = "Assets/RPGMaker/Storage/Map/JSON/tileTable.json";
            string TileAssetFolderPath = "Assets/RPGMaker/Storage/Map/TileAssets/";
            string SystemTileAssetFolderPath = "Assets/RPGMaker/Storage/System/Map/";

            // ディレクトリパス
            List<string> path = new List<string>()
            {
                "AutoTileA/",
                "AutoTileB/",
                "AutoTileC/",
                "LargeParts/",
                "Effect/",
                "NormalTile/",
            };

            var tiletable = new List<TileDataModelInfo>();

            for (int tileType = 0; tileType < path.Count; tileType++)
            {
                // 通常タイル全取得
                var tiles = Directory.GetFiles(TileAssetFolderPath + path[tileType], "*.asset", SearchOption.AllDirectories)
                    .Select(assetPath => UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<TileDataModel>(assetPath))
                    .Where(tileAsset => tileAsset != null)
                    .ToList()
                    .OrderBy(tile => tile.serialNumber).ToList();

                for (int i = 0; i < tiles.Count; i++)
                {
                    // タイル情報データ生成
                    var tileDataModelInfo = new TileDataModelInfo();
                    tileDataModelInfo.id = tiles[i].id;
                    tileDataModelInfo.listNumber = i + 1;
                    tileDataModelInfo.serialNumber = tiles[i].serialNumber;
                    tileDataModelInfo.largePartsDataModel = tiles[i].largePartsDataModel;
                    tileDataModelInfo.type = tiles[i].type;
                    tiletable.Add(tileDataModelInfo);
                }

                if (tileType + 1 == path.Count)
                {
                    // システムタイル
                    tiles = Directory.GetFiles(SystemTileAssetFolderPath, "*.asset", SearchOption.AllDirectories)
                                .Select(assetPath => UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<TileDataModel>(assetPath))
                                .Where(tileAsset => tileAsset != null)
                                .ToList()
                                .OrderBy(tile => tile.serialNumber).ToList();

                    for (int i = 0; i < tiles.Count; i++)
                    {
                        var tileDataModelInfo = new TileDataModelInfo();
                        tileDataModelInfo.id = tiles[i].id;
                        tileDataModelInfo.serialNumber = tiles[i].serialNumber;
                        tileDataModelInfo.largePartsDataModel = tiles[i].largePartsDataModel;
                        tileDataModelInfo.type = tiles[i].type;
                        tileDataModelInfo.regionId = tiles[i].regionId;
                        tiletable.Add(tileDataModelInfo);
                    }
                }
            }

            File.WriteAllText(TileTableJsonPath, JsonHelper.ToJsonArray(tiletable));
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh2();
#endif
        }
    }
}