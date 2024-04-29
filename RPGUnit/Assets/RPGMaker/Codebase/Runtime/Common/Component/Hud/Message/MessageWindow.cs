using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.UiSetting;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Runtime.Map;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud.Message
{
    /// <summary>
    /// 文章表示イベントのWindow
    /// </summary>
    public class MessageWindow : MonoBehaviour
    {
        // const
        //--------------------------------------------------------------------------------------------------------------
        private const string PrefabPath =
            "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/MessageWindow.prefab";

        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        private DatabaseManagementService _databaseManagementService;
        private Image                     _faceImage;
        private Image                     _nameBg;
        private Image                     _nameFrame;
        private GameObject _nameObject;
        private GameObject _nameTextObject;
        private GameObject                _noImageText;
        private GameObject                _panel;
        private GameObject                _frame;
        private Image                     _panelBg;
        private Image                     _picture;
        private GameObject                _frameDark;
        private GameObject                _nameFrameDark;
        private GameObject                _goldWindow;

        // 表示要素プロパティ
        //--------------------------------------------------------------------------------------------------------------
        private GameObject         _prefab;
        private UiSettingDataModel _uiSettingDataModel;
        private ControlCharacter.ControlCharacter _controlCharacterName;
        private ControlCharacter.ControlCharacter _controlCharacter;

        // initialize methods
        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 初期化
        /// </summary>
        public void Init() {
            var loadPrefab = UnityEditorWrapper.PrefabUtilityWrapper.LoadPrefabContents(PrefabPath);
            _prefab = Instantiate(
                loadPrefab,
                gameObject.transform,
                false
            );
            UnityEditorWrapper.PrefabUtilityWrapper.UnloadPrefabContents(loadPrefab);
            //falseで保存されることがある
            _prefab.SetActive(true);
            //UI設定の取得
            _databaseManagementService = new DatabaseManagementService();
            _uiSettingDataModel = _databaseManagementService.LoadUiSettingDataModel();

            _panel = _prefab.transform.Find("Canvas/DisplayArea/Image").gameObject;
            _frame = _prefab.transform.Find("Canvas/DisplayArea/Frame").gameObject;
            _panelBg = _panel.GetComponent<Image>();
            _noImageText = _prefab.transform.Find("Canvas/DisplayArea/Image/NoIconText").gameObject;
            _faceImage = _prefab.transform.Find("Canvas/DisplayArea/Image/Image").GetComponent<Image>();
            _picture = _prefab.transform.Find("Canvas/DisplayArea/Picture").GetComponent<Image>();
            _nameObject = _prefab.transform.Find("Canvas/DisplayArea/Name").gameObject;
            _nameBg = _nameObject.transform.Find("Image").GetComponent<Image>();
            _nameFrame = _nameObject.transform.Find("Frame").GetComponent<Image>();
            _nameTextObject = _nameObject.transform.Find("NameText").gameObject;
            _frameDark = _prefab.transform.Find("Canvas/DisplayArea/FrameDark").gameObject;
            _nameFrameDark = _prefab.transform.Find("Canvas/DisplayArea/Name/FrameDark").gameObject;
            _goldWindow = _prefab.transform.Find("Canvas/DisplayArea/GoldWindow").gameObject;

            _nameObject.SetActive(false);
            _faceImage.gameObject.SetActive(false);
            _picture.gameObject.SetActive(false);
            _noImageText.gameObject.SetActive(true);
            _frameDark.SetActive(false);
            _nameFrameDark.SetActive(false);
            _goldWindow.SetActive(false);

            _noImageText.GetComponent<RectTransform>().offsetMin = new Vector2(64.0f, 64.0f);
        }

        /// <summary>
        /// 顔グラフィック
        /// </summary>
        /// <param name="iconPath"></param>
        public void ShowFaceIcon(string iconPath) {
            if (string.IsNullOrEmpty(iconPath)) return;
            var imageName = iconPath.Contains(".png") ? iconPath : iconPath + ".png";
            var path = "Assets/RPGMaker/Storage/Images/Faces/" +
                       imageName;
            var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);
            _faceImage.sprite = tex;
            _faceImage.gameObject.SetActive(true);
            _noImageText.gameObject.SetActive(true);

            //顔グラフィックを表示する場合には座標調整する
            _noImageText.GetComponent<RectTransform>().offsetMin = new Vector2(250.0f, 64.0f);
        }

        /// <summary>
        /// 名前表示
        /// </summary>
        /// <param name="actorName"></param>
        public void ShowName(string actorName) {
            _nameObject.SetActive(true);

            var nameFont = _uiSettingDataModel.talkMenu.characterMenu.nameFontSetting;
            
            _controlCharacterName = _nameTextObject.gameObject.GetComponent<ControlCharacter.ControlCharacter>() == null
                ? _nameTextObject.gameObject.AddComponent<ControlCharacter.ControlCharacter>()
                : _nameTextObject.gameObject.GetComponent<ControlCharacter.ControlCharacter>();
            _controlCharacterName.InitControl(
                _nameTextObject.gameObject,
                actorName,
                nameFont.font,
                nameFont.size,
                new Color(nameFont.color[0] / 255f, nameFont.color[1] / 255f, nameFont.color[2] / 255f),
                _goldWindow,
                isAllSkip: false);
            _controlCharacterName.ExecCharacterByName();
        }

        /// <summary>
        /// 立ち絵表示
        /// </summary>
        /// <param name="pictureName"></param>
        public void ShowPicture(string pictureName) {
            if (string.IsNullOrEmpty(pictureName)) return;
            var imageName = pictureName.Contains(".png") ? pictureName : pictureName + ".png";
            var path = "Assets/RPGMaker/Storage/Images/Pictures/" + imageName;
            var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);
            if (tex != null)
            {
                _picture.GetComponent<RectTransform>().sizeDelta = new Vector2(tex.texture.width, tex.texture.height);
                _picture.GetComponent<RectTransform>().anchoredPosition = new Vector2((_picture.GetComponent<RectTransform>().sizeDelta.x / 2f) * -1f, _picture.GetComponent<RectTransform>().anchoredPosition.y);
                _picture.sprite = tex;
                _picture.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 文章表示
        /// </summary>
        /// <param name="message"></param>
        public void ShowMessage(string message) {
            //マップで文章表示を行っている場合は、メニューを非表示にする
            if (GameStateHandler.IsMap())
                MenuManager.MenuBase.MenuHidden(false);
            var setting = DataManager.Self().GetUiSettingDataModel().talkMenu.characterMenu.talkFontSetting;
            var setting2 = _uiSettingDataModel.talkMenu.characterMenu.talkFontSetting;
            _controlCharacter = _noImageText.gameObject.AddComponent<ControlCharacter.ControlCharacter>();
            _controlCharacter.InitControl(
                _noImageText.gameObject,
                message,
                setting2.font,
                setting2.size,
                new Color(setting.color[0] / 255f, setting.color[1] / 255f, setting.color[2] / 255f),
                _goldWindow,
                isAllSkip: false);
            _controlCharacter.ExecCharacter();
        }

        /// <summary>
        /// ウィンドウ色
        /// </summary>
        /// <param name="kind"></param>
        public void SetWindowColor(int kind) {
            switch (kind)
            {
                // 通常
                case 0:
                    break;
                // 暗い
                case 1:
                    _frame.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                    _nameFrame.color = new Color(0, 0, 0, 0);
                    _frameDark.SetActive(true);
                    _nameFrameDark.SetActive(true);
                    break;
                // 透明
                case 2:
                    _panelBg.color = new Color(0, 0, 0, 0);
                    _frame.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                    _nameBg.color = new Color(0, 0, 0, 0);
                    _nameFrame.color = new Color(0, 0, 0, 0);
                    break;
            }
        }

        /// <summary>
        /// ウィンドウの表示位置
        /// </summary>
        /// <param name="kind"></param>
        public void SetWindowPos(int kind) {
            switch (kind)
            {
                // 初期設定
                case -1:
                    //初期のままなので何もしない
                    break;
                // 上
                case 0:
                    _panel.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 740, 0);
                    _frame.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 740, 0);
                    _frameDark.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 740, 0);
                    _nameObject.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(40, 570, 0);
                    break;
                // 中
                case 1:
                    _panel.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 400, 0);
                    _frame.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 400, 0);
                    _frameDark.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 400, 0);
                    _nameObject.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(40, 716, 0);
                    break;
                //下
                case 2:
                    break;
            }
        }

        public void Next() {
            _controlCharacter.ExecCharacter();
        }

        public bool NextMessage() {
            return true;
        }

        public void Destroy() {
            if (GameStateHandler.IsMap())
                MenuManager.MenuBase.MenuHidden(true);

            if (_controlCharacter != null)
            {
                _controlCharacter.Destroy();
                _controlCharacter = null;
            }
            if (_prefab != null)
            {
                Destroy(_prefab);
                _prefab = null;
            }
        }

        public bool IsWait() {
            return _controlCharacter.IsWaitForButtonInput;
        }

        public bool IsEnd() {
            return _controlCharacter.IsEnd;
        }
        public bool IsNotWaitInput() {
            return _controlCharacter.IsNotWaitInput;
        }
        public void SetIsNotWaitInput(bool flg) {
            _controlCharacter.SetIsNotWaitCommand(flg);
        }
    }
}