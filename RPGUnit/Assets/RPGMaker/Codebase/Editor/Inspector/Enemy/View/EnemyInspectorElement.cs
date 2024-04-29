using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Enemy;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCustom;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.State;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using RPGMaker.Codebase.Editor.Hierarchy.Enum;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Character.View;
using RPGMaker.Codebase.Editor.Inspector.Trait.View;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.Editor.Common.AutoguideHelper;
using static RPGMaker.Codebase.Editor.Inspector.Trait.View.TraitsInspectorElement;

namespace RPGMaker.Codebase.Editor.Inspector.Enemy.View
{
    /// <summary>
    /// [バトルの編集]-[敵キャラ] Inspector
    /// </summary>
    public class EnemyInspectorElement : AbstractInspectorElement
    {
        private VisualElement _actionPatternArea;
        private List<ArmorDataModel> _armorDataModels;

        private int _currentSelectId;

        private CurrentType _currentType = CurrentType.DropItem;

        private VisualElement _dropItemArea;

        //ヒエラルキー側の保持
        private EnemyDataModel _enemy;

        private List<EnemyDataModel> _enemyDataModels;
        private readonly VisualElement _enemyTraitsArea = null;

        private string _id = "";
        private List<ItemDataModel> _itemDataModels;

        //敵キャラのプレビューサイズ
        private readonly float                  _previewSize = 200f;
        private          List<StateDataModel>   _stateDataModels;
        private          SystemSettingDataModel _systemSettingDataModel;
        private          List<WeaponDataModel>  _weaponDataModels;
        private List<SkillCustomDataModel> _skillCustomDataModels;

        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Enemy/Asset/inspector_enemy.uxml"; } }

        public EnemyInspectorElement(string id, BattleHierarchyView element) {
            _id = id;
            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();

            _enemyDataModels = databaseManagementService.LoadEnemy();
            _systemSettingDataModel = databaseManagementService.LoadSystem();
            _weaponDataModels = databaseManagementService.LoadWeapon();
            _armorDataModels = databaseManagementService.LoadArmor();
            _itemDataModels = databaseManagementService.LoadItem();
            _stateDataModels = databaseManagementService.LoadStateEdit();
            _skillCustomDataModels = databaseManagementService.LoadSkillCustom();

            if (_id != "-1")
            {
                for (var i = 0; i < _enemyDataModels.Count; i++)
                    if (_enemyDataModels[i].id == _id)
                    {
                        _enemy = _enemyDataModels[i];
                        break;
                    }
            }
            else
            {
                _id = Guid.NewGuid().ToString();
                _enemy = EnemyDataModel.CreateDefault(_id, "#" + string.Format("{0:D4}", _enemyDataModels.Count + 1));
                _enemyDataModels.Add(_enemy);
                Save();
            }
            
            Initialize();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            //enemy_id_text
            Label enemyIdText = RootContainer.Query<Label>("enemy_id_text");
            enemyIdText.text = _enemy.SerialNumberString;

            ImTextField enemyName = RootContainer.Query<ImTextField>("enemy_name");
            enemyName.value = _enemy.name;
            enemyName.RegisterCallback<FocusOutEvent>(evt =>
            {
                _enemy.name = enemyName.value;
                Save();
                _UpdateSceneView();
            });

            //enemy_elements_dropdown
            VisualElement enemyElementsDropdown = RootContainer.Query<VisualElement>("enemy_elements_dropdown");
            var enemyElementList = ElementList();
            var hedDropdownPopupField =
                new PopupFieldBase<string>(
                    enemyElementList,
                    _enemy.elements[0]
                );
            enemyElementsDropdown.Add(hedDropdownPopupField);
            hedDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                _enemy.elements[0] = hedDropdownPopupField.index;
                Save();
            });


            //enemy_level
            IntegerField enemyLevel = RootContainer.Query<IntegerField>("enemy_level");
            if (_enemy.level < 1)
            {
                _enemy.level = 1;
            }
            enemyLevel.value = _enemy.level;
            BaseInputFieldHandler.IntegerFieldCallback(enemyLevel, evt =>
            {
                _enemy.level = enemyLevel.value;
                Save();
            }, 1, 99);

            //メモ
            ImTextField enemyMemo = RootContainer.Query<ImTextField>("enemy_memo");
            enemyMemo.value = _enemy.memo;
            enemyMemo.RegisterCallback<FocusOutEvent>(evt =>
            {
                _enemy.memo = enemyMemo.value;
                Save();
            });

            //オートガイド
            IntegerField enemyAutoguideLevel = RootContainer.Query<IntegerField>("enemy_autoguide_level");
            enemyAutoguideLevel.value = _enemy.autoGuide.level;
            if (enemyAutoguideLevel.value > 99)
                enemyAutoguideLevel.value = 99;
            else if (enemyAutoguideLevel.value < 1)
                enemyAutoguideLevel.value = 1;
            BaseInputFieldHandler.IntegerFieldCallback(enemyAutoguideLevel, evt =>
            {
                _enemy.autoGuide.level = enemyAutoguideLevel.value;
                Save();
            }, 1, 99);

            //enemy_autoguide_attackTurn
            FloatField enemyAutoguideAttackTurn = RootContainer.Query<FloatField>("enemy_autoguide_attackTurn");
            if (_enemy.autoGuide.attackTurn < 1)
            {
                _enemy.autoGuide.attackTurn = 1;
            }
            enemyAutoguideAttackTurn.value = _enemy.autoGuide.attackTurn;
            BaseInputFieldHandler.FloatFieldCallback(enemyAutoguideAttackTurn, evt =>
            {
                _enemy.autoGuide.attackTurn = enemyAutoguideAttackTurn.value;
                Save();
            }, 0, 9999, 2);
            //enemy_autoguide_guardTurn
            FloatField enemyAutoguideGuardTurn = RootContainer.Query<FloatField>("enemy_autoguide_guardTurn");
            if (_enemy.autoGuide.guardTurn < 1)
            {
                _enemy.autoGuide.guardTurn = 1;
            }
            enemyAutoguideGuardTurn.value = _enemy.autoGuide.guardTurn;
            BaseInputFieldHandler.FloatFieldCallback(enemyAutoguideGuardTurn, evt =>
            {
                _enemy.autoGuide.guardTurn = enemyAutoguideGuardTurn.value;
                Save();
            }, 0, 9999, 2);

            // 魔法力のトグル設定
            RadioButton magicToggle1 = RootContainer.Query<RadioButton>("radioButton-battleEdit-display7");
            RadioButton magicToggle2 = RootContainer.Query<RadioButton>("radioButton-battleEdit-display8");
            RadioButton magicToggle3 = RootContainer.Query<RadioButton>("radioButton-battleEdit-display9");

            if (_enemy.autoGuide.magicPowerSetting != 0 &&
                _enemy.autoGuide.magicPowerSetting != 1 &&
                _enemy.autoGuide.magicPowerSetting != 2)
                _enemy.autoGuide.magicPowerSetting = 0;

