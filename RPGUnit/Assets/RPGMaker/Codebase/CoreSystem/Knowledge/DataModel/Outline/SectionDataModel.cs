using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline
{
    public class SectionDataModel : WithSerialNumberDataModel, IOutlineDataModel
    {
        public SectionDataModel(
            string uuid,
            string chapterID,
            string name,
            List<MapSubDataModel> maps,
            List<SwitchSubDataModel> belongingSwitches,
            List<SwitchSubDataModel> referringSwitches,
            List<string> relatedBySwitchSectionIds,
            string memo,
            float posX = 0,
            float posY = 0
        ) {
            ID = uuid;
            ChapterID = chapterID;
            Name = name;
            Maps = maps;
            BelongingSwitches = belongingSwitches;
            ReferringSwitches = referringSwitches;
            RelatedBySwitchSectionIds = relatedBySwitchSectionIds;
            Memo = memo;
            PosX = posX;
            PosY = posY;
        }

        public string ID { get; set; }
        public string ChapterID { get; set; }
        public string Name { get; set; }
        public List<MapSubDataModel> Maps { get; set; }
        public List<SwitchSubDataModel> BelongingSwitches { get; set; } // このセクションに所属するスイッチ一覧
        public List<SwitchSubDataModel> ReferringSwitches { get; set; } // on/offがこのセクションに影響を与えるスイッチ一覧
        public List<string> RelatedBySwitchSectionIds { get; set; } // スイッチによって関連付けられた（Nextとしてコネクトされる）セクションID一覧
        public string Memo { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }

        public static SectionDataModel Create() {
            return new SectionDataModel(
                Guid.NewGuid().ToString(),
                null,
                "New Section",
                new List<MapSubDataModel>(),
                new List<SwitchSubDataModel>(),
                new List<SwitchSubDataModel>(),
                new List<string>(),
                ""
            );
        }
    }
}