using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace RPGMaker.Codebase.Runtime.Common
{
    public class TforuUtility : SingletonMonoBehaviour<TforuUtility>
    {
        private void Start() {
            if (gameObject.transform.parent != null &&
                gameObject.transform.parent.GetComponent<Canvas>() != null)
                gameObject.AddComponent<RectTransform>();
        }

        public void Base64Encode(int saveId, string json) {
            var bytesToEncode = Encoding.UTF8.GetBytes(json);
            var encodeText = Convert.ToBase64String(bytesToEncode);


            File.WriteAllText(_GetLocalPath() + _GetSaveId(saveId), encodeText);
        }

        public string Base64Decode(int saveId) {
            var saveTxt = "";

            var fi = new FileInfo(_GetLocalPath() + _GetSaveId(saveId));

            using (var sr = new StreamReader(fi.OpenRead(), Encoding.UTF8))
            {
                saveTxt = sr.ReadToEnd();
            }


            var decodedBytes = Convert.FromBase64String(saveTxt);
            var decodedText = Encoding.UTF8.GetString(decodedBytes);

            return decodedText;
        }


        private string _GetLocalPath() {
            return "Assets/Tkool/Editor/SaveData/";
        }

        private string _GetSaveId(int saveId) {
            if (saveId < 0)
                return "config.rpgsave";
            if (saveId == 0)
                return "global.rpgsave";
            return "file" + saveId + ".rpgsave";
        }

        public static double MathRandom() {
            return new Random().Next(0, 100) / 100f;
        }
        
        public static float ScrollToCore(ScrollRect scrollRect, GameObject go, float align)
        {
            var targetRect = go.transform.GetComponent<RectTransform>();
            var contentHeight = scrollRect.content.rect.height;
            var viewportHeight = scrollRect.viewport.rect.height;

            if (contentHeight < viewportHeight) return 0f;

            var targetPos = contentHeight + GetPosY(targetRect) + targetRect.rect.height * align;
            var gap = viewportHeight * align;
            var normalizedPos = (targetPos - gap) / (contentHeight - viewportHeight);

            normalizedPos = Mathf.Clamp01(normalizedPos);
            scrollRect.verticalNormalizedPosition = normalizedPos;
            return normalizedPos;
        }

        public static float GetPosY(RectTransform transform)
        {
            return transform.localPosition.y + transform.rect.y;
        }
    }
}