using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.UiSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using RPGMaker.Codebase.Runtime.Battle.Window;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.View.Preview
{
    /// <summary>
    /// ゲーム用のプレビュー
    /// </summary>
    public class BattlePreview : AbstractPreview
    {
        private const string BattlePrefabPath = "Assets/RPGMaker/Codebase/Runtime/Battle/Windows";
        private const string MenuPrefabBackgroundPath = "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/MenuPreview/Background.prefab";

        private GameObject             _obj = null;
        private GameObject             _battleCanvas;
        private UiSettingDataModel     _uiSettingDataModel;
        private SystemSettingDataModel _systemSettingDataModel;
        private Camera                 _sceneCamera;

        private GameObject _backgroundObj;

        private GameObject _partyCommandWindow;
        private GameObject _actorCommandWindow;
        private GameObject _battleStatusWindow;
        private GameObject _helpWindow;

        private List<GameObject> _actorObj;
        private List<GameObject> _hpObj;
        private List<GameObject> _mpObj;
        private List<GameObject> _tpObj;

        public BattlePreview() {
        }

        public void SetUiData(UiSettingDataModel uiSettingDataModel) {
            var databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
            _systemSettingDataModel = databaseManagementService.LoadSystem();
            _uiSettingDataModel = uiSettingDataModel;
        }

        /// <summary>
        /// 初期状態のUI設定
        /// </summary>
        public override void InitUi(SceneWindow scene, bool isChange = false) {
            DestroyLocalData();
            _obj = AssetDatabase.LoadAssetAtPath<GameObject>(BattlePrefabPath + "0" + (int.Parse(_systemSettingDataModel.uiPatternId) + 1) + ".prefab");
            if (_obj != null)
            {
                // ベースキャンバスを作成する
                var _mainCanvas = new GameObject("canvas");
                _mainCanvas.AddComponent<Canvas>();
                _mainCanvas.AddComponent<CanvasScaler>();
                _mainCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
                _mainCanvas.GetComponent<Canvas>().worldCamera = scene.Camera;
                _mainCanvas.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                _mainCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
                _mainCanvas.GetComponent<CanvasScaler>().screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                _mainCanvas.transform.localScale = Vector3.one;

                // prefabから作成
                _battleCanvas = Object.Instantiate(_obj);
                _battleCanvas.transform.SetParent(_mainCanvas.transform);
                _battleCanvas.transform.localPosition = new Vector3(0f, 0f, 0f);
                _battleCanvas.transform.localScale = Vector3.one;

                // 設定
                PreviewSetting();

                // プレビューシーンに移動
                scene.MoveGameObjectToPreviewScene(_mainCanvas);
                scene.SetRenderingSize(scene.GetRenderingSize().x, scene.GetRenderingSize().x * 9 / 16);

                _sceneCamera = scene.Camera;

                // 背景設定
                var background = AssetDatabase.LoadAssetAtPath<GameObject>(MenuPrefabBackgroundPath);
                _backgroundObj = Object.Instantiate(background);
                _backgroundObj.transform.localPosition = new Vector3(-10f, 4f, 1f);
                _backgroundObj.transform.localScale = Vector3.one;
                scene.MoveGameObjectToPreviewScene(_backgroundObj);
                _backgroundObj.SetActive(true);

                // Window取得
                _partyCommandWindow = _battleCanvas.transform.Find("WindowPartyCommand").gameObject;
                _actorCommandWindow = _battleCanvas.transform.Find("WindowActorCommand").gameObject;
                _battleStatusWindow = _battleCanvas.transform.Find("WindowBattleStatus").gameObject;
                _helpWindow = _battleCanvas.transform.Find("WindowHelp").gameObject;
                

                //コマンドの言語設定
                DataManager.Self().SetTroopForBattle(null, true);
                var actorCommandWindow = _actorCommandWindow.GetComponent<WindowActorCommand>();
                actorCommandWindow.Initialize();
                actorCommandWindow.SetBattlePreviewMode(DataManager.Self().GetGameParty().Actors[0]);
                actorCommandWindow.Open();

                var helpWindow = _helpWindow.GetComponent<WindowHelp>();
                helpWindow.Initialize();
                helpWindow.Open();

                _actorObj = new List<GameObject>();
                _hpObj = new List<GameObject>();
                _mpObj = new List<GameObject>();
                _tpObj = new List<GameObject>();

                // アクター取得
                //アクターが何人いるか
                var databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
                List<CharacterActorDataModel> characterActorDataModels = databaseManagementService.LoadCharacterActor();

                for (int i = 0; i < 4; i++)
                    _actorObj.Add(_battleStatusWindow.transform.Find("WindowArea/Ui/ActorStatus" + (i + 1)).gameObject);

                // hp mp tp取得
                for (int i = 0; i < _actorObj.Count; i++)
                {
                    _hpObj.Add(_actorObj[i].transform.Find("HPBox").gameObject);
                    _mpObj.Add(_actorObj[i].transform.Find("MPBox").gameObject);
                    _tpObj.Add(_actorObj[i].transform.Find("TPBox").gameObject);
                }

                //アクター情報の取得
                var actorDataModels = new List<CharacterActorDataModel>();
                for (int i = 0; i < characterActorDataModels.Count; i++)
                {
                    if (characterActorDataModels[i].charaType == (int) ActorTypeEnum.ACTOR)
                    {
                        actorDataModels.Add(characterActorDataModels[i]);
                    }
                }
                
                //マスタデータが、表示するオブジェクトより少なかったら、非表示にする
                for (int i = 0; i < _actorObj.Count; i++)
                {
                    if (i > actorDataModels.Count - 1)
                    {
                        _actorObj[i].SetActive(false);
                    }
                }

                for (int i = 1; i <= 4; i++)
                {
                    if(i > _actorObj.Count) return;
                    var actor = actorDataModels[i - 1];
                    _actorObj[i - 1].SetActive(true);
                    _actorObj[i - 1].transform.Find("ActorName").gameObject.GetComponent<TextMeshProUGUI>().text = actor.basic.name;
                    var face = _actorObj[i - 1].transform.Find("Face").gameObject.GetComponent<Image>();
                    if (face != null)
                    {
                        face.transform.localScale = new Vector2(1f,1f);
                        face.preserveAspect = false;
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
                        else if (_uiSettingDataModel.commonMenus[0].characterType ==
                                 (int) MenuIconTypeEnum.SD)
                        {
                            //SDキャラ
                            var assetId = DataManager.Self().GetActorDataModel(actor.uuId).image.character;
                            CharacterGraphic characterGraphic =
                                new GameObject().AddComponent<CharacterGraphic>();
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
                                        new Vector2(
                                            face.transform.localScale.x / face.transform.localScale.y,
                                            1.0f);
                                }
                                else
                                {
                                    face.transform.localScale = new Vector2(1.0f,
                                        face.transform.localScale.y / face.transform.localScale.x);
                                }
                            }

                            //face.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                            Object.DestroyImmediate(characterGraphic.gameObject);
                        }
                        else if (_uiSettingDataModel.commonMenus[0].characterType ==
                                 (int) MenuIconTypeEnum.PICTURE)
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

                // 表示切替
                for (int i = 0; i < _actorObj.Count; i++)
                {
                    _hpObj[i].SetActive(_uiSettingDataModel.battleMenu.menuHp.enabled == 0);
                    _mpObj[i].SetActive(_uiSettingDataModel.battleMenu.menuMp.enabled == 0);
                    _tpObj[i].SetActive(_uiSettingDataModel.battleMenu.menuTp.enabled == 0);

                    var hpText = _hpObj[i].transform.Find("HPText").GetComponent<TextMeshProUGUI>();
                    var mpText = _mpObj[i].transform.Find("MPText").GetComponent<TextMeshProUGUI>();
                    var tpText = _tpObj[i].transform.Find("TPText").GetComponent<TextMeshProUGUI>();
                    hpText.text = TextManager.hp;
                    mpText.text = TextManager.mp;
                    tpText.text = TextManager.tp;
                }
            }
        }

        // 初期状態の設定（保存する際に呼び出す）
        void DefaultSetting() {
            if (_partyCommandWindow != null) _partyCommandWindow.SetActive(true);
            if (_actorCommandWindow != null) _actorCommandWindow.SetActive(false);
            if (_battleStatusWindow != null) _battleStatusWindow.SetActive(true);
        }

        // Preview表示用の設定
        void PreviewSetting() {
            if (_partyCommandWindow != null) _partyCommandWindow.SetActive(false);
            if (_actorCommandWindow != null) _actorCommandWindow.SetActive(true);
            if (_battleStatusWindow != null) _battleStatusWindow.SetActive(true);
        }

        public void Render() {
            _battleCanvas.SetActive(true);
            if (_actorObj != null)
            {
                for (int i = 0; i < _actorObj.Count; i++)
                {
                    _hpObj[i].SetActive(_uiSettingDataModel.battleMenu.menuHp.enabled == 0);
                    _mpObj[i].SetActive(_uiSettingDataModel.battleMenu.menuMp.enabled == 0);
                    _tpObj[i].SetActive(_uiSettingDataModel.battleMenu.menuTp.enabled == 0);
                }

                // 設定を戻す
                DefaultSetting();

                // マッププレハブ保存
                PrefabUtility.SaveAsPrefabAsset(_battleCanvas, BattlePrefabPath + "0" + (int.Parse(_systemSettingDataModel.uiPatternId) + 1) + ".prefab");

                // 再設定
                PreviewSetting();
            }
        }
        
        public override void Update() {
        }

        public override void DestroyLocalData() {
            if (_backgroundObj != null) Object.DestroyImmediate(_backgroundObj);
            if (_battleCanvas != null) Object.DestroyImmediate(_battleCanvas);
            if (_sceneCamera != null) Object.DestroyImmediate(_sceneCamera);
            if (_partyCommandWindow != null) Object.DestroyImmediate(_partyCommandWindow);
            if (_actorCommandWindow != null) Object.DestroyImmediate(_actorCommandWindow);
            if (_battleStatusWindow != null) Object.DestroyImmediate(_battleStatusWindow);

            _backgroundObj = null;
            _battleCanvas = null;
            _sceneCamera = null;
            _partyCommandWindow = null;
            _actorCommandWindow = null;
            _battleStatusWindow = null;

            for (int i = 0; _actorObj != null && i < _actorObj.Count; i++)
            {
                Object.DestroyImmediate(_actorObj[i]);
            }

            _actorObj = null;

            for (int i = 0; _hpObj != null && i < _hpObj.Count; i++)
            {
                Object.DestroyImmediate(_hpObj[i]);
            }

            _hpObj = null;

            for (int i = 0; _mpObj != null && i < _mpObj.Count; i++)
            {
                Object.DestroyImmediate(_mpObj[i]);
            }

            _mpObj = null;

            for (int i = 0; _tpObj != null && i < _tpObj.Count; i++)
            {
                Object.DestroyImmediate(_tpObj[i]);
            }

            _tpObj = null;
        }
    }
}