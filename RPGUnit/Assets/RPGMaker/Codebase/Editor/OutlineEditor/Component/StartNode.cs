using RPGMaker.Codebase.Editor.DatabaseEditor.View.Preview;
using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using UnityEditor;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.OutlineEditor.Component
{
    public class StartNode : Node
    {
        private const string UssClassName = "oe-node";

        protected override void BuildPartList() {
            PartList.AppendPart(VerticalPortContainerPart.Create("top-port-container-part", PortDirection.Input, Model,
                this,
                UssClassName));
            PartList.AppendPart(StartNodePart.Create("chapter-container-part", Model, this, UssClassName));
            PartList.AppendPart(InOutPortContainerPart.Create("side-port-container-part", Model, this, UssClassName));
            PartList.AppendPart(VerticalPortContainerPart.Create("bottom-port-container-part", PortDirection.Output,
                Model,
                this, UssClassName));
        }
    }

    public class StartNodePart : BaseModelUIPart
    {
        private const string        StartNodeColor  = "#5d4f6a";
        private const string        NoImageFilePath = "Assets/RPGMaker/Codebase/Editor/OutlineEditor/Asset/NoImage.png";
        private const string        UssClassName    = "oe-node-part";
        private       VisualElement _baseContainer;

        private VisualElement _parentContainer;
        private VisualElement _titleBackgroundImage;
        private VisualElement _titleFrontImage;
        private Label         _titleLabel;
        private Label         _typeLabel;


        private StartNodePart(
            string name,
            IGraphElementModel model,
            IModelUI ownerElement,
            string parentClassName
        )
            : base(name, model, ownerElement, parentClassName) {
        }

        public override VisualElement Root => _baseContainer;

        public static StartNodePart Create(
            string name,
            IGraphElementModel model,
            IModelUI modelUI,
            string parentClassName
        ) {
            return model is INodeModel ? new StartNodePart(name, model, modelUI, parentClassName) : null;
        }

        protected override void BuildPartUI(VisualElement container) {
            if (!(m_Model is StartNodeModel))
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
            var _block1 = new VisualElement();
            _block1.AddToClassList(m_ParentClassName.WithUssElement("block1"));
            // チャプターコード
            _typeLabel = new Label("");
            _block1.Add(_typeLabel);
            _baseContainer.Add(_block1);

            // ブロック3
            //-----------------------------
            var _block3 = new VisualElement();
            _block3.AddToClassList(m_ParentClassName.WithUssElement("block3"));
            // ゲームタイトル
            _titleLabel = new Label("");
            _block3.Add(_titleLabel);
            _baseContainer.Add(_block3);

            // ブロック5
            //-----------------------------
            var _block5 = new VisualElement();
            _block5.AddToClassList(m_ParentClassName.WithUssElement("block5"));
            _block5.style.overflow = Overflow.Hidden;

            // 画像
            _titleBackgroundImage = new VisualElement();
            _titleBackgroundImage.AddToClassList(m_ParentClassName.WithUssElement("map-image"));
            _block5.Add(_titleBackgroundImage);

            _titleFrontImage = new VisualElement();
            _titleFrontImage.AddToClassList(m_ParentClassName.WithUssElement("map-image"));
            _titleFrontImage.style.unityBackgroundScaleMode = ScaleMode.StretchToFill;
            _titleBackgroundImage.Add(_titleFrontImage);

            _baseContainer.Add(_block5);

            // コンテナツリー生成
            //-----------------------------------------------------------------------------------
            _parentContainer.Add(_baseContainer);
        }

        protected override void PostBuildPartUI() {
            var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/RPGMaker/Codebase/Editor/OutlineEditor/Asset/OutlineNode.uss");
            if (stylesheet != null)
                _parentContainer.styleSheets.Add(stylesheet);

            // 色変え。
            ColorUtility.TryParseHtmlString(StartNodeColor, out var color);
            _parentContainer.Q<VisualElement>("top-port-container-part").style.backgroundColor = color;
            _parentContainer.Q<VisualElement>("bottom-port-container-part").style.backgroundColor = color;
        }

        protected override void UpdatePartFromModel() {
            if (!(m_Model is StartNodeModel startNodeModel))
                return;

            var runtimeTitleDataModel = Editor.Hierarchy.Hierarchy.databaseManagementService.LoadTitle();

            _typeLabel.text = "Start";

            _titleLabel.text = runtimeTitleDataModel.gameTitle;

            // 画像表示設定。
            {
                var titelFrontSprite = TitlePreview.GetTitleFrontSprite(runtimeTitleDataModel);
                _titleFrontImage.style.backgroundImage = titelFrontSprite?.texture;
                var newScale = new Vector3(runtimeTitleDataModel.titleFront.scale / 100f, runtimeTitleDataModel.titleFront.scale / 100f, 0);
                //幅と高さが取得できない場合
                var resolvedStyleWidth = float.IsNaN(_titleFrontImage.resolvedStyle.width) ? 200f : _titleFrontImage.resolvedStyle.width;
                var resolvedStyleHeight = float.IsNaN(_titleFrontImage.resolvedStyle.height) ? 120f : _titleFrontImage.resolvedStyle.height;
                var x = -(runtimeTitleDataModel.titleFront.position[0]) / 1920f * resolvedStyleWidth + (resolvedStyleWidth / 2 * runtimeTitleDataModel.titleFront.scale / 100f);
                var y = -(runtimeTitleDataModel.titleFront.position[1]) / 1080f * resolvedStyleHeight + (resolvedStyleHeight / 2 * runtimeTitleDataModel.titleFront.scale / 100f);

                _titleFrontImage.style.right = x;
                _titleFrontImage.style.bottom = y;
                _titleFrontImage.transform.scale = newScale;

                var titleBackgroundSprite = TitlePreview.GetTitleBackgroundSprite(runtimeTitleDataModel);
                _titleBackgroundImage.style.backgroundImage = titleBackgroundSprite?.texture;
            }
        }
    }
}