using RPGMaker.Codebase.CoreSystem.Helper;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Common.View
{
    /// <summary>
    /// 長いテキストに対応したTextField。
    /// 但し、長いテキストの場合に複数のTextFieldで分割表示するだけのもの。
    /// </summary>
    /// <remarks>
    /// Unityの制限によりTextFieldに長い文字列を設定した場合に以下の警告が表示され、
    ///   Generated text will be truncated because it exceeds 49152 vertices.
    /// 後方の文字が表示されない問題を回避する。
    /// 
    /// 本クラスから分割用TextFieldへ値が反映されるプロパティは以下。
    ///     style.width
    /// </remarks>
    public class LongTextField : VisualElement
    {
        public LongTextField()
        {
            RegisterCallback<FocusOutEvent>(focusOutEvent =>
            {
                // 分割し直し。
                value = value;
                DebugUtil.Log($"value.Length={value.Length}, childCount={childCount}");
            });
        }

        public bool multiline { get; set; }

        public string value
        {
            get
            {
                return ConcatTexts();
            }

            set
            {
                SetTextFields(
                    Runtime.Common.Component.Hud.Message.MessageTextScroll.SplitTextForUnityText(
                        value.Replace("\r\n", "\n")));
                DebugUtil.Assert(value == ConcatTexts());
            }
        }

        private string ConcatTexts()
        {
            return string.Concat(Children().Select(ve => ((ImTextField)ve).value));
        }

        private void SetTextFields(List<string> splitedTexts)
        {
            Clear();
            foreach (var splitedText in splitedTexts)
            {
                AddTextField(splitedText);
            }

            if (splitedTexts.Count == 0)
            {
                AddTextField("");
            }

            void AddTextField(string text)
            {
                var textField = new ImTextField() { value = text, multiline = multiline };
                Add(textField);
                textField.style.width = style.width;
            }
        }
    }
}