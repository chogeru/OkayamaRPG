using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.GameProgress
{
    /// <summary>
    ///     [セルフスイッチの操作]コマンドのコマンド設定枠の表示物
    /// </summary>
    public class GameSelfSwitch : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_self_switch_control.uxml";

        private EventCommand _targetCommand;

        public GameSelfSwitch(
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

            _targetCommand = EventDataModels[EventIndex].eventCommands[EventCommandIndex];
            if (_targetCommand.parameters.Count == 0)
            {
                _targetCommand.parameters.Add("A");
                _targetCommand.parameters.Add("1");
                _targetCommand.parameters.Add(EventDataModels[EventIndex].page.ToString());
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            VisualElement selfSwitch = RootElement.Q<VisualElement>("command_selfSwitchControl")
                .Query<VisualElement>("selfSwitch");
            RadioButton control_on = RootElement.Q<VisualElement>("command_selfSwitchControl").Query<RadioButton>("radioButton-eventCommand-display105");
            RadioButton control_off = RootElement.Q<VisualElement>("command_selfSwitchControl").Query<RadioButton>("radioButton-eventCommand-display106");

            if (_targetCommand.parameters[1] == "1")
                control_on.value = true;
            else
                control_off.value = true;
            
            var defaultControl = _targetCommand.parameters[1] == "1" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {control_on, control_off},
                defaultControl, new List<System.Action>
                {
                    //ON
                    () =>
                    {
                        _targetCommand.parameters[1] = "1";
                        Save(EventDataModels[EventIndex]);
                    },
                    //OFF
                    () =>
                    {
                        _targetCommand.parameters[1] = "0";
                        Save(EventDataModels[EventIndex]);
                    }
                });

            
            var selectID = 0;
            var selfSwitchList = new List<string> {"A", "B", "C", "D"};
            selectID = selfSwitchList.IndexOf(_targetCommand.parameters[0]);
            if (selectID == -1)
                selectID = 0;
            var otherListPopupField = new PopupFieldBase<string>(selfSwitchList, selectID);
            selfSwitch.Add(otherListPopupField);
            otherListPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[0] = otherListPopupField.value;
                Save(EventDataModels[EventIndex]);
            });
        }
    }
}