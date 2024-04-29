using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Picture
{
    /// <summary>
    ///     [ピクチャの移動]のコマンド設定枠の表示物
    /// </summary>
    public class PictureMove : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_picture_move.uxml";

        private EventCommand _targetCommand;

        public PictureMove(
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
                for (var i = 1; i < 6; i++) _targetCommand.parameters.Add("0");
                _targetCommand.parameters.Add("100");
                _targetCommand.parameters.Add("100");
                _targetCommand.parameters.Add("255");
                _targetCommand.parameters.Add("0");
                _targetCommand.parameters.Add("60");
                _targetCommand.parameters.Add("0");


                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            // 番号
            {
                // 全マップから[ピクチャの表示]の番号を取得
                VisualElement number = RootElement.Q<VisualElement>("command_movePicture")
                    .Query<VisualElement>("number");
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

            // イージング
            var selectID = 0;
            VisualElement easing = RootElement.Q<VisualElement>("command_movePicture").Query<VisualElement>("easing");
            var easingList = EditorLocalize.LocalizeTexts(new List<string>
                {"WORD_1123", "WORD_1124", "WORD_1125", "WORD_1126"});
            selectID = int.Parse(_targetCommand.parameters[1]);
            if (selectID == -1)
                selectID = 0;
            var easingPopupField = new PopupFieldBase<string>(easingList, selectID);
            easing.Clear();
            easing.Add(easingPopupField);
            easingPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[1] = easingPopupField.index.ToString();
                Save(EventDataModels[EventIndex]);
            });

            RadioButton startingPoint_upLeft = RootElement.Q<VisualElement>("command_movePicture")
                .Query<RadioButton>("radioButton-eventCommand-display136");
            RadioButton startingPoint_center = RootElement.Q<VisualElement>("command_movePicture")
                .Query<RadioButton>("radioButton-eventCommand-display137");

            if (_targetCommand.parameters[2] == "0")
                startingPoint_upLeft.value = true;
            else
                startingPoint_center.value = true;
            
            var defaultPoint = _targetCommand.parameters[2] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {startingPoint_upLeft, startingPoint_center},
                defaultPoint, new List<System.Action>
                {
                    //左上
                    () =>
                    {
                        _targetCommand.parameters[2] = "0";
                        Save(EventDataModels[EventIndex]);
                    },
                    //中央
                    () =>
                    {
                        _targetCommand.parameters[2] = "1";
                        Save(EventDataModels[EventIndex]);
                    }
                });

            

            var flagDataModel =
                DatabaseManagementService.LoadFlags();
            var variableDropdownChoices = new List<string>();
            var variableNameDropdownChoices = new List<string>();
            for (var i = 0; i < flagDataModel.variables.Count; i++)
            {
                variableDropdownChoices.Add(flagDataModel.variables[i].id);
                if (flagDataModel.variables[i].name == "")
                    variableNameDropdownChoices.Add(EditorLocalize.LocalizeText("WORD_1518"));
                else
                    variableNameDropdownChoices.Add(flagDataModel.variables[i].name);
            }

            RadioButton direct_toggle = RootElement.Q<VisualElement>("command_movePicture").Query<RadioButton>("radioButton-eventCommand-display138");
            RadioButton variable_toggle = RootElement.Q<VisualElement>("command_movePicture").Query<RadioButton>("radioButton-eventCommand-display139");
            IntegerField x_num = RootElement.Query<IntegerField>("x_num");
            IntegerField y_num = RootElement.Query<IntegerField>("y_num");
            VisualElement x_variable = RootElement.Q<VisualElement>("command_movePicture").Query<VisualElement>("x_variable");
            VisualElement y_variable = RootElement.Q<VisualElement>("command_movePicture").Query<VisualElement>("y_variable");

            if (_targetCommand.parameters[3] == "0")
                x_num.value = int.Parse(_targetCommand.parameters[4]);
            x_num.RegisterCallback<FocusOutEvent>(evt =>
            {
                _targetCommand.parameters[4] = x_num.value.ToString();
                Save(EventDataModels[EventIndex]);
            });
            if (_targetCommand.parameters[3] == "0")
                y_num.value = int.Parse(_targetCommand.parameters[5]);
            y_num.RegisterCallback<FocusOutEvent>(evt =>
            {
                _targetCommand.parameters[5] = y_num.value.ToString();
                Save(EventDataModels[EventIndex]);
            });


            if (_targetCommand.parameters[3] == "1")
                selectID = variableDropdownChoices.IndexOf(_targetCommand.parameters[4]);
            if (selectID == -1)
                selectID = 0;
            var x_variablePopupField = new PopupFieldBase<string>(variableNameDropdownChoices, selectID);
            x_variable.Clear();
            x_variable.Add(x_variablePopupField);
            x_variablePopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[4] = variableDropdownChoices[x_variablePopupField.index];
                Save(EventDataModels[EventIndex]);
            });

            if (_targetCommand.parameters[3] == "1")
                selectID = variableDropdownChoices.IndexOf(_targetCommand.parameters[5]);
            if (selectID == -1)
                selectID = 0;
            var y_variablePopupField = new PopupFieldBase<string>(variableNameDropdownChoices, selectID);
            y_variable.Clear();
            y_variable.Add(y_variablePopupField);
            y_variablePopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[5] = variableDropdownChoices[y_variablePopupField.index];
                Save(EventDataModels[EventIndex]);
            });

            if (_targetCommand.parameters[3] == "0")
            {
                direct_toggle.value = true;
                x_num.SetEnabled(true);
                y_num.SetEnabled(true);
                x_variable.SetEnabled(false);
                y_variable.SetEnabled(false);
            }
            else
            {
                variable_toggle.value = true;
                x_num.SetEnabled(false);
                y_num.SetEnabled(false);
                x_variable.SetEnabled(true);
                y_variable.SetEnabled(true);
            }
            
            var defaultSelect = _targetCommand.parameters[3] == "0" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {direct_toggle, variable_toggle},
                defaultSelect, new List<System.Action>
                {
                    //直接指定
                    () =>
                    {
                        _targetCommand.parameters[3] = "0";
                        _targetCommand.parameters[4] = x_num.value.ToString();
                        _targetCommand.parameters[5] = y_num.value.ToString();
                        Save(EventDataModels[EventIndex]);
                        x_num.SetEnabled(true);
                        y_num.SetEnabled(true);
                        x_variable.SetEnabled(false);
                        y_variable.SetEnabled(false);
                    },
                    //変数で指定
                    () =>
                    {
                        _targetCommand.parameters[3] = "1";
                        _targetCommand.parameters[4] = variableDropdownChoices[x_variablePopupField.index];
                        _targetCommand.parameters[5] = variableDropdownChoices[y_variablePopupField.index];
                        Save(EventDataModels[EventIndex]);
                        x_num.SetEnabled(false);
                        y_num.SetEnabled(false);
                        x_variable.SetEnabled(true);
                        y_variable.SetEnabled(true);
                    }
                });
            

            IntegerField width = RootElement.Query<IntegerField>("width");
            IntegerField height = RootElement.Query<IntegerField>("height");
            if (_targetCommand.parameters[6] != null)
                width.value =
                    int.Parse(_targetCommand.parameters[6]);

            if (width.value < 10)
            {
                width.value = 10;
                _targetCommand.parameters[6] = width.value.ToString();
                Save(EventDataModels[EventIndex]);
            }
            else if (width.value > 400)
            {
                width.value = 400;
                _targetCommand.parameters[6] = width.value.ToString();
                Save(EventDataModels[EventIndex]);
            }

            BaseInputFieldHandler.IntegerFieldCallback(width, evt =>
            {
                _targetCommand.parameters[6] = width.value.ToString();
                Save(EventDataModels[EventIndex]);
            }, 10, 400);
            
            if (_targetCommand.parameters[7] != null)
                height.value = int.Parse(_targetCommand.parameters[7]);
            
            if (height.value < 10)
            {
                height.value = 10;
                _targetCommand.parameters[7] = height.value.ToString();
                Save(EventDataModels[EventIndex]);
                
            }
            else if (height.value > 400)
            {
                height.value = 400;
                _targetCommand.parameters[7] = height.value.ToString();
                Save(EventDataModels[EventIndex]);
            }
            
            BaseInputFieldHandler.IntegerFieldCallback(height, evt =>
            {
                _targetCommand.parameters[7] = height.value.ToString();
                Save(EventDataModels[EventIndex]);
            }, 10, 400);


            IntegerField opacity = RootElement.Query<IntegerField>("opacity");
            VisualElement synthetic =
                RootElement.Q<VisualElement>("command_movePicture").Query<VisualElement>("synthetic");
            if (_targetCommand.parameters[8] != null)
                opacity.value = int.Parse(_targetCommand.parameters[8]);
            opacity.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (opacity.value > 255)
                    opacity.value = 255;
                else if (opacity.value < 0)
                    opacity.value = 0;
                _targetCommand.parameters[8] = opacity.value.ToString();
                Save(EventDataModels[EventIndex]);
            });

            var syntheticList = EditorLocalize.LocalizeTexts(new List<string>
                {"WORD_0548", "WORD_0976", "WORD_0977", "WORD_0978"});
            if (_targetCommand.parameters[9] != null)
                selectID = syntheticList.IndexOf(_targetCommand.parameters[9]);
            if (selectID == -1)
                selectID = 0;
            var syntheticPopupField = new PopupFieldBase<string>(syntheticList, selectID);
            synthetic.Clear();
            synthetic.Add(syntheticPopupField);
            syntheticPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[9] =
                    syntheticPopupField.index.ToString();
                Save(EventDataModels[EventIndex]);
            });

            IntegerField flame = RootElement.Query<IntegerField>("flame");
            if (_targetCommand.parameters[10] != null)
                flame.value = int.Parse(_targetCommand.parameters[10]);

            BaseInputFieldHandler.IntegerFieldCallback(flame, evt =>
            {
                _targetCommand.parameters[10] = flame.value.ToString();
                Save(EventDataModels[EventIndex]);
            }, 0, 999);

            Toggle wait_toggle = RootElement.Q<VisualElement>("command_movePicture").Query<Toggle>("wait_toggle");
            if (_targetCommand.parameters[11] == "1") wait_toggle.value = true;

            wait_toggle.RegisterValueChangedCallback(o =>
            {
                var num = 0;
                if (wait_toggle.value)
                    num = 1;
                _targetCommand.parameters[11] = num.ToString();
                Save(EventDataModels[EventIndex]);
            });
        }
    }
}