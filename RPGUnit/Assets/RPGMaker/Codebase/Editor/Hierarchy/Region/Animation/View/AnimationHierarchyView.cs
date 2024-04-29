using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Animation;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Base.View.Component;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Animation.View
{
    /// <summary>
    /// アニメーションのHierarchyView
    /// </summary>
    public class AnimationHierarchyView : AbstractHierarchyView
    {
        // const
        //--------------------------------------------------------------------------------------------------------------
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Hierarchy/Region/Animation/Asset/database_animation.uxml"; } }

        // ヒエラルキー本体クラス
        //--------------------------------------------------------------------------------------------------------------
        private readonly AnimationHierarchy _animationHierarchy;
        private Button _addAnimationButton;

        // 利用するデータ
        //--------------------------------------------------------------------------------------------------------------
        private List<AnimationDataModel> _animationDataModels;
        private List<AnimationDataModel> _animationDataModelsWork;
        private VisualElement            _animationListContainer;
        private HierarchyItemListView    _animationListView;

        // コピー時の保持データINDEX
        //--------------------------------------------------------------------------------------------------------------
        private int _index;

        //--------------------------------------------------------------------------------------------------------------
        //
        // methods
        //
        //--------------------------------------------------------------------------------------------------------------

        // 初期化・更新系
        //--------------------------------------------------------------------------------------------------------------
        public AnimationHierarchyView(AnimationHierarchy animationHierarchy) {
            _animationHierarchy = animationHierarchy;
            InitUI();
        }

        /// <summary>
        /// 各コンテンツデータの初期化
        /// </summary>
        protected override void InitContentsData() {
            SetFoldout("battle_effect_foldout");

            // リストコンテナ初期化
            _animationListContainer = UxmlElement.Query<VisualElement>("animation_list");

            // リストインスタンス初期化
            _animationListView = new HierarchyItemListView(ViewName);
            _animationListContainer.Add(_animationListView);

            // uxml全体をrootに設置
            Add(UxmlElement);

            InitEventHandlers();
        }

        /// <summary>
        /// イベントの初期設定
        /// </summary>
        private void InitEventHandlers() {
            InitContextMenu(RegistrationLimit.None);
            var dic = new Dictionary<string, List<string>>
            {
                {
                    KeyNameBattleEffect,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_1349"), EditorLocalize.LocalizeText("WORD_1463")
                    }
                }
            };
            SetParentContextMenu(dic);
            _animationListView.SetEventHandler(OnClickItem, OnRightClickItem);
        }

        /// <summary>
        /// データ更新
        /// </summary>
        /// <param name="animationDataModels"></param>
        public void Refresh([CanBeNull] List<AnimationDataModel> animationDataModels = null) {
            if (animationDataModels != null)
                _animationDataModels = animationDataModels;
            base.Refresh();
        }

        /// <summary>
        /// データ更新
        /// </summary>
        protected override void RefreshContents() {
            base.RefreshContents();
            //「なし」を抜いた状態にする
            _animationDataModelsWork = _animationDataModels.Where(item => item.id != "54b168ea-5141-48ed-9e42-4336ac58755c").Select(item => item).ToList();
            var particleNames = _animationDataModelsWork.Select(item => item.particleName).ToList();
            _animationListView.Refresh(particleNames);
        }

        // イベントハンドラ
        //--------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// アニメーションデータのクリック時イベント
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        private void OnClickItem(int index, string value) {
            Inspector.Inspector.AnimEditView(index);
        }

        /// <summary>
        /// アニメーションデータの右クリック
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        private void OnRightClickItem(int index, string value) {
            var childDic = new Dictionary<string, ContextMenuData>
            {
                {
                    KeyNameBattleEffect, new ContextMenuData()
                    {
                        UuId = _animationDataModelsWork[index].id,
                        Names =
                            new List<string>()
                            {
                                EditorLocalize.LocalizeText("WORD_1462"),
                                EditorLocalize.LocalizeText("WORD_0383")
                            },
                        SerialNumber = index,
                        DisplayStartNum = DisplayStartNum.None
                    }
                }
            };
            SetChildContextMenu(childDic);
        }

        protected override VisualElement CreateDataModel(string keyName) {
            VisualElement visualElement = base.CreateDataModel(keyName);
            if (keyName == KeyNameBattleEffect)
            {
                _animationHierarchy.CreateAnimationDataModel();
                visualElement = LastAnimationIndex();
            }
            return visualElement;
        }

        protected override VisualElement DuplicateDataModel(string keyName, string uuId) {
            VisualElement visualElement = base.DuplicateDataModel(keyName, uuId);
            if (keyName == KeyNameBattleEffect)
            {
                var animationDataModel = _animationDataModels.FirstOrDefault(a => a.id == uuId);
                _animationHierarchy.DuplicateAnimationDataModel(animationDataModel);
                visualElement = LastAnimationIndex();
            }
            return visualElement;
        }

        protected override VisualElement DeleteDataModel(string keyName, string uuId) {
            VisualElement visualElement = base.DeleteDataModel(keyName, uuId);
            if (keyName == KeyNameBattleEffect)
            {
                AnimationDataModel animationDataModel = null;
                int index = 0;
                for (int i = 0; i < _animationDataModels.Count; i++)
                {
                    if (_animationDataModels[i].id == uuId)
                    {
                        animationDataModel = _animationDataModels[i];
                        index = i;
                        break;
                    }
                }
                _animationHierarchy.DeleteAnimationDataModel(animationDataModel);

                var elements = new List<VisualElement>();
                _animationListView.Query<Button>().ForEach(button => { elements.Add(button); });
                visualElement = elements.Count - 1 < index
                    ? LastAnimationIndex()
                    : elements.FirstOrDefault(e => e.name == "AnimationHierarchyView" + (index - 1)); //「なし」は画面上表示しないので、-1する
            }
            return visualElement;
        }


        /// <summary>
        /// 最終選択していたアニメーションデータ返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastAnimationIndex() {
            var elements = new List<VisualElement>();
            _animationListView.Query<Button>().ForEach(button => { elements.Add(button); });

            //選択可能なVisualElementが存在しない場合には、Inspectorを初期化して終了する
            if (elements.Count == 0)
            {
                Inspector.Inspector.Clear();
                (WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.DatabaseSceneWindow) as DatabaseEditor.Window.SceneWindow)?.Clear();
                return null;
            }

            return elements[elements.Count - 1];
        }
    }
}