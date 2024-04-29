using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Animation;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using RPGMaker.Codebase.Editor.Hierarchy.Enum;
using RPGMaker.Codebase.Editor.Inspector.Animation.View;
using RPGMaker.Codebase.Editor.Inspector.Trait.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.Editor.Inspector.Trait.View.TraitsInspectorElement;

namespace RPGMaker.Codebase.Editor.Inspector.Weapon.View
{
    /// <summary>
    /// [装備・アイテムの編集]-[武器] Inspector
    /// </summary>
    public class WeaponInspectorElement : AbstractInspectorElement
    {
        private          List<AnimationDataModel>  _animationDataModels;
        private          List<ClassDataModel>      _classDataModels;

        private int _num;

        private SceneWindow _sceneView;
        private WeaponDataModel _weaponDataModel;

        private          List<WeaponDataModel> _weaponDataModels;

        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Weapon/Asset/inspector_weaponEdit.uxml"; } }

        public WeaponInspectorElement(WeaponDataModel weaponDataModel) {
            _weaponDataModel = weaponDataModel;
            _sceneView = DatabaseEditor.DatabaseEditor.GetDatabaseSceneWindow();

            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            _weaponDataModels = databaseManagementService.LoadWeapon();
            _animationDataModels = databaseManagementService.LoadAnimation();
            _classDataModels = databaseManagementService.LoadClassCommon();
            _weaponDataModel =
                _weaponDataModels.Find(item => item.basic.id == _weaponDataModel.basic.id);
            
            if (_weaponDataModel == null)
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

            var systemData = databaseManagementService.LoadSystem();
            var weaponTypesName = new List<string>();
            weaponTypesName.AddRange(systemData.weaponTypes.Select(v => v.value));

            //武器のID
            Label weapon_id = RootContainer.Query<Label>("weapon_id");
            weapon_id.text = _weaponDataModel.SerialNumberString;

            //武器の名前
            ImTextField weapon_name = RootContainer.Query<ImTextField>("weapon_name");
            weapon_name.value = _weaponDataModel.basic.name;
            weapon_name.RegisterCallback<FocusOutEvent>(o =>
            {
                _weaponDataModel.basic.name = weapon_name.value;
                SaveData();
                _UpdateSceneView();
            });

            // 画像選択
            //------------------------------------------------------------------------------------------------------------------------------
            // プレビュー画像
            Image iconImage = RootContainer.Query<Image>("icon_image");
            
            var tex2d = AssetDatabase.LoadAssetAtPath<Texture2D>(PathManager.IMAGE_ICON + _weaponDataModel.basic.iconId + ".png");
            BackgroundImageHelper.SetBackground(iconImage, new Vector2(66, 66), tex2d, LengthUnit.Pixel);

            // 画像名
            Label iconImageName = RootContainer.Query<Label>("icon_image_name");
            iconImageName.text = _weaponDataModel.basic.iconId;

            // 画像変更ボタン
            Button iconChangeBtn = RootContainer.Query<Button>("icon_image_change_btn");
            iconChangeBtn.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.IMAGE_ICON, true);
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("WORD_1611"), data =>
                {
                    var imageName = (string) data;
                    _weaponDataModel.basic.iconId = imageName;
                    iconImageName.text = imageName;
                    tex2d = AssetDatabase.LoadAssetAtPath<Texture2D>(PathManager.IMAGE_ICON + _weaponDataModel.basic.iconId + ".png");
                    BackgroundImageHelper.SetBackground(iconImage, new Vector2(66, 66), tex2d, LengthUnit.Pixel);
                    SaveData();
                }, _weaponDataModel.basic.iconId);
            };

            // 画像インポートボタン
            Button iconImportBtn = RootContainer.Query<Button>("icon_image_import_btn");
            iconImportBtn.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.IMAGE_ICON);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    _weaponDataModel.basic.iconId = path;
                    iconImageName.text = path;
                    tex2d = AssetDatabase.LoadAssetAtPath<Texture2D>(PathManager.IMAGE_ICON + _weaponDataModel.basic.iconId + ".png");
                    BackgroundImageHelper.SetBackground(iconImage, new Vector2(66, 66), tex2d, LengthUnit.Pixel);
                    SaveData();
                }
            };

            ImTextField weapon_description = RootContainer.Query<ImTextField>("weapon_description");
            weapon_description.value = _weaponDataModel.basic.description;
            weapon_description.RegisterCallback<FocusOutEvent>(o =>
            {
                _weaponDataModel.basic.description = weapon_description.value;
                SaveData();
            });

            var weaponIndex = systemData.weaponTypes.FindIndex(v => v.id == _weaponDataModel.basic.weaponTypeId);
            if (weaponIndex == -1)
                weaponIndex = 0;

            VisualElement weapon_type = RootContainer.Query<VisualElement>("weapon_type");
            var weapon_typePopupField = new PopupFieldBase<string>(weaponTypesName, weaponIndex);
            weapon_type.Add(weapon_typePopupField);
            weapon_typePopupField.RegisterValueChangedCallback(evt =>
            {
                _weaponDataModel.basic.weaponTypeId =
                    systemData.weaponTypes[weapon_typePopupField.index].id;
                SaveData();
            });

            IntegerField weapon_price = RootContainer.Query<IntegerField>("weapon_price");
            weapon_price.value = _weaponDataModel.basic.price;
            BaseInputFieldHandler.IntegerFieldCallback(weapon_price, evt =>
            {
                _weaponDataModel.basic.price = weapon_price.value;
                SaveData();
            }, 0, 0);
            IntegerField weapon_sell = RootContainer.Query<IntegerField>("weapon_sell");
            weapon_sell.value = _weaponDataModel.basic.sell;
            BaseInputFieldHandler.IntegerFieldCallback(weapon_sell, evt =>
            {
                _weaponDataModel.basic.sell = weapon_sell.value;
                SaveData();
            }, 0, 0);


            //アニメーションのフォールドダウン
            VisualElement weaponAnimationFolddown = RootContainer.Query<VisualElement>("weapon_animation");
            var animationFolddownPopupField = new PopupFieldBase<string>(ParticleList(),
                AnimationIdToIndex(_weaponDataModel.basic.animationId));
            weaponAnimationFolddown.Add(animationFolddownPopupField);
            animationFolddownPopupField.RegisterValueChangedCallback(evt =>
            {
                _weaponDataModel.basic.animationId =
                    _animationDataModels[animationFolddownPopupField.index].id;
                ShowAnimationView(animationFolddownPopupField.index);
                SaveData();
            });

            ShowAnimationView(animationFolddownPopupField.index);

            void ShowAnimationView(int animationIndex) {
                if (_animationDataModels.Count == 0)
                    AnimationInspectorElement.ShowAnimationPreview(null);
                else
                    AnimationInspectorElement.ShowAnimationPreview(
                        _animationDataModels[animationIndex]);
            }

            //売却可
            var sellToggleList = new List<RadioButton>();
            sellToggleList.Add(RootContainer.Query<RadioButton>("radioButton-equipItem-display1"));
            sellToggleList.Add(RootContainer.Query<RadioButton>("radioButton-equipItem-display2"));

            var sellActions = new List<Action>
            {
                () =>
                {
                    _weaponDataModel.basic.canSell = 0;
                    SaveData();
                },
                () =>
                {
                    _weaponDataModel.basic.canSell = 1;
                    SaveData();
                }
            };
            new CommonToggleSelector().SetRadioSelector(sellToggleList, _weaponDataModel.basic.canSell, sellActions);

            //スイッチアイテム
            var switchItemToggleList = new List<RadioButton>();
            switchItemToggleList.Add(RootContainer.Query<RadioButton>("radioButton-equipItem-display3"));
            switchItemToggleList.Add(RootContainer.Query<RadioButton>("radioButton-equipItem-display4"));

            var switchItemActions = new List<Action>
            {
                () =>
                {
                    _weaponDataModel.basic.switchItem = 0;
                    SaveData();
                },
                () =>
                {
                    _weaponDataModel.basic.switchItem = 1;
                    SaveData();
                }
            };
            new CommonToggleSelector().SetRadioSelector(switchItemToggleList, _weaponDataModel.basic.switchItem, switchItemActions);

            //各種パラメーターの表示の実施
            //パラメータ数に合わせて固定回数for
            for (var i = 0; i < 8; i++)
            {
                var weaponNum = i;
                IntegerField weapon_num = RootContainer.Query<IntegerField>("weapon_num" + weaponNum);

                //パラメータが足りない場合追加を行う
                if (_weaponDataModel.parameters.Count <= weaponNum)
                    _weaponDataModel.parameters.Add(weaponNum == 8 ? 1 : 0);

                weapon_num.value = _weaponDataModel.parameters[weaponNum];

                //レベルの時は上限下限が違う
                if (weaponNum == 8)
                    BaseInputFieldHandler.IntegerFieldCallback(weapon_num, evt =>
                    {
                        _weaponDataModel.parameters[weaponNum] = weapon_num.value;
                        SaveData();
                    }, 1, 99);
                else
                    BaseInputFieldHandler.IntegerFieldCallback(weapon_num, evt =>
                    {
                        _weaponDataModel.parameters[weaponNum] = weapon_num.value;
                        SaveData();
                    }, 0, 0);
            }

            ImTextField weapon_memo = RootContainer.Query<ImTextField>("weapon_memo");
            weapon_memo.value = _weaponDataModel.memo;
            weapon_memo.RegisterCallback<FocusOutEvent>(o =>
            {
                _weaponDataModel.memo = weapon_memo.value;
                SaveData();
            });

            //class_traits_new
            //class_traits_area
            VisualElement classTraitsArea = RootContainer.Query<VisualElement>("class_traits_area");
            var traitWindow = new TraitsInspectorElement();
            classTraitsArea.Add(traitWindow);
            traitWindow.Init(_weaponDataModel.traits, TraitsType.TRAITS_TYPE_WEAPON, evt =>
            {
                _weaponDataModel.traits = (List<TraitCommonDataModel>) evt;
                SaveData();
            });

            VisualElement status_edit = RootContainer.Query<VisualElement>("status_edit");
            var disp_auto_guide = RootContainer.Query<Button>("disp_auto_guide").AtIndex(0);

            // 入手想定レベル
            IntegerField weaponLevel = RootContainer.Query<IntegerField>("weapon_level");
            weaponLevel.value = _weaponDataModel.parameters[0];
            if (weaponLevel.value < 1)
            {
                weaponLevel.value = 1;
            }
            BaseInputFieldHandler.IntegerFieldCallback(weaponLevel, evt => {
                _weaponDataModel.parameters[0] = weaponLevel.value;
                SaveData();
            }, 1, 99);

            disp_auto_guide.clicked += () =>
            {
                StartAutoGuide();
                SaveData();
            };
        }

        // オートガイドのパラメータ設定
        private void StartAutoGuide() {
            // 入手想定レベル
            IntegerField weaponLevel = RootContainer.Query<IntegerField>("weapon_level");
            BaseInputFieldHandler.IntegerFieldCallback(weaponLevel, evt => { }, 1, 99);
            // 初期設定データ
            var classData = _classDataModels[0];

            // 攻撃力
            //---------------------------------------------------------------
            // 係数
            // レベル1の強さ
            var coefC = 0.25f;
            // 計算用パラメータ
            var paramC = classData.baseHpMaxValue * coefC;
            var paramA = paramC * 0.045f;
            var paramB = paramC * (1.0f * classData.clearLevel / classData.maxLevel * 0.15f + 0.85f);

            var attack = 0;
            if (weaponLevel.value < classData.clearLevel + 1)
                attack = (int) (paramA + (paramB - paramA) / (classData.clearLevel - 1) * (weaponLevel.value - 1));
            else if (weaponLevel.value > classData.clearLevel)
                attack = (int) (paramB + (paramC - paramB) / (classData.maxLevel - classData.clearLevel) *
                    (weaponLevel.value - classData.clearLevel));

            // 買値
            //---------------------------------------------------------------
            // 係数
            float priceParamA = 5;
            float priceParamC = 600;
            var priceParamB = priceParamC * (1f * classData.clearLevel / classData.maxLevel * 0.15f + 0.85f);

            var price = 0;
            if (weaponLevel.value < classData.clearLevel + 1)
            {
                var priceCoef = priceParamA + (priceParamB - priceParamA) * Mathf.Pow(weaponLevel.value - 1, 2) /
                    Mathf.Pow(classData.clearLevel - 1, 2);
                price = (int) (attack * priceCoef);
            }
            else
            {
                var priceCoef = priceParamB + (priceParamC - priceParamB) * (weaponLevel.value - classData.clearLevel) /
                    (classData.maxLevel - classData.clearLevel);
                price = (int) (attack * priceCoef);
            }

            // 売値
            //---------------------------------------------------------------
            var sellPrice = (int) (price * 0.6f);

            // 値の反映
            //---------------------------------------------------------------
            // オートガイド下反映
            Label attackLabel = RootContainer.Query<Label>("weapon_cant_enter0");
            Label priceLabel = RootContainer.Query<Label>("weapon_cant_enter1");
            Label sellPriceLabel = RootContainer.Query<Label>("weapon_cant_enter2");
            attackLabel.text = attack.ToString();
            priceLabel.text = price.ToString();
            sellPriceLabel.text = sellPrice.ToString();

            // パラメータに反映
            
            IntegerField attackValue = RootContainer.Query<IntegerField>("weapon_num2");
            IntegerField weapon_price = RootContainer.Query<IntegerField>("weapon_price");
            IntegerField weapon_sell = RootContainer.Query<IntegerField>("weapon_sell");
            attackValue.value = attack;
            weapon_price.value = price;
            weapon_sell.value = sellPrice;
            _weaponDataModel.basic.price = price;
            _weaponDataModel.basic.sell = sellPrice;
            //攻撃パラメーターに代入
            _weaponDataModel.parameters[2] = attack;
        }

        private void SaveData() {
            databaseManagementService.SaveWeapon(_weaponDataModels);
        }

        private void _UpdateSceneView() {
            _ = Editor.Hierarchy.Hierarchy.Refresh(Region.Equip, _weaponDataModel.basic.id);
        }

        //アニメーションのデータの名前をListで返す
        private List<string> ParticleList() {
            var returnList = new List<string>();
            foreach (var animation in _animationDataModels) returnList.Add(animation.particleName.Replace("/", "／"));

            return returnList;
        }

        //アニメーションのIdを今のIndexに変換して返す
        private int AnimationIdToIndex(string id) {
            var returnIndex = 0;
            for (var i = 0; i < _animationDataModels.Count; i++)
                if (_animationDataModels[i].id == id)
                {
                    returnIndex = i;
                    break;
                }

            return returnIndex;
        }
    }
}