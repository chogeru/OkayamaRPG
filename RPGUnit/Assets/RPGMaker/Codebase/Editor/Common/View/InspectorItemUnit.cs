using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Common.View
{
    public sealed class InspectorItemUnit : VisualElement
    {
        public InspectorItemUnit() {
            AddToClassList("block_40_60");

            RegisterCallback<AttachToPanelEvent>(e =>
            {
                SetClass();
            });
        }

        /// <summary>
        /// 更新がかかったときに再設定するため
        /// </summary>
        public void SetClass() {
            var elements = Children().ToList();

            if (elements.Count != 2) return;

            var firstElement = new VisualElement();
            var secondElement = new VisualElement();

            firstElement.AddToClassList("block_40_60_first");
            secondElement.AddToClassList("block_40_60_second");

            firstElement.Add(elements[0]);
            secondElement.Add(elements[1]);

            Add(firstElement);
            Add(secondElement);
        }

        public new class UxmlFactory : UxmlFactory<InspectorItemUnit, UxmlTraits>
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