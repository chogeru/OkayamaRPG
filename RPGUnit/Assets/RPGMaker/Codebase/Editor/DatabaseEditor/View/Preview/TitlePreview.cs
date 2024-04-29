using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.Editor.Common.Window;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.View.Preview
{
    /// <summary>
    ///     タイトル用のプレビュー
    /// </summary>
    public class TitlePreview : AbstractPreview
    {
        private const string TitlePrefabPath =
            "Assets/RPGMaker/Codebase/Runtime/Title/TitleCanvas.prefab";

        private const string TitleBackgroundPath =
            "Assets/RPGMaker/Storage/Images/Titles1/";

        private const string TitleTitlefrontPath =
            "Assets/RPGMaker/Storage/Images/Titles2/";

        private const string TitleNameImagePath = "Assets/RPGMaker/Storage/Images/Ui/TitleName/";

        private Image      _backGround;
        private Image      _frameBackground;
        private Image      _imageTitleLog;
        private Text       _textContinue;
        private Text       _textNewGame;
        private Text       _textOption;
        private Text       _textTitleLog;

        private GameObject _titleMenu;
        private GameObject _continueObject;
        private GameObject _optionGameObject;


        private GameObject            _titleCanvas;
        private RuntimeTitleDataModel _titleData;

        public TitlePreview(RuntimeTitleDataModel runtimeTitleDataModel) {
            _titleData = runtimeTitleDataModel;
        }

        // データ設定
        public void SetTitleData(RuntimeTitleDataModel runtimeTitleDataModel) {
            _titleData = runtimeTitleDataModel;
        }

        public override void Update() {
        }

        /// <summary>
        ///     初期状態のUI設定
        /// </summary>
        public override void InitUi(SceneWindow sceneWindow, bool isChange = false) {
            DestroyLocalData();

            //ゲーム実行中はプレビューを表示しない
            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(TitlePrefabPath);
            if (EditorApplication.isPlaying)
            {
                obj.SetActive(true);
                return;
            }

            _titleCanvas = Object.Instantiate(obj);
            _titleCanvas.transform.localScale = Vector3.one * 10;
            sceneWindow.MoveGameObjectToPreviewScene(_titleCanvas);
            sceneWindow.SetRenderingSize(sceneWindow.GetRenderingSize().x, sceneWindow.GetRenderingSize().x * 9 / 16);
            _titleCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
            _titleCanvas.GetComponent<Canvas>().worldCamera = sceneWindow.Camera;

            //オプションwindowオブジェクトの取得
            _titleMenu = _titleCanvas.transform.Find("Menu/TitleMenu").gameObject;
            _continueObject = _titleCanvas.transform.Find("SaveLoadWindow").gameObject;
            _continueObject.SetActive(false);
            _optionGameObject = _titleCanvas.transform.Find("OptionMenu").gameObject;
            _optionGameObject.SetActive(false);

            //Previewでは、アニメーションを行わない
            _titleMenu.GetComponent<Animator>().enabled = false;

            //背景取得
            _backGround = _titleCanvas.transform.Find("Background").GetComponent<Image>();
            //フレーム取得
            _frameBackground = _titleCanvas.transform.Find("TitleFront").GetComponent<Image>();

            //タイトル文字の取得
            _imageTitleLog = _titleCanvas.transform.Find("TitleName/Image").GetComponent<Image>();
            _textTitleLog = _titleCanvas.transform.Find("TitleName/Text").GetComponent<Text>();

            //ニューゲーム文字取得
            _textNewGame = _titleCanvas.transform.Find("Menu/TitleMenu/Menus/NewGame/Text").GetComponent<Text>();
            //コンテニュー文字取得
            _textContinue = _titleCanvas.transform.Find("Menu/TitleMenu/Menus/Continue/Text").GetComponent<Text>();
            //オプション文字取得
            _textOption = _titleCanvas.transform.Find("Menu/TitleMenu/Menus/Option/Text").GetComponent<Text>();

            //画像の描写処理

            //タイトルフロントの描写
            _frameBackground.sprite = GetTitleFrontSprite(_titleData);

            //タイトルイメージの描写

            //表示文字の反映
            //ニューゲーム
            _textNewGame.text = _titleData.startMenu.menuNewGame.value;
            //コンティニュー
            _textContinue.text = _titleData.startMenu.menuContinue.value;
            //オプション
            _textOption.text = _titleData.startMenu.menuOption.value;

            //表示非表示の制御
            //ニューゲーム
            _textNewGame.transform.parent.gameObject.SetActive(_titleData.startMenu.menuNewGame.enabled);
            //コンティニュー
            _textContinue.transform.parent.gameObject.SetActive(_titleData.startMenu.menuContinue.enabled);
            //オプション
            _textOption.transform.parent.gameObject.SetActive(_titleData.startMenu.menuOption.enabled);


            //描画サイズの制御
            //タイトルフロント
            var rectTransform = _frameBackground.GetComponent<RectTransform>();
            rectTransform.localScale =
                new Vector3((_titleData.titleFront.scale / 100f), (_titleData.titleFront.scale / 100f), 0);
            //描画位置の制御
            //タイトルフロント
            rectTransform.localPosition =
                new Vector3(_titleData.titleFront.position[0], _titleData.titleFront.position[1], 0);
            //タイトル文字
            rectTransform = _textTitleLog.GetComponent<RectTransform>();
            //場所
            rectTransform.anchoredPosition =
                new Vector3(_titleData.gameTitleCommon.position[0], _titleData.gameTitleCommon.position[1], 0);
            //大きさ
            _textTitleLog.fontSize = _titleData.gameTitleText.size;
            //色
            _textTitleLog.color = new Color(
                _titleData.gameTitleText.color[0] / 255f,
                _titleData.gameTitleText.color[1] / 255f,
                _titleData.gameTitleText.color[2] / 255f
            );
            //ゲームメニューのUI(タイトル部分なのでニューゲーム、コンティニュー、オプティカルの3部分)
            //ココだけz座標の指定ができます
            _titleMenu.transform.localPosition = new Vector3(
                _titleData.startMenu.menuUiSetting.position[0],
                _titleData.startMenu.menuUiSetting.position[1],
                _titleData.startMenu.menuUiSetting.position[2]
            );

            //サイズの制御
            _textNewGame.fontSize = _titleData.startMenu.menuFontSetting.size * 2;
            _textContinue.fontSize = _titleData.startMenu.menuFontSetting.size * 2;
            _textOption.fontSize = _titleData.startMenu.menuFontSetting.size * 2;

            _textNewGame.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            _textContinue.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            _textOption.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            
            //プレビューでは非表示で良い
            obj.SetActive(false);
        }

        public void Render() {
            //ゲーム実行中はプレビューを表示しない
            if (EditorApplication.isPlaying)
            {
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(TitlePrefabPath);
                obj.SetActive(true);
                return;
            }
            if (_titleCanvas == null)
            {
                return;
            }

            _titleCanvas.SetActive(true);

            _backGround.sprite = GetTitleBackgroundSprite(_titleData);

            //タイトルネームの画像設定
            //画像読み込み
            _imageTitleLog.sprite = GetGameTitleSprite(_titleData);
            if (_imageTitleLog.sprite != null)
            {
                _imageTitleLog.transform.GetComponent<RectTransform>().sizeDelta =
                    new Vector2(_imageTitleLog.sprite.rect.width, _imageTitleLog.sprite.rect.height);
                //位置調整
                _imageTitleLog.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(_titleData.gameTitleCommon.position[0],
                    -_titleData.gameTitleCommon.position[1], 0);
                //サイズ調整
                _imageTitleLog.transform.localScale =
                    new Vector3((_titleData.gameTitleImage.scale / 100f), (_titleData.gameTitleImage.scale / 100f), 0);
                //タイトルネーム画像
                _imageTitleLog.gameObject.SetActive(_titleData.gameTitleCommon.gameTitleType == 2);
            }
            else
            {
                _imageTitleLog.gameObject.SetActive(false);
            }

            //タイトルネームのテキスト設定
            _textTitleLog.text = _titleData.gameTitle;
            _textTitleLog.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(_titleData.gameTitleCommon.position[0],
                -_titleData.gameTitleCommon.position[1], 0);
            _textTitleLog.fontSize = _titleData.gameTitleText.size;
            _textTitleLog.color = new Color(
                _titleData.gameTitleText.color[0] / 255f,
                _titleData.gameTitleText.color[1] / 255f,
                _titleData.gameTitleText.color[2] / 255f
            );


            _frameBackground.sprite = GetTitleFrontSprite(_titleData);
            _frameBackground.transform.localPosition = new Vector3(_titleData.titleFront.position[0],
                -_titleData.titleFront.position[1], 0);
            _frameBackground.transform.localScale = new Vector3((_titleData.titleFront.scale /100f),
                (_titleData.titleFront.scale /100f), 0);

            //表示非表示の制御
            //タイトルネームテキスト
            _textTitleLog.gameObject.SetActive(_titleData.gameTitleCommon.gameTitleType == 1);
            //ニューゲーム
            _textNewGame.transform.parent.gameObject.SetActive(_titleData.startMenu.menuNewGame.enabled);
            //コンティニュー
            _textContinue.transform.parent.gameObject.SetActive(_titleData.startMenu.menuContinue.enabled);
            //オプション
            _textOption.transform.parent.gameObject.SetActive(_titleData.startMenu.menuOption.enabled);

            //ニューゲーム
            _textNewGame.text = _titleData.startMenu.menuNewGame.value;
            _textNewGame.fontSize = _titleData.startMenu.menuFontSetting.size * 2;
            _textNewGame.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            //コンティニュー
            _textContinue.text = _titleData.startMenu.menuContinue.value;
            _textContinue.fontSize = _titleData.startMenu.menuFontSetting.size * 2;
            _textContinue.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            //オプション
            _textOption.text = _titleData.startMenu.menuOption.value;
            _textOption.fontSize = _titleData.startMenu.menuFontSetting.size * 2;
            _textOption.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            var menuColor = new Color(_titleData.startMenu.menuFontSetting.color[0] / 255f,
                _titleData.startMenu.menuFontSetting.color[1] / 255f,
                _titleData.startMenu.menuFontSetting.color[2] / 255f
            );
            _textNewGame.color = menuColor;
            _textContinue.color = menuColor;
            _textOption.color = menuColor;

            //メニューウィンドウ
            //場所
            _titleMenu.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(
                _titleData.startMenu.menuUiSetting.position[0],
                -_titleData.startMenu.menuUiSetting.position[1],
                _titleData.startMenu.menuUiSetting.position[2]
            );

            //Previewでは、アニメーションを行わない
            _titleMenu.GetComponent<Animator>().enabled = false;

            //再生時に呼ばれ無い
            if (!MenuWindowParams.instance.IsRpgEditorPlayMode)
            {
                // マッププレハブ保存
                PrefabUtility.SaveAsPrefabAsset(_titleCanvas, TitlePrefabPath);
            }
        }

        public static Sprite GetGameTitleSprite(RuntimeTitleDataModel runtimeTitleDataModel) {
            return AssetDatabase.LoadAssetAtPath<Sprite>(
                TitleNameImagePath + runtimeTitleDataModel.gameTitleImage.image + ".png");
        }

        public static Sprite GetTitleFrontSprite(RuntimeTitleDataModel runtimeTitleDataModel) {
            return AssetDatabase.LoadAssetAtPath<Sprite>(
                TitleTitlefrontPath + runtimeTitleDataModel.titleFront.image + ".png");
        }

        public static Sprite GetTitleBackgroundSprite(RuntimeTitleDataModel runtimeTitleDataModel) {
            return AssetDatabase.LoadAssetAtPath<Sprite>(
                TitleBackgroundPath + runtimeTitleDataModel.titleBackgroundImage + ".png");
        }

        public override void DestroyLocalData() {
            if (_titleCanvas != null) Object.DestroyImmediate(_titleCanvas);
            if (_optionGameObject != null) Object.DestroyImmediate(_optionGameObject);
            if (_backGround != null) Object.DestroyImmediate(_backGround);
            if (_frameBackground != null) Object.DestroyImmediate(_frameBackground);
            if (_imageTitleLog != null) Object.DestroyImmediate(_imageTitleLog);
            if (_textNewGame != null) Object.DestroyImmediate(_textNewGame);
            if (_textContinue != null) Object.DestroyImmediate(_textContinue);
            if (_textOption != null) Object.DestroyImmediate(_textOption);
            _titleCanvas = null;
            _optionGameObject = null;
            _backGround = null;
            _frameBackground = null;
            _imageTitleLog = null;
            _textNewGame = null;
            _textContinue = null;
            _textOption = null;
        }
    }
}