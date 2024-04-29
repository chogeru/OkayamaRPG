using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RPGMaker.Codebase.CoreSystem.Helper
{
    public class UnityEditorWrapper
    {
        // AssetDatabase
        public class AssetDatabaseWrapper
        {
            public static void Refresh() {
#if UNITY_EDITOR
                // AssetDatabase.Refresh();
#endif
            }

            public static void Refresh2() {
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }

            public static string LoadJsonString(string path) {
#if UNITY_EDITOR
                var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var sr = new StreamReader(fs, Encoding.GetEncoding("UTF-8"));
                return sr.ReadToEnd();
#else
                return AddressableManager.Load.LoadAssetSync<TextAsset>(path)?.text;
#endif
            }

            public static void CreateAsset(Object asset, string path) {
#if UNITY_EDITOR
                AssetDatabase.CreateAsset(asset, path);
#endif
            }

            public static Object LoadAssetAtPath(string assetPath, Type type) {
#if UNITY_EDITOR
                return AssetDatabase.LoadAssetAtPath(assetPath, type);
#else
                switch (type.Name)
                {
                    case "TextAsset":
                        return AddressableManager.Load.LoadAssetSync<TextAsset>(assetPath);
                    case "Sprite":
                        return AddressableManager.Load.LoadAssetSync<Sprite>(assetPath);
                    case "Texture":
                        return AddressableManager.Load.LoadAssetSync<Texture>(assetPath);
                }
                return null;
#endif
            }

            public static T LoadAssetAtPath<T>(string assetPath) where T : Object {
#if UNITY_EDITOR
                return AssetDatabase.LoadAssetAtPath<T>(assetPath);
#else
                return AddressableManager.Load.LoadAssetSync<T>(assetPath);
#endif
            }

            public static string[] FindAssets(string filter, string[] searchInFolders) {
#if UNITY_EDITOR
                return AssetDatabase.FindAssets(filter, searchInFolders);
#else
                return new string[0];
#endif
            }

            public static string GUIDToAssetPath(string guid) {
#if UNITY_EDITOR
                return AssetDatabase.GUIDToAssetPath(guid);
#else
                return "";
#endif
            }

            public static void SaveAssets() {
#if UNITY_EDITOR
                AssetDatabase.SaveAssets();
#endif
            }

            public static string CreateFolder(string parentFolder, string newFolderName) {
#if UNITY_EDITOR
                return AssetDatabase.CreateFolder(parentFolder, newFolderName);
#else
                return "";
#endif
            }
        }

        // PrefabUtility
        public class PrefabUtilityWrapper
        {
            public static GameObject LoadPrefabContents(string assetPath) {
#if UNITY_EDITOR
                if (!File.Exists(assetPath))
                {
                    var oldAssetPath = assetPath;
                    assetPath = assetPath.Replace("SavedMaps", "SampleMaps");
                    if (assetPath != oldAssetPath)
                    {
                        DebugUtil.LogWarning(
                            $"ロードするプレハブファイルが存在しなかったので、ディレクトリを変更したパスでロードします" +
                            $" (\"{oldAssetPath}\" → \"{assetPath}\")。");
                    }
                }

                return PrefabUtility.LoadPrefabContents(assetPath);
#else
                return AddressableManager.Load.LoadAssetSync<GameObject>(assetPath);
#endif
            }

            public static void UnloadPrefabContents(GameObject prefab) {
#if UNITY_EDITOR
                PrefabUtility.UnloadPrefabContents(prefab);
#endif
            }

            public static GameObject SaveAsPrefabAsset(GameObject instanceRoot, string assetPath) {
#if UNITY_EDITOR
                return PrefabUtility.SaveAsPrefabAsset(instanceRoot, assetPath);
#else
                return null;
#endif
            }

            public static void RemovePrefabAsset(string assetPath) {
#if UNITY_EDITOR
                AssetDatabase.DeleteAsset(assetPath);
#else
#endif
            }
        }

        // FileUtil
        public class FileUtilWrapper
        {
            public static void CopyFileOrDirectory(string source, string dest) {
#if UNITY_EDITOR
                FileUtil.CopyFileOrDirectory(source, dest);
#endif
            }

            public static bool DeleteFileOrDirectory(string path) {
#if UNITY_EDITOR
                return FileUtil.DeleteFileOrDirectory(path);
#else
                return false;
#endif
            }
        }
    }
}