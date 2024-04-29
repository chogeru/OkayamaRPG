using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.Editor.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.GameProgress
{
    public class GameVal : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            ret += "◆" + EditorLocalize.LocalizeText("WORD_1015") + " : ";
            var variable = _GetVariablesList();
            if (eventCommand.parameters[8] == "0")
            {
                var data = variable.FirstOrDefault(c => c.id == eventCommand.parameters[0]);
                if (variable.Count == 0)
                {
                    variable.Add(FlagDataModel.Variable.CreateDefault());
                }
                if (data == null) data = variable[0];
                var name = data.name;
                if (name == "") name = EditorLocalize.LocalizeText("WORD_1518");
                ret += "◆ #" + (variable.IndexOf(data) + 1).ToString("0000") + " " + name;
            }
            else
            {
                ret += "#" +
                       int.Parse(eventCommand.parameters[0]).ToString("0000") + "." +
                       int.Parse(eventCommand.parameters[1]).ToString("0000");
            }

            if (int.Parse(eventCommand.parameters[2]) == 0)
                ret += " = ";
            else if (int.Parse(eventCommand.parameters[2]) == 1)
                ret += " += ";
            else if (int.Parse(eventCommand.parameters[2]) == 2)
                ret += " -= ";
            else if (int.Parse(eventCommand.parameters[2]) == 3)
                ret += " *= ";
            else if (int.Parse(eventCommand.parameters[2]) == 4)
                ret += " /= ";
            else
                ret += " %= ";


            if (int.Parse(eventCommand.parameters[3]) == 0)
            {
                ret += int.Parse(eventCommand.parameters[4]).ToString();
            }
            else if (int.Parse(eventCommand.parameters[3]) == 1)
            {
                var data = variable.FirstOrDefault(c => c.id == eventCommand.parameters[4]);
                if (data == null)
                    data = variable[int.Parse(eventCommand.parameters[4])];
                var name = data.name;
                if (name == "") name = EditorLocalize.LocalizeText("WORD_1518");
                ret += "#" + (variable.IndexOf(data) + 1).ToString("0000") + name;
            }
            else if (int.Parse(eventCommand.parameters[3]) == 2)
            {
                ret += EditorLocalize.LocalizeText("WORD_0447") + " " +
                       int.Parse(eventCommand.parameters[4]) +
                       ".." + int.Parse(eventCommand.parameters[5]);
            }
            else if (int.Parse(eventCommand.parameters[3]) == 3)
            {
                var gameDataParam = eventCommand.parameters[4] + "|" +
                                    eventCommand.parameters[5] + "|" +
                                    eventCommand.parameters[6];
                string[] del = {"|"};
                var path = gameDataParam.Split(del, StringSplitOptions.None);
                if (path[0] == "0")
                {
                    var item = DatabaseManagementService.LoadItem();
                    var data = item.FirstOrDefault(c => c.basic.id == path[1]);
                    if (data != null)
                        ret += EditorLocalize.LocalizeText("WORD_1117") + " " + data.basic.name;
                }
                else if (path[0] == "1")
                {
                    ret += EditorLocalize.LocalizeText("WORD_1117") + " " + DatabaseManagementService.LoadWeapon()
                        .FirstOrDefault(c => c.basic.id == path[1])?.basic.name;
                }
                else if (path[0] == "2")
                {
                    if (eventCommand.parameters[7] == EditorLocalize.LocalizeText("WORD_0113"))
                        ret += EditorLocalize.LocalizeText("WORD_0113");
                    else
                        ret += EditorLocalize.LocalizeText("WORD_1117") + " " + DatabaseManagementService.LoadArmor()
                            .FirstOrDefault(c => c.basic.id == eventCommand.parameters[7])?.basic.name;
                }
                else if (path[0] == "3")
                {
                    var status = new List<string>
                    {
                        "WORD_0139", "WORD_0144", "WORD_0133", "WORD_0135", "WORD_0395", "WORD_0539", "WORD_0177",
                        "WORD_0178", "WORD_0179", "WORD_0180", "WORD_0181", "WORD_0182", "WORD_0136"
                    };
                    ret += EditorLocalize.LocalizeText(status[int.Parse(path[2])]) + " of " +
                           DatabaseManagementService.LoadCharacterActor().FirstOrDefault(c => c.uuId == path[1])?.basic
                               .name;
                }
                else if (path[0] == "4")
                {
                    var enemyStatus = new List<string>
                    {
                        "WORD_0133", "WORD_0135", "WORD_0395", "WORD_0539", "WORD_0177", "WORD_0178", "WORD_0179",
                        "WORD_0180", "WORD_0181", "WORD_0182", "WORD_0136"
                    };
                    ret += EditorLocalize.LocalizeText(enemyStatus[int.Parse(path[2])]) + " of #" +
                           DatabaseManagementService.LoadEnemy().FirstOrDefault(c => c.id == path[1])?.name;
                }
                else if (path[0] == "5")
                {
                    var name = "";
                    var dataList = new List<string> {"WORD_1025", "WORD_1026", "WORD_0858", "WORD_1027", "WORD_1028"};
                    if (path[1] == "-1")
                        name = EditorLocalize.LocalizeText("WORD_0920");
                    else if (path[1] == "-2")
                        name = EditorLocalize.LocalizeText("WORD_0860");
                    else
                        name = GetEventDisplayName(path[1]);

                    ret += EditorLocalize.LocalizeText(dataList[int.Parse(path[2])]) + " of " + name;
                }
                else if (path[0] == "6")
                {
                    //Indexが入ってくるので+1して表示する
                    ret += EditorLocalize.LocalizeText("WORD_1604") + " #" + (int.Parse(path[1]) + 1);
                }
                else if (path[0] == "7")
                {
                    var justBefor = new List<string>
                    {
                        "WORD_1035", "WORD_1036", "WORD_1037", "WORD_1038",
                        "WORD_1039", "WORD_1040"
                    };
                    try
                    {
                        ret += EditorLocalize.LocalizeText(justBefor[int.Parse(path[1])]);
                    }
                    catch(Exception)
                    {
                        ret += EditorLocalize.LocalizeText(justBefor[0]);
                    }
                }
                else if (path[0] == "8")
                {
                    var otherList = new List<string>
                    {
                        "WORD_0995", "WORD_1041", "WORD_0581", "WORD_0698", "WORD_1042", "WORD_1043", "WORD_1044",
                        "WORD_1045", "WORD_1046", "WORD_1047"
                    };
                    ret += EditorLocalize.LocalizeText(otherList[int.Parse(path[1])]);
                }
            }

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }

        private List<FlagDataModel.Variable> _GetVariablesList() {
            var flagDataModel = DatabaseManagementService.LoadFlags();
            var fileNames = new List<FlagDataModel.Variable>();
            for (var i = 0; i < flagDataModel.variables.Count; i++)
                fileNames.Add(flagDataModel.variables[i]);
            return fileNames;
        }
    }
}