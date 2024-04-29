using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TextMP = TMPro.TextMeshProUGUI;

namespace RPGMaker.Codebase.Runtime.Map.Item
{
    public class CharacterItem : MonoBehaviour
    {
        private Image          _body;
        private TextMP         _class;
        private ClassDataModel _classData;
        private Image          _face;

        private Slider _hpBar;

        private Text   _hpName;
        private TextMP _hpValue;

        private int    _iconIndex;
        private Text   _level;
        private TextMP _levelNumber;
        private Slider _mpBar;
        private Text   _mpName;
        private TextMP _mpValue;
        private TextMP _name;

        private TextMP               _secondName;
        private List<Image>          _stateIcons = new List<Image>();

        private GameObject _stateObj;
        private Slider     _tpBar;
        private Text       _tpName;
        private TextMP     _tpValue;

        public GameActor GameActor { get; private set; }

        public RuntimeActorDataModel RuntimeActorDataModel { get; private set; }

        public void Init(RuntimeActorDataModel actor) {
            //表示データ
            RuntimeActorDataModel = actor;

            GameActor = DataManager.Self().GetGameActors().Actor(actor);
            int characterType = DataManager.Self().GetUiSettingDataModel().commonMenus[0].characterType;
            //職業
            _classData = DataManager.Self().GetClassDataModels().FirstOrDefault(c => c.id == RuntimeActorDataModel.classId);

            //名前、レベル
            _name = transform.Find("Name").GetComponent<TextMP>();
            _level = transform.Find("Level").GetComponent<Text>();
            _levelNumber = transform.Find("Level/Number").GetComponent<TextMP>();
            //通り名
            if (transform.Find("SecondName") != null)
            {
                _secondName = transform.Find("SecondName").GetComponent<TextMP>();
                _secondName.text = RuntimeActorDataModel.nickname;
            }
            //Faceの有無確認
            if (transform.Find("Face") != null)
                _face = transform.Find("Face").GetComponent<Image>();
            //立ち絵の有無確認
            if (transform.Find("body") != null) 
                _body = transform.Find("body").GetComponent<Image>();

            _class = transform.Find("Class").GetComponent<TextMP>();
            _hpName = transform.Find("Hp/HpName").GetComponent<Text>();
            _mpName = transform.Find("Mp/MpName").GetComponent<Text>();
            _tpName = transform.Find("Tp/TpName").GetComponent<Text>();
            _hpValue = transform.Find("Hp/HpValue").GetComponent<TextMP>();
            _mpValue = transform.Find("Mp/MpValue").GetComponent<TextMP>();
            _tpValue = transform.Find("Tp/TpValue").GetComponent<TextMP>();

            //各種ステータスバーの表示の有無の確認
            //(1つずつ行っていますがまとめてでもいいかもしれない)
            if (transform.Find("Hp/HpBar") != null) _hpBar = transform.Find("Hp/HpBar").GetComponent<Slider>();
            if (transform.Find("Mp/MpBar") != null) _mpBar = transform.Find("Mp/MpBar").GetComponent<Slider>();
            if (transform.Find("Tp/TpBar") != null) _tpBar = transform.Find("Tp/TpBar").GetComponent<Slider>();

            //顔アイコン、立ち絵、SDキャラクターの設定値に応じて、表示するものを変更する
            //双方の部品が存在する場合は、顔アイコン,SDキャラと立ち絵画像で、表示する部品を変更する
            if (_face != null && _body != null)
            {
                if (characterType == (int) MenuIconTypeEnum.FACE)
                {
                    //顔アイコン
                    var sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                        "Assets/RPGMaker/Storage/Images/Faces/" +
                        RuntimeActorDataModel.faceImage + ".png");
                    _face.enabled = true;
                    _face.sprite = sprite;
                    _face.material = null;
                    _face.color = Color.white;
                    _face.preserveAspect = true;

                    _face.gameObject.SetActive(true);
                    _body.gameObject.SetActive(false);
                }
                else if (characterType == (int) MenuIconTypeEnum.SD)
                {
                    //SDキャラ
                    var assetId = DataManager.Self().GetActorDataModel(RuntimeActorDataModel.actorId).image.character;
                    CharacterGraphic characterGraphic = new GameObject().AddComponent<CharacterGraphic>();
                    characterGraphic.Init(assetId);

                    _face.enabled = true;
                    _face.sprite = characterGraphic.GetCurrentSprite();
                    _face.color = Color.white;
                    _face.material = characterGraphic.GetMaterial();
                    _face.transform.localScale = characterGraphic.GetSize();

                    if (_face.transform.localScale.x > 1.0f || _face.transform.localScale.y > 1.0f)
                    {
                        if (_face.transform.localScale.y > 1.0f)
                        {
                            _face.transform.localScale = new Vector2(_face.transform.localScale.x / _face.transform.localScale.y, 1.0f);
                        }
                        else
                        {
                            _face.transform.localScale = new Vector2(1.0f, _face.transform.localScale.y / _face.transform.localScale.x);
                        }
                    }

                    _face.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    characterGraphic.gameObject.SetActive(false);

                    _face.gameObject.SetActive(true);
                    _body.gameObject.SetActive(false);
                }
                else if (characterType == (int) MenuIconTypeEnum.PICTURE)
                {
                    //立ち絵
                    var imageName = RuntimeActorDataModel.advImage.Contains(".png")
                        ? RuntimeActorDataModel.advImage
                        : RuntimeActorDataModel.advImage + ".png";
                    var path = "Assets/RPGMaker/Storage/Images/Pictures/" + imageName;
                    var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);

                    _body.enabled = true;
                    _body.sprite = tex;
                    _body.color = Color.white;
                    _body.preserveAspect = true;

                    _face.gameObject.SetActive(false);
                    _body.gameObject.SetActive(true);
                }
                else
                {
                    _face.gameObject.SetActive(false);
                    _body.gameObject.SetActive(false);
                }
            }
            //片方の部品しかない場合は、いずれのパターンでもその部品に描画する
            else if (_face != null)
            {
                if (characterType == (int) MenuIconTypeEnum.FACE)
                {
                    //顔アイコン
                    var sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                        "Assets/RPGMaker/Storage/Images/Faces/" +
                        RuntimeActorDataModel.faceImage + ".png");
                    _face.enabled = true;
                    _face.sprite = sprite;
                    _face.color = Color.white;
                    _face.preserveAspect = true;
                }
                else if (characterType == (int) MenuIconTypeEnum.SD)
                {
                    //SDキャラ
                    var assetId = DataManager.Self().GetActorDataModel(RuntimeActorDataModel.actorId).image.character;
                    CharacterGraphic characterGraphic = new GameObject().AddComponent<CharacterGraphic>();
                    characterGraphic.Init(assetId);

                    _face.enabled = true;
                    _face.sprite = characterGraphic.GetCurrentSprite();
                    _face.color = Color.white;
                    _face.material = characterGraphic.GetMaterial();
                    _face.transform.localScale = characterGraphic.GetSize();

                    if (_face.transform.localScale.x > 1.0f || _face.transform.localScale.y > 1.0f)
                    {
                        if (_face.transform.localScale.y > 1.0f)
                        {
                            _face.transform.localScale = new Vector2(_face.transform.localScale.x / _face.transform.localScale.y, 1.0f);
                        }
                        else
                        {
                            _face.transform.localScale = new Vector2(1.0f, _face.transform.localScale.y / _face.transform.localScale.x);
                        }
                    }

                    _face.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    characterGraphic.gameObject.SetActive(false);
                }
                else if (characterType == (int) MenuIconTypeEnum.PICTURE)
                {
                    //立ち絵
                    var imageName = RuntimeActorDataModel.advImage.Contains(".png")
                        ? RuntimeActorDataModel.advImage
                        : RuntimeActorDataModel.advImage + ".png";
                    var path = "Assets/RPGMaker/Storage/Images/Pictures/" + imageName;
                    var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);

