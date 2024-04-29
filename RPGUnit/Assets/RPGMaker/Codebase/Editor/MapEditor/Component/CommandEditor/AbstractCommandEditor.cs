using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Enemy;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventBattle;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventCommon;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.MapEditor.Window.EventEdit;
using RPGMaker.Codebase.Runtime.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor
{
    public class AbstractCommandEditor
    {
        /// <summary>
        ///     マップからかコモンからか
        /// </summary>
        protected enum EventType
        {
            Map,
            Common
        }

        protected readonly DatabaseManagementService DatabaseManagementService;
        protected readonly List<EventDataModel>      EventDataModels;
        protected readonly int                       EventIndex;
        protected readonly int                       EventCommandIndex;

        protected readonly EventManagementService  EventManagementService;

        protected readonly MapManagementService    MapManagementService;
        protected readonly VisualElement RootElement;
        protected          EventType     Type = EventType.Map;

        protected AbstractCommandEditor(
            VisualElement rootElement,
            List<EventDataModel> eventDataModels,
            int eventIndex,
            int eventCommandIndex
        ) {
            EventManagementService = new EventManagementService();
            MapManagementService = Editor.Hierarchy.Hierarchy.mapManagementService;
            DatabaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;

            RootElement = rootElement;
            EventDataModels = eventDataModels;
            EventIndex = eventIndex;
            EventCommandIndex = eventCommandIndex;

            RootElement.style.display = DisplayStyle.Flex;
        }

        protected EventDataModel EventDataModel => EventDataModels[EventIndex];
        protected EventDataModel.EventCommand EventCommand => EventDataModel.eventCommands[EventCommandIndex];

        // このイベントが配置されているマップのid。何処にも配置されていない (＝コモンイベント) ならnull。
        protected string ThisEventMapId => GetMapId();
        private string GetMapId() {
            var eventMaps = EventManagementService.LoadEventMap();
            for (int i = 0; i < eventMaps.Count; i++)
                if (eventMaps[i].eventId == EventDataModel.id)
                    return eventMaps[i].mapId;
            return null;
        }

        protected ExecutionContentsWindow ExecutionContentsWindow =>
            (ExecutionContentsWindow)WindowLayoutManager.GetActiveWindow(
                WindowLayoutManager.WindowLayoutId.MapEventExecutionContentsWindow);

        protected bool IsCommonEvent()
        {
            return IsCommonEvent(EventDataModel.id);
        }

        public static bool IsCommonEvent(string eventId)
        {
            return FindEventCommonDataModel(eventId) != null;
        }

        public static EventCommonDataModel FindEventCommonDataModel(string eventId)
        {
            var eventCommon = new EventManagementService().LoadEventCommon();
            for (int i = 0; i < eventCommon.Count; i++)
                if (eventCommon[i].eventId == eventId)
                    return eventCommon[i];

            return null;
        }

        public virtual void Invoke() {
        }

        protected VisualElement QRoot(string name)
        {
            return RootElement.Q<VisualElement>(name);
        }

        /// <summary>
        /// コモンイベント用の仮マップの『マップ選択』PopupFieldの追加もしくは追加先項目UIを非表示と
        /// 『キャラクター』PopupFieldの追加。
        /// </summary>
        /// <param name="targetCharacterParameterIndex">
        /// 対象キャラクター値が割り振られているパラメータのインデックス。
        /// </param>
        /// <param name="addTargetCharacterPopupFieldAction">対象キャラクターPopupFieldを追加するAction。</param>
        protected GenericPopupFieldBase<MapDataChoice> AddOrHideProvisionalMapAndAddTargetCharacterPopupField(
            int targetCharacterParameterIndex,
            System.Action<GenericPopupFieldBase<MapDataChoice>> addTargetCharacterPopupFieldAction)
        {
            // コモンイベント用の仮マップの『マップ選択』PopupFieldの追加もしくは追加先項目UIを非表示。
            var provisionalMapPopupField = AddProvisionalMapPopupFieldOrHide(
                QRoot("provisional_map_popupfield_container"),
                EventCommand.parameters[targetCharacterParameterIndex]);

            provisionalMapPopupField?.RegisterValueChangedCallback(
                changeEvent =>
                {
                    // 対象キャラクター。
                    addTargetCharacterPopupFieldAction(provisionalMapPopupField);
                });

            // 対象キャラクター。
            addTargetCharacterPopupFieldAction(provisionalMapPopupField);

            return provisionalMapPopupField;
        }

        /// <summary>
        /// コモンイベント用の仮のマップを選択するPopupFieldを追加もしくは追加先項目UIを非表示にする。
        /// </summary>
        /// <remarks>
        /// コモンイベントの場合はcontainerにPopupFieldを追加、そうでない場合はcontainerの親の親の親を非表示にする。
        /// </remarks>
        /// <param name="container">追加先VisualElement。</param>
        /// <param name="targetCharacterId">設定されている対象キャラクターid。</param>
        /// <returns>追加したPopupField。</returns>
        protected GenericPopupFieldBase<MapDataChoice> AddProvisionalMapPopupFieldOrHide(
            VisualElement container, string targetCharacterId)
        {
            var isCommonEvent = IsCommonEvent();
            container.parent.parent.parent.style.display = isCommonEvent ? DisplayStyle.Flex : DisplayStyle.None;
            if (!isCommonEvent)
            {
                return null;
            }
            
            // 対象キャラクターとして設定しているイベントのイベントid。
            var targetCharacterEventId = new Commons.TargetCharacter(targetCharacterId, EventDataModel.id).GetEventId();

            // 既定で選択状態にする選択項目のマップid。
            var defaultMapId = GetEventMapDataModelByEventId(targetCharacterEventId)?.mapId;

            var mapPopupField = GenericPopupFieldBase<MapDataChoice>.Add(
                container,
                MapDataChoice.GenerateChoices(),
                defaultMapId);
            return mapPopupField;
        }

        /// <summary>
        /// 対象キャラクターを選択するPopupFieldを追加する。
        /// </summary>
        /// <param name="container">追加先VisualElement。</param>
        /// <param name="parameterIndex">選択項目のid値を参照＆設定するパラメータのインデックス。</param>
        /// <param name="alternativeSaveAction">代替セーブ処理。</param>
        /// <param name="forceDefaultIndexIsZero">強制的に既定選択項目を先頭のものにする。</param>
        /// <param name="excludePlayer">選択項目に『プレイヤー』を含めいない。</param>
        /// <param name="forceMapId">
        /// 選択項目としてリストアップするイベント(キャラクター)抽出元のマップの指定を、
        /// このイベントが存在するマップではなく指定idのマップとする。
        /// </param>
        /// <returns>追加したPopupField。</returns>
        protected GenericPopupFieldBase<TargetCharacterChoice> AddTargetCharacterPopupField(
            VisualElement container,
            int parameterIndex,
            System.Action<ChangeEvent<TargetCharacterChoice>> alternativeSaveAction = null,
            bool forceDefaultIndexIsZero = false,
            bool excludePlayer = false,
            string forceMapId = null,
            bool forceInitialize = true)
        {
            var targetCharacterPopupField = GenericPopupFieldBase<TargetCharacterChoice>.Add(
                container,
                TargetCharacterChoice.GenerateChoices(
                    forceMapId ?? GetEventMapDataModelByEventId(EventDataModel.id)?.mapId, excludePlayer),
                forceDefaultIndexIsZero ? string.Empty : EventCommand.parameters[parameterIndex]);

            // 上の追加処理で選択項目が設定値と違いが生じた場合は、設定値を変更する。
            // 選択項目を使いまわしているケースの場合、強制的に保存するとまずいケースがあるため、初期化していいイベントのみ、以下の処理を実施
            if (forceInitialize && targetCharacterPopupField.value.Id != EventCommand.parameters[parameterIndex])
            {
                EventCommand.parameters[parameterIndex] = targetCharacterPopupField.value.Id;
                Save(EventDataModel);
            }

            targetCharacterPopupField.RegisterValueChangedCallback(
                changeEvent =>
                {
                    if (alternativeSaveAction != null)
                    {
                        alternativeSaveAction(changeEvent);
                    }
                    else
                    {
                        EventCommand.parameters[parameterIndex] = changeEvent.newValue.Id;
                        Save(EventDataModel);
                    }
                });

            return targetCharacterPopupField;
        }

        protected void Save(EventDataModel eventDataModelEntity) {
            EventManagementService.SaveEvent(eventDataModelEntity);
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            ExecutionContentsWindow.ProcessTextSetting();
        }

        /// <summary>
        /// 指定idのマップに配置されている、全EventMapDataModel列を取得する。
        /// </summary>
        /// <param name="mapId">指定マップid。</param>
        /// <returns>EventMapDataModel列。</returns>
        public static List<EventMapDataModel> GetEventMapDataModelsInMap(string mapId) {
            return new EventManagementService().LoadEventMap().
                Where(eventMapDataModel => eventMapDataModel.mapId == mapId).ToList();
        }

        /// <summary>
        /// 指定idのイベントが配置されている、EventMapDataModelを取得する。
        /// </summary>
        /// <param name="eventId">イベントid。</param>
        /// <returns>
        /// 指定イベントが配置されているEventMapDataModel。
        /// 指定イベントがどのマップにも配置されていなければnull。
        /// </returns>
        public static EventMapDataModel GetEventMapDataModelByEventId(string eventId) {
            var eventMap = new EventManagementService().LoadEventMap();
            for (int i = 0; i < eventMap.Count; i++)
                if (eventMap[i].eventId == eventId)
                    return eventMap[i];

            return null;
        }

        /// <summary>
        /// 画面に表示するイベントの名称を返却
        /// </summary>
        /// <returns></returns>
        public static string GetEventDisplayName(EventMapDataModel data) {
            if (data.name == "")
                return "EV" + string.Format("{0:D4}", data.SerialNumber) + " " + EditorLocalize.LocalizeText("WORD_1518");
            return "EV" + string.Format("{0:D4}", data.SerialNumber) + " " + data.name;
        }

        /// <summary>
        /// 『移動ルート指定』『ジャンプ』の移動元のタイル座標を取得。
        /// </summary>
        /// <param name="eventDataModel">このイベントのデータ。</param>
        /// <param name="parameters">このイベントコマンドのパラメータ。</param>
        /// <param name="targetCharacterId">対象キャラクターid。</param>
        /// <param name="provisionalMapId">コモンイベント用の仮マップid。</param>
        /// <returns>移動元タイル座標。紐づいたEventMapDataModelが存在しない場合はnull。</returns>
        public static Vector2Int? GetMoveFromTilePositon(
            EventDataModel eventDataModel, string targetCharacterId, string provisionalMapId)
        {
            var eventId =
                new Commons.TargetCharacter(targetCharacterId, eventDataModel.id).
                    GetEventId(ifPlayerIsThisEventId: true);

            var eventMap = new EventManagementService().LoadEventMap();
            EventMapDataModel eventMapDataModel = null;
            for (int i = 0; i < eventMap.Count; i++)
                if (eventMap[i].eventId == eventId)
                {
                    eventMapDataModel = eventMap[i];
                    break;
                }

            if (eventMapDataModel != null)
            {
                // イベントの座標。
                return eventMapDataModel != null ?
                    new Vector2Int(eventMapDataModel.x, eventMapDataModel.y) : null;
            }
            else
            {
                // コモンイベントで対象キャラクターが『プレイヤー』または『このイベント』の場合は、
                // 仮マップのマップ中心座標 (Y座標は負値) とする。
                DebugUtil.Assert(IsCommonEvent(eventId));

                // コモンイベント用仮マップid。
                if (string.IsNullOrEmpty(provisionalMapId))
                {
                    return null;
                }

                var mapDataModel = Editor.Hierarchy.Hierarchy.mapManagementService.LoadMapById(provisionalMapId);
                return new Vector2Int(mapDataModel.width / 2, -mapDataModel.height / 2);
            }
        }

        /// <summary>
        /// バトルイベントで利用する、敵データリスト取得
        /// </summary>
        /// <returns></returns>
        protected List<EnemyDataModel> GetEnemyList() {
            var enemyDataModels =
                DatabaseManagementService.LoadEnemy();
            var fileNames = new List<EnemyDataModel>();
            for (var i = 0; i < enemyDataModels.Count; i++)
                if (enemyDataModels[i].deleted == 0)
                    fileNames.Add(enemyDataModels[i]);

            return fileNames;
        }

        /// <summary>
        /// バトルイベントで利用する、敵の名称リスト取得
        /// </summary>
        /// <returns></returns>
        protected List<string> GetEnemyNameList(bool needAllEnemy = false) {
            var enemies = new List<string>()
            {
                EditorLocalize.LocalizeText("WORD_1104")
            };
            if (!needAllEnemy) enemies.Clear();

            var systemData = DatabaseManagementService.LoadSystem();

            var battleEvent = EventManagementService.LoadEventBattle();
            EventBattleDataModel eventBattle = null;
            for (int i = 0; i < battleEvent.Count; i++)
            {
                for (int j = 0; j < battleEvent[i].pages.Count; j++)
                {
                    if (battleEvent[i].pages[j].eventId == EventDataModels[EventIndex].id)
                    {
                        eventBattle = battleEvent[i];
                        break;
                    }
                }
            }

            if (eventBattle == null)
            {
                //イベントバトルがNULLのケースは、バトルイベントと紐づいていない、コモンイベントのケースとなる
                if (needAllEnemy)
                {
                    return new List<string>
                    {
                        EditorLocalize.LocalizeText("WORD_1104"), "#1","#2","#3","#4","#5","#6","#7","#8"
                    };
                }
                else
                {
                    return new List<string>
                    {
                        "#1","#2","#3","#4","#5","#6","#7","#8"
                    };
                }
            }

            var troopList = DatabaseManagementService.LoadTroop()
                .Find(troop => troop.battleEventId == eventBattle.eventId);

            if (systemData.battleScene.viewType == 1)
            {
                for (int i = 0; i < troopList?.sideViewMembers.Count; i++)
                {
                    var enemy = DatabaseManagementService.LoadEnemy()
                        .Find(e => e.id == troopList.sideViewMembers[i].enemyId);
                    enemies.Add($"#{i + 1} {enemy.name}");
                }
            }
            else
            {
                for (int i = 0; i < troopList?.frontViewMembers.Count; i++)
                {
                    var enemy = DatabaseManagementService.LoadEnemy()
                        .Find(e => e.id == troopList.frontViewMembers[i].enemyId);
                    enemies.Add($"#{i + 1} {enemy.name}");
                }
            }

            return enemies;
        }
    }
}