using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace RPGMaker.Codebase.Editor.Common
{
    internal static class GraphicsSettingsHelper
    {
        private static readonly string kPreloadShaderPath = 
        "Assets/RPGMaker/Codebase/Runtime/Map/Component/Character/ShaderVariants.shadervariants";

        private static readonly string kAlwaysIncludedShaderName = 
            "UI/DefaultETC1";
        
        internal static void ConfigureDefaultSettings()
        {
            var settings = GraphicsSettings.GetGraphicsSettings();
            var serializedObject = new SerializedObject(GraphicsSettings.GetGraphicsSettings());

            var b1 = SetAlwaysIncludedShaders(serializedObject);
            var b2 = SetPreloadedShaders(serializedObject);
            if (b1 || b2)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private static bool SetAlwaysIncludedShaders(SerializedObject serializedObject) 
        {
            var shader = Shader.Find(kAlwaysIncludedShaderName);
            if (shader == null)
            {
                return false;
            }

            var alwaysIncludedShaders = serializedObject.FindProperty("m_AlwaysIncludedShaders");
            
            //Find if shader is already included
            for (int i = 0; i < alwaysIncludedShaders.arraySize; ++i)
            {
                if (alwaysIncludedShaders.GetArrayElementAtIndex(i).objectReferenceValue == shader)
                {
                    return false;
                }
            }
            alwaysIncludedShaders.InsertArrayElementAtIndex(alwaysIncludedShaders.arraySize);
            alwaysIncludedShaders.GetArrayElementAtIndex(alwaysIncludedShaders.arraySize - 1).objectReferenceValue = shader;

            return true;
        }
        
        private static bool SetPreloadedShaders(SerializedObject serializedObject)
        {
            var collection = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(kPreloadShaderPath);

            if (collection == null)
            {
                return false;
            }
            
            var preloadedShaders = serializedObject.FindProperty("m_PreloadedShaders");

            //Find if SVC is already included
            for (int i = 0; i < preloadedShaders.arraySize; ++i)
            {
                if (preloadedShaders.GetArrayElementAtIndex(i).objectReferenceValue == collection)
                {
                    return false;
                }
            }
            
            preloadedShaders.InsertArrayElementAtIndex(preloadedShaders.arraySize);
            preloadedShaders.GetArrayElementAtIndex(preloadedShaders.arraySize - 1).objectReferenceValue = collection;

            return true;
        }
        
    }
}