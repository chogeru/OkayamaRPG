using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.WordDefinition;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement.Repository;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace RPGMaker.Codebase.CoreSystem.Lib.Migration.ExecutionClasses
{
    /// <summary>
    /// Version1.0.2へのMigration用クラス
    /// </summary>
    internal class Migration_1_0_2_Class : IExecutionClassCore
    {
        public string GetIdentifier()
        {
            return "Migration102Class";
        }

        public void Execute()
        {
            Migration_Word_Formation();
            Tile_Convert();
        }

        public void Rollback()
        {
            //説明文の変更のみのため、特にRollbackは不要
        }

        public bool IsStorageUpdate()
        {
            return true;
        }

        public List<string> ListStorageCopy()
        {
            return new List<string>
            {
                "Images/Enemy/004_enemy_Assassin_01.png",
                "Images/Enemy/005_enemy_Assassin_02.png",
                "Images/Enemy/006_enemy_Assassin_03.png",
                "Images/Enemy/023_enemy_Femaleghost_01.png",
                "Images/Enemy/024_enemy_Femaleghost_02.png",
                "Images/Enemy/025_enemy_Femaleghost_03.png",
                "Images/Enemy/026_enemy_Sandgolem_01.png",
                "Images/Enemy/027_enemy_Sandgolem_02.png",
                "Images/Enemy/028_enemy_Sandgolem_03.png",
                "Images/Enemy/030_enemy_Smallminotaur_02.png",
                "Images/Enemy/031_enemy_Smallminotaur_03.png",
                "Images/Enemy/032_enemy_Bonepirate_01.png",
                "Images/Enemy/033_enemy_Bonepirate_02.png",
                "Images/Enemy/034_enemy_Bonepirate_03.png",
                "Images/Enemy/041_enemy_Wraith_01.png",
                "Images/Enemy/042_enemy_Wraith_02.png",
                "Images/Enemy/043_enemy_Gnome.png",
                "Images/Enemy/044_enemy_Machinerybee.png",
                "Images/Enemy/045_enemy_Mimic.png",
                "Images/Enemy/046_enemy_Salamander.png",
                "Images/Enemy/047_enemy_Siren.png",
                "Images/Enemy/048_enemy_Stoneknight.png",
                "Images/Enemy/050_enemy_Undine.png",
                "Images/Enemy/051_enemy_Kraken.png",
                "Images/Enemy/052_enemy_Sandworm.png",
                "Images/Enemy/053_enemy_Gazer_01.png",
                "Images/Enemy/054_enemy_Gazer_02.png",
                "Images/Enemy/055_enemy_Gazer_03.png",
                "Images/Enemy/056_enemy_Snake_01.png",
                "Images/Enemy/057_enemy_Snake_02.png",
                "Images/Enemy/058_enemy_Succubus_01.png",
                "Images/Enemy/059_enemy_Succubus_02.png",
                "Images/Enemy/060_enemy_Ghost.png",
                "Images/Enemy/061_enemy_Spider.png",
                "Images/Enemy/062_enemy_Willowisp.png",
                "Images/Enemy/064_enemy_Chimera.png",
                "Images/Enemy/065_enemy_Behemoth.png",
                "Images/Enemy/066_enemy_Wyvern.png",
                "Images/Enemy/067_enemy_Treant_01.png",
                "Images/Enemy/068_enemy_Treant_02.png",
                "Images/Enemy/069_enemy_Hi_monster.png",
                "Images/Enemy/070_enemy_Ketos.png",
                "Images/Enemy/073_enemy_Lamia.png",
                "Images/Enemy/075_enemy_Thoughtform_boss.png",
                "Images/Enemy/079_enemy_Dragon_boss.png",
                "Images/Enemy/080_enemy_Evilgod_boss.png",
                "Images/Enemy/081_enemy_cleric_male.png",
                "Images/Enemy/082_enemy_fullarmor_male.png",
                "Images/Enemy/083_enemy_fullarmor_female.png",

                "Images/Pictures/charaupperbody_001_Actor.png",
                "Images/Pictures/charaupperbody_002_Actor.png",
                "Images/Pictures/charaupperbody_003_Actor.png",
                "Images/Pictures/charaupperbody_004_Actor.png",
                "Images/Pictures/charaupperbody_005_Actor.png",
                "Images/Pictures/charaupperbody_008_Actor.png",
                "Images/Pictures/charaupperbody_009_Actor.png",
                "Images/Pictures/charaupperbody_011_Actor.png",
                "Images/Pictures/charaupperbody_013_Actor.png",
                "Images/Pictures/charaupperbody_014_Actor.png",
                "Images/Pictures/charaupperbody_015_Actor.png",
                "Images/Pictures/charaupperbody_016_Actor.png",
                "Images/Pictures/charaupperbody_017_Actor.png",
                "Images/Pictures/charaupperbody_018_Actor.png",
                "Images/Pictures/charaupperbody_019_Actor.png",
                "Images/Pictures/charaupperbody_021_Actor.png",
                "Images/Pictures/charaupperbody_022_Actor.png",
                "Images/Pictures/charaupperbody_023_Actor.png",
                "Images/Pictures/charaupperbody_024_Actor.png",
                "Images/Pictures/charaupperbody_025_Evil.png",
                "Images/Pictures/charaupperbody_026_Evil.png",
                "Images/Pictures/charaupperbody_027_Evil.png",
                "Images/Pictures/charaupperbody_028_Evil.png",
                "Images/Pictures/charaupperbody_029_Evil.png",
                "Images/Pictures/charaupperbody_030_Evil.png",
                "Images/Pictures/charaupperbody_032_Evil.png",
                "Images/Pictures/charaupperbody_033_Demihuman.png",
                "Images/Pictures/charaupperbody_034_Demihuman.png",
                "Images/Pictures/charaupperbody_035_Demihuman.png",
                "Images/Pictures/charaupperbody_036_Demihuman.png",
                "Images/Pictures/charaupperbody_037_Demihuman.png",
                "Images/Pictures/charaupperbody_038_Demihuman.png",
                "Images/Pictures/charaupperbody_039_Demihuman.png",
                "Images/Pictures/charaupperbody_040_Demihuman.png",
                "Images/Pictures/charaupperbody_041_People.png",
                "Images/Pictures/charaupperbody_042_People.png",
                "Images/Pictures/charaupperbody_043_People.png",
                "Images/Pictures/charaupperbody_045_People.png",
                "Images/Pictures/charaupperbody_046_People.png",
                "Images/Pictures/charaupperbody_047_People.png",
                "Images/Pictures/charaupperbody_048_People.png",
                "Images/Pictures/charaupperbody_049_People.png",
                "Images/Pictures/charaupperbody_050_People.png",
                "Images/Pictures/charaupperbody_052_People.png",
                "Images/Pictures/charaupperbody_053_People.png",
                "Images/Pictures/charaupperbody_054_People.png",
                "Images/Pictures/charaupperbody_055_People.png",
                "Images/Pictures/charaupperbody_056_People.png",
                "Images/Pictures/charaupperbody_057_People.png",
                "Images/Pictures/charaupperbody_058_People.png",
                "Images/Pictures/charaupperbody_059_People.png",
                "Images/Pictures/charaupperbody_060_People.png",
                "Images/Pictures/charaupperbody_061_People.png",
                "Images/Pictures/charaupperbody_062_People.png",
                "Images/Pictures/charaupperbody_063_People.png",
                "Images/Pictures/charaupperbody_064_People.png",
                "Images/Pictures/charaupperbody_065_People.png",
                "Images/Pictures/charaupperbody_066_People.png",
                "Images/Pictures/charaupperbody_068_People.png",
                "Images/Pictures/charaupperbody_069_People.png",
                "Images/Pictures/charaupperbody_070_People.png",

                "Images/Faces/charaface_001_Actor.png",
                "Images/Faces/charaface_002_Actor.png",
                "Images/Faces/charaface_003_Actor.png",
                "Images/Faces/charaface_004_Actor.png",
                "Images/Faces/charaface_008_Actor.png",
                "Images/Faces/charaface_009_Actor.png",
                "Images/Faces/charaface_011_Actor.png",
                "Images/Faces/charaface_013_Actor.png",
                "Images/Faces/charaface_014_Actor.png",
                "Images/Faces/charaface_015_Actor.png",
                "Images/Faces/charaface_016_Actor.png",
                "Images/Faces/charaface_017_Actor.png",
                "Images/Faces/charaface_022_Actor.png",
                "Images/Faces/charaface_023_Actor.png",
                "Images/Faces/charaface_024_Actor.png",
                "Images/Faces/charaface_026_Evil.png",
                "Images/Faces/charaface_027_Evil.png",
                "Images/Faces/charaface_028_Evil.png",
                "Images/Faces/charaface_029_Evil.png",
                "Images/Faces/charaface_030_Evil.png",
                "Images/Faces/charaface_032_Evil.png",
                "Images/Faces/charaface_033_Demihuman.png",
                "Images/Faces/charaface_034_Demihuman.png",
                "Images/Faces/charaface_035_Demihuman.png",
                "Images/Faces/charaface_036_Demihuman.png",
                "Images/Faces/charaface_037_Demihuman.png",
                "Images/Faces/charaface_038_Demihuman.png",
                "Images/Faces/charaface_040_Demihuman.png",
                "Images/Faces/charaface_041_People.png",
                "Images/Faces/charaface_043_People.png",
                "Images/Faces/charaface_045_People.png",
                "Images/Faces/charaface_046_People.png",
                "Images/Faces/charaface_047_People.png",
                "Images/Faces/charaface_048_People.png",
                "Images/Faces/charaface_049_People.png",
                "Images/Faces/charaface_050_People.png",
                "Images/Faces/charaface_052_People.png",
                "Images/Faces/charaface_053_People.png",
                "Images/Faces/charaface_054_People.png",
                "Images/Faces/charaface_055_People.png",
                "Images/Faces/charaface_056_People.png",
                "Images/Faces/charaface_057_People.png",
                "Images/Faces/charaface_058_People.png",
                "Images/Faces/charaface_059_People.png",
                "Images/Faces/charaface_060_People.png",
                "Images/Faces/charaface_061_People.png",
                "Images/Faces/charaface_062_People.png",
                "Images/Faces/charaface_063_People.png",
                "Images/Faces/charaface_065_People.png",
                "Images/Faces/charaface_066_People.png",
                "Images/Faces/charaface_067_People.png",
                "Images/Faces/charaface_068_People.png",
                "Images/Faces/charaface_069_People.png",
                "Images/Faces/charaface_070_People.png",

                "Images/Characters/Vehicle_001.png",
                "Images/Characters/Vehicle_002.png",
                "Images/Characters/Vehicle_003.png",
                "Images/Characters/Vehicle_004.png",
                "Images/Characters/Vehicle_005.png",
                "Images/Characters/Vehicle_006.png",
                "Images/Characters/Vehicle_007.png",
                "Images/Characters/Vehicle_008.png",
                "Images/Characters/Vehicle_009.png",
                "Images/Characters/Vehicle_010.png",
                "Images/Characters/Vehicle_011.png",
                "Images/Characters/Vehicle_012.png",
                "Images/Characters/Vehicle_017.png",
                "Images/Characters/Vehicle_018.png",
                "Images/Characters/Vehicle_019.png",
                "Images/Characters/Vehicle_020.png",
                "Images/Characters/Vehicle_021.png",
                "Images/Characters/Vehicle_022.png",
                "Images/Characters/Vehicle_023.png",
                "Images/Characters/Vehicle_029.png",
                "Images/Characters/Vehicle_030.png",
                "Images/Characters/Vehicle_031.png",
                "Images/Characters/Vehicle_032.png",
                "System/Map/loading.png",


                "Map/TileAssets/AutoTileB/1e5014ae-7a7c-4b3c-bd3c-e92b3c00be89/defaultSprite.png",
                "Map/TileAssets/AutoTileB/1e5014ae-7a7c-4b3c-bd3c-e92b3c00be89/shape0.png",
                "Map/TileAssets/AutoTileB/1e5014ae-7a7c-4b3c-bd3c-e92b3c00be89/shape1.png",
                "Map/TileAssets/AutoTileB/1e5014ae-7a7c-4b3c-bd3c-e92b3c00be89/shape2.png",
                "Map/TileAssets/AutoTileB/1e5014ae-7a7c-4b3c-bd3c-e92b3c00be89/shape3.png",
                "Map/TileAssets/AutoTileB/1e5014ae-7a7c-4b3c-bd3c-e92b3c00be89/shape4.png",
                "Map/TileAssets/AutoTileB/1e5014ae-7a7c-4b3c-bd3c-e92b3c00be89/shape5.png",
                "Map/TileAssets/AutoTileB/1e5014ae-7a7c-4b3c-bd3c-e92b3c00be89/shape6.png",
                "Map/TileAssets/AutoTileB/1e5014ae-7a7c-4b3c-bd3c-e92b3c00be89/shape7.png",
                "Map/TileAssets/AutoTileB/1e5014ae-7a7c-4b3c-bd3c-e92b3c00be89/shape8.png",
                "Map/TileAssets/AutoTileB/1e5014ae-7a7c-4b3c-bd3c-e92b3c00be89/shape9.png",
                "Map/TileAssets/AutoTileB/1e5014ae-7a7c-4b3c-bd3c-e92b3c00be89/shape10.png",
                "Map/TileAssets/AutoTileB/1e5014ae-7a7c-4b3c-bd3c-e92b3c00be89/shape11.png",
                "Map/TileAssets/AutoTileB/1e5014ae-7a7c-4b3c-bd3c-e92b3c00be89/shape12.png",
                "Map/TileAssets/AutoTileB/1e5014ae-7a7c-4b3c-bd3c-e92b3c00be89/shape13.png",
                "Map/TileAssets/AutoTileB/1e5014ae-7a7c-4b3c-bd3c-e92b3c00be89/shape14.png",
                "Map/TileAssets/AutoTileB/1e5014ae-7a7c-4b3c-bd3c-e92b3c00be89/shape15.png",

                "Map/TileAssets/AutoTileB/628e6f23-9cc6-4622-b620-2be8504f0871/defaultSprite.png",
                "Map/TileAssets/AutoTileB/628e6f23-9cc6-4622-b620-2be8504f0871/shape0.png",
                "Map/TileAssets/AutoTileB/628e6f23-9cc6-4622-b620-2be8504f0871/shape1.png",
                "Map/TileAssets/AutoTileB/628e6f23-9cc6-4622-b620-2be8504f0871/shape2.png",
                "Map/TileAssets/AutoTileB/628e6f23-9cc6-4622-b620-2be8504f0871/shape3.png",
                "Map/TileAssets/AutoTileB/628e6f23-9cc6-4622-b620-2be8504f0871/shape4.png",
                "Map/TileAssets/AutoTileB/628e6f23-9cc6-4622-b620-2be8504f0871/shape5.png",
                "Map/TileAssets/AutoTileB/628e6f23-9cc6-4622-b620-2be8504f0871/shape6.png",
                "Map/TileAssets/AutoTileB/628e6f23-9cc6-4622-b620-2be8504f0871/shape7.png",
                "Map/TileAssets/AutoTileB/628e6f23-9cc6-4622-b620-2be8504f0871/shape8.png",
                "Map/TileAssets/AutoTileB/628e6f23-9cc6-4622-b620-2be8504f0871/shape9.png",
                "Map/TileAssets/AutoTileB/628e6f23-9cc6-4622-b620-2be8504f0871/shape10.png",
                "Map/TileAssets/AutoTileB/628e6f23-9cc6-4622-b620-2be8504f0871/shape11.png",
                "Map/TileAssets/AutoTileB/628e6f23-9cc6-4622-b620-2be8504f0871/shape12.png",
                "Map/TileAssets/AutoTileB/628e6f23-9cc6-4622-b620-2be8504f0871/shape13.png",
                "Map/TileAssets/AutoTileB/628e6f23-9cc6-4622-b620-2be8504f0871/shape14.png",
                "Map/TileAssets/AutoTileB/628e6f23-9cc6-4622-b620-2be8504f0871/shape15.png"
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
        private void Tile_Convert()
        {
#if UNITY_EDITOR
            string TileTableJsonPath         = "Assets/RPGMaker/Storage/Map/JSON/tileTable.json";
            string TileAssetFolderPath       = "Assets/RPGMaker/Storage/Map/TileAssets/";
            string SystemTileAssetFolderPath = "Assets/RPGMaker/Storage/System/Map/";

            // 移動先ディレクトリパス
            List<string> path = new List<string>()
            {
                "AutoTileA/",
                "AutoTileB/",
                "AutoTileC/",
                "LargeParts/",
                "Effect/",
                "NormalTile/",
            };
            for (int i = 0; i < path.Count; i++)
                Directory.CreateDirectory(TileAssetFolderPath + path[i]);

            var tiletable = new List<TileDataModelInfo>();

            // 通常タイル全取得
            var tiles = Directory.GetFiles(TileAssetFolderPath, "*.asset", SearchOption.AllDirectories)
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

                string destPath = TileRepository.GetAssetPath(tileDataModelInfo, true);
                if (destPath == "")
                    continue;

                if (File.Exists(TileAssetFolderPath + tiles[i].id + ".asset"))
                    File.Move(TileAssetFolderPath + tiles[i].id + ".asset", destPath + tiles[i].id + ".asset");
                if (File.Exists(TileAssetFolderPath + tiles[i].id + ".asset.meta"))
                    File.Move(TileAssetFolderPath + tiles[i].id + ".asset.meta", destPath + tiles[i].id + ".asset.meta");
                if (File.Exists(TileAssetFolderPath + tiles[i].id + ".meta"))
                    File.Move(TileAssetFolderPath + tiles[i].id + ".meta", destPath + tiles[i].id + ".meta");
                if (Directory.Exists(TileAssetFolderPath + tiles[i].id))
                    Directory.Move(TileAssetFolderPath + tiles[i].id, destPath + tiles[i].id);
            }

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

            File.WriteAllText(TileTableJsonPath, JsonHelper.ToJsonArray(tiletable));
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh2();
#endif
        }

        private void Migration_Word_Formation() 
        {
#if UNITY_EDITOR
            // 現在の言語設定
            var assembly2 = typeof(EditorWindow).Assembly;
            var localizationDatabaseType2 = assembly2.GetType("UnityEditor.LocalizationDatabase");
            var currentEditorLanguageProperty2 = localizationDatabaseType2.GetProperty("currentEditorLanguage");
            var lang2 = (SystemLanguage) currentEditorLanguageProperty2.GetValue(null);

            //英語の場合のみ処理
            if (lang2 == SystemLanguage.English)
            {
                WordDefinitionRepository repository = new WordDefinitionRepository();
                WordDefinitionDataModel dataModel = repository.Load();
                dataModel.commands.GetData("Sort").value = "Formation";
                repository.Save(dataModel);
            }
#endif
        }
    }
}