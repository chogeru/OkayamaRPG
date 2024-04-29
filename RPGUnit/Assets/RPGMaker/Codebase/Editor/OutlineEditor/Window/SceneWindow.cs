using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.Editor.OutlineEditor.GTF;
using RPGMaker.Codebase.Editor.OutlineEditor.View;
using System;
using System.Collections.Generic;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Window
{
    /// <summary>
    ///     アウトラインエディター用シーンウィンドウ.
    ///     GraphViewEditorWindowを継承し、ビジュアルスクリプトエディタを実装する.
    /// </summary>
    public class SceneWindow : GraphViewEditorWindow
    {
        private GraphView graphView;
        private readonly List<OeGraphAssetModel> oeGraphAssetModels = new();

        public OeGraphAssetModel Asset { get; private set; }

        protected override void OnEnable() {
            //バージョンアップ時の隙間でのみエラーになる問題の対処
            try {
                base.OnEnable();
            }
            catch (Exception) {
                return;
            }
            EditorToolName = OeStencil.toolName;
            titleContent = new GUIContent(OeStencil.toolName);

            rootVisualElement.Add(new SceneViewNavi(m_GraphContainer));

            m_MainToolbar.style.display = DisplayStyle.None;
            m_SidePanel.style.display = DisplayStyle.None;
            m_ErrorToolbar.style.display = DisplayStyle.None;
            //m_GraphContainer.style.display = DisplayStyle.None;
        }

        public void Init(OutlineDataModel outlineDataModel) {
            DestroyOeGraphAssetModels();

            if (CommandDispatcher == null) return;

            const string assetName = "OeGraphAsset";
            var graphAsset =
                GraphAssetCreationHelpers<OeGraphAssetModel>.CreateInMemoryGraphAsset(typeof(OeStencil), assetName);

            oeGraphAssetModels.Add(graphAsset as OeGraphAssetModel);

            CommandDispatcher.Dispatch(new LoadGraphAssetCommand(graphAsset));
        }

        protected override void OnDestroy()
        {
            DestroyOeGraphAssetModels();
            base.OnDestroy();
        }

        private void DestroyOeGraphAssetModels()
        {
            foreach (var oeGraphAssetModel in oeGraphAssetModels)
            {
                DestroyImmediate(oeGraphAssetModel);
            }

            oeGraphAssetModels.Clear();
        }

        protected override bool CanHandleAssetType(IGraphAssetModel asset) {
            return asset is OeGraphAssetModel;
        }

        protected override GraphView CreateGraphView() {
            graphView = new OeGraphView(this, CommandDispatcher, "Outline Editor Graph View");
            return graphView;
        }

        public GraphView GetGraephView() {
            return graphView;
        }


        protected override GraphToolState CreateInitialState() {
            var prefs = Preferences.CreatePreferences(EditorToolName);
            return new OeGraphState(GUID, prefs);
        }
    }
}