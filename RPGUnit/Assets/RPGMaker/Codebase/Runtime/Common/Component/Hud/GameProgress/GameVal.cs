using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement;
using RPGMaker.Codebase.Runtime.Map;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using System.Collections.Generic;
using System.Linq;
using UnityEngine; //バトルでも利用するが、直接マップ側を編集することで良い
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud.GameProgress
{
    public class GameVal
    {
        public const int MinValue = -99999999;
        public const int MaxValue = 99999999;

        private string _currentID;

        private DatabaseManagementService _databaseManagementService;

        // コアシステムサービス
        //--------------------------------------------------------------------------------------------------------------
        private EventManagementService _eventManagementService;
        private string _itemId;

        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        private RuntimeSaveDataModel _runtimeSaveDataModel;

        /// <summary>
        /// 初期化
        /// </summary>
        public void Init() {
            _runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
            _eventManagementService = new EventManagementService();
            _databaseManagementService = new DatabaseManagementService();
        }

        /// <summary>
        /// 変数設定
        /// </summary>
        /// <param name="command"></param>
        /// <param name="currentID"></param>
        public void SetGameVal(EventDataModel.EventCommand command, string currentID) {
            var runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
            _currentID = currentID;
            var eventMapList = new List<EventMapDataModel>();
            var eventMapDataModels = _eventManagementService.LoadEventMap();
            for (var i = 0; i < eventMapDataModels.Count; i++) eventMapList.Add(eventMapDataModels[i]);


            var value = "0";

            switch (int.Parse(command.parameters[3]))
            {
                case 0:

                    value = command.parameters[4];
                    break;
                case 1:

                    var flagDataModel = _databaseManagementService.LoadFlags();
                    for (var i = 0; i < flagDataModel.variables.Count; i++)
                        if (flagDataModel.variables[i].id == command.parameters[4])
                        {
                            value = runtimeSaveDataModel.variables.data[i];
                            break;
                        }

                    break;
                case 2:
                    void Rand() {
                        //int型の最小値、最大値を含めるために、ロジックを微修正する
                        if (int.Parse(command.parameters[4]) == int.MinValue && int.Parse(command.parameters[5]) == int.MaxValue)
                        {
                            //intの最小値から最大値の乱数の場合、負数か正数かを決定後に乱数を行う
                            int rand = Random.Range(0, 2);
                            if (rand == 0)
                            {
                                //負数で乱数を行う
                                value = Random.Range(int.Parse(command.parameters[4]), 0).ToString();
                            }
                            else
                            {
                                //正数で乱数を行う
                                value = (Random.Range(-1, int.Parse(command.parameters[5])) + 1).ToString();
                            }
                        }
                        else
                        {
                            if (int.Parse(command.parameters[5]) == int.MaxValue)
                            {
                                //intの最大値までの乱数を行う
                                value = (Random.Range(int.Parse(command.parameters[4]) - 1, int.Parse(command.parameters[5])) + 1).ToString();
                            }
                            else
                            {
                                //指定された範囲で、そのまま乱数を行う
                                value = Random.Range(int.Parse(command.parameters[4]), int.Parse(command.parameters[5]) + 1).ToString();
                            }
                        }
                    }

                    // 範囲はここで処理
                    if (command.parameters[8] == "1")
                    {
                        for (var i = int.Parse(command.parameters[0]) - 1; i < int.Parse(command.parameters[1]); i++)
                        {
                            Rand();
                            _runtimeSaveDataModel.variables.data[i] = value;
                        }
                        return;
                    }
                    else
                        Rand();

                    break;
                case 3:
                    _itemId = command.parameters[5];

                    switch (int.Parse(command.parameters[4]))
                    {
                        case 0:
                            value = ItemNum(command);
                            break;
                        case 1:
                            value = WeaponNum(command);
                            break;
                        case 2:
                            value = ArmorNum(command);
                            break;
                        case 3:
                            value = Actor(command);
                            break;
                        case 4:
                            value = Enemy(command);
                            break;
                        case 5:
                            value = Character(command);
                            break;
                        case 6:
                            value = Party(command);
                            break;
                        case 7:
                            value = JustBefore(command);
                            break;
                        case 8:
                            value = Other(command);
                            break;
                    }

                    break;
            }

            SetData(command, value);
        }

        private string ItemNum(EventDataModel.EventCommand command) {
            var value = "0";
            foreach (var item in _runtimeSaveDataModel.runtimePartyDataModel.items)
                if (item.itemId == _itemId)
                    value = item.value.ToString();

            return value;
        }

        private string WeaponNum(EventDataModel.EventCommand command) {
            var value = "0";
            foreach (var weapon in _runtimeSaveDataModel.runtimePartyDataModel.weapons)
                if (weapon.weaponId == _itemId)
                    value = weapon.value.ToString();

            return value;
        }

        private string ArmorNum(EventDataModel.EventCommand command) {
            var value = "0";
            foreach (var armors in _runtimeSaveDataModel.runtimePartyDataModel.armors)
                if (armors.armorId == command.parameters[7])
                    value = armors.value.ToString();

            return value;
        }

        private string Actor(EventDataModel.EventCommand command) {
            var value = "0";
            var classDataModels = DataManager.Self().GetClassDataModels();
            foreach (var actor in _runtimeSaveDataModel.runtimeActorDataModels)
            {
                var gameActor = DataManager.Self().GetGameActors().Actor(actor);

                if (actor.actorId == _itemId)
                    switch (int.Parse(command.parameters[6]))
                    {
                        case 0:
                            value = gameActor.Level.ToString();
                            break;
                        case 1:
                            value = gameActor.CurrentExp().ToString();
                            break;
                        case 2:
                            value = gameActor.Hp.ToString();
                            break;
                        case 3:
                            value = gameActor.Mp.ToString();
                            break;
                        case 4:
                            value = gameActor.Mhp.ToString();
                            break;
                        case 5:
                            value = gameActor.Mmp.ToString();
                            break;
                        case 6:
                            value = gameActor.Atk.ToString();
                            break;
                        case 7:
                            value = gameActor.Def.ToString();
                            break;
                        case 8:
                            value = gameActor.Mat.ToString();
                            break;
                        case 9:
                            value = gameActor.Mdf.ToString();
                            break;
                        case 10:
                            value = gameActor.Speed.ToString();
                            break;
                        case 11:
                            value = gameActor.Luk.ToString();
                            break;
                        case 12:
                            value = gameActor.Tp.ToString();
                            break;
                    }
            }

            return value;
        }

        private string Enemy(EventDataModel.EventCommand command) {
            var value = "0";
            var enemies = DataManager.Self().GetEnemyDataModels();
            foreach (var enemy in enemies)
                if (enemy.id == _itemId)
                    switch (int.Parse(command.parameters[6]))
                    {
                        case 0:
                            value = enemy.param[0].ToString();
                            break; //HP
                        case 1:
                            value = enemy.param[1].ToString();
                            break; //MP
                        case 2:
                            value = enemy.param[0].ToString();
                            break; //MaxHP
                        case 3:
                            value = enemy.param[1].ToString();
                            break; //MaxMP
                        case 4:
                            value = enemy.param[3].ToString();
                            break; //攻撃力
                        case 5:
                            value = enemy.param[4].ToString();
                            break; //防御力
                        case 6:
                            value = enemy.param[5].ToString();
                            break; //魔法力
                        case 7:
                            value = enemy.param[6].ToString();
                            break; //魔法防御
                        case 8:
                            value = enemy.param[7].ToString();
                            break; //敏捷性
                        case 9:
                            value = enemy.param[8].ToString();
                            break; //運
                        case 10:
                            value = enemy.param[1].ToString();
                            break; //TP
                    }

            return value;
        }

        private string Character(EventDataModel.EventCommand command) {
            var value = "0";
            var pos = new Vector2(0, 0);
            if (command.parameters[5] == "-2") //プレイヤー
            {
                var playerOnMap = MapManager.OperatingCharacter;
                switch (int.Parse(command.parameters[6]))
                {
                    case 0:
                        value = playerOnMap.x_now.ToString();
                        break;
                    case 1:
                        value = (playerOnMap.y_now < 0 ? playerOnMap.y_now * -1 : playerOnMap.y_now).ToString();
                        break;
                    case 2:
                        value = ((int)playerOnMap.GetCurrentDirection()).ToString();
                        break;
                    case 3:
                        value = (playerOnMap.x_now * 96 + 48).ToString();
                        break;
                    case 4:
                        value = ((playerOnMap.y_now < 0 ? playerOnMap.y_now * -1 : playerOnMap.y_now) * 96 + 48).ToString();
                        break;
                }
            }
            else if (command.parameters[5] == "-1") //このイベント
            {
                List<EventOnMap> eventOnMaps = MapEventExecutionController.Instance.EventsOnMap;
                EventOnMap eventOnMap = null;
                for (int i = 0; i < eventOnMaps.Count; i++)
                {
                    if (eventOnMaps[i].MapDataModelEvent.eventId == _currentID)
                    {
                        eventOnMap = eventOnMaps[i];
                        break;
                    }
                }

                if (eventOnMap == null)
                    return value;

                switch (int.Parse(command.parameters[6]))
                {
                    case 0:
                        value = eventOnMap.x_now.ToString();
                        break;
                    case 1:
                        value = (eventOnMap.y_now < 0 ? eventOnMap.y_now * -1 : eventOnMap.y_now).ToString();
                        break;
                    case 2:
                        value = ((int)eventOnMap.GetCurrentDirection()).ToString();
                        break;
                    case 3:
                        value = (eventOnMap.x_now * 96 + 48).ToString();
                        break;
                    case 4:
                        value = ((eventOnMap.y_now < 0 ? eventOnMap.y_now * -1 : eventOnMap.y_now) * 96 + 48).ToString();
                        break;
                }
            }
            else
            {
                var id = command.parameters[5];
                var eventMapDataModels = _eventManagementService.LoadEventMap();
                var eventMap = eventMapDataModels.FirstOrDefault(c => c.eventId == id);

                List<EventOnMap> eventOnMaps = MapEventExecutionController.Instance.EventsOnMap;
                EventOnMap eventOnMap = null;
                for (int i = 0; i < eventOnMaps.Count; i++)
                {
                    if (eventOnMaps[i].MapDataModelEvent.eventId == eventMap.eventId)
                    {
                        eventOnMap = eventOnMaps[i];
                        break;
                    }
                }

                if (eventOnMap == null)
                    return value;

                switch (int.Parse(command.parameters[6]))
                {
                    case 0:
                        value = eventOnMap.x_now.ToString();
                        break;
                    case 1:
                        value = (eventOnMap.y_now < 0 ? eventOnMap.y_now * -1 : eventOnMap.y_now).ToString();
                        break;
                    case 2:
                        value = eventOnMap.GetCurrentDirection().ToString();
                        break;
                    case 3:
                        value = (eventOnMap.x_now * 96 + 48).ToString();
                        break;
                    case 4:
                        value = ((eventOnMap.y_now < 0 ? eventOnMap.y_now * -1 : eventOnMap.y_now) * 96 + 48).ToString();
                        break;
                }
            }

            return value;
        }

        private string Party(EventDataModel.EventCommand command) {
            var value = "0";
            if (_runtimeSaveDataModel.runtimePartyDataModel.actors.Count > int.Parse(command.parameters[5]))
                value = DataManager.Self().GetActorDataModel(_runtimeSaveDataModel.runtimePartyDataModel.actors[int.Parse(command.parameters[5])]).SerialNumber.ToString();
            return value;
        }

        private string JustBefore(EventDataModel.EventCommand command) {
            var value = "0";
            switch (int.Parse(command.parameters[5]))
            {
                case 0: //直前に使用したスキルID
                    value = _runtimeSaveDataModel.runtimePartyDataModel.lastData.skillId.ToString();
                    break;
                case 1: //直前に使用したアイテムID
                    value = _runtimeSaveDataModel.runtimePartyDataModel.lastData.itemId.ToString();
                    break;
                case 2: //直前に行動したアクターID
                    value = _runtimeSaveDataModel.runtimePartyDataModel.lastData.actionActorId.ToString();
                    break;
                case 3: //直前に行動した敵Index
                    value = _runtimeSaveDataModel.runtimePartyDataModel.lastData.actionEnemyIndex.ToString();
                    break;
                case 4: //直前に対象となったアクターID
                    value = _runtimeSaveDataModel.runtimePartyDataModel.lastData.targetActorId.ToString();
                    break;
                case 5: //直前に対象となった敵Index
                    value = _runtimeSaveDataModel.runtimePartyDataModel.lastData.targetEnemyIndex.ToString();
                    break;
            }

            return value;
        }

        private string Other(EventDataModel.EventCommand command) {
            var value = "0";
            switch (int.Parse(command.parameters[5]))
            {
                case 0:
                    string mapId = _runtimeSaveDataModel.runtimePlayerDataModel.map.mapId;
                    var mapManagementService = new MapManagementService();
                    var mapDataModel = mapManagementService.LoadMapById(mapId);
                    List<MapBaseDataModel> work = mapManagementService.LoadMapBase();
                    for (int i = 0; i < work.Count; i++) {
                        if (mapDataModel.id == work[i].id) {
                            value = work[i].SerialNumber.ToString();
                            break;
                        }
                    }
                    break;
                case 1:
                    value = _runtimeSaveDataModel.runtimePartyDataModel.actors.Count.ToString();
                    break;
                case 2:
                    value = _runtimeSaveDataModel.runtimePartyDataModel.gold.ToString();
                    break;
                case 3:
                    value = _runtimeSaveDataModel.runtimePartyDataModel.steps.ToString();
                    break;
                case 4:
                    value = Mathf.Floor(_runtimeSaveDataModel.runtimeSystemConfig.playTime).ToString();
                    break;
                case 5:
                    var obj = HudDistributor.Instance.NowHudHandler().TimerInitObject();
                    int timer = 0;
                    if (obj != null && obj.activeSelf)
                    {
                        var _second = obj.transform.Find("Canvas/DisplayArea/Timer/Second").GetComponent<Text>();
                        var _minute = obj.transform.Find("Canvas/DisplayArea/Timer/Minute").GetComponent<Text>();
                        timer = int.Parse(_minute.text) * 60 + int.Parse(_second.text);
                    }
                    value = timer.ToString();
                    break;
                case 6:
                    value = _runtimeSaveDataModel.runtimeSystemConfig.saveCount.ToString();
                    break;
                case 7:
                    value = _runtimeSaveDataModel.runtimeSystemConfig.battleCount.ToString();
                    break;
                case 8:
                    value = _runtimeSaveDataModel.runtimeSystemConfig.winCount.ToString();
                    break;
                case 9:
                    value = _runtimeSaveDataModel.runtimeSystemConfig.escapeCount.ToString();
                    break;
            }
            return value;
        }

        private void SetData(EventDataModel.EventCommand command, string value) {
            // 代入用関数
            int SetValue(int value1, int value2, int operatorNum) {
                switch (operatorNum)
                {
                    case 0:
                        return value2;
                    case 1:
                        return CSharpUtil.AddValue(value1, value2, MinValue, MaxValue);
                    case 2:
                        return CSharpUtil.SubValue(value1, value2, MinValue, MaxValue);
                    case 3:
                        return CSharpUtil.MulValue(value1, value2, MinValue, MaxValue);
                    case 4:
                        return value1 / value2;
                    case 5:
                        return value1 % value2;
                }

                return 0;
            }

            // 範囲指定の判定
            if (command.parameters[8] == "1")
            {
                for (var i = int.Parse(command.parameters[0]) - 1; i < int.Parse(command.parameters[1]); i++)
                    _runtimeSaveDataModel.variables.data[i] =
                        SetValue(int.Parse(_runtimeSaveDataModel.variables.data[i]), int.Parse(value),
                            int.Parse(command.parameters[2])).ToString();
            }
            else
            {
                // index取得
                var vari = new DatabaseManagementService().LoadFlags().variables;

                var index1 = 0;
                for (var i = 0; i < vari.Count; i++)
                    if (vari[i].id == command.parameters[0])
                    {
                        index1 = i;
                        break;
                    }

                if (_runtimeSaveDataModel.variables.data.Count > index1)
                {
                    _runtimeSaveDataModel.variables.data[index1] =
                        SetValue(int.Parse(_runtimeSaveDataModel.variables.data[index1]), int.Parse(value),
                            int.Parse(command.parameters[2])).ToString();
                }
            }
        }
    }
}