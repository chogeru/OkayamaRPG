namespace RPGMaker.Codebase.CoreSystem.Knowledge.Misc
{
    public class Bitmap
    {
        public float height = 0;

        public float width = 0;

        public Bitmap(UnityEngine.Sprite unitySprite) {
            UnitySprite = unitySprite;
        }

        public UnityEngine.Sprite UnitySprite { get; set; }

        public bool IsReady() {
            return true;
        }
    }
}