                    _face.enabled = true;
                    _face.sprite = tex;
                    _face.color = Color.white;
                    _face.preserveAspect = true;
                }
                else
                    _face.enabled = false;
            }
            else if (_body != null)
            {
                if (characterType == (int) MenuIconTypeEnum.FACE)
                {
                    //顔アイコン
                    var sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                        "Assets/RPGMaker/Storage/Images/Faces/" +
                        RuntimeActorDataModel.faceImage + ".png");
                    _body.enabled = true;
                    _body.sprite = sprite;
                    _body.color = Color.white;
                    _body.preserveAspect = true;
                }
                else if (characterType == (int) MenuIconTypeEnum.SD)
                {
                    //SDキャラ
                    var assetId = DataManager.Self().GetActorDataModel(RuntimeActorDataModel.actorId).image.character;
                    CharacterGraphic characterGraphic = new GameObject().AddComponent<CharacterGraphic>();
                    characterGraphic.Init(assetId);

                    _body.enabled = true;
                    _body.sprite = characterGraphic.GetCurrentSprite();
                    _body.color = Color.white;
                    _body.material = characterGraphic.GetMaterial();
                    _body.transform.localScale = characterGraphic.GetSize();

                    if (_body.transform.localScale.x > 1.0f || _body.transform.localScale.y > 1.0f)
                    {
                        if (_body.transform.localScale.y > 1.0f)
                        {
                            _body.transform.localScale = new Vector2(_body.transform.localScale.x / _body.transform.localScale.y, 1.0f);
                        }
                        else
                        {
                            _body.transform.localScale = new Vector2(1.0f, _body.transform.localScale.y / _body.transform.localScale.x);
                        }
                    }

                    _body.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    characterGraphic.gameObject.SetActive(false);
                }
                else if (characterType == (int) MenuIconTypeEnum.PICTURE)
                {
                    //立ち絵
                    var imageName = RuntimeActorDataModel.advImage.Contains(".png")
                        ? RuntimeActorDataModel.advImage
                        : RuntimeActorDataModel.advImage + ".png";
                    var path = "Assets/RPGMaker/Storage/Images/Pictures/" + imageName;
                    var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);

                    _body.enabled = true;
                    _body.sprite = tex;
                    _body.color = Color.white;
                    _body.preserveAspect = true;
                }
                else
                    _body.enabled = false;
            }

            if (_name != null) _name.text = RuntimeActorDataModel.name;
            else transform.Find("Name").GetComponent<Text>().text = RuntimeActorDataModel.name;

            if (_levelNumber != null) _levelNumber.text = RuntimeActorDataModel.level.ToString();
            else transform.Find("Level/Number").GetComponent<Text>().text = RuntimeActorDataModel.level.ToString();

            if (_class != null) _class.text = _classData.basic.name;
            else transform.Find("Class").GetComponent<Text>().text = _classData.basic.name;

            _stateObj = transform.Find("StatusIcons").gameObject;
            _stateIcons = _stateObj.GetComponentsInChildren<Image>(true).ToList();

            SetHp();
            SetMp();
            SetTp();

            transform.Find("Mp").transform.gameObject.SetActive(_classData.basic.abilityEnabled.mp == 1);
            transform.Find("Tp").transform.gameObject.SetActive(_classData.basic.abilityEnabled.tp == 1);

            //Lv,Hp、Mp、Tpの表記の切り替え
            _level.text = TextManager.levelA;
            _hpName.text = TextManager.hpA;
            _mpName.text = TextManager.mpA;
            _tpName.text = TextManager.tpA;

            _iconIndex = 0;
            foreach (var state in _stateIcons) state.gameObject.SetActive(false);
            if (GameActor.Actor.states.Count > 0)
            {
                for (int i = 0; i < GameActor.Actor.states.Count; i++)
                {
                    _stateObj.SetActive(true);
                    SetState(_stateIcons[_iconIndex], GameActor.Actor.states[i].id);
                    _iconIndex++;
                    if (_iconIndex > 2) _iconIndex = 0;
                }
            }
        }

        public void UpdateData(RuntimeActorDataModel actor) {
            RuntimeActorDataModel = actor;
            GameActor = DataManager.Self().GetGameActors().Actor(actor);
            SetHp();
            SetMp();
            SetTp();

            _iconIndex = 0;
            foreach (var state in _stateIcons) state.gameObject.SetActive(false);
            if (GameActor.Actor.states.Count > 0)
            {
                for (int i = 0; i < GameActor.Actor.states.Count; i++)
                {
                    _stateObj.SetActive(true);
                    SetState(_stateIcons[_iconIndex], GameActor.Actor.states[i].id);
                    _iconIndex++;
                    if (_iconIndex > 2) _iconIndex = 0;
                }
            }
        }


        public void SetHp() {
            if (_hpBar != null) _hpBar.value = GameActor.Hp / (float) GameActor.Mhp;

            if (_hpValue != null) _hpValue.text = GameActor.Hp.ToString();
            else transform.Find("Hp/HpValue").GetComponent<Text>().text = GameActor.Hp.ToString();
        }

        public void SetMp() {
            if (_mpBar != null) _mpBar.value = GameActor.Mp / (float) GameActor.Mmp;

            if (_mpValue != null) _mpValue.text = GameActor.Mp.ToString();
            else transform.Find("Mp/MpValue").GetComponent<Text>().text = GameActor.Mp.ToString();
        }

        public void SetTp() {
            if (_tpBar != null) _tpBar.value = RuntimeActorDataModel.tp / 100f; 

            if (_tpValue != null) _tpValue.text = RuntimeActorDataModel.tp.ToString();
            else transform.Find("Tp/TpValue").GetComponent<Text>().text = RuntimeActorDataModel.tp.ToString();
        }

        public void SetState(Image icon, string id) {
            icon.gameObject.SetActive(true);
            var stateData = DataManager.Self().GetStateDataModel(id);
            var path = "Assets/RPGMaker/Storage/Images/System/IconSet/" + stateData.iconId + ".png";
            var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(path);
            icon.sprite = Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                Vector2.zero
            );
            ;
        }

        public string ActorId() {
            return RuntimeActorDataModel.actorId;
        }

        public string PartyId() {
            //押されたオブジェクトの順番にパーティに所属するアクターを取得する(object名は1からなので配列index用に-1する)
            //intにする(このobject名(最後の1文字を取得))1引く
            var objectNumber = int.Parse(gameObject.name.Substring(gameObject.name.Length - 1)) - 1;
            return DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.actors[objectNumber];
        }
    }
}