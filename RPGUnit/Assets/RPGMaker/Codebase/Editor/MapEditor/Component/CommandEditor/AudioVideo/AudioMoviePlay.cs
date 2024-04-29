using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.AudioVideo
{
    public class AudioMoviePlay : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_audio_movie_play.uxml";

        public AudioMoviePlay(
            VisualElement rootElement,
            List<EventDataModel> eventDataModels,
            int eventIndex,
            int eventCommandIndex
        )
            : base(rootElement, eventDataModels, eventIndex, eventCommandIndex) {
        }

        public override void Invoke() {
            RootElement.Clear();
            var commandTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SettingUxml);
            VisualElement commandFromUxml = commandTree.CloneTree();
            EditorLocalize.LocalizeElements(commandFromUxml);
            commandFromUxml.style.flexGrow = 1;
            RootElement.Add(commandFromUxml);

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            var fileNames = GetMovie();


            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count == 0)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add(fileNames[0]);
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }


            VisualElement menu = RootElement.Q<VisualElement>("command_moviePlay").Query<VisualElement>("menu");
            var selectID = fileNames.IndexOf(EventDataModels[EventIndex].eventCommands[EventCommandIndex]
                .parameters[0]);
            if (selectID == -1) selectID = 0;

            if (fileNames.Count == 0)
                fileNames.Add(EditorLocalize.LocalizeText("WORD_0113"));

            var menuPopupField = new PopupFieldBase<string>(fileNames, selectID);
            menu.Clear();
            menu.Add(menuPopupField);
            menuPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                    menuPopupField.value;
                Save(EventDataModels[EventIndex]);
            });

            Button import = RootElement.Q<VisualElement>("command_moviePlay").Query<Button>("import");
            import.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("mp4", PathManager.MOVIES);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = path;
                    Save(EventDataModels[EventIndex]);
                    Invoke();
                }
            };
        }

        private List<string> GetMovie() {
            var dir = new DirectoryInfo(PathManager.MOVIES);
            var info = dir.GetFiles("*.mp4");
            var fileNames = new List<string>()
            {
                EditorLocalize.LocalizeText("WORD_0113")
            };
            foreach (var f in info) fileNames.Add(f.Name.Replace(".mp4", ""));

            return fileNames;
        }
    }
}