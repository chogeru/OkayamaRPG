using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.MapEditor.Window.MapEdit;
using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using UnityEditor;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Component
{
    public class ChapterNode : Node
    {
        private const string UssClassName = "oe-node";

        protected override void BuildPartList() {
            PartList.AppendPart(VerticalPortContainerPart.Create("top-port-container-part", PortDirection.Input, Model,
                this,
                UssClassName));
            PartList.AppendPart(ChapterNodePart.Create("chapter-container-part", Model, this, UssClassName));
            PartList.AppendPart(InOutPortContainerPart.Create("side-port-container-part", Model, this, UssClassName));
            PartList.AppendPart(VerticalPortContainerPart.Create("bottom-port-container-part", PortDirection.Output,
                Model,
                this, UssClassName));
        }
    }

    public class ChapterNodePart : BaseModelUIPart
    {
        private const string        ChapterNodeColor = "#660066";
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
        private Label         _supposedLevelMaxLabel;
        private Label         _supposedLevelMinLabel;


        private ChapterNodePart(
            string name,
            IGraphElementModel model,
            IModelUI ownerElement,
            string parentClassName
        )
            : base(name, model, ownerElement, parentClassName) {
        }

        public override VisualElement Root => _baseContainer;

        public static ChapterNodePart Create(
            string name,
            IGraphElementModel model,
            IModelUI modelUI,
            string parentClassName
        ) {
            return model is INodeModel ? new ChapterNodePart(name, model, modelUI, parentClassName) : null;
        }

        protected override void BuildPartUI(VisualElement container) {
            if (!(m_Model is ChapterNodeModel))
                return;

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
            // チャプターコード
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
            // チャプター名
            _nameLabel = new Label("");
            _block3.Add(_nameLabel);

            // ブロック4
            //-----------------------------
            _block4 = new VisualElement();
            _block4.AddToClassList(m_ParentClassName.WithUssElement("block4"));
            // 想定レベル下限
            _supposedLevelMinLabel = new Label("");
            _block4.Add(_supposedLevelMinLabel);
            // 想定レベル上限
            _supposedLevelMaxLabel = new Label("");
            _block4.Add(_supposedLevelMaxLabel);

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
            var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/RPGMaker/Codebase/Editor/OutlineEditor/Asset/OutlineNode.uss");
            if (stylesheet != null)
                _parentContainer.styleSheets.Add(stylesheet);

            // 色変え。
            ColorUtility.TryParseHtmlString(ChapterNodeColor, out var color);
            _parentContainer.Q<VisualElement>("top-port-container-part").style.backgroundColor = color;
            _parentContainer.Q<VisualElement>("bottom-port-container-part").style.backgroundColor = color;
        }

        protected override void UpdatePartFromModel() {
            if (!(m_Model is ChapterNodeModel chapterNodeModel))
                return;

            var chapterCode = OutlineEditor.OutlineDataModel.GetChapterCode(chapterNodeModel.ChapterDataModel);
            _codeLabel.text = $"Chapter {chapterCode}";

            _fieldMapNameLabel.text =
                chapterNodeModel.FieldMapSubDataModel?.Name ?? EditorLocalize.LocalizeText("WORD_0113");
            _nameLabel.text = chapterNodeModel.name;
            _supposedLevelMinLabel.text =
                EditorLocalize.LocalizeText("WORD_0040") + " " + EditorLocalize.LocalizeText("WORD_0041") + ": " +
                chapterNodeModel.supposedLevelMin;
            _supposedLevelMaxLabel.text =
                EditorLocalize.LocalizeText("WORD_0040") + " " + EditorLocalize.LocalizeText("WORD_0042") + ": " +
                chapterNodeModel.supposedLevelMax;

            // マップサムネール画像表示設定。
            {
                var mapThumbnailImageFilePath =
                    MapEditWindow.GetThumbnailImageFilePathThatExist(
                        chapterNodeModel.FieldMapSubDataModel?.ID);
                mapThumbnailImageFilePath ??= NoImageFilePath;
                _mapImage.style.backgroundImage = new StyleBackground(
                    ImageUtility.LoadImageFileToTexture(mapThumbnailImageFilePath));
            }
        }
    }
}