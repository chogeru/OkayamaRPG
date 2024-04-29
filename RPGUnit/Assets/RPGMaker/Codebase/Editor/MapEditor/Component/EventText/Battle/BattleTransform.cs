using RPGMaker.Codebase.Editor.Common;
using System.Linq;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Battle
{
    /// <summary>
    ///     [敵キャラの変身]の実行内容枠の表示物
    /// </summary>
    public class BattleTransform : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventCommand eventCommand) {
            ret = indent;
            int memberIndex = 0;
            if (int.TryParse(eventCommand.parameters[0], out memberIndex))
                memberIndex -= 1; // 1から始まる番号で格納されているのでインデックス用に調整
            var memberList = GetEnemyNameList();

            var enemyDataList = GetEnemyList();
            var enemyId = eventCommand.parameters[1];
            var enemyData = enemyDataList.FirstOrDefault(c => c.id == enemyId);

            if (GetEnemyNameList().Count <= memberIndex)
            {
                memberIndex = 0;
            }

            string data = "";
            if (GetEnemyNameList().Count > memberIndex)
                data = GetEnemyNameList()[memberIndex];

            ret += "◆" + EditorLocalize.LocalizeText("WORD_1108");
            ret += $" : #{memberIndex + 1} {data}, {enemyData?.name}";

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}