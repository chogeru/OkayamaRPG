using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.Editor.Common;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Actor
{
    public class ActorChangeEquipment : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;

            //各種データ読込
            var characterActorDataModels = DatabaseManagementService.LoadCharacterActor();
            var systemSettingDataModel = DatabaseManagementService.LoadSystem();

            //装備タイプ = 武器、盾、頭、体、その他ユーザーが設定したリスト
            var equipTypes = DatabaseManagementService.LoadSystem().equipTypes;

            var actorId = eventCommand.parameters[0];
            if (actorId == "-1")
            {
                //最初のアクターを選択状態とする
                ret += "◆" + EditorLocalize.LocalizeText("WORD_0911") + " : " + characterActorDataModels[0].basic.name +
                       ",";
            }
            else
            {
                //設定されているアクターを選択状態とする
                var index = characterActorDataModels.IndexOf(
                    characterActorDataModels.FirstOrDefault(c => c.uuId == actorId));
                if (index < 0) index = 0;
                ret += "◆" + EditorLocalize.LocalizeText("WORD_0911") + " : " +
                       characterActorDataModels[index].basic.name + ",";
            }


            //設定されているアイテムは、装備タイプが武器なら武器リストから、防具なら防具リストから検索する
            var equipId = equipTypes.IndexOf(equipTypes.FirstOrDefault(c => c.id == eventCommand.parameters[1]));
            if (equipId < 0) //見つからない場合は武器の方に流す
                equipId = 0;
            if (equipId == 0)
            {
                //武器
                var weaponList = DatabaseManagementService.LoadWeapon();
                WeaponDataModel weaponData = null;
                for (var i = 0; i < weaponList.Count; i++)
                    if (weaponList[i].basic.id == eventCommand.parameters[2])
                    {
                        weaponData = weaponList[i];
                        break;
                    }

                if (weaponData == null)
                    ret += equipTypes[equipId].name + " = " + EditorLocalize.LocalizeText("WORD_0113");
                else
                    ret += equipTypes[equipId].name + " = " + weaponData.basic.name;
            }
            else
            {
                //防具
                var armorList = DatabaseManagementService.LoadArmor();
                ArmorDataModel armorData = null;
                for (var i = 0; i < armorList.Count; i++)
                    if (armorList[i].basic.id == eventCommand.parameters[2])
                    {
                        armorData = armorList[i];
                        break;
                    }

                if (armorData == null)
                    ret += equipTypes[equipId].name + " = " + EditorLocalize.LocalizeText("WORD_0113");
                else
                    ret += equipTypes[equipId].name + " = " + armorData.basic.name;
            }

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}