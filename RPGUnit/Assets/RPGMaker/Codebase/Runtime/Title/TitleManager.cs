using RPGMaker.Codebase.Runtime.Common;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RPGMaker.Codebase.Runtime.Title
{
    public class TitleManager : WindowBase
    {
        [SerializeField] private TitleController _titleController;

        protected void Start() {
#if !UNITY_EDITOR
#if !UNITY_IOS && !UNITY_ANDROID && !UNITY_SWITCH

            Vector2 displaySize = (Vector2) DataManager.Self().GetSystemDataModel().GetDisplaySize();
            Screen.SetResolution((int)displaySize.x, (int)displaySize.y, false);
#endif
            _titleController.gameObject.SetActive(true);
#else
            if (EditorApplication.isPlaying)
            {
                _titleController.gameObject.SetActive(true);
            }
#endif
        }
    }
}