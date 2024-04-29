using System.Linq;
using UnityEditor;

namespace RPGMaker.Codebase.Editor.Common
{
    internal static class EditorBuildSettingsHelper
    {
        private static readonly string[] kRPG_MAKER_SCENES =
        {
            "Assets/RPGMaker/Codebase/Runtime/Title/Title.unity", 
            "Assets/RPGMaker/Codebase/Runtime/Map/SceneMap.unity",
            "Assets/RPGMaker/Codebase/Runtime/Battle/Battle.unity",
            "Assets/RPGMaker/Codebase/Runtime/GameOver/GameOver.unity"
        };
        
        internal static void ConfigureDefaultSettings()
        {
            // This script is intended to run on Editor startup only.
            // Do nothing when entering playmode
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            // Create EditorBuildSettingsScene
            var newScenes = 
                kRPG_MAKER_SCENES.Select(scenePath => new EditorBuildSettingsScene(scenePath, true)).ToList();

            // Add non-RPGMaker scenes from existing config
            newScenes.AddRange(EditorBuildSettings.scenes.Where(s => !kRPG_MAKER_SCENES.Contains(s.path)));

            EditorBuildSettings.scenes = newScenes.ToArray();
        }
    }
}