using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.State;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository;
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

namespace RPGMaker.Codebase.Editor.Inspector.State.View
{
    /// <summary>
    /// [ステートの編集] Inspector
    /// </summary>
    public class StateEditInspectorElement : AbstractInspectorElement
    {
        private          StateDataModel       _stateDataModel;
        private readonly List<StateDataModel> _stateDataModels;

        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/State/Asset/inspector_stateEdit.uxml"; } }

        public StateEditInspectorElement(StateDataModel stateDataModel) {
            _stateDataModels = databaseManagementService.LoadStateEdit();

            for (var i = 0; i < _stateDataModels.Count; i++)
                if (_stateDataModels[i].id == stateDataModel.id)
                {
                    _stateDataModel = _stateDataModels[i];
                    break;
                }

            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            _stateDataModel = databaseManagementService.LoadStateEdit()
                .Find(item => item.id == _stateDataModel.id);

            if (_stateDataModel == null)
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
            var stateEnum = new StateEnums();

            //ID
            Label stateId = RootContainer.Query<Label>("state_id");
            stateId.text = _stateDataModel.SerialNumberString;

            ImTextField stateName = RootContainer.Query<ImTextField>("state_name");
            stateName.value = _stateDataModel.name;
            stateName.RegisterCallback<FocusOutEvent>(o =>
            {
                _stateDataModel.name = stateName.value;
                SaveData();
                _UpdateSceneView();
            });

            IntegerField statePriority = RootContainer.Query<IntegerField>("state_priority");
            statePriority.value = _stateDataModel.priority;
            BaseInputFieldHandler.IntegerFieldCallback(statePriority, evt =>
            {
                _stateDataModel.priority = statePriority.value;
                SaveData();
            }, 0, 100);

            // 画像選択
            //------------------------------------------------------------------------------------------------------------------------------
            // プレビュー画像
            Image iconImage = RootContainer.Query<Image>("icon_image");
            
            var tex2d = AssetDatabase.LoadAssetAtPath<Texture2D>(PathManager.IMAGE_ICON + _stateDataModel.iconId + ".png");
            BackgroundImageHelper.SetBackground(iconImage, new Vector2(66, 66), tex2d, LengthUnit.Pixel);

            // 画像名
            Label iconImageName = RootContainer.Query<Label>("icon_image_name");
            iconImageName.text = _stateDataModel.iconId;

            // 画像変更ボタン
            Button iconChangeBtn = RootContainer.Query<Button>("icon_image_change_btn");
            iconChangeBtn.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.IMAGE_ICON, true);
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("WORD_1611"), data =>
                {
                    var imageName = (string) data;
                    _stateDataModel.iconId = imageName;
                    iconImageName.text = imageName;
                    tex2d = AssetDatabase.LoadAssetAtPath<Texture2D>(PathManager.IMAGE_ICON + _stateDataModel.iconId + ".png");
                    BackgroundImageHelper.SetBackground(iconImage, new Vector2(66, 66), tex2d, LengthUnit.Pixel);
                    SaveData();
                }, _stateDataModel.iconId);
            };

            // 画像インポートボタン
            Button iconImportBtn = RootContainer.Query<Button>("icon_image_import_btn");
            iconImportBtn.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.IMAGE_ICON);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    _stateDataModel.iconId = path;
                    iconImageName.text = path;
                    tex2d = AssetDatabase.LoadAssetAtPath<Texture2D>(PathManager.IMAGE_ICON + _stateDataModel.iconId + ".png");
                    BackgroundImageHelper.SetBackground(iconImage, new Vector2(66, 66), tex2d, LengthUnit.Pixel);
                    SaveData();
                }
            };

            VisualElement stateAnimation = RootContainer.Query<VisualElement>("state_animation");
            var stateAnimationTextDropdownChoices = EditorLocalize.LocalizeTexts(stateEnum.StateAnimation);
            var stateAnimationPopupField = new PopupFieldBase<string>(stateAnimationTextDropdownChoices,
                _stateDataModel.motion);
            stateAnimation.Add(stateAnimationPopupField);
            stateAnimationPopupField.RegisterValueChangedCallback(evt =>
            {
                _stateDataModel.motion =
                    stateAnimationTextDropdownChoices.IndexOf(stateAnimationPopupField.value);
                SaveData();
            });

            // 素材管理からデータ取得
            AssetManageRepository.OrderManager.OrderData orderData;
            var assetManageData = new List<AssetManageDataModel>();
            orderData = AssetManageRepository.OrderManager.Load();

            var manageDatas = databaseManagementService.LoadAssetManage();

            if (orderData.orderDataList[(int) AssetCategoryEnum.SUPERPOSITION].idList != null)
                for (var i = 0; i < orderData.orderDataList[(int) AssetCategoryEnum.SUPERPOSITION].idList.Count; i++)
                {
                    var data = manageDatas.Find(c =>
                        c.id == orderData.orderDataList[(int) AssetCategoryEnum.SUPERPOSITION].idList[i]);
                    assetManageData.Add(data);
                }

            VisualElement stateSuperposition = RootContainer.Query<VisualElement>("state_superposition");
            var stateSuperpositionTextDropdownChoices = new List<string>();

            var index = 0;

            stateSuperpositionTextDropdownChoices.Add(EditorLocalize.LocalizeText("WORD_0113"));
            for (var i = 0; i < assetManageData.Count; i++)
            {
                stateSuperpositionTextDropdownChoices.Add(assetManageData[i].name);
                if (assetManageData[i].id == _stateDataModel.overlay) index = i + 1;
            }


            var stateSuperpositionPopupField =
                new PopupFieldBase<string>(stateSuperpositionTextDropdownChoices,
                    index);
            stateSuperposition.Add(stateSuperpositionPopupField);
            stateSuperpositionPopupField.RegisterValueChangedCallback(evt =>
            {
                if (stateSuperpositionPopupField.index == 0)
                    _stateDataModel.overlay = "";
                else
                    _stateDataModel.overlay = assetManageData[stateSuperpositionPopupField.index - 1].id;

                SaveData();
            });

            VisualElement stateApplication = RootContainer.Query<VisualElement>("state_application");
            var stateApplicationTextDropdownChoices = EditorLocalize.LocalizeTexts(stateEnum.StateApplication);
            var stateApplicationPopupField =
                new PopupFieldBase<string>(stateApplicationTextDropdownChoices,
                    _stateDataModel.stateOn);
            stateApplication.Add(stateApplicationPopupField);
            stateApplicationPopupField.RegisterValueChangedCallback(evt =>
            {
                _stateDataModel.stateOn =
                    stateApplicationTextDropdownChoices.IndexOf(stateApplicationPopupField.value);
                SaveData();
            });

            ImTextField stateMemo = RootContainer.Query<ImTextField>("state_memo");
            stateMemo.value = _stateDataModel.note;
            //折り返し
            stateMemo.style.whiteSpace = WhiteSpace.Normal;
            stateMemo.multiline = true;

            stateMemo.RegisterCallback<FocusOutEvent>(o =>
            {
                //140字制限
                if (stateMemo.value.Length >= 140)
                    stateMemo.value.Substring(0, 140);

                _stateDataModel.note = stateMemo.value;
                SaveData();
            });

            VisualElement stateActionConstraints = RootContainer.Query<VisualElement>("state_action_constraints");
            var stateActionConstraintsTextDropdownChoices =
                EditorLocalize.LocalizeTexts(stateEnum.StateActionConstraints);
            var stateActionConstraintsPopupField = new PopupFieldBase<string>(
                stateActionConstraintsTextDropdownChoices, _stateDataModel.restriction);
            stateActionConstraints.Add(stateActionConstraintsPopupField);
            stateActionConstraintsPopupField.RegisterValueChangedCallback(evt =>
            {
                _stateDataModel.restriction = stateActionConstraintsPopupField.index;
                SaveData();
            });

            /////////////////////////////////////////////////////////▼バトル中//////////////////////////////////////////////////////////////////////

            Toggle stateEndBattleTime = RootContainer.Query<Toggle>("state_end_battle_time");
            stateEndBattleTime.value = Convert.ToBoolean(_stateDataModel.removeAtBattleEnd);
            stateEndBattleTime.RegisterValueChangedCallback(o =>
            {
                _stateDataModel.removeAtBattleEnd =
                    Convert.ToInt32(stateEndBattleTime.value);
                SaveData();
            });

            //先にターンの要素取得を行う
            IntegerField stateContinuationNumTurnsMin = RootContainer.Query<IntegerField>("state_continuation_num_turns_min");
            IntegerField stateContinuationNumTurnsMax = RootContainer.Query<IntegerField>("state_continuation_num_turns_max");


            //戦闘中解除のタイミング
            Toggle stateOutCombatToggle = RootContainer.Query<Toggle>("state_out_combat_toggle");
            new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                stateOutCombatToggle,
                RootContainer.Query<VisualElement>("state_out_combat_contents"),
                Convert.ToBoolean(_stateDataModel.removeAtBattling),
                () =>
                {
                    _stateDataModel.removeAtBattling = Convert.ToInt32(stateOutCombatToggle.value);
                    SaveData();
                    stateContinuationNumTurnsMin.SetEnabled(_stateDataModel.autoRemovalTiming > 0 && _stateDataModel.removeAtBattling == 1);
                    stateContinuationNumTurnsMax.SetEnabled(_stateDataModel.autoRemovalTiming > 0 && _stateDataModel.removeAtBattling == 1);
                }
            );

            //マイナスだった場合、初期値に戻す
            if (_stateDataModel.autoRemovalTiming < 0) _stateDataModel.autoRemovalTiming = 0;

            stateContinuationNumTurnsMin.SetEnabled(_stateDataModel.autoRemovalTiming > 0 && _stateDataModel.removeAtBattling == 1);
            stateContinuationNumTurnsMax.SetEnabled(_stateDataModel.autoRemovalTiming > 0 && _stateDataModel.removeAtBattling == 1);

            VisualElement stateOutCombatTiming = RootContainer.Query<VisualElement>("state_out_combat_Timing");
            var stateOutCombatTimingPopupField = new PopupFieldBase<string>(
                EditorLocalize.LocalizeTexts(stateEnum.StateBattling), _stateDataModel.autoRemovalTiming);
            stateOutCombatTiming.Add(stateOutCombatTimingPopupField);
            stateOutCombatTimingPopupField.RegisterValueChangedCallback(evt =>
            {
                _stateDataModel.autoRemovalTiming = stateOutCombatTimingPopupField.index;
                stateContinuationNumTurnsMin.SetEnabled(_stateDataModel.autoRemovalTiming > 0 && _stateDataModel.removeAtBattling == 1);
                stateContinuationNumTurnsMax.SetEnabled(_stateDataModel.autoRemovalTiming > 0 && _stateDataModel.removeAtBattling == 1);
                SaveData();
            });


            stateContinuationNumTurnsMin.value = _stateDataModel.minTurns;
            BaseInputFieldHandler.IntegerFieldCallback(stateContinuationNumTurnsMin, evt =>
            {
                _stateDataModel.minTurns = stateContinuationNumTurnsMin.value;
                SaveData();
            }, 1, 9999);

            stateContinuationNumTurnsMax.value = _stateDataModel.maxTurns;
            BaseInputFieldHandler.IntegerFieldCallback(stateContinuationNumTurnsMax, evt =>
            {
                _stateDataModel.maxTurns = stateContinuationNumTurnsMax.value;
                SaveData();
            }, 1, 9999);

            Toggle stateReleaseActionRestrictions = RootContainer.Query<Toggle>("state_release_action_restrictions");
            stateReleaseActionRestrictions.value =
                Convert.ToBoolean(_stateDataModel.inBattleRemoveRestriction);
            stateReleaseActionRestrictions.RegisterValueChangedCallback(o =>
            {
                _stateDataModel.inBattleRemoveRestriction =
                    Convert.ToInt32(stateReleaseActionRestrictions.value);
                SaveData();
            });

            IntegerField stateProbability = RootContainer.Query<IntegerField>("state_probability");
            stateProbability.value = _stateDataModel.inBattleRemoveProbability;
            BaseInputFieldHandler.IntegerFieldCallback(stateProbability, evt =>
            {
                _stateDataModel.inBattleRemoveProbability = stateProbability.value;
                SaveData();
            }, 0, 100);
            Toggle stateCanceledDamage = RootContainer.Query<Toggle>("state_canceled_damage");
            stateCanceledDamage.value =
                Convert.ToBoolean(_stateDataModel.inBattleRemoveDamage);
            if (stateCanceledDamage.value)
                stateProbability.SetEnabled(true);
            else
                stateProbability.SetEnabled(false);
            stateCanceledDamage.RegisterValueChangedCallback(o =>
            {
                _stateDataModel.inBattleRemoveDamage =
                    Convert.ToInt32(stateCanceledDamage.value);
                SaveData();
                if (stateCanceledDamage.value)
                    stateProbability.SetEnabled(true);
                else
                    stateProbability.SetEnabled(false);
            });
            /////////////////////////////////////////////////////////▼バトル中//////////////////////////////////////////////////////////////////////

            /////////////////////////////////////////////////////////▼移動中//////////////////////////////////////////////////////////////////////
            IntegerField stateReleaseNumStep = RootContainer.Query<IntegerField>("state_release_num_step");
            stateReleaseNumStep.value = _stateDataModel.stepsToRemove;
            BaseInputFieldHandler.IntegerFieldCallback(stateReleaseNumStep, evt =>
            {
                stateReleaseNumStep.value =
                    _stateDataModel.stepsToRemove = stateReleaseNumStep.value;
                SaveData();
            }, 0, 9999);
            Toggle stateReleaseNumStepToggle = RootContainer.Query<Toggle>("state_release_num_step_toggle");
            stateReleaseNumStepToggle.value =
                Convert.ToBoolean(_stateDataModel.removeByWalking);
            if (stateReleaseNumStepToggle.value)
                stateReleaseNumStep.SetEnabled(true);
            else
                stateReleaseNumStep.SetEnabled(false);
            stateReleaseNumStepToggle.RegisterValueChangedCallback(o =>
            {
                _stateDataModel.removeByWalking =
                    Convert.ToInt32(stateReleaseNumStepToggle.value);
                SaveData();
                if (stateReleaseNumStepToggle.value)
                    stateReleaseNumStep.SetEnabled(true);
                else
                    stateReleaseNumStep.SetEnabled(false);
            });
            /////////////////////////////////////////////////////////▼移動中//////////////////////////////////////////////////////////////////////


            /////////////////////////////////////////////////////////▼常時//////////////////////////////////////////////////////////////////////
            Toggle stateReleasedBehavioralConstraintse =
                RootContainer.Query<Toggle>("state_released_behavioral_constraintse");
            stateReleasedBehavioralConstraintse.value =
                Convert.ToBoolean(_stateDataModel.removeByRestriction);
            stateReleasedBehavioralConstraintse.RegisterValueChangedCallback(o =>
            {
                _stateDataModel.removeByRestriction =
                    Convert.ToInt32(stateReleasedBehavioralConstraintse.value);
                SaveData();
            });


            IntegerField stateContinuousProbability = RootContainer.Query<IntegerField>("state_continuous_probability");
            stateContinuousProbability.value = _stateDataModel.inBattleRemoveProbability;
            BaseInputFieldHandler.IntegerFieldCallback(stateContinuousProbability, evt =>
            {
                stateContinuousProbability.value = _stateDataModel.inBattleRemoveProbability =
                    stateContinuousProbability.value;
                SaveData();
            }, 0, 100);
            Toggle stateContinuousCanceledDamage = RootContainer.Query<Toggle>("state_continuous_canceled_damage");
            stateContinuousCanceledDamage.value =
                Convert.ToBoolean(_stateDataModel.removeByDamage);
            if (stateContinuousCanceledDamage.value)
                stateContinuousProbability.SetEnabled(true);
            else
                stateContinuousProbability.SetEnabled(false);
            stateContinuousCanceledDamage.RegisterValueChangedCallback(o =>
            {
                _stateDataModel.removeByDamage =
                    Convert.ToInt32(stateContinuousCanceledDamage.value);
                SaveData();
                if (stateContinuousCanceledDamage.value)
                    stateContinuousProbability.SetEnabled(true);
                else
                    stateContinuousProbability.SetEnabled(false);
            });

            /////////////////////////////////////////////////////////▼常時//////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////▼発生頻度//////////////////////////////////////////////////////////////////////

            IntegerField statusContinuousNumStep = RootContainer.Query<IntegerField>("status_continuous_num_step");
            Toggle stepGeneration = RootContainer.Query<Toggle>("step_generation");

            stepGeneration.value = _stateDataModel.stepGeneration == 1;
            statusContinuousNumStep.SetEnabled(stepGeneration.value);

            stepGeneration.RegisterCallback<FocusOutEvent>(o =>
            {
                _stateDataModel.stepGeneration = stepGeneration.value ? 1 : 0;
                statusContinuousNumStep.SetEnabled(stepGeneration.value);
                SaveData();
            });
            statusContinuousNumStep.value = _stateDataModel.occurrenceFrequencyStep;
            BaseInputFieldHandler.IntegerFieldCallback(statusContinuousNumStep, evt =>
            {
                statusContinuousNumStep.value = _stateDataModel.occurrenceFrequencyStep =
                    statusContinuousNumStep.value;
                SaveData();
            }, 0, 9999);
            /////////////////////////////////////////////////////////▼発生頻度//////////////////////////////////////////////////////////////////////


            ImTextField stateAddedByAllyTiming = RootContainer.Query<ImTextField>("state_added_by_ally_timing");
            stateAddedByAllyTiming.value = _stateDataModel.message1;
            stateAddedByAllyTiming.RegisterCallback<FocusOutEvent>(o =>
            {
                _stateDataModel.message1 = stateAddedByAllyTiming.value;
                SaveData();
            });

            ImTextField stateAddedByEnemyTiming = RootContainer.Query<ImTextField>("state_added_by_enemy_timing");
            stateAddedByEnemyTiming.value = _stateDataModel.message2;
            stateAddedByEnemyTiming.RegisterCallback<FocusOutEvent>(o =>
            {
                _stateDataModel.message2 = stateAddedByEnemyTiming.value;
                SaveData();
            });

            ImTextField stateContinuedTiming = RootContainer.Query<ImTextField>("state_continued_timing");
            stateContinuedTiming.value = _stateDataModel.message3;
            stateContinuedTiming.RegisterCallback<FocusOutEvent>(o =>
            {
                _stateDataModel.message3 = stateContinuedTiming.value;
                SaveData();
            });

            ImTextField stateCanceledTiming = RootContainer.Query<ImTextField>("state_canceled_timing");
            stateCanceledTiming.value = _stateDataModel.message4;
            stateCanceledTiming.RegisterCallback<FocusOutEvent>(o =>
            {
                _stateDataModel.message4 = stateCanceledTiming.value;
                SaveData();
            });


            if (_stateDataModel == null)
            {
                stateContinuousProbability.SetEnabled(false);
                stateContinuousCanceledDamage.SetEnabled(false);
                stateReleasedBehavioralConstraintse.SetEnabled(false);
                stateReleaseNumStepToggle.SetEnabled(false);
                stateReleaseNumStep.SetEnabled(false);
                stateProbability.SetEnabled(false);
                stateCanceledDamage.SetEnabled(false);
                stateReleaseActionRestrictions.SetEnabled(false);
                stateContinuationNumTurnsMax.SetEnabled(false);
                stateContinuationNumTurnsMin.SetEnabled(false);
                stateOutCombatTiming.SetEnabled(false);
                stateEndBattleTime.SetEnabled(false);
                stateActionConstraints.SetEnabled(false);
                stateApplication.SetEnabled(false);
                stateSuperposition.SetEnabled(false);
                stateAnimation.SetEnabled(false);
                statePriority.SetEnabled(false);
                stateName.SetEnabled(false);
                statusContinuousNumStep.SetEnabled(false);
                stateAddedByAllyTiming.SetEnabled(false);
                stateAddedByEnemyTiming.SetEnabled(false);
                stateContinuedTiming.SetEnabled(false);
                stateCanceledTiming.SetEnabled(false);
            }

            //class_traits_new
            //class_traits_area
            VisualElement stateTraitsArea = RootContainer.Query<VisualElement>("state_traits_area");
            var traitWindow = new TraitsInspectorElement();
            stateTraitsArea.Add(traitWindow);
            if (_stateDataModel != null)
                traitWindow.Init(_stateDataModel.traits, TraitsType.TRAITS_TYPE_STATE, evt =>
                {
                    _stateDataModel.traits = (List<TraitCommonDataModel>) evt;
                    SaveData();
                });
        }

        private void SaveData() {
            databaseManagementService.SaveStateEdit(_stateDataModels);
        }

        private void _UpdateSceneView() {
            _ = Editor.Hierarchy.Hierarchy.Refresh(Region.StateEdit, _stateDataModel.id);
        }
    }
}