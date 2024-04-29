using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.GraphView.View
{
    /// <summary>
    /// [キャラクター]-[職業の編集]の能力値のグラフ表示用クラス
    /// </summary>
    public class GraphViewElement : VisualElement
    {
        public static readonly Color HP_GRAPH_COLOR    = new Color(1, 0.8f, 0.4f);
        public static readonly Color MP_GRAPH_COLOR    = new Color(0.3f, 0.6f, 1.0f);
        public static readonly Color ATK_GRAPH_COLOR   = new Color(1.0f, 0.3f, 0.4f);
        public static readonly Color DEF_GRAPH_COLOR   = new Color(0.3f, 0.8f, 0.5f);
        public static readonly Color MATK_GRAPH_COLOR  = new Color(1.0f, 0.3f, 1.0f);
        public static readonly Color MDEF_GRAPH_COLOR  = new Color(0.0f, 0.7f, 0.0f);
        public static readonly Color SPEED_GRAPH_COLOR = new Color(0.0f, 0.8f, 1.0f);
        public static readonly Color LUCK_GRAPH_COLOR  = new Color(1.0f, 0.9f, 0.3f);

        private readonly List<ClassDataModel> _classDataModels;

        private readonly TemplateContainer container;

        public GraphViewElement() {
            var databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
            var treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/RPGMaker/Codebase/Editor/Inspector/GraphView/Asset/GraphViewElement.uxml");
            container = treeAsset.Instantiate();
            hierarchy.Add(container);


            _classDataModels = databaseManagementService.LoadClassCommon();
        }

        public void SetStatusValue(List<int> pram, Color graphColor) {
            VisualElement graph = container.Query<VisualElement>("graph");
            graph.Clear();
            for (var i = 0; i < _classDataModels[0].maxLevel - 1; i++)
            {
                var bar = new VisualElement();
                bar.style.backgroundColor = new StyleColor(graphColor);
                bar.style.height = new StyleLength(new Length(pram[i] / 100, LengthUnit.Percent));
                bar.style.width = new StyleLength(new Length(1, LengthUnit.Percent));
                graph.Add(bar);
            }
        }

        public new class UxmlFactory : UxmlFactory<GraphViewElement, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {
                base.Init(ve, bag, cc);
            }
        }
    }
}