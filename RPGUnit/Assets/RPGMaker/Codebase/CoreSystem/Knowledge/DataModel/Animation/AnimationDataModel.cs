using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Animation
{
    [Serializable]
    public class AnimationDataModel : WithSerialNumberDataModel
    {
        public int         expansion;
        public List<Flash> flashList;
        public string      id;
        public string      offset;
        public string      particleId;
        public string      particleName;
        public int         particlePos;
        public int         particleType;
        public int         playSpeed;
        public string      rotation;
        public List<Se>    seList;
        public string      targetImageName;

        public AnimationDataModel(
            string id,
            string particleName,
            int particleType,
            int particlePos,
            string particleId,
            string targetImageName,
            int expansion,
            int playSpeed,
            string rotation,
            string offset,
            List<Se> seList,
            List<Flash> flashList
        ) {
            this.id = id;
            this.particleName = particleName;
            this.particleType = particleType;
            this.particlePos = particlePos;
            this.particleId = particleId;
            this.targetImageName = targetImageName;
            this.expansion = expansion;
            this.playSpeed = playSpeed;
            this.rotation = rotation;
            this.offset = offset;
            this.seList = seList;
            this.flashList = flashList;
        }

        public static AnimationDataModel CreateDefault(string id) {
            return new AnimationDataModel(
                id,
                "",
                0,
                0,
                "",
                "",
                0,
                0,
                "",
                "",
                new List<Se>(),
                new List<Flash>());
        }

        public bool isEqual(AnimationDataModel data) {
            if (flashList.Count != data.flashList.Count) 
                return false;
            for (int i = 0; i < flashList.Count; i++)
                if (!flashList[i].isEqual(data.flashList[i]))
                    return false;

            if (seList.Count != data.seList.Count)
                return false;
            for (int i = 0; i < seList.Count; i++)
                if (!seList[i].isEqual(data.seList[i]))
                    return false;

            return expansion == data.expansion &&
                   id == data.id &&
                   offset == data.offset &&
                   particleId == data.particleId &&
                   particleName == data.particleName &&
                   particlePos == data.particlePos &&
                   particleType == data.particleType &&
                   playSpeed == data.playSpeed &&
                   rotation == data.rotation &&
                   targetImageName == data.targetImageName;
        }

        [Serializable]
        public class Flash
        {
            public string color;
            public int    flashId;
            public int    flashType;
            public int    frame;
            public int    time;

            public Flash(int flashId, int frame, int time, string color, int flashType) {
                this.flashId = flashId;
                this.frame = frame;
                this.time = time;
                this.color = color;
                this.flashType = flashType;
            }

            public bool isEqual(Flash data) {
                return color == data.color &&
                       flashId == data.flashId &&
                       flashType == data.flashType &&
                       frame == data.frame &&
                       time == data.time;
            }
        }

        [Serializable]
        public class Se
        {
            public int    frame;
            public int    seId;
            public string seName;

            public Se(int seId, string seName, int frame) {
                this.seId = seId;
                this.seName = seName;
                this.frame = frame;
            }

            public bool isEqual(Se data) {
                return frame == data.frame &&
                       seId == data.seId &&
                       seName == data.seName;
            }
        }
    }
}