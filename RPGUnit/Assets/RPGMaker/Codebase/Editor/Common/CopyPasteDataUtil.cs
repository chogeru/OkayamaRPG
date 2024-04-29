using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.GraphToolsFoundation.Overdrive;

/// <summary>
/// CopyPasteData の内部構造にアクセスするヘルパー
/// </summary>
namespace RPGMaker.Codebase.Editor.Common
{
    public static class CopyPasteDataUtil
    {
        public static bool IsEmpty() {
            var lastCopiedDataField = typeof(CopyPasteData).
                GetField("s_LastCopiedData", BindingFlags.Static | BindingFlags.NonPublic);
            var lastCopiedData = lastCopiedDataField?.GetValue(null) as CopyPasteData;

            var isEmptyMethod = typeof(CopyPasteData).GetMethod("IsEmpty", BindingFlags.Instance | BindingFlags.NonPublic);

            try
            {
                var result = isEmptyMethod?.Invoke(lastCopiedData, null);
                return result != null && (bool) result;
            }
            catch (Exception)
            {
                return true;
            }
        }

        public static T GetLastCopiedNode<T>() where T : class {
            if (IsEmpty()) return null;
            var lastCopiedDataField = typeof(CopyPasteData).
                GetField("s_LastCopiedData", BindingFlags.Static | BindingFlags.NonPublic);
            var lastCopiedData = lastCopiedDataField?.GetValue(null) as CopyPasteData;

            var nodeField = typeof(CopyPasteData).GetField("nodes", BindingFlags.Instance | BindingFlags.NonPublic);
            var nodeData = nodeField?.GetValue(lastCopiedData) as List<INodeModel>;

            return nodeData?.Single() as T;
        }

        public static void TryRemoveCopiedNode(INodeModel nodeModel) {
            if (IsEmpty()) return;

            var lastCopiedDataField =
                typeof(CopyPasteData).GetField("s_LastCopiedData", BindingFlags.Static | BindingFlags.NonPublic);
            var lastCopiedData = lastCopiedDataField?.GetValue(null) as CopyPasteData;

            var nodeField = typeof(CopyPasteData).GetField("nodes", BindingFlags.Instance | BindingFlags.NonPublic);
            var nodeData = nodeField?.GetValue(lastCopiedData) as List<INodeModel>;

            nodeData?.Remove(nodeModel);
        }
    }
}
