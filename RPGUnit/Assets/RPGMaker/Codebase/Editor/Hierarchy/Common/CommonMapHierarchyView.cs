#define DISABLE_EVENT_COPY_AND_PASTE
// #define TEST_PREVIE_SCENE_AGING

using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement.Repository;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Map.View;
using RPGMaker.Codebase.Editor.MapEditor.Component.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Common
{
    public enum ExecEventType
    {
        None = 0,
        Here
    }

    /// <summary>
    ///     マップヒエラルキーを関連の共用クラス。
    /// </summary>
    public class CommonMapHierarchyView
    {
        private static readonly string copiedEventId = null;

        public static MapDataModel MapDataModel;

        private static MapHierarchyView MapHierarchyView;
        private MapDataModel mapDataModel;

        //EVページ用
        private static EventMapDataModel eventMapDataModel = null;
        private static EventMapDataModel.EventMapPage eventMapPage = null;

        
        //マップ改善用変数
        public string MapId;
        public Foldout MapFoldout;
        public VisualElement MapEdit;
        public VisualElement BattleEdit;
        private Foldout EventFoldout;
        
        private static bool canPasteEvent
        {
            get
            {
                var eventManagementService = new EventManagementService();
                return
                    !string.IsNullOrEmpty(copiedEventId) &&
                    eventManagementService.LoadEventMap().Where(em => em.eventId == copiedEventId).Count() == 1 &&
                    eventManagementService.LoadEvent().Where(e => e.id == copiedEventId).Count() == 1;
            }
        }

        public static string GetMapEditButtonName(string mapId) {
            return mapId + "_edit";
        }

        public static string GetEventPageButtonName(string eventId, int pageNum) {
            return eventId + "_page_" + pageNum;
        }

        /// <summary>
        ///     ヒエラルキーに1つのマップのFoldoutを追加する。
        /// </summary>
        /// <param name="mapEntity">マップ情報</param>
        /// <param name="mapHierarchyInfo">マップヒエラルキー用情報</param>
        /// <remarks>
        ///     以下のヒエラルキー下で使用する。
        ///     ・データベースのマップリスト。
        ///     ・アウトラインのチャプター。
        ///     ・アウトラインのセクション。
        /// </remarks>
        public CommonMapHierarchyView AddMapFoldout(
            MapDataModel mapEntity,
            IMapHierarchyInfo mapHierarchyInfo,
            MapHierarchyView mapHierarchyView = null
        ) {
            if (mapHierarchyView != null)
                MapHierarchyView = mapHierarchyView;
            
            // - - マップ名もしくは"フィールドマップ" Foldout
            var mapFoldout = new Foldout {text = mapHierarchyInfo.Name ?? mapEntity.name};
            mapFoldout.value = false;
            mapFoldout.name = mapEntity.id;
            MapFoldout = mapFoldout;
            BaseClickHandler.ClickEvent(mapFoldout, evt =>
            {
                if (evt != (int) MouseButton.RightMouse) return;
                var menu = new GenericMenu();

                // マップのコピー。
                menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0028")), false,
                    () => { MapDataModel = mapEntity; });

                // マップの削除。
                menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0029")), false, () =>
                {
                    if (EditorUtility.DisplayDialog(
                        EditorLocalize.LocalizeText("WORD_5021"),
                        EditorLocalize.LocalizeText("WORD_5025"),
                        EditorLocalize.LocalizeText("WORD_5023"),
                        EditorLocalize.LocalizeText("WORD_5024")) == true) {
                        MapDataModel = null;
                        MapEditor.MapEditor.RemoveMap(mapEntity);
                        _ = Hierarchy.Refresh(Enum.Region.Map, AbstractHierarchyView.RefreshTypeMapDelete + "," + mapEntity.id);
                    }
                });

                // イベントの新規作成。
                menu.AddItem(
                    new GUIContent(EditorLocalize.LocalizeText("WORD_0010")),
                    false,
                    () =>
                    {
                        MapEditor.MapEditor.LaunchEventPutMode(mapEntity);
                        mapHierarchyInfo.ExecEventType = ExecEventType.Here;
                    });

#if !DISABLE_EVENT_COPY_AND_PASTE
                // イベントの貼り付け。
                menu.AddItem(
                    !canPasteEvent,
                    new GUIContent(EditorLocalize.LocalizeText("WORD_0011")),
                    false,
                    () =>
                    {
                        var eventManagementService = new EventManagementService();

                        // Json経由でクローン作成。
                        var eventMapDataModel = JsonHelper.Clone(
                            eventManagementService.LoadEventMap().Single(em => em.eventId == copiedEventId));
                        var eventDataModel = JsonHelper.Clone(
                            eventManagementService.LoadEvent().Single(e => e.id == copiedEventId));

                        // イベントidを新規生成したものに変更。
                        eventMapDataModel.eventId =
                        eventDataModel.id = System.Guid.NewGuid().ToString();

                        // マップidを貼り付け位置のものに変更。
                        eventMapDataModel.mapId = mapEntity.id;

                        // セーブ。
                        eventManagementService.SaveEventMap(eventMapDataModel);
                        eventManagementService.SaveEvent(eventDataModel);

                        // Hierarchy更新。
                        Hierarchy.Refresh(Enum.Region.Map);
                   });
#endif

                menu.ShowAsContext();
            });

            mapHierarchyInfo.ParentVe.Add(mapFoldout);
            mapHierarchyInfo.ParentClass.SetFoldout(mapFoldout.name, mapFoldout);

            //Foldoutの中身を作成
            return CreateMapFoldoutContents(mapEntity, mapHierarchyInfo, mapHierarchyView, mapFoldout);
        }

        /// <summary>
        ///     既にあるMapFoldoutの中身を刷新する
        /// </summary>
        /// <param name="mapEntity">マップ情報</param>
        /// <param name="mapHierarchyInfo">マップヒエラルキー用情報</param>
        /// <remarks>
        ///     以下のヒエラルキー下で使用する。
        ///     ・データベースのマップリスト。
        ///     ・アウトラインのチャプター。
        ///     ・アウトラインのセクション。
        /// </remarks>
        public void EditMapFoldout(
            MapDataModel mapEntity,
            IMapHierarchyInfo mapHierarchyInfo,
            MapHierarchyView mapHierarchyView = null
        ) {
            if (mapHierarchyView != null)
                MapHierarchyView = mapHierarchyView;


            //既に作成済みのFoldoutを取得
            var mapFoldout = mapHierarchyInfo.ParentClass.GetFoldout(mapEntity.id);
            mapFoldout.text = mapHierarchyInfo.Name ?? mapEntity.name;
            mapFoldout.Clear();

            //Foldoutの中身を再生成
            CreateMapFoldoutContents(mapEntity, mapHierarchyInfo, mapHierarchyView, mapFoldout);
        }

        /// <summary>
        ///     マップのFoldout内のコンテンツを作成する。
        /// </summary>
        /// <param name="mapEntity">マップ情報</param>
        /// <param name="mapHierarchyInfo">マップヒエラルキー用情報</param>
        /// <remarks>
        ///     以下のヒエラルキー下で使用する。
        ///     ・データベースのマップリスト。
        ///     ・アウトラインのチャプター。
        ///     ・アウトラインのセクション。
        /// </remarks>
        public CommonMapHierarchyView CreateMapFoldoutContents(
            MapDataModel mapEntity,
            IMapHierarchyInfo mapHierarchyInfo,
            MapHierarchyView mapHierarchyView,
            Foldout mapFoldout
        ) {
            MapId = mapEntity.id;
            mapDataModel = mapEntity;

            // - - - マップ編集ボタン
            MapDataModel mapEntityWork = mapEntity;
            var btnEditMap = new Button { text = EditorLocalize.LocalizeText("WORD_0012") };
            btnEditMap.name = GetMapEditButtonName(mapEntity.id);
            btnEditMap.AddToClassList("button-transparent");
            btnEditMap.AddToClassList("AnalyticsTag__page_view__map_edit");
            Hierarchy.AddSelectableElementAndAction(btnEditMap,
                () => { MapEditor.MapEditor.LaunchMapEditMode(Hierarchy.mapManagementService.LoadMapById(mapEntity.id)); });
            btnEditMap.clicked += () => { Hierarchy.InvokeSelectableElementAction(btnEditMap); };
            mapFoldout.Add(btnEditMap);

            MapEdit = btnEditMap;

#if TEST_PREVIE_SCENE_AGING
            DebugUtil.Execution(() =>
            {
                btnEditMap.clicked += () =>
                {
                    DebugUtil.EditorRepeatExecution(
                        () => { Hierarchy.InvokeSelectableElementAction(btnEditMap); },
                        "マップ編集",
                        100,
                        0.1f);
                };
            });
#endif

            // - - - バトル編集ボタン
            var btnEditBattle = new Button { text = EditorLocalize.LocalizeText("WORD_0013") };
            btnEditBattle.name = mapEntity.id + "_battle";
            btnEditBattle.AddToClassList("button-transparent");
            btnEditBattle.AddToClassList("AnalyticsTag__page_view__map_battle_edit");
            Hierarchy.AddSelectableElementAndAction(btnEditBattle,
                () => { MapEditor.MapEditor.LaunchBattleEditMode(mapEntity); });
            btnEditBattle.clicked += () => { Hierarchy.InvokeSelectableElementAction(btnEditBattle); };
            mapFoldout.Add(btnEditBattle);

            BattleEdit = btnEditBattle;

#if TEST_PREVIE_SCENE_AGING
            DebugUtil.Execution(() =>
            {
                btnEditBattle.clicked += () =>
                {
                    DebugUtil.EditorRepeatExecution(
                        () => { Hierarchy.InvokeSelectableElementAction(btnEditBattle); },
                        "バトル編集",
                        100,
                        0.1f);
                };
            });
#endif

            // - - - "イベント" Foldout
            VisualElement foldoutElement = new VisualElement();

            EventFoldout = new Foldout();
            EventFoldout.AddToClassList("AnalyticsTag__page_view__map_event");
            EventFoldout.name = mapFoldout.name + "_event";
            var foldoutLabel = new Label {text = EditorLocalize.LocalizeText("WORD_0014")};
            foldoutLabel.name = EventFoldout.name + "_label";
            foldoutLabel.style.position = Position.Absolute;
            foldoutLabel.style.left = 35f;
            foldoutLabel.style.right = 2f;
            foldoutLabel.style.overflow = Overflow.Hidden;
            foldoutLabel.style.paddingTop = 2f;
            foldoutElement.Add(EventFoldout);
            foldoutElement.Add(foldoutLabel);
            mapFoldout.Add(foldoutElement);
            mapHierarchyInfo.ParentClass.SetFoldout(EventFoldout.name, EventFoldout);
            
            foldoutLabel.RegisterCallback<ClickEvent>(evt =>
            {
                Inspector.Inspector.Clear(true);
                Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(EventFoldout);
                MapEditor.MapEditor.LaunchEventPutMode(mapEntity);
                mapHierarchyInfo.ExecEventType = ExecEventType.Here;
            });

            BaseClickHandler.ClickEvent(foldoutLabel, (evt) =>
            {
                if (evt != (int) MouseButton.RightMouse) return;
                var menu = new GenericMenu(); 
                
                // イベントの新規作成。
                menu.AddItem(
                    new GUIContent(EditorLocalize.LocalizeText("WORD_0010")),
                    false,
                    () =>
                    {
                        MapEditor.MapEditor.LaunchEventPutMode(mapEntity);
                        mapHierarchyInfo.ExecEventType = ExecEventType.Here;
                    });
                menu.ShowAsContext();
            });

            // - - - - イベント一覧
            CreateEventContent(mapHierarchyInfo);

            return this;
        }

        public void CreateEventContent(IMapHierarchyInfo mapHierarchyInfo) {
            EventFoldout.Clear();

            var localText16 = EditorLocalize.LocalizeText("WORD_0016");
            var localText570 = EditorLocalize.LocalizeText("WORD_0570");
            var localText571 = EditorLocalize.LocalizeText("WORD_0571");
            var localText572 = EditorLocalize.LocalizeText("WORD_0572");
            var localText573 = EditorLocalize.LocalizeText("WORD_0573");
            var localText19 = EditorLocalize.LocalizeText("WORD_0019");

           var matchingMapDataList = mapHierarchyInfo.EventMapDataModels.Where(m => m.mapId == MapId).ToList();

            for (int i = 0; i < matchingMapDataList.Count; i++)
            {
                if (matchingMapDataList[i].pages.Count > 0)
                {
                    var eventEntity = matchingMapDataList[i];
                    var copyPageNum = 0;

                    // イベント名Foldout（ページを内包）
                    var eventFoldout = new Foldout
                    {
                        text = !string.IsNullOrEmpty(eventEntity.name)
                            ? eventEntity.name
                            : $"EV{matchingMapDataList.IndexOf(eventEntity) + 1:000}"
                    };
                    int eventIndex = matchingMapDataList.IndexOf(eventEntity);
                    eventFoldout.name = EventFoldout.name + "_" + eventEntity.eventId;
                    mapHierarchyInfo.ParentClass.SetFoldout(eventFoldout.name, eventFoldout);
                    EventFoldout.Add(eventFoldout);

                    BaseClickHandler.ClickEvent(eventFoldout, evt =>
                    {
                        if (evt != (int) MouseButton.RightMouse) return;
                        var menu = new GenericMenu();
        #if !DISABLE_EVENT_COPY_AND_PASTE
                        // イベントのコピー。
                        menu.AddItem(
                            new GUIContent(EditorLocalize.LocalizeText("WORD_0015")),
                            false,
                            () =>
                            {
                                copiedEventId = eventEntity.eventId;
                            });
        #endif

                        // EVページの新規作成。
                        menu.AddItem(new GUIContent(localText570), false, () =>
                        {
                            mapHierarchyInfo.ExecEventType = ExecEventType.Here;
                            var pageNum = eventEntity.pages[eventEntity.pages.Count - 1].page + 1;
                            MapEditor.MapEditor.CreatePage(eventEntity, pageNum, 1);

                            // ヒエラルキーの該当イベントページを選択状態にする。
                            _ = Hierarchy.Refresh(Enum.Region.Map, AbstractHierarchyView.RefreshTypeEventEvCreate + "," + MapId, false);
                            Hierarchy.SelectButton(GetEventPageButtonName(eventEntity.eventId, pageNum));
                        });
                        // EVページの貼り付け
                        if (eventMapDataModel != null)
                        {

                            menu.AddItem(new GUIContent(localText571), false, () =>
                            {
                                if (eventMapDataModel != null)
                                {
                                    if (eventEntity != eventMapDataModel)
                                        copyPageNum = eventEntity.pages[eventEntity.pages.Count - 1].page + 1;

                                    MapEditor.MapEditor.CopyPage(eventMapDataModel, eventMapPage, copyPageNum, 1,
                                        eventEntity);
                                    _ = Hierarchy.Refresh(Enum.Region.Map, AbstractHierarchyView.RefreshTypeEvenEvDuplicate + "," + MapId, false);
                                }
                            });

                        }
                        else
                        {
                            menu.AddDisabledItem(new GUIContent(localText571));
                        }
                        // イベントの削除。
                        menu.AddItem(
                                new GUIContent(localText16),
                                false,
                        () =>
                        {
                            if (EditorUtility.DisplayDialog(
                                EditorLocalize.LocalizeText("WORD_5021"),
                                EditorLocalize.LocalizeText("WORD_5022"),
                                EditorLocalize.LocalizeText("WORD_5023"),
                                EditorLocalize.LocalizeText("WORD_5024")) == true)
                            {
                                var activeButtonName = GetActiveButtonNameByDeletingEvent(eventEntity);

                                EventEditCanvas.CopyEventId = "";
                                MapEditor.MapEditor.DeleteEventMap(eventEntity);
                                _ = Hierarchy.Refresh(Enum.Region.Map, AbstractHierarchyView.RefreshTypeEventDelete + "," + MapId, false);
                                Hierarchy.CompensateActiveButton(activeButtonName);
                            }             
                        });
                        menu.ShowAsContext();
                    });

                    if (eventEntity.pages == null || eventEntity.pages.Count == 0) continue;

                    var visualElements = new List<VisualElement>();
                    // ページ一覧（クリックするとイベントエディタを開く）
                    for (int i2 = 0; i2 < eventEntity.pages.Count; i2++)
                    {
                        var page = eventEntity.pages[i2];
                        var pageNum = page.page;
                        var btnLoadEventPage = new Button
                        {
                            text = localText19 + (page.page + 1)
                        };
                        btnLoadEventPage.name = GetEventPageButtonName(eventEntity.eventId, pageNum);
                        btnLoadEventPage.AddToClassList("button-transparent");
                        Hierarchy.AddSelectableElementAndAction(btnLoadEventPage,
                            () => { MapEditor.MapEditor.LaunchEventEditMode(mapDataModel, eventEntity, pageNum); });
                        btnLoadEventPage.AddToClassList(Hierarchy.ButtonTypeTag_WithEventSubWindows);
                        visualElements.Add(btnLoadEventPage);
        #if TEST_PREVIE_SCENE_AGING
                            DebugUtil.Execution(() =>
                            {
                                btnLoadEventPage.clicked += () =>
                                {
                                    DebugUtil.EditorRepeatExecution(
                                        () => { MapEditor.MapEditor.LaunchEventEditMode(mapDataModel, eventEntity, pageNum); },
                                        $"ページ{page.page + 1}",
                                        100,
                                        0.1f);
                                };
                            });
        #endif
                        BaseClickHandler.ClickEvent(btnLoadEventPage, evt =>
                        {
                            if (evt == (int) MouseButton.RightMouse)
                            {
                                var menu = new GenericMenu();

                                // EVページのコピー。
                                menu.AddItem(new GUIContent(localText572), false,
                                        () =>
                                        {
                                            mapHierarchyInfo.ExecEventType = ExecEventType.Here;
                                            eventMapDataModel = eventEntity;
                                            eventMapPage = page;
                                            copyPageNum = eventEntity.pages[eventEntity.pages.Count - 1].page + 1;
                                        });

                                // EVページの削除。
                                menu.AddItem(new GUIContent(localText573), false,
                                        () =>
                                        {
                                            var activeButtonName = GetActiveButtonNameByDeletingEventPage(eventEntity, pageNum);

                                            eventMapDataModel = null;
                                            mapHierarchyInfo.ExecEventType = ExecEventType.Here;
                                            MapEditor.MapEditor.DeletePage(eventEntity, pageNum);
                                            _ = Hierarchy.Refresh(Enum.Region.Map, AbstractHierarchyView.RefreshTypeEventEvDelete + "," + MapId, false);
                                            Hierarchy.CompensateActiveButton(activeButtonName);
                                        });

                                menu.ShowAsContext();
                            }
                            else
                            {
                                Hierarchy.InvokeSelectableElementAction(btnLoadEventPage);
                            }
                        });
                        eventFoldout.Add(btnLoadEventPage);
                    }
                }
            }
        }

        /// <summary>
        ///     ヒエラルキーに1つのサンプルマップのFoldoutを追加する。
        /// </summary>
        /// <param name="mapEntity">マップ情報</param>
        /// <param name="mapHierarchyInfo">マップヒエラルキー用情報</param>
        public static void AddMapSampleFoldout(
            MapDataModel mapEntity,
            IMapHierarchyInfo mapHierarchyInfo
        ) {
            // - - - マップビューボタン
            var btnEditMap = new Button {text = mapHierarchyInfo.Name ?? mapEntity.name};
            btnEditMap.name = GetMapEditButtonName(mapEntity.id);
            btnEditMap.AddToClassList("button-transparent");
            btnEditMap.AddToClassList("AnalyticsTag__page_view__map_edit");
            Hierarchy.AddSelectableElementAndAction(btnEditMap,
                () => { MapEditor.MapEditor.LaunchMapPreviewMode(mapEntity); });
            btnEditMap.clicked += () => { Hierarchy.InvokeSelectableElementAction(btnEditMap); };
            mapHierarchyInfo.ParentVe.Add(btnEditMap);

            BaseClickHandler.ClickEvent(btnEditMap, evt =>
            {
                if (evt != (int) MouseButton.RightMouse) return;
                var menu = new GenericMenu();

                // サンプルマップから作成
                menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_3009")), false, () =>
                {
                    // 指定のマップデータのIDのみ変更&Prefab複製
                    var mapData = MapDataModel.CopyData(mapEntity);
                    mapData.id = Guid.NewGuid().ToString();
                    if (Hierarchy.mapManagementService.LoadMaps().Count > 0)
                        mapData.index = Hierarchy.mapManagementService.LoadMaps()[Hierarchy.mapManagementService.LoadMaps().Count - 1].index++;
                    else
                        mapData.index = 0;
                    MapDataModel.CopyMapPrefabForEditor(mapEntity, mapData.id);

                    // 名前設定
                    mapData.name += " - " + EditorLocalize.LocalizeText("WORD_1462");
                    mapData.name = MapEditor.MapEditor.MapNameDuplicateCheck(mapData.name);

                    // データ更新
                    // 新規作成時は、Prefabを強制的に保存する
                    MapEditor.MapEditor.SaveMap(mapData, MapRepository.SaveType.SAVE_PREFAB_FORCE);
                    MapEditor.MapEditor.ReloadMap(mapData);
                    MapEditor.MapEditor.LaunchMapEditMode(mapData);
                    MapHierarchyView.SetInit();
                    _ = Hierarchy.Refresh(Enum.Region.Map);
                });

                menu.ShowAsContext();
            });
        }

        // イベントページ削除によりアクティブボタンがなくなった場合に設定するアクティブボタンの名を取得。
        private static string GetActiveButtonNameByDeletingEventPage(
            EventMapDataModel deleteEventMapDataModel,
            int deletePageNum
        ) {
            //今のページ番号が、現在のページ数-1よりも小さい場合は、同じページ番号になる予定のところを選択する
            if (deletePageNum < deleteEventMapDataModel.pages.Count - 1)
                return GetEventPageButtonName(deleteEventMapDataModel.eventId, deletePageNum);
            //今のページ番号が、現在のページ数-1以上だった場合は、ページ番号を1小さくしたところを選択する
            //ただし、削除したページが0の場合を除く
            //これらの条件に合致しないケースは、イベントページが1つもなくなった場合
            if (deletePageNum - 1 >= 0)
                return GetEventPageButtonName(deleteEventMapDataModel.eventId, deletePageNum - 1);

            return GetActiveButtonNameByDeletingEvent(deleteEventMapDataModel);
        }

        // イベント削除によりアクティブボタンがなくなった場合に設定するアクティブボタンの名を取得。
        private static string GetActiveButtonNameByDeletingEvent(EventMapDataModel deleteEventMapDataModel) {
            var eventManagementService = new EventManagementService();

            var eventMapDataModels = eventManagementService.LoadEventMap().Where(eventMapDataModel =>
                eventMapDataModel.mapId == deleteEventMapDataModel.mapId);

            var eventMapDataModelIndex = eventMapDataModels
                .Select((eventMapDataModel, index) => new {eventMapDataModel, index})
                .Where(pair =>
                    pair.eventMapDataModel.eventId == deleteEventMapDataModel.eventId)
                .Select(pair => pair.index + 1).SingleOrDefault() - 1;

            return
                // 1つ後のイベントに移動？
                eventMapDataModelIndex + 1 < eventMapDataModels.Count() ? GetOffsetEventPageButtonName(+1) :
                // 1つ前のイベントに移動？
                eventMapDataModelIndex - 1 >= 0 ? GetOffsetEventPageButtonName(-1) :
                // 『マップ編集』ボタンへ移動。
                GetMapEditButtonName(deleteEventMapDataModel.mapId);

            // 1つ後のイベントの先頭のイベントページ または 1つ前のイベントの末尾のイベントページ のボタン名を取得。
            string GetOffsetEventPageButtonName(int offset) {
                var eventMapDataModel = eventMapDataModels.ElementAt(eventMapDataModelIndex + offset);
                return GetEventPageButtonName(
                    eventMapDataModel.eventId,
                    offset > 0 ? 0 : eventMapDataModel.pages.Count - 1);
            }
        }
    }

    /// <summary>
    ///     マップヒエラルキー情報インターフェイス。
    /// </summary>
    public interface IMapHierarchyInfo
    {
        VisualElement ParentVe { get; }
        AbstractHierarchyView ParentClass { get; }
        string Name { get; }
        Dictionary<string, Foldout> MapFoldouts { get; }
        Dictionary<string, Foldout> EventFoldouts { get; }
        ExecEventType ExecEventType { get; set; }
        List<EventMapDataModel> EventMapDataModels { get; }
        void RefreshMapHierarchy(string[] mapIds = null);
        void RefreshEventHierarchy(string updateData);
    }
}