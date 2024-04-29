using RPGMaker.Codebase.Editor.Common;
using System.Linq;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Battle
{
    /// <summary>
    ///     [戦闘アニメーションの表示]の実行内容枠の表示物
    /// </summary>
    public class BattleShowAnimation : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventCommand eventCommand) {
            ret = indent;
            var memberIndex = 0;
            if (int.TryParse(eventCommand.parameters[0], out memberIndex))
            {
                memberIndex -= 1; // 1から始まる番号で格納されているのでインデックス用に調整
            }
            if (GetEnemyNameList().Count <= memberIndex)
            {
                memberIndex = 0;
            }

            var animId = eventCommand.parameters[1];
            var animationData = DatabaseManagementService.LoadAnimation();
            var animName = EditorLocalize.LocalizeText("WORD_0113");
            var animData = animationData.FirstOrDefault(c => c.id == animId);
            if (animData != null)
            {
                animName = animData.particleName;
            }


            string data = "";
            if (GetEnemyNameList().Count > memberIndex)
                data = GetEnemyNameList()[memberIndex];

            ret += "◆" + EditorLocalize.LocalizeText("WORD_1110");
            ret += $" : #{memberIndex + 1} {data}, {animName}";

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}