using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventCommon;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using System.Collections.Generic;

namespace RPGMaker.Codebase.Runtime.Scene.Map
{
    public class CustomMoveEventManager
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

        Dictionary<string, EventCommandChainLauncher> _eventIdEventCommandChainLauncherDic;
        /// <summary>
        /// コンストラクタ
        /// データの定義のみ行う
        /// </summary>
        public CustomMoveEventManager() {
            // データベース取得
            _databaseManagementService = new DatabaseManagementService();
            _flagDataModel = _databaseManagementService.LoadFlags();

            // イベント取得
            _eventManagementService = new EventManagementService();
            // コモンイベント
            _eventCommonDataModels = _eventManagementService.LoadEventCommon();

            _eventIdEventCommandChainLauncherDic = new Dictionary<string, EventCommandChainLauncher>();
        }

        public void Init() {
            //保持しているものがあれば初期化する。
        }

        public void RegisterCustomMove(string commandEventID, List<EventDataModel.EventCommand> commands, int index, int endIndex, bool parallelProcess)
        {
            //commandsからindex～endIndexの範囲のカスタム移動を並列処理される形で登録する。
            if (commands[index].parameters[1] != "-1")
            {
                //このイベントではない。
                commandEventID = commands[index].parameters[1];
            }
            EventCommandChainLauncher launcher = null;
            if (_eventIdEventCommandChainLauncherDic.ContainsKey(commandEventID))
            {
                launcher = _eventIdEventCommandChainLauncherDic[commandEventID];
                launcher.ExitCommandChain();
            }
            else
            {
                launcher = new EventCommandChainLauncher();
                _eventIdEventCommandChainLauncherDic.Add(commandEventID, launcher);
            }
            var eventCommands = new List<EventDataModel.EventCommand>();
            for (int i = index; i <= endIndex; i++)
            {
                var newCommand = commands[i].Clone();
                if (i == index || i == endIndex)
                {
                    newCommand.parameters = new List<string>(newCommand.parameters);
                }
                eventCommands.Add(newCommand);
            }
            eventCommands[0].parameters.Add("1");
            eventCommands[eventCommands.Count - 1].parameters.Clear();
            // 登録元のcommands[endIndex].parametersを記録。
            eventCommands.Add(new EventDataModel.EventCommand(0, commands[endIndex].parameters, null));

            EventDataModel ev = new EventDataModel(commandEventID, 0, 0, eventCommands);
            var eventMapDataModel = new EventMapDataModel();
            eventMapDataModel.eventId = commandEventID;
            launcher.LaunchCommandChain(eventMapDataModel, ev, EventEndCallback, true);
        }

        void EventEndCallback(EventMapDataModel eventMapDataModel, EventDataModel eventDataModel) {
            var parameters = eventDataModel.eventCommands[eventDataModel.eventCommands.Count - 1].parameters;
            if (parameters.Count > 0)
            {
                parameters[0] = "1";
            }
        }

        public void InitializeCount() {
            foreach (var entry in _eventIdEventCommandChainLauncherDic)
            {
                entry.Value.InitializeCount();
            }
        }


        /// <summary>
        /// マップから破棄される際に、各種データを破棄する
        /// </summary>
        public void DestroyEvent() {
            foreach (var entry in _eventIdEventCommandChainLauncherDic)
            {
                entry.Value.ExitCommandChain();
            }
            _eventIdEventCommandChainLauncherDic.Clear();
        }

    }
}
