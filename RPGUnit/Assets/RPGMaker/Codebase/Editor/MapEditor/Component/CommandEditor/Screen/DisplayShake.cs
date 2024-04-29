using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Screen
{
    /// <summary>
    ///     [画面のシェイク]コマンドのコマンド設定枠の表示物
    /// </summary>
    public class DisplayShake : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_display_shake.uxml";

        private EventCommand _targetCommand;

        public DisplayShake(
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
                _targetCommand.parameters.Add("1"); //シェイク
                _targetCommand.parameters.Add("1"); //速さ
                _targetCommand.parameters.Add("255"); //フレーム数
                _targetCommand.parameters.Add("0"); //完了までウェイト
                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }
            
            var strengthSliderArea = RootElement.Query<VisualElement>("strength_sliderArea");
            SliderAndFiledBase.IntegerSliderCallBack(strengthSliderArea, 1, 9, "",
                int.Parse( _targetCommand.parameters[0]), evt =>
                {
                    _targetCommand.parameters[0] =
                        ((int) evt).ToString();
                    Save(EventDataModels[EventIndex]);
                });
            
            var speedSliderArea = RootElement.Query<VisualElement>("speed_sliderArea");
            SliderAndFiledBase.IntegerSliderCallBack(speedSliderArea, 1, 9, "",
                int.Parse(_targetCommand.parameters[1]), evt =>
                {
                    _targetCommand.parameters[1] =
                        ((int) evt).ToString();
                    Save(EventDataModels[EventIndex]);
                });
            

            IntegerField flame = RootElement.Query<IntegerField>("flame");

            var frameValue = -1;
            if (int.TryParse(_targetCommand.parameters[2], out frameValue))
                flame.value = frameValue;
            
            BaseInputFieldHandler.IntegerFieldCallback(flame, evt =>
            {
                _targetCommand.parameters[2] = flame.value.ToString();
                Save(EventDataModels[EventIndex]);
            }, 1, 999);
            

            Toggle wait_toggle = RootElement.Query<Toggle>("wait_toggle");

            wait_toggle.value = _targetCommand.parameters[3] == "1";
            wait_toggle.RegisterValueChangedCallback(o =>
            {
                _targetCommand.parameters[3] = wait_toggle.value ? "1" : "0";
                Save(EventDataModels[EventIndex]);
            });
        }
    }
}