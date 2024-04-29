using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement.Repository;
using RPGMaker.Codebase.Editor.MapEditor.Window.EventEdit;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.Map.View
{
    /// <summary>
    /// [マップ設定]-[マップリスト]-[各マップ] 内のInspector表示の制御クラス
    /// </summary>
    public class MapInspectorView : AbstractInspectorElement
    {
        private readonly VisualElement _rootVisualElement;
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Map/Asset/inspector_mapEditor.uxml"; } }
        
        private PenButtonMenu _penButtonMenu;
        public PenButtonMenu GetPenButtonMenu() { return _penButtonMenu; }
        
        private enum ViewType
        {
            NONE,
            MAP_ENTITY,
            DISTANT_VIEW,
            BACKGROUND_VIEW,
            BACKGROUND_COLLISION_VIEW,
            TILE_ENTITY,
            EVENT_ENTITY
        }
        private ViewType _type = ViewType.NONE;

        private MapInspector _mapInspector;
        private DistantViewInspector _distantViewInspector;
        private BackgroundViewInspector _backgroundViewInspector;
        private BackgroundCollisionInspector _backgroundCollisionInspector;
        private TileInspector _tileInspector;
        private EventInspector _eventInspector;

        public MapInspectorView() {
            _rootVisualElement = this;
            _type = ViewType.NONE;
            InitializeData();
            Clear();
        }

        private void InitializeData() {
            _mapInspector = null;
            _distantViewInspector = null;
            _backgroundViewInspector = null;
            _backgroundCollisionInspector = null;
            _tileInspector = null;
            _eventInspector = null;
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            switch (_type)
            {
                case ViewType.MAP_ENTITY:
                    _mapInspector?.Refresh();
                    break;
                case ViewType.DISTANT_VIEW:
                    _distantViewInspector?.Refresh();
                    break;
                case ViewType.BACKGROUND_VIEW:
                    _backgroundViewInspector?.Refresh();
                    break;
                case ViewType.BACKGROUND_COLLISION_VIEW:
                    _backgroundCollisionInspector?.Refresh();
                    break;
                case ViewType.TILE_ENTITY:
                    _tileInspector?.Refresh();
                    break;
                case ViewType.EVENT_ENTITY:
                    _eventInspector?.Refresh();
                    break;
            }
        }

        public void SetMapEntity(MapDataModel mapDataModel, bool isSampleMap) {
            //初期化処理
            InitializeData();
            _type = ViewType.MAP_ENTITY;

            _rootVisualElement.Clear();
            _penButtonMenu = new PenButtonMenu(PenButtonMenu.EditTarget.Map);
            var scrollView = new ScrollView();
            _rootVisualElement.Add(scrollView);
            if (!isSampleMap)
                scrollView.Add(_penButtonMenu.GetPenButtonMenuElement());

            _mapInspector = new MapInspector(mapDataModel, isSampleMap, map =>
            {
                MapEditor.MapEditor.SaveMap(map, MapRepository.SaveType.NO_PREFAB);
            });
            scrollView.Add(_mapInspector);
        }

        public void SetDistantView(MapDataModel mapEntity) {
            //初期化処理
            InitializeData();
            _type = ViewType.DISTANT_VIEW;

            _rootVisualElement.Clear();
            _distantViewInspector = new DistantViewInspector(mapEntity, map =>
            {
                MapEditor.MapEditor.SaveMap(map, MapRepository.SaveType.NO_PREFAB);
            });
            _rootVisualElement.Add(_distantViewInspector);
        }

        public void SetBackgroundView(MapDataModel mapEntity) {
            //初期化処理
            InitializeData();
            _type = ViewType.BACKGROUND_VIEW;

            _rootVisualElement.Clear();
            _backgroundViewInspector = new BackgroundViewInspector(mapEntity, map =>
            {
                MapEditor.MapEditor.SaveMap(map, MapRepository.SaveType.NO_PREFAB);
            });
            _rootVisualElement.Add(_backgroundViewInspector);
        }

        public void SetBackgroundCollisionView(TileDataModel tileDataModel) {
            //初期化処理
            InitializeData();
            _type = ViewType.BACKGROUND_COLLISION_VIEW;

            _rootVisualElement.Clear();
            var penButtonMenu = new PenButtonMenu(PenButtonMenu.EditTarget.Map);
            _rootVisualElement.Add(penButtonMenu.GetPenButtonMenuElement());

            _backgroundCollisionInspector = new BackgroundCollisionInspector(tileDataModel, async () =>
            {
                await MapEditor .MapEditor.SaveTile(tileDataModel);
                MapEditor.MapEditor.ReloadTiles();
            });
            _rootVisualElement.Add(_backgroundCollisionInspector);
        }

        public void SetTileEntity(TileDataModel tileDataModel, TileInspector.TYPE inspectorType) {
            //初期化処理
            InitializeData();
            _type = ViewType.TILE_ENTITY;

            _rootVisualElement.Clear();
            _tileInspector = new TileInspector(tileDataModel, async () =>
            {
                await MapEditor .MapEditor.SaveTile(tileDataModel);
                MapEditor.MapEditor.ReloadTiles();
            }, inspectorType);
            _rootVisualElement.Add(_tileInspector);
        }

        public void SetEventEntity(
            EventMapDataModel eventMapDataModelList,
            FlagDataModel flags,
            List<ItemDataModel> items,
            List<WeaponDataModel> weapons,
            List<ArmorDataModel> armors,
            List<CharacterActorDataModel> actors,
            int pageNum,
            EventEditWindow element,
            MapDataModel mapDataModel
        )
        {
            //初期化処理
            InitializeData();
            _type = ViewType.EVENT_ENTITY;

            _eventInspector = new EventInspector(eventMapDataModelList, flags, items, weapons, armors, actors, pageNum, element, mapDataModel);
            _rootVisualElement.Clear();
            _rootVisualElement.Add(_eventInspector);
        }
    }
}