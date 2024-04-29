using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Animation;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCommon;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCustom;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using RPGMaker.Codebase.Editor.Hierarchy.Enum;
using RPGMaker.Codebase.Editor.Inspector.Effect.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCustom.SkillCustomDataModel;
using static RPGMaker.Codebase.Editor.Common.AutoguideHelper;

namespace RPGMaker.Codebase.Editor.Inspector.SkillCustom.View
{
    /// <summary>
    /// [スキルの編集]-[基本スキル/カスタムスキル] Inspector
    /// </summary>
    public class SkillCustomInspectorElement : AbstractInspectorElement
    {
        private List<AnimationDataModel> _animationDataModels;

        //攻撃タイプ
        //通常攻撃、魔法、必殺技リスト
        private Dictionary<string, string> _attackType;

        //攻撃タイプの選択PU
        private          PopupFieldBase<string>    _attackTypeSelect;
        private          Foldout                   _foldoutAutoGuideNotUseParams;
        private          Foldout                   _foldoutAutoGuideUseParams;
        private          List<string>              _formulaList = new List<string>(); //ダメージ計算式

        private ImTextField _itemDamageFormula;

        //オートガイドで利用する部品群
        private IntegerField               _itemLevelAutoGuide; //想定習得レベル
        private ImTextField                _itemUserImpactDamageFormula;
        private Label                      _labelAmagAutoGuide; //攻撃側倍率の変更
        private Label                      _labelBmagAutoGuide; //対象側倍率の変更
        private Label                      _labelCalcAutoGuide; //計算式
        private Label                      _labelFixAutoGuide; //固定値
        private Label                      _labelMaxAutoGuide; //最大値
        private Label                      _labelMinAutoGuide; //最小値
        private Label                      _labelMpAutoGuide; //消費MP
        private SkillCommonDataModel       _skillCommonDataModel;
        private List<SkillCommonDataModel> _skillCommonDataModels;
        private SkillCustomDataModel       _skillCustomDataModel;
        private List<SkillCustomDataModel> _skillCustomDataModels;
        private SystemSettingDataModel     _systemSettingDataModel;

        //オートガイド関連
        //オートガイドを利用しない（自由入力）
        private RadioButton _toggleAutoGuideNotUse;

        //オートガイドを利用する
        private RadioButton _toggleAutoGuideUse;

        private IntegerField _use_MP;

        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/SkillCustom/Asset/inspector_skillCustom.uxml"; } }

        //対象者のダメージ部分
        private readonly string targetDamageUxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/SkillCustom/Asset/inspector_skillCustom_targetDamageTemp.uxml";

        //使用者のダメージ部分
        private readonly string userDamageUxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/SkillCustom/Asset/inspector_skillCustom_userDamageTemp.uxml";

        public SkillCustomInspectorElement(SkillCustomDataModel skillCustomDataModel) {
            _skillCustomDataModel = skillCustomDataModel;
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            _skillCustomDataModels = databaseManagementService.LoadSkillCustom();
            _systemSettingDataModel = databaseManagementService.LoadSystem();
            _skillCustomDataModel =
                _skillCustomDataModels.Find(item => item.basic.id == _skillCustomDataModel?.basic.id);
            _skillCommonDataModels = databaseManagementService.LoadSkillCommon();
            _skillCommonDataModel = _skillCommonDataModels[0];

            //ダメージ計算式の
            _formulaList = new List<string>
            {
                "a.atk * " + _skillCommonDataModel.damage.normalAttack.aMag + " - b.def * " +
                _skillCommonDataModel.damage.normalAttack.bMag,
                _skillCommonDataModel.damage.magicAttack.cDmg + " + a.mat * " +
                _skillCommonDataModel.damage.magicAttack.aMag + " - b.mdf * " +
                _skillCommonDataModel.damage.magicAttack.bMag,
                _skillCommonDataModel.damage.magicAttack.cDmg + " + a.atk * " +
                _skillCommonDataModel.damage.specialAttack.aMag + " - b.def * " +
                _skillCommonDataModel.damage.specialAttack.bMag
            };


            if (_skillCustomDataModel == null)
            {
                Clear();
                return;
            }

            LoadData();
            Initialize();
        }

        private void LoadData() {
            //攻撃タイプ設定
            _attackType = EditorLocalize.LocalizeDictionaryValues(new Dictionary<string, string>
            {
                {SkillCommonDataModel.SkillType.None.ToString(), "WORD_0113"},
                {SkillCommonDataModel.SkillType.NormalAttack.ToString(), "WORD_0415"},
                {SkillCommonDataModel.SkillType.MagicAttack.ToString(), "WORD_0462"},
                {SkillCommonDataModel.SkillType.SpecialAttack.ToString(), "WORD_0422"}
            });
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            var itemEnum = new ItemEnums();
            var battleEnum = new BattleEnums();
            var effectsEnum = new EffectsEnums();
            var traitsEnum = new TraitsEnums();
            _animationDataModels = databaseManagementService.LoadAnimation();


            //////////////////////////////////////Basic//////////////////////////////////////////////

            //ID
            Label skill_custom_id = RootContainer.Query<Label>("skill_custom_id");
            skill_custom_id.text = _skillCustomDataModel.SerialNumberString;

            ImTextField skill_custom_name = RootContainer.Query<ImTextField>("skill_custom_name");
            skill_custom_name.value = _skillCustomDataModel.basic.name;
            skill_custom_name.RegisterCallback<FocusOutEvent>(o =>
            {
                _skillCustomDataModel.basic.name = skill_custom_name.value;
                databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
                SkillCustomUpdate(true);
            });
            
            // プレビュー画像
            Image iconImage = RootContainer.Query<Image>("icon_image");
            var tex2d = AssetDatabase.LoadAssetAtPath<Texture2D>(PathManager.IMAGE_ICON + _skillCustomDataModel.basic.iconId + ".png");
            BackgroundImageHelper.SetBackground(iconImage, new Vector2(66, 66), tex2d, LengthUnit.Pixel);

            
            // 画像名
            Label iconImageName = RootContainer.Query<Label>("icon_image_name");
            iconImageName.text = _skillCustomDataModel.basic.iconId;

            // 画像変更ボタン
            Button iconChangeBtn = RootContainer.Query<Button>("icon_image_change_btn");
            iconChangeBtn.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.IMAGE_ICON, true);
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("WORD_1611"), data =>
                {
                    var imageName = (string) data;
                    _skillCustomDataModel.basic.iconId = imageName;
                    iconImageName.text = imageName;
                    tex2d = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        PathManager.IMAGE_ICON + _skillCustomDataModel.basic.iconId + ".png");
                    BackgroundImageHelper.SetBackground(iconImage, new Vector2(66, 66), tex2d, LengthUnit.Pixel);

