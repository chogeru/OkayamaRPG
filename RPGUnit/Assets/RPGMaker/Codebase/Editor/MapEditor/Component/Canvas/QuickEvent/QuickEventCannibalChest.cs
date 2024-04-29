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
    public class QuickEventCannibalChest : QuickEventEdit
    {
        //表示セリフ
        private const string         CANNIBAL_CHEST_TEXT = "WORD_3105";
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
            var QuickeventCannibalChestModalwindow = new QuickeventCannibalChestModalwindow();
            QuickeventCannibalChestModalwindow.ShowWindow(
                eventEditCanvas,
                eventList,
                mapDataModel,
                EditorLocalize.LocalizeWindowTitle("WORD_1577"),
                data =>
                {
                    // 0.sdName 1.エンカウント方針の選択番号 2.変数、敵グループのID 3.敗北可能か 4.逃走可能か 
                    var modalValue = (List<object>) data;

                    if (data != null)
                    {
                        _eventManagementService = new EventManagementService();
                        _eventMapDataModel = MapEditor.CreateEvent(mapDataModel, pos.x, pos.y);
                        _eventDataModel = _eventManagementService.LoadEventById(_eventMapDataModel.eventId);

                        // SE再生
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

                        //宝箱が開くアニメーション
                        //-------------------------------------------------------------
                        //左を向く
                        _eventDataModel.eventCommands.Add(
                            new EventDataModel.EventCommand(
                                (int) EventEnum.MOVEMENT_TURN_LEFT,
                                new List<string> {"-1", "0", "0"},
                                new List<EventDataModel.EventCommandMoveRoute>()
                            ));
                        //少し待つ
                        _eventDataModel.eventCommands.Add(
                            new EventDataModel.EventCommand(
                                (int) EventEnum.EVENT_CODE_TIMING_WAIT,
                                new List<string> {"3"},
                                new List<EventDataModel.EventCommandMoveRoute>()
                            ));
                        //右を向く
                        _eventDataModel.eventCommands.Add(
                            new EventDataModel.EventCommand(
                                (int) EventEnum.MOVEMENT_TURN_RIGHT,
                                new List<string> {"-1", "0", "0"},
                                new List<EventDataModel.EventCommandMoveRoute>()
                            ));
                        //少し待つ
                        _eventDataModel.eventCommands.Add(
                            new EventDataModel.EventCommand(
                                (int) EventEnum.EVENT_CODE_TIMING_WAIT,
                                new List<string> {"3"},
                                new List<EventDataModel.EventCommandMoveRoute>()
                            ));

                        //セルフスイッチのセット
                        //-------------------------------------------------------------
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
                                new List<string>
                                    {"0", "0", "0", "0", "2", EditorLocalize.LocalizeText("WORD_0092"), "", "", ""},
                                new List<EventDataModel.EventCommandMoveRoute>()
                            ));

                        // テキスト文章
                        _eventDataModel.eventCommands.Add(
                            new EventDataModel.EventCommand(
                                (int) EventEnum.EVENT_CODE_MESSAGE_TEXT_ONE_LINE,
                                new List<string> {EditorLocalize.LocalizeText(CANNIBAL_CHEST_TEXT), ""},
                                new List<EventDataModel.EventCommandMoveRoute>()
                            ));

                        //戦闘の処理の部分
                        //-------------------------------------------------------------
                        _eventDataModel.eventCommands.Add(
                            new EventDataModel.EventCommand(
                                (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG,
                                //初期値ランダムエンカウント
                                new List<string> {"2", "0", "0", "0"},
                                new List<EventDataModel.EventCommandMoveRoute>()
                            ));

                        //条件によっては出てくる
                        if (modalValue[3].ToString() == "1" || modalValue[4].ToString() == "1")
                        {
                            //勝った時
                            //-------------------------------------------------------------
                            _eventDataModel.eventCommands.Add(
                                new EventDataModel.EventCommand(
                                    (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_WIN,
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
                        }

                        //逃走可能時
                        if (modalValue[4].ToString() == "1")
                        {
                            //逃げる時
                            //-------------------------------------------------------------
                            _eventDataModel.eventCommands.Add(
                                new EventDataModel.EventCommand(
                                    (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_ESCAPE,
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
                        }

                        //敗北可能時
                        if (modalValue[3].ToString() == "1")
                        {
                            //逃げる時
                            //-------------------------------------------------------------
                            _eventDataModel.eventCommands.Add(
                                new EventDataModel.EventCommand(
                                    (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_LOSE,
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
                        }

                        //条件によっては出てくる
                        if (modalValue[3].ToString() == "1" || modalValue[4].ToString() == "1")
                            //条件終了時
                            //-------------------------------------------------------------
                            _eventDataModel.eventCommands.Add(
                                new EventDataModel.EventCommand(
                                    (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END,
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

                        // 空行
                        //-------------------------------------------------------------
                        _eventDataModel.eventCommands.Add(
                            new EventDataModel.EventCommand(
                                0,
                                new List<string>(),
                                new List<EventDataModel.EventCommandMoveRoute>()
                            ));
                        _eventMapDataModel.pages[0].image.sdName = modalValue[0]?.ToString();
                        _eventMapDataModel.pages[0].condition.image.enabled = 1;

                        // イベント設定
                        foreach (var command in _eventDataModel.eventCommands)
                            if (command.code == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG)
                            {
                                command.parameters[0] = modalValue[1].ToString();
                                command.parameters[1] = modalValue[2].ToString();
                                command.parameters[2] = modalValue[3].ToString();
                                command.parameters[3] = modalValue[4].ToString();
                            }

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
                        _eventMapDataModel.pages[1].walk.directionFix = 2;

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