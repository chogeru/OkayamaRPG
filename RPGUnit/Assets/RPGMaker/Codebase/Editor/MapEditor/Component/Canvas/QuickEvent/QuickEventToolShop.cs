using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Common;
using RPGMaker.Codebase.Editor.MapEditor.Component.Canvas.QuickEvent.ModalWindow;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.Canvas.QuickEvent
{
    public class QuickEventToolShop : QuickEventEdit
    {
        //各種メッセージ置き場
        private const string         SALESPERSON_TEXT = "WORD_3106";
        private       EventDataModel _eventDataModel;

        private EventManagementService _eventManagementService;
        private EventMapDataModel      _eventMapDataModel;

        public void Init(
            EventEditCanvas eventEditCanvas,
            List<EventMapDataModel> eventList,
            Vector3Int pos,
            MapDataModel mapDataModel
        ) {
            //簡単選択ポップアップ
            //-------------------------------------------------------------
            var QuickeventToolShopModalwindow = new QuickeventToolShopModalwindow();
            QuickeventToolShopModalwindow.ShowWindow(
                eventEditCanvas,
                eventList,
                mapDataModel,
                EditorLocalize.LocalizeWindowTitle("WORD_1578"),
                data =>
                {
                    // 0.sdName 1.売却可能か 2.陳列する品物データのList
                    var modalValue = (List<object>) data;

                    if (data != null)
                    {
                        //イベントデータの新規作成
                        _eventManagementService = new EventManagementService();
                        _eventMapDataModel = MapEditor.CreateEvent(mapDataModel, pos.x, pos.y);
                        _eventDataModel = _eventManagementService.LoadEventById(_eventMapDataModel.eventId);

                        // テキスト
                        //-------------------------------------------------------------
                        // テキスト設定
                        _eventDataModel.eventCommands.Add(
                            new EventDataModel.EventCommand(
                                (int) EventEnum.EVENT_CODE_MESSAGE_TEXT,
                                new List<string>
                                    {"0", "0", "0", "0", "2", EditorLocalize.LocalizeText("WORD_0092"), "", "", ""},
                                new List<EventDataModel.EventCommandMoveRoute>()
                            ));

                        // テキスト文章
                        _eventDataModel.eventCommands.Add(
                            new EventDataModel.EventCommand(
                                (int) EventEnum.EVENT_CODE_MESSAGE_TEXT_ONE_LINE,
                                new List<string> {EditorLocalize.LocalizeText(SALESPERSON_TEXT), ""},
                                new List<EventDataModel.EventCommandMoveRoute>()
                            ));

                        //ショップの部分
                        _eventDataModel.eventCommands.Add(
                            new EventDataModel.EventCommand(
                                (int) EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG,
                                new List<string> {"0", "0", "0", "0", modalValue[1].ToString()},
                                new List<EventDataModel.EventCommandMoveRoute>()
                            ));

                        //商品陳列(棚出し分回す)
                        var items = (List<object>) modalValue[2];
                        foreach (var item in items)
                        {
                            var itemData = (List<string>) item;
                            _eventDataModel.eventCommands.Add(
                                new EventDataModel.EventCommand(
                                    (int) EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG_LINE,
                                    new List<string> {itemData[0], itemData[1], itemData[2], itemData[3]},
                                    new List<EventDataModel.EventCommandMoveRoute>()
                                ));
                        }

                        // 空行
                        //-------------------------------------------------------------
                        _eventDataModel.eventCommands.Add(
                            new EventDataModel.EventCommand(
                                0,
                                new List<string>(),
                                new List<EventDataModel.EventCommandMoveRoute>()
                            ));
                        _eventMapDataModel.pages[0].image.sdName = modalValue[0].ToString();
                        _eventMapDataModel.pages[0].condition.image.enabled = 1;

                        _eventManagementService.SaveEventMap(_eventMapDataModel);
                        _eventManagementService.SaveEvent(_eventDataModel);
                        MapEditor.ReloadMaps();
                        // ヒエラルキーの該当イベントページを選択状態にする。
                        Hierarchy.Hierarchy.SelectButton(
                            CommonMapHierarchyView.GetEventPageButtonName(_eventMapDataModel.eventId, 0));
                    }
                });
        }
    }
}