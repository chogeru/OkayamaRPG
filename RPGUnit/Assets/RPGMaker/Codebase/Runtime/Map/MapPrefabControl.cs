using RPGMaker.Codebase.Runtime.Common;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Map
{
    /// <summary>
    /// マップのインスタンス化を確認するだけのクラス。
    /// </summary>
    public class MapPrefabControl : MonoBehaviour
    {
        public bool IsInstantiate => _isInstantiate;
        private bool _isInstantiate = false;
        
        void Awake() {
            _isInstantiate = false;
        }

        void Start() {
            TimeHandler.Instance.AddTimeActionFrame(1, SetActiveObj, false);
        }

        void SetActiveObj() {
            _isInstantiate = true;
        }
    }
}