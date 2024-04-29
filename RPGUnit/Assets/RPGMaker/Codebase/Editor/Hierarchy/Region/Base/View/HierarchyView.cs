using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Animation.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.AssetManage.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Character.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.CommonEvent.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Equip.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Flags.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Initialization.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Map.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Outline.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Skill.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Sound.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.State.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Type.View;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Base.View
{
    /// <summary>
    /// Hierarchy全体の表示を行うクラス
    /// </summary>
    public class HierarchyView : AbstractHierarchyView
    {
        // const
        //--------------------------------------------------------------------------------------------------------------
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Hierarchy/Region/Base/Asset/hierarchy.uxml"; } }
        private readonly AnimationHierarchyView _animationHierarchyView;
        private readonly AssetManageHierarchyView _assetManageHierarchy;
        private readonly CharacterHierarchyView _characterHierarchyView;
        private readonly BattleHierarchyView _battleHierarchyView;
        private readonly CommonEventHierarchyView _commonEventHierarchy;
        private readonly EquipHierarchyView _equipHierarchy;
        private readonly FlagsHierarchyView _flagsHierarchy;

        // リージョン別ヒエラルキー
        //--------------------------------------------------------------------------------------------------------------
        private readonly InitializationHierarchyView _initializationHierarchyView;
        private readonly MapHierarchyView            _mapHierarchy;
        private readonly MapSampleHierarchyView      _mapSampleHierarchy;
        private readonly OutlineHierarchyView        _outlineHierarchyView;
        private readonly SkillHierarchyView          _skillHierarchy;
        private readonly SoundHierarchyView          _soundHierarchyView;
        private readonly StateHierarchyView          _stateHierarchyView;
        private readonly TypeHierarchyView           _typeHierarchy;
        private          Foldout                     _databaseArea;
        private          Foldout                     _outlineArea;

        // UI要素
        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Hierarchy
        /// </summary>
        /// <param name="initializationHierarchyView"></param>
        /// <param name="characterHierarchyView"></param>
        /// <param name="soundHierarchyView"></param>
        /// <param name="skillHierarchy"></param>
        /// <param name="stateHierarchyView"></param>
        /// <param name="equipHierarchy"></param>
        /// <param name="typeHierarchy"></param>
        /// <param name="animationHierarchyView"></param>
        /// <param name="commonEventHierarchy"></param>
        /// <param name="assetManageHierarchy"></param>
        /// <param name="flagsHierarchy"></param>
        /// <param name="mapHierarchy"></param>
        /// <param name="mapSampleHierarchy"></param>
        /// <param name="outlineHierarchyView"></param>
        public HierarchyView(
            InitializationHierarchyView initializationHierarchyView,
            CharacterHierarchyView characterHierarchyView,
            BattleHierarchyView battleHierarchyView,
            SoundHierarchyView soundHierarchyView,
            SkillHierarchyView skillHierarchy,
            StateHierarchyView stateHierarchyView,
            EquipHierarchyView equipHierarchy,
            TypeHierarchyView typeHierarchy,
            AnimationHierarchyView animationHierarchyView,
            CommonEventHierarchyView commonEventHierarchy,
            AssetManageHierarchyView assetManageHierarchy,
            FlagsHierarchyView flagsHierarchy,
            MapHierarchyView mapHierarchy,
            MapSampleHierarchyView mapSampleHierarchy,
            OutlineHierarchyView outlineHierarchyView
        ) {
            _initializationHierarchyView = initializationHierarchyView;
            _characterHierarchyView = characterHierarchyView;
            _battleHierarchyView = battleHierarchyView;
            _soundHierarchyView = soundHierarchyView;
            _skillHierarchy = skillHierarchy;
            _stateHierarchyView = stateHierarchyView;
            _equipHierarchy = equipHierarchy;
            _typeHierarchy = typeHierarchy;
            _animationHierarchyView = animationHierarchyView;
            _commonEventHierarchy = commonEventHierarchy;
            _assetManageHierarchy = assetManageHierarchy;
            _flagsHierarchy = flagsHierarchy;
            _mapHierarchy = mapHierarchy;
            _mapSampleHierarchy = mapSampleHierarchy;
            _outlineHierarchyView = outlineHierarchyView;
            InitUI();
        }

        /// <summary>
        /// 各コンテンツデータの初期化
        /// </summary>
        override protected void InitContentsData() {
            //xmlの読み込み後に、xmlに定義してあるものはすべての右クリックを無効にする
            UxmlElement.Query<VisualElement>().ForEach(element =>
            {
                //右クリックしたときに飛んでくる
                element.RegisterCallback<MouseUpEvent>(evt =>
                {
                    evt.StopPropagation();
                });
            });
            _databaseArea = UxmlElement.Query<Foldout>("database_area");
            _databaseArea.Clear();
            _databaseArea.Add(_initializationHierarchyView);
            _databaseArea.Add(_characterHierarchyView);
            _databaseArea.Add(_battleHierarchyView);
            _databaseArea.Add(_soundHierarchyView);
            _databaseArea.Add(_skillHierarchy);
            _databaseArea.Add(_stateHierarchyView);
            _databaseArea.Add(_equipHierarchy);
            _databaseArea.Add(_typeHierarchy);
            _databaseArea.Add(_animationHierarchyView);
            _databaseArea.Add(_commonEventHierarchy);
            _databaseArea.Add(_assetManageHierarchy);
            _databaseArea.Add(_flagsHierarchy);
            _databaseArea.Add(_mapSampleHierarchy);
            _databaseArea.Add(_mapHierarchy);

            _outlineArea = UxmlElement.Query<Foldout>("outline_area");
            _outlineArea.Clear();
            _outlineArea.Add(_outlineHierarchyView);
            OutlineHierarchyView.AddContextMenu(_outlineArea);

            // すべてのFoldoutを閉じる（初期状態）
            CollapseAllFoldouts();
        }

        public void CollapseAllFoldouts() {
        }

        public void ExpandAllFoldouts() {
            UxmlElement.Query<Foldout>().ForEach(f => { f.value = true; });
        }

        public void DeactivateAllItems() {
            UxmlElement.Query<Foldout>().ForEach(f => { f.RemoveFromClassList("active"); });
            UxmlElement.Query<Button>().ForEach(f => { f.RemoveFromClassList("active"); });
            UxmlElement.Query<Label>().ForEach(f => { f.RemoveFromClassList("active"); });
        }

        public VisualElement GetActiveClassItem() {
            return UxmlElement.Q(null, new[] {"active"});
        }

        public T GetItem<T>(string name = null, params string[] classes) where T : VisualElement {
            return UxmlElement.Q<T>(name, classes);
        }
    }
}