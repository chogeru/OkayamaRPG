using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventCommon;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.State;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement.Repository;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace RPGMaker.Codebase.CoreSystem.Lib.Migration.ExecutionClasses
{
    /// <summary>
    /// Version1.0.1へのMigration用クラス
    /// </summary>
    internal class Migration_1_0_1_Class : IExecutionClassCore
    {
        public string GetIdentifier()
        {
            return "Migration101Class";
        }

        public void Execute()
        {
            //7156 中国語のStorageの場合に、説明文を中国語に変換
            //7172 英語のStorageの場合に、睡眠の継続文を変更
            Migration_7156();

            Migration_EventCommon_Convert();
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
                "Images/Characters/002_Actor_fielddamage_8.png",
                "Images/Characters/002_Actor_walk_down_6.png",
                "Images/Characters/002_Actor_walk_left_6.png",
                "Images/Characters/002_Actor_walk_right_6.png",
                "Images/Characters/002_Actor_walk_up_6.png",

                "Images/Characters/022_Actor_fielddamage_8.png",
                "Images/Characters/022_Actor_walk_down_6.png",
                "Images/Characters/022_Actor_walk_left_6.png",
                "Images/Characters/022_Actor_walk_right_6.png",
                "Images/Characters/022_Actor_walk_up_6.png",

                "Images/Characters/033_Demihuman_fielddamage_8.png",
                "Images/Characters/033_Demihuman_walk_down_6.png",
                "Images/Characters/033_Demihuman_walk_left_6.png",
                "Images/Characters/033_Demihuman_walk_right_6.png",
                "Images/Characters/033_Demihuman_walk_up_6.png",

                "Images/Characters/049_People_fielddamage_8.png",
                "Images/Characters/049_People_walk_down_6.png",
                "Images/Characters/049_People_walk_left_6.png",
                "Images/Characters/049_People_walk_right_6.png",
                "Images/Characters/049_People_walk_up_6.png",

                "Images/Characters/050_People_fielddamage_8.png",
                "Images/Characters/050_People_walk_down_6.png",
                "Images/Characters/050_People_walk_left_6.png",
                "Images/Characters/050_People_walk_right_6.png",
                "Images/Characters/050_People_walk_up_6.png",

                "Images/Characters/067_People_fielddamage_8.png",
                "Images/Characters/067_People_walk_down_6.png",
                "Images/Characters/067_People_walk_left_6.png",
                "Images/Characters/067_People_walk_right_6.png",
                "Images/Characters/067_People_walk_up_6.png",
               
                "Images/Enemy/004_enemy_Assassin_01.png",
                "Images/Enemy/005_enemy_Assassin_02.png",
                "Images/Enemy/006_enemy_Assassin_03.png",
                "Images/Enemy/013_enemy_Werewolf_01.png",
                "Images/Enemy/014_enemy_Werewolf_02.png",
                "Images/Enemy/015_enemy_Werewolf_03.png",
                "Images/Enemy/081_enemy_cleric_male.png",

                "Images/Faces/charaface_001_Actor.png",
                "Images/Faces/charaface_002_Actor.png",
                "Images/Faces/charaface_003_Actor.png",
                "Images/Faces/charaface_006_Actor.png",
                "Images/Faces/charaface_007_Actor.png",
                "Images/Faces/charaface_008_Actor.png",
                "Images/Faces/charaface_009_Actor.png",
                "Images/Faces/charaface_010_Actor.png",
                "Images/Faces/charaface_011_Actor.png",
                "Images/Faces/charaface_012_Actor.png",
                "Images/Faces/charaface_017_Actor.png",
                "Images/Faces/charaface_018_Actor.png",
                "Images/Faces/charaface_019_Actor.png",
                "Images/Faces/charaface_022_Actor.png",
                "Images/Faces/charaface_024_Actor.png",
                "Images/Faces/charaface_026_Evil.png",
                "Images/Faces/charaface_033_Demihuman.png",
                "Images/Faces/charaface_040_Demihuman.png",
                "Images/Faces/charaface_049_People.png",
                "Images/Faces/charaface_050_People.png",
                "Images/Faces/charaface_057_People.png",
                "Images/Faces/charaface_067_People.png",

                "Images/Pictures/charaupperbody_001_Actor.png",
                "Images/Pictures/charaupperbody_002_Actor.png",
                "Images/Pictures/charaupperbody_003_Actor.png",
                "Images/Pictures/charaupperbody_006_Actor.png",
                "Images/Pictures/charaupperbody_007_Actor.png",
                "Images/Pictures/charaupperbody_008_Actor.png",
                "Images/Pictures/charaupperbody_009_Actor.png",
                "Images/Pictures/charaupperbody_010_Actor.png",
                "Images/Pictures/charaupperbody_011_Actor.png",
                "Images/Pictures/charaupperbody_012_Actor.png",
                "Images/Pictures/charaupperbody_017_Actor.png",
                "Images/Pictures/charaupperbody_018_Actor.png",
                "Images/Pictures/charaupperbody_019_Actor.png",
                "Images/Pictures/charaupperbody_022_Actor.png",
                "Images/Pictures/charaupperbody_024_Actor.png",
                "Images/Pictures/charaupperbody_026_Evil.png",
                "Images/Pictures/charaupperbody_033_Demihuman.png",
                "Images/Pictures/charaupperbody_040_Demihuman.png",
                "Images/Pictures/charaupperbody_049_People.png",
                "Images/Pictures/charaupperbody_050_People.png",
                "Images/Pictures/charaupperbody_057_People.png",
                "Images/Pictures/charaupperbody_067_People.png",

                "Images/SV_Actors/002_Actor_cast_delay_6.png",
                "Images/SV_Actors/002_Actor_critical_health_6.png",
                "Images/SV_Actors/002_Actor_damage_6.png",
                "Images/SV_Actors/002_Actor_dead_6.png",
                "Images/SV_Actors/002_Actor_escape_8.png",
                "Images/SV_Actors/002_Actor_evasion_8.png",
                "Images/SV_Actors/002_Actor_general_skill_8.png",
                "Images/SV_Actors/002_Actor_guard_6.png",
                "Images/SV_Actors/002_Actor_items_8.png",
                "Images/SV_Actors/002_Actor_magic_8.png",
                "Images/SV_Actors/002_Actor_move_forward_6.png",
                "Images/SV_Actors/002_Actor_projectile_8.png",
                "Images/SV_Actors/002_Actor_sleep_6.png",
                "Images/SV_Actors/002_Actor_standard_delay_6.png",
                "Images/SV_Actors/002_Actor_status_ailment_6.png",
                "Images/SV_Actors/002_Actor_swing_8.png",
                "Images/SV_Actors/002_Actor_thrust_8.png",
                "Images/SV_Actors/002_Actor_victory_8.png",

                "Images/SV_Actors/022_Actor_cast_delay_6.png",
                "Images/SV_Actors/022_Actor_critical_health_6.png",
                "Images/SV_Actors/022_Actor_damage_6.png",
                "Images/SV_Actors/022_Actor_dead_6.png",
                "Images/SV_Actors/022_Actor_escape_8.png",
                "Images/SV_Actors/022_Actor_evasion_8.png",
                "Images/SV_Actors/022_Actor_general_skill_8.png",
                "Images/SV_Actors/022_Actor_guard_6.png",
                "Images/SV_Actors/022_Actor_items_8.png",
                "Images/SV_Actors/022_Actor_magic_8.png",
                "Images/SV_Actors/022_Actor_move_forward_6.png",
                "Images/SV_Actors/022_Actor_projectile_8.png",
                "Images/SV_Actors/022_Actor_sleep_6.png",
                "Images/SV_Actors/022_Actor_standard_delay_6.png",
                "Images/SV_Actors/022_Actor_status_ailment_6.png",
                "Images/SV_Actors/022_Actor_swing_8.png",
                "Images/SV_Actors/022_Actor_thrust_8.png",
                "Images/SV_Actors/022_Actor_victory_8.png",

                "Map/TileImages/Outside_A4_026.png",
                "Map/TileImages/Outside_A4_027.png",
            };
        }

        public List<string> ListStorageDelete()
        {
            return null;
        }

        /// <summary>
        /// 7156 中国語のStorageの場合に、説明文を中国語に変換
        /// </summary>
        private void Migration_7156 ()
        {
#if UNITY_EDITOR
            // 現在の言語設定
            var assembly2 = typeof(EditorWindow).Assembly;
            var localizationDatabaseType2 = assembly2.GetType("UnityEditor.LocalizationDatabase");
            var currentEditorLanguageProperty2 = localizationDatabaseType2.GetProperty("currentEditorLanguage");
            var lang2 = (SystemLanguage)currentEditorLanguageProperty2.GetValue(null);

            // 中国語の場合のみ処理
            if (lang2 == SystemLanguage.Chinese || lang2 == SystemLanguage.ChineseSimplified || lang2 == SystemLanguage.ChineseTraditional)
            {
                StateRepository repository = new StateRepository();
                List<StateDataModel> DataModels = repository.Load();

                for (int i = 0; i < DataModels.Count; i++)
                {
                    if (DataModels[i].id == "8fd93d41-fb58-401b-8d6b-f7d5396d3fec")
                    {
                        DataModels[i].note = "状态1会在HP0的时候被附加。";
                        break;
                    }
                }
                repository.Save(DataModels);
            }
            // 中国語でも日本語でもない場合（英語）
            else if (lang2 != SystemLanguage.Japanese)
            {
                StateRepository repository = new StateRepository();
                List<StateDataModel> DataModels = repository.Load();

                for (int i = 0; i < DataModels.Count; i++)
                {
                    if (DataModels[i].id == "0219aa38-7a8e-446f-8a34-901775bea0ab")
                    {
                        DataModels[i].message3 = "%1 is sleeping.";
                        break;
                    }
                }
                repository.Save(DataModels);
            }
#endif
        }

        /// <summary>
        /// 下記のイベントでマップがない場合のコモンイベントで新規作成時にエラーになるため、途中で作られたデータが正しくなくなる
        /// そのために、データを再度作り直すようのメソッド
        /// [場所移動]eventCode = 201
        /// [乗り物位置設定]eventCode = 202
        /// </summary>
        private void Migration_EventCommon_Convert()
        {
#if UNITY_EDITOR
            EventCommonRepository repository = new EventCommonRepository();
            EventRepository evntRepository = new EventRepository();
            List<EventCommonDataModel> DataModels = repository.Load();

            for (int i = 0; i < DataModels.Count; i++)
            {
                var eventData = evntRepository.LoadEventById(DataModels[i].eventId);
                var flg = false;
                for (int j = 0; j < eventData.eventCommands.Count; j++)
                {
                    if (eventData.eventCommands[j].code == 201)
                    {
                        if (eventData.eventCommands[j].parameters.Count != 8)
                        {
                            eventData.eventCommands[j].parameters = new List<string>
                            {
                                "0",
                                "",//マップが存在しない
                                "-1",//マップが存在しない
                                "0",
                                "0",
                                "0",
                                "0",
                                "-1"//マップが存在しない
                            };
                            flg = true;
                        }
                    }

                    if (eventData.eventCommands[j].code == 202)
                    {
                        if (eventData.eventCommands[j].parameters.Count != 5)
                        {
                            eventData.eventCommands[j].parameters = new List<string>
                            {
                                "1",
                                "0",
                                "-1", //マップが存在しない
                                "0",
                                "0"
                            };

                            flg = true;
                        }
                    }
                }

                if (flg)
                {
                    evntRepository.Save(eventData);
                }
            }
#endif

        }
    }
}