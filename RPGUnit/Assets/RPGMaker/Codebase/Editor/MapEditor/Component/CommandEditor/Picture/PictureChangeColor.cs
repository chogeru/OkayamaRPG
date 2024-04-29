using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Picture
{
    /// <summary>
    ///     [ピクチャの色調変更]のコマンド設定枠の表示物
    /// </summary>
    public class PictureChangeColor : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_picture_change_color.uxml";

        private EventCommand _targetCommand;

        public PictureChangeColor(
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
                _targetCommand.parameters.Add("-1");
                _targetCommand.parameters.Add("0");
                _targetCommand.parameters.Add("0");
                _targetCommand.parameters.Add("0");
                _targetCommand.parameters.Add("0");
                _targetCommand.parameters.Add("0");
                _targetCommand.parameters.Add("255");
                _targetCommand.parameters.Add("0");
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            // 番号
            {
                // 全マップから[ピクチャの表示]の番号を取得
                VisualElement number = RootElement.Query<VisualElement>("number");
                var pictureNumList = new List<string>();
                foreach (var dataModel in EventDataModels)
                {
                    foreach (var command in dataModel.eventCommands)
                    {
                        if (command.code == (int) EventEnum.EVENT_CODE_PICTURE_SHOW)
                        {
                            if (!pictureNumList.Contains(command.parameters[0]))
                            {
                                pictureNumList.Add(command.parameters[0]);
                            }
                        }

                    }
                }
                pictureNumList.Sort((a, b) => int.Parse(a) - int.Parse(b));
                pictureNumList.Insert(0,EditorLocalize.LocalizeText("WORD_0113"));

                var numIndex = -1;
                if (!string.IsNullOrEmpty(_targetCommand.parameters[0]))
                    numIndex = pictureNumList.IndexOf(_targetCommand.parameters[0]);
                // どの番号とも一致しない場合は一旦「なし」を格納する
                if (numIndex == -1)
                {
                    numIndex = pictureNumList.Count == 1 ? 0 : 1;
                    _targetCommand.parameters[0] = pictureNumList[numIndex];
                }

                var pictureNumPopupField = new PopupFieldBase<string>(pictureNumList, numIndex);
                number.Clear();
                number.Add(pictureNumPopupField);
                pictureNumPopupField.RegisterValueChangedCallback(evt =>
                {
                    _targetCommand.parameters[0] = pictureNumPopupField.value;
                    Save(EventDataModels[EventIndex]);
                });
            }

            var selectID = 0;
            ColorFieldBase colorPicker = RootElement.Query<ColorFieldBase>("colorPicker");
            VisualElement template = RootElement.Query<VisualElement>("template");

            var syntheticList =
                EditorLocalize.LocalizeTexts(new List<string>
                    {"WORD_3031", "WORD_1132", "WORD_1133", "WORD_1134", "WORD_1135"});
            if (_targetCommand.parameters[1] != null)
                selectID = int.Parse(_targetCommand.parameters[1]);
            if (selectID == -1)
                selectID = 0;
            var syntheticPopupField = new PopupFieldBase<string>(syntheticList, selectID);
            template.Clear();
            template.Add(syntheticPopupField);
            syntheticPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[1] =
                    syntheticPopupField.index.ToString();
                switch (syntheticList.IndexOf(syntheticPopupField.value))
                {
                    case 0:
                        _targetCommand.parameters[2] = "0";
                        _targetCommand.parameters[3] = "0";
                        _targetCommand.parameters[4] = "0";
                        _targetCommand.parameters[5] = "0";
                        break;
                    case 1:
                        _targetCommand.parameters[2] = "187";
                        _targetCommand.parameters[3] = "187";
                        _targetCommand.parameters[4] = "187";
                        _targetCommand.parameters[5] = "170";
                        break;
                    case 2:
                        _targetCommand.parameters[2] = "255";
                        _targetCommand.parameters[3] = "221";
                        _targetCommand.parameters[4] = "187";
                        _targetCommand.parameters[5] = "170";
                        break;
                    case 3:
                        _targetCommand.parameters[2] = "255";
                        _targetCommand.parameters[3] = "221";
                        _targetCommand.parameters[4] = "221";
                        _targetCommand.parameters[5] = "170";
                        break;
                    case 4:
                        _targetCommand.parameters[2] = "187";
                        _targetCommand.parameters[3] = "187";
                        _targetCommand.parameters[4] = "255";
                        _targetCommand.parameters[5] = "170";
                        break;
                }

                Save(EventDataModels[EventIndex]);
                var intR = int.Parse(_targetCommand.parameters[2]);
                var intG = int.Parse(_targetCommand.parameters[3]);
                var intB = int.Parse(_targetCommand.parameters[4]);
                var intA = int.Parse(_targetCommand.parameters[5]);
                colorPicker.value = new Color32((byte) intR, (byte) intG, (byte) intB, (byte) intA);
            });

            var r = int.Parse(_targetCommand.parameters[2]);
            var g = int.Parse(_targetCommand.parameters[3]);
            var b = int.Parse(_targetCommand.parameters[4]);
            var a = int.Parse(_targetCommand.parameters[5]);
            colorPicker.value = new Color32((byte) r, (byte) g, (byte) b, (byte) a);
            colorPicker.RegisterValueChangedCallback(evt =>
            {
                Color32 co = colorPicker.value;
                var coR = co.r;
                var coG = co.g;
                var coB = co.b;
                var alpha = co.a;
                _targetCommand.parameters[2] = coR.ToString(CultureInfo.InvariantCulture);
                _targetCommand.parameters[3] = coG.ToString(CultureInfo.InvariantCulture);
                _targetCommand.parameters[4] = coB.ToString(CultureInfo.InvariantCulture);
                _targetCommand.parameters[5] = alpha.ToString(CultureInfo.InvariantCulture);

                Save(EventDataModels[EventIndex]);
            });

            IntegerField flame = RootElement
                .Query<IntegerField>("flame");
            if (_targetCommand.parameters[6] != null)
                flame.value = int.Parse(_targetCommand.parameters[6]);
            flame.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (flame.value < 0)
                    flame.value = 0;
                else if (flame.value > 999)
                    flame.value = 999;
                _targetCommand.parameters[6] = flame.value.ToString();
                Save(EventDataModels[EventIndex]);
            });

            Toggle wait_toggle = RootElement.Query<Toggle>("wait_toggle");
            if (_targetCommand.parameters[7] == "1") wait_toggle.value = true;

            wait_toggle.RegisterValueChangedCallback(o =>
            {
                var num = 0;
                if (wait_toggle.value)
                    num = 1;
                _targetCommand.parameters[7] = num.ToString();
                Save(EventDataModels[EventIndex]);
            });
        }
    }
}