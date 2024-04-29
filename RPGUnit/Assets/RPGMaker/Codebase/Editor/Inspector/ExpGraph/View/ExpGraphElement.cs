using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using RPGMaker.Codebase.Editor.Inspector.ExpGraphValue.View;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.ExpGraph.View
{
    /// <summary>
    /// [キャラクター]-[職業の編集]の経験値グラフ用クラス
    /// </summary>
    public class ExpGraphElement : VisualElement
    {
        private List<int> expTable = new List<int>();

        /// <summary>
        /// 経験値の値設定
        /// </summary>
        /// <param name="classDataModel"></param>
        /// <param name="maxLevel"></param>
        /// <param name="clearLevel"></param>
        /// <param name="valueA"></param>
        /// <param name="valueB"></param>
        /// <param name="growthValue"></param>
        /// <param name="maxExp"></param>
        public void SetExp(ClassDataModel classDataModel, int maxLevel, int clearLevel, int valueA, int valueB, int growthValue, int maxExp) {
            expTable = classDataModel.GetExpTable(maxLevel, valueA, valueB, growthValue, clearLevel, maxExp);
            DispNextExp();
        }

        // レベルアップ経験値表示
        public void DispNextExp() {
            hierarchy.Clear();

            for (var i = 0; i < expTable.Count; i++)
            {
                int value = expTable[i];
                if (value > 0) value -= expTable[i - 1];
                var test = new ExpGraphValueElement(i + 1, value);
                hierarchy.Add(test);
            }
        }

        // 累計経験値表示
        public void DispTotalExp() {
            hierarchy.Clear();
            for (var i = 0; i < expTable.Count; i++)
            {
                var test = new ExpGraphValueElement(i + 1, expTable[i]);
                hierarchy.Add(test);
            }
        }

        public new class UxmlFactory : UxmlFactory<ExpGraphElement, UxmlTraits>
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