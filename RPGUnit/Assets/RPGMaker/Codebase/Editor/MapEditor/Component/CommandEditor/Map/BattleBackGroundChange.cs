using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Map
{
    public class BattleBackGroundChange : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_battle_bacground_change.uxml";

        public BattleBackGroundChange(
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

            var targetParameters = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters;

            if (targetParameters.Count == 0)
            {
                targetParameters.Add("");
                targetParameters.Add("");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters = targetParameters;
                Save(EventDataModels[EventIndex]);
            }

            // 背景画像（下）設定
            //------------------------------------------------------------------------------------------------------------------------------            

            Label imageNameLabelTitle1 = RootElement.Query<Label>("battle_scene_bg_top_image_title");
            imageNameLabelTitle1.text = "(" + EditorLocalize.LocalizeText("WORD_0299") + ")";

            
            // プレビュー画像
            Image previewImage1 = RootElement.Query<Image>("battle_scene_bg_top_image");
            previewImage1.scaleMode = ScaleMode.ScaleToFit;
            previewImage1.image = ImageManager.LoadBattleback1(targetParameters[0])?.texture;

            // 画像名
            Label imageNameLabel1 = RootElement.Query<Label>("battle_scene_bg_top_image_name");
            imageNameLabel1.text = ImageManager.GetBattlebackName(targetParameters[0], 1) + ".png";

            // 画像変更ボタン
            Button changeButton1 = RootElement.Query<Button>("battle_scene_bg_top_image_change");
            changeButton1.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.BATTLE_BACKGROUND_1, true);
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    targetParameters[0] = imageName;
                    imageNameLabel1.text = ImageManager.GetBattlebackName(imageName, 1) + ".png";
                    previewImage1.image = ImageManager.LoadBattleback1(targetParameters[0]).texture;
                    Save(EventDataModels[EventIndex]);
                }, targetParameters[0]);
            };

            // 背景画像インポート
            Button importButton1 = RootElement.Query<Button>("battle_scene_bg_top_image_import");
            importButton1.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.BATTLE_BACKGROUND_1);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    targetParameters[0] = path;
                    imageNameLabel1.text = ImageManager.GetBattlebackName(path, 1) + ".png";
                    previewImage1.image = ImageManager.LoadBattleback1(targetParameters[0]).texture;
                    Save(EventDataModels[EventIndex]);
                }
            };

            // 背景画像（上）設定
            //------------------------------------------------------------------------------------------------------------------------------            
            
            Label imageNameLabelTitle2 = RootElement.Query<Label>("battle_scene_bg_bottom_image_title");
            imageNameLabelTitle2.text = "(" + EditorLocalize.LocalizeText("WORD_0297") + ")";

            // プレビュー画像
            Image previewImage2 = RootElement.Query<Image>("battle_scene_bg_bottom_image");
            previewImage2.scaleMode = ScaleMode.ScaleToFit;
            previewImage2.image = ImageManager.LoadBattleback2(targetParameters[1])?.texture;

            // 画像名
            Label imageNameLabel2 = RootElement.Query<Label>("battle_scene_bg_bottom_image_name");
            imageNameLabel2.text = ImageManager.GetBattlebackName(targetParameters[1], 2) + ".png";

            // 画像変更ボタン
            Button changeButton2 = RootElement.Query<Button>("battle_scene_bg_bottom_image_change");
            changeButton2.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.BATTLE_BACKGROUND_2, true);
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    targetParameters[1] = imageName;
                    imageNameLabel2.text = ImageManager.GetBattlebackName(imageName, 2) + ".png";
                    previewImage2.image = ImageManager.LoadBattleback2(targetParameters[1]).texture;
                    Save(EventDataModels[EventIndex]);
                }, targetParameters[1]);
            };

            // 背景画像インポート
            Button importButton2 = RootElement.Query<Button>("battle_scene_bg_bottom_image_import");
            importButton2.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.BATTLE_BACKGROUND_2);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    targetParameters[1] = path;
                    imageNameLabel2.text = ImageManager.GetBattlebackName(path, 2) + ".png";
                    previewImage2.image = ImageManager.LoadBattleback2(targetParameters[1]).texture;
                    Save(EventDataModels[EventIndex]);
                }
            };
        }
    }
}