using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.FlowControl
{
    public class FlowJumpCommon : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            var eventManagementService = new EventManagementService();
            var eventCommonDataModels = eventManagementService.LoadEventCommon();

            // 該当イベントの番号を取得
            var num = 0;
            for (var i = 0; i < eventCommonDataModels.Count; i++)
                if (eventCommonDataModels[i].eventId == eventCommand.parameters[0])
                {
                    num = i;
                    break;
                }

            ret = indent;

            ret += "◆" + EditorLocalize.LocalizeText("WORD_0506") + " : #" + (num + 1).ToString("0000");

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}