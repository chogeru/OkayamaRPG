using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class FlagsSO : ScriptableObject
    {
        public FlagDataModel dataModel;

        public bool isEquals(FlagsSO flagsSO) {
            if (dataModel.switches.Count != flagsSO.dataModel.switches.Count)
                return false;

            for (int i = 0; i < dataModel.switches.Count; i++)
                if (!dataModel.switches[i].isEqual(flagsSO.dataModel.switches[i]))
                    return false;

            return true;
        }
    }
}