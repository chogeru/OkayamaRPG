using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.GameProgress
{
    public class GameSwitch : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_switch_control.uxml";

        //スイッチのリスト
        private List<FlagDataModel.Switch> _switches;


        public GameSwitch(
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

            _switches = _GetSwitchList();


            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count == 0)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add(_switches[0].id);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("1");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }


            RadioButton sole_toggle = RootElement.Q<VisualElement>("command_switchControl").Query<RadioButton>("radioButton-eventCommand-display87");
            RadioButton range_toggle = RootElement.Q<VisualElement>("command_switchControl").Query<RadioButton>("radioButton-eventCommand-display88");
            VisualElement sole = RootElement.Q<VisualElement>("command_switchControl").Query<VisualElement>("sole");
            IntegerField range_min = RootElement.Query<IntegerField>("range_min");
            IntegerField range_max = RootElement.Query<IntegerField>("range_max");

            var switchNameList = new List<string>();
            var switchIDList = new List<string>();
            var selectID = 0;
            for (var i = 0; i < _switches.Count; i++)
            {
                if (_switches[i].name == "")
                    switchNameList.Add(EditorLocalize.LocalizeText("WORD_1518"));
                else
                    switchNameList.Add(_switches[i].name);

                switchIDList.Add(_switches[i].id);
            }

            selectID = switchIDList.IndexOf(EventDataModels[EventIndex].eventCommands[EventCommandIndex]
                .parameters[1]);
            if (selectID == -1) selectID = 0;
            var solePopupField = new PopupFieldBase<string>(switchNameList, selectID);
            sole.Clear();
            sole.Add(solePopupField);
            solePopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "0";
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    switchIDList[solePopupField.index];
                Save(EventDataModels[EventIndex]);
            });


            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] == "0")
            {
                sole_toggle.value = true;
                range_toggle.value = false;
                sole.SetEnabled(true);
                range_min.SetEnabled(false);
                range_max.SetEnabled(false);
                range_max.value =
                    int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2]);
            }
            else
            {
                sole_toggle.value = false;
                range_toggle.value = true;
                sole.SetEnabled(false);
                range_min.SetEnabled(true);
                range_max.SetEnabled(true);
                range_min.value =
                    int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]);
            }
            
            var defaultSelect = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {sole_toggle, range_toggle},
                defaultSelect, new List<System.Action>
                {
                    //単独
                    () =>
                    {
                        sole.SetEnabled(true);
                        range_min.SetEnabled(false);
                        range_max.SetEnabled(false);
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "0";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                            switchIDList[solePopupField.index];
                        Save(EventDataModels[EventIndex]);
                    },
                    //範囲
                    () =>
                    {
                        sole.SetEnabled(false);
                        range_min.SetEnabled(true);
                        range_max.SetEnabled(true);
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "1";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = range_min.value.ToString();
                        Save(EventDataModels[EventIndex]);
                    }
                });

            var paramsSwitch = 0;
            try
            {
                paramsSwitch = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0]);
            }
            catch (Exception)
            {
            }


            if (paramsSwitch >= 1)
                range_min.value =
                    int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]);

            range_min.RegisterCallback<FocusOutEvent>(o =>
            {
                //スイッチのデータの更新
                _switches = _GetSwitchList();

                if (range_min.value > _switches.Count)
                    range_min.value = _switches.Count;
                else if (range_min.value < 1)
                    range_min.value = 1;

                //最大値より大きい値が入力できないようにする
                if (range_min.value >
                    int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2]))
                    range_min.value =
                        int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2]);

                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "1";
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = range_min.value.ToString();
                Save(EventDataModels[EventIndex]);
            });

            if (paramsSwitch >= 1)
                range_max.value =
                    int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2]);

            range_max.RegisterCallback<FocusOutEvent>(o =>
            {
                //スイッチのデータの更新
                _switches = _GetSwitchList();

                if (range_max.value > _switches.Count)
                    range_max.value = _switches.Count;
                else if (range_max.value < 1)
                    range_max.value = 1;

                //最小値より小さい値が入力できないようにする
                if (range_max.value <
                    int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]))
                    range_max.value =
                        int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]);

                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "1";
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] = range_max.value.ToString();
                Save(EventDataModels[EventIndex]);
            });

            RadioButton control_on = RootElement.Q<VisualElement>("command_switchControl").Query<RadioButton>("radioButton-eventCommand-display89");
            RadioButton control_off = RootElement.Q<VisualElement>("command_switchControl").Query<RadioButton>("radioButton-eventCommand-display90");
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] == "0")
            {
                control_on.value = true;
                control_off.value = false;
            }
            else
            {
                control_on.value = false;
                control_off.value = true;
            }
            
            var defaultControl = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {control_on, control_off},
                defaultControl, new List<System.Action>
                {
                    //ON
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] = "0";
                        Save(EventDataModels[EventIndex]);
                    },
                    //OFF
                    () =>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] = "1";
                        Save(EventDataModels[EventIndex]);
                    }
                });

            
        }

        private List<FlagDataModel.Switch> _GetSwitchList() {
            var flagDataModel =
                DatabaseManagementService.LoadFlags();
            var fileNames = new List<FlagDataModel.Switch>();
            for (var i = 0; i < flagDataModel.switches.Count; i++) fileNames.Add(flagDataModel.switches[i]);

            return fileNames;
        }
    }
}