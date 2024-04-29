using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Animation;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item;
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
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.Editor.Inspector.Effect.View.EffectInspectorElement;

namespace RPGMaker.Codebase.Editor.Inspector.Item.View
{
    /// <summary>
    /// [装備・アイテムの編集]-[アイテム] Inspector
    /// </summary>
    public class ItemEditInspectorElement : AbstractInspectorElement
    {
        private          List<AnimationDataModel>  _animationDataModels;

        private ItemDataModel _itemDataModel;
        private List<ItemDataModel> _itemDataModels;

        //その他全部
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Item/Asset/inspector_itemEdit.uxml"; } }

        //対象者のダメージ部分
        private readonly string targetDamageUxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/Item/Asset/inspector_ItemEdit_targetDamageTemp.uxml";

        //使用者のダメージ部分
        private readonly string userDamageUxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/Item/Asset/inspector_itemEdit_userDamageTemp.uxml";

        public ItemEditInspectorElement(ItemDataModel itemDataModel) {
            _itemDataModel = itemDataModel;
            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            _itemDataModels = databaseManagementService.LoadItem();
            _itemDataModel = _itemDataModels.Find(item => item.basic.id == _itemDataModel.basic.id);
            
            if (_itemDataModel == null)
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

            var itemEnum = new ItemEnums();
            _animationDataModels = databaseManagementService.LoadAnimation();

            // 画像選択
            //------------------------------------------------------------------------------------------------------------------------------
            // プレビュー画像
            Image iconImage = RootContainer.Query<Image>("icon_image");            
            var tex2d = AssetDatabase.LoadAssetAtPath<Texture2D>(PathManager.IMAGE_ICON + _itemDataModel.basic.iconId + ".png");
            BackgroundImageHelper.SetBackground(iconImage, new Vector2(66, 66), tex2d, LengthUnit.Pixel);


            // 画像名
            Label iconImageName = RootContainer.Query<Label>("icon_image_name");
            iconImageName.text = _itemDataModel.basic.iconId;

            // 画像変更ボタン
            Button iconChangeBtn = RootContainer.Query<Button>("icon_image_change_btn");
            iconChangeBtn.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.IMAGE_ICON, true);
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("WORD_1611"), data =>
                {
                    var imageName = (string) data;
                    _itemDataModel.basic.iconId = imageName;
                    iconImageName.text = imageName;
                    tex2d = AssetDatabase.LoadAssetAtPath<Texture2D>(PathManager.IMAGE_ICON + _itemDataModel.basic.iconId + ".png");
                    BackgroundImageHelper.SetBackground(iconImage, new Vector2(66, 66), tex2d, LengthUnit.Pixel);
                    Save();
                }, _itemDataModel.basic.iconId);
            };

            // 画像インポートボタン
            Button iconImportBtn = RootContainer.Query<Button>("icon_image_import_btn");
            iconImportBtn.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.IMAGE_ICON);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    _itemDataModel.basic.iconId = path;
                    iconImageName.text = path;
                    tex2d = AssetDatabase.LoadAssetAtPath<Texture2D>(PathManager.IMAGE_ICON + _itemDataModel.basic.iconId + ".png");
                    BackgroundImageHelper.SetBackground(iconImage, new Vector2(66, 66), tex2d, LengthUnit.Pixel);
                    Save();
                }
            };

            //ID
            Label itemDd = RootContainer.Query<Label>("item_id");
            itemDd.text = _itemDataModel.SerialNumberString;

            //名前
            ImTextField itemName = RootContainer.Query<ImTextField>("item_name");
            itemName.value = _itemDataModel.basic.name;
            itemName.RegisterCallback<FocusOutEvent>(o =>
            {
                _itemDataModel.basic.name = itemName.value;
                Save();
                UpdateData();
            });

            //説明
            ImTextField itemExplanation = RootContainer.Query<ImTextField>("item_explanation");
            itemExplanation.value = _itemDataModel.basic.description;
            itemExplanation.RegisterCallback<FocusOutEvent>(o =>
            {
                _itemDataModel.basic.description = itemExplanation.value;
                Save();
            });

            //アイテムタイプ
            VisualElement itemType = RootContainer.Query<VisualElement>("item_type");
            var itemTypePopupField = new PopupFieldBase<string>(EditorLocalize.LocalizeTexts(itemEnum.itemTypeLabel),
                _itemDataModel.basic.itemType);
            itemType.Add(itemTypePopupField);
            

            //売価と価格
            IntegerField itemPrice = RootContainer.Query<IntegerField>("item_price");
            itemPrice.value = _itemDataModel.basic.price;
            IntegerField itemSell = RootContainer.Query<IntegerField>("item_sell");
            itemSell.value = _itemDataModel.basic.sell;

            BaseInputFieldHandler.IntegerFieldCallback(itemPrice, evt =>
            {
                _itemDataModel.basic.price = itemPrice.value;
                //価格が入力された際に半分の値を売価に
                itemSell.value = itemPrice.value / 2;
                Save();
            }, 0, 0);
            BaseInputFieldHandler.IntegerFieldCallback(itemSell, evt =>
            {
                _itemDataModel.basic.sell = itemSell.value;
                Save();
            }, 0, 0);

            //消耗する
            var consumableToggleList = new List<RadioButton>();
            consumableToggleList.Add(RootContainer.Query<RadioButton>("radioButton-equipItem-display9"));
            consumableToggleList.Add(RootContainer.Query<RadioButton>("radioButton-equipItem-display10"));
            var consumableActions = new List<Action>
            {
                () =>
                {
                    _itemDataModel.basic.consumable = 0;
                    Save();
                },
                () =>
                {
                    _itemDataModel.basic.consumable = 1;
                    Save();
                }
            };
            new CommonToggleSelector().SetRadioSelector(consumableToggleList, _itemDataModel.basic.consumable, consumableActions);

            VisualElement itemAvailable = RootContainer.Query<VisualElement>("item_available");
            var itemAvailablePopupField = new PopupFieldBase<string>(
                EditorLocalize.LocalizeTexts(itemEnum.itemCanUseTimingLabel),
                _itemDataModel.basic.canUseTiming);
            itemAvailable.Add(itemAvailablePopupField);
            itemAvailablePopupField.RegisterValueChangedCallback(evt =>
            {
                _itemDataModel.basic.canUseTiming =
                    EditorLocalize.LocalizeTexts(itemEnum.itemCanUseTimingLabel).IndexOf(itemAvailablePopupField.value);
                Save();
            });

            //売却可
            var sellToggleList = new List<RadioButton>();
            sellToggleList.Add(RootContainer.Query<RadioButton>("radioButton-equipItem-display11"));
            sellToggleList.Add(RootContainer.Query<RadioButton>("radioButton-equipItem-display12"));
            
            //大事なものだった場合、売却不可にする
            if (_itemDataModel.basic.itemType == 2)
            {
                _itemDataModel.basic.canSell = 1;
                sellToggleList[0].SetEnabled(false);
                sellToggleList[0].value = false;
                sellToggleList[1].value = true;
            }
            else
            {
                sellToggleList[0].SetEnabled(true);
            }
            
            itemTypePopupField.RegisterValueChangedCallback(evt =>
            {
                _itemDataModel.basic.itemType =
                    EditorLocalize.LocalizeTexts(itemEnum.itemTypeLabel).IndexOf(itemTypePopupField.value);

                //大事なものだった場合、売却不可にする
                if (_itemDataModel.basic.itemType == 2)
                {
                    _itemDataModel.basic.canSell = 1;
                    sellToggleList[0].SetEnabled(false);
                    sellToggleList[0].value = false;
                    sellToggleList[1].value = true;
                }
                else
                {
                    sellToggleList[0].SetEnabled(true);
                }
                
                Save();
            });

            var sellActions = new List<Action>
            {
                () =>
                {
                    _itemDataModel.basic.canSell = 0;
                    Save();
                },
                () =>
                {
                    _itemDataModel.basic.canSell = 1;
                    Save();
                }
            };
            new CommonToggleSelector().SetRadioSelector(sellToggleList, _itemDataModel.basic.canSell, sellActions);

            //スイッチアイテム
            var switchItemToggleList = new List<RadioButton>();
            switchItemToggleList.Add(RootContainer.Query<RadioButton>("radioButton-equipItem-display13"));
            switchItemToggleList.Add(RootContainer.Query<RadioButton>("radioButton-equipItem-display14"));

            var switchItemActions = new List<Action>
            {
                () =>
                {
                    _itemDataModel.basic.switchItem = 0;
                    Save();
                },
                () =>
                {
                    _itemDataModel.basic.switchItem = 1;
                    Save();
                }
            };
            new CommonToggleSelector().SetRadioSelector(switchItemToggleList, _itemDataModel.basic.switchItem, switchItemActions);

            //対象者への効果
            //範囲
            var itemTargetToggleList = new List<RadioButton>();
            var itemTargetFoldoutList = new List<Foldout>();
            for (var i = 15; i <= 18; i++)
            {
                RadioButton toggle = RootContainer.Query<RadioButton>("radioButton-equipItem-display" + i);
                itemTargetToggleList.Add(toggle);
            }

            for (var i = 0; i < 3; i++)
            {
                Foldout foldout = RootContainer.Query<Foldout>("item_target_foldout" + (i + 1));
                itemTargetFoldoutList.Add(foldout);
            }

            //トグル切り替え時の非表示設定領域
            var item_target_toggleContents = new List<VisualElement>();
            item_target_toggleContents.Add(new VisualElement());
            item_target_toggleContents.Add(RootContainer.Query<VisualElement>("item_target_foldout1"));
            item_target_toggleContents.Add(RootContainer.Query<VisualElement>("item_target_foldout2"));
            item_target_toggleContents.Add(RootContainer.Query<VisualElement>("item_target_foldout3"));

            var targetTeamActions = new List<Action>
            {
                //なし
                () =>
                {
                    _itemDataModel.targetEffect.targetTeam = 0;
                    _itemDataModel.targetEffect.targetRange = 0;
                    Save();
                },
                //敵
                () =>
                {
                    _itemDataModel.targetEffect.targetTeam = 1;
                    //敵の範囲の部分
                    for (var i = 19; i <= 21; i++)
                    {
                        RadioButton toggle = RootContainer.Query<RadioButton>("radioButton-equipItem-display" + i);
                        if (toggle.value)
                        {
                            _itemDataModel.targetEffect.targetRange = i - 19;
                        }
                    }
                    Save();
                },
                //味方
                () =>
                {
                    _itemDataModel.targetEffect.targetTeam = 2;
                    //味方の範囲の部分
                    for (var i = 22; i <= 23; i++)
                    {
                        RadioButton toggle = RootContainer.Query<RadioButton>("radioButton-equipItem-display" + i);
                        if (toggle.value)
                        {
                            _itemDataModel.targetEffect.targetRange = i - 22;
                        }
                    }
                    Save();
                },
                //敵と味方
                () =>
                {
                    _itemDataModel.targetEffect.targetTeam = 3;
                    _itemDataModel.targetEffect.targetRange = 0;
                    Save();
                }
            };

            new CommonToggleSelector().SetRadioInVisualElementSelector(itemTargetToggleList, item_target_toggleContents,
                _itemDataModel.targetEffect.targetTeam, targetTeamActions);

            //使用者トグルのみ別枠で保存される
            Toggle itemTargetToggleUser = RootContainer.Query<Toggle>("item_target_toggle5");
            itemTargetToggleUser.value = _itemDataModel.targetEffect.targetUser == 1;
            itemTargetToggleUser.RegisterValueChangedCallback(o =>
            {
                _itemDataModel.targetEffect.targetUser = itemTargetToggleUser.value ? 1 : 0;
                Save();
            });

            //敵の範囲の部分
            var itemTargetEnemySpecificationToggleList = new List<RadioButton>();
            //ランダムの後ろの数入力
            IntegerField itemRandomNum = RootContainer.Query<IntegerField>("item_randomNum");
            for (var i = 19; i <= 21; i++)
            {
                RadioButton toggle = RootContainer.Query<RadioButton>("radioButton-equipItem-display" + i);
                itemTargetEnemySpecificationToggleList.Add(toggle);
            }

            var itemTargetEnemySpecificationActions = new List<Action>
            {
                () =>
                {
                    if (_itemDataModel.targetEffect.targetTeam != 1) return;
                    _itemDataModel.targetEffect.targetRange = 0;
                    itemRandomNum.SetEnabled(false);
                    Save();
                },
                () =>
                {
                    if (_itemDataModel.targetEffect.targetTeam != 1) return;
                    _itemDataModel.targetEffect.targetRange = 1;
                    itemRandomNum.SetEnabled(false);
                    Save();
                },
                () =>
                {
                    if (_itemDataModel.targetEffect.targetTeam != 1) return;
                    _itemDataModel.targetEffect.targetRange = 2;
                    itemRandomNum.SetEnabled(true);
                    Save();
                }
            };

            new CommonToggleSelector().SetRadioSelector(itemTargetEnemySpecificationToggleList,
                _itemDataModel.targetEffect.targetTeam == 1 ? _itemDataModel.targetEffect.targetRange : 0,
                itemTargetEnemySpecificationActions);

            itemRandomNum.value = _itemDataModel.targetEffect.randomNumber;
            BaseInputFieldHandler.IntegerFieldCallback(itemRandomNum,
                evt => { _itemDataModel.targetEffect.randomNumber = itemRandomNum.value; }, 1, 4);

            //味方の範囲の部分
            var itemTargetAlliesSpecificationToggleList = new List<RadioButton>();
            for (var i = 22; i <= 23; i++)
            {
                RadioButton toggle = RootContainer.Query<RadioButton>("radioButton-equipItem-display" + i);
                itemTargetAlliesSpecificationToggleList.Add(toggle);
            }

            var itemTargetAlliesSpecificationActions = new List<Action>
            {
                () =>
                {
                    if (_itemDataModel.targetEffect.targetTeam != 2) return;
                    _itemDataModel.targetEffect.targetRange = 0;
                    Save();
                },
                () =>
                {
                    if (_itemDataModel.targetEffect.targetTeam != 2) return;
                    _itemDataModel.targetEffect.targetRange = 1;
                    Save();
                }
            };
            new CommonToggleSelector().SetRadioSelector(itemTargetAlliesSpecificationToggleList,
                _itemDataModel.targetEffect.targetTeam == 2 ? _itemDataModel.targetEffect.targetRange : 0,
                itemTargetAlliesSpecificationActions);

            //状態異常
            var itemTargetStateToggles = new List<RadioButton>();
            for (var i = 24; i <= 26; i++)
            {
                RadioButton toggle = RootContainer.Query<RadioButton>("radioButton-equipItem-display" + i);
                itemTargetStateToggles.Add(toggle);
            }

            var itemTargetStateActions = new List<Action>
            {
                () =>
                {
                    if (_itemDataModel.targetEffect.targetTeam != 2) return;
                    _itemDataModel.targetEffect.targetStatus = 0;
                    Save();
                },
                () =>
                {
                    if (_itemDataModel.targetEffect.targetTeam != 2) return;
                    _itemDataModel.targetEffect.targetStatus = 1;
                    Save();
                },
                () =>
                {
                    if (_itemDataModel.targetEffect.targetTeam != 2) return;
                    _itemDataModel.targetEffect.targetStatus = 2;
                    Save();
                }
            };


            new CommonToggleSelector().SetRadioSelector(itemTargetStateToggles,
                _itemDataModel.targetEffect.targetTeam == 2 ? _itemDataModel.targetEffect.targetStatus : 0,
                itemTargetStateActions);

            //発動
            IntegerField itemSpeedCorrection = RootContainer.Query<IntegerField>("item_speed_correction");
            itemSpeedCorrection.value = _itemDataModel.targetEffect.activate.correctionSpeed;
            BaseInputFieldHandler.IntegerFieldCallback(itemSpeedCorrection, evt =>
            {
                _itemDataModel.targetEffect.activate.correctionSpeed =
                    itemSpeedCorrection.value;
                Save();
            }, -2000, 2000);

            IntegerField itemSuccessRate = RootContainer.Query<IntegerField>("item_success_rate");
            itemSuccessRate.value = _itemDataModel.targetEffect.activate.successRate;
            BaseInputFieldHandler.IntegerFieldCallback(itemSuccessRate, evt =>
            {
                _itemDataModel.targetEffect.activate.successRate = itemSuccessRate.value;
                Save();
            }, 0, 100);

            IntegerField itemConsecutiveNum = RootContainer.Query<IntegerField>("item_consecutive_num");
            itemConsecutiveNum.value = _itemDataModel.targetEffect.activate.continuousNumber;
            BaseInputFieldHandler.IntegerFieldCallback(itemConsecutiveNum, evt =>
            {
                _itemDataModel.targetEffect.activate.continuousNumber =
                    itemConsecutiveNum.value;
                Save();
            }, 1, 1);

            var itemHitTypeList = new List<Toggle>();
            for (var i = 0; i < 3; i++)
            {
                Toggle toggle = RootContainer.Query<Toggle>("item_hit_type" + (i + 1));
                itemHitTypeList.Add(toggle);
            }

            if (_itemDataModel.targetEffect.activate.hitType > -1)
            {
                itemHitTypeList[_itemDataModel.targetEffect.activate.hitType].value = true;
            }

            for (var i = 0; i < 3; i++)
            {
                var num2 = i;
                itemHitTypeList[num2].RegisterValueChangedCallback(o =>
                {
                    if (itemHitTypeList[num2].value)
                    {
                        _itemDataModel.targetEffect.activate.hitType = num2;
                        itemHitTypeList[num2].value = true;
                        if (num2 != 0)
                        {
                            itemHitTypeList[0].value = false;
                            itemHitTypeList[0].SetEnabled(true);
                        }

                        if (num2 != 1)
                        {
                            itemHitTypeList[1].value = false;
                            itemHitTypeList[1].SetEnabled(true);
                        }

                        if (num2 != 2)
                        {
                            itemHitTypeList[2].value = false;
                            itemHitTypeList[2].SetEnabled(true);
                        }

                        Save();
                    }else
                    {
                        //すべてチェックついていなかったら、「なし」扱い
                        if (!itemHitTypeList[0].value && !itemHitTypeList[1].value && !itemHitTypeList[2].value)
                        {
                            _itemDataModel.targetEffect.activate.hitType = -1;
                            Save();
                        }
                    }
                });
            }

            IntegerField itemTpEarnedValue = RootContainer.Query<IntegerField>("item_tp_earned_value");
            itemTpEarnedValue.value = _itemDataModel.userEffect.activate.getTp;
            BaseInputFieldHandler.IntegerFieldCallback(itemTpEarnedValue, evt =>
            {
                _itemDataModel.userEffect.activate.getTp =
                    itemTpEarnedValue.value;
                Save();
            }, 0, 0);

            //回復 - 対象者
            Toggle itemHpToggle = RootContainer.Query<Toggle>("item_hp_toggle");
            VisualElement itemHpFoldout = RootContainer.Query<VisualElement>("item_hp_foldout");
            Toggle itemMpToggle = RootContainer.Query<Toggle>("item_mp_toggle");
            VisualElement itemMpFoldout = RootContainer.Query<VisualElement>("item_mp_foldout");
            Toggle itemTpToggle = RootContainer.Query<Toggle>("item_tp_toggle");
            VisualElement itemTpFoldout = RootContainer.Query<VisualElement>("item_tp_foldout");

            new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                itemHpToggle,
                itemHpFoldout,
                _itemDataModel.targetEffect.heal.hp.enabled == 1,
                () =>
                {
                    _itemDataModel.targetEffect.heal.hp.enabled = itemHpToggle.value ? 1 : 0;
                    Save();
                }
            );
            new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                itemMpToggle,
                itemMpFoldout,
                _itemDataModel.targetEffect.heal.mp.enabled == 1,
                () =>
                {
                    _itemDataModel.targetEffect.heal.mp.enabled = itemMpToggle.value ? 1 : 0;
                    Save();
                }
            );
            new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                itemTpToggle,
                itemTpFoldout,
                _itemDataModel.targetEffect.heal.tp.enabled == 1,
                () =>
                {
                    _itemDataModel.targetEffect.heal.tp.enabled = itemTpToggle.value ? 1 : 0;
                    Save();
                }
            );

            //回復 - 使用者
            Toggle itemUserHpToggle = RootContainer.Query<Toggle>("item_user_hp_toggle");
            VisualElement itemUserHpFoldout = RootContainer.Query<VisualElement>("item_user_hp_foldout");
            Toggle itemUserMpToggle = RootContainer.Query<Toggle>("item_user_mp_toggle");
            VisualElement itemUserMpFoldout = RootContainer.Query<VisualElement>("item_user_mp_foldout");
            Toggle itemUserTpToggle = RootContainer.Query<Toggle>("item_user_tp_toggle");
            VisualElement itemUserTpFoldout = RootContainer.Query<VisualElement>("item_user_tp_foldout");

            new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                itemUserHpToggle,
                itemUserHpFoldout,
                _itemDataModel.userEffect.heal.hp.enabled == 1,
                () =>
                {
                    _itemDataModel.userEffect.heal.hp.enabled = itemUserHpToggle.value ? 1 : 0;
                    Save();
                }
            );
            new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                itemUserMpToggle,
                itemUserMpFoldout,
                _itemDataModel.userEffect.heal.mp.enabled == 1,
                () =>
                {
                    _itemDataModel.userEffect.heal.mp.enabled = itemUserMpToggle.value ? 1 : 0;
                    Save();
                }
            );
            new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                itemUserTpToggle,
                itemUserTpFoldout,
                _itemDataModel.userEffect.heal.tp.enabled == 1,
                () =>
                {
                    _itemDataModel.userEffect.heal.tp.enabled = itemUserTpToggle.value ? 1 : 0;
                    Save();
                }
            );

            //対象者
            DamageInspectorCreate(true);
            DamageInspectorSetting(true);
            //使用者
            DamageInspectorCreate(false);
            DamageInspectorSetting(false);

            VisualElement itemTraits = RootContainer.Query<VisualElement>("item_traits");
            var effectWindow = new EffectInspectorElement();
            itemTraits.Add(effectWindow);
            effectWindow.Init(_itemDataModel.targetEffect.otherEffects, data =>
            {
                var work = (List<TraitCommonDataModel>) data;
                _itemDataModel.targetEffect.otherEffects = work;
                Save();
            }, DisplayType.Item);

            VisualElement itemUserTraits = RootContainer.Query<VisualElement>("item_user_impact_traits");
            var userEffectWindow = new EffectInspectorElement();
            itemUserTraits.Add(userEffectWindow);
            userEffectWindow.Init(_itemDataModel.userEffect.otherEffects, data =>
            {
                var work = (List<TraitCommonDataModel>) data;
                _itemDataModel.userEffect.otherEffects = work;
                Save();
            }, DisplayType.Item);

            //アニメーションのフォールドダウン
            VisualElement animatiouFolddown = RootContainer.Query<VisualElement>("animatiou_folddown");
            var animatiouFolddownPopupField = new PopupFieldBase<string>(ParticleList(),
                ParticleIdToIndex(_itemDataModel.targetEffect.activate.animationId));
            animatiouFolddown.Add(animatiouFolddownPopupField);
            animatiouFolddownPopupField.RegisterValueChangedCallback(evt =>
            {
                _itemDataModel.targetEffect.activate.animationId =
                    _animationDataModels[animatiouFolddownPopupField.index].id;
                Save();
            });

            //回復フォールドアウト以下の要素(対象者)
            HealInspectorSetting(true, RootContainer);

            //回復フォールドアウト以下の要素(使用者)
            HealInspectorSetting(false, RootContainer);

            //メモ部分
            ImTextField itemMemo = RootContainer.Query<ImTextField>("item_memo");
            itemMemo.value = _itemDataModel.memo;
            itemMemo.RegisterCallback<FocusOutEvent>(o =>
            {
                _itemDataModel.memo = itemMemo.value;
                Save();
            });
        }

        //回復
        private void HealInspectorSetting(bool isTarget, VisualElement items) {
            //0==HP 1==MP 2==TP
            for (var i = 0; i < 3; i++)
            {
                var tag = "";
                var tag2 = "";
                var num2 = i;
                var heal = ItemDataModel.ItemEffectHealParam.CreateDefault();

                //使用者だった場合「tag2」位置に「user_impact_」が入る
                if (!isTarget)
                    tag2 = "user_impact_";

                switch (num2)
                {
                    case 0:
                        tag = "HP";
                        if (isTarget)
                            heal = _itemDataModel.targetEffect.heal.hp;
                        else
                            heal = _itemDataModel.userEffect.heal.hp;
                        break;
                    case 1:
                        tag = "MP";
                        if (isTarget)
                            heal = _itemDataModel.targetEffect.heal.mp;
                        else
                            heal = _itemDataModel.userEffect.heal.mp;
                        break;
                    case 2:
                        tag = "TP";
                        if (isTarget)
                            heal = _itemDataModel.targetEffect.heal.tp;
                        else
                            heal = _itemDataModel.userEffect.heal.tp;
                        break;
                }

                //計算式
                RadioButton itemFormulaToggle = items.Query<RadioButton>("radioButton-equipItem-display-" + tag2  + tag + "1");
                ImTextField itemInputField = items.Query<ImTextField>("item_" + tag2 + "input_field_" + tag);
                //割合
                RadioButton itemPercentageMaxToggle = items.Query<RadioButton>("radioButton-equipItem-display-" + tag2 + tag + "2");
                IntegerField itemRate = items.Query<IntegerField>("item_" + tag2 + "rate_" + tag);
                //固定値
                RadioButton itemFixedValueToggle = items.Query<RadioButton>("radioButton-equipItem-display-" + tag2 + tag + "3");
                IntegerField itemFixedValue = items.Query<IntegerField>("item_" + tag2 + "" + tag + "_fixed_value");
                //分散度
                Toggle itemDispersityToggle = items.Query<Toggle>("item_" + tag2 + "" + tag + "_dispersity_toggle");
                IntegerField itemDispersity = items.Query<IntegerField>("item_" + tag2 + "" + tag + "_dispersity");
                //最大値
                Toggle itemMaxValueToggle = items.Query<Toggle>("item_" + tag2 + "" + tag + "_max_value_toggle");
                IntegerField itemMaxValue = items.Query<IntegerField>("item_" + tag2 + "" + tag + "_max_value");

                //トグル設定
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
                        Save();
                    },
                    () =>
                    {
                        heal.calc.enabled = 0;
                        heal.fix.enabled = Convert.ToInt32(itemFixedValueToggle.value);
                        heal.perMax.enabled = 0;
                        SetHealData(isTarget, num2, heal);
                        Save();
                    },
                    () =>
                    {
                        heal.calc.enabled = 0;
                        heal.fix.enabled = 0;
                        heal.perMax.enabled = Convert.ToInt32(itemPercentageMaxToggle.value);
                        SetHealData(isTarget, num2, heal);
                        Save();
                    }
                };

                //計算式、固定値、割合
                new CommonToggleSelector().SetRadioInVisualElementSelector(
                    new List<RadioButton> { itemFormulaToggle, itemFixedValueToggle, itemPercentageMaxToggle },
                    new List<VisualElement> {
                        items.Query<VisualElement>("item_" + tag2 + "input_field_" + tag + "_contents"),
                        items.Query<VisualElement>("item_" + tag2 + "" + tag + "_fixed_value_contents"),
                        items.Query<VisualElement>("item_" + tag2 + "rate_" + tag + "_contents")
                    },
                    defaultSelect, actions);

                //分散度
                new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                    itemDispersityToggle,
                    items.Query<VisualElement>("item_" + tag2 + "" + tag + "_dispersity_contents"),
                    Convert.ToBoolean(heal.perMax.distributeEnabled),
                    () =>
                    {
                        heal.perMax.distributeEnabled = Convert.ToInt32(itemDispersityToggle.value);
                        SetHealData(isTarget, num2, heal);
                        Save();
                    }
                );

                //最大値
                new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                    itemMaxValueToggle,
                    items.Query<VisualElement>("item_" + tag2 + "" + tag + "_max_value_contents"),
                    Convert.ToBoolean(heal.perMax.maxEnabled),
                    () =>
                    {
                        heal.perMax.maxEnabled = Convert.ToInt32(itemMaxValueToggle.value);
                        SetHealData(isTarget, num2, heal);
                        Save();
                    }
                );

                //数値入力
                //分散度
                itemDispersity.value = heal.perMax.distribute;
                BaseInputFieldHandler.IntegerFieldCallback(itemDispersity, evt =>
                {
                    heal.perMax.distribute = itemDispersity.value;
                    SetHealData(isTarget, num2, heal);
                    Save();
                }, 0, 100);

                //最大値
                itemMaxValue.value = heal.perMax.max;
                BaseInputFieldHandler.IntegerFieldCallback(itemMaxValue, evt =>
                {
                    heal.perMax.max = itemMaxValue.value;
                    SetHealData(isTarget, num2, heal);
                    Save();
                }, 0, 0);

                //計算式
                itemInputField.value = heal.calc.value;
                itemInputField.RegisterCallback<FocusOutEvent>(o =>
                {
                    heal.calc.value = itemInputField.value;
                    SetHealData(isTarget, num2, heal);
                    Save();
                });

                //割合
                itemRate.value = heal.perMax.value;
                BaseInputFieldHandler.IntegerFieldCallback(itemRate, evt =>
                {
                    heal.perMax.value = itemRate.value;
                    SetHealData(isTarget, num2, heal);
                    Save();
                }, 0, 100);

                //固定値
                itemFixedValue.value = heal.fix.value;
                BaseInputFieldHandler.IntegerFieldCallback(itemFixedValue, evt =>
                {
                    heal.fix.value = itemFixedValue.value;
                    SetHealData(isTarget, num2, heal);
                    Save();
                }, 0, 0);

                //トグルの初期値
                itemFormulaToggle.value = Convert.ToBoolean(heal.calc.enabled);
                itemPercentageMaxToggle.value = Convert.ToBoolean(heal.perMax.enabled);
                itemFixedValueToggle.value = Convert.ToBoolean(heal.fix.enabled);
            }
        }

        //ダメージ部分
        private void DamageInspectorSetting(bool isTargetEffect) {
            var tag2 = "";
            var itemEnum = new ItemEnums();
            var damage = ItemDataModel.ItemEffectDamage.CreateDefault();
            if (!isTargetEffect)
            {
                if (_itemDataModel.userEffect.damage != null)
                    damage = _itemDataModel.userEffect.damage;
                tag2 = "user_impact_";
            }
            else
            {
                if (_itemDataModel.targetEffect.damage != null)
                    damage = _itemDataModel.targetEffect.damage;
            }

            VisualElement element = RootContainer.Query<VisualElement>("item_" + tag2 + "damage_list");

            //計算式
            ImTextField itemDamageFormula = element.Query<ImTextField>("item_" + tag2 + "damage_formula");
            itemDamageFormula.value = damage.value;
            itemDamageFormula.RegisterCallback<FocusOutEvent>(evt =>
            {
                damage.value = itemDamageFormula.value;
                SetDamageData(isTargetEffect, damage);
                Save();
            });

            //ダメージタイプ
            if (isTargetEffect)
            {
                VisualElement itemImpactDamageArea = element.Query<VisualElement>("item_damage_type_contents");
                VisualElement itemAttributeTypeArea = element.Query<VisualElement>("item_attribute_type");
                VisualElement item_damage_type = element.Query<VisualElement>("item_" + tag2 + "damage_type");
                var item_damage_typePopupField =
                    new PopupFieldBase<string>(EditorLocalize.LocalizeTexts(itemEnum.damageTypeLabel),
                        damage.damageType);

                itemImpactDamageArea.SetEnabled(damage.damageType != 0);
                itemAttributeTypeArea.SetEnabled(damage.damageType != 0);
                item_damage_type.Add(item_damage_typePopupField);
                item_damage_typePopupField.RegisterValueChangedCallback(evt =>
                {
                    damage.damageType = item_damage_typePopupField.index;
                    itemImpactDamageArea.SetEnabled(damage.damageType != 0);
                    itemAttributeTypeArea.SetEnabled(damage.damageType != 0);
                    SetDamageData(isTargetEffect, damage);
                    Save();
                });
            }
            else
            {
                VisualElement itemUserImpactDamageArea = element.Query<VisualElement>("item_user_impact_damage_area");
                VisualElement item_damage_type = element.Query<VisualElement>("item_" + tag2 + "damage_type");
                var damageList = new List<string>()
                {
                    EditorLocalize.LocalizeText("WORD_0113"),
                    EditorLocalize.LocalizeText("WORD_0475"),
                };
                var item_damage_typePopupField = new PopupFieldBase<string>(damageList, damage.damageType);
                itemUserImpactDamageArea.SetEnabled(damage.damageType != 0);
                item_damage_type.Add(item_damage_typePopupField);
                item_damage_typePopupField.RegisterValueChangedCallback(evt =>
                {
                    damage.damageType = item_damage_typePopupField.index;
                    itemUserImpactDamageArea.SetEnabled(damage.damageType != 0);
                    SetDamageData(isTargetEffect, damage);
                    Save();
                });
            }

            //仮置きの属性
            VisualElement itemAttributeType = element.Query<VisualElement>("item_" + tag2 + "attribute_type");
            var itemAttributeTypeTextDropdownChoices = new List<string>();
            foreach (var a in databaseManagementService.LoadSystem().elements)
                itemAttributeTypeTextDropdownChoices.Add(EditorLocalize.LocalizeText(a.value));
            if (damage.elements.Count == 0) damage.elements = new List<int>();

            var damageElementIndex = damage.elements.Count > 0 ? damage.elements[0] : 0;
            var itemAttributeTypePopupField =
                new PopupFieldBase<string>(itemAttributeTypeTextDropdownChoices, damageElementIndex);
            itemAttributeType.Add(itemAttributeTypePopupField);
            itemAttributeTypePopupField.RegisterValueChangedCallback(evt =>
            {
                //暫定対処
                damage.elements = new List<int>();
                damage.elements.Add(itemAttributeTypePopupField.index);
                SetDamageData(isTargetEffect, damage);
                Save();
            });

            //分散度
            Toggle itemDispersityInputToggle = element.Query<Toggle>("item_" + tag2 + "damage_dispersity_toggle");
            new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                itemDispersityInputToggle,
                element.Query<VisualElement>("item_" + tag2 + "damage_dispersity_contents"),
                Convert.ToBoolean(damage.distributeEnabled),
                () =>
                {
                    damage.distributeEnabled = Convert.ToInt32(itemDispersityInputToggle.value);
                    SetDamageData(isTargetEffect, damage);
                    Save();
                }
            );

            IntegerField itemMaxValueInput = element.Query<IntegerField>("item_" + tag2 + "damage_dispersity");
            itemMaxValueInput.value = damage.distribute;
            BaseInputFieldHandler.IntegerFieldCallback(itemMaxValueInput, evt =>
            {
                damage.distribute = itemMaxValueInput.value;
                SetDamageData(isTargetEffect, damage);
                Save();
            }, 0, 100);

            //最大値
            Toggle itemDamageMaxValueToggle = element.Query<Toggle>("item_" + tag2 + "damage_max_value_toggle");
            new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                itemDamageMaxValueToggle,
                element.Query<VisualElement>("item_" + tag2 + "damage_max_value_contents"),
                Convert.ToBoolean(damage.maxEnabled),
                () =>
                {
                    damage.maxEnabled = Convert.ToInt32(itemDamageMaxValueToggle.value);
                    SetDamageData(isTargetEffect, damage);
                    Save();
                }
            );

            IntegerField itemDamageMaxValue = element.Query<IntegerField>("item_" + tag2 + "damage_max_value");
            itemDamageMaxValue.value = damage.max;
            BaseInputFieldHandler.IntegerFieldCallback(itemDamageMaxValue, evt =>
            {
                damage.max = itemDamageMaxValue.value;
                SetDamageData(isTargetEffect, damage);
                Save();
            }, 0, 0);

            //最小値
            Toggle itemDamageMinValueToggle = element.Query<Toggle>("item_" + tag2 + "damage_min_value_toggle");
            new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                itemDamageMinValueToggle,
                element.Query<VisualElement>("item_" + tag2 + "damage_min_value_contents"),
                Convert.ToBoolean(damage.maxEnabled),
                () =>
                {
                    damage.minEnabled = Convert.ToInt32(itemDamageMinValueToggle.value);
                    SetDamageData(isTargetEffect, damage);
                    Save();
                }
            );

            IntegerField itemDamageMinValue = element.Query<IntegerField>("item_" + tag2 + "damage_min_value");
            itemDamageMinValue.value = damage.min;
            BaseInputFieldHandler.IntegerFieldCallback(itemDamageMinValue, evt =>
            {
                damage.min = itemDamageMinValue.value;
                SetDamageData(isTargetEffect, damage);
                Save();
            }, 0, 0);

            //会心
            VisualElement itemSatisfaction = element.Query<VisualElement>("item_" + tag2 + "satisfaction");
            GroupBox groupBox = new GroupBox();
            itemSatisfaction.Add(groupBox);
            var itemSatisfactionTypeTextDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string> {"WORD_0498", "WORD_0113"});
            var itemSatisfactionTypeTextDropdownChoicesName =
                isTargetEffect ? 
                    new List<string> {"radioButton-skillEdit-display27", "radioButton-skillEdit-display28"}:
                    new List<string> {"radioButton-skillEdit-display29", "radioButton-skillEdit-display30"};

            var itemSatisfactionToggles = new List<RadioButton>();
            int count = 0;
            foreach (var choice in itemSatisfactionTypeTextDropdownChoices)
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

            var actions = new List<Action>
            {
                () =>
                {
                    damage.critical = 1;
                    Save();
                },
                () =>
                {
                    damage.critical = 0;
                    Save();
                }
            };
            new CommonToggleSelector().SetRadioSelector(itemSatisfactionToggles, damage.critical, actions);
        }

        private void DamageInspectorCreate(bool isTargetEffect) {
            if (isTargetEffect)
            {
                var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(targetDamageUxml);
                VisualElement labelFromUxml = visualTree.CloneTree();
                EditorLocalize.LocalizeElements(labelFromUxml);
                RootContainer.Q<VisualElement>("item_damage_list").Add(labelFromUxml);
            }
            else
            {
                var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(userDamageUxml);
                VisualElement labelFromUxml = visualTree.CloneTree();
                EditorLocalize.LocalizeElements(labelFromUxml);
                RootContainer.Q<VisualElement>("item_user_impact_damage_list").Add(labelFromUxml);
            }
        }

        private void SetHealData(bool isTargetEffect, int num2, ItemDataModel.ItemEffectHealParam heal) {
            if (isTargetEffect)
                switch (num2)
                {
                    case 0:
                        _itemDataModel.targetEffect.heal.hp = heal;
                        break;
                    case 1:
                        _itemDataModel.targetEffect.heal.mp = heal;
                        break;
                    case 2:
                        _itemDataModel.targetEffect.heal.tp = heal;
                        break;
                }
            else
                switch (num2)
                {
                    case 0:
                        _itemDataModel.userEffect.heal.hp = heal;
                        break;
                    case 1:
                        _itemDataModel.userEffect.heal.mp = heal;
                        break;
                    case 2:
                        _itemDataModel.userEffect.heal.tp = heal;
                        break;
                }
        }

        private void SetDamageData(bool isTargetEffect, ItemDataModel.ItemEffectDamage damage) {
            if (isTargetEffect)
                _itemDataModel.targetEffect.damage = damage;
            else
                _itemDataModel.userEffect.damage = damage;
        }

        override protected void SaveContents() {
            databaseManagementService.SaveItem(_itemDataModels);
        }

        //アニメーションのデータの名前をListで返す
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

        private void UpdateData() {
            _ = Editor.Hierarchy.Hierarchy.Refresh(Region.Equip);
        }
    }
}