using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Common
{
    public class CameraResolutionManager : ResolutionManager
    {
        // 解像度の更新
        protected override void UpdateResolution() {
            var camera = gameObject.transform.GetComponent<Camera>();
            float baseSize = RESOLUTION_HEIGHT / 96f / 2f;

            // 高さが大きい場合
            if ((float)_screenHeight / RESOLUTION_RATIO_HEIGHT > (float) (_screenWidth - _widthDiffLeft - _widthDiffRight) / RESOLUTION_RATIO_WIDTH)
            {
                float baseAspect = (float) RESOLUTION_HEIGHT / RESOLUTION_WIDTH;
                float currentAspect = (float) _screenHeight / (_screenWidth - _widthDiffLeft - _widthDiffRight);
                camera.orthographicSize = baseSize * (currentAspect / baseAspect);
            }
            else
            {
                camera.orthographicSize = baseSize;
            }
        }
    }
}
