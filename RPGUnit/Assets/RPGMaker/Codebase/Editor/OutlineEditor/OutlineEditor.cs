#if DEBUG
// #define DEBUG_LOG_VERBOSE
#endif

using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement;
using RPGMaker.Codebase.CoreSystem.Service.OutlineManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.OutlineEditor.Command;
using RPGMaker.Codebase.Editor.OutlineEditor.Command.CustomOnGTFCommand;
using RPGMaker.Codebase.Editor.OutlineEditor.Command.DataEdit;
using RPGMaker.Codebase.Editor.OutlineEditor.Command.ViewUpdate;
using RPGMaker.Codebase.Editor.OutlineEditor.GTF;
using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using RPGMaker.Codebase.Editor.OutlineEditor.Window;
using RPGMaker.Codebase.Editor.Common.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.UIElements;
using Region = RPGMaker.Codebase.Editor.Hierarchy.Enum.Region;

namespace RPGMaker.Codebase.Editor.OutlineEditor
{
    public enum Rendered
    {
        None,
        StoryLine,
        SwitchLine
    }

    public class OutlineEditor
    {
        // window
        private static SceneWindow _sceneWindow;

        // core system service
        private static OutlineManagementService _outlineManagementService;
        private static MapManagementService     _mapManagementService;

        // data
        public static OutlineDataModel OutlineDataModel { get; private set; }
        public static List<MapSubDataModel> MapDataModels { 
            get {
                return _outlineManagementService.LoadMaps();
            } 
        }
        public static Dictionary<string, OutlineNodeModel> NodeModelsByUuid { get; private set; }
        public static Dictionary<string, OutlineEdgeModel> EdgeModelsByUuid { get; private set; }

        private static SectionDataModel _copySectionData;

        public static Rendered Rendered { get; private set; }

        public static void Init() {
            _outlineManagementService = new OutlineManagementService();
            _mapManagementService = Editor.Hierarchy.Hierarchy.mapManagementService;

            // アウトラインデータロード
            OutlineDataModel = _outlineManagementService.LoadOutline();

            _sceneWindow = WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.OutlineSceneWindow) as
                SceneWindow;
            _sceneWindow.titleContent =
                new GUIContent(
                    EditorLocalize.LocalizeText("WORD_1563") + " " + EditorLocalize.LocalizeText("WORD_1571"));

            RenderStoryLine();

            // 最初の選択ノードを設定する。
            {
                var initialPartyMapId =
                    Editor.Hierarchy.Hierarchy.databaseManagementService.LoadSystem().initialParty.startMap.mapId;

                // パーティーの初期位置のマップを持つセクションノードid列。
                var nodeIds =
                    OutlineDataModel.Sections.Where(section => section.Maps.Any(map => map.ID == initialPartyMapId))
                        .Select(section => section.ID).ToList();

                // パーティーの初期位置のマップをフィールドとするチャプターノードid列を追加。
                nodeIds.AddRange(
                    OutlineDataModel.Chapters.Where(chapter => chapter.FieldMapSubDataModel?.ID == initialPartyMapId)
                        .Select(chapter => chapter.ID).ToList());

                // スタートノードid列を追加。
                nodeIds.AddRange(OutlineDataModel.Starts.Select(start => start.ID).ToList());

                // ノードid列からノードモデルがnullではない最初のノードidを取得。
                if (NodeModelsByUuid != null)
                {
                    var nodeId = nodeIds.FirstOrDefault(nodeId => NodeModelsByUuid[nodeId] != null);

                    // Unite初回起動時の画面遷移先は、アウトラインエディターかつ、最初のセクションを選択状態とする
                    if (!RpgMakerEditorParam.instance.IsWindowInitialized)
                        // ノードidがnullでなければ、選択状態にする。
                        if (nodeId != null)
                            SelectNode(NodeModelsByUuid[nodeId]);
                }
            }

            DebugLog();
        }

        /**
         * ストーリーラインを描画.
         */
        public static void RenderStoryLine() {
            if (_sceneWindow.CommandDispatcher == null)
                return;

            // ノードモデルマップ初期化
            NodeModelsByUuid = new Dictionary<string, OutlineNodeModel>();
            EdgeModelsByUuid = new Dictionary<string, OutlineEdgeModel>();

            _sceneWindow.Init(OutlineDataModel);

            OutlineDataModel.Starts.ForEach(start =>
            {
                _sceneWindow.CommandDispatcher.Dispatch(new CreateStartNodeCommand(start));
            });
            OutlineDataModel.Chapters.ForEach(chapter =>
            {
                _sceneWindow.CommandDispatcher.Dispatch(new CreateChapterNodeCommand(chapter));
            });
            OutlineDataModel.Sections.ForEach(section =>
            {
                _sceneWindow.CommandDispatcher.Dispatch(new CreateSectionNodeCommand(section));
            });
            OutlineDataModel.Connections.ForEach(connection =>
            {
                _sceneWindow.CommandDispatcher.Dispatch(new CreateConnectionCommand(connection));
            });

            Rendered = Rendered.StoryLine;
        }

