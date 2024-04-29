using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Battle
{
    public class BattleAppear : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;

            int.TryParse(eventCommand.parameters[0], out var memberIndex);
            if (GetEnemyNameList().Count <= memberIndex)
            {
                memberIndex = 0;
            }

            string data = "";
            if (GetEnemyNameList().Count > memberIndex)
                data = GetEnemyNameList()[memberIndex];
            ret = "◆" + EditorLocalize.LocalizeText("WORD_1107");
            ret += $" : #{memberIndex + 1} {data}";

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}