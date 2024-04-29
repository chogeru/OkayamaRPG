using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RPGMaker.Codebase.CoreSystem.Service.MapManagement
{
    public class MapRenderingOrderManager
    {
        public static void SetLayerRendererSortingLayer(
            GameObject layerGameObject, MapDataModel.Layer.LayerType layerType)
        {
            SetLayerRendererSortingLayer(layerGameObject, GetMapLayerSortingLayerId(layerType));
        }

        public static void SetLayerRendererSortingLayer(GameObject layerGameObject, int sortingLayeId)
        {
            var tilemapRenderer = layerGameObject.GetComponent<TilemapRenderer>();
            if (tilemapRenderer != null)
            {
                tilemapRenderer.sortingLayerID = sortingLayeId;
                return;
            }

            // 遠景などTilemapではなくSpriteの場合があるので、SpriteRendererにも対応。
            var spriteRenderer = layerGameObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingLayerID = sortingLayeId;
                return;
            }

            DebugUtil.LogWarning($"ソーティングレイヤーが設定できませんでした ({layerGameObject}, {sortingLayeId})。");
        }

        private static int GetMapLayerSortingLayerId(MapDataModel.Layer.LayerType layerType)
        {
            return UnityUtil.SortingLayerManager.GetId(
                layerType switch
                {
                    MapDataModel.Layer.LayerType.DistantView => UnityUtil.SortingLayerManager.SortingLayerIndex.MapLayerDistantView,
                    MapDataModel.Layer.LayerType.Background => UnityUtil.SortingLayerManager.SortingLayerIndex.MapLayerBackground,
                    MapDataModel.Layer.LayerType.BackgroundCollision => UnityUtil.SortingLayerManager.SortingLayerIndex.MapLayerBackgroundCollision,
                    MapDataModel.Layer.LayerType.A => UnityUtil.SortingLayerManager.SortingLayerIndex.MapLayerA,
                    MapDataModel.Layer.LayerType.A_Effect => UnityUtil.SortingLayerManager.SortingLayerIndex.MapLayerA_Effect,
                    MapDataModel.Layer.LayerType.B => UnityUtil.SortingLayerManager.SortingLayerIndex.MapLayerB,
                    MapDataModel.Layer.LayerType.B_Effect => UnityUtil.SortingLayerManager.SortingLayerIndex.MapLayerB_Effect,
                    MapDataModel.Layer.LayerType.Shadow => UnityUtil.SortingLayerManager.SortingLayerIndex.MapLayerShadow,
                    MapDataModel.Layer.LayerType.C => UnityUtil.SortingLayerManager.SortingLayerIndex.MapLayerC,
                    MapDataModel.Layer.LayerType.C_Effect => UnityUtil.SortingLayerManager.SortingLayerIndex.MapLayerC_Effect,
                    MapDataModel.Layer.LayerType.D => UnityUtil.SortingLayerManager.SortingLayerIndex.MapLayerD,
                    MapDataModel.Layer.LayerType.D_Effect => UnityUtil.SortingLayerManager.SortingLayerIndex.MapLayerD_Effect,
                    MapDataModel.Layer.LayerType.ForRoute => UnityUtil.SortingLayerManager.SortingLayerIndex.MapLayerForRoute,
                    MapDataModel.Layer.LayerType.Region => UnityUtil.SortingLayerManager.SortingLayerIndex.MapLayerRegion,
                    _ => throw new System.NotImplementedException(),
                });
        }

        public static int GetMapUpperLayerSortingLayerId(MapDataModel.Layer.LayerType layerType)
        {
            return UnityUtil.SortingLayerManager.GetId(
                layerType switch
                {
                    MapDataModel.Layer.LayerType.A => UnityUtil.SortingLayerManager.SortingLayerIndex.Runtime_MapLayerA_Upper,
                    MapDataModel.Layer.LayerType.B => UnityUtil.SortingLayerManager.SortingLayerIndex.Runtime_MapLayerB_Upper,
                    MapDataModel.Layer.LayerType.C => UnityUtil.SortingLayerManager.SortingLayerIndex.Runtime_MapLayerC_Upper,
                    MapDataModel.Layer.LayerType.D => UnityUtil.SortingLayerManager.SortingLayerIndex.Runtime_MapLayerD_Upper,
                    MapDataModel.Layer.LayerType.DistantView => throw new System.NotImplementedException(),
                    MapDataModel.Layer.LayerType.Background => throw new System.NotImplementedException(),
                    MapDataModel.Layer.LayerType.BackgroundCollision => throw new System.NotImplementedException(),
                    MapDataModel.Layer.LayerType.A_Effect => throw new System.NotImplementedException(),
                    MapDataModel.Layer.LayerType.B_Effect => throw new System.NotImplementedException(),
                    MapDataModel.Layer.LayerType.Shadow => throw new System.NotImplementedException(),
                    MapDataModel.Layer.LayerType.C_Effect => throw new System.NotImplementedException(),
                    MapDataModel.Layer.LayerType.D_Effect => throw new System.NotImplementedException(),
                    MapDataModel.Layer.LayerType.ForRoute => throw new System.NotImplementedException(),
                    MapDataModel.Layer.LayerType.Region => throw new System.NotImplementedException(),
                    _ => throw new System.NotImplementedException(),
                });
        }

        public static int GetCharacterSortingLayerId(
            EventMapDataModel.EventMapPage.PriorityType priorityType,
            bool isFlying)
        {
            return UnityUtil.SortingLayerManager.GetId(
                priorityType switch
                {
                    EventMapDataModel.EventMapPage.PriorityType.Under =>
                        isFlying ?
                            UnityUtil.SortingLayerManager.SortingLayerIndex.Runtime_MapCharacterInFlyingPriorityUnder :
                            UnityUtil.SortingLayerManager.SortingLayerIndex.Runtime_MapCharacterPriorityUnder,
                    EventMapDataModel.EventMapPage.PriorityType.Normal =>
                        isFlying ?
                            UnityUtil.SortingLayerManager.SortingLayerIndex.Runtime_MapCharacterInFlyingPriorityNormal :
                            UnityUtil.SortingLayerManager.SortingLayerIndex.Runtime_MapCharacterPriorityNormal,
                    EventMapDataModel.EventMapPage.PriorityType.Upper =>
                        isFlying ?
                            UnityUtil.SortingLayerManager.SortingLayerIndex.Runtime_MapCharacterInFlyingPriorityUpper :
                            UnityUtil.SortingLayerManager.SortingLayerIndex.Runtime_MapCharacterPriorityUpper,
                    _ => throw new System.NotImplementedException(),
                });
        }
    }
}
