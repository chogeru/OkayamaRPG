using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map.Menu;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Map.Item
{
    public class EquipmentEquips : MonoBehaviour
    {

        /// <summary>
        /// スクロールの初期の高さ
        /// </summary>
        private const int EQIPSVALUES_SCROLL_DISPLAY_HEIGHT = 560;
        /// <summary>
        /// 装備品一つの高さ
        /// </summary>
        private const int EQIPSVALUES_ITEM_HEIGHT = 116;
        
        /// <summary>
        /// 別の窓に表示させる場合に、その窓ごとにオブジェクトを作成し、
        /// それぞれを表示非表示に切り替えるためのオブジェクトを作成する際の親オブジェクト
        /// </summary>
        [SerializeField] private GameObject _contentObject;
        /// <summary>
        /// 装備メニュー
        /// </summary>
        private EquipMenu _equipMenu;
        /// <summary>
        /// 装備オブジェクト 各部位ごとのリスト
        /// </summary>
        private List<GameObject> _equipObjects;
        /// <summary>
        /// 装備品
        /// </summary>
        private List<GameObject> _equipObjs = new List<GameObject>();

        /// <summary>
        /// 所持装備品
        /// </summary>
        private List<GameObject> _equipInventoryObjs = new List<GameObject>();

        /// <summary>
        /// 装備一覧
        /// </summary>
        private List<EquipmentItems> _itemses = new List<EquipmentItems>();
        /// <summary>
        /// 現在装備中のアイテムのClone元GameObject
        /// </summary>
        [SerializeField] private GameObject _itemsObject;
        /// <summary>
        /// 変更する装備アイテムのClone元GameObject
        /// </summary>
        [SerializeField] private GameObject _orijinGameObject;
        /// <summary>
        /// 現在装備中のアイテムを表示する親GameObject
        /// </summary>
        [SerializeField] private GameObject _parentObject;
        
        [SerializeField] private List<GameObject> _equipsObject;

        /// <summary>
        /// RuntimeActorDataModel
        /// </summary>
        private RuntimeActorDataModel _runtimeActorDataModel;
        /// <summary>
        /// SystemSettingDataModel
        /// </summary>
        private SystemSettingDataModel _systemSettingDataModel;
        /// <summary>
        /// 現在装備中のアイテムのButton
        /// </summary>
        private Button[] _selects;
        /// <summary>
        /// Windowステータス
        /// </summary>
        public enum Window
        {
            MENU = 0,
            EQUIP,
            SELECTITEM
        }
        /// <summary>
        /// 現在のWindowステータス
        /// </summary>
        private Window _state;
        /// <summary>
        /// 変更を試みている装備index番号
        /// </summary>
        private int _selectIndex;

        private ScrollRect _scrollRect;

        /// <summary>
        /// 現在の装備を表示
        /// </summary>
        /// <param name="runtimeActorDataModel"></param>
        /// <param name="equipMenu"></param>
        public void EquipsTypeDisplay(RuntimeActorDataModel runtimeActorDataModel, EquipMenu equipMenu) {
            //初期化処理
            _runtimeActorDataModel = runtimeActorDataModel;
            _equipMenu = equipMenu;
            _systemSettingDataModel = DataManager.Self().GetSystemDataModel();
            _state = Window.MENU;

            //リストの初期化
            if (_itemses == null) 
                _itemses = new List<EquipmentItems>();

            if (_equipObjs.Count > 0)
            {
                for (var i = 0; i < _equipObjs.Count; i++) Destroy(_equipObjs[i].gameObject);
                _equipObjs = new List<GameObject>();
            }

            _parentObject.SetActive(true);
            ResetEquipObject();

            // 装備オブジェクトを取得
            _equipObjects = new List<GameObject>();
            _equipObjects.Add(_equipsObject[0]);
            _equipObjects.Add(_equipsObject[1]);
            _equipObjects.Add(_equipsObject[2]);
            _equipObjects.Add(_equipsObject[3]);
            _equipObjects.Add(_equipsObject[4]);
            
            _scrollRect = _parentObject.transform.parent.parent.GetComponent<ScrollRect>();
            var equipCount = _runtimeActorDataModel.equips.Count - _parentObject.transform.childCount;
            if (equipCount > 0)
            {
                var equipValues = _parentObject.GetComponent<RectTransform>();
                var originObject = _equipsObject[4].gameObject;
                for (int i = 0; i < equipCount; i++)
                {
                    var obj = Instantiate(originObject, originObject.transform);
                    obj.transform.SetParent(originObject.transform.parent);
                    obj.name = "Equip" + (_equipObjects.Count + i + 1);
                    equipValues.sizeDelta = new Vector2(equipValues.sizeDelta.x, EQIPSVALUES_SCROLL_DISPLAY_HEIGHT + EQIPSVALUES_ITEM_HEIGHT * (i + 1));
                    _equipObjects.Add(obj);
                }
                _parentObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(_parentObject.GetComponent<RectTransform>().anchoredPosition.x, 0f);
            }

            //アクターが装備するものを頭から順に表示
            for (var i = 0; i < runtimeActorDataModel.equips.Count; i++)
            {
                int index = i;
                _equipObjects[i].SetActive(true);
                var equipmentItems = _equipObjects[i].GetComponent<EquipmentItems>();
                if (equipmentItems == null)
                {
                    equipmentItems = _equipObjects[i].AddComponent<EquipmentItems>();
                }

                //装備種別を取得
                SystemSettingDataModel.EquipType equipType = null;
                for (int i2 = 0; i2 < _systemSettingDataModel.equipTypes.Count; i2++)
                    if (_systemSettingDataModel.equipTypes[i2].id == runtimeActorDataModel.equips[i].equipType)
                    {
                        equipType = _systemSettingDataModel.equipTypes[i2];
                        break;
                    }

                //現在装備中のものを設定
                equipmentItems.NowEquips(equipType, runtimeActorDataModel, equipMenu, i);

                if (_equipObjects[i].GetComponent<WindowButtonBase>() != null)
                {
                    _equipObjects[i].GetComponent<WindowButtonBase>().ScrollView = _scrollRect.gameObject;
                    _equipObjects[i].GetComponent<WindowButtonBase>().Content = _parentObject;

                    //装備固定または、装備封印の特徴を持っている場合には、装備変更ボタンを押下不可とする
                    if (ItemManager.CheckTraitEquipLock(runtimeActorDataModel, equipType) ||
                        ItemManager.CheckTraitEquipSea(runtimeActorDataModel, equipType, i))
                    {
                        _equipObjects[i].GetComponent<WindowButtonBase>().SetGray(true);

                        //装備封印の場合、チェック時に装備を外しているため再設定
                        if (ItemManager.CheckTraitEquipSea(runtimeActorDataModel, equipType, i))
                        {
                            equipmentItems.NowEquips(equipType, runtimeActorDataModel, equipMenu, i);
                        }
                    }
                    else
                    {
                        _equipObjects[i].GetComponent<WindowButtonBase>().SetGray(false);
                        _equipObjects[i].GetComponent<WindowButtonBase>().OnFocus = new UnityEngine.Events.UnityEvent();
                        _equipObjects[i].GetComponent<WindowButtonBase>().OnFocus.AddListener(() =>
                        {
                            _equipMenu.MessageDisplay(equipmentItems.EquipsMessage);
                        });
                        _equipObjects[i].GetComponent<WindowButtonBase>().OnClick = new Button.ButtonClickedEvent();
                        _equipObjects[i].GetComponent<WindowButtonBase>().OnClick.AddListener(() =>
                        {
                            _selectIndex = index;
                            ItemSelect(equipmentItems.EquipId);
                        });
                    }
                }
                _itemses.Add(equipmentItems);
                ItemsClone(equipType.id, index);
            }

            var work = new List<Button>();
            foreach (var equipObject in _equipObjects)
            {
                work.Add(equipObject.GetComponent<Button>());
            }

            _selects = work.ToArray();

            for (var i = 0; i < _selects.Length; i++)
            {
                _selects[i].targetGraphic = _selects[i].gameObject.GetComponent<Image>();
                var nav = _selects[i].navigation;
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnUp = _selects[i == 0 ? _selects.Length - 1 : i - 1];
                nav.selectOnDown = _selects[(i + 1) % _selects.Length];

                int index = i;
                _selects[i].navigation = nav;
            }

            //フォーカス制御
            _state = Window.MENU;
            ChangeFocusList(true);
        }
        
        /// <summary>
        /// リストのフォーカス制御
        /// </summary>
        private void ChangeFocusList(bool initialize) {
            if (_state == Window.MENU)
            {
                //第一階層のメニューは選択不可とする
                for (var i = 0; i < _selects.Length; i++)
                {
                    _selects[i].GetComponent<WindowButtonBase>().SetEnabled(false, true);
                }
            }
            else if (_state == Window.EQUIP) 
            {
                //第一階層のメニューを選択可能とする
                int num = 0;
                for (var i = 0; i < _selects.Length; i++)
                {
                    _selects[i].GetComponent<WindowButtonBase>().SetEnabled(true);
                    if (_selects[i].GetComponent<WindowButtonBase>().IsHighlight())
                        num = i;
                }
                //現在の状態に応じてフォーカス設定位置を変更する
                if (!initialize)
                {
                    _selects[num].GetComponent<Button>().Select();
                    _selects[num].GetComponent<WindowButtonBase>().SetHighlight(true);
                    _equipMenu.MessageDisplay(_selects[num].GetComponent<EquipmentItems>().EquipsMessage);
                }
                else
                {
                    _selects[0].GetComponent<Button>().Select();
                    _selects[0].GetComponent<WindowButtonBase>().SetHighlight(true);
                    _equipMenu.MessageDisplay(_selects[0].GetComponent<EquipmentItems>().EquipsMessage);
                }
            }
            else
            {
                //第一階層のメニューは選択不可とする
                for (var i = 0; i < _selects.Length; i++)
                {
                    _selects[i].GetComponent<WindowButtonBase>().SetEnabled(false);
                }
            }
        }

        /// <summary>
        /// 装備選択
        /// </summary>
        public void SelectedEquip() {
            _state = Window.EQUIP;
            ChangeFocusList(false);
        }

        /// <summary>
        /// 戻る
        /// </summary>
        public void BackEquip() {
            _equipMenu.MessageDisplay("");
            if (_state == Window.SELECTITEM)
            {
                _state = Window.EQUIP;
                ChangeFocusList(false);
            }
            else
            {
                _state = Window.MENU;
                ChangeFocusList(false);
            }
        }

        /// <summary>
        /// 固定の装備タイプより追加されたものは削除する
        /// </summary>
        private void ResetEquipObject() {
            if (_equipObjects?.Count > 4)
            {
                for (int i = 5; i < _equipObjects.Count; i++)
                {
                    DestroyImmediate(_equipObjects[i].gameObject);
                }
            }
        }

        /// <summary>
        /// 現在装備中のアイテム表示
        /// </summary>
        /// <param name="typeId"></param>
        private void ItemsClone(string typeId, int equipIndex) {
            var cloneItem = Instantiate(_itemsObject);
            cloneItem.transform.SetParent(_contentObject.transform);
            cloneItem.transform.localPosition = _itemsObject.transform.localPosition;
            cloneItem.transform.localScale = _itemsObject.transform.localScale;
            cloneItem.SetActive(false);
            cloneItem.name = typeId;
            _equipObjs.Add(cloneItem);
            PossessionEquips(typeId, cloneItem, equipIndex);
        }

        /// <summary>
        /// 武器防具変更時にアイテムの一覧を表示する
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cloneItem"></param>
        private void PossessionEquips(string id, GameObject cloneItem, int equipIndex) {
            //各種初期化
            var weaponDataModels = DataManager.Self().GetWeaponDataModels();
            var armorDataModels = DataManager.Self().GetArmorDataModels();
            var classes = DataManager.Self().GetClassDataModels();
            ClassDataModel classData = null;
            for (int i = 0; i < classes.Count; i++)
                if (classes[i].id == _runtimeActorDataModel.classId)
                {
                    classData = classes[i];
                    break;
                }
            var runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();

            GameObject equips;
            string description;
            EquipmentItems equipmentItems;
            List<string> data;
            
            //武器のマスタデータを一通り確認する
            for (var i = 0; i < weaponDataModels.Count; i++)
                //装備部位が一致しているかどうか
                if (weaponDataModels[i].basic.equipmentTypeId == id)
                {
                    //該当の武器を所持しているかどうか
                    int count = 0;
                    for (var j = 0; j < runtimeSaveDataModel.runtimePartyDataModel.weapons.Count; j++)
                        if (runtimeSaveDataModel.runtimePartyDataModel.weapons[j].weaponId == weaponDataModels[i].basic.id)
                        {
                            count = runtimeSaveDataModel.runtimePartyDataModel.weapons[j].value;
                            break;
                        }

                    //所持していない場合は終了
                    if (count == 0)
                        continue;

                    //該当の職業がが装備できる武器タイプかどうか
                    var isMatch = ItemManager.CanEquipType(_runtimeActorDataModel, weaponDataModels[i].basic.weaponTypeId);

                    //装備できなければ終了
                    if (isMatch == false) 
                        continue;

                    //武器のボタンを生成
                    _orijinGameObject.SetActive(false);

                    equips = Instantiate(_orijinGameObject);
                    equips.transform.SetParent(cloneItem.transform);
                    equips.transform.localScale = _orijinGameObject.transform.localScale;
                    equips.SetActive(true);
                    equips.name = weaponDataModels[i].basic.name;

                    //押された際のイベント挿入
                    var parameters = weaponDataModels[i].parameters;
                    var itemId = weaponDataModels[i].basic.id;
                    

                    if (equips.GetComponent<WindowButtonBase>() != null)
                    {
                        equips.GetComponent<WindowButtonBase>().OnFocus = new UnityEngine.Events.UnityEvent();
                        var index = i;
                        equips.GetComponent<WindowButtonBase>().OnFocus.AddListener(() =>
                        {
                            description = weaponDataModels[index].basic.description;
                            _equipMenu.MessageDisplay(description);
                            EquipSelect(id, parameters);
                        });
                        equips.GetComponent<WindowButtonBase>().OnClick = new Button.ButtonClickedEvent();
                        equips.GetComponent<WindowButtonBase>().OnClick.AddListener(() =>
                        {
                            EquipChange(itemId);
                        });
                    }

                    equipmentItems = equips.AddComponent<EquipmentItems>();
                    data = new List<string>();
                    data.Add(weaponDataModels[i].basic.id);
                    data.Add(weaponDataModels[i].basic.name);
                    data.Add(count.ToString());
                    equipmentItems.EquipWindow(data);
                    _equipInventoryObjs.Add(equips);
                }

            //防具のマスタデータを一通り確認する
            for (var i = 0; i < armorDataModels.Count; i++)
                //装備部位が一致しているかどうか
                if (armorDataModels[i].basic.equipmentTypeId == id)
                {
                    //該当の防具を所持しているかどうか
                    var count = 0;
                    for (var j = 0; j < runtimeSaveDataModel.runtimePartyDataModel.armors.Count; j++)
                        if (runtimeSaveDataModel.runtimePartyDataModel.armors[j].armorId == armorDataModels[i].basic.id)
                        {
                            count = runtimeSaveDataModel.runtimePartyDataModel.armors[j].value;
                            break;
                        }

                    //所持していない場合は終了
                    if (count == 0)
                        continue;

                    //該当の職業がが装備できる防具タイプかどうか
                    var isMatch = ItemManager.CanEquipType(_runtimeActorDataModel, armorDataModels[i].basic.armorTypeId);

                    //装備できなければ終了
                    if (isMatch == false)
                        continue;

                    //防具のボタンを生成
                    equips = Instantiate(_orijinGameObject);
                    equips.transform.SetParent(cloneItem.transform);
                    equips.transform.localScale = _orijinGameObject.transform.localScale;
                    equips.SetActive(true);
                    equips.name = armorDataModels[i].basic.name;
                    //押された際のイベント挿入
                    var parameters = armorDataModels[i].parameters;
                    var itemId = armorDataModels[i].basic.id;

                    if (equips.GetComponent<WindowButtonBase>() != null)
                    {
                        equips.GetComponent<WindowButtonBase>().OnFocus = new UnityEngine.Events.UnityEvent();
                        var index = i;
                        equips.GetComponent<WindowButtonBase>().OnFocus.AddListener(() =>
                        {
                            description = armorDataModels[index].basic.description;
                            _equipMenu.MessageDisplay(description);
                            EquipSelect(id, parameters);
                        });
                        equips.GetComponent<WindowButtonBase>().OnClick = new Button.ButtonClickedEvent();
                        equips.GetComponent<WindowButtonBase>().OnClick.AddListener(() =>
                        {
                            EquipChange(itemId);
                        });

                        //装備した際の音は別なので、共通部品では鳴動しない
                        equips.GetComponent<WindowButtonBase>().SetSilentClick(true);
                    }

                    equipmentItems = equips.AddComponent<EquipmentItems>();
                    data = new List<string>();
                    data.Add(armorDataModels[i].basic.id);
                    data.Add(armorDataModels[i].basic.name);
                    data.Add(count.ToString());
                    equipmentItems.EquipWindow(data);
                    _equipInventoryObjs.Add(equips);

                }

            //リストの末尾に、外す用のボタンを作成
            _orijinGameObject.SetActive(false);

            equips = Instantiate(_orijinGameObject);
            equips.transform.SetParent(cloneItem.transform);
            equips.transform.localScale = _orijinGameObject.transform.localScale;
            equips.SetActive(true);
            equips.name = "";

            //押された際のイベント挿入
            description = "";

            if (equips.GetComponent<WindowButtonBase>() != null)
            {
                equips.GetComponent<WindowButtonBase>().OnFocus = new UnityEngine.Events.UnityEvent();
                equips.GetComponent<WindowButtonBase>().OnFocus.AddListener(() =>
                {
                    _equipMenu.MessageDisplay("");

                    var equipTypes = DataManager.Self().GetSystemDataModel().equipTypes;
                    string equipTypeId = null;
                    for (int i = 0; i < equipTypes.Count; i++)
                        if (equipTypes[i].id == _runtimeActorDataModel.equips[equipIndex].equipType)
                        {
                            equipTypeId = equipTypes[i].id;
                            break;
                        }
                    EquipSelect(equipTypeId, null);
                });
                equips.GetComponent<WindowButtonBase>().OnClick = new Button.ButtonClickedEvent();
                equips.GetComponent<WindowButtonBase>().OnClick.AddListener(() =>
                {
                    RemoveEquipment();
                });
            }
            _equipInventoryObjs.Add(equips);

            equipmentItems = equips.AddComponent<EquipmentItems>();
            data = new List<string>();
            data.Add("");
            data.Add(null);
            data.Add(null);
            equipmentItems.EquipWindow(data);
        }

        /// <summary>
        /// 武器防具装備の選択時処理
        /// </summary>
        /// <param name="equipTypeId"></param>
        /// <param name="parameters"></param>
        public void EquipSelect(
            string equipTypeId,
            List<int> parameters
        ) {
            _equipMenu.EqValuesDisplay(parameters, equipTypeId, _selectIndex);
        }

        /// <summary>
        /// 武器防具装備の変更時処理
        /// </summary>
        /// <param name="itemId"></param>
        public void EquipChange(string itemId) {
            //武器防具装備IDと装備タイプIDと武器か防具かが入ってくる
            _equipMenu.EquipChange(itemId, _selectIndex);
        }

        /// <summary>
        /// 装備を外す
        /// </summary>
        /// <param name="equipTypeId"></param>
        public void RemoveEquipment() {
            _equipMenu.RemoveEquip(_selectIndex);
        }

        /// <summary>
        /// 武器防具装備窓を開く
        /// </summary>
        /// <param name="id"></param>
        private void ItemSelect(string id) {
            //対象の武器防具装備のウィンドウオープン
            _parentObject.transform.parent.parent.gameObject.SetActive(false);

            var scrollView = gameObject.transform.Find("Scroll View").gameObject;
            var content = gameObject.transform.Find("Scroll View/Viewport/Content/" + id).gameObject;
            scrollView.SetActive(true);
            content.SetActive(true);

            scrollView.GetComponent<ScrollRect>().content = content.GetComponent<RectTransform>();
            
            var selects = gameObject.transform.Find("Scroll View/Viewport/Content").GetComponentsInChildren<Button>();

            for (var i = 0; i < selects.Length; i++)
            {
                selects[i].targetGraphic = selects[i].gameObject.GetComponent<Image>();
                selects[i].GetComponent<WindowButtonBase>().ScrollView = scrollView;
                selects[i].GetComponent<WindowButtonBase>().Content = content;

                var nav = selects[i].navigation;
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnUp = selects[i == 0 ? selects.Length - 1 : i - 1];
                nav.selectOnDown = selects[(i + 1) % selects.Length];
                selects[i].navigation = nav;
                selects[i].targetGraphic = selects[i].transform.Find("Highlight").GetComponent<Image>();
            }

            if (selects.Length > 0) selects[0].Select();
            
            Canvas.ForceUpdateCanvases();

            // enableを切り替えないとサイズの更新がされない
            content.transform.GetComponent<ContentSizeFitter>().enabled = false;
            content.transform.GetComponent<ContentSizeFitter>().enabled = true;

            //フォーカス制御
            _state = Window.SELECTITEM;
            ChangeFocusList(false);
        }

        /// <summary>
        /// 装備選択画面を閉じる処理
        /// </summary>
        public void EquipsSelectClose() {
            _parentObject.transform.parent.parent.gameObject.SetActive(true);
            //フォーカス制御
            _state = Window.EQUIP;
            ChangeFocusList(false);
        }

        /// <summary>
        /// 装備アイテムのフォーカス解除
        /// </summary>
        public void RemoveFocus() {
            for (var i = 0; i < _equipInventoryObjs.Count; i++)
                if(_equipInventoryObjs[i] != null)
                    _equipInventoryObjs[i]?.GetComponent<WindowButtonBase>()?.RemoveFocus();
        }
    }
}