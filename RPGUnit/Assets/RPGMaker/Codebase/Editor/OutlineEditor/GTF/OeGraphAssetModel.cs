using System;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace RPGMaker.Codebase.Editor.OutlineEditor.GTF
{
    public class OeGraphAssetModel : GraphAssetModel
    {
        protected override Type GraphModelType => typeof(OeGraphModel);

        private void Awake() {
            hideFlags = UnityEngine.HideFlags.DontSave;
        }
    }
}