        /**
         * スイッチラインを描画.
         */
        public static void RenderSwitchLine() {
            if (_sceneWindow.CommandDispatcher == null)
                return;

            // ノードモデルマップ初期化
            NodeModelsByUuid = new Dictionary<string, OutlineNodeModel>();
            EdgeModelsByUuid = new Dictionary<string, OutlineEdgeModel>();

            _sceneWindow.Init(OutlineDataModel);

            OutlineDataModel.Starts.ForEach(chapter =>
            {
                _sceneWindow.CommandDispatcher.Dispatch(new CreateStartNodeCommand(chapter));
            });
            OutlineDataModel.Chapters.ForEach(chapter =>
            {
                _sceneWindow.CommandDispatcher.Dispatch(new CreateChapterNodeCommand(chapter));
            });
            OutlineDataModel.Sections.ForEach(section =>
            {
                _sceneWindow.CommandDispatcher.Dispatch(new CreateSectionNodeCommand(section));
            });

            SectionCorrelationEventInfo.Instance.SetSectionsRelated();
            var connections = new List<ConnectionDataModel>();
            foreach (var connectionOutSection in OutlineDataModel.Sections)
            foreach (var connectionInSectionId in connectionOutSection.RelatedBySwitchSectionIds)
                connections.Add(new ConnectionDataModel(
                    Guid.NewGuid().ToString(),
                    connectionOutSection.ID,
                    (int) PortDirection.Output,
                    (int) PortOrientation.Horizontal,
                    connectionInSectionId,
                    (int) PortDirection.Input,
                    (int) PortOrientation.Horizontal
                ));

            connections.ForEach(connection =>
            {
                _sceneWindow.CommandDispatcher.Dispatch(new CreateConnectionCommand(connection));
            });

            Rendered = Rendered.SwitchLine;
        }

        public static void SaveOutline(string updateData = null, bool force = false) {
            _outlineManagementService.SaveOutline(OutlineDataModel);
            _ = Hierarchy.Hierarchy.Refresh(Region.Outline, updateData, true, force);
        }

        public static void SetDataModelToInspector(IOutlineDataModel dataModel) {
            Inspector.Inspector.OutlineView(dataModel);
        }

        public static void AddNewStartNode() {
            var newStart = AddNewStartDataModel();
            _sceneWindow.CommandDispatcher.Dispatch(new CreateStartNodeCommand(newStart));
        }

        public static void AddNewChapterNode(int x = 0, int y = 0) {
            var newChapter = AddNewChapterDataModel(x, y);
            _sceneWindow.CommandDispatcher.Dispatch(new CreateChapterNodeCommand(newChapter));
            InvokeSelectableElementAction(
                Hierarchy.Hierarchy.GetChapterFoldout(newChapter.ID));
            DebugLog();
        }

        public static void AddNewSectionNode(string chapterId, int x = 0, int y = 0) {
            var newSection = AddNewSectionDataModel(chapterId, x, y);
            _sceneWindow.CommandDispatcher.Dispatch(new CreateSectionNodeCommand(newSection));
            InvokeSelectableElementAction(
                Hierarchy.Hierarchy.GetSectionFoldout(newSection.ID));
            DebugLog();
        }

        public static void RemoveGraphElementModel(string id) {
            RemoveGraphElementModel(NodeModelsByUuid[id]);
        }

        public static void CopySectionNode(SectionDataModel data) {
            _copySectionData = data;
        }

        public static void RefreshCopySectionNode() {
            _copySectionData = null;
        }

        public static bool CanPasteSectionNode() {
            if (_copySectionData != null && Hierarchy.Hierarchy.GetChapterFoldout(_copySectionData.ChapterID) != null)
                return true;
            return false;
        }

        public static void PasteSectionNode(Vector2 pos) {
            var newSection = SectionDataModel.Create();
            newSection.ChapterID = _copySectionData.ChapterID;
            newSection.Name = _copySectionData.Name;
            newSection.Maps = _copySectionData.Maps;
            newSection.BelongingSwitches = _copySectionData.BelongingSwitches;
            newSection.ReferringSwitches = _copySectionData.ReferringSwitches;
            newSection.RelatedBySwitchSectionIds = _copySectionData.RelatedBySwitchSectionIds;
            newSection.Memo = _copySectionData.Memo;
            newSection.PosX = pos.x;
            newSection.PosY = pos.y;
            OutlineDataModel.Sections.Add(newSection);
            SaveOutline(AbstractHierarchyView.RefreshTypeSectionDuplicate + "," + _copySectionData.ChapterID + "," + newSection.ID);

            _sceneWindow.CommandDispatcher.Dispatch(new CreateSectionNodeCommand(newSection));
            InvokeSelectableElementAction(
                Hierarchy.Hierarchy.GetSectionFoldout(newSection.ID));

            DebugLog();
        }

