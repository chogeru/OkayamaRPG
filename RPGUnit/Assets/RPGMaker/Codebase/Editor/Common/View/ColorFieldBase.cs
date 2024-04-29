using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Common.View
{
    public class ColorFieldBase : ColorField
    {
        public ColorFieldBase() {
            showEyeDropper = false;
        }
        
        public new class UxmlFactory : UxmlFactory<ColorFieldBase, UxmlTraits>
        {
        }
        
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield return new UxmlChildElementDescription(typeof(VisualElement)); }
            }
        }
    }
}