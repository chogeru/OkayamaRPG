using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Battle
{
    public class BattleChangeStatus : AbstractEventText, IEventCommandView
    {
        private readonly List<int> hpIndexList = new List<int> {2, 4, 5, 6, 7, 8, 9, 3};
        private readonly List<int> mpIndexList = new List<int> {11, 13, 14, 15, 16, 17, 18, 12};
        private readonly List<int> tpIndexList = new List<int> {20, 22, 23, 24, 25, 21};

        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;


            TitleEdit(eventCommand);

            if (eventCommand.code == (int) EventEnum.EVENT_CODE_BATTLE_CHANGE_STATUS &&
                eventCommand.parameters[1] != "True")
            {
                if (eventCommand.parameters[10] == "True")
                    MPStatus(mpIndexList, eventCommand);
                else if (eventCommand.parameters[19] == "True")
                    TPStatus(tpIndexList, eventCommand);
            }
            else
            {
                switch (eventCommand.code)
                {
                    case (int) EventEnum.EVENT_CODE_BATTLE_CHANGE_STATUS:
                        HPStatus(hpIndexList, eventCommand);
                        break;
                    case (int) EventEnum.EVENT_CODE_BATTLE_CHANGE_MP:
                        MPStatus(mpIndexList, eventCommand);
                        break;
                    case (int) EventEnum.EVENT_CODE_BATTLE_CHANGE_TP:
                        TPStatus(tpIndexList, eventCommand);
                        break;
                }
            }


            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }

        private void TitleEdit(EventDataModel.EventCommand eventCommand) {
            var enemyData = GetEnemyList();
            var enemyid = eventCommand.parameters[0];
            var text = "";
            var count = 0;
            if (eventCommand.parameters[1] == "True")
                count++;
            if (eventCommand.parameters[10] == "True")
                count++;
            if (eventCommand.parameters[19] == "True")
                count++;

            if (eventCommand.code == (int) EventEnum.EVENT_CODE_BATTLE_CHANGE_STATUS &&
                eventCommand.parameters[1] != "True")
            {
                if (eventCommand.parameters[1] == "True")
                    text = " HP";
                else if (eventCommand.parameters[10] == "True")
                    text = " MP";
                else if (eventCommand.parameters[19] == "True")
                    text = " TP";
            }
            else
            {
                switch (eventCommand.code)
                {
                    case (int) EventEnum.EVENT_CODE_BATTLE_CHANGE_STATUS:
                        text = " HP";
                        break;
                    case (int) EventEnum.EVENT_CODE_BATTLE_CHANGE_MP:
                        text = " MP";
                        break;
                    case (int) EventEnum.EVENT_CODE_BATTLE_CHANGE_TP:
                        text = " TP";
                        break;
                }
            }

            if (enemyid == "-1")
            {
                ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_1103") + text + " : Entire Troop, ";
            }
            else
            {
                string data = "";
                int.TryParse(eventCommand.parameters[0], out var memberIndex);
                
 
                if (memberIndex >= 0 && GetEnemyNameList().Count > memberIndex)
                {
                    data = GetEnemyNameList()[memberIndex];
                }
                else
                {
                    memberIndex = 0;
                }
                ret += EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_1103") + text + " : #" + (memberIndex + 1) + " " + data + ", ";
            }
        }

        private void HPStatus(List<int> index, EventDataModel.EventCommand eventCommand) {
            if (eventCommand.parameters[index[0]] == "up")
                ret += " + ";
            else
                ret += " - ";

            if (eventCommand.parameters[index[1]] == "True") ret += eventCommand.parameters[index[2]];

            if (eventCommand.parameters[index[3]] == "True") ret += eventCommand.parameters[index[4]] + "%";

            if (eventCommand.parameters[index[5]] == "True")
            {
                var variables = _GetVariablesList();
                var data = variables.FirstOrDefault(c => c.id == eventCommand.parameters[index[6]]);
                var name = data?.name;
                if (name == "") name = EditorLocalize.LocalizeText("WORD_1518");
                ret += "#" + (variables.IndexOf(data) + 1).ToString("0000") + name;
            }

            if (eventCommand.parameters[index[7]] == "True") ret += " (" + EditorLocalize.LocalizeText("WORD_0896") + ")";
        }

        private void MPStatus(List<int> index, EventDataModel.EventCommand eventCommand) {
            if (eventCommand.parameters[index[0]] == "up")
                ret += " + ";
            else
                ret += " - ";

            if (eventCommand.parameters[index[1]] == "True") ret += eventCommand.parameters[index[2]];

            if (eventCommand.parameters[index[3]] == "True") ret += eventCommand.parameters[index[4]] + "%";

            if (eventCommand.parameters[index[5]] == "True")
            {
                var variables = _GetVariablesList();
                var data = variables.FirstOrDefault(c => c.id == eventCommand.parameters[index[6]]);
                var name = data?.name;
                if (name == "") name = EditorLocalize.LocalizeText("WORD_1518");
                ret += "#" + (variables.IndexOf(data) + 1).ToString("0000") + name;
            }
        }

        private void TPStatus(List<int> index, EventDataModel.EventCommand eventCommand) {
            if (eventCommand.parameters[index[0]] == "up")
                ret += " + ";
            else
                ret += " - ";

            if (eventCommand.parameters[index[1]] == "True") ret += eventCommand.parameters[index[2]];

            if (eventCommand.parameters[index[3]] == "True")
            {
                var variables = _GetVariablesList();
                var data = variables.FirstOrDefault(c => c.id == eventCommand.parameters[index[4]]);
                var name = data?.name;
                if (name == "") name = EditorLocalize.LocalizeText("WORD_1518");
                ret += "#" + (variables.IndexOf(data) + 1).ToString("0000") + name;
            }
        }

        private List<FlagDataModel.Variable> _GetVariablesList() {
            var flagDataModel = DatabaseManagementService.LoadFlags();
            var fileNames = new List<FlagDataModel.Variable>();
            for (var i = 0; i < flagDataModel.variables.Count; i++) fileNames.Add(flagDataModel.variables[i]);

            return fileNames;
        }
    }
}