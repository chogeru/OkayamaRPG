using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Map.Component.Map
{
    public class TilesOnThePosition : MonoBehaviour
    {
        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        [CanBeNull] private TileDataModel _tileA;
        [CanBeNull] private TileDataModel _tileB;
        [CanBeNull] private TileDataModel _tileC;
        [CanBeNull] private TileDataModel _tileD;
        [CanBeNull] private TileDataModel _tileCollider;
        [CanBeNull] private TileDataModel _tileRegion;

        // 状態プロパティ
        //--------------------------------------------------------------------------------------------------------------

        // 表示要素プロパティ
        //--------------------------------------------------------------------------------------------------------------

        // 関数プロパティ
        //--------------------------------------------------------------------------------------------------------------

        // enum / interface / local class
        //--------------------------------------------------------------------------------------------------------------


        // initialize methods
        //--------------------------------------------------------------------------------------------------------------
        /**
         * 初期化
         */
        public void InitForRuntime(MapDataModel mapDataModel, Vector2 position) {
            var mapPrefabManager = mapDataModel.MapPrefabManagerForRuntime;
            _tileA = mapPrefabManager.GetLayerByType(MapDataModel.Layer.LayerType.A).GetTileDataModelByPosition(position);
            _tileB = mapPrefabManager.GetLayerByType(MapDataModel.Layer.LayerType.B).GetTileDataModelByPosition(position);
            _tileC = mapPrefabManager.GetLayerByType(MapDataModel.Layer.LayerType.C).GetTileDataModelByPosition(position);
            _tileD = mapPrefabManager.GetLayerByType(MapDataModel.Layer.LayerType.D).GetTileDataModelByPosition(position);
            _tileCollider = mapPrefabManager.layers[(int) MapDataModel.Layer.LayerType.BackgroundCollision].GetTileDataModelByPosition(position);
            _tileRegion = mapPrefabManager.layers[(int) MapDataModel.Layer.LayerType.Region].GetTileDataModelByPosition(position);
        }

        // 判定系
        //--------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 同位置のこのタイル群に進入できるか？
        /// </summary>
        /// <param name="moveDirectionEnum">進入の向き</param>
        /// <param name="vehicleId">乗り物id (nullの場合、通常のキャラクター)</param>
        /// <returns></returns>
        public bool CanEnterThisTiles(CharacterMoveDirectionEnum moveDirectionEnum, string vehicleId = null)
        {
            foreach (var tileDataModel in new TileDataModel[] { _tileD, _tileC, _tileB, _tileA, _tileCollider })
            {
                // tileDataModelが存在し、すり抜け (キャラクターがタイルの下側を通り抜ける) 以外？
                if (tileDataModel != null && !tileDataModel.CanGoThrough(moveDirectionEnum, vehicleId))
                {
                    return !tileDataModel.CannotEnter(moveDirectionEnum, vehicleId);
                }
            }

            // すり抜け以外のタイルが存在しない場合は、進入不可。
            return false;
        }

        /**
         * 梯子属性の取得
         */
        public bool? GetLadderTile() {
            if (!_tileA && !_tileB && !_tileC && !_tileD && !_tileCollider)
            {
                // タイルが存在しない場合
                return false;
            }

            if ((_tileA != null && _tileA.isLadder) ||
                (_tileB != null && _tileB.isLadder) ||
                (_tileC != null && _tileC.isLadder) ||
                (_tileD != null && _tileD.isLadder) ||
                (_tileCollider != null && _tileCollider.isLadder))
            {
                return true;
            }

            return false;
        }

        /**
         * 茂み属性の取得
         */
        public bool? GetBushTile() {
            if (!_tileA && !_tileB && !_tileC && !_tileD && !_tileCollider)
            {
                // タイルが存在しない場合
                return false;
            }

            if ((_tileA != null && _tileA.isBush) ||
                (_tileB != null && _tileB.isBush) ||
                (_tileC != null && _tileC.isBush) ||
                (_tileD != null && _tileD.isBush) ||
                (_tileCollider != null && _tileCollider.isBush))
            {
                return true;
            }

            return false;
        }
        
        public TileDataModel GetDamageTile(CharacterMoveDirectionEnum moveDirectionEnum) {
            foreach (var tileDataModel in new TileDataModel[] { _tileD, _tileC, _tileB, _tileA, _tileCollider })
            {
                // tileDataModelが存在し、通れるか
                if (tileDataModel != null && !tileDataModel.CannotEnter(moveDirectionEnum) && tileDataModel.isDamageFloor)
                {
                    return tileDataModel;
                }
            }

            return null;
        }

        /**
         * 目の前のタイルがカウンター属性か取得
         */
        public bool? GetHasCounterTile() {
            if (!_tileA && !_tileB && !_tileC && !_tileD && !_tileCollider)
            {
                // タイルが存在しない場合
                return false;
            }

            if ((_tileA != null && _tileA.isCounter) ||
                (_tileB != null && _tileB.isCounter) ||
                (_tileC != null && _tileC.isCounter) ||
                (_tileD != null && _tileD.isCounter) ||
                (_tileCollider != null && _tileCollider.isCounter))
            {
                return true;
            }

            return false;
        }

        /**
         * 現地点のリージョンタイルデータを取得。
         */
        public TileDataModel GetRegionTileDataModel() {
            return _tileRegion;
        }
    }
}