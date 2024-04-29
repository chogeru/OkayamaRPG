using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;

namespace RPGMaker.Codebase.Editor.Common
{
    internal static class TagManagerHelper {
        /**
         * Add tags used in RPGMaker Unite to Unity's TagManager.
         */
        private static readonly string[] s_rpgMakerTags =
        {
            "sound", 
            "EventSystem",
            "Second", 
            "Minute", 
            "Canvas", 
            "Rain", 
            "Storm",
            "Snow", 
            "Map LayerDistantView",
            "Map LayerBackground",
            "Map LayerBackgroundCollision",
            "Map LayerA",
            "Map LayerA_Effect",
            "Map LayerB",
            "Map LayerB_Effect",
            "Map LayerShadow",
            "Map LayerC",
            "Map LayerC_Effect",
            "Map LayerD",
            "Map LayerD_Effect",
            "Map LayerForRoute",
            "Map LayerRegion"
        };
        
        internal static void ConfigureDefaultSettings()
        {
            // This script is intended to run on Editor startup only.
            // Do nothing when entering playmode
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
            
            var tags = new List< string >( InternalEditorUtility.tags );

            foreach (var tag in s_rpgMakerTags)
            {
                if (!tags.Contains(tag))
                {
                    InternalEditorUtility.AddTag(tag);
                }
            }
        }
    }    
}