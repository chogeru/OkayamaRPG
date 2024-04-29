using Effekseer;
using RPGMaker.Codebase.CoreSystem.Helper;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Common
{
    public class OverrideEffectForBattle : MonoBehaviour
    {
        public delegate void CallBackNextEvent();
        public delegate void CallBackSetActive();

        public SpriteRenderer sr = null;
        public Texture texture = null;

        /// <summary>
        /// エフェクト再生用のGameObject
        /// </summary>
        private GameObject _obj;
        /// <summary>
        /// エフェクト再生用のEffekseerPlayer
        /// </summary>
        private EffekseerPlayer _effekseer;

        /// <summary>
        /// Sprite設定処理
        /// </summary>
        /// <param name="name"></param>
        public void SetSprite(string name) {
            if (name.EndsWith(".asset"))
            {
                _obj = new GameObject();
                _effekseer = _obj.gameObject.AddComponent<EffekseerPlayer>();

                var effect = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<EffekseerEffectAsset>(
                    "Assets/RPGMaker/Storage/Animation/Effekseer" + "/" + name);
                _effekseer.SetEffectData(effect);
                _effekseer.Play();
                var instance = Instantiate(_obj,
                    new Vector3(0f, 0f, 1f),
                    Quaternion.identity);
                instance.transform.SetParent(transform);
                instance.transform.localPosition = new Vector3(0f, 0f, 1f);
            }
            else
            {
                var obj = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath(
                    "Assets/RPGMaker/Storage/Animation/Prefab/" + name.Replace(".prefab", "") + ".prefab",
                    typeof(GameObject)) as GameObject;
                _obj = Instantiate(obj,
                    new Vector3(0f, 0f, 1f),
                    Quaternion.identity);
                _obj.transform.SetParent(transform);
                _obj.transform.localPosition = new Vector3(0f, 0f, 1f);
            }
        }

        /// <summary>
        /// 破棄時処理
        /// </summary>
        private void OnDestroy() {
            _effekseer = null;
            if (_obj != null)
                DestroyImmediate(_obj);
        }

        /// <summary>
        /// 再生中かどうか
        /// </summary>
        /// <returns></returns>
        public bool IsPlaying() {
            if (_effekseer != null)
                return _effekseer.IsPlaying();
            return false;
        }
    }
}