using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map.Item;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TextMP = TMPro.TextMeshProUGUI;

namespace RPGMaker.Codebase.Runtime.Map.Menu
{
    public class StatusMenu : WindowBase
    {
        /// <summary>
        /// スクロールの初期の高さ
        /// </summary>
        private const int EQIPSVALUES_SCROLL_DISPLAY_HEIGHT = 420;
        /// <summary>
        /// 装備品一つの高さ
        /// </summary>
        private const int EQIPSVALUES_ITEM_HEIGHT = 85;
        
        [SerializeField] private GameObject _equipGameObject;
        [SerializeField] private GameObject _statusObject;
        [SerializeField] private Text       _StNameAg = null;

        //ステータスの項目部分
        [SerializeField] private Text   _StNameAt  = null;
        [SerializeField] private Text   _StNameDf  = null;
        [SerializeField] private Text   _StNameLu  = null;
        [SerializeField] private Text   _StNameMa  = null;
        [SerializeField] private Text   _StNameMd  = null;
        [SerializeField] private TextMP _StValueAg = null;

        //ステータス部分
        [SerializeField] private TextMP _StValueAt = null;
        [SerializeField] private TextMP _StValueDf = null;
        [SerializeField] private TextMP _StValueLu = null;
        [SerializeField] private TextMP _StValueMa = null;
        [SerializeField] private TextMP _StValueMd = null;

        
        private MenuBase                  _base;
        private ClassDataModel            _classDataModel;
        private List<ClassDataModel>      _classDataModels;
        private DatabaseManagementService _databaseManagementService;

        private TextMP _description1 = null;
        private TextMP _description2 = null;

        // 装備アイテム表示項目
        private List<GameObject> _equipObjects = new List<GameObject>();


        //装備一覧
        private List<EquipmentItems> _items = new List<EquipmentItems>();
        private Text                 _nextExp;

        //経験値の表示部分
        private Text _nowExp;

        //表示させるactorがPartyの何番目かを保持する
        private int _partyNumber;

        //データ部分
        private RuntimeActorDataModel _runtimeActorDataModel;

        public void Init(MenuBase @base, string actorId) {
            _base = @base;
            _databaseManagementService = new DatabaseManagementService();
            _classDataModels = _databaseManagementService.LoadClassCommon();

            // 装備オブジェクトを取得
            _equipObjects.Add(_equipGameObject.transform.Find("Scroll View/Viewport/EquipValues/Weapon").gameObject);
            _equipObjects.Add(_equipGameObject.transform.Find("Scroll View/Viewport/EquipValues/Shield").gameObject);
            _equipObjects.Add(_equipGameObject.transform.Find("Scroll View/Viewport/EquipValues/Head").gameObject);
            _equipObjects.Add(_equipGameObject.transform.Find("Scroll View/Viewport/EquipValues/Body").gameObject);
            _equipObjects.Add(_equipGameObject.transform.Find("Scroll View/Viewport/EquipValues/Accessory").gameObject);

            var window = transform.Find("MenuArea/StatusWindow").gameObject;
            
            _description1 = transform.Find("MenuArea/Description/DescriptionText1").GetComponent<TextMP>();
            _description2 = transform.Find("MenuArea/Description/DescriptionText2").GetComponent<TextMP>();
            
            var characterItem = _statusObject.transform.Find("Status").gameObject.AddComponent<CharacterItem>();
            var actors = DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels;
            RuntimeActorDataModel actor = null;
            for (int i = 0; i < actors.Count; i++)
                if (actors[i].actorId == actorId)
                {
                    actor = actors[i];
                    break;
                }
            characterItem.Init(actor);

            var party = DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.actors;
            for (int i = 0; i < party.Count; i++)
                if (party[i] == actorId)
                {
                    _partyNumber = i;
                    break;
                }

            _runtimeActorDataModel = characterItem.RuntimeActorDataModel;

            //NoneというGameObjectがあるため、-1する
            var equipCount = _runtimeActorDataModel.equips.Count - (_equipGameObject.transform.Find("Scroll View/Viewport/EquipValues").childCount - 1);
            if (equipCount > 0)
            {
                var scrollRect = _equipGameObject.transform.Find("Scroll View").GetComponent<ScrollRect>();
                var equipValues = _equipGameObject.transform.Find("Scroll View/Viewport/EquipValues").GetComponent<RectTransform>();
                var originObject = _equipGameObject.transform.Find("Scroll View/Viewport/EquipValues/Accessory").gameObject;
                for (int i = 0; i < equipCount; i++)
                {
                    var obj = Instantiate(originObject, originObject.transform);
                    obj.transform.SetParent(originObject.transform.parent);
                    obj.name = "Equip" + (_equipObjects.Count + i + 1);
                    obj.transform.SetSiblingIndex(_equipObjects.Count);
                    equipValues.sizeDelta = new Vector2(equipValues.sizeDelta.x, EQIPSVALUES_SCROLL_DISPLAY_HEIGHT + EQIPSVALUES_ITEM_HEIGHT * (i + 1));
                    _equipObjects.Add(obj);
                }
                equipValues.GetComponent<RectTransform>().anchoredPosition = new Vector2(equipValues.GetComponent<RectTransform>().anchoredPosition.x, 0f);
            }
            
            _classDataModel = _classDataModels.FirstOrDefault(v => v.id == _runtimeActorDataModel.classId);

            StatusValuesDisplay();

            //各項目の有効化切り替え
            ChangeStatusDisplay();

            EquipsTypeDisplay(_runtimeActorDataModel);
            MessageDisplay(_runtimeActorDataModel.profile);

            //ステータスの項目名の表示
            StNamesDisplay();
            //経験値の左側の部分の表示
            ShowExp();
            //共通のウィンドウの適応
            Init();
        }

