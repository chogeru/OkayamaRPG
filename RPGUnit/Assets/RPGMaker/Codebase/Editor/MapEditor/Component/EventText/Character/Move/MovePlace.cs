using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.Editor.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Character.Move
{
    public class MovePlace : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            var text = EditorLocalize.LocalizeText("WORD_1514") + EditorLocalize.LocalizeText("WORD_0993") + " : ";
            List<string> direct = EditorLocalize.LocalizeTexts(new List<string>{"WORD_0926", "WORD_0815", "WORD_0813", "WORD_0814", "WORD_0812"});
            var fead = new List<string>
            {
                EditorLocalize.LocalizeText("WORD_0998"),
                EditorLocalize.LocalizeText("WORD_0999"),
                EditorLocalize.LocalizeText("WORD_0113")
            };
            int parse;
            var directNum = 0;
            var feadNum = 0;
            
            if (int.TryParse(eventCommand.parameters[5], out parse))
                directNum = int.Parse(eventCommand.parameters[5]);
            if (int.TryParse(eventCommand.parameters[6], out parse))
                feadNum = int.Parse(eventCommand.parameters[6]);

            var mapManagementService = Editor.Hierarchy.Hierarchy.mapManagementService;
            var mapEntities = mapManagementService.LoadMaps();

            string x, y;

            if (eventCommand.parameters[0] == "0")
            {
                var mapDataModel = mapEntities.FirstOrDefault(c => c.id == eventCommand.parameters[2]);
                text += $"{mapDataModel?.name ?? string.Empty} ";
                x = eventCommand.parameters[3];
                y = eventCommand.parameters[4].TrimStart(new char[] { '-' });
            }
            else
            {
                var variables = _GetVariablesList();
                var mapName = xy(eventCommand.parameters[2]);
                text += $"{mapName} ";
                x = xy(eventCommand.parameters[3]);
                y = xy(eventCommand.parameters[4]);

                string xy(string variableId)
                {
                    var variable = variables.FirstOrDefault(c => c.id == variableId);
                    var name = variable?.name;
                    if (string.IsNullOrEmpty(name))
                    {
                        name = EditorLocalize.LocalizeText("WORD_1518");
                    }

                    return $"#{variables.IndexOf(variable) + 1:0000} {name}";
                }
            }

            text += $"(x : {x}, y : {y})";

            try
            {
                if (eventCommand.parameters[5] != "0" && eventCommand.parameters[6] != "0")
                    text += "(" + EditorLocalize.LocalizeText("WORD_0858") + " : " + direct[directNum] + ", " +
                            EditorLocalize.LocalizeText("WORD_0997") + " : " + fead[feadNum] + ")";
                else if (eventCommand.parameters[5] != "0")
                    text += "(" + EditorLocalize.LocalizeText("WORD_0858") + " : " + direct[directNum] + ")";
                else if (eventCommand.parameters[6] != "0")
                    text += "(" + EditorLocalize.LocalizeText("WORD_0997") + " : " + fead[feadNum] + ")";
            }
            catch (Exception)
            {
            }

            ret += text;


            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }

        private List<FlagDataModel.Variable> _GetVariablesList() {
            var flagDataModel = DatabaseManagementService.LoadFlags();
            var fileNames = new List<FlagDataModel.Variable>();
            for (var i = 0; i < flagDataModel.variables.Count; i++) fileNames.Add(flagDataModel.variables[i]);

            return fileNames;
        }
    }
}