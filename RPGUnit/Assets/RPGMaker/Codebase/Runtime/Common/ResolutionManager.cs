using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Common
{
    public class ResolutionManager : MonoBehaviour
    {
        // 画面比率
        protected readonly int RESOLUTION_RATIO_WIDTH = 16;
        protected readonly int RESOLUTION_RATIO_HEIGHT = 9;
        protected readonly int RESOLUTION_WIDTH = 1920;
        protected readonly int RESOLUTION_HEIGHT = 1080;

        protected float _screenHeight = 0;
        protected float _screenWidth = 0;
        protected float _widthDiffLeft = 0;
        protected float _widthDiffRight = 0;
        protected float _heightDiffTop = 0;
        protected float _heightDiffBottom = 0;

        // Start is called before the first frame update
        void Start()
        {
            UpdateResolution();
        }

        // Update is called once per frame
        void Update()
        {
#if UNITY_IOS || UNITY_ANDROID
            // 解像度変更時に更新
            if (_screenHeight != Screen.height || _screenWidth != Screen.width)
            {
                _screenHeight = (int) Screen.height;
                _screenWidth = (int) Screen.width;
                _widthDiffLeft = (int)Screen.safeArea.xMin;
                _widthDiffRight = (int) (Screen.width - Screen.safeArea.xMax);
                _heightDiffTop = (int) (Screen.height - Screen.safeArea.yMax);
                _heightDiffBottom = (int) Screen.safeArea.yMin;
                UpdateResolution();
            }
#else
            // 解像度変更時に更新
            if (_screenHeight != Screen.height || _screenWidth != Screen.width)
            {
                _screenHeight = Screen.height;
                _screenWidth = Screen.width;
                UpdateResolution();
            }
#endif
        }

        // 解像度の更新
        protected virtual void UpdateResolution() {
        }
    }
}
