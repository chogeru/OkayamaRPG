using System;

#if UNITY_EDITOR
#endif

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map
{
    [Serializable]
    public class VehicleType
    {
        public string                 vehicleId;
        public TileDataModel.PassType vehiclePassType;

        public VehicleType(TileDataModel.PassType vehiclePassType, string vehicleId) {
            this.vehiclePassType = vehiclePassType;
            this.vehicleId = vehicleId;
        }

	    public bool isEqual(VehicleType vehicleType)
	    {
	        return vehicleId == vehicleType.vehicleId &&
	               vehiclePassType == vehicleType.vehiclePassType;
	    }
    }
}