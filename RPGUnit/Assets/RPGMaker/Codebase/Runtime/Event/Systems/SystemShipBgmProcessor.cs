using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Runtime.Common;
using System.Collections.Generic;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime.RuntimeSystemConfigDataModel;

namespace RPGMaker.Codebase.Runtime.Event.Systems
{
    /// <summary>
    /// [システム]-[乗り物BGM変更]
    /// </summary>
    public class SystemShipBgmProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            var databaseManagementService = new DatabaseManagementService();
            var vehiclesDataModels = databaseManagementService.LoadCharacterVehicles();

            //IDから乗り物を探す
            VehicleSound sound = new VehicleSound();
            for (var i = 0; i < vehiclesDataModels.Count; i++)
                if (vehiclesDataModels[i].id == command.parameters[4])
                {
                    //IDの一致した乗り物のデータの変更
                    var runtimeSystemConfig = DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig;
                    if (runtimeSystemConfig.vehicleSound == null)
                        runtimeSystemConfig.vehicleSound = new List<VehicleSound>();

                    bool flg = false;
                    for (int j = 0; j < runtimeSystemConfig.vehicleSound.Count; j++)
                    {
                        if (runtimeSystemConfig.vehicleSound[i].id == vehiclesDataModels[i].id)
                        {
                            runtimeSystemConfig.vehicleSound[i].sound.name = command.parameters[0];
                            runtimeSystemConfig.vehicleSound[i].sound.volume = int.Parse(command.parameters[1]);
                            runtimeSystemConfig.vehicleSound[i].sound.pitch = int.Parse(command.parameters[2]);
                            runtimeSystemConfig.vehicleSound[i].sound.pan = int.Parse(command.parameters[3]);
                            sound = runtimeSystemConfig.vehicleSound[i];
                            flg = true;
                            break;
                        }
                    }
                    if (!flg)
                    {
                        sound.id = vehiclesDataModels[i].id;
                        sound.sound = new Sound(command.parameters[0], int.Parse(command.parameters[3]), int.Parse(command.parameters[2]), int.Parse(command.parameters[1]));
                        runtimeSystemConfig.vehicleSound.Add(sound);
                    }
                    break;
                }

            //データの保存
            databaseManagementService.SaveCharacterVehicles(vehiclesDataModels);
            //次へ
            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}