using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCustom;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using RPGMaker.Codebase.Runtime.Map.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;
using TextMP = TMPro.TextMeshProUGUI;

namespace RPGMaker.Codebase.Runtime.Map.Menu
{
    public class SkillMenu : WindowBase
    {
        public enum Window
        {
            SkillType = 0,
            SkillCommand,
            Party
        }
        
        private          TextMP               _class;
        private          List<ClassDataModel> _classDataModels;
        private          RectTransform        _content;

        private TextMP _description1;
        private TextMP _description2;
        private Image  _face;
        private Image  _body;
        private Slider _hpBar;

        private Text       _hpName;
        private TextMP     _hpValue;
        private Text       _level;
        private TextMP     _levelNumber;
        private Slider     _mpBar;
        private Text       _mpName;
        private TextMP     _mpValue;
        private TextMP     _name;
        private GameObject _originObj;

        private RuntimeActorDataModel _runtimeActorDataModel;
        
        private SkillCommand    _skillCommand;
        private List<SkillItem> _skillItems = new List<SkillItem>();

        private Slider _tpBar;
        private Text   _tpName;
        private TextMP _tpValue;
        
        private GameObject _commandOrigin;
        private List<Button> _skillTypeButtons;
        private int _pattern;
        private List<SkillCustomDataModel> skillData;

        private GameObject _scrollObject;
        private GameObject _contentObject;

        public Window WindowType { get; private set; }

        public MenuBase MenuBase { get; private set; }

        private GameActor _actor;

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="base"></param>
        /// <param name="actorId"></param>
        public void Init(MenuBase @base, string actorId) {
            MenuBase = @base;
            int characterType = DataManager.Self().GetUiSettingDataModel().commonMenus[0].characterType;

            SystemSettingDataModel systemSettingDataModel = DataManager.Self().GetSystemDataModel();
            _pattern = int.Parse(systemSettingDataModel.uiPatternId) + 1;
            if (_pattern < 1 || _pattern > 6)
                _pattern = 1;

            _skillCommand = transform.Find("MenuArea").GetComponent<SkillCommand>();
            _description1 = transform.Find("MenuArea/Description/DescriptionText1").GetComponent<TextMP>();
            _description2 = transform.Find("MenuArea/Description/DescriptionText2").GetComponent<TextMP>();
            _description1.text = "";
            _description2.text = "";
            _name = transform.Find("MenuArea/Status/Name").GetComponent<TextMP>();
            _level = transform.Find("MenuArea/Status/Level").GetComponent<Text>();
            _levelNumber = transform.Find("MenuArea/Status/Level/Number").GetComponent<TextMP>();
            if (transform.Find("MenuArea/Status/Face") != null)
                _face = transform.Find("MenuArea/Status/Face").GetComponent<Image>();
            if (transform.Find("MenuArea/Status/body") != null)
                _body = transform.Find("MenuArea/Status/body").GetComponent<Image>();
            _class = transform.Find("MenuArea/Status/Class").GetComponent<TextMP>();

            _hpName = transform.Find("MenuArea/Status/Hp/HpName").GetComponent<Text>();
            _hpValue = transform.Find("MenuArea/Status/Hp/HpValue").GetComponent<TextMP>();
            _hpBar = transform.Find("MenuArea/Status/Hp/HpBar").GetComponent<Slider>();

            _mpName = transform.Find("MenuArea/Status/Mp/MpName").GetComponent<Text>();
            _mpValue = transform.Find("MenuArea/Status/Mp/MpValue").GetComponent<TextMP>();
            _mpBar = transform.Find("MenuArea/Status/Mp/MpBar").GetComponent<Slider>();

            _tpName = transform.Find("MenuArea/Status/Tp/TpName").GetComponent<Text>();
            _tpValue = transform.Find("MenuArea/Status/Tp/TpValue").GetComponent<TextMP>();
            _tpBar = transform.Find("MenuArea/Status/Tp/TpBar").GetComponent<Slider>();
            
            _commandOrigin = transform.Find("MenuArea/Command/Scroll View/Viewport/Content/Command").gameObject;

            _scrollObject = transform.Find("MenuArea/Command/Scroll View").gameObject;
            _contentObject = transform.Find("MenuArea/Command/Scroll View/Viewport/Content").gameObject;
            _scrollObject.GetComponent<ScrollRect>().movementType = ScrollRect.MovementType.Clamped;

            var rectTransform = _commandOrigin.transform.parent.GetComponent<RectTransform>();
            _classDataModels = DataManager.Self().GetClassDataModels();

            var runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
            _runtimeActorDataModel = runtimeSaveDataModel.runtimeActorDataModels.FirstOrDefault(t => t.actorId == actorId);

            var skillCommands = DataManager.Self().GetSystemDataModel().skillTypes;

            if (_skillTypeButtons?.Count > 0)
            {
                foreach (var btn in _skillTypeButtons)
                {
                    DestroyImmediate(btn.gameObject);
                }
            }

            _skillTypeButtons = new List<Button>();

            //1はNoneのため飛ばす
            for (int i = 1; i < skillCommands.Count; i++)
            {
                var obj = Instantiate(_commandOrigin);
                obj.transform.SetParent(_commandOrigin.transform.parent);
                obj.transform.localScale = _commandOrigin.transform.localScale;
                obj.name = "Item" + i;
                obj.SetActive(true);
                int index = i;
                _skillTypeButtons.Add(obj.GetComponent<Button>());

                if (obj.GetComponent<WindowButtonBase>() != null)
                {
                    obj.GetComponent<WindowButtonBase>().ScrollView = _scrollObject;
                    obj.GetComponent<WindowButtonBase>().Content = _contentObject;
                    if (_pattern == 3 || _pattern == 4)
                        obj.GetComponent<WindowButtonBase>().IsHorizontal = true;
                    obj.transform.GetComponent<WindowButtonBase>().OnClick = new Button.ButtonClickedEvent();
                    obj.transform.GetComponent<WindowButtonBase>().OnClick.AddListener(() => CreateItem(index));
                }

                obj.transform.Find("Text").GetComponent<TextMP>().text = skillCommands[i].value;
            }

            if (_pattern == 3 || _pattern == 4)
            {
                rectTransform.sizeDelta =
                    new Vector2(_commandOrigin.GetComponent<RectTransform>().rect.width * (skillCommands.Count - 1),
                        rectTransform.sizeDelta.y);
                rectTransform.anchoredPosition = new Vector2(0,0);
            }
            else
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x,
                    _commandOrigin.GetComponent<RectTransform>().rect.height * skillCommands.Count);
            }

