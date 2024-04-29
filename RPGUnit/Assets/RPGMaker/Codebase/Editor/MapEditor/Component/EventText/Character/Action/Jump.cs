using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Character.Action
{
    /// <summary>
    /// イベントコマンド『ジャンプ』テキスト化。
    /// </summary>
    public class Jump : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            var name = "";
            if (eventCommand.parameters[0] == "-1")
            {
                name = EditorLocalize.LocalizeText("WORD_0920");
            }
            else if (eventCommand.parameters[0] == "-2")
            {
                name = EditorLocalize.LocalizeText("WORD_0860");
            }
            else
            {
                name = GetEventDisplayName(eventCommand.parameters[0]);
            }

            // その場で。
            var text = EditorLocalize.LocalizeText("WORD_0984");

            // 座標指定。
            if (eventCommand.parameters[1] == "0")
            {
                // 引数から得られないので仕方なくウィンドウ経由でEventDataModelを取得。
                var eventDataModel =
                    ((Window.EventEdit.ExecutionContentsWindow)WindowLayoutManager.
                    GetActiveWindow(WindowLayoutManager.WindowLayoutId.MapEventExecutionContentsWindow)).
                    EventDataModel;

                var pos = CommandEditor.Character.Action.Jump.GetJumpToTilePositon(
                    eventDataModel, eventCommand.parameters);

                text =
                    $"{EditorLocalize.LocalizeText("WORD_0983")}(" +
                    $"x : {(pos != null ? ((Vector2Int)pos).x : "")}, " +
                    $"y : {(pos != null ? -((Vector2Int)pos).y : "")})";
            }

            ret += "◆" + EditorLocalize.LocalizeText("WORD_1640") + " : " + EditorLocalize.LocalizeText("WORD_0014") +
                   " " + name + "/" + EditorLocalize.LocalizeText("WORD_0982") + " " + text;

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}