        public static void RemoveGraphElementModel(IGraphElementModel element) {
            if (element is OutlineNodeModel nodeModel)
            {
                // チャプターの子セクションを削除。
                if (nodeModel is ChapterNodeModel chapterNodeModel)
                    foreach (var sectionDataModel in
                        OutlineDataModel.Sections.Where(section => section.ChapterID == chapterNodeModel.GetEntityID())
                            .ToArray())
                        RemoveGraphElementModel(NodeModelsByUuid[sectionDataModel.ID]);

                // ノードに接続しているコネクションを削除。
                DispatchRemoveNotConnectedConnections(nodeModel.GetEntityID());
            }

            // エレメント自身を削除。
            _sceneWindow.CommandDispatcher.Dispatch(new DeleteElementsCommand(new List<IGraphElementModel> {element}));

            DebugLog();
        }

        public static StartDataModel AddNewStartDataModel() {
            var newStart = StartDataModel.Create();
            OutlineDataModel.Starts.Add(newStart);
            SaveOutline();
            return newStart;
        }

        public static ChapterDataModel AddNewChapterDataModel(int x = 0, int y = 0) {
            var newChapter = ChapterDataModel.Create();
            newChapter.PosX = x;
            newChapter.PosY = y;
            OutlineDataModel.Chapters.Add(newChapter);
            SaveOutline(AbstractHierarchyView.RefreshTypeChapterCreate + "," + newChapter.ID);
            return newChapter;
        }

        public static ChapterDataModel PasteChapterDataModel(int x = 0, int y = 0) {
            var newChapter = ChapterDataModel.Create();
            newChapter.PosX = x;
            newChapter.PosY = y;
            OutlineDataModel.Chapters.Add(newChapter);
            return newChapter;
        }

        public static SectionDataModel AddNewSectionDataModel(string chapterId, int x = 0, int y = 0, bool save = true) {
            var newSection = SectionDataModel.Create();
            newSection.ChapterID = chapterId;
            newSection.PosX = x;
            newSection.PosY = y;
            OutlineDataModel.Sections.Add(newSection);
            if (save)
                SaveOutline(AbstractHierarchyView.RefreshTypeSectionCreate + "," + chapterId + "," + newSection.ID);
            return newSection;
        }

        public static MapDataModel AddNewMapToSectionNode(SectionNodeModel sectionNodeModel) {
            var mapDataModel = MapEditor.MapEditor.LaunchMapEditMode(null);
            AddSectionMap(sectionNodeModel.id, MapDataModels.Single(m => m.ID == mapDataModel.id));
            UpdateSectionView(sectionNodeModel.SectionDataModel);
            DebugLog();
            return mapDataModel;
        }

        public static void SelectElementsCommandProcess(OutlineNodeModel outlineNodeModel) {
            switch (outlineNodeModel)
            {
                case StartNodeModel startNodeModel:
                    outlineNodeModel.SetUpToInspector();
                    break;
                case ChapterNodeModel chapterNodeModel:
                    InvokeSelectableElementAction(
                        Hierarchy.Hierarchy.GetChapterFoldout(chapterNodeModel.GetEntityID()));
                    break;
                case SectionNodeModel sectionNodeModel:
                    InvokeSelectableElementAction(
                        Hierarchy.Hierarchy.GetSectionFoldout(sectionNodeModel.GetEntityID()));
                    break;
            }
        }

        private static void InvokeSelectableElementAction(VisualElement ve) {
            var foldout = ve.Children().ElementAt(0).Children().ElementAt(0) as Foldout;
            Hierarchy.Hierarchy.InvokeSelectableElementAction(foldout);
        }

