//#define DISPLAY_IN_UNITY_SCENE_WINDOW_TO_CHECK

using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.UiSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using RPGMaker.Codebase.Runtime.Common.Component.Hud;
using RPGMaker.Codebase.Runtime.Common.ControlCharacter;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.View.Preview
{
    /// <summary>
    /// 会話ウィンドウ用のプレビュー
    /// </summary>
    public class TalkWindowPreview : AbstractPreview
    {
        private const bool OUTLINE = false;

        private const int FONT_SELECT_BASE_SIZE   = 42;
        private const int FONT_NUMBER_BASE_SIZE   = 66;

        private const string MenuPrefabBackgroundPath = "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/MenuPreview/Background.prefab";

        // プレビュータイプ
        public enum TalkWindowType
        {
            Character = 0,
            Select    = 1,
            Number    = 2,
            Item      = 3,
            Event     = 4,
            Max,
        }

        private readonly string[] TalkWindowPrefabPath = new string[]
        {
            "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/MessageWindow.prefab",
            "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/MessageInputSelect.prefab",
            "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/MessageInputNum.prefab",
            "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/MessageInputSelectItem.prefab",
            "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/MessageWindow.prefab",
        };

        private GameObject         _talkWindowCanvas;
        private UiSettingDataModel _uiSettingDataModel;
        private Camera             _sceneCamera;
        private TalkWindowType     _talkWindowType;
        private GameObject         _partyCommandWindow;
        private GameObject         _actorCommandWindow;
        private GameObject         _battleStatusWindow;

        private GameObject         _talkPrefab;
        private GameObject         _backgroundObj;

        private List<GameObject> _windowObj;

        private List<int>                     _toggles;
        private CharacterActorDataModel.Image _actorImage = null;
        private string _name = "";
        private string _message = "";
        private int _background;
        private int _position;

        private ControlCharacter _controlCharacterName;
        private ControlCharacter _controlCharacter;

        public TalkWindowPreview() {
        }

        public void SetUiData(UiSettingDataModel uiSettingDataModel) {
            _uiSettingDataModel = uiSettingDataModel;
        }

        public void SetWindowType(TalkWindowType type) {
            _talkWindowType = type;
        }

        /// <summary>
        /// 初期状態のUI設定
        /// </summary>
        public override void InitUi(SceneWindow scene, bool isChange = false) {
            DestroyLocalData();

#if DISPLAY_IN_UNITY_SCENE_WINDOW_TO_CHECK
            Object.DestroyImmediate(GameObject.Find("MessageWindow(Clone)"));
            Object.DestroyImmediate(GameObject.Find("Background(Clone)"));
#endif
            
            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(TalkWindowPrefabPath[(int) _talkWindowType]);
            _talkWindowCanvas = Object.Instantiate(obj);
            _talkWindowCanvas.transform.localPosition = new Vector3(-1000f, 0f, 0f);
            _talkWindowCanvas.transform.localScale = Vector3.one;
            if (_talkWindowCanvas.transform.GetChild(0) != null &&
                _talkWindowCanvas.transform.GetChild(0).GetComponent<Canvas>() != null)
                _talkWindowCanvas.transform.GetChild(0).GetComponent<Canvas>().sortingOrder = 1000;

            // プレビューシーンに移動
#if !DISPLAY_IN_UNITY_SCENE_WINDOW_TO_CHECK
            scene.MoveGameObjectToPreviewScene(_talkWindowCanvas);
#endif
            _sceneCamera = scene.Camera;

            // 会話ウィンドウ表示
            ShowTalkWindow(scene);

            var background = AssetDatabase.LoadAssetAtPath<GameObject>(MenuPrefabBackgroundPath);
            _backgroundObj = Object.Instantiate(background);
            _backgroundObj.transform.localPosition = new Vector3(-10f, 4f, 1f);
            _backgroundObj.transform.localScale = Vector3.one;
#if !DISPLAY_IN_UNITY_SCENE_WINDOW_TO_CHECK
            scene.MoveGameObjectToPreviewScene(_backgroundObj);
#endif
            _backgroundObj.SetActive(true);

            // 初期化
            InitWindow();

            // 設定
            PreviewSetting();

            // 表示更新
            UpdateDisplay();

            //プレビューでは非表示で良い
            obj.SetActive(false);
        }

        // ウィンドウの表示初期設定
        void InitWindow() {
            _windowObj = new List<GameObject>();
            var databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
            //アクターの読み込み
            List<CharacterActorDataModel> characterActorDataModels = databaseManagementService.LoadCharacterActor();

            //アクターがいるか
            bool actorEnabled = characterActorDataModels.FindAll(item => item.charaType == (int) ActorTypeEnum.ACTOR).Count != 0;


            // ウィンドウタイプによって初期化する
            switch (_talkWindowType)
            {
                // キャラクター表示
                case TalkWindowType.Character:
                    //ステータスメニューに表示されるアクターの更新
                    CharacterActorDataModel actor = characterActorDataModels.FindAll(item => item.charaType == (int) ActorTypeEnum.ACTOR)[0];

                    // 名前取得
                    _windowObj.Add(_talkWindowCanvas.transform.Find("Canvas/DisplayArea/Name").gameObject);
                    // 画像取得
                    _windowObj.Add(_talkWindowCanvas.transform.Find("Canvas/DisplayArea/Image/Image").gameObject);
                    _talkWindowCanvas.transform.Find("Canvas/DisplayArea/Image/Image").gameObject.GetComponent<Image>().sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>("Assets/RPGMaker/Storage/Images/Faces/" + actor.image.face + ".png");
                    // アイコン用テキスト取得
                    _windowObj.Add(null);
                    // テキスト取得
                    _windowObj.Add(_talkWindowCanvas.transform.Find("Canvas/DisplayArea/Image/NoIconText").gameObject);
                    // Picture取得
                    _windowObj.Add(_talkWindowCanvas.transform.Find("Canvas/DisplayArea/Picture").gameObject);

                    var picture = _talkWindowCanvas.transform.Find("Canvas/DisplayArea/Picture").gameObject.GetComponent<Image>();
                    var sp = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>("Assets/RPGMaker/Storage/Images/Pictures/" + actor.image.adv + ".png");
                    if (sp != null) {
                        picture.GetComponent<RectTransform>().sizeDelta = new Vector2(sp.texture.width, sp.texture.height);
                        picture.GetComponent<RectTransform>().anchoredPosition = new Vector2(picture.GetComponent<RectTransform>().sizeDelta.x / 2f * -1, picture.GetComponent<RectTransform>().anchoredPosition.y);
                    }
                    picture.sprite = sp;
                    //_talkWindowCanvas.transform.Find("Canvas/DisplayArea/Picture").gameObject.GetComponent<Image>().sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>("Assets/RPGMaker/Storage/Images/Pictures/" + actor.image.adv + ".png");

                    // 所持金ウィンドウ
                    _windowObj.Add(_talkWindowCanvas.transform.Find("Canvas/DisplayArea/GoldWindow/").gameObject);
                    break;

                // 選択肢
                case TalkWindowType.Select:
                    // 選択肢の親を取得
                    _windowObj.Add(_talkWindowCanvas.transform.Find("Margin/BackImage/Frame").gameObject);
                    _talkWindowCanvas.transform.Find("Margin/ArrowUp").gameObject.SetActive(false);
                    _talkWindowCanvas.transform.Find("Margin/ArrowDown").gameObject.SetActive(false);
                    break;

                // 数値入力
                case TalkWindowType.Number:
                    // 背景を取得
                    _windowObj.Add(_talkWindowCanvas.transform.Find("Canvas/Image").gameObject);
                    // フレームを取得
                    _windowObj.Add(_talkWindowCanvas.transform.Find("Canvas/Frame").gameObject);
                    break;

                // アイテム表示
                case TalkWindowType.Item:
                    // フレームを取得
                    _windowObj.Add(_talkWindowCanvas.gameObject);
                    break;
                // イベントのメッセージプレビューに使う
                case TalkWindowType.Event:
                    // 名前取得
                    _windowObj.Add(_talkWindowCanvas.transform.Find("Canvas/DisplayArea/Name").gameObject);
                    // 画像取得
                    _windowObj.Add(_talkWindowCanvas.transform.Find("Canvas/DisplayArea/Image/Image").gameObject);
                    // アイコン用テキスト取得
                    _windowObj.Add(null);
                    // テキスト取得
                    _windowObj.Add(_talkWindowCanvas.transform.Find("Canvas/DisplayArea/Image/NoIconText").gameObject);
                    // Picture取得
                    _windowObj.Add(_talkWindowCanvas.transform.Find("Canvas/DisplayArea/Picture").gameObject);
                    // frame
                    _windowObj.Add(_talkWindowCanvas.transform.Find("Canvas/DisplayArea/Frame").gameObject);
                    // image
                    _windowObj.Add(_talkWindowCanvas.transform.Find("Canvas/DisplayArea/Image").gameObject);
                    // 暗くする用のframe
                    _windowObj.Add(_talkWindowCanvas.transform.Find("Canvas/DisplayArea/FrameDark").gameObject);
                    // 暗くする用の名前部分のframe
                    _windowObj.Add(_talkWindowCanvas.transform.Find("Canvas/DisplayArea/Name/FrameDark").gameObject);
                    // 所持金ウィンドウ
                    _windowObj.Add(_talkWindowCanvas.transform.Find("Canvas/DisplayArea/GoldWindow/").gameObject);
                    break;
            }
        }

        // 表示更新
        void UpdateDisplay() {
            // ウィンドウタイプによって更新する
            switch (_talkWindowType)
            {
                // キャラクター表示
                case TalkWindowType.Character:
                    // 名前設定
                    _windowObj[0].SetActive(_uiSettingDataModel.talkMenu.characterMenu.nameEnabled == 1);
                    // アイコン設定
                    _windowObj[1].SetActive(_uiSettingDataModel.talkMenu.characterMenu.characterEnabled == 0);
                    // 通常テキスト
                    _windowObj[3].SetActive(true);
                    if (_uiSettingDataModel.talkMenu.characterMenu.characterEnabled == 0)
                    {
                        _windowObj[3].GetComponent<RectTransform>().offsetMin = new Vector2(250.0f, 64.0f);
                    }
                    else
                    {
                        _windowObj[3].GetComponent<RectTransform>().offsetMin = new Vector2(64.0f, 64.0f);
                    }
                    // 通常テキスト
                    _windowObj[4].SetActive(_uiSettingDataModel.talkMenu.characterMenu.characterEnabled == 1);

                    //
                    var nameObject = _windowObj[0].transform.Find("NameText").gameObject;
                    // テキスト設定
                    if (nameObject.GetComponentsInChildren<Text>().Length > 0)
                    {
                        foreach (var transform in nameObject.GetComponentsInChildren<Text>())
                        {
                            Object.DestroyImmediate(transform.gameObject);
                        }
                    }
                    var databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
                    //アクターの読み込み
                    List<CharacterActorDataModel> characterActorDataModels = databaseManagementService.LoadCharacterActor();
                
                    //ステータスメニューに表示されるアクターの更新
                    CharacterActorDataModel actor = characterActorDataModels[0];

                    _controlCharacterName = nameObject.gameObject.GetComponent<ControlCharacter>() == null
                        ? nameObject.gameObject.AddComponent<ControlCharacter>()
                        : nameObject.gameObject.GetComponent<ControlCharacter>();

                    var settingName = _uiSettingDataModel.talkMenu.characterMenu.nameFontSetting;
                    _controlCharacterName.InitControl(
                        nameObject,
                        actor.basic.name,
                        settingName.font,
                        settingName.size,
                        new Color(settingName.color[0] / 255f, settingName.color[1] / 255f, settingName.color[2] / 255f),
                        _windowObj[5],
                        isAllSkip: true);
                    _controlCharacterName.ExecEditorByName();
                    
                    // 所持金ウィンドウ
                    _windowObj[5].SetActive(false);

                    // テキスト設定
                    if (_windowObj[3].GetComponentsInChildren<Text>().Length > 0)
                    {
                        foreach (var transform in _windowObj[3].GetComponentsInChildren<Text>())
                        {
                            Object.DestroyImmediate(transform.gameObject);
                        }
                    }
                    //if (_controlCharacter == null)
                    {
                        var setting = _uiSettingDataModel.talkMenu.characterMenu.talkFontSetting;
                        _controlCharacter = _windowObj[3].gameObject.GetComponent<ControlCharacter>() == null
                            ? _windowObj[3].gameObject.AddComponent<ControlCharacter>()
                            : _windowObj[3].gameObject.GetComponent<ControlCharacter>();
                        _controlCharacter.InitControl(
                            _windowObj[3].gameObject,
                            EditorLocalize.LocalizeText("WORD_0092"),
                            setting.font,
                            setting.size,
                            new Color(setting.color[0] / 255f, setting.color[1] / 255f, setting.color[2] / 255f),
                            _windowObj[5],
                            isAllSkip: true);
                    }
                    _controlCharacter.ExecEditor();
                    
                    break;

                // 選択肢
                case TalkWindowType.Select:
                    // 選択肢のリストを取得
                    List<HorizontalLayoutGroup> selectList = new List<HorizontalLayoutGroup>();
                    selectList.Add(_windowObj[0].transform.Find("Mask/Highlight/Ruck_1/Box").GetComponent<HorizontalLayoutGroup>());
                    selectList.Add(_windowObj[0].transform.Find("Mask/Highlight/Ruck_2/Box").GetComponent<HorizontalLayoutGroup>());

                    // 選択肢の設定反映
                    for (int i = 0; i < selectList.Count; i++)
                    {
                        // テキスト取得
                        Text selectText = selectList[i].transform.GetChild(0).GetComponentInChildren<Text>();

                        // テキストカラー設定
                        selectText.color
                            = new Color(_uiSettingDataModel.talkMenu.selectMenu.menuFontSetting.color[0] / 255f,
                                _uiSettingDataModel.talkMenu.selectMenu.menuFontSetting.color[1] / 255f,
                                _uiSettingDataModel.talkMenu.selectMenu.menuFontSetting.color[2] / 255f);


                        // テキストサイズ設定
                        selectText.fontSize = FONT_SELECT_BASE_SIZE *
                            _uiSettingDataModel.talkMenu.selectMenu.menuFontSetting.size / 100;
                        
                        // 1以下だと描画が崩れる
                        if (selectText.fontSize < 2)
                            selectText.fontSize = 2;

                        //取得したTextをピッタリ収まるようにサイズ変更(Heightが長い状態)
                        selectText.rectTransform.sizeDelta =
                            new Vector2(selectText.preferredWidth, selectText.preferredHeight);

                        //再度、ピッタリ収まるようにサイズ変更(Heightもピッタリ合うように)
                        selectText.rectTransform.sizeDelta =
                            new Vector2(selectText.preferredWidth, selectText.preferredHeight);
                        
                        selectList[i].GetComponent<RectTransform>().sizeDelta =
                            new Vector2(selectText.preferredWidth + 60, selectText.preferredHeight);

                        selectList[i].GetComponent<RectTransform>().sizeDelta =
                            new Vector2(selectText.preferredWidth + 60, selectText.preferredHeight);
                    }

                    //補正値、250F
                    var width = selectList[0].GetComponent<RectTransform>().sizeDelta.x - 250f;
                    var rectTransform = _talkWindowCanvas.transform.Find("Margin").GetComponent<RectTransform>();
                    if (width > 0f)
                    {
                        //補正値、*1.5F
                        rectTransform.sizeDelta = new Vector2(1840f - (width * 1.5f), rectTransform.sizeDelta.y);
                    }
                    else
                    {
                        rectTransform.sizeDelta = new Vector2(1840f, rectTransform.sizeDelta.y);
                    }

                    // 即時に反映されない為各更新呼び出し
                    Canvas.ForceUpdateCanvases();
                    _windowObj[0].GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
                    _windowObj[0].GetComponent<VerticalLayoutGroup>().CalculateLayoutInputHorizontal();
                    _windowObj[0].GetComponent<VerticalLayoutGroup>().SetLayoutVertical();
                    _windowObj[0].GetComponent<VerticalLayoutGroup>().SetLayoutHorizontal();
                    break;

                // 数値入力
                case TalkWindowType.Number:
                    // アンカーの設定（中央・右下・右上・左下・左上）
                    Vector2[] anchorParamNumber = new Vector2[]
                    {
                        new Vector2(0.5f, 0.5f),
                        new Vector2(1.0f, 0.0f),
                        new Vector2(1.0f, 1.0f),
                        new Vector2(0.0f, 0.0f),
                        new Vector2(0.0f, 1.0f),
                    };

                    // 表示原点変更（アンカー設定）
                    // 背景
                    _windowObj[0].transform.GetComponent<RectTransform>().anchorMin
                        = anchorParamNumber[_uiSettingDataModel.talkMenu.numberMenu.numberEnabled];
                    _windowObj[0].transform.GetComponent<RectTransform>().anchorMax
                        = anchorParamNumber[_uiSettingDataModel.talkMenu.numberMenu.numberEnabled];
                    _windowObj[0].transform.GetComponent<RectTransform>().pivot
                        = anchorParamNumber[_uiSettingDataModel.talkMenu.numberMenu.numberEnabled];
                    // フレーム
                    _windowObj[1].transform.GetComponent<RectTransform>().anchorMin
                        = anchorParamNumber[_uiSettingDataModel.talkMenu.numberMenu.numberEnabled];
                    _windowObj[1].transform.GetComponent<RectTransform>().anchorMax
                        = anchorParamNumber[_uiSettingDataModel.talkMenu.numberMenu.numberEnabled];
                    _windowObj[1].transform.GetComponent<RectTransform>().pivot
                        = anchorParamNumber[_uiSettingDataModel.talkMenu.numberMenu.numberEnabled];

                    // 位置更新
                    _windowObj[0].transform.GetComponent<RectTransform>().anchoredPosition
                        = new Vector3(
                            _uiSettingDataModel.talkMenu.numberMenu.positionNumberWindow[0],
                            -_uiSettingDataModel.talkMenu.numberMenu.positionNumberWindow[1],
                            0);
                    _windowObj[1].transform.GetComponent<RectTransform>().anchoredPosition
                        = new Vector3(
                            _uiSettingDataModel.talkMenu.numberMenu.positionNumberWindow[0],
                            -_uiSettingDataModel.talkMenu.numberMenu.positionNumberWindow[1],
                            0);

                    // ナンバーを取得
                    List<GameObject> numberObj = new List<GameObject>();
                    for (int i = 0; i < _windowObj[1].transform.GetChild(0).transform.childCount; i++)
                    {
                        numberObj.Add(_windowObj[1].transform.GetChild(0).GetChild(i).gameObject);
                    }

                    for (int i = 0; i < numberObj.Count; i++)
                    {
                        // テキスト取得
                        Text numberText = numberObj[i].GetComponent<Text>();
                        
                        // テキストカラー設定
                        numberText.color
                            = new Color(_uiSettingDataModel.talkMenu.numberMenu.menuFontSetting.color[0] / 255f,
                                _uiSettingDataModel.talkMenu.numberMenu.menuFontSetting.color[1] / 255f,
                                _uiSettingDataModel.talkMenu.numberMenu.menuFontSetting.color[2] / 255f);

                        // テキストサイズ設定
                        numberText.fontSize = FONT_NUMBER_BASE_SIZE *
                            _uiSettingDataModel.talkMenu.numberMenu.menuFontSetting.size / 100;
                        // 1以下だと描画が崩れる
                        if (numberText.fontSize < 2)
                            numberText.fontSize = 2;

                        numberObj[i].GetComponent<Outline>().enabled = OUTLINE;
                    }

                    break;

                // アイテム表示
                case TalkWindowType.Item:
                    // アンカーの設定（中央・右下・右上・左下・左上）
                    Vector2[] anchorParamItem = new Vector2[]
                    {
                        new Vector2(0.5f, 1.0f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.0f),
                    };

                    // 表示原点変更（アンカー設定）
                    // フレーム
                    _windowObj[0].transform.GetComponent<RectTransform>().anchorMin
                        = anchorParamItem[_uiSettingDataModel.talkMenu.itemSelectMenu.positionItemWindow];
                    _windowObj[0].transform.GetComponent<RectTransform>().anchorMax
                        = anchorParamItem[_uiSettingDataModel.talkMenu.itemSelectMenu.positionItemWindow];

                    // 画面外に表示されるため補正
                    _windowObj[0].transform.GetComponent<RectTransform>().anchoredPosition
                        = new Vector2(0, -50 + _uiSettingDataModel.talkMenu.itemSelectMenu.positionItemWindow * 180);

                    // 更新処理
                    _talkWindowCanvas.SetActive(true);
                    
                    // FontSize設定
                    var iFontSetting = _uiSettingDataModel.talkMenu.itemSelectMenu.menuFontSetting;
                    var iUniteFontResize = new FontSize(iFontSetting.size);

                    ItemWindow itemWindow = _windowObj[0].GetComponent<ItemWindow>();
                    // 既に表示しているアイテムがあれば削除
                    itemWindow.ClearItemList();

                    var databaseManagementServiceWork = Editor.Hierarchy.Hierarchy.databaseManagementService;
                    var itemData = databaseManagementServiceWork.LoadItem();
                    ItemDataModel item = null;

                    for (int i = 0; i < itemData.Count; i++)
                    {
                        if (itemData[i].basic.itemType == (int) ItemEnums.ItemType.NORMAL)
                        {
                            item = itemData[i];
                            break;
                        }
                    }

                    if (item == null)
                    {
                        item = ItemDataModel.CreateDefault("dummy");
                        item.basic.name = EditorLocalize.LocalizeText("WORD_0068");
                        item.basic.iconId = "IconSet_176";
                    }
                    var itemShopContent = itemWindow.CreateContent(0, null, item);
                    itemShopContent.SetFontSize(iUniteFontResize.ComponentFontSize);
                    break;
                case TalkWindowType.Event:
                    // 名前設定
                    _windowObj[0].SetActive(_toggles[0] == 1);
                    // アイコン設定
                    _windowObj[1].SetActive(_toggles[1] == 1);
                    // 通常テキスト
                    _windowObj[3].SetActive(true);
                    if (_toggles[1] == 1)
                    {
                        _windowObj[3].GetComponent<RectTransform>().offsetMin = new Vector2(250.0f, 64.0f);
                    }
                    else
                    {
                        _windowObj[3].GetComponent<RectTransform>().offsetMin = new Vector2(64.0f, 64.0f);
                    }
                    // バストアップ
                    _windowObj[4].SetActive(_toggles[2] == 1);

                    // ウィンドウ色
                    switch (_background)
                    {
                        // 通常
                        case 0:
                            _windowObj[7].SetActive(false);
                            _windowObj[8].SetActive(false);
                            break;
                        // 暗い
                        case 1:
                            _windowObj[5].GetComponent<Image>().color = new Color(0, 0, 0, 0);
                            _windowObj[0].transform.Find("Frame").GetComponent<Image>().color = new Color(0, 0, 0, 0);
                            _windowObj[7].SetActive(true);
                            _windowObj[8].SetActive(true);

                            break;
                        // 透明
                        case 2:
                            _windowObj[5].GetComponent<Image>().color = new Color(0, 0, 0, 0);
                            _windowObj[6].GetComponent<Image>().color = new Color(0, 0, 0, 0);
                            _windowObj[0].transform.Find("Frame").GetComponent<Image>().color = new Color(0, 0, 0, 0);
                            _windowObj[0].transform.Find("Image").GetComponent<Image>().color = new Color(0, 0, 0, 0);
                            _windowObj[7].SetActive(false);
                            _windowObj[8].SetActive(false);
                            break;
                    }

                    // ポジション
                    switch (_position)
                    {
                        // 初期設定
                        case -1:
                            //初期のままなので何もしない
                            break;
                        // 上
                        case 0:
                            _windowObj[5].GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 740, 0);
                            _windowObj[6].GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 740, 0);
                            _windowObj[7].GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 740, 0);
                            _windowObj[0].transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(40, 570, 0);
                            break;
                        // 中
                        case 1:
                            _windowObj[5].GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 400, 0);
                            _windowObj[6].GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 400, 0);
                            _windowObj[7].GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 400, 0);
                            _windowObj[0].transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(40, 716, 0);
                            break;
                        //下
                        case 2:
                            break;
                    }

                    // 名前テキスト取得
                    var nameTextObject = _windowObj[0].transform.Find("NameText").gameObject;
                    // テキスト取得
                    var eNormalText = _windowObj[3].gameObject;
                    
                    if (_controlCharacterName == null)
                    {
                        _controlCharacterName = nameTextObject.gameObject.GetComponent<ControlCharacter>() == null
                            ? nameTextObject.gameObject.AddComponent<ControlCharacter>()
                            : nameTextObject.gameObject.GetComponent<ControlCharacter>();
                        var settingNameText = _uiSettingDataModel.talkMenu.characterMenu.nameFontSetting;
                        _controlCharacterName.InitControl(
                            nameTextObject,
                            _name,
                            settingNameText.font,
                            settingNameText.size,
                            new Color(settingNameText.color[0] / 255f, settingNameText.color[1] / 255f,
                                settingNameText.color[2] / 255f),
                            _windowObj[9],
                            isAllSkip: true);
                    }
                    _controlCharacterName.ExecEditorByName();

                    // 所持金ウィンドウ
                    _windowObj[9].SetActive(false);

                    
                    Image face = _windowObj[1].transform.GetComponent<Image>();
                    var path = "Assets/RPGMaker/Storage/Images/Faces/" + _actorImage.face + ".png";
                    Sprite tex = (Sprite) AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    face.sprite = tex;
                    Image picture = _windowObj[4].transform.GetComponent<Image>();
                    path = "Assets/RPGMaker/Storage/Images/Pictures/" + _actorImage.adv + ".png";
                    tex = (Sprite) AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (tex != null) {
                        picture.GetComponent<RectTransform>().sizeDelta = new Vector2(tex.texture.width, tex.texture.height);
                        picture.GetComponent<RectTransform>().anchoredPosition = new Vector2(picture.GetComponent<RectTransform>().sizeDelta.x / 2f * -1, picture.GetComponent<RectTransform>().anchoredPosition.y);
                    }
                    picture.sprite = tex;

                    if (_controlCharacter == null)
                    {
                        var setting = _uiSettingDataModel.talkMenu.characterMenu.talkFontSetting;
                        _controlCharacter = eNormalText.gameObject.GetComponent<ControlCharacter>() == null
                            ? eNormalText.gameObject.AddComponent<ControlCharacter>()
                            : eNormalText.gameObject.GetComponent<ControlCharacter>();
                        _controlCharacter.InitControl(
                            eNormalText.gameObject,
                            _message,
                            setting.font,
                            setting.size,
                            new Color(setting.color[0] / 255f, setting.color[1] / 255f, setting.color[2] / 255f),
                            _windowObj[9],
                            isAllSkip: true);
                    }
                    _controlCharacter.ExecEditor();

                    break;
            }
        }

        public void SetEventData(List<int> toggles, CharacterActorDataModel.Image actor, string name, string message, int background, int position) {
            _toggles = toggles;
            _actorImage = actor;
            _name = name;
            _message = message;
            _background = background;
            _position = position;
        }

        // 初期状態の設定（保存する際に呼び出す）
        void DefaultSetting() {
            if (_talkWindowCanvas.transform.GetChild(0) != null &&
                _talkWindowCanvas.transform.GetChild(0).GetComponent<Canvas>() != null)
                _talkWindowCanvas.transform.GetChild(0).GetComponent<Canvas>().sortingOrder = 0;

            // ウィンドウタイプによって初期化する
            switch (_talkWindowType)
            {
                // キャラクター表示
                case TalkWindowType.Character:
                    _talkWindowCanvas.transform.GetChild(0).GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                    break;

                case TalkWindowType.Event:
                    _talkWindowCanvas.transform.GetChild(0).GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                    break;

                // 選択肢
                case TalkWindowType.Select:
                    _talkWindowCanvas.transform.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

                    // 選択肢のリストを取得
                    List<HorizontalLayoutGroup> selectList = new List<HorizontalLayoutGroup>();
                    selectList.Add(_windowObj[0].transform.Find("Mask/Highlight/Ruck_1").GetComponent<HorizontalLayoutGroup>());
                    selectList.Add(_windowObj[0].transform.Find("Mask/Highlight/Ruck_2").GetComponent<HorizontalLayoutGroup>());
                    // 選択肢の表示切替(二つ非表示にする)
                    for (int i = 0; i < 2; i++)
                        selectList[i].gameObject.SetActive(false);

                    List<Text> texts = new List<Text>();
                    texts.Add(_windowObj[0].transform.Find("Mask/Highlight/Ruck_1/Box/Text").GetComponent<Text>());
                    texts.Add(_windowObj[0].transform.Find("Mask/Highlight/Ruck_2/Box/Text").GetComponent<Text>());
                    //サイズ自動調節をOFFにする
                    for (var i = 0; i < texts.Count; i++)
                    {
                        texts[i].resizeTextForBestFit = false;
                        texts[i].transform.GetComponent<Outline>().enabled = OUTLINE;
                    }

                    break;

                // 数値入力
                case TalkWindowType.Number:
                    _talkWindowCanvas.transform.GetChild(0).GetComponent<Canvas>().renderMode =
                        RenderMode.ScreenSpaceOverlay;
                    break;

                // アイテム表示
                case TalkWindowType.Item:
                    _talkWindowCanvas.transform.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                    break;
            }
        }

        // Preview表示用の設定
        void PreviewSetting() {
            if (_talkWindowCanvas.transform.GetChild(0) != null &&
                _talkWindowCanvas.transform.GetChild(0).GetComponent<Canvas>() != null)
                _talkWindowCanvas.transform.GetChild(0).GetComponent<Canvas>().sortingOrder = 1000;

            // ウィンドウタイプによって初期化する
            switch (_talkWindowType)
            {
                // キャラクター表示
                case TalkWindowType.Character:
                case TalkWindowType.Event:
                    _talkWindowCanvas.transform.GetChild(0).GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
                    _talkWindowCanvas.transform.GetChild(0).GetComponent<Canvas>().worldCamera = _sceneCamera;
                    break;

                // 選択肢
                case TalkWindowType.Select:
                    _talkWindowCanvas.transform.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
                    _talkWindowCanvas.transform.GetComponent<Canvas>().worldCamera = _sceneCamera;

                    // 選択肢のリストを取得
                    List<HorizontalLayoutGroup> selectList = new List<HorizontalLayoutGroup>();
                    selectList.Add(_windowObj[0].transform.Find("Mask/Highlight/Ruck_1").GetComponent<HorizontalLayoutGroup>());
                    selectList.Add(_windowObj[0].transform.Find("Mask/Highlight/Ruck_2").GetComponent<HorizontalLayoutGroup>());
                    // 選択肢の表示切替(とりあえず二つ表示する)
                    for (int i = 0; i < 2; i++)
                    {
                        selectList[i].gameObject.SetActive(true);
                        selectList[i].transform.Find("Box/Text").GetComponent<Text>().text = EditorLocalize.LocalizeText("WORD_2599")+ " " +(i+1);
                    }
                    //少し待ってからActiveにして、サイズ等が反映するようにする
                    _windowObj[0].transform.GetComponent<RectTransform>().localScale = new Vector3(0.0f, 0.0f, 0.0f);
                    MessageInputSelectActive();

                    break;

                // 数値入力
                case TalkWindowType.Number:
                    _talkWindowCanvas.transform.GetChild(0).GetComponent<Canvas>().renderMode =
                        RenderMode.ScreenSpaceCamera;
                    _talkWindowCanvas.transform.GetChild(0).GetComponent<Canvas>().worldCamera = _sceneCamera;
                    break;

                // アイテム表示
                case TalkWindowType.Item:
                    _talkWindowCanvas.transform.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
                    _talkWindowCanvas.transform.GetComponent<Canvas>().worldCamera = _sceneCamera;
                    break;
            }
        }

        private void MessageInputSelectActive() {
            //サイズ調整
            _windowObj[0].transform.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);

            //Textの中で最も長い文字列を取得
            float maxWidth = 0.0f;
            float maxHeight = 0.0f;

            List<Text> _text = new List<Text>();
            _text.Add(_windowObj[0].transform.Find("Mask/Highlight/Ruck_1/Box/Text").GetComponent<Text>());
            _text.Add(_windowObj[0].transform.Find("Mask/Highlight/Ruck_2/Box/Text").GetComponent<Text>());

            for (var i = 0; i < _text.Count; i++)
            {
                float width = _text[i].preferredWidth;
                float height = _text[i].preferredHeight;
                if (width > maxWidth) maxWidth = width;
                if (height > maxHeight) maxHeight = height;
            }

            //Textのサイズを、全ての選択肢の中で、最も長いものに変更する
            //その後Preview用に、サイズ自動調節をONにする
            for (var i = 0; i < _text.Count; i++)
            {
                _text[i].rectTransform.sizeDelta = new Vector2(maxWidth, maxHeight);
                _text[i].resizeTextForBestFit = true;
            }

            //Box（Button）
            List<Button> _buttons = new List<Button>();
            _buttons.Add(_windowObj[0].transform.Find("Mask/Highlight/Ruck_1/Box").GetComponent<Button>());
            _buttons.Add(_windowObj[0].transform.Find("Mask/Highlight/Ruck_2/Box").GetComponent<Button>());

            for (var i = 0; i < _buttons.Count; i++)
            {
                _buttons[i].GetComponent<HorizontalLayoutGroup>().CalculateLayoutInputVertical();
                _buttons[i].GetComponent<HorizontalLayoutGroup>().CalculateLayoutInputHorizontal();
                _buttons[i].GetComponent<HorizontalLayoutGroup>().SetLayoutVertical();
                _buttons[i].GetComponent<HorizontalLayoutGroup>().SetLayoutHorizontal();
                LayoutRebuilder.ForceRebuildLayoutImmediate(_buttons[i].GetComponent<RectTransform>());
            }

            //Ruck
            List<HorizontalLayoutGroup> _ruckList = new List<HorizontalLayoutGroup>();
            _ruckList.Add(_windowObj[0].transform.Find("Mask/Highlight/Ruck_1").GetComponent<HorizontalLayoutGroup>());
            _ruckList.Add(_windowObj[0].transform.Find("Mask/Highlight/Ruck_2").GetComponent<HorizontalLayoutGroup>());

            for (int i = 0; i < _ruckList.Count; i++)
            {
                _ruckList[i].CalculateLayoutInputVertical();
                _ruckList[i].CalculateLayoutInputHorizontal();
                _ruckList[i].SetLayoutVertical();
                _ruckList[i].SetLayoutHorizontal();
                LayoutRebuilder.ForceRebuildLayoutImmediate(_ruckList[i].GetComponent<RectTransform>());
            }

            //Higilight
            _windowObj[0].transform.Find("Mask/Highlight").GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
            _windowObj[0].transform.Find("Mask/Highlight").GetComponent<VerticalLayoutGroup>().CalculateLayoutInputHorizontal();
            _windowObj[0].transform.Find("Mask/Highlight").GetComponent<VerticalLayoutGroup>().SetLayoutVertical();
            _windowObj[0].transform.Find("Mask/Highlight").GetComponent<VerticalLayoutGroup>().SetLayoutHorizontal();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_windowObj[0].transform.Find("Mask/Highlight").GetComponent<RectTransform>());

            //Frame
            _windowObj[0].GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
            _windowObj[0].GetComponent<VerticalLayoutGroup>().CalculateLayoutInputHorizontal();
            _windowObj[0].GetComponent<VerticalLayoutGroup>().SetLayoutVertical();
            _windowObj[0].GetComponent<VerticalLayoutGroup>().SetLayoutHorizontal();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_windowObj[0].GetComponent<RectTransform>());

            // 表示更新
            UpdateDisplay();
        }

        public void Render() {
            _talkWindowCanvas.SetActive(true);

            // 表示更新
            UpdateDisplay();

            // 設定を戻す
            DefaultSetting();

            // マッププレハブ保存
            if (_talkWindowType != TalkWindowType.Event)
            {
                if (_talkWindowCanvas.transform.GetChild(0) != null &&
                    _talkWindowCanvas.transform.GetChild(0).GetComponent<Canvas>() != null)
                {

                    switch (_talkWindowType)
                    {
                        case TalkWindowType.Character:
                        case TalkWindowType.Item:
                            _talkWindowCanvas.transform.GetChild(0).GetComponent<Canvas>().sortingOrder = 50;
                            break;
                        case TalkWindowType.Select:
                        case TalkWindowType.Number:
                            _talkWindowCanvas.transform.GetChild(0).GetComponent<Canvas>().sortingOrder = 60;
                            break;
                        default:
                            break;
                    }
                }
                PrefabUtility.SaveAsPrefabAsset(_talkWindowCanvas, TalkWindowPrefabPath[(int) _talkWindowType]);
            }

            // 再設定
            PreviewSetting();
        }
        
        private void ShowTalkWindow(SceneWindow scene) {
            // 会話ウィンドウ表示
            if (_talkWindowType == TalkWindowType.Number || _talkWindowType == TalkWindowType.Select)
            {
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(TalkWindowPrefabPath[0]);
                _talkPrefab = Object.Instantiate(obj);
                _talkPrefab.transform.localPosition = new Vector3(-1000f, 0f, 0f);
                _talkPrefab.transform.localScale = Vector3.one;

                // プレビューシーンに移動
                scene.MoveGameObjectToPreviewScene(_talkPrefab);

                var databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
                //アクターの読み込み
                List<CharacterActorDataModel> characterActorDataModels = databaseManagementService.LoadCharacterActor();
                
                //ステータスメニューに表示されるアクターの更新
                CharacterActorDataModel actor = characterActorDataModels[0];

                List<GameObject> gameObjects = new List<GameObject>();

                gameObjects.Add(_talkPrefab.transform.Find("Canvas/DisplayArea/Name").gameObject);
                // 画像取得
                gameObjects.Add(_talkPrefab.transform.Find("Canvas/DisplayArea/Image/Image").gameObject);
                // アイコン用テキスト取得
                gameObjects.Add(null);
                // テキスト取得
                gameObjects.Add(_talkPrefab.transform.Find("Canvas/DisplayArea/Image/NoIconText").gameObject);
                // Picture取得
                gameObjects.Add(_talkPrefab.transform.Find("Canvas/DisplayArea/Picture").gameObject);
                //_talkPrefab.transform.Find("Canvas/DisplayArea/Picture").gameObject.GetComponent<Image>().sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>("Assets/RPGMaker/Storage/Images/Pictures/" + actor.image.adv + ".png");
                var picture = _talkPrefab.transform.Find("Canvas/DisplayArea/Picture").gameObject.GetComponent<Image>();
                var sp = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>("Assets/RPGMaker/Storage/Images/Pictures/" + actor.image.adv + ".png");
                if (sp != null) {
                    picture.GetComponent<RectTransform>().sizeDelta = new Vector2(sp.texture.width, sp.texture.height);
                    picture.GetComponent<RectTransform>().anchoredPosition = new Vector2(picture.GetComponent<RectTransform>().sizeDelta.x / 2f * -1, picture.GetComponent<RectTransform>().anchoredPosition.y);
                }
                picture.sprite = sp;
                // 所持金ウィンドウ
                gameObjects.Add(_talkPrefab.transform.Find("Canvas/DisplayArea/GoldWindow/").gameObject);
                _talkPrefab.transform.GetChild(0).GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

                // 名前設定
                gameObjects[0].SetActive(_uiSettingDataModel.talkMenu.characterMenu.nameEnabled == 1);
                // アイコン設定
                gameObjects[1].SetActive(_uiSettingDataModel.talkMenu.characterMenu.characterEnabled == 0);
                // 通常テキスト
                gameObjects[3].SetActive(true);
                if (_uiSettingDataModel.talkMenu.characterMenu.characterEnabled == 0)
                {
                    gameObjects[3].GetComponent<RectTransform>().offsetMin = new Vector2(250.0f, 64.0f);
                }
                else
                {
                    gameObjects[3].GetComponent<RectTransform>().offsetMin = new Vector2(64.0f, 64.0f);
                }
                // 通常テキスト
                gameObjects[4].SetActive(_uiSettingDataModel.talkMenu.characterMenu.characterEnabled == 1);

                // 名前テキスト取得
                var nameTextObject = gameObjects[0].transform.Find("NameText").gameObject;
                
                // テキスト設定
                if (nameTextObject.GetComponentsInChildren<Text>().Length > 0)
                {
                    foreach (var transform in gameObjects[3].GetComponentsInChildren<Text>())
                    {
                        Object.DestroyImmediate(transform.gameObject);
                    }
                }
                if (_controlCharacterName == null)
                {
                    var setting = _uiSettingDataModel.talkMenu.characterMenu.nameFontSetting;
                    _controlCharacterName = nameTextObject.gameObject.GetComponent<ControlCharacter>() == null
                        ? nameTextObject.gameObject.AddComponent<ControlCharacter>()
                        : nameTextObject.gameObject.GetComponent<ControlCharacter>();                    
                    _controlCharacterName.InitControl(
                        nameTextObject,
                        actor.basic.name,
                        setting.font,
                        setting.size,
                        new Color(setting.color[0] / 255f, setting.color[1] / 255f, setting.color[2] / 255f),
                        gameObjects[5],
                        isAllSkip: true);
                }
                _controlCharacterName.ExecEditorByName();

                // 所持金ウィンドウ
                gameObjects[5].SetActive(false);

                // テキスト設定
                if (gameObjects[3].GetComponentsInChildren<Text>().Length > 0)
                {
                    foreach (var transform in gameObjects[3].GetComponentsInChildren<Text>())
                    {
                        Object.DestroyImmediate(transform.gameObject);
                    }
                }
                if (_controlCharacter == null)
                {
                    var setting = _uiSettingDataModel.talkMenu.characterMenu.talkFontSetting;
                    _controlCharacter = gameObjects[3].gameObject.GetComponent<ControlCharacter>() == null
                        ? gameObjects[3].gameObject.AddComponent<ControlCharacter>()
                        : gameObjects[3].gameObject.GetComponent<ControlCharacter>();
                    _controlCharacter.InitControl(
                        gameObjects[3].gameObject,
                        EditorLocalize.LocalizeText("WORD_0092"),
                        setting.font,
                        setting.size,
                        new Color(setting.color[0] / 255f, setting.color[1] / 255f, setting.color[2] / 255f),
                        gameObjects[5],
                        isAllSkip: true);
                }
                _controlCharacter.ExecEditor();

                _talkPrefab.transform.GetChild(0).GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
                _talkPrefab.transform.GetChild(0).GetComponent<Canvas>().worldCamera = _sceneCamera;
            }
        }

        public override void Update() {
        }

        override public void DestroyLocalData() {
            base.DestroyLocalData();

            if (_backgroundObj != null) Object.DestroyImmediate(_backgroundObj);
            if (_talkPrefab != null) Object.DestroyImmediate(_talkPrefab);
            if (_talkWindowCanvas != null) Object.DestroyImmediate(_talkWindowCanvas);
            if (_sceneCamera != null) Object.DestroyImmediate(_sceneCamera);
            if (_partyCommandWindow != null) Object.DestroyImmediate(_partyCommandWindow);
            if (_actorCommandWindow != null) Object.DestroyImmediate(_actorCommandWindow);
            if (_battleStatusWindow != null) Object.DestroyImmediate(_battleStatusWindow);
            if (_controlCharacter != null)
            {
                _controlCharacter.Destroy();
                Object.DestroyImmediate(_controlCharacter);
            }
            _backgroundObj = null;
            _talkPrefab = null;
            _controlCharacter = null;
            _talkWindowCanvas = null;
            _sceneCamera = null;
            _partyCommandWindow = null;
            _actorCommandWindow = null;
            _battleStatusWindow = null;

            for (int i = 0; _windowObj != null && i < _windowObj.Count; i++)
            {
                Object.DestroyImmediate(_windowObj[i]);
            }

            _windowObj = null;
        }
    }
}