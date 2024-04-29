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
    public class QuickEventSavePoint : QuickEventEdit
    {
        //各種メッセージ置き場
        private const string         SAVE_TEXT_0         = "WORD_3100";
        private const string         SAVE_TEXT_1         = "WORD_3101";
        private const string         SELECT_SELECTED_NO  = "WORD_3102";
        private const string         SELECT_SELECTED_YES = "WORD_3103";
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
            var QuickeventSavePointModalwindow = new QuickeventSavePointModalwindow();
            QuickeventSavePointModalwindow.ShowWindow(
                eventEditCanvas,
                eventList,
                mapDataModel,
                EditorLocalize.LocalizeWindowTitle("WORD_1575"),
                data =>
                {
                    // 0.sdName 
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
                                new List<string> {EditorLocalize.LocalizeText(SAVE_TEXT_0), ""},
                                new List<EventDataModel.EventCommandMoveRoute>()
                            ));

                        // テキスト文章
                        _eventDataModel.eventCommands.Add(
                            new EventDataModel.EventCommand(
                                (int) EventEnum.EVENT_CODE_MESSAGE_TEXT_ONE_LINE,
                                new List<string> {EditorLocalize.LocalizeText(SAVE_TEXT_1), ""},
                                new List<EventDataModel.EventCommandMoveRoute>()
                            ));

                        // 選択肢
                        _eventDataModel.eventCommands.Add(
                            new EventDataModel.EventCommand(
                                (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT,
                                new List<string> {"2", "0", "0", "0", "1"},
                                new List<EventDataModel.EventCommandMoveRoute>()
                            ));

                        // 選択肢(はい)
                        _eventDataModel.eventCommands.Add(
                            new EventDataModel.EventCommand(
                                (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_SELECTED,
                                new List<string> {"1", "0", EditorLocalize.LocalizeText(SELECT_SELECTED_YES)},
                                new List<EventDataModel.EventCommandMoveRoute>()
                            ));

                        // セーブ画面起動
                        _eventDataModel.eventCommands.Add(
                            new EventDataModel.EventCommand(
                                (int) EventEnum.EVENT_CODE_SCENE_SAVE_OPEN,
                                new List<string>(),
                                new List<EventDataModel.EventCommandMoveRoute>()
                            ));

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

                        // 選択肢(いいえ)
                        _eventDataModel.eventCommands.Add(
                            new EventDataModel.EventCommand(
                                (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_SELECTED,
                                new List<string> {"2", "0", EditorLocalize.LocalizeText(SELECT_SELECTED_NO)},
                                new List<EventDataModel.EventCommandMoveRoute>()
                            ));

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

                        // 選択肢(キャンセル)
                        _eventDataModel.eventCommands.Add(
                            new EventDataModel.EventCommand(
                                (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_END,
                                new List<string> {"-1", "0", " "},
                                new List<EventDataModel.EventCommandMoveRoute>()
                            ));

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