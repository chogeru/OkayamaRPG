using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map;
using UnityEngine; //バトルでは本コマンドは利用しない
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime.RuntimePlayerDataModel;

namespace RPGMaker.Codebase.Runtime.Event.Character
{
    /// <summary>
    /// [キャラクター]-[乗り物位置指定]
    /// </summary>
    public class MovePlaceShipProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            var pos = new Vector2(int.Parse(command.parameters[3]), int.Parse(command.parameters[4]));
            MovePlaceShip(
                command.parameters[0],
                command.parameters[1] == "0" ? true : false,
                command.parameters[2],
                pos
            );
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }

        private void MovePlaceShip(string vehicle, bool isVariable, string mapEvent, Vector2 pos) {
            Vhicle vehicleData = null;
            for (var i = 0; i < DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.vehicles.Count; i++)
                if (DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.vehicles[i].id == vehicle)
                {
                    DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.vehicles[i].mapId = mapEvent;
                    DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.vehicles[i].x = (int) pos.x;
                    DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.vehicles[i].y = (int) pos.y;
                    vehicleData = DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.vehicles[i];
                    break;
                }

            MapManager.SetVehicleOnMap(new Vector2(pos.x, pos.y), vehicleData);
            CloseCharacterAnimationSetting();
        }

        private void CloseCharacterAnimationSetting() {
            ProcessEndAction();
        }
    }
}