using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Character.Animation
{
    public class AnimationSetting : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_animation_setting.uxml";

        private GenericPopupFieldBase<TargetCharacterChoice> _targetCharacterPopupField;

        public AnimationSetting(
            VisualElement rootElement,
            List<EventDataModel> eventDataModels,
            int eventIndex,
            int eventCommandIndex
        )
            : base(rootElement, eventDataModels, eventIndex, eventCommandIndex) {
        }

        public override void Invoke() {
            var commandTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SettingUxml);
            VisualElement commandFromUxml = commandTree.CloneTree();
            EditorLocalize.LocalizeElements(commandFromUxml);
            commandFromUxml.style.flexGrow = 1;
            RootElement.Add(commandFromUxml);

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count == 0)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("-2");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            // キャラクター。
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

            VisualElement animation = RootElement.Query<VisualElement>("animation");
            var animationTextDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string> {"WORD_0968", "WORD_0969", "WORD_0970", "WORD_0971"});
            var num = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]);
            if (num == -1)
                num = 0;
            var animationPopupField = new PopupFieldBase<string>(animationTextDropdownChoices, num);
            animation.Clear();
            animation.Add(animationPopupField);
            animationPopupField.RegisterValueChangedCallback(evt =>
            {
                switch (animationTextDropdownChoices.IndexOf(animationPopupField.value))
                {
                    case 0:
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].code =
                            (int) EventMoveEnum.MOVEMENT_WALKING_ANIMATION_ON;
                        break;
                    case 1:
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].code =
                            (int) EventMoveEnum.MOVEMENT_WALKING_ANIMATION_OFF;
                        break;
                    case 2:
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].code =
                            (int) EventMoveEnum.MOVEMENT_STEPPING_ANIMATION_ON;
                        break;
                    case 3:
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].code =
                            (int) EventMoveEnum.MOVEMENT_STEPPING_ANIMATION_OFF;
                        break;
                }

                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    animationTextDropdownChoices.IndexOf(animationPopupField.value).ToString();
                Save(EventDataModels[EventIndex]);
            });
        }
    }
}