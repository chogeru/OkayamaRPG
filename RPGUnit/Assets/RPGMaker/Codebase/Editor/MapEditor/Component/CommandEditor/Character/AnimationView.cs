using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventBattle;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Inspector.Animation.View;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Character
{
    public class AnimationView : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_animation_view.uxml";

        private GenericPopupFieldBase<TargetCharacterChoice> _targetCharacterPopupField;

        public AnimationView(
            VisualElement rootElement,
            List<EventDataModel> eventDataModels,
            int eventIndex,
            int eventCommandIndex
        )
            : base(rootElement, eventDataModels, eventIndex, eventCommandIndex) {
        }

        public override void Invoke()
        {
            var commandTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SettingUxml);
            VisualElement commandFromUxml = commandTree.CloneTree();
            EditorLocalize.LocalizeElements(commandFromUxml);
            commandFromUxml.style.flexGrow = 1;
            RootElement.Add(commandFromUxml);

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            // パラメータ。

            var animationDataModels = DatabaseManagementService.LoadAnimation();

            if (EventCommand.parameters.Count == 0)
            {
                EventCommand.parameters.Add("-2");
                EventCommand.parameters.Add(animationDataModels[0].id);
                EventCommand.parameters.Add("0");
                EventManagementService.SaveEvent(EventDataModel);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            // コモンイベント用の仮マップの『マップ選択』PopupFieldの追加もしくは追加先項目UIを非表示と
            // 『キャラクター』PopupFieldの追加。
            {
                int targetCharacterParameterIndex = 0;
                AddOrHideProvisionalMapAndAddTargetCharacterPopupField(
                    targetCharacterParameterIndex,
                    provisionalMapPopupField =>
                    {
                        _targetCharacterPopupField = AddTargetCharacterPopupField(
                            QRoot("character"),
                            targetCharacterParameterIndex,
                            forceMapId: provisionalMapPopupField?.value.MapDataModel?.id);
                    });
            }

            // アニメーション。

            var animationChaices =
                animationDataModels.Select(animationDataModel => animationDataModel.particleName).ToList();
            if (animationDataModels.Count == 0)
                animationChaices.Add("アニメーションがありません");

            var animationDataModel = animationDataModels.SingleOrDefault(animationDataModel =>
                animationDataModel.id == EventCommand.parameters[1]);
            var animationChaiceIndex = Math.Max(animationDataModels.IndexOf(animationDataModel), -1);

            //バトルイベントかどうかで、処理を振り分ける
            EventManagementService eventManagementService = new EventManagementService();
            List<EventBattleDataModel> eventBattleDataModels = eventManagementService.LoadEventBattle();
            EventBattleDataModel eventBattleDataModel = null;
            for (int i = 0; i < eventBattleDataModels.Count; i++)
                if (eventBattleDataModels[i].eventId == EventDataModels[EventIndex].id)
                {
                    eventBattleDataModel = eventBattleDataModels[i];
                    break;
                }

            VisualElement animationVe =
                RootElement.Q<VisualElement>("command_animationView").Query<VisualElement>("animation");

            bool isNone = false;

            if (animationChaiceIndex == -1)
            {
                animationChaiceIndex = 0;
                isNone = true;
            }

            var animationPopupField = new PopupFieldBase<string>(animationChaices, animationChaiceIndex);
            animationVe.Clear();
            animationVe.Add(animationPopupField);
            animationPopupField.RegisterValueChangedCallback(evt =>
            {
                EventCommand.parameters[1] = animationDataModels[animationPopupField.index].id;
                Save(EventDataModel);

                ShowAnimationView(animationPopupField.index, eventBattleDataModel != null ? true : false);
            });

            //設定しているデータがなかった場合、「なし」を表示させる
            if (isNone)
            {
                animationPopupField.ChangeButtonText(EditorLocalize.LocalizeText("WORD_0113"));
            }


            ShowAnimationView(animationChaiceIndex, eventBattleDataModel != null ? true : false);

            // 完了までウェイト。

            Toggle weightToggle = RootElement.Q<VisualElement>("command_animationView").Query<Toggle>("weight_toggle");
            weightToggle.value = EventCommand.parameters[2] != "0";
            weightToggle.RegisterValueChangedCallback(evt =>
            {
                EventCommand.parameters[2] = weightToggle.value ? "1" : "0";
                Save(EventDataModel);
            });

            void ShowAnimationView(int animationIndex, bool isBattle)
            {
                if (animationDataModels.Count == 0)
                    return;

                AnimationInspectorElement.ShowAnimationPreview(
                    animationDataModels[animationIndex]);
            }
        }
    }
}