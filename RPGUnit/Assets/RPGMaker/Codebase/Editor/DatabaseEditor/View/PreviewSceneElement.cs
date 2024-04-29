using RPGMaker.Codebase.CoreSystem.Helper;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.View
{
    /// <summary>
    /// プレビューシーンを持ったVisualElement。
    /// </summary>
    public class PreviewSceneElement : VisualElement
    {
        private readonly Scene _previewScene;
        private RenderTexture _renderTexture;
        private Image _renderTextureCanvasImage;
        private readonly List<GameObject> _gameObjects = new List<GameObject>();

        public PreviewSceneElement() {
            DebugUtil.LogMethod("NewPreviewScene()");
            _previewScene = EditorSceneManager.NewPreviewScene();

            // Deactivate unused cameras
            var oldCameras = _previewScene
                .GetRootGameObjects()
                .SelectMany(x => x.GetComponentsInChildren<Camera>());
            foreach (var oldCamera in oldCameras) oldCamera.enabled = false;
            var cameraGo = new GameObject("Preview Scene Camera", typeof(Camera));
            cameraGo.transform.position = new Vector3(0, 0, -150);
            Camera = cameraGo.GetComponent<Camera>();
            Camera.cameraType = CameraType.SceneView;
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            Camera.orthographic = true;
            Camera.forceIntoRenderTexture = false;
            Camera.scene = _previewScene;
            Camera.enabled = false;

            _renderTextureCanvasImage = new Image();
            Add(_renderTextureCanvasImage);
            MoveGameObjectToPreviewScene(cameraGo);
        }

        public Camera Camera { get; }
        public Vector2Int RenderTextureSize { get; set; } = new Vector2Int(1024, 1024);

        ~PreviewSceneElement() {
            Dispose();
        }

        /// <summary>
        /// ゲームオブジェクトを現在属しているシーンからプレビューシーンに移動させる。
        /// </summary>
        /// <param name="go">ゲームオブジェクト</param>
        public void MoveGameObjectToPreviewScene(GameObject go) {
            if (!_previewScene.isLoaded) return;

            if (_gameObjects.Contains(go)) return;

            SceneManager.MoveGameObjectToScene(go, _previewScene);
            _gameObjects.Add(go);
        }

        public void Render() {
            if (!_previewScene.isLoaded) return;

            if (!_renderTexture || _renderTexture.width != RenderTextureSize.x ||
                _renderTexture.height != RenderTextureSize.y)
            {
                if (_renderTexture)
                {
                    Object.DestroyImmediate(_renderTexture);
                    _renderTexture = null;
                }

                var format = Camera.allowHDR ? GraphicsFormat.R16G16B16A16_SFloat : GraphicsFormat.R8G8B8A8_UNorm;
                _renderTexture = new RenderTexture(RenderTextureSize.x, RenderTextureSize.y, 32, format);
            }

            Camera.aspect = _renderTexture.width / (float) _renderTexture.height;
            Camera.targetTexture = _renderTexture;
            Camera.Render();
            Camera.targetTexture = null;

            _renderTextureCanvasImage.image = _renderTexture;
        }

        public void Dispose() {
            if (Camera != null) Camera.targetTexture = null;

            if (_renderTexture != null)
            {
                Object.DestroyImmediate(_renderTexture);
                _renderTexture = null;
            }

            foreach (var go in _gameObjects) Object.DestroyImmediate(go);

            _gameObjects.Clear();

            DebugUtil.LogMethod("ClosePreviewScene()");
            EditorSceneManager.ClosePreviewScene(_previewScene);
        }
    }
}