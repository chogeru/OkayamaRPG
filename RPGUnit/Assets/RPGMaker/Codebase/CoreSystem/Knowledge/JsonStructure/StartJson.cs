using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure
{
    [Serializable]
    public class StartJson : IJsonStructure
    {
        public int         beforeStartId;
        public int         deleted;
        public string      description;
        public int         nextStartId;
        public List<float> position;
        public string      startId;
        public string      title;

        public StartJson(
            string startId,
            string title,
            string description,
            int beforeStartId,
            int nextStartId,
            List<float> position,
            int deleted
        ) {
            this.startId = startId;
            this.title = title;
            this.description = description;
            this.beforeStartId = beforeStartId;
            this.nextStartId = nextStartId;
            this.position = position;
            this.deleted = deleted;
        }

        public string GetID() {
            return startId;
        }
    }
}