using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCustom;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.UiSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.ControlCharacter;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.View.Preview
{
    /// <summary>
    /// タイトル用のプレビュー
    /// </summary>
    public class MenuPreview : AbstractPreview
    {
        // メニュー表示定義
        public enum DISPLAY_TYPE
        {
            MAIN_MENU = 0,
            ITEM,
            SKILL,
            EQUIP,
            STATUS,
            SORT,
            OPTION,
            SAVE,
            END,
            MAX,
        }

        // メニュー設定更新定義
        public enum UI_CHANGE_TYPE
        {
            NONE = 0,
            PATTERN,
            CHARACTER,
            FONT,
            FONT_SIZE,
            FONT_COLOR,
            BG_COLOR,
            BG_IMAGE,
            WINDOW_COLOR,
            WINDOW_IMAGE,
            FRAME_COLOR,
            FRAME_IMAGE,
            FRAME_HIGHLIGHT,
            BUTTON_COLOR,
            BUTTON_IMAGE,
            BUTTON_HIGHLIGHT,
            BUTTON_FRAME_COLOR,
            BUTTON_FRAME_IMAGE,
        }

        private DatabaseManagementService _databaseManagementService = null;
        private SystemSettingDataModel    _systemSettingDataModel;

        private const string MenuPrefabPath = "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/MenuWindow";
        private const string MenuPrefabBackgroundPath = "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/MenuPreview/Background.prefab";
        private DISPLAY_TYPE _displayType;

        private GameObject         _menuCanvas;
        private GameObject         _mainWindow;
        private GameObject         _menuIcon;
        private UiSettingDataModel _uiSettingDataModel;
        
        //プレビューに表示するアクター用
        private List<CharacterActorDataModel> _characterActorDataModels;
        private List<ClassDataModel> _classDataModels;

        //メニューの背景に表示するマップ
        private GameObject _backgroundObj = null;
        // メインメニュー内の項目
        private GameObject _menuItemObj   = null;
        private GameObject _menuSkillObj  = null;
        private GameObject _menuEquipObj  = null;
        private GameObject _menuStatusObj = null;
        private GameObject _menuSortObj   = null;
        private GameObject _menuOptionObj = null;
        private GameObject _menuSaveObj   = null;
        private GameObject _menuEndObj    = null;

        // メインメニュー内の文言
        private GameObject _menuItemText   = null;
        private GameObject _menuSkillText  = null;
        private GameObject _menuEquipText  = null;
        private GameObject _menuStatusText = null;
        private GameObject _menuSortText   = null;
        private GameObject _menuOptionText = null;
        private GameObject _menuSaveText   = null;
        private GameObject _menuEndText    = null;
        private GameObject _menuGoldText   = null;
        private GameObject _mainActorLv    = null;
        private GameObject _mainActorHp    = null;
        private GameObject _mainActorMp    = null;
        private GameObject _mainActorTp    = null;

        // 各メニューのオブジェクト
        private GameObject _itemObj = null;
        private GameObject _skillObj      = null;
        private GameObject _equipObj      = null;
        private GameObject _statusObj     = null;
        private GameObject _sortObj       = null;
        private GameObject _optionObj     = null;
        private GameObject _saveObj       = null;
        private GameObject _endObj        = null;
        
        //スキルウィンドウの文言
        private GameObject _skillStatusLv     = null;
        private GameObject _skillContent      = null;
        private GameObject _skillBase         = null;
        private GameObject _skillStatusHP = null;
        private GameObject _skillStatusMP = null;
        private GameObject _skillStatusTP = null;

        
        //装備ウィンドウの文言
        private GameObject _equipMenuItem1               = null;
        private GameObject _equipMenuItem2               = null;
        private GameObject _equipMenuItem3               = null;
        private GameObject _equipStatusValueAttack       = null;
        private GameObject _equipStatusValueDefense      = null;
        private GameObject _equipStatusValueMagic        = null;
        private GameObject _equipStatusValueMagicDefense = null;
        private GameObject _equipStatusValueAgility      = null;
        private GameObject _equipStatusValueLuck         = null;
        private GameObject _equipEquipValueWeapon        = null;
        private GameObject _equipEquipValueShield        = null;
        private GameObject _equipEquipValueHead          = null;
        private GameObject _equipEquipValueBody          = null;
        private GameObject _equipEquipValueAccessory     = null;
        
        //ステータスウィンドウの文言
        private GameObject _statusStatusLevel                  = null;
        private GameObject _statusStatusHP                     = null;
        private GameObject _statusStatusMP                     = null;
        private GameObject _statusStatusTP                     = null;
        private GameObject _statusExp                          = null;
        private GameObject _statusNextExp                      = null;
        private GameObject _statusParamStatusValueAttack       = null;
        private GameObject _statusParamStatusValueDefense      = null;
        private GameObject _statusParamStatusValueMagic        = null;
        private GameObject _statusParamStatusValueMagicDefense = null;
        private GameObject _statusParamStatusValueAgility      = null;
        private GameObject _statusParamStatusValueLuck         = null;
        private GameObject _statusParamEquipWeapon             = null;
        private GameObject _statusParamEquipShield             = null;
        private GameObject _statusParamEquipHead               = null;
        private GameObject _statusParamEquipBody               = null;
        private GameObject _statusParamEquipAccessory          = null;
        
        //並び替えのウィンドウの文言
        private GameObject _sortActorLv = null;
        private GameObject _sortActorHp = null;
        private GameObject _sortActorMp = null;
        private GameObject _sortActorTp = null;
        
        //オプションウィンドウの文言
        private GameObject _optionAlwaysDashText      = null;
        private GameObject _optionCommandRememberText = null;
        private GameObject _optionVolumeBGMText       = null;
        private GameObject _optionVolumeBGSText       = null;
        private GameObject _optionVolumeMEText        = null;
        private GameObject _optionVolumeSEText        = null;
        
        //セーブウィンドウの文言
        private GameObject _saveTitleText = null;

        //エンドウィンドウの文言
        private GameObject _endGameEndText = null;
        private GameObject _endBackText    = null;

        //アイテムウィンドウのオブジェクト
        private GameObject _itemItemObj = null;
        private GameObject _itemWeaponObj = null;
        private GameObject _itemArmorObj = null;
        private GameObject _itemImportantObj = null;
        private GameObject _itemContent = null;
        private GameObject _itemBase = null;
        private List<GameObject> _items = null;

        //アイテムウィンドウの文言
        private GameObject _itemItem1Text = null;
        private GameObject _itemItem2Text = null;
        private GameObject _itemItem3Text = null;
        private GameObject _itemItem4Text = null;

        private string MenuPrefabFilePath
        {
            // パターンIDを加算（新UI）
            get { return MenuPrefabPath + (int.Parse(_systemSettingDataModel.uiPatternId) + 1).ToString("00") + ".prefab"; }
        }

        public MenuPreview() {
        }

        public void SetUiData(UiSettingDataModel uiSettingDataModel) {
            _uiSettingDataModel = uiSettingDataModel;
        }

        public void SetDisplayType(int displayType) {
            _displayType = (DISPLAY_TYPE) displayType;
        }

        /// <summary>
        /// 初期状態のUI設定
        /// </summary>
        public void InitUi(SceneWindow sceneWindow, UI_CHANGE_TYPE uiSetting = UI_CHANGE_TYPE.NONE) {
            DestroyLocalData();

            _databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
            _systemSettingDataModel = _databaseManagementService.LoadSystem();
            //アクターの読み込み
            _characterActorDataModels = _databaseManagementService.LoadCharacterActor();
            _classDataModels = _databaseManagementService.LoadClassCommon();
            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(MenuPrefabFilePath);

            _menuCanvas = Object.Instantiate(obj);
            _menuCanvas.transform.localPosition = new Vector3(-1000f, 0f, 0f);
            _menuCanvas.transform.localScale = Vector3.one;
            
            //アクターがいるか
            bool actorEnabled = _characterActorDataModels.FindAll(item => item.charaType == (int) ActorTypeEnum.ACTOR).Count != 0;
            //アクターが何人いるか
            int actorCount =
                _characterActorDataModels.FindAll(item => item.charaType == (int) ActorTypeEnum.ACTOR).Count <= 4
                    ? _characterActorDataModels.FindAll(item => item.charaType == (int) ActorTypeEnum.ACTOR).Count
                    : 4;
            
            // プレビューシーンに移動
            sceneWindow?.MoveGameObjectToPreviewScene(_menuCanvas);

            if (_menuCanvas.transform.Find("UICanvas").GetComponent<GraphicRaycaster>() != null)
            {
                _menuCanvas.transform.Find("UICanvas").GetComponent<GraphicRaycaster>().enabled = false;
            }
            
            _menuCanvas.transform.Find("UICanvas").GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
            _menuCanvas.transform.Find("UICanvas").GetComponent<Canvas>().worldCamera = sceneWindow?.Camera;

            _mainWindow = _menuCanvas.transform.Find("UICanvas/MainWindow").gameObject;

            //メインメニューの項目
            _menuItemObj   = _mainWindow.transform.Find("MenuArea/MenuWindow/MenuItems/Item").gameObject;
            _menuSkillObj  = _mainWindow.transform.Find("MenuArea/MenuWindow/MenuItems/Skill").gameObject;
            _menuEquipObj  = _mainWindow.transform.Find("MenuArea/MenuWindow/MenuItems/Equipment").gameObject;
            _menuStatusObj = _mainWindow.transform.Find("MenuArea/MenuWindow/MenuItems/Status").gameObject;
            _menuSortObj   = _mainWindow.transform.Find("MenuArea/MenuWindow/MenuItems/Sort").gameObject;
            _menuOptionObj = _mainWindow.transform.Find("MenuArea/MenuWindow/MenuItems/Option").gameObject;
            _menuSaveObj   = _mainWindow.transform.Find("MenuArea/MenuWindow/MenuItems/Save").gameObject;
            _menuEndObj    = _mainWindow.transform.Find("MenuArea/MenuWindow/MenuItems/End").gameObject;

            //メインメニュー文言の取得
            _menuItemText = _menuItemObj.transform.Find("Text").gameObject;
            _menuSkillText = _menuSkillObj.transform.Find("Text").gameObject;
            _menuEquipText = _menuEquipObj.transform.Find("Text").gameObject;
            _menuStatusText = _menuStatusObj.transform.Find("Text").gameObject;
            _menuSortText = _menuSortObj.transform.Find("Text").gameObject;
            _menuOptionText = _menuOptionObj.transform.Find("Text").gameObject;
            _menuSaveText = _menuSaveObj.transform.Find("Text").gameObject;
            _menuEndText = _menuEndObj.transform.Find("Text").gameObject;
            _menuGoldText = _mainWindow.transform.Find("MenuArea/GoldWindow/MenuItems/Currency").gameObject;
            

            //メインメニューの項目の表示切り替え
            _menuItemObj.SetActive(_uiSettingDataModel.gameMenu.menuItem.enabled == 1);
            _menuSkillObj.SetActive(_uiSettingDataModel.gameMenu.menuSkill.enabled == 1);
            _menuEquipObj.SetActive(_uiSettingDataModel.gameMenu.menuEquipment.enabled == 1);
            _menuStatusObj.SetActive(_uiSettingDataModel.gameMenu.menuStatus.enabled == 1);
            _menuSortObj.SetActive(_uiSettingDataModel.gameMenu.menuSort.enabled == 1);
            _menuOptionObj.SetActive(_uiSettingDataModel.gameMenu.menuOption.enabled == 1);
            _menuSaveObj.SetActive(_uiSettingDataModel.gameMenu.menuSave.enabled == 1);
            _menuEndObj.SetActive(_uiSettingDataModel.gameMenu.menuGameEnd.enabled == 1);
            
            //メインメニューの文言の切り替え
            _menuItemText.GetComponent<TextMeshProUGUI>().text = TextManager.item;
            _menuSkillText.GetComponent<TextMeshProUGUI>().text = TextManager.skill;
            _menuEquipText.GetComponent<TextMeshProUGUI>().text = TextManager.equip;
            _menuStatusText.GetComponent<TextMeshProUGUI>().text = TextManager.status;
            _menuSortText.GetComponent<TextMeshProUGUI>().text = TextManager.formation;
            _menuOptionText.GetComponent<TextMeshProUGUI>().text = TextManager.options;
            _menuSaveText.GetComponent<TextMeshProUGUI>().text = TextManager.save;
            _menuEndText.GetComponent<TextMeshProUGUI>().text = TextManager.gameEnd;
            _menuGoldText.GetComponent<Text>().text = TextManager.money;

            //メインメニューのアクター数分回す
            for (int i = 1; i <= 4; i++)
            {
                if (i <= actorCount)
                {
                    _mainWindow.transform.Find("MenuArea/PartyWindow/PartyItems/Actor" + i).gameObject.SetActive(true);
                    //メインメニューの文言取得
                    _mainActorLv = _mainWindow.transform.Find("MenuArea/PartyWindow/PartyItems/Actor" + i + "/Level").gameObject;
                    _mainActorHp = _mainWindow.transform.Find("MenuArea/PartyWindow/PartyItems/Actor" + i + "/Hp/HpName").gameObject;
                    _mainActorMp = _mainWindow.transform.Find("MenuArea/PartyWindow/PartyItems/Actor" + i + "/Mp/MpName").gameObject;
                    _mainActorTp = _mainWindow.transform.Find("MenuArea/PartyWindow/PartyItems/Actor" + i + "/Tp/TpName").gameObject;
                    //メインメニューの文言切り替え
                    _mainActorLv.GetComponent<Text>().text = TextManager.levelA;
                    _mainActorHp.GetComponent<Text>().text = TextManager.hpA;
                    _mainActorMp.GetComponent<Text>().text = TextManager.mpA;
                    _mainActorTp.GetComponent<Text>().text = TextManager.tpA;
                    
                    //アクター情報の表示
                    CharacterActorDataModel actor = _characterActorDataModels.FindAll(item => item.charaType == (int) ActorTypeEnum.ACTOR)[i - 1];
                    _mainWindow.transform.Find("MenuArea/PartyWindow/PartyItems/Actor" + i + "/Name").gameObject.GetComponent<TextMeshProUGUI>().text = actor.basic.name;
                    foreach (var classDataModel in _classDataModels)
                        if (classDataModel.id == actor.basic.classId)
                        {
                            _mainWindow.transform.Find("MenuArea/PartyWindow/PartyItems/Actor" + i + "/Class").gameObject.GetComponent<TextMeshProUGUI>().text = classDataModel.basic.name;
                            break;
                        }

                    //顔アイコン、立ち絵、SDキャラクターの設定値に応じて、表示するものを変更する
                    Image face = null;
                    Image body = null;
                    if (_mainWindow.transform.Find("MenuArea/PartyWindow/PartyItems/Actor" + i + "/Face") != null)
                        face = _mainWindow.transform.Find("MenuArea/PartyWindow/PartyItems/Actor" + i + "/Face").GetComponent<Image>();
                    if (_mainWindow.transform.Find("MenuArea/PartyWindow/PartyItems/Actor" + i + "/body") != null)
                        body = _mainWindow.transform.Find("MenuArea/PartyWindow/PartyItems/Actor" + i + "/body").GetComponent<Image>();

                    //双方の部品が存在する場合は、顔アイコン,SDキャラと立ち絵画像で、表示する部品を変更する
                    if (face != null && body != null)
                    {
                        if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.FACE)
                        {
                            //顔アイコン
                            var sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                                "Assets/RPGMaker/Storage/Images/Faces/" +
                                actor.image.face + ".png");
                            face.enabled = true;
                            face.sprite = sprite;
                            face.material = null;
                            face.color = Color.white;
                            face.preserveAspect = true;

                            face.transform.localScale = Vector3.one;

                            face.gameObject.SetActive(true);
                            body.gameObject.SetActive(false);
                        }
                        else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.SD)
                        {
                            //SDキャラ
                            var assetId = DataManager.Self().GetActorDataModel(actor.uuId).image.character;
                            CharacterGraphic characterGraphic = new GameObject().AddComponent<CharacterGraphic>();
                            characterGraphic.Init(assetId);

                            face.preserveAspect = false;
                            face.enabled = true;
                            face.sprite = characterGraphic.GetCurrentSprite();
                            face.color = Color.white;
                            face.material = characterGraphic.GetMaterial();
                            face.transform.localScale = characterGraphic.GetSize();

                            if (face.transform.localScale.x > 1.0f || face.transform.localScale.y > 1.0f)
                            {
                                if (face.transform.localScale.y > 1.0f)
                                {
                                    face.transform.localScale = new Vector2(face.transform.localScale.x / face.transform.localScale.y, 1.0f);
                                }
                                else
                                {
                                    face.transform.localScale = new Vector2(1.0f, face.transform.localScale.y / face.transform.localScale.x);
                                }
                            }

                            face.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                            Object.DestroyImmediate(characterGraphic.gameObject);

                            face.gameObject.SetActive(true);
                            body.gameObject.SetActive(false);
                        }
                        else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.PICTURE)
                        {
                            //立ち絵
                            var imageName = actor.image.adv.Contains(".png")
                                ? actor.image.adv
                                : actor.image.adv + ".png";
                            var path = "Assets/RPGMaker/Storage/Images/Pictures/" + imageName;
                            var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);

                            body.enabled = true;
                            body.sprite = tex;
                            body.color = Color.white;
                            body.preserveAspect = true;

                            body.transform.localScale = Vector3.one;

                            face.gameObject.SetActive(false);
                            body.gameObject.SetActive(true);
                        }
                        else
                        {
                            face.gameObject.SetActive(false);
                            body.gameObject.SetActive(false);
                        }
                    }
                    //片方の部品しかない場合は、いずれのパターンでもその部品に描画する
                    else if (face != null)
                    {
                        if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.FACE)
                        {
                            //顔アイコン
                            var sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                                "Assets/RPGMaker/Storage/Images/Faces/" +
                                actor.image.face + ".png");
                            face.enabled = true;
                            face.sprite = sprite;
                            face.color = Color.white;
                            face.preserveAspect = true;
                            face.transform.localScale = Vector3.one;
                        }
                        else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.SD)
                        {
                            //SDキャラ
                            var assetId = DataManager.Self().GetActorDataModel(actor.uuId).image.character;
                            CharacterGraphic characterGraphic = new GameObject().AddComponent<CharacterGraphic>();
                            characterGraphic.Init(assetId);

                            face.enabled = true;
                            face.preserveAspect = false;
                            face.sprite = characterGraphic.GetCurrentSprite();
                            face.color = Color.white;
                            face.material = characterGraphic.GetMaterial();
                            face.transform.localScale = characterGraphic.GetSize();

                            if (face.transform.localScale.x > 1.0f || face.transform.localScale.y > 1.0f)
                            {
                                if (face.transform.localScale.y > 1.0f)
                                {
                                    face.transform.localScale = new Vector2(face.transform.localScale.x / face.transform.localScale.y, 1.0f);
                                }
                                else
                                {
                                    face.transform.localScale = new Vector2(1.0f, face.transform.localScale.y / face.transform.localScale.x);
                                }
                            }

                            face.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                            Object.DestroyImmediate(characterGraphic.gameObject);
                        }
                        else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.PICTURE)
                        {
                            //立ち絵
                            var imageName = actor.image.adv.Contains(".png")
                                ? actor.image.adv
                                : actor.image.adv + ".png";
                            var path = "Assets/RPGMaker/Storage/Images/Pictures/" + imageName;
                            var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);

                            face.enabled = true;
                            face.sprite = tex;
                            face.color = Color.white;
                            face.preserveAspect = true;
                            face.transform.localScale = Vector3.one;
                        }
                        else
                            face.enabled = false;
                    }
                    else if (body != null)
                    {
                        if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.FACE)
                        {
                            //顔アイコン
                            var sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                                "Assets/RPGMaker/Storage/Images/Faces/" +
                                actor.image.face + ".png");
                            body.enabled = true;
                            body.sprite = sprite;
                            body.color = Color.white;
                            body.preserveAspect = true;
                            body.transform.localScale = Vector3.one;
                        }
                        else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.SD)
                        {
                            //SDキャラ
                            var assetId = DataManager.Self().GetActorDataModel(actor.uuId).image.character;
                            CharacterGraphic characterGraphic = new GameObject().AddComponent<CharacterGraphic>();
                            characterGraphic.Init(assetId);

                            body.enabled = true;
                            body.sprite = characterGraphic.GetCurrentSprite();
                            body.color = Color.white;
                            body.material = characterGraphic.GetMaterial();
                            body.transform.localScale = characterGraphic.GetSize();

                            if (body.transform.localScale.x > 1.0f || body.transform.localScale.y > 1.0f)
                            {
                                if (body.transform.localScale.y > 1.0f)
                                {
                                    body.transform.localScale = new Vector2(body.transform.localScale.x / body.transform.localScale.y, 1.0f);
                                }
                                else
                                {
                                    body.transform.localScale = new Vector2(1.0f, body.transform.localScale.y / body.transform.localScale.x);
                                }
                            }

                            body.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                            Object.DestroyImmediate(characterGraphic.gameObject);
                        }
                        else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.PICTURE)
                        {
                            //立ち絵
                            var imageName = actor.image.adv.Contains(".png")
                                ? actor.image.adv
                                : actor.image.adv + ".png";
                            var path = "Assets/RPGMaker/Storage/Images/Pictures/" + imageName;
                            var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);

                            body.enabled = true;
                            body.sprite = tex;
                            body.color = Color.white;
                            body.preserveAspect = true;
                            body.transform.localScale = Vector3.one;
                        }
                        else
                            body.enabled = false;
                    }
                }
                else
                {
                    //表示させるアクターがいなかった場合に非表示にする
                    _mainWindow.transform.Find("MenuArea/PartyWindow/PartyItems/Actor" + i).gameObject.SetActive(false);
                }
            }

            //アイテムウィンドウ
            _itemObj = _menuCanvas.transform.Find("UICanvas/ItemWindow").gameObject;

            //アイテムウィンドウの項目の取得
            _itemItemObj = _itemObj.transform.Find("MenuArea/Menu/Menus/Item1").gameObject;
            _itemWeaponObj = _itemObj.transform.Find("MenuArea/Menu/Menus/Item2").gameObject;
            _itemArmorObj = _itemObj.transform.Find("MenuArea/Menu/Menus/Item3").gameObject;
            _itemImportantObj = _itemObj.transform.Find("MenuArea/Menu/Menus/Item4").gameObject;
            
            //アイテムウィンドウの文言の取得
            _itemItem1Text = _itemItemObj.transform.Find("Name").gameObject;
            _itemItem2Text = _itemWeaponObj.transform.Find("Name").gameObject;
            _itemItem3Text = _itemArmorObj.transform.Find("Name").gameObject;
            _itemItem4Text = _itemImportantObj.transform.Find("Name").gameObject;
            
            //アイテムウィンドウの項目の表示切替
            _itemItemObj.SetActive(_uiSettingDataModel.gameMenu.categoryItem.enabled == 1);
            _itemWeaponObj.SetActive(_uiSettingDataModel.gameMenu.categoryWeapon.enabled == 1);
            _itemArmorObj.SetActive(_uiSettingDataModel.gameMenu.categoryArmor.enabled == 1);
            _itemImportantObj.SetActive(_uiSettingDataModel.gameMenu.categoryImportant.enabled == 1);
            
            //アイテムウィンドウの文言切り替え
            _itemItem1Text.GetComponent<TextMeshProUGUI>().text　= TextManager.item;
            _itemItem2Text.GetComponent<TextMeshProUGUI>().text　= TextManager.weapon;
            _itemItem3Text.GetComponent<TextMeshProUGUI>().text　= TextManager.armor;
            _itemItem4Text.GetComponent<TextMeshProUGUI>().text　= TextManager.keyItem;
            
            InitItem(false);

            //スキルウィンドウの項目取得
            _skillObj = _menuCanvas.transform.Find("UICanvas/SkillWindow").gameObject;
            _equipObj = _menuCanvas.transform.Find("UICanvas/EquipWindow").gameObject;
            _statusObj = _menuCanvas.transform.Find("UICanvas/StatusWindow").gameObject;
            _sortObj = _menuCanvas.transform.Find("UICanvas/SortWindow").gameObject;
            _optionObj = _menuCanvas.transform.Find("UICanvas/OptionMenu").gameObject;
            _saveObj = _menuCanvas.transform.Find("UICanvas/SaveWindow").gameObject;
            _endObj = _menuCanvas.transform.Find("UICanvas/EndWindow").gameObject;

            _skillStatusHP = _skillObj.transform.Find("MenuArea/Status/Hp/HpName").gameObject;
            _skillStatusMP = _skillObj.transform.Find("MenuArea/Status/Mp/MpName").gameObject;
            _skillStatusTP = _skillObj.transform.Find("MenuArea/Status/Tp/TpName").gameObject;
            
            _skillStatusHP.GetComponent<Text>().text = TextManager.hpA;
            _skillStatusMP.GetComponent<Text>().text = TextManager.mpA;
            _skillStatusTP.GetComponent<Text>().text = TextManager.tpA;
            
            //スキルウィンドウの文言取得
            _skillStatusLv = _skillObj.transform.Find("MenuArea/Status/Level").gameObject;
            //スキルウィンドウの文言切り替え
            _skillStatusLv.GetComponent<Text>().text = TextManager.levelA;
            //スキルに表示されるアクターの更新
            if (actorEnabled)
            {
                CharacterActorDataModel actor = _characterActorDataModels.FindAll(item => item.charaType == (int) ActorTypeEnum.ACTOR)[0];
                _skillObj.transform.Find("MenuArea/Status/Name").gameObject.GetComponent<TextMeshProUGUI>().text = actor.basic.name;
                foreach (var classDataModel in _classDataModels)
                    if (classDataModel.id == actor.basic.classId)
                    {
                        _skillObj.transform.Find("MenuArea/Status/Class").gameObject.GetComponent<TextMeshProUGUI>()
                            .text = classDataModel.basic.name;
                        break;
                    }

                if (_skillObj.transform.Find("MenuArea/Status/Face") != null)
                {
                    _skillObj.transform.Find("MenuArea/Status/Face").gameObject.GetComponent<Image>().sprite =
                        UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                            "Assets/RPGMaker/Storage/Images/Faces/" + actor.image.face + ".png");
                }
                else
                {
                    _skillObj.transform.Find("MenuArea/Status/body").gameObject.GetComponent<Image>().sprite =
                        UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                            "Assets/RPGMaker/Storage/Images/Pictures/" + actor.image.adv + ".png");
                }
            }

            //装備ウィンドウの文言取得
                //上のメニュー
            _equipMenuItem1 = _equipObj.transform.Find("MenuArea/Menus/Item1/Name").gameObject;
            _equipMenuItem2 = _equipObj.transform.Find("MenuArea/Menus/Item2/Name").gameObject;
            _equipMenuItem3 = _equipObj.transform.Find("MenuArea/Menus/Item3/Name").gameObject;
                //ステータス
            _equipStatusValueAttack = _equipObj.transform.Find("MenuArea/StatusWindow/StatusValues/Attack").gameObject;
            _equipStatusValueDefense = _equipObj.transform.Find("MenuArea/StatusWindow/StatusValues/Defense").gameObject;
            _equipStatusValueMagic = _equipObj.transform.Find("MenuArea/StatusWindow/StatusValues/Magic").gameObject;
            _equipStatusValueMagicDefense = _equipObj.transform.Find("MenuArea/StatusWindow/StatusValues/MagicDefense").gameObject;
            _equipStatusValueAgility = _equipObj.transform.Find("MenuArea/StatusWindow/StatusValues/Agility").gameObject;
            _equipStatusValueLuck = _equipObj.transform.Find("MenuArea/StatusWindow/StatusValues/Luck").gameObject;
                //装備項目
            _equipEquipValueWeapon = _equipObj.transform.Find("MenuArea/Equip/EquipTypeScrollView/Viewport/EquipValues/Weapon/Type").gameObject;
            _equipEquipValueShield = _equipObj.transform.Find("MenuArea/Equip/EquipTypeScrollView/Viewport/EquipValues/Shield/Type").gameObject;
            _equipEquipValueHead = _equipObj.transform.Find("MenuArea/Equip/EquipTypeScrollView/Viewport/EquipValues/Head/Type").gameObject;
            _equipEquipValueBody = _equipObj.transform.Find("MenuArea/Equip/EquipTypeScrollView/Viewport/EquipValues/Body/Type").gameObject;
            _equipEquipValueAccessory = _equipObj.transform.Find("MenuArea/Equip/EquipTypeScrollView/Viewport/EquipValues/Accessory/Type").gameObject;

            //装備ウィンドウの文言切り替え
                //上のメニュー
            _equipMenuItem1.GetComponent<TextMeshProUGUI>().text = TextManager.equipment2;
            _equipMenuItem2.GetComponent<TextMeshProUGUI>().text = TextManager.optimize;
            _equipMenuItem3.GetComponent<TextMeshProUGUI>().text = TextManager.clear;
                //ステータス
            _equipStatusValueAttack.GetComponent<Text>().text = TextManager.stAttack;
            _equipStatusValueDefense.GetComponent<Text>().text = TextManager.stGuard;
            _equipStatusValueMagic.GetComponent<Text>().text = TextManager.stMagic;
            _equipStatusValueMagicDefense.GetComponent<Text>().text = TextManager.stMagicGuard;
            _equipStatusValueAgility.GetComponent<Text>().text = TextManager.stSpeed;
            _equipStatusValueLuck.GetComponent<Text>().text = TextManager.stLuck;
            //装備項目
            _equipEquipValueWeapon.GetComponent<Text>().text = _systemSettingDataModel.equipTypes[0].name;
            _equipEquipValueShield.GetComponent<Text>().text = _systemSettingDataModel.equipTypes[1].name;
            _equipEquipValueHead.GetComponent<Text>().text = _systemSettingDataModel.equipTypes[2].name;
            _equipEquipValueBody.GetComponent<Text>().text = _systemSettingDataModel.equipTypes[3].name;
            if(_systemSettingDataModel.equipTypes.Count > 4)
                _equipEquipValueAccessory.GetComponent<Text>().text = _systemSettingDataModel.equipTypes[4].name;
            //装備メニューに表示されるアクターの更新
            if (actorEnabled)
            {
                CharacterActorDataModel actor = _characterActorDataModels.FindAll(item => item.charaType == (int) ActorTypeEnum.ACTOR)[0];
                _equipObj.transform.Find("MenuArea/StatusWindow/Name").gameObject.GetComponent<TextMeshProUGUI>().text = actor.basic.name;
                if (_equipObj.transform.Find("MenuArea/StatusWindow/Face") != null)
                {
                    _equipObj.transform.Find("MenuArea/StatusWindow/Face").gameObject.GetComponent<Image>().sprite =
                        UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                            "Assets/RPGMaker/Storage/Images/Faces/" + actor.image.face + ".png");
                }
                else
                {
                    _equipObj.transform.Find("MenuArea/StatusWindow/body/Image").gameObject.GetComponent<Image>().sprite =
                        UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                            "Assets/RPGMaker/Storage/Images/Pictures/" + actor.image.adv + ".png");
                    
                }
            }

                
            //ステータスウィンドウの文言取得
                //上のメニュー
            _statusStatusLevel = _statusObj.transform.Find("MenuArea/StatusWindow/Status/Level").gameObject;
            _statusStatusHP = _statusObj.transform.Find("MenuArea/StatusWindow/Status/Hp/HpName").gameObject;
            _statusStatusMP = _statusObj.transform.Find("MenuArea/StatusWindow/Status/Mp/MpName").gameObject;
            _statusStatusTP = _statusObj.transform.Find("MenuArea/StatusWindow/Status/Tp/TpName").gameObject;
            _statusExp = _statusObj.transform.Find("MenuArea/StatusWindow/Exp").gameObject;
            _statusNextExp = _statusObj.transform.Find("MenuArea/StatusWindow/NextExp").gameObject;
                //パラメーターウィンドウ
            _statusParamStatusValueAttack = _statusObj.transform.Find("MenuArea/ParamWindow/MiniStatus/StatusValues/Attack").gameObject;
            _statusParamStatusValueDefense = _statusObj.transform.Find("MenuArea/ParamWindow/MiniStatus/StatusValues/Defense").gameObject;
            _statusParamStatusValueMagic = _statusObj.transform.Find("MenuArea/ParamWindow/MiniStatus/StatusValues/Magic").gameObject;
            _statusParamStatusValueMagicDefense = _statusObj.transform.Find("MenuArea/ParamWindow/MiniStatus/StatusValues/MagicDefense").gameObject;
            _statusParamStatusValueAgility = _statusObj.transform.Find("MenuArea/ParamWindow/MiniStatus/StatusValues/Agility").gameObject;
            _statusParamStatusValueLuck = _statusObj.transform.Find("MenuArea/ParamWindow/MiniStatus/StatusValues/Luck").gameObject;
                //パラメーター装備
            _statusParamEquipWeapon = _statusObj.transform.Find("MenuArea/ParamWindow/Equip/Scroll View/Viewport/EquipValues/Weapon/Type").gameObject;
            _statusParamEquipShield = _statusObj.transform.Find("MenuArea/ParamWindow/Equip/Scroll View/Viewport/EquipValues/Shield/Type").gameObject;
            _statusParamEquipHead = _statusObj.transform.Find("MenuArea/ParamWindow/Equip/Scroll View/Viewport/EquipValues/Head/Type").gameObject;
            _statusParamEquipBody = _statusObj.transform.Find("MenuArea/ParamWindow/Equip/Scroll View/Viewport/EquipValues/Body/Type").gameObject;
            _statusParamEquipAccessory = _statusObj.transform.Find("MenuArea/ParamWindow/Equip/Scroll View/Viewport/EquipValues/Accessory/Type").gameObject;
            
            //ステータスウィンドウの文言切り替え
                //上のメニュー
            _statusStatusLevel.GetComponent<Text>().text = TextManager.levelA;
            _statusStatusHP.GetComponent<Text>().text = TextManager.hpA;
            _statusStatusMP.GetComponent<Text>().text = TextManager.mpA;
            _statusStatusTP.GetComponent<Text>().text = TextManager.tpA;
            _statusExp.GetComponent<Text>().text = TextManager.expTotal.Replace("%1", TextManager.expA);
            _statusNextExp.GetComponent<Text>().text = TextManager.expNext.Replace("%1", TextManager.levelA);
                //パラメーターステータス
            _statusParamStatusValueAttack.GetComponent<Text>().text = TextManager.stAttack;
            _statusParamStatusValueDefense.GetComponent<Text>().text = TextManager.stGuard;
            _statusParamStatusValueMagic.GetComponent<Text>().text = TextManager.stMagic;
            _statusParamStatusValueMagicDefense.GetComponent<Text>().text = TextManager.stMagicGuard;
            _statusParamStatusValueAgility.GetComponent<Text>().text = TextManager.stSpeed;
            _statusParamStatusValueLuck.GetComponent<Text>().text = TextManager.stLuck;
                //パラメーター装備
            _statusParamEquipWeapon.GetComponent<Text>().text = _systemSettingDataModel.equipTypes[0].name;
            _statusParamEquipShield.GetComponent<Text>().text = _systemSettingDataModel.equipTypes[1].name;
            _statusParamEquipHead.GetComponent<Text>().text = _systemSettingDataModel.equipTypes[2].name;
            _statusParamEquipBody.GetComponent<Text>().text = _systemSettingDataModel.equipTypes[3].name;
            if(_systemSettingDataModel.equipTypes.Count > 4)
                _statusParamEquipAccessory.GetComponent<Text>().text = _systemSettingDataModel.equipTypes[4].name;
            //ステータスメニューに表示されるアクターの更新
            if (actorEnabled)
            {
                CharacterActorDataModel actor = _characterActorDataModels.FindAll(item => item.charaType == (int) ActorTypeEnum.ACTOR)[0];
                _statusObj.transform.Find("MenuArea/StatusWindow/Status/Name").gameObject.GetComponent<TextMeshProUGUI>().text = actor.basic.name;
                foreach (var classDataModel in _classDataModels)
                    if (classDataModel.id == actor.basic.classId)
                    {
                        _statusObj.transform.Find("MenuArea/StatusWindow/Status/Class").gameObject.GetComponent<TextMeshProUGUI>()
                            .text = classDataModel.basic.name;
                        break;
                    }

                if (_statusObj.transform.Find("MenuArea/StatusWindow/Status/Face") != null)
                {
                    _statusObj.transform.Find("MenuArea/StatusWindow/Status/Face").gameObject.GetComponent<Image>()
                        .sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                        "Assets/RPGMaker/Storage/Images/Faces/" + actor.image.face + ".png");
                }
                else
                {
                    _statusObj.transform.Find("MenuArea/StatusWindow/Status/body").gameObject.GetComponent<Image>()
                        .sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                        "Assets/RPGMaker/Storage/Images/Pictures/" + actor.image.adv + ".png");
                }
            }

            
            //オプションウィンドウの文言取得
            _optionAlwaysDashText = _optionObj.transform.Find("MenuArea/Menus/AlwaysDash/Text").gameObject;
            _optionCommandRememberText = _optionObj.transform.Find("MenuArea/Menus/CommandRemember/Text").gameObject;
            _optionVolumeBGMText = _optionObj.transform.Find("MenuArea/Menus/VolumeBGM/Text").gameObject;
            _optionVolumeBGSText = _optionObj.transform.Find("MenuArea/Menus/VolumeBGS/Text").gameObject;
            _optionVolumeMEText = _optionObj.transform.Find("MenuArea/Menus/VolumeME/Text").gameObject;
            _optionVolumeSEText = _optionObj.transform.Find("MenuArea/Menus/VolumeSE/Text").gameObject;
            //オプションウィンドウの文言切り替え
            _optionAlwaysDashText.GetComponent<Text>().text = TextManager.alwaysDash;
            _optionCommandRememberText.GetComponent<Text>().text = TextManager.commandRemember;
            _optionVolumeBGMText.GetComponent<Text>().text = TextManager.bgmVolume;
            _optionVolumeBGSText.GetComponent<Text>().text = TextManager.bgsVolume;
            _optionVolumeMEText.GetComponent<Text>().text = TextManager.meVolume;
            _optionVolumeSEText.GetComponent<Text>().text = TextManager.seVolume;
            
            //セーブウィンドウの文言取得
            _saveTitleText = _saveObj.transform.Find("MenuArea/Description/Title").gameObject;
            //セーブウィンドウの文言切り替え
            _saveTitleText.GetComponent<TextMeshProUGUI>().text = TextManager.saveMessage;
            
            //エンドウィンドウの文言取得
            _endGameEndText = _endObj.transform.Find("MenuArea/Menu/GameEnd/Text").gameObject;
            _endBackText    = _endObj.transform.Find("MenuArea/Menu/Back/Text").gameObject;
            //エンドウィンドウの文言切り替え
            _endGameEndText.GetComponent<TextMeshProUGUI>().text = TextManager.toTitle;
            _endBackText.GetComponent<TextMeshProUGUI>().text = TextManager.cancel;

            var background = AssetDatabase.LoadAssetAtPath<GameObject>(MenuPrefabBackgroundPath);
            _backgroundObj = Object.Instantiate(background);
            _backgroundObj.transform.localPosition = new Vector3(-10f, 4f, 1f);
            _backgroundObj.transform.localScale = Vector3.one;
            sceneWindow?.MoveGameObjectToPreviewScene(_backgroundObj);
            _backgroundObj.SetActive(true);

            // 表示するメニューを切り替え
            ChangeDisplay();

            // 画像設定の反映
            if(uiSetting != UI_CHANGE_TYPE.NONE)
                ImageSettingApply(uiSetting);

            _menuIcon = _menuCanvas.transform.Find("UICanvas/MenuIcon").gameObject;
            _menuCanvas.SetActive(false);
            _menuIcon.SetActive(false);
        }

        private void ChangeDisplay() {
            AllDisplayDisable();

            switch (_displayType)
            {
                case DISPLAY_TYPE.MAIN_MENU:
                    _mainWindow.SetActive(true);
                    break;
                case DISPLAY_TYPE.ITEM:
                    InitItem();
                    _itemObj.SetActive(true);
                    break;
                case DISPLAY_TYPE.SKILL:
                    InitSkill();
                    _skillObj.SetActive(true);
                    break;
                case DISPLAY_TYPE.EQUIP:
                    InitEquip();
                    _equipObj.SetActive(true);
                    break;
                case DISPLAY_TYPE.STATUS:
                    InitStatus();
                    _statusObj.SetActive(true);
                    break;
                case DISPLAY_TYPE.SORT:
                    InitSort();
                    _sortObj.SetActive(true);
                    break;
                case DISPLAY_TYPE.OPTION:
                    _optionObj.SetActive(true);
                    break;
                case DISPLAY_TYPE.SAVE:
                    _saveObj.SetActive(true);
                    break;
                case DISPLAY_TYPE.END:
                    _endObj.SetActive(true);
                    break;
            }
        }

        private void InitItem(bool isIcon = true) {
            var itemData = _databaseManagementService.LoadItem();
            _items = new List<GameObject>();

            _itemContent = _itemObj.transform.Find("MenuArea/Skill/Scroll View/Viewport/Content").gameObject;
            _itemBase = _itemObj.transform.Find("MenuArea/Skill/Items/Orijin").gameObject;

            // アイテム表示
            if (isIcon)
            {
                int cnt = 0;
                for (int i = 0; cnt < 12 && i < itemData.Count; i++)
                {
                    if (itemData[i].basic.itemType == (int) ItemEnums.ItemType.NORMAL)
                    {
                        var obj = Object.Instantiate(_itemBase);
                        obj.transform.SetParent(_itemContent.transform);
                        obj.transform.localScale = Vector3.one;
                        obj.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = itemData[i].basic.name;
                        obj.transform.Find("Value").GetComponent<TextMeshProUGUI>().text = "1";
                        obj.transform.Find("Icon").GetComponent<Image>().sprite = GetItemImage(itemData[i].basic.iconId,
                            obj.transform.Find("Icon").gameObject);
                        obj.transform.Find("Highlight").GetComponent<Image>().color = Color.clear;
                        obj.SetActive(true);
                        _items.Add(obj);
                        cnt++;
                    }
                }
            }

            // ハイライト切り替え
            if (_itemItemObj.activeSelf)
            {
                _itemItemObj.transform.Find("Highlight").gameObject.SetActive(true);
                _itemWeaponObj.transform.Find("Highlight").gameObject.SetActive(false);
                _itemArmorObj.transform.Find("Highlight").gameObject.SetActive(false);
                _itemImportantObj.transform.Find("Highlight").gameObject.SetActive(false);
            }
            else if (_itemWeaponObj.activeSelf)
            {
                _itemItemObj.transform.Find("Highlight").gameObject.SetActive(false);
                _itemWeaponObj.transform.Find("Highlight").gameObject.SetActive(true);
                _itemArmorObj.transform.Find("Highlight").gameObject.SetActive(false);
                _itemImportantObj.transform.Find("Highlight").gameObject.SetActive(false);
            }
            else if (_itemArmorObj.activeSelf)
            {
                _itemItemObj.transform.Find("Highlight").gameObject.SetActive(false);
                _itemWeaponObj.transform.Find("Highlight").gameObject.SetActive(false);
                _itemArmorObj.transform.Find("Highlight").gameObject.SetActive(true);
                _itemImportantObj.transform.Find("Highlight").gameObject.SetActive(false);
            }
            else if (_itemImportantObj.activeSelf)
            {
                _itemItemObj.transform.Find("Highlight").gameObject.SetActive(false);
                _itemWeaponObj.transform.Find("Highlight").gameObject.SetActive(false);
                _itemArmorObj.transform.Find("Highlight").gameObject.SetActive(false);
                _itemImportantObj.transform.Find("Highlight").gameObject.SetActive(true);
            }
        }

        private void InitSkill() {
            var skillData = _databaseManagementService.LoadSkillCustom();
            var classData = _databaseManagementService.LoadCharacterActorClass();
            CharacterActorDataModel actor = null;
            for (int i = 0; i < _characterActorDataModels.Count; i++)
                if (_characterActorDataModels[i].charaType == (int) ActorTypeEnum.ACTOR)
                {
                    actor = _characterActorDataModels[i];
                    break;
                }
            List<ClassDataModel.SkillType> skillTypes = new List<ClassDataModel.SkillType>();
            for (int i = 0; i < classData.Count; i++)
                if (classData[i].id == actor.basic.classId)
                {
                    skillTypes = classData[i].skillTypes;
                    break;
                }

            _skillContent = _skillObj.transform.Find("MenuArea/Skill/Scroll View/Viewport/Content/Ruck1").gameObject;
            _skillBase = _skillContent.transform.Find("Item1").gameObject;
            _skillBase.SetActive(false);

            // スキル表示追加
            if (actor != null)
            {
                for (int i = 0; i < skillTypes.Count; i++)
                {
                    SkillCustomDataModel customDataModel = null;
                    for (int i2 = 0; i2 < skillData.Count; i2++)
                        if (skillData[i2].basic.id == skillTypes[i].skillId)
                        {
                            customDataModel = skillData[i2];
                            break;
                        }
                    if (customDataModel == null) continue;

                    var obj = Object.Instantiate(_skillBase);
                    obj.transform.SetParent(_skillContent.transform);
                    obj.transform.localScale = Vector3.one;
                    obj.transform.localPosition = Vector3.one;
                    obj.SetActive(true);

                    obj.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = 
                        customDataModel.basic.name;
                    obj.transform.Find("Icon").GetComponent<Image>().sprite =
                        GetItemImage(customDataModel.basic.iconId, obj.transform.Find("Icon").gameObject);
                    obj.transform.Find("Value").GetComponent<TextMeshProUGUI>().text =
                        customDataModel.basic.costMp > 0 ?
                        customDataModel.basic.costMp.ToString() :
                        customDataModel.basic.costTp.ToString();
                    obj.transform.Find("Highlight").GetComponent<Image>().color = Color.clear;
                }

                //顔アイコン、立ち絵、SDキャラクターの設定値に応じて、表示するものを変更する
                Image face = null;
                Image body = null;
                if (_skillObj.transform.Find("MenuArea/Status/Face") != null)
                    face = _skillObj.transform.Find("MenuArea/Status/Face").GetComponent<Image>();
                if (_skillObj.transform.Find("MenuArea/Status/body") != null)
                    body = _skillObj.transform.Find("MenuArea/Status/body").GetComponent<Image>();

                //双方の部品が存在する場合は、顔アイコン,SDキャラと立ち絵画像で、表示する部品を変更する
                if (face != null && body != null)
                {
                    if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.FACE)
                    {
                        //顔アイコン
                        var sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                            "Assets/RPGMaker/Storage/Images/Faces/" +
                            actor.image.face + ".png");
                        face.enabled = true;
                        face.sprite = sprite;
                        face.material = null;
                        face.color = Color.white;
                        face.preserveAspect = true;

                        face.gameObject.SetActive(true);
                        body.gameObject.SetActive(false);
                    }
                    else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.SD)
                    {
                        //SDキャラ
                        var assetId = DataManager.Self().GetActorDataModel(actor.uuId).image.character;
                        CharacterGraphic characterGraphic = new GameObject().AddComponent<CharacterGraphic>();
                        characterGraphic.Init(assetId);

                        face.enabled = true;
                        face.sprite = characterGraphic.GetCurrentSprite();
                        face.color = Color.white;
                        face.material = characterGraphic.GetMaterial();
                        face.transform.localScale = characterGraphic.GetSize();

                        if (face.transform.localScale.x > 1.0f || face.transform.localScale.y > 1.0f)
                        {
                            if (face.transform.localScale.y > 1.0f)
                            {
                                face.transform.localScale = new Vector2(face.transform.localScale.x / face.transform.localScale.y, 1.0f);
                            }
                            else
                            {
                                face.transform.localScale = new Vector2(1.0f, face.transform.localScale.y / face.transform.localScale.x);
                            }
                        }

                        face.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                        Object.DestroyImmediate(characterGraphic.gameObject);

                        face.gameObject.SetActive(true);
                        body.gameObject.SetActive(false);
                    }
                    else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.PICTURE)
                    {
                        //立ち絵
                        var imageName = actor.image.adv.Contains(".png")
                            ? actor.image.adv
                            : actor.image.adv + ".png";
                        var path = "Assets/RPGMaker/Storage/Images/Pictures/" + imageName;
                        var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);

                        body.enabled = true;
                        body.sprite = tex;
                        body.color = Color.white;
                        body.preserveAspect = true;

                        face.gameObject.SetActive(false);
                        body.gameObject.SetActive(true);
                    }
                    else
                    {
                        face.gameObject.SetActive(false);
                        body.gameObject.SetActive(false);
                    }
                }
                //片方の部品しかない場合は、いずれのパターンでもその部品に描画する
                else if (face != null)
                {
                    if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.FACE)
                    {
                        //顔アイコン
                        var sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                            "Assets/RPGMaker/Storage/Images/Faces/" +
                            actor.image.face + ".png");
                        face.enabled = true;
                        face.sprite = sprite;
                        face.color = Color.white;
                        face.preserveAspect = true;
                    }
                    else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.SD)
                    {
                        //SDキャラ
                        var assetId = DataManager.Self().GetActorDataModel(actor.uuId).image.character;
                        CharacterGraphic characterGraphic = new GameObject().AddComponent<CharacterGraphic>();
                        characterGraphic.Init(assetId);

                        face.enabled = true;
                        face.sprite = characterGraphic.GetCurrentSprite();
                        face.color = Color.white;
                        face.material = characterGraphic.GetMaterial();
                        face.transform.localScale = characterGraphic.GetSize();

                        if (face.transform.localScale.x > 1.0f || face.transform.localScale.y > 1.0f)
                        {
                            if (face.transform.localScale.y > 1.0f)
                            {
                                face.transform.localScale = new Vector2(face.transform.localScale.x / face.transform.localScale.y, 1.0f);
                            }
                            else
                            {
                                face.transform.localScale = new Vector2(1.0f, face.transform.localScale.y / face.transform.localScale.x);
                            }
                        }

                        face.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                        Object.DestroyImmediate(characterGraphic.gameObject);
                    }
                    else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.PICTURE)
                    {
                        //立ち絵
                        var imageName = actor.image.adv.Contains(".png")
                            ? actor.image.adv
                            : actor.image.adv + ".png";
                        var path = "Assets/RPGMaker/Storage/Images/Pictures/" + imageName;
                        var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);

                        face.enabled = true;
                        face.sprite = tex;
                        face.color = Color.white;
                        face.preserveAspect = true;
                    }
                    else
                        face.enabled = false;
                }
                else if (body != null)
                {
                    if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.FACE)
                    {
                        //顔アイコン
                        var sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                            "Assets/RPGMaker/Storage/Images/Faces/" +
                            actor.image.face + ".png");
                        body.enabled = true;
                        body.sprite = sprite;
                        body.color = Color.white;
                        body.preserveAspect = true;
                    }
                    else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.SD)
                    {
                        //SDキャラ
                        var assetId = DataManager.Self().GetActorDataModel(actor.uuId).image.character;
                        CharacterGraphic characterGraphic = new GameObject().AddComponent<CharacterGraphic>();
                        characterGraphic.Init(assetId);

                        body.enabled = true;
                        body.sprite = characterGraphic.GetCurrentSprite();
                        body.color = Color.white;
                        body.material = characterGraphic.GetMaterial();
                        body.transform.localScale = characterGraphic.GetSize();

                        if (body.transform.localScale.x > 1.0f || body.transform.localScale.y > 1.0f)
                        {
                            if (body.transform.localScale.y > 1.0f)
                            {
                                body.transform.localScale = new Vector2(body.transform.localScale.x / body.transform.localScale.y, 1.0f);
                            }
                            else
                            {
                                body.transform.localScale = new Vector2(1.0f, body.transform.localScale.y / body.transform.localScale.x);
                            }
                        }

                        body.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                        Object.DestroyImmediate(characterGraphic.gameObject);
                    }
                    else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.PICTURE)
                    {
                        //立ち絵
                        var imageName = actor.image.adv.Contains(".png")
                            ? actor.image.adv
                            : actor.image.adv + ".png";
                        var path = "Assets/RPGMaker/Storage/Images/Pictures/" + imageName;
                        var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);

                        body.enabled = true;
                        body.sprite = tex;
                        body.color = Color.white;
                        body.preserveAspect = true;
                    }
                    else
                        body.enabled = false;
                }
            }
        }

        private void InitEquip() {
            var weaponData = _databaseManagementService.LoadWeapon();
            var armorData = _databaseManagementService.LoadArmor();
            var classData = _databaseManagementService.LoadCharacterActorClass();
            CharacterActorDataModel actor = null;
            for (int i = 0; i < _characterActorDataModels.Count; i++)
                if (_characterActorDataModels[i].charaType == (int) ActorTypeEnum.ACTOR)
                {
                    actor = _characterActorDataModels[i];
                    break;
                }

            var equipWeapon = _equipObj.transform.Find("MenuArea/Equip/EquipTypeScrollView/Viewport/EquipValues/Weapon").gameObject;
            var equipShield = _equipObj.transform.Find("MenuArea/Equip/EquipTypeScrollView/Viewport/EquipValues/Shield").gameObject;
            var equipHead = _equipObj.transform.Find("MenuArea/Equip/EquipTypeScrollView/Viewport/EquipValues/Head").gameObject;
            var equipBody = _equipObj.transform.Find("MenuArea/Equip/EquipTypeScrollView/Viewport/EquipValues/Body").gameObject;
            var equipAccessory = _equipObj.transform.Find("MenuArea/Equip/EquipTypeScrollView/Viewport/EquipValues/Accessory").gameObject;
            var descriptionText1 = _equipObj.transform.Find("MenuArea/Description/DescriptionText1").GetComponent<TextMeshProUGUI>();
            var descriptionText2 = _equipObj.transform.Find("MenuArea/Description/DescriptionText2").GetComponent<TextMeshProUGUI>();
            descriptionText1.text = "";
            descriptionText2.text = "";
            List<GameObject> objects = new List<GameObject>()
            {
                equipWeapon,
                equipShield,
                equipHead,
                equipBody,
                equipAccessory
            };

            List<string> equipIds = new List<string>()
            {
                actor.equips[0].value,
                actor.equips[1].value,
                actor.equips[2].value,
                actor.equips[3].value,
                actor.equips[4].value,
            };

            for (int i = 0; i < objects.Count; i++)
            {
                var name = objects[i].transform.Find("Name").GetComponent<TextMeshProUGUI>();
                var icon = objects[i].transform.Find("Icon").GetComponent<Image>();
                objects[i].transform.Find("Highlight").GetComponent<Image>().color = Color.clear;

                name.text = "";
                icon.enabled = true;

                WeaponDataModel weapon = null;
                for (int i2 = 0; i2 < weaponData.Count; i2++)
                    if (weaponData[i2].basic.id == equipIds[i])
                    {
                        weapon = weaponData[i2];
                        break;
                    }

                ArmorDataModel armor = null;
                for (int i2 = 0; i2 < armorData.Count; i2++)
                    if (armorData[i2].basic.id == equipIds[i])
                    {
                        armor = armorData[i2];
                        break;
                    }

                if (weapon != null)
                {
                    name.text = weapon.basic.name;
                    icon.sprite = GetItemImage(weapon.basic.iconId, objects[i].gameObject);
                }
                else if (armor != null)
                {
                    name.text = armor.basic.name;
                    icon.sprite = GetItemImage(armor.basic.iconId, objects[i].gameObject);
                }
                else
                    icon.enabled = false;
            }

            // ハイライト切り替え
            _equipObj.transform.Find("MenuArea/Menus/Item1/Highlight").gameObject.SetActive(true);
            _equipObj.transform.Find("MenuArea/Menus/Item2/Highlight").gameObject.SetActive(false);
            _equipObj.transform.Find("MenuArea/Menus/Item3/Highlight").gameObject.SetActive(false);

            //顔アイコン、立ち絵、SDキャラクターの設定値に応じて、表示するものを変更する
            Image face = null;
            Image body = null;
            if (_equipObj.transform.Find("MenuArea/StatusWindow/Face") != null)
                face = _equipObj.transform.Find("MenuArea/StatusWindow/Face").GetComponent<Image>();
            if (_equipObj.transform.Find("MenuArea/StatusWindow/body") != null)
                body = _equipObj.transform.Find("MenuArea/StatusWindow/body/Image").GetComponent<Image>();

            //双方の部品が存在する場合は、顔アイコン,SDキャラと立ち絵画像で、表示する部品を変更する
            if (face != null && body != null)
            {
                if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.FACE)
                {
                    //顔アイコン
                    var sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                        "Assets/RPGMaker/Storage/Images/Faces/" +
                        actor.image.face + ".png");
                    face.enabled = true;
                    face.sprite = sprite;
                    face.material = null;
                    face.color = Color.white;
                    face.preserveAspect = true;

                    face.gameObject.SetActive(true);
                    body.gameObject.SetActive(false);
                }
                else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.SD)
                {
                    //SDキャラ
                    var assetId = DataManager.Self().GetActorDataModel(actor.uuId).image.character;
                    CharacterGraphic characterGraphic = new GameObject().AddComponent<CharacterGraphic>();
                    characterGraphic.Init(assetId);

                    face.enabled = true;
                    face.sprite = characterGraphic.GetCurrentSprite();
                    face.color = Color.white;
                    face.material = characterGraphic.GetMaterial();
                    face.transform.localScale = characterGraphic.GetSize();

                    if (face.transform.localScale.x > 1.0f || face.transform.localScale.y > 1.0f)
                    {
                        if (face.transform.localScale.y > 1.0f)
                        {
                            face.transform.localScale = new Vector2(face.transform.localScale.x / face.transform.localScale.y, 1.0f);
                        }
                        else
                        {
                            face.transform.localScale = new Vector2(1.0f, face.transform.localScale.y / face.transform.localScale.x);
                        }
                    }

                    face.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    Object.DestroyImmediate(characterGraphic.gameObject);

                    face.gameObject.SetActive(true);
                    body.gameObject.SetActive(false);
                }
                else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.PICTURE)
                {
                    //立ち絵
                    var imageName = actor.image.adv.Contains(".png")
                        ? actor.image.adv
                        : actor.image.adv + ".png";
                    var path = "Assets/RPGMaker/Storage/Images/Pictures/" + imageName;
                    var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);

                    body.enabled = true;
                    body.sprite = tex;
                    body.color = Color.white;
                    body.preserveAspect = true;

                    face.gameObject.SetActive(false);
                    body.gameObject.SetActive(true);
                }
                else
                {
                    face.gameObject.SetActive(false);
                    body.gameObject.SetActive(false);
                }
            }
            //片方の部品しかない場合は、いずれのパターンでもその部品に描画する
            else if (face != null)
            {
                if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.FACE)
                {
                    //顔アイコン
                    var sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                        "Assets/RPGMaker/Storage/Images/Faces/" +
                        actor.image.face + ".png");
                    face.enabled = true;
                    face.sprite = sprite;
                    face.color = Color.white;
                    face.preserveAspect = true;
                }
                else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.SD)
                {
                    //SDキャラ
                    var assetId = DataManager.Self().GetActorDataModel(actor.uuId).image.character;
                    CharacterGraphic characterGraphic = new GameObject().AddComponent<CharacterGraphic>();
                    characterGraphic.Init(assetId);

                    face.enabled = true;
                    face.sprite = characterGraphic.GetCurrentSprite();
                    face.color = Color.white;
                    face.material = characterGraphic.GetMaterial();
                    face.transform.localScale = characterGraphic.GetSize();

                    if (face.transform.localScale.x > 1.0f || face.transform.localScale.y > 1.0f)
                    {
                        if (face.transform.localScale.y > 1.0f)
                        {
                            face.transform.localScale = new Vector2(face.transform.localScale.x / face.transform.localScale.y, 1.0f);
                        }
                        else
                        {
                            face.transform.localScale = new Vector2(1.0f, face.transform.localScale.y / face.transform.localScale.x);
                        }
                    }

                    face.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    Object.DestroyImmediate(characterGraphic.gameObject);
                }
                else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.PICTURE)
                {
                    //立ち絵
                    var imageName = actor.image.adv.Contains(".png")
                        ? actor.image.adv
                        : actor.image.adv + ".png";
                    var path = "Assets/RPGMaker/Storage/Images/Pictures/" + imageName;
                    var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);

                    face.enabled = true;
                    face.sprite = tex;
                    face.color = Color.white;
                    face.preserveAspect = true;
                }
                else
                    face.enabled = false;
            }
            else if (body != null)
            {
                if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.FACE)
                {
                    //顔アイコン
                    var sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                        "Assets/RPGMaker/Storage/Images/Faces/" +
                        actor.image.face + ".png");
                    body.enabled = true;
                    body.sprite = sprite;
                    body.color = Color.white;
                    body.preserveAspect = true;
                }
                else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.SD)
                {
                    //SDキャラ
                    var assetId = DataManager.Self().GetActorDataModel(actor.uuId).image.character;
                    CharacterGraphic characterGraphic = new GameObject().AddComponent<CharacterGraphic>();
                    characterGraphic.Init(assetId);

                    body.enabled = true;
                    body.sprite = characterGraphic.GetCurrentSprite();
                    body.color = Color.white;
                    body.material = characterGraphic.GetMaterial();
                    body.transform.localScale = characterGraphic.GetSize();

                    if (body.transform.localScale.x > 1.0f || body.transform.localScale.y > 1.0f)
                    {
                        if (body.transform.localScale.y > 1.0f)
                        {
                            body.transform.localScale = new Vector2(body.transform.localScale.x / body.transform.localScale.y, 1.0f);
                        }
                        else
                        {
                            body.transform.localScale = new Vector2(1.0f, body.transform.localScale.y / body.transform.localScale.x);
                        }
                    }

                    body.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    Object.DestroyImmediate(characterGraphic.gameObject);
                }
                else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.PICTURE)
                {
                    //立ち絵
                    var imageName = actor.image.adv.Contains(".png")
                        ? actor.image.adv
                        : actor.image.adv + ".png";
                    var path = "Assets/RPGMaker/Storage/Images/Pictures/" + imageName;
                    var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);

                    body.enabled = true;
                    body.sprite = tex;
                    body.color = Color.white;
                    body.preserveAspect = true;
                }
                else
                    body.enabled = false;
            }
        }
        private void InitStatus() {
            var weaponData = _databaseManagementService.LoadWeapon();
            var armorData = _databaseManagementService.LoadArmor();
            var classData = _databaseManagementService.LoadCharacterActorClass();
            CharacterActorDataModel actor = null;
            for (int i = 0; i < _characterActorDataModels.Count; i++)
                if (_characterActorDataModels[i].charaType == (int) ActorTypeEnum.ACTOR)
                {
                    actor = _characterActorDataModels[i];
                    break;
                }

            var statusWeapon = _statusObj.transform.Find("MenuArea/ParamWindow/Equip/Scroll View/Viewport/EquipValues/Weapon").gameObject;
            var statusShield = _statusObj.transform.Find("MenuArea/ParamWindow/Equip/Scroll View/Viewport/EquipValues/Shield").gameObject;
            var statusHead = _statusObj.transform.Find("MenuArea/ParamWindow/Equip/Scroll View/Viewport/EquipValues/Head").gameObject;
            var statusBody = _statusObj.transform.Find("MenuArea/ParamWindow/Equip/Scroll View/Viewport/EquipValues/Body").gameObject;
            var statusAcce = _statusObj.transform.Find("MenuArea/ParamWindow/Equip/Scroll View/Viewport/EquipValues/Accessory").gameObject;

            List<GameObject> objects = new List<GameObject>()
            {
                statusWeapon,
                statusShield,
                statusHead,
                statusBody,
                statusAcce
            };

            List<string> equipIds = new List<string>()
            {
                actor.equips[0].value,
                actor.equips[1].value,
                actor.equips[2].value,
                actor.equips[3].value,
                actor.equips[4].value,
            };

            for (int i = 0; i < objects.Count; i++)
            {
                var name = objects[i].transform.Find("Name").GetComponent<TextMeshProUGUI>();
                var icon = objects[i].transform.Find("Icon").GetComponent<Image>();

                name.text = "";
                icon.enabled = true;

                WeaponDataModel weapon = null;
                for (int i2 = 0; i2 < weaponData.Count; i2++)
                    if (weaponData[i2].basic.id == equipIds[i])
                    {
                        weapon = weaponData[i2];
                        break;
                    }

                ArmorDataModel armor = null;
                for (int i2 = 0; i2 < armorData.Count; i2++)
                    if (armorData[i2].basic.id == equipIds[i])
                    {
                        armor = armorData[i2];
                        break;
                    }

                if (weapon != null)
                {
                    name.text = weapon.basic.name;
                    icon.sprite = GetItemImage(weapon.basic.iconId, objects[i].gameObject);
                }
                else if (armor != null)
                {
                    name.text = armor.basic.name;
                    icon.sprite = GetItemImage(armor.basic.iconId, objects[i].gameObject);
                }
                else
                    icon.enabled = false;
            }

            //顔アイコン、立ち絵、SDキャラクターの設定値に応じて、表示するものを変更する
            Image face = null;
            Image body = null;
            if (_statusObj.transform.Find("MenuArea/StatusWindow/Status/Face") != null)
                face = _statusObj.transform.Find("MenuArea/StatusWindow/Status/Face").GetComponent<Image>();
            if (_statusObj.transform.Find("MenuArea/StatusWindow/Status/body") != null)
                body = _statusObj.transform.Find("MenuArea/StatusWindow/Status/body").GetComponent<Image>();

            //双方の部品が存在する場合は、顔アイコン,SDキャラと立ち絵画像で、表示する部品を変更する
            if (face != null && body != null)
            {
                if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.FACE)
                {
                    //顔アイコン
                    var sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                        "Assets/RPGMaker/Storage/Images/Faces/" +
                        actor.image.face + ".png");
                    face.enabled = true;
                    face.sprite = sprite;
                    face.material = null;
                    face.color = Color.white;
                    face.preserveAspect = true;

                    face.gameObject.SetActive(true);
                    body.gameObject.SetActive(false);
                }
                else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.SD)
                {
                    //SDキャラ
                    var assetId = DataManager.Self().GetActorDataModel(actor.uuId).image.character;
                    CharacterGraphic characterGraphic = new GameObject().AddComponent<CharacterGraphic>();
                    characterGraphic.Init(assetId);

                    face.enabled = true;
                    face.sprite = characterGraphic.GetCurrentSprite();
                    face.color = Color.white;
                    face.material = characterGraphic.GetMaterial();
                    face.transform.localScale = characterGraphic.GetSize();

                    if (face.transform.localScale.x > 1.0f || face.transform.localScale.y > 1.0f)
                    {
                        if (face.transform.localScale.y > 1.0f)
                        {
                            face.transform.localScale = new Vector2(face.transform.localScale.x / face.transform.localScale.y, 1.0f);
                        }
                        else
                        {
                            face.transform.localScale = new Vector2(1.0f, face.transform.localScale.y / face.transform.localScale.x);
                        }
                    }

                    face.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    Object.DestroyImmediate(characterGraphic.gameObject);

                    face.gameObject.SetActive(true);
                    body.gameObject.SetActive(false);
                }
                else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.PICTURE)
                {
                    //立ち絵
                    var imageName = actor.image.adv.Contains(".png")
                        ? actor.image.adv
                        : actor.image.adv + ".png";
                    var path = "Assets/RPGMaker/Storage/Images/Pictures/" + imageName;
                    var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);

                    body.enabled = true;
                    body.sprite = tex;
                    body.color = Color.white;
                    body.preserveAspect = true;

                    face.gameObject.SetActive(false);
                    body.gameObject.SetActive(true);
                }
                else
                {
                    face.gameObject.SetActive(false);
                    body.gameObject.SetActive(false);
                }
            }
            //片方の部品しかない場合は、いずれのパターンでもその部品に描画する
            else if (face != null)
            {
                if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.FACE)
                {
                    //顔アイコン
                    var sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                        "Assets/RPGMaker/Storage/Images/Faces/" +
                        actor.image.face + ".png");
                    face.enabled = true;
                    face.sprite = sprite;
                    face.color = Color.white;
                    face.preserveAspect = true;
                }
                else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.SD)
                {
                    //SDキャラ
                    var assetId = DataManager.Self().GetActorDataModel(actor.uuId).image.character;
                    CharacterGraphic characterGraphic = new GameObject().AddComponent<CharacterGraphic>();
                    characterGraphic.Init(assetId);

                    face.enabled = true;
                    face.sprite = characterGraphic.GetCurrentSprite();
                    face.color = Color.white;
                    face.material = characterGraphic.GetMaterial();
                    face.transform.localScale = characterGraphic.GetSize();

                    if (face.transform.localScale.x > 1.0f || face.transform.localScale.y > 1.0f)
                    {
                        if (face.transform.localScale.y > 1.0f)
                        {
                            face.transform.localScale = new Vector2(face.transform.localScale.x / face.transform.localScale.y, 1.0f);
                        }
                        else
                        {
                            face.transform.localScale = new Vector2(1.0f, face.transform.localScale.y / face.transform.localScale.x);
                        }
                    }

                    face.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    Object.DestroyImmediate(characterGraphic.gameObject);
                }
                else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.PICTURE)
                {
                    //立ち絵
                    var imageName = actor.image.adv.Contains(".png")
                        ? actor.image.adv
                        : actor.image.adv + ".png";
                    var path = "Assets/RPGMaker/Storage/Images/Pictures/" + imageName;
                    var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);

                    face.enabled = true;
                    face.sprite = tex;
                    face.color = Color.white;
                    face.preserveAspect = true;
                }
                else
                    face.enabled = false;
            }
            else if (body != null)
            {
                if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.FACE)
                {
                    //顔アイコン
                    var sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                        "Assets/RPGMaker/Storage/Images/Faces/" +
                        actor.image.face + ".png");
                    body.enabled = true;
                    body.sprite = sprite;
                    body.color = Color.white;
                    body.preserveAspect = true;
                }
                else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.SD)
                {
                    //SDキャラ
                    var assetId = DataManager.Self().GetActorDataModel(actor.uuId).image.character;
                    CharacterGraphic characterGraphic = new GameObject().AddComponent<CharacterGraphic>();
                    characterGraphic.Init(assetId);

                    body.enabled = true;
                    body.sprite = characterGraphic.GetCurrentSprite();
                    body.color = Color.white;
                    body.material = characterGraphic.GetMaterial();
                    body.transform.localScale = characterGraphic.GetSize();

                    if (body.transform.localScale.x > 1.0f || body.transform.localScale.y > 1.0f)
                    {
                        if (body.transform.localScale.y > 1.0f)
                        {
                            body.transform.localScale = new Vector2(body.transform.localScale.x / body.transform.localScale.y, 1.0f);
                        }
                        else
                        {
                            body.transform.localScale = new Vector2(1.0f, body.transform.localScale.y / body.transform.localScale.x);
                        }
                    }

                    body.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    Object.DestroyImmediate(characterGraphic.gameObject);
                }
                else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.PICTURE)
                {
                    //立ち絵
                    var imageName = actor.image.adv.Contains(".png")
                        ? actor.image.adv
                        : actor.image.adv + ".png";
                    var path = "Assets/RPGMaker/Storage/Images/Pictures/" + imageName;
                    var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);

                    body.enabled = true;
                    body.sprite = tex;
                    body.color = Color.white;
                    body.preserveAspect = true;
                }
                else
                    body.enabled = false;
            }

            // 詳細
            _statusObj.transform.Find("MenuArea/Description/DescriptionText1").GetComponent<TextMeshProUGUI>().text =
                EditorLocalize.LocalizeText("WORD_4017");
            _statusObj.transform.Find("MenuArea/Description/DescriptionText2").GetComponent<TextMeshProUGUI>().text =
                "";
        }

        private void InitSort() {

            //アクターが何人いるか
            int actorCount =
                _characterActorDataModels.FindAll(item => item.charaType == (int) ActorTypeEnum.ACTOR).Count <= 4
                    ? _characterActorDataModels.FindAll(item => item.charaType == (int) ActorTypeEnum.ACTOR).Count
                    : 4;


            //並び替えのアクター数分回す
            for (int i = 1; i <= 4; i++)
            {
                if (i <= actorCount)
                {
                    _sortObj.transform.Find("PartyWindow/BeforePartyItems/Actor" + i).gameObject.SetActive(true);

                    //並び替えウィンドウの文言取得
                    _sortActorLv = _sortObj.transform.Find("PartyWindow/BeforePartyItems/Actor" + i + "/Level")
                        .gameObject;
                    _sortActorHp = _sortObj.transform.Find("PartyWindow/BeforePartyItems/Actor" + i + "/Hp/HpName")
                        .gameObject;
                    _sortActorMp = _sortObj.transform.Find("PartyWindow/BeforePartyItems/Actor" + i + "/Mp/MpName")
                        .gameObject;
                    _sortActorTp = _sortObj.transform.Find("PartyWindow/BeforePartyItems/Actor" + i + "/Tp/TpName")
                        .gameObject;
                    //並び替えウィンドウの文言切り替え
                    _sortActorLv.GetComponent<Text>().text = TextManager.levelA;
                    _sortActorHp.GetComponent<Text>().text = TextManager.hpA;
                    _sortActorMp.GetComponent<Text>().text = TextManager.mpA;
                    _sortActorTp.GetComponent<Text>().text = TextManager.tpA;

                    //アクター情報の表示
                    CharacterActorDataModel actor =
                        _characterActorDataModels.FindAll(item => item.charaType == (int) ActorTypeEnum.ACTOR)[i - 1];
                    _sortObj.transform.Find("PartyWindow/BeforePartyItems/Actor" + i + "/Name").gameObject
                        .GetComponent<TextMeshProUGUI>().text = actor.basic.name;
                    foreach (var classDataModel in _classDataModels)
                        if (classDataModel.id == actor.basic.classId)
                        {
                            _sortObj.transform.Find("PartyWindow/BeforePartyItems/Actor" + i + "/Class").gameObject
                                .GetComponent<TextMeshProUGUI>().text = classDataModel.basic.name;
                            break;
                        }

                    _sortObj.transform.Find("PartyWindow/BeforePartyItems/Actor" + i + "/Face").gameObject
                            .GetComponent<Image>().sprite =
                        UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                            "Assets/RPGMaker/Storage/Images/Faces/" + actor.image.face + ".png");

                    //顔アイコン、立ち絵、SDキャラクターの設定値に応じて、表示するものを変更する
                    Image face = null;
                    if (_sortObj.transform.Find("PartyWindow/BeforePartyItems/Actor" + i + "/Face") != null)
                        face = _sortObj.transform.Find("PartyWindow/BeforePartyItems/Actor" + i + "/Face")
                            .GetComponent<Image>();

                    if (face != null)
                    {
                        if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.FACE)
                        {
                            //顔アイコン
                            var sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                                "Assets/RPGMaker/Storage/Images/Faces/" +
                                actor.image.face + ".png");
                            face.enabled = true;
                            face.sprite = sprite;
                            face.color = Color.white;
                            face.preserveAspect = true;
                        }
                        else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.SD)
                        {
                            //SDキャラ
                            var assetId = DataManager.Self().GetActorDataModel(actor.uuId).image.character;
                            CharacterGraphic characterGraphic = new GameObject().AddComponent<CharacterGraphic>();
                            characterGraphic.Init(assetId);

                            face.enabled = true;
                            face.sprite = characterGraphic.GetCurrentSprite();
                            face.color = Color.white;
                            face.material = characterGraphic.GetMaterial();
                            face.transform.localScale = characterGraphic.GetSize();

                            if (face.transform.localScale.x > 1.0f || face.transform.localScale.y > 1.0f)
                            {
                                if (face.transform.localScale.y > 1.0f)
                                {
                                    face.transform.localScale =
                                        new Vector2(face.transform.localScale.x / face.transform.localScale.y, 1.0f);
                                }
                                else
                                {
                                    face.transform.localScale = new Vector2(1.0f,
                                        face.transform.localScale.y / face.transform.localScale.x);
                                }
                            }

                            face.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                            Object.DestroyImmediate(characterGraphic.gameObject);
                        }
                        else if (_uiSettingDataModel.commonMenus[0].characterType == (int) MenuIconTypeEnum.PICTURE)
                        {
                            //立ち絵
                            var imageName = actor.image.adv.Contains(".png")
                                ? actor.image.adv
                                : actor.image.adv + ".png";
                            var path = "Assets/RPGMaker/Storage/Images/Pictures/" + imageName;
                            var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);

                            face.enabled = true;
                            face.sprite = tex;
                            face.color = Color.white;
                            face.preserveAspect = true;
                        }
                        else
                            face.enabled = false;
                    }
                }
                else
                {
                    //表示させるアクターがいなかった場合に非表示にする
                    _sortObj.transform.Find("PartyWindow/BeforePartyItems/Actor" + i).gameObject.SetActive(false);
                }
            }
        }

        private void ResetSetting() {
            switch (_displayType)
            {
                case DISPLAY_TYPE.MAIN_MENU:
                    break;
                case DISPLAY_TYPE.ITEM:
                    for (int i = 0; i < _items.Count; i++)
                        Object.DestroyImmediate(_items[i]);
                    _items = null;
                    break;
                case DISPLAY_TYPE.SKILL:
                    break;
                case DISPLAY_TYPE.EQUIP:
                    break;
                case DISPLAY_TYPE.STATUS:
                    break;
                case DISPLAY_TYPE.SORT:
                    break;
                case DISPLAY_TYPE.OPTION:
                    break;
                case DISPLAY_TYPE.SAVE:
                    break;
                case DISPLAY_TYPE.END:
                    break;
            }
        }

        public Sprite GetItemImage(string iconName, GameObject gameObject) {
            var iconSetTexture =
                UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                    "Assets/RPGMaker/Storage/Images/System/IconSet/" + iconName + ".png");

            var iconTexture = iconSetTexture;
            if (iconTexture == null)
            {
                gameObject.gameObject.SetActive(false);
                return null;
            }

            var sprite = Sprite.Create(
                iconTexture,
                new Rect(0, 0, iconTexture.width, iconTexture.height),
                new Vector2(0.5f, 0.5f)
            );

            return sprite;
        }

        /// <summary>
        /// UIパターンの適用
        /// </summary>
        public void ImageSettingApply(UI_CHANGE_TYPE uiSetting) {
            AssetDatabase.StartAssetEditing();
            
            // 各使用パラメータ
            string UI_PATH = "Assets/RPGMaker/Storage/Images/Ui/";
            string SYSTEM_UI_PATH = "Assets/RPGMaker/Storage/System/Images/Ui/";

            UiPatternParameter param = new UiPatternParameter();
            param.uiPatternId = 0;
            param.bgSprite =
                UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(UI_PATH + "Bg/" + _uiSettingDataModel.commonMenus[param.uiPatternId].backgroundImage.image + ".png");
            param.bgImageSprite =
                UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(UI_PATH + "Window/" + _uiSettingDataModel.commonMenus[param.uiPatternId].windowBackgroundImage.image + ".png");
            param.frImageSprite =
                UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(UI_PATH + "Window/" + _uiSettingDataModel.commonMenus[param.uiPatternId].windowFrameImage.image + ".png");
            param.buttonImageSprite =
                UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(UI_PATH + "Button/" + _uiSettingDataModel.commonMenus[param.uiPatternId].buttonImage.image + ".png");
            param.buttonFrameImageSprite =
                UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(UI_PATH + "Button/" + _uiSettingDataModel.commonMenus[param.uiPatternId].buttonFrameImage.image + ".png");
            param.planeImageSprite =
                UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(SYSTEM_UI_PATH + "UI_system_frame.png");
            param.planeBgSprite =
                UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(SYSTEM_UI_PATH + "UI_system_bg.png");

            // WindowBase継承クラスを取得
            param.skillTypes = Assembly
                .GetAssembly(typeof(WindowBase))
                    .GetTypes()
                    .Where(t => {
                        return t.IsSubclassOf(typeof(WindowBase)) && !t.IsAbstract;
                    }).ToList();

            param.backgroundImageColor = new Color();
            param.backgroundImageColor.r = _uiSettingDataModel.commonMenus[param.uiPatternId].backgroundImage.color[0] / 255f;
            param.backgroundImageColor.g = _uiSettingDataModel.commonMenus[param.uiPatternId].backgroundImage.color[1] / 255f;
            param.backgroundImageColor.b = _uiSettingDataModel.commonMenus[param.uiPatternId].backgroundImage.color[2] / 255f;
            param.backgroundImageColor.a = _uiSettingDataModel.commonMenus[param.uiPatternId].backgroundImage.color[3] / 255f;

            param.windowBackgroundImageColor = new Color();
            param.windowBackgroundImageColor.r = _uiSettingDataModel.commonMenus[param.uiPatternId].windowBackgroundImage.color[0] / 255f;
            param.windowBackgroundImageColor.g = _uiSettingDataModel.commonMenus[param.uiPatternId].windowBackgroundImage.color[1] / 255f;
            param.windowBackgroundImageColor.b = _uiSettingDataModel.commonMenus[param.uiPatternId].windowBackgroundImage.color[2] / 255f;
            param.windowBackgroundImageColor.a = _uiSettingDataModel.commonMenus[param.uiPatternId].windowBackgroundImage.color[3] / 255f;

            param.windowFrameImageColor = new Color();
            param.windowFrameImageColor.r = _uiSettingDataModel.commonMenus[param.uiPatternId].windowFrameImage.color[0] / 255f;
            param.windowFrameImageColor.g = _uiSettingDataModel.commonMenus[param.uiPatternId].windowFrameImage.color[1] / 255f;
            param.windowFrameImageColor.b = _uiSettingDataModel.commonMenus[param.uiPatternId].windowFrameImage.color[2] / 255f;
            param.windowFrameImageColor.a = _uiSettingDataModel.commonMenus[param.uiPatternId].windowFrameImage.color[3] / 255f;

            param.windowFrameImageHighlightColor = new Color();
            param.windowFrameImageHighlightColor.r = _uiSettingDataModel.commonMenus[param.uiPatternId].windowFrameImageHighlight[0] / 255f;
            param.windowFrameImageHighlightColor.g = _uiSettingDataModel.commonMenus[param.uiPatternId].windowFrameImageHighlight[1] / 255f;
            param.windowFrameImageHighlightColor.b = _uiSettingDataModel.commonMenus[param.uiPatternId].windowFrameImageHighlight[2] / 255f;
            param.windowFrameImageHighlightColor.a = _uiSettingDataModel.commonMenus[param.uiPatternId].windowFrameImageHighlight[3] / 255f;

            param.buttonImageColor = new Color();
            param.buttonImageColor.r = _uiSettingDataModel.commonMenus[param.uiPatternId].buttonImage.color[0] / 255f;
            param.buttonImageColor.g = _uiSettingDataModel.commonMenus[param.uiPatternId].buttonImage.color[1] / 255f;
            param.buttonImageColor.b = _uiSettingDataModel.commonMenus[param.uiPatternId].buttonImage.color[2] / 255f;
            param.buttonImageColor.a = _uiSettingDataModel.commonMenus[param.uiPatternId].buttonImage.color[3] / 255f;

            param.buttonFrameImageColor = new Color();
            param.buttonFrameImageColor.r = _uiSettingDataModel.commonMenus[param.uiPatternId].buttonFrameImage.color[0] / 255f;
            param.buttonFrameImageColor.g = _uiSettingDataModel.commonMenus[param.uiPatternId].buttonFrameImage.color[1] / 255f;
            param.buttonFrameImageColor.b = _uiSettingDataModel.commonMenus[param.uiPatternId].buttonFrameImage.color[2] / 255f;
            param.buttonFrameImageColor.a = _uiSettingDataModel.commonMenus[param.uiPatternId].buttonFrameImage.color[3] / 255f;

            param.buttonImageHighlightColor = new Color();
            param.buttonImageHighlightColor.r = _uiSettingDataModel.commonMenus[param.uiPatternId].buttonImageHighlight[0] / 255f;
            param.buttonImageHighlightColor.g = _uiSettingDataModel.commonMenus[param.uiPatternId].buttonImageHighlight[1] / 255f;
            param.buttonImageHighlightColor.b = _uiSettingDataModel.commonMenus[param.uiPatternId].buttonImageHighlight[2] / 255f;
            param.buttonImageHighlightColor.a = _uiSettingDataModel.commonMenus[param.uiPatternId].buttonImageHighlight[3] / 255f;


            //メニューへの適用
            ImageSettingApplyMenu(param, uiSetting);
            //セーブメニューへの適用
            ImageSettingApplySaveMenu(param, uiSetting);
            //バトルメニューへの適用
            ImageSettingApplyBattleMenu(param, uiSetting);
            //タイトルメニューへの適用
            ImageSettingApplyTitleMenu(param, uiSetting);
            //ショップへの適用
            ImageSettingApplyShop(param, uiSetting);
            //ショップアイテムへの適用
            ImageSettingApplyShopItem(param, uiSetting);
            //名前入力への適用
            ImageSettingApplyInputName(param, uiSetting);
            //数値入力への適用
            ImageSettingApplyInputNum(param, uiSetting);
            //アイテム選択への適用
            ImageSettingApplySelectItem(param, uiSetting);
            //メッセージ表示への適用
            ImageSettingApplyMessage(param, uiSetting);
            //選択肢への適用
            ImageSettingApplyInputSelect(param, uiSetting);

            //フォント適用
            FontSetting(uiSetting);

            AssetDatabase.StopAssetEditing();
        }

        /// <summary>
        /// 背景設定
        /// </summary>
        /// <param name="param"></param>
        /// <param name="img"></param>
        private void ImageSettingBackground(UiPatternParameter param, Image img) {
            switch (_uiSettingDataModel.commonMenus[param.uiPatternId].windowBackgroundImage.type)
            {
                //色
                case 0:
                    img.color = param.windowBackgroundImageColor;
                    img.sprite = param.planeBgSprite;
                    break;
                //画像
                case 1:
                    img.color = new Color32(255, 255, 255, 255);
                    img.sprite = param.bgImageSprite;
                    img.type = Image.Type.Tiled;
                    break;
            }
        }

        /// <summary>
        /// フレーム設定
        /// </summary>
        /// <param name="param"></param>
        /// <param name="img"></param>
        private void ImageSettingFrame(UiPatternParameter param, Image img) {
            switch (_uiSettingDataModel.commonMenus[param.uiPatternId].windowFrameImage.type)
            {
                //色
                case 0:
                    img.color = param.windowFrameImageColor;
                    img.sprite = param.planeImageSprite;
                    break;
                //画像
                case 1:
                    img.color = new Color32(255, 255, 255, 255);
                    img.sprite = param.frImageSprite;
                    break;
            }
        }

        /// <summary>
        /// ハイライト設定
        /// </summary>
        /// <param name="param"></param>
        /// <param name="img"></param>
        private void ImageSettingHighlight(UiPatternParameter param, Image frame, Image highlight, int type) {
            if (highlight != null)
            {
                highlight.color = param.windowFrameImageHighlightColor;
                highlight.sprite = null;
                highlight.type = Image.Type.Tiled;
            }
            else if (type == 1)
            {
                var highlightObj = Object.Instantiate(frame);
                highlightObj.name = "Highlight";
                highlightObj.transform.SetParent(frame.transform);
                highlightObj.GetComponent<RectTransform>().anchoredPosition3D = frame.GetComponent<RectTransform>().anchoredPosition3D;
                highlightObj.GetComponent<RectTransform>().sizeDelta = frame.GetComponent<RectTransform>().sizeDelta;
                highlightObj.GetComponent<RectTransform>().pivot = frame.GetComponent<RectTransform>().pivot;
                highlightObj.GetComponent<RectTransform>().localScale = frame.GetComponent<RectTransform>().localScale;
                highlightObj.color = param.windowFrameImageHighlightColor;
                highlightObj.sprite = null;
                highlightObj.type = Image.Type.Tiled;

            }
        }

        /// <summary>
        /// ボタン背景設定
        /// </summary>
        /// <param name="param"></param>
        /// <param name="img"></param>
        private void ImageSettingButtonBackground(UiPatternParameter param, GameObject obj, Image img) {
            if (img != null)
            {
                if ((obj.name != "Up" && obj.name != "Down"))
                {
                    switch (_uiSettingDataModel.commonMenus[param.uiPatternId].buttonImage.type)
                    {
                        //色
                        case 0:
                            img.color = param.buttonImageColor;
                            img.sprite = param.planeBgSprite;
                            break;
                        //画像
                        case 1:
                            img.color = new Color32(255, 255, 255, 255);
                            img.sprite = param.buttonImageSprite;
                            img.type = Image.Type.Tiled;
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// ボタンフレーム設定
        /// </summary>
        /// <param name="param"></param>
        /// <param name="img"></param>
        private void ImageSettingButtonFrame(UiPatternParameter param, Image img) {
            if (img == null) return;

            switch (_uiSettingDataModel.commonMenus[param.uiPatternId].buttonFrameImage.type)
            {
                //色
                case 0:
                    img.color = param.buttonFrameImageColor;
                    img.sprite = param.planeImageSprite;
                    break;
                //画像
                case 1:
                    img.color = new Color32(255, 255, 255, 255);
                    img.sprite = param.buttonFrameImageSprite;
                    break;
            }
        }

        /// <summary>
        /// ボタンハイライト設定
        /// </summary>
        /// <param name="param"></param>
        /// <param name="img"></param>
        private void ImageSettingButtonHighlight(UiPatternParameter param, Image highlight) {
            if (highlight != null)
            {
                highlight.type = Image.Type.Tiled;
                highlight.color = param.buttonImageHighlightColor;
                highlight.sprite = null;
            }
        }

        /// <summary>
        /// UIパターンの適用 メニュー
        /// </summary>
        private void ImageSettingApplyMenu(UiPatternParameter param, UI_CHANGE_TYPE uiSetting) {
            if (uiSetting == UI_CHANGE_TYPE.FONT ||
                uiSetting == UI_CHANGE_TYPE.FONT_SIZE ||
                uiSetting == UI_CHANGE_TYPE.FONT_COLOR)
                return;

            var backgroundColor = _menuCanvas.transform.Find("UICanvas/BgColor").GetComponent<Image>();
            backgroundColor.color = param.backgroundImageColor;
            backgroundColor.sprite = null;

            switch (uiSetting)
            {
                case UI_CHANGE_TYPE.PATTERN:
                    UiSetting();
                    UiButtonSetting();
                    break;
                case UI_CHANGE_TYPE.BG_COLOR:
                case UI_CHANGE_TYPE.BG_IMAGE:
                case UI_CHANGE_TYPE.WINDOW_COLOR:
                case UI_CHANGE_TYPE.WINDOW_IMAGE:
                case UI_CHANGE_TYPE.FRAME_COLOR:
                case UI_CHANGE_TYPE.FRAME_IMAGE:
                case UI_CHANGE_TYPE.FRAME_HIGHLIGHT:
                    UiSetting();
                    break;
                case UI_CHANGE_TYPE.BUTTON_COLOR:
                case UI_CHANGE_TYPE.BUTTON_IMAGE:
                case UI_CHANGE_TYPE.BUTTON_HIGHLIGHT:
                case UI_CHANGE_TYPE.BUTTON_FRAME_COLOR:
                case UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE:
                    UiButtonSetting();
                    break;
            }

            Save();

            void UiSetting() {
                if (uiSetting == UI_CHANGE_TYPE.BG_COLOR ||
                    uiSetting == UI_CHANGE_TYPE.BG_IMAGE ||
                    uiSetting == UI_CHANGE_TYPE.PATTERN)
                {
                    var backgroundImage = _menuCanvas.transform.Find("UICanvas/BgImage").GetComponent<Image>();
                    if (_uiSettingDataModel.commonMenus[param.uiPatternId].backgroundImage.type == 1)
                    {
                        backgroundImage.color = new Color32(255, 255, 255, 255);
                        backgroundImage.sprite = param.bgSprite;
                        backgroundImage.type = Image.Type.Tiled;
                    }
                    else
                    {
                        backgroundImage.color = new Color32(255, 255, 255, 0);
                    }
                }

                if (uiSetting == UI_CHANGE_TYPE.WINDOW_COLOR || 
                    uiSetting == UI_CHANGE_TYPE.WINDOW_IMAGE || 
                    uiSetting == UI_CHANGE_TYPE.PATTERN ||
                    uiSetting == UI_CHANGE_TYPE.FRAME_COLOR || 
                    uiSetting == UI_CHANGE_TYPE.FRAME_IMAGE ||
                    uiSetting == UI_CHANGE_TYPE.FRAME_HIGHLIGHT)
                {
                    // 取得したクラスが付いているオブジェクトをprefabから取得
                    for (int i = 0; i < param.skillTypes.Count(); i++)
                    {
                        // gameobjectを取得
                        var objects = _menuCanvas.transform.GetComponentsInChildren(param.skillTypes[i], true);
                        if (objects == null) continue;
                        if (objects.Length <= 0) continue;

                        // 画像適用先object取得
                        for (int i2 = 0; i2 < objects.Length; i2++)
                        {
                            var commonObejects = (objects[i2] as WindowBase).commonWindowObject;
                            if (commonObejects.Length <= 0) continue;

                            // 画像適用処理
                            for (int i3 = 0; i3 < commonObejects.Length; i3++)
                            {
                                if (uiSetting == UI_CHANGE_TYPE.WINDOW_COLOR || uiSetting == UI_CHANGE_TYPE.WINDOW_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                                {
                                    // 背景
                                    var background = commonObejects[i3].transform.Find("Image").GetComponent<Image>();
                                    ImageSettingBackground(param, background);
                                }
                                var frame = commonObejects[i3].transform.Find("Frame").GetComponent<Image>();
                                if (uiSetting == UI_CHANGE_TYPE.FRAME_COLOR || uiSetting == UI_CHANGE_TYPE.FRAME_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                                {
                                    // フレーム
                                    frame.type = Image.Type.Tiled;
                                    ImageSettingFrame(param, frame);
                                }
                                if (uiSetting == UI_CHANGE_TYPE.FRAME_HIGHLIGHT || uiSetting == UI_CHANGE_TYPE.PATTERN)
                                {
                                    // ハイライト
                                    var highlight = commonObejects[i3].transform.Find("Frame/Highlight")?.GetComponent<Image>();
                                    ImageSettingHighlight(param, frame, highlight, 1);
                                }
                            }
                        }
                    }
                }
            }

            // ボタン
            void UiButtonSetting() {
                // gameobjectを取得
                var objects = _menuCanvas.transform.GetComponentsInChildren(typeof(WindowButtonBase), true);

                // 画像適用先object取得
                for(int i2 = 0; i2 < objects?.Length; i2++)
                {
                    var commonObejects = (objects[i2] as WindowButtonBase).gameObject;
                    if(commonObejects == null) continue;

                    // 画像適用処理
                    {
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_COLOR || uiSetting == UI_CHANGE_TYPE.BUTTON_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // 背景
                            var background = commonObejects.GetComponent<Image>();
                            ImageSettingButtonBackground(param, commonObejects, background);
                        }
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_FRAME_COLOR || uiSetting == UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // フレーム
                            var frame = commonObejects.transform.Find("Image")?.GetComponent<Image>();
                            if (frame != null)
                            {
                                frame.type = Image.Type.Tiled;
                                ImageSettingButtonFrame(param, frame);
                            }                        
                        }
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_HIGHLIGHT || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // ハイライト
                            var highlight = commonObejects.transform.Find("Highlight")?.GetComponent<Image>();
                            ImageSettingButtonHighlight(param, highlight);
                        }
                    }
                }
            }        
        }

        /// <summary>
        /// UIパターンの適用 セーブメニュー
        /// </summary>
        private void ImageSettingApplySaveMenu(UiPatternParameter param, UI_CHANGE_TYPE uiSetting) {
            if (uiSetting == UI_CHANGE_TYPE.FONT ||
                uiSetting == UI_CHANGE_TYPE.FONT_SIZE ||
                uiSetting == UI_CHANGE_TYPE.FONT_COLOR ||
                uiSetting == UI_CHANGE_TYPE.BG_COLOR ||
                uiSetting == UI_CHANGE_TYPE.BG_IMAGE ||
                uiSetting == UI_CHANGE_TYPE.WINDOW_COLOR ||
                uiSetting == UI_CHANGE_TYPE.WINDOW_IMAGE ||
                uiSetting == UI_CHANGE_TYPE.FRAME_COLOR ||
                uiSetting == UI_CHANGE_TYPE.FRAME_IMAGE ||
                uiSetting == UI_CHANGE_TYPE.FRAME_HIGHLIGHT)
                return;

            var itemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RPGMaker/Codebase/Runtime/Title/SaveItem.prefab");
            var itemData = Object.Instantiate(itemPrefab);

            switch (uiSetting)
            {
                case UI_CHANGE_TYPE.PATTERN:
                    UiButtonSetting();
                    break;
                case UI_CHANGE_TYPE.BUTTON_COLOR:
                case UI_CHANGE_TYPE.BUTTON_IMAGE:
                case UI_CHANGE_TYPE.BUTTON_HIGHLIGHT:
                case UI_CHANGE_TYPE.BUTTON_FRAME_COLOR:
                case UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE:
                    UiButtonSetting();
                    break;
            }

            // 保存
            PrefabUtility.SaveAsPrefabAsset(itemData, "Assets/RPGMaker/Codebase/Runtime/Title/SaveItem.prefab");
            Object.DestroyImmediate(itemData);

            // ボタン
            void UiButtonSetting() {
                // gameobjectを取得
                var objects = itemData.transform.GetComponentsInChildren(typeof(WindowButtonBase), true);

                // 画像適用先object取得
                for (int i2 = 0; i2 < objects?.Length; i2++)
                {
                    var commonObejects = (objects[i2] as WindowButtonBase);
                    if (commonObejects == null) continue;

                    // 画像適用処理
                    {
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_COLOR || uiSetting == UI_CHANGE_TYPE.BUTTON_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // 背景
                            var background = commonObejects.GetComponent<Image>();
                            ImageSettingButtonBackground(param, commonObejects.gameObject, background);
                        }
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_FRAME_COLOR || uiSetting == UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // フレーム
                            if (commonObejects.transform.Find("Image") != null)
                            {
                                var frame = commonObejects.transform.Find("Image").GetComponent<Image>();
                                if (frame.GetComponent<Animator>() != null)
                                {
                                    Object.DestroyImmediate(frame.GetComponent<Animator>());
                                }
                                frame.type = Image.Type.Tiled;
                                ImageSettingButtonFrame(param, frame);
                            }                        
                        }
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_HIGHLIGHT || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // ハイライト
                            if (commonObejects.transform.Find("Highlight") != null)
                            {
                                var highlight = commonObejects.transform.Find("Highlight")?.GetComponent<Image>();
                                if (highlight.GetComponent<Animator>() != null)
                                {
                                    Object.DestroyImmediate(highlight.GetComponent<Animator>());
                                }
                                ImageSettingButtonHighlight(param, highlight);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// UIパターンの適用 バトルメニュー
        /// </summary>
        private void ImageSettingApplyBattleMenu(UiPatternParameter param, UI_CHANGE_TYPE uiSetting) {
            if (uiSetting == UI_CHANGE_TYPE.FONT ||
                uiSetting == UI_CHANGE_TYPE.FONT_SIZE ||
                uiSetting == UI_CHANGE_TYPE.FONT_COLOR)
                return;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RPGMaker/Codebase/Runtime/Battle/Windows" + "0" + (int.Parse(_systemSettingDataModel.uiPatternId) + 1) + ".prefab");
            var battleMenu = Object.Instantiate(prefab);

            switch (uiSetting)
            {
                case UI_CHANGE_TYPE.PATTERN:
                    UiSetting();
                    UiButtonSetting();
                    break;
                case UI_CHANGE_TYPE.BG_COLOR:
                case UI_CHANGE_TYPE.BG_IMAGE:
                case UI_CHANGE_TYPE.WINDOW_COLOR:
                case UI_CHANGE_TYPE.WINDOW_IMAGE:
                case UI_CHANGE_TYPE.FRAME_COLOR:
                case UI_CHANGE_TYPE.FRAME_IMAGE:
                case UI_CHANGE_TYPE.FRAME_HIGHLIGHT:
                    UiSetting();
                    break;
                case UI_CHANGE_TYPE.BUTTON_COLOR:
                case UI_CHANGE_TYPE.BUTTON_IMAGE:
                case UI_CHANGE_TYPE.BUTTON_HIGHLIGHT:
                case UI_CHANGE_TYPE.BUTTON_FRAME_COLOR:
                case UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE:
                    UiButtonSetting();
                    break;
            }

            // 保存
            PrefabUtility.SaveAsPrefabAsset(battleMenu, "Assets/RPGMaker/Codebase/Runtime/Battle/Windows" + "0" + (int.Parse(_systemSettingDataModel.uiPatternId) + 1) + ".prefab");
            Object.DestroyImmediate(battleMenu);

            void UiSetting() {
                // 取得したクラスが付いているオブジェクトをprefabから取得
                for (int i = 0; i < param.skillTypes.Count(); i++)
                {
                    // gameobjectを取得
                    var objects = battleMenu.transform.GetComponentsInChildren(param.skillTypes[i], true);
                    if (objects == null) continue;
                    if (objects.Length <= 0) continue;

                    // 画像適用先object取得
                    for (int i2 = 0; i2 < objects.Length; i2++)
                    {
                        var commonObejects = (objects[i2] as WindowBase).commonWindowObject;
                        if (commonObejects.Length <= 0) continue;

                        // 画像適用処理
                        for (int i3 = 0; i3 < commonObejects.Length; i3++)
                        {
                            if (uiSetting == UI_CHANGE_TYPE.WINDOW_COLOR || uiSetting == UI_CHANGE_TYPE.WINDOW_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                            {
                                // 背景
                                var background = commonObejects[i3].transform.Find("Image").GetComponent<Image>();
                                ImageSettingBackground(param, background);
                            }
                            var frame = commonObejects[i3].transform.Find("Frame").GetComponent<Image>();
                            if (uiSetting == UI_CHANGE_TYPE.FRAME_COLOR || uiSetting == UI_CHANGE_TYPE.FRAME_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                            {
                                // フレーム
                                frame.type = Image.Type.Tiled;
                                ImageSettingFrame(param, frame);
                            }
                            if (uiSetting == UI_CHANGE_TYPE.FRAME_HIGHLIGHT || uiSetting == UI_CHANGE_TYPE.PATTERN)
                            {
                                // ハイライト
                                var highlight = commonObejects[i3].transform.Find("Highlight")?.GetComponent<Image>();
                                ImageSettingHighlight(param, frame, highlight, 0);
                            }
                        }
                    }
                }
            }

            // ボタン
            void UiButtonSetting() {
                // gameobjectを取得
                var objects = battleMenu.transform.GetComponentsInChildren(typeof(WindowButtonBase), true);

                // 画像適用先object取得
                for (int i2 = 0; i2 < objects?.Length; i2++)
                {
                    var commonObejects = (objects[i2] as WindowButtonBase);
                    if (commonObejects == null) continue;

                    // 画像適用処理
                    {
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_COLOR || uiSetting == UI_CHANGE_TYPE.BUTTON_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // 背景
                            var background = commonObejects.GetComponent<Image>();
                            ImageSettingButtonBackground(param, commonObejects.gameObject, background);
                        }
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_FRAME_COLOR || uiSetting == UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // フレーム
                            var frame = commonObejects.transform.Find("Image").GetComponent<Image>();
                            frame.type = Image.Type.Tiled;
                            ImageSettingButtonFrame(param, frame);
                        }
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_HIGHLIGHT || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // ハイライト
                            var highlight = commonObejects.transform.Find("Highlight")?.GetComponent<Image>();
                            ImageSettingButtonHighlight(param, highlight);
                        }
                    }
                }
            }            
        }

        /// <summary>
        /// UIパターンの適用 タイトルメニュー
        /// </summary>
        private void ImageSettingApplyTitleMenu(UiPatternParameter param, UI_CHANGE_TYPE uiSetting) {
            if (uiSetting == UI_CHANGE_TYPE.FONT ||
                uiSetting == UI_CHANGE_TYPE.FONT_SIZE ||
                uiSetting == UI_CHANGE_TYPE.FONT_COLOR)
                return;

            var titlePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RPGMaker/Codebase/Runtime/Title/TitleCanvas.prefab");
            var titleMenu = Object.Instantiate(titlePrefab);

            switch (uiSetting)
            {
                case UI_CHANGE_TYPE.PATTERN:
                    UiSetting();
                    UiButtonSetting();
                    break;
                case UI_CHANGE_TYPE.BG_COLOR:
                case UI_CHANGE_TYPE.BG_IMAGE:
                case UI_CHANGE_TYPE.WINDOW_COLOR:
                case UI_CHANGE_TYPE.WINDOW_IMAGE:
                case UI_CHANGE_TYPE.FRAME_COLOR:
                case UI_CHANGE_TYPE.FRAME_IMAGE:
                case UI_CHANGE_TYPE.FRAME_HIGHLIGHT:
                    UiSetting();
                    break;
                case UI_CHANGE_TYPE.BUTTON_COLOR:
                case UI_CHANGE_TYPE.BUTTON_IMAGE:
                case UI_CHANGE_TYPE.BUTTON_HIGHLIGHT:
                case UI_CHANGE_TYPE.BUTTON_FRAME_COLOR:
                case UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE:
                    UiButtonSetting();
                    break;
            }

            // 保存
            PrefabUtility.SaveAsPrefabAsset(titleMenu, "Assets/RPGMaker/Codebase/Runtime/Title/TitleCanvas.prefab");
            Object.DestroyImmediate(titleMenu);

            void UiSetting() {
                // 取得したクラスが付いているオブジェクトをprefabから取得
                for (int i = 0; i < param.skillTypes.Count(); i++)
                {
                    // gameobjectを取得
                    var objects = titleMenu.transform.GetComponentsInChildren(param.skillTypes[i], true);
                    if (objects == null) continue;
                    if (objects.Length <= 0) continue;

                    // 画像適用先object取得
                    for (int i2 = 0; i2 < objects.Length; i2++)
                    {
                        var commonObejects = (objects[i2] as WindowBase).commonWindowObject;
                        if (commonObejects.Length <= 0) continue;

                        // 画像適用処理
                        for (int i3 = 0; i3 < commonObejects.Length; i3++)
                        {
                            if (uiSetting == UI_CHANGE_TYPE.WINDOW_COLOR || uiSetting == UI_CHANGE_TYPE.WINDOW_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                            {
                                // 背景
                                var background = commonObejects[i3].transform.Find("Image").GetComponent<Image>();
                                ImageSettingBackground(param, background);
                            }
                            var frame = commonObejects[i3].transform.Find("Frame").GetComponent<Image>();
                            if (uiSetting == UI_CHANGE_TYPE.FRAME_COLOR || uiSetting == UI_CHANGE_TYPE.FRAME_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                            {
                                // フレーム
                                frame.type = Image.Type.Tiled;
                                ImageSettingFrame(param, frame);
                            }
                            if (uiSetting == UI_CHANGE_TYPE.FRAME_HIGHLIGHT || uiSetting == UI_CHANGE_TYPE.PATTERN)
                            {
                                // ハイライト
                                var highlight = commonObejects[i3].transform.Find("Highlight")?.GetComponent<Image>();
                                ImageSettingHighlight(param, frame, highlight, 0);
                            }
                        }
                    }
                }
            }

            // ボタン
            void UiButtonSetting() {
                // gameobjectを取得
                var objects = titleMenu.transform.GetComponentsInChildren(typeof(WindowButtonBase), true);

                // 画像適用先object取得
                for (int i2 = 0; i2 < objects?.Length; i2++)
                {
                    var commonObject = (objects[i2] as WindowButtonBase);
                    if (commonObject == null) continue;

                    // 画像適用処理
                    {
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_COLOR || uiSetting == UI_CHANGE_TYPE.BUTTON_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // 背景
                            var background = commonObject.GetComponent<Image>();
                            ImageSettingButtonBackground(param, commonObject.gameObject, background);
                        }
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_FRAME_COLOR || uiSetting == UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // フレーム
                            var frame = commonObject.transform.Find("Image").GetComponent<Image>();
                            frame.type = Image.Type.Tiled;
                            ImageSettingButtonFrame(param, frame);
                        }
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_HIGHLIGHT || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // ハイライト
                            var highlight = commonObject.transform.Find("Highlight")?.GetComponent<Image>();
                            ImageSettingButtonHighlight(param, highlight);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// UIパターンの適用 ショップ
        /// </summary>
        private void ImageSettingApplyShop(UiPatternParameter param, UI_CHANGE_TYPE uiSetting) {
            if (uiSetting == UI_CHANGE_TYPE.FONT ||
                uiSetting == UI_CHANGE_TYPE.FONT_SIZE ||
                uiSetting == UI_CHANGE_TYPE.FONT_COLOR)
                return;

            var shopPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RPGMaker/Codebase/Runtime/Map/Shop/Asset/Prefab/ItemShopCanvas.prefab");
            var shopMenu = Object.Instantiate(shopPrefab);

            switch (uiSetting)
            {
                case UI_CHANGE_TYPE.PATTERN:
                    UiSetting();
                    UiButtonSetting();
                    break;
                case UI_CHANGE_TYPE.BG_COLOR:
                case UI_CHANGE_TYPE.BG_IMAGE:
                case UI_CHANGE_TYPE.WINDOW_COLOR:
                case UI_CHANGE_TYPE.WINDOW_IMAGE:
                case UI_CHANGE_TYPE.FRAME_COLOR:
                case UI_CHANGE_TYPE.FRAME_IMAGE:
                case UI_CHANGE_TYPE.FRAME_HIGHLIGHT:
                    UiSetting();
                    break;
                case UI_CHANGE_TYPE.BUTTON_COLOR:
                case UI_CHANGE_TYPE.BUTTON_IMAGE:
                case UI_CHANGE_TYPE.BUTTON_HIGHLIGHT:
                case UI_CHANGE_TYPE.BUTTON_FRAME_COLOR:
                case UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE:
                    UiButtonSetting();
                    break;
            }

            // 保存
            PrefabUtility.SaveAsPrefabAsset(shopMenu, "Assets/RPGMaker/Codebase/Runtime/Map/Shop/Asset/Prefab/ItemShopCanvas.prefab");
            Object.DestroyImmediate(shopMenu);

            void UiSetting() {
                // 取得したクラスが付いているオブジェクトをprefabから取得
                for (int i = 0; i < param.skillTypes.Count(); i++)
                {
                    // gameobjectを取得
                    var objects = shopMenu.transform.GetComponentsInChildren(param.skillTypes[i], true);
                    if (objects == null) continue;
                    if (objects.Length <= 0) continue;

                    // 画像適用先object取得
                    for (int i2 = 0; i2 < objects.Length; i2++)
                    {
                        var commonObejects = (objects[i2] as WindowBase).commonWindowObject;
                        if (commonObejects.Length <= 0) continue;

                        // 画像適用処理
                        for (int i3 = 0; i3 < commonObejects.Length; i3++)
                        {
                            if (uiSetting == UI_CHANGE_TYPE.WINDOW_COLOR || uiSetting == UI_CHANGE_TYPE.WINDOW_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                            {
                                // 背景
                                if (commonObejects[i3].transform.Find("Image") != null)
                                {
                                    var background = commonObejects[i3].transform.Find("Image").GetComponent<Image>();
                                    if (background != null)
                                    {
                                        ImageSettingBackground(param, background);
                                    }
                                }                            
                            }
                            if (uiSetting == UI_CHANGE_TYPE.FRAME_COLOR || uiSetting == UI_CHANGE_TYPE.FRAME_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                            {
                                // フレーム
                                if (commonObejects[i3].transform.Find("Frame") != null)
                                {
                                    var frame = commonObejects[i3].transform.Find("Frame").GetComponent<Image>();
                                    frame.type = Image.Type.Tiled;
                                    ImageSettingFrame(param, frame);
                                }                            
                            }
                            if (uiSetting == UI_CHANGE_TYPE.FRAME_HIGHLIGHT || uiSetting == UI_CHANGE_TYPE.PATTERN)
                            {
                                // ハイライト
                                if (commonObejects[i3].transform.Find("Highlight") != null)
                                {
                                    var highlight = commonObejects[i3].transform.Find("Highlight")?.GetComponent<Image>();
                                    ImageSettingHighlight(param, null, highlight, 0);
                                }                            
                            }
                        }
                    }
                }
            }

            // ボタン
            void UiButtonSetting() {
                // gameobjectを取得
                var objects = shopMenu.transform.GetComponentsInChildren(typeof(WindowButtonBase), true);

                // 画像適用先object取得
                for (int i2 = 0; i2 < objects?.Length; i2++)
                {
                    var commonObejects = (objects[i2] as WindowButtonBase);
                    if (commonObejects == null) continue;

                    // 画像適用処理
                    {
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_COLOR || uiSetting == UI_CHANGE_TYPE.BUTTON_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // 背景
                            var background = commonObejects.GetComponent<Image>();
                            ImageSettingButtonBackground(param, commonObejects.gameObject, background);
                        }
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_FRAME_COLOR || uiSetting == UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // フレーム
                            if (commonObejects.transform.Find("Image") != null)
                            {
                                var frame = commonObejects.transform.Find("Image").GetComponent<Image>();
                                if (frame.GetComponent<Animator>() != null)
                                {
                                    Object.DestroyImmediate(frame.GetComponent<Animator>());
                                }
                                frame.type = Image.Type.Tiled;
                                ImageSettingButtonFrame(param, frame);
                            }                        
                        }
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_HIGHLIGHT || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // ハイライト
                            if (commonObejects.transform.Find("Highlight") != null)
                            {
                                var highlight = commonObejects.transform.Find("Highlight")?.GetComponent<Image>();
                                if (highlight.GetComponent<Animator>() != null)
                                {
                                    Object.DestroyImmediate(highlight.GetComponent<Animator>());
                                }
                                ImageSettingButtonHighlight(param, highlight);
                            }                        
                        }
                    }
                }
            }
        }

        /// <summary>
        /// UIパターンの適用 ショップアイテム
        /// </summary>
        private void ImageSettingApplyShopItem(UiPatternParameter param, UI_CHANGE_TYPE uiSetting) {
            if (uiSetting == UI_CHANGE_TYPE.FONT ||
                uiSetting == UI_CHANGE_TYPE.FONT_SIZE ||
                uiSetting == UI_CHANGE_TYPE.FONT_COLOR ||
                uiSetting == UI_CHANGE_TYPE.BG_COLOR ||
                uiSetting == UI_CHANGE_TYPE.BG_IMAGE ||
                uiSetting == UI_CHANGE_TYPE.WINDOW_COLOR ||
                uiSetting == UI_CHANGE_TYPE.WINDOW_IMAGE ||
                uiSetting == UI_CHANGE_TYPE.FRAME_COLOR ||
                uiSetting == UI_CHANGE_TYPE.FRAME_IMAGE ||
                uiSetting == UI_CHANGE_TYPE.FRAME_HIGHLIGHT)
                return;

            var itemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RPGMaker/Codebase/Runtime/Map/Shop/Asset/Prefab/ItemPrefab.prefab");
            var itemData = Object.Instantiate(itemPrefab);

            switch (uiSetting)
            {
                case UI_CHANGE_TYPE.PATTERN:
                    UiButtonSetting();
                    break;
                case UI_CHANGE_TYPE.BUTTON_COLOR:
                case UI_CHANGE_TYPE.BUTTON_IMAGE:
                case UI_CHANGE_TYPE.BUTTON_HIGHLIGHT:
                case UI_CHANGE_TYPE.BUTTON_FRAME_COLOR:
                case UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE:
                    UiButtonSetting();
                    break;
            }

            // 保存
            PrefabUtility.SaveAsPrefabAsset(itemData, "Assets/RPGMaker/Codebase/Runtime/Map/Shop/Asset/Prefab/ItemPrefab.prefab");
            Object.DestroyImmediate(itemData);

            // ボタン
            void UiButtonSetting() {
                // gameobjectを取得
                var objects = itemData.transform.GetComponentsInChildren(typeof(WindowButtonBase), true);

                // 画像適用先object取得
                for (int i2 = 0; i2 < objects?.Length; i2++)
                {
                    var commonObejects = (objects[i2] as WindowButtonBase);
                    if (commonObejects == null) continue;

                    // 画像適用処理
                    {
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_COLOR || uiSetting == UI_CHANGE_TYPE.BUTTON_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // 背景
                            var background = commonObejects.GetComponent<Image>();
                            ImageSettingButtonBackground(param, commonObejects.gameObject, background);
                        }
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_FRAME_COLOR || uiSetting == UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // フレーム
                            if (commonObejects.transform.Find("Image") != null)
                            {
                                var frame = commonObejects.transform.Find("Image").GetComponent<Image>();
                                if (frame.GetComponent<Animator>() != null)
                                {
                                    Object.DestroyImmediate(frame.GetComponent<Animator>());
                                }
                                frame.type = Image.Type.Tiled;
                                ImageSettingButtonFrame(param, frame);
                            }                        }
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_HIGHLIGHT || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // ハイライト
                            if (commonObejects.transform.Find("Highlight") != null)
                            {
                                var highlight = commonObejects.transform.Find("Highlight")?.GetComponent<Image>();
                                if (highlight.GetComponent<Animator>() != null)
                                {
                                    Object.DestroyImmediate(highlight.GetComponent<Animator>());
                                }
                                ImageSettingButtonHighlight(param, highlight);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// UIパターンの適用 名前入力
        /// </summary>
        private void ImageSettingApplyInputName(UiPatternParameter param, UI_CHANGE_TYPE uiSetting) {
            if (uiSetting == UI_CHANGE_TYPE.FONT ||
                uiSetting == UI_CHANGE_TYPE.FONT_SIZE ||
                uiSetting == UI_CHANGE_TYPE.FONT_COLOR)
                return;

            var inputNamePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/InputNameCanvas.prefab");
            var inputName = Object.Instantiate(inputNamePrefab);

            switch (uiSetting)
            {
                case UI_CHANGE_TYPE.PATTERN:
                    UiSetting();
                    UiButtonSetting();
                    break;
                case UI_CHANGE_TYPE.BG_COLOR:
                case UI_CHANGE_TYPE.BG_IMAGE:
                case UI_CHANGE_TYPE.WINDOW_COLOR:
                case UI_CHANGE_TYPE.WINDOW_IMAGE:
                case UI_CHANGE_TYPE.FRAME_COLOR:
                case UI_CHANGE_TYPE.FRAME_IMAGE:
                case UI_CHANGE_TYPE.FRAME_HIGHLIGHT:
                    UiSetting();
                    break;
                case UI_CHANGE_TYPE.BUTTON_COLOR:
                case UI_CHANGE_TYPE.BUTTON_IMAGE:
                case UI_CHANGE_TYPE.BUTTON_HIGHLIGHT:
                case UI_CHANGE_TYPE.BUTTON_FRAME_COLOR:
                case UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE:
                    UiButtonSetting();
                    break;
            }

            // 保存
            PrefabUtility.SaveAsPrefabAsset(inputName, "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/InputNameCanvas.prefab");
            Object.DestroyImmediate(inputName);

            void UiSetting() {
                // 取得したクラスが付いているオブジェクトをprefabから取得
                for (int i = 0; i < param.skillTypes.Count(); i++)
                {
                    // gameobjectを取得
                    var objects = inputName.transform.GetComponentsInChildren(param.skillTypes[i], true);
                    if (objects == null) continue;
                    if (objects.Length <= 0) continue;

                    // 画像適用先object取得
                    for (int i2 = 0; i2 < objects.Length; i2++)
                    {
                        var commonObject = (objects[i2] as WindowBase).commonWindowObject;
                        if (commonObject.Length <= 0) continue;

                        // 画像適用処理
                        for (int i3 = 0; i3 < commonObject.Length; i3++)
                        {
                            if (uiSetting == UI_CHANGE_TYPE.WINDOW_COLOR || uiSetting == UI_CHANGE_TYPE.WINDOW_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                            {
                                // 背景
                                if (commonObject[i3].transform.Find("Image") == null) continue;
                                var background = commonObject[i3].transform.Find("Image").GetComponent<Image>();
                                ImageSettingBackground(param, background);
                            }
                            var frame = commonObject[i3].transform.Find("Frame").GetComponent<Image>();
                            if (uiSetting == UI_CHANGE_TYPE.FRAME_COLOR || uiSetting == UI_CHANGE_TYPE.FRAME_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                            {
                                // フレーム
                                ImageSettingFrame(param, frame);
                            }
                            if (uiSetting == UI_CHANGE_TYPE.FRAME_HIGHLIGHT || uiSetting == UI_CHANGE_TYPE.PATTERN)
                            {
                                // ハイライト
                                var highlight = commonObject[i3].transform.Find("Highlight")?.GetComponent<Image>();
                                ImageSettingHighlight(param, frame, highlight, 0);
                            }
                        }
                    }
                }
            }

            // ボタン
            void UiButtonSetting() {
                // gameobjectを取得
                var objects = inputName.transform.GetComponentsInChildren(typeof(WindowButtonBase), true);

                // 画像適用先object取得
                for (int i2 = 0; i2 < objects?.Length; i2++)
                {
                    var commonObject = (objects[i2] as WindowButtonBase);
                    if (commonObject == null) continue;

                    // 画像適用処理
                    {
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_COLOR || uiSetting == UI_CHANGE_TYPE.BUTTON_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // 背景
                            var background = commonObject.GetComponent<Image>();
                            ImageSettingButtonBackground(param, commonObject.gameObject, background);
                        }
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_FRAME_COLOR || uiSetting == UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // フレーム
                            var frame = commonObject.transform.Find("Image")?.GetComponent<Image>();
                            if (frame != null) frame.type = Image.Type.Tiled;
                            ImageSettingButtonFrame(param, frame);
                        }
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_HIGHLIGHT || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // ハイライト
                            var highlight = commonObject.transform.Find("Highlight")?.GetComponent<Image>();
                            ImageSettingButtonHighlight(param, highlight);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// UIパターンの適用 数値入力
        /// </summary>
        private void ImageSettingApplyInputNum(UiPatternParameter param, UI_CHANGE_TYPE uiSetting) {
            if (uiSetting == UI_CHANGE_TYPE.FONT ||
                uiSetting == UI_CHANGE_TYPE.FONT_SIZE ||
                uiSetting == UI_CHANGE_TYPE.FONT_COLOR)
                return;

            var inputNumPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/MessageInputNum.prefab");
            var inputNum = Object.Instantiate(inputNumPrefab);

            switch (uiSetting)
            {
                case UI_CHANGE_TYPE.PATTERN:
                    UiSetting();
                    UiButtonSetting();
                    break;
                case UI_CHANGE_TYPE.BG_COLOR:
                case UI_CHANGE_TYPE.BG_IMAGE:
                case UI_CHANGE_TYPE.WINDOW_COLOR:
                case UI_CHANGE_TYPE.WINDOW_IMAGE:
                case UI_CHANGE_TYPE.FRAME_COLOR:
                case UI_CHANGE_TYPE.FRAME_IMAGE:
                case UI_CHANGE_TYPE.FRAME_HIGHLIGHT:
                    UiSetting();
                    break;
                case UI_CHANGE_TYPE.BUTTON_COLOR:
                case UI_CHANGE_TYPE.BUTTON_IMAGE:
                case UI_CHANGE_TYPE.BUTTON_HIGHLIGHT:
                case UI_CHANGE_TYPE.BUTTON_FRAME_COLOR:
                case UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE:
                    UiButtonSetting();
                    break;
            }

            // 保存
            PrefabUtility.SaveAsPrefabAsset(inputNum, "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/MessageInputNum.prefab");
            Object.DestroyImmediate(inputNum);

            void UiSetting() {
                var commonObjects = inputNum.GetComponent<WindowBase>().commonWindowObject;
                // 画像適用処理
                for (int i3 = 0; i3 < commonObjects.Length; i3++)
                {
                    if (uiSetting == UI_CHANGE_TYPE.WINDOW_COLOR || uiSetting == UI_CHANGE_TYPE.WINDOW_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                    {
                        // 背景
                        if (commonObjects[i3].transform.Find("Image") == null) continue;
                        var background = commonObjects[i3].transform.Find("Image").GetComponent<Image>();
                        ImageSettingBackground(param, background);
                    }
                    var frame = commonObjects[i3].transform.Find("Frame").GetComponent<Image>();
                    if (uiSetting == UI_CHANGE_TYPE.FRAME_COLOR || uiSetting == UI_CHANGE_TYPE.FRAME_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                    {
                        // フレーム
                        frame.type = Image.Type.Tiled;
                        ImageSettingFrame(param, frame);
                    }
                    if (uiSetting == UI_CHANGE_TYPE.FRAME_HIGHLIGHT || uiSetting == UI_CHANGE_TYPE.PATTERN)
                    {
                        // ハイライト
                        var highlight = commonObjects[i3].transform.Find("Highlight")?.GetComponent<Image>();
                        ImageSettingHighlight(param, frame, highlight, 0);
                    }
                }
            }

            // ボタン
            void UiButtonSetting() {
                // gameobjectを取得
                var objects = inputNum.transform.GetComponentsInChildren(typeof(WindowButtonBase), true);

                // 画像適用先object取得
                for (int i2 = 0; i2 < objects?.Length; i2++)
                {
                    var commonObject = (objects[i2] as WindowButtonBase);
                    if (commonObject == null) continue;

                    // 画像適用処理
                    {
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_COLOR || uiSetting == UI_CHANGE_TYPE.BUTTON_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // 背景
                            var background = commonObject.GetComponent<Image>();
                            ImageSettingButtonBackground(param, commonObject.gameObject, background);
                        }
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_FRAME_COLOR || uiSetting == UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // フレーム
                            var frame = commonObject.transform.Find("Image").GetComponent<Image>();
                            frame.type = Image.Type.Tiled;
                            ImageSettingButtonFrame(param, frame);
                        }
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_HIGHLIGHT || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // ハイライト
                            var highlight = commonObject.transform.Find("Highlight")?.GetComponent<Image>();
                            ImageSettingButtonHighlight(param, highlight);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// UIパターンの適用 アイテム選択
        /// </summary>
        private void ImageSettingApplySelectItem(UiPatternParameter param, UI_CHANGE_TYPE uiSetting) {
            if (uiSetting == UI_CHANGE_TYPE.FONT ||
                uiSetting == UI_CHANGE_TYPE.FONT_SIZE ||
                uiSetting == UI_CHANGE_TYPE.FONT_COLOR)
                return;

            var inputSelectItemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/MessageInputSelectItem.prefab");
            var inputSelectItem = Object.Instantiate(inputSelectItemPrefab);

            switch (uiSetting)
            {
                case UI_CHANGE_TYPE.PATTERN:
                    UiSetting();
                    UiButtonSetting();
                    break;
                case UI_CHANGE_TYPE.BG_COLOR:
                case UI_CHANGE_TYPE.BG_IMAGE:
                case UI_CHANGE_TYPE.WINDOW_COLOR:
                case UI_CHANGE_TYPE.WINDOW_IMAGE:
                case UI_CHANGE_TYPE.FRAME_COLOR:
                case UI_CHANGE_TYPE.FRAME_IMAGE:
                case UI_CHANGE_TYPE.FRAME_HIGHLIGHT:
                    UiSetting();
                    break;
                case UI_CHANGE_TYPE.BUTTON_COLOR:
                case UI_CHANGE_TYPE.BUTTON_IMAGE:
                case UI_CHANGE_TYPE.BUTTON_HIGHLIGHT:
                case UI_CHANGE_TYPE.BUTTON_FRAME_COLOR:
                case UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE:
                    UiButtonSetting();
                    break;
            }

            // 保存
            PrefabUtility.SaveAsPrefabAsset(inputSelectItem, "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/MessageInputSelectItem.prefab");
            Object.DestroyImmediate(inputSelectItem);

            void UiSetting() {
                var selectItemCommonObjects = inputSelectItem.GetComponent<WindowBase>().commonWindowObject;
                // 画像適用処理
                for (int i3 = 0; i3 < selectItemCommonObjects.Length; i3++)
                {
                    if (uiSetting == UI_CHANGE_TYPE.WINDOW_COLOR || uiSetting == UI_CHANGE_TYPE.WINDOW_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                    {
                        // 背景
                        if (selectItemCommonObjects[i3].transform.Find("Image") == null) continue;
                        var background = selectItemCommonObjects[i3].transform.Find("Image").GetComponent<Image>();
                        ImageSettingBackground(param, background);
                    }
                    var frame = selectItemCommonObjects[i3].transform.Find("Frame").GetComponent<Image>();
                    if (uiSetting == UI_CHANGE_TYPE.FRAME_COLOR || uiSetting == UI_CHANGE_TYPE.FRAME_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                    {
                        // フレーム
                        frame.type = Image.Type.Tiled;
                        ImageSettingFrame(param, frame);
                    }
                    if (uiSetting == UI_CHANGE_TYPE.FRAME_HIGHLIGHT || uiSetting == UI_CHANGE_TYPE.PATTERN)
                    {
                        // ハイライト
                        var highlight = selectItemCommonObjects[i3].transform.Find("Highlight")?.GetComponent<Image>();
                        ImageSettingHighlight(param, frame, highlight, 0);
                    }
                }
            }

            // ボタン
            void UiButtonSetting(){
                // gameobjectを取得
                var objects = inputSelectItem.transform.GetComponentsInChildren(typeof(WindowButtonBase), true);

                // 画像適用先object取得
                for (int i2 = 0; i2 < objects?.Length; i2++)
                {
                    var commonObject = (objects[i2] as WindowButtonBase);
                    if (commonObject == null) continue;

                    // 画像適用処理
                    {
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_COLOR || uiSetting == UI_CHANGE_TYPE.BUTTON_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // 背景
                            var background = commonObject.GetComponent<Image>();
                            ImageSettingButtonBackground(param, commonObject.gameObject, background);
                        }
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_FRAME_COLOR || uiSetting == UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // フレーム
                            var frame = commonObject.transform.Find("Image").GetComponent<Image>();
                            frame.type = Image.Type.Tiled;
                            ImageSettingButtonFrame(param, frame);
                        }
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_HIGHLIGHT || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // ハイライト
                            var highlight = commonObject.transform.Find("Highlight")?.GetComponent<Image>();
                            ImageSettingButtonHighlight(param, highlight);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// UIパターンの適用 メッセージ表示
        /// </summary>
        private void ImageSettingApplyMessage(UiPatternParameter param, UI_CHANGE_TYPE uiSetting) {
            if (uiSetting == UI_CHANGE_TYPE.FONT ||
                uiSetting == UI_CHANGE_TYPE.FONT_SIZE ||
                uiSetting == UI_CHANGE_TYPE.FONT_COLOR)
                return;

            var messageWindowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/MessageWindow.prefab");
            var messageWindow = Object.Instantiate(messageWindowPrefab);

            switch (uiSetting)
            {
                case UI_CHANGE_TYPE.PATTERN:
                    UiSetting();
                    UiButtonSetting();
                    break;
                case UI_CHANGE_TYPE.BG_COLOR:
                case UI_CHANGE_TYPE.BG_IMAGE:
                case UI_CHANGE_TYPE.WINDOW_COLOR:
                case UI_CHANGE_TYPE.WINDOW_IMAGE:
                case UI_CHANGE_TYPE.FRAME_COLOR:
                case UI_CHANGE_TYPE.FRAME_IMAGE:
                case UI_CHANGE_TYPE.FRAME_HIGHLIGHT:
                    UiSetting();
                    break;
                case UI_CHANGE_TYPE.BUTTON_COLOR:
                case UI_CHANGE_TYPE.BUTTON_IMAGE:
                case UI_CHANGE_TYPE.BUTTON_HIGHLIGHT:
                case UI_CHANGE_TYPE.BUTTON_FRAME_COLOR:
                case UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE:
                    UiButtonSetting();
                    break;
            }

            //テキスト下に子ができていたら、消す
            if (messageWindow.transform.Find("Canvas/DisplayArea/Image/NoIconText").GetComponents<ControlCharacter>()
                .Length > 0)
            {
                foreach (var controlCharacter in messageWindow.transform.Find("Canvas/DisplayArea/Image/NoIconText").GetComponents<ControlCharacter>())
                {
                    Object.DestroyImmediate(controlCharacter);
                }
            }
            if (messageWindow.transform.Find("Canvas/DisplayArea/Image/NoIconText").GetComponentsInChildren<Text>().Length > 0)
            {
                foreach (var transform in messageWindow.transform.Find("Canvas/DisplayArea/Image/NoIconText").GetComponentsInChildren<Text>())
                {
                    Object.DestroyImmediate(transform.gameObject);
                }
            }

            // 保存
            PrefabUtility.SaveAsPrefabAsset(messageWindow, "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/MessageWindow.prefab");
            Object.DestroyImmediate(messageWindow);

            void UiSetting() {
                var messageWindowCommonObjects = messageWindow.GetComponent<WindowBase>().commonWindowObject;
                // 画像適用処理
                for (int i3 = 0; i3 < messageWindowCommonObjects.Length; i3++)
                {
                    if (uiSetting == UI_CHANGE_TYPE.WINDOW_COLOR || uiSetting == UI_CHANGE_TYPE.WINDOW_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                    {
                        // 背景
                        if (messageWindowCommonObjects[i3].transform.Find("Image") == null) continue;
                        var background = messageWindowCommonObjects[i3].transform.Find("Image").GetComponent<Image>();
                        ImageSettingBackground(param, background);
                    }
                    var frame = messageWindowCommonObjects[i3].transform.Find("Frame").GetComponent<Image>();
                    if (uiSetting == UI_CHANGE_TYPE.FRAME_COLOR || uiSetting == UI_CHANGE_TYPE.FRAME_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                    {
                        // フレーム
                        frame.type = Image.Type.Tiled;
                        ImageSettingFrame(param, frame);
                    }
                    if (uiSetting == UI_CHANGE_TYPE.FRAME_HIGHLIGHT || uiSetting == UI_CHANGE_TYPE.PATTERN)
                    {
                        // ハイライト
                        var highlight = messageWindowCommonObjects[i3].transform.Find("Highlight")?.GetComponent<Image>();
                        ImageSettingHighlight(param, frame, highlight, 0);
                    }
                }
            }

            // ボタン
            void UiButtonSetting(){
                // gameobjectを取得
                var objects = messageWindow.transform.GetComponentsInChildren(typeof(WindowButtonBase), true);

                // 画像適用先object取得
                for (int i2 = 0; i2 < objects?.Length; i2++)
                {
                    var commonObject = (objects[i2] as WindowButtonBase);
                    if (commonObject == null) continue;

                    // 画像適用処理
                    {
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_COLOR || uiSetting == UI_CHANGE_TYPE.BUTTON_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // 背景
                            var background = commonObject.GetComponent<Image>();
                            ImageSettingButtonBackground(param, commonObject.gameObject, background);
                        }
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_FRAME_COLOR || uiSetting == UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // フレーム
                            var frame = commonObject.transform.Find("Image")?.GetComponent<Image>();
                            if (frame != null)
                            {
                                frame.type = Image.Type.Tiled;
                                ImageSettingButtonFrame(param, frame);
                            }
                        }
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_HIGHLIGHT || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // ハイライト
                            var highlight = commonObject.transform.Find("Highlight")?.GetComponent<Image>();
                            ImageSettingButtonHighlight(param, highlight);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// UIパターンの適用 選択肢
        /// </summary>
        private void ImageSettingApplyInputSelect(UiPatternParameter param, UI_CHANGE_TYPE uiSetting) {
            if (uiSetting == UI_CHANGE_TYPE.FONT ||
                uiSetting == UI_CHANGE_TYPE.FONT_SIZE ||
                uiSetting == UI_CHANGE_TYPE.FONT_COLOR)
                return;

            var messageInputSelectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/MessageInputSelect.prefab");
            var messageInputSelect = Object.Instantiate(messageInputSelectPrefab);

            switch (uiSetting)
            {
                case UI_CHANGE_TYPE.PATTERN:
                    UiSetting();
                    UiButtonSetting();
                    break;
                case UI_CHANGE_TYPE.BG_COLOR:
                case UI_CHANGE_TYPE.BG_IMAGE:
                case UI_CHANGE_TYPE.WINDOW_COLOR:
                case UI_CHANGE_TYPE.WINDOW_IMAGE:
                case UI_CHANGE_TYPE.FRAME_COLOR:
                case UI_CHANGE_TYPE.FRAME_IMAGE:
                case UI_CHANGE_TYPE.FRAME_HIGHLIGHT:
                    UiSetting();
                    break;
                case UI_CHANGE_TYPE.BUTTON_COLOR:
                case UI_CHANGE_TYPE.BUTTON_IMAGE:
                case UI_CHANGE_TYPE.BUTTON_HIGHLIGHT:
                case UI_CHANGE_TYPE.BUTTON_FRAME_COLOR:
                case UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE:
                    UiButtonSetting();
                    break;
            }

            // 保存
            PrefabUtility.SaveAsPrefabAsset(messageInputSelect, "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/MessageInputSelect.prefab");
            Object.DestroyImmediate(messageInputSelect);


            void UiSetting() {
                var messageInputSelectObjects = messageInputSelect.GetComponent<WindowBase>().commonWindowObject;

                // 画像適用処理
                for (int i3 = 0; i3 < messageInputSelectObjects.Length; i3++)
                {
                    if (messageInputSelectObjects[i3].transform.Find("Margin/BackImage") == null) continue;
                    var background = messageInputSelectObjects[i3].transform.Find("Margin/BackImage").GetComponent<Image>();
                    
                    if (uiSetting == UI_CHANGE_TYPE.WINDOW_COLOR || uiSetting == UI_CHANGE_TYPE.WINDOW_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                    {
                        // 背景
                        ImageSettingBackground(param, background);
                    }
                    if (uiSetting == UI_CHANGE_TYPE.FRAME_COLOR || uiSetting == UI_CHANGE_TYPE.FRAME_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                    {
                        // フレーム
                        var frame = messageInputSelectObjects[i3].transform.Find("Margin/BackImage").transform.Find("Frame").GetComponent<Image>();
                        frame.type = Image.Type.Tiled;
                        ImageSettingFrame(param, frame);
                    }
                    if (uiSetting == UI_CHANGE_TYPE.FRAME_HIGHLIGHT || uiSetting == UI_CHANGE_TYPE.PATTERN)
                    {
                        // ハイライト
                        var highlight = messageInputSelectObjects[i3].transform.Find("Margin/BackImage/Frame/Highlight")?.GetComponent<Image>();
                        if (highlight != null)
                        {
                            highlight.GetComponent<RectTransform>().anchoredPosition = background.GetComponent<RectTransform>().anchoredPosition;
                            highlight.GetComponent<RectTransform>().anchorMin = background.GetComponent<RectTransform>().anchorMin;
                            highlight.GetComponent<RectTransform>().anchorMax = background.GetComponent<RectTransform>().anchorMax;
                            highlight.GetComponent<RectTransform>().sizeDelta = background.GetComponent<RectTransform>().sizeDelta;
                            highlight.type = Image.Type.Tiled;
                            highlight.color = param.windowFrameImageHighlightColor;
                            highlight.sprite = null;
                        }
                    }
                }
            }

            // ボタン
            void UiButtonSetting() {
                // gameobjectを取得
                var objects = messageInputSelect.transform.GetComponentsInChildren(typeof(WindowButtonBase), true);

                // 画像適用先object取得
                for (int i2 = 0; i2 < objects?.Length; i2++)
                {
                    var commonObject = (objects[i2] as WindowButtonBase);
                    if (commonObject == null) continue;

                    // 画像適用処理
                    {
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_COLOR || uiSetting == UI_CHANGE_TYPE.BUTTON_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // 背景
                            var background = commonObject.GetComponent<Image>();
                            ImageSettingButtonBackground(param, commonObject.gameObject, background);
                        }
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_FRAME_COLOR || uiSetting == UI_CHANGE_TYPE.BUTTON_FRAME_IMAGE || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // フレーム
                            var frame = commonObject.transform.Find("Text/Image")?.GetComponent<Image>();
                            if (frame != null)
                            {
                                frame.type = Image.Type.Tiled;
                                ImageSettingButtonFrame(param, frame);
                            }
                        }
                        if (uiSetting == UI_CHANGE_TYPE.BUTTON_HIGHLIGHT || uiSetting == UI_CHANGE_TYPE.PATTERN)
                        {
                            // ハイライト
                            var highlight = commonObject.transform.Find("Text/Highlight")?.GetComponent<Image>();
                            ImageSettingButtonHighlight(param, highlight);
                        }
                    }
                }
            }
        }

        private void FontSetting(UI_CHANGE_TYPE uiSetting) {
            if (uiSetting != UI_CHANGE_TYPE.PATTERN &&
                uiSetting != UI_CHANGE_TYPE.FONT_SIZE &&
                uiSetting != UI_CHANGE_TYPE.FONT_COLOR)
                return;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RPGMaker/Codebase/Runtime/Battle/Windows" + "0" + (int.Parse(_systemSettingDataModel.uiPatternId) + 1) + ".prefab");
            var battleMenu = Object.Instantiate(prefab);

            switch (uiSetting)
            {
                case UI_CHANGE_TYPE.PATTERN:
                    SizeSetting();
                    ColorSetting();
                    break;
                case UI_CHANGE_TYPE.FONT_SIZE:
                    SizeSetting();
                    break;
                case UI_CHANGE_TYPE.FONT_COLOR:
                    ColorSetting();
                    break;
            }

            Save();

            // 保存
            PrefabUtility.SaveAsPrefabAsset(battleMenu, "Assets/RPGMaker/Codebase/Runtime/Battle/Windows" + "0" + (int.Parse(_systemSettingDataModel.uiPatternId) + 1) + ".prefab");
            Object.DestroyImmediate(battleMenu);

            void SizeSetting() {
                // サイズ設定
                var menu = new List<TextMeshProUGUI>();
                menu.AddRange(_menuCanvas.transform.Find("UICanvas/MainWindow/MenuArea/MenuWindow/MenuItems").GetComponentsInChildren<TextMeshProUGUI>(true));
                menu.AddRange(_menuCanvas.transform.Find("UICanvas/ItemWindow/MenuArea/Menu").GetComponentsInChildren<TextMeshProUGUI>(true));
                menu.AddRange(_menuCanvas.transform.Find("UICanvas/SkillWindow/MenuArea/Command").GetComponentsInChildren<TextMeshProUGUI>(true));
                menu.AddRange(_menuCanvas.transform.Find("UICanvas/EquipWindow/MenuArea/Menus").GetComponentsInChildren<TextMeshProUGUI>(true));
                menu.AddRange(_menuCanvas.transform.Find("UICanvas/EndWindow/MenuArea/Menu").GetComponentsInChildren<TextMeshProUGUI>(true));
                foreach (var t in menu)
                    t.fontSize = _uiSettingDataModel.commonMenus[0].menuFontSetting.size;

                menu = new List<TextMeshProUGUI>();
                menu.AddRange(battleMenu.transform.Find("WindowPartyCommand/WindowArea").GetComponentsInChildren<TextMeshProUGUI>(true));
                menu.AddRange(battleMenu.transform.Find("WindowActorCommand/WindowArea").GetComponentsInChildren<TextMeshProUGUI>(true));
                foreach (var t in menu)
                    t.fontSize = _uiSettingDataModel.commonMenus[0].menuFontSetting.size;
            }

            void ColorSetting() {
                // カラー設定
                var color = new Color(
                    _uiSettingDataModel.commonMenus[0].menuFontSetting.color[0] / 255f,
                    _uiSettingDataModel.commonMenus[0].menuFontSetting.color[1] / 255f,
                    _uiSettingDataModel.commonMenus[0].menuFontSetting.color[2] / 255f,
                    1);

                var textsMp = _menuCanvas.transform.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in textsMp)
                    t.color = color;
                var texts = _menuCanvas.transform.GetComponentsInChildren<Text>(true);
                foreach (var t in texts)
                    t.color = color;

                var outlines = _menuCanvas.transform.GetComponentsInChildren<Outline>(true);
                foreach (var o in outlines)
                    o.enabled = false;

                textsMp = battleMenu.transform.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in textsMp)
                    t.color = color;

                texts = battleMenu.transform.GetComponentsInChildren<Text>(true);
                foreach (var t in texts)
                    t.color = color;

                outlines = battleMenu.transform.GetComponentsInChildren<Outline>(true);
                foreach (var o in outlines)
                    o.enabled = false;
            }
        }

        public void MenuDisplayChangeSetting() {
            Render();
            ResetSetting();
            Save();
            ChangeDisplay();
        }

        private void AllDisplayDisable() {
            _mainWindow.SetActive(false);
            _itemObj.SetActive(false);
            _skillObj.SetActive(false);
            _equipObj.SetActive(false);
            _statusObj.SetActive(false);
            _sortObj.SetActive(false);
            _optionObj.SetActive(false);
            _saveObj.SetActive(false);
            _endObj.SetActive(false);
        }

        public void Render() {
            
            _menuCanvas?.SetActive(true);

            _menuItemObj.SetActive(_uiSettingDataModel.gameMenu.menuItem.enabled == 1);
            _menuSkillObj.SetActive(_uiSettingDataModel.gameMenu.menuSkill.enabled == 1);
            _menuEquipObj.SetActive(_uiSettingDataModel.gameMenu.menuEquipment.enabled == 1);
            _menuStatusObj.SetActive(_uiSettingDataModel.gameMenu.menuStatus.enabled == 1);
            _menuSortObj.SetActive(_uiSettingDataModel.gameMenu.menuSort.enabled == 1);
            _menuOptionObj.SetActive(_uiSettingDataModel.gameMenu.menuOption.enabled == 1);
            _menuSaveObj.SetActive(_uiSettingDataModel.gameMenu.menuSave.enabled == 1);
            _menuEndObj.SetActive(_uiSettingDataModel.gameMenu.menuGameEnd.enabled == 1);
            
            _itemItemObj.SetActive(_uiSettingDataModel.gameMenu.categoryItem.enabled == 1);
            _itemWeaponObj.SetActive(_uiSettingDataModel.gameMenu.categoryWeapon.enabled == 1);
            _itemArmorObj.SetActive(_uiSettingDataModel.gameMenu.categoryArmor.enabled == 1);
            _itemImportantObj.SetActive(_uiSettingDataModel.gameMenu.categoryImportant.enabled == 1);
        }

        private void Save() {
            if (_menuCanvas.transform.Find("UICanvas").GetComponent<GraphicRaycaster>() != null)
            {
                _menuCanvas.transform.Find("UICanvas").GetComponent<GraphicRaycaster>().enabled = true;
            }
            PrefabUtility.SaveAsPrefabAsset(_menuCanvas, MenuPrefabFilePath);
        }

        public override void Update() {
        }

        public override void DestroyLocalData() {
            
            if (_backgroundObj != null) Object.DestroyImmediate(_backgroundObj);
            if (_menuCanvas != null) Object.DestroyImmediate(_menuCanvas);
            if (_mainWindow != null) Object.DestroyImmediate(_mainWindow);
            if (_menuItemObj != null) Object.DestroyImmediate(_menuItemObj);
            if (_menuSkillObj != null) Object.DestroyImmediate(_menuSkillObj);
            if (_menuEquipObj != null) Object.DestroyImmediate(_menuEquipObj);
            if (_menuStatusObj != null) Object.DestroyImmediate(_menuStatusObj);
            if (_menuSortObj != null) Object.DestroyImmediate(_menuSortObj);
            if (_menuOptionObj != null) Object.DestroyImmediate(_menuOptionObj);
            if (_menuSaveObj != null) Object.DestroyImmediate(_menuSaveObj);
            if (_menuEndObj != null) Object.DestroyImmediate(_menuEndObj);
            if (_itemObj != null) Object.DestroyImmediate(_itemObj);
            if (_skillObj != null) Object.DestroyImmediate(_skillObj);
            if (_equipObj != null) Object.DestroyImmediate(_equipObj);
            if (_statusObj != null) Object.DestroyImmediate(_statusObj);
            if (_sortObj != null) Object.DestroyImmediate(_sortObj);
            if (_optionObj != null) Object.DestroyImmediate(_optionObj);
            if (_saveObj != null) Object.DestroyImmediate(_saveObj);
            if (_endObj != null) Object.DestroyImmediate(_endObj);
            _backgroundObj = null;
            _menuCanvas = null;
            _mainWindow = null;
            _menuItemObj = null;
            _menuSkillObj = null;
            _menuEquipObj = null;
            _menuStatusObj = null;
            _menuSortObj = null;
            _menuOptionObj = null;
            _menuSaveObj = null;
            _menuEndObj = null;
            _itemObj = null;
            _skillObj = null;
            _equipObj = null;
            _statusObj = null;
            _sortObj = null;
            _optionObj = null;
            _saveObj = null;
            _endObj = null;
        }

        private class UiPatternParameter
        {
            public int uiPatternId = 0;
            public Sprite bgSprite;
            public Sprite bgImageSprite;
            public Sprite frImageSprite;
            public Sprite buttonImageSprite;
            public Sprite buttonFrameImageSprite;
            public Sprite planeImageSprite;
            public Sprite planeBgSprite;
            public List<System.Type> skillTypes;
            public Color backgroundImageColor;
            public Color windowBackgroundImageColor;
            public Color windowFrameImageColor;
            public Color windowFrameImageHighlightColor;
            public Color buttonImageColor;
            public Color buttonFrameImageColor;
            public Color buttonImageHighlightColor;
        }
    }
}