using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.UiSetting;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using RPGMaker.Codebase.Runtime.Common.ControlCharacter;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.View.Preview
{
    public class ScrollPreview : AbstractPreview
    {
        private const string ScrollPreviewPrefabPath =
            "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/MessageScroll.prefab";

        private const int                FONT_CHARCTER_BASE_SIZE = 20;
        private       string             _message;
        private       SceneWindow        _sceneView;
        private       UiSettingDataModel _uiSettingDataModel;
        private       int                scrollSpeed;
        private EditorCoroutine _coroutine;

        public void SetUiData(UiSettingDataModel uiSettingDataModel) {
            _uiSettingDataModel = uiSettingDataModel;
        }

        public void SetEventData(string message, int speed) {
            _message = message;
            scrollSpeed = speed;
        }

        /// <summary>
        ///     初期状態のUI設定
        /// </summary>
        public override void InitUi(SceneWindow scene, bool isChange = false) {
            DestroyLocalData();
            _sceneView = scene;
            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(ScrollPreviewPrefabPath);
            _scrollChanvas = Object.Instantiate(obj);
            _scrollChanvas.transform.localScale = Vector3.one * 1f;

            // プレビューシーンに移動
            scene.MoveGameObjectToPreviewScene(_scrollChanvas);
            _scrollChanvas.transform.Find("Canvas").GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
            _scrollChanvas.transform.Find("Canvas").GetComponent<Canvas>().worldCamera = scene.Camera;

            _scrollText = _scrollChanvas.transform.Find("Canvas/Panel/Text").GetComponent<Text>();
            _scrollText.GetComponent<RectTransform>().localPosition = new Vector3(200f,
                _scrollText.GetComponent<RectTransform>().localPosition.y,
                _scrollText.GetComponent<RectTransform>().localPosition.z);
            _scrollText.color
                = new Color(_uiSettingDataModel.talkMenu.characterMenu.talkFontSetting.color[0] / 255f,
                    _uiSettingDataModel.talkMenu.characterMenu.talkFontSetting.color[1] / 255f,
                    _uiSettingDataModel.talkMenu.characterMenu.talkFontSetting.color[2] / 255f);
            
            var fontSize = new FontSize(_uiSettingDataModel.talkMenu.characterMenu.talkFontSetting.size);
            
            _scrollText.fontSize = fontSize.ComponentFontSize;
            if (_scrollText.fontSize < 1)
                _scrollText.fontSize = 1;
            _scrollText.text = _message;
            _coroutine = EditorCoroutineUtility.StartCoroutine(ScrollProcess(), this);
        }

        public override void DestroyLocalData() {
            base.DestroyLocalData();
            if (_scrollChanvas != null) Object.DestroyImmediate(_scrollChanvas);
            if (_scrollText != null) Object.DestroyImmediate(_scrollText);
            if (_coroutine != null) EditorCoroutineUtility.StopCoroutine(_coroutine);
            _scrollChanvas = null;
            _scrollText = null;
            _coroutine = null;
        }
        
        public override void Update() {
        }

        private IEnumerator ScrollProcess() {
            var rect = _scrollText.GetComponent<RectTransform>();

            while (true)
            {
                // 毎フレームループします
                if (rect.sizeDelta.y + Screen.height + 40 >
                    rect.localPosition.y)
                {
                    rect.localPosition += new Vector3(0f, 1f * (scrollSpeed / 10f));
                    _sceneView.Render();
                    yield return null;
                }
                else
                {
                    //スクロールしきったため、文字を消して非表示にする
                    _scrollText.text = "";
                    _sceneView.Render();
                    yield break;
                }
            }
        }
    }
}