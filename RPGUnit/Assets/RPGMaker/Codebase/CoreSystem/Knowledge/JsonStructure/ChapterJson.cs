using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure
{
    [Serializable]
    public class ChapterJson : IJsonStructure
    {
        public int         beforeChapterId;
        public string      chapterId;
        public int         deleted;
        public string      description;
        public string      image;
        public int         levelHigh;
        public int         levelLow;
        public string      mapId;
        public int         nextChapterId;
        public List<float> position;
        public string      title;

        public ChapterJson(
            string chapterId,
            string title,
            int levelLow,
            int levelHigh,
            string description,
            string image,
            string mapId,
            int beforeChapterId,
            int nextChapterId,
            List<float> position,
            int deleted
        ) {
            this.chapterId = chapterId;
            this.title = title;
            this.levelLow = levelLow;
            this.levelHigh = levelHigh;
            this.description = description;
            this.image = image;
            this.mapId = mapId;
            this.beforeChapterId = beforeChapterId;
            this.nextChapterId = nextChapterId;
            this.position = position;
            this.deleted = deleted;
        }

        public string GetID() {
            return chapterId;
        }
    }
}