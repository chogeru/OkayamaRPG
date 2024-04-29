#define PROCESSING_FOR_INCONSISTENT_DATA

using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Editor.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.OutlineEditor
{
    // セクション相関イベント情報クラス。
    public class SectionCorrelationEventInfo
    {
        private static SectionCorrelationEventInfo instance;

        // シングルトンなのでprivateなコンストラクタ。
        private SectionCorrelationEventInfo() {
        }

        public static SectionCorrelationEventInfo Instance
        {
            get { return instance ??= new SectionCorrelationEventInfo(); }
        }

        private static string GetSectionName(string sectionId) {
            var sectionDataModel = !string.IsNullOrEmpty(sectionId)
                ? OutlineEditor.OutlineDataModel.GetSectionDataModel(sectionId)
                : null;
            return sectionDataModel != null ? sectionDataModel.Name : $"<{EditorLocalize.LocalizeText("WORD_1608")}>";
        }

        private static (List<EventMapDataModel>, List<EventDataModel>) LoadEventDatas() {
            var eventManagementService = new EventManagementService();
            var eventMapDataModels = eventManagementService.LoadEventMap();
            var eventDataModels = eventManagementService.LoadEvent();
            return (eventMapDataModels, eventDataModels);
        }

        private static Dictionary<string, MapSubDataModel> GetIdToMapSubDataModelDictionary() {
            return OutlineEditor.MapDataModels.ToDictionary(mapDataModel => mapDataModel.ID);
        }

        // 全イベントページ列を抽出。確認時の件数 2189。
        private static IEnumerable<EventPage> GetEventPages(
            List<EventMapDataModel> eventMapDataModels,
            List<EventDataModel> eventDataModels,
            Dictionary<string, MapSubDataModel> idToMapSubDataModelDictionary,
            string sectionId = null
        ) {
#if PROCESSING_FOR_INCONSISTENT_DATA
            var eventPages = new List<EventPage>();
            foreach (var eventMapDataModel in eventMapDataModels)
            {
                foreach (var eventMapPage in eventMapDataModel.pages)
                {
                    if (sectionId == null || eventMapPage.sectionId == sectionId)
                    {
                        var candidateEventDataModels = eventDataModels.Where(eventDataModel =>
                            eventDataModel.id == eventMapDataModel.eventId);
                        if (candidateEventDataModels.Count() >= 1)
                        {
                            if (candidateEventDataModels.Count() > 1)
                                DebugUtil.LogWarning(
                                    "同じidのイベントデータが存在します。\n" +
                                    string.Join(
                                        ",\n",
                                        candidateEventDataModels.Select(whereEventDataModel =>
                                            $"  EventDataModel {{ id=\"{whereEventDataModel.id}\", page={whereEventDataModel.page} }}")));


                            try
                            {
                                var eventDataModel = candidateEventDataModels.First();
                                eventPages.Add(new EventPage(
                                    eventMapDataModel,
                                    eventMapPage,
                                    eventDataModel,
                                    idToMapSubDataModelDictionary[eventMapDataModel.mapId]));

                                DebugUtil.LogWarningIf(eventDataModel.page != eventMapPage.page, "ページ値が違います。");
                            }
                            catch (Exception)
                            {
                                //マップIDが見つからないなど、データに不整合が生じた場合に到達する
                                //その場合は無視する
                            }
                        }
                        else
                        {
                            DebugUtil.LogWarning(
                                $"マップイベントデータ EventMapDataModel {{ eventId=\"{eventMapDataModel.eventId}\" }} が参照している、" +
                                $"イベントデータ EventDataModel {{ id=\"{eventMapDataModel.eventId}\" }} が、" +
                                "存在しません。");
                        }
                    }
                }
            }

            return eventPages.AsEnumerable();
#else
            return eventMapDataModels.
                SelectMany(eventMapDataModel => eventMapDataModel.pages.
                    Select(eventMapPage =>
                        new EventPage(
                            eventMapDataModel,
                            eventMapPage,
                            eventDataModels.ForceSingle(eventDataModel =>
                                eventDataModel.id == eventMapDataModel.eventId),
                            idToMapSubDataModelDictionary[eventMapDataModel.mapId])));
#endif
        }

        // 全イベントページ列から、スイッチONイベントコマンド列を抽出。確認時の件数 267。
        private static IEnumerable<(EventPage eventPage, EventDataModel.EventCommand eventCommand)>
            GetSwitchOnEventCommands(IEnumerable<EventPage> eventPages) {
            return eventPages.SelectMany(eventPage => eventPage.EventDataModel.eventCommands.Where(eventCommand =>
                (EventEnum) eventCommand.code == EventEnum.EVENT_CODE_GAME_SWITCH &&
                // ON？
                eventCommand.parameters[(int) Sopi.OnOff] == "0").Select(eventCommand => (eventPage, eventCommand)));
        }

        // 全イベントページ列から、アイテム増イベントコマンド列を抽出。
        private static IEnumerable<(EventPage eventPage, EventDataModel.EventCommand eventCommand)>
            GetItemAddEventCommands(IEnumerable<EventPage> eventPages) {
            return eventPages.SelectMany(eventPage => eventPage.EventDataModel.eventCommands.Where(eventCommand =>
                    (EventEnum) eventCommand.code == EventEnum.EVENT_CODE_PARTY_ITEM &&
                    // 増？
                    eventCommand.parameters[(int) Pipi.AddOrSubtraction] == "0")
                .Select(eventCommand => (eventPage, eventCommand)));
        }

        // 全イベントページ列から、スイッチONでの条件分岐イベントコマンド列を抽出。確認時の件数 61。
        private static IEnumerable<(EventPage eventPage, EventDataModel.EventCommand eventCommand)>
            GetIfSwitchEventCommands(IEnumerable<EventPage> eventPages) {
            return eventPages.SelectMany(eventPage => eventPage.EventDataModel.eventCommands.Where(eventCommand =>
                (EventEnum) eventCommand.code == EventEnum.EVENT_CODE_FLOW_IF &&
                // スイッチの値が条件？
                eventCommand.parameters[(int) Fipi.SwitchEnabled] != "0" &&
                // ONで条件成立？
                eventCommand.parameters[(int) Fipi.OnOff] == "0").Select(eventCommand => (eventPage, eventCommand)));
        }

        // 全イベントページ列から、出現条件の有効なスイッチを抽出。
        private static IEnumerable<(EventPage eventPage, EventMapDataModel.EventMapPageConditionSwitch appearSwitch)>
            GetAppearCinditionSwitches(IEnumerable<EventPage> eventPages) {
            return eventPages.Select(eventPage =>
                    new[]
                    {
                        (eventPage, appearSwitch: eventPage.EventMapPage.condition.switchOne),
                        (eventPage, appearSwitch: eventPage.EventMapPage.condition.switchTwo)
                    }.Where(_ => _.appearSwitch.enabled != 0 && _.appearSwitch.switchId != ""))
                .Aggregate((destination, source) => destination.Concat(source));
        }

        // 全イベントページ列から、出現条件の有効なスイッチアイテムを抽出。
        private static IEnumerable<(EventPage eventPage, EventMapDataModel.EventMapPageConditionSwitchItem
                appearSwitchItem)>
            GetAppearCinditionSwitchItems(IEnumerable<EventPage> eventPages) {
            return eventPages.Where(eventPage => eventPage.EventMapPage.condition.switchItem.enabled != 0)
                .Select(eventPage => (eventPage, eventPage.EventMapPage.condition.switchItem));
        }

        // スイッチONイベントコマンド列から、本セクションのものを抽出。
        private static IEnumerable<(EventPage eventPage, EventDataModel.EventCommand eventCommand)>
            GetSectionSwitchOnEventCommands(
                SectionDataModel sectionDataModel,
                IEnumerable<(EventPage eventPage, EventDataModel.EventCommand eventCommand)> switchOnEventCommands
            ) {
            return switchOnEventCommands.Where(switchOnEventCommand =>
                switchOnEventCommand.eventPage.EventMapPage.sectionId == sectionDataModel.ID);
        }

        // アイテム増イベントコマンド列から、本セクションのものを抽出。
        private static IEnumerable<(EventPage eventPage, EventDataModel.EventCommand eventCommand)>
            GetSectionItemAddEventCommands(
                SectionDataModel sectionDataModel,
                IEnumerable<(EventPage eventPage, EventDataModel.EventCommand eventCommand)> itemAddEventCommands
            ) {
            return itemAddEventCommands.Where(switchOnEventCommand =>
                switchOnEventCommand.eventPage.EventMapPage.sectionId == sectionDataModel.ID);
        }

        // スイッチONでの条件分岐イベントコマンド列から、本セクションのものを抽出。
        private static IEnumerable<(EventPage eventPage, EventDataModel.EventCommand eventCommand)>
            GetSectionIfSwitchEventCommands(
                SectionDataModel sectionDataModel,
                IEnumerable<(EventPage eventPage, EventDataModel.EventCommand eventCommand)> ifSwitchEventCommands
            ) {
            return ifSwitchEventCommands.Where(ifSwitchEventCommand =>
                ifSwitchEventCommand.eventPage.EventMapPage.sectionId == sectionDataModel.ID);
        }

        // 出現条件の有効なスイッチ列から、本セクションのものを抽出。
        private static IEnumerable<(EventPage eventPage, EventMapDataModel.EventMapPageConditionSwitch appearSwitch)>
            GetSectionAppearCinditionSwitches(
                SectionDataModel sectionDataModel,
                IEnumerable<(EventPage eventPage,
                    EventMapDataModel.EventMapPageConditionSwitch appearSwitch)> appearCinditionSwitches
            ) {
            return appearCinditionSwitches.Where(appearCinditionSwitch =>
                appearCinditionSwitch.eventPage.EventMapPage.sectionId == sectionDataModel.ID);
        }

        // 出現条件の有効なスイッチアイテム列から、本セクションのものを抽出。
        private static IEnumerable<(EventPage eventPage, EventMapDataModel.EventMapPageConditionSwitchItem
                appearSwitchItem)>
            GetSectionAppearCinditionSwitchItems(
                SectionDataModel sectionDataModel,
                IEnumerable<(EventPage eventPage, EventMapDataModel.EventMapPageConditionSwitchItem appearSwitchItem)>
                    eventMapPageSwitchItems
            ) {
            return eventMapPageSwitchItems.Where(eventMapPageSwitchItem =>
                eventMapPageSwitchItem.eventPage.EventMapPage.sectionId == sectionDataModel.ID);
        }

        // スイッチONイベントコマンド列を、スイッチidをキーにLookup化して抽出の高速化を図る。確認時の件数 153。
        private static ILookup<string, (EventPage eventPage, EventDataModel.EventCommand eventCommand)>
            GetSwitchIdToSwitchOnEventCommandsLookup(
                IEnumerable<(EventPage eventPage, EventDataModel.EventCommand eventCommand)> switchOnEventCommands
            ) {
            return switchOnEventCommands.ToLookup(switchOnEventCommand =>
                switchOnEventCommand.eventCommand.parameters[(int) Sopi.SwitchId]);
        }

        // アイテム増イベントコマンド列を、アイテムidをキーにLookup化して抽出の高速化を図る。
        private static ILookup<string, (EventPage eventPage, EventDataModel.EventCommand eventCommand)>
            GetItemIdToItemAddEventCommandsLookup(
                IEnumerable<(EventPage eventPage, EventDataModel.EventCommand eventCommand)> itemAddEventCommands
            ) {
            return itemAddEventCommands.ToLookup(itemAddEventCommand =>
                itemAddEventCommand.eventCommand.parameters[(int) Pipi.ItemId]);
        }

        // スイッチONでの条件分岐イベントコマンド列を、スイッチidをキーにLookup化して抽出の高速化を図る。確認時の件数 29。
        private static ILookup<string, (EventPage eventPage, EventDataModel.EventCommand eventCommand)>
            GetSwitchIdToIfSwitchEventCommandsLookup(
                IEnumerable<(EventPage eventPage, EventDataModel.EventCommand eventCommand)> ifSwitchEventCommands
            ) {
            return ifSwitchEventCommands.ToLookup(ifSwitchEventCommand =>
                ifSwitchEventCommand.eventCommand.parameters[(int) Fipi.SwitchId]);
        }

        // 出現条件の有効なスイッチアイテム列を、アイテムidをキーにLookup化して抽出の高速化を図る。
        private static ILookup<string, (EventPage eventPage, EventMapDataModel.EventMapPageConditionSwitchItem
                switchItem)>
            GetItemIdToAppearCinditionSwitchItemsLookup(
                IEnumerable<(EventPage eventPage, EventMapDataModel.EventMapPageConditionSwitchItem switchItem)>
                    eventMapPageSwitchItems
            ) {
            return eventMapPageSwitchItems.ToLookup(eventMapPageSwitchItem =>
                eventMapPageSwitchItem.switchItem.switchItemId);
        }

        // アウトラインエディターのスイッチライン用のセクション相関関係情報を設定。
        public void SetSectionsRelated() {
            var (eventMapDataModels, eventDataModels) = LoadEventDatas();

            var eventPages = GetEventPages(eventMapDataModels, eventDataModels, GetIdToMapSubDataModelDictionary());

            var appearCinditionSwitches = GetAppearCinditionSwitches(eventPages);
            var appearCinditionSwitchItems = GetAppearCinditionSwitchItems(eventPages);
            var ifSwitchEventCommands = GetIfSwitchEventCommands(eventPages);

            var switchIdToSwitchOnEventCommandsLookup =
                GetSwitchIdToSwitchOnEventCommandsLookup(GetSwitchOnEventCommands(eventPages));
            var itemIdToItemAddEventCommandsLookup =
                GetItemIdToItemAddEventCommandsLookup(GetItemAddEventCommands(eventPages));

            // 接続情報をクリア。
            foreach (var sectionDataModel in OutlineEditor.OutlineDataModel.Sections)
                sectionDataModel.RelatedBySwitchSectionIds.Clear();

            // 入力ポートにコネクション(接続線)を接続するセクションデータモデルのループ。
            foreach (var connectionInSectionDataModel in OutlineEditor.OutlineDataModel.Sections)
            {
                // 出力ポートにコネクション(接続線)を接続するセクションデータモデル列。
                var connectionOutSectionDataModels = new List<SectionDataModel>();

                // 出力ポートに接続するセクションデータモデル列を追加。
                {
                    // 条件情報 - 出現条件 - スイッチ1, スイッチ2。
                    foreach (var sectionAppearCinditionSwitch in
                        GetSectionAppearCinditionSwitches(connectionInSectionDataModel, appearCinditionSwitches))
                        connectionOutSectionDataModels.AddRange(
                            GetSectionDataModels(
                                switchIdToSwitchOnEventCommandsLookup[
                                    sectionAppearCinditionSwitch.appearSwitch.switchId]));

                    // 条件情報 - 出現条件 - スイッチアイテム。
                    foreach (var sectionEventMapPageSwitchItem in
                        GetSectionAppearCinditionSwitchItems(connectionInSectionDataModel, appearCinditionSwitchItems))
                        connectionOutSectionDataModels.AddRange(
                            GetSectionDataModels(
                                itemIdToItemAddEventCommandsLookup[
                                    sectionEventMapPageSwitchItem.appearSwitchItem.switchItemId]));

                    // 条件情報 - 分岐条件。
                    foreach (var sectionIfSwitchEventCommand in
                        GetSectionIfSwitchEventCommands(connectionInSectionDataModel, ifSwitchEventCommands))
                        connectionOutSectionDataModels.AddRange(
                            GetSectionDataModels(
                                switchIdToSwitchOnEventCommandsLookup[
                                    sectionIfSwitchEventCommand.eventCommand.parameters[(int) Fipi.SwitchId]]));
                }

                // 出力セクション(接続元)に入力セクション(接続先)を追加。
                foreach (var connectionOutSectionDataModel in connectionOutSectionDataModels)
                    if (connectionOutSectionDataModel != null &&
                        connectionOutSectionDataModel.ID != connectionInSectionDataModel.ID &&
                        !connectionOutSectionDataModel.RelatedBySwitchSectionIds.Contains(connectionInSectionDataModel
                            .ID))
                        connectionOutSectionDataModel.RelatedBySwitchSectionIds.Add(connectionInSectionDataModel.ID);

                // イベント命令が属するマップページの列からセクション列を取得。
                static IEnumerable<SectionDataModel> GetSectionDataModels(
                    IEnumerable<(EventPage eventPage, EventDataModel.EventCommand eventCommand)> eventPageCommands
                ) {
                    return eventPageCommands.Select(eventPageCommand =>
                        OutlineEditor.OutlineDataModel.GetSectionDataModel(
                            eventPageCommand.eventPage.EventMapPage.sectionId));
                }
            }
        }

        // セクションInspectorにイベント情報UIを設定。
        public void ShowSectionInspectorUi(SectionDataModel sectionDataModel, VisualElement parentVe) {
            var (eventMapDataModels, eventDataModels) = LoadEventDatas();

            var eventPages = GetEventPages(eventMapDataModels, eventDataModels, GetIdToMapSubDataModelDictionary(), sectionDataModel.ID);

            // スイッチ情報。
            var switchOnEventCommands = GetSwitchOnEventCommands(eventPages);
            var switchIdToSwitchOnEventCommandsLookup = GetSwitchIdToSwitchOnEventCommandsLookup(switchOnEventCommands);
            var itemAddEventCommands = GetItemAddEventCommands(eventPages);
            var itemIdToItemAddEventCommandsLookup = GetItemIdToItemAddEventCommandsLookup(itemAddEventCommands);

            // 条件情報 - 出現条件。
            var appearCinditionSwitches = GetAppearCinditionSwitches(eventPages);
            var appearCinditionSwitchItems = GetAppearCinditionSwitchItems(eventPages);
            var itemIdToAppearCinditionSwitchItemsLookup =
                GetItemIdToAppearCinditionSwitchItemsLookup(appearCinditionSwitchItems);

            // 条件情報 - 分岐設定 (仕様書では分岐条件)。
            var ifSwitchEventCommands = GetIfSwitchEventCommands(eventPages);
            var switchIdToIfSwitchEventCommandsLookup = GetSwitchIdToIfSwitchEventCommandsLookup(ifSwitchEventCommands);

            var flagDataModel = Editor.Hierarchy.Hierarchy.databaseManagementService.LoadFlags();
            var itemDataModels = Editor.Hierarchy.Hierarchy.databaseManagementService.LoadItem();

            // スイッチ情報。
            {
                // "スイッチ情報"Foldout。
                var fSwitch = parentVe.Q<Foldout>("fSwitch");
                fSwitch.Clear();

                // 本セクションのスイッチONイベントコマンド列のループ。
                foreach (var sectionSwitchOnEventCommand in
                    GetSectionSwitchOnEventCommands(sectionDataModel, switchOnEventCommands))
                {
                    var sFoldout = new Foldout
                    {
                        text = GetSwitchNameOrRange(flagDataModel, sectionSwitchOnEventCommand.eventCommand)
                    };
                    fSwitch.Add(sFoldout);

                    AddEventInfo(
                        sFoldout,
                        sectionSwitchOnEventCommand.eventPage,
                        new[]
                        {
                            "on",
                            sectionSwitchOnEventCommand.eventPage.EventDataModel.id,
                            sectionSwitchOnEventCommand.eventPage.EventMapDataModel.name,
                            sectionSwitchOnEventCommand.eventPage.MapSubDataModel.Name
                        });

                    foreach (var switchId in
                        GetSwitchIds(flagDataModel, sectionSwitchOnEventCommand.eventCommand))
                    foreach (var switchMatchIfSwitchEventCommand in switchIdToIfSwitchEventCommandsLookup[switchId])
                        AddEventInfo(
                            sFoldout,
                            switchMatchIfSwitchEventCommand.eventPage,
                            new[]
                            {
                                "ref",
                                switchMatchIfSwitchEventCommand.eventPage.EventDataModel.id,
                                switchMatchIfSwitchEventCommand.eventPage.EventMapDataModel.name,
                                switchMatchIfSwitchEventCommand.eventPage.MapSubDataModel.Name,
                                GetSectionName(switchMatchIfSwitchEventCommand.eventPage.EventMapPage.sectionId)
                            });
                }

                // 本セクションのアイテム増イベントコマンド列のループ。
                foreach (var sectionItemAddEventCommand in
                    GetSectionItemAddEventCommands(sectionDataModel, itemAddEventCommands))
                {
                    var itemId = sectionItemAddEventCommand.eventCommand.parameters[(int) Pipi.ItemId];

                    var sFoldout = new Foldout
                    {
                        text = GetItemName(itemDataModels, itemId)
                    };
                    fSwitch.Add(sFoldout);

                    AddEventInfo(
                        sFoldout,
                        sectionItemAddEventCommand.eventPage,
                        new[]
                        {
                            "on",
                            sectionItemAddEventCommand.eventPage.EventDataModel.id,
                            sectionItemAddEventCommand.eventPage.EventMapDataModel.name,
                            sectionItemAddEventCommand.eventPage.MapSubDataModel.Name
                        });

                    foreach (var appearCinditionSwitchItem in itemIdToAppearCinditionSwitchItemsLookup[itemId])
                        AddEventInfo(
                            sFoldout,
                            appearCinditionSwitchItem.eventPage,
                            new[]
                            {
                                "ref",
                                appearCinditionSwitchItem.eventPage.EventDataModel.id,
                                appearCinditionSwitchItem.eventPage.EventMapDataModel.name,
                                appearCinditionSwitchItem.eventPage.MapSubDataModel.Name,
                                GetSectionName(appearCinditionSwitchItem.eventPage.EventMapPage.sectionId)
                            });
                }
            }

            // 条件情報 - 出現条件。
            {
                // "出現条件"Foldout。
                var fAppearanceCondition = parentVe.Q<Foldout>("fAppearanceCondition");
                fAppearanceCondition.Clear();

                // 本セクションの出現条件の有効なスイッチ列のループ。
                foreach (var sectionAppearCinditionSwitch in
                    GetSectionAppearCinditionSwitches(sectionDataModel, appearCinditionSwitches))
                {
                    var switchId = sectionAppearCinditionSwitch.appearSwitch.switchId;

                    var sFoldout = new Foldout {text = GetSwitchName(flagDataModel, switchId)};
                    fAppearanceCondition.Add(sFoldout);

                    AddSwitchOnEvents(sFoldout, switchIdToSwitchOnEventCommandsLookup[switchId]);

                    AddEventInfo(
                        sFoldout,
                        sectionAppearCinditionSwitch.eventPage,
                        new[]
                        {
                            "ref",
                            sectionAppearCinditionSwitch.eventPage.EventDataModel.id,
                            sectionAppearCinditionSwitch.eventPage.EventMapDataModel.name,
                            sectionAppearCinditionSwitch.eventPage.MapSubDataModel.Name
                        });
                }

                // 本セクションの出現条件の有効なスイッチアイテム列のループ。
                foreach (var sectionEventMapPageSwitchItem in
                    GetSectionAppearCinditionSwitchItems(sectionDataModel, appearCinditionSwitchItems))
                {
                    var itemId = sectionEventMapPageSwitchItem.appearSwitchItem.switchItemId;

                    var sFoldout = new Foldout
                    {
                        text = GetItemName(itemDataModels, itemId)
                    };
                    fAppearanceCondition.Add(sFoldout);

                    // 該当アイテム増イベントコマンド列のループ。
                    foreach (var itemAddEventCommand in itemIdToItemAddEventCommandsLookup[itemId])
                        AddEventInfo(
                            sFoldout,
                            itemAddEventCommand.eventPage,
                            new[]
                            {
                                "on",
                                itemAddEventCommand.eventPage.EventDataModel.id,
                                itemAddEventCommand.eventPage.EventMapDataModel.name,
                                itemAddEventCommand.eventPage.MapSubDataModel.Name,
                                GetSectionName(itemAddEventCommand.eventPage.EventMapPage.sectionId)
                            });

                    AddEventInfo(
                        sFoldout,
                        sectionEventMapPageSwitchItem.eventPage,
                        new[]
                        {
                            "ref",
                            sectionEventMapPageSwitchItem.eventPage.EventDataModel.id,
                            sectionEventMapPageSwitchItem.eventPage.EventMapDataModel.name,
                            sectionEventMapPageSwitchItem.eventPage.MapSubDataModel.Name
                        });
                }
            }

            // 条件情報 - 分岐設定 (仕様書では分岐条件)。
            {
                // "分岐設定Foldout。
                var fBranchCondition = parentVe.Q<Foldout>("fBranchCondition");
                fBranchCondition.Clear();
                foreach (var sectionIfSwitchEventCommand in
                    GetSectionIfSwitchEventCommands(sectionDataModel, ifSwitchEventCommands))
                {
                    var switchId = sectionIfSwitchEventCommand.eventCommand.parameters[(int) Fipi.SwitchId];

                    var sFoldout = new Foldout {text = GetSwitchName(flagDataModel, switchId)};
                    fBranchCondition.Add(sFoldout);

                    AddSwitchOnEvents(sFoldout, switchIdToSwitchOnEventCommandsLookup[switchId]);

                    AddEventInfo(
                        sFoldout,
                        sectionIfSwitchEventCommand.eventPage,
                        new[]
                        {
                            "ref",
                            sectionIfSwitchEventCommand.eventPage.EventDataModel.id,
                            sectionIfSwitchEventCommand.eventPage.EventMapDataModel.name,
                            sectionIfSwitchEventCommand.eventPage.MapSubDataModel.Name
                        });
                }
            }

            // 指定スイッチをONにする『スイッチの操作』イベント群を追加。
            static void AddSwitchOnEvents(
                VisualElement parentVe,
                IEnumerable<(EventPage eventPage, EventDataModel.EventCommand eventCommand)>
                    switchMatchSwitchOnEventCommands
            ) {
                foreach (var switchMatchSwitchOnEventCommand in switchMatchSwitchOnEventCommands)
                    AddEventInfo(
                        parentVe,
                        switchMatchSwitchOnEventCommand.eventPage,
                        new[]
                        {
                            "on",
                            switchMatchSwitchOnEventCommand.eventPage.EventDataModel.id,
                            switchMatchSwitchOnEventCommand.eventPage.EventMapDataModel.name,
                            switchMatchSwitchOnEventCommand.eventPage.MapSubDataModel.Name,
                            GetSectionName(switchMatchSwitchOnEventCommand.eventPage.EventMapPage.sectionId)
                        });
            }

            // ひとつのイベントに関する情報のUIを追加。
            static void AddEventInfo(VisualElement parentVe, EventPage eventPage, string[] eventValues) {
                var labels = new Queue<string>(
                    EditorLocalize.LocalizeTexts(new List<string>
                        {"WORD_1186", "WORD_1558", "WORD_1176", "WORD_1559"}));
                var values = new Queue<string>(eventValues);

                var labelLeftMargin = 32;
                var valueLeftMargin = 108;
                var isFirstValue = true;

                while (values.Any())
                {
                    var eventVe = new VisualElement();
                    parentVe.Add(eventVe);
                    eventVe.style.flexDirection = FlexDirection.Row;
                    eventVe.style.alignItems = Align.Center;

                    // "on" または "ref"？
                    if (isFirstValue) eventVe.Add(new Label(EditorLocalize.LocalizeText(values.Dequeue())));

                    VisualElement currentVe;

                    // 項目名。
                    eventVe.Add(currentVe = new Label(labels.Dequeue()));
                    currentVe.style.position = Position.Absolute;
                    currentVe.style.marginLeft = labelLeftMargin;

                    // 項目値。
                    if (isFirstValue)
                    {
                        var button = new Button {text = values.Dequeue()};
                        eventVe.Add(button);
                        RegisterEventIdClicked(button, eventPage);
                        currentVe = button;
                    }
                    else
                    {
                        eventVe.Add(currentVe = new Label(values.Dequeue()));
                    }

                    currentVe.style.position = Position.Absolute;
                    currentVe.style.marginLeft = valueLeftMargin;

                    // 行幅を確保する為にPosition.Absoluteにしない透明ラベルを追加する。
                    eventVe.Add(new Label(" "));

                    isFirstValue = false;
                }
            }

            // イベントidボタンクリック時の処理を登録する。
            static void RegisterEventIdClicked(Button button, EventPage eventPage) {
                button.clicked += () =>
                {
                    MapEditor.MapEditor.LaunchEventEditMode(
                        Editor.Hierarchy.Hierarchy.mapManagementService.LoadMapById(eventPage.EventMapDataModel.mapId),
                        eventPage.EventMapDataModel,
                        eventPage.EventMapPage.page);
                };
            }

            // 範囲指定を考慮しスイッチid列を取得。
            static List<string> GetSwitchIds(FlagDataModel flagDataModel, EventDataModel.EventCommand eventCommand) {
                DebugUtil.Assert((EventEnum) eventCommand.code == EventEnum.EVENT_CODE_GAME_SWITCH);
                if (eventCommand.parameters[(int) Sopi.RangeEnabled] == "0")
                    return new List<string> {eventCommand.parameters[(int) Sopi.SwitchId]};

                var startIndex = int.Parse(eventCommand.parameters[(int) Sopi.StartNumber]) - 1;
                var endIndex = int.Parse(eventCommand.parameters[(int) Sopi.EndNumber]) - 1;
                return flagDataModel.switches.GetRange(startIndex, endIndex - startIndex + 1).Select(sw => sw.id)
                    .ToList();
            }

            static string GetSwitchNameOrRange(
                FlagDataModel flagDataModel,
                EventDataModel.EventCommand eventCommand,
                bool withSwitchId = false
            ) {
                DebugUtil.Assert((EventEnum) eventCommand.code == EventEnum.EVENT_CODE_GAME_SWITCH);
                return eventCommand.parameters[(int) Sopi.RangeEnabled] == "0"
                    ? GetSwitchName(flagDataModel, eventCommand.parameters[(int) Sopi.SwitchId], withSwitchId)
                    : GetSwitchRange(eventCommand);
            }

            static string GetSwitchName(FlagDataModel flagDataModel, string switchId, bool withSwitchId = false) {
                DebugUtil.Assert(!string.IsNullOrWhiteSpace(switchId));
                var sw = flagDataModel.switches.SingleOrDefault(sw => sw.id == switchId);
                var name = sw.name == "" ? $"#{1 + flagDataModel.switches.IndexOf(sw):0000}" : sw.name;

                if (withSwitchId) name += $" ({switchId})";

                return name;
            }

            static string GetSwitchRange(EventDataModel.EventCommand eventCommand) {
                return
                    $"#{int.Parse(eventCommand.parameters[(int) Sopi.StartNumber]):0000} - " +
                    $"#{int.Parse(eventCommand.parameters[(int) Sopi.EndNumber]):0000}";
            }

            static string GetItemName(List<ItemDataModel> itemDataModels, string itemId) {
                return itemDataModels.SingleOrDefault(itemDataModel => itemDataModel.basic.id == itemId)?.basic.name;
            }
        }

        // 『スイッチの操作』イベントのパラメータのインデックス。
        // EventEnum.EVENT_CODE_GAME_SWITCH, class ExecSWitch
        // Game Switch Parameter Index.
        private enum Sopi
        {
            RangeEnabled, // 1:範囲指定を有効にする。
            SwitchId, // 2:範囲指定ではない場合、スイッチのId。
            StartNumber = SwitchId, // 2:範囲指定の場合、スイッチの開始番号。
            EndNumber, // 3:範囲指定の場合、スイッチの終了番号。
            OnOff // 4:スイッチに設定する値を指定(0:ON, 1:OFF)。
        }

        // 『条件分岐』イベントのパラメータのインデックス。
        // EventEnum.EVENT_CODE_FLOW_IF, class ExecFlowIf
        // Flow If Parameter Index.
        private enum Fipi
        {
            ElseEnabled, // 1:条件を満たさない場合の処理を有効にする。
            AndOrEnabled, // 2:複数条件を有効にする。
            AndOr, // 3:複数条件の演算子を指定(0:AND, 1:OR)
            SwitchEnabled, // 4:スイッチの値を比較条件とする。
            SwitchId, // 5:比較するスイッチのid。
            OnOff // 6:スイッチの値と等しいか比較する値を指定(0:ON, 1:OFF)。
        }

        // 『アイテムの増減』イベントのパラメータのインデックス。
        // EventEnum.EVENT_CODE_PARTY_ITEM, class ExecPartyItem
        // Party Item Parameter Index.
        private enum Pipi
        {
            ItemId, // 1:アイテムID
            AddOrSubtraction, // 2:操作(0:増やす 1:減らす)
            ConstantOrVariable, // 3:オペランドタイプ(0:定数 1:変数)
            Value // 4:オペランド(設定値 or 変数番号)
        }

        private class EventPage
        {
            public EventPage(
                EventMapDataModel eventMapDataModel,
                EventMapDataModel.EventMapPage eventMapPage,
                EventDataModel eventDataModel,
                MapSubDataModel mapSubDataModel
            ) {
                EventMapDataModel = eventMapDataModel;
                EventMapPage = eventMapPage;
                EventDataModel = eventDataModel;
                MapSubDataModel = mapSubDataModel;
            }

            public EventMapDataModel EventMapDataModel { get; }
            public EventMapDataModel.EventMapPage EventMapPage { get; }
            public EventDataModel EventDataModel { get; }
            public MapSubDataModel MapSubDataModel { get; }
        }
    }
}