using System;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common
{
    [Serializable]
    public class TraitCommonDataModel
    {
        public int categoryId;
        public int effectId;
        public int traitsId;
        public int value;

        public TraitCommonDataModel(int categoryId, int traitsId, int effectId, int value) {
            this.categoryId = categoryId;
            this.traitsId = traitsId;
            this.effectId = effectId;
            this.value = value;
        }

        public bool isEqual(TraitCommonDataModel data) {
            return categoryId == data.categoryId &&
                   effectId == data.effectId &&
                   traitsId == data.traitsId &&
                   value == data.value;
        }
    }
}