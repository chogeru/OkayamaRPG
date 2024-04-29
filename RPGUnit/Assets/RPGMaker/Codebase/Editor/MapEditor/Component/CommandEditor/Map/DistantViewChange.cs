using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Map
{
    /// <summary>
    ///     [遠景の変更]の実行内容枠の表示物
    /// </summary>
    public class DistantViewChange : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_distant_view_change.uxml";

        private EventCommand _targetCommand;

        private string imageName;

        public DistantViewChange(
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

            var dir = new DirectoryInfo(PathManager.MAP_PARALLAX);
            var info = dir.GetFiles("*.png");
            var fileNames = new List<string>();
            List<string> fileNameChoices;
            foreach (var f in info)
                fileNames.Add(f.Name.Replace(".png", ""));
            fileNameChoices = new List<string>(fileNames);
            if (fileNames.Count == 0)
            {
                var fileName = EditorLocalize.LocalizeText("WORD_1612");
                fileNames.Add(fileName);
                fileNameChoices.Add(fileName);
            }

            _targetCommand = EventDataModels[EventIndex].eventCommands[EventCommandIndex];
            if (_targetCommand.parameters.Count == 0)
            {
                _targetCommand.parameters.Add(fileNames[0]);
                _targetCommand.parameters.Add("0");
                _targetCommand.parameters.Add("0");
                _targetCommand.parameters.Add("0");
                _targetCommand.parameters.Add("0");
                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            // 遠景画像設定
            //------------------------------------------------------------------------------------------------------------------------------            
            // プレビュー画像
            Image previewImage1 = RootElement.Query<Image>("distant_view_image");
            previewImage1.scaleMode = ScaleMode.ScaleToFit;
            previewImage1.image =
                AssetDatabase.LoadAssetAtPath<Texture2D>(PathManager.MAP_PARALLAX + _targetCommand.parameters[0] +
                                                         ".png");

            // 画像名
            Label imageNameLabel1 = RootElement.Query<Label>("distant_view_image_name");
            imageNameLabel1.text = _targetCommand.parameters[0];

            // 画像変更ボタン
            Button changeButton1 = RootElement.Query<Button>("distant_view_image_change");
            changeButton1.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.MAP_PARALLAX, true);
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    _targetCommand.parameters[0] = imageName;
                    imageNameLabel1.text = imageName;
                    previewImage1.image =
                        AssetDatabase.LoadAssetAtPath<Texture2D>(
                            PathManager.MAP_PARALLAX + _targetCommand.parameters[0] + ".png");
                    Save(EventDataModels[EventIndex]);
                }, _targetCommand.parameters[0]);
            };

            // 画像インポート
            Button importButton1 = RootElement.Query<Button>("distant_view_image_import");
            importButton1.clicked += () => 
            { 
                var path = AssetManageImporter.StartToFile("png", PathManager.MAP_PARALLAX);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    _targetCommand.parameters[0] = path;
                    imageNameLabel1.text = path;
                    previewImage1.image =
                        AssetDatabase.LoadAssetAtPath<Texture2D>(
                            PathManager.MAP_PARALLAX + _targetCommand.parameters[0] + ".png");
                    Save(EventDataModels[EventIndex]);
                }
            };

            Toggle height = RootElement.Q<VisualElement>("command_distantViewChange").Query<Toggle>("height");
            Toggle width = RootElement.Q<VisualElement>("command_distantViewChange").Query<Toggle>("width");
            IntegerField heightScrollNum = RootElement.Q<VisualElement>("command_distantViewChange")
                .Query<IntegerField>("heightScrollNum");
            IntegerField widthScrollNum = RootElement.Q<VisualElement>("command_distantViewChange")
                .Query<IntegerField>("widthScrollNum");

            height.value = _targetCommand.parameters[1] == "1";
            heightScrollNum.value = int.Parse(_targetCommand.parameters[3]);
            heightScrollNum.SetEnabled(height.value);

            width.value = _targetCommand.parameters[2] == "1";
            widthScrollNum.value = int.Parse(_targetCommand.parameters[4]);
            widthScrollNum.SetEnabled(width.value);

            height.RegisterValueChangedCallback(o =>
            {
                _targetCommand.parameters[1] = height.value ? "1" : "0";
                heightScrollNum.SetEnabled(height.value);
                Save(EventDataModels[EventIndex]);
            });

            width.RegisterValueChangedCallback(o =>
            {
                _targetCommand.parameters[2] = width.value ? "1" : "0";
                widthScrollNum.SetEnabled(width.value);
                Save(EventDataModels[EventIndex]);
            });

            heightScrollNum.RegisterCallback<FocusOutEvent>(e =>
            {
                heightScrollNum.value = Math.Min(Math.Max(heightScrollNum.value, -32), 32);
                _targetCommand.parameters[3] = heightScrollNum.value.ToString();
                Save(EventDataModels[EventIndex]);
            });

            widthScrollNum.RegisterCallback<FocusOutEvent>(e =>
            {
                widthScrollNum.value = Math.Min(Math.Max(widthScrollNum.value, -32), 32);
                _targetCommand.parameters[4] = widthScrollNum.value.ToString();
                Save(EventDataModels[EventIndex]);
            });
        }
    }
}