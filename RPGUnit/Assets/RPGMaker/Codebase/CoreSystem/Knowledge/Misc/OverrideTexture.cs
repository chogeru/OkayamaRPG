using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.Misc
{
    public class OverrideTexture
    {
        public float height = 0;

        public float width = 0;

        public OverrideTexture(Texture unityTexture) {
            UnityTexture = unityTexture;
        }

        public Texture UnityTexture { get; set; }

        public bool IsReady() {
            return true;
        }
    }
}