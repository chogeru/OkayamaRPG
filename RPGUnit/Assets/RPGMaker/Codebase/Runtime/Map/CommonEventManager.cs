using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventCommon;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Runtime.Common;
using System.Collections.Generic;

namespace RPGMaker.Codebase.Runtime.Scene.Map
{
    /// <summary>
    /// コモンイベントの処理クラス 特徴に設定されているコモンイベントの実行時に利用
    /// </summary>
    public class CommonEventManager
    {
        //データ
        private DatabaseManagementService  _databaseManagementService   = null;
        private EventManagementService     _eventManagementService      = null;

        private FlagDataModel              _flagDataModel         = null;
        private List<EventCommonDataModel> _eventCommonDataModels = null;

        // イベント管理用
        private List<EventCommandChainLauncher> _commonEventList     = new List<EventCommandChainLauncher>();
        private List<EventDataModel>            _eventDataList       = new List<EventDataModel>();
        private List<EventCommonDataModel>      _eventCommonDataList = new List<EventCommonDataModel>();
        
        /// <summary>
        /// コンストラクタ
        /// データの定義のみ行う
        /// </summary>
        public CommonEventManager() {
            // データベース取得
            _databaseManagementService = new DatabaseManagementService();
            _flagDataModel = _databaseManagementService.LoadFlags();

            // イベント取得
            _eventManagementService = new EventManagementService();
            // コモンイベント
            _eventCommonDataModels = _eventManagementService.LoadEventCommon();
        }

        /// <summary>
        /// コモンイベントを開始する
        /// 実行対象のidを受け取る
        /// </summary>
        /// <param name="eventId"></param>
        public void LaunchCommandChain(
            string eventId
        ) {
            // 実行対象イベント取得
            var eventData = _eventManagementService.LoadEventById(eventId);
            EventCommonDataModel eventCommonData = null;
            for (int i = 0; i < _eventCommonDataModels.Count; i++)
                if (_eventCommonDataModels[i].eventId == eventId)
                {
                    eventCommonData = _eventCommonDataModels[i];
                    break;
                }

            // イベント登録
            var eventLauncher = new EventCommandChainLauncher();
            _commonEventList.Add(eventLauncher);
            _eventCommonDataList.Add(eventCommonData);
            _eventDataList.Add(eventData);
        }

        /// <summary>
        /// コモンイベントの更新
        /// </summary>
        public void UpdateCommonEvent() {
            for (var i = 0; i < _commonEventList.Count; i++)
                switch (_eventCommonDataList[i].conditions[0].trigger)
                {
                    case 0: // なし
                        _commonEventList[i].LaunchCommandChain(null, _eventDataList[i]);
                        _commonEventList.RemoveAt(i);
                        _eventCommonDataList.RemoveAt(i);
                        _eventDataList.RemoveAt(i);
                        i--;
                        break;

                    case 1: // 自動実行
                        for (var i2 = 0; i2 < _flagDataModel.switches.Count; i2++)
                        {
                            if (_commonEventList[i].IsRunning())
                                continue;

                            if (_flagDataModel.switches[i].id == _eventCommonDataList[i].conditions[0].switchId)
                                if (DataManager.Self().GetRuntimeSaveDataModel().switches.data[i2])
                                    _commonEventList[i].LaunchCommandChain(null, _eventDataList[i], null, true);
                        }

                        break;

                    case 2: // 並列処理
                        for (var i2 = 0; i2 < _flagDataModel.switches.Count; i2++)
                        {
                            if (_commonEventList[i].IsRunning())
                                continue;

                            if (_flagDataModel.switches[i].id == _eventCommonDataList[i].conditions[0].switchId)
                                if (DataManager.Self().GetRuntimeSaveDataModel().switches.data[i2])
                                    _commonEventList[i].LaunchCommandChain(null, _eventDataList[i], null, true);
                        }

                        break;
                }
        }

        /// <summary>
        /// マップから破棄される際に、各種データを破棄する
        /// </summary>
        public void DestroyEvent() {
            // 実行中イベントがあれば停止
            for(int i = 0; i <  _commonEventList.Count; i++)
            {
                _commonEventList[i].ExitCommandChain();
                _commonEventList[i] = null;
            }
            _commonEventList.Clear();
            _eventDataList.Clear();
            _eventCommonDataList.Clear();
        }
    }
}