        public static void SelectNode(OutlineNodeModel node) {
            _sceneWindow.CommandDispatcher.Dispatch(
                new SelectElementsCommandCustom(SelectElementsCommand.SelectionMode.Replace, node));

            // OutlineSceneWindowをアクティブにする
            var window = WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.OutlineSceneWindow);
            window.titleContent = new GUIContent(EditorLocalize.LocalizeText("WORD_1563") + " " +
                                                 EditorLocalize.LocalizeText("WORD_1571"));
        }

        public static void RemoveNodeDataModel(string id) {
            CopyPasteDataUtil.TryRemoveCopiedNode(NodeModelsByUuid[id]);

            NodeModelsByUuid.Remove(id);

            ChapterDataModel chapter = null;
            for (int i = 0; i < OutlineDataModel.Chapters.Count; i++)
                if (OutlineDataModel.Chapters[i].ID == id)
                {
                    chapter = OutlineDataModel.Chapters[i];
                    break;
                }
            if (chapter != null)
            {
                var index = OutlineDataModel.Chapters.IndexOf(chapter);
                OutlineDataModel.Chapters.Remove(chapter);

                // チャプターコードがずれたチャプターノードの表示更新。
                foreach (var chapterToUpdate in OutlineDataModel.Chapters.Skip(index))
                    UpdateChapterView(chapterToUpdate);
            }

            SectionDataModel section = null;
            for (int i = 0; i < OutlineDataModel.Sections.Count; i++)
                if (OutlineDataModel.Sections[i].ID == id)
                {
                    section = OutlineDataModel.Sections[i];
                    break;
                }
            if (section != null)
            {
                var index = OutlineDataModel.Sections.IndexOf(section);
                OutlineDataModel.Sections.Remove(section);

                // セクションコードがずれたセクションノードの表示更新。
                foreach (var sectionToUpdate in OutlineDataModel.Sections.Skip(index))
                    UpdateSectionView(sectionToUpdate);
            }

            // 削除したノードのチャプターまたはセクションを設定してあるマップイベントページのidを消去する。
            {
                var eventManagementService = new EventManagementService();
                var modified = false;
                foreach (var eventMapDataModel in eventManagementService.LoadEventMap())
                foreach (var eventMapPage in eventMapDataModel.pages)
                {
                    if (chapter != null && eventMapPage.chapterId == chapter.ID)
                    {
                        eventMapPage.chapterId = string.Empty;
                        modified = true;
                    }

                    if (section != null && eventMapPage.sectionId == section.ID)
                    {
                        eventMapPage.sectionId = string.Empty;
                        modified = true;
                    }
                }

                if (modified) eventManagementService.SaveEventMap();
            }

            if (chapter != null)
                SaveOutline(AbstractHierarchyView.RefreshTypeChapterDelete + "," + id, true);
            else if (section != null)
                SaveOutline(AbstractHierarchyView.RefreshTypeSectionDelete + "," + section.ChapterID + "," + section.ID, true);
            else
                SaveOutline();
        }

        public static void RemoveConnectionDataModel(string id) {
            EdgeModelsByUuid.Remove(id);

            OutlineDataModel.Connections.RemoveAll(connection => connection.ID == id);

            SaveOutline();
        }

        public static void SelectMap(MapSubDataModel map) {
            if (map == null) return;
            var mapDataModel = _mapManagementService.LoadMapById(map.ID);
            if (mapDataModel == null) return;
            MapEditor.MapEditor.LaunchMapEditMode(mapDataModel);
        }

        public static ConnectionDataModel AddNewConnection(
            string lUuid,
            int lPortDirection,
            int lPortOrientation,
            string rUuid,
            int rPortDirection,
            int rPortOrientation
        ) {
            var newConnection = ConnectionDataModel.Create(lUuid, lPortDirection, lPortOrientation, rUuid,
                rPortDirection,
                rPortOrientation);
            OutlineDataModel.Connections.Add(newConnection);
            SaveOutline();
            return newConnection;
        }

        /// <summary>
        ///     ノードに繋がっていないコネクションを削除する。
        /// </summary>
        /// <param name="nodeIdToDelete">削除するノードのid</param>
        /// <remarks>
        ///     ノード削除後に接続先のノードが存在しないコネクションの削除に使用する想定。
        /// </remarks>
        private static void DispatchRemoveNotConnectedConnections(string nodeIdToDelete = null) {
            // 全ノードのidリスト。
            var allNodeIds = new[]
            {
                OutlineDataModel.Starts.Select(s => s.ID),
                OutlineDataModel.Chapters.Select(s => s.ID),
                OutlineDataModel.Sections.Select(s => s.ID)
            }.SelectMany(_ => _).ToList();

            allNodeIds.Remove(nodeIdToDelete);

            var edgeModels =
                OutlineDataModel.Connections.Where(c => !allNodeIds.Contains(c.LUuid) || !allNodeIds.Contains(c.RUuid))
                    .Select(c => EdgeModelsByUuid[c.ID]).ToList();

            foreach (var edgeModel in edgeModels) RemoveGraphElementModel(edgeModel);
        }

        /// <summary>
        ///     アウトラインエディタが整合性の取れている状態か確認をする。
        /// </summary>
        [Conditional("DEBUG")]
        public static void DebugLog() {
#if DEBUG_LOG_VERBOSE
            DebugUtil.EnableLogFileOutput(true);
#endif

            DebugLogVerbose("[DebugLog]");

            DebugLogVerbose("");

            var warningStrings = new List<string>();

            var allNodeIds = new[]
            {
                OutlineDataModel.Starts.Select(s => s.ID),
                OutlineDataModel.Chapters.Select(s => s.ID),
                OutlineDataModel.Sections.Select(s => s.ID)
            }.SelectMany(_ => _).ToList();

            var allConnectionIds = new[]
            {
                OutlineDataModel.Connections.Select(s => s.ID)
            }.SelectMany(_ => _).ToList();

            DebugLogVerbose("OutlineDataModel.Starts=");
            foreach (var _ in OutlineDataModel.Starts)
            {
                var nodeId = NodeId(_.ID);
                DebugLogVerbose($"    {_.GetType().Name} {{ ID={nodeId} }}");
            }

            DebugLogVerbose("OutlineDataModel.Chapters=");
            foreach (var _ in OutlineDataModel.Chapters)
            {
                var nodeId = NodeId(_.ID);
                DebugLogVerbose($"    {_.GetType().Name} {{ ID={nodeId} }}");
            }

            DebugLogVerbose("OutlineDataModel.Sections=");
            foreach (var _ in OutlineDataModel.Sections)
            {
                var nodeId = NodeId(_.ID);
                DebugLogVerbose($"    {_.GetType().Name} {{ ID={nodeId} }}");
            }

            DebugLogVerbose("OutlineDataModel.Connections=");
            foreach (var _ in OutlineDataModel.Connections)
            {
                var connectionId = ConnectionId(_.ID);
                ;
                var leftConnectionId = ConnectionNodeId(_.LUuid);
                var rightConnectionId = ConnectionNodeId(_.RUuid);
                DebugLogVerbose(string.Concat(
                    $"    {_.GetType().Name} {{ ",
                    $"ID={connectionId}, ",
                    $"LUuid={leftConnectionId}, ",
                    $"RUuid={rightConnectionId} }}"));
            }

            DebugLogVerbose("");

            DebugLogVerbose("OutlineEditor.NodeModelsByUuid=");
            if (NodeModelsByUuid != null)
            {
                foreach (var _ in NodeModelsByUuid)
                {
                    var keyNodeId = KeyNodeId(_.Key);
                    var valueNodeModelId = ValueNodeModelId(_.Value, _.Key);
                    DebugLogVerbose(string.Concat(
                        "    { ",
                        $"ID={keyNodeId} -> ",
                        $"{nameof(OutlineNodeModel)}.GetEntityID()={valueNodeModelId}, GetHashCode()={_.Value.GetHashCode():X} }}"));
                }
            }

            DebugLogVerbose("OutlineEditor.EdgeModelsByUuid=");
            if (EdgeModelsByUuid != null)
            {
                foreach (var _ in EdgeModelsByUuid)
                {
                    var valueConnectionModelId = ValueConnectionModelId(_.Value, _.Key);
                    DebugLogVerbose(string.Concat(
                        "    { ",
                        $"ID={KeyConnectionId(_.Key)} -> ",
                        $"{nameof(OutlineEdgeModel)}.ConnectionDataModel.ID={valueConnectionModelId}, GetHashCode()={_.Value.GetHashCode():X}  }}"));
                }
            }

            DebugLogVerbose("");

            DebugLogVerbose("GraphView.GraphElements=");
            if (_sceneWindow != null && _sceneWindow.GraphView != null && _sceneWindow.GraphView.GraphElements != null)
            {
                foreach (var _ in _sceneWindow.GraphView.GraphElements.ToList())
                    DebugLogVerbose(
                        $"    {_.GetType().Name} {{ GetHashCode()={_.GetHashCode():X}, userData={_.userData} }}");
            }

            DebugLogVerbose("");

            if (warningStrings.Count > 0)
            {
                foreach (var warningString in warningStrings) DebugUtil.LogWarning($"アウトラインエディタ: {warningString}。");

                DebugLogVerbose("");
            }

#if DEBUG_LOG_VERBOSE
            DebugUtil.EnableLogFileOutput(false);
#endif

            string NodeId(string id) {
                var s = id;
                if (NodeModelsByUuid == null || !NodeModelsByUuid.ContainsKey(id))
                {
                    var message = $"{nameof(NodeModelsByUuid)}にID({id})が登録されていません";
                    warningStrings.Add(message);
                    s += " [Warning]";
                }

                return s;
            }

            string ConnectionId(string id) {
                var s = id;
                if (EdgeModelsByUuid == null || !EdgeModelsByUuid.ContainsKey(id))
                {
                    var message = $"{nameof(EdgeModelsByUuid)}にID({id})が登録されていません";
                    warningStrings.Add(message);
                    s += " [Warning]";
                }

                return s;
            }

            string ConnectionNodeId(string id) {
                var s = id;
                if (allNodeIds == null || !allNodeIds.Contains(id))
                {
                    var message = $"Connectionに登録されているID({id})のノード(Start|Chapter|Section)DataModelが存在しません";
                    warningStrings.Add(message);
                    s += " [Warning]";
                }

                return s;
            }

            string KeyNodeId(string id) {
                var s = id;
                if (allNodeIds == null || !allNodeIds.Contains(id))
                {
                    var message =
                        $"{nameof(NodeModelsByUuid)}に登録されているID({id})のノード(Start|Chapter|Section)DataModelが存在しません";
                    warningStrings.Add(message);
                    s += " [Warning]";
                }

                return s;
            }

            string ValueNodeModelId(OutlineNodeModel nodeModel, string keyId) {
                var nodeModelId = nodeModel.GetEntityID();
                var s = nodeModelId;
                if (nodeModelId != keyId)
                {
                    var message = $"キーID({keyId})とノードID({nodeModelId})が違っています";
                    warningStrings.Add(message);
                    s += " [Warning]";
                }

                return s;
            }

            string KeyConnectionId(string id) {
                var s = id;
                if (allConnectionIds == null || !allConnectionIds.Contains(id))
                {
                    var message = $"ID({id})のConnectionDataModelが存在しません";
                    warningStrings.Add(message);
                    s += " [Warning]";
                }

                return s;
            }

            string ValueConnectionModelId(OutlineEdgeModel edgeModel, string keyId) {
                var connectionModelId = edgeModel.ConnectionDataModel.ID;
                var s = connectionModelId;
                if (connectionModelId == null || connectionModelId != keyId)
                {
                    var message = $"キーID({keyId})とコネクションID({connectionModelId})が違っています";
                    warningStrings.Add(message);
                    s += " [Warning]";
                }

                return s;
            }
        }

        [Conditional("DEBUG_LOG_VERBOSE")]
        private static void DebugLogVerbose(object message) {
            DebugUtil.Log(message);
        }

        //--------------------------------------------------------------------------
        //
        // データ編集系
        //
        //--------------------------------------------------------------------------
        public static void ChangeChapterName(string chapterId, string value) {
            ChapterDataModel targetChapter = null;
            for (int i = 0; i < OutlineDataModel.Chapters.Count; i++)
                if (OutlineDataModel.Chapters[i].ID == chapterId)
                {
                    targetChapter = OutlineDataModel.Chapters[i];
                    break;
                }
            targetChapter.Name = value;
            SaveOutline(AbstractHierarchyView.RefreshTypeChapterName + "," + chapterId);

            var nodeModel = (ChapterNodeModel) NodeModelsByUuid[chapterId];
            var command = new SetChapterNameCommand(new[] {nodeModel}, value);
            _sceneWindow.CommandDispatcher.Dispatch(command);
        }

        public static void ChangeChapterSupposedLevelMax(string chapterId, int value) {
            ChapterDataModel targetChapter = null;
            for (int i = 0; i < OutlineDataModel.Chapters.Count; i++)
                if (OutlineDataModel.Chapters[i].ID == chapterId)
                {
                    targetChapter = OutlineDataModel.Chapters[i];
                    break;
                }
            targetChapter.SupposedLevelMax = value;
            SaveOutline(AbstractHierarchyView.RefreshTypeChapterEdit);

            var nodeModel = (ChapterNodeModel) NodeModelsByUuid[chapterId];
            var command = new SetChapterSupposedLevelMaxCommand(new[] {nodeModel}, value);
            _sceneWindow.CommandDispatcher.Dispatch(command);
        }

        public static void ChangeChapterSupposedLevelMin(string chapterId, int value) {
            ChapterDataModel targetChapter = null;
            for (int i = 0; i < OutlineDataModel.Chapters.Count; i++)
                if (OutlineDataModel.Chapters[i].ID == chapterId)
                {
                    targetChapter = OutlineDataModel.Chapters[i];
                    break;
                }
            targetChapter.SupposedLevelMin = value;
            SaveOutline(AbstractHierarchyView.RefreshTypeChapterEdit);

            var nodeModel = (ChapterNodeModel) NodeModelsByUuid[chapterId];
            var command = new SetChapterSupposedLevelMinCommand(new[] {nodeModel}, value);
            _sceneWindow.CommandDispatcher.Dispatch(command);
        }

        public static void ChangeChapterFieldMap(string chapterId, MapSubDataModel mapSubDataModel) {
            ChapterDataModel targetChapter = null;
            for (int i = 0; i < OutlineDataModel.Chapters.Count; i++)
                if (OutlineDataModel.Chapters[i].ID == chapterId)
                {
                    targetChapter = OutlineDataModel.Chapters[i];
                    break;
                }
            targetChapter.FieldMapSubDataModel = mapSubDataModel;
            SaveOutline();

            var nodeModel = (ChapterNodeModel) NodeModelsByUuid[chapterId];
            var command = new SetChapterFieldMapCommand(new[] {nodeModel}, mapSubDataModel);
            _sceneWindow.CommandDispatcher.Dispatch(command);
        }

        public static void ChangeChapterMemo(string chapterId, string value) {
            ChapterDataModel targetChapter = null;
            for (int i = 0; i < OutlineDataModel.Chapters.Count; i++)
                if (OutlineDataModel.Chapters[i].ID == chapterId)
                {
                    targetChapter = OutlineDataModel.Chapters[i];
                    break;
                }
            targetChapter.Memo = value;
            SaveOutline(AbstractHierarchyView.RefreshTypeChapterEdit);

            var nodeModel = (ChapterNodeModel) NodeModelsByUuid[chapterId];
            var command = new SetChapterMemoCommand(new[] {nodeModel}, value);
            _sceneWindow.CommandDispatcher.Dispatch(command);
        }

        public static void ChangeSectionChapterID(string sectionId, string value) {
            SectionDataModel targetSection = null;
            for (int i = 0; i < OutlineDataModel.Sections.Count; i++)
                if (OutlineDataModel.Sections[i].ID == sectionId)
                {
                    targetSection = OutlineDataModel.Sections[i];
                    break;
                }
            var befChapterID = targetSection.ChapterID;
            targetSection.ChapterID = value;

            //セクションに紐づいているイベントを更新する
            var eventMapDataModel = Hierarchy.Hierarchy.eventManagementService.LoadEventMap();
            for(int i = 0;i < eventMapDataModel.Count;i++) {
                for(int j = 0; j < eventMapDataModel[i].pages.Count;j++) {
                    if(targetSection.ID == eventMapDataModel[i].pages[j].sectionId)
                    {
                        eventMapDataModel[i].pages[j].chapterId = value;
                        MapEditor.MapEditor.SaveEventMap(eventMapDataModel[i]);
                    }
                }
            }

            SaveOutline(AbstractHierarchyView.RefreshTypeSectionUpdate + "," + targetSection.ChapterID + "," + targetSection.ID + "," + befChapterID);

            var nodeModel = (SectionNodeModel) NodeModelsByUuid[sectionId];
            var command = new SetSectionChapterIdCommand(new[] {nodeModel}, value);
            _sceneWindow.CommandDispatcher.Dispatch(command);
        }

        public static void ChangeSectionName(string sectionId, string value) {
            SectionDataModel targetSection = null;
            for (int i = 0; i < OutlineDataModel.Sections.Count; i++)
                if (OutlineDataModel.Sections[i].ID == sectionId)
                {
                    targetSection = OutlineDataModel.Sections[i];
                    break;
                }
            targetSection.Name = value;
            SaveOutline(AbstractHierarchyView.RefreshTypeSectionName + "," + targetSection.ChapterID + "," + targetSection.ID);

            var nodeModel = (SectionNodeModel) NodeModelsByUuid[sectionId];
            var command = new SetSectionNameCommand(new[] {nodeModel}, value);
            _sceneWindow.CommandDispatcher.Dispatch(command);
        }

        public static void UpdateSectionMapName(string sectionId, string mapId, string value) {
            SectionDataModel targetSection = null;
            for (int i = 0; i < OutlineDataModel.Sections.Count; i++)
                if (OutlineDataModel.Sections[i].ID == sectionId)
                {
                    targetSection = OutlineDataModel.Sections[i];
                    break;
                }

            for (int i = 0; i < targetSection.Maps.Count; i++)
            {
                if (targetSection.Maps[i].ID == mapId)
                {
                    targetSection.Maps[i].Name = value;
                    break;
                }
            }

            SaveOutline(AbstractHierarchyView.RefreshTypeSectionName + "," + targetSection.ChapterID + "," + targetSection.ID);
        }

        public static void AddSectionMap(string sectionId, MapSubDataModel mapSubDataModel) {
            var nodeModel = (SectionNodeModel) NodeModelsByUuid[sectionId];
            var command = new AddSectionMapCommand(new[] {nodeModel}, mapSubDataModel);
            _sceneWindow.CommandDispatcher.Dispatch(command);
        }

        public static void AddSectionMaps(string sectionId, List<MapSubDataModel> mapSubDataModels) {
            var nodeModel = (SectionNodeModel) NodeModelsByUuid[sectionId];
            for (var i = 0; i < mapSubDataModels.Count; i++)
            {
                var command = new AddSectionMapCommand(new[] {nodeModel}, mapSubDataModels[i]);
                _sceneWindow.CommandDispatcher.Dispatch(command);
            }
        }

        public static void RemoveSectionMap(string sectionId, string mapId, bool refresh = true) {
            SectionDataModel targetSection = null;
            for (int i = 0; i < OutlineDataModel.Sections.Count; i++)
                if (OutlineDataModel.Sections[i].ID == sectionId)
                {
                    targetSection = OutlineDataModel.Sections[i];
                    break;
                }
            targetSection.Maps.RemoveAll(mapEntity => mapEntity.ID == mapId);
            if (refresh)
                SaveOutline(AbstractHierarchyView.RefreshTypeSectionMapRemove + "," + targetSection.ChapterID + "," + targetSection.ID + "," + mapId);
            else
                SaveOutline(AbstractHierarchyView.RefreshTypeSectionEdit);
            var nodeModel = (SectionNodeModel) NodeModelsByUuid[sectionId];
            var command = new RemoveSectionMapCommand(new[] {nodeModel}, mapId);
            _sceneWindow.CommandDispatcher.Dispatch(command);
        }

        public static void ChangeSectionMemo(string sectionId, string value) {
            SectionDataModel targetSection = null;
            for (int i = 0; i < OutlineDataModel.Sections.Count; i++)
                if (OutlineDataModel.Sections[i].ID == sectionId)
                {
                    targetSection = OutlineDataModel.Sections[i];
                    break;
                }
            targetSection.Memo = value;
            SaveOutline(AbstractHierarchyView.RefreshTypeSectionEdit);

            var nodeModel = (SectionNodeModel) NodeModelsByUuid[sectionId];
            var command = new SetSectionMemoCommand(new[] {nodeModel}, value);
            _sceneWindow.CommandDispatcher.Dispatch(command);
        }

        public static void ResetViewScale() {
            ChangeViewScale(float.NaN);
        }

        public static void ZoomInViewScale() {
            ChangeViewScale(+ContentZoomer.DefaultScaleStep);
        }

        public static void ZoomOutViewScale() {
            ChangeViewScale(-ContentZoomer.DefaultScaleStep);
        }

        private static void ChangeViewScale(float delta) {
            var graphView = _sceneWindow.GetGraephView();

            var position = graphView.ViewTransform.position;
            var scale = graphView.ViewTransform.scale;

            var zoomCenter = graphView.ContentViewContainer.layout.size / 2.0f;
            var x = zoomCenter.x + graphView.ContentViewContainer.layout.x;
            var y = zoomCenter.y + graphView.ContentViewContainer.layout.y;

            position += Vector3.Scale(new Vector3(x, y, 0), scale);

            // Apply the new zoom.
            var zoom = float.IsNaN(delta)
                ? 1.0f
                : CSharpUtil.Clamp(GetViewScale() + delta, ContentZoomer.DefaultMinScale,
                    ContentZoomer.DefaultMaxScale);
            scale.x = zoom;
            scale.y = zoom;
            scale.z = 1.0f;

            position -= Vector3.Scale(new Vector3(x, y, 0), scale);

            graphView.CommandDispatcher.Dispatch(new ReframeGraphViewCommand(position, scale));
        }

        private static float GetViewScale() {
            return _sceneWindow.GetGraephView().ViewTransform.scale.y;
        }

        //--------------------------------------------------------------------------
        //
        // その他
        //
        //--------------------------------------------------------------------------
        public static void OpenMapToEdit(MapSubDataModel mapSubDataModel) {
            if (mapSubDataModel == null)
                return;
            var mapDataModel = _mapManagementService.LoadMapById(mapSubDataModel.ID);
            if (mapDataModel == null)
                return;
            MapEditor.MapEditor.LaunchMapEditMode(mapDataModel);
        }

        // 指定マップが設定されたノードの表示更新。
        public static void UpdateNodesView(string mapId) {
            foreach (var chapter in OutlineDataModel.Chapters.Where(c =>
                c.FieldMapSubDataModel != null && c.FieldMapSubDataModel.ID == mapId)) UpdateChapterView(chapter);

            foreach (var section in OutlineDataModel.Sections.Where(s => s.Maps.Any(m => m.ID == mapId)))
                UpdateSectionView(section);
        }

        public static void UpdateChapterView(ChapterDataModel chapterDataModel = null) {
            foreach (var chapter in
                OutlineDataModel.Chapters.Where(c => chapterDataModel == null || c.ID == chapterDataModel.ID))
            {
                var nodeModel = (ChapterNodeModel) NodeModelsByUuid[chapter.ID];
                var command = new UpdateChapterViewCommand(new[] {nodeModel});
                _sceneWindow.CommandDispatcher.Dispatch(command);
            }
        }

        public static void UpdateSectionView(SectionDataModel sectionDataModel = null) {
            foreach (var section in
                OutlineDataModel.Sections.Where(s => sectionDataModel == null || s.ID == sectionDataModel.ID))
            {
                var nodeModel = (SectionNodeModel) NodeModelsByUuid[section.ID];
                var command = new UpdateSectionViewCommand(new[] {nodeModel});
                _sceneWindow.CommandDispatcher.Dispatch(command);
            }
        }

        public static void UpdateStartView() {
            // スタートノードは1つしかないはずだが、チャプターやセクションに合わせて複数持てるようになっている。
            foreach (var start in OutlineDataModel.Starts)
            {
                if (NodeModelsByUuid != null)
                {
                    var nodeModel = (StartNodeModel) NodeModelsByUuid[start.ID];
                    var command = new UpdateStartViewCommand(new[] {nodeModel});
                    _sceneWindow.CommandDispatcher.Dispatch(command);
                }
            }
        }

        public static OeGraphView GetOeGraphView() {
            var window =
                WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.OutlineSceneWindow) as
                    SceneWindow;
            window.titleContent = new GUIContent(EditorLocalize.LocalizeText("WORD_1563") + " " +
                                                 EditorLocalize.LocalizeText("WORD_1571"));
            return window.GetGraephView() as OeGraphView;
        }
    }
}