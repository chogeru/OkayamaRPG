using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure
{
    [Serializable]
    public class SectionJson : IJsonStructure
    {
        public string       chapterId;
        public List<int>    clearFlag;
        public int          deleted;
        public string       description;
        public string       image;
        public List<string> mapIds;
        public string       memo;
        public List<float>  position;
        public string       sectionId;
        public List<int>    startFlag;
        public string       title;

        public SectionJson(
            string sectionId,
            string chapterId,
            string title,
            string description,
            string memo,
            string image,
            List<string> mapIds,
            List<float> position,
            List<int> startFlag,
            List<int> clearFlag,
            int deleted
        ) {
            this.sectionId = sectionId;
            this.chapterId = chapterId;
            this.title = title;
            this.description = description;
            this.memo = memo;
            this.image = image;
            this.mapIds = mapIds;
            this.position = position;
            this.startFlag = startFlag;
            this.clearFlag = clearFlag;
            this.deleted = deleted;
        }

        public string GetID() {
            return sectionId;
        }
    }
}