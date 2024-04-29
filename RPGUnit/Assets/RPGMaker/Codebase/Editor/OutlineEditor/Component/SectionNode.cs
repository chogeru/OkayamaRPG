using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.MapEditor.Window.MapEdit;
using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using System.Linq;
using UnityEditor;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Component
{
    public class SectionNode : Node
    {
        private const string UssClassName = "oe-node";

        protected override void BuildPartList() {
            PartList.AppendPart(VerticalPortContainerPart.Create("top-port-container-part", PortDirection.Input, Model,
                this,
                UssClassName));
            PartList.AppendPart(SectionNodePart.Create("chapter-container-part", Model, this, UssClassName));
            PartList.AppendPart(InOutPortContainerPart.Create("side-port-container-part", Model, this, UssClassName));
            PartList.AppendPart(VerticalPortContainerPart.Create("bottom-port-container-part", PortDirection.Output,
                Model,
                this, UssClassName));
        }
    }

    public class SectionNodePart : BaseModelUIPart
    {
        private const string        SectionNodeColor = "#bfabbd";
        private const string        NoImageFilePath = "Assets/RPGMaker/Codebase/Editor/OutlineEditor/Asset/NoImage.png";
        private const string        UssClassName = "oe-node-part";
        private       VisualElement _baseContainer;
        private       VisualElement _block1;
        private       VisualElement _block2;
        private       VisualElement _block3;
        private       VisualElement _block4;
        private       VisualElement _block5;
        private       Label         _codeLabel;
        private       Label         _fieldMapNameLabel;
        private       VisualElement _mapImage;
        private       Label         _nameLabel;

        private VisualElement _parentContainer;

        private SectionNodePart(
            string name,
            IGraphElementModel model,
            IModelUI ownerElement,
            string parentClassName
        )
            : base(name, model, ownerElement, parentClassName) {
        }

        public override VisualElement Root => _baseContainer;

        public static SectionNodePart Create(
            string name,
            IGraphElementModel model,
            IModelUI modelUI,
            string parentClassName
        ) {
            return model is INodeModel ? new SectionNodePart(name, model, modelUI, parentClassName) : null;
        }

        protected override void BuildPartUI(VisualElement container) {
            // 親コンテナ
            _parentContainer = container;

            // 全体コンテナ
            //-----------------------------------------------------------------------------------
            _baseContainer = new VisualElement {name = PartName};
            _baseContainer.AddToClassList(UssClassName);
            _baseContainer.AddToClassList(m_ParentClassName.WithUssElement(PartName));

            // ブロック1
            //-----------------------------
            _block1 = new VisualElement();
            _block1.AddToClassList(m_ParentClassName.WithUssElement("block1"));
            // セクションコード
            _codeLabel = new Label("");
            _block1.Add(_codeLabel);

            // ブロック2
            //-----------------------------
            _block2 = new VisualElement();
            _block2.AddToClassList(m_ParentClassName.WithUssElement("block2"));
            // フィールドマップ名
            _fieldMapNameLabel = new Label("");
            _block2.Add(_fieldMapNameLabel);

            // ブロック3
            //-----------------------------
            _block3 = new VisualElement();
            _block3.AddToClassList(m_ParentClassName.WithUssElement("block3"));
            // セクション名
            _nameLabel = new Label("");
            _block3.Add(_nameLabel);

            // ブロック4
            //-----------------------------
            _block4 = new VisualElement();
            _block4.AddToClassList(m_ParentClassName.WithUssElement("block4"));
            // 属するスイッチ一覧

            // ブロック4
            //-----------------------------
            _block5 = new VisualElement();
            _block5.AddToClassList(m_ParentClassName.WithUssElement("block5"));
            // マップ画像
            _mapImage = new VisualElement();
            _mapImage.AddToClassList(m_ParentClassName.WithUssElement("map-image"));
            _block5.Add(_mapImage);

            // コンテナツリー生成
            //-----------------------------------------------------------------------------------
            _baseContainer.Add(_block1);
            _baseContainer.Add(_block2);
            _baseContainer.Add(_block3);
            _baseContainer.Add(_block4);
            _baseContainer.Add(_block5);
            _parentContainer.Add(_baseContainer);
        }

        protected override void PostBuildPartUI() {
            base.PostBuildPartUI();

            var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/RPGMaker/Codebase/Editor/OutlineEditor/Asset/OutlineNode.uss");

            if (stylesheet != null) _parentContainer.styleSheets.Add(stylesheet);

            ColorUtility.TryParseHtmlString(SectionNodeColor, out var color);
            _parentContainer.Q<VisualElement>("top-port-container-part").style.backgroundColor = color;
            _parentContainer.Q<VisualElement>("bottom-port-container-part").style.backgroundColor = color;
        }

        protected override void UpdatePartFromModel() {
            if (!(m_Model is SectionNodeModel sectionNodeModel))
                return;

            var sectionCode = OutlineEditor.OutlineDataModel.GetSectionCode(sectionNodeModel.SectionDataModel);
            _codeLabel.text = $"Section {sectionCode}";

            _fieldMapNameLabel.text = sectionNodeModel.maps.Count > 0
                ? sectionNodeModel.maps.Select(map => map.Name).Aggregate((a, b) => $"{a}, {b}")
                : EditorLocalize.LocalizeText("WORD_3070");
            _nameLabel.text = sectionNodeModel.name;

            _block4.Clear();
            foreach (var switchEntity in sectionNodeModel.belongingSwitches) _block4.Add(new Label(switchEntity.Name));

            // マップサムネール画像表示設定 (マップが複数の場合、最初にサムネール画像が存在するマップのそれを使用する)。
            {
                var mapThumbnailImageFilePath = sectionNodeModel.maps
                    .Select(map => MapEditWindow.GetThumbnailImageFilePathThatExist(map.ID))
                    .FirstOrDefault(filePath => filePath != null);
                mapThumbnailImageFilePath ??= NoImageFilePath;
                _mapImage.style.backgroundImage = new StyleBackground(
                    ImageUtility.LoadImageFileToTexture(mapThumbnailImageFilePath));
            }
        }
    }
}