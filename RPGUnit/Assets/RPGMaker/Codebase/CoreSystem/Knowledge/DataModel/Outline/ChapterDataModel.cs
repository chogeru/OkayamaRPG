using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline
{
    [Serializable]
    public class ChapterDataModel : WithSerialNumberDataModel, IOutlineDataModel
    {
        public ChapterDataModel(
            string uuid,
            string name,
            int supposedLevelMin,
            int supposedLevelMax,
            MapSubDataModel fieldMapSubDataModel,
            string memo,
            float posX = 0,
            float posY = 0
        ) {
            ID = uuid;
            Name = name;
            SupposedLevelMin = supposedLevelMin;
            SupposedLevelMax = supposedLevelMax;
            FieldMapSubDataModel = fieldMapSubDataModel;
            Memo = memo;
            PosX = posX;
            PosY = posY;
        }

        public string ID { get; set; }
        public string Name { get; set; }
        public int SupposedLevelMin { get; set; } // 当該チャプターに到達する想定レベル
        public int SupposedLevelMax { get; set; }
        public List<SectionDataModel> Sections { get; set; }
        public MapSubDataModel FieldMapSubDataModel { get; set; }
        public string Memo { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }

        public static ChapterDataModel Create() {
            return new ChapterDataModel(
                Guid.NewGuid().ToString(),
                "New Chapter",
                0,
                0,
                null,
                ""
            );
        }
    }
}