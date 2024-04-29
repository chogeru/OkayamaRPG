using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Picture
{
    /// <summary>
    ///     [ピクチャの消去]のコマンド設定枠の表示物
    /// </summary>
    public class PictureErase : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_picture_erase.uxml";

        private EventCommand _targetCommand;

        public PictureErase(
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
                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            // 番号
            {
                // 全マップから[ピクチャの表示]の番号を取得
                VisualElement number = RootElement.Q<VisualElement>("command_erasurePicture")
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
                    numIndex = 0;
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
        }
    }
}