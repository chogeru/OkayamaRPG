using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.AddonUIUtil;
using RPGMaker.Codebase.Runtime.Addon;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Addons
{
    public class AddonCommand : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            var addonInfos = AddonManager.Instance.GetAddonInfos();
            var addonInfo = addonInfos.GetAddonInfo(eventCommand.parameters[0]);
            var commandInfo = addonInfo != null
                ? addonInfo.commandInfos.GetCommandInfo(eventCommand.parameters[1])
                : null;
            var parameters =
                new AddonParameterContainer(JsonHelper.FromJsonArray<AddonParameter>(eventCommand.parameters[2]));
            var comamndText = commandInfo != null ? commandInfo.infos.GetParameterValue("text") : null;
            var list = new List<string>();
            if (commandInfo != null)
                foreach (var paramInfo in commandInfo.args)
                {
                    var text = paramInfo.GetInfo("text")?.value;
                    var value = AddonManager.GetInitialValue(parameters, paramInfo);
                    value = AddonUIUtil.GetEasyReadableText(addonInfo, paramInfo, null, value);
                    list.Add($"{(text != null ? text : paramInfo.name)} = {value}");
                }

            ret = indent;
            ret += "◆" + EditorLocalize.LocalizeText("WORD_2510") + " : " + eventCommand.parameters[0] +
                   (commandInfo != null ? $", {(comamndText != null ? comamndText : commandInfo.name)}" : "");
            if (list.Count > 0) ret += "(" + string.Join(", ", list) + ")";
            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}