            if (_enemy.autoGuide.magicPowerSetting == 0)
                magicToggle1.value = true;
            else if (_enemy.autoGuide.magicPowerSetting == 1)
                magicToggle2.value = true;
            else if (_enemy.autoGuide.magicPowerSetting == 2) magicToggle3.value = true;

            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {magicToggle1, magicToggle2, magicToggle3},
                _enemy.autoGuide.magicPowerSetting, new List<Action>
                {
                    //なし
                    () =>
                    {
                        _enemy.autoGuide.magicPowerSetting = 0;
                        Save();
                    },
                    //普通
                    () =>
                    {
                        _enemy.autoGuide.magicPowerSetting = 1;
                        Save();
                    },
                    //強い
                    () =>
                    {
                        _enemy.autoGuide.magicPowerSetting = 2;
                        Save();
                    }
                });


            // 魔防のトグル設定
            RadioButton magicDefenceToggle1 = RootContainer.Query<RadioButton>("radioButton-battleEdit-display10");
            RadioButton magicDefenceToggle2 = RootContainer.Query<RadioButton>("radioButton-battleEdit-display11");
            RadioButton magicDefenceToggle3 = RootContainer.Query<RadioButton>("radioButton-battleEdit-display12");

            if (_enemy.autoGuide.magicGuardSetting != 0 &&
                _enemy.autoGuide.magicGuardSetting != 1 &&
                _enemy.autoGuide.magicGuardSetting != 2)
                _enemy.autoGuide.magicGuardSetting = 0;

            if (_enemy.autoGuide.magicGuardSetting == 0)
                magicDefenceToggle1.value = true;
            else if (_enemy.autoGuide.magicGuardSetting == 1)
                magicDefenceToggle2.value = true;
            else if (_enemy.autoGuide.magicGuardSetting == 2)
                magicDefenceToggle3.value = true;

            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {magicDefenceToggle1, magicDefenceToggle2, magicDefenceToggle3},
                _enemy.autoGuide.magicGuardSetting, new List<Action>
                {
                    //なし
                    () =>
                    {
                        _enemy.autoGuide.magicGuardSetting = 0;
                        Save();
                    },
                    //普通
                    () =>
                    {
                        _enemy.autoGuide.magicGuardSetting = 1;
                        Save();
                    },
                    //強い
                    () =>
                    {
                        _enemy.autoGuide.magicGuardSetting = 2;
                        Save();
                    }
                });

            

            // オートガイド自動入力フィールド
            IntegerField enemyAutoGuideMaxHp = RootContainer.Query<IntegerField>("enemy_autoguide_maxHp");
            IntegerField enemyAutoGuideMaxMp = RootContainer.Query<IntegerField>("enemy_autoguide_maxMp");
            IntegerField enemyAutoGuideAttack = RootContainer.Query<IntegerField>("enemy_autoguide_attack");
            IntegerField enemyAutoGuideGuard = RootContainer.Query<IntegerField>("enemy_autoguide_guard");
            IntegerField enemyAutoGuideMagic = RootContainer.Query<IntegerField>("enemy_autoguide_magic");
            IntegerField enemyAutoGuideMagicGuard = RootContainer.Query<IntegerField>("enemy_autoguide_magicGuard");
            IntegerField enemyAutoGuideSpeed = RootContainer.Query<IntegerField>("enemy_autoguide_speed");
            IntegerField enemyAutoGuideLuck = RootContainer.Query<IntegerField>("enemy_autoguide_luck");
            IntegerField enemyAutoGuideExp = RootContainer.Query<IntegerField>("enemy_autoguide_exp");
            IntegerField enemyAutoGuideMoney = RootContainer.Query<IntegerField>("enemy_autoguide_money");
            enemyAutoGuideMaxHp.isReadOnly = true;
            enemyAutoGuideMaxMp.isReadOnly = true;
            enemyAutoGuideAttack.isReadOnly = true;
            enemyAutoGuideGuard.isReadOnly = true;
            enemyAutoGuideMagic.isReadOnly = true;
            enemyAutoGuideMagicGuard.isReadOnly = true;
            enemyAutoGuideSpeed.isReadOnly = true;
            enemyAutoGuideLuck.isReadOnly = true;
            enemyAutoGuideExp.isReadOnly = true;
            enemyAutoGuideMoney.isReadOnly = true;

            //enemy_autoguide_reflect
            Button enemyAutoguideReflect = RootContainer.Query<Button>("enemy_autoguide_reflect");
            //enemy_maxHp
            IntegerField enemyMaxHp = RootContainer.Query<IntegerField>("enemy_maxHp");
            enemyMaxHp.value = _enemy.param[0];
            BaseInputFieldHandler.IntegerFieldCallback(enemyMaxHp, evt =>
            {
                _enemy.param[0] = enemyMaxHp.value;
                Save();
            }, 1, 1);
            //enemy_maxMp
            IntegerField enemyMaxMp = RootContainer.Query<IntegerField>("enemy_maxMp");
            //enemyMaxMp.isReadOnly = true;
            enemyMaxMp.value = _enemy.param[1];
            BaseInputFieldHandler.IntegerFieldCallback(enemyMaxMp, evt =>
            {
                _enemy.param[1] = enemyMaxMp.value;
                Save();
            }, 0, 0);
            //enemy_attack
            IntegerField enemyAttack = RootContainer.Query<IntegerField>("enemy_attack");
            enemyAttack.value = _enemy.param[2];
            BaseInputFieldHandler.IntegerFieldCallback(enemyAttack, evt =>
            {
                _enemy.param[2] = enemyAttack.value;
                Save();
            }, 1, 1);
            //enemy_guard
            IntegerField enemyGuard = RootContainer.Query<IntegerField>("enemy_guard");
            enemyGuard.value = _enemy.param[3];
            BaseInputFieldHandler.IntegerFieldCallback(enemyGuard, evt =>
            {
                _enemy.param[3] = enemyGuard.value;
                Save();
            }, 1, 1);
            //enemy_magic
            IntegerField enemyMagic = RootContainer.Query<IntegerField>("enemy_magic");
            enemyMagic.value = _enemy.param[4];
            BaseInputFieldHandler.IntegerFieldCallback(enemyMagic, evt =>
            {
                _enemy.param[4] = enemyMagic.value;
                Save();
            }, 1, 1);
            //enemy_magicGuard
            IntegerField enemyMagicGuard = RootContainer.Query<IntegerField>("enemy_magicGuard");
            enemyMagicGuard.value = _enemy.param[5];
            BaseInputFieldHandler.IntegerFieldCallback(enemyMagicGuard, evt =>
            {
                _enemy.param[5] = enemyMagicGuard.value;
                Save();
            }, 1, 1);
            //enemy_agility
            IntegerField enemySpeed = RootContainer.Query<IntegerField>("enemy_speed");
            enemySpeed.value = _enemy.param[6];
            BaseInputFieldHandler.IntegerFieldCallback(enemySpeed, evt =>
            {
                _enemy.param[6] = enemySpeed.value;
                Save();
            }, 1, 1);
            //enemy_luck
            IntegerField enemyLuck = RootContainer.Query<IntegerField>("enemy_luck");
            enemyLuck.value = _enemy.param[7];
            BaseInputFieldHandler.IntegerFieldCallback(enemyLuck, evt =>
            {
                _enemy.param[7] = enemyLuck.value;
                Save();
            }, 1, 1);

            // 画像選択
            //------------------------------------------------------------------------------------------------------------------------------
            // プレビュー画像
            Image enemyImage = RootContainer.Query<Image>("enemy_image");
            enemyImage.scaleMode = ScaleMode.ScaleToFit;
            enemyImage.image = ImageManager.LoadEnemyByTexture(_enemy.images.image);

            enemyImage.style.height = new StyleLength(_previewSize);
            enemyImage.style.width = new StyleLength(_previewSize);

            // 画像名
            Label enemyImageName = RootContainer.Query<Label>("enemy_image_name");
            enemyImageName.text = _enemy.images.image;

            // 画像変更ボタン
            Button enemyChangeBtn = RootContainer.Query<Button>("enemy_image_change_btn");
            enemyChangeBtn.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.IMAGE_ENEMY, true);
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    _enemy.images.image = imageName;
                    enemyImage.image = ImageManager.LoadEnemyByTexture(_enemy.images.image);
                    enemyImageName.text = imageName;
                    Save();
                }, _enemy.images.image);
            };

            // 画像インポートボタン
            Button enemyImportBtn = RootContainer.Query<Button>("enemy_image_import_btn");
            enemyImportBtn.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.IMAGE_ENEMY);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    _enemy.images.image = path;
                    enemyImage.image = ImageManager.LoadEnemyByTexture(_enemy.images.image);
                    enemyImageName.text = path;
                    Save();
                    Refresh();
                }
            };

            var enemyAutoFitList = new List<string>
            {
                "WORD_0586",
                "WORD_0587",
                "WORD_0588"
            };
            //enemy_automatic_pullDown
            VisualElement enemyAutomaticPullDown = RootContainer.Query<VisualElement>("enemy_automatic_pullDown");

            var enemyAutoFitPopupField =
                new PopupFieldBase<string>(EditorLocalize.LocalizeTexts(enemyAutoFitList),
                    _enemy.images.autofitPattern);
            enemyAutomaticPullDown.Add(enemyAutoFitPopupField);
            enemyAutoFitPopupField.RegisterValueChangedCallback(evt =>
            {
                _enemy.images.autofitPattern = enemyAutoFitPopupField.index;
                Save();
            });

            RadioButton enemyAutomaticScalingToggle1 = RootContainer.Query<RadioButton>("radioButton-battleEdit-display13");
            RadioButton enemyAutomaticScalingToggle2 = RootContainer.Query<RadioButton>("radioButton-battleEdit-display14");
            VisualElement enemyDisplaySizeSliderArea = RootContainer.Query<VisualElement>("enemy_displaySize_sliderArea");
            SliderAndFiledBase.IntegerSliderCallBack(enemyDisplaySizeSliderArea, 10, 200, "%",
                _enemy.images.scale, evt =>
                {
                    _enemy.images.scale = (int) evt;
                    Save();
                });

            enemyAutomaticScalingToggle1.value = (AutoFitEnum) _enemy.images.autofit == AutoFitEnum.ON;
            enemyAutomaticScalingToggle2.value = (AutoFitEnum) _enemy.images.autofit == AutoFitEnum.OFF;

            enemyDisplaySizeSliderArea.SetEnabled(!enemyAutomaticScalingToggle1.value);
            
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {enemyAutomaticScalingToggle1, enemyAutomaticScalingToggle2},
                _enemy.images.autofit, new List<Action>
                {
                    //自動調整する
                    () =>
                    {
                        _enemy.images.autofit = (int) AutoFitEnum.ON;
                        enemyDisplaySizeSliderArea.SetEnabled(false);
                        Save();
                    },
                    //拡大縮小率設定
                    () =>
                    {
                        _enemy.images.autofit = (int) AutoFitEnum.OFF;
                        enemyDisplaySizeSliderArea.SetEnabled(true);
                        Save();
                    }
                    
                });

            RadioButton enemyAlignment0Toggle = RootContainer.Query<RadioButton>("radioButton-battleEdit-display15");
            RadioButton enemyAlignment1Toggle = RootContainer.Query<RadioButton>("radioButton-battleEdit-display16");
            RadioButton enemyAlignment2Toggle = RootContainer.Query<RadioButton>("radioButton-battleEdit-display17");
            enemyAlignment0Toggle.value = (BattleAlignmentEnum) _enemy.images.battleAlignment == BattleAlignmentEnum.AUTO;
            enemyAlignment1Toggle.value = (BattleAlignmentEnum) _enemy.images.battleAlignment == BattleAlignmentEnum.BOTTOM;
            enemyAlignment2Toggle.value = (BattleAlignmentEnum) _enemy.images.battleAlignment == BattleAlignmentEnum.TOP;

            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {enemyAlignment0Toggle, enemyAlignment1Toggle, enemyAlignment2Toggle},
                _enemy.images.battleAlignment, new List<Action>
                {
                    //自動
                    () =>
                    {
                        _enemy.images.battleAlignment =
                            enemyAlignment0Toggle.value ? (int) BattleAlignmentEnum.AUTO : 0;
                        Save();
                    },
                    //下揃え
                    () =>
                    {
                        _enemy.images.battleAlignment =
                            enemyAlignment1Toggle.value ? (int) BattleAlignmentEnum.BOTTOM : 0;
                        Save();
                    },
                    //上揃え
                    () =>
                    {
                        _enemy.images.battleAlignment = enemyAlignment2Toggle.value ? (int) BattleAlignmentEnum.TOP : 0;
                        Save();
                    }
                });
        
            IntegerField enemyExp = RootContainer.Query<IntegerField>("enemy_exp");
            enemyExp.value = _enemy.exp;
            BaseInputFieldHandler.IntegerFieldCallback(enemyExp, evt =>
            {
                _enemy.exp = enemyExp.value;
                Save();
            }, 0, 0);

            IntegerField enemyGold = RootContainer.Query<IntegerField>("enemy_gold");
            enemyGold.value = _enemy.gold;
            BaseInputFieldHandler.IntegerFieldCallback(enemyGold, evt =>
            {
                _enemy.gold = enemyGold.value;
                Save();
            }, 0, 0);

            // オートガイド反映
            enemyAutoguideReflect.clicked += () => { StartAutoGuide(); };


            Button enemyDropItemAdd = RootContainer.Query<Button>("enemy_dropItem_add");
            Foldout enemyDropitemFoldout = RootContainer.Query<Foldout>("enemy_dropitem_foldout");
            _dropItemArea = RootContainer.Query<VisualElement>("enemy_dropItems_area");

            if (_enemy.dropItems.Count > 0)
            {
                for (var i = 0; i < _enemy.dropItems.Count; i++) _SetDropItem(i);
            }
            else
            {
            }

            enemyDropItemAdd.clicked += () =>
            {
                _enemy.dropItems.Add(new EnemyDataModel.DropItem());
                _SetDropItem(_enemy.dropItems.Count - 1);
            };

            Button enemyActionPatternAdd = RootContainer.Query<Button>("enemy_actionPattern_add");
            Foldout enemyActionPatternFoldout = RootContainer.Query<Foldout>("enemy_actionPattern_foldout");
            _actionPatternArea = RootContainer.Query<VisualElement>("enemy_actionPattern_area");
            for (var i = 0; i < _enemy.actions.Count; i++) _SetActionPattern(i);

            enemyActionPatternAdd.clicked += () =>
            {
                _enemy.actions.Add(new EnemyDataModel.EnemyAction());
                _SetActionPattern(_enemy.actions.Count - 1);
                Save();
            };

            FloatField enemyTraitFixed1 = RootContainer.Query<FloatField>("enemy_trait_fixed1");
            FloatField enemyTraitFixed2 = RootContainer.Query<FloatField>("enemy_trait_fixed2");
            VisualElement enemyTraitFixed3 = RootContainer.Query<VisualElement>("enemy_trait_fixed3");

            enemyTraitFixed1.value = _enemy.traits[0].value / 10f;
            BaseInputFieldHandler.FloatFieldCallback(enemyTraitFixed1, evt =>
            {
                _enemy.traits[0].value = (int)(enemyTraitFixed1.value * 10);
                Save();
            }, 0, 100);

            enemyTraitFixed2.value = _enemy.traits[1].value / 10f;
            BaseInputFieldHandler.FloatFieldCallback(enemyTraitFixed2, evt =>
            {
                _enemy.traits[1].value = (int)(enemyTraitFixed2.value * 10);
                Save();
            }, 0, 100);

            var enemyTraitFixedPopupField =
                new PopupFieldBase<string>(ElementList(), _enemy.traits[2].effectId);
            
            enemyTraitFixed3.Add(enemyTraitFixedPopupField);
            enemyTraitFixedPopupField.RegisterValueChangedCallback(evt =>
            {
                _enemy.traits[2].effectId = enemyTraitFixedPopupField.index;
                Save();
            });

            var traitsWork = new List<TraitCommonDataModel>();
            if (_enemy.traits.Count > 3)
                for (var i = 3; i < _enemy.traits.Count; i++)
                    traitsWork.Add(_enemy.traits[i]);

            //上記で固定特徴を持っているので3つ飛ばす
            List<TraitCommonDataModel> enemyTraits;
            VisualElement classTraitsArea = RootContainer.Query<VisualElement>("class_traits_area");
            var traitWindow = new TraitsInspectorElement();
            classTraitsArea.Add(traitWindow);
            traitWindow.Init(traitsWork, TraitsType.TRAITS_TYPE_ENEMY, evt =>
            {
                enemyTraits = (List<TraitCommonDataModel>) evt;
                //上で三つ飛ばした部分から更新をする
                traitsWork = enemyTraits;

                //固定以外の特徴の削除
                for (var i = _enemy.traits.Count - 1; i > 2; i--) _enemy.traits.RemoveAt(i);

                //固定以外の特徴の更新
                for (var i = 0; i < enemyTraits.Count; i++)
                    _enemy.traits.Add(enemyTraits[i]);

                for (var i = 0; i < _enemyDataModels.Count; i++)
                    if (_enemyDataModels[i].id == _enemy.id)
                    {
                        _enemyDataModels[i] = _enemy;
                        break;
                    }

                //保存
                Save();
            });
       }

        // オートガイドのパラメータ設定
        private void StartAutoGuide() {
            //オートガイドのエレメント取得
            IntegerField enemyAutoguideLevel = RootContainer.Query<IntegerField>("enemy_autoguide_level");
            FloatField enemyAutoguideAttackTurn = RootContainer.Query<FloatField>("enemy_autoguide_attackTurn");
            FloatField enemyAutoguideGuardTurn = RootContainer.Query<FloatField>("enemy_autoguide_guardTurn");
            //基本能力値
            IntegerField enemyMaxHp = RootContainer.Query<IntegerField>("enemy_maxHp");
            IntegerField enemyMaxMp = RootContainer.Query<IntegerField>("enemy_maxMp");
            IntegerField enemyAttack = RootContainer.Query<IntegerField>("enemy_attack");
            IntegerField enemyGuard = RootContainer.Query<IntegerField>("enemy_guard");
            IntegerField enemyMagic = RootContainer.Query<IntegerField>("enemy_magic");
            IntegerField enemyMagicGuard = RootContainer.Query<IntegerField>("enemy_magicGuard");
            IntegerField enemySpeed = RootContainer.Query<IntegerField>("enemy_speed");
            IntegerField enemyLuck = RootContainer.Query<IntegerField>("enemy_luck");
            IntegerField enemyExp = RootContainer.Query<IntegerField>("enemy_exp");
            IntegerField enemyGold = RootContainer.Query<IntegerField>("enemy_gold");

            //オートガイドが参照する項目の取得
            var classDataModel = databaseManagementService.LoadClassCommon()[0];
            var clearLevel = classDataModel.clearLevel; //クリアレベル
            var maxLevel = classDataModel.maxLevel; //最大レベル
            var expGainIncreaseValue = classDataModel.expGainIncreaseValue; //経験値の上限
            var maxHp = classDataModel.baseHpMaxValue;
            var skillCommonDataModel = databaseManagementService.LoadSkillCommon()[0];

            //オートガイド共通計算用の、標準モデル
            var standardModel = CalcStandardModel(maxLevel, clearLevel, expGainIncreaseValue, maxHp, 0);
            //想定習得レベルでのパラメータを算出
            standardModel.CalcAssumedLevel(enemyAutoguideLevel.value);
            standardModel.DebugLog();

            //攻撃力の計算
            //---------------------------------------------------------------
            var attack =
                Mathf.RoundToInt(
                    (standardModel.hp / enemyAutoguideAttackTurn.value +
                     standardModel.defense * skillCommonDataModel.damage.normalAttack.bMag) /
                    skillCommonDataModel.damage.normalAttack.aMag);

            //防御力の計算
            //---------------------------------------------------------------
            //敵キャラの防御力 = 遭遇想定レベルの標準モデル攻撃力 * 0.5
            var defenceWork = (float)Math.Round(standardModel.attack * 0.5f, MidpointRounding.AwayFromZero);
            var defence = Mathf.RoundToInt(defenceWork);

            //HPの計算
            //---------------------------------------------------------------
            var hp = Mathf.RoundToInt(standardModel.attack *
                                      (skillCommonDataModel.damage.normalAttack.aMag -
                                       skillCommonDataModel.damage.normalAttack.bMag * 0.5f) *
                                      enemyAutoguideGuardTurn.value);

            // 魔法防御の計算
            //---------------------------------------------------------------
            // 計算用パラメータ
            float paramA = 0;
            float paramB = 0;
            float paramC = 0;

            // パラメータ設定
            switch (_enemy.autoGuide.magicGuardSetting)
            {
                //弱い
                case 0:
                    // 初期値を使用
                    paramA = 0.2f;
                    break;
                //普通
                case 1:
                    paramA = 1.0f;
                    break;
                //強い
                case 2:
                    paramA = 2.0f;
                    break;
            }

            //係数値
            //=(最小HP / 4 + 最小魔法防御) / 最小魔法力 * 0.26
            var a = (standardModel.minHp / 4.0f + standardModel.minMagicDefense) / standardModel.minMagic *
                    0.26f;
            var c = a * 2.0f;
            var b = c * ((clearLevel * 1f) / (maxLevel* 1f) * 0.15f + 0.85f);
            float amag = 0f;

            //攻撃側倍率 =ROUND(a + (b - a) / (クリアレベル - 1) * (想定レベル - 1),3)
            amag = a + ((b - a) / (clearLevel - 1)) * (enemyAutoguideLevel.value - 1);
            amag = (float) Math.Round(amag, 3);

            //固定値
            var cdmg = standardModel.hp / 4 + standardModel.magicDefense - standardModel.magic * amag;
            cdmg = (float) Math.Round(cdmg, 0);

            //対象側倍率の変更 (b.mag)
            //var bmag = 1.0f;

            // 魔法防御力設定
            var magicDefence =
                Mathf.RoundToInt((cdmg + standardModel.magic * amag - hp / enemyAutoguideGuardTurn.value) * paramA);

            // 魔法力の計算
            //---------------------------------------------------------------
            switch (_enemy.autoGuide.magicPowerSetting)
            {
                //なし
                case 0:
                    // 初期値を使用
                    paramA = 0;
                    break;
                //普通
                case 1:
                    paramA = 1.0f;
                    break;
                //強い
                case 2:
                    paramA = 1.43f;
                    break;
            }

            // 魔法力の設定
            // =ROUND((( HP 想定レベルでの標準モデルの値 / 攻撃ターン数 + 魔法防御 想定レベルでの標準モデルの値 - cdmg ) / amag)*paramA,0)
            var magic = Mathf.RoundToInt((standardModel.hp / enemyAutoguideAttackTurn.value + standardModel.magicDefense - cdmg) / amag * paramA);

            // MPの計算
            //---------------------------------------------------------------
            switch (_enemy.autoGuide.magicPowerSetting)
            {
                //なし
                case 0:
                    // 初期値を使用
                    paramC = 0;
                    paramA = 0;
                    paramB = 0;
                    break;
                //普通
                case 1:
                    paramC = standardModel.maxMp * 1.0f;
                    paramA = paramC * 0.045f;
                    paramB = paramC * (1.0f * clearLevel / maxLevel * 0.15f + 0.85f);
                    break;
                //強い
                case 2:
                    paramC = standardModel.maxMp * 1.43f;
                    paramA = paramC * 0.045f;
                    paramB = paramC * (1.0f * clearLevel / maxLevel * 0.15f + 0.85f);
                    break;
            }

            // MPの設定
            var mp = 0;
            // =ROUNDDOWN(paramA + (paramB - paramA) * (遭遇想定レベル - 1) / (クリアレベル - 1),0)
            mp = (int) (paramA + (paramB - paramA) * (enemyAutoguideLevel.value - 1) / (clearLevel - 1));

            // 敏捷性の計算
            //---------------------------------------------------------------
            paramC = standardModel.maxSpeed * 0.85f;
            paramA = paramC * 0.045f;
            paramB = paramC * ((1.0f * clearLevel) / (maxLevel * 1f) * 0.15f + 0.85f);

            // 敏捷性の設定
            var speed = 0;
            if (enemyAutoguideLevel.value < clearLevel + 1)
                //敵キャラの敏捷性 = a + (b - a) * (遭遇想定レベル - 1) / ( クリアレベル - 1)
                speed = (int) (paramA + (paramB - paramA) * (enemyAutoguideLevel.value - 1) / (clearLevel - 1));
            else
                //敵キャラの敏捷性 =ROUNDDOWN(b + (c - b) * (遭遇想定レベル - クリアレベル) / (最大レベル  - クリアレベル),0)
                speed = (int) (paramB + (paramC - paramB) * (enemyAutoguideLevel.value - clearLevel) /
                    (maxLevel - clearLevel));

            // 運の計算
            //---------------------------------------------------------------
            paramC = standardModel.maxLuck * 0.85f;
            paramA = paramC * 0.045f;
            // paramC * (1.0 * クリアレベル / 最大レベル * 0.15 + 0.85)
            paramB = paramC * (1.0f * clearLevel / maxLevel * 0.15f + 0.85f);
            


            var luck = 0;
            if (enemyAutoguideLevel.value < clearLevel + 1)
            {
                luck = (int) (paramA + (paramB - paramA) * (enemyAutoguideLevel.value - 1) / (clearLevel - 1));
            }
            else
            {
                //運 = ROUNDDOWN(b + (c - b) * (遭遇想定レベル - クリアレベル) / (最大レベル  - クリアレベル),0)
                var luck1 = (paramC - paramB) * (enemyAutoguideLevel.value - clearLevel);
                var luck2 = (maxLevel - clearLevel) * 1f;
                luck = (int) (paramB + luck1 / luck2);
            }

            // 所持金の計算
            //---------------------------------------------------------------
            // 武器の買値算出
            // 係数
            float priceParamA = 5f;
            float priceParamB = 600f;
            var priceParamC = priceParamB * ((clearLevel * 1f) / (maxLevel * 1f) * 0.15f + 0.85f);

            var price = 0;
            var priceCoef = 0f;
            if (enemyAutoguideLevel.value < clearLevel + 1)
            {
                //係数1 = a + (b - a) * (入手想定レベル - 1) ^ 2 / (クリアレベル-1) ^ 2
                priceCoef = priceParamA + (priceParamC - priceParamA) *
                    Mathf.Pow(enemyAutoguideLevel.value - 1, 2) / Mathf.Pow(clearLevel - 1, 2);
                //買値 = 入手レベルの武器攻撃力 * 係数1
                //price = (int) (attack * priceCoef);
            }
            else
            {
                //係数1 = b + (c - b) * (入手想定レベル - クリアレベル) / (最大レベル - クリアレベル)
                priceCoef = priceParamC + (priceParamB - priceParamC) * (enemyAutoguideLevel.value - clearLevel) /
                    (maxLevel - clearLevel);
                //買値 = 入手レベルの武器攻撃力 * 係数1
                //price = (int) (attack * priceCoef);
            }
            //武器の攻撃力の計算
            float attackParamB = maxHp * 0.25f;
            float attackParamA = attackParamB * 0.045f;
            var attackLevel = (clearLevel * 1f / maxLevel * 1f);
            float attackParamC = attackParamB * (attackLevel * 0.15f + 0.85f);
            float armsCoef = 0f;
            if (enemyAutoguideLevel.value < clearLevel + 1)
            {
                armsCoef = attackParamA +
                           (attackParamC - attackParamA) * (enemyAutoguideLevel.value - 1) / (clearLevel - 1);
            }
            else
            {
                armsCoef = attackParamC +
                           (attackParamB - attackParamC) * (enemyAutoguideLevel.value - clearLevel) / (maxLevel - clearLevel);
                
            }

            price = (int)(priceCoef * armsCoef);


            // 所持金用パラメータ
            paramA = 2.5f;
            paramB = 372f;
            paramC = paramB * ((clearLevel * 1f) / (maxLevel * 1f) * 0.15f + 0.85f);

            // 所持金の設定
            var moneyCoef = 0f;
            var money = 0;
            if (enemyAutoguideLevel.value < clearLevel + 1)
            {
                //係数1 = a + (b - a) * (遭遇想定レベル - 1) ^ 2 / (クリアレベル-1) ^ 2
                moneyCoef = paramA + (paramC - paramA) * (Mathf.Pow(enemyAutoguideLevel.value - 1, 2f)) / (Mathf.Pow(clearLevel - 1, 2f));
                //所持金 = 遭遇想定レベルの武器買値 / 係数1
                money = (int) (price / moneyCoef);
            }
            else if (enemyAutoguideLevel.value > clearLevel)
            {
                //係数1 = b + (c - b) * (遭遇想定レベル - クリアレベル) / (最大レベル - クリアレベル)
                moneyCoef = paramC + (paramB - paramC) * (enemyAutoguideLevel.value - clearLevel) / (maxLevel - clearLevel);
                //所持金 = 遭遇想定レベルの武器買値 / 係数1
                money = (int) (price / moneyCoef);
            }
            

            //経験値の計算
            //---------------------------------------------------------------
            //傾きA
            paramA = 3.5f;
            //傾きB
            paramB = 540;
            //補正値
            paramC = 30;

            // 計算用値
            //係数1 = (クリアレベル - 1) ^ 傾きA
            var expCoef1 = Mathf.Pow(clearLevel - 1, paramA);
            //係数2 = クリアレベル ^ 傾きA - (クリアレベル - 1) ^ 傾きA
            var expCoef2 = Mathf.Pow(clearLevel, paramA) - Mathf.Pow(clearLevel - 1, paramA);
            //最大レベルの計算値 = ((最大レベル - クリアレベル) / (1 + 1 / 2.95)  / 傾きB + 1) * 係数2 * (最大レベル - クリアレベル) + 係数1
            var maxLevelParam =
                ((maxLevel - clearLevel) / (1 + 1 / 2.95f) / paramB + 1) * expCoef2 * (maxLevel - clearLevel) +
                expCoef1;
            //平均化係数 = (経験値の上限 - 補正値 * (最大レベル - 1)) / 最大レベルの計算値
            var averageCoef = (expGainIncreaseValue - paramC * (maxLevel - 1)) / maxLevelParam;

            //経験値1の計算
            float expParam1 = 0;
            if (enemyAutoguideLevel.value < clearLevel + 1)
            {
                //経験値1 = ((遭遇想定レベル - 1) ^ 傾きA) * 平均化係数 + (遭遇想定レベル - 1) * 補正値
                expParam1 = (int) (Math.Pow(enemyAutoguideLevel.value - 1, paramA) * averageCoef +
                                   (enemyAutoguideLevel.value - 1) * paramC);
            }
            else
            {
                //係数3 = (遭遇想定レベル - クリアレベル)
                float expCoef3 = enemyAutoguideLevel.value - clearLevel;
                //経験値1 = ((係数3 / (1 + 係数3 ^ 2 / ((最大レベル - クリアレベル) ^ 2 * 2.95)) / 傾きB + 1) * 係数2 * 係数3 + 係数1) * 平均化係数 + (遭遇想定レベル - 1) * 補正値
                //経験値1 = ((expCoef3 / (1 + expCoef3 ^ 2 / (( 最大レベル - クリアレベル) ^ 2 * 2.95)) / paramB + 1) * expCoef2 * expCoef3 + expCoef1) * 平均化係数 + (想定レベル - 1) * paramC
                expParam1 =
                    ((expCoef3 / (1 + Mathf.Pow(expCoef3, 2) / (Mathf.Pow(maxLevel - clearLevel, 2) * 2.95f)) / paramB +
                      1) * expCoef2 * expCoef3 + expCoef1) * averageCoef + (enemyAutoguideLevel.value - 1) * paramC;

                expParam1 =
                    ((expCoef3 / (1f + Mathf.Pow(expCoef3, 2) / (Mathf.Pow(maxLevel - clearLevel, 2) * 2.95f)) / paramB + 
                      1) * expCoef2 * expCoef3 + expCoef1) * averageCoef + (enemyAutoguideLevel.value - 1) * paramC;
            }

            //経験値2の計算
            float expParam2 = 0;
            if (enemyAutoguideLevel.value < clearLevel + 1)
            {
                //経験値2 = 遭遇想定レベル ^ 傾きA * 平均化係数 + 遭遇想定レベル * 補正値
                expParam2 = Mathf.Pow(enemyAutoguideLevel.value, paramA) * averageCoef +
                            enemyAutoguideLevel.value * paramC;
            }
            else
            {
                //係数3 = (遭遇想定レベル + 1 - クリアレベル)
                float expCoef3 = enemyAutoguideLevel.value + 1 - clearLevel;
                //経験値2 = ((係数3 / (1 + 係数3 ^ 2 / ((最大レベル - クリアレベル) ^ 2 * 2.95)) / 傾きB + 1) * 係数2 * 係数3 + 係数1) * 平均化係数 + 遭遇想定レベル * 補正値
                expParam2 =
                    ((expCoef3 / (1 + Mathf.Pow(expCoef3, 2) / (Mathf.Pow(maxLevel - clearLevel, 2) * 2.95f)) / paramB +
                      1) * expCoef2 * expCoef3 + expCoef1) * averageCoef + enemyAutoguideLevel.value * paramC;
            }

            //次レベルまでの経験値
            var nextLevelExp = expParam2 - expParam1;

            // 経験値計算
            expCoef1 = 0.135f;
            expCoef2 = 0.135f / paramA;

            // 経験値の設定
            var exp = 0;
            if (enemyAutoguideLevel.value < clearLevel + 1)
            {
                //係数3 = 0.135 - (遭遇想定レベル - 1) * (係数1 - 係数2) / (クリアレベル - 1)
                var expCoef3 = 0.135f - (enemyAutoguideLevel.value - 1) * (expCoef1 - expCoef2) / (clearLevel - 1);
                //経験値 = 次のレベルまでの値 * 係数3
                exp = (int) (nextLevelExp * expCoef3);
            }
            else
            {
                //経験値 = 次のレベルまでの値 * 係数2
                exp = (int) (nextLevelExp * expCoef2);
            }

            // パラメータの反映
            //---------------------------------------------------------------
            // フィールドに反映
            IntegerField enemyAutoGuideMaxHp = RootContainer.Query<IntegerField>("enemy_autoguide_maxHp");
            IntegerField enemyAutoGuideMaxMp = RootContainer.Query<IntegerField>("enemy_autoguide_maxMp");
            IntegerField enemyAutoGuideAttack = RootContainer.Query<IntegerField>("enemy_autoguide_attack");
            IntegerField enemyAutoGuideGuard = RootContainer.Query<IntegerField>("enemy_autoguide_guard");
            IntegerField enemyAutoGuideMagic = RootContainer.Query<IntegerField>("enemy_autoguide_magic");
            IntegerField enemyAutoGuideMagicGuard = RootContainer.Query<IntegerField>("enemy_autoguide_magicGuard");
            IntegerField enemyAutoGuideSpeed = RootContainer.Query<IntegerField>("enemy_autoguide_speed");
            IntegerField enemyAutoGuideLuck = RootContainer.Query<IntegerField>("enemy_autoguide_luck");
            IntegerField enemyAutoGuideExp = RootContainer.Query<IntegerField>("enemy_autoguide_exp");
            IntegerField enemyAutoGuideMoney = RootContainer.Query<IntegerField>("enemy_autoguide_money");

            // 入力している値によっては、各種値がマイナスになる可能性がある
            // マイナスの値は0にする
            hp = hp < 0 ? 0 : hp;
            mp = mp < 0 ? 0 : mp;
            attack = attack < 0 ? 0 : attack;
            defence = defence < 0 ? 0 : defence;
            magic = magic < 0 ? 0 : magic;
            magicDefence = magicDefence < 0 ? 0 : magicDefence;
            speed = speed < 0 ? 0 : speed;
            luck = luck < 0 ? 0 : luck;
            exp = exp < 0 ? 0 : exp;
            money = money < 0 ? 0 : money;

            enemyAutoGuideMaxHp.value = hp;
            enemyAutoGuideMaxMp.value = mp;
            enemyAutoGuideAttack.value = attack;
            enemyAutoGuideGuard.value = defence;
            enemyAutoGuideMagic.value = magic;
            enemyAutoGuideMagicGuard.value = magicDefence;
            enemyAutoGuideSpeed.value = speed;
            enemyAutoGuideLuck.value = luck;
            enemyAutoGuideExp.value = exp;
            enemyAutoGuideMoney.value = money;

            //オートガイド仕様上、能力値、報酬にも反映することになっている
            //従って、同一の値を反映する
            //最大HP、最大MP、攻撃力、防御力、魔法力、魔法防御、俊敏性、運														
            _enemy.param[0] = hp;
            _enemy.param[1] = mp;
            _enemy.param[2] = attack;
            _enemy.param[3] = defence;
            _enemy.param[4] = magic;
            _enemy.param[5] = magicDefence;
            _enemy.param[6] = speed;
            _enemy.param[7] = luck;
            _enemy.exp = exp;
            _enemy.gold = money;

            enemyMaxHp.value = hp;
            enemyMaxMp.value = mp;
            enemyAttack.value = attack;
            enemyGuard.value = defence;
            enemyMagic.value = magic;
            enemyMagicGuard.value = magicDefence;
            enemySpeed.value = speed;
            enemyLuck.value = luck;
            enemyExp.value = exp;
            enemyGold.value = money;
            Save();
        }

        /// <summary>
        ///     特徴一覧に要素を追加する
        /// </summary>
        /// <param name="index">特徴一覧でのインデックス</param>
        private void _SetTraits(int index) {
            var traitFoldout = new Foldout();
            traitFoldout.name = "foldout_traits_" + _enemy.id + "_" + index;
            BaseClickHandler.ClickEvent(traitFoldout, evt =>
            {
                _currentType = CurrentType.Traits;
                _currentSelectId = index;
                HandleRightClickDelete(evt);
            });

            traitFoldout.AddToClassList("character_foldout");
            traitFoldout.text = EditorLocalize.LocalizeText("WORD_0347") + (index + 1);
            _enemyTraitsArea.Add(traitFoldout);
            var categoryIdList = EditorLocalize.LocalizeTexts(new List<string>
                {"category1", "category2", "category3", "category4", "category5", "category6"});
            var categoryIdPopupField =
                new PopupFieldBase<string>(categoryIdList, _enemy.traits[index].categoryId);
            traitFoldout.Add(categoryIdPopupField);

            var effectIdList = EditorLocalize.LocalizeTexts(new List<string>
                {"effect1", "effect2", "effect3", "effect4", "effect5", "effect6"});
            var effectIdPopupField =
                new PopupFieldBase<string>(effectIdList, _enemy.traits[index].effectId);
            traitFoldout.Add(effectIdPopupField);

            var traitsIdList = EditorLocalize.LocalizeTexts(new List<string>
                {"traits1", "traits2", "traits3", "traits4", "traits5", "traits6"});
            var traitsIdPopupField =
                new PopupFieldBase<string>(traitsIdList, _enemy.traits[index].traitsId);
            traitFoldout.Add(traitsIdPopupField);

            var t = new IntegerField();
            t.label = EditorLocalize.LocalizeText("WORD_0537");
            t.maxLength = 3;
            t.value = _enemy.traits[index].value;
            t.RegisterCallback<FocusOutEvent>(evt => { _enemy.traits[index].value = t.value; });
            traitFoldout.Add(t);

            SetFoldout(traitFoldout);
        }

        /// <summary>
        ///     ドロップアイテム一覧に要素を追加する
        /// </summary>
        /// <param name="index">ドロップアイテム一覧でのインデックス</param>
        private void _SetDropItem(int index) {
            // ドロップアイテムリスト内のフォールドアウト単体
            var itemFoldout = new Foldout();
            itemFoldout.name = "foldout_dropitem_" + _enemy.id + "_" + index;

            BaseClickHandler.ClickEvent(itemFoldout, evt =>
            {
                _currentType = CurrentType.DropItem;
                _currentSelectId = index;
                HandleRightClickDelete(evt);
            });
            itemFoldout.text = EditorLocalize.LocalizeText("WORD_2591") + " " + (index + 1);

            // アイテム選択部分
            VisualElement itemSelectElement = new InspectorItemUnit();
            itemSelectElement.AddToClassList("enemy_list_area");
            itemFoldout.Add(itemSelectElement);
            var l = new Label(EditorLocalize.LocalizeText("WORD_0082"));
            itemSelectElement.Add(l);
            var dropItemList = new List<string>();
            {
                // 武器・防具・アイテムの名前を一通り選択肢に追加
                dropItemList.AddRange(_weaponDataModels.Select(v => v.basic.name));
                dropItemList.AddRange(_armorDataModels.Select(v => v.basic.name));
                dropItemList.AddRange(_itemDataModels.Select(v => v.basic.name));
            }
            var dropItemPopupField =
                new PopupFieldBase<string>(dropItemList, IdToIndex(_enemy.dropItems[index].dataId));

            //idをいい感じのIndexにするメソッド
            int IdToIndex(string id) {
                var returnIndex = 0;

                returnIndex = _weaponDataModels.FindIndex(v => id == v.basic.id);
                if (returnIndex > -1) return returnIndex;

                returnIndex = _armorDataModels.FindIndex(v => id == v.basic.id);
                if (returnIndex > -1) return returnIndex + _weaponDataModels.Count;

                returnIndex = _itemDataModels.FindIndex(v => id == v.basic.id);
                if (returnIndex > -1) return returnIndex + _weaponDataModels.Count + _armorDataModels.Count;

                return 0;
            }

            dropItemPopupField.style.width = 200;
            itemSelectElement.Add(dropItemPopupField);

            dropItemPopupField.RegisterValueChangedCallback(evt =>
            {
                if (_weaponDataModels.Count > dropItemPopupField.index)
                {
                    _enemy.dropItems[index].kind = (int) GameItem.DataClassEnum.Weapon;
                    _enemy.dropItems[index].dataId =
                        _weaponDataModels[dropItemPopupField.index].basic.id;
                }
                else if (_weaponDataModels.Count + _armorDataModels.Count >
                         dropItemPopupField.index)
                {
                    _enemy.dropItems[index].kind = (int) GameItem.DataClassEnum.Armor;
                    _enemy.dropItems[index].dataId =
                        _armorDataModels[-_weaponDataModels.Count + dropItemPopupField.index].basic.id;
                }
                else if (_weaponDataModels.Count + _armorDataModels.Count +
                    _itemDataModels.Count > dropItemPopupField.index)
                {
                    _enemy.dropItems[index].kind = (int) GameItem.DataClassEnum.Item;
                    _enemy.dropItems[index].dataId = _itemDataModels[
                        -_weaponDataModels.Count - _armorDataModels.Count +
                        dropItemPopupField.index].basic.id;
                }

                Save();
            });

            // レーティング入力部分
            var unit2 = new InspectorItemUnit();
            var ratingInputField = new IntegerField();
            ratingInputField.maxLength = 3;
            ratingInputField.value = _enemy.dropItems[index].denominator;
            BaseInputFieldHandler.IntegerFieldCallback(ratingInputField, evt =>
            {
                _enemy.dropItems[index].denominator = ratingInputField.value;
                Save();
            }, 0, 100);
            unit2.Add(new Label {text = EditorLocalize.LocalizeText("WORD_0598")});
            unit2.Add(ratingInputField);
            itemFoldout.Add(unit2);

            _dropItemArea.Add(itemFoldout);

            SetFoldout(itemFoldout);
        }

        /// <summary>
        ///     行動パターン一覧に要素を追加する
        /// </summary>
        /// <param name="index">ド行動パターン一覧でのインデックス</param>
        private void _SetActionPattern(int index) {
            var actionPatternFoldout = new Foldout();
            actionPatternFoldout.name = "foldout_actionpattern_" + _enemy.id + "_" + index;
            BaseClickHandler.ClickEvent(actionPatternFoldout, evt =>
            {
                _currentType = CurrentType.ActionPattern;
                _currentSelectId = index;
                HandleRightClickDelete(evt);
            });
            actionPatternFoldout.AddToClassList("character_foldout");
            actionPatternFoldout.text = EditorLocalize.LocalizeText("WORD_2590") + " " + (index + 1);

            VisualElement skillList = new InspectorItemUnit();
            skillList.AddToClassList("enemy_list_area");
            actionPatternFoldout.Add(skillList);
            var l = new Label(EditorLocalize.LocalizeText("WORD_0069"));
            skillList.Add(l);
            var customs = _skillCustomDataModels;
            var actionList = new List<string>();
            var customNum = 0;
            for (var i = 0; i < customs.Count; i++)
            {
                actionList.Add(customs[i].basic.name);
                if (_enemy.actions[index].skillId == customs[i].basic.id) customNum = i;
            }

            var actionPopupField =
                new PopupFieldBase<string>(actionList, customNum);
            actionPopupField.style.width = 200;
            skillList.Add(actionPopupField);
            actionPopupField.RegisterValueChangedCallback(evt =>
            {
                _enemy.actions[index].skillId = customs[actionPopupField.index].basic.id;
                Save();
            });

            var emenyRatingContainer = new InspectorItemUnit();
            var enemyRating = new IntegerField();
            if (_enemy.actions[index].rating < 1)
            {
                _enemy.actions[index].rating = 1;
            }
            enemyRating.value = _enemy.actions[index].rating;
            BaseInputFieldHandler.IntegerFieldCallback(enemyRating,
                evt => { _enemy.actions[index].rating = enemyRating.value; }, 1, 9);
            emenyRatingContainer.Add(new Label {text = EditorLocalize.LocalizeText("WORD_0832")});
            emenyRatingContainer.Add(enemyRating);
            actionPatternFoldout.Add(emenyRatingContainer);

            var pattern = new Foldout();
            pattern.name = "foldout_actionpattern_" + _enemy.id + "_" + index + "_pattern";
            pattern.text = EditorLocalize.LocalizeText("WORD_0600");
            actionPatternFoldout.Add(pattern);
            var patternArea = new VisualElement();
            pattern.Add(patternArea);
            var patternList =
                EditorLocalize.LocalizeTexts(new List<string>
                    {"WORD_0427", "WORD_0601", "WORD_0133", "WORD_0135", "WORD_0602", "WORD_0603", "WORD_0605"});
            var patternPopupField =
                new PopupFieldBase<string>(patternList, _enemy.actions[index].conditionType);
            patternArea.Add(patternPopupField);

            var turnArea = new VisualElement();
            actionPatternFoldout.Add(turnArea);
            var turnName = new Foldout();
            turnName.name = "foldout_actionpattern_" + _enemy.id + "_" + index + "_turn";
            turnName.text = EditorLocalize.LocalizeText("WORD_1585");
            turnArea.Add(turnName);

            var turnParam1Container = new InspectorItemUnit();
            var turnParam1 = new IntegerField();
            turnParam1.value = _enemy.actions[index].conditionParam1;
            BaseInputFieldHandler.IntegerFieldCallback(turnParam1, evt =>
            {
                _enemy.actions[index].conditionParam1 = turnParam1.value;
                Save();
            }, 0, 9999);
            turnParam1Container.Add(new Label {text = EditorLocalize.LocalizeText("WORD_2593")});
            turnParam1Container.Add(turnParam1);
            turnName.Add(turnParam1Container);

            var turnParam2Container = new InspectorItemUnit();
            var turnParam2 = new IntegerField();
            turnParam2.value = _enemy.actions[index].conditionParam2;
            BaseInputFieldHandler.IntegerFieldCallback(turnParam2, evt =>
            {
                _enemy.actions[index].conditionParam2 = turnParam2.value;
                Save();
            }, 0, 9999);
            turnParam2Container.Add(new Label {text = EditorLocalize.LocalizeText("WORD_1586")});
            turnParam2Container.Add(turnParam2);
            turnName.Add(turnParam2Container);

            var hpArea = new VisualElement();
            actionPatternFoldout.Add(hpArea);
            var hpName = new Foldout();
            hpName.name = "foldout_actionpattern_" + _enemy.id + "_" + index + "_hp";
            hpName.text = EditorLocalize.LocalizeText("WORD_1587");
            hpArea.Add(hpName);

            var hpParam1Container = new InspectorItemUnit();
            var hpParam1 = new IntegerField();
            var hpParam2 = new IntegerField();
            hpParam1.value = _enemy.actions[index].conditionParam1;
            BaseInputFieldHandler.IntegerFieldCallback(hpParam1, evt =>
            {
                _enemy.actions[index].conditionParam1 = hpParam1.value;
                if (hpParam1.value > hpParam2.value)
                {
                    hpParam2.value = hpParam1.value;
                    _enemy.actions[index].conditionParam2 = hpParam2.value;
                }
                Save();
            }, 0, 100);
            hpParam1Container.Add(new Label {text = EditorLocalize.LocalizeText("WORD_1588")});
            hpParam1Container.Add(hpParam1);
            hpName.Add(hpParam1Container);
            var hpParam2Container = new InspectorItemUnit();
            hpParam2.value = _enemy.actions[index].conditionParam2;
            BaseInputFieldHandler.IntegerFieldCallback(hpParam2, evt =>
            {
                _enemy.actions[index].conditionParam2 = hpParam2.value;
                Save();
            }, 0, 100);
            hpParam2Container.Add(new Label {text = EditorLocalize.LocalizeText("WORD_1589")});
            hpParam2Container.Add(hpParam2);
            hpName.Add(hpParam2Container);

            var mpArea = new VisualElement();
            actionPatternFoldout.Add(mpArea);
            var mpName = new Foldout();
            hpName.name = "foldout_actionpattern_" + _enemy.id + "_" + index + "_mp";
            mpName.text = EditorLocalize.LocalizeText("WORD_1590");
            mpArea.Add(mpName);
            var mpParam1Container = new InspectorItemUnit();
            var mpParam1 = new IntegerField();
            var mpParam2 = new IntegerField();
            mpParam1.value = _enemy.actions[index].conditionParam1;
            BaseInputFieldHandler.IntegerFieldCallback(mpParam1, evt =>
            {
                _enemy.actions[index].conditionParam1 = mpParam1.value;
                if (mpParam1.value > mpParam2.value)
                {
                    mpParam2.value = mpParam1.value;
                    _enemy.actions[index].conditionParam2 = mpParam2.value;
                }
                Save();
            }, 0, 100);
            mpParam1Container.Add(new Label {text = EditorLocalize.LocalizeText("WORD_1588")});
            mpParam1Container.Add(mpParam1);
            mpName.Add(mpParam1Container);
            var mpParam2Container = new InspectorItemUnit();
            mpParam2.value = _enemy.actions[index].conditionParam2;
            BaseInputFieldHandler.IntegerFieldCallback(mpParam2, evt =>
            {
                _enemy.actions[index].conditionParam2 = mpParam2.value;
                Save();
            }, 0, 100);
            mpParam2Container.Add(new Label {text = EditorLocalize.LocalizeText("WORD_1589")});
            mpParam2Container.Add(mpParam2);
            mpName.Add(mpParam2Container);

            var stateArea = new VisualElement();
            actionPatternFoldout.Add(stateArea);
            var stateName = new Foldout();
            hpName.name = "foldout_actionpattern_" + _enemy.id + "_" + index + "_state";
            stateName.text = EditorLocalize.LocalizeText("WORD_1591");
            stateArea.Add(stateName);
            var stateList = new List<string>();
            foreach (var name in _stateDataModels) stateList.Add(name.name);

            var statePopupField =
                new PopupFieldBase<string>(stateList, _enemy.actions[index].conditionParam1);

            stateName.Add(statePopupField);
            statePopupField.RegisterValueChangedCallback(evt =>
            {
                _enemy.actions[index].conditionParam1 = statePopupField.index;
                Save();
            });

            var partyLvArea = new VisualElement();
            actionPatternFoldout.Add(partyLvArea);
            var partyLvName = new Foldout();
            hpName.name = "foldout_actionpattern_" + _enemy.id + "_" + index + "_partylv";
            partyLvName.text = EditorLocalize.LocalizeText("WORD_1592");
            partyLvArea.Add(partyLvName);
            var partyLvParam1Container = new InspectorItemUnit();
            var partyLvParam1 = new IntegerField();
            partyLvParam1.value = _enemy.actions[index].conditionParam1;
            BaseInputFieldHandler.IntegerFieldCallback(partyLvParam1, evt =>
            {
                _enemy.actions[index].conditionParam1 = partyLvParam1.value;
                Save();
            }, 1, 99);
            partyLvParam1Container.Add(new Label
                {text = EditorLocalize.LocalizeText("WORD_2589")});
            partyLvParam1Container.Add(partyLvParam1);
            partyLvName.Add(partyLvParam1Container);

            //スイッチ選択
            var switchArea = new VisualElement();
            actionPatternFoldout.Add(switchArea);
            var switchName = new Foldout();
            hpName.name = "foldout_actionpattern_" + _enemy.id + "_" + index + "_switch";
            switchName.text = EditorLocalize.LocalizeText("WORD_1593");
            switchArea.Add(switchName);
            var flags = databaseManagementService.LoadFlags().switches;
            var switchList = new List<string>();
            var count = 0;
            foreach (var flag in flags)
            {
                count++;
                switchList.Add(flag.name);
            }

            var switchPopupField =
                new PopupFieldBase<string>(switchList, 0);

            if (_enemy.actions[index].conditionType == (int) AttackPatternEnum.SWITCH &&
                _enemy.actions[index].conditionParam1 < switchList.Count)
                statePopupField.index = _enemy.actions[index].conditionParam1;

            switchName.Add(switchPopupField);
            switchPopupField.RegisterValueChangedCallback(evt =>
            {
                _enemy.actions[index].conditionParam1 = switchPopupField.index;
                Save();
            });

            turnArea.style.display = (AttackPatternEnum) _enemy.actions[index].conditionType == AttackPatternEnum.TURN
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            hpArea.style.display = (AttackPatternEnum) _enemy.actions[index].conditionType == AttackPatternEnum.HP
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            mpArea.style.display = (AttackPatternEnum) _enemy.actions[index].conditionType == AttackPatternEnum.MP
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            
            stateArea.style.display = (AttackPatternEnum) _enemy.actions[index].conditionType == AttackPatternEnum.STATE
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            partyLvArea.style.display =
                (AttackPatternEnum) _enemy.actions[index].conditionType == AttackPatternEnum.PARTY_LV
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            switchArea.style.display =
                (AttackPatternEnum) _enemy.actions[index].conditionType == AttackPatternEnum.SWITCH
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

            patternPopupField.RegisterValueChangedCallback(evt =>
            {
                _enemy.actions[index].conditionType = patternPopupField.index;
                // 初期値代入
                _enemy.actions[index].conditionParam1 = 0;
                _enemy.actions[index].conditionParam2 = 0;

                if ((AttackPatternEnum) _enemy.actions[index].conditionType == AttackPatternEnum.TURN)
                {
                    turnParam1.value = 0;
                    turnParam2.value = 0;
                }
                if ((AttackPatternEnum) _enemy.actions[index].conditionType == AttackPatternEnum.HP)
                {
                    hpParam1.value = 0;
                    hpParam2.value = 0;
                }
                if ((AttackPatternEnum) _enemy.actions[index].conditionType == AttackPatternEnum.MP)
                {
                    mpParam1.value = 0;
                    mpParam2.value = 0;
                }


                //レベル選択の際は初期値「1」
                if (patternPopupField.index == patternList.IndexOf(EditorLocalize.LocalizeText("WORD_0603")))
                    _enemy.actions[index].conditionParam1 = 1;

                turnArea.style.display =
                    (AttackPatternEnum) _enemy.actions[index].conditionType == AttackPatternEnum.TURN
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;
                hpArea.style.display = (AttackPatternEnum) _enemy.actions[index].conditionType == AttackPatternEnum.HP
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
                mpArea.style.display = (AttackPatternEnum) _enemy.actions[index].conditionType == AttackPatternEnum.MP
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
                stateArea.style.display =
                    (AttackPatternEnum) _enemy.actions[index].conditionType == AttackPatternEnum.STATE
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;
                partyLvArea.style.display =
                    (AttackPatternEnum) _enemy.actions[index].conditionType == AttackPatternEnum.PARTY_LV
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;
                switchArea.style.display =
                    (AttackPatternEnum) _enemy.actions[index].conditionType == AttackPatternEnum.SWITCH
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;

                Save();
            });

            _actionPatternArea.Add(actionPatternFoldout);

            SetFoldout(actionPatternFoldout);
            SetFoldout(pattern);
            SetFoldout(turnName);
            SetFoldout(hpName);
            SetFoldout(mpName);
            SetFoldout(stateName);
            SetFoldout(partyLvName);
            SetFoldout(switchName);
        }

        override protected void SaveContents() {
            //セーブ部位の作成
            databaseManagementService.SaveEnemy(_enemyDataModels);
        }

        private void _UpdateSceneView() {
            _ = Editor.Hierarchy.Hierarchy.Refresh(Region.Battle, _id);
        }

        private void HandleRightClickDelete(int evt) {
            if (evt != (int) MouseButton.RightMouse) return;

            _OnRightClickEventDelete(_currentSelectId);
        }

        private void _OnRightClickEventDelete(int index) {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0383")), false, () =>
            {
                switch (_currentType)
                {
                    case CurrentType.DropItem:
                        _enemy.dropItems.RemoveAt(index);
                        _dropItemArea.Clear();
                        for (var i = 0; i < _enemy.dropItems.Count; i++) _SetDropItem(i);

                        break;
                    case CurrentType.ActionPattern:
                        _enemy.actions.RemoveAt(index);
                        _actionPatternArea.Clear();
                        for (var i = 0; i < _enemy.actions.Count; i++) _SetActionPattern(i);

                        break;
                    case CurrentType.Traits:
                        _enemy.traits.RemoveAt(index);
                        _enemyTraitsArea.Clear();
                        for (var i = 0; i < _enemy.traits.Count; i++) _SetTraits(i);

                        break;
                }
                Save();
            });
            menu.ShowAsContext();
        }

        private List<string> ElementList() {
            var returnList = new List<string>();

            foreach (var value in _systemSettingDataModel.elements) returnList.Add(value.value);

            return returnList;
        }

        private enum CurrentType
        {
            DropItem = 0,
            ActionPattern,
            Traits
        }
    }
}