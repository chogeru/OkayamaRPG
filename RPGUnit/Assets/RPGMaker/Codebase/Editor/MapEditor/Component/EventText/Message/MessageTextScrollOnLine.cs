#define USE_LONG_TEXT_FIELD

using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
#if USE_LONG_TEXT_FIELD
using RPGMaker.Codebase.Editor.Common.View;
#endif
using System.Linq;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.Editor.MapEditor.Window.EventEdit.ExecutionContentsWindow;


namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Message
{
    public class MessageTextScrollOnLine : AbstractEventText, IEventCommandView
    {
#if USE_LONG_TEXT_FIELD
        private readonly LongTextField _textField = new();
#else
        private readonly ImTextField _textField = new();
#endif

        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {

            var instance = ExecutionContentsWindowParam.instance;
            var dataModelsIndex = instance.eventId;
            var eventIndex = -1;
            var page = instance.page;
            var eventDataModels = ExecutionContentsWindow.EventDataModels;
            for (var i = 0; i < eventDataModels.Count; i++)
            {
                if (eventDataModels[i].id == dataModelsIndex && eventDataModels[i].page == page)
                {
                    eventIndex = i;
                    break;
                }
            }

                ret = indent;
            Element.style.flexDirection = FlexDirection.Row;
            Element.style.alignItems = Align.Center;

            ret += "◇#";
            LabelElement.text = ret;
            _textField.multiline = true;
            _textField.style.width = 400;

            _textField.value = eventCommand.parameters[0];
            _textField.RegisterCallback<FocusOutEvent>(o =>
            {
                ExecutionContentsWindow.IsSaveWait = true;

                var data = eventDataModels[eventIndex].eventCommands.FirstOrDefault(c =>
                    c.code == eventCommand.code &&
                    c.parameters[0] == eventCommand.parameters[0] &&
                    c.parameters[1] == eventCommand.parameters[1]);

                var index = eventDataModels[eventIndex].eventCommands.IndexOf(data);
                eventCommand.parameters[0] = _textField.value;
                eventDataModels[eventIndex].eventCommands[index].parameters[0] = _textField.value;
                ExecutionContentsWindow.SetUpData();
            });

            Element.Add(LabelElement);
            Element.Add(_textField);
            return Element;
        }
    }
}