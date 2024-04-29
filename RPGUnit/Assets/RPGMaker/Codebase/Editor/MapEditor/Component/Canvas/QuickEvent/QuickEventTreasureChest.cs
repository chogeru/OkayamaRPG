using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Common;
using RPGMaker.Codebase.Editor.MapEditor.Component.Canvas.QuickEvent.ModalWindow;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.Canvas.QuickEvent
{
    public class QuickEventTreasureChest : QuickEventEdit
    {
        // $に金額を入れる
        private const string         TREASURE_CHEST_TEXT = "WORD_3104";
        private       EventDataModel _eventDataModel;

        private EventManagementService _eventManagementService;
        private EventMapDataModel      _eventMapDataModel;

        public void Init(
            EventEditCanvas eventEditCanvas,
            List<EventMapDataModel> eventList,
            Vector3Int pos,
            MapDataModel mapDataModel
        ) {
            _eventManagementService = new EventManagementService();

            //簡単選択ポップアップ
            //-------------------------------------------------------------
            var QuickeventTreasureChestModalwindow = new QuickeventTreasureChestModalwindow();
            QuickeventTreasureChestModalwindow.ShowWindow(
                eventEditCanvas,
                eventList,
                mapDataModel,
                EditorLocalize.LocalizeWindowTitle("WORD_1576"),
                data =>
                {
                    // 0.sdName 1.選択番号 2.金額 3.アイテムID 4.武器ID 5.防具ID 6.アイテム名
                    var modalValue = (List<object>) data;

                    if (data != null)
                    {
                        //イベントデータの作成
                        _eventMapDataModel = MapEditor.CreateEvent(mapDataModel, pos.x, pos.y);
                        _eventDataModel = _eventManagementService.LoadEventById(_eventMapDataModel.eventId);

                        //イベント設定 (文章、条件分岐、所持金増減が返る)
                        var eventCommand = SetEvent();
                        _eventMapDataModel.pages[0].image.sdName = modalValue[0]?.ToString();
                        _eventMapDataModel.pages[0].condition.image.enabled = 1;
                        //イベント設定
                        SetEventItem(modalValue);
                        //テキスト設定
                        eventCommand.parameters[0] = ("$" + EditorLocalize.LocalizeText(TREASURE_CHEST_TEXT))
                            .Replace("$", modalValue[6].ToString());

                        //ページを増やす
                        MapEditor.CreatePage(_eventMapDataModel, 1, 1);
                        //宝箱が開いた後のデータを設定
                        //画像設定
                        _eventMapDataModel.pages[1].image.sdName = modalValue[0]?.ToString();
                        _eventMapDataModel.pages[1].condition.image.enabled = 1;
                        //セルフスイッチA=1が条件
                        _eventMapDataModel.pages[1].condition.selfSwitch.enabled = 1;
                        _eventMapDataModel.pages[1].condition.selfSwitch.selfSwitch = "A";
                        //向きは右固定
                        _eventMapDataModel.pages[1].walk.direction = 2;
                        _eventMapDataModel.pages[1].walk.directionFix = 1;

                        //データ保存
                        _eventManagementService.SaveEventMap(_eventMapDataModel);
                        _eventManagementService.SaveEvent(_eventDataModel);

                        MapEditor.ReloadMaps();
                        // ヒエラルキーの該当イベントページを選択状態にする。
                        Hierarchy.Hierarchy.SelectButton(
                            CommonMapHierarchyView.GetEventPageButtonName(_eventMapDataModel.eventId, 0));
                    }
                });
        }

        //-------------------------------------------------------------
        // イベントの設定処理
        //-------------------------------------------------------------
        private EventDataModel.EventCommand SetEvent() {
            // 返り値用
            EventDataModel.EventCommand eventCommands;

            // ME再生
            //-------------------------------------------------------------
            //サウンドファイルの取得部分
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            var path = "Assets/RPGMaker/Storage/Sounds/SE/";
            var dir = new DirectoryInfo(path);
            var info = dir.GetFiles("*.ogg");
            var fileNames = new List<string>();
            foreach (var f in info) fileNames.Add(f.Name.Replace(".ogg", ""));
            info = dir.GetFiles("*.wav");
            foreach (var f in info) fileNames.Add(f.Name.Replace(".wav", ""));
            // 初期値があるか確認
            var defaultValue = fileNames[0];
            if (fileNames.Contains("Chest1"))
                defaultValue = "Chest1";

            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_AUDIO_SE_PLAY,
                    new List<string> {defaultValue, "90", "100", "0"},
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // 向き変更
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.MOVEMENT_TURN_LEFT,
                    new List<string>
                    {
                        "-1",
                        "0",
                        "0"
                    },
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // Wait
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_TIMING_WAIT,
                    new List<string>
                    {
                        "3"
                    },
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // 向き変更
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.MOVEMENT_TURN_RIGHT,
                    new List<string>
                    {
                        "-1",
                        "0",
                        "0"
                    },
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // Wait
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_TIMING_WAIT,
                    new List<string>
                    {
                        "3"
                    },
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // セルフスイッチAを有効にする
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_GAME_SELF_SWITCH,
                    new List<string> {"A", "1", "0"},
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // テキスト
            //-------------------------------------------------------------
            // テキスト設定
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_MESSAGE_TEXT,
                    new List<string> {"0", "0", "0", "0", "2", EditorLocalize.LocalizeText("WORD_0092"), "", "", ""},
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // テキスト文章
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    (int) EventEnum.EVENT_CODE_MESSAGE_TEXT_ONE_LINE,
                    new List<string> {("$" + EditorLocalize.LocalizeText(TREASURE_CHEST_TEXT)).Replace("$", ""), "2203"},
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));

            // 返り値配列に追加
            eventCommands = _eventDataModel.eventCommands[_eventDataModel.eventCommands.Count - 1];

            return eventCommands;
        }

        // アイテム等追加イベントの設定
        private void SetEventItem(List<object> data) {
            // 選択番号によって処理を分ける
            switch (data[1])
            {
                // ゴールド
                case 0:
                    // 所持金増減
                    //-------------------------------------------------------------
                    _eventDataModel.eventCommands.Add(
                        new EventDataModel.EventCommand(
                            (int) EventEnum.EVENT_CODE_PARTY_GOLD,
                            new List<string> {"0", "0", data[2].ToString()},
                            new List<EventDataModel.EventCommandMoveRoute>()
                        ));
                    break;
                // アイテム
                case 1:
                    // アイテム増減
                    //-------------------------------------------------------------
                    _eventDataModel.eventCommands.Add(
                        new EventDataModel.EventCommand(
                            (int) EventEnum.EVENT_CODE_PARTY_ITEM,
                            new List<string> {data[3].ToString(), "0", "0", "1"},
                            new List<EventDataModel.EventCommandMoveRoute>()
                        ));
                    break;
                // 武器
                case 2:
                    // 武器増減
                    //-------------------------------------------------------------
                    _eventDataModel.eventCommands.Add(
                        new EventDataModel.EventCommand(
                            (int) EventEnum.EVENT_CODE_PARTY_WEAPON,
                            new List<string> {data[4].ToString(), "0", "0", "1", "0"},
                            new List<EventDataModel.EventCommandMoveRoute>()
                        ));
                    break;
                // 防具
                case 3:
                    // 防具増減
                    //-------------------------------------------------------------
                    _eventDataModel.eventCommands.Add(
                        new EventDataModel.EventCommand(
                            (int) EventEnum.EVENT_CODE_PARTY_ARMS,
                            new List<string> {data[5].ToString(), "0", "0", "1", "0"},
                            new List<EventDataModel.EventCommandMoveRoute>()
                        ));
                    break;
            }

            // 空行
            //-------------------------------------------------------------
            _eventDataModel.eventCommands.Add(
                new EventDataModel.EventCommand(
                    0,
                    new List<string>(),
                    new List<EventDataModel.EventCommandMoveRoute>()
                ));
        }
    }
}