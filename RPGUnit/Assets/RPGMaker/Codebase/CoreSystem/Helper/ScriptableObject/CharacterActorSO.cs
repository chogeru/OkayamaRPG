using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper.SO
{
    public class CharacterActorSO : ScriptableObject
    {
        public List<CharacterActorDataModel> dataModels;

        public bool isEquals(CharacterActorSO characterActorSO) {
            if (dataModels.Count != characterActorSO.dataModels.Count)
                return false;

            for (int i = 0; i < dataModels.Count; i++)
                if (!dataModels[i].isEqual(characterActorSO.dataModels[i]))
                    return false;

            return true;
        }
    }
}