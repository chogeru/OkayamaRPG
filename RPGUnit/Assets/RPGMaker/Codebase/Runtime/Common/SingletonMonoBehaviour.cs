using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Common
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var t = typeof(T);

                    _instance = (T) FindObjectOfType(t);
                    if (_instance == null)
                    {
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake() {
            CheckInstance();
        }

        protected bool CheckInstance() {
            if (_instance == null)
            {
                _instance = this as T;
                return true;
            }

            if (Instance == this)
            {
                return true;
            }

            Destroy(this);
            return false;
        }
    }
}