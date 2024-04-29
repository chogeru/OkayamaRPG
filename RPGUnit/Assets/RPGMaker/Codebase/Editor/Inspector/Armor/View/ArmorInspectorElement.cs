using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using RPGMaker.Codebase.Editor.Hierarchy.Enum;
using RPGMaker.Codebase.Editor.Inspector.Trait.View;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.Editor.Inspector.Trait.View.TraitsInspectorElement;

namespace RPGMaker.Codebase.Editor.Inspector.Armor.View
{
    /// <summary>
    /// [装備・アイテムの編集]-[防具] Inspector
    /// </summary>
    public class ArmorInspectorElement : AbstractInspectorElement
    {
        private ArmorDataModel _armorDataModel;

        private          List<ArmorDataModel>      _armorDataModels;
        private          List<ClassDataModel>      _classDataModels;

        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Armor/Asset/inspector_armorEdit.uxml"; } }

        public ArmorInspectorElement(ArmorDataModel armorDataModel) {
            _armorDataModel = armorDataModel;
            Refresh();
        }

        /// <summary>
        /// データの更新
        /// </summary>
        override protected void RefreshContents() {
            base.RefreshContents();
            _armorDataModels = databaseManagementService.LoadArmor();
            _classDataModels = databaseManagementService.LoadClassCommon();
            _armorDataModel = _armorDataModels.Find(item => item.basic.id == _armorDataModel.basic.id);
            
            if (_armorDataModel == null)
            {
                Clear();
                return;
            }
            
            Initialize();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            var armorTypesName = new List<string>();
            var equipTypesName = new List<string>();

            var systemData = databaseManagementService.LoadSystem();
            foreach (var armor in systemData.armorTypes)
                armorTypesName.Add(armor.name);

            for (int i = 1; i < systemData.equipTypes.Count; i++)
            {
                equipTypesName.Add(systemData.equipTypes[i].name);
            }

            //防具のID
            Label armor_id = RootContainer.Query<Label>("armor_id");
            armor_id.text = _armorDataModel.SerialNumberString;

            ImTextField armor_name = RootContainer.Query<ImTextField>("armor_name");
            armor_name.value = _armorDataModel.basic.name;
            armor_name.RegisterCallback<FocusOutEvent>(o =>
            {
                _armorDataModel.basic.name = armor_name.value;
                SaveData();
                _UpdateSceneView();
            });

            // 画像選択
            //------------------------------------------------------------------------------------------------------------------------------
            // プレビュー画像
            Image iconImage = RootContainer.Query<Image>("icon_image");            
            var tex2d = AssetDatabase.LoadAssetAtPath<Texture2D>(PathManager.IMAGE_ICON + _armorDataModel.basic.iconId + ".png");
            BackgroundImageHelper.SetBackground(iconImage, new Vector2(66, 66), tex2d, LengthUnit.Pixel);

            // 画像名
            Label iconImageName = RootContainer.Query<Label>("icon_image_name");
            iconImageName.text = _armorDataModel.basic.iconId;

            // 画像変更ボタン
            Button iconChangeBtn = RootContainer.Query<Button>("icon_image_change_btn");
            iconChangeBtn.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.IMAGE_ICON, true);
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("WORD_1611"), data =>
                {
                    var imageName = (string) data;
                    _armorDataModel.basic.iconId = imageName;
                    iconImageName.text = imageName;
                    tex2d = AssetDatabase.LoadAssetAtPath<Texture2D>(PathManager.IMAGE_ICON + _armorDataModel.basic.iconId + ".png");
                    BackgroundImageHelper.SetBackground(iconImage, new Vector2(66, 66), tex2d, LengthUnit.Pixel);
                    SaveData();
                }, _armorDataModel.basic.iconId);
            };

            // 画像インポートボタン
            Button iconImportBtn = RootContainer.Query<Button>("icon_image_import_btn");
            iconImportBtn.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.IMAGE_ICON);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    _armorDataModel.basic.iconId = path;
                    iconImageName.text = path;
                    tex2d = AssetDatabase.LoadAssetAtPath<Texture2D>(PathManager.IMAGE_ICON + _armorDataModel.basic.iconId + ".png");
                    BackgroundImageHelper.SetBackground(iconImage, new Vector2(66, 66), tex2d, LengthUnit.Pixel);
                    SaveData();
                }
            };

            ImTextField armor_description = RootContainer.Query<ImTextField>("armor_description");
            armor_description.value = _armorDataModel.basic.description;
            armor_description.RegisterCallback<FocusOutEvent>(o =>
            {
                _armorDataModel.basic.description = armor_description.value;
                SaveData();
            });

            var armorIndex = 0;
            for (var i = 0; i < systemData.armorTypes.Count; i++)
                if (_armorDataModel.basic.armorTypeId ==
                    systemData.armorTypes[i].id)
                {
                    armorIndex = i;
                    break;
                }

            VisualElement armor_equipment_type = RootContainer.Query<VisualElement>("armor_equipment_type");
            var armor_equipment_typePopupField = new PopupFieldBase<string>(armorTypesName,
                armorIndex);
            armor_equipment_type.Add(armor_equipment_typePopupField);
            armor_equipment_typePopupField.RegisterValueChangedCallback(evt =>
            {
                _armorDataModel.basic.armorTypeId =
                    systemData.armorTypes[armor_equipment_typePopupField.index].id;
                SaveData();
            });

            // アーマータイプ
            VisualElement armor_type = RootContainer.Query<VisualElement>("armor_type");
            var equipNum = 0;
            for (var i = 0; i < equipTypesName.Count; i++)
                if (systemData.equipTypes[i + 1].id ==
                    _armorDataModel.basic.equipmentTypeId)
                {
                    equipNum = i;
                    break;
                }

            var armor_type_constraintsPopupField = new PopupFieldBase<string>(equipTypesName,
                equipNum);
            armor_type.Add(armor_type_constraintsPopupField);
            armor_type_constraintsPopupField.RegisterValueChangedCallback(evt =>
            {
                _armorDataModel.basic.equipmentTypeId = systemData
                    .equipTypes[armor_type_constraintsPopupField.index + 1].id;
                SaveData();
            });

            IntegerField armor_price = RootContainer.Query<IntegerField>("armor_price");
            armor_price.value = _armorDataModel.basic.price;
            BaseInputFieldHandler.IntegerFieldCallback(armor_price, evt =>
            {
                _armorDataModel.basic.price = armor_price.value;
                SaveData();
            }, 0, 0);
            IntegerField armor_sell = RootContainer.Query<IntegerField>("armor_sell");
            armor_sell.value = _armorDataModel.basic.sell;
            BaseInputFieldHandler.IntegerFieldCallback(armor_sell, evt =>
            {
                _armorDataModel.basic.sell = armor_sell.value;
                SaveData();
            }, 0, 0);

            //売却可
            var sellToggleList = new List<RadioButton>();
            sellToggleList.Add(RootContainer.Query<RadioButton>("radioButton-equipItem-display5"));
            sellToggleList.Add(RootContainer.Query<RadioButton>("radioButton-equipItem-display6"));

            var sellActions = new List<Action>
            {
                () =>
                {
                    _armorDataModel.basic.canSell = 0;
                    SaveData();
                },
                () =>
                {
                    _armorDataModel.basic.canSell = 1;
                    SaveData();
                }
            };
            new CommonToggleSelector().SetRadioSelector(sellToggleList, _armorDataModel.basic.canSell, sellActions);

            //スイッチアイテム
            var switchItemToggleList = new List<RadioButton>();
            switchItemToggleList.Add(RootContainer.Query<RadioButton>("radioButton-equipItem-display7"));
            switchItemToggleList.Add(RootContainer.Query<RadioButton>("radioButton-equipItem-display8"));

            var switchItemActions = new List<Action>
            {
                () =>
                {
                    _armorDataModel.basic.switchItem = 0;
                    SaveData();
                },
                () =>
                {
                    _armorDataModel.basic.switchItem = 1;
                    SaveData();
                }
            };
            new CommonToggleSelector().SetRadioSelector(switchItemToggleList, _armorDataModel.basic.switchItem, switchItemActions);

            //各種パラメーターの表示の実施
            //パラメータ数に合わせて固定回数for
            for (var i = 0; i < 8; i++)
            {
                var armorNum = i;

                IntegerField armor_num = RootContainer.Query<IntegerField>("armor_num" + armorNum);

                //パラメータが足りない場合追加を行う
                if (_armorDataModel.parameters.Count <= armorNum)
                    _armorDataModel.parameters.Add(armorNum == 8 ? 1 : 0);

                armor_num.value = _armorDataModel.parameters[armorNum];

                //レベルの時は上限下限が違う
                if (armorNum == 8)
                    BaseInputFieldHandler.IntegerFieldCallback(armor_num, evt =>
                    {
                        _armorDataModel.parameters[armorNum] = armor_num.value;
                        SaveData();
                    }, 1, 99);
                else
                    BaseInputFieldHandler.IntegerFieldCallback(armor_num, evt =>
                    {
                        _armorDataModel.parameters[armorNum] = armor_num.value;
                        SaveData();
                    }, 0, 0);
            }

            // 特徴
            VisualElement armorTraits = RootContainer.Query<VisualElement>("armor_traits");
            var traitWindow = new TraitsInspectorElement();
            armorTraits.Add(traitWindow);
            //自分がアーマーであることを伝える
            traitWindow.isArmor = true;
            traitWindow.Init(_armorDataModel.traits, TraitsType.TRAITS_TYPE_ARMOR, evt =>
            {
                _armorDataModel.traits = (List<TraitCommonDataModel>) evt;
                SaveData();
            });

            ImTextField armor_memo = RootContainer.Query<ImTextField>("armor_memo");
            armor_memo.value = _armorDataModel.memo;
            armor_memo.RegisterCallback<FocusOutEvent>(o =>
            {
                _armorDataModel.memo = armor_memo.value;
                SaveData();
            });

            VisualElement status_edit = RootContainer.Query<VisualElement>("status_edit");
            var disp_auto_guide = RootContainer.Query<Button>("disp_auto_guide").AtIndex(0);

            // レベルの要素取得
            IntegerField armorLevel = RootContainer.Query<IntegerField>("armor_level");
            armorLevel.value = _armorDataModel.parameters[0];
            if (armorLevel.value < 1)
            {
                armorLevel.value = 1;
            }
            BaseInputFieldHandler.IntegerFieldCallback(armorLevel, evt => 
            {
                _armorDataModel.parameters[0] = armorLevel.value;
                SaveData();
            }, 1, 99);

            disp_auto_guide.clicked += () => { StartAutoGuide(); };
        }

        // オートガイドのパラメータ設定
        private void StartAutoGuide() {
            // システムデータ
            var systemData = databaseManagementService.LoadSystem();
            var classData = _classDataModels[0];

            // 係数
            float coefC = 0;
            // 計算用値
            float paramA = 0;
            float paramB = 0;
            float paramC = 0;

            // 防具タイプごとに係数等を設定する
            //---------------------------------------------------------------
            // 身体
            if (_armorDataModel.basic.equipmentTypeId == systemData.equipTypes[3].id)
            {
                coefC = 0.105f;
                paramA = 5;
                paramC = 600;
                paramB = paramC * (1.0f * classData.clearLevel / classData.maxLevel * 0.15f + 0.85f);
            }
            // 盾
            else if (_armorDataModel.basic.equipmentTypeId == systemData.equipTypes[1].id)
            {
                coefC = 0.08f;
                paramA = 5;
                paramC = 600 * 1.18f;
                paramB = paramC * (1.0f * classData.clearLevel / classData.maxLevel * 0.15f + 0.85f);
            }
            // 頭
            else if (_armorDataModel.basic.equipmentTypeId == systemData.equipTypes[2].id)
            {
                coefC = 0.065f;
                paramA = 5;
                paramC = 600 * 1.35f;
                paramB = paramC * (1.0f * classData.clearLevel / classData.maxLevel * 0.15f + 0.85f);
            }
            else
            {
                return;
            }

            //防御力計算
            var defenceParamC = classData.baseHpMaxValue * coefC;
            var defenceParamA = defenceParamC * 0.045f;
            var defenceParamB = defenceParamC * (1.0f * classData.clearLevel / classData.maxLevel * 0.15f + 0.85f);

            // レベルの要素取得
            IntegerField armorLevel = RootContainer.Query<IntegerField>("armor_level");
            BaseInputFieldHandler.IntegerFieldCallback(armorLevel, evt => { }, 1, 99);

            var defence = 0;
            if (armorLevel.value < _classDataModels[0].clearLevel + 1)
                defence = (int) (defenceParamA + (defenceParamB - defenceParamA) /
                    (_classDataModels[0].clearLevel - 1) * (armorLevel.value - 1));
            else
                defence = (int) (defenceParamB + (defenceParamC - defenceParamB) /
                    (_classDataModels[0].maxLevel - _classDataModels[0].clearLevel) *
                    (armorLevel.value - _classDataModels[0].clearLevel));

            // 買値の計算
            //---------------------------------------------------------------            
            var price = 0;
            if (armorLevel.value < _classDataModels[0].clearLevel + 1)
            {
                var priceCoef = (paramA + (paramB - paramA) * Mathf.Pow(armorLevel.value - 1, 2) /
                    Mathf.Pow(_classDataModels[0].clearLevel - 1, 2));
                price = (int)(defence * priceCoef);
            }
            else if (armorLevel.value > _classDataModels[0].clearLevel)
            {
                var priceCoef = (paramB + (paramC - paramB) *
                    (armorLevel.value - _classDataModels[0].clearLevel) /
                    (_classDataModels[0].maxLevel - _classDataModels[0].clearLevel));
                price = (int)(defence * priceCoef);
            }

            // 売値の計算
            //---------------------------------------------------------------            
            var sellPrice = (int) (price * 0.6f);

            // パラメータの設定
            //---------------------------------------------------------------            
            Label defenceLabel = RootContainer.Query<Label>("armor_cant_enter0");
            Label priceLabel = RootContainer.Query<Label>("armor_cant_enter1");
            Label cellPriceLabel = RootContainer.Query<Label>("armor_cant_enter2");
            IntegerField armor_price = RootContainer.Query<IntegerField>("armor_price");
            IntegerField armor_sell = RootContainer.Query<IntegerField>("armor_sell");
            IntegerField armor_defence = RootContainer.Query<IntegerField>("armor_num3");

            defenceLabel.text = defence.ToString();
            priceLabel.text = price.ToString();
            cellPriceLabel.text = sellPrice.ToString();
            armor_price.value = price;
            armor_sell.value = sellPrice;
            armor_defence.value = defence;

            _armorDataModel.basic.price = price;
            _armorDataModel.basic.sell = sellPrice;
            //防御パラメーターに代入
            _armorDataModel.parameters[3] = defence;

            SaveData();
        }

        private void SaveData() {
            databaseManagementService.SaveArmor(_armorDataModels);
        }

        private void _UpdateSceneView() {
            _ = Editor.Hierarchy.Hierarchy.Refresh(Region.Equip, _armorDataModel.basic.id);
        }
    }
}