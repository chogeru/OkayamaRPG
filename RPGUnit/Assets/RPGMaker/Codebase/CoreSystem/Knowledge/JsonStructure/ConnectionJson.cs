using System;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure
{
    [Serializable]
    public class ConnectionJson : IJsonStructure
    {
        public string id;
        public int    lPortDirection;
        public int    lPortOrientation;
        public string lUuid;
        public int    rPortDirection;
        public int    rPortOrientation;
        public string rUuid;

        public ConnectionJson(
            string id,
            string lUuid,
            int lPortDirection,
            int lPortOrientation,
            string rUuid,
            int rPortDirection,
            int rPortOrientation
        ) {
            this.id = id;
            this.lUuid = lUuid;
            this.lPortDirection = lPortDirection;
            this.lPortOrientation = lPortOrientation;
            this.rUuid = rUuid;
            this.rPortDirection = rPortDirection;
            this.rPortOrientation = rPortOrientation;
        }

        public string GetID() {
            return id;
        }
    }
}