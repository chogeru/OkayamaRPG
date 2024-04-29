using System;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline
{
    [Serializable]
    public class ConnectionDataModel : IOutlineDataModel
    {
        public ConnectionDataModel(
            string id,
            string lUuid,
            int lPortDirection,
            int lPortOrientation,
            string rUuid,
            int rPortDirection,
            int rPortOrientation
        ) {
            ID = id;
            LUuid = lUuid;
            LPortDirection = lPortDirection;
            LPortOrientation = lPortOrientation;
            RUuid = rUuid;
            RPortDirection = rPortDirection;
            RPortOrientation = rPortOrientation;
        }

        public string ID { get; private set; }
        public string LUuid { get; private set; }
        public int LPortDirection { get; private set; }
        public int LPortOrientation { get; private set; }
        public string RUuid { get; private set; }
        public int RPortDirection { get; private set; }
        public int RPortOrientation { get; private set; }

        public static ConnectionDataModel Create(
            string lUuid,
            int lPortDirection,
            int lPortOrientation,
            string rUuid,
            int rPortDirection,
            int rPortOrientation
        ) {
            return new ConnectionDataModel(
                Guid.NewGuid().ToString(),
                lUuid,
                lPortDirection,
                lPortOrientation,
                rUuid,
                rPortDirection,
                rPortOrientation
            );
        }
    }
}