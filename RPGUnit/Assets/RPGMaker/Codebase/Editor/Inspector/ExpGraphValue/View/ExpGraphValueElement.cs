using RPGMaker.Codebase.Editor.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.ExpGraphValue.View
{
    /// <summary>
    /// [キャラクター]-[職業の編集]の経験値グラフの1要素
    /// </summary>
    public class ExpGraphValueElement : VisualElement
    {
        public ExpGraphValueElement(int level, int exp) {
            var treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/RPGMaker/Codebase/Editor/Inspector/ExpGraphValue/Asset/ExpGraphValueElement.uxml");
            var container = treeAsset.Instantiate();

            var levelText = container.Q<Label>("level");
            //ライトモードの場合は、全体のLabelが黒のため、個別に白にする
            if (!EditorGUIUtility.isProSkin)
            {
                levelText.style.color = Color.white;
            }
            var expText = container.Q<Label>("WORD_0154");

            levelText.text = EditorLocalize.LocalizeText("WORD_0147") + " " + level +
                             EditorLocalize.LocalizeText("WORD_1515");
            expText.text = exp.ToString();


            hierarchy.Add(container);
        }
    }
}