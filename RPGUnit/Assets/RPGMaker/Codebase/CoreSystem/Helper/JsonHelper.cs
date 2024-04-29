using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper
{
    public static class JsonHelper
    {
        public static T Clone<T>(T obj) {
            return FromJson<T>(ToJson(obj));
        }

        public static T FromJson<T>(string json) {
            // JsonUtility.FromJsonを使えるようにするためだけのwrapper
            return JsonUtility.FromJson<T>(json);
        }

        public static string ToJson(object obj, bool prettyPrint = false) {
            // JsonUtility.ToJsonを使えるようにするためだけのwrapper
            return JsonUtility.ToJson(obj, prettyPrint);
        }

        public static List<T> FromJsonArray<T>(string json) {
            var dummyJson = $"{{\"{DummyNode<T>.RootName}\": {json}}}";
            var obj = JsonUtility.FromJson<DummyNode<T>>(dummyJson);
            return obj.array?.ToList() ?? new List<T>();
        }

        public static string ToJsonArray<T>(IEnumerable<T> collection) {
            var json = JsonUtility.ToJson(new DummyNode<T>(collection));
            var start = DummyNode<T>.RootName.Length + 4;
            var len = json.Length - start - 1;
            return json.Substring(start, len);
        }

        [Serializable]
        private struct DummyNode<T>
        {
            public const string RootName = nameof(array);
            public       T[]    array;

            public DummyNode(IEnumerable<T> collection) {
                array = collection.ToArray();
            }
        }
    }
}