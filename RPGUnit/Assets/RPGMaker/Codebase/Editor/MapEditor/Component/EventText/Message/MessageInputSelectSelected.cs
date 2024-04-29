using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Message
{
    public class MessageInputSelectSelected : AbstractEventText, IEventCommandView
    {
        //選択肢最大文字数
        private readonly int       _maxChooseString = 100;
        private readonly ImTextField _textField       = new ImTextField();
        private readonly Label     tailLabel        = new Label("」");

        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            Element.style.flexDirection = FlexDirection.Row;
            Element.style.alignItems = Align.Center;

            // 言語でWORD_1207の挿入位置が変わるので、一旦文字列を結合して鍵括弧で分割する
            var text = EditorLocalize.LocalizeTextFormat("WORD_1207", $"#{eventCommand.parameters[0]}「」");

            ret += $"◇{text}";
            var splitCount = ret.IndexOf("」");
            LabelElement.text = ret.Substring(0, splitCount);

            _textField.style.width = 400;
            _textField.value = eventCommand.parameters[2];
            _textField.RegisterCallback<FocusOutEvent>(o =>
            {
                //文字数制限
                string valueText;
                if (_textField.value.Length > 100)
                    valueText = _textField.value.Substring(0, _maxChooseString);
                else
                    valueText = _textField.value;

                eventCommand.parameters[2] = valueText;
                ExecutionContentsWindow.SetUpData();
            });

            tailLabel.text = ret.Substring(splitCount, ret.Length - splitCount);


            Element.Add(LabelElement);
            Element.Add(_textField);
            Element.Add(tailLabel);
            return Element;
        }
    }
}