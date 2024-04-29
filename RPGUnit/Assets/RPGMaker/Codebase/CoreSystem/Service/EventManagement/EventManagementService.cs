using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventBattle;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventCommon;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement.Repository;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Service.EventManagement
{
    public class EventManagementService
    {
        private static EventRepository       _eventRepository;
        private static EventMapRepository    _eventMapRepository;
        private static EventCommonRepository _eventCommonRepository;
        private static EventBattleRepository _eventBattleRepository;

        public EventManagementService() {
            _eventRepository = new EventRepository();
            _eventMapRepository = new EventMapRepository();
            _eventCommonRepository = new EventCommonRepository();
            _eventBattleRepository = new EventBattleRepository();
        }

        public List<EventMapDataModel> LoadEventMapEntitiesByMapId(string mapId) {
            return _eventMapRepository.LoadEventMapEntitiesByMapIdFromJson(mapId);
        }

        public void SaveEvent(EventDataModel evt) {
            _eventRepository.Save(evt);
        }

        public void SaveEventCommon(EventCommonDataModel evt) {
            _eventCommonRepository.Save(evt);
        }

        public void SaveEventMap() {
            _eventMapRepository.Save();
        }

        public void SaveEventMap(EventMapDataModel eventMapDataModel) {
            _eventMapRepository.Save(eventMapDataModel);
        }

        public void SaveEventBattle(List<EventBattleDataModel> eventBattleDataModels) {
            _eventBattleRepository.Save(eventBattleDataModels);
        }

        public void DeleteEventMap(EventMapDataModel eventMapDataModel) {
            _eventMapRepository.Delete(eventMapDataModel);
        }

        public void DeleteEvent(EventDataModel evt) {
            _eventRepository.Delete(evt);
        }

        public void PageStuffing(EventDataModel evt) {
            _eventRepository.PageStuffing(evt);
        }

        public void DeleteCommonEvent(EventCommonDataModel evt) {
            _eventCommonRepository.Delete(evt);
        }

        public List<EventDataModel> LoadEvent() {
            return _eventRepository.Load();
        }

        public EventDataModel LoadEventById(string eventId, int page = 0) {
            return _eventRepository.LoadEventById(eventId, page);
        }

        public List<EventDataModel> LoadEventsById(string eventId) {
            return _eventRepository.Load().FindAll(item => item.id == eventId);
        }

        public List<EventCommonDataModel> LoadEventCommon() {
            return _eventCommonRepository.Load();
        }

        public List<EventMapDataModel> LoadEventMap() {
            return _eventMapRepository.Load();
        }

        public List<EventBattleDataModel> LoadEventBattle() {
            return _eventBattleRepository.Load();
        }

        public void ResetEventBattle() {
            _eventBattleRepository.Reset();
        }

        public void SetEventIndent(EventDataModel eventData) {
            //渡されたイベントリスト内の全コマンドにindentを設定する
            var indent = 0;
            var eventCode = 0;
            for (var i = 0; i < eventData.eventCommands.Count; i++)
            {
                //現在のイベントコード
                eventCode = eventData.eventCommands[i].code;

                //コマンドの内容が、以下の場合は、現在のインデント-1を設定する
                //・選択肢の分岐、終了
                //・条件文の、else文、endif文
                //・ループ終了
                //・バトルの、勝利した/敗北した/逃走した/終了
                if (eventCode == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_SELECTED || //選択肢の分岐
                    eventCode == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_CANCELED || //選択肢の分岐（キャンセル）
                    eventCode == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_END || //選択肢の終了
                    eventCode == (int) EventEnum.EVENT_CODE_FLOW_ELSE || //条件文の、else文
                    eventCode == (int) EventEnum.EVENT_CODE_FLOW_ENDIF || //条件文終了
                    eventCode == (int) EventEnum.EVENT_CODE_FLOW_LOOP_END || //ループ終了
                    eventCode == (int) EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE_END || //カスタム移動終了
                    eventCode == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_WIN || //バトル勝利
                    eventCode == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_LOSE || //バトル敗北
                    eventCode == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_ESCAPE || //バトル逃走
                    eventCode == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END //バトル終了
                )
                {
                    //現在のインデント-1を設定する
                    eventData.eventCommands[i].indent = indent - 1;

                    //終了系は、インデント自体を-1する
                    if (eventCode == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_END || //選択肢の終了
                        eventCode == (int) EventEnum.EVENT_CODE_FLOW_ENDIF || //条件文終了
                        eventCode == (int) EventEnum.EVENT_CODE_FLOW_LOOP_END || //ループ終了
                        eventCode == (int) EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE_END || //カスタム移動終了
                        eventCode == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END //バトル終了
                    )
                        indent--;
                }
                //T4U処理の内部的に、インデントとは異なるものの、1段下げておきたいイベントについては、indentは変えずに
                //該当のイベントのインデントだけをずらす
                //・メッセージの文章
                //・ショップ
                else if (eventCode == (int) EventEnum.EVENT_CODE_MESSAGE_TEXT_SCROLL_ONE_LINE || //メッセージのスクロールの本文
                         eventCode == (int) EventEnum.EVENT_CODE_MESSAGE_TEXT_ONE_LINE || //文章表示の本文
                         eventCode == (int) EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG_LINE //ショップの購入物
                )
                {
                    //現在のインデント+1を設定する
                    eventData.eventCommands[i].indent = indent + 1;
                }
                else
                {
                    //現在のインデントを設定する
                    eventData.eventCommands[i].indent = indent;

                    //コマンドの内容が、以下の場合は、次以降のイベントのインデントを+1する
                    //・選択肢の分岐開始
                    //・条件文の、if文
                    //・ループ文
                    //・バトル開始
                    if (eventCode == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT || //選択肢の開始
                        eventCode == (int) EventEnum.EVENT_CODE_FLOW_IF || //条件文
                        eventCode == (int) EventEnum.EVENT_CODE_FLOW_LOOP || //ループ文
                        eventCode == (int) EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE //カスタム移動
                        )
                        //インデントを増やす
                        indent++;
                    //・バトル開始
                    if (eventCode == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG) //バトル開始
                        //次のイベントがバトル勝利時であった場合のみ、インデントを操作する
                        if (eventData.eventCommands[i + 1].code ==
                            (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_WIN)
                            //インデントを増やす
                            indent++;
                }
            }
        }
    }
}