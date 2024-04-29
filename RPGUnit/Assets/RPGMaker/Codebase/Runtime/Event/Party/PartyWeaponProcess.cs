using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Runtime.Common;

namespace RPGMaker.Codebase.Runtime.Event.Party
{
    /// <summary>
    /// [パーティ]-[武器の増減]
    /// </summary>
    public class PartyWeaponProcess : AbstractEventCommandProcessor
    {
        private DatabaseManagementService _databaseManagementService;

        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            _databaseManagementService = new DatabaseManagementService();
            var runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();

            //増減する装備が今手元にあるかのフラグ
            var haved = false;
            //あった場合のindex
            var index = 0;
            //変動させる数値の保持
            var value = 0;

            for (var i = 0; i < DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.weapons.Count; i++)
            {
                if (DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.weapons[i].weaponId == command.parameters[0])
                {
                    haved = true;
                    index = i;
                    break;
                }
            }

            //何個増減するか
            //定数、変数の分岐
            switch (command.parameters[2])
            {
                case "0":
                    value = int.Parse(command.parameters[3]);
                    break;
                case "1":
                    var flagDataModel = _databaseManagementService.LoadFlags();
                    for (var i = 0; i < flagDataModel.variables.Count; i++)
                    {
                        if (flagDataModel.variables[i].id == command.parameters[3])
                        {
                            value = int.Parse(runtimeSaveDataModel.variables.data[i]);
                            break;
                        }
                    }

                    break;
            }

            //今あるので手持ちのものから増減する
            if (haved)
            {
                //加減算の分岐
                switch (command.parameters[1])
                {
                    case "0":
                        DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.weapons[index].value += value;
                        break;
                    case "1":
                        DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.weapons[index].value -= value;
                        //所持数がマイナスにはならない
                        if (DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.weapons[index].value < 0)
                        {
                            value = -1 * DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.weapons[index].value;
                            DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.weapons[index].value = 0;
                        }
                        else
                        {
                            value = 0;
                        }
                        break;
                }
            }

            //パーティに持っていないか、減算しきれなかった場合
            if (!haved || (command.parameters[1] == "1" && value != 0))
            {
                //加減算の分岐
                switch (command.parameters[1])
                {
                    case "0":
                        var weapon = new RuntimePartyDataModel.Weapon();
                        weapon.weaponId = command.parameters[0];
                        weapon.value = value;
                        DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.weapons.Add(weapon);
                        break;
                    case "1":
                        //装備品から減算可能なら消す
                        if (command.parameters[4] == "1")
                        {
                            foreach (var actor in DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels)
                            {
                                int equipIndex = 0;
                                foreach (var equip in actor.equips)
                                {
                                    if (command.parameters[0] == equip.itemId)
                                    {
                                        //装備を外す
                                        var equipTypes = DataManager.Self().GetSystemDataModel().equipTypes;
                                        SystemSettingDataModel.EquipType equipTypeData = null;
                                        for (int i = 0; i < equipTypes.Count; i++)
                                            if (equipTypes[i].id == actor.equips[equipIndex].equipType)
                                            {
                                                equipTypeData = equipTypes[i];
                                                break;
                                            }
                                        ItemManager.RemoveEquipment(actor, equipTypeData, equipIndex, true);

                                        //装備を変更した場合、GameActorにも反映する
                                        var actors = DataManager.Self().GetGameParty().Actors;
                                        for (int i = 0; i < actors.Count; i++)
                                            if (actors[i].ActorId == actor.actorId)
                                                actors[i].ResetActorData();

                                        //減算する数を-1する
                                        value--;
                                        //減算しきった場合は処理終了
                                        if (value == 0) break;
                                    }
                                    equipIndex++;
                                }
                                if (value == 0) break;
                            }
                        }
                        break;
                }
            }

            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}