                    databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
                }, _skillCustomDataModel.basic.iconId);
            };

            // 画像インポートボタン
            Button iconImportBtn = RootContainer.Query<Button>("icon_image_import_btn");
            iconImportBtn.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.IMAGE_ICON);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    _skillCustomDataModel.basic.iconId = path;
                    iconImageName.text = path;
                    tex2d = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        PathManager.IMAGE_ICON + _skillCustomDataModel.basic.iconId + ".png");
                    BackgroundImageHelper.SetBackground(iconImage, new Vector2(66, 66), tex2d, LengthUnit.Pixel);

                    databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
                }
            };
            
            
            ImTextField skill_description = RootContainer.Query<ImTextField>("skill_description");
            skill_description.value = _skillCustomDataModel.basic.description;
            skill_description.RegisterCallback<FocusOutEvent>(o =>
            {
                _skillCustomDataModel.basic.description = skill_description.value;
                databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
            });

            VisualElement skill_type = RootContainer.Query<VisualElement>("skill_type");
            var skillTypes = _systemSettingDataModel.skillTypes;
            var skills = new List<string>();
            foreach (var s in skillTypes) skills.Add(s.value);

            if (skills.Count > 0)
            {
                var skill_typePopupField = new PopupFieldBase<string>(skills,
                    _skillCustomDataModel.basic.skillType);
                skill_type.Add(skill_typePopupField);
                skill_typePopupField.RegisterValueChangedCallback(evt =>
                {
                    _skillCustomDataModel.basic.skillType =
                        skills.IndexOf(skill_typePopupField.value);
                    SetFormula();
                    databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
                });
            }

            _use_MP = RootContainer.Query<IntegerField>("use_MP");
            _use_MP.value = _skillCustomDataModel.basic.costMp;
            BaseInputFieldHandler.IntegerFieldCallback(_use_MP, evt =>
            {
                _skillCustomDataModel.basic.costMp = _use_MP.value;
                databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
            }, 0, 0);

            IntegerField use_TP = RootContainer.Query<IntegerField>("use_TP");
            use_TP.value = _skillCustomDataModel.basic.costTp;
            BaseInputFieldHandler.IntegerFieldCallback(use_TP, evt =>
            {
                _skillCustomDataModel.basic.costTp = use_TP.value;
                databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
            }, 0, 0);

            VisualElement usage_time = RootContainer.Query<VisualElement>("usage_time");
            var usage_timePopupField = new PopupFieldBase<string>(
                EditorLocalize.LocalizeTexts(itemEnum.itemCanUseTimingLabel),
                _skillCustomDataModel.basic.canUseTiming);
            usage_time.Add(usage_timePopupField);
            usage_timePopupField.RegisterValueChangedCallback(evt =>
            {
                _skillCustomDataModel.basic.canUseTiming =
                    EditorLocalize.LocalizeTexts(itemEnum.itemCanUseTimingLabel).IndexOf(usage_timePopupField.value);
                databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
            });

            ImTextField skill_message = RootContainer.Query<ImTextField>("skill_message");
            skill_message.value = _skillCustomDataModel.basic.message.Trim();
            skill_message.RegisterCallback<FocusOutEvent>(o =>
            {
                _skillCustomDataModel.basic.message = skill_message.value.Trim();
                databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
            });

            VisualElement weapon_type1 = RootContainer.Query<VisualElement>("weapon_type1");
            var weaponList = new List<string>();
            foreach (var name in _systemSettingDataModel.weaponTypes) weaponList.Add(name.value);

            var weapon_type1PopupField = new PopupFieldBase<string>(weaponList,
                _skillCustomDataModel.basic.requiredWTypeId1);
            weapon_type1.Add(weapon_type1PopupField);
            weapon_type1PopupField.RegisterValueChangedCallback(evt =>
            {
                _skillCustomDataModel.basic.requiredWTypeId1 =
                    weapon_type1PopupField.index;
                databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
            });

            VisualElement weapon_type2 = RootContainer.Query<VisualElement>("weapon_type2");
            var weapon_type2PopupField = new PopupFieldBase<string>(weaponList,
                _skillCustomDataModel.basic.requiredWTypeId2);
            weapon_type2.Add(weapon_type2PopupField);
            weapon_type2PopupField.RegisterValueChangedCallback(evt =>
            {
                _skillCustomDataModel.basic.requiredWTypeId2 =
                    weapon_type2PopupField.index;
                databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
            });

            //////////////////////////////////////Basic//////////////////////////////////////////////

            //対象者への効果
            var item_target_toggleList = new List<RadioButton>();
            for (var i = 1; i <= 4; i++)
            {
                RadioButton toggle = RootContainer.Query<RadioButton>("radioButton-skillEdit-display" + i);
                item_target_toggleList.Add(toggle);
            }

            //トグル切り替え時の非表示設定領域
            var item_target_toggleContents = new List<VisualElement>();
            item_target_toggleContents.Add(new VisualElement());
            item_target_toggleContents.Add(RootContainer.Query<VisualElement>("item_target_foldout1"));
            item_target_toggleContents.Add(RootContainer.Query<VisualElement>("item_target_foldout2"));
            item_target_toggleContents.Add(RootContainer.Query<VisualElement>("item_target_foldout3"));

            //初期値
            var itemTargetFoldoutList = new List<Foldout>();
            for (var i = 0; i < 3; i++)
            {
                Foldout foldout = RootContainer.Query<Foldout>("item_target_foldout" + (i + 1));
                itemTargetFoldoutList.Add(foldout);
            }

            var targetTeamActions = new List<Action>
            {
                //なし
                () =>
                {
                    _skillCustomDataModel.targetEffect.targetTeam = 0;
                    _skillCustomDataModel.targetEffect.targetRange = 0;
                    databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
                },
                //敵
                () =>
                {
                    _skillCustomDataModel.targetEffect.targetTeam = 1;
                    //敵の範囲の部分
                    var itemTargetEnemySpecificationToggleList = new List<RadioButton>();
                    for (var i = 5; i <= 7; i++)
                    {
                        RadioButton toggle = RootContainer.Query<RadioButton>("radioButton-skillEdit-display" + i);
                        if (toggle.value)
                        {
                            _skillCustomDataModel.targetEffect.targetRange = i - 5;
                            break;
                        }
                    }
                    databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
                },
                //味方
                () =>
                {
                    _skillCustomDataModel.targetEffect.targetTeam = 2;
                    //味方の範囲の部分
                    var itemTargetAlliesSpecificationToggleList = new List<RadioButton>();
                    for (var i = 8; i <= 9; i++)
                    {
                        RadioButton toggle = RootContainer.Query<RadioButton>("radioButton-skillEdit-display" + i);
                        if (toggle.value)
                        {
                            _skillCustomDataModel.targetEffect.targetRange = i - 8;
                            break;
                        }
                    }
                    databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
                },
                //敵と味方
                () =>
                {
                    _skillCustomDataModel.targetEffect.targetTeam = 3;
                    _skillCustomDataModel.targetEffect.targetRange = 0;
                    databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
                }
            };

            new CommonToggleSelector().SetRadioInVisualElementSelector(item_target_toggleList, item_target_toggleContents,
                _skillCustomDataModel.targetEffect.targetTeam, targetTeamActions);

            //使用者トグルのみ別枠で保存される
            Toggle targetUser = RootContainer.Query<Toggle>("item_target_toggle5");
            //ランダムの後ろの数入力
            IntegerField itemRandomNum = RootContainer.Query<IntegerField>("item_randomNum");
            if (_skillCustomDataModel.targetEffect.targetTeam != 1 || _skillCustomDataModel.targetEffect.targetRange != 2)
            {
                itemRandomNum.SetEnabled(false);
            }
            else
            {
                itemRandomNum.SetEnabled(true);
            }
            targetUser.value = _skillCustomDataModel.targetEffect.targetUser == 1;
            targetUser.RegisterValueChangedCallback(o =>
            {
                _skillCustomDataModel.targetEffect.targetUser = targetUser.value ? 1 : 0;
                SkillCustomUpdate();
            });

            //敵の範囲の部分
            var itemTargetEnemySpecificationToggleList = new List<RadioButton>();
            for (var i = 5; i <= 7; i++)
            {
                RadioButton toggle = RootContainer.Query<RadioButton>("radioButton-skillEdit-display" + i);
                itemTargetEnemySpecificationToggleList.Add(toggle);
            }

            var itemTargetEnemySpecificationActions = new List<Action>
            {
                () =>
                {
                    if (_skillCustomDataModel.targetEffect.targetTeam != 1) return;
                    _skillCustomDataModel.targetEffect.targetRange = 0;
                    itemRandomNum.SetEnabled(false);
                    databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
                },
                () =>
                {
                    if (_skillCustomDataModel.targetEffect.targetTeam != 1) return;
                    _skillCustomDataModel.targetEffect.targetRange = 1;
                    itemRandomNum.SetEnabled(false);
                    databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
                },
                () =>
                {
                    if (_skillCustomDataModel.targetEffect.targetTeam != 1) return;
                    _skillCustomDataModel.targetEffect.targetRange = 2;
                    itemRandomNum.SetEnabled(true);
                    databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
                }
            };

            new CommonToggleSelector().SetRadioSelector(itemTargetEnemySpecificationToggleList,
                _skillCustomDataModel.targetEffect.targetTeam == 1 ? _skillCustomDataModel.targetEffect.targetRange : 0,
                itemTargetEnemySpecificationActions);

            itemRandomNum.value = _skillCustomDataModel.targetEffect.randomNumber;
            BaseInputFieldHandler.IntegerFieldCallback(itemRandomNum,
                evt => { _skillCustomDataModel.targetEffect.randomNumber = itemRandomNum.value; }, 1, 1);


            //味方の範囲の部分
            var itemTargetAlliesSpecificationToggleList = new List<RadioButton>();
            for (var i = 8; i <= 9; i++)
            {
                RadioButton toggle = RootContainer.Query<RadioButton>("radioButton-skillEdit-display" + i);
                itemTargetAlliesSpecificationToggleList.Add(toggle);
            }
            
            var itemTargetAlliesSpecificationActions = new List<Action>
            {
                () =>
                {
                    if (_skillCustomDataModel.targetEffect.targetTeam != 2) return;
                    _skillCustomDataModel.targetEffect.targetRange = 0;
                    databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
                },
                () =>
                {
                    if (_skillCustomDataModel.targetEffect.targetTeam != 2) return;
                    _skillCustomDataModel.targetEffect.targetRange = 1;
                    databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
                }
            };
            new CommonToggleSelector().SetRadioSelector(itemTargetAlliesSpecificationToggleList,
                _skillCustomDataModel.targetEffect.targetTeam == 2 ? _skillCustomDataModel.targetEffect.targetRange : 0,
                itemTargetAlliesSpecificationActions);


            //状態異常
            var itemTargetStateToggles = new List<RadioButton>();
            for (var i = 10; i <= 12; i++)
            {
                RadioButton toggle = RootContainer.Query<RadioButton>("radioButton-skillEdit-display" + i);
                itemTargetStateToggles.Add(toggle);
            }

            var itemTargetStateActions = new List<Action>
            {
                () =>
                {
                    if (_skillCustomDataModel.targetEffect.targetTeam != 2) return;
                    _skillCustomDataModel.targetEffect.targetStatus = 0;
                    databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
                },
                () =>
                {
                    if (_skillCustomDataModel.targetEffect.targetTeam != 2) return;
                    _skillCustomDataModel.targetEffect.targetStatus = 1;
                    databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
                },
                () =>
                {
                    if (_skillCustomDataModel.targetEffect.targetTeam != 2) return;
                    _skillCustomDataModel.targetEffect.targetStatus = 2;
                    databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
                }
            };

            new CommonToggleSelector().SetRadioSelector(itemTargetStateToggles,
                _skillCustomDataModel.targetEffect.targetTeam == 2
                    ? _skillCustomDataModel.targetEffect.targetStatus
                    : 0,
                itemTargetStateActions);

            //速度補正
            IntegerField item_speed_correction = RootContainer.Query<IntegerField>("item_speed_correction");
            item_speed_correction.value =
                _skillCustomDataModel.targetEffect.activate.correctionSpeed;
            BaseInputFieldHandler.IntegerFieldCallback(item_speed_correction, evt =>
            {
                _skillCustomDataModel.targetEffect.activate.correctionSpeed =
                    item_speed_correction.value;
                SkillCustomUpdate();
            }, -2000, 2000);

            //成功率
            IntegerField item_success_rate = RootContainer.Query<IntegerField>("item_success_rate");
            item_success_rate.value = _skillCustomDataModel.targetEffect.activate.successRate;
            BaseInputFieldHandler.IntegerFieldCallback(item_success_rate, evt =>
            {
                _skillCustomDataModel.targetEffect.activate.successRate =
                    item_success_rate.value;
                SkillCustomUpdate();
            }, 1, 100);

            //連続回数
            IntegerField item_consecutivenum = RootContainer.Query<IntegerField>("item_consecutive_num");
            item_consecutivenum.value =
                _skillCustomDataModel.targetEffect.activate.continuousNumber;
            BaseInputFieldHandler.IntegerFieldCallback(item_consecutivenum, evt =>
            {
                _skillCustomDataModel.targetEffect.activate.continuousNumber =
                    item_consecutivenum.value;
                SkillCustomUpdate();
            }, 1, 1);

            //命中タイプ
            var item_hit_typeList = new List<Toggle>();
            for (var i = 0; i < 3; i++)
            {
                Toggle toggle = RootContainer.Query<Toggle>("item_hit_type" + (i + 1));
                item_hit_typeList.Add(toggle);
            }
            
            if (_skillCustomDataModel.targetEffect.activate.hitType > 2)
            {
                _skillCustomDataModel.targetEffect.activate.hitType = 2;
            }
            if (_skillCustomDataModel.targetEffect.activate.hitType > -1)
            {
                item_hit_typeList[_skillCustomDataModel.targetEffect.activate.hitType].value = true;
            }

            for (var i = 0; i < 3; i++)
            {
                var num2 = i;
                item_hit_typeList[num2].RegisterValueChangedCallback(o =>
                {
                    if (item_hit_typeList[num2].value)
                    {
                        _skillCustomDataModel.targetEffect.activate.hitType = num2;
                        item_hit_typeList[num2].value = true;

                        if (num2 != 0)
                        {
                            item_hit_typeList[0].value = false;
                            item_hit_typeList[0].SetEnabled(true);
                        }

                        if (num2 != 1)
                        {
                            item_hit_typeList[1].value = false;
                            item_hit_typeList[1].SetEnabled(true);
                        }

                        if (num2 != 2)
                        {
                            item_hit_typeList[2].value = false;
                            item_hit_typeList[2].SetEnabled(true);
                        }
                        SkillCustomUpdate();
                    }
                    else
                    {
                        //すべてチェックついていなかったら、「なし」扱い
                        if (!item_hit_typeList[0].value && !item_hit_typeList[1].value && !item_hit_typeList[2].value)
                        {
                            _skillCustomDataModel.targetEffect.activate.hitType = -1;
                            SkillCustomUpdate();
                        }
                    }
                });
            }

            //対象者
            DamageInspectorCreate(true);
            //使用者
            DamageInspectorCreate(false);

            VisualElement itemTraits = RootContainer.Query<VisualElement>("item_traits");
            var effectWindow = new EffectInspectorElement();
            itemTraits.Add(effectWindow);
            effectWindow.Init(_skillCustomDataModel.targetEffect.otherEffects, data =>
            {
                var work = (List<TraitCommonDataModel>) data;
                _skillCustomDataModel.targetEffect.otherEffects = work;
                SkillCustomUpdate();
            }, EffectInspectorElement.DisplayType.skill);

            VisualElement itemUserTraits = RootContainer.Query<VisualElement>("item_user_impact_traits");
            var userEffectWindow = new EffectInspectorElement();
            itemUserTraits.Add(userEffectWindow);
            userEffectWindow.Init(_skillCustomDataModel.userEffect.otherEffects, data =>
            {
                var work = (List<TraitCommonDataModel>) data;
                _skillCustomDataModel.userEffect.otherEffects = work;
                SkillCustomUpdate();
            }, EffectInspectorElement.DisplayType.skill);

            //TP獲得値
            IntegerField item_tp_earned_value = RootContainer.Query<IntegerField>("item_tp_earned_value");
            item_tp_earned_value.value = _skillCustomDataModel.userEffect.getTp;
            BaseInputFieldHandler.IntegerFieldCallback(item_tp_earned_value, evt =>
            {
                _skillCustomDataModel.userEffect.getTp = item_tp_earned_value.value;
                SkillCustomUpdate();
            }, 0, 0);

            //アニメーションのフォールドダウン
            VisualElement animatiouFolddown = RootContainer.Query<VisualElement>("animatiou_folddown");
            var animatiouFolddownPopupField = new PopupFieldBase<string>(ParticleList(),
                ParticleIdToIndex(_skillCustomDataModel.targetEffect.activate.animationId));
            animatiouFolddown.Add(animatiouFolddownPopupField);
            animatiouFolddownPopupField.RegisterValueChangedCallback(evt =>
            {
                _skillCustomDataModel.targetEffect.activate.animationId =
                    _animationDataModels[animatiouFolddownPopupField.index].id;
                SkillCustomUpdate();
            });

            //回復フォールドアウト以下の要素(対象者)
            HealInspectorSetting(true);


            //回復フォールドアウト以下の要素(使用者)
            HealInspectorSetting(false);

            //メモ部分
            ImTextField item_memo = RootContainer.Query<ImTextField>("item_memo");
            item_memo.value = _skillCustomDataModel.memo;
            item_memo.RegisterCallback<FocusOutEvent>(o =>
            {
                _skillCustomDataModel.memo = item_memo.value;
                SkillCustomUpdate();
            });
        }

        //回復
        private void HealInspectorSetting(bool isTarget) {
            if (isTarget)
            {
                Toggle itemHpToggle = RootContainer.Query<Toggle>("item_hp_toggle");
                VisualElement itemHpFoldout = RootContainer.Query<VisualElement>("item_hp_foldout");
                Toggle itemMpToggle = RootContainer.Query<Toggle>("item_mp_toggle");
                VisualElement itemMpFoldout = RootContainer.Query<VisualElement>("item_mp_foldout");
                Toggle itemTpToggle = RootContainer.Query<Toggle>("item_tp_toggle");
                VisualElement itemTpFoldout = RootContainer.Query<VisualElement>("item_tp_foldout");

                new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                    itemHpToggle,
                    itemHpFoldout,
                    _skillCustomDataModel.targetEffect.heal.hp.enabled == 1,
                    () =>
                    {
                        _skillCustomDataModel.targetEffect.heal.hp.enabled = itemHpToggle.value ? 1 : 0;
                        SkillCustomUpdate();
                    }
                );
                new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                    itemMpToggle,
                    itemMpFoldout,
                    _skillCustomDataModel.targetEffect.heal.mp.enabled == 1,
                    () =>
                    {
                        _skillCustomDataModel.targetEffect.heal.mp.enabled = itemMpToggle.value ? 1 : 0;
                        SkillCustomUpdate();
                    }
                );
                new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                    itemTpToggle,
                    itemTpFoldout,
                    _skillCustomDataModel.targetEffect.heal.tp.enabled == 1,
                    () =>
                    {
                        _skillCustomDataModel.targetEffect.heal.tp.enabled = itemTpToggle.value ? 1 : 0;
                        SkillCustomUpdate();
                    }
                );
            }
            else
            {
                Toggle itemUserHpToggle = RootContainer.Query<Toggle>("item_user_hp_toggle");
                VisualElement itemUserHpFoldout = RootContainer.Query<VisualElement>("item_user_hp_foldout");
                Toggle itemUserMpToggle = RootContainer.Query<Toggle>("item_user_mp_toggle");
                VisualElement itemUserMpFoldout = RootContainer.Query<VisualElement>("item_user_mp_foldout");
                Toggle itemUserTpToggle = RootContainer.Query<Toggle>("item_user_tp_toggle");
                VisualElement itemUserTpFoldout = RootContainer.Query<VisualElement>("item_user_tp_foldout");

                new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                    itemUserHpToggle,
                    itemUserHpFoldout,
                    _skillCustomDataModel.userEffect.heal.hp.enabled == 1,
                    () =>
                    {
                        _skillCustomDataModel.userEffect.heal.hp.enabled = itemUserHpToggle.value ? 1 : 0;
                        SkillCustomUpdate();
                    }
                );
                new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                    itemUserMpToggle,
                    itemUserMpFoldout,
                    _skillCustomDataModel.userEffect.heal.mp.enabled == 1,
                    () =>
                    {
                        _skillCustomDataModel.userEffect.heal.mp.enabled = itemUserMpToggle.value ? 1 : 0;
                        SkillCustomUpdate();
                    }
                );
                new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                    itemUserTpToggle,
                    itemUserTpFoldout,
                    _skillCustomDataModel.userEffect.heal.tp.enabled == 1,
                    () =>
                    {
                        _skillCustomDataModel.userEffect.heal.tp.enabled = itemUserTpToggle.value ? 1 : 0;
                        SkillCustomUpdate();
                    }
                );
            }

            //0==HP 1==MP 2==TP
            for (var i = 0; i < 3; i++)
            {
                var tag = "";
                var tag2 = "";
                var num2 = i;
                var heal = ItemEffectHealParam.CreateDefault();

                //使用者だった場合「tag2」位置に「user_impact_」が入る
                if (!isTarget)
                    tag2 = "user_impact_";

                switch (num2)
                {
                    case 0:
                        tag = "HP";
                        if (isTarget)
                            heal = _skillCustomDataModel.targetEffect.heal.hp;
                        else
                            heal = _skillCustomDataModel.userEffect.heal.hp;
                        break;
                    case 1:
                        tag = "MP";
                        if (isTarget)
                            heal = _skillCustomDataModel.targetEffect.heal.mp;
                        else
                            heal = _skillCustomDataModel.userEffect.heal.mp;
                        break;
                    case 2:
                        tag = "TP";
                        if (isTarget)
                            heal = _skillCustomDataModel.targetEffect.heal.tp;
                        else
                            heal = _skillCustomDataModel.userEffect.heal.tp;
                        break;
                }

                //計算式
                RadioButton itemFormulaToggle = RootContainer.Query<RadioButton>("radioButton-skillEdit-display-" + tag2  + tag + "1");
                ImTextField itemInputField = RootContainer.Query<ImTextField>("item_" + tag2 + "input_field_" + tag);
                //割合
                RadioButton itemPercentageMaxToggle = RootContainer.Query<RadioButton>("radioButton-skillEdit-display-" + tag2 + tag + "2");
                IntegerField itemRate = RootContainer.Query<IntegerField>("item_" + tag2 + "rate_" + tag);
                //固定値
                RadioButton itemFixedValueToggle = RootContainer.Query<RadioButton>("radioButton-skillEdit-display-" + tag2  + tag + "3");
                IntegerField itemFixedValue = RootContainer.Query<IntegerField>("item_" + tag2 + "" + tag + "_fixed_value");
                //分散度
                Toggle itemDispersityToggle = RootContainer.Query<Toggle>("item_" + tag2 + "" + tag + "_dispersity_toggle");
                IntegerField itemDispersity = RootContainer.Query<IntegerField>("item_" + tag2 + "" + tag + "_dispersity");
                //最大値
                Toggle itemMaxValueToggle = RootContainer.Query<Toggle>("item_" + tag2 + "" + tag + "_max_value_toggle");
                IntegerField itemMaxValue = RootContainer.Query<IntegerField>("item_" + tag2 + "" + tag + "_max_value");

                //トグル
                var defaultSelect = heal.calc.enabled == 1 ? 0 :
                    heal.fix.enabled == 1 ? 1 :
                    heal.perMax.enabled == 1 ? 2 : 0;

                var actions = new List<Action>
                {
                    () =>
                    {
                        heal.calc.enabled = Convert.ToInt32(itemFormulaToggle.value);
                        heal.fix.enabled = 0;
                        heal.perMax.enabled = 0;
                        SetHealData(isTarget, num2, heal);
                        SkillCustomUpdate();
                    },
                    () =>
                    {
                        heal.calc.enabled = 0;
                        heal.fix.enabled = Convert.ToInt32(itemFixedValueToggle.value);
                        heal.perMax.enabled = 0;
                        SetHealData(isTarget, num2, heal);
                        SkillCustomUpdate();
                    },
                    () =>
                    {
                        heal.calc.enabled = 0;
                        heal.fix.enabled = 0;
                        heal.perMax.enabled = Convert.ToInt32(itemPercentageMaxToggle.value);
                        SetHealData(isTarget, num2, heal);
                        SkillCustomUpdate();
                    }
                };

                new CommonToggleSelector().SetRadioInVisualElementSelector(
                    new List<RadioButton> { itemFormulaToggle, itemFixedValueToggle, itemPercentageMaxToggle },
                    new List<VisualElement> {
                        RootContainer.Query<VisualElement>("item_" + tag2 + "input_field_" + tag + "_contents"),
                        RootContainer.Query<VisualElement>("item_" + tag2 + "" + tag + "_fixed_value_contents"),
                        RootContainer.Query<VisualElement>("item_" + tag2 + "rate_" + tag + "_contents")
                    },
                    defaultSelect, actions);

                //分散度
                new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                    itemDispersityToggle,
                    RootContainer.Query<VisualElement>("item_" + tag2 + "" + tag + "_dispersity_contents"),
                    Convert.ToBoolean(heal.perMax.distributeEnabled),
                    () =>
                    {
                        heal.perMax.distributeEnabled = Convert.ToInt32(itemDispersityToggle.value);
                        SetHealData(isTarget, num2, heal);
                        SkillCustomUpdate();
                    }
                );

                //最大値
                new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                    itemMaxValueToggle,
                    RootContainer.Query<VisualElement>("item_" + tag2 + "" + tag + "_max_value_contents"),
                    Convert.ToBoolean(heal.perMax.maxEnabled),
                    () =>
                    {
                        heal.perMax.maxEnabled = Convert.ToInt32(itemMaxValueToggle.value);
                        SetHealData(isTarget, num2, heal);
                        SkillCustomUpdate();
                    }
                );

                //数値入力
                //割合
                itemRate.value = heal.perMax.value;
                BaseInputFieldHandler.IntegerFieldCallback(itemRate, evt =>
                {
                    heal.perMax.value = itemRate.value;
                    SetHealData(isTarget, num2, heal);
                    SkillCustomUpdate();
                }, 0, 100);

                //分散度
                itemDispersity.value = heal.perMax.distribute;
                BaseInputFieldHandler.IntegerFieldCallback(itemDispersity, evt =>
                {
                    heal.perMax.distribute = itemDispersity.value;
                    SetHealData(isTarget, num2, heal);
                    SkillCustomUpdate();
                }, 0, 100);

                //最大値
                itemMaxValue.value = heal.perMax.max;
                BaseInputFieldHandler.IntegerFieldCallback(itemMaxValue, evt =>
                {
                    heal.perMax.max = itemMaxValue.value;
                    SetHealData(isTarget, num2, heal);
                    SkillCustomUpdate();
                }, 0, 0);

                //固定値
                itemFixedValue.value = heal.fix.value;
                BaseInputFieldHandler.IntegerFieldCallback(itemFixedValue, evt =>
                {
                    heal.fix.value = itemFixedValue.value;
                    SetHealData(isTarget, num2, heal);
                    SkillCustomUpdate();
                }, 0, 0);

                //計算式
                itemInputField.value = heal.calc.value;
                itemInputField.RegisterCallback<FocusOutEvent>(o =>
                {
                    heal.calc.value = itemInputField.value;
                    SetHealData(isTarget, num2, heal);
                    SkillCustomUpdate();
                });
            }
        }

        private void DamageInspectorCreate(bool isTargetEffect) {
            //対象者、使用者どちらでも利用するデータ定義
            var itemEnum = new ItemEnums();
            var damage = Damage.CreateDefault();

            //対象のダメージデータ
            if (!isTargetEffect)
            {
                if (_skillCustomDataModel.userEffect.damage != null)
                    damage = _skillCustomDataModel.userEffect.damage;
                else
                    _skillCustomDataModel.userEffect.damage = damage;
            }
            else
            {
                if (_skillCustomDataModel.targetEffect.damage != null)
                    damage = _skillCustomDataModel.targetEffect.damage;
                else
                    _skillCustomDataModel.targetEffect.damage = damage;
            }

            //対象者の場合
            if (isTargetEffect)
            {
                var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(targetDamageUxml);
                VisualElement labelFromUXML = visualTree.CloneTree();
                EditorLocalize.LocalizeElements(labelFromUXML);
                RootContainer.Q<VisualElement>("item_damage_list").Add(labelFromUXML);

                //入力枠に関する制御
                //オートガイドの使用
                //自由入力
                _toggleAutoGuideNotUse = labelFromUXML.Query<RadioButton>("radioButton-skillEdit-display13");
                //使用する
                _toggleAutoGuideUse = labelFromUXML.Query<RadioButton>("radioButton-skillEdit-display14");

                //自由入力時の入力枠
                //Foldout
                _foldoutAutoGuideNotUseParams = labelFromUXML.Query<Foldout>("autoguide_not_use_params");

                //使用する時の入力枠
                //Foldout
                _foldoutAutoGuideUseParams = labelFromUXML.Query<Foldout>("autoguide_use_params");

                //トグル切り替え時の挙動
                var defaultSelect = damage.autoguideEnabled;
                
                var autoGuideActions = new List<Action>
                {
                    () =>
                    {
                        if (_toggleAutoGuideNotUse.value)
                        {
                            _toggleAutoGuideUse.value = false;
                            damage.autoguideEnabled = 0;
                        }
                        SkillCustomUpdate();
                    },
                    () =>
                    {
                        if (_toggleAutoGuideUse.value)
                        {
                            _toggleAutoGuideNotUse.value = false;
                            damage.autoguideEnabled = 1;
                        }
                        SkillCustomUpdate();
                    }
                };

                new CommonToggleSelector().SetRadioInVisualElementSelector(
                    new List<RadioButton> {_toggleAutoGuideNotUse, _toggleAutoGuideUse}, 
                    new List<VisualElement> { _foldoutAutoGuideNotUseParams, _foldoutAutoGuideUseParams},
                    defaultSelect, autoGuideActions);
                

                //初期値
                if (damage.autoguideEnabled == 0)
                {
                    //オートガイドを利用しない
                    _toggleAutoGuideUse.value = false;
                    _toggleAutoGuideNotUse.value = true;
                }
                else
                {
                    //オートガイドを利用する
                    _toggleAutoGuideUse.value = true;
                    _toggleAutoGuideNotUse.value = false;
                }

                //以下、各部品に対する設定項目
                //タイプ
                VisualElement itemUserImpactDamageArea =
                    labelFromUXML.Query<VisualElement>("item_impact_damage_area");
                itemUserImpactDamageArea.SetEnabled(damage.damageType != 0);
                VisualElement item_damage_type = labelFromUXML.Query<VisualElement>("item_damage_type");
                var item_damage_typePopupField =
                    new PopupFieldBase<string>(EditorLocalize.LocalizeTexts(itemEnum.damageTypeLabel),
                        damage.damageType);
                item_damage_type.Add(item_damage_typePopupField);
                item_damage_typePopupField.RegisterValueChangedCallback(evt =>
                {
                    damage.damageType = item_damage_typePopupField.index;
                    itemUserImpactDamageArea.SetEnabled(damage.damageType != 0);
                    SetDamageData(isTargetEffect, 0, damage);
                    SkillCustomUpdate();
                });

                //攻撃タイプ
                VisualElement item_attack_type = labelFromUXML.Query<VisualElement>("item_attack_type");
                var item_attack_type_list = new List<string>();

                var container = (VisualElement) labelFromUXML.Query<VisualElement>("item_attack_type");
                //配列では0開始になるため
                var attackTypeWork = damage.attackType + 1;
                
                _attackTypeSelect = new PopupFieldBase<string>(_attackType.Values.ToList(), attackTypeWork);
                container.Add(_attackTypeSelect);

                //CB設定
                _attackTypeSelect.RegisterValueChangedCallback(evt =>
                {
                    //計算式の初期化
                    if (_attackTypeSelect.index == 0)
                        _itemDamageFormula.value = "";
                    else
                        _itemDamageFormula.value = _formulaList[_attackTypeSelect.index - 1];

                    damage.value = _itemDamageFormula.value;
                    SetDamageData(isTargetEffect, 0, damage);

                    //選択されたキーを取得
                    var selectedKey = _attackType.FirstOrDefault(kv => kv.Value == _attackTypeSelect.value).Key;
                    //キーをJSONに保存
                    //選択されたものが魔法攻撃の場合、[対象者への効果]-[ダメージ]内の[オートガイド]を有効にする
                    if (selectedKey == SkillCommonDataModel.SkillType.MagicAttack.ToString())
                    {
                        //オートガイドを有効にする
                        _toggleAutoGuideUse.SetEnabled(true);
                        _toggleAutoGuideNotUse.SetEnabled(true);
                    }
                    else
                    {
                        //オートガイドを無効にする
                        _toggleAutoGuideUse.SetEnabled(false);
                        _toggleAutoGuideNotUse.SetEnabled(false);
                        //現在オートガイドにチェックが入っていた場合でも、一律で自由入力に切り替え
                        _toggleAutoGuideNotUse.value = true;
                        _toggleAutoGuideUse.value = false;
                    }

                    if (selectedKey == SkillCommonDataModel.SkillType.NormalAttack.ToString()) damage.attackType = 0;
                    else if (selectedKey == SkillCommonDataModel.SkillType.MagicAttack.ToString())
                        damage.attackType = 1;
                    else if (selectedKey == SkillCommonDataModel.SkillType.SpecialAttack.ToString())
                        damage.attackType = 2;
                    else if (selectedKey == SkillCommonDataModel.SkillType.None.ToString())
                        damage.attackType = -1;
                    SkillCustomUpdate();
                });
                if (damage.attackType == 1)
                {
                    //オートガイドを有効にする
                    _toggleAutoGuideUse.SetEnabled(true);
                    _toggleAutoGuideNotUse.SetEnabled(true);
                }
                else
                {
                    //オートガイドを無効にする
                    _toggleAutoGuideUse.SetEnabled(false);
                    _toggleAutoGuideNotUse.SetEnabled(false);
                    //現在オートガイドにチェックが入っていた場合でも、一律で自由入力に切り替え
                    _toggleAutoGuideNotUse.value = true;
                    _toggleAutoGuideUse.value = false;
                }

                //初期化
                _attackTypeSelect.index = _skillCustomDataModel.targetEffect.damage.attackType;

                //属性
                VisualElement item_attribute_type = labelFromUXML.Query<VisualElement>("item_attribute_type");
                var item_attribute_typeTextDropdownChoices = new List<string>();
                var attribute = _systemSettingDataModel.elements;
                foreach (var a in attribute) item_attribute_typeTextDropdownChoices.Add(a.value);

                if (damage.elements.Count == 0) damage.elements = new List<int>();

                var damageElementIndex = damage.elements.Count > 0 ? damage.elements[0] : 0;
                var item_attribute_typePopupField =
                    new PopupFieldBase<string>(item_attribute_typeTextDropdownChoices, damageElementIndex);
                item_attribute_type.Add(item_attribute_typePopupField);
                item_attribute_typePopupField.RegisterValueChangedCallback(evt =>
                {
                    damage.elements = new List<int>();
                    damage.elements.Add(item_attribute_typePopupField.index);
                    SetDamageData(isTargetEffect, 0, damage);
                    SkillCustomUpdate();
                });

                //自由入力時の入力枠
                //計算式
                _itemDamageFormula = labelFromUXML.Query<ImTextField>("item_damage_formula");
                _itemDamageFormula.value = damage.value;
                _itemDamageFormula.RegisterCallback<FocusOutEvent>(evt =>
                {
                    damage.value = _itemDamageFormula.value;
                    SetDamageData(isTargetEffect, 0, damage);
                    SkillCustomUpdate();
                });

                //最小値
                IntegerField item_damage_min_value = labelFromUXML.Query<IntegerField>("item_damage_min_value");
                item_damage_min_value.value = damage.min;
                BaseInputFieldHandler.IntegerFieldCallback(item_damage_min_value, evt =>
                {
                    damage.min = item_damage_min_value.value;
                    SetDamageData(isTargetEffect, 0, damage);
                    SkillCustomUpdate();
                }, 0, 0);

                Toggle toggle_damage_min_value = labelFromUXML.Query<Toggle>("toggle_damage_min_value");
                new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                    toggle_damage_min_value,
                    labelFromUXML.Query<VisualElement>("item_damage_min_value_contents"),
                    damage.minEnabled == 1,
                    () =>
                    {
                        damage.minEnabled = toggle_damage_min_value.value ? 1 : 0;
                        SkillCustomUpdate();
                    }
                );

                //最大値
                IntegerField item_damage_max_value = labelFromUXML.Query<IntegerField>("item_damage_max_value");
                item_damage_max_value.value = damage.max;
                BaseInputFieldHandler.IntegerFieldCallback(item_damage_max_value, evt =>
                {
                    damage.max = item_damage_max_value.value;
                    SetDamageData(isTargetEffect, 0, damage);
                    SkillCustomUpdate();
                }, 0, 0);

                Toggle toggle_damage_max_value = labelFromUXML.Query<Toggle>("toggle_damage_max_value");
                new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                    toggle_damage_max_value,
                    labelFromUXML.Query<VisualElement>("item_damage_max_value_contents"),
                    damage.maxEnabled == 1,
                    () =>
                    {
                        damage.maxEnabled = toggle_damage_max_value.value ? 1 : 0;
                        SkillCustomUpdate();
                    }
                );

                //分散度
                IntegerField item_max_value_input = labelFromUXML.Query<IntegerField>("item_damage_dispersity");
                item_max_value_input.value = damage.distribute;
                BaseInputFieldHandler.IntegerFieldCallback(item_max_value_input, evt =>
                {
                    damage.distribute = item_max_value_input.value;
                    SetDamageData(isTargetEffect, 0, damage);
                    SkillCustomUpdate();
                }, 0, 100);

                Toggle toggle_damage_dispersity = labelFromUXML.Query<Toggle>("toggle_damage_dispersity");
                new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                    toggle_damage_dispersity,
                    labelFromUXML.Query<VisualElement>("item_damage_dispersity_contents"),
                    damage.distributeEnabled == 1,
                    () =>
                    {
                        damage.distributeEnabled = toggle_damage_dispersity.value ? 1 : 0;
                        SkillCustomUpdate();
                    }
                );

                //会心
                VisualElement item_satisfaction = labelFromUXML.Query<VisualElement>("item_satisfaction");
                GroupBox groupBox = new GroupBox();
                item_satisfaction.Add(groupBox);
                var item_satisfaction_typeTextDropdownChoices =
                    EditorLocalize.LocalizeTexts(new List<string> {"WORD_0113", "WORD_0498"});
                var itemSatisfactionTypeTextDropdownChoicesName =
                    new List<string> {"radioButton-skillEdit-display15", "radioButton-skillEdit-display16"};

                var itemSatisfactionToggles = new List<RadioButton>();
                int count = 0;
                foreach (var choice in item_satisfaction_typeTextDropdownChoices)
                {
                    var toggle = new RadioButton
                    {
                        label = choice,
                        name = itemSatisfactionTypeTextDropdownChoicesName[count]
                    };
                    count++;
                    itemSatisfactionToggles.Add(toggle);
                    groupBox.Add(toggle);
                }
                var actions = new List<Action>
                {
                    () =>
                    {
                        damage.critical = 0;
                        SkillCustomUpdate();
                    },
                    () =>
                    {
                        damage.critical = 1;
                        SkillCustomUpdate();
                    }
                };
                
                new CommonToggleSelector().SetRadioSelector(itemSatisfactionToggles, damage.critical, actions);
                
                //オートガイド関連
                //習得想定レベル
                _itemLevelAutoGuide = labelFromUXML.Query<IntegerField>("item_level_autoguide");
                _itemLevelAutoGuide.value = damage.autoguide.level;
                BaseInputFieldHandler.IntegerFieldCallback(_itemLevelAutoGuide, evt =>
                {
                    damage.autoguide.level = _itemLevelAutoGuide.value;
                    SetDamageData(isTargetEffect, 0, damage);
                    SkillCustomUpdate();
                }, 1, 99);

                //対象の体数
                RadioButton toggle_target_one = labelFromUXML.Query<RadioButton>("radioButton-skillEdit-display17");
                RadioButton toggle_target_all = labelFromUXML.Query<RadioButton>("radioButton-skillEdit-display18");
                RadioButton toggle_target_random = labelFromUXML.Query<RadioButton>("radioButton-skillEdit-display19");
                
                var defaultTargetSelect = damage.autoguide.targetRange;
                var targetActions = new List<Action>
                {
                    () =>
                    {
                        if (toggle_target_one.value)
                        {
                            toggle_target_all.value = false;
                            toggle_target_random.value = false;
                            damage.autoguide.targetRange = 0;
                            SkillCustomUpdate();
                        }
                    },
                    () =>
                    {
                        if (toggle_target_all.value)
                        {
                            toggle_target_one.value = false;
                            toggle_target_random.value = false;
                            damage.autoguide.targetRange = 1;
                            SkillCustomUpdate();
                        }
                    },
                    () =>
                    {
                        if (toggle_target_random.value)
                        {
                            toggle_target_one.value = false;
                            toggle_target_all.value = false;
                            damage.autoguide.targetRange = 2;
                            SkillCustomUpdate();
                        }
                    }
                };

                new CommonToggleSelector().SetRadioSelector(
                    new List<RadioButton> {toggle_target_one, toggle_target_all, toggle_target_random}, defaultTargetSelect, targetActions);

                //反映ボタン
                Button autoguideExecute = labelFromUXML.Query<Button>("button_calc");
                autoguideExecute.clicked += () =>
                {
                    AutoGuideExecute(damage);
                    SetDamageData(isTargetEffect, 0, damage);
                    SkillCustomUpdate();
                };


                //以下はラベルで、計算結果を表示する用
                //固定値
                //攻撃側倍率の変更
                //対象側倍率の変更
                //消費MP
                //計算式
                //最小値
                //最大値
                _labelFixAutoGuide = labelFromUXML.Query<Label>("label_fix_autoguide");
                _labelAmagAutoGuide = labelFromUXML.Query<Label>("label_amag_autoguide");
                _labelBmagAutoGuide = labelFromUXML.Query<Label>("label_bmag_autoguide");
                _labelMpAutoGuide = labelFromUXML.Query<Label>("label_mp_autoguide");
                _labelCalcAutoGuide = labelFromUXML.Query<Label>("label_calc_autoguide");
                _labelMinAutoGuide = labelFromUXML.Query<Label>("label_min_autoguide");
                _labelMaxAutoGuide = labelFromUXML.Query<Label>("label_max_autoguide");

                //初期値
                _labelFixAutoGuide.text = damage.autoguide.fix.ToString("N2");
                _labelAmagAutoGuide.text = damage.autoguide.aMag.ToString("N2");
                _labelBmagAutoGuide.text = damage.autoguide.bMag.ToString("N2");
                _labelMpAutoGuide.text = damage.autoguide.mp.ToString();
                _labelCalcAutoGuide.text = damage.autoguide.calc;
                _labelMinAutoGuide.text = damage.autoguide.min.ToString("N2");
                _labelMaxAutoGuide.text = damage.autoguide.max.ToString("N2");

                //分散度
                IntegerField item_distribute_autoguide = labelFromUXML.Query<IntegerField>("item_distribute_autoguide");
                item_distribute_autoguide.value = damage.autoguide.distribute;
                BaseInputFieldHandler.IntegerFieldCallback(item_distribute_autoguide, evt =>
                {
                    damage.autoguide.distribute = item_distribute_autoguide.value;
                    SetDamageData(isTargetEffect, 0, damage);
                    SkillCustomUpdate();
                }, 0, 100);

                Toggle toggle_damage_dispersity_autoguide = labelFromUXML.Query<Toggle>("toggle_damage_dispersity_autoguide");
                new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                    toggle_damage_dispersity_autoguide,
                    labelFromUXML.Query<VisualElement>("item_distribute_autoguide_contents"),
                    damage.autoguide.distributeEnabled == 1,
                    () =>
                    {
                        damage.autoguide.distributeEnabled = toggle_damage_dispersity_autoguide.value ? 1 : 0;
                        SkillCustomUpdate();
                    }
                );

                //会心
                VisualElement itemSatisfactionAuto =
                    labelFromUXML.Query<VisualElement>("item_satisfaction_autoguide");
                GroupBox groupBox2 = new GroupBox();
                itemSatisfactionAuto.Add(groupBox2);
                var itemSatisfactionAutoTypeTextDropdownChoices =
                    EditorLocalize.LocalizeTexts(new List<string> {"WORD_0113", "WORD_0498"});
                var itemSatisfactionAutoTypeTextDropdownChoicesName =
                    new List<string> {"radioButton-skillEdit-display20", "radioButton-skillEdit-display21"};
                var itemSatisfactionAutoToggles = new List<RadioButton>();
                int autoCount = 0;
                foreach (var choice in itemSatisfactionAutoTypeTextDropdownChoices)
                {
                    var toggle = new RadioButton
                    {
                        label = choice,
                        name = itemSatisfactionAutoTypeTextDropdownChoicesName[autoCount]
                    };
                    itemSatisfactionAutoToggles.Add(toggle);
                    groupBox2.Add(toggle);
                    autoCount++;
                }
                var itemSatisfactionAutoTogglesActions = new List<Action>
                {
                    () =>
                    {
                        damage.autoguide.critical = 0;
                        SkillCustomUpdate();
                    },
                    () =>
                    {
                        damage.autoguide.critical = 1;
                        SkillCustomUpdate();
                
                    }
                };
                new CommonToggleSelector().SetRadioSelector(itemSatisfactionAutoToggles, damage.autoguide.critical, itemSatisfactionAutoTogglesActions);
                
            }
            else
            {
                var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(userDamageUxml);
                VisualElement labelFromUXML = visualTree.CloneTree();
                EditorLocalize.LocalizeElements(labelFromUXML);
                RootContainer.Q<VisualElement>("item_user_impact_damage_list").Add(labelFromUXML);

                //以下、各部品に対する設定項目
                //タイプ
                VisualElement itemUserImpactDamageArea =
                    labelFromUXML.Query<VisualElement>("item_user_impact_damage_area");
                itemUserImpactDamageArea.SetEnabled(damage.damageType == 1);
                VisualElement item_damage_type = labelFromUXML.Query<VisualElement>("item_user_impact_damage_type");
                var damageList = new List<string>()
                {
                    EditorLocalize.LocalizeText("WORD_0113"),
                    EditorLocalize.LocalizeText("WORD_0475"),
                };
                var item_damage_typePopupField =
                    new PopupFieldBase<string>(damageList, damage.damageType);
                item_damage_type.Add(item_damage_typePopupField);
                item_damage_typePopupField.RegisterValueChangedCallback(evt =>
                {
                    damage.damageType = item_damage_typePopupField.index;
                    itemUserImpactDamageArea.SetEnabled(damage.damageType == 1);
                    SetDamageData(isTargetEffect, 0, damage);
                    SkillCustomUpdate();
                });

                //属性
                VisualElement item_attribute_type =
                    labelFromUXML.Query<VisualElement>("item_user_impact_attribute_type");
                var item_attribute_typeTextDropdownChoices = new List<string>();
                var attribute = _systemSettingDataModel.elements;
                foreach (var a in attribute) item_attribute_typeTextDropdownChoices.Add(a.value);

                if (damage.elements.Count == 0) damage.elements = new List<int>();

                var damageElementIndex = damage.elements.Count > 0 ? damage.elements[0] : 0;
                var item_attribute_typePopupField =
                    new PopupFieldBase<string>(item_attribute_typeTextDropdownChoices, damageElementIndex);
                item_attribute_type.Add(item_attribute_typePopupField);
                item_attribute_typePopupField.RegisterValueChangedCallback(evt =>
                {
                    //暫定対処
                    damage.elements = new List<int>();
                    damage.elements.Add(item_attribute_typePopupField.index);
                    SetDamageData(isTargetEffect, 0, damage);
                    SkillCustomUpdate();
                });

                //計算式
                _itemUserImpactDamageFormula = labelFromUXML.Query<ImTextField>("item_user_impact_damage_formula");
                _itemUserImpactDamageFormula.value = damage.value;
                _itemUserImpactDamageFormula.RegisterCallback<FocusOutEvent>(evt =>
                {
                    damage.value = _itemUserImpactDamageFormula.value;
                    SetDamageData(isTargetEffect, 0, damage);
                    SkillCustomUpdate();
                });

                //分散度
                IntegerField item_max_value_input = labelFromUXML.Query<IntegerField>("item_user_impact_damage_dispersity");
                item_max_value_input.value = damage.distribute;
                BaseInputFieldHandler.IntegerFieldCallback(item_max_value_input, evt =>
                {
                    damage.distribute = item_max_value_input.value;
                    SetDamageData(isTargetEffect, 0, damage);
                    SkillCustomUpdate();
                }, 0, 100);

                Toggle item_user_impact_damage_dispersity_toggle = labelFromUXML.Query<Toggle>("item_user_impact_damage_dispersity_toggle");
                new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                    item_user_impact_damage_dispersity_toggle,
                    labelFromUXML.Query<VisualElement>("item_user_impact_damage_dispersity_contents"),
                    damage.distributeEnabled == 1,
                    () =>
                    {
                        damage.distributeEnabled = item_user_impact_damage_dispersity_toggle.value ? 1 : 0;
                        SkillCustomUpdate();
                    }
                );

                //会心
                VisualElement item_satisfaction = labelFromUXML.Query<VisualElement>("item_user_impact_satisfaction");
                GroupBox groupBox = new GroupBox();
                item_satisfaction.Add(groupBox);
                var item_satisfaction_typeTextDropdownChoices =
                    EditorLocalize.LocalizeTexts(new List<string> {"WORD_0113", "WORD_0498"});
                var itemSatisfactionTypeTextDropdownChoicesName =
                    new List<string> {"radioButton-skillEdit-display22", "radioButton-skillEdit-display23"};
                var itemSatisfactionToggles = new List<RadioButton>();
                int count = 0;
                foreach (var choice in item_satisfaction_typeTextDropdownChoices)
                {
                    var toggle = new RadioButton
                    {
                        label = choice,
                        name = itemSatisfactionTypeTextDropdownChoicesName[count]
                    };
                    itemSatisfactionToggles.Add(toggle);
                    groupBox.Add(toggle);
                    count++;
                }
                var itemSatisfactionTogglesActions = new List<Action>
                {
                    () =>
                    {
                        damage.critical = 0;
                        SkillCustomUpdate();
                    },
                    () =>
                    {
                        damage.critical = 1;
                        SkillCustomUpdate();
                
                    }
                };
                new CommonToggleSelector().SetRadioSelector(itemSatisfactionToggles, damage.critical, itemSatisfactionTogglesActions);
            }
        }

        private void SetHealData(
            bool isTargetEffect,
            int num2,
            ItemEffectHealParam heal
        ) {
            if (isTargetEffect)
                switch (num2)
                {
                    case 0:
                        _skillCustomDataModel.targetEffect.heal.hp = heal;
                        break;
                    case 1:
                        _skillCustomDataModel.targetEffect.heal.mp = heal;
                        break;
                    case 2:
                        _skillCustomDataModel.targetEffect.heal.tp = heal;
                        break;
                }
            else
                switch (num2)
                {
                    case 0:
                        _skillCustomDataModel.userEffect.heal.hp = heal;
                        break;
                    case 1:
                        _skillCustomDataModel.userEffect.heal.mp = heal;
                        break;
                    case 2:
                        _skillCustomDataModel.userEffect.heal.tp = heal;
                        break;
                }
        }

        private void SetDamageData(bool isTargetEffect, int i, Damage damage) {
            if (isTargetEffect)
                _skillCustomDataModel.targetEffect.damage = damage;
            else
                _skillCustomDataModel.userEffect.damage = damage;
        }

        private void SkillCustomUpdate(bool isHierarchyRefresh = false) {
            //セーブ部位
            databaseManagementService.SaveSkillCustom(_skillCustomDataModels);
            if (isHierarchyRefresh) _ = Editor.Hierarchy.Hierarchy.Refresh(Region.Skill);
        }

        private List<string> ParticleList() {
            var returnList = new List<string>();
            foreach (var animation in _animationDataModels) returnList.Add(animation.particleName.Replace("/", "／"));

            return returnList;
        }

        //ParticleのIdを今のIndexに変換して返す
        private int ParticleIdToIndex(string id) {
            var returnIndex = 0;
            for (var i = 0; i < _animationDataModels.Count; i++)
                if (_animationDataModels[i].id == id)
                {
                    returnIndex = i;
                    break;
                }

            return returnIndex;
        }

        //オートガイドを利用した計算を行う
        private void AutoGuideExecute(Damage damage) {
            //オートガイドが参照する項目の取得
            var classDataModel = databaseManagementService.LoadClassCommon()[0];
            var clearLevel = classDataModel.clearLevel; //クリアレベル
            var maxLevel = classDataModel.maxLevel; //最大レベル
            var expGainIncreaseValue = classDataModel.expGainIncreaseValue; //経験値の上限
            var maxHp = classDataModel.baseHpMaxValue;

            //習得想定レベル、体数は、本Inspector枠内で定義している
            //各パラメーターに有効な値が入っているかどうかの確認
            if (clearLevel < 1 || maxLevel < 1 || expGainIncreaseValue < 1 || damage.autoguide.level < 1 ||
                damage.autoguide.targetRange < 0)
                //有効な値が入っていないため、処理終了
                return;

            //オートガイド共通計算用の、標準モデル
            var standardModel = CalcStandardModel(maxLevel, clearLevel, expGainIncreaseValue, maxHp, 0);
            //想定習得レベルでのパラメータを算出
            standardModel.CalcAssumedLevel(damage.autoguide.level);
            standardModel.DebugLog();

            //攻撃側倍率の変更
            //係数値
            var a = (1.0f * standardModel.minHp / 4.0f + standardModel.minMagicDefense) / standardModel.minMagic *
                    0.26f;
            var c = a * 2.0f;
            var b = c * (clearLevel * 1f / maxLevel * 1f * 0.15f + 0.85f);
            
            //計算結果
            float amag = 0;
            //習得想定レベルとクリアレベルの関係から計算方法を変更する
            if (damage.autoguide.level < clearLevel + 1)
                //攻撃側倍率 = a + (b - a) / (クリアレベル - 1) * (習得想定レベル - 1)
                amag = a + (1.0f * b - a) / (clearLevel - 1.0f) * (damage.autoguide.level - 1.0f);
            else
                //攻撃側倍率 = b + (c - b) / (最大レベル - クリアレベル) * (習得想定レベル - クリアレベル)
                amag = b + (1.0f * c - b) / (1.0f * maxLevel - clearLevel) * (damage.autoguide.level - clearLevel);
            amag = (float) Math.Round(amag, 3);
            

            //固定値
            var cdmg = standardModel.hp / 4.0f + standardModel.magicDefense - standardModel.magic * amag;
            cdmg = (float) Math.Round(cdmg, 0);

            //対象側倍率の変更 (b.mag)
            var bmag = 1.0f;

            //消費MP
            // cdmgLvOne = 標準モデルの最小HP / 4 + 標準モデルの最小魔法防御力 - 標準モデルの最小魔法力 * a
            var cdmgLvOne = Mathf.RoundToInt(standardModel.minHp / 4.0f + standardModel.minMagicDefense - standardModel.minMagic * a);
            // aMP = (cdmgLvOne + 標準モデルの最小魔法力 * a) / (標準モデルの最小MP / 3)
            var aMp = (cdmgLvOne + standardModel.minMagic * a) / (standardModel.minMp / 3.0f);
            var cMp = aMp * 1.5f;
            var bMp = cMp * (1.0f * clearLevel / maxLevel * 0.15f + 0.85f);

            float calcOne;
            if (damage.autoguide.level < clearLevel + 1)
                calcOne = aMp + (1.0f * bMp - aMp) / (clearLevel - 1.0f) * (damage.autoguide.level - 1.0f);
            else
                calcOne = bMp + (1.0f * cMp - bMp) / (1.0f * maxLevel - clearLevel) *
                    (damage.autoguide.level - clearLevel);

            var mpf = (cdmg + standardModel.magic * amag) / calcOne;

            
            if (damage.autoguide.targetRange >= 1) mpf = mpf * 2;
            var mp = Mathf.RoundToInt(mpf);

            //最大値（ダメージ）
            //習得想定レベル+5
            //計算結果
            var level5 = damage.autoguide.level + 5;
            if (level5 > maxLevel) level5 = maxLevel;

            //係数値
            // c = HPの上限 * 0.45
            c = maxHp * 0.45f;
            a = c * 0.045f;
            b = c * (1.0f * clearLevel / maxLevel * 0.15f + 0.85f);

            //習得想定レベルの魔法力計算
            float magic;
            if (level5 < clearLevel + 1)
                //magic = a + (b - a) / (クリアレベル - 1) * (level5 - 1)
                magic = a + (b - a) / (clearLevel - 1.0f) * (level5 - 1.0f);
            else
                //magic = b + (c - b) / (最大レベル - クリアレベル) * (level5 - クリアレベル)
                magic = b + (c - b) / (1.0f * maxLevel - clearLevel) * (1.0f * level5 - clearLevel);

            //最大値
            var maxDamage = cdmg + magic * amag;
            maxDamage = Mathf.RoundToInt(maxDamage);

            //最小値
            float minDamage = 0;

            //計算結果を反映する
            damage.autoguide.fix = cdmg;
            damage.autoguide.aMag = amag;
            damage.autoguide.bMag = bmag;
            damage.autoguide.mp = mp;
            damage.autoguide.calc = cdmg.ToString("N2") + " + a.mat * " + amag.ToString("N2") + " - b.mdf * " +
                                    bmag.ToString("N2");
            damage.autoguide.min = minDamage;
            damage.autoguide.max = maxDamage;

            //画面上にも表示する
            _labelFixAutoGuide.text = damage.autoguide.fix.ToString("N2");
            _labelAmagAutoGuide.text = damage.autoguide.aMag.ToString("N2");
            _labelBmagAutoGuide.text = damage.autoguide.bMag.ToString("N2");
            _labelMpAutoGuide.text = damage.autoguide.mp.ToString();
            _labelCalcAutoGuide.text = damage.autoguide.calc;
            _labelMinAutoGuide.text = damage.autoguide.min.ToString("N2");
            _labelMaxAutoGuide.text = damage.autoguide.max.ToString("N2");

            //消費MPは自動反映する
            _skillCustomDataModel.basic.costMp = damage.autoguide.mp;
            _use_MP.value = _skillCustomDataModel.basic.costMp;
        }

        // 計算式の設定
        private void SetFormula() {
            // 計算式の初期値を入れる
            var system = databaseManagementService.LoadSystem();
            var skillCommon = databaseManagementService.LoadSkillCommon();
            if (system.skillTypes[_skillCustomDataModel.basic.skillType].id == "7955b088-df05-461c-b2e3-7d9dfc3941f6")
            {
                _skillCustomDataModel.targetEffect.damage.value =
                    "a.atk * " + skillCommon[0].damage.normalAttack.aMag + " - b.def * " +
                    skillCommon[0].damage.normalAttack.bMag;
                _skillCustomDataModel.userEffect.damage.value =
                    "a.atk * " + skillCommon[0].damage.normalAttack.aMag + " - b.def * " +
                    skillCommon[0].damage.normalAttack.bMag;
            }
            else if (system.skillTypes[_skillCustomDataModel.basic.skillType].id ==
                     "64183358-c72f-4190-b42c-4ba9b373282c")
            {
                _skillCustomDataModel.targetEffect.damage.value =
                    skillCommon[0].damage.magicAttack.cDmg + " + a.mat * " + skillCommon[0].damage.magicAttack.aMag +
                    " - b.mdf * " + skillCommon[0].damage.magicAttack.bMag;
                _skillCustomDataModel.userEffect.damage.value =
                    skillCommon[0].damage.magicAttack.cDmg + " + a.mat * " + skillCommon[0].damage.magicAttack.aMag +
                    " - b.mdf * " + skillCommon[0].damage.magicAttack.bMag;
            }
            else if (system.skillTypes[_skillCustomDataModel.basic.skillType].id ==
                     "16b91790-cb8f-4a38-a663-d3a4fe2a4614")
            {
                _skillCustomDataModel.targetEffect.damage.value =
                    skillCommon[0].damage.specialAttack.cDmg + " + a.atk * " +
                    skillCommon[0].damage.specialAttack.aMag + " - b.def * " + skillCommon[0].damage.specialAttack.bMag;
                _skillCustomDataModel.userEffect.damage.value =
                    skillCommon[0].damage.specialAttack.cDmg + " + a.atk * " +
                    skillCommon[0].damage.specialAttack.aMag + " - b.def * " + skillCommon[0].damage.specialAttack.bMag;
            }

            _itemDamageFormula.value = _skillCustomDataModel.targetEffect.damage.value;
            _itemUserImpactDamageFormula.value = _skillCustomDataModel.userEffect.damage.value;
        }
    }
}