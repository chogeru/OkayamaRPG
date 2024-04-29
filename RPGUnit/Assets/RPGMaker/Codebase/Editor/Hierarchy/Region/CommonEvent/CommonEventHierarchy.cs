using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventCommon;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Region.CommonEvent.View;
using RPGMaker.Codebase.Editor.MapEditor.Window.EventEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.CommonEvent
{
    /// <summary>
    /// コモンイベントのHierarchy
    /// </summary>
    public class CommonEventHierarchy : AbstractHierarchy
    {
        private List<EventCommonDataModel> _eventCommonDataModels;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CommonEventHierarchy() {
            View = new CommonEventHierarchyView(this);
        }

        /// <summary>
        /// View
        /// </summary>
        public CommonEventHierarchyView View { get; }

        /// <summary>
        /// データの読込
        /// </summary>
        override protected void LoadData() {
            base.LoadData();
            _eventCommonDataModels = eventManagementService.LoadEventCommon();
        }

        /// <summary>
        /// Viewの更新
        /// </summary>
        protected override void UpdateView(string updateData = null) {
            base.UpdateView();
            View.Refresh(_eventCommonDataModels, Hierarchy.ButtonTypeTag_WithEventSubWindows);
        }

        /// <summary>
        /// コモンイベントのInspector表示
        /// </summary>
        /// <param name="eventCommonDataModel"></param>
        public async void OpenEventCommonInspector(EventCommonDataModel eventCommonDataModel) {
            Inspector.Inspector.CommonEventEditView(eventCommonDataModel);
            var currentMainWindowId = GetCurrentMainWindowId();

            var eventEntity = eventManagementService.LoadEventById(eventCommonDataModel.eventId);
            MapEditor.MapEditor.ChangeEvent(eventEntity);
            if (currentMainWindowId != WindowLayoutManager.WindowLayoutId.None)
            {
                var eventEditWindow = WindowLayoutManager.SwitchWindows(
                    WindowLayoutManager.WindowLayoutId.MapEventEditWindow,
                    currentMainWindowId
                ) as EventEditWindow;

                eventEditWindow.titleContent =
                    new GUIContent(EditorLocalize.LocalizeText("WORD_1563") + " " + EditorLocalize.LocalizeText("WORD_1564"));
            }

            await Task.Delay(50);

            // 『イベントコマンド』枠を開く。
            var commandSettingWindow = (CommandSettingWindow)WindowLayoutManager.GetActiveWindow(
                WindowLayoutManager.WindowLayoutId.MapEventCommandSettingWindow);
            if (commandSettingWindow == null)
            {
                commandSettingWindow = WindowLayoutManager.OpenAndDockWindow(
                    WindowLayoutManager.WindowLayoutId.MapEventCommandSettingWindow,
                    WindowLayoutManager.WindowLayoutId.DatabaseInspectorWindow,
                    Docker.DockPosition.Bottom
                ) as CommandSettingWindow;
                commandSettingWindow.titleContent =
                    new GUIContent(EditorLocalize.LocalizeWindowTitle("WORD_1570"));
            }

            // 『イベント実行内容』枠を開く。
            // (『イベントコマンド』枠を参照しているので、『イベントコマンド』枠の後で開く)
            var executionContentsWindow = (ExecutionContentsWindow)WindowLayoutManager.GetActiveWindow(
                WindowLayoutManager.WindowLayoutId.MapEventExecutionContentsWindow);
            if (executionContentsWindow == null)
            {
                executionContentsWindow = WindowLayoutManager.OpenAndDockWindow(
                    WindowLayoutManager.WindowLayoutId.MapEventExecutionContentsWindow,
                    WindowLayoutManager.WindowLayoutId.MapEventEditWindow,
                    Docker.DockPosition.Bottom
                ) as ExecutionContentsWindow;
                executionContentsWindow.titleContent =
                    new GUIContent(EditorLocalize.LocalizeWindowTitle("WORD_1569"));
            }

            executionContentsWindow.Init(eventEntity, true, ExecutionContentsWindow.EventType.Common);

            // MapEventEditWindowをアクティブにする
            WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.MapEventEditWindow);
        }

        /// <summary>
        /// コモンイベントの新規作成
        /// </summary>
        public void CreateEventCommonDataModel() {
            var newEventModel = EventDataModel.CreateDefault();
            eventManagementService.SaveEvent(newEventModel);

            var commonName =  "#" + string.Format("{0:D4}", _eventCommonDataModels.Count + 1) + "　" + 
                              EditorLocalize.LocalizeText("WORD_1518");
            
            var newModel = EventCommonDataModel.CreateDefault(newEventModel.id, commonName);
            newModel.eventId = newEventModel.id;
            newModel.conditions.Add(new EventCommonDataModel.EventCommonCondition(0, ""));
            eventManagementService.SaveEventCommon(newModel);
            Refresh();
        }

        /// <summary>
        /// コモンイベントのコピー＆貼り付け処理
        /// </summary>
        /// <param name="eventCommonDataModel"></param>
        public void DuplicateEventCommonDataModel(EventCommonDataModel eventCommonDataModel) {
            //EventCommonContent側のデータ複製
            //今回複製するコモンイベントを探す
            var eventDataModel = eventManagementService.LoadEventById(eventCommonDataModel.eventId);
            //複製
            var duplicatedData = eventDataModel.Clone();
            //新規IDにする
            duplicatedData.id = Guid.NewGuid().ToString();
            
            //EventDataModelのセーブ
            eventManagementService.SaveEvent(duplicatedData);

            //Inspector側のデータ複製
            //コモンイベントのInspectorのクローン
            var duplicated = eventCommonDataModel.Clone();
            //同じIDだと良くないので修正
            duplicated.eventId = duplicatedData.id;
            var eventCommonNames = eventManagementService.LoadEventCommon().Select(e => e.name).ToList();
            duplicated.name = CreateDuplicateName(eventCommonNames, duplicated.name);

            //コモンイベントのInspectorセーブ
            eventManagementService.SaveEventCommon(duplicated);
            Refresh();
        }

        /// <summary>
        /// コモンイベントの削除
        /// </summary>
        /// <param name="eventCommonDataModel"></param>
        public void DeleteEventCommonDataModel(EventCommonDataModel eventCommonDataModel) {
            eventManagementService.DeleteCommonEvent(eventCommonDataModel);
            Refresh();
        }

        private new static WindowLayoutManager.WindowLayoutId GetCurrentMainWindowId() {
            var ids = new List<WindowLayoutManager.WindowLayoutId>()
            {
                WindowLayoutManager.WindowLayoutId.MapEditWindow,
                WindowLayoutManager.WindowLayoutId.MapBattleEditWindow,
                WindowLayoutManager.WindowLayoutId.MapTileEditWindow,
                WindowLayoutManager.WindowLayoutId.MapTileGroupEditWindow,
                WindowLayoutManager.WindowLayoutId.MapEventEditWindow
            };

            for (int i = 0; i < ids.Count; i++)
                if (WindowLayoutManager.IsActiveWindow(ids[i]))
                    return ids[i];

            return WindowLayoutManager.WindowLayoutId.None;
        }
    }
}