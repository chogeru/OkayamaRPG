using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Map
{
    public class MapChangeBattleBackGround : AbstractEventText, IEventCommandView
    {
        private static List<string> DefaultBattlebackName = new List<string>
        {
            "battlebacks1_nature_008",
            "battlebacks2_nature_008"
        };

        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;

            //戦闘背景については、[なし]の場合にはシステム内で持つ画像をデフォルト値
            string battlebacks1 = eventCommand.parameters[0];
            if (battlebacks1 == "") battlebacks1 = DefaultBattlebackName[0];
            string battlebacks2 = eventCommand.parameters[1];
            if (battlebacks2 == "") battlebacks2 = DefaultBattlebackName[1];

            ret += "◆" + EditorLocalize.LocalizeText("WORD_1179") + " : " + battlebacks1 + "," + battlebacks2;

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}