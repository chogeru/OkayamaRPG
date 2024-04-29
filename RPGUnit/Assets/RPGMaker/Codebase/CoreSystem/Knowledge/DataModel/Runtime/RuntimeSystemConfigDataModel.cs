using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime
{
    [Serializable]
    public class RuntimeSystemConfigDataModel {
        public Sound battleBgm;
        public int battleCount;
        public Sound bgmOnSave;
        public Sound bgsOnSave;
        public int chapterId;
        public Sound defeatMe;
        public int encounterEnabled;
        public int escapeCount;
        public int formationEnabled;
        public float playTime;
        public int menuEnabled;
        public int saveCount;
        public Sound savedBgm;
        public int saveEnabled;
        public int sectionId;
        public Sound victoryMe;
        public Sound walkingBgm;
        public int winCount;
        public List<int> windowTone;
        public int follow = -1;
        public List<VehicleSound> vehicleSound;

        public RuntimeSystemConfigDataModel(
            int saveEnabled,
            int menuEnabled,
            int encounterEnabled,
            int formationEnabled,
            int battleCount,
            int winCount,
            int escapeCount,
            int saveCount,
            int playTime,
            Sound bgmOnSave,
            Sound bgsOnSave,
            List<int> windowTone,
            Sound battleBgm,
            Sound victoryMe,
            Sound defeatMe,
            Sound savedBgm,
            Sound walkingBgm,
            int chapterId,
            int sectionId,
            int follow = -1
        ) {
            this.saveEnabled = saveEnabled;
            this.menuEnabled = menuEnabled;
            this.encounterEnabled = encounterEnabled;
            this.formationEnabled = formationEnabled;
            this.battleCount = battleCount;
            this.winCount = winCount;
            this.escapeCount = escapeCount;
            this.saveCount = saveCount;
            this.playTime = playTime;
            this.bgmOnSave = bgmOnSave;
            this.bgsOnSave = bgsOnSave;
            this.windowTone = windowTone;
            this.battleBgm = battleBgm;
            this.victoryMe = victoryMe;
            this.defeatMe = defeatMe;
            this.savedBgm = savedBgm;
            this.walkingBgm = walkingBgm;
            this.chapterId = chapterId;
            this.sectionId = sectionId;
            this.playTime = playTime;
            this.follow = follow;
            this.vehicleSound = new List<VehicleSound>();
        }

        public static RuntimeSystemConfigDataModel CreateDefault() {
            return new RuntimeSystemConfigDataModel(
                1,
                1,
                1,
                1,
                0,
                0,
                0,
                0,
                0,
                Sound.CreateDefault(),
                Sound.CreateDefault(),
                new List<int>(),
                Sound.CreateDefault(),
                Sound.CreateDefault(),
                Sound.CreateDefault(),
                Sound.CreateDefault(),
                Sound.CreateDefault(),
                0,
                0,
                -1
            );
        }

        [Serializable]
        public class VehicleSound
        {
            public string id;
            public Sound sound;
        }

        [Serializable]
        public class Sound
        {
            public string name;
            public int    pan;
            public int    pitch;
            public int    volume;

            public Sound(string name, int pan, int pitch, int volume) {
                this.name = name;
                this.pan = pan;
                this.pitch = pitch;
                this.volume = volume;
            }

            public static Sound CreateDefault() {
                return new Sound("", 0, 0, 100);
            }
        }
    }
}