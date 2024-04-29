using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting
{
    [CreateAssetMenu(menuName = "MyScriptable/BattleTestScriptableObject")]
    public class BattleTestScriptableObject : ScriptableObject
    {
        public enum TestType
        {
            Map,
            Unit
        }

        // 個別指定の情報。
        public string       bgImageName1 = string.Empty;
        public string       bgImageName2 = string.Empty;
        public List<string> enemyIds     = new List<string>();

        [SerializeField] private List<Actor> mapActors = new List<Actor>();

        // マップ指定の情報。
        public string mapId    = string.Empty;
        public int    regionId = -1;
        public string troopId  = string.Empty;

        [SerializeField] private List<Actor> unitActors    = new List<Actor>();
        public                   bool        useEnemyChara = true;

        // 『マップ指定』フラグ(非『個別指定』フラグ)。
        public bool useMapRegionSetting = true;

        public List<Actor> actors => useMapRegionSetting ? mapActors : unitActors;

        // List<Actor>[]型はシリアライズできないので、このプロパティで代用する。
        public List<Actor>[] testTypedActors
        {
            get { return new[] {mapActors, unitActors}; }
            set
            {
                mapActors = value[(int) TestType.Map];
                unitActors = value[(int) TestType.Unit];
            }
        }

        [Serializable]
        public class Actor
        {
            [SerializeField] private List<string> equipIds = new List<string>();
            public                   string       id;
            public                   int          level;

            public string[] EquipIds => equipIds.ToArray();

            public static Actor CreateDefault(
                DatabaseManagementService databaseManagementService
            ) {
                var dataModels = databaseManagementService.LoadCharacterActor();
                return dataModels.Count > 0
                    ? new Actor {id = dataModels[0].uuId, level = dataModels[0].initialLevel}
                    : null;
            }

            public static bool IsCreatable(
                DatabaseManagementService databaseManagementService
            ) {
                return databaseManagementService.LoadCharacterActor().Count > 0;
            }

            public string GetEquipId(int index) {
                return index < equipIds.Count ? equipIds[index] : "";
            }

            public void SetEquipId(int index, string id) {
                while (index >= equipIds.Count)
                {
                    equipIds.Add("");
                }

                equipIds[index] = id;
            }
        }
    }
}