            _commandOrigin.SetActive(false);

            //十字キーでの操作登録
            var selects = _skillTypeButtons;
            for (var i = 0; i < selects.Count; i++)
            {
                var nav = selects[i].navigation;
                nav.mode = Navigation.Mode.Explicit;

                //UIパターンに応じて十字キーを変更する
                if (_pattern == 1 || _pattern == 2 || _pattern == 5 || _pattern == 6)
                {
                    nav.selectOnUp = selects[i == 0 ? selects.Count - 1 : i - 1];
                    nav.selectOnDown = selects[(i + 1) % selects.Count];
                }
                else
                {
                    nav.selectOnLeft = selects[i == 0 ? selects.Count - 1 : i - 1];
                    nav.selectOnRight = selects[(i + 1) % selects.Count];
                }

                selects[i].navigation = nav;
                selects[i].targetGraphic = selects[i].transform.Find("Highlight").GetComponent<Image>();
            }

            if (selects.Count > 0)
            {
                selects[0].Select();
            }

            _originObj = transform.Find("MenuArea/Skill/Scroll View/Viewport/Content/Ruck1/Item1").gameObject;
            _content = transform.Find("MenuArea/Skill/Scroll View/Viewport/Content").gameObject.GetComponent<RectTransform>();

            _originObj.SetActive(false);
            _content.sizeDelta = new Vector2(0f, 100 * 6);

            _skillCommand.Init(this);

