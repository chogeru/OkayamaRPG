using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Event.Map
{
    /// <summary>
    /// [マップ]-[指定位置の情報取得]
    /// </summary>
    public class MapGetPoint : AbstractEventCommandProcessor
    {
        private static readonly Vector2Int invalidPos = new(int.MinValue, int.MinValue);

        protected override void Process(string eventID, EventDataModel.EventCommand command)
        {
            //0:変数リスト
            //1：[地形タグ][イベントID][タイルID][リージョンID]
            //2：[レイヤー1][レイヤー2][レイヤー3][レイヤー4]
            //3：場所
            //4：x
            //5：y

            string value = null;

            Vector2Int pos = GetPos(eventID, command.parameters);
            if (pos != invalidPos)
            {
                switch (command.parameters[1])
                {
                    // 地形タグ
                    case "0":
                    {
                            var layerType = int.Parse(command.parameters[2]) switch
                            {
                                0 => MapDataModel.Layer.LayerType.A,
                                1 => MapDataModel.Layer.LayerType.B,
                                2 => MapDataModel.Layer.LayerType.C,
                                3 => MapDataModel.Layer.LayerType.D,
                                _ => throw new System.ArgumentOutOfRangeException()
                            };

                            var layerTilemap = MapManager.GetTileMapForRuntime(layerType);

                            if (pos.y > 0)
                            {
                                pos.y = -pos.y;
                            }

                            var tileDataModel = layerTilemap.GetTile<TileDataModel>(new Vector3Int(pos.x, pos.y, 0));
                            value = tileDataModel?.terrainTagValue.ToString();
                            break;
                    }

                    //イベントID
                    case "1":
                    {
                        var currentMapId = MapManager.CurrentMapDataModel.id;
                        var currentMapEventMapDataModels = new EventManagementService().LoadEventMap().
                            Where(eventMapDataModel => eventMapDataModel.mapId == currentMapId);
                        foreach (var eventMapDataModel in currentMapEventMapDataModels)
                        {
                            if (eventMapDataModel.x == pos.x &&
                                System.Math.Abs(eventMapDataModel.y) == System.Math.Abs(pos.y))
                            {
                                value = eventMapDataModel.SerialNumberString;
                                break;
                            }
                        }

                        break;
                    }

                    //タイルID
                    case "2":
                    {
                        var layerType = int.Parse(command.parameters[2]) switch
                        {
                            0 => MapDataModel.Layer.LayerType.A,
                            1 => MapDataModel.Layer.LayerType.B,
                            2 => MapDataModel.Layer.LayerType.C,
                            3 => MapDataModel.Layer.LayerType.D,
                            _ => throw new System.ArgumentOutOfRangeException()
                        };

                        var layerTilemap = MapManager.GetTileMapForRuntime(layerType);

                        if (pos.y > 0)
                        {
                            pos.y = -pos.y;
                        }

                        var tileDataModel = layerTilemap.GetTile<TileDataModel>(new Vector3Int(pos.x, pos.y, 0));
                        value = tileDataModel?.SerialNumberString;
                        break;
                    }

                    //リージョンID
                    case "3":
                    {
                        var regionLayer = RegionLayerForRuntime;
                        value = regionLayer.GetTileDataModelWithYPosCorrection(pos)?.regionId.ToString();
                        value ??= "0";
                        break;
                    }
                }
            }

            DebugUtil.Log($"value={value}");

            // 値を変数に代入。
            if (value != null)
            {
                var variableIndex = GetVariableIndex(command.parameters[0]);
                if (variableIndex >= 0)
                {
                    DataManager.Self().GetRuntimeSaveDataModel().variables.data[variableIndex] = value;
                }
            }

            SendBackToLauncher.Invoke();
        }

        private static MapDataModel.Layer RegionLayerForRuntime =>
            MapManager.CurrentMapDataModel.MapPrefabManagerForRuntime.layers[(int)MapDataModel.Layer.LayerType.Region];

        private static Vector2Int GetPos(string eventId, List<string> parameters)
        {
            return parameters[3] switch
            {
                // 直接指定 (即値)。
                "0" => new Vector2Int(int.Parse(parameters[4]), int.Parse(parameters[5])),
                // 変数で指定。
                "1" => GetPositionByVariables(parameters[4], parameters[5]),
                // キャラクターで指定。
                "2" => new Commons.TargetCharacter(
                    parameters.Count >= 7 ? (parameters[6] == "-1" ? eventId : parameters[6]) : (parameters[4] == "-1" ? eventId : parameters[4])/*過去の間違った値格納場所*/, eventId).
                        GetTilePositionOnTile(),
                // 他。
                _ => invalidPos
            };
        }

        private static Vector2Int GetPositionByVariables(string xVariableId, string yVariableId)
        {
            int xVariableIndex = GetVariableIndex(xVariableId);
            int yVariableIndex = GetVariableIndex(yVariableId);

            if (xVariableIndex < 0 || !int.TryParse(GetVariableValue(xVariableIndex), out int x) ||
                yVariableIndex < 0 || !int.TryParse(GetVariableValue(yVariableIndex), out int y))
            {
                return invalidPos;
            }

            return new Vector2Int(x, y);
        }

        private static int GetVariableIndex(string variableId)
        {
            return new CoreSystem.Service.DatabaseManagement.DatabaseManagementService().LoadFlags().
                variables.FindIndex(v => v.id == variableId);
        }

        private static string GetVariableValue(int variableIndex)
        {
            return DataManager.Self().GetRuntimeSaveDataModel().variables.data[variableIndex];
        }
    }
}