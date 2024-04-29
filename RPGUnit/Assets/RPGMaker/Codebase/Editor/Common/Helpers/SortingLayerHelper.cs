using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.Common
{
    internal static class SortingLayerHelper
    {
        internal static void ConfigureDefaultSettings()
        {
            // This script is intended to run on Editor startup only.
            // Do nothing when entering playmode
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
            
            var serializedObject =
                new SerializedObject(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
            var sortingLayers = serializedObject.FindProperty("m_SortingLayers");
            var layerNamesList = new List<string>();

            for (int i = 0; i < sortingLayers.arraySize; ++i)
            {
                layerNamesList.Add(sortingLayers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue);
            }
            
            var sortingLayerIndices = (CoreSystem.Helper.UnityUtil.SortingLayerManager.SortingLayerIndex[])
                System.Enum.GetValues(typeof(CoreSystem.Helper.UnityUtil.SortingLayerManager.SortingLayerIndex));
            
            foreach (var layerIndex in sortingLayerIndices)
            {
                var name = layerIndex.ToString();
                FixSortingLayer(name, (int)layerIndex);
            }
        }

        /**
         * Note: expected to add sorting layer from lower index.
         */
        private static void FixSortingLayer(string layerName, int layerIndex)
        {
            var serializedObject =
                new SerializedObject(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
            var sortingLayers = serializedObject.FindProperty("m_SortingLayers");

            int oldIndex = -1;
            for (int i = 0; i < sortingLayers.arraySize; ++i)
            {
                if (sortingLayers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue
                    .Equals(layerName))
                {
                    // uniqueID が正しく設定されていない場合、設定しなおす
                    var oldLayer = sortingLayers.GetArrayElementAtIndex(i);
                    if (oldLayer.FindPropertyRelative("uniqueID").intValue != Mathf.Abs(layerName.GetHashCode()))
                    {
                        oldLayer.FindPropertyRelative("uniqueID").intValue = Mathf.Abs(layerName.GetHashCode());
                        serializedObject.ApplyModifiedProperties();
                    }
                    if (i == layerIndex)
                    {
                        return; // already set
                    }

                    oldIndex = i;
                    break;
                }
            }

            // the layer already exists in different index
            if (oldIndex != -1)
            {
                sortingLayers.MoveArrayElement(oldIndex, layerIndex);
            }

            // there is already existing layer in target layer index
            else if (layerIndex < sortingLayers.arraySize)
            {
                sortingLayers.InsertArrayElementAtIndex(layerIndex);
                var newLayer = sortingLayers.GetArrayElementAtIndex(layerIndex);
                newLayer.FindPropertyRelative("name").stringValue = layerName;
                newLayer.FindPropertyRelative("uniqueID").intValue = Mathf.Abs(layerName.GetHashCode());
            }
            else
            {
                // just add new one
                sortingLayers.InsertArrayElementAtIndex(sortingLayers.arraySize);
                var newLayer = sortingLayers.GetArrayElementAtIndex(sortingLayers.arraySize - 1);
                newLayer.FindPropertyRelative("name").stringValue = layerName;
                newLayer.FindPropertyRelative("uniqueID").intValue = Mathf.Abs(layerName.GetHashCode());
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
