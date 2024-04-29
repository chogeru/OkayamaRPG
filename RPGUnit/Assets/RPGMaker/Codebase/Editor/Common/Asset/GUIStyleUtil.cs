using UnityEngine;

namespace Assets.RPGMaker.Codebase.Editor.Common.Asset
{
    public static class GUIStyleUtil
    {
        public static Vector2 CalcSize( 
            this GUIStyle self, 
            string text 
        )
        {
            var content = new GUIContent( text );
            var size = self.CalcSize( content );
            return size;
        }
    }
}