using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.SceneControl
{
    public class SceneSetShopConfig : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;

            // 先頭
            if (eventCommand.code == (int) EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG)
            {
                ret += "◆" + EditorLocalize.LocalizeText("WORD_1059") + " : ";
                if (eventCommand.parameters[4] == "1")
                    ret += EditorLocalize.LocalizeText("WORD_1060");
            }
            else
            {
                ret += "                       : ";

                int parse;
                if (eventCommand.parameters[0] == "0")
                {
                    var itemDataModels = DatabaseManagementService.LoadItem();
                    if (int.TryParse(eventCommand.parameters[1], out parse) &&
                        itemDataModels.Count > int.Parse(eventCommand.parameters[1]))
                        ret += itemDataModels[int.Parse(eventCommand.parameters[1])].basic.name;
                    else
                        foreach (var data in itemDataModels)
                            if (data.basic.id == eventCommand.parameters[1])
                                ret += data.basic.name;
                }
                else if (eventCommand.parameters[0] == "1")
                {
                    var weaponDataModels = DatabaseManagementService.LoadWeapon();
                    if (int.TryParse(eventCommand.parameters[1], out parse) &&
                        weaponDataModels.Count > int.Parse(eventCommand.parameters[1]))
                        ret += weaponDataModels[int.Parse(eventCommand.parameters[1])].basic.name;
                    else
                        foreach (var data in weaponDataModels)
                            if (data.basic.id == eventCommand.parameters[1])
                                ret += data.basic.name;
                }
                else if (eventCommand.parameters[0] == "2")
                {
                    var armorDataModels = DatabaseManagementService.LoadArmor();
                    if (int.TryParse(eventCommand.parameters[1], out parse) &&
                        armorDataModels.Count > int.Parse(eventCommand.parameters[1]))
                        ret += armorDataModels[int.Parse(eventCommand.parameters[1])].basic.name;
                    else
                        foreach (var data in armorDataModels)
                            if (data.basic.id == eventCommand.parameters[1])
                                ret += data.basic.name;
                }
            }

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}