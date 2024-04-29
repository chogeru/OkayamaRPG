using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.Editor.MapEditor.Component.Canvas.QuickEvent;
using RPGMaker.Codebase.Editor.MapEditor.Window.EventEdit;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.Canvas
{
    public class QuickEventEdit
    {
        private QuickEventCannibalChest _quickEventCannibalChest;
        private QuickEventDoor          _quickEventDoor;
        private QuickEventInn           _quickEventInn;
        private QuickEventMovement      _quickEventMovement;
        private QuickEventSavePoint     _quickEventSavePoint;
        private QuickEventToolShop      _quickEventToolShop;
        private QuickEventTreasureChest _quickEventTreasureChest;

        public void Init(
            Vector3Int pos,
            EventEditCanvas eventEditCanvas,
            EventEditWindow eventEditWindow,
            EventEditCanvas.QuickEventContentsEnum QuickEventContents,
            List<EventMapDataModel> eventList,
            MapDataModel mapDataModel
        ) {
            _quickEventDoor = new QuickEventDoor();
            _quickEventMovement = new QuickEventMovement();
            _quickEventSavePoint = new QuickEventSavePoint();
            _quickEventTreasureChest = new QuickEventTreasureChest();
            _quickEventCannibalChest = new QuickEventCannibalChest();
            _quickEventToolShop = new QuickEventToolShop();
            _quickEventInn = new QuickEventInn();

            switch (QuickEventContents)
            {
                //ドア
                case (int) EventEditCanvas.QuickEventContentsEnum.door:
                    _quickEventDoor.Init(eventEditCanvas, eventList, pos, mapDataModel);
                    break;
                //移動
                case EventEditCanvas.QuickEventContentsEnum.movement:
                    _quickEventMovement.Init(eventEditCanvas, eventList, pos, mapDataModel);
                    break;
                //セーブポイント
                case EventEditCanvas.QuickEventContentsEnum.savePoint:
                    _quickEventSavePoint.Init(eventEditCanvas, eventList, pos, mapDataModel);
                    break;
                //宝箱
                case EventEditCanvas.QuickEventContentsEnum.treasureChest:
                    _quickEventTreasureChest.Init(eventEditCanvas, eventList, pos, mapDataModel);
                    break;
                //人食い箱
                case EventEditCanvas.QuickEventContentsEnum.cannibalChest:
                    _quickEventCannibalChest.Init(eventEditCanvas, eventList, pos, mapDataModel);
                    break;
                //道具屋
                case EventEditCanvas.QuickEventContentsEnum.toolShop:
                    _quickEventToolShop.Init(eventEditCanvas, eventList, pos, mapDataModel);
                    break;
                //宿屋
                case EventEditCanvas.QuickEventContentsEnum.inn:
                    _quickEventInn.Init(eventEditCanvas, eventList, pos, mapDataModel);
                    break;
            }
        }
    }
}