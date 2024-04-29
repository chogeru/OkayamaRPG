using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Common.Window;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.Canvas.QuickEvent.ModalWindow
{
    public class QuickeventToolShopModalwindow : BaseModalWindow
    {
        //初動で表示する画像名
        private const string _initialImageName = "100370ee-0948-45ab-af03-681a93fea5c5";

        //カテゴリーのプルダウンの横幅
        private const int _categoryWidth = 200;

        //アイテム名のプルダウンの横幅
        private const int                  _itemNameWidth = 200;
        private       Button               _addButton;
        private       List<ArmorDataModel> _armorDataModels;

        //データ保持用
        private DatabaseManagementService _databaseManagementService;
        private VisualElement             _image;
        private Button                    _imageButton;

        //returnする値
        private          string                     _imageName;
        private          Toggle                     _isBuy;
        private          int                        _IsBuy;
        private          List<ItemDataModel>        _itemDataModels;
        private readonly List<object>               _itemList = new List<object>();
        private          VisualElement              _items;
        private          List<AssetManageDataModel> _manageData;
        private          List<WeaponDataModel>      _weaponDataModels;
        private          Button                     CANCEL_button;

        //追加されるアイテムのuxml
        private readonly string ItemUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Component/Canvas/QuickEvent/ModalWindow/Uxml/quickevent_tool_shop_items.uxml";

        //表示要素
        private Button OK_button;


        protected override string ModalUxml =>
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Component/Canvas/QuickEvent/ModalWindow/Uxml/quickevent_tool_shop_modalwindow.uxml";

        public void ShowWindow(
            EventEditCanvas eventEditCanvas,
            List<EventMapDataModel> eventList,
            MapDataModel mapDataModel,
            string modalTitle,
            CallBackWidow callBack
        ) {
            var wnd = GetWindow<QuickeventToolShopModalwindow>();

            if (callBack != null) _callBackWindow = callBack;

            wnd.titleContent = new GUIContent(modalTitle);
            wnd.Init();
            //ウィンドウサイズ固定用
            var size = new Vector2(630, 360);
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

            var itemVisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ItemUxml);
            VisualElement itemLabelFromUxml = itemVisualTree.CloneTree();
            EditorLocalize.LocalizeElements(itemLabelFromUxml);

            //データの取得
            _databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
            _itemDataModels = _databaseManagementService.LoadItem();
            _weaponDataModels = _databaseManagementService.LoadWeapon();
            _armorDataModels = _databaseManagementService.LoadArmor();
            _manageData = _databaseManagementService.LoadAssetManage();


            // 画像選択
            _imageButton = labelFromUxml.Query<Button>("button");
            _image = labelFromUxml.Query<VisualElement>("image");

            //素材のdefaultデータの取得
            AssetManageDataModel defaultData = null;
            for (int i = 0; i < _manageData.Count; i++)
                if (_manageData[i].id == _initialImageName)
                {
                    defaultData = _manageData[i];
                    break;
                }

            if (defaultData != null)
            {
                _imageName = defaultData.id;
                ImageSetting(ImageManager.LoadSvCharacter(defaultData.id));
            }
            else
            {
                _imageName = "";
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

            //購入のみトグル
            _isBuy = labelFromUxml.Query<Toggle>("buy_toggle");
            _isBuy.RegisterValueChangedCallback(o => { _IsBuy = _isBuy.value ? 1 : 0; });

            //道具屋に並ぶアイテム部分
            _items = labelFromUxml.Query<VisualElement>("item_aria");
            _addButton = labelFromUxml.Query<Button>("add");
            _addButton.clicked += () =>
            {
                //表示の追加
                VisualElement itemLabelFromUxml = itemVisualTree.CloneTree();
                EditorLocalize.LocalizeElements(itemLabelFromUxml);
                ItemPage(itemLabelFromUxml);
            };

            //OKボタン
            OK_button = labelFromUxml.Query<Button>("OK_button");
            OK_button.clicked += () =>
            {
                //決定されたのでポップアップのデータを渡す
                var returnValue = new List<object>
                {
                    _imageName,
                    _IsBuy,
                    _itemList
                };
                _callBackWindow(returnValue);
                _callBackWindow = null;
                Close();
            };

            //CANCELボタン
            CANCEL_button = labelFromUxml.Query<Button>("CANCEL_button");
            CANCEL_button.clicked += () =>
            {
                //入っている初期値で決定にするためこのままこのウィンドウを閉じる
                _callBackWindow(null);
                _callBackWindow = null;
                Close();
            };
        }

        //アイテムの追加uxmlの処理
        //引数uxml そのデータ
        private void ItemPage(VisualElement Items) {
            //表示の追加を実施
            _items.Add(Items);
            //返信用のListにデータの追加を実施
            //データの追加(初期値は全部「0」)
            //0.アイテムカテゴリー 1.アイテムID 2.標準か指定か 3.指定の値段
            var data = new List<string>
            {
                "0", "0", "0", "0"
            };
            _itemList.Add(data);

            var itemTextList = new List<string>();
            var itemIdList = new List<string>();
            switch (int.Parse(data[0]))
            {
                case 0:
                    foreach (var itemdata in _itemDataModels)
                    {
                        itemTextList.Add(itemdata.basic.name);
                        itemIdList.Add(itemdata.basic.id);
                    }

                    break;
                case 1:
                    foreach (var weapondata in _weaponDataModels)
                    {
                        itemTextList.Add(weapondata.basic.name);
                        itemIdList.Add(weapondata.basic.id);
                    }

                    break;
                case 2:
                    foreach (var armordata in _armorDataModels)
                    {
                        itemTextList.Add(armordata.basic.name);
                        itemIdList.Add(armordata.basic.id);
                    }

                    break;
            }

            var selectID = 0;

            var classificationList =
                EditorLocalize.LocalizeTexts(new List<string> {"WORD_0068", "WORD_0128", "WORD_0129"});
            var classificationPopupField = new PopupFieldBase<string>(classificationList, selectID);

            // アイテムIDからindexを取得
            int parse;
            switch (classificationList.IndexOf(classificationPopupField.value))
            {
                case 0:
                    // 値が数値の場合
                    if (int.TryParse(data[1], out parse) &&
                        _itemDataModels.Count > int.Parse(data[1]))
                        selectID = int.Parse(data[1]);
                    // 値がID
                    else
                        for (var i = 0; i < _itemDataModels.Count; i++)
                            if (_itemDataModels[i].basic.id == data[1])
                                selectID = i;
                    break;
                case 1:
                    // 値が数値の場合
                    if (int.TryParse(data[1], out parse) &&
                        _weaponDataModels.Count > int.Parse(data[1]))
                        selectID = int.Parse(data[1]);
                    // 値がID
                    else
                        for (var i = 0; i < _weaponDataModels.Count; i++)
                            if (_weaponDataModels[i].basic.id == data[1])
                                selectID = i;
                    break;
                case 2:
                    // 値が数値の場合
                    if (int.TryParse(data[1], out parse) &&
                        _armorDataModels.Count > int.Parse(data[1]))
                        selectID = int.Parse(data[1]);
                    // 値がID
                    else
                        for (var i = 0; i < _armorDataModels.Count; i++)
                            if (_armorDataModels[i].basic.id == data[1])
                                selectID = i;
                    break;
            }

            var itemListPopupField = new PopupFieldBase<string>(itemTextList, selectID);
            VisualElement itemList = Items.Query<VisualElement>("itemList");

            VisualElement classification = Items.Query<VisualElement>("classification");
            classification.style.width = _categoryWidth;
            classification.Clear();
            classification.Add(classificationPopupField);
            classificationPopupField.RegisterValueChangedCallback(evt =>
            {
                data[0] = classificationPopupField.index.ToString();
                itemTextList.Clear();
                itemIdList.Clear();
                var idWork = "0";
                switch (classificationList.IndexOf(classificationPopupField.value))
                {
                    case 0:
                        foreach (var data in _itemDataModels)
                        {
                            itemTextList.Add(data.basic.name);
                            itemIdList.Add(data.basic.id);
                            if (itemIdList.Count == 1) idWork = data.basic.id;
                        }

                        break;
                    case 1:
                        foreach (var data in _weaponDataModels)
                        {
                            itemTextList.Add(data.basic.name);
                            itemIdList.Add(data.basic.id);
                            if (itemIdList.Count == 1) idWork = data.basic.id;
                        }

                        break;
                    case 2:
                        foreach (var data in _armorDataModels)
                        {
                            itemTextList.Add(data.basic.name);
                            itemIdList.Add(data.basic.id);
                            if (itemIdList.Count == 1) idWork = data.basic.id;
                        }

                        break;
                }

                data[1] = idWork;

                itemListPopupField = new PopupFieldBase<string>(itemTextList, selectID);
                itemList.Clear();
                itemList.Add(itemListPopupField);
                itemListPopupField.RegisterValueChangedCallback(evt =>
                {
                    data[1] = itemIdList[itemListPopupField.index];
                });
            });

            itemList.Clear();
            itemList.style.width = _itemNameWidth;
            itemList.Add(itemListPopupField);
            itemListPopupField.RegisterValueChangedCallback(evt =>
            {
                data[1] = itemIdList[itemListPopupField.index];
            });

            RadioButton standard = Items.Query<RadioButton>("radioButton-eventCommand-price1");
            RadioButton designation = Items.Query<RadioButton>("radioButton-eventCommand-price2");
            IntegerField num = Items.Query<IntegerField>("num");
            if (data[2] == "0")
                standard.value = true;
            else
                designation.value = true;
            standard.RegisterValueChangedCallback(o =>
            {
                if (!standard.value)
                {
                    standard.value = false;
                    designation.value = true;
                }

                if (standard.value)
                {
                    standard.value = true;
                    designation.value = false;
                    data[2] = "0";
                }

                num.SetEnabled(designation.value);
            });
            designation.RegisterValueChangedCallback(o =>
            {
                if (!designation.value)
                {
                    designation.value = false;
                    standard.value = true;
                }

                if (designation.value)
                {
                    designation.value = true;
                    standard.value = false;
                    data[2] = "1";
                }

                num.SetEnabled(designation.value);
            });
            num.value = int.Parse(data[3]);
            num.RegisterCallback<FocusOutEvent>(evt => { data[3] = num.value.ToString(); });

            Button delete = Items.Query<Button>("delete");
            delete.clicked += () =>
            {
                //表示の削除を実施
                _items.Remove(Items);
                //データの削除を実施
                _itemList.Remove(data);
            };
        }

        // 画像設定
        private void ImageSetting(Texture2D tex) {
            BackgroundImageHelper.SetBackground(_image, new Vector2(66, 76), tex, LengthUnit.Pixel);
        }

        private void OnDestroy() {
            // ウィンドウが閉じられた際にcallbackが呼ばれていなければ呼ぶ
            if (_callBackWindow != null) _callBackWindow(null);
        }
    }
}