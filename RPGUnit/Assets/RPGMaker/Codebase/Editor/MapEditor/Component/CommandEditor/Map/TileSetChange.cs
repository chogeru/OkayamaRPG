using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Map
{
    public class TileSetChange : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_tileset_change.uxml";

        private readonly string assetPath = "Assets/RPGMaker/Storage/Map/TileImages/";

        public TileSetChange(
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

            var dir = new DirectoryInfo(assetPath);
            var info = dir.GetFiles("*.png");
            var tileSets = new List<string>();
            foreach (var f in info)
                tileSets.Add(f.Name.Replace(".png", ""));
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count == 0)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add(tileSets[0]);
                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }


            VisualElement tileSet =
                RootElement.Q<VisualElement>("command_tileSetChange").Query<VisualElement>("tileSet");

            var selectID = tileSets.IndexOf(EventDataModels[EventIndex].eventCommands[EventCommandIndex]
                .parameters[0]);
            if (selectID == -1)
                selectID = 0;
            var tileSetPopupField = new PopupFieldBase<string>(tileSets, selectID);
            tileSet.Clear();
            tileSet.Add(tileSetPopupField);
            tileSetPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                    tileSetPopupField.value;
                Save(EventDataModels[EventIndex]);
            });
        }
    }
}