        private void StatusValuesDisplay() {
            _StValueAt.text = _runtimeActorDataModel.GetCurrentAttack(_classDataModel).ToString();
            _StValueDf.text = _runtimeActorDataModel.GetCurrentDefense(_classDataModel).ToString();
            _StValueMa.text = _runtimeActorDataModel.GetCurrentMagicAttack(_classDataModel).ToString();
            _StValueMd.text = _runtimeActorDataModel.GetCurrentMagicDefense(_classDataModel).ToString();
            _StValueAg.text = _runtimeActorDataModel.GetCurrentAgility(_classDataModel).ToString();
            _StValueLu.text = _runtimeActorDataModel.GetCurrentLuck(_classDataModel).ToString();
        }

        //ステータスの項目の表示
        private void StNamesDisplay() {
            _StNameAt.text = TextManager.stAttack;
            _StNameDf.text = TextManager.stGuard;
            _StNameMa.text = TextManager.stMagic;
            _StNameMd.text = TextManager.stMagicGuard;
            _StNameAg.text = TextManager.stSpeed;
            _StNameLu.text = TextManager.stLuck;
        }

        //各項目の有効化切り替え
        private void ChangeStatusDisplay() {
            _StNameMa.gameObject.SetActive(_classDataModel.basic.abilityEnabled.magicAttack == 1);
            _StNameMd.gameObject.SetActive(_classDataModel.basic.abilityEnabled.magicDefense == 1);
            _StNameAg.gameObject.SetActive(_classDataModel.basic.abilityEnabled.speed == 1);
            _StNameLu.gameObject.SetActive(_classDataModel.basic.abilityEnabled.luck == 1);

            transform.Find("MenuArea/StatusWindow/Status/Mp").gameObject
                .SetActive(_classDataModel.basic.abilityEnabled.mp == 1);
            transform.Find("MenuArea/StatusWindow/Status/Tp").gameObject
                .SetActive(_classDataModel.basic.abilityEnabled.tp == 1);
        }

