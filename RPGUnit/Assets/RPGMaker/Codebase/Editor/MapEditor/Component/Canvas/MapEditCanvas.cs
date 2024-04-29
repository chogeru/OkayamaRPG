using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement.Repository;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.Canvas
{
    /// <summary>
    /// マップのタイルを編集する機能を持つキャンバスコンポーネント
    /// </summary>
    public class MapEditCanvas : MapCanvas
    {
        private          Vector3Int _startPosition;
        private readonly string     shadowMapPath = "Assets/RPGMaker/Storage/System/Map/ShadowMap.asset";

        /// <summary>
        /// タイルの複数選択用の枠線画像
        /// </summary>
        private const string SelectedMapMassMultiImg = "Assets/RPGMaker/Storage/System/Map/SelectedMapMassMulti/SelectedMapMassMulti.asset";
        /// <summary>
        /// タイルの選択開始位置
        /// </summary>
        private Vector3Int _selectedStartPos = Vector3Int.back;
        /// <summary>
        /// タイルのドラッグ位置
        /// </summary>
        private Vector3Int _selectedPos = Vector3Int.back;
        /// <summary>
        /// タイルの枠画像のz座標
        /// </summary>
        private int _selectedZindex = -6;
        /// <summary>
        /// 編集中のマップのレイヤー
        /// </summary>
        private List<MapDataModel.Layer> _layers;
        /// <summary>
        /// スポイトしたタイルデータ
        /// </summary>
        private List<ClipTileData> _selectedTiles;
        /// <summary>
        /// スポイトしたタイルデータ(影)
        /// </summary>
        private List<ClipTileData> _selectedShadowTiles;
        /// <summary>
        /// 選択した幅
        /// </summary>
        private int _selectedX;
        /// <summary>
        /// 選択した高さ
        /// </summary>
        private int _selectedY;
        /// <summary>
        /// スポイトしたタイルを塗る際の開始座標
        /// </summary>
        private Vector3Int _drawStartPos = Vector3Int.back;
        /// <summary>
        /// スポイトしたタイルを塗る際の現在のドラッグ位置
        /// </summary>
        private Vector3Int _drawPos = Vector3Int.back;
        /// <summary>
        /// Altキー押下処理中
        /// </summary>
        private bool _isAltMode = false;

        /// <summary>
        /// スポイトデータ用クラス
        /// </summary>
        private class ClipTileData
        {
            /// <summary>
            /// 保存するタイルのレイヤー
            /// </summary>
            public MapDataModel.Layer layer;
            /// <summary>
            /// 保存するタイルデータ
            /// </summary>
            public TileDataModel tileData;
            /// <summary>
            /// x座標
            /// </summary>
            public int x;
            /// <summary>
            /// y座標
            /// </summary>
            public int y;
        }

        // 状態プロパティ
        private MapDataModel.Layer  _currentTargetMapLayer;
        private          TileDataModel       _currentTileToDraw;
        private          List<TileDataModel> _currentTilesToDraw;
        private readonly TileDataModel       _shadowToDraw;
        private          DrawMode            _drawMode;

        private bool _isUndo;
        private bool _isSpoitMode;
        private bool _isSpoitDrawMode;

        // local enum
        public enum DrawMode
        {
            Put,
            Rectangle,
            Ellipse,
            Fill,
            Shadow,

            Delete,
            DeleteRectangle,
            DeleteEllipse,
            DeleteFill,
            DeleteShadow
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mapDataModel"></param>
        /// <param name="repaintMethod"></param>
        /// <param name="isSampleMap"></param>
        public MapEditCanvas(MapDataModel mapDataModel, Action repaintMethod, bool isSampleMap = false)
            : base(mapDataModel, repaintMethod, isSampleMap)
        {
            DebugUtil.Log($"MapEditCanvas(name=\"{mapDataModel.name}\", id=\"{mapDataModel.id}\")");

            Undo.undoRedoPerformed += RenderAndSave;

            if (isSampleMap) return;

            LayersProcessForEditor(ref _shadowToDraw);

            SetAllEventTiles(null);
            SetEarlyPosition();

            //スポイト機能用の初期化処理
            _isSpoitMode = false;
            _isSpoitDrawMode = false;

            void LayersProcessForEditor(ref TileDataModel shadowToDraw)
            {
                var layers = MapDataModel.MapPrefabManagerForEditor.layers;

                //シャドウ用のデータモデルを作成
                foreach (var layer in layers)
                {
                    if (layer.type == MapDataModel.Layer.LayerType.Shadow)
                    {
                        layer.tilesOnPalette = new List<TileDataModelInfo>();
                        var tileDataModel =
                            UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<TileDataModel>(shadowMapPath);
                        layer.tilesOnPalette.Add(tileDataModel.tileDataModelInfo);
                        shadowToDraw = layer.tilesOnPalette[0].TileDataModel;
                        break;
                    }
                }

                //_shadowToDraw

                ChangeTargetLayer(layers.First());

                //イベント描画処理移植
                foreach (var layer in layers)
                {
                    if (layer.type == MapDataModel.Layer.LayerType.ForRoute)
                    {
                        _routeTileMap = layer.tilemap;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 描画対象レイヤーを変更
        /// </summary>
        /// <param name="mapLayer"></param>
        public void ChangeTargetLayer(MapDataModel.Layer mapLayer) {
            _currentTargetMapLayer = mapLayer;
            _currentTileToDraw = null;
            Render();
        }

        /// <summary>
        /// 現在のタイルデータを取得
        /// </summary>
        /// <returns></returns>
        public TileDataModel GetTileToDraw() {
            return _currentTileToDraw;
        }

        /// <summary>
        /// 描画タイルを変更
        /// </summary>
        /// <param name="tileDataModel"></param>
        /// <param name="tileDataModels"></param>
        public void ChangeTileToDraw(TileDataModel tileDataModel, List<TileDataModel> tileDataModels) {
            _currentTileToDraw = tileDataModel;
            _currentTilesToDraw = tileDataModels;

            if (_currentTargetMapLayer != null)
            {
                // 背景CollisionはInspectorを開く
                if (_currentTargetMapLayer.type == MapDataModel.Layer.LayerType.BackgroundCollision)
                    Inspector.Inspector.MapBackgroundCollisionView(tileDataModel);
            }

            //タイルが変更された場合には、右クリックによるスポイト機能を中断
            _isSpoitMode = false;
            _isSpoitDrawMode = false;
            RemoveCursorPos();

            ChangePenButtonDisplay();
        }

        /// <summary>
        /// 選択されたタイルによってペンの表示を変更
        /// </summary>
        private void ChangePenButtonDisplay() {
            // 各タイルタイプによってペンの状態を設定
            if (_currentTileToDraw?.type == TileDataModel.Type.LargeParts)
            {
                if (_drawMode == DrawMode.Ellipse || 
                    _drawMode == DrawMode.Rectangle || 
                    _drawMode == DrawMode.Fill || 
                    _drawMode == DrawMode.DeleteRectangle || 
                    _drawMode == DrawMode.DeleteEllipse ||
                    _drawMode == DrawMode.Shadow)
                    Inspector.Inspector.GetPenButtonMenu()?.ResetButton(Common.Window.MenuWindow.BtnType.Pen);
                Inspector.Inspector.GetPenButtonMenu()?.ButtonDisplayChange(Common.Window.MenuWindow.BtnType.Ellipse, DisplayStyle.None);
                Inspector.Inspector.GetPenButtonMenu()?.ButtonDisplayChange(Common.Window.MenuWindow.BtnType.Rectangle, DisplayStyle.None);            
                Inspector.Inspector.GetPenButtonMenu()?.ButtonDisplayChange(Common.Window.MenuWindow.BtnType.Fill, DisplayStyle.None);
            }
            else if (_currentTileToDraw?.type == TileDataModel.Type.AutoTileC)
            {
                if (_drawMode == DrawMode.Fill ||
                    _drawMode == DrawMode.Shadow)
                    Inspector.Inspector.GetPenButtonMenu()?.ResetButton(Common.Window.MenuWindow.BtnType.Pen);
                Inspector.Inspector.GetPenButtonMenu()?.ButtonDisplayChange(Common.Window.MenuWindow.BtnType.Ellipse, DisplayStyle.Flex);
                Inspector.Inspector.GetPenButtonMenu()?.ButtonDisplayChange(Common.Window.MenuWindow.BtnType.Rectangle, DisplayStyle.Flex);
                Inspector.Inspector.GetPenButtonMenu()?.ButtonDisplayChange(Common.Window.MenuWindow.BtnType.Fill, DisplayStyle.None);
            }
            else if (_currentTileToDraw == _shadowToDraw)
            {
                if (_drawMode == DrawMode.Fill)
                    Inspector.Inspector.GetPenButtonMenu()?.ResetButton(Common.Window.MenuWindow.BtnType.Shadow);
                Inspector.Inspector.GetPenButtonMenu()?.ButtonDisplayChange(Common.Window.MenuWindow.BtnType.Ellipse, DisplayStyle.Flex);
                Inspector.Inspector.GetPenButtonMenu()?.ButtonDisplayChange(Common.Window.MenuWindow.BtnType.Rectangle, DisplayStyle.Flex);
                Inspector.Inspector.GetPenButtonMenu()?.ButtonDisplayChange(Common.Window.MenuWindow.BtnType.Fill, DisplayStyle.None);
            }
            else
            {
                if (_drawMode == DrawMode.Shadow)
                    Inspector.Inspector.GetPenButtonMenu()?.ResetButton(Common.Window.MenuWindow.BtnType.Pen);
                Inspector.Inspector.GetPenButtonMenu()?.ButtonDisplayChange(Common.Window.MenuWindow.BtnType.Fill, DisplayStyle.Flex);
                Inspector.Inspector.GetPenButtonMenu()?.ButtonDisplayChange(Common.Window.MenuWindow.BtnType.Ellipse, DisplayStyle.Flex);
                Inspector.Inspector.GetPenButtonMenu()?.ButtonDisplayChange(Common.Window.MenuWindow.BtnType.Rectangle, DisplayStyle.Flex);
            }
        }

        public void ChangeDrawMode(DrawMode drawMode) {
            _drawMode = drawMode;
        }

        // 描画処理
        //--------------------------------------------------------------------------------------

        /// <summary>
        /// タイルブラッシング実行
        /// </summary>
        /// <param name="cellPos"></param>
        private void Brush(Vector3Int cellPos) {
            // タイルがnullで消しゴムか影以外であればreturn
            if (_currentTileToDraw == null &&
                _drawMode != DrawMode.Delete &&
                _drawMode != DrawMode.DeleteShadow &&
                _drawMode != DrawMode.DeleteRectangle &&
                _drawMode != DrawMode.DeleteEllipse &&
                _drawMode != DrawMode.Shadow)
            {
                //スポイト機能中でもない場合はreturn
                if (!_isSpoitDrawMode)
                {
                    return;
                }
            }

            // 大型パーツはDragでの塗りなし
            if (_currentTileToDraw != null &&
                _currentTileToDraw.type == TileDataModel.Type.LargeParts &&
                _drawMode != DrawMode.Delete &&
                _drawMode != DrawMode.DeleteShadow &&
                _drawMode != DrawMode.DeleteRectangle &&
                _drawMode != DrawMode.DeleteEllipse)
                return;

            if (!_isUndo)
            {
                _isUndo = true;
                Undo.RegisterFullObjectHierarchyUndo(_currentTargetMapLayer.tilemap.transform.parent.gameObject, name);
            }

            switch (_drawMode)
            {
                case DrawMode.Put:
                    if (_isSpoitDrawMode)
                    {
                        //スポイトしている状況であれば、スポイトしたタイルを塗る
                        BrushSpoitDataPen(cellPos);
                        break;
                    }
                    SetTile(cellPos, _currentTileToDraw);
                    break;
                case DrawMode.Rectangle:
                    RectangleBrush(cellPos);
                    break;
                case DrawMode.Ellipse:
                    EllipseBrush(cellPos);
                    break;
                case DrawMode.Fill:
                    Fill(cellPos, _currentTargetMapLayer.GetTileDataModelByPosition(new Vector2(cellPos.x, cellPos.y)));
                    break;
                case DrawMode.Shadow:
                    _layers[(int) MapDataModel.Layer.LayerType.Shadow].tilemap.SetTile(cellPos, _shadowToDraw);
                    break;
                case DrawMode.Delete:
                    _currentTargetMapLayer.tilemap.SetTile(cellPos, null);
                    break;
                case DrawMode.DeleteShadow:
                    _currentTargetMapLayer.tilemap.SetTile(cellPos, null);
                    break;
                case DrawMode.DeleteRectangle:
                    RectangleDelete(cellPos);
                    break;
                case DrawMode.DeleteEllipse:
                    EllipseDelete(cellPos);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Render();
        }

        /// <summary>
        /// 大型パーツタイルブラッシング実行
        /// </summary>
        /// <param name="cellPos"></param>
        private void BrushLargeParts(Vector3Int cellPos) {
            if (!_isUndo)
            {
                _isUndo = true;
                Undo.RegisterFullObjectHierarchyUndo(_currentTargetMapLayer.tilemap.transform.parent.gameObject, name);
            }

            var drawTiles = _currentTilesToDraw.Where(
                t => t.type == TileDataModel.Type.LargeParts &&
                     t.largePartsDataModel.parentId == _currentTileToDraw.largePartsDataModel.parentId).ToArray();
            foreach (var drawTile in drawTiles)
                if (cellPos.x + drawTile.largePartsDataModel.x < MapDataModel.width &&
                    cellPos.x + drawTile.largePartsDataModel.x >= 0 &&
                    cellPos.y - drawTile.largePartsDataModel.y > -MapDataModel.height &&
                    cellPos.y - drawTile.largePartsDataModel.y <= 0)

                    SetTile(
                        new Vector3Int(
                            cellPos.x + drawTile.largePartsDataModel.x,
                            cellPos.y - drawTile.largePartsDataModel.y,
                            cellPos.z),
                        drawTile);

            Render();
        }

        /// <summary>
        /// スポイトしたタイルを塗る
        /// </summary>
        /// <param name="cellPos"></param>
        private void BrushSpoitDataPen(Vector3Int cellPos) {
            int startX = (int) Mathf.Min(_selectedStartPos.x, _selectedPos.x);
            int endX = (int) Mathf.Max(_selectedStartPos.x, _selectedPos.x);
            int startY = (int) Mathf.Min(_selectedStartPos.y, _selectedPos.y);
            int endY = (int) Mathf.Max(_selectedStartPos.y, _selectedPos.y);

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    //ペンの場合は補正を行わない
                    BrushSpoitData(new Vector3Int(x, y, 0), true);
                }
            }
        }

        /// <summary>
        /// 四角のブラシ
        /// </summary>
        /// <param name="cellPos"></param>
        private void RectangleBrush(Vector3Int cellPos) {
            var min = cellPos - _startPosition;
            var start = _startPosition;

            // X座標の調整
            if (cellPos.x >= _startPosition.x)
            {
                min.x++;
            }
            else
            {
                start.x++;
                min.x--;
            }

            // Y座標の調整
            if (min.y >= 0)
            {
                min.y++;
            }
            else
            {
                start.y++;
                min.y--;
            }

            var bounds = new BoundsInt(start, new Vector3Int(min.x, min.y, 1));
            var pos = new List<Vector3Int>();
            var tile = new List<TileDataModel>();
            foreach (var location in bounds.allPositionsWithin)
            {
                pos.Add(location);
                tile.Add(_currentTileToDraw);
            }

            if (_isSpoitDrawMode)
            {
                for (int i = 0; i < pos.Count; i++)
                {
                    BrushSpoitData(pos[i]);
                }
                return;
            }
            SetTiles(pos, tile);
        }

        /// <summary>
        /// 塗りつぶし
        /// </summary>
        /// <param name="cellPos"></param>
        /// <param name="fillTarget"></param>
        private void Fill(Vector3Int cellPos, TileDataModel fillTarget) {
            // 描画範囲最適化
            _currentTargetMapLayer.tilemap.CompressBounds();
            var pos = new Vector3Int(MapDataModel.width - 1, -MapDataModel.height + 1, 0);
            if (_currentTargetMapLayer.GetTileDataModelByPosition(new Vector2(pos.x, pos.y)) == null)
            {
                // 右下端が塗られていなければ一時的に塗る
                SetTile(pos, _currentTileToDraw);
                _currentTargetMapLayer.tilemap.SetTile(pos, null);
            }
            pos = new Vector3Int(MapDataModel.width - 1, 0, 0);
            if (_currentTargetMapLayer.GetTileDataModelByPosition(new Vector2(pos.x, pos.y)) == null)
            {
                // 右上端が塗られていなければ一時的に塗る
                SetTile(pos, _currentTileToDraw);
                _currentTargetMapLayer.tilemap.SetTile(pos, null);
            }
            _currentTargetMapLayer.tilemap.FloodFill(cellPos, _currentTileToDraw);
        }

        /// <summary>
        /// 四角の消しゴム
        /// </summary>
        /// <param name="cellPos"></param>
        private void RectangleDelete(Vector3Int cellPos) {
            var min = cellPos - _startPosition;
            var start = _startPosition;

            // X座標の調整
            if (cellPos.x >= _startPosition.x)
            {
                min.x++;
            }
            else
            {
                start.x++;
                min.x--;
            }

            // Y座標の調整
            if (min.y >= 0)
            {
                min.y++;
            }
            else
            {
                start.y++;
                min.y--;
            }

            var bounds = new BoundsInt(start, new Vector3Int(min.x, min.y, 1));
            var pos = new List<Vector3Int>();
            var tile = new List<TileDataModel>();
            foreach (var location in bounds.allPositionsWithin)
            {
                pos.Add(location);
                tile.Add(null);
            }

            _currentTargetMapLayer.tilemap.SetTiles(pos.ToArray(), tile.ToArray());
        }

        /// <summary>
        /// 丸型のブラシ
        /// </summary>
        /// <param name="cellPos"></param>
        private void EllipseBrush(Vector3Int cellPos) {
            var min = cellPos - _startPosition;
            var start = _startPosition;

            // X座標の調整
            if (cellPos.x >= _startPosition.x)
                min.x++;
            else
                min.x--;
            // Y座標の調整
            if (min.y >= 0)
                min.y++;
            else
                min.y--;

            var newRect = new RectInt(new Vector2Int(start.x, start.y),
                new Vector2Int(min.x,
                    min.y));

            var signX = 1;
            var signY = 1;
            if (cellPos.x < start.x) signX = -1;
            if (cellPos.y < start.y) signY = -1;

            var a = Mathf.Abs(newRect.width) * 0.5;
            var b = Mathf.Abs(newRect.height) * 0.5;
            var aa = a * a;
            var bb = b * b;
            //
            for (var y = 0; y < Mathf.Abs(newRect.height); y++)
                for (var x = 0; x < Mathf.Abs(newRect.width); x++)
                {
                    var xx = (x + 0.5 - a) * (x + 0.5 - a);
                    var yy = (y + 0.5 - b) * (y + 0.5 - b);
                    if (xx / aa + yy / bb < 1)
                    {
                        if (_isSpoitDrawMode)
                        {
                            BrushSpoitData(new Vector3Int(newRect.x + x * signX, newRect.y + y * signY, 0));
                            continue; ;
                        }

                        SetTile(
                            new Vector3Int(newRect.x + x * signX, newRect.y + y * signY, 0),
                            _currentTileToDraw);
                    }
                    else
                    {
                        _currentTargetMapLayer.tilemap.SetTile(
                            new Vector3Int(newRect.x + x * signX, newRect.y + y * signY, 0),
                            null);
                    }
                }
        }

        /// <summary>
        /// 丸型の消しゴム
        /// </summary>
        /// <param name="cellPos"></param>
        private void EllipseDelete(Vector3Int cellPos) {
            var min = cellPos - _startPosition;
            var start = _startPosition;

            // X座標の調整
            if (cellPos.x >= _startPosition.x)
                min.x++;
            else
                min.x--;
            // Y座標の調整
            if (min.y >= 0)
                min.y++;
            else
                min.y--;

            var newRect = new RectInt(new Vector2Int(start.x, start.y),
                new Vector2Int(min.x,
                    min.y));

            var signX = 1;
            var signY = 1;
            if (cellPos.x < start.x) signX = -1;
            if (cellPos.y < start.y) signY = -1;

            var a = Mathf.Abs(newRect.width) * 0.5;
            var b = Mathf.Abs(newRect.height) * 0.5;
            var aa = a * a;
            var bb = b * b;
            //
            for (var y = 0; y < Mathf.Abs(newRect.height); y++)
                for (var x = 0; x < Mathf.Abs(newRect.width); x++)
                {
                    var xx = (x + 0.5 - a) * (x + 0.5 - a);
                    var yy = (y + 0.5 - b) * (y + 0.5 - b);
                    if (xx / aa + yy / bb < 1)
                        _currentTargetMapLayer.tilemap.SetTile(
                            new Vector3Int(newRect.x + x * signX, newRect.y + y * signY, 0),
                            null);
                }
        }

        /// <summary>
        /// タイルをセット
        /// （AutoTileC用の処理も行う）
        /// </summary>
        /// <param name="cellPos"></param>
        /// <param name="tileDataModel"></param>
        /// <returns></returns>
        public bool SetTile(Vector3Int cellPos, TileDataModel tileDataModel = null) {
            var tileData = tileDataModel;

            // セット先のタイル
            var destTile = _currentTargetMapLayer.GetTileDataModelByPosition(new Vector2(cellPos.x, cellPos.y));
            // 設定先の1マス上
            var destTileTop = _currentTargetMapLayer.GetTileDataModelByPosition(new Vector2(cellPos.x, cellPos.y) + new Vector2Int(0, 1));
            // 設定先の1マス下
            var destTileUnder = _currentTargetMapLayer.GetTileDataModelByPosition(new Vector2(cellPos.x, cellPos.y) + new Vector2Int(0, -1));

            // Set先か上のタイルがAutoTileCであれば終了
            if ((destTile != null && destTile.type == TileDataModel.Type.AutoTileC && tileData?.id != destTile.id) ||
                (destTileTop != null && destTileTop.type == TileDataModel.Type.AutoTileC && tileData?.id != destTileTop.id))
                return false;

            // ここでセット
            _currentTargetMapLayer.tilemap.SetTile(cellPos, tileData);

            // AutoTileCであれば専用処理を行う
            if (tileData?.type == TileDataModel.Type.AutoTileC)
            {
                if (tileData.id != destTileUnder?.id)
                    _currentTargetMapLayer.tilemap.SetTile(cellPos + new Vector3Int(0, -1, 0), null);
            }
            return true;
        }

        /// <summary>
        /// 複数タイルをセット
        /// </summary>
        /// <param name="cellsPos"></param>
        /// <param name="tileDataModels"></param>
        public void SetTiles(List<Vector3Int> cellsPos, List<TileDataModel> tileDataModels) {
            for (int i = 0; i < cellsPos.Count; i++)
                SetTile(cellsPos[i], tileDataModels[i]);
        }

        /// <summary>
        /// マップサイズ変更
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void ChangeMapSizeForEditor(int width, int height) {
            // サイズを大きくした場合は処理しない
            if (MapDataModel.width <= width && MapDataModel.height <= height)
                return;

            var layers = MapDataModel.MapPrefabManagerForEditor.layers;

            // 範囲外を削除
            // X
            if (MapDataModel.width > width)
            {
                var bounds = new BoundsInt(new Vector3Int(width, 0, 0),
                    new Vector3Int(MapDataModel.width, MapDataModel.height, 1));
                foreach (var location in bounds.allPositionsWithin)
                {
                    var loc = location;
                    loc.y = loc.y * -1;

                    if (_currentTargetMapLayer.tilemap == null) continue;

                    if (_currentTargetMapLayer.type == MapDataModel.Layer.LayerType.Shadow)
                    {
                        loc.x = loc.x * 2;
                        loc.y = loc.y * 2;
                        _currentTargetMapLayer.tilemap.SetTile(loc, null);
                        _currentTargetMapLayer.tilemap.SetTile(loc + new Vector3Int(0, 1, 0), null);
                        _currentTargetMapLayer.tilemap.SetTile(loc + new Vector3Int(1, 0, 0), null);
                        _currentTargetMapLayer.tilemap.SetTile(loc + new Vector3Int(1, 1, 0), null);
                    }
                    else
                    {
                        _currentTargetMapLayer.tilemap.SetTile(loc, null);
                    }
                }
            }

            // Y
            if (MapDataModel.height > height)
            {
                var bounds = new BoundsInt(new Vector3Int(0, height, 0),
                    new Vector3Int(MapDataModel.width, MapDataModel.height, 1));
                foreach (var location in bounds.allPositionsWithin)
                {
                    var loc = location;
                    loc.y = loc.y * -1;

                    if (_currentTargetMapLayer.tilemap == null) continue;

                    if (_currentTargetMapLayer.type == MapDataModel.Layer.LayerType.Shadow)
                    {
                        loc.x = loc.x * 2;
                        loc.y = loc.y * 2;
                        _currentTargetMapLayer.tilemap.SetTile(loc, null);
                        _currentTargetMapLayer.tilemap.SetTile(loc + new Vector3Int(0, 1, 0), null);
                        _currentTargetMapLayer.tilemap.SetTile(loc + new Vector3Int(1, 0, 0), null);
                        _currentTargetMapLayer.tilemap.SetTile(loc + new Vector3Int(1, 1, 0), null);
                    }
                    else
                    {
                        _currentTargetMapLayer.tilemap.SetTile(loc, null);
                    }
                }
            }
        }

        /// <summary>
        /// マップイベント削除
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DeleteEvent(int width, int height) {
            // イベントを一括削除
            var eventMaps = _eventManagementService.LoadEventMap();
            for (var i = 0; i < eventMaps.Count; i++)
                if (eventMaps[i].mapId == MapDataModel.id)
                    if (eventMaps[i].x >= width || eventMaps[i].y <= -height)
                    {
                        MapEditor.DeleteEventMap(eventMaps[i]);
                        i--;
                    }

            _ = Editor.Hierarchy.Hierarchy.Refresh(Region.Map, AbstractHierarchyView.RefreshTypeMapSize + "," + MapDataModel.id);
        }

        // イベントハンドラ
        //--------------------------------------------------------------------------------------
        protected override void OnMouseDown(IMouseEvent e) {
            if (e.altKey || (MouseButton)e.button == MouseButton.MiddleMouse)
            {
                IsMouseDown = true;
                _isAltMode = true;
                return;
            }

            if (_currentTargetMapLayer.tilemap == null)
                return;

            // 座標取得
            Vector3Int cellPos = _currentTargetMapLayer.type == MapDataModel.Layer.LayerType.Shadow || _drawMode == DrawMode.Shadow
                ? GetTilePosOnMousePosForEditor(e.mousePosition, true, true)
                : GetTilePosOnMousePosForEditor(e.mousePosition, false, true);

            if (cellPos.x < MapDataModel.width && cellPos.x >= 0 && cellPos.y > -MapDataModel.height &&
                cellPos.y <= 0 ||
                cellPos.x < MapDataModel.width * 2 && cellPos.x >= 0 && cellPos.y > -MapDataModel.height * 2 + 1 &&
                cellPos.y <= 1 && (_currentTargetMapLayer.type == MapDataModel.Layer.LayerType.Shadow || _drawMode == DrawMode.Shadow))
            {
                IsMouseDown = true;

                _startPosition = cellPos;

                //右クリックだった場合には、スポイト機能開始
                if ((MouseButton) e.button == MouseButton.RightMouse)
                {
                    //ただし、スポイト機能は影ペンでは利用できない（MV準拠）
                    //一枚絵背景、遠景画像も趣旨とことなるため利用しない
                    if (_currentTargetMapLayer.type != MapDataModel.Layer.LayerType.Shadow &&
                        _currentTargetMapLayer.type != MapDataModel.Layer.LayerType.Background &&
                        _currentTargetMapLayer.type != MapDataModel.Layer.LayerType.BackgroundCollision &&
                        _currentTargetMapLayer.type != MapDataModel.Layer.LayerType.DistantView)
                    {
                        //スポイト機能中
                        _isSpoitMode = true;
                        //既に選択中であった場合、初期化
                        RemoveCursorPos();
                        //選択中の枠線を表示
                        SetCursorPos(GetTilePosOnMousePosForEditor(e.mousePosition, false, true));
                        //スポイト機能中のメニュー表示変更
                        _currentTileToDraw = null;
                        ChangePenButtonDisplay();
                    }
                    return;
                }
                if ((MouseButton) e.button == MouseButton.LeftMouse && _isSpoitMode)
                {
                    //スポイトしている状況であれば、スポイトしたタイルを塗る
                    _isSpoitDrawMode = true;
                    //枠線を移動する（非表示にする）
                    MoveCursorPos(GetTilePosOnMousePosForEditor(e.mousePosition, false, true));
                }

                // 大型パーツ
                if (_currentTileToDraw != null &&
                    _currentTileToDraw.type == TileDataModel.Type.LargeParts &&
                    _drawMode != DrawMode.Delete &&
                    _drawMode != DrawMode.DeleteRectangle &&
                    _drawMode != DrawMode.DeleteEllipse &&
                    _drawMode != DrawMode.DeleteFill &&
                    _drawMode != DrawMode.DeleteShadow)
                {
                    BrushLargeParts(GetTilePosOnMousePosForEditor(e.mousePosition));
                    return;
                }

                Brush(cellPos);
            }
        }

        protected override void OnMouseUp(IMouseEvent e) {
            base.OnMouseUp(e);
            _isUndo = false;

            // 中央ボタン(ホイール)押下中または、Altキー押下中の処理実施中だった場合には処理しない
            if (e.pressedButtons == MousePressedButtons.Middle || _isAltMode)
            {
                _isAltMode = false;
                return;
            }

            //右クリックだった場合には、スポイト機能
            if (_isSpoitMode && !_isSpoitDrawMode)
            {
                //選択中の枠線を表示
                SetCursorPos(GetTilePosOnMousePosForEditor(e.mousePosition, false, true));
                //選択した範囲内のタイルデータを保存
                SpoitTileData();
                return;
            }
            //スポイトしたタイルを塗っていた場合は初期化する
            if (_isSpoitDrawMode)
            {
                _isSpoitDrawMode = false;
                _drawStartPos = Vector3Int.back;
                _drawPos = Vector3Int.back;
                //枠線を移動する（表示する）
                MoveCursorPos(GetTilePosOnMousePosForEditor(e.mousePosition, false, true));
            }

            MapEditor.SaveMap(MapDataModel, MapRepository.SaveType.SAVE_PREFAB);
            UnityEditorWrapper.AssetDatabaseWrapper.SaveAssets();
        }

        protected override void OnMouseDrag(MouseMoveEvent e) {
            // 中央ボタン(ホイール)押下中ならベースに処理させる。
            if (e.pressedButtons == MousePressedButtons.Middle)
            {
                base.OnMouseDrag(e);
                return;
            }

            if (!IsMouseDown || !(e.pressedButtons == MousePressedButtons.Left || e.pressedButtons == MousePressedButtons.Right))
            {
                //マウスをクリックしていない
                IsMouseDown = false;
                if (_isSpoitMode)
                {
                    //スポイト機能中で、マウスをクリックしていない状況で移動する場合、枠線を移動する
                    MoveCursorPos(GetTilePosOnMousePosForEditor(e.mousePosition, false, true));
                }
                return;
            }

            // Altキー押下中ならマップ移動。
            if (e.altKey)
            {
                IsAltOn = true;
                _isAltMode = true;
                base.OnMouseDrag(e);
                return;
            }

            if (IsAltOn)
            {
                IsMouseDown = false;
                IsAltOn = false;
                return;
            }

            //スポイト機能中のドラッグ操作
            if (_isSpoitMode && !_isSpoitDrawMode)
            {
                //選択中の枠線を表示
                SetCursorPos(GetTilePosOnMousePosForEditor(e.mousePosition, false, true));
                return;
            }
            if (_isSpoitDrawMode)
            {
                //枠線を移動する（非表示にする）
                MoveCursorPos(GetTilePosOnMousePosForEditor(e.mousePosition, false, true));
            }

            if (_currentTargetMapLayer.tilemap == null)
                return;

            // 座標取得
            Vector3Int cellPos = _currentTargetMapLayer.type == MapDataModel.Layer.LayerType.Shadow ||
                _drawMode == DrawMode.Shadow
                ? GetTilePosOnMousePosForEditor(e.mousePosition, true, true)
                : GetTilePosOnMousePosForEditor(e.mousePosition, false, true);

            //X座標の補正
            if (cellPos.x < 0) cellPos.x = 0;
            if ((_currentTargetMapLayer.type == MapDataModel.Layer.LayerType.Shadow || _drawMode == DrawMode.Shadow) && cellPos.x >= MapDataModel.width * 2) cellPos.x = MapDataModel.width * 2 - 1;
            else if ((_currentTargetMapLayer.type != MapDataModel.Layer.LayerType.Shadow && _drawMode != DrawMode.Shadow) && cellPos.x >= MapDataModel.width) cellPos.x = MapDataModel.width - 1;

            //Y座標の補正
            if ((_currentTargetMapLayer.type == MapDataModel.Layer.LayerType.Shadow || _drawMode == DrawMode.Shadow) && cellPos.y > 1) cellPos.y = 1;
            else if ((_currentTargetMapLayer.type != MapDataModel.Layer.LayerType.Shadow && _drawMode != DrawMode.Shadow) && cellPos.y > 0) cellPos.y = 0;
            if ((_currentTargetMapLayer.type == MapDataModel.Layer.LayerType.Shadow || _drawMode == DrawMode.Shadow) && cellPos.y <= -MapDataModel.height * 2 + 1) cellPos.y = -MapDataModel.height * 2 + 2;
            else if ((_currentTargetMapLayer.type != MapDataModel.Layer.LayerType.Shadow && _drawMode != DrawMode.Shadow) && cellPos.y <= -MapDataModel.height) cellPos.y = -MapDataModel.height + 1;

            // 矩形、円形時は塗り状態を戻す
            if (_drawMode == DrawMode.Rectangle || _drawMode == DrawMode.Ellipse || _drawMode == DrawMode.DeleteRectangle || _drawMode == DrawMode.DeleteEllipse)
            if (_isUndo)
            {
                Undo.PerformUndo();
                _isUndo = false;
            }

            Brush(cellPos);
        }

        /// <summary>
        /// カーソル位置を設定する。
        /// </summary>
        public void SetCursorPos(Vector3Int cellPos) {
            //不正値を補正
            if (cellPos.x < 0)
            {
                cellPos.x = 0;
            }
            if (cellPos.x >= MapDataModel.width)
            {
                cellPos.x = MapDataModel.width - 1;
            }

            if (cellPos.y > 0)
            {
                cellPos.y = 0;
            }
            if (cellPos.y <= -MapDataModel.height)
            {
                cellPos.y = -MapDataModel.height + 1;
            }

            //座標に変化が無ければ処理終了
            if (_selectedPos == cellPos)
            {
                return;
            }
            //レイヤーが無ければ処理終了
            if (_tilemapLayers == null)
            {
                return;
            }
            
            //変数初期化
            var tilemap = _tilemapLayers[(int) TilemapLayer.Type.Cursor].tilemap;
            int startX;
            int endX;
            int startY;
            int endY;

            if (_selectedStartPos != Vector3Int.back)
            {
                //既に右クリックによる範囲選択状態であれば、直前までに描画していたものを破棄
                startX = (int) Mathf.Min(_selectedStartPos.x, _selectedPos.x);
                endX = (int) Mathf.Max(_selectedStartPos.x, _selectedPos.x);
                startY = (int) Mathf.Min(_selectedStartPos.y, _selectedPos.y);
                endY = (int) Mathf.Max(_selectedStartPos.y, _selectedPos.y);

                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, _selectedZindex), null);
                    }
                }
            }

            //現在の座標を保持
            _selectedPos = new Vector3Int(cellPos.x, cellPos.y, _selectedZindex);

            if (_selectedStartPos == Vector3Int.back)
            {
                //右クリックによる範囲選択開始状態であれば、現在の座標を保持
                _selectedStartPos = _selectedPos;
            }

            //範囲にタイルを設置
            startX = (int) Mathf.Min(_selectedStartPos.x, _selectedPos.x);
            endX = (int) Mathf.Max(_selectedStartPos.x, _selectedPos.x);
            startY = (int) Mathf.Min(_selectedStartPos.y, _selectedPos.y);
            endY = (int) Mathf.Max(_selectedStartPos.y, _selectedPos.y);

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    tilemap.SetTile(new Vector3Int(x, y, _selectedZindex), AssetDatabase.LoadAssetAtPath<TileBase>(SelectedMapMassMultiImg));
                }
            }

            //描画を更新
            Render();
        }

        /// <summary>
        /// カーソル位置を削除する。
        /// </summary>
        public void RemoveCursorPos() {
            if (_selectedPos == Vector3Int.back)
            {
                return;
            }

            //変数初期化
            var tilemap = _tilemapLayers[(int) TilemapLayer.Type.Cursor].tilemap;
            int startX = (int) Mathf.Min(_selectedStartPos.x, _selectedPos.x);
            int endX = (int) Mathf.Max(_selectedStartPos.x, _selectedPos.x);
            int startY = (int) Mathf.Min(_selectedStartPos.y, _selectedPos.y);
            int endY = (int) Mathf.Max(_selectedStartPos.y, _selectedPos.y);

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    tilemap.SetTile(new Vector3Int(x, y, _selectedZindex), null);
                }
            }

            //初期化
            _selectedPos = Vector3Int.back;
            _selectedStartPos = Vector3Int.back;
            _drawStartPos = Vector3Int.back;
            _drawPos = Vector3Int.back;
            _selectedTiles = new List<ClipTileData>();
            _selectedShadowTiles = new List<ClipTileData>();

            //描画を更新
            Render();
        }

        /// <summary>
        /// カーソル位置を移動する。
        /// </summary>
        public void MoveCursorPos(Vector3Int cellPos) {
            //不正値を補正
            if (cellPos.x < 0)
            {
                cellPos.x = 0;
            }
            if (cellPos.y > 0)
            {
                cellPos.y = 0;
            }

            //座標に変化が無ければ処理終了
            if (_selectedPos == cellPos)
            {
                return;
            }
            //レイヤーが無ければ処理終了
            if (_tilemapLayers == null)
            {
                return;
            }

            //変数初期化
            var tilemap = _tilemapLayers[(int) TilemapLayer.Type.Cursor].tilemap;
            int startX = (int) Mathf.Min(_selectedStartPos.x, _selectedPos.x);
            int endX = (int) Mathf.Max(_selectedStartPos.x, _selectedPos.x);
            int startY = (int) Mathf.Min(_selectedStartPos.y, _selectedPos.y);
            int endY = (int) Mathf.Max(_selectedStartPos.y, _selectedPos.y);

            //直前までに描画していたものを破棄
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    tilemap.SetTile(new Vector3Int(x, y, _selectedZindex), null);
                }
            }

            //座標を移動する
            int diffX = cellPos.x - _selectedPos.x;
            int diffY = cellPos.y - _selectedPos.y;

            //現在の座標を保持
            _selectedPos = new Vector3Int(cellPos.x, cellPos.y, _selectedZindex);
            _selectedStartPos = new Vector3Int(_selectedStartPos.x + diffX, _selectedStartPos.y + diffY, _selectedZindex);

            //タイル描画中の場合は枠線は表示しない
            if (_isSpoitDrawMode) return;

            //範囲にタイルを設置
            startX = (int) Mathf.Min(_selectedStartPos.x, _selectedPos.x);
            endX = (int) Mathf.Max(_selectedStartPos.x, _selectedPos.x);
            startY = (int) Mathf.Min(_selectedStartPos.y, _selectedPos.y);
            endY = (int) Mathf.Max(_selectedStartPos.y, _selectedPos.y);

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    tilemap.SetTile(new Vector3Int(x, y, _selectedZindex), AssetDatabase.LoadAssetAtPath<TileBase>(SelectedMapMassMultiImg));
                }
            }

            //描画を更新
            Render();
        }

        /// <summary>
        /// 現在編集中のマップのレイヤー
        /// </summary>
        /// <param name="layers"></param>
        public void SetMapLayer(List<MapDataModel.Layer> layers) {
            _layers = layers;
        }

        /// <summary>
        /// タイルデータをスポイトする
        /// </summary>
        public void SpoitTileData() {
            //スポイトデータの初期化
            _selectedTiles = new List<ClipTileData>();
            _selectedShadowTiles = new List<ClipTileData>();

            //選択した座標
            int startX = (int) Mathf.Min(_selectedStartPos.x, _selectedPos.x);
            int endX = (int) Mathf.Max(_selectedStartPos.x, _selectedPos.x);
            int startY = (int) Mathf.Min(_selectedStartPos.y, _selectedPos.y);
            int endY = (int) Mathf.Max(_selectedStartPos.y, _selectedPos.y);

            if (_layers == null) return;

            //選択した範囲内のタイルデータを保持する
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    //各レイヤーのタイルを保持
                    foreach (var layer in _layers)
                    {
                        //影は別で実施
                        if (layer.type == MapDataModel.Layer.LayerType.Shadow)
                            continue;

                        ClipTileData data = new ClipTileData();
                        data.x = x - startX;
                        data.y = y - startY;
                        data.layer = layer;
                        //コピー対象が存在しない可能性があるため、try catch で括り、エラーだった場合には後続の処理を行わない
                        try
                        {
                            data.tileData = layer.GetTileDataModelByPosition(new Vector2(x, y));
                        } catch (Exception)
                        {
                            continue;
                        }
                        _selectedTiles.Add(data);
                    }
                }
            }

            //影データを保持する
            for (int x = startX * 2; x <= endX * 2 + 1; x++)
            {
                for (int y = startY * 2; y <= endY * 2 + 1; y++)
                {
                    //各レイヤーのタイルを保持
                    foreach (var layer in _layers)
                    {
                        //影以外は別で実施
                        if (layer.type != MapDataModel.Layer.LayerType.Shadow)
                            continue;

                        ClipTileData data = new ClipTileData();
                        data.x = x - (startX * 2);
                        data.y = y - (startY * 2);
                        data.layer = layer;
                        //コピー対象が存在しない可能性があるため、try catch で括り、エラーだった場合には後続の処理を行わない
                        try
                        {
                            data.tileData = layer.GetTileDataModelByPosition(new Vector2(x, y));
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        _selectedShadowTiles.Add(data);
                    }
                }
            }


            //選択領域の幅高さを保持
            _selectedX = endX - startX + 1;
            _selectedY = endY - startY + 1;
        }

        /// <summary>
        /// スポイトしたタイルを描画する
        /// </summary>
        public void BrushSpoitData(Vector3Int cellPos, bool flg = false) {
            if (cellPos.x < MapDataModel.width && cellPos.x >= 0 && cellPos.y > -MapDataModel.height &&
                cellPos.y <= 0 ||
                cellPos.x < MapDataModel.width * 2 && cellPos.x >= 0 && cellPos.y > -MapDataModel.height * 2 + 1 &&
                cellPos.y <= 1 && _currentTargetMapLayer.type == MapDataModel.Layer.LayerType.Shadow)
            {
                //座標に変化がなければ処理終了
                if (_drawPos == cellPos)
                {
                    return;
                }
                //タイルデータが無ければ処理終了
                if (_selectedTiles == null || _selectedTiles.Count == 0)
                {
                    return;
                }

                //座標を保持
                _drawPos = cellPos;
                if (_drawStartPos == Vector3Int.back)
                {
                    _drawStartPos = cellPos;
                }

                //該当の箇所に塗るタイルを決定する
                //起点とする座標
                int startX = (int) Mathf.Min(_selectedStartPos.x, _selectedPos.x);
                int startY = (int) Mathf.Min(_selectedStartPos.y, _selectedPos.y);

                int subX = _drawPos.x - _drawStartPos.x;
                int subY = _drawPos.y - _drawStartPos.y;

                //タイルデータは、選択範囲の左下を起点に詰めており、
                //塗るときは選択範囲の終了地点が起点になるため、
                //選択範囲の終了地点と、選択範囲の左下起点の差分の補正を行う
                if (!flg)
                {
                    if (_selectedPos.x > _selectedStartPos.x)
                    {
                        subX += (_selectedPos.x - _selectedStartPos.x);
                    }
                    if (_selectedPos.y > _selectedStartPos.y)
                    {
                        subY += (_selectedPos.y - _selectedStartPos.y);
                    }
                }

                //配列の範囲内に丸める
                if (subX < 0)
                {
                    while (subX < 0) subX += _selectedX;
                }
                if (subX >= _selectedX)
                {
                    while (subX >= _selectedX) subX -= _selectedX;
                }
                if (subY < 0)
                {
                    while (subY < 0) subY += _selectedY;
                }
                if (subY >= _selectedY)
                {
                    while (subY >= _selectedY) subY -= _selectedY;
                }

                //subX, subYの位置にあるタイルを塗る
                for (int i = 0; i < _selectedTiles.Count; i++)
                {
                    if (_selectedTiles[i].x == subX && _selectedTiles[i].y == subY)
                    {
                        var layer = _currentTargetMapLayer;
                        _currentTargetMapLayer = _selectedTiles[i].layer;
                        SetTile(cellPos, _selectedTiles[i].tileData);
                        _currentTargetMapLayer = layer;
                    }
                }

                //subX, subYの位置にある影を最大4タイル分塗る
                for (int i = 0; i < _selectedShadowTiles.Count; i++)
                {
                    if (_selectedShadowTiles[i].x == subX * 2 && _selectedShadowTiles[i].y == subY * 2)
                    {
                        _selectedShadowTiles[i].layer.tilemap.SetTile(new Vector3Int(cellPos.x * 2, cellPos.y * 2), _selectedShadowTiles[i].tileData);
                    }
                    else if (_selectedShadowTiles[i].x == subX * 2 + 1 && _selectedShadowTiles[i].y == subY * 2)
                    {
                        _selectedShadowTiles[i].layer.tilemap.SetTile(new Vector3Int(cellPos.x * 2 + 1, cellPos.y * 2), _selectedShadowTiles[i].tileData);
                    }
                    if (_selectedShadowTiles[i].x == subX * 2 && _selectedShadowTiles[i].y == subY * 2 + 1)
                    {
                        _selectedShadowTiles[i].layer.tilemap.SetTile(new Vector3Int(cellPos.x * 2, cellPos.y * 2 + 1), _selectedShadowTiles[i].tileData);
                    }
                    else if (_selectedShadowTiles[i].x == subX * 2 + 1 && _selectedShadowTiles[i].y == subY * 2 + 1)
                    {
                        _selectedShadowTiles[i].layer.tilemap.SetTile(new Vector3Int(cellPos.x * 2 + 1, cellPos.y * 2 + 1), _selectedShadowTiles[i].tileData);
                    }
                }

                //描画を更新
                Render();
            }
        }

        /// <summary>
        /// UNDO/REDOした時の処理
        /// セーブして、Renderしなおす
        /// </summary>
        protected virtual void RenderAndSave() {
            if (MapDataModel == null)
            {
                Undo.undoRedoPerformed -= RenderAndSave;
                return;
            }
            MapEditor.SaveMap(MapDataModel, MapRepository.SaveType.SAVE_PREFAB);
            UnityEditorWrapper.AssetDatabaseWrapper.SaveAssets();
            Render();
        }
    }
}