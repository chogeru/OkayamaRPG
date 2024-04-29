using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Enemy;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventBattle;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Troop;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Character.View;
using RPGMaker.Codebase.Runtime.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventBattle.EventBattleDataModel;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Character
{
    /// <summary>
    /// キャラクター及び、敵のHierarchy
    /// </summary>
    public class BattleHierarchy : AbstractHierarchy
    {
        private List<EnemyDataModel>          _enemyDataModels;
        private List<EventBattleDataModel>    _eventBattleDataModels;
        private List<TroopDataModel>          _troopDataModels;
        private int _updateType = 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattleHierarchy() {
            View = new BattleHierarchyView(this);
            //Refresh();
        }

        /// <summary>
        /// View
        /// </summary>
        public BattleHierarchyView View { get; }


        /// <summary>
        /// データ読込
        /// </summary>
        override protected void LoadData() {
            base.LoadData();

            //各Modelデータの再読込
            _enemyDataModels = databaseManagementService.LoadEnemy();
            var troops = databaseManagementService.LoadTroop();
            _troopDataModels = new List<TroopDataModel>();
            foreach (var troop in troops)
            {
                if (troop.id == TroopDataModel.TROOP_PREVIEW) continue;
                if (troop.id == TroopDataModel.TROOP_BTATLE_TEST) continue;
                if (troop.id == TroopDataModel.TROOP_AUTOMATCHING) continue;
                _troopDataModels.Add(troop);
            }
            _eventBattleDataModels = eventManagementService.LoadEventBattle();
        }

        /// <summary>
        /// Viewのアップデート
        /// </summary>
        protected override void UpdateView(string updateData = null) {
            base.UpdateView();
            View.Refresh(
                updateData,
                _enemyDataModels,
                _troopDataModels,
                _eventBattleDataModels,
                _updateType
            );
            _updateType = 0;
        }

        /// <summary>
        /// バトルの編集のInspector表示
        /// </summary>
        public void OpenBattleSceneInspector() {
            Inspector.Inspector.BattleSceneView();
        }

        /// <summary>
        /// 敵キャラのInspector表示
        /// </summary>
        public void OpenEnemyInspector(
            EnemyDataModel enemyDataModel,
            BattleHierarchyView battleHierarchyView
        ) {
            Inspector.Inspector.CharacterEnemyView(enemyDataModel.id, battleHierarchyView);
        }

        /// <summary>
        /// 敵データ作成
        /// </summary>
        /// <param name="characterHierarchyView"></param>
        public void CreateEnemyDataModel(BattleHierarchyView characterHierarchyView) {
            var newModel = EnemyDataModel.CreateDefault(Guid.NewGuid().ToString(),
                "#" + string.Format("{0:D4}", _enemyDataModels.Count + 1) + "　" + 
                EditorLocalize.LocalizeText("WORD_1518"));

            newModel.images.image = ImageManager.GetImageNameList(PathManager.IMAGE_ENEMY)[0];
            
            _enemyDataModels.Add(newModel);
            databaseManagementService.SaveEnemy(_enemyDataModels);

            Refresh();
        }

        /// <summary>
        /// 敵のコピー＆貼り付け処理
        /// </summary>
        /// <param name="battleHierarchyView"></param>
        /// <param name="enemyDataModel"></param>
        public void PasteEnemyDataModel(BattleHierarchyView battleHierarchyView, EnemyDataModel enemyDataModel) {
            var uuid = Guid.NewGuid().ToString();
            var newModel = enemyDataModel.DataClone();
            newModel.id = uuid;
            newModel.name = CreateDuplicateName(_enemyDataModels.Select(e => e.name).ToList(), newModel.name);
            _enemyDataModels.Add(newModel);
            databaseManagementService.SaveEnemy(_enemyDataModels);
            Refresh();
        }

        /// <summary>
        /// 敵の削除
        /// </summary>
        /// <param name="enemyDataModel"></param>
        public void DeleteEnemyDataModel(EnemyDataModel enemyDataModel) {
            _enemyDataModels.Remove(enemyDataModel);
            databaseManagementService.SaveEnemy(_enemyDataModels);
            Refresh();
        }

        /// <summary>
        /// 敵グループのInspector表示
        /// </summary>
        /// <param name="troopDataModel"></param>
        /// <param name="battleHierarchyView"></param>
        /// <param name="eventNum"></param>
        public void OpenTroopInspector(
            TroopDataModel troopDataModel,
            BattleHierarchyView battleHierarchyView,
            int eventNum = -1
        ) {
            Inspector.Inspector.TroopSceneView(troopDataModel.id, battleHierarchyView, eventNum);
        }

        /// <summary>
        /// 敵グループ作成
        /// </summary>
        /// <param name="battleHierarchyView"></param>
        public void CreateTroopDataModel(BattleHierarchyView battleHierarchyView, TroopDataModel troopDataModelWork = null) {
            TroopDataModel newModel;
            if (troopDataModelWork == null)
            {
                newModel = TroopDataModel.CreateDefault();
                var enemy = databaseManagementService.LoadEnemy().First();
                newModel.name = "#" + string.Format("{0:D4}", _troopDataModels.Count + 1) + "　" + EditorLocalize.LocalizeText("WORD_1518");
                newModel.sideViewMembers = new List<TroopDataModel.SideViewMember>
                    {TroopDataModel.SideViewMember.CreateDefault(enemy.id)};
                newModel.frontViewMembers = new List<TroopDataModel.FrontViewMember>
                    {TroopDataModel.FrontViewMember.CreateDefault(enemy.id)};
            }
            else
            {
                newModel = troopDataModelWork;
            }

            _troopDataModels.Add(newModel);
            databaseManagementService.SaveTroop(_troopDataModels);
            //Preview側がDataManagerを参照しているため、追加する
            var troopDataModel = DataManager.Self().GetTroopDataModel(newModel.id);
            if (troopDataModel == null)
            {
                DataManager.Self().GetTroopDataModels().Add(newModel);
            }
            OpenTroopInspector(newModel, battleHierarchyView);

            Refresh();
        }

        /// <summary>
        /// 敵グループ削除
        /// </summary>
        /// <param name="troopDataModel"></param>
        public void DeleteTroopDataModel(TroopDataModel troopDataModel) {
            _troopDataModels.Remove(troopDataModel);
            databaseManagementService.SaveTroop(_troopDataModels);
            Refresh();
        }

        /// <summary>
        /// 敵グループのコピー＆貼り付け処理
        /// </summary>
        /// <param name="battleHierarchyView"></param>
        /// <param name="troopDataModel"></param>
        public void PasteTroopDataModel(BattleHierarchyView battleHierarchyView, TroopDataModel troopDataModel) {
            var newModel = TroopDataModel.CreateDefault();
            var uuid = newModel.id;
            newModel = troopDataModel.DataClone();
            newModel.id = uuid;
            newModel.name = CreateDuplicateName(_troopDataModels.Select(t=>t.name).ToList(),newModel.name);
            newModel.battleEventId = "";
            CreateTroopDataModel(battleHierarchyView, newModel);

            //コピー元の敵グループに、イベントが存在した場合は、同様に複製する
            var battleEvent = _eventBattleDataModels.Find(item => item.eventId == troopDataModel.battleEventId);
            if (battleEvent != null && battleEvent.pages != null && battleEvent.pages.Count > 0)
            {
                for (int i = 0; i < battleEvent.pages.Count; i++)
                {
                    PasteTroopEventDataModel(troopDataModel, newModel, i);
                }
            }
            //Hierarchy更新
            Refresh();
        }

        /// <summary>
        /// 敵グループのイベント作成
        /// </summary>
        /// <param name="troopDataModel"></param>
        public void CreateTroopEventDataModel(TroopDataModel troopDataModel) {
            EventBattleDataModel eventBattleData = null;

            //バトルイベントが既に存在するかどうかの検索            
            for (var i = 0; i < _eventBattleDataModels.Count; i++)
                if (troopDataModel.battleEventId == _eventBattleDataModels[i].eventId)
                    eventBattleData = _eventBattleDataModels[i];

            //存在しない場合はバトルイベントを作成
            //存在する場合は、そのバトルイベントにページを追加
            if (eventBattleData == null)
            {
                eventBattleData = CreateDefault();
                eventBattleData.eventId = Guid.NewGuid().ToString();
                troopDataModel.battleEventId = eventBattleData.eventId;
                _eventBattleDataModels.Add(eventBattleData);
            }

            // イベントページを追加
            var page = CreateDefaultEventBattlePage(0);
            var pageId = Guid.NewGuid().ToString();
            page.eventId = pageId;
            eventBattleData.pages.Add(page);

            // イベント作成
            var eventData = new EventDataModel(pageId, 0, 0, new List<EventDataModel.EventCommand>());

            eventManagementService.SaveEvent(eventData);
            eventManagementService.SaveEventBattle(_eventBattleDataModels);
            databaseManagementService.SaveTroop(_troopDataModels);

            _updateType = 1;
        }

        /// <summary>
        /// 敵グループのイベントのコピー＆貼り付け処理
        /// </summary>
        /// <param name="copyTroopDataModel"></param>
        /// <param name="pasteTroopDataModel"></param>
        /// <param name="eventNum"></param>
        public void PasteTroopEventDataModel(
            TroopDataModel copyTroopDataModel,
            TroopDataModel pasteTroopDataModel,
            int eventNum
        ) {
            var troopDataModelCopy = copyTroopDataModel.DataClone();
            var battleEvent = _eventBattleDataModels.Find(item => item.eventId == troopDataModelCopy.battleEventId);

            if (battleEvent != null && battleEvent.pages.Count > eventNum)
            {
                var eventDataModels = eventManagementService.LoadEvent();
                var eventDataModel = eventDataModels.Find(item => item.id == battleEvent.pages[eventNum].eventId)
                    .DataClone();

                //それぞれのIDを新しく作成
                if (copyTroopDataModel.id != pasteTroopDataModel.id)
                {
                    EventBattleDataModel eventBattleData = null;
                    for (var i = 0; i < _eventBattleDataModels.Count; i++)
                        if (pasteTroopDataModel.battleEventId == _eventBattleDataModels[i].eventId)
                            eventBattleData = _eventBattleDataModels[i];

                    //存在しない場合はバトルイベントを作成
                    //存在する場合は、そのバトルイベントにページを追加
                    if (eventBattleData == null)
                    {
                        eventBattleData = CreateDefault();
                        eventBattleData.eventId = Guid.NewGuid().ToString();
                        pasteTroopDataModel.battleEventId = eventBattleData.eventId;
                        _eventBattleDataModels.Add(eventBattleData);
                    }

                    // イベントページを追加
                    var page = battleEvent.pages[eventNum].DataClone<EventBattlePage>();
                    var pageId = Guid.NewGuid().ToString();
                    page.eventId = pageId;
                    eventDataModel.id = pageId;
                    page.page = eventBattleData.pages.Count;
                    eventBattleData.pages.Add(page);
                    databaseManagementService.SaveTroop(_troopDataModels);
                }
                else
                {
                    var pageId = Guid.NewGuid().ToString();
                    var page = battleEvent.pages[eventNum].DataClone<EventBattlePage>();
                    page.eventId = pageId;
                    page.page = battleEvent.pages.Count;
                    battleEvent.pages.Add(page);
                    eventDataModel.id = pageId;
                }

                eventManagementService.SaveEvent(eventDataModel);
                eventManagementService.SaveEventBattle(_eventBattleDataModels);

                Refresh();
            }

            //Inspector側への反映
            Inspector.Inspector.Refresh();
            // マップエディタ
            MapEditor.MapEditor.EventRefresh(true);

            _updateType = 1;
        }

        /// <summary>
        /// 敵グループ削除
        /// </summary>
        /// <param name="troopDataModel"></param>
        /// <param name="eventNum"></param>
        public void DeleteTroopEventDataModel(TroopDataModel troopDataModel, int eventNum) {
            LoadData();

            var battleEvent =
                _eventBattleDataModels.Find(item => item.eventId == troopDataModel.battleEventId);

            if (battleEvent != null && battleEvent.pages.Count > eventNum)
            {
                var eventDataModels = eventManagementService.LoadEvent();

                // イベント削除
                eventManagementService.DeleteEvent(
                    eventDataModels.Find(item => item.id == battleEvent.pages[eventNum].eventId));

                // バトルイベント削除
                battleEvent.pages.RemoveAt(eventNum);
                eventManagementService.SaveEventBattle(_eventBattleDataModels);

                Refresh();
            }
        }
    }
}