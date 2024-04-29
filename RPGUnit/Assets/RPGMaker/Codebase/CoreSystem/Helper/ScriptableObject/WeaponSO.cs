using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class WeaponSO : ScriptableObject
    {
        public List<WeaponDataModel> dataModels;

        public bool isEquals(WeaponSO weaponSO) {
            if (dataModels.Count != weaponSO.dataModels.Count)
                return false;

            for (int i = 0; i < dataModels.Count; i++)
                if (dataModels[i].isEqual(weaponSO.dataModels[i]) == false)
                    return false;

            return true;
        }
    }
}