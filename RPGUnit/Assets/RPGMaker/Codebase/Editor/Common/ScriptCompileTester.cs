//#define COMPILE_TEST
#if COMPILE_TEST
using System.IO;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.Common
{
    public static class ScriptCompileTester
    {
        /// <summary>
        /// 現在のBuildTargetに対してコンパイルチェックを行う
        /// </summary>
        [MenuItem("Tools/Compile Check/Current Active Target")]
        public static void CompileTestForCurrentBuildTarget() {
            CompileTest(EditorUserBuildSettings.activeBuildTarget, EditorUserBuildSettings.selectedBuildTargetGroup);
        }

        /// <summary>
        /// iOSに対してコンパイルチェックを行う
        /// </summary>
        [MenuItem("Tools/Compile Check/iOS")]
        public static void CompileTestForiOS() {
            CompileTest(BuildTarget.iOS, BuildTargetGroup.iOS);
        }

        /// <summary>
        /// Androidに対してコンパイルチェックを行う
        /// </summary>
        [MenuItem("Tools/Compile Check/Android")]
        public static void CompileTestForAndroid() {
            CompileTest(BuildTarget.Android, BuildTargetGroup.Android);
        }

        /// <summary>
        /// コンパイルチェックを行う
        /// </summary>
        public static void CompileTest(BuildTarget buildTarget, BuildTargetGroup buildTargetGroup) {
            var tempBuildPath = "Temp/CompileTest";

            var option = new ScriptCompilationSettings();
            option.target = buildTarget;
            option.group = buildTargetGroup;
            option.options = ScriptCompilationOptions.None;

            Debug.Log($"Compile Test Running - BuildTarget: {option.target}");

            // NOTE: エラーログはCompilePlayerScripts内で出力される
            var result = PlayerBuildInterface.CompilePlayerScripts(option, tempBuildPath);

            if (result.assemblies != null && result.assemblies.Count != 0 && result.typeDB != null)
            {
                Debug.Log($"Compile Test Success! - BuildTarget: {option.target}");
            }

            // NOTE: tempBuildPathにはコンパイル後のDLLが吐き出されている
            if (Directory.Exists(tempBuildPath))
            {
                Directory.Delete(tempBuildPath, true);
            }
        }
    }
}
#endif