            //顔アイコン、立ち絵、SDキャラクターの設定値に応じて、表示するものを変更する
            //双方の部品が存在する場合は、顔アイコン,SDキャラと立ち絵画像で、表示する部品を変更する
            if (_face != null && _body != null)
            {
                if (characterType == (int) MenuIconTypeEnum.FACE)
                {
                    //顔アイコン
                    var sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                        "Assets/RPGMaker/Storage/Images/Faces/" +
                        _runtimeActorDataModel.faceImage + ".png");
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
                    var assetId = DataManager.Self().GetActorDataModel(_runtimeActorDataModel.actorId).image.character;
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
                    var imageName = _runtimeActorDataModel.advImage.Contains(".png")
                        ? _runtimeActorDataModel.advImage
                        : _runtimeActorDataModel.advImage + ".png";
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
                        _runtimeActorDataModel.faceImage + ".png");
                    _face.enabled = true;
                    _face.sprite = sprite;
                    _face.color = Color.white;
                    _face.preserveAspect = true;
                }
                else if (characterType == (int) MenuIconTypeEnum.SD)
                {
                    //SDキャラ
                    var assetId = DataManager.Self().GetActorDataModel(_runtimeActorDataModel.actorId).image.character;
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
                    var imageName = _runtimeActorDataModel.advImage.Contains(".png")
                        ? _runtimeActorDataModel.advImage
                        : _runtimeActorDataModel.advImage + ".png";
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
                        _runtimeActorDataModel.faceImage + ".png");
                    _body.enabled = true;
                    _body.sprite = sprite;
                    _body.color = Color.white;
                    _body.preserveAspect = true;
                }
                else if (characterType == (int) MenuIconTypeEnum.SD)
                {
                    //SDキャラ
                    var assetId = DataManager.Self().GetActorDataModel(_runtimeActorDataModel.actorId).image.character;
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
                    var imageName = _runtimeActorDataModel.advImage.Contains(".png")
                        ? _runtimeActorDataModel.advImage
                        : _runtimeActorDataModel.advImage + ".png";
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

            _name.text = _runtimeActorDataModel.name;
            _levelNumber.text = _runtimeActorDataModel.level.ToString();

            //クラス名にする
            _class.text = _GetClass(_runtimeActorDataModel.classId);

            if (_skillItems.Count > 0)
            {
                foreach (var s in _skillItems)
                {
                    Destroy(s.gameObject);
                }
            }
            _skillItems = new List<SkillItem>();
            
            var party = DataManager.Self().GetGameParty();
            for (int i = 0; i < party.Actors.Count; i++)
                if (party.Actors[i].ActorId == _runtimeActorDataModel.actorId)
                {
                    _actor = party.Actors[i];
                    break;
                }

            UpdateStatus();

            //共通のウィンドウの適応
            Init();

            //フォーカス設定
            ChangeFocusList(true);
        }

        /// <summary>
        /// パーティメンバーの選択画面表示
        /// </summary>
        private void OpenParty() {
            WindowType = Window.Party;
            ChangeFocusList(false);
        }

        /// <summary>
        /// パーティメンバーの選択画面終了
        /// </summary>
        private void CloseParty() {
            WindowType = Window.SkillCommand;
            UpdateStatus();
            ChangeFocusList(false);
        }

        /// <summary>
        /// リストのフォーカス位置を変更する
        /// </summary>
        private void ChangeFocusList(bool initialize) {
            if (WindowType == Window.SkillType)
            {
                //第一階層のメニューを選択可能とする
                int num = 0;
                
                for (var i = 0; i < _skillTypeButtons.Count; i++)
                {
                    _skillTypeButtons[i].GetComponent<WindowButtonBase>().SetEnabled(true);
                    if (_skillTypeButtons[i].GetComponent<WindowButtonBase>().IsHighlight())
                        num = i;
                }

                //第二階層のメニューは選択不可とする
                //ハイライト表示は初期化する
                for (var i = 0; i < _skillItems.Count; i++)
                {
                    var select = _skillItems[i].GetComponent<Button>();
                    select.GetComponent<WindowButtonBase>().SetEnabled(false, true);
                }

                //現在の状態に応じてフォーカス設定位置を変更する
                if (!initialize)
                    _skillTypeButtons[num].GetComponent<Button>().Select();
                else
                    _skillTypeButtons[0].GetComponent<Button>().Select();
            }
            else if (WindowType == Window.SkillCommand)
            {
                //第一階層のメニューは選択不可とする
                int num = 0;
                for (var i = 0; i < _skillTypeButtons.Count; i++)
                {
                    _skillTypeButtons[i].GetComponent<WindowButtonBase>().SetEnabled(false);
                }

                //第二階層のメニューを選択可能とする
                for (var i = 0; i < _skillItems.Count; i++)
                {
                    var select = _skillItems[i].GetComponent<Button>();
                    select.GetComponent<WindowButtonBase>().SetEnabled(true);
                    if (select.GetComponent<WindowButtonBase>().IsHighlight())
                        num = i;
                }

                //先頭にフォーカスをあてる
                if (_skillItems.Count > 0)
                {
                    int index = 0;

                    //最終選択したアクターを初期選択状態とする
                    if (DataManager.Self().GetRuntimeConfigDataModel()?.commandRemember == 1)
                        for (int i = 0; i < skillData.Count; i++)
                            if (_runtimeActorDataModel.lastMenuSkill.itemId == skillData[i].basic.id)
                            {
                                index = i;
                                break;
                            }
                            //カーソル記憶が働いてない場合はinitializeフラグを見る
                            else if (!initialize)
                                index = num;

                    _skillItems[index].GetComponent<Button>().Select();
                }
            }
            else
            {
                //第一階層のメニューは選択不可とする
                for (var i = 0; i < _skillTypeButtons.Count; i++)
                {
                    _skillTypeButtons[i].GetComponent<WindowButtonBase>().SetEnabled(false);
                }

                //第二階層のメニューは選択不可とする
                //フォーカスは初期化しない
                for (var i = 0; i < _skillItems.Count; i++)
                {
                    var select = _skillItems[i].GetComponent<Button>();
                    select.GetComponent<WindowButtonBase>().SetEnabled(false);
                }
            }
        }

        /// <summary>
        /// スキルボタン作成処理
        /// </summary>
        /// <param name="type"></param>
        public void CreateItem(int type) {
            if (_skillItems.Count > 0)
                foreach (var s in _skillItems)
                    Destroy(s.gameObject);
            _skillItems = new List<SkillItem>();

            var classIds = new List<string>();

            //現在付与されているステートに、行動制約「行動不能」が設定されている場合には、スキル使用不可
            bool disabledSkill = false;
            for (int i = 0; i < _runtimeActorDataModel.states.Count; i++)
            {
                if (DataManager.Self().GetStateDataModel(_runtimeActorDataModel.states[i].id).restriction == 4)
                {
                    disabledSkill = true;
                    break;
                }
            }

            skillData = new List<SkillCustomDataModel>();
            var skills = _actor.Skills();
            for (var i = 0; i < skills.Count; i++)
            {
                var flg = false;
                for (var j = 0; j < classIds.Count; j++)
                    if (classIds[j] == skills[i].basic.id)
                    {
                        flg = true;
                        break;
                    }

                if (!flg)
                {
                    var skill = DataManager.Self().GetSkillCustomDataModel(skills[i].basic.id);
                    if (skill != null)
                    {
                        if (skill.basic.skillType == type)
                            skillData.Add(skill);
                    }
                }
            }

            skillData.Sort((a, b) =>
            {
                return a.SerialNumber - b.SerialNumber;
            });
            
            for (var i = 0; i < skillData.Count; i++)
            {
                var obj = Instantiate(_originObj);
                obj.transform.SetParent(_originObj.transform.parent);
                obj.transform.localScale = _originObj.transform.localScale;
                obj.name = "Item" + i;
                obj.SetActive(true);
                var skill = obj.AddComponent<SkillItem>();
                skill.Init(this, skillData[i].basic.id, OpenParty, CloseParty);
                if (disabledSkill)
                {
                    skill.GetComponent<WindowButtonBase>().SetGray(true);
                }
                _skillItems.Add(skill);
            }

            if (_skillItems.Count == 0)
            {
                //0件の場合は空のアイテムを1つだけ追加する
                var obj = Instantiate(_originObj);
                obj.transform.SetParent(_originObj.transform.parent);
                obj.transform.localScale = _originObj.transform.localScale;
                obj.name = "Item1";
                obj.SetActive(true);
                var skill = obj.AddComponent<SkillItem>();
                skill.Init(this, "", null, CloseParty);
                _skillItems.Add(skill);
            }

            //十字キーでの操作登録
            var selects = _originObj.transform.parent.GetComponentsInChildren<Button>();
            if (selects.Length > 1)
            {
                for (var i = 0; i < selects.Length; i++)
                {
                    var nav = selects[i].navigation;
                    nav.mode = Navigation.Mode.Explicit;
                    //UIパターンに応じて十字キーを変更する
                    if (_pattern == 1 || _pattern == 2 || _pattern == 3 || _pattern == 4)
                    {
                        nav.selectOnLeft = selects[i == 0 ? selects.Length - 1 : i - 1];
                        nav.selectOnUp = selects[i < 2 ? selects.Length - Math.Abs(i - 2) : i - 2];
                        nav.selectOnRight = selects[(i + 1) % selects.Length];
                        nav.selectOnDown = selects[(i + 2) % selects.Length];
                    }
                    else
                    {
                        nav.selectOnUp = selects[i == 0 ? selects.Length - 1 : i - 1];
                        nav.selectOnDown = selects[(i + 1) % selects.Length];
                    }

                    selects[i].navigation = nav;
                    selects[i].targetGraphic = selects[i].transform.Find("Highlight").GetComponent<Image>();
                }
            }
            else if (selects.Length == 1)
            {
                var nav = selects[0].navigation;
                nav.mode = Navigation.Mode.None;
                selects[0].navigation = nav;
            }

            if (selects.Length > 0)
            {
                int index = 0;
                //最終選択したアクターを初期選択状態とする
                if (DataManager.Self().GetRuntimeConfigDataModel()?.commandRemember == 1)
                    for (int i = 0; i < skillData.Count; i++)
                        if (_runtimeActorDataModel.lastMenuSkill.itemId == skillData[i].basic.id)
                        {
                            index = i;
                            break;
                        }
                selects[index].Select();
            }

            WindowType = Window.SkillCommand;

            //フォーカス設定
            ChangeFocusList(true);
            
        }

        /// <summary>
        /// ステータス更新
        /// </summary>
        public void UpdateStatus() {
            SetLv();
            SetHp();
            SetMp();
            SetTp();
        }
        
        /// <summary>
        /// クラス名取得
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private string _GetClass(string id) {
            for (var i = 0; i < _classDataModels.Count; i++)
                if (_classDataModels[i].id == id)
                    return _classDataModels[i].basic.name;

            return "";
        }

        /// <summary>
        /// レベル設定
        /// </summary>
        private void SetLv() {
            _level.text = TextManager.levelA;
        }

        /// <summary>
        /// HP設定
        /// </summary>
        private void SetHp() {
            _hpBar.value = _actor.Hp / (float)_actor.Mhp;
            _hpValue.text = _actor.Hp.ToString();
            _hpName.text = TextManager.hpA;
        }

        /// <summary>
        /// MP設定
        /// </summary>
        private void SetMp() {
            _mpBar.value = _actor.Mp / (float) _actor.Mmp;
            _mpValue.text = _actor.Mp.ToString();
            _mpName.text = TextManager.mpA;
        }

        /// <summary>
        /// TP設定
        /// </summary>
        private void SetTp() {
            _tpBar.value = _actor.Tp / 100f;
            _tpValue.text = _actor.Tp.ToString();
            _tpName.text = TextManager.tpA;
        }

        /// <summary>
        /// 説明文設定
        /// </summary>
        /// <param name="text"></param>
        public void SetDescription(string text) {
            var width = _description1.transform.parent.GetComponent<RectTransform>().sizeDelta + _description1.GetComponent<RectTransform>().sizeDelta;
            _description1.text = "";
            _description2.text = "";
            
            if (text.Contains("\\n"))
            {
                var textList = text.Split("\\n");
                _description1.text = textList[0];
                if (width.x >= _description1.preferredWidth)
                {
                    _description2.text = textList[1];
                    return;
                }
                text = textList[0];
            }
            var isNextLine = false;
            _description1.text = "";
            _description2.text = "";
            for (int i = 0; i < text.Length; i++)
            {
                if (!isNextLine)
                {
                    _description1.text += text[i];
                    if (width.x <= _description1.preferredWidth)
                    {
                        var lastChara = _description1.text.Substring(_description1.text.Length - 1);
                        _description2.text += lastChara;
                        _description1.text = _description1.text.Remove(_description1.text.Length - 1);
                        isNextLine = true;
                    }
                }
                else
                {
                    _description2.text += text[i];
                    if (width.x <= _description2.preferredWidth)
                    {
                        _description2.text = _description2.text.Remove(_description2.text.Length - 1);
                        break;
                    }

                }

            }
        }

        /// <summary>
        /// アクター取得
        /// </summary>
        /// <returns></returns>
        public RuntimeActorDataModel GetActor() {
            return _runtimeActorDataModel;
        }

        /// <summary>
        /// アクターのID取得
        /// </summary>
        /// <returns></returns>
        public string ActorId() {
            return _runtimeActorDataModel.actorId;
        }

        /// <summary>
        /// 戻る操作
        /// </summary>
        public new void Back() {
            if (WindowType == Window.SkillType)
            {
                MenuBase.BackMenu();
            }
            else if (WindowType == Window.SkillCommand)
            {
                if (_skillItems.Count > 0)
                {
                    foreach (var s in _skillItems)
                    {
                        Destroy(s.gameObject);
                    }
                }
                _skillItems = new List<SkillItem>();

                _skillTypeButtons[0].Select();
                WindowType = Window.SkillType;

                //フォーカス設定
                ChangeFocusList(false);
            }
        }

        /// <summary>
        /// キャラクター切り替え
        /// </summary>
        /// <param name="isNext"></param>
        public void CharacterChange(bool isNext) {
            if (WindowType != Window.Party)
                _skillCommand.CharacterChange(isNext);
        }
    }
}