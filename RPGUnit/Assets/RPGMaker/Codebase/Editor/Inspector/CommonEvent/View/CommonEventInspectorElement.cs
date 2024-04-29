using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventCommon;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Enum;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.CommonEvent.View
{
    /// <summary>
    /// [コモンイベント] Inspector
    /// </summary>
    public class CommonEventInspectorElement : AbstractInspectorElement
    {
        private readonly EventCommonDataModel   _eventCommonDataModel;
        private          FlagDataModel          _flagDataModel;

        private readonly List<string> _triggerTextList =
            EditorLocalize.LocalizeTexts(new List<string> {"WORD_0113", "WORD_0877", "WORD_0878"});

        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/CommonEvent/Asset/inspector_CommonEvent.uxml"; } }

        public CommonEventInspectorElement(EventCommonDataModel eventCommonDataModel) {
            _eventCommonDataModel = eventCommonDataModel;
            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            _flagDataModel = databaseManagementService.LoadFlags();
            Initialize();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            //ID
            Label commonEventID = RootContainer.Query<Label>("common_event_id");
            commonEventID.text = _eventCommonDataModel.SerialNumberString;

            //名前
            ImTextField commonEventName = RootContainer.Query<ImTextField>("common_event_name");
            commonEventName.value = _eventCommonDataModel.name;
            commonEventName.RegisterCallback<FocusOutEvent>(o =>
            {
                _eventCommonDataModel.name = commonEventName.value;
                Save();
                _UpdateSceneView();
            });

            //条件
            var eventCommonCondition = _eventCommonDataModel.conditions[0];

            //トリガー
            VisualElement commonEventTrigger = RootContainer.Query<VisualElement>("common_event_trigger");
            var triggerTextDropdownChoices = _triggerTextList;
            var commonEventTriggerPopupField =
                new PopupFieldBase<string>(triggerTextDropdownChoices, eventCommonCondition.trigger);
            commonEventTrigger.Add(commonEventTriggerPopupField);
            commonEventTriggerPopupField.RegisterValueChangedCallback(evt =>
            {
                eventCommonCondition.trigger = triggerTextDropdownChoices.IndexOf(commonEventTriggerPopupField.value);
                _eventCommonDataModel.conditions[0] = eventCommonCondition;
                Save();
            });

            //変数
            VisualElement commonEventSwitch = RootContainer.Query<VisualElement>("common_event_switch");
            var switchDropdownChoices = SwitchList();
            var switchNum = -1;
            switchNum = IdToIndex(eventCommonCondition.switchId);
            if (switchNum == -1)
            {
                switchNum = 0;
                if (_flagDataModel.switches.Count > 0)
                {
                    eventCommonCondition.switchId = _flagDataModel.switches[0].id;
                    _eventCommonDataModel.conditions[0] = eventCommonCondition;
                }
            }

            var commonEventSwitchPopupField =
                new PopupFieldBase<string>(switchDropdownChoices, switchNum);
            commonEventSwitch.Add(commonEventSwitchPopupField);
            commonEventSwitchPopupField.RegisterValueChangedCallback(evt =>
            {
                eventCommonCondition.switchId =
                    _flagDataModel.switches[commonEventSwitchPopupField.index].id;
                _eventCommonDataModel.conditions[0] = eventCommonCondition;
                Save();
            });
        }

        //スイッチのリスト取得
        private List<string> SwitchList() {
            var returnList = new List<string>();
            var ct = 1;
            foreach (var switchesValue in _flagDataModel.switches)
            {
                returnList.Add(switchesValue.name);
                ct++;
            }

            return returnList;
        }

        //スイッチのIDを今のIndexに変換する
        private int IdToIndex(string id) {
            var returmIndex = 0;
            for (var i = 0; i < _flagDataModel.switches.Count; i++)
                if (_flagDataModel.switches[i].id == id)
                    returmIndex = i;

            return returmIndex;
        }

        //セーブ
        override protected void SaveContents() {
            eventManagementService.SaveEventCommon(_eventCommonDataModel);
        }

        private void _UpdateSceneView() {
            _ = Editor.Hierarchy.Hierarchy.Refresh(Region.CommonEvent, _eventCommonDataModel.eventId);
        }
    }
}