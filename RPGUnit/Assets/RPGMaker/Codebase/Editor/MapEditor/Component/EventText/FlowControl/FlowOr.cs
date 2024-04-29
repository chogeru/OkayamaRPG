using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Enemy;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.Editor.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.FlowControl
{
    public class FlowOr : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;

            ret += "◇OR " + EditorLocalize.LocalizeText("WORD_0600") + " : ";
            var kind = int.Parse(eventCommand.parameters[0]);
            switch ((FlowKind) kind)
            {
                case FlowKind.SWITCH:
                    var switches = _GetSwitchList();
                    ret += EditorLocalize.LocalizeText("WORD_0605");

                    var data = switches.FirstOrDefault(c => c.id == eventCommand.parameters[1]);
                    var name = data.name;
                    if (name == "") name = EditorLocalize.LocalizeText("WORD_1518");

                    ret += "#" + (switches.IndexOf(data) + 1).ToString("0000") + name + " = ";

                    if (int.Parse(eventCommand.parameters[2]) == 0)
                        ret += "ON";
                    else
                        ret += "OFF";

                    break;
                case FlowKind.VARIABLE:
                    var variables = _GetVariablesList();
                    ret += EditorLocalize.LocalizeText("WORD_0839");

                    var variablesData = variables.FirstOrDefault(c => c.id == eventCommand.parameters[1]);
                    var variablesName = variablesData.name;
                    if (variablesName == "") variablesName = EditorLocalize.LocalizeText("WORD_1518");

                    ret += "#" + (variables.IndexOf(variablesData) + 1).ToString("0000") + variablesName;

                    var formula = new List<string> {"=", "≧", "≦", "＞", "＜", "≠"};
                    ret += " " + formula[int.Parse(eventCommand.parameters[2])] + " ";
                    if (int.Parse(eventCommand.parameters[3]) != 0)
                    {
                        ret += eventCommand.parameters[4];
                    }
                    else
                    {
                        var data2 = variables.FirstOrDefault(c => c.id == eventCommand.parameters[5]);
                        var name2 = "";
                        if (data2 != null)
                            name2 = data2.name;
                        else
                            data2 = variables[0];
                        if (name2 == "") name2 = EditorLocalize.LocalizeText("WORD_1518");

                        ret += "#" + (variables.IndexOf(data2) + 1).ToString("0000") + name2;
                    }

                    break;
                case FlowKind.SELF_SWITCH:
                    var selfSwitchList = new List<string> {"A", "B", "C", "D"};
                    ret += EditorLocalize.LocalizeText("WORD_0840") + " " +
                           selfSwitchList[
                               int.Parse(eventCommand.parameters[1])] + " = ";
                    if (int.Parse(eventCommand.parameters[2]) == 0)
                        ret += "ON";
                    else
                        ret += "OFF";

                    break;
                case FlowKind.TIMER:
                    ret += EditorLocalize.LocalizeText("WORD_1043");
                    var timer = new List<string> {"≧", "≦"};
                    ret +=
                        " " + timer[int.Parse(eventCommand.parameters[1])] + " ";
                    ret +=
                        int.Parse(eventCommand.parameters[2]) + EditorLocalize.LocalizeText("WORD_1051") + " " +
                        int.Parse(eventCommand.parameters[3]) + EditorLocalize.LocalizeText("WORD_0938");
                    break;
                case FlowKind.ACTOR:
                    ret += EditorLocalize.LocalizeText("WORD_0301");
                    ret += DatabaseManagementService.LoadCharacterActor()
                        .FirstOrDefault(c => c.uuId == eventCommand.parameters[1])?.name + " ";
                    if (int.Parse(eventCommand.parameters[2]) == 0)
                        ret += EditorLocalize.LocalizeText("WORD_1600");
                    else if (int.Parse(eventCommand.parameters[2]) == 1)
                        ret += EditorLocalize.LocalizeText("WORD_0039") + " = " + eventCommand.parameters[3];
                    else if (int.Parse(eventCommand.parameters[2]) == 2)
                        ret += EditorLocalize.LocalizeText("WORD_0336") + " = " + DatabaseManagementService
                            .LoadCharacterActorClass()
                            .FirstOrDefault(c => c.basic.id == eventCommand.parameters[4])
                            ?.basic.name;
                    else if (int.Parse(eventCommand.parameters[2]) == 3)
                        ret += DatabaseManagementService.LoadSkillCustom()
                                   .FirstOrDefault(c => c.basic.id == eventCommand.parameters[5])?.basic.name +
                               EditorLocalize.LocalizeText("WORD_1145");
                    else if (int.Parse(eventCommand.parameters[2]) == 4)
                        ret += DatabaseManagementService.LoadWeapon()
                                   .FirstOrDefault(c => c.basic.id == eventCommand.parameters[6])?.basic.name +
                               EditorLocalize.LocalizeText("WORD_1146");
                    else if (int.Parse(eventCommand.parameters[2]) == 5)
                        ret += DatabaseManagementService.LoadArmor()
                                   .FirstOrDefault(c => c.basic.id == eventCommand.parameters[7])?.basic.name +
                               EditorLocalize.LocalizeText("WORD_1146");
                    else if (int.Parse(eventCommand.parameters[2]) == 6)
                        ret += DatabaseManagementService.LoadStateEdit()
                                   .FirstOrDefault(c => c.id == eventCommand.parameters[8])?.name +
                               EditorLocalize.LocalizeText("WORD_1147");

                    break;
                case FlowKind.ENEMY:
                    ret += EditorLocalize.LocalizeText("WORD_0559");

                    int.TryParse(eventCommand.parameters[1], out var memberIndex);
                    if (GetEnemyNameList().Count <= memberIndex)
                    {
                        memberIndex = 0;
                    }

                    string enemyName = "";
                    if (GetEnemyNameList().Count > memberIndex)
                        enemyName = GetEnemyNameList()[memberIndex];
                    ret += $"#{memberIndex + 1} {enemyName}";

                    if (int.Parse(eventCommand.parameters[2]) == 0)
                        ret += EditorLocalize.LocalizeText("WORD_1148");
                    else
                        ret += DatabaseManagementService.LoadStateEdit()
                                   .FirstOrDefault(c => c.id == eventCommand.parameters[3])?.name +
                               EditorLocalize.LocalizeText("WORD_1147");

                    break;
                case FlowKind.CHARACTER:
                    ret += EditorLocalize.LocalizeText("WORD_0287");
                    var characterName = "";
                    if (eventCommand.parameters[1] == "-2")
                        characterName = EditorLocalize.LocalizeText("WORD_0860");
                    else if (eventCommand.parameters[1] == "-1")
                        characterName = EditorLocalize.LocalizeText("WORD_0920");
                    else
                        try
                        {
                            var events =
                                EventManagementService.LoadEvent();
                            characterName += "EV" + (events.IndexOf(events.FirstOrDefault
                                    (c => c.id == eventCommand.parameters[1])) + 1)
                                .ToString("0000");
                        }
                        catch (Exception)
                        {
                        }

                    var direct = new List<string> {"WORD_0299", "WORD_0813", "WORD_0814", "WORD_0297"};
                    ret += characterName + EditorLocalize.LocalizeText("WORD_1149") + " " +
                           EditorLocalize.LocalizeText(direct[int.Parse(eventCommand.parameters[2])]);
                    break;
                case FlowKind.VEHICLE:
                    ret += EditorLocalize.LocalizeText("WORD_1009");
                    ret += DatabaseManagementService.LoadCharacterVehicles()
                               .FirstOrDefault(c => c.id == eventCommand.parameters[1])?.name + " " +
                           EditorLocalize.LocalizeText("WORD_1150");
                    break;
                case FlowKind.GOLD:
                    ret += EditorLocalize.LocalizeText("WORD_1544");
                    var gold = EditorLocalize.LocalizeTexts(new List<string> {"WORD_1509", "WORD_1510", "WORD_1512"});
                    ret += EditorLocalize.LocalizeText("WORD_0581") + " " +
                           gold[int.Parse(eventCommand.parameters[1])] +
                           " " + int.Parse(eventCommand.parameters[2]);
                    break;
                case FlowKind.ITEM:
                    ret += EditorLocalize.LocalizeText("WORD_0068");
                    ret += DatabaseManagementService.LoadItem()
                               .FirstOrDefault(c => c.basic.id == eventCommand.parameters[1])?.basic.name +
                           EditorLocalize.LocalizeText("WORD_1151");
                    break;
                case FlowKind.WEAPON:
                    ret += EditorLocalize.LocalizeText("WORD_0128");
                    ret += DatabaseManagementService.LoadWeapon()
                               .FirstOrDefault(c => c.basic.id == eventCommand.parameters[1])?.basic.name +
                           EditorLocalize.LocalizeText("WORD_1151");
                    break;
                case FlowKind.ARMOR:
                    ret += EditorLocalize.LocalizeText("WORD_0129");
                    ret += DatabaseManagementService.LoadArmor()
                               .FirstOrDefault(c => c.basic.id == eventCommand.parameters[1])?.basic.name +
                           EditorLocalize.LocalizeText("WORD_1151");
                    break;
                case FlowKind.BUTTON:
                    var buttons = new List<string>
                    {
                        "WORD_1154",
                        "WORD_1155",
                        "WORD_1156",
                        "WORD_1157",
                        "WORD_1158",
                        "WORD_1159",
                        "WORD_1160",
                        "WORD_1161",
                        "WORD_1162"
                    };
                    var pushd = new List<string>
                    {
                        "WORD_1163",
                        "WORD_1164",
                        "WORD_1165"
                    };
                    ret += EditorLocalize.LocalizeText("WORD_1153") + " [" + EditorLocalize.LocalizeText(buttons[int.Parse(eventCommand.parameters[1])]) +
                           "] " +
                           EditorLocalize.LocalizeText(pushd[int.Parse(eventCommand.parameters[2])]);
                    break;
            }

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }

        private List<FlagDataModel.Switch> _GetSwitchList() {
            var flagDataModel = DatabaseManagementService.LoadFlags();
            var fileNames = new List<FlagDataModel.Switch>();
            for (var i = 0; i < flagDataModel.switches.Count; i++) fileNames.Add(flagDataModel.switches[i]);

            return fileNames;
        }

        private List<FlagDataModel.Variable> _GetVariablesList() {
            var flagDataModel = DatabaseManagementService.LoadFlags();
            var fileNames = new List<FlagDataModel.Variable>();
            for (var i = 0; i < flagDataModel.variables.Count; i++) fileNames.Add(flagDataModel.variables[i]);

            return fileNames;
        }

        private List<EnemyDataModel> _GetEnemyList() {
            var enemyDataModels = DatabaseManagementService.LoadEnemy();
            var fileNames = new List<EnemyDataModel>();
            for (var i = 0; i < enemyDataModels.Count; i++)
                if (enemyDataModels[i].deleted == 0)
                    fileNames.Add(enemyDataModels[i]);

            return fileNames;
        }

        private enum FlowKind
        {
            SWITCH,
            VARIABLE,
            SELF_SWITCH,
            TIMER,
            ACTOR,
            ENEMY,
            CHARACTER,
            VEHICLE,
            GOLD,
            ITEM,
            WEAPON,
            ARMOR,
            BUTTON
        }
    }
}