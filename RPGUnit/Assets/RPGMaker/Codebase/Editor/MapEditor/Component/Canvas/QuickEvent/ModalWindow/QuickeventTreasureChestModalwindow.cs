using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Common.Window;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.Canvas.QuickEvent.ModalWindow
{
    public class QuickeventTreasureChestModalwindow : BaseModalWindow
    {
        // 初期価格
        private const int DEFAULT_GOLD = 10;

        // 最低、最高価格
        private const int             MIN_GOLD = 0;
        private const int             MAX_GOLD = 9999;
        private       VisualElement   _armor;
        private       string          _armorId;
        private       Button          _CANCEL_button;
        private       EventEditCanvas _eventEditCanvas;
        private       IntegerField    _gold;
        private       int             _goldValue;
        private       VisualElement   _image;
        private       Button          _imageButton;

        //returnする値
        private string        _imageName;
        private VisualElement _item;
        private string        _itemId;
        private string        _itemName;

        //表示要素
        private Button        _OK_button;
        private int           _selectNum;
        private List<RadioButton>  _toggles;
        private VisualElement _weapon;
        private string        _weaponId;

        protected override string ModalUxml =>
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Component/Canvas/QuickEvent/ModalWindow/Uxml/quickevent_treasure_chest_modalwindow.uxml";

        public void ShowWindow(
            EventEditCanvas eventEditCanvas,
            List<EventMapDataModel> eventList,
            MapDataModel mapDataModel,
            string modalTitle,
            CallBackWidow callBack
        ) {
            var wnd = GetWindow<QuickeventTreasureChestModalwindow>();

            _eventEditCanvas = eventEditCanvas;

            if (callBack != null) _callBackWindow = callBack;

            wnd.titleContent = new GUIContent(modalTitle);
            wnd.Init();
            //サイズ固定用
            var size = new Vector2(400, 185);
            wnd.minSize = size;
            wnd.maxSize = size;
            wnd.maximized = false;
        }

        public override void Init() {
            var root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModalUxml);
            VisualElement labelFromUxml = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUxml);
            root.Add(labelFromUxml);

            // 要素取得
            // radioButton-other-display1 ~ radioButton-other-display4
            _toggles = labelFromUxml.Query<RadioButton>().ToList();
            _gold = labelFromUxml.Q<IntegerField>("gold");
            _item = labelFromUxml.Q<VisualElement>("item");
            _weapon = labelFromUxml.Q<VisualElement>("weapon");
            _armor = labelFromUxml.Q<VisualElement>("armor");

            // アイテム設定
            SetItems();


            // 画像選択
            _imageButton = labelFromUxml.Query<Button>("button");
            _image = labelFromUxml.Query<VisualElement>("image");

            // 基本データを取得する
            var _databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
            var manageData = _databaseManagementService.LoadAssetManage();
            AssetManageDataModel defaultData = null;
            for (int i = 0; i < manageData.Count; i++)
                if (manageData[i].id == "1dd21a8b-d674-4788-a9ff-a83f252c8bb2")
                {
                    defaultData = manageData[i];
                    break;
                }
            if (defaultData != null)
            {
                _imageName = "1dd21a8b-d674-4788-a9ff-a83f252c8bb2";
                ImageSetting(ImageManager.LoadSvCharacter(_imageName));
            }

            _imageButton.clickable.clicked += () =>
            {
                var sdSelectModalWindow = new SdSelectModalWindow();
                sdSelectModalWindow.CharacterSdType = SdSelectModalWindow.CharacterType.Map;
                sdSelectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select IconImage"), data =>
                {
                    var imageName = (string) data;
                    var imageCharacter = ImageManager.LoadSvCharacter(imageName);
                    ImageSetting(imageCharacter);
                    _imageName = imageName;
                }, _imageName);
            };

            //OKボタン
            _OK_button = labelFromUxml.Query<Button>("OK_button");
            _OK_button.clicked += () =>
            {
                //決定されたのでポップアップのデータを渡す
                var returnValue = new List<object>
                {
                    _imageName,
                    _selectNum,
                    _goldValue,
                    _itemId,
                    _weaponId,
                    _armorId,
                    _itemName
                };
                _callBackWindow(returnValue);
                _callBackWindow = null;
                Close();
            };

            //CANCELボタン
            _CANCEL_button = labelFromUxml.Query<Button>("CANCEL_button");
            _CANCEL_button.clicked += () =>
            {
                //入っている初期値で決定にするためこのままこのウィンドウを閉じる
                _callBackWindow(null);
                _callBackWindow = null;
                Close();
            };
        }

        // 各取得アイテム設定
        private void SetItems() {
            _selectNum = 0;

            // アイテム系データ取得
            var databaseManagement = Editor.Hierarchy.Hierarchy.databaseManagementService;

            // データ読み込み
            var items =
                databaseManagement.LoadItem();
            var weapons =
                databaseManagement.LoadWeapon();
            var armors =
                databaseManagement.LoadArmor();

            var itemsText = new List<string>();
            var weaponsText = new List<string>();
            var armorsText = new List<string>();

            // 名前を取得
            for (var i = 0; i < items.Count; i++)
                itemsText.Add(items[i].basic.name);
            for (var i = 0; i < weapons.Count; i++)
                weaponsText.Add(weapons[i].basic.name);
            for (var i = 0; i < armors.Count; i++)
                armorsText.Add(armors[i].basic.name);

            var itemPopup = new PopupFieldBase<string>(itemsText, 0);
            var weaponPopup = new PopupFieldBase<string>(weaponsText, 0);
            var armorPopup = new PopupFieldBase<string>(armorsText, 0);

            // 選択時にIDを設定
            itemPopup.RegisterValueChangedCallback(evt =>
            {
                _itemId = items[itemPopup.index].basic.id;
                _itemName = items[itemPopup.index].basic.name;
            });
            weaponPopup.RegisterValueChangedCallback(evt =>
            {
                _weaponId = weapons[weaponPopup.index].basic.id;
                _itemName = weapons[weaponPopup.index].basic.name;
            });
            armorPopup.RegisterValueChangedCallback(evt =>
            {
                _armorId = armors[armorPopup.index].basic.id;
                _itemName = armors[armorPopup.index].basic.name;
            });

            _item.Add(itemPopup);
            _weapon.Add(weaponPopup);
            _armor.Add(armorPopup);

            // 価格設定
            _goldValue = DEFAULT_GOLD;
            _gold.value = _goldValue;
            _gold.RegisterValueChangedCallback(evt =>
            {
                if (_gold.value < MIN_GOLD)
                    _gold.value = MIN_GOLD;
                else if (_gold.value > MAX_GOLD)
                    _gold.value = MAX_GOLD;
                _goldValue = _gold.value;
                _itemName = _goldValue + EditorLocalize.LocalizeText("WORD_0155");
            });

            // チェックボックスの選択が切り替わった際の処理（enable切替）
            var actions = new List<Action>
            {
                // 金額
                () =>
                {
                    _gold.SetEnabled(true);
                    _item.SetEnabled(false);
                    _weapon.SetEnabled(false);
                    _armor.SetEnabled(false);
                    _selectNum = 0;
                    _itemName = _goldValue + "\\G";
                },
                // アイテム
                () =>
                {
                    _gold.SetEnabled(false);
                    _item.SetEnabled(true);
                    _weapon.SetEnabled(false);
                    _armor.SetEnabled(false);
                    _selectNum = 1;
                    _itemId = items[itemPopup.index].basic.id;
                    _itemName = items[itemPopup.index].basic.name;
                },
                // 武器
                () =>
                {
                    _gold.SetEnabled(false);
                    _item.SetEnabled(false);
                    _weapon.SetEnabled(true);
                    _armor.SetEnabled(false);
                    _selectNum = 2;
                    _weaponId = weapons[weaponPopup.index].basic.id;
                    _itemName = weapons[weaponPopup.index].basic.name;
                },
                // 防具
                () =>
                {
                    _gold.SetEnabled(false);
                    _item.SetEnabled(false);
                    _weapon.SetEnabled(false);
                    _armor.SetEnabled(true);
                    _selectNum = 3;
                    _armorId = armors[armorPopup.index].basic.id;
                    _itemName = armors[armorPopup.index].basic.name;
                }
            };

            // トグルのセレクター設定
            new CommonToggleSelector().SetRadioSelector(_toggles, 0, actions);
        }

        // 画像設定
        private void ImageSetting(Texture2D tex) {
            // ウィンドウサイズ取得
            var width = _imageButton.layout.width;
            var height = _imageButton.layout.height;
            if (float.IsNaN(width)) width = 65;
            if (float.IsNaN(height)) height = 80;

            BackgroundImageHelper.SetBackground(_image, new Vector2(width, height), tex);
        }

        private void OnDestroy() {
            // ウィンドウが閉じられた際にcallbackが呼ばれていなければ呼ぶ
            if (_callBackWindow != null) _callBackWindow(null);
        }
    }
}