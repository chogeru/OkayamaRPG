using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RPGMaker.Codebase.Runtime.Map.Component.Map
{
    public class MapLoop : MonoBehaviour
    {
        private const int RANGE_WIDTH  = 28;
        private const int RANGE_HEIGHT = 18;

        private List<Tilemap>      _tilemaps = new List<Tilemap>();
        private MapDataModel       _mapDataModel;
        private List<EventOnMap>   _eventOnMaps   = new List<EventOnMap>();
        private List<VehicleOnMap> _vehiclesOnMap = new List<VehicleOnMap>();

        private int _xPosMin;
        private int _xPosMax;
        private int _yPosMin;
        private int _yPosMax;

        private bool _isHLoop;
        private bool _isRLoop;

        private int  _centerPosX;
        private int  _centerPosY;
        private bool _isLoad = false;

        public string vehicle = "";

        public void SetUp(
            GameObject mapPrefab,
            MapDataModel mapDataModel,
            List<VehicleOnMap> vehicleOnMaps,
            List<EventOnMap> eventOnMaps
        ) {
            _vehiclesOnMap = vehicleOnMaps;
            _eventOnMaps = eventOnMaps;
            var runtimeTilemaps = mapPrefab.GetComponentsInChildren<Tilemap>().ToList();
            _tilemaps = mapDataModel.LayersForRuntime.ConvertAll(layer => layer.tilemap);
            for (int i = 0; i < runtimeTilemaps.Count; i++)
            {
                if (runtimeTilemaps[i].name == "Layer A_Upper" ||
                    runtimeTilemaps[i].name == "Layer B_Upper" ||
                    runtimeTilemaps[i].name == "Layer C_Upper" ||
                    runtimeTilemaps[i].name == "Layer D_Upper")
                    _tilemaps.Add(runtimeTilemaps[i]);
            }
            _mapDataModel = mapDataModel;

            _centerPosX = _mapDataModel.width / 2;
            _centerPosY = -_mapDataModel.height / 2;

            if (_mapDataModel.scrollType == MapDataModel.MapScrollType.LoopBoth)
            {
                _isHLoop = true;
                _isRLoop = true;
            }
            else if (_mapDataModel.scrollType == MapDataModel.MapScrollType.LoopHorizontal)
            {
                _isHLoop = false;
                _isRLoop = true;
            }
            else if (_mapDataModel.scrollType == MapDataModel.MapScrollType.LoopVertical)
            {
                _isHLoop = true;
                _isRLoop = false;
            }
            else
            {
                _isHLoop = false;
                _isRLoop = false;
            }

            if (_isLoad)
                LoopInit();
        }

        private void Start() {
            if(_isHLoop == true || _isRLoop == true)
                LoopInit();

            _isLoad = true;
        }

        /// <summary>
        /// 初回のループ処理
        /// </summary>
        private void LoopInit() {
            var pos = MapManager.OperatingCharacter.GetComponent<CharacterOnMap>().GetCurrentPositionOnLoopMapTile();
            
            // 画面に表示される範囲の設定
            int rangeWidth = RANGE_WIDTH;
            int rangeHeight = RANGE_HEIGHT;

            // XY座標の最大値設定
            int xMin = (int) pos.x - rangeWidth / 2;
            int xMax = (int) pos.x + rangeWidth / 2;
            int yMin = (int) pos.y + rangeHeight / 2;
            int yMax = (int) pos.y - rangeHeight / 2;

            // 範囲設定
            int SetRange(int pos, int range, int size, bool minus = false, bool startToZero = false) {     
                int value = pos + range;
                int add = 0;

                // 開始位置が0の場合
                if (startToZero)
                    add = size - Math.Abs((Math.Abs(value) - (size - 1)) % size);
                else
                    add = size - Math.Abs(value % size);

                value += minus == true ? -Math.Abs(add) : Math.Abs(add);
                return value;
            }

            // マップの最大最小サイズ計算
            if (xMin >= 0 && xMax < _mapDataModel.width - 1)
            {
                xMin = 0;
                xMax = _mapDataModel.width - 1;
            }
            else
            {
                if(xMin >= 0)
                {
                    xMax = SetRange((int)pos.x, RANGE_WIDTH, _mapDataModel.width, false, true);
                    xMin = 0;
                }
                else if(xMax < _mapDataModel.width)
                {
                    xMin = SetRange((int)pos.x, -RANGE_WIDTH, _mapDataModel.width, true);
                    xMax = _mapDataModel.width - 1;
                }
                else
                {
                    xMax = SetRange((int) pos.x, RANGE_WIDTH / 2, _mapDataModel.width, false, true);
                    xMin = SetRange((int) pos.x, -RANGE_WIDTH / 2, _mapDataModel.width, true);
                }
            }

            if (yMin <= 0 && yMax > -_mapDataModel.height + 1)
            {
                yMin = 0;
                yMax = -_mapDataModel.height + 1;
            }
            else
            {
                if (yMin <= 0)
                {
                    yMin = 0;
                    yMax = SetRange((int)pos.y, -RANGE_HEIGHT / 2, _mapDataModel.height, true, true);
                }
                else if (yMax > -_mapDataModel.height + 1)
                {
                    yMax = -_mapDataModel.height + 1;
                    yMin = SetRange((int) pos.y, RANGE_HEIGHT / 2, _mapDataModel.height);
                }
                else
                {
                    yMax = SetRange((int) pos.y, -RANGE_HEIGHT / 2, _mapDataModel.height, true, true);
                    yMin = SetRange((int) pos.y, RANGE_HEIGHT / 2, _mapDataModel.height);
                }
            }

            // タイルの複製
            if (_isRLoop == true)
            {
                for (int i = 0; i < _tilemaps.Count; i++)
                {
                    if (_tilemaps[i] == null) continue;

                    var tilemap = _tilemaps[i];
                    int addValue = i == (int) MapDataModel.Layer.LayerType.Shadow ? 2 : 1;

                    if (i == (int) MapDataModel.Layer.LayerType.Shadow)
                    {
                        // -x方向
                        // 左に不足しているタイルを描画
                        for (int x = 0; x >= xMin * addValue - 1; x--)
                        {
                            for (int y = yMax * addValue; y <= yMin * addValue; y++)
                            {
                                if (tilemap.GetTile(new Vector3Int(x, y, 0)) != null) continue;
                                tilemap.SetTile(new Vector3Int(x, y, 0), tilemap.GetTile<TileBase>(new Vector3Int(_mapDataModel.width * addValue + x, y, 0)));
                            }
                        }
                        // x方向
                        // 右に不足しているタイルを描画
                        for (int x = _mapDataModel.width * addValue - 1; x <= xMax * addValue + 1; x++)
                        {
                            for (int y = yMax * addValue; y <= yMin * addValue; y++)
                            {
                                if (tilemap.GetTile(new Vector3Int(x, y, 0)) != null) continue;
                                tilemap.SetTile(new Vector3Int(x, y, 0), tilemap.GetTile<TileBase>(new Vector3Int(x - _mapDataModel.width * addValue, y, 0)));
                            }
                        }
                    }
                    else
                    {
                        // -x方向
                        // 左に不足しているタイルを描画
                        for (int x = -1; x >= xMin * addValue; x--)
                        {
                            for (int y = yMax * addValue; y <= yMin * addValue; y++)
                            {
                                tilemap.SetTile(new Vector3Int(x, y, 0), tilemap.GetTile<TileBase>(new Vector3Int(_mapDataModel.width * addValue + x, y, 0)));
                            }
                        }
                        // x方向
                        // 右に不足しているタイルを描画
                        for (int x = _mapDataModel.width * addValue; x <= xMax * addValue; x++)
                        {
                            for (int y = yMax * addValue; y <= yMin * addValue; y++)
                            {
                                tilemap.SetTile(new Vector3Int(x, y, 0), tilemap.GetTile<TileBase>(new Vector3Int(x - _mapDataModel.width * addValue, y, 0)));
                            }
                        }
                    }
                }
            }
            if (_isHLoop == true)
            {
                for (int i = 0; i < _tilemaps.Count; i++)
                {
                    if (_tilemaps[i] == null) continue;

                    var tilemap = _tilemaps[i];
                    int addValue = i == (int) MapDataModel.Layer.LayerType.Shadow ? 2 : 1;

                    if (i == (int) MapDataModel.Layer.LayerType.Shadow)
                    {
                        // y方向
                        // 上に不足しているタイルを描画（一部はx方向で描画したものと重複する）
                        for (var y = 0; y <= yMin * addValue + 1; y++)
                        {
                            for (int x = xMin * addValue - 1; x <= xMax * addValue + 1; x++)
                            {
                                if (tilemap.GetTile(new Vector3Int(x, y, 0)) != null) continue;
                                tilemap.SetTile(new Vector3Int(x, y, 0), tilemap.GetTile<TileBase>(new Vector3Int(x, -_mapDataModel.height * addValue + y, 0)));
                            }
                        }
                        // -y方向
                        // 下に不足しているタイルを描画（一部はx方向で描画したものと重複する）
                        for (var y = -_mapDataModel.height * addValue + 1; y >= yMax * addValue - 1; y--)
                        {
                            for (int x = xMin * addValue - 1; x <= xMax * addValue + 1; x++)
                            {
                                if (tilemap.GetTile(new Vector3Int(x, y, 0)) != null) continue;
                                tilemap.SetTile(new Vector3Int(x, y, 0), tilemap.GetTile<TileBase>(new Vector3Int(x, y + _mapDataModel.height * addValue, 0)));
                            }
                        }
                    }
                    else
                    {
                        // y方向
                        // 上に不足しているタイルを描画（一部はx方向で描画したものと重複する）
                        for (var y = 1; y <= yMin * addValue; y++)
                        {
                            for (int x = xMin * addValue; x <= xMax * addValue; x++)
                            {
                                tilemap.SetTile(new Vector3Int(x, y, 0), tilemap.GetTile<TileBase>(new Vector3Int(x, -_mapDataModel.height * addValue + y, 0)));
                            }
                        }
                        // -y方向
                        // 下に不足しているタイルを描画（一部はx方向で描画したものと重複する）
                        for (var y = -_mapDataModel.height * addValue; y >= yMax * addValue; y--)
                        {
                            for (int x = xMin * addValue; x <= xMax * addValue; x++)
                            {
                                tilemap.SetTile(new Vector3Int(x, y, 0), tilemap.GetTile<TileBase>(new Vector3Int(x, y + _mapDataModel.height * addValue, 0)));
                            }
                        }
                    }
                }
            }

            // マップの最大最小座標設定
            _xPosMin = xMin;
            _xPosMax = xMax;
            _yPosMin = yMin;
            _yPosMax = yMax;
        }

        public Vector2 SetInitializePosition(Vector2 partyPosition) {
            //パーティメンバーの移動
            //パーティメンバーの初期表示位置は、マップサイズ内に収める
            if (_isRLoop)
            {
                if (partyPosition.x >= 0)
                {
                    partyPosition.x = partyPosition.x % _mapDataModel.width;
                }
                else
                {
                    partyPosition.x = _mapDataModel.width - (-1 * partyPosition.x % _mapDataModel.width);
                }
            }
            if (_isHLoop)
            {
                if (partyPosition.y <= 0)
                {
                    partyPosition.y = -1 * (-1 * partyPosition.y % _mapDataModel.height);
                }
                else
                {
                    partyPosition.y = -1 * (partyPosition.y % _mapDataModel.height + _mapDataModel.height);
                }
            }

            //イベント座標の移動
            var wmin = partyPosition.x - _centerPosX;
            var wmax = wmin + _mapDataModel.width;
            var hmin = partyPosition.y + _centerPosY;
            var hmax = hmin + (_mapDataModel.height - 1);

            for (int i = 0; i < _eventOnMaps.Count; i++)
            {
                if (MapEventExecutionController.Instance.GetCarryEventOnMap() == _eventOnMaps[i].gameObject)
                    continue;

                if (_isRLoop)
                {
                    if (_eventOnMaps[i].x_now < wmin)
                    {
                        while (_eventOnMaps[i].x_now < wmin)
                            _eventOnMaps[i].SetToPositionOnTileLoop(new Vector2(_eventOnMaps[i].x_now + _mapDataModel.width, _eventOnMaps[i].y_now));
                    }
                    if (_eventOnMaps[i].x_now > wmax)
                    {
                        while (_eventOnMaps[i].x_now > wmax)
                            _eventOnMaps[i].SetToPositionOnTileLoop(new Vector2(_eventOnMaps[i].x_now - _mapDataModel.width, _eventOnMaps[i].y_now));
                    }
                }
                if (_isHLoop)
                {
                    if (_eventOnMaps[i].y_now < hmin)
                    {
                        while (_eventOnMaps[i].y_now < hmin)
                            _eventOnMaps[i].SetToPositionOnTileLoop(new Vector2(_eventOnMaps[i].x_now, _eventOnMaps[i].y_now + _mapDataModel.height));
                    }
                    if (_eventOnMaps[i].y_now > hmax)
                    {
                        while (_eventOnMaps[i].y_now > hmax)
                            _eventOnMaps[i].SetToPositionOnTileLoop(new Vector2(_eventOnMaps[i].x_now, _eventOnMaps[i].y_now - _mapDataModel.height));
                    }
                }
            }

            return partyPosition;
        }

        //マップのループ処理
        public void MapLoopDirection(CharacterOnMap _actorOnMap, CharacterMoveDirectionEnum direction) {
            switch (direction)
            {
                case CharacterMoveDirectionEnum.Up:
                    MapLoopUp(_actorOnMap);
                    break;
                case CharacterMoveDirectionEnum.Down:
                    MapLoopDown(_actorOnMap);
                    break;
                case CharacterMoveDirectionEnum.Left:
                    MapLoopLeft(_actorOnMap);
                    break;
                case CharacterMoveDirectionEnum.Right:
                    MapLoopRight(_actorOnMap);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        /// <summary>
        ///     上に移動。
        /// </summary>
        /// <param name="actorMap">アクター</param>
        public void MapLoopUp(CharacterOnMap actorMap) {
            if (!_isHLoop) return;

            var actorPosY = actorMap.transform.localPosition.y;

            var hmin = actorPosY + _centerPosY;
            var hmax = hmin + (_mapDataModel.height - 1);

            for (int i = 0; i < _tilemaps.Count; i++)
            {
                if (_tilemaps[i] == null) continue;

                var tilemap = _tilemaps[i];
                if (i == (int) MapDataModel.Layer.LayerType.Shadow)
                {
                    for (var x = _xPosMin * 2 - 2; x <= _xPosMax * 2 + 2; ++x)
                    {
                        MoveTile(tilemap, new Vector3Int(x, _yPosMax * 2, 0), new Vector3Int(x, _yPosMin * 2 + 2, 0));
                        MoveTile(tilemap, new Vector3Int(x, _yPosMax * 2 + 1, 0), new Vector3Int(x, _yPosMin * 2 + 2 + 1, 0));
                    }
                }
                else
                {
                    for (var x = _xPosMin; x <= _xPosMax; ++x)
                    {
                        MoveTile(tilemap, new Vector3Int(x, _yPosMax, 0), new Vector3Int(x, _yPosMin + 1, 0));
                    }
                }
            }
            _yPosMax++;
            _yPosMin++;

            for (int i = 0; i < _eventOnMaps.Count; i++)
                if (_eventOnMaps[i].y_next < hmin)
                {
                    int next = _eventOnMaps[i].y_next - _eventOnMaps[i].y_now;
                    _eventOnMaps[i].SetToPositionOnTileLoop(new Vector2(_eventOnMaps[i].x_now, _eventOnMaps[i].y_now + _mapDataModel.height));
                    _eventOnMaps[i].y_next += next;
                }

            for (int i = 0; i < _vehiclesOnMap.Count; i++)
                if (_vehiclesOnMap[i].y_next < hmin)
                {
                    int next = _vehiclesOnMap[i].y_next - _vehiclesOnMap[i].y_now;
                    _vehiclesOnMap[i].SetToPositionOnTile(new Vector2(_vehiclesOnMap[i].x_now, _vehiclesOnMap[i].y_now + _mapDataModel.height));
                    _vehiclesOnMap[i].x_next += next;
                }
        }

        /// <summary>
        ///     下に移動。
        /// </summary>
        /// <param name="actorMap">アクター</param>
        public void MapLoopDown(CharacterOnMap actorMap) {
            if (!_isHLoop) return;

            var actorPosY = actorMap.transform.localPosition.y;

            var hmin = actorPosY + _centerPosY;
            var hmax = hmin + (_mapDataModel.height - 1);

            for (int i = 0; i < _tilemaps.Count; i++)
            {
                if (_tilemaps[i] == null) continue;

                var tilemap = _tilemaps[i];
                if (i == (int) MapDataModel.Layer.LayerType.Shadow)
                {
                    for (var x = _xPosMin * 2 - 2; x <= _xPosMax * 2 + 2; ++x)
                    {
                        MoveTile(tilemap, new Vector3Int(x, _yPosMin * 2, 0), new Vector3Int(x, _yPosMax * 2 - 2, 0));
                        MoveTile(tilemap, new Vector3Int(x, _yPosMin * 2 + 1, 0), new Vector3Int(x, _yPosMax * 2 - 2 + 1, 0));
                    }
                }
                else
                {
                    for (var x = _xPosMin; x <= _xPosMax; ++x)
                    {
                        MoveTile(tilemap, new Vector3Int(x, _yPosMin, 0), new Vector3Int(x, _yPosMax - 1, 0));
                    }
                }
            }
            _yPosMax--;
            _yPosMin--;
            
            for (int i = 0; i < _eventOnMaps.Count; i++)
                if (_eventOnMaps[i].y_next > hmax)
                {
                    int next = _eventOnMaps[i].y_next - _eventOnMaps[i].y_now;
                    _eventOnMaps[i].SetToPositionOnTileLoop(new Vector2(_eventOnMaps[i].x_now, _eventOnMaps[i].y_now - _mapDataModel.height));
                    _eventOnMaps[i].y_next += next;
                }

            for (int i = 0; i < _vehiclesOnMap.Count; i++)
                if (_vehiclesOnMap[i].y_next > hmax)
                {
                    int next = _vehiclesOnMap[i].y_next - _vehiclesOnMap[i].y_now;
                    _vehiclesOnMap[i].SetToPositionOnTile(new Vector2(_vehiclesOnMap[i].x_now, _vehiclesOnMap[i].y_now - _mapDataModel.height));
                    _vehiclesOnMap[i].x_next += next;
                }
        }

        /// <summary>
        ///     右に移動。
        /// </summary>
        /// <param name="actorMap">アクター</param>
        public void MapLoopRight(CharacterOnMap actorMap) {
            if (!_isRLoop) return;

            var actorPosX = actorMap.transform.localPosition.x;

            var wmin = actorPosX - _centerPosX;
            var wmax = wmin + _mapDataModel.width;

            for (int i = 0; i < _tilemaps.Count; i++)
            {
                if (_tilemaps[i] == null) continue;

                var tilemap = _tilemaps[i];
                if (i == (int) MapDataModel.Layer.LayerType.Shadow)
                {
                    for (var y = _yPosMax * 2 - 2; y <= _yPosMin * 2 + 2; y++)
                    {
                        MoveTile(tilemap, new Vector3Int(_xPosMin * 2, y, 0), new Vector3Int(_xPosMax * 2 + 2, y, 0));
                        MoveTile(tilemap, new Vector3Int(_xPosMin * 2 + 1, y, 0), new Vector3Int(_xPosMax * 2 + 2 + 1, y, 0));
                    }
                }
                else
                {
                    for (var y = _yPosMax; y <= _yPosMin; y++)
                        MoveTile(tilemap, new Vector3Int(_xPosMin, y, 0), new Vector3Int(_xPosMax + 1, y, 0));
                }
            }
            _xPosMax++;
            _xPosMin++;

            for(int i = 0; i < _eventOnMaps.Count; i++)
                if (_eventOnMaps[i].x_next < wmin)
                {
                    int next = _eventOnMaps[i].x_next - _eventOnMaps[i].x_now;
                    _eventOnMaps[i].SetToPositionOnTileLoop(new Vector2(_eventOnMaps[i].x_now + _mapDataModel.width, _eventOnMaps[i].y_now));
                    _eventOnMaps[i].x_next += next;
                }

            for (int i = 0; i < _vehiclesOnMap.Count; i++)
                if (_vehiclesOnMap[i].x_next < wmin)
                {
                    int next = _vehiclesOnMap[i].x_next - _vehiclesOnMap[i].x_now;
                    _vehiclesOnMap[i].SetToPositionOnTile(new Vector2(_vehiclesOnMap[i].x_now + _mapDataModel.width, _vehiclesOnMap[i].y_now));
                    _vehiclesOnMap[i].x_next += next;
                }
        }

        /// <summary>
        ///     左に移動。
        /// </summary>
        /// <param name="actorMap">アクター</param>
        public void MapLoopLeft(CharacterOnMap actorMap) {
            if (!_isRLoop) return;

            var actorPosX = actorMap.transform.localPosition.x;

            var wmin = actorPosX - _centerPosX;
            var wmax = wmin + _mapDataModel.width;

            for (int i = 0; i < _tilemaps.Count; i++)
            {
                if (_tilemaps[i] == null) continue;

                var tilemap = _tilemaps[i];
                if (i == (int) MapDataModel.Layer.LayerType.Shadow)
                {
                    for (var y = _yPosMax * 2 - 2; y <= _yPosMin * 2 + 2; y++)
                    {
                        MoveTile(tilemap, new Vector3Int(_xPosMax * 2, y, 0), new Vector3Int(_xPosMin * 2 - 2, y, 0));
                        MoveTile(tilemap, new Vector3Int(_xPosMax * 2 + 1, y, 0), new Vector3Int(_xPosMin * 2 - 2 + 1, y, 0));
                    }
                }
                else
                {
                    for (var y = _yPosMax; y <= _yPosMin; y++)
                        MoveTile(tilemap, new Vector3Int(_xPosMax, y, 0), new Vector3Int(_xPosMin - 1, y, 0));
                }
            }
            _xPosMax--;
            _xPosMin--;
            
            for (int i = 0; i < _eventOnMaps.Count; i++)
                if (_eventOnMaps[i].x_next > wmax)
                {
                    int next = _eventOnMaps[i].x_next - _eventOnMaps[i].x_now;
                    _eventOnMaps[i].SetToPositionOnTileLoop(new Vector2(_eventOnMaps[i].x_now - _mapDataModel.width, _eventOnMaps[i].y_now));
                    _eventOnMaps[i].x_next += next;
                }

            for (int i = 0; i < _vehiclesOnMap.Count; i++)
                if (_vehiclesOnMap[i].x_next > wmax)
                {
                    int next = _vehiclesOnMap[i].x_next - _vehiclesOnMap[i].x_now;
                    _vehiclesOnMap[i].SetToPositionOnTile(new Vector2(_vehiclesOnMap[i].x_now - _mapDataModel.width, _vehiclesOnMap[i].y_now));
                    _vehiclesOnMap[i].x_next += next;
                }
        }

        private void MoveTile(Tilemap tilemap, Vector3Int srcPos, Vector3Int dstPos) {
            var tile = tilemap.GetTile<TileBase>(srcPos);
            if (tile == null) return;

            tilemap.SetTile(dstPos, tile);
            tilemap.SetTile(srcPos, null);
        }

        public void MovePointLoopCharacter(Vector2 movePos, CharacterOnMap actorMap) {
            //現在のマップのループ状況に応じた座標変換
            Vector2 pos = MovePointLoop(movePos);

            //そこまで移動する
            if (_isRLoop)
            {
                if (pos.x < actorMap.x_next)
                {
                    while (pos.x < actorMap.x_next)
                    {
                        actorMap.x_now--;
                        actorMap.x_next--;
                        MapManager.OperatingActor.SetToPositionOnTile(new Vector2(actorMap.x_next, actorMap.y_next));
                        MapLoopLeft(actorMap);
                    }
                }
                else if (pos.x > actorMap.x_next)
                {
                    while (pos.x > actorMap.x_next)
                    {
                        actorMap.x_now++;
                        actorMap.x_next++;
                        MapManager.OperatingActor.SetToPositionOnTile(new Vector2(actorMap.x_next, actorMap.y_next));
                        MapLoopRight(actorMap);
                    }
                }
            }
            if (_isHLoop)
            {
                if (pos.y < actorMap.y_next)
                {
                    while (pos.y < actorMap.y_next)
                    {
                        actorMap.y_now--;
                        actorMap.y_next--;
                        MapManager.OperatingActor.SetToPositionOnTile(new Vector2(actorMap.x_next, actorMap.y_next));
                        MapLoopDown(actorMap);
                    }
                }
                else if (pos.y > actorMap.y_next)
                {
                    while (pos.y > actorMap.y_next)
                    {
                        actorMap.y_now++;
                        actorMap.y_next++;
                        MapManager.OperatingActor.SetToPositionOnTile(new Vector2(actorMap.x_next, actorMap.y_next));
                        MapLoopUp(actorMap);
                    }
                }
            }

            MapManager.OperatingActor.SetToPositionOnTile(pos);
            MapManager.OperatingActor.GetComponent<CharacterOnMap>().ResetBush(false, false);
            //パーティメンバーも強制的に同じ座標に移動
            if (MapManager.PartyOnMap != null)
            {
                for (int i = 0; i < MapManager.PartyOnMap.Count; i++)
                {
                    MapManager.PartyOnMap[i].SetToPositionOnTile(pos);
                    MapManager.PartyOnMap[i].GetComponent<CharacterOnMap>().ResetBush(false, false);
                }
            }
            //乗り物に搭乗中であれば、乗り物の座標も移動する
            if (MapManager.GetRideVehicle() != null)
            {
                MapManager.GetRideVehicle().SetToPositionOnTile(pos);
                MapManager.GetRideVehicle().GetComponent<CharacterOnMap>().ResetBush(false, false);
            }
        }

        public Vector2 MovePointLoopEvent(Vector2 pos) {
            pos = MovePointLoop(pos);

            var actorPosX = MapManager.OperatingActor.x_next;
            var actorPosY = MapManager.OperatingActor.y_next;

            var wmin = actorPosX - _centerPosX;
            var wmax = wmin + _mapDataModel.width;

            var hmin = actorPosY + _centerPosY;
            var hmax = hmin + (_mapDataModel.height - 1);

            if (_isRLoop)
            {
                if (pos.x > wmax)
                {
                    pos.x -= _mapDataModel.width;
                }
                else if (pos.x < wmin)
                {
                    pos.x += _mapDataModel.width;
                }
            }
            if (_isHLoop)
            {
                if (pos.y > hmax)
                {
                    pos.y -= _mapDataModel.height;
                }
                else if (pos.y < hmin)
                {
                    pos.y += _mapDataModel.height;
                }
            }

            return pos;
        }

        public Vector2 MovePointLoop(Vector2 pos) {
            //指定された座標を、現在のループ中の座標に変換
            if (_isRLoop)
            {
                if (pos.x < _xPosMin)
                {
                    while (pos.x < _xPosMin)
                        pos.x += _mapDataModel.width;
                }
                else if (pos.x > _xPosMax)
                {
                    while (pos.x > _xPosMax)
                        pos.x -= _mapDataModel.width;
                }
            }
            if (_isHLoop)
            {
                if (pos.y > _yPosMin)
                {
                    while (pos.y > _yPosMin)
                        pos.y -= _mapDataModel.height;
                }
                else if (pos.y < _yPosMax)
                {
                    while (pos.y < _yPosMax)
                        pos.y += _mapDataModel.height;
                }
            }
            return pos;
        }
    }
}