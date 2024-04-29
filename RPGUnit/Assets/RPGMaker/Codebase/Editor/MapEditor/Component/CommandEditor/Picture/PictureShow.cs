using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Picture
{
    /// <summary>
    ///     [ピクチャの表示]のコマンド設定枠の表示物
    /// </summary>
    public class PictureShow : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_picture_show.uxml";

        private EventCommand _targetCommand;

        public PictureShow(
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

            VisualElement image = RootElement.Query<VisualElement>("image");
            var dir = new DirectoryInfo(PathManager.IMAGE_ADV);
            var info = dir.GetFiles("*.png");
            var fileNames = new List<string>();
            foreach (var f in info)
                fileNames.Add(f.Name.Replace(".png", ""));

            _targetCommand = EventDataModels[EventIndex].eventCommands[EventCommandIndex];
            if (_targetCommand.parameters.Count == 0)
            {
                _targetCommand.parameters.Add("1");
                _targetCommand.parameters.Add(fileNames[0]);
                for (var i = 2; i < 6; i++)
                    _targetCommand.parameters.Add("0");

                _targetCommand.parameters.Add("100");
                _targetCommand.parameters.Add("100");
                _targetCommand.parameters.Add("255");
                _targetCommand.parameters.Add("0");

                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            IntegerField number = RootElement.Query<IntegerField>("number");
            try
            {
                number.value = int.Parse(_targetCommand.parameters[0]);
            }
            catch
            {
                number.value = 1;
            }
            
            BaseInputFieldHandler.IntegerFieldCallback(number, evt =>
            {
                _targetCommand.parameters[0] = number.value.ToString();
                Save(EventDataModels[EventIndex]);
            },1,100);
            
            // 画像設定
            //------------------------------------------------------------------------------------------------------------------------------            
            // プレビュー画像
            Image previewImage1 = RootElement.Query<Image>("picture_image");
            previewImage1.scaleMode = ScaleMode.ScaleToFit;
            previewImage1.image = AssetDatabase.LoadAssetAtPath<Texture2D>
                (PathManager.IMAGE_ADV + _targetCommand.parameters[1] + ".png");

            // 画像名
            Label imageNameLabel1 = RootElement.Query<Label>("picture_image_name");
            imageNameLabel1.text = _targetCommand.parameters[1];

            // 画像変更ボタン
            Button changeButton1 = RootElement.Query<Button>("picture_image_change");
            changeButton1.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.IMAGE_ADV, true);
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    _targetCommand.parameters[1] = imageName;
                    imageNameLabel1.text = imageName;
                    previewImage1.image = AssetDatabase.LoadAssetAtPath<Texture2D>
                        (PathManager.IMAGE_ADV + _targetCommand.parameters[1] + ".png");

                    Save(EventDataModels[EventIndex]);
                }, _targetCommand.parameters[1]);
            };

            // 画像インポート
            Button importButton1 = RootElement.Query<Button>("picture_image_import");
            importButton1.clicked += () => 
            { 
                var path = AssetManageImporter.StartToFile("png", PathManager.IMAGE_ADV); 
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    _targetCommand.parameters[1] = path;
                    imageNameLabel1.text = path;
                    previewImage1.image = AssetDatabase.LoadAssetAtPath<Texture2D>
                        (PathManager.IMAGE_ADV + _targetCommand.parameters[1] + ".png");

                    Save(EventDataModels[EventIndex]);
                }
            };


            RadioButton startingPoint_upLeft = RootElement.Query<RadioButton>("radioButton-eventCommand-display132");
            RadioButton startingPoint_center = RootElement.Query<RadioButton>("radioButton-eventCommand-display133");

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

            RadioButton direct_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display134");
            RadioButton variable_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display135");
            IntegerField x_num = RootElement.Query<IntegerField>("x_num");
            IntegerField y_num = RootElement.Query<IntegerField>("y_num");
            VisualElement x_variable = RootElement.Query<VisualElement>("x_variable");
            VisualElement y_variable = RootElement.Query<VisualElement>("y_variable");

            var selectID = 0;
            if (_targetCommand.parameters[4] != null)
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

            if (_targetCommand.parameters[5] != null)
                selectID = variableDropdownChoices.IndexOf(_targetCommand.parameters[5]);
            if (selectID == -1)
                selectID = 0;
            var y_variablePopupField = new PopupFieldBase<string>(variableNameDropdownChoices, selectID);
            y_variable.Clear();
            y_variable.Add(y_variablePopupField);
            y_variablePopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[5] =
                    variableDropdownChoices[y_variablePopupField.index];
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

            //幅、高さ
            IntegerField width = RootElement.Query<IntegerField>("width");
            IntegerField height = RootElement.Query<IntegerField>("height");
            if (_targetCommand.parameters[6] != null)
                width.value = int.Parse(_targetCommand.parameters[6]);
            
            BaseInputFieldHandler.IntegerFieldCallback(width, evt =>
            {
                _targetCommand.parameters[6] = width.value.ToString();
                Save(EventDataModels[EventIndex]);
            }, 10, 400);

            if (_targetCommand.parameters[7] != null)
                height.value = int.Parse(_targetCommand.parameters[7]);
            
            BaseInputFieldHandler.IntegerFieldCallback(height, evt =>
            {
                _targetCommand.parameters[7] = height.value.ToString();
                Save(EventDataModels[EventIndex]);
            }, 10, 400);

            //透明度
            IntegerField opacity = RootElement.Query<IntegerField>("opacity");
            VisualElement synthetic = RootElement.Query<VisualElement>("synthetic");
            var syntheticList = EditorLocalize.LocalizeTexts(new List<string> {"WORD_0548", "WORD_0976", "WORD_0977", "WORD_0978"});
            if (_targetCommand.parameters[8] != null)
                opacity.value = int.Parse(_targetCommand.parameters[8]);

            BaseInputFieldHandler.IntegerFieldCallback(opacity, evt =>
            {
                _targetCommand.parameters[8] = opacity.value.ToString();
                Save(EventDataModels[EventIndex]);
            }, 0, 255);
            
            if (_targetCommand.parameters[9] != null)
                selectID = int.Parse(_targetCommand.parameters[9]);
            if (selectID == -1)
                selectID = 0;
            var syntheticPopupField = new PopupFieldBase<string>(syntheticList, selectID);
            synthetic.Clear();
            synthetic.Add(syntheticPopupField);
            syntheticPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[9] = syntheticPopupField.index.ToString();
                Save(EventDataModels[EventIndex]);
            });
        }
    }
}