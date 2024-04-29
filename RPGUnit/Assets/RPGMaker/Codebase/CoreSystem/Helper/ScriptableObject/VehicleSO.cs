using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Vehicle;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class VehicleSO : ScriptableObject
    {
        public List<VehiclesDataModel> dataModels;

        public bool isEquals(VehicleSO vehicleSO) {
            if (dataModels.Count != vehicleSO.dataModels.Count)
                return false;

            for (int i = 0; i < dataModels.Count; i++)
                if (dataModels[i].isEqual(vehicleSO.dataModels[i]) == false)
                    return false;

            return true;
        }
    }
}