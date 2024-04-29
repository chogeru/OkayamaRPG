using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.State;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Battle
{
    public class BattleChangeState : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            
            ret = indent;
            int.TryParse(eventCommand.parameters[0], out var memberIndex);
            var enemyData = GetEnemyList();
            var stateData = _GetStateList();

            if (GetEnemyNameList(true).Count <= memberIndex)
            {
                memberIndex = 0;
            }
            string data = GetEnemyNameList(true)[memberIndex];

            ret = "◆" + EditorLocalize.LocalizeText("WORD_1106");
            if (memberIndex == 0)
                ret += " : " + data + " ";
            else
                ret += $" : #{memberIndex} {data}";

            if (eventCommand.parameters[1] == "0")
                ret += "+ ";
            else
                ret += "- ";

            var state = stateData.FirstOrDefault(c => c.id == eventCommand.parameters[2]);

            ret += state != null ? state.name : EditorLocalize.LocalizeText("WORD_0113");

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }

        private List<StateDataModel> _GetStateList() {
            var stateDataModels = DatabaseManagementService.LoadStateEdit();
            var fileNames = new List<StateDataModel>();
            for (var i = 0; i < stateDataModels.Count; i++)
            {
                fileNames.Add(stateDataModels[i]);
            }

            return fileNames;
        }
    }
}