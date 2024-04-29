using System;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline
{
    [Serializable]
    public class StartDataModel : IOutlineDataModel
    {
        public StartDataModel(
            string uuid,
            string name,
            string memo,
            float posX = 0,
            float posY = 0
        ) {
            ID = uuid;
            Name = name;
            Memo = memo;
            PosX = posX;
            PosY = posY;
        }

        public string ID { get; set; }
        public string Name { get; set; }
        public string Memo { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }

        public static StartDataModel Create() {
            return new StartDataModel(
                Guid.NewGuid().ToString(),
                "New Start",
                "",
                0,
                0
            );
        }
    }
}