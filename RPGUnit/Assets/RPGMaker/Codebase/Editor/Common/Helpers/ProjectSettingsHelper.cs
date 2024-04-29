using System;
using UnityEditor;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.Common
{
    internal static class ProjectSettingsHelper
    {
        private static readonly string kUniteLogoPath = 
            "Assets/RPGMaker/Storage/System/Images/SplashScreen/Unite_SplashScreen.png";

        private const int kDefaultPlayerHeight = 1920; 
        private const int kDefaultPlayerWidth  = 1080; 
        
        internal static void ConfigureDefaultSettings()
        {
            // This script is intended to run on Editor startup only.
            // Do nothing when entering playmode
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            var logos = new PlayerSettings.SplashScreenLogo[2];

            // Unity Logo
            logos[0] = PlayerSettings.SplashScreenLogo.CreateWithUnityLogo();

            // RPGMaker Unite logo
            var uniteLogo = (Sprite)AssetDatabase.LoadAssetAtPath(kUniteLogoPath, typeof(Sprite));
            logos[1] = PlayerSettings.SplashScreenLogo.Create(2f, uniteLogo);
            
            PlayerSettings.SplashScreen.logos = logos;
            PlayerSettings.defaultWebScreenWidth = kDefaultPlayerHeight;
            PlayerSettings.defaultScreenHeight = kDefaultPlayerWidth;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.asyncShaderCompilation = false;
            
#if true
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (symbols.Contains("SET_PRODUCTSETTING") == false)
            {
                symbols += ";SET_PRODUCTSETTING";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
                PlayerSettings.productName = Guid.NewGuid().ToString();
            }
#endif
        }
    }
}