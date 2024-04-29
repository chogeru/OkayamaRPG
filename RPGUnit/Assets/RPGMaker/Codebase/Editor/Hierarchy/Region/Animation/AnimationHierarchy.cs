using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Animation;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Animation.View;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Animation
{
    /// <summary>
    /// アニメーションのHierarchy
    /// </summary>
    public class AnimationHierarchy : AbstractHierarchy
    {
        private List<AnimationDataModel> _animationDataModels;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AnimationHierarchy() {
            LoadData();
            View = new AnimationHierarchyView(this);
        }

        /// <summary>
        /// View
        /// </summary>
        public AnimationHierarchyView View { get; }

        /// <summary>
        /// データの読込
        /// </summary>
        override protected void LoadData() {
            base.LoadData();
            _animationDataModels = databaseManagementService.LoadAnimation();
        }

        /// <summary>
        /// Viewの更新
        /// </summary>
        protected override void UpdateView(string updateData = null) {
            base.UpdateView();
            View.Refresh(_animationDataModels);
        }

        /// <summary>
        /// アニメーションの新規作成
        /// </summary>
        public void CreateAnimationDataModel() {
            var newDataModel = AnimationDataModel.CreateDefault(Guid.NewGuid().ToString());
            newDataModel.particleName = "#" + string.Format("{0:D4}", _animationDataModels.Count + 1) + " " +
                                        EditorLocalize.LocalizeText("WORD_1518");
            newDataModel.particleId = ImageManager.GetSvIdList(AssetCategoryEnum.BATTLE_EFFECT).Count > 0
                ? ImageManager.GetSvIdList(AssetCategoryEnum.BATTLE_EFFECT)[0].id
                : "";
            newDataModel.offset = "10;10";
            newDataModel.rotation = "0;0;0";

            _animationDataModels.Add(newDataModel);
            databaseManagementService.SaveAnimation(_animationDataModels);
            Refresh();
        }

        /// <summary>
        /// アニメーションの削除
        /// </summary>
        /// <param name="targetAnimationDataModel"></param>
        public void DeleteAnimationDataModel(AnimationDataModel targetAnimationDataModel) {
            _animationDataModels.Remove(targetAnimationDataModel);
            databaseManagementService.SaveAnimation(_animationDataModels);
            Refresh();
        }

        /// <summary>
        /// アニメーションのコピー＆貼り付け処理
        /// </summary>
        /// <param name="originalAnimationDataModel"></param>
        public void DuplicateAnimationDataModel(AnimationDataModel originalAnimationDataModel) {
            var uuid = Guid.NewGuid().ToString();
            var duplicated = originalAnimationDataModel.DataClone();
            duplicated.id = uuid;
            duplicated.particleName = CreateDuplicateName(_animationDataModels.Select(a => a.particleName).ToList(),
                originalAnimationDataModel.particleName);
            _animationDataModels.Add(duplicated);
            databaseManagementService.SaveAnimation(_animationDataModels);
            // アニメーション領域のみリフレッシュ
            Refresh();
        }
    }
}