        public void EquipsTypeDisplay(RuntimeActorDataModel runtimeActorDataModel) {
            systemSettingDataModel = DataManager.Self().GetSystemDataModel();

            //リストが入っていたら、一度消す
            if (_items?.Count > 0)
            {
                _items.Clear();
                _items = null;
                _items = new List<EquipmentItems>();
            }

            //装備するものを頭から順に表示
            for (var i = 0; i < runtimeActorDataModel.equips.Count; i++)
            {
                var equipmentItems = _equipObjects[i].GetComponent<EquipmentItems>() ??
                                     _equipObjects[i].AddComponent<EquipmentItems>();

                SystemSettingDataModel.EquipType equipTypeData = null;
                for (int i2 = 0; i2 < systemSettingDataModel.equipTypes.Count; i2++)
                    if (systemSettingDataModel.equipTypes[i2].id == runtimeActorDataModel.equips[i].equipType)
                    {
                        equipTypeData = systemSettingDataModel.equipTypes[i2];
                        break;
                    }
                equipmentItems.NowEquips(equipTypeData, runtimeActorDataModel, null, i);
                _items.Add(equipmentItems);
            }
        }

        public void MessageDisplay(string message) {
            var width = _description1.transform.parent.GetComponent<RectTransform>().sizeDelta +
                        _description1.GetComponent<RectTransform>().sizeDelta;
            _description1.text = "";
            _description2.text = "";
            if (message.Contains("\\n"))
            {
                var textList = message.Split("\\n");
                _description1.text = textList[0];
                if (width.x >= _description1.preferredWidth)
                {
                    _description2.text = textList[1];
                    return;
                }
                message = textList[0];
            }
            var isNextLine = false;
            _description1.text = "";
            _description2.text = "";
            for (int i = 0; i < message.Length; i++)
            {
                if (!isNextLine)
                {
                    _description1.text += message[i];
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
                    _description2.text += message[i];
                    if (width.x <= _description2.preferredWidth)
                    {
                        _description2.text = _description2.text.Remove(_description2.text.Length - 1);
                        break;
                    }

                }

            }
        }

        public void changeCharactor(string calculation) {
            switch (calculation)
            {
                case "plus":
                    _partyNumber++;
                    break;
                case "minus":
                    _partyNumber--;
                    break;
            }

            var partyCount = DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.actors.Count;
            if (_partyNumber < 0) _partyNumber = partyCount - 1;
            else if (_partyNumber >= partyCount) _partyNumber = 0;

            Reload();
        }

        public new void Back() {
            _base.BackMenu();
        }

        private void Reload() {
            var window = transform.Find("MenuArea/StatusWindow").gameObject;

            var characterItem = window.transform.Find("Status").gameObject.GetComponent<CharacterItem>();
            var actorIds = DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.actors;
            var actors = DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels;
            for (int i = 0; i < actors.Count; i++)
            {
                if (actorIds[_partyNumber] == actors[i].actorId)
                {
                    characterItem.Init(actors[i]);
                }
            }

            _runtimeActorDataModel = characterItem.RuntimeActorDataModel;
            _classDataModel = _classDataModels.FirstOrDefault(v => v.id == _runtimeActorDataModel.classId);

            MessageDisplay(_runtimeActorDataModel.profile);
            StatusValuesDisplay();
            EquipsTypeDisplay(_runtimeActorDataModel);
            ShowExp();
        }

        /// <summary>
        /// 経験値関連表示
        /// </summary>
        private void ShowExp() {
            _nowExp = _statusObject.transform.Find("Exp").GetComponent<Text>();
            _nextExp = _statusObject.transform.Find("NextExp").GetComponent<Text>();
            _nowExp.text = TextManager.expTotal.Replace("%1", TextManager.expA);
            _nextExp.text = TextManager.expNext.Replace("%1", TextManager.levelA);

            var gameActor = DataManager.Self().GetGameActors().Actor(_runtimeActorDataModel);

            if (_runtimeActorDataModel.level < gameActor.MaxLevel())
            {
                _statusObject.transform.Find("Exp/Number").GetComponent<TextMP>().text = gameActor.CurrentExp().ToString();
                _statusObject.transform.Find("NextExp/Number").GetComponent<TextMP>().text = gameActor.NextRequiredExp().ToString();
            }
            else
            {
                _statusObject.transform.Find("Exp/Number").GetComponent<TextMP>().text = "______";
                _statusObject.transform.Find("NextExp/Number").GetComponent<TextMP>().text = "______";
